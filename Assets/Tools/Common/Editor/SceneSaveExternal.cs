using System;
using System.Collections.Generic;
using System.IO;
using System.Linq.Expressions;
using UnityEditor;
using UnityEngine;
using UnityEngineEditor = UnityEditor.Editor;
using CFUtilPoolLib;
using XEditor.Level;
using XLevel;

namespace CFEngine.Editor
{
    public partial class SceneSaveExternal
    {
        public static void Init ()
        {
            SceneSerialize.preSaveExternalDataCb = PreSaveExternalData;
            SceneSerialize.saveHeadExternalDataCb = SaveHeadExternalData;
            SceneSerialize.saveChunkExternalDataCb = SaveChunkExternalData;
        }

        static QuadTreeMap[] gridQuadTrees;
        static List<Vector3> navipoints = new List<Vector3> ();
        static int[, ] PathMatrix = null;
        static bool saveClientGridData = false;

        private static void PreSaveExternalData (SceneSaveContext ssc)
        {
            //collider grid
            // int TerrainColliderGridCount = 0;
            float GridSize = 1;
            string clientDataPath = string.Format ("{0}/{1}_TerrainCollider.mapheight",
                ssc.sceneContext.terrainDir,
                ssc.sceneContext.name);
            try
            {
                using (FileStream fs = new FileStream (clientDataPath, FileMode.Open))
                {
                    BinaryReader br = new BinaryReader (fs);
                    GridSize = br.ReadSingle ();
                    int width = ssc.chunkWidth * ssc.widthCount;
                    int height = ssc.chunkHeight * ssc.heightCount;
                    LevelMapData.GridColCount = (int) (width * 10 / (int) (10 * GridSize)); //������0.1��ʱ��ֱ�ӳ���256/0.1=2559����������
                    LevelMapData.GridRowCount = (int) (height * 10 / (int) (10 * GridSize));

                    LevelMapData.GridSize = GridSize;
                    LevelMapData.BlockSize = ssc.chunkWidth;
                }
            }
            catch (Exception e)
            {
                Debug.LogError (e.Message);
            }

            try
            {
                QuadTreeElement.gridSize = LevelMapData.GridSize;
                QuadTreeElement.gridcolcount = LevelMapData.GridColCount;
                gridQuadTrees = new QuadTreeMap[ssc.widthCount * ssc.heightCount];
                for (int z = 0; z < ssc.heightCount; ++z)
                    for (int x = 0; x < ssc.widthCount; ++x)
                    {
                        int index = z * ssc.widthCount + x;
                        QuadTreeMap qtm = new QuadTreeMap (16, GridSize);
                        gridQuadTrees[index] = qtm;

                        string path = string.Format ("{0}/{1}_{2}.mapheight", 
                        ssc.sceneContext.terrainDir, x.ToString (), 
                        z.ToString ());
                        if (File.Exists (path))
                        {
                            using (FileStream ffs = new FileStream (path, FileMode.Open))
                            {

                                BinaryReader br = new BinaryReader (ffs);
                                qtm.LoadFromFile (br);
                            }
                        }

                    }
            }
            catch (Exception e)
            {
                Debug.LogError (e.StackTrace);
            }
            navipoints.Clear ();
            PathMatrix = null;
            clientDataPath = string.Format ("{0}/{1}_NaviMap.navi", ssc.sceneContext.terrainDir, ssc.sceneContext.name);
            try
            {
                using (FileStream fs = new FileStream (clientDataPath, FileMode.Open))
                {
                    BinaryReader br = new BinaryReader (fs);

                    int count = br.ReadInt32 ();

                    PathMatrix = new int[count, count];

                    for (int i = 0; i < count; ++i)
                    {
                        float x = br.ReadSingle ();
                        float y = br.ReadSingle ();
                        float z = br.ReadSingle ();
                        //int linkIndex = br.ReadInt32 ();
                        br.ReadInt32 ();

                        navipoints.Add (new Vector3 (x, y, z));
                    }

                    for (int i = 0; i < count; ++i)
                        for (int j = 0; j < count; ++j)
                        {
                            br.ReadSingle ();
                        }

                    for (int i = 0; i < count; ++i)
                        for (int j = 0; j < count; ++j)
                        {
                            PathMatrix[i, j] = br.ReadInt32 ();
                        }

                }
            }
            catch (Exception e)
            {
                // Debug.LogError (e.Message);
                Debug.LogWarning (e.Message);
            }
        }
        private static void SaveHeadExternalData (BinaryWriter bw, SceneSaveContext ssc)
        {
            bw.Write (LevelMapData.GridSize);
            int navCount = navipoints.Count;
            bw.Write (navCount);

            if (navCount > 0)
            {
                for (int i = 0; i < navipoints.Count; ++i)
                {
                    EditorCommon.WriteVector (bw, navipoints[i]);
                }

                for (int i = 0; i < navCount; ++i)
                    for (int j = 0; j < navCount; ++j)
                    {
                        bw.Write (PathMatrix[i, j]);
                    }
            }
        }
        private static void SaveChunkExternalData (BinaryWriter bw, int i)
        {
            bw.Write(saveClientGridData);
            if (saveClientGridData)
            {
                if (gridQuadTrees != null)
                {
                    var gqt = gridQuadTrees[i];
                    if (gqt != null && gqt.TotalTreeNodeCount > 0)
                    {
                        gqt.WriteToFile (bw);
                    }

                }
            }

        }
    }
}