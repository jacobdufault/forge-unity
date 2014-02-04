using Forge.Entities;
using UnityEngine;

namespace Forge.Unity {
    /// <summary>
    /// An IEventMonitor that manages the creation of EntityContainers and registering/removing data
    /// from those EntityContainers as the data state changes in entities.
    /// </summary>
    [EventMonitorAutomaticInstantiation]
    internal class EntityContainerEventMonitor : IEventMonitor {
        private static void OnDataAdded(AddedDataEvent addedData) {
            var container = EntityContainer.GetContainer(addedData.Entity);
            var accessor = new DataAccessor(addedData.AddedDataType);
            DataRegistry.TryAddRenderer(accessor, container.gameObject, addedData.Entity);
        }

        private static void OnDataRemoved(RemovedDataEvent removedData) {
            var container = EntityContainer.GetContainer(removedData.Entity);
            var accessor = new DataAccessor(removedData.RemovedDataType);
            DataRegistry.TryRemoveRenderer(accessor, container.gameObject);
        }

        private static void OnEntityCreated(IEntity entity, GameObject root) {
            EntityContainer.CreateEntityContainer(entity, root);
        }

        private static void OnEntityDestroyed(IEntity entity) {
            EntityContainer container = EntityContainer.GetContainer(entity);
            EntityContainer.DestroyContainer(container);
        }

        /// <summary>
        /// Returns the root GameObject that should be the parent of all created EntityContainers.
        /// </summary>
        private GameObject GetRoot() {
            const string rootName = "Entities";

            // if we're in a debug build, do some verification to make sure that we are not creating
            // two GameObjects with the same rootName
            if (Debug.isDebugBuild) {
                GameObject root = GameObject.Find(rootName);
                if (root != null) {
                    Debug.LogError("Life-cycle error; there was already an object with name = "
                        + rootName);
                    return root;
                }
            }

            return new GameObject(rootName);
        }

        public void Initialize(IEventNotifier notifier) {
            GameObject root = GetRoot();

            notifier.OnEvent<EntityAddedEvent>(evnt => {
                OnEntityCreated(evnt.Entity, root);
            });
            notifier.OnEvent<EntityRemovedEvent>(evnt => {
                OnEntityDestroyed(evnt.Entity);
            });
            notifier.OnEvent<AddedDataEvent>(OnDataAdded);
            notifier.OnEvent<RemovedDataEvent>(OnDataRemoved);
        }
    }
}