using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.UI;
using UnityEngine;

namespace GUIFrameWork
{
    public class GUIExAssets
    {
        public static string ImagePath = "Packages/com.pwrd.devops/Devops-assetCheck/Editor/Resources/Images/";
        static Texture2D AssetCheckToggleOpen;
        static Texture2D AssetCheckToggleClose;
        static Texture2D AssetCheckButtonOrange;
        static Texture2D AssetCheckButtonOrangeOutLine;

        public static Texture2D GetAssetCheckToggleOpen()
        {
            if (AssetCheckToggleOpen == null)
            {
                AssetCheckToggleOpen = AssetDatabase.LoadAssetAtPath<Texture2D>($"{ImagePath}ToggleOpen.png");
            }
            return AssetCheckToggleOpen;
        }

        public static Texture2D GetAssetCheckToggleClose()
        {
            if (AssetCheckToggleClose == null)
            {
                AssetCheckToggleClose = AssetDatabase.LoadAssetAtPath<Texture2D>($"{ImagePath}ToggleClose.png");
            }
            return AssetCheckToggleClose;
        }

        public static Texture2D GetAssetButtonOrange()
        {
            if (AssetCheckButtonOrange == null)
            {
                AssetCheckButtonOrange = AssetDatabase.LoadAssetAtPath<Texture2D>($"{ImagePath}btnOrange.png");
            }
            return AssetCheckButtonOrange;
        }

        public static Texture2D GetAssetButtonOrangeOutLine()
        {
            if (AssetCheckButtonOrangeOutLine == null)
            {
                AssetCheckButtonOrangeOutLine = AssetDatabase.LoadAssetAtPath<Texture2D>($"{ImagePath}btnOrangeOutLine.png");
            }
            return AssetCheckButtonOrangeOutLine;
        }
    }
}