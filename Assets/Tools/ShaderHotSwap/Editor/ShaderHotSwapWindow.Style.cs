using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace UsingTheirs.ShaderHotSwap
{
    public partial class ShaderHotSwapWindow
    {
        private class Styles
        {
            public GUIStyle log;
            public GUIStyle scroll;
            public Texture2D icon;
            public GUIStyle linkButton;

            public Styles()
            {
            }

            public void Build(ScriptableObject parent)
            {
                if (log == null)
                {
                    log = new GUIStyle(GUI.skin.textArea);
                    log.richText = true;
                    //log.normal.background = MakeTex(1, 1, new Color(0.1f, 0.1f, 0.1f));
                }

                if (scroll == null)
                {
                    scroll = new GUIStyle(GUI.skin.scrollView);
                    scroll.margin = new RectOffset(0, 0, 2, 2);
                }

                if (icon == null)
                {
                    try
                    {
                        var iconPath = Path.Combine(GetIconDirectoryPath(parent), "Logo.png");
                        icon = AssetDatabase.LoadAssetAtPath<Texture2D>(iconPath);
                    }
                    catch (Exception)
                    {
                     Debug.LogError("[ShaderHotSwap] Icon Not Found.");
                    }
                }

                if (linkButton == null)
                {
                    linkButton = new GUIStyle(EditorStyles.label);
                    linkButton.richText = true;
                }
            }

            static Texture2D MakeTex(int width, int height, Color col)
            {
                Color[] pix = new Color[width * height];

                for (int i = 0; i < pix.Length; i++)
                    pix[i] = col;

                Texture2D result = new Texture2D(width, height);
                result.SetPixels(pix);
                result.Apply();

                return result;
            }

            static string GetIconDirectoryPath(ScriptableObject parent)
            {
                var scriptAsset = MonoScript.FromScriptableObject(parent);
                var path = AssetDatabase.GetAssetPath(scriptAsset);
                return Path.Combine(Path.GetDirectoryName(path), "Icon");
            }
        }
    }
}