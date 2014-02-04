using Forge.Collections;
using Forge.Entities;
using UnityEngine;

namespace Forge.Unity {
    /// <summary>
    /// Base MonoBehavior type that can be used to implement custom data renderers. DataRenderers
    /// need to be annotated with a CustomDataRegistry attribute.
    /// </summary>
    public abstract class DataRenderer : MonoBehaviour {
        /// <summary>
        /// The entity that the renderer should render.
        /// </summary>
        protected IQueryableEntity Entity {
            get;
            private set;
        }

        public void Initialize(IQueryableEntity entity) {
            Entity = entity;
            DataRendererManager.Instance.Add(this);

            OnInitialize();
            UpdateVisualization(1.0f);
        }

        /// <summary>
        /// Called when the renderer has been initialized. The entity that this renderer will
        /// operate on is populated under Entity.
        /// </summary>
        protected abstract void OnInitialize();

        /// <summary>
        /// Called when the DataRenderer has been destroyed by Unity. Removes the DataRenderer from
        /// the DataRendererManager.
        /// </summary>
        protected virtual void OnDestroy() {
            if (DataRendererManager.HasInstance) {
                DataRendererManager.Instance.Remove(this);
            }
        }

        /// <summary>
        /// Used by the visualizer for fast removes.
        /// </summary>
        private UnorderedListMetadata _visualizationMetadata = new UnorderedListMetadata();
        internal UnorderedListMetadata VisualizationMetadata {
            get { return _visualizationMetadata; }
        }

        /// <summary>
        /// Called when the visualization should be updated. Visualizations are updated
        /// continuously, often multiple times between game updates.
        /// </summary>
        /// <param name="percentage">The interpolation percentage that should be used between the
        /// last frame and this frame.</param>
        public abstract void UpdateVisualization(float percentage);

        protected static Vector3 Interpolate(Vector3 start, Vector3 end, float percentage) {
            return new Vector3(
                Interpolate(start.x, end.x, percentage),
                Interpolate(start.y, end.y, percentage),
                Interpolate(start.z, end.z, percentage));
        }

        protected static float Interpolate(float start, float end, float percentage) {
            float delta = end - start;
            return start + (delta * percentage);

            //return (start * (1 - percentage)) + (end * percentage);
        }
    }
}