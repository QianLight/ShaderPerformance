using System.IO;
using CFEngine;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;

namespace UnityEngine.Rendering.Universal
{
    public class EnviromentSHBakerHelper
    {
        public static EnviromentSHBakerHelper instance { get; } = new EnviromentSHBakerHelper();
        
        private readonly int m_AmbientProbeOutputBufferParam = Shader.PropertyToID ("_AmbientProbeOutputBuffer");
        private readonly int m_AmbientProbeInputCubemap = Shader.PropertyToID ("_AmbientProbeInputCubemap");

        private SphericalHarmonicsL2 m_AmbientProbe;
        public SphericalHarmonicsL2 AmbientProbe
        {
            get { return m_AmbientProbe; }
        }
        private ComputeShader m_ComputeAmbientProbeCS;
        private int m_ComputeAmbientProbeKernel = -1;
        private static float[] m_coefs = new float[28];

        private Color[] faces = new Color[6];
        private Cubemap cube;

        public void Init (ComputeShader shBaker)
        {
            if (m_ComputeAmbientProbeCS == null)
                m_ComputeAmbientProbeCS = shBaker;
            if (m_ComputeAmbientProbeCS != null && m_ComputeAmbientProbeKernel == -1)
                m_ComputeAmbientProbeKernel = m_ComputeAmbientProbeCS.FindKernel ("AmbientProbeConvolution");
        }

        public bool BakeSkyBoxSphericalHarmonics (Texture cube, float scale, string path, bool save = true)
        {
            ComputeBuffer resultBuffer;
            float[] resultArr;
            resultBuffer = new ComputeBuffer (27, 4);
            resultArr = new float[27];
            m_ComputeAmbientProbeCS.SetBuffer (m_ComputeAmbientProbeKernel, "_AmbientProbeOutputBuffer", resultBuffer);
            m_ComputeAmbientProbeCS.SetTexture (m_ComputeAmbientProbeKernel, "_AmbientProbeInputCubemap", cube, 0);
            m_ComputeAmbientProbeCS.Dispatch (m_ComputeAmbientProbeKernel, 1, 1, 1);
            resultBuffer.GetData (resultArr);
            resultBuffer.Release ();
            for (int channel = 0; channel < 3; ++channel)
            {
                for (int coeff = 0; coeff < 9; ++coeff)
                {
                    m_AmbientProbe[channel, coeff] = (resultArr[channel * 9 + coeff]) * scale;
                }
            }
            if (save)
                SaveShaderField (m_AmbientProbe, path);
            return true;
        }
        private bool BakeSkyBoxSphericalHarmonics (Texture cube, float scale)
        {
            if (m_ComputeAmbientProbeCS != null && m_ComputeAmbientProbeKernel >= 0)
            {
                ComputeBuffer resultBuffer;
                float[] resultArr;
                resultBuffer = new ComputeBuffer (27, 4);
                resultArr = new float[27];
                m_ComputeAmbientProbeCS.SetBuffer (m_ComputeAmbientProbeKernel, "_AmbientProbeOutputBuffer", resultBuffer);
                m_ComputeAmbientProbeCS.SetTexture (m_ComputeAmbientProbeKernel, "_AmbientProbeInputCubemap", cube, 0);
                m_ComputeAmbientProbeCS.Dispatch (m_ComputeAmbientProbeKernel, 1, 1, 1);
                resultBuffer.GetData (resultArr);
                resultBuffer.Release ();
                for (int channel = 0; channel < 3; ++channel)
                {
                    for (int coeff = 0; coeff < 9; ++coeff)
                    {
                        m_AmbientProbe[channel, coeff] = (resultArr[channel * 9 + coeff]) * scale;
                    }
                }
                return true;
            }
            else
            {
                return false;
            }

        }

        public void BakeSkyBoxSphericalHarmonics (ref SHInfo sh)
        {
            if (sh.ambientMode == AmbientType.Flat)
            {
                m_AmbientProbe.Clear ();
                m_AmbientProbe.AddAmbientLight (sh.flatColor.linear);
            }
            else if (sh.ambientMode == AmbientType.Trilight)
            {
                faces[0] = sh.equatorColor;
                faces[1] = sh.equatorColor;
                faces[2] = sh.skyColor;
                faces[3] = sh.groundColor;
                faces[4] = sh.equatorColor;
                faces[5] = sh.equatorColor;
                int width = 128;
                if (cube == null)
                    cube = new Cubemap (width, TextureFormat.RGBAHalf, true);
                for (int f = 0; f < 6; f++)
                {
                    for (int i = 0; i < width; i++)
                    {
                        for (int j = 0; j < width; j++)
                        {
                            cube.SetPixel ((CubemapFace) f, i, j, faces[f]);
                        }
                    }
                }
                cube.Apply ();
                if (!BakeSkyBoxSphericalHarmonics (cube, 1))
                {
                    DebugLog.AddErrorLog ("bake fail");
                }
            }
            else if (sh.ambientMode == AmbientType.SkyBox)
            {
                if (sh.skyCube != null)
                {
                    if (!BakeSkyBoxSphericalHarmonics (sh.skyCube, sh.skyIntensity))
                    {
                        DebugLog.AddErrorLog ("bake fail");
                    }
                }
                else
                {
                    DebugLog.AddErrorLog ("sky cube can not be null");
                    return;
                }
            }
            else
            {
                return;
            }
            PrepareCoefs (ref m_AmbientProbe,
                ref sh.shAr, ref sh.shAg, ref sh.shAb,
                ref sh.shBr, ref sh.shBg, ref sh.shBb,
                ref sh.shC);

        }
        public static void PrepareCoefs (ref SphericalHarmonicsL2 sh,
            ref Vector4 shAr, ref Vector4 shAg, ref Vector4 shAb,
            ref Vector4 shBr, ref Vector4 shBg, ref Vector4 shBb, ref Vector4 shC)
        {
            PrepareCoefs (sh, ref m_coefs);
            shAr = new Vector4 (m_coefs[0], m_coefs[1], m_coefs[2], m_coefs[3]);
            shAg = new Vector4 (m_coefs[4], m_coefs[5], m_coefs[6], m_coefs[7]);
            shAb = new Vector4 (m_coefs[8], m_coefs[9], m_coefs[10], m_coefs[11]);
            shBr = new Vector4 (m_coefs[12], m_coefs[13], m_coefs[14], m_coefs[15]);
            shBg = new Vector4 (m_coefs[16], m_coefs[17], m_coefs[18], m_coefs[19]);
            shBb = new Vector4 (m_coefs[20], m_coefs[21], m_coefs[22], m_coefs[24]);
            shC = new Vector4 (m_coefs[24], m_coefs[25], m_coefs[26], m_coefs[27]);
        }

