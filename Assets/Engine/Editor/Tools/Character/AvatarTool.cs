using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System;
using UnityEngine.CFUI;
using CFEngine;

namespace CFEngine.Editor
{
    public class AvatarTool : CommonToolTemplate
    {
        // enum OpType
        // {
        //     OpNone,
        //     OpExportAvatar,
        //     OpRefreshAvatar,
        //     OpRefreshAnimation,
        // }
        private AvatarData avatarData;
        private Vector2 avatarConfigScroll = Vector2.zero;
        private Vector2 fbxConfigScroll = Vector2.zero;
        // private OpType opType = OpType.OpNone;
        // private int exportAvatarIndex = -1;
        private int refreshFbxIndex = -1;
        private int refreshFbxAnimIndex = -1;
        public override void OnInit()
        {
            base.OnInit();
            string path = string.Format("{0}/Avatar/AvatarConfig.asset", AssetsConfig.instance.Creature_Path);
            avatarData = AssetDatabase.LoadAssetAtPath<AvatarData>(path);
            if (avatarData == null)
            {
                avatarData = ScriptableObject.CreateInstance<AvatarData>();
                avatarData = CommonAssets.CreateAsset<AvatarData>(path, ".asset", avatarData);
            }

        }

        public override void OnUninit()
        {
            base.OnUninit();
        }
        public override void DrawGUI(ref Rect rect)
        {
            if (avatarData != null)
            {
                GUILayout.BeginHorizontal();
                if (GUILayout.Button("Save", GUILayout.MaxWidth(160)))
                {
                    CommonAssets.SaveAsset(avatarData);
                }
                GUILayout.EndHorizontal();

                EditorCommon.BeginGroup("AvatarConfig");
                GUILayout.BeginHorizontal();
                if (GUILayout.Button("Add Avatar", GUILayout.MaxWidth(160)))
                {
                    AvatarConfig ac = new AvatarConfig();
                    avatarData.avatarConfig.Add(ac);
                }
                GUILayout.EndHorizontal();
                int count = avatarData.avatarConfig.Count > 10 ? 10 : avatarData.avatarConfig.Count;
                avatarConfigScroll = GUILayout.BeginScrollView(avatarConfigScroll, GUILayout.MinHeight(count * 20 + 10));
                int deleteIndex = -1;
                for (int i = 0; i < avatarData.avatarConfig.Count; ++i)
                {
                    var ac = avatarData.avatarConfig[i];
                    GUILayout.BeginHorizontal();
                    ac.fbx = EditorGUILayout.ObjectField(ac.fbx, typeof(GameObject), false, GUILayout.MaxWidth(160)) as GameObject;
                    ac.avatarMask = EditorGUILayout.ObjectField(ac.avatarMask, typeof(AvatarMask), false, GUILayout.MaxWidth(200)) as AvatarMask;
                    ac.isHuman = EditorGUILayout.Toggle("Is Human", ac.isHuman);
                    // if (GUILayout.Button("Export", GUILayout.MaxWidth(160)))
                    // {
                    //     exportAvatarIndex = i;
                    // }
                    if (GUILayout.Button("Delete", GUILayout.MaxWidth(100)))
                    {
                        deleteIndex = i;
                    }
                    GUILayout.EndHorizontal();
                }
                if (deleteIndex >= 0)
                {
                    avatarData.avatarConfig.RemoveAt(deleteIndex);
                }
                GUILayout.EndScrollView();
                EditorCommon.EndGroup();

                EditorCommon.BeginGroup("Fbx Config");
                GUILayout.BeginHorizontal();
                if (GUILayout.Button("Add Fbx", GUILayout.MaxWidth(160)))
                {
                    FbxConfig fc = new FbxConfig();
                    avatarData.fbxConfig.Add(fc);
                }
                GUILayout.EndHorizontal();
                count = avatarData.avatarConfig.Count > 10 ? 10 : avatarData.fbxConfig.Count;
                fbxConfigScroll = GUILayout.BeginScrollView(fbxConfigScroll, GUILayout.MinHeight(count * 20 + 10));
                deleteIndex = -1;
                for (int i = 0; i < avatarData.fbxConfig.Count; ++i)
                {
                    var fc = avatarData.fbxConfig[i];
                    GUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField(fc.fbxName, GUILayout.MaxWidth(200));
                    GameObject fbx = null;
                    GameObject select = EditorGUILayout.ObjectField(fbx, typeof(GameObject), false, GUILayout.MaxWidth(100)) as GameObject;
                    if (select != null)
                    {
                        string path = AssetDatabase.GetAssetPath(select);
                        string folderName;
                        if (AssetsPath.GetCreatureFolderName(path, out folderName))
                        {
                            fc.fbxName = folderName;
                        }
                    }
                    fc.fbx = EditorGUILayout.ObjectField(fc.fbx, typeof(GameObject), false, GUILayout.MaxWidth(200)) as GameObject;
                    if (GUILayout.Button("BindAvatar", GUILayout.MaxWidth(160)))
                    {
                        refreshFbxIndex = i;
                    }
                    if (GUILayout.Button("BindAnimation", GUILayout.MaxWidth(160)))
                    {
                        refreshFbxAnimIndex = i;
                    }
                    if (GUILayout.Button("Delete", GUILayout.MaxWidth(100)))
                    {
                        deleteIndex = i;
                    }
                    GUILayout.EndHorizontal();
                }
                if (deleteIndex >= 0)
                {
                    avatarData.fbxConfig.RemoveAt(deleteIndex);
                }
                GUILayout.EndScrollView();
                EditorCommon.EndGroup();
            }
        }

