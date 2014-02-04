using Forge.Unity;
using UnityEditor;
using UnityEngine;

namespace Forge.Editing {
    public class PrefabInspector : DataInspector<PrefabData> {
        /// <summary>
        /// The prefab that the PrefabData points to.
        /// </summary>
        private GameObject _prefab;

        /// <summary>
        /// Have we told the the user that we could not discover the prefab from its PrefabPath?
        /// </summary>
        private bool _badPathNotified;

        /// <summary>
        /// Loads a GameObject in a Resources folder.
        /// </summary>
        private static bool FindInResources(string path, out GameObject result) {
            result = (GameObject)Resources.Load(path);
            return result != null;
        }

        /// <summary>
        /// Loads a GameObject from anywhere inside of the top-level Assets folder.
        /// </summary>
        private static bool FindEverywhere(string path, out GameObject result) {
            result = (GameObject)AssetDatabase.LoadAssetAtPath(path, typeof(GameObject));
            return result != null;
        }

        /// <summary>
        /// Loads a GameObject from anywhere by its GUID.
        /// </summary>
        private static bool FindByGuid(string guid, out GameObject result) {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            return FindInResources(path, out result) || FindEverywhere(path, out result);
        }

        protected override void Prepare(PrefabData data, GameObject context) {
            // Asset discovery strategy:
            // 1. Search for the asset in Resources (normal case, no data recovery)
            // 2. Search for the asset outside of Resources (editor only, data recovery mode)
            // 3. Search for the asset by its GUID (editor only, data recovery mode)

            _badPathNotified = false;
            _prefab = null;

            if (FindInResources(data.PrefabResourcePath, out _prefab)) {
                return;
            }

            if (FindEverywhere(data.PrefabResourcePath, out _prefab)) {
                Debug.LogWarning("Recovered resource \"" + data.PrefabResourcePath + "\"; this " +
                    "will not happen at run-time (make sure to put it in a resources folder)",
                    context);
            }

            if (FindByGuid(data.PrefabGuid, out _prefab)) {
                Debug.LogWarning("Recovered resource \"" + data.PrefabResourcePath + "\" by " +
                    "using its GUID; GUID recovery will not happen at run-time", context);
            }

            if (string.IsNullOrEmpty(data.PrefabResourcePath)) {
                _badPathNotified = true;
            }
        }

        protected override void Edit(PrefabData data, GameObject context) {
            _prefab = (GameObject)EditorGUILayout.ObjectField("Prefab", _prefab, typeof(GameObject), true);
            EditorGUILayout.LabelField("GUID", data.PrefabGuid);

            string rawPath = AssetDatabase.GetAssetPath(_prefab);
            string path = GetResourcePath(rawPath, context);
            string guid = AssetDatabase.AssetPathToGUID(rawPath);

            if (AreStringsEqual(path, data.PrefabResourcePath) == false) {
                _badPathNotified = false;
            }

            data.PrefabResourcePath = path;
            data.PrefabGuid = guid;
        }

        /// <summary>
        /// Removes the extension from a file, if there is one.
        /// </summary>
        private string RemoveExtension(string path) {
            int extensionIndex = path.LastIndexOf(".");
            if (extensionIndex < 0) {
                return path;
            }

            return path.Substring(0, extensionIndex);
        }

        /// <summary>
        /// Takes a path. If it contains a .../Resources/... header, then the trailing ... is
        /// returned without an extension.
        /// </summary>
        private string GetResourcePath(string path, GameObject gameObject) {
            int resourcePathIndex = path.LastIndexOf("Resources");
            if (resourcePathIndex < 0) {
                if (_badPathNotified == false) {
                    _badPathNotified = true;
                    Debug.LogError("Prefab resource \"" + path + "\" must be in a Resources folder",
                        gameObject);
                }
                return path;
            }

            int startIndex = resourcePathIndex + "Resources\\".Length;
            return RemoveExtension(path.Substring(startIndex, path.Length - startIndex));
        }

        /// <summary>
        /// Returns true if the two strings are equal. Treats null and "" as equal.
        /// </summary>
        private static bool AreStringsEqual(string a, string b) {
            if (string.IsNullOrEmpty(a) && string.IsNullOrEmpty(b)) {
                return true;
            }

            return a == b;
        }
    }
}