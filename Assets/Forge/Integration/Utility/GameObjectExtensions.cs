using System;
using System.Collections.Generic;
using UnityEngine;

namespace Forge.Unity {
    public static class GameObjectExtensions {
        /// <summary>
        /// Gets the first instance of the given component any of the parents, or null.
        /// </summary>
        public static T GetComponentInParent<T>(this GameObject obj) where T : Component {
            while (obj != null) {
                T instance = obj.GetComponent<T>();
                if (instance != null) {
                    return instance;
                }

                if (obj.transform.parent == null) {
                    obj = null;
                }
                else {
                    obj = obj.transform.parent.gameObject;
                }
            }

            return null;
        }
    }
}