        public override void Update()
        {
            // switch (opType)
            // {
            //     case OpType.OpExportAvatar:
            //         {

            //         }
            //         break;
            //     case OpType.OpRefreshAvatar:
            //         {

            //         }
            //         break;
            //     case OpType.OpRefreshAnimation:
            //         {

            //         }
            //         break;
            // }
            // opType = OpType.OpNone;
            // if (exportAvatarIndex >= 0)
            // {
            //     var ac = avatarData.avatarConfig[exportAvatarIndex];
            //     if (ac.fbx != null)
            //     {
            //         Animator ator = ac.fbx.GetComponent<Animator>();
            //         if (ator != null && ator.avatar != null)
            //         {
            //             string avatarPath = string.Format("{0}/Avatar/{1}.asset", AssetsConfig.GlobalAssetsConfig.Creature_Path, ator.avatar.name);
            //             Avatar avatar = UnityEngine.Object.Instantiate<Avatar>(ator.avatar);
            //             avatar.name = ator.avatar.name;
            //             avatar = CommonAssets.CreateAsset<Avatar>(avatarPath, ".asset", avatar);
            //             ac.avatar = avatar;
            //         }
            //     }

            //     exportAvatarIndex = -1;
            // }
            if (refreshFbxIndex >= 0)
            {
                var fc = avatarData.fbxConfig[refreshFbxIndex];
                if (fc.fbx != null)
                {
                    AvatarConfig avatarConfig = avatarData.avatarConfig.Find((ac) => { return ac.fbx == fc.fbx; });
                    if (avatarConfig != null)
                    {
                        FBXAssets.Fbx_BindAvatar(string.Format("{0}/{1}", AssetsConfig.instance.Creature_Path, fc.fbxName), avatarConfig.fbx, avatarConfig.isHuman);
                    }
                }

                refreshFbxIndex = -1;
            }

            if (refreshFbxAnimIndex >= 0)
            {
                var fc = avatarData.fbxConfig[refreshFbxAnimIndex];
                if (fc.fbx != null)
                {
                    AvatarConfig avatarConfig = avatarData.avatarConfig.Find((ac) => { return ac.fbx == fc.fbx; });
                    if (avatarConfig != null)
                    {
                        FBXAssets.Fbx_BindAnimation(string.Format("{0}/{1}", AssetsConfig.instance.Creature_Path, fc.fbxName), avatarConfig.avatarMask, avatarConfig.isHuman);
                    }
                }

                refreshFbxAnimIndex = -1;
            }

        }
    }
}