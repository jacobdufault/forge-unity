using Forge.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Forge.Unity {
    /// <summary>
    /// The base container provides Unity component integration for IQueryableEntities.
    /// </summary>
    /// <remarks>
    /// TODO: potential optimization is that if an entity has no renderers, it does not need an
    ///       associated GameObject
    // </remarks>
    [ExecuteInEditMode]
    public abstract class BaseContainer : MonoBehaviour {
        /// <summary>
        /// The queryable entity interface that is used for retrieving renderable data from.
        /// </summary>
        public abstract IQueryableEntity QueryableEntity {
            get;
        }

        /// <summary>
        /// The original name of the GameObject.
        /// </summary>
        protected string _originalName;

        /// <summary>
        /// Updates the name of the container so that it also reflects the name of the
        /// QueryableEntity.
        /// </summary>
        public void UpdateName() {
            if (string.IsNullOrEmpty(_originalName)) {
                gameObject.name = QueryableEntity.ToString();
            }
            else {
                gameObject.name = QueryableEntity.ToString() + " " + _originalName;
            }
        }

        /// <summary>
        /// Constructs a container GameObject for the given entity under the given root object.
        /// </summary>
        /// <param name="entity">The entity to create a GameObject for</param>
        /// <param name="root">The root object to create the GameObject under</param>
        /// <returns>The created GameObject.</returns>
        protected static GameObject CreateGameObject(IQueryableEntity entity, GameObject root) {
            GameObject created = null;

            // There is special logic for instantiating an object If the entity contains prefab data
            // that specifies it should be instantiated from a specific resource, then we load that
            // resource and clone it instead of creating a new object
            if (entity.ContainsData<PrefabData>()) {
                string resourcePath = entity.Current<PrefabData>().PrefabResourcePath;
                GameObject prefab = (GameObject)Resources.Load(resourcePath);
                if (prefab == null) {
                    created = new GameObject("");
                    Debug.LogError("Unable to find prefab resource \"" + resourcePath + "\"", created);
                }
                else {
                    created = (GameObject)Instantiate(prefab);
                }
            }

            else {
                created = new GameObject("");
            }

            created.transform.parent = root.transform;
            return created;
        }

        /// <summary>
        /// Initializes renderers for any data that is currently contained within the given entity.
        /// </summary>
        /// <param name="containingObject">The GameObject that contains the BaseContainer for the
        /// entity.</param>
        /// <param name="entity">The entity to select data from.</param>
        protected static void InitializeRenderers(GameObject containingObject, IQueryableEntity entity) {
            ICollection<DataAccessor> entityData = entity.SelectData();
            foreach (DataAccessor accessor in entityData) {
                DataRegistry.TryAddRenderer(accessor, containingObject, entity);
            }
        }
    }

    /// <summary>
    /// Base class for EntityContainer and TemplateContainer that contains a large amount of common
    /// code that needs to be strongly typed. The non-generic BaseContainer is provided for API
    /// convenience.
    /// </summary>
    /// <typeparam name="TDerived">The type that is deriving this.</typeparam>
    /// <typeparam name="TEntity">The type of entity that the derived type provides.</typeparam>
    public abstract class BaseContainer<TDerived, TEntity> : BaseContainer
        where TDerived : BaseContainer<TDerived, TEntity>
        where TEntity : IQueryableEntity {

        /// <summary>
        /// Maintain a cache of our entity containers so that we can have fast container lookup. We
        /// use a dictionary instead of a sparse array because the ids may get quite high
        /// (millions), which will consume a *ton* of memory.
        /// </summary>
        [NonSerialized] // redundant, but for code clarity
        private static Dictionary<int, TDerived> _containers = new Dictionary<int, TDerived>();

        /// <summary>
        /// Returns a pretty list of the containers that are cached.
        /// </summary>
        public static string PrettyCachedContainers {
            get {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine(typeof(TDerived).Name);
                foreach (var tuple in _containers) {
                    sb.Append("  ");
                    sb.Append(tuple.Key);
                    sb.Append(" => ");
                    sb.AppendLine(tuple.Value.ToString());
                }
                return sb.ToString();
            }
        }

        /// <summary>
        /// Destroys any entity containers that still exist.
        /// </summary>
        public static void ClearContainers(GameObject root) {
            List<GameObject> toDestroy = new List<GameObject>();

            foreach (Transform child in root.transform) {
                if (child.GetComponent<TDerived>() != null) {
                    toDestroy.Add(child.gameObject);
                }
                else {
                    ClearContainers(child.gameObject);
                }
            }

            foreach (GameObject child in toDestroy) {
                DestroyImmediate(child);
            }
        }

        /// <summary>
        /// Finds the container that the given entity is attached to and destroys it and the
        /// associated GameObject. This method should *only* be called by the LevelDesigner or the
        /// GameEngineManager.
        /// </summary>
        public static void DestroyContainer(TDerived container) {
            if (Application.isEditor) {
                DestroyImmediate(container.gameObject);
            }
            else {
                Destroy(container.gameObject);
            }
        }

        /// <summary>
        /// When the container is destroyed (and it hasn't already been destroyed by some other
        /// source) , which will happen when the user, ie, deletes the GameObject, we want to remove
        /// the entity from the level. This will then propagate back into EntityContainer which will
        /// ultimately remove the container.
        /// </summary>
        protected void OnDestroy() {
            _containers.Remove(GetEntityId());
            NotifyLevelDesignerOfDestruction();
        }

        /// <summary>
        /// Creates a new container for the given entity.
        /// </summary>
        protected static TDerived CreateContainer(TEntity entity, int id, GameObject parent) {
            if (_containers.ContainsKey(id)) {
                Debug.LogError("Already created a EntityContainer for entity " + entity);
                return _containers[id];
            }

            GameObject gameObject = CreateGameObject(entity, parent);

            TDerived container = gameObject.AddComponent<TDerived>();
            _containers[id] = container;

            container.Initialize(entity);
            container._originalName = gameObject.name;
            container.UpdateName();
            InitializeRenderers(container.gameObject, entity);

            return container;
        }

        /// <summary>
        /// Run any custom initialization logic for the container with the given entity.
        /// </summary>
        /// <param name="entity">The entity that this container will contain.</param>
        protected abstract void Initialize(TEntity entity);

        /// <summary>
        /// Return the unique id relative to all other containers that share this container's type.
        /// </summary>
        protected abstract int GetEntityId();

        /// <summary>
        /// Called when the LevelDesigner should be notified that this container has been destroyed.
        /// </summary>
        protected abstract void NotifyLevelDesignerOfDestruction();

        /// <summary>
        /// Fetch a container by its id in O(1).
        /// </summary>
        protected static TDerived GetContainerById(int entityId) {
            TDerived value;
            if (_containers.TryGetValue(entityId, out value)) {
                return value;
            }

            return null;
        }
    }
}