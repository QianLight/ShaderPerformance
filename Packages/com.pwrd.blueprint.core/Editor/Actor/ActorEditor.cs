using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

namespace Blueprint.ActorEditor
{
    using Blueprint.Actor;
    using PENet;
    using Blueprint.UtilEditor;
    using System.Linq;

    [InitializeOnLoadAttribute]
    public class ActorEditor : EditorWindow
    {

        private static double renameTime;

        private const string c_PrefabSuffix = ".prefab";

        /// <summary>
        /// 此常量需要和蓝图内的actor类配置相同
        /// </summary>
        private const string c_ActorSuffix = ".actorClass~";

        private const string c_OldActorSuffix = ".actorClass";

        static ActorEditor()
        {
            BpClient.OnReceiveMessage += HandleBlueprintMsg;
        }

        /// <summary>
        /// 通知蓝图创建actor
        /// </summary>
        [MenuItem("Assets/Create/Blueprint Actor", false, 40)]
        public static void CreateActor()
        {
            if (!BpClient.IsConnected)
            {
                UnityEditor.EditorUtility.DisplayDialog("警告", "蓝图未连接", "确定");
                return ;
            }

            // 通知蓝图创建actor类
            string name = InnerGetUniqueName("NewActor");
            string localPath = Path.Combine(GetSelectedPathOrFallback(), name);
            ActorMessageData data = new ActorMessageData()
            {
                path = localPath,
            };
            BpClient.SendMessage(MessageType.CreateActor, JsonUtility.ToJson(data));
        }

        /// <summary>
        /// 创建actor prefab,该方法仅会在收到蓝图消息后调用
        /// </summary>
        /// <param name="path"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static string CreateOrUpdateActorPrefab(string path, string exportClassName)
        {
            GameObject obj = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (obj != null)
            {
                // 如果存在，则表示是prefab转化actor或者更新数据
                BlueprintActor actor = obj.GetComponent<BlueprintActor>();

                if (actor == null)
                {
                    actor = obj.AddComponent<BlueprintActor>();
                }

                if (actor != null)
                {
                    actor.RefreshActorParam();
                    actor.bpClassExportName = exportClassName;
                    actor.bpClassName = obj.name;
                    ActorEditor.SendAcotrRefresh(path, actor);
                    EditorUtility.SetDirty(obj);
                }
                return null;
            }

            obj = new GameObject();
            BlueprintActor blueprintActor = obj.AddComponent<BlueprintActor>();
            blueprintActor.bpClassExportName = exportClassName;
            blueprintActor.bpClassName = Path.GetFileNameWithoutExtension(path);

            GameObject prefab = PrefabUtility.SaveAsPrefabAssetAndConnect(obj, path, InteractionMode.UserAction);
            Object.DestroyImmediate(obj);

            prefab.GetComponent<BlueprintActor>().RefreshActorParam();

            blueprintActor = prefab.GetComponent<BlueprintActor>();
            blueprintActor.bpClassName = prefab.name;
            EditorUtility.SetDirty(prefab);
            SendAcotrRefresh(path, blueprintActor);
            return null;
        }

        /// <summary>
        /// 通知蓝图创建子actor
        /// </summary>
        [MenuItem("Assets/Create/BluePrint Actor Child", false, 40)]
        public static void CreateChildActor()
        {
            if (!BpClient.IsConnected)
            {
                UnityEditor.EditorUtility.DisplayDialog("警告", "蓝图未连接", "确定");
                return ;
            }

            GameObject parent = Selection.activeGameObject;
            if (parent != null && parent.GetComponent<BlueprintActor>())
            {
                string name = InnerGetUniqueName(parent.name + "_Child");
                string localPath = Path.Combine(GetSelectedPathOrFallback(), name);

                ActorMessageData data = new ActorMessageData()
                {
                    path = localPath,
                    parentPath = AssetDatabase.GetAssetPath(parent),
                };
                EditorUtility.SetDirty(parent);
                BpClient.SendMessage(MessageType.CreateChildActor, JsonUtility.ToJson(data));
            }
            else
            {
                UnityEditor.EditorUtility.DisplayDialog("警告", "请在actor prefab上选择", "确定");
            }
        }

