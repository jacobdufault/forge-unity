using Forge.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Forge.Unity {
    [CustomEditor(typeof(LevelDesigner))]
    public class LevelDesignerEditor : Editor {
        private Lazy<List<ISystemProvider>> _systemProviders = new Lazy<List<ISystemProvider>>(
            () => {
                return (
                    from assembly in AppDomain.CurrentDomain.GetAssemblies()
                    from type in assembly.GetTypes()

                    // the type has to implement ISystemProvider
                    where type.IsImplementationOf(typeof(ISystemProvider))

                    // the type has to have an empty constructor
                    where type.GetConstructor(Type.EmptyTypes) != null

                    select (ISystemProvider)Activator.CreateInstance(type)
                ).ToList();
            });

        private bool _debugFoldout = true;

        public override void OnInspectorGUI() {
            LevelDesigner designer = (LevelDesigner)target;

            bool hasLevelTemplates = designer.LevelTemplates != null;
            bool hasSharedTemplates = designer.SharedTemplates != null;
            bool hasSnapshot = designer.Snapshot != null;
            bool allowEditing = hasSnapshot && hasSharedTemplates && hasLevelTemplates;

            // template management

            ForgeEditorUtils.HorizontalGroup(() => {
                GUILayout.Label("SharedTemplateGroup", ForgeEditorUtils.HeaderStyle);

                ForgeEditorUtils.VerticalGroup(() => {
                    ForgeEditorUtils.EnableGroup(hasSharedTemplates == false, () => {
                        if (GUILayout.Button("Create")) {
                            designer.CreateSharedTemplateGroup();
                        }
                    });
                    ForgeEditorUtils.EnableGroup(hasSharedTemplates && hasSnapshot == false, () => {
                        if (GUILayout.Button("Destroy")) {
                            designer.ClearSharedTemplates();
                        }
                    });
                });

                GUILayout.FlexibleSpace();

                ForgeEditorUtils.VerticalGroup(() => {
                    ForgeEditorUtils.HorizontalGroup(() => {
                        ForgeEditorUtils.EnableGroup(hasSharedTemplates == false, () => {
                            if (GUILayout.Button("Load")) {
                                designer.ImportSharedTemplates(File.ReadAllText(designer.SharedTemplatePath));
                            }
                        });
                        ForgeEditorUtils.EnableGroup(hasSharedTemplates, () => {
                            if (GUILayout.Button("Save")) {
                                designer.ExportSharedTemplates();
                            }
                        });
                    }, GUILayout.ExpandWidth(false));

                    ForgeEditorUtils.HorizontalGroup(() => {
                        GUILayout.Label("Path", GUILayout.ExpandWidth(false));
                        designer.SharedTemplatePath = EditorGUILayout.TextField(designer.SharedTemplatePath, GUILayout.ExpandWidth(true));
                    }, GUILayout.ExpandWidth(false));
                });
            });

            EditorGUILayout.Separator();

            ForgeEditorUtils.HorizontalGroup(() => {
                GUILayout.Label("LevelTemplateGroup", ForgeEditorUtils.HeaderStyle);

                ForgeEditorUtils.VerticalGroup(() => {
                    ForgeEditorUtils.EnableGroup(hasLevelTemplates == false, () => {
                        if (GUILayout.Button("Create")) {
                            designer.CreateLevelTemplateGroup();
                        }
                    });
                    ForgeEditorUtils.EnableGroup(hasLevelTemplates && hasSnapshot == false, () => {
                        if (GUILayout.Button("Destroy")) {
                            designer.ClearLevelTemplates();
                        }
                    });
                });

                GUILayout.FlexibleSpace();

                ForgeEditorUtils.VerticalGroup(() => {
                    ForgeEditorUtils.HorizontalGroup(() => {
                        ForgeEditorUtils.EnableGroup(hasLevelTemplates == false, () => {
                            if (GUILayout.Button("Load")) {
                                designer.ImportLevelTemplates(File.ReadAllText(designer.LevelTemplatePath));
                            }
                        });
                        ForgeEditorUtils.EnableGroup(hasLevelTemplates, () => {
                            if (GUILayout.Button("Save")) {
                                designer.ExportLevelTemplates();
                            }
                        });
                    }, GUILayout.ExpandWidth(false));

                    ForgeEditorUtils.HorizontalGroup(() => {
                        GUILayout.Label("Path", GUILayout.ExpandWidth(false));
                        designer.LevelTemplatePath = EditorGUILayout.TextField(designer.LevelTemplatePath, GUILayout.ExpandWidth(true));
                    }, GUILayout.ExpandWidth(false));
                });
            });

            EditorGUILayout.Separator();

            // snapshot management

            ForgeEditorUtils.HorizontalGroup(() => {
                GUILayout.Label("GameSnapshot", ForgeEditorUtils.HeaderStyle);

                ForgeEditorUtils.VerticalGroup(() => {
                    ForgeEditorUtils.EnableGroup(hasLevelTemplates && hasSharedTemplates && hasSnapshot == false, () => {
                        if (GUILayout.Button("Create")) {
                            designer.CreateSnapshot();
                        }
                    });
                    ForgeEditorUtils.EnableGroup(hasSnapshot, () => {
                        if (GUILayout.Button("Destroy")) {
                            designer.ClearSnapshot();
                        }
                    });
                });

                GUILayout.FlexibleSpace();

                ForgeEditorUtils.VerticalGroup(() => {
                    ForgeEditorUtils.HorizontalGroup(() => {
                        ForgeEditorUtils.EnableGroup(hasLevelTemplates && hasSharedTemplates && hasSnapshot == false, () => {
                            if (GUILayout.Button("Load")) {
                                designer.ImportSnapshot(File.ReadAllText(designer.SnapshotPath));
                            }
                        });
                        ForgeEditorUtils.EnableGroup(hasSnapshot, () => {
                            if (GUILayout.Button("Save")) {
                                designer.ExportSnapshot();
                            }
                        });
                    }, GUILayout.ExpandWidth(false));

                    ForgeEditorUtils.HorizontalGroup(() => {
                        GUILayout.Label("Path", GUILayout.ExpandWidth(false));
                        designer.SnapshotPath = EditorGUILayout.TextField(designer.SnapshotPath, GUILayout.ExpandWidth(true));
                    }, GUILayout.ExpandWidth(false));
                });
            });

            EditorGUILayout.Separator();

            ForgeEditorUtils.EnableGroup(hasSnapshot, () => {
                if (GUILayout.Button("Add Entity")) {
                    designer.AddEntity();
                }
            });

            ForgeEditorUtils.EnableGroup(hasSharedTemplates, () => {
                if (GUILayout.Button("Add Shared Template")) {
                    designer.AddSharedTemplate();
                }
            });

            ForgeEditorUtils.EnableGroup(hasLevelTemplates, () => {
                if (GUILayout.Button("Add Level Template")) {
                    designer.AddLevelTemplate();
                }
            });

            ForgeEditorUtils.EnableGroup(allowEditing, () => {
                EditorGUILayout.Separator();

                if (GUILayout.Button("Clear Systems")) {
                    designer.Snapshot.Systems.Clear();
                }

                GUILayout.Label("Inject Systems from a System Provider");
                GUILayout.BeginVertical();
                foreach (ISystemProvider provider in _systemProviders.Value) {
                    if (GUILayout.Button(provider.ToString())) {
                        designer.Snapshot.Systems.AddRange(provider.GetSystems());
                    }
                }
                GUILayout.EndVertical();
            });

            if (_debugFoldout = EditorGUILayout.Foldout(_debugFoldout, "Debug Information")) {
                GUILayout.Label("Containers");
                GUILayout.TextArea(TemplateContainer.PrettyCachedContainers + "\n" + EntityContainer.PrettyCachedContainers);
                GUILayout.Label("Shared Template Restoration State");
                GUILayout.TextArea(designer.SavedSharedTemplateState ?? "");
                GUILayout.Label("Level Template Restoration State");
                GUILayout.TextArea(designer.SavedLevelTemplateState ?? "");
                GUILayout.Label("Snapshot Restoration State");
                GUILayout.TextArea(designer.SavedSnapshotState ?? "");
            }
        }

        private string DrawItem(Rect position, string item) {
            return EditorGUI.TextField(position, item);
        }

        private void DrawEmpty() {
        }

    }
}