/*
Based on ObjExporter.cs, this "wrapper" lets you export to .OBJ directly from the editor menu.

This should be put in your "Editor"-folder. Use by selecting the objects you want to export, and select
the appropriate menu item from "Custom->Export". Exported models are put in a folder called
"ExportedObj" in the root of your Unity-project. Textures should also be copied and placed in the
same folder. */

using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System;
using UnityEditor.SceneManagement;

struct ObjMaterial
{
    public string name;
    public string textureName;
    public List<string> normalTexNames;
}

public class EditorObjExporter : ScriptableObject
{
    private static int vertexOffset = 0;
    private static int normalOffset = 0;
    private static int uvOffset = 0;


    //User should probably be able to change this. It is currently left as an excercise for
    //the reader.
    private static string targetFolder = "ExportedObj";


    private static string MeshToString(MeshFilter mf, Dictionary<string, ObjMaterial> materialList)
    {
        Mesh m = mf.sharedMesh;
        Material[] mats = mf.GetComponent<Renderer>().sharedMaterials;

        StringBuilder sb = new StringBuilder();

        sb.Append("g ").Append(mf.name).Append("\n");
        foreach (Vector3 lv in m.vertices)
        {
            Vector3 wv = mf.transform.TransformPoint(lv);

            //This is sort of ugly - inverting x-component since we're in
            //a different coordinate system than "everyone" is "used to".
            sb.Append(string.Format("v {0} {1} {2}\n", -wv.x, wv.y, wv.z));
        }

        sb.Append("\n");

        foreach (Vector3 lv in m.normals)
        {
            Vector3 wv = mf.transform.TransformDirection(lv);

            sb.Append(string.Format("vn {0} {1} {2}\n", -wv.x, wv.y, wv.z));
        }

        sb.Append("\n");

        foreach (Vector3 v in m.uv)
        {
            sb.Append(string.Format("vt {0} {1}\n", v.x, v.y));
        }

        Material tempMat;
        for (int mat = 0; mat < mats.Length; mat++)
        {
            tempMat = mats[mat];
            sb.Append("\n");
            sb.Append("usemtl ").Append(tempMat.name).Append("\n");
            sb.Append("usemap ").Append(tempMat.name).Append("\n");


            //See if this material is already in the materiallist.
            try
            {
                ObjMaterial objMaterial = new ObjMaterial();

                objMaterial.name = tempMat.name;

                if (tempMat.mainTexture)
                    objMaterial.textureName = AssetDatabase.GetAssetPath(mats[mat].mainTexture);
                else
                    objMaterial.textureName = null;

                objMaterial.normalTexNames = GetNormalTexs(tempMat);

                materialList.Add(objMaterial.name, objMaterial);
            }
            catch (ArgumentException)
            {
                //Already in the dictionary
            }
        }

        for (int material = 0; material < m.subMeshCount; material++)
        {
            int[] triangles = m.GetTriangles(material);
            for (int i = 0; i < triangles.Length; i += 3)
            {
                //Because we inverted the x-component, we also needed to alter the triangle winding.
                sb.Append(string.Format("f {1}/{1}/{1} {0}/{0}/{0} {2}/{2}/{2}\n",
                    triangles[i] + 1 + vertexOffset, triangles[i + 1] + 1 + normalOffset, triangles[i + 2] + 1 + uvOffset));
            }
        }

        vertexOffset += m.vertices.Length;
        normalOffset += m.normals.Length;
        uvOffset += m.uv.Length;

        return sb.ToString();
    }

    private static List<string> GetNormalTexs(Material targetM)
    {
        if (targetM == null || targetM.shader == null)
        {
            return null;
        }

        Texture normalTex;
        string shaderName = targetM.shader.name;
        List<Texture> normalTexList = new List<Texture>();
        string[] tempStrs = shaderName.Split('/');
        if (tempStrs.Length >= 3)
        {
            if (tempStrs[1].Equals("Scene"))
            {
                if (tempStrs[2].Equals("Uber"))
                {
                    normalTexList.Add(targetM.GetTexture("_BumpMap"));
                }
                else if (tempStrs[2].Equals("Layer"))
                {
                    normalTexList.Add(targetM.GetTexture("_ProcedureTex0"));
                    normalTexList.Add(targetM.GetTexture("_ProcedureTex1"));
                    normalTexList.Add(targetM.GetTexture("_ProcedureTex2"));
                }
                else
                {
                }
            }
            else if (tempStrs[1].Equals("Role"))
            {
                normalTexList.Add(targetM.GetTexture("_ProcedureTex0"));
            }
            else
            {
            }
        }
        normalTexList.Add(null);

        if (normalTexList.Count <= 0)
        {
            return null;
        }
        
        List<string> result = new List<string>();
        for (int i = 0; i < normalTexList.Count; i++)
        {
            if (normalTexList[i] != null)
            {
                result.Add(AssetDatabase.GetAssetPath(normalTexList[i]));
            }
        }

        return result;
    }