        /// <summary>
        /// 将已有的prefab转化为actor
        /// </summary>
        [MenuItem("Assets/Create/Transform BlueprintActor", false, 40)]
        public static void TransformToActor()
        {
            if (!BpClient.IsConnected)
            {
                UnityEditor.EditorUtility.DisplayDialog("警告", "蓝图未连接", "确定");
                return ;
            }

            GameObject prefab = Selection.activeGameObject;

            if (prefab == null || !PrefabUtility.IsPartOfAnyPrefab(prefab))
            {
                UnityEditor.EditorUtility.DisplayDialog("警告", "请在prefab上选择", "确定");
                return ;
            }

            if (prefab.GetComponent<BlueprintActor>())
            {
                UnityEditor.EditorUtility.DisplayDialog("警告", "该prefab已经是蓝图actor", "确定");
                return ;
            }

            if (PrefabUtility.IsPartOfVariantPrefab(prefab))
            {
                GameObject parent = PrefabUtility.GetCorrespondingObjectFromSource(prefab);
                if (parent != null && prefab.GetComponent<BlueprintActor>() == null)
                {
                    UnityEditor.EditorUtility.DisplayDialog("警告", "需要先将base prefab转化为actor", "确定");
                    return ;
                }
            }
            string name = prefab.name;
            string localPath = Path.Combine(GetSelectedPathOrFallback(), name);

            ActorMessageData data = new ActorMessageData()
            {
                path = localPath,
            };
            BpClient.SendMessage(MessageType.CreateActor, JsonUtility.ToJson(data));
        }

        /// <summary>
        /// 转化旧版本actor的后缀
        /// </summary>
        [MenuItem("Blueprint/Transform BlueprintActor", false, 40)]
        public static void TransformActorSuffix()
        {
            EditorUtility.DisplayProgressBar("转换actor后缀", "查找所有actor", 0f);
            List<FileInfo> files = new List<FileInfo>();
            GetAllActorAndDbPath(Application.dataPath, files);

            float transCount = 0;
            float totalCount = files.Count;
            for (int i = 0; i < files.Count; i++)
            {
                var info = files[i];
                transCount++;
                if (File.Exists(info.FullName))
                {
                    var suffix = info.Extension.Equals(c_OldActorSuffix) ? c_ActorSuffix : ".db~";
                    var copyPath = Path.Combine(info.DirectoryName, Path.GetFileNameWithoutExtension(info.Name).Replace(".", "") + suffix);

                    File.Copy(info.FullName, copyPath);
                    File.Delete(info.FullName);
                    EditorUtility.DisplayProgressBar("转换actor后缀", "转换后缀", transCount / totalCount);
                }
            }
            EditorUtility.ClearProgressBar();
            UnityEditor.EditorUtility.DisplayDialog("注意", "转换完成，请重启蓝图", "确定");
        }

        /// <summary>
        /// 获取所有actor和db文件
        /// </summary>
        /// <param name="path"></param>
        /// <param name="list"></param>
        public static void GetAllActorAndDbPath(string path, List<FileInfo> list)
        {
            string[] dirPaths = Directory.GetDirectories(path);

            foreach (var dir in dirPaths)
            {
                GetAllActorAndDbPath(dir, list);
            }

            string[] filePaths = Directory.GetFiles(path);

            foreach (var filePath in filePaths)
            {
                FileInfo info = new FileInfo(filePath);

                if (info.Extension.Equals(c_OldActorSuffix) || info.Extension.Equals(".db")) 
                {
                    list.Add(info);
                }
            }
        }

        /// <summary>
        ///  根据actor的物体唯一名称查找该物体的最父actor
        /// </summary>
        /// <param name="data"></param>
        public static void FindActorParent(ActorMessageData data)
        {
            if (data == null || data.path == null)
            {
                return;
            }
            string prefabPath = data.path + c_PrefabSuffix;
            GameObject obj = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);

