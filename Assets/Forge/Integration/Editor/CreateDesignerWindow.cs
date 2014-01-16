using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Forge.Unity {
    public class CreateDesignerWindow : EditorWindow {
        private string _snapshotPath = "snapshot.json";
        private string _levelTemplatePath = "level_templates.json";
        private string _sharedTemplatePath = "shared_templates.json";
        private int _selectedDependencyType = 0;

        private static string[] _dependencyTypeNames;
        private static Type[] _dependencyTypes;

        static CreateDesignerWindow() {
            _dependencyTypes = (from assembly in AppDomain.CurrentDomain.GetAssemblies()
                                from type in assembly.GetExportedTypes()
                                where typeof(ForgeDependencyComponent).IsAssignableFrom(type)
                                where type.IsAbstract == false
                                where type.IsInterface == false
                                select type).ToArray();
            _dependencyTypeNames = (from type in _dependencyTypes
                                    select type.FullName).ToArray();
        }

        [MenuItem("Forge/Create Level")]
        private static void Init() {
            // Get existing open window or if none, make a new one
            EditorWindow.GetWindow<CreateDesignerWindow>(/*utility:*/ true);
        }

        protected void OnEnable() {
            title = "New Level";
            position = new Rect(position.x, position.y, 500, 260);
        }

        protected void OnGUI() {
            EditorGUILayout.HelpBox("Please set the file location for your snapshot and " +
                "template JSON files. The snapshot contains the IEntity instances in the game. " +
                "The level template contains templates that are specific to this level, whereas " +
                "the shared templates contains templates that are used in every level. Shared " +
                "templates can reference level templates, and level templates can reference " +
                "shared templates.", MessageType.Info);

            _snapshotPath = EditorGUILayout.TextField("Snapshot Path", _snapshotPath);
            _levelTemplatePath = EditorGUILayout.TextField("Level Template Path", _levelTemplatePath);
            _sharedTemplatePath = EditorGUILayout.TextField("Shared Template Path", _sharedTemplatePath);

            EditorGUILayout.HelpBox("The dependency type provides input management for Forge. " +
                "The AutomaticTurnGameDependencyComponent is well suited for RTS and " +
                "tower-defense style games", MessageType.Info);
            _selectedDependencyType = EditorGUILayout.Popup("Dependency Type", _selectedDependencyType, _dependencyTypeNames);

            List<string> missingFiles = new List<string>();
            if (File.Exists(_snapshotPath) == false) missingFiles.Add(_snapshotPath);
            if (File.Exists(_levelTemplatePath) == false) missingFiles.Add(_levelTemplatePath);
            if (File.Exists(_sharedTemplatePath) == false) missingFiles.Add(_sharedTemplatePath);

            if (missingFiles.Count > 0) {
                EditorGUILayout.HelpBox("Cannot find files " + string.Join(", ", missingFiles.ToArray()), MessageType.Warning);
            }

            ForgeEditorUtils.HorizontalGroup(() => {
                if (GUILayout.Button("Cancel")) {
                    Close();
                }

                GUILayout.FlexibleSpace();

                if (GUILayout.Button("Create")) {
                    Type dependencyType = _dependencyTypes[_selectedDependencyType];
                    ForgeLoader.LoadForge(_snapshotPath, _levelTemplatePath, _sharedTemplatePath,
                        dependencyType);
                    Close();
                }
            });
        }
    }
}