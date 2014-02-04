using Forge.Entities;
using System;
using System.IO;
using UnityEngine;

namespace Forge.Unity {
    [ExecuteInEditMode]
    public class LevelDesigner : MonoBehaviour {
        /// <summary>
        /// The starting id for the shared template group.
        /// </summary>
        private const int FirstSharedTemplateId = 0;

        /// <summary>
        /// The starting template id for level-specific templates.
        /// </summary>
        private const int FirstLevelTemplateId = 10000;

        /// <summary>
        /// Finds or creates a child entity with the given name (relative to this game object).
        /// </summary>
        private GameObject FindChild(string name) {
            Transform child = transform.FindChild(name);
            if (child != null) {
                return child.gameObject;
            }

            GameObject go = new GameObject(name);
            go.transform.parent = transform;
            return go;
        }

        private GameObject RemovedChild {
            get {
                return FindChild("Removed");
            }
        }
        private GameObject AddedChild {
            get {
                return FindChild("Added");
            }
        }
        private GameObject ActiveChild {
            get {
                return FindChild("Active");
            }
        }
        private GameObject LevelTemplatesChild {
            get {
                return FindChild("LevelTemplates");
            }
        }
        private GameObject SharedTemplatesChild {
            get {
                return FindChild("SharedTemplates");
            }
        }

        /// <summary>
        /// An empty template group that is used to instantiate entities from when we are editing
        /// IGameSnapshots. This relies upon internals of the serialization framework; by using
        /// this, we just specify that we don't populate the ITemplate instances (w.r.t. to the
        /// IGameSnapshot references) with any data.
        /// </summary>
        private readonly string EmptySerializedTemplateGroup =
            LevelManager.SaveTemplateGroup(LevelManager.CreateTemplateGroup());

        public IGameSnapshot Snapshot {
            get;
            private set;
        }

        public ITemplateGroup LevelTemplates {
            get;
            private set;
        }

        public ITemplateGroup SharedTemplates {
            get;
            private set;
        }

        public string SavedSnapshotState;
        public string SavedLevelTemplateState;
        public string SavedSharedTemplateState;

        public string SnapshotPath;
        public string LevelTemplatePath;
        public string SharedTemplatePath;

        /// <summary>
        /// Adds an entity to the current snapshot.
        /// </summary>
        public void AddEntity() {
            if (Snapshot == null) {
                Debug.LogError("Attempt to add an entity with no snapshot; either create an empty one or load one from a file");
                return;
            }

            IEntity entity = Snapshot.CreateEntity();
            EntityContainer.CreateEntityContainer(entity, AddedChild);
        }

        public void OnTemplateDestroyed(ITemplate template) {
            if (LevelTemplates != null) {
                LevelTemplates.RemoveTemplate(template);
            }

            if (SharedTemplates != null) {
                SharedTemplates.RemoveTemplate(template);
            }
        }

        public void OnEntityDestroyed(EntityContainer container) {
            // if we don't have a snapshot, then we don't care that the entity was destroyed
            if (Snapshot == null) {
                return;
            }

            IEntity entity = container.Entity;
            GameSnapshotEntityRemoveResult removeResult = Snapshot.RemoveEntity(entity);

            switch (removeResult) {
                case GameSnapshotEntityRemoveResult.Destroyed:
                    break;

                case GameSnapshotEntityRemoveResult.IntoRemoved:
                    EntityContainer.CreateEntityContainer(entity, RemovedChild);
                    break;

                case GameSnapshotEntityRemoveResult.Failed:
                    Debug.LogError("You cannot remove entity " + entity + " (recreating it)");
                    GameObject parent = container.Parent;
                    EntityContainer.CreateEntityContainer(entity, parent);
                    break;

                default:
                    throw new InvalidOperationException("Unknown remove result " + removeResult);
            }
        }

        /// <summary>
        /// Adds a template to the current shared template group.
        /// </summary>
        public void AddSharedTemplate() {
            if (SharedTemplates == null) {
                Debug.LogError("Attempt to add a shared template with no loaded shared templates; " +
                    "either create an empty one or load one from a file");
                return;
            }

            ITemplate template = SharedTemplates.CreateTemplate();
            TemplateContainer.CreateTemplateContainer(template, SharedTemplatesChild);
        }

