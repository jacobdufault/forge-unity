using System;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace Forge.Unity {
    public static class ForgeEditorUtility {
        public static bool IsCompiling {
            get {
#if UNITY_EDITOR
                return EditorApplication.isCompiling;
#else
                return false;
#endif
            }
        }

        public static bool IsPlaying {
            get {
#if UNITY_EDITOR
                return EditorApplication.isPlaying;
#else
                return false;
#endif
            }
        }

        public static bool IsPlayingOrWillChangePlaymode {
            get {
#if UNITY_EDITOR
                return EditorApplication.isPlayingOrWillChangePlaymode;
#else
                return false;
#endif
            }
        }
    }
}