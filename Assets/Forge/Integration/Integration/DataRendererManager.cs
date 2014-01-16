using Forge.Collections;
using Forge.Networking.AutomaticTurnGame;

namespace Forge.Unity {
    /// <summary>
    /// The IDataRendererManager updates the interpolation status of all DataRenderers.
    /// </summary>
    public interface IDataRendererManager {
        /// <summary>
        /// Add the given item to the list of items which will receive visualization events.
        /// </summary>
        /// <param name="renderer">The visualized item.</param>
        void Add(DataRenderer renderer);

        /// <summary>
        /// Removes the given item from the list of items that receive visualization events.
        /// </summary>
        /// <param name="renderer">The visualized item.</param>
        void Remove(DataRenderer renderer);
    }

    public class DataRendererManager : SingletonBehavior<DataRendererManager, IDataRendererManager>, IDataRendererManager {
        /// <summary>
        /// The dependency component that contains the current interpolation percentage
        /// </summary>
        public ForgeDependencyComponent Dependencies;

        /// <summary>
        /// Should the visualizer interpolate between frames?
        /// </summary>
        public bool Interpolate = true;

        /// <summary>
        /// The list of items that need to be rendered (or rather, have their rendering states
        /// updated) .
        /// </summary>
        protected UnorderedList<DataRenderer> _renderers = new UnorderedList<DataRenderer>();

        void IDataRendererManager.Add(DataRenderer renderer) {
            _renderers.Add(renderer, renderer.VisualizationMetadata);
        }

        void IDataRendererManager.Remove(DataRenderer renderer) {
            _renderers.Remove(renderer, renderer.VisualizationMetadata);
        }

        /// <summary>
        /// Updates all objects which have been registered to be visualized.
        /// </summary>
        protected void Update() {
            float interpolate = 1.0f;
            if (Interpolate) {
                interpolate = Dependencies.InterpolationPercentage;
            }

            for (int i = 0; i < _renderers.Length; ++i) {
                _renderers[i].UpdateVisualization(interpolate);
            }
        }
    }
}