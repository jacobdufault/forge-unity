using Forge.Entities;
using Forge.Unity;
using Forge.Utilities;
using FullInspector;
using UnityEditor;
using UnityEngine;

// This file contains a number of PropertyEditors which are directly associated with Forge. More
// directly, this file contains property editors for IQueryableEntity, IEntity, and ITemplate.

namespace Forge.Editing {
    [CustomPropertyEditor(typeof(IQueryableEntity))]
    public class QueryableEntityPropertyEditor : PropertyEditor<IQueryableEntity> {
        private BaseContainer TryGetContainer(IQueryableEntity element) {
            if (element is IEntity) {
                var container = EntityContainer.GetContainer((IEntity)element);
                if (container != null) {
                    return container;
                }
            }

            if (element is ITemplate) {
                var container = TemplateContainer.GetContainer((ITemplate)element);
                if (container != null) {
                    return container;
                }
            }

            if (element != null) {
                Debug.LogError("Data loss! Could not find a container for " + element +
                    "; make sure the right templates are loaded.");
            }

            return null;
        }

        public override IQueryableEntity Edit(Rect region, GUIContent label, IQueryableEntity element) {
            BaseContainer container = TryGetContainer(element);

            var result = (BaseContainer)EditorGUI.ObjectField(region, label, container, typeof(BaseContainer), true);

            if (result == null) {
                return null;
            }
            return result.QueryableEntity;
        }

        public override float GetElementHeight(GUIContent label, IQueryableEntity element) {
            return EditorStyles.objectField.CalcHeight(label, 100);
        }
    }

    /// <summary>
    /// A property editor that allows editing of IEntity fields/properties
    /// </summary>
    [CustomPropertyEditor(typeof(IEntity))]
    public class EntityPropertyEditor : PropertyEditor<IEntity> {
        public override IEntity Edit(Rect region, GUIContent label, IEntity element) {
            EntityContainer container = null;
            if (element != null) {
                container = EntityContainer.GetContainer(element);
                if (container == null) {
                    Debug.LogError("Data loss! Unable to find entity container for " + element +
                        "; element has been set to null as a result");
                }
            }

            var resultContainer = (EntityContainer)EditorGUI.ObjectField(region, label, container,
                typeof(EntityContainer), true);
            if (resultContainer == null) {
                return null;
            }

            IEntity result = (IEntity)resultContainer.QueryableEntity;

            // If our new entity has the same id as our original entity, then we just want to return
            // the original entity. If we're playing a game, our original entity might be a
            // RuntimeEntity but our container might contain a ContentEntity; switching the entity
            // to reference a ContentEntity could break systems
            if (element != null && element.UniqueId == result.UniqueId) {
                return element;
            }
            return result;
        }

        public override float GetElementHeight(GUIContent label, IEntity element) {
            return EditorStyles.objectField.CalcHeight(label, 100);
        }
    }

    /// <summary>
    /// A property editor that allows editing of EntityTemplate fields/references.
    /// </summary>
    [CustomPropertyEditor(typeof(ITemplate))]
    public class TemplatePropertyEditor : PropertyEditor<ITemplate> {
        public override ITemplate Edit(Rect region, GUIContent label, ITemplate element) {
            TemplateContainer container = null;
            if (element != null) {
                container = TemplateContainer.GetContainer(element);
                if (container == null) {
                    Debug.LogError("Data loss! Unable to find template container for " + element +
                        "; element has been set to null as a result; make sure the right " +
                        "templates are loaded");
                }
            }

            var resultContainer = (TemplateContainer)EditorGUI.ObjectField(region, label, container,
                typeof(TemplateContainer), true);
            if (resultContainer == null) {
                return null;
            }

            ITemplate result = (ITemplate)resultContainer.QueryableEntity;

            // If our new template has the same id as our original template, then we just want to
            // return the original template. If we're playing a game, our original template might be
            // a RuntimeTemplate but our container might contain a ContentTemplate; switching the
            // entity to reference a ContentTemplate would break systems which use the template to
            // instantiate objects
            if (element != null && element.TemplateId == result.TemplateId) {
                return element;
            }
            return result;
        }

        public override float GetElementHeight(GUIContent label, ITemplate element) {
            return EditorStyles.objectField.CalcHeight(label, 100);
        }
    }

    [CustomPropertyEditor(typeof(Real))]
    public class RealPropertyEditor : PropertyEditor<Real> {
        public override Real Edit(Rect region, GUIContent label, Real element) {
            float updated = EditorGUI.FloatField(region, label, element.AsFloat);
            return Real.Create(updated);
        }

        public override float GetElementHeight(GUIContent label, Real element) {
            return EditorStyles.label.CalcHeight(label, 1000);
        }
    }

    [CustomPropertyEditor(typeof(Vector2r))]
    public class Vector2rPropertyEditor : PropertyEditor<Vector2r> {
        public override Vector2r Edit(Rect region, GUIContent label, Vector2r element) {
            Vector2 updated = EditorGUI.Vector2Field(region, label,
                new Vector2(element.X.AsFloat, element.Z.AsFloat));

            return new Vector2r(updated.x, updated.y);
        }

        public override float GetElementHeight(GUIContent label, Vector2r element) {
            return EditorStyles.label.CalcHeight(label, 1000) * 2;
        }
    }
}