#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEngine;
using UnityEngineEditor = UnityEditor.Editor;

namespace CFEngine
{
    public partial class SFXData
    {
        public void ConvertMat (Material mat)
        {
            if (mat.shader.name == "Effect/UVEffect_test")
            {
                string dir = AssetsPath.GetAssetDir (mat, out var path);
                var newMat = new Material (Shader.Find ("Custom/SFX/Uber"));
                newMat.name = string.Format ("{0}_sfx", mat.name);

                newMat.SetTexture (ShaderManager._MainTex, mat.GetTexture (ShaderManager._MainTex));
                newMat.SetVector ("_UVST0", mat.GetVector ("_MainTex_ST"));

                Vector4 _Param = new Vector4 (-101, 0, 0, 0);
                Vector4 _Param0 = new Vector4 (0, 0, 0, 0);
                Vector4 _Param1 = new Vector4 (-1, 0, 0, 0);
                Vector4 _Param2 = new Vector4 (1, 0, 0, 0);
                Vector4 _Param3 = new Vector4 (0, 1, 1, -1);
                Vector4 _Param4 = new Vector4 (0, 1, 0.5f, 0);
                Vector4 _Param5 = new Vector4 (0, 0, -1, 0);
                var _ShaderMode = mat.GetFloat ("_ShaderMode");
                if (_ShaderMode > 0.5f)
                {
                    //material
                    _Param.x = -101;
                    _Param1.w = mat.GetFloat ("_EdgeWidth");
                    _Param2.x = mat.GetFloat ("_MainPannerX");
                    _Param2.y = mat.GetFloat ("_MainPannerY");
                    _Param2.z = mat.GetFloat ("_Dissolve");
                    _Param2.w = mat.GetFloat ("_DistortPower");

                    bool useFresnel = mat.GetFloat ("_Usefresnel") > 0.5f?true : false;
                    if (useFresnel)
                    {
                        _Param1.x = mat.GetFloat("_Flip");
                    }
                    else
                    {
                        _Param1.x = -1;
                    }

                }
                else
                {
                    //custom data
                }

                string newPath = string.Format ("{0}/{1}_sfx.mat", dir, mat.name);
            }
        }
    }
}
#endif