        public static void PrepareCoefs (SphericalHarmonicsL2 sh, ref float[] coefs)
        {
            for (var i = 0; i < 3; i++)
            {
                coefs[4 * i + 0] = sh[i, 3];
                coefs[4 * i + 1] = sh[i, 1];
                coefs[4 * i + 2] = sh[i, 2];
                coefs[4 * i + 3] = sh[i, 0] - sh[i, 6];
            }
            for (var i = 0; i < 3; i++)
            {
                coefs[12 + 4 * i + 0] = sh[i, 4];
                coefs[12 + 4 * i + 1] = sh[i, 5];
                coefs[12 + 4 * i + 2] = sh[i, 6] * 3;
                coefs[12 + 4 * i + 3] = sh[i, 7];
            }

            coefs[24] = sh[0, 8];
            coefs[25] = sh[1, 8];
            coefs[26] = sh[2, 8];
            coefs[27] = 1;
        }

        public void SaveShaderField (SphericalHarmonicsL2 sh, string path)
        {
            path = string.Format ("{0}/{1}", Application.dataPath, path);
            if (File.Exists (path))
            {
                File.Delete (path);
            }
            float[] coefs = new float[28];
            PrepareCoefs (sh, ref coefs);
            System.IO.StreamWriter file = new System.IO.StreamWriter (path, false);
            for (int i = 0; i < 28; i++)
            {
                file.WriteLine (coefs[i].ToString ());
            }
            file.Flush ();
            file.Close ();
            file.Dispose ();
        }

        public float[] ReadFile (string path)
        {
            path = string.Format ("{0}/{1}", Application.dataPath, path);
            if (File.Exists (path))
            {
                float[] coefs = new float[28];
                string line;
                int index = 0;
                StreamReader file = new StreamReader (path);
                while ((line = file.ReadLine ()) != null)
                {
                    coefs[index] = float.Parse (line);
                    index++;
                }
                file.Close ();
                return coefs;
            }
            else
            {
                throw new System.Exception (string.Format ("{0} is not exist", path));
            }
        }

        public void ApplySHToObjects (float[] sh)
        {
            Renderer[] mrs = GameObject.FindObjectsOfType<Renderer> ();
            MaterialPropertyBlock properties = new MaterialPropertyBlock ();
            ApplySHToMaterialBlock (sh, properties);
            for (int i = 0; i < mrs.Length; i++)
                mrs[i].SetPropertyBlock (properties);
        }

        public void ApplySHToMaterialBlock (float[] sh, MaterialPropertyBlock properties)
        {
            for (var i = 0; i < 3; i++)
                properties.SetVector (
                    _idSHA[i],
                    new Vector4 (sh[4 * i + 0], sh[4 * i + 1], sh[4 * i + 2], sh[4 * i + 3])
                );

            for (var i = 0; i < 3; i++)
                properties.SetVector (
                    _idSHB[i],
                    new Vector4 (sh[4 * i + 12], sh[4 * i + 13], sh[4 * i + 14], sh[4 * i + 15])
                );

            properties.SetVector (
                _idSHC,
                new Vector4 (sh[24], sh[25], sh[26], sh[27])
            );
        }

        static int[] _idSHA = {
            Shader.PropertyToID ("unity_SHAr"),
            Shader.PropertyToID ("unity_SHAg"),
            Shader.PropertyToID ("unity_SHAb")
        };

        static int[] _idSHB = {
            Shader.PropertyToID ("unity_SHBr"),
            Shader.PropertyToID ("unity_SHBg"),
            Shader.PropertyToID ("unity_SHBb")
        };

        static int _idSHC =
        Shader.PropertyToID ("unity_SHC");
    }
}

//MeshRenderer[] mrs = GameObject.FindObjectsOfType<MeshRenderer>();
//MaterialPropertyBlock properties = new MaterialPropertyBlock();
//for (var i = 0; i < 3; i++)
//    properties.SetVector(
//        _idSHA[i],
//        new Vector4(sh[i, 3], sh[i, 1], sh[i, 2], sh[i, 0] - sh[i, 6])
//        );

//for (var i = 0; i < 3; i++)
//    properties.SetVector(
//        _idSHB[i],
//        new Vector4(sh[i, 4], sh[i, 5], sh[i, 6] * 3, sh[i, 7])
//        );

//properties.SetVector(
//    _idSHC,
//    new Vector4(sh[0, 8], sh[1, 8], sh[2, 8], 1)
//    );
//for (int i = 0; i < mrs.Length; i++)
//    mrs[i].SetPropertyBlock(properties);
//return;