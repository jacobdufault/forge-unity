using Forge.Collections;
using Forge.Entities;
using Forge.Utilities;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Forge.Unity {
    /// <summary>
    /// Specifies that the given type should be used as a custom data renderer.
    /// </summary>
    public class DataRegistryAttribute : Attribute {
        /// <summary>
        /// The type of data that the annotated type renders.
        /// </summary>
        public Type DataType;

        public DataRegistryAttribute(Type dataType) {
            DataType = dataType;
        }
    }

    /// <summary>
    /// Mapping of Forge Data.IData types to their Unity rendering components.
    /// </summary>
    public static class DataRegistry {
        /// <summary>
        /// Returns all types that have a CustomDataRegistryAttribute attribute.
        /// </summary>
        private static IEnumerable<Tuple<Type, AttributeType>> GetTypesWithAttribute<AttributeType>()
            where AttributeType : Attribute {
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies()) {
                foreach (Type type in assembly.GetTypes()) {
                    object[] attributes = type.GetCustomAttributes(typeof(AttributeType), true);
                    if (attributes.Length > 0) {
                        yield return Tuple.Create(type, (AttributeType)attributes[0]);
                    }
                    if (attributes.Length > 1) {
                        throw new InvalidOperationException("Too many satisfying attributes");
                    }
                }
            }
        }

        static DataRegistry() {
            foreach (var tuple in GetTypesWithAttribute<DataRegistryAttribute>()) {
                DataAccessor dataAccessor = new DataAccessor(tuple.Item2.DataType);
                int id = dataAccessor.Id;

                Type type = tuple.Item1;
                if (type.IsSubclassOf(typeof(DataRenderer))) {
                    _renderers[id] = type;
                }
            }
        }

        /// <summary>
        /// Fast lookup from DataAccessor id to renderer type.
        /// </summary>
        private static SparseArray<Type> _renderers = new SparseArray<Type>();

        /// <summary>
        /// Attempts to remove the data renderer for the given data type from the given GameObject.
        /// </summary>
        /// <param name="dataType">The type of data that we should remove a renderer for.</param>
        /// <param name="context">The GameObject that is potentially containing the
        /// renderer.</param>
        public static void TryRemoveRenderer(DataAccessor dataType, GameObject context) {
            int id = dataType.Id;
            if (_renderers.ContainsKey(id)) {
                Type rendererType = _renderers[id];
                var provider = context.GetComponent(rendererType);
                if (provider != null) {
                    if (Application.isEditor) {
                        UnityEngine.Object.DestroyImmediate(provider);
                    }
                    else {
                        UnityEngine.Object.Destroy(provider);
                    }
                }
            }
        }

        /// <summary>
        /// Adds the registered data renderer for the given data type to the given context
        /// GameObject. If there is no registered data renderer, then this method does not modify
        /// the context.
        /// </summary>
        /// <param name="dataType">The type of data to add a renderer for.</param>
        /// <param name="context">The object to add the renderer to.</param>
        /// <param name="entity">The entity that the renderer will operate on.</param>
        /// <returns></returns>
        public static Maybe<DataRenderer> TryAddRenderer(DataAccessor dataType, GameObject context,
            IQueryableEntity entity) {
            int id = dataType.Id;
            if (_renderers.ContainsKey(id)) {
                Type rendererType = _renderers[id];

                // only add the renderer if we don't have one already
                if (context.GetComponent(rendererType) == null) {
                    DataRenderer renderer = (DataRenderer)context.AddComponent(rendererType);
                    renderer.Initialize(entity);
                    return Maybe.Just(renderer);
                }
            }

            return Maybe<DataRenderer>.Empty;
        }
    }
}