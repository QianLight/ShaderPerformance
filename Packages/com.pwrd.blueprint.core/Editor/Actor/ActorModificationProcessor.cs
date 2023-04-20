using UnityEngine;
using UnityEditor;
using System.IO;
using System;

namespace Blueprint.ActorEditor
{

    using Blueprint.Actor;
    using Blueprint.UtilEditor;

    public class ActorModificationProcessor : UnityEditor.AssetModificationProcessor
    {

        /// <summary>
        /// 抓取actor删除
        /// </summary>
        /// <param name="assetPath"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        private static AssetDeleteResult OnWillDeleteAsset(string assetPath, RemoveAssetOptions options)
        {
            // 返回该枚举让unity操作删除
            AssetDeleteResult result = AssetDeleteResult.DidNotDelete;

            GameObject obj = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(assetPath) as GameObject;

            if (obj != null && obj.GetComponent<BlueprintActor>())
            {
                var check = UnityEditor.EditorUtility.DisplayDialog("警告", "该操作会同时删除蓝图actor资产", "确定", "取消");

                if (!check)
                {
                    // 如果是取消
                    result = AssetDeleteResult.FailedDelete;
                }
                else
                {
                    if (PENet.BpClient.IsConnected)
                    {
                        ActorEditor.DeletaActorBlueprintAsset(assetPath);
                        result = AssetDeleteResult.DidDelete;
                    }
                    else 
                    {
                        UnityEditor.EditorUtility.DisplayDialog("警告", "actor仅允许在蓝图连接时删除", "确定");
                        result = AssetDeleteResult.FailedDelete;
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// 抓取actor改名，并发送蓝图处理
        /// </summary>
        /// <param name="sourcePath"></param>
        /// <param name="destinationPath"></param>
        /// <returns></returns>
        private static AssetMoveResult OnWillMoveAsset(string sourcePath, string destinationPath)
        {
            // 返回该枚举让unity操作移动
            AssetMoveResult result = AssetMoveResult.DidNotMove;

            // get the file attributes for file or directory
            FileAttributes attr = File.GetAttributes(AssetUtil.ToCompletePath(sourcePath));

            if (!attr.HasFlag(FileAttributes.Directory))
            {
                // 判断是否actor
                string sourceDir = Path.GetDirectoryName(sourcePath);
                string destDir = Path.GetDirectoryName(destinationPath);


                GameObject obj = AssetDatabase.LoadAssetAtPath<GameObject>(sourcePath);
                if (obj != null && obj.GetComponent<BlueprintActor>())
                {
                    // 如果是actor，则由蓝图执行
                    SendChangeNameMsg(sourcePath, destinationPath);
                    result = AssetMoveResult.FailedMove;
                }
            }

            return result;
        }

        private static void SendChangeNameMsg(string source, string dest)
        {
            if (PENet.BpClient.IsConnected)
            {
                ActorMessageData data = new ActorMessageData()
                {
                    path = source,
                    destPath = dest,
                };
                PENet.BpClient.SendMessage(PENet.MessageType.ReNameActor, JsonUtility.ToJson(data));
            }
            else
            {
                UnityEditor.EditorUtility.DisplayDialog("警告", "actor仅允许在蓝图连接时改名或移动", "确定");
            }
        }
    }
}
