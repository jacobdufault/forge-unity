using Forge.Entities;
using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Forge.Unity {
    [CustomEditor(typeof(GameEngineManager))]
    public class GameEngineManagerEditor : Editor {
        private string SavePath;

        public override void OnInspectorGUI() {
            GameEngineManager engine = (GameEngineManager)target;

            SavePath = EditorGUILayout.TextField("Save Path", SavePath);
            if (GUILayout.Button("Save")) {
                try {
                    string json = LevelManager.SaveSnapshot(engine.Engine.TakeSnapshot());
                    File.WriteAllText(SavePath, json);
                }
                catch (Exception e) {
                    Debug.LogError("While trying to take snapshot, got exception " + e);
                }
            }
        }
    }
}