    private static void Clear()
    {
        vertexOffset = 0;
        normalOffset = 0;
        uvOffset = 0;
    }

    private static Dictionary<string, ObjMaterial> PrepareFileWrite()
    {
        Clear();

        return new Dictionary<string, ObjMaterial>();
    }

    private static void MaterialsToFile(Dictionary<string, ObjMaterial> materialList, string folder, string filename)
    {
        using (StreamWriter sw = new StreamWriter(folder + "/" + filename + ".mtl"))
        {
            foreach (KeyValuePair<string, ObjMaterial> kvp in materialList)
            {
                sw.Write("\n");
                sw.Write("newmtl {0}\n", kvp.Key);
                sw.Write("Ka  0.6 0.6 0.6\n");
                sw.Write("Kd  0.6 0.6 0.6\n");
                sw.Write("Ks  0.9 0.9 0.9\n");
                sw.Write("d  1.0\n");
                sw.Write("Ns  0.0\n");
                sw.Write("illum 2\n");

                if (kvp.Value.textureName != null)
                {
                    string destinationFile = kvp.Value.textureName;
                    int stripIndex = destinationFile.LastIndexOf('/'); //FIXME: Should be Path.PathSeparator;
                    if (stripIndex >= 0)
                        destinationFile = destinationFile.Substring(stripIndex + 1).Trim();

                    string relativeFile = destinationFile;

                    destinationFile = folder + "/" + destinationFile;

                    Debug.Log("Copying texture from " + kvp.Value.textureName + " to " + destinationFile);

                    try
                    {
                        //Copy the source file
                        File.Copy(kvp.Value.textureName, destinationFile);
                    }
                    catch
                    {
                    }

                    sw.Write("map_Kd {0}", relativeFile);
                }

                if (kvp.Value.normalTexNames!= null && kvp.Value.normalTexNames.Count > 0)
                {
                    for (int i = 0; i < kvp.Value.normalTexNames.Count; i++)
                    {
                        string destinationFile = kvp.Value.normalTexNames[i];
                        int stripIndex = destinationFile.LastIndexOf('/');
                        if (stripIndex >= 0)
                            destinationFile = destinationFile.Substring(stripIndex + 1).Trim();

                        string relativeFile = destinationFile;

                        destinationFile = folder + "/" + destinationFile;

                        Debug.Log("Copying texture from " + kvp.Value.normalTexNames[i] + " to " + destinationFile);

                        try
                        {
                            //Copy the source file
                            File.Copy(kvp.Value.normalTexNames[i], destinationFile);
                        }
                        catch
                        {
                        }
                    }
                }
                
                sw.Write("\n\n\n");
            }
        }
    }

    private static void MeshToFile(MeshFilter mf, string folder, string filename)
    {
        Dictionary<string, ObjMaterial> materialList = PrepareFileWrite();

        using (StreamWriter sw = new StreamWriter(folder + "/" + filename + ".obj"))
        {
            sw.Write("mtllib ./" + filename + ".mtl\n");

            sw.Write(MeshToString(mf, materialList));
        }

        MaterialsToFile(materialList, folder, filename);
    }

    private static void MeshesToFile(MeshFilter[] mf, string folder, string filename)
    {
        Dictionary<string, ObjMaterial> materialList = PrepareFileWrite();

        using (StreamWriter sw = new StreamWriter(folder + "/" + filename + ".obj"))
        {
            sw.Write("mtllib ./" + filename + ".mtl\n");

            for (int i = 0; i < mf.Length; i++)
            {
                sw.Write(MeshToString(mf[i], materialList));
            }
        }

        MaterialsToFile(materialList, folder, filename);
    }

