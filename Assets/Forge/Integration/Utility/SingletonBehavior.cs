using Forge.Utilities;
using UnityEngine;

namespace Forge.Unity {
    /// <summary>
    /// A MonoBehaviour base type that provides facilities for only a single instance of an object.
    /// </summary>
    /// <typeparam name="StoredType">The derived class type; used for fetching the global
    /// instance</typeparam> <typeparam name="InterfaceType">The interface that clients will work
    /// with</typeparam>
    public abstract class SingletonBehavior<StoredType, InterfaceType> : MonoBehaviour
        where StoredType : MonoBehaviour, InterfaceType
        where InterfaceType : class {

        private static T FindSafeObjectOfType<T>() where T : Component {
            T instance = (T)FindObjectOfType(typeof(T));

            if (instance == null) {
                Debug.LogError("Unable to find object of type " + typeof(T));
            }

            return instance;
        }

        private static Reference<InterfaceType> _reference;

        private static object _lock = new object();

        /// <summary>
        /// Check to determine if there is currently an instance of the given singleton. If this is
        /// false, then simply call Instance to create an instance.
        /// </summary>
        public static bool HasInstance {
            get {
                return FindObjectOfType(typeof(StoredType)) != null;
            }
        }

        /// <summary>
        /// Returns the instance of the singleton type
        /// </summary>
        public static InterfaceType Instance {
            get {
                lock (_lock) {
                    if (_reference == null) {
                        _reference = new Reference<InterfaceType>(FindSafeObjectOfType<StoredType>());
                    }

                    return _reference.Value;
                }
            }
        }

        protected virtual void OnEnable() {
            // When interacting with a singleton using, say, ExecuteInEditor, caching will get
            // messed up where the cached reference refers to a different object instance than the
            // one that most recently went through the serialize/deserialize cycle. At least, this
            // is the most logical explanation that I can think of.
            _reference = null;
        }

        /// <summary>
        /// When Unity quits, it destroys objects in a random order. In principle, a Singleton is
        /// only destroyed when application quits. If any script calls Instance after it have been
        /// destroyed, it will create a buggy ghost object that will stay on the Editor scene even
        /// after stopping playing the Application. Really bad! So, this was made to be sure we're
        /// not creating that buggy ghost object.
        /// </summary>
        protected virtual void OnDestroy() {
            _reference = null;
        }
    }
}