            if (obj != null)
            {
                BlueprintActor actor = obj.GetComponent<BlueprintActor>();
                BlueprintActorGoParam targetObj = actor.GetBlueprintActorGoParam(data.msg);

                while (targetObj != null && targetObj.isInherit)
                {
                    obj = PrefabUtility.GetCorrespondingObjectFromSource(targetObj.gameObject);
                    obj = obj.transform.root.gameObject;

                    actor = obj.GetComponent<BlueprintActor>();
                    targetObj = actor.GetBlueprintActorGoParam(data.msg);
                }

                if (targetObj != null && targetObj.gameObject != null)
                {
                    string path = AssetDatabase.GetAssetPath(targetObj.gameObject);
                    data.path = path;
                    PENet.BpClient.SendMessage(PENet.MessageType.OpenActor, JsonUtility.ToJson(data));
                }
            }
        }

        /// <summary>
        /// 创建子actor prefab
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="path"></param>
        public static void CreateChildActorPrefab(GameObject parent, string path, string exportClassName)
        {
            GameObject child = PrefabUtility.InstantiatePrefab(parent) as GameObject;
            BlueprintActor blueprintActor = child.GetComponent<BlueprintActor>();
            blueprintActor.RefreshActorParam();
            blueprintActor.bpClassExportName = exportClassName;
            blueprintActor.bpClassName = Path.GetFileNameWithoutExtension(path);
            GameObject prefab = PrefabUtility.SaveAsPrefabAssetAndConnect(child, path, InteractionMode.UserAction);

            blueprintActor = prefab.GetComponent<BlueprintActor>();
            blueprintActor.RefreshActorParam();
            blueprintActor.bpClassExportName = exportClassName;
            blueprintActor.bpClassName = prefab.name;
            EditorUtility.SetDirty(prefab);
            SendAcotrRefresh(path, blueprintActor);

            Object.DestroyImmediate(child);
        }

        /// <summary>
        /// 发送到蓝图，让蓝图删除actor资产
        /// </summary>
        /// <param name="path"></param>
        public static void DeletaActorBlueprintAsset(string path)
        {
            string actorPath = ConvertPrefabPathToActorPath(path);
            string actorFullpath = AssetUtil.ToCompletePath(actorPath) + c_ActorSuffix;

            if (File.Exists(actorFullpath))
            {
                ActorParamMessage data = new ActorParamMessage()
                {
                    path = actorFullpath,
                };
                if (PENet.BpClient.IsConnected)
                {
                    PENet.BpClient.SendMessage(PENet.MessageType.DeleteActor, JsonUtility.ToJson(data));
                }
            }
        }

        [MenuItem("Assets/Create/BluePrint Actor Test", false, 40)]
        public static void Test()
        {

        }

        public static void SendAcotrRefresh(string path, BlueprintActor actor)
        {
            ActorParamMessage data = new ActorParamMessage()
            {
                path = path,
                actorParam = actor.blueprintActorGoParams,
            };
            if (PENet.BpClient.IsConnected)
            {
                PENet.BpClient.SendMessage(PENet.MessageType.UpdateActorParam, JsonUtility.ToJson(data));
            }
        }

        /// <summary>
        /// 生成actor唯一名称
        /// </summary>
        /// <returns></returns>
        public static string InnerGetUniqueName(string name)
        {
            string[] guids = AssetDatabase.FindAssets("t:prefab");

            List<string> names = new List<string>();
            if (guids != null)
            {
                foreach (string s in guids)
                {
                    string path = AssetDatabase.GUIDToAssetPath(s);
                    names.Add(Path.GetFileNameWithoutExtension(path).ToLower());
                }
            }

            if (!names.Contains(name.ToLower()))
            {
                return name;
            }

            int num = 1;
            string uniqueName = name + num.ToString();
            while (names.Contains(uniqueName.ToLower()))
            {
                num++;
                uniqueName = name + num.ToString();
            }

            return uniqueName;
        }