    private static bool CreateTargetFolder()
    {
        try
        {
            System.IO.Directory.CreateDirectory(targetFolder);
        }
        catch
        {
            EditorUtility.DisplayDialog("Error!", "Failed to create target folder!", "");
            return false;
        }

        return true;
    }

    [MenuItem("Tools/场景/Export all MeshFilters in selection to separate OBJs")]
    static void ExportSelectionToSeparate()
    {
        if (!CreateTargetFolder())
            return;

        Transform[] selection = Selection.GetTransforms(SelectionMode.Editable | SelectionMode.ExcludePrefab);

        if (selection.Length == 0)
        {
            EditorUtility.DisplayDialog("No source object selected!", "Please select one or more target objects", "");
            return;
        }

        int exportedObjects = 0;

        for (int i = 0; i < selection.Length; i++)
        {
            Component[] meshfilter = selection[i].GetComponentsInChildren(typeof(MeshFilter));

            for (int m = 0; m < meshfilter.Length; m++)
            {
                exportedObjects++;
                MeshToFile((MeshFilter) meshfilter[m], targetFolder, selection[i].name + "_" + i + "_" + m);
            }
        }

        if (exportedObjects > 0)
            EditorUtility.DisplayDialog("Objects exported", "Exported " + exportedObjects + " objects", "");
        else
            EditorUtility.DisplayDialog("Objects not exported", "Make sure at least some of your selected objects have mesh filters!", "");
    }

    [MenuItem("Tools/场景/Export whole selection to single OBJ")]
    static void ExportWholeSelectionToSingle()
    {
        if (!CreateTargetFolder())
            return;


        Transform[] selection = Selection.GetTransforms(SelectionMode.Editable | SelectionMode.ExcludePrefab);

        if (selection.Length == 0)
        {
            EditorUtility.DisplayDialog("No source object selected!", "Please select one or more target objects", "");
            return;
        }

        int exportedObjects = 0;

        ArrayList mfList = new ArrayList();

        for (int i = 0; i < selection.Length; i++)
        {
            Component[] meshfilter = selection[i].GetComponentsInChildren(typeof(MeshFilter));

            for (int m = 0; m < meshfilter.Length; m++)
            {
                exportedObjects++;
                mfList.Add(meshfilter[m]);
            }
        }

        if (exportedObjects > 0)
        {
            MeshFilter[] mf = new MeshFilter[mfList.Count];

            for (int i = 0; i < mfList.Count; i++)
            {
                mf[i] = (MeshFilter) mfList[i];
            }

            string filename = EditorSceneManager.GetActiveScene().path + "_" + exportedObjects;


            int stripIndex = filename.LastIndexOf('/'); //FIXME: Should be Path.PathSeparator

            if (stripIndex >= 0)
                filename = filename.Substring(stripIndex + 1).Trim();

            MeshesToFile(mf, targetFolder, filename);


            EditorUtility.DisplayDialog("Objects exported", "Exported " + exportedObjects + " objects to " + filename, "");
        }
        else
            EditorUtility.DisplayDialog("Objects not exported", "Make sure at least some of your selected objects have mesh filters!", "");
    }


    [MenuItem("Tools/场景/Export each selected to single OBJ")]
    static void ExportEachSelectionToSingle()
    {
        if (!CreateTargetFolder())
            return;

        Transform[] selection = Selection.GetTransforms(SelectionMode.Editable | SelectionMode.ExcludePrefab);

        if (selection.Length == 0)
        {
            EditorUtility.DisplayDialog("No source object selected!", "Please select one or more target objects", "");
            return;
        }

        int exportedObjects = 0;


        for (int i = 0; i < selection.Length; i++)
        {
            Component[] meshfilter = selection[i].GetComponentsInChildren(typeof(MeshFilter));

            MeshFilter[] mf = new MeshFilter[meshfilter.Length];

            for (int m = 0; m < meshfilter.Length; m++)
            {
                exportedObjects++;
                mf[m] = (MeshFilter) meshfilter[m];
            }

            MeshesToFile(mf, targetFolder, selection[i].name + "_" + i);
        }

        if (exportedObjects > 0)
        {
            EditorUtility.DisplayDialog("Objects exported", "Exported " + exportedObjects + " objects", "");
        }
        else
            EditorUtility.DisplayDialog("Objects not exported", "Make sure at least some of your selected objects have mesh filters!", "");
    }
}