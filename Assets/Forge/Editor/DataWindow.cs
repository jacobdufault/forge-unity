using Forge.Entities;
using Forge.Unity;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace Forge.Editing {
    internal class DataInspectorView {

        /// <summary>
        /// The data types which are currently hidden / folded in. If the key in the dictionary is
        /// negative, then the key is equal to -(key + 1) = entityId. Otherwise, if the key is
        /// positive, then it is the template id. The HashSet contains the ids of the hidden data.
        /// </summary>
        private Dictionary<int, HashSet<int>> _hidden = new Dictionary<int, HashSet<int>>();

        private bool IsHidden(IQueryableEntity entity, Type dataType) {
            int id;
            if (entity is ITemplate) {
                id = ((ITemplate)entity).TemplateId;
            }
            else {
                id = -(((IEntity)entity).UniqueId + 1);
            }

            HashSet<int> hiddenData;
            if (_hidden.TryGetValue(id, out hiddenData) == false) {
                return false;
            }

            return hiddenData.Contains(new DataAccessor(dataType).Id);
        }

        private void SetHidden(IQueryableEntity entity, Type dataType, bool value) {
            int id;
            if (entity is ITemplate) {
                id = ((ITemplate)entity).TemplateId;
            }
            else {
                id = -(((IEntity)entity).UniqueId + 1);
            }

            if (value == true) {
                HashSet<int> hiddenData;
                if (_hidden.TryGetValue(id, out hiddenData) == false) {
                    hiddenData = new HashSet<int>();
                    _hidden[id] = hiddenData;
                }

                hiddenData.Add(new DataAccessor(dataType).Id);
            }

            else {
                HashSet<int> hiddenData;
                if (_hidden.TryGetValue(id, out hiddenData)) {
                    hiddenData.Remove(new DataAccessor(dataType).Id);
                }
            }
        }

        private enum DataEditState {
            Current,
            Previous
        };

        /// <summary>
        /// The data types which we are editing the current values for. The key in the dictionary
        /// maps to the IEntity unique ids (as only IEntitys can have previous data) and the value
        /// in the HashSet maps to the data accessor ids which are on previous data.
        /// </summary>
        private Dictionary<int, HashSet<int>> _previous = new Dictionary<int, HashSet<int>>();

        /// <summary>
        /// Returns the current editing state for the given data type in the given entity.
        /// </summary>
        private DataEditState GetEditState(IQueryableEntity entity, Type dataType) {
            if (CanEditPrevious(entity, dataType)) {
                HashSet<int> value;
                if (_previous.TryGetValue(((IEntity)entity).UniqueId, out value)) {
                    int id = new DataAccessor(dataType).Id;
                    return value.Contains(id) ? DataEditState.Previous : DataEditState.Current;
                }
            }

            // ITemplate / Data.NonVersioned don't support previous
            return DataEditState.Current;
        }

        /// <summary>
        /// Returns true if the given data type has a previous instance that can be edited for the
        /// given entity.
        /// </summary>
        private bool CanEditPrevious(IQueryableEntity entity, Type dataType) {
            return entity is IEntity && typeof(Data.IVersioned).IsAssignableFrom(dataType);
        }

        /// <summary>
        /// Sets the editing state for the given data type in the given entity to the given value.
        /// </summary>
        private void SetEditState(IQueryableEntity entity, Type dataType, DataEditState value) {
            int entityId = ((IEntity)entity).UniqueId;

            HashSet<int> hashSet;
            if (_previous.TryGetValue(entityId, out hashSet) == false) {
                hashSet = new HashSet<int>();
                _previous[entityId] = hashSet;
            }

            int dataId = new DataAccessor(dataType).Id;
            switch (value) {
                case DataEditState.Current:
                    hashSet.Remove(dataId);
                    break;
                case DataEditState.Previous:
                    hashSet.Add(dataId);
                    break;
            }
        }

        /// <summary>
        /// Returns the data header that should be shown above the data type.
        /// </summary>
        private static string GetDataHeader(Type dataType, IQueryableEntity queryableEntity,
            DataEditState editState) {
            StringBuilder result = new StringBuilder();
            result.Append(dataType.Name);

            if (queryableEntity is IEntity) {
                result.Append(editState == DataEditState.Current ? " (current)" : " (previous)");

                IEntity entity = (IEntity)queryableEntity;
                DataAccessor accessor = new DataAccessor(dataType);
                if (entity.WasAdded(accessor)) {
                    result.Append(" (added)");
                }
                if (entity.WasModified(accessor)) {
                    result.Append(" (modified)");
                }
                if (entity.WasRemoved(accessor)) {
                    result.Append(" (removed)");
                }
            }

            return result.ToString();
        }

        /// <summary>
        /// Removes the given data type from the entity.
        /// </summary>
        private static void RemoveData(IQueryableEntity entity, Type dataType) {
            var accessor = new DataAccessor(dataType);

            if (entity is IEntity) {
                ((IEntity)entity).RemoveData(accessor);
            }
            else if (entity is ITemplate) {
                ((ITemplate)entity).RemoveDefaultData(accessor);
            }
        }

        /// <summary>
        /// Returns the data instance that should be edited based on the given edit state and data
        /// type.
        /// </summary>
        private static Data.IData GetEditedData(IQueryableEntity entity,
            DataEditState editState, Type dataType) {
            var accessor = new DataAccessor(dataType);

            switch (editState) {
                case DataEditState.Current:
                    return entity.Current(accessor);
                case DataEditState.Previous:
                    return entity.Previous(accessor);
            }

            throw new InvalidOperationException("Unknown edit state");
        }

        public void DrawDataInspector(IQueryableEntity entity, Type dataType, GameObject context) {
            DataEditState editState = GetEditState(entity, dataType);
            bool removedData = false;

            EditorGUILayout.BeginHorizontal();
            {
                SetHidden(entity, dataType, ForgeEditorUtils.DrawFoldout(IsHidden(entity, dataType)));

                EditorGUILayout.LabelField(GetDataHeader(dataType, entity, editState),
                    ForgeEditorUtils.HeaderStyle);

                if (CanEditPrevious(entity, dataType)) {
                    bool editingCurrent = ForgeEditorUtils.DrawPrettyToggle(
                        editState == DataEditState.Current,
                        "Switch to Previous", "Switch to Current",
                        GUILayout.ExpandWidth(false));

                    SetEditState(entity, dataType,
                        editingCurrent ? DataEditState.Current : DataEditState.Previous);
                }

                if (GUILayout.Button("X", GUILayout.ExpandWidth(false))) {
                    RemoveData(entity, dataType);
                    removedData = true;
                }
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Separator();

            // It is possible that we removed the data instance from the entity as above. In that
            // case, we don't want to actually edit it.
            if (removedData) {
                return;
            }

            // edit the data
            if (IsHidden(entity, dataType) == false) {
                IDataInspector inspector = DataInspector.Get(dataType);
                Data.IData data = GetEditedData(entity, editState, dataType);
                try {
                    inspector.Edit(data, context);
                }
                catch (ExitGUIException) { throw; }
                catch (Exception e) {
                    Debug.LogError("While running inspector caught exception " + e);
                }
            }
        }

        public void DrawSceneGUI(Data.IData data, GameObject context) {
            // edit the data
            IDataInspector inspector = DataInspector.Get(data.GetType());
            inspector.OnSceneGUI(data, context);
        }
    }

    public class DataWindow : EditorWindow {
        /// <summary>
        /// Makes it simple to create a new DataWindow.
        /// </summary>
        [MenuItem("Forge/Data View")]
        private static void Init() {
            // Get existing open window or if none, make a new one
            EditorWindow.GetWindow(typeof(DataWindow));
        }

        /// <summary>
        /// How far the inspector has scrolled
        /// </summary>
        private Vector2 _inspectorScroll;

        private DataInspectorView _dataInspectorView;

        /// <summary>
        /// Setup references.
        /// </summary>
        protected void OnEnable() {
            title = "Data";
            _dataInspectorView = new DataInspectorView();
            SceneView.onSceneGUIDelegate += this.OnSceneGUI;
        }

        protected void OnDisable() {
            SceneView.onSceneGUIDelegate -= this.OnSceneGUI;
        }

        /// <summary>
        /// We want to constantly redraw the inspector.
        /// </summary>
        protected void Update() {
            Repaint();
        }

        public void DrawAddData(IQueryableEntity entity) {
            GUILayout.Space(5);

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            Rect rect = GUILayoutUtility.GetRect(new GUIContent("Add Data"), new GUIStyle("Button"), GUILayout.Width(250));
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            _lastAddDataRect = GUIToScreenRect(rect);
            if (GUI.Button(rect, "Add Data")) {
                AddDataMenuItem();
            }
        }

        private static Rect _lastAddDataRect;

        // # -> shift & -> alt
        [MenuItem("Forge/Add Data #&d")]
        public static void AddDataMenuItem() {
            // if we don't have a queryable entity, then we can't add data to anything, so don't
            // bother showing the popup
            if (ForgeEditorUtils.TryGetQueryableEntity() == null) {
                return;
            }

            AddDataPopupWindow.Toggle(_lastAddDataRect);
        }

        internal static Rect GUIToScreenRect(Rect guiRect) {
            Vector2 vector2 = GUIUtility.GUIToScreenPoint(new Vector2(guiRect.x, guiRect.y));
            guiRect.x = vector2.x;
            guiRect.y = vector2.y;
            return guiRect;
        }

        /// <summary>
        /// Called when we have an entity to inspect.
        /// </summary>
        private void DrawInspector(IQueryableEntity entity, GameObject context) {
            _inspectorScroll = EditorGUILayout.BeginScrollView(_inspectorScroll);

            EditorGUILayout.LabelField("Metadata", ForgeEditorUtils.HeaderStyle);
            entity.PrettyName = EditorGUILayout.TextField("Pretty Name", entity.PrettyName);
            if (GUI.changed) {
                BaseContainer container = context.GetComponent<BaseContainer>();
                container.UpdateName();
            }
            ForgeEditorUtils.DrawSeperator();

            foreach (DataAccessor accessor in entity.SelectData(/*includeRemoved:*/ true)) {
                Type dataType = entity.Current(accessor).GetType();
                _dataInspectorView.DrawDataInspector(entity, dataType, context);
                ForgeEditorUtils.DrawSeperator();
            }

            DrawAddData(entity);
            EditorGUILayout.EndScrollView();
        }

        /// <summary>
        /// Called when there is no GameObject currently selected.
        /// </summary>
        private void DrawNothingSelected() {
            EditorGUILayout.HelpBox("Select a GameObject with an EntityContainer or a " +
                "TemplateContainer component to view the Forge data stored inside of it", MessageType.Info);
        }

        /// <summary>
        /// Called when there is a GameObject selected, but it does not have an EntityContainer.
        /// </summary>
        private void DrawNoContainer(GameObject gameObject) {
            EditorGUILayout.HelpBox("There is no EntityContainer or TemplateContainer in the " +
                "selected GameObject", MessageType.Info);
        }

        private void OnGUI() {
            GameObject selected = Selection.activeGameObject;
            if (selected == null) {
                DrawNothingSelected();
                return;
            }

            BaseContainer container = selected.GetComponent<BaseContainer>();
            if (container == null) {
                DrawNoContainer(selected);
                return;
            }

            DrawInspector(container.QueryableEntity, selected);
            SceneView.RepaintAll();
        }

        protected void OnSceneGUI(SceneView sceneView) {
            GameObject selected = Selection.activeGameObject;
            if (selected == null) {
                return;
            }

            BaseContainer container = selected.GetComponent<BaseContainer>();
            if (container == null) {
                return;
            }

            foreach (DataAccessor accessor in container.QueryableEntity.SelectData()) {
                Data.IData data = container.QueryableEntity.Current(accessor);
                _dataInspectorView.DrawSceneGUI(data, selected);
            }
        }
    }
}