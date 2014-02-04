using Forge.Unity;
using UnityEditor;
using UnityEngine;

namespace Forge.Editing {
    public class ManagementWindow : EditorWindow {
        [MenuItem("Forge/Management")]
        private static void Init() {
            // Get existing open window or if none, make a new one
            EditorWindow.GetWindow(typeof(ManagementWindow));
        }

        protected void OnEnable() {
            title = "Management";
        }

        protected void Update() {
            Repaint();
        }

        protected void OnGUI() {
            /*
            if (Selection.activeGameObject == null ||
                Selection.activeGameObject.GetComponentInParent<LevelDesigner>() == null) {

                EditorGUILayout.HelpBox("Select a GameObject that contains a LevelDesigner so " +
                    "that you can add IEntities and ITemplates using this window", MessageType.Info);
                return;
            }

            if (Selection.activeGameObject.GetComponentInParent<LevelDesigner>().Snapshot == null) {
                EditorGUILayout.HelpBox("Load a snapshot so that you can add IEntities and " +
                    "ITemplates using this window", MessageType.Info);
                return;
            }
            */

            /*
             1. Create a new Texture2D(1,1)
2. Fill it with the desired color ( SetPixel )
3. Set it's wrap mode to repeat
4. Use Texture2D.Apply() to apply the changes
5. Create a GUIStyle
6. Set it's .normal background texture to the texture you just created
             */

            ForgeEditorUtils.VerticalGroup(() => {
                GUILayout.Label("Shared Template Group", ForgeEditorUtils.HeaderStyle);
                ForgeEditorUtils.TextField("Path", "");
                ForgeEditorUtils.HorizontalGroup(() => {
                    GUILayout.Button("Load");
                    GUILayout.Button("Destroy");
                });
                GUILayout.Button("Save");

                GUILayout.Label("Level Template Group", ForgeEditorUtils.HeaderStyle);
                ForgeEditorUtils.TextField("Path", "");
                ForgeEditorUtils.HorizontalGroup(() => {
                    GUILayout.Button("Load");
                    GUILayout.Button("Destroy");
                });
                GUILayout.Button("Save");

                GUILayout.Label("Snapshot", ForgeEditorUtils.HeaderStyle);
                ForgeEditorUtils.TextField("Path", "");
                ForgeEditorUtils.HorizontalGroup(() => {
                    GUILayout.Button("Load");
                    GUILayout.Button("Destroy");
                });
                GUILayout.Button("Save");
            });

            ForgeEditorUtils.DrawSeperator(6);

            GUILayout.Label("IQueryableEntity Creation", ForgeEditorUtils.HeaderStyle);
            GUILayout.Button("Add Entity");
            GUILayout.Button("Add Shared Template");
            GUILayout.Button("Add Level Template");

            ForgeEditorUtils.DrawSeperator(6);

            GUILayout.Label("System Management", ForgeEditorUtils.HeaderStyle);
            GUILayout.Button("Clear");
            GUILayout.Button("Add systems from {provider0}");
            GUILayout.Button("Add systems from {provider1}");
        }
    }
}