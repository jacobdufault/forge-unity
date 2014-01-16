using Forge.Entities;
using Forge.Networking.AutomaticTurnGame;
using Forge.Networking.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace Forge.Unity {
    public class GameEngineManager : MonoBehaviour {
        private IGameEngine _gameEngine;

        public IGameEngine Engine {
            get {
                return _gameEngine;
            }
        }

        public ForgeDependencyComponent Dependencies;

        /// <summary>
        /// Returns the ITemplateGroup JSON that the engine will be loaded with.
        /// </summary>
        protected static string GetTemplateJson() {
            // get the saved level JSON or read it from the file
            string levelJson = LevelDesigner.Instance.SavedLevelTemplateState;
            if (string.IsNullOrEmpty(levelJson)) {
                levelJson = File.ReadAllText(LevelDesigner.Instance.LevelTemplatePath);
            }

            // get the saved shared JSON or read it from the file
            string sharedJson = LevelDesigner.Instance.SavedSharedTemplateState;
            if (string.IsNullOrEmpty(sharedJson)) {
                sharedJson = File.ReadAllText(LevelDesigner.Instance.SharedTemplatePath);
            }

            return LevelManager.MergeTemplateGroups(
                new List<string>() {
                    levelJson,
                    sharedJson
                });
        }

        /// <summary>
        /// Returns the IGameSnapshot JSON that the engine will be loaded with.
        /// </summary>
        protected static string GetSnapshotJson() {
            string snapshotJson = LevelDesigner.Instance.SavedSnapshotState;
            if (string.IsNullOrEmpty(snapshotJson)) {
                snapshotJson = File.ReadAllText(LevelDesigner.Instance.SnapshotPath);
            }

            return snapshotJson;
        }

        protected void OnEnable() {
            // get the JSON that the engine will be initialized with
            string snapshotJson = GetSnapshotJson();
            string templateJson = GetTemplateJson();

            // allocate the engine
            // TODO: examine the maybe
            Debug.Log(string.Format("Creating engine from snapshot={0}, templates={1}", snapshotJson, templateJson));
            _gameEngine = GameEngineFactory.CreateEngine(snapshotJson, templateJson).Value;

            // create the event monitors
            CreateEventMonitors(_gameEngine.EventNotifier);

            // create template containers -- this will be a separate island reference from the
            // engine's templates, but the runtime templates are immutable, so it shouldn't matter
            // TODO: examine the maybe
            TemplateContainer.CreateContainers(LevelManager.LoadTemplateGroup(templateJson).Value,
                TemplatesChild);
        }

        protected void OnDisable() {
            TemplateContainer.ClearContainers(TemplatesChild);
            Destroy(TemplatesChild);
        }

        private GameObject TemplatesChild {
            get {
                Transform child = transform.FindChild("Templates");
                if (child != null) {
                    return child.gameObject;
                }

                GameObject go = new GameObject("Templates");
                go.transform.parent = transform;
                return go;
            }
        }

        protected void Update() {
            List<IGameInput> input;
            if (Dependencies.TryGetInput(out input)) {
                _gameEngine.Update(input).Wait();
                _gameEngine.SynchronizeState().Wait();
                _gameEngine.DispatchEvents();
            }
        }

        /// <summary>
        /// Discovers allocatable event monitors, allocates them, and then initializes them with the
        /// given event notifier.
        /// </summary>
        /// <param name="eventNotifier">The event notifier to initialize the monitors with</param>
        private static void CreateEventMonitors(IEventNotifier eventNotifier) {
            var monitors =
                from assembly in AppDomain.CurrentDomain.GetAssemblies()
                from type in assembly.GetTypes()
                where typeof(IEventMonitor).IsAssignableFrom(type)
                where type.IsAbstract == false
                where type.IsInterface == false
                where type.IsClass == true
                where Attribute.IsDefined(type, typeof(EventMonitorAutomaticInstantiationAttribute))
                select (IEventMonitor)Activator.CreateInstance(type, /*nonPublic:*/ true);

            foreach (IEventMonitor monitor in monitors) {
                monitor.Initialize(eventNotifier);
            }
        }
    }
}