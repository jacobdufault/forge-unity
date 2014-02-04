using UnityEditor;
using UnityEngine;

namespace Forge.Unity {
    [CustomEditor(typeof(TemplateContainer))]
    public class TemplateContainerEditor : Editor {
        public override void OnInspectorGUI() {
            TemplateContainer container = target as TemplateContainer;

            container.Template.PrettyName = EditorGUILayout.TextField("PrettyName",
                container.Template.PrettyName);

            ForgeEditorUtils.DrawAd();

            if (GUI.changed) {
                container.UpdateName();
                EditorUtility.SetDirty(target);
            }

        }
    }
}