using Forge.Entities;
using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Forge.Unity {
    internal static class ForgeEditorUtils {
        public static GUIStyle HeaderStyle;
        public static GUIStyle BoldStyle;
        public static GUIStyle RegularStyle;

        /// <summary>
        /// True if the Forge.Editing package is currently loaded. If this is false, then we display
        /// an ad for the package.
        /// </summary>
        private static bool _hasForgeExtension;

        static ForgeEditorUtils() {
            // Determine if the editing package is loaded
            _hasForgeExtension = (from assembly in AppDomain.CurrentDomain.GetAssemblies()
                                  from type in assembly.GetExportedTypes()
                                  where type.Namespace != null
                                  select type).Any(type => type.Namespace.Contains("Forge.Editing"));

            // Create styles

            HeaderStyle = new GUIStyle();
            HeaderStyle.normal.textColor = EditorGUIUtility.isProSkin
                ? new Color(0.8f, 0.8f, 0.8f)
                : new Color(0.2f, 0.2f, 0.2f);
            HeaderStyle.fontSize = 16;
            HeaderStyle.margin = new RectOffset(5, 0, 0, 0);

            BoldStyle = new GUIStyle();
            BoldStyle.normal.textColor = EditorGUIUtility.isProSkin
                ? new Color(0.8f, 0.8f, 0.8f)
                : new Color(0.2f, 0.2f, 0.2f);
            BoldStyle.fontStyle = FontStyle.Bold;
            BoldStyle.margin = new RectOffset(5, 0, 0, 0);

            RegularStyle = new GUIStyle();
            RegularStyle.normal.textColor = EditorGUIUtility.isProSkin
                ? new Color(0.8f, 0.8f, 0.8f)
                : new Color(0.2f, 0.2f, 0.2f);
            RegularStyle.margin = new RectOffset(5, 0, 0, 0);
        }

        /// <summary>
        /// Attempts to retrieve an IQueryableEntity from the currently selected game object.
        /// Returns null if there is no selected IQueryableEntity.
        /// </summary>
        public static IQueryableEntity TryGetQueryableEntity() {
            GameObject selected = Selection.activeGameObject;
            if (selected == null) {
                return null;
            }

            BaseContainer container = selected.GetComponent<BaseContainer>();
            if (container == null) {
                return null;
            }

            return container.QueryableEntity;
        }

        public static void DrawSeperator() {
            // TODO: make some of the separators draggable
            if (GUILayout.RepeatButton("", GUI.skin.FindStyle("Box"), GUILayout.Height(4), GUILayout.ExpandWidth(true))) {
                Debug.Log("Pressing " + Event.current.mousePosition);
            }
            //GUILayout.Box("", GUILayout.Height(4), GUILayout.ExpandWidth(true));
        }

        public static bool DrawFoldout(bool current) {
            return DrawPrettyToggle(current, ">", "v", GUILayout.ExpandWidth(false));
        }

        public static bool DrawPrettyToggle(bool current, string a, string b, params GUILayoutOption[] options) {
            if (current) {
                return !GUILayout.Button(a, options);
            }

            return GUILayout.Button(b, options);
        }

        public static void EnableGroup(bool enabled, Action code) {
            EditorGUI.BeginDisabledGroup(!enabled);
            code();
            EditorGUI.EndDisabledGroup();
        }

        public static void HorizontalGroup(Action code, params GUILayoutOption[] options) {
            GUILayout.BeginHorizontal(options);
            code();
            GUILayout.EndHorizontal();
        }

        public static void VerticalGroup(Action code, params GUILayoutOption[] options) {
            GUILayout.BeginVertical(options);
            code();
            GUILayout.EndVertical();
        }

        /// <summary>
        /// Draws an ad for the forge extension on the asset store if the extension is not currently
        /// loaded.
        /// </summary>
        public static void DrawAd() {
            if (_hasForgeExtension == false) {
                EditorGUILayout.HelpBox("The forge editor in the asset store makes creating and " +
                    "viewing your snapshot and templates work with the inspector. It lets you " +
                    "work with dictionaries, structs, and interfaces too!", MessageType.Info);
            }
        }
    }
}