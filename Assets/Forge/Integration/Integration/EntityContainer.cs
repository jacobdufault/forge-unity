using Forge.Entities;
using UnityEngine;

namespace Forge.Unity {
    public class EntityContainer : BaseContainer<EntityContainer, IEntity> {
        public IEntity Entity {
            get;
            private set;
        }

        public override IQueryableEntity QueryableEntity {
            get { return Entity; }
        }

        /// <summary>
        /// Used to restore the EntityContainer when the user destroys it but isn't allowed to.
        /// </summary>
        public GameObject Parent;

        /// <summary>
        /// Clears out the current containers and adds new containers for all of the entities inside
        /// of the given snapshot.
        /// </summary>
        /// <param name="snapshot">The snapshot to create containers for.</param>
        /// <param name="globalParent">The parent object for the global entity.</param>
        /// <param name="addedParent">The parent object for added entities.</param>
        /// <param name="activeParent">The parent object for active entities.</param>
        /// <param name="removedParent">The parent object for removed entities.</param>
        public static void CreateContainers(IGameSnapshot snapshot, GameObject globalParent,
            GameObject addedParent, GameObject activeParent, GameObject removedParent) {
            EntityContainer.CreateEntityContainer(snapshot.GlobalEntity, globalParent);

            foreach (var entity in snapshot.AddedEntities) {
                EntityContainer.CreateEntityContainer(entity, addedParent);
            }

            foreach (var entity in snapshot.ActiveEntities) {
                EntityContainer.CreateEntityContainer(entity, activeParent);
            }

            foreach (var entity in snapshot.RemovedEntities) {
                EntityContainer.CreateEntityContainer(entity, removedParent);
            }
        }

        protected override int GetEntityId() {
            return Entity.UniqueId;
        }

        protected override void NotifyLevelDesignerOfDestruction() {
            var designer = gameObject.GetComponentInParent<LevelDesigner>();
            if (designer != null) {
                designer.OnEntityDestroyed(this);
            }
        }

        protected override void Initialize(IEntity entity) {
            Parent = transform.parent.gameObject;
            Entity = entity;
        }

        /// <summary>
        /// Creates a new EntityContainer for the given entity.
        /// </summary>
        public static EntityContainer CreateEntityContainer(IEntity entity, GameObject parent) {
            return CreateContainer(entity, entity.UniqueId, parent);
        }

        /// <summary>
        /// Finds the entity container for the given entity.
        /// </summary>
        public static EntityContainer GetContainer(IEntity entity) {
            return GetContainerById(entity.UniqueId);
        }
    }
}