using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace CFEngine.Editor
{
    [CFDecorator (typeof (CFResPathAttribute))]
    public sealed class AssetDecorator : AttributeDecorator<CFResPathAttribute, ResParam>
    {
        public override SerializedPropertyType GetSType ()
        {
            return SerializedPropertyType.String;
        }

        public override void InnerOnGUI (
            GUIContent title,
            float width,
            uint flag)
        {
            ListElementContext lec = new ListElementContext ();
            lec.draw = true;
            float lineHeight = 21;
            int lineCount = 1 + 3;
            lec.rect = GUILayoutUtility.GetRect (1, lineHeight * lineCount);

            ToolsUtility.InitListContext (ref lec, lineHeight);
            var resetline = MaskToggle (ref lec, ref maskX);
            //EditorGUI.BeginChangeCheck ();
            var asset = editParam.asset;
            var oldAsset = editParam.res as UnityEngine.Object;
            if (asset == null && oldAsset != asset)
            {
                asset = oldAsset;
            }
            ToolsUtility.ObjectField (ref lec, title.text, 120, ref asset, attr.type, 200, resetline);
            if (asset != editParam.asset)
            {
                UndoRecord (title.text);

                if (AssetsConfig.GetResType (asset, ref editParam.resType))
                {
                    editParam.asset = asset;
                    editParam.res = asset;
                    if (attr.redirectRes)
                    {
                        editParam.value = asset != null ? asset.name : "";
                    }
                    else
                    {
                        editParam.value = EditorCommon.GetAssetPath (asset, false);
                    }
                }
                else
                {
                    editParam.asset = null;
                    editParam.res = null;
                    editParam.value = "";
                }
                valueChange = true;
            }

            ToolsUtility.NewLine (ref lec, 10);
            ToolsUtility.Label (ref lec, editParam.value, 300, true);
            string resExt = "";
            byte resType = editParam.resType;
            if (resType == ResObject.Tex_2D)
            {
                resExt = "Tex2D_tga";
            }
            else if (resType == ResObject.Tex_2D_PNG)
            {
                resExt = "Tex2D_png";
            }
            else if (resType == ResObject.Tex_Cube)
            {
                resExt = "TexCube_tga";
            }
            else if (resType == ResObject.Tex_Cube_PNG)
            {
                resExt = "TexCube_png";
            }
            else if (resType == ResObject.Tex_Cube_EXR)
            {
                resExt = "TexCube_exr";
            }
            else if (resType == ResObject.Tex_3D)
            {
                resExt = "Tex3D";
            }
            else if (resType == ResObject.Tex_2D_EXR)
            {
                resExt = "Tex2D_exr";
            }
            else if (resType == ResObject.Mat)
            {
                resExt = "Mat";
            }
            else if (resType == ResObject.Mesh)
            {
                resExt = "Mesh";
            }
            ToolsUtility.NewLine (ref lec, 10);
            ToolsUtility.Label (ref lec, resExt, 160, true);
            ToolsUtility.Label (ref lec, "ResIndex:" + profileParam.resOffset.ToString (), 100);
            ToolsUtility.NewLine (ref lec, 10);
            if (ToolsUtility.Button (ref lec, "Refresh", 80, true))
            {
                if (AssetsConfig.GetResType (asset, ref editParam.resType))
                {
                    editParam.asset = asset;
                    editParam.res = asset;
                    if (attr.redirectRes)
                    {
                        editParam.value = asset != null?asset.name: "";
                    }
                    else
                    {
                        editParam.value = EditorCommon.GetAssetPath (asset, false);
                    }
                }
                else
                {
                    editParam.asset = null;
                    editParam.res = null;
                    editParam.value = "";
                }
                valueChange = true;
            }
            if (ToolsUtility.Button (ref lec, "CopyName", 80))
            {
                GUIUtility.systemCopyBuffer = editParam.value;
            }
        }
        public override void InnerResetValue ()
        {
            profileParam.value = "";
            profileParam.asset = null;
        }
    }
}