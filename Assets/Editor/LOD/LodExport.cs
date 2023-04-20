using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Linq;

namespace XEditor
{
    public class LodExport
    {
        private static SkinnedMeshRenderer[] lod0_parts;

        private static Mesh CopyMesh(SkinnedMeshRenderer renderer)
        {
            var mesh = Object.Instantiate<Mesh>(renderer.sharedMesh);
            mesh.name = renderer.sharedMesh.name;
            return mesh;
        }

        class Bone
        {
            public string name;
            public int flag;
            public Transform tf;
        }

        public static void SortLod(LODAsset[] lods, LodNode lodNode)
        {
            for (int k = 0; k < lods.Length - 1; k++)
            {
                for (int i = 0; i < lods[0].renders.Length; i++)
                {
                    List<Bone> list = new List<Bone>();
                    // sort based lod0
                    var render = lods[k].renders[i];
                    for (int j = 0; j < render.bones.Length; j++)
                    {
                        list.Add(new Bone() { name = render.bones[j].name, flag = -1, tf = render.bones[j] });
                    }
                    int g_idx = 0;
                    for (int j = lods.Length - 1; j >= k; j--)
                    {
                        render = lods[j].renders[i];
                        foreach (var b in render.bones)
                        {
                            var bone = list.Find(x => x.name == b.name);
                            if (bone == null)
                            {
                                Debug.LogError("lod" + j + " " + b.name);
                                continue;
                            }
                            if (bone.flag < 0) bone.flag = g_idx++;
                        }
                    }

                    Mesh mesh = CopyMesh(render);
                    Matrix4x4[] obind = render.sharedMesh.bindposes;
                    Matrix4x4[] nbind = new Matrix4x4[obind.Length];
                    for (int j = 0; j < obind.Length; j++)
                    {
                        nbind[list[j].flag] = obind[j];
                    }
                    var weights = render.sharedMesh.boneWeights;
                    var nweights = new BoneWeight[weights.Length];
                    for (int j = 0; j < weights.Length; j++)
                    {
                        nweights[j] = weights[j];
                        nweights[j].boneIndex0 = list[weights[j].boneIndex0].flag;
                        nweights[j].boneIndex1 = list[weights[j].boneIndex1].flag;
                        nweights[j].boneIndex2 = list[weights[j].boneIndex2].flag;
                        nweights[j].boneIndex3 = list[weights[j].boneIndex3].flag;
                    }
                    mesh.boneWeights = nweights;
                    mesh.bindposes = nbind;
                    list.Sort((x, y) => x.flag.CompareTo(y.flag));
                    render.bones = list.Select(x => x.tf).ToArray();
                    render.sharedMesh = mesh;
                    lods[0].boneInfo = null;
                }
            }

            GeneratePrefab(lodNode.prefab, lods[0]);
        }


        private static void GeneratePrefab(string prefab, LODAsset asset)
        {
            GameObject go = asset.go;
            string path = LodUtil.pref + prefab + ".prefab";
            var amtor = go.GetComponent<Animator>();
            if (amtor == null) amtor = go.AddComponent<Animator>();
            string p = "Assets/BundleRes/Controller/XAnimator.controller";
            amtor.runtimeAnimatorController = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(p);

            // temp
            Material mat = AssetDatabase.LoadAssetAtPath<Material>("Assets/Editor/Lod/lod.mat");
            foreach (var render in asset.renders)
            {
                render.sharedMaterial = mat;
            }

            PrefabUtility.SaveAsPrefabAsset(go, path);
            AssetDatabase.ImportAsset(path);
        }


        public static void Export(LODAsset[] lods, LodNode lodNode)
        {
            SortLod(lods, lodNode);

            lod0_parts = lods[0].go.GetComponentsInChildren<SkinnedMeshRenderer>();
            string name = lods[0].go.name;
            name = name.Substring(0, name.LastIndexOf("_LOD"));
            // lod0 export directly
            for (int i = 0; i < lod0_parts.Length; i++)
            {
                var mesh = CopyMesh(lod0_parts[i]);
                Save(name, 0, mesh, lodNode);
            }
            // recalculte mesh weights & bindpose
            for (int i = 1; i < lods.Length; i++)
            {
                var ps = lods[i].go.GetComponentsInChildren<SkinnedMeshRenderer>();
                foreach (var render in ps)
                {
                    var mesh = CopyMesh(render);
                    if (Recalculate(render, ref mesh))
                        Save(name, i, mesh, lodNode);
                }
            }
            AssetDatabase.Refresh();
        }


        private static bool Recalculate(SkinnedMeshRenderer render, ref Mesh mesh)
        {
            string name = render.name;
            int part = -1;
            for (int i = 0; i < lod0_parts.Length; i++)
            {
                if (lod0_parts[i].name == name)
                {
                    part = i;
                    break;
                }
            }
            if (part >= 0)
            {
                Dictionary<int, int> map = new Dictionary<int, int>();
                for (int i = 0; i < render.bones.Length; i++)
                {
                    string bone = render.bones[i].name;
                    int idx = IndexBone(part, bone);
                    if (idx >= 0)
                    {
                        map.Add(i, idx);
                        mesh.bindposes[i] = lod0_parts[part].sharedMesh.bindposes[idx];
                    }
                    else
                    {
                        Debug.LogError("not found bone in lod0 " + bone);
                    }
                }
                RecalcutMeshWeights(map, mesh);
                return true;
            }
            else
            {
                Debug.LogError("not found part in lod0 " + name);
            }
            return false;
        }


