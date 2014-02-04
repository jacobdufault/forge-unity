using Forge.Entities;
using Forge.Unity;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Forge.Editing {
    public class AddDataPopupWindow : EditorWindow {
        private static AddDataPopupWindow _window;

        /// <summary>
        /// Opens the window at the given rect if it is currently closed, or closes the window if it
        /// is currently open.
        /// </summary>
        /// <param name="rect">Where to open the window at.</param>
        public static void Toggle(Rect rect) {
            if (_enabled && _window != null) {
                _window.Close();
                return;
            }

            if (_window == null) {
                _window = ScriptableObject.CreateInstance<AddDataPopupWindow>();
            }

            const int width = 300;
            const int height = 300;
            rect.y += 3;
            rect.x += (float)((rect.width - width) / 2.0);

            _window.ShowAsDropDown(rect, new Vector2(width, height));
        }

        private static bool _enabled;

        protected void OnDisable() {
            _enabled = false;
        }

        protected void OnEnable() {
            _enabled = true;
            if (_currentFilter == null) {
                _currentFilter = "";
            }
        }

        private Lazy<List<Type>> _cachedDataSubtypes = new Lazy<List<Type>>(() => {
            var types = new List<Type>();

            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies()) {
                foreach (var type in assembly.GetTypes()) {
                    if (type.IsAbstract) {
                        continue;
                    }

                    if (typeof(Data.IData).IsAssignableFrom(type) == false) {
                        continue;
                    }

                    types.Add(type);
                }
            }

            return types;
        });

        /// <summary>
        /// Returns the list of data types available after being filtered by the given filter.
        /// </summary>
        private List<Type> GetFilteredTypes(string filter) {
            List<Type> result = new List<Type>();

            foreach (var candidateType in _cachedDataSubtypes.Value) {
                string candidateName = candidateType.FullName;
                string[] subwords = filter.Split(new[] { ' ', '.' },
                    StringSplitOptions.RemoveEmptyEntries);

                bool contains = true;
                foreach (string subword in subwords) {
                    if (candidateName.IndexOf(subword, StringComparison.OrdinalIgnoreCase) < 0) {
                        contains = false;
                        break;
                    }
                }

                if (contains) {
                    result.Add(candidateType);
                }
            }

            return result;
        }

        /// <summary>
        /// Returns true if the given data type can be added to the given entity.
        /// </summary>
        private static bool CanAdd(IQueryableEntity entity, Type dataType) {
            if (entity == null) {
                return true;
            }

            var accessor = new DataAccessor(dataType);
            return entity.ContainsData(accessor) == false;
        }

        private void AddDataInstance(Type dataType, IQueryableEntity queryableEntity) {
            if (queryableEntity is IEntity) {
                DataAccessor accessor = new DataAccessor(dataType);
                ((IEntity)queryableEntity).AddData(accessor);
            }

            else if (queryableEntity is ITemplate) {
                var data = (Data.IData)Activator.CreateInstance(dataType);
                ((ITemplate)queryableEntity).AddDefaultData(data);
            }
        }

        private string _currentFilter;
        private Vector2 _dataAdderScroll;

        private const string FilterFocusName = "DataAddFilter";

        public static bool KeyPressed(KeyCode key) {
            return Event.current.type == EventType.KeyUp && Event.current.keyCode == key;
        }

        /// <summary>
        /// Returns the data types which can be added to the given entity.
        /// </summary>
        private List<Type> SelectAddableDataTypes(IQueryableEntity entity, IEnumerable<Type> dataTypes) {
            return (from dataType in dataTypes
                    where CanAdd(entity, dataType)
                    select dataType).ToList();
        }

        protected void OnGUI() {
            GUI.FocusControl(FilterFocusName);

            IQueryableEntity entity = ForgeEditorUtils.TryGetQueryableEntity();
            if (entity == null) {
                GUILayout.Label("Select an Entity/Template Container", ForgeEditorUtils.HeaderStyle);
                return;
            }

            List<Type> candidateTypes = GetFilteredTypes(_currentFilter);

            List<Type> addableTypes = SelectAddableDataTypes(entity, candidateTypes);
            EnsureFocus(addableTypes);
            if (KeyPressed(KeyCode.DownArrow)) {
                Event.current.Use();
                FocusNext(addableTypes);
            }
            if (KeyPressed(KeyCode.UpArrow)) {
                Event.current.Use();
                FocusPrevious(addableTypes);
            }

            GUI.Box(new Rect(0, 0, position.width, position.height), GUIContent.none);

            // filter
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            GUI.SetNextControlName(FilterFocusName);
            _currentFilter = GUILayout.TextField(_currentFilter, GUI.skin.FindStyle("ToolbarSeachTextField"), GUILayout.MaxWidth(150));
            if (GUILayout.Button("", GUI.skin.FindStyle("ToolbarSeachCancelButton"))) {
                _currentFilter = "";
                GUI.FocusControl(null);
            }

            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            // data buttons
            GUIStyle buttonStyle = new GUIStyle("Button");
            buttonStyle.alignment = TextAnchor.MiddleLeft;

            GUIStyle focusButtonStyle = new GUIStyle("Button");
            focusButtonStyle.fontStyle = FontStyle.Bold;
            focusButtonStyle.alignment = TextAnchor.MiddleLeft;

            _dataAdderScroll = EditorGUILayout.BeginScrollView(_dataAdderScroll, false, false);
            {
                foreach (var candidateType in candidateTypes) {

                    EditorGUI.BeginDisabledGroup(CanAdd(entity, candidateType) == false);

                    bool add = false;
                    if (_focused == candidateType) {
                        add = GUILayout.Button(candidateType.FullName, focusButtonStyle);
                        if (KeyPressed(KeyCode.Return)) {
                            add = true;
                            Event.current.Use();
                        }
                    }

                    else {
                        add = GUILayout.Button(candidateType.FullName, buttonStyle);
                    }

                    if (add) {
                        AddDataInstance(candidateType, entity);
                        Close();
                    }

                    EditorGUI.EndDisabledGroup();
                }
            }
            EditorGUILayout.EndScrollView();
        }

        private Type _focused;

        private void EnsureFocus(List<Type> candidateTypes) {
            if (candidateTypes.Count == 0) {
                _focused = null;
                return;
            }

            if (_focused == null) {
                _focused = candidateTypes[0];
                return;
            }

            if (candidateTypes.IndexOf(_focused) == -1) {
                _focused = candidateTypes[0];
                return;
            }
        }

        private void FocusNext(List<Type> candidateTypes) {
            if (candidateTypes.Count == 0) {
                return;
            }

            int index = candidateTypes.IndexOf(_focused);
            int nextIndex = (index + 1) % candidateTypes.Count;
            _focused = candidateTypes[nextIndex];
        }

        private void FocusPrevious(List<Type> candidateTypes) {
            if (candidateTypes.Count == 0) {
                return;
            }

            int index = candidateTypes.IndexOf(_focused);
            int prevIndex = Math.Max(0, index - 1);
            _focused = candidateTypes[prevIndex];
        }
    }
}