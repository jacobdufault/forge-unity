using Forge.Entities;
using Forge.Utilities;
using Newtonsoft.Json;
using System;
using System.IO;
using UnityEngine;

namespace Forge.Unity {
    [ExecuteInEditMode]
    public class AutomaticBackupComponent : MonoBehaviour {
        /// <summary>
        /// By default we save the editor every 5 minutes
        /// </summary>
        public float BackupIntervalInSeconds = 5 * 60;

        /// <summary>
        /// The number of backups to store.
        /// </summary>
        public int NumberOfBackups = 5;

        /// <summary>
        /// The directory to save backups in.
        /// </summary>
        public string BackupDirectory = "ForgeEditorBackups";

        /// <summary>
        /// The amount of time that we have accumulated until our next backup.
        /// </summary>
        private float _nextAutosaveTime;

        public bool BackupNow;

        protected void Update() {
            // we don't bother to auto-save if we're not an editor or if we are playing a game
            if (Application.isEditor == false || Application.isPlaying) {
                return;
            }

            //Debug.Log(_nextAutosaveTime);
            _nextAutosaveTime += Time.deltaTime;
            if (_nextAutosaveTime > BackupIntervalInSeconds || BackupNow) {
                RunBackup();

                _nextAutosaveTime = 0;
                BackupNow = false;
            }
        }

        /// <summary>
        /// Trims the backup directory so that it contains the given maximum number of files. Newer
        /// files are kept over older files.
        /// </summary>
        /// <param name="count">The number of files to keep.</param>
        private void TrimDirectory(int count) {
            string[] files = Directory.GetFiles(BackupDirectory);
            Array.Sort(files, (a, b) => {
                return File.GetLastWriteTime(a) > File.GetLastWriteTime(b) ? -1 : 1;
            });

            for (int i = count; i < files.Length; ++i) {
                File.Delete(files[i]);
            }
        }

        private string GetBackupPath() {
            return string.Format("{0}/backup_{1:MM_dd_yy_H_mm_ss}.json", BackupDirectory, DateTime.Now);
        }

        private void EnsureBackupDirectory() {
            if (Directory.Exists(BackupDirectory) == false) {
                Directory.CreateDirectory(BackupDirectory);
            }
        }

        private void RunBackup() {
            LevelDesigner level = GetComponent<LevelDesigner>();

            if (level == null ||
                (level.Snapshot == null && level.LevelTemplates == null && level.SharedTemplates == null)) {
                Debug.Log("No backup ran; nothing to backup");
            }

            BackupFormat backup = new BackupFormat() {
                SnapshotJson = LevelManager.SaveSnapshot(level.Snapshot),
                LevelTemplateJson = LevelManager.SaveTemplateGroup(level.LevelTemplates),
                SharedTemplateJson = LevelManager.SaveTemplateGroup(level.SharedTemplates)
            };

            string backupPath = GetBackupPath();
            EnsureBackupDirectory();
            File.WriteAllText(backupPath, SerializationHelpers.Serialize(backup));

            TrimDirectory(NumberOfBackups);
        }

        [JsonObject(MemberSerialization.OptIn)]
        private class BackupFormat {
            [JsonProperty]
            public string SnapshotJson;
            [JsonProperty]
            public string SharedTemplateJson;
            [JsonProperty]
            public string LevelTemplateJson;
        }
    }
}