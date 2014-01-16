using System;
using UnityEngine;

namespace Forge.Unity {
    /// <summary>
    /// Helper class for loading forge.
    /// </summary>
    public static class ForgeLoader {
        /// <summary>
        /// Loads forge.
        /// </summary>
        /// <param name="snapshotPath">The path for the snapshot.</param>
        /// <param name="levelTemplatePath">The path for the level template group.</param>
        /// <param name="sharedTemplatePath">The path for the shared template group.</param>
        /// <param name="dependenciesType">The component type that will provide additional
        /// dependencies to forge. This type must extend ForgeDependencyComponent.</param>
        /// <returns>The created GameObject that contains all of the forge related
        /// components</returns>
        public static GameObject LoadForge(string snapshotPath, string levelTemplatePath,
            string sharedTemplatePath, Type dependenciesType) {

            if (typeof(ForgeDependencyComponent).IsAssignableFrom(dependenciesType) == false) {
                throw new InvalidOperationException("Bad dependenciesType " + dependenciesType +
                    "; it must derive from ForgeDependencyComponent");
            }

            GameObject go = new GameObject("forge");

            LevelDesigner designer = go.AddComponent<LevelDesigner>();
            designer.SnapshotPath = snapshotPath;
            designer.LevelTemplatePath = levelTemplatePath;
            designer.SharedTemplatePath = sharedTemplatePath;

            ForgeDependencyComponent dependencies = (ForgeDependencyComponent)go.AddComponent(dependenciesType);
            go.AddComponent<GameEngineManager>().Dependencies = dependencies;
            go.AddComponent<GameInputManager>().Dependencies = dependencies;
            go.AddComponent<DataRendererManager>().Dependencies = dependencies;
            go.AddComponent<UnityLogWindow>();

            return go;
        }
    }
}