        /// <summary>
        /// 移除指定路径下prefab的actor相关脚本，该路径为Unity Assets相对路径
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static bool RemoveActorScript(string path)
        {
            GameObject obj = AssetDatabase.LoadAssetAtPath<GameObject>(path);

            if (obj != null && obj.GetComponent<BlueprintActor>())
            {
                Object.DestroyImmediate(obj.GetComponent<BlueprintActor>());
                return true;
            }

            return false;
        }

        /// <summary>
        /// Retrieves selected folder on Project view.
        /// </summary>
        /// <returns></returns>
        private static string GetSelectedPathOrFallback()
        {
            string path = "Assets";

            foreach (UnityEngine.Object obj in Selection.GetFiltered(typeof(UnityEngine.Object), SelectionMode.Assets))
            {
                path = AssetDatabase.GetAssetPath(obj);
                if (!string.IsNullOrEmpty(path) && File.Exists(path))
                {
                    path = Path.GetDirectoryName(path);
                    break;
                }
            }
            return path;
        }

        /// <summary>
        /// 将prefab路径转换为不带后缀的actor文件路径
        /// </summary>
        /// <param name="actorPath"></param>
        /// <returns></returns>
        private static string ConvertPrefabPathToActorPath(string actorPath)
        {
            string directory = Path.GetDirectoryName(actorPath);
            string name = Path.GetFileNameWithoutExtension(actorPath);

            return Path.Combine(directory, name);
        }

        private static void HandleBlueprintMsg(MessageData messageData)
        {
            ActorMessageData data = null;
            switch(messageData.messageType)
            {
                case MessageType.CreateActorPrefab:
                    data = JsonUtility.FromJson<ActorMessageData>(messageData.data);
                    if (!string.IsNullOrEmpty(data.msg))
                    {
                        UnityEditor.EditorUtility.DisplayDialog("警告", data.msg, "确定");
                        RemoveActorScript(data.path);
                        return;
                    }
                    CreateOrUpdateActorPrefab(data.path + c_PrefabSuffix, data.exportClassName);
                    break;
                case MessageType.CreateChildActorPrefab:
                    data = JsonUtility.FromJson<ActorMessageData>(messageData.data);
                    GameObject parentPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(data.parentPath + c_PrefabSuffix);
                    CreateChildActorPrefab(parentPrefab, data.path + c_PrefabSuffix, data.exportClassName);
                    break;
                case MessageType.UpdateActorParam:
                    data = JsonUtility.FromJson<ActorMessageData>(messageData.data);
                    CreateOrUpdateActorPrefab(data.path + c_PrefabSuffix, data.exportClassName);
                    break;
                case MessageType.ReNameActor:
                    data = JsonUtility.FromJson<ActorMessageData>(messageData.data);
                    if (!string.IsNullOrEmpty(data.msg))
                    {
                        UnityEditor.EditorUtility.DisplayDialog("警告", data.msg, "确定");
                        return;
                    }
                    AssetDatabase.Refresh();
                    GameObject obj = AssetDatabase.LoadAssetAtPath<GameObject>(data.path);

                    if (obj != null)
                    {
                        BlueprintActor actor = obj.GetComponent<BlueprintActor>();
                        actor.bpClassExportName = data.exportClassName;
                        actor.bpClassName = obj.name;
                    }
                    break;
                case MessageType.BlueprintResourcePath:
                    if(messageData.data != null && messageData.data == BpClient.BindBpPjtPath)
                        BpClient.IsConnected = true;  
                    else 
                    {
                        BpClient.WrongPostIds.Add(BpClient.currentPostId);
                        BpClient.HasConnectedCount++;
                        BpClient.ConnectNextBpProcess();
                    }    
                    break;
                case MessageType.QueryAcotrParent:
                    data = JsonUtility.FromJson<ActorMessageData>(messageData.data);
                    FindActorParent(data);
                    break;
            }
        }

    }

}