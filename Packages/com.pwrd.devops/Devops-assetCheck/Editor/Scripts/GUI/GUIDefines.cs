using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using GUIFrameWork;

namespace AssetCheck
{
    public static class GUIDefines
    {
        public static string ImagePath = "Packages/com.pwrd.devops/Devops-assetCheck/Editor/Resources/Images";
        // title名字
        public static GUIStyle TitleNameStyle;
        // title规则
        public static GUIStyle TitleRuleStyle;
        // rule全部启用
        public static GUIStyle RuleFoldoutHeaderStyle;
        // 加号图片
        public static GUIContent ContentAdd;
        // 垃圾桶图片
        public static GUIContent ContentTrash;
        // orange按钮
        public static GUIStyle ButtonOrangeStyle;
        // orangeoutline按钮
        public static GUIStyle ButtonOrangeOutlineStyle;
        // 灰色文字
        public static GUIStyle DisableText;
        // 灰色
        public static Color ColorGrayBack = new Color(62f/225f, 62f/225f, 62f/225f);
        // 
        public static GUIStyle FoldoutHeaderStyle;
        //
        public static GUIStyle ParamStyle;
        static GUIDefines()
        {
            TitleNameStyle = new GUIStyle();
            TitleNameStyle.fontSize = 15;
            TitleNameStyle.fontStyle = FontStyle.Normal;
            TitleNameStyle.normal.textColor = Color.white;

            TitleRuleStyle = new GUIStyle();
            TitleRuleStyle.fontSize = 13;
            TitleRuleStyle.fontStyle = FontStyle.Normal;
            TitleRuleStyle.normal.textColor = Color.white;

            RuleFoldoutHeaderStyle = new GUIStyle(EditorStyles.foldoutHeader);

            //TextureAdd = UnityEditor.EditorGUIUtility.FindTexture("PrefabOverlayAdded Icon");
            ContentAdd = EditorGUIUtility.IconContent("CreateAddNew");
            ContentTrash = EditorGUIUtility.IconContent("TreeEditor.Trash");

            ButtonOrangeStyle = new GUIStyle(GUI.skin.button);
            ButtonOrangeStyle.normal.background = GUIExAssets.GetAssetButtonOrange();
            ButtonOrangeStyle.hover.background = GUIExAssets.GetAssetButtonOrange();
            ButtonOrangeStyle.onNormal.background = GUIExAssets.GetAssetButtonOrange();
            ButtonOrangeStyle.onHover.background = GUIExAssets.GetAssetButtonOrange();
            ButtonOrangeStyle.onActive.background = GUIExAssets.GetAssetButtonOrange();
            ButtonOrangeStyle.focused.background = GUIExAssets.GetAssetButtonOrange();
            ButtonOrangeStyle.onFocused.background = GUIExAssets.GetAssetButtonOrange();

            Color colorOrange = new Color(251f / 255f, 126f / 255f, 5f / 255f);
            ButtonOrangeOutlineStyle = new GUIStyle(GUI.skin.button);
            ButtonOrangeOutlineStyle.normal.background = GUIExAssets.GetAssetButtonOrangeOutLine();
            ButtonOrangeOutlineStyle.normal.textColor = colorOrange;
            ButtonOrangeOutlineStyle.hover.background = GUIExAssets.GetAssetButtonOrangeOutLine();
            ButtonOrangeOutlineStyle.hover.textColor = colorOrange;
            ButtonOrangeOutlineStyle.onNormal.background = GUIExAssets.GetAssetButtonOrangeOutLine();
            ButtonOrangeOutlineStyle.onNormal.textColor = colorOrange;
            ButtonOrangeOutlineStyle.onHover.background = GUIExAssets.GetAssetButtonOrangeOutLine();
            ButtonOrangeOutlineStyle.onHover.textColor = colorOrange;
            ButtonOrangeOutlineStyle.onActive.background = GUIExAssets.GetAssetButtonOrangeOutLine();
            ButtonOrangeOutlineStyle.onActive.textColor = colorOrange;
            ButtonOrangeOutlineStyle.focused.background = GUIExAssets.GetAssetButtonOrangeOutLine();
            ButtonOrangeOutlineStyle.focused.textColor = colorOrange;
            ButtonOrangeOutlineStyle.onFocused.background = GUIExAssets.GetAssetButtonOrangeOutLine();
            ButtonOrangeOutlineStyle.onFocused.textColor = colorOrange;

            DisableText = new GUIStyle(GUI.skin.label);
            DisableText.normal.textColor = Color.gray;

            FoldoutHeaderStyle = new GUIStyle(GUI.skin.button);
            FoldoutHeaderStyle.fontSize = 20;
            FoldoutHeaderStyle.fontStyle = FontStyle.Normal;
            FoldoutHeaderStyle.alignment = TextAnchor.MiddleLeft;
            FoldoutHeaderStyle.normal.textColor = Color.white;
            FoldoutHeaderStyle.normal.background = (Texture2D)EditorGUIUtility.IconContent("transparent").image;
            FoldoutHeaderStyle.hover.textColor = Color.white;
            FoldoutHeaderStyle.hover.background = (Texture2D)EditorGUIUtility.IconContent("transparent").image;
            FoldoutHeaderStyle.onNormal.textColor = Color.white;
            FoldoutHeaderStyle.onNormal.background = (Texture2D)EditorGUIUtility.IconContent("transparent").image;
            FoldoutHeaderStyle.onHover.textColor = Color.white;
            FoldoutHeaderStyle.onHover.background = (Texture2D)EditorGUIUtility.IconContent("transparent").image;
            FoldoutHeaderStyle.onActive.textColor = Color.white;
            FoldoutHeaderStyle.onActive.background = (Texture2D)EditorGUIUtility.IconContent("transparent").image;
            FoldoutHeaderStyle.focused.textColor = Color.white;
            FoldoutHeaderStyle.focused.background = (Texture2D)EditorGUIUtility.IconContent("transparent").image;
            FoldoutHeaderStyle.onFocused.textColor = Color.white;
            FoldoutHeaderStyle.onFocused.background = (Texture2D)EditorGUIUtility.IconContent("transparent").image;

            ParamStyle = new GUIStyle(GUI.skin.label);
        }
    }
}

