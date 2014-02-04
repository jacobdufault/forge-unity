using UnityEditor;
using UnityEngine;

namespace Forge.Unity {
    [CustomEditor(typeof(EntityContainer))]
    public class EntityContainerEditor : Editor {
        public override void OnInspectorGUI() {
            EntityContainer container = target as EntityContainer;

            container.Entity.PrettyName = EditorGUILayout.TextField("PrettyName",
                container.Entity.PrettyName);

            ForgeEditorUtils.DrawAd();

            if (GUI.changed) {
                container.UpdateName();
                EditorUtility.SetDirty(target);
            }
        }
    }
}