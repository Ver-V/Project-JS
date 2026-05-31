using Steamworks;
using System.Text;
using System.IO;
using System.Collections.Generic;
using ProjectJS.Skills;
using UnityEngine;

namespace ProjectJS.Manager
{
    public static class SteamCloudSave
    {
        [System.Serializable]
        public class BossClearRecord
        {
            public string bossName;
            public float clearTime;
            public string clearDate;
        }

        [System.Serializable]
        public class SaveData
        {
            public List<ShardSpecies> ownedShards = new List<ShardSpecies>();
            public List<BossClearRecord> bossRecords = new List<BossClearRecord>();
        }

        private const string SAVE_FILE_NAME = "save_data.json";

        public static void SaveGame(SaveData data)
        {
            string json = JsonUtility.ToJson(data, true);
            
            string localPath = Path.Combine(Application.persistentDataPath, SAVE_FILE_NAME);
            File.WriteAllText(localPath, json);

            if (SteamClient.IsValid)
            {
                SteamRemoteStorage.FileWrite(SAVE_FILE_NAME, Encoding.UTF8.GetBytes(json));
            }
        }

        public static SaveData LoadGame()
        {
            string json = "";
            if (SteamClient.IsValid && SteamRemoteStorage.FileExists(SAVE_FILE_NAME))
            {
                json = Encoding.UTF8.GetString(SteamRemoteStorage.FileRead(SAVE_FILE_NAME));
            }
            else
            {
                string localPath = Path.Combine(Application.persistentDataPath, SAVE_FILE_NAME);
                if (File.Exists(localPath)) json = File.ReadAllText(localPath);
            }

            return string.IsNullOrEmpty(json) ? new SaveData() : JsonUtility.FromJson<SaveData>(json);
        }
    }
}
