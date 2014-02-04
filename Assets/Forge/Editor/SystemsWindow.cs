using Forge.Entities;
using Forge.Unity;
using Forge.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Forge.Editing {
    public class SystemsWindow : EditorWindow {
        [MenuItem("Forge/Systems View")]
        private static void Init() {
            // Get existing open window or if none, make a new one
            EditorWindow.GetWindow(typeof(SystemsWindow));
        }

        private SystemsView _systemsView;

        protected void OnEnable() {
            title = "Systems";
            _systemsView = new SystemsView();
        }

        protected void Update() {
            Repaint();
        }

        protected void OnGUI() {
            _systemsView.DrawSystemsInterested();
        }

        internal class SystemsView {
            private struct SystemSuccess {
                public ISystem System;
                public List<Type> ContainedTypes;

                public void DoGUI() {
                    string[] passedNames = ContainedTypes.Select(t => t.Name).ToArray();

                    string satisfied = string.Join(", ", passedNames);

                    EditorGUILayout.BeginVertical();
                    GUILayout.Label(System.GetType().FullName, ForgeEditorUtils.BoldStyle);
                    GUILayout.Label("Success: " + satisfied, ForgeEditorUtils.RegularStyle);
                    EditorGUILayout.EndVertical();
                }
            }

            private struct SystemPartial {
                public ISystem System;
                public List<Type> MissingTypes;
                public List<Type> ContainedTypes;

                public void DoGUI() {
                    string[] passedNames = ContainedTypes.Select(t => t.Name).ToArray();
                    string[] failedNames = MissingTypes.Select(t => t.Name).ToArray();

                    string satisfied = string.Join(", ", passedNames);
                    string failed = string.Join(", ", failedNames);

                    EditorGUILayout.BeginVertical();
                    GUILayout.Label(System.GetType().FullName, ForgeEditorUtils.BoldStyle);
                    if (passedNames.Count() > 0) {
                        GUILayout.Label("Success: " + satisfied, ForgeEditorUtils.RegularStyle);
                    }
                    GUILayout.Label("Failed: " + failed, ForgeEditorUtils.RegularStyle);
                    EditorGUILayout.EndVertical();
                }
            }

            private struct SystemFail {
                public ISystem System;

                public void DoGUI() {
                    Type[] dataTypes = ((ITriggerFilterProvider)System).RequiredDataTypes;
                    string[] requiredNames = dataTypes.Select(t => t.Name).ToArray();

                    string required = string.Join(", ", requiredNames);

                    EditorGUILayout.BeginVertical();
                    GUILayout.Label(System.GetType().FullName, ForgeEditorUtils.BoldStyle);
                    GUILayout.Label("Required: " + required, ForgeEditorUtils.RegularStyle);
                    EditorGUILayout.EndVertical();
                }
            }

            /// <summary>
            /// Returns all of the ISystems that are associated with the LevelDesigner that is
            /// hierarchically associated with the given selected object.
            /// </summary>
            private List<ISystem> GetSystems(GameObject selected) {
                var designer = selected.GetComponentInParent<LevelDesigner>();
                return designer.Snapshot.Systems;
            }

            private void ComputeSystems(GameObject selected, IQueryableEntity entity,
                out List<SystemSuccess> success, out List<SystemPartial> partial,
                out List<SystemFail> failed, out List<ISystem> generic) {

                success = new List<SystemSuccess>();
                partial = new List<SystemPartial>();
                failed = new List<SystemFail>();
                generic = new List<ISystem>();

                foreach (ISystem system in GetSystems(selected)) {
                    if (system is ITriggerFilterProvider) {
                        if (entity == null) {
                            failed.Add(new SystemFail() {
                                System = system
                            });
                        }

                        else {
                            ITriggerFilterProvider filter = (ITriggerFilterProvider)system;

                            List<Type> missing, contained;
                            ComputeDataTypes(filter.RequiredDataTypes, entity, out missing, out contained);

                            if (contained.Count == 0) {
                                failed.Add(new SystemFail() {
                                    System = filter
                                });
                            }
                            else if (contained.Count > 0) {
                                if (missing.Count == 0) {
                                    success.Add(new SystemSuccess() {
                                        System = system,
                                        ContainedTypes = contained
                                    });
                                }
                                else {
                                    partial.Add(new SystemPartial() {
                                        System = system,
                                        ContainedTypes = contained,
                                        MissingTypes = missing
                                    });
                                }
                            }
                        }
                    }

                    else {
                        generic.Add(system);
                    }
                }
            }

            private void ComputeDataTypes(IEnumerable<Type> requiredTypes, IQueryableEntity entity,
                out List<Type> missing, out List<Type> contained) {
                missing = new List<Type>();
                contained = new List<Type>();

                foreach (var type in requiredTypes) {
                    DataAccessor accessor = new DataAccessor(type);
                    if (entity.ContainsData(accessor)) {
                        contained.Add(type);
                    }
                    else {
                        missing.Add(type);
                    }
                }
            }

            private Vector2 _scroller;
            private string _currentFilter = "";
            private const string FilterFocusName = "SystemsWindowSearchFocus";

            private bool PassesSearchFilter(ISystem system) {
                if (string.IsNullOrEmpty(_currentFilter)) {
                    return true;
                }

                return system.GetType().FullName.IndexOf(_currentFilter, StringComparison.OrdinalIgnoreCase) != -1;
            }

            public void DrawSystemsInterested() {
                if (Selection.activeGameObject == null ||
                    Selection.activeGameObject.GetComponentInParent<LevelDesigner>() == null) {

                    EditorGUILayout.HelpBox("Select a GameObject that contains a LevelDesigner to view the ISystems", MessageType.Info);
                    return;
                }

                if (Selection.activeGameObject.GetComponentInParent<LevelDesigner>().Snapshot == null) {
                    EditorGUILayout.HelpBox("Load a snapshot to view the the ISystems in the LevelDesigner", MessageType.Info);
                    return;
                }

                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();

                GUI.SetNextControlName(FilterFocusName);
                _currentFilter = GUILayout.TextField(_currentFilter, GUI.skin.FindStyle("ToolbarSeachTextField"), GUILayout.MaxWidth(300));
                if (GUILayout.Button("", GUI.skin.FindStyle("ToolbarSeachCancelButton"))) {
                    _currentFilter = "";
                    GUI.FocusControl(null);
                }
                GUILayout.EndHorizontal();

                IQueryableEntity entity = ForgeEditorUtils.TryGetQueryableEntity();

                _scroller = EditorGUILayout.BeginScrollView(_scroller);

                List<SystemSuccess> success;
                List<SystemPartial> partial;
                List<SystemFail> failed;
                List<ISystem> generic;
                ComputeSystems(Selection.activeGameObject, entity, out success, out partial,
                    out failed, out generic);

                if (success.Count > 0) {
                    GUILayout.Label("Systems Processing Selected Entity", ForgeEditorUtils.HeaderStyle);
                    foreach (var system in success) {
                        if (PassesSearchFilter(system.System)) {
                            system.DoGUI();
                            GUILayout.Space(3);
                        }
                    }
                    GUILayout.Space(5);
                }

                if (partial.Count > 0) {
                    GUILayout.Label("Partially Fulfilled Systems", ForgeEditorUtils.HeaderStyle);
                    foreach (var system in partial) {
                        if (PassesSearchFilter(system.System)) {
                            system.DoGUI();
                            GUILayout.Space(3);
                        }
                    }
                    GUILayout.Space(5);
                }

                if (failed.Count > 0) {
                    GUILayout.Label("Other Systems", ForgeEditorUtils.HeaderStyle);
                    foreach (var system in failed) {
                        if (PassesSearchFilter(system.System)) {
                            system.DoGUI();
                            GUILayout.Space(3);
                        }
                    }
                    GUILayout.Space(5);
                }

                if (generic.Count > 0) {
                    GUILayout.Label("Generic Systems", ForgeEditorUtils.HeaderStyle);
                    foreach (var system in generic) {
                        if (PassesSearchFilter(system)) {
                            GUILayout.Label(system.GetType().FullName, ForgeEditorUtils.BoldStyle);
                            GUILayout.Space(3);
                        }
                    }
                }

                EditorGUILayout.EndScrollView();
            }
        }
    }
}