using UnityEngine;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Collections.Generic;
using LitJsonForSaveGame;

namespace Blueprint.Logic
{
    [NotReflectConstructor]
    public class SaveGame : Blueprint.Logic.BP_Base
    {
        [Serializable]
        public class SaveInfo
        {
            public string slot;
            public string classType;
            public string jsonStr;
        }

        [Serializable]
        public class SaveInfoList
        {
            public List<SaveInfo> saveInfoList;
        }

        private static SaveInfoList saveInfoCache = null;

        public override void Start()
        {
            base.Start();
        }

        [NotReflect]
        public static bool SaveGameToSlot(string path, SaveGame saveObj)
        {
            var savePath = Path.Combine(Application.persistentDataPath, "SaveGame.json");
            var sil = LoadSaveInfoList();
            var saveInfoList = sil.saveInfoList;
            foreach (var si in saveInfoList)
            {
                if (si.slot == path)
                {
                    saveInfoList.Remove(si);
                    break;
                }
            }

            string saveJsonStr = JsonMapper.ToJson(saveObj);
            SaveInfo saveInfo = new SaveInfo();
            saveInfo.slot = path;
            saveInfo.classType = saveObj.GetType().ToString();
            saveInfo.jsonStr = saveJsonStr;
            saveInfoList.Add(saveInfo);
            string saveInfoListJsonStr = JsonMapper.ToJson(sil);
            File.WriteAllText(savePath, saveInfoListJsonStr, Encoding.UTF8);
            return File.Exists(savePath);
        }

        [NotReflect]
        public static async Task<bool> AsyncSaveGameToSlot(string path, SaveGame saveObj)
        {
            var savePath = Path.Combine(Application.persistentDataPath, "SaveGame.json");
            var sil = LoadSaveInfoList();
            var saveInfoList = sil.saveInfoList;
            foreach (var si in saveInfoList)
            {
                if (si.slot == path)
                {
                    saveInfoList.Remove(si);
                    break;
                }
            }

            string saveJsonStr = JsonMapper.ToJson(saveObj);
            SaveInfo saveInfo = new SaveInfo();
            saveInfo.slot = path;
            saveInfo.classType = saveObj.GetType().ToString();
            saveInfo.jsonStr = saveJsonStr;
            saveInfoList.Add(saveInfo);
            string saveInfoListJsonStr = JsonMapper.ToJson(sil);
            await Task.Run(() =>
            {
                File.WriteAllText(savePath, saveInfoListJsonStr, Encoding.UTF8);
            });
            return File.Exists(savePath);
        }

        [NotReflect]
        public static SaveGame LoadGameFromSlot(string path)
        {
            var saveInfoList = LoadSaveInfoList().saveInfoList;
            foreach (var si in saveInfoList)
            {
                if (si.slot == path)
                {
                    string classType = si.classType;
                    string jsonStr = si.jsonStr;
                    return JsonMapper.ToObject(jsonStr, BPClassManager.Instance.GetClass(classType)) as SaveGame;
                }
            }
            return null;
        }

        [NotReflect]
        public static async Task<SaveGame> AsyncLoadGameFromSlot(string path)
        {
            var savePath = Path.Combine(Application.persistentDataPath, "SaveGame.json");
            string json = await Task.Run(() =>
            {
                if (saveInfoCache != null)
                    return string.Empty;
                return File.ReadAllText(savePath);
            });

            List<SaveInfo> saveInfoList = null;
            if (saveInfoCache != null)
                saveInfoList = saveInfoCache.saveInfoList;
            else if (json.Length > 0)
            {
                saveInfoCache = JsonMapper.ToObject<SaveInfoList>(json);
                saveInfoList = saveInfoCache.saveInfoList;
            }

            foreach (var si in saveInfoList)
            {
                if (si.slot == path)
                {
                    string classType = si.classType;
                    string jsonStr = si.jsonStr;
                    return JsonMapper.ToObject(jsonStr, BPClassManager.Instance.GetClass(classType)) as SaveGame;
                }
            }
            return null;
        }

        [NotReflect]
        public static bool DeleteGameInSlot(string path)
        {
            var savePath = Path.Combine(Application.persistentDataPath, "SaveGame.json");
            var sil = LoadSaveInfoList();
            var saveInfoList = sil.saveInfoList;
            foreach (var si in saveInfoList)
            {
                if (si.slot == path)
                {
                    saveInfoList.Remove(si);
                    break;
                }
            }
            string saveInfoListJsonStr = JsonMapper.ToJson(sil);
            File.WriteAllText(savePath, saveInfoListJsonStr, Encoding.UTF8);
            return true;
        }

        [NotReflect]
        private static SaveInfoList LoadSaveInfoList()
        {
            if (saveInfoCache != null)
                return saveInfoCache;

            var savePath = Path.Combine(Application.persistentDataPath, "SaveGame.json");
            if (!File.Exists(savePath))
                File.Create(savePath).Dispose();

            string json = File.ReadAllText(savePath);
            if (json.Length > 0)
                saveInfoCache = JsonMapper.ToObject<SaveInfoList>(json);
            else
            {
                saveInfoCache = new SaveInfoList();
                saveInfoCache.saveInfoList = new List<SaveInfo>();
            }
            return saveInfoCache;
        }
    }
}