        // error occur if boneindex overange
        private static void RecalcutMeshWeights(Dictionary<int, int> map, Mesh mesh)
        {
            var weights = mesh.boneWeights;
            BoneWeight[] boneWeights = new BoneWeight[weights.Length];
            for (int i = 0; i < weights.Length; i++)
            {
                boneWeights[i] = weights[i];
                boneWeights[i].boneIndex0 = map[weights[i].boneIndex0];
                boneWeights[i].boneIndex1 = map[weights[i].boneIndex1];
                boneWeights[i].boneIndex2 = map[weights[i].boneIndex2];
                boneWeights[i].boneIndex3 = map[weights[i].boneIndex3];
            }
            mesh.boneWeights = boneWeights;
            mesh.RecalculateBounds();
        }

        private static int IndexBone(int part, string bone)
        {
            var bones = lod0_parts[part].bones;
            for (int i = 0; i < bones.Length; i++)
            {
                if (bones[i].name == bone)
                {
                    return i;
                }
            }
            return -1;
        }

        private static void Save(string name, int level, Mesh mesh, LodNode lodNode)
        {
            string part = mesh.name;
            string dir = LodUtil.prefix + name + "/lod" + level;
            string file = part + ".mesh";
            // string bytes = part + ".bytes";
            // GenBytes(Path.Combine(dir, bytes), mesh.triangles);
            // mesh.triangles = null;

            ExMesh format = lodNode.Format(level);
            if ((format & ExMesh.COLOR) <= 0) mesh.colors = null;
            if ((format & ExMesh.UV1) <= 0) mesh.uv = null;
            if ((format & ExMesh.UV2) <= 0) mesh.uv2 = null;
            if ((format & ExMesh.UV3) <= 0) mesh.uv3 = null;
            if ((format & ExMesh.UV4) <= 0) mesh.uv4 = null;
            if ((format & ExMesh.NORMAL) <= 0) mesh.normals = null;
            if ((format & ExMesh.TANGENT) <= 0) mesh.tangents = null;
            if ((format & ExMesh.OUTLINE) <= 0) PreparaOutline(mesh);

            mesh.Optimize();
            var path = Path.Combine(dir, file);
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
            AssetDatabase.CreateAsset(mesh, path);
            AssetDatabase.ImportAsset(path);
        }

        private static void GenBytes(string path, int[] tris)
        {
            FileStream fs = new FileStream(path, FileMode.OpenOrCreate, FileAccess.Write);
            BinaryWriter writer = new BinaryWriter(fs);
            bool max = tris.Length >= (1 << 16); // 超过unshort边界 就用int记
            writer.Write(max);
            for (int i = 0; i < tris.Length; i++)
            {
                if (max)
                {
                    writer.Write(tris[i]);
                }
                else
                {
                    ushort v = (ushort)tris[i];
                    writer.Write(v);
                }
            }
            writer.Close();
            fs.Close();
            AssetDatabase.ImportAsset(path);
        }

        private static void PreparaOutline(Mesh mesh)
        {
            var vc = mesh.colors;
            if (vc != null)
            {
                List<Color> colors = new List<Color>();
                var tangents = mesh.tangents;
                var normals = mesh.normals;
                Vector2[] uv3 = mesh.uv3.Length == vc.Length ? mesh.uv3 : null;
                for (int j = 0; j < vc.Length; ++j)
                {
                    ref var t = ref tangents[j];
                    ref var n = ref normals[j];
                    ref var color = ref vc[j];

                    Vector3 tangent = Vector3.Normalize(new Vector3(t.x, t.y, t.z));
                    Vector3 normal = Vector3.Normalize(n);
                    Vector3 binormal = Vector3.Cross(normal, tangent) * t.w;
                    Matrix4x4 matr = new Matrix4x4();
                    matr.m00 = tangent.x;
                    matr.m01 = tangent.y;
                    matr.m02 = tangent.z;
                    matr.m03 = 0;
                    matr.m10 = binormal.x;
                    matr.m11 = binormal.y;
                    matr.m12 = binormal.z;
                    matr.m13 = 0;
                    matr.m20 = normal.x;
                    matr.m21 = normal.y;
                    matr.m22 = normal.z;
                    matr.m23 = 0;
                    matr.m30 = 0;
                    matr.m31 = 0;
                    matr.m32 = 0;
                    matr.m33 = 1;
                    float a = color.a;
                    Vector4 c = color * new Color(2, 2, 2, 1) - new Color(1, 1, 1, 0);
                    c = matr * c;
                    //Debug.Log( mesh.uv3.Length);
                    if (uv3 != null)
                    {
                        ref var outlineControl = ref uv3[j];
                        c.w = outlineControl.x;
                    }
                    else
                    {
                        c.w = a;
                    }
                    colors.Add(c);
                }
                mesh.SetColors(colors);
                mesh.uv3 = null;
            }
        }


    }

}