        /// <summary>
        /// Adds a template to the current level template group.
        /// </summary>
        public void AddLevelTemplate() {
            if (LevelTemplates == null) {
                Debug.LogError("Attempt to add a level template with no loaded level templates; " +
                    "either create an empty one or load one from a file");
                return;
            }

            ITemplate template = LevelTemplates.CreateTemplate();
            TemplateContainer.CreateTemplateContainer(template, LevelTemplatesChild);
        }

        /// <summary>
        /// Imports shared templates using the given template JSON.
        /// </summary>
        public void ImportSharedTemplates(string templateJson) {
            // TODO: examine the maybe
            SharedTemplates = LevelManager.LoadTemplateGroup(templateJson).Value;
            SavedSharedTemplateState = templateJson;

            TemplateContainer.ClearContainers(SharedTemplatesChild);
            TemplateContainer.CreateContainers(SharedTemplates, SharedTemplatesChild);
        }

        /// <summary>
        /// Imports level templates using the given template JSON.
        /// </summary>
        public void ImportLevelTemplates(string templateJson) {
            // TODO: examine the maybe
            LevelTemplates = LevelManager.LoadTemplateGroup(templateJson).Value;
            SavedLevelTemplateState = templateJson;

            TemplateContainer.ClearContainers(LevelTemplatesChild);
            TemplateContainer.CreateContainers(LevelTemplates, LevelTemplatesChild);
        }

        public void ImportSnapshot(string snapshotJson) {
            // TODO: examine the maybe
            Snapshot = LevelManager.LoadSnapshot(snapshotJson, EmptySerializedTemplateGroup).Value;
            SavedSnapshotState = snapshotJson;

            EntityContainer.ClearContainers(gameObject);
            EntityContainer.CreateContainers(Snapshot, gameObject, AddedChild, ActiveChild, RemovedChild);
        }

        /// <summary>
        /// Exports the current level reference to the export path.
        /// </summary>
        public void ExportSnapshot() {
            if (Snapshot == null) {
                Debug.LogError("Cannot export an empty snapshot");
                return;
            }

            File.WriteAllText(SnapshotPath, LevelManager.SaveSnapshot(Snapshot));
        }

        /// <summary>
        /// Writes the current state of the shared templates to the template path.
        /// </summary>
        public void ExportSharedTemplates() {
            if (SharedTemplates == null) {
                Debug.LogError("Cannot export an empty shared template group");
                return;
            }

            File.WriteAllText(SharedTemplatePath, LevelManager.SaveTemplateGroup(SharedTemplates));
        }

        /// <summary>
        /// Writes the current state of the level templates to the template path.
        /// </summary>
        public void ExportLevelTemplates() {
            if (LevelTemplates == null) {
                Debug.LogError("Cannot export an empty level template group");
                return;
            }

            File.WriteAllText(LevelTemplatePath, LevelManager.SaveTemplateGroup(LevelTemplates));
        }

        /// <summary>
        /// Prepares the restoration state and clears out entity/template containers in preparation
        /// either a Unity serialization cycle or entering play mode.
        /// </summary>
        private void PrepareForRestoration() {
            SavedLevelTemplateState = null;
            SavedSharedTemplateState = null;
            SavedSnapshotState = null;

            if (LevelTemplates != null) {
                SavedLevelTemplateState = LevelManager.SaveTemplateGroup(LevelTemplates);
            }
            if (SharedTemplates != null) {
                SavedSharedTemplateState = LevelManager.SaveTemplateGroup(SharedTemplates);
            }
            if (Snapshot != null) {
                SavedSnapshotState = LevelManager.SaveSnapshot(Snapshot);
            }

            LevelTemplates = null;
            SharedTemplates = null;
            Snapshot = null;

            TemplateContainer.ClearContainers(LevelTemplatesChild);
            TemplateContainer.ClearContainers(SharedTemplatesChild);
            EntityContainer.ClearContainers(gameObject);
        }

        protected void Update() {
            // If we have a level or a template group and we are compiling code, then we want to
            // save the level state so that we can restore it when the level is done compiling
            if ((Snapshot != null || LevelTemplates != null || SharedTemplates != null) &&
                ForgeEditorUtility.IsCompiling) {
                PrepareForRestoration();
            }

            // We don't want to bother restoring the templates and the snapshot if we're playing the
            // game or still compiling, as the GameEngineManager is in control of the inspector.
            if (ForgeEditorUtility.IsPlaying == false && ForgeEditorUtility.IsCompiling == false) {
                // If our template reference is null, but we *do* have a saved template state, then
                // we should restore our template reference
                if (LevelTemplates == null && string.IsNullOrEmpty(SavedLevelTemplateState) == false) {
                    ImportLevelTemplates(SavedLevelTemplateState);
                }

                // If our template reference is null, but we *do* have a saved template state, then
                // we should restore our template reference
                if (SharedTemplates == null && string.IsNullOrEmpty(SavedSharedTemplateState) == false) {
                    ImportSharedTemplates(SavedSharedTemplateState);
                }

                // If our snapshot reference is null, but we *do* have a saved snapshot state, then
                // we should restore our snapshot reference.
                if (Snapshot == null && string.IsNullOrEmpty(SavedSnapshotState) == false) {
                    if (LevelTemplates == null || SharedTemplates == null) {
                        Debug.LogWarning("Potentially bad internal state; a snapshot save " +
                            "exists but one of the template groups does not");
                    }
                    ImportSnapshot(SavedSnapshotState);
                }
            }
        }

        protected void OnDisable() {
            // The play button has been pressed. We want to update our restoration state so we can
            // restore correctly. We also want to clear out our snapshot and template data, so that
            // the GameEngineManager can allocate containers appropriately.
            if ((Snapshot != null || LevelTemplates != null || SharedTemplates != null) &&
                (ForgeEditorUtility.IsPlayingOrWillChangePlaymode && ForgeEditorUtility.IsPlaying == false)) {
                PrepareForRestoration();
            }
        }

        /// <summary>
        /// Removes the current level.
        /// </summary>
        public void ClearSnapshot() {
            // remove internal automatic restoration references
            Snapshot = null;
            SavedSnapshotState = null;

            // remove all EntityContainer GameObjects (this doesn't go through the RemoveEntity
            // pipeline, though).
            EntityContainer.ClearContainers(gameObject);
        }

        /// <summary>
        /// Removes the level template group.
        /// </summary>
        public void ClearLevelTemplates() {
            // remove internal automatic restoration references
            LevelTemplates = null;
            SavedLevelTemplateState = null;

            // remove all of the TemplateContainer GameObjects (this doesn't go through the
            // RemoveTemplate pipeline, though).
            TemplateContainer.ClearContainers(LevelTemplatesChild);
        }

        /// <summary>
        /// Removes the shared template group.
        /// </summary>
        public void ClearSharedTemplates() {
            // remove internal automatic restoration references
            SharedTemplates = null;
            SavedSharedTemplateState = null;

            // remove all of the TemplateContainer GameObjects (this doesn't go through the
            // RemoveTemplate pipeline, though).
            TemplateContainer.ClearContainers(SharedTemplatesChild);
        }

        /// <summary>
        /// Creates a new empty snapshot.
        /// </summary>
        public void CreateSnapshot() {
            Snapshot = LevelManager.CreateSnapshot();
            SavedSnapshotState = LevelManager.SaveSnapshot(Snapshot);

            EntityContainer.ClearContainers(gameObject);
            EntityContainer.CreateContainers(Snapshot,
                gameObject,
                AddedChild,
                ActiveChild,
                RemovedChild);
        }

        /// <summary>
        /// Creates a new empty level template group.
        /// </summary>
        public void CreateLevelTemplateGroup() {
            LevelTemplates = LevelManager.CreateTemplateGroup(FirstLevelTemplateId);
            SavedLevelTemplateState = EmptySerializedTemplateGroup;

            TemplateContainer.ClearContainers(LevelTemplatesChild);
            TemplateContainer.CreateContainers(LevelTemplates, LevelTemplatesChild);
        }

        /// <summary>
        /// Creates a new empty shared template group.
        /// </summary>
        public void CreateSharedTemplateGroup() {
            SharedTemplates = LevelManager.CreateTemplateGroup(FirstSharedTemplateId);
            SavedSharedTemplateState = EmptySerializedTemplateGroup;

            TemplateContainer.ClearContainers(SharedTemplatesChild);
            TemplateContainer.CreateContainers(SharedTemplates, SharedTemplatesChild);
        }

    }
}