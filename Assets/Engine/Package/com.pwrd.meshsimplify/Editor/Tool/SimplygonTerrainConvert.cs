using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Rendering;

namespace Athena.MeshSimplify
{
  internal static class SimplygonTerrainConvert
  {
    private static Terrain terrain;
    private static TerrainConvertInfo terrainConvertInfo;
    public static ProgressFunction callback;
    private static bool basemapSplit = false;
    private static Vector4 basemapSplitOffsetScale = new Vector4(1f, 1f, 0.0f, 0.0f);

    public static Mesh[] Convert(
      Terrain _terrain,
      TerrainConvertInfo _terrainConvertInfo,
      bool _normalizeUV,
      ProgressFunction _callback = null)
    {
      if ((UnityEngine.Object) _terrain == (UnityEngine.Object) null)
      {
        Debug.LogWarning((object) "Terrain To Mesh: Can not convert 'Terrain' is null.\n");
        return (Mesh[]) null;
      }

      if (_terrainConvertInfo == null)
        _terrainConvertInfo = new TerrainConvertInfo();
      if (_terrainConvertInfo.chunkCountHorizontal < 1)
        _terrainConvertInfo.chunkCountHorizontal = 1;
      if (_terrainConvertInfo.chunkCountVertical < 1)
        _terrainConvertInfo.chunkCountVertical = 1;
      if (_terrainConvertInfo.vertexCountHorizontal < 2)
        _terrainConvertInfo.vertexCountHorizontal = 2;
      if (_terrainConvertInfo.vertexCountVertical < 2)
        _terrainConvertInfo.vertexCountVertical = 2;
      // if (_terrainConvertInfo.GetVertexCountPerChunk() > 65000)
      // {
      //   Debug.LogWarning(
      //     (object)
      //     "Terrain To Mesh: Mesh vertex count limit exceeded.\nUnity mesh can have maximum 65.000 vertices.\n");
      //   return (Mesh[]) null;
      // }

      SimplygonTerrainConvert.callback = _callback;
      Vector3 position = _terrain.transform.position;
      Quaternion rotation = _terrain.transform.rotation;
      Vector3 localScale = _terrain.transform.localScale;
      _terrain.transform.position = Vector3.zero;
      _terrain.transform.rotation = Quaternion.identity;
      _terrain.transform.localScale = Vector3.one;
      SimplygonTerrainConvert.terrain = _terrain;
      
      SimplygonTerrainConvert.terrainConvertInfo = _terrainConvertInfo;
      Mesh[] meshArray;
      if (SimplygonTerrainConvert.terrainConvertInfo.chunkCountHorizontal *
        SimplygonTerrainConvert.terrainConvertInfo.chunkCountVertical == 1)
      {
        var mesh = SimplygonTerrainConvert.GenerateTerrain();
        //越界转换成32位
        if (CheckMeshOutSize(mesh))
        {
          mesh = ConvertMeshToIndexFormat32(mesh, _terrainConvertInfo.vertexCountVertical, _terrainConvertInfo.vertexCountHorizontal);
        }
        
        meshArray = new Mesh[1]
        {
          mesh
        };
      }
      else
      {
        PreMesh[] _preMeshes = (PreMesh[]) null;
        SimplygonTerrainConvert.GenerateTerrainBaseChunks(ref _preMeshes);
        meshArray = new Mesh[_preMeshes.Length];
        int index1 = -1;
        for (int index3 = 0; index3 < SimplygonTerrainConvert.terrainConvertInfo.chunkCountVertical; ++index3)
        {
          for (int index2 = 0; index2 < SimplygonTerrainConvert.terrainConvertInfo.chunkCountHorizontal; ++index2)
          {
            float _chunkH_Width = SimplygonTerrainConvert.terrain.terrainData.size.x / (float) SimplygonTerrainConvert.terrainConvertInfo.chunkCountHorizontal;
            float _chunkH_StartOffset = (float) index2 * _chunkH_Width;
            float _chunkV_Length = SimplygonTerrainConvert.terrain.terrainData.size.z / (float) SimplygonTerrainConvert.terrainConvertInfo.chunkCountVertical;
            float _chunkV_StartOffset = (float) index3 * _chunkV_Length;
            if (SimplygonTerrainConvert.callback != null)
              SimplygonTerrainConvert.callback(
                "Chunk [" + (object) (index1 + 1) + " of " + (object) meshArray.Length + "]  ",
                (float) ((1.0 + (double) index1) /
                         (double) (SimplygonTerrainConvert.terrainConvertInfo.chunkCountHorizontal *
                                   SimplygonTerrainConvert.terrainConvertInfo.chunkCountVertical)));
            ++index1;
            meshArray[index1] = SimplygonTerrainConvert.GenerateTerrainMainChunks(ref _preMeshes[index1], _chunkH_Width,
              _chunkH_StartOffset, _chunkV_Length, _chunkV_StartOffset, _normalizeUV);
            meshArray[index1].RecalculateBounds();
            //越界转换成32位
            if (CheckMeshOutSize(meshArray[index1]))
            {
              meshArray[index1] = ConvertMeshToIndexFormat32(meshArray[index1], _terrainConvertInfo.vertexCountVertical, _terrainConvertInfo.vertexCountHorizontal);
            }
          }
        }
      }

      if (SimplygonTerrainConvert.terrainConvertInfo.generateSkirt)
      {
        for (int index = 0; index < meshArray.Length; ++index)
          SimplygonTerrainConvert.AddSkirt(meshArray[index]);
      }

      _terrain.transform.position = position;
      _terrain.transform.rotation = rotation;
      _terrain.transform.localScale = localScale;
      return meshArray;
    }
    
    public static MeshConvertInfo[] ConvertToInfo(
      Terrain _terrain,
      TerrainConvertInfo _terrainConvertInfo,
      bool _normalizeUV,
      ProgressFunction _callback = null)
    {
      if ((UnityEngine.Object) _terrain == (UnityEngine.Object) null)
      {
        Debug.LogWarning((object) "Terrain To Mesh: Can not convert 'Terrain' is null.\n");
        return (MeshConvertInfo[]) null;
      }

      if (_terrainConvertInfo == null)
        _terrainConvertInfo = new TerrainConvertInfo();
      if (_terrainConvertInfo.chunkCountHorizontal < 1)
        _terrainConvertInfo.chunkCountHorizontal = 1;
      if (_terrainConvertInfo.chunkCountVertical < 1)
        _terrainConvertInfo.chunkCountVertical = 1;
      if (_terrainConvertInfo.vertexCountHorizontal < 2)
        _terrainConvertInfo.vertexCountHorizontal = 2;
      if (_terrainConvertInfo.vertexCountVertical < 2)
        _terrainConvertInfo.vertexCountVertical = 2;

      SimplygonTerrainConvert.callback = _callback;
      Vector3 position = _terrain.transform.position;
      Quaternion rotation = _terrain.transform.rotation;
      Vector3 localScale = _terrain.transform.localScale;
      _terrain.transform.position = Vector3.zero;
      _terrain.transform.rotation = Quaternion.identity;
      _terrain.transform.localScale = Vector3.one;
      SimplygonTerrainConvert.terrain = _terrain;
      
      SimplygonTerrainConvert.terrainConvertInfo = _terrainConvertInfo;
      MeshConvertInfo[] allMeshConvertInfo;
      if (SimplygonTerrainConvert.terrainConvertInfo.chunkCountHorizontal *
        SimplygonTerrainConvert.terrainConvertInfo.chunkCountVertical == 1)
      {
        MeshConvertInfo meshConvertInfo = new MeshConvertInfo();
        meshConvertInfo.chunkCountHorizontal = 1;
        meshConvertInfo.chunkCountVertical = 1;
        meshConvertInfo.chunkIndexHorizontal = 0;
        meshConvertInfo.chunkIndexVertical = 0;
        meshConvertInfo.chunkSizeHorizontal = SimplygonTerrainConvert.terrain.terrainData.size.x;
        meshConvertInfo.chunkSizeVertical = SimplygonTerrainConvert.terrain.terrainData.size.z;
        meshConvertInfo.mesh = SimplygonTerrainConvert.GenerateTerrain();
        //越界转换成32位
        if (CheckMeshOutSize(meshConvertInfo.mesh))
        {
          meshConvertInfo.mesh = ConvertMeshToIndexFormat32(meshConvertInfo.mesh, _terrainConvertInfo.vertexCountVertical, _terrainConvertInfo.vertexCountHorizontal);
        }
        allMeshConvertInfo = new MeshConvertInfo[]
        {
          meshConvertInfo
        };
      }
      else
      {
        PreMesh[] _preMeshes = (PreMesh[]) null;
        SimplygonTerrainConvert.GenerateTerrainBaseChunks(ref _preMeshes);
        allMeshConvertInfo = new MeshConvertInfo[_preMeshes.Length];
        int index1 = -1;
        for (int j = 0; j < SimplygonTerrainConvert.terrainConvertInfo.chunkCountVertical; ++j)
        {
          for (int i = 0; i < SimplygonTerrainConvert.terrainConvertInfo.chunkCountHorizontal; ++i)
          {
            float _chunkH_Width = SimplygonTerrainConvert.terrain.terrainData.size.x / (float) SimplygonTerrainConvert.terrainConvertInfo.chunkCountHorizontal;
            float _chunkH_StartOffset = (float) i * _chunkH_Width;
            float _chunkV_Length = SimplygonTerrainConvert.terrain.terrainData.size.z / (float) SimplygonTerrainConvert.terrainConvertInfo.chunkCountVertical;
            float _chunkV_StartOffset = (float) j * _chunkV_Length;
            if (SimplygonTerrainConvert.callback != null)
              SimplygonTerrainConvert.callback(
                "Chunk [" + (object) (index1 + 1) + " of " + (object) allMeshConvertInfo.Length + "]  ",
                (float) ((1.0 + (double) index1) /
                         (double) (SimplygonTerrainConvert.terrainConvertInfo.chunkCountHorizontal *
                                   SimplygonTerrainConvert.terrainConvertInfo.chunkCountVertical)));
            ++index1;
            var mesh = SimplygonTerrainConvert.GenerateTerrainMainChunks(ref _preMeshes[index1], _chunkH_Width,
              _chunkH_StartOffset, _chunkV_Length, _chunkV_StartOffset, _normalizeUV);
            mesh.RecalculateBounds();
            
            MeshConvertInfo meshConvertInfo = new MeshConvertInfo();
            meshConvertInfo.chunkCountHorizontal = SimplygonTerrainConvert.terrainConvertInfo.chunkCountHorizontal;
            meshConvertInfo.chunkCountVertical = SimplygonTerrainConvert.terrainConvertInfo.chunkCountVertical;
            meshConvertInfo.chunkIndexHorizontal = i;
            meshConvertInfo.chunkIndexVertical = j;
            meshConvertInfo.chunkSizeHorizontal = SimplygonTerrainConvert.terrain.terrainData.size.x / (float)SimplygonTerrainConvert.terrainConvertInfo.chunkCountHorizontal; ;
            meshConvertInfo.chunkSizeVertical = SimplygonTerrainConvert.terrain.terrainData.size.z / (float)SimplygonTerrainConvert.terrainConvertInfo.chunkCountVertical; ;
            meshConvertInfo.mesh = mesh;
            //越界转换成32位
            if (CheckMeshOutSize(meshConvertInfo.mesh))
            {
              meshConvertInfo.mesh = ConvertMeshToIndexFormat32(meshConvertInfo.mesh, _terrainConvertInfo.vertexCountVertical, _terrainConvertInfo.vertexCountHorizontal);
            }
            allMeshConvertInfo[index1] = meshConvertInfo;
          }
        }
      }

      if (SimplygonTerrainConvert.terrainConvertInfo.generateSkirt)
      {
        for (int index = 0; index < allMeshConvertInfo.Length; ++index)
          SimplygonTerrainConvert.AddSkirt(allMeshConvertInfo[index].mesh);
      }

      _terrain.transform.position = position;
      _terrain.transform.rotation = rotation;
      _terrain.transform.localScale = localScale;
      return allMeshConvertInfo;
    }

    private static bool CheckMeshOutSize(Mesh mesh)
    {
      return mesh.vertices.Length > 65000;
    }

    public static Mesh ConvertMeshToIndexFormat32(Mesh originMesh, int vertexCountVertical, int vertexCountHorizontal)
    {
          //set mesh index format 32
          var newMesh = new Mesh();
          newMesh.indexFormat = IndexFormat.UInt32;
          newMesh.vertices = originMesh.vertices;
          // newMesh.normals = originMesh.normals;
          // newMesh.tangents = originMesh.tangents;
          newMesh.colors = originMesh.colors;
          newMesh.uv = originMesh.uv;
          newMesh.uv2 = originMesh.uv2;
          //重新计算三角面以及法线切线
          newMesh.triangles = GetTriangles(vertexCountVertical, vertexCountHorizontal);
          newMesh.RecalculateNormals();
          // newMesh.RecalculateTangents();
          GenerateTangents(newMesh.vertices, newMesh.triangles, newMesh.normals, newMesh.uv, out Vector4[] tangents);
          newMesh.tangents = originMesh.tangents;
          newMesh.RecalculateBounds();

          return newMesh;
    }
    
    public static Mesh ConvertMeshToIndexFormat16(Mesh originMesh)
    {
      if (originMesh.indexFormat == IndexFormat.UInt16)
      {
        return originMesh;
      }
      if (CheckMeshOutSize(originMesh))
      {
        Debug.LogError("Convet To Mesh: Mesh vertex count limit exceeded.\nUnity mesh can have maximum 65.000 vertices.\n");
        return null;
      }
      //set mesh index format 32
      var newMesh = Mesh.Instantiate(originMesh);
      newMesh.indexFormat = IndexFormat.UInt16;
      return newMesh;
    }

    private static int[] GetTriangles(int vertexCountVertical, int vertexCountHorizontal)
    {
      var verticalCount = vertexCountVertical - 1;
      var horizontalCount = vertexCountHorizontal - 1;
      List<int> triangles = new List<int>();
      //最后一行和最后一列不需要进行处理
      for (int i = 0; i < verticalCount; i++)
      {
        for (int j = 0; j < horizontalCount; j++)
        {
          var m = i * (verticalCount + 1) + j;  //index
          var n = (i + 1) * (horizontalCount + 1) + j; // up index
          triangles.Add(m);
          triangles.Add(n);
          triangles.Add(n+1);
          triangles.Add(m);
          triangles.Add(n+1);
          triangles.Add(m+1);
        }
      }
      return triangles.ToArray();
    }

    public static Texture2D[] ExtractSplatmaps(Terrain _terrain)
    {
      if (!((UnityEngine.Object) _terrain != (UnityEngine.Object) null) ||
          !((UnityEngine.Object) _terrain.terrainData != (UnityEngine.Object) null))
        return (Texture2D[]) null;
      Texture2D[] texture2DArray = new Texture2D[_terrain.terrainData.alphamapTextures.Length];
      for (int index = 0; index < texture2DArray.Length; ++index)
      {
        Texture2D alphamapTexture = _terrain.terrainData.alphamapTextures[index];
        if ((UnityEngine.Object) alphamapTexture == (UnityEngine.Object) null)
        {
          texture2DArray[index] = SimplygonTerrainConvert.GetBlackTexture(_terrain.terrainData.alphamapResolution,
            _terrain.terrainData.alphamapResolution);
        }
        else
        {
          texture2DArray[index] = new Texture2D(alphamapTexture.width, alphamapTexture.height);
          texture2DArray[index].SetPixels(alphamapTexture.GetPixels());
          texture2DArray[index].Apply();
        }

        texture2DArray[index].wrapMode = TextureWrapMode.Clamp;
        texture2DArray[index].name = "Splatmap " + (object) index;
      }

      return texture2DArray;
    }

    public static Texture2D[] ExtractSplatmaps(Terrain _terrain, int _width, int _height)
    {
      if ((UnityEngine.Object) _terrain == (UnityEngine.Object) null &&
          (UnityEngine.Object) _terrain.terrainData == (UnityEngine.Object) null ||
          (_terrain.terrainData.alphamapTextures == null || _terrain.terrainData.alphamapTextures.Length == 0))
        return (Texture2D[]) null;
      Texture2D[] texture2DArray = new Texture2D[_terrain.terrainData.alphamapTextures.Length];
      for (int index = 0; index < _terrain.terrainData.alphamapTextures.Length; ++index)
      {
        SimplygonTerrainConvert.ResizePro((Texture) _terrain.terrainData.alphamapTextures[index], _width, _height,
          out texture2DArray[index], false);
        if ((UnityEngine.Object) texture2DArray[index] != (UnityEngine.Object) null)
          texture2DArray[index].name = "Splatmap " + (object) index;
      }

      return texture2DArray;
    }

    public static Texture2D ExtractHolesmap(Terrain _terrain)
    {
      if (!((UnityEngine.Object) _terrain != (UnityEngine.Object) null) ||
          !((UnityEngine.Object) _terrain.terrainData != (UnityEngine.Object) null) ||
          !((UnityEngine.Object) _terrain.terrainData.holesTexture != (UnityEngine.Object) null))
        return (Texture2D) null;
      TerrainData terrainData = _terrain.terrainData;
      bool flag = terrainData.heightmapTexture.format == RenderTextureFormat.R8 &&
                  SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.R8) &&
                  SystemInfo.SupportsTextureFormat(TextureFormat.R8);
      bool textureCompression = terrainData.enableHolesTextureCompression;
      terrainData.enableHolesTextureCompression = false;
      int num = terrainData.holesResolution < 2 ? 2 : terrainData.holesResolution;
      RenderTexture temporary = RenderTexture.GetTemporary(num, num, 0);
      temporary.Create();
      Graphics.Blit(terrainData.holesTexture, temporary);
      Texture2D texture2D = (Texture2D) null;
      SimplygonTerrainConvert.RenderTextureToTexture2D(temporary, flag ? TextureFormat.R8 : TextureFormat.RGBA32,
        ref texture2D);
      texture2D.wrapMode = TextureWrapMode.Clamp;
      texture2D.name = "Holesmap";
      RenderTexture.ReleaseTemporary(temporary);
      terrainData.enableHolesTextureCompression = textureCompression;
      return texture2D;
    }

    public static Texture2D ExtractHolesmap(Terrain _terrain, int _width, int _height)
    {
      if (!((UnityEngine.Object) _terrain != (UnityEngine.Object) null) ||
          !((UnityEngine.Object) _terrain.terrainData != (UnityEngine.Object) null) ||
          !((UnityEngine.Object) _terrain.terrainData.holesTexture != (UnityEngine.Object) null))
        return (Texture2D) null;
      Texture2D dstTexture = (Texture2D) null;
      SimplygonTerrainConvert.ResizePro(_terrain.terrainData.holesTexture, _width, _height, out dstTexture, true);
      dstTexture.name = "Holesmap";
      return dstTexture;
    }

    public static void ExtractBasemaps(
      Terrain _terrain,
      out Texture2D[] _diffuseMap,
      out Texture2D[] _normalMap,
      int _textureWidth,
      int _textureHeight,
      int _splitCountHorizontal,
      int _splitCountVertical,
      bool sRGB)
    {
      if (_splitCountHorizontal < 1)
        _splitCountHorizontal = 1;
      if (_splitCountVertical < 1)
        _splitCountVertical = 1;
      _diffuseMap = new Texture2D[_splitCountHorizontal * _splitCountHorizontal];
      _normalMap = new Texture2D[_splitCountHorizontal * _splitCountHorizontal];
      int index1 = 0;
      for (int index2 = 0; index2 < _splitCountVertical; ++index2)
      {
        for (int index3 = 0; index3 < _splitCountHorizontal; ++index3)
        {
          SimplygonTerrainConvert.basemapSplit = true;
          SimplygonTerrainConvert.basemapSplitOffsetScale = new Vector4(1f / (float) _splitCountHorizontal,
            1f / (float) _splitCountVertical, (float) index3 / (float) _splitCountHorizontal,
            (float) index2 / (float) _splitCountVertical);
          Texture2D _diffuseMap1;
          Texture2D _normalMap1;
          SimplygonTerrainConvert.ExtractBasemap(_terrain, out _diffuseMap1, out _normalMap1, _textureWidth,
            _textureHeight, sRGB);
          if ((UnityEngine.Object) _diffuseMap1 != (UnityEngine.Object) null)
            _diffuseMap1.name = _terrain.name + "_Basemap_Diffuse_x" + (object) index3 + "_y" + (object) index2;
          if ((UnityEngine.Object) _normalMap1 != (UnityEngine.Object) null)
            _normalMap1.name = _terrain.name + "_Basemap_Normal_" + (object) index3 + "_" + (object) index2;
          _diffuseMap[index1] = _diffuseMap1;
          _normalMap[index1] = _normalMap1;
          ++index1;
        }
      }
    }

    public static void ExtractBasemap(
      Terrain _terrain,
      out Texture2D _diffuseMap,
      out Texture2D _normalMap,
      int _width,
      int _height,
      bool sRGB)
    {
      if (_width < 4)
        _width = 4;
      if (_height < 4)
        _height = 4;
      _diffuseMap = (Texture2D) null;
      _normalMap = (Texture2D) null;
      if ((UnityEngine.Object) _terrain == (UnityEngine.Object) null ||
          (UnityEngine.Object) _terrain.terrainData == (UnityEngine.Object) null ||
          (_terrain.terrainData.alphamapTextures == null || _terrain.terrainData.alphamapTextures.Length == 0))
      {
        Debug.LogWarning((object) "Can not create basemap, terrain has no splatmaps\n");
      }
      else
      {
        Shader shader = Shader.Find("Hidden/VacuumShaders/Terrain To Mesh/Basemap");
        if ((UnityEngine.Object) shader == (UnityEngine.Object) null)
        {
          Debug.LogWarning((object) "'Hidden/VacuumShaders/Terrain To Mesh/Basemap' shader not found\n");
        }
        else
        {
          Material mat = new Material(shader);
          Texture2D[] alphamapTextures = _terrain.terrainData.alphamapTextures;
          Texture2D[] _diffuseTextures;
          Texture2D[] _normalTextures;
          Vector2[] _uvScale;
          Vector2[] _uvOffset;
          int texturesInfo = SimplygonTerrainConvert.ExtractTexturesInfo(_terrain, out _diffuseTextures,
            out _normalTextures, out _uvScale, out _uvOffset, out float[] _, out float[] _);
          if (texturesInfo == 0)
          {
            Debug.LogWarning((object) "Terrain has no enough data for Basemap generating\n");
          }
          else
          {
            Texture2D texture2D1 = new Texture2D(2, 2, TextureFormat.ARGB32, false);
            texture2D1.SetPixels(new Color[4]
            {
              Color.clear,
              Color.clear,
              Color.clear,
              Color.clear
            });
            texture2D1.Apply();
            RenderTexture active = RenderTexture.active;
            RenderTexture temporary1 = RenderTexture.GetTemporary(_width, _height, 24);
            RenderTexture temporary2 = RenderTexture.GetTemporary(_width, _height, 24);
            if (SimplygonTerrainConvert.basemapSplit)
            {
              mat.SetVector("_V_T2M_Splat_uvOffset", SimplygonTerrainConvert.basemapSplitOffsetScale);
              SimplygonTerrainConvert.basemapSplit = false;
            }
            else
              mat.SetVector("_V_T2M_Splat_uvOffset", new Vector4(1f, 1f, 0.0f, 0.0f));

            if (_diffuseTextures != null)
            {
              temporary1.DiscardContents();
              SimplygonTerrainConvert.Blit((Texture) texture2D1, temporary1, false);
              for (int index = 0; index < texturesInfo; ++index)
              {
                if (index % 4 == 0)
                  mat.SetTexture("_V_T2M_Control", (Texture) alphamapTextures[index / 4]);
                mat.SetFloat("_V_T2M_ChannelIndex", 0.5f + (float) (index % 4));
                if ((UnityEngine.Object) _diffuseTextures[index] == (UnityEngine.Object) null)
                {
                  Debug.LogWarning((object) ("Terrain '" + _terrain.name + "' is missing diffuse texture " +
                                             (object) index));
                  mat.SetTexture("_V_T2M_Splat_D", (Texture) null);
                }
                else
                {
                  mat.SetTexture("_V_T2M_Splat_D", (Texture) _diffuseTextures[index]);
                  mat.SetVector("_V_T2M_Splat_uvScale",
                    new Vector4(_uvScale[index].x, _uvScale[index].y, _uvOffset[index].x, _uvOffset[index].y));
                }

                temporary2.DiscardContents();
                SimplygonTerrainConvert.Blit((Texture) temporary1, temporary2, mat, sRGB, 0);
                temporary1.DiscardContents();
                SimplygonTerrainConvert.Blit((Texture) temporary2, temporary1, sRGB);
              }

              RenderTexture.active = temporary1;
              _diffuseMap = new Texture2D(_width, _height, TextureFormat.ARGB32, true);
              _diffuseMap.ReadPixels(new Rect(0.0f, 0.0f, (float) _width, (float) _height), 0, 0);
              _diffuseMap.Apply();
              _diffuseMap.wrapMode = TextureWrapMode.Clamp;
            }

            Texture2D texture2D2 = new Texture2D(2, 2, TextureFormat.ARGB32, false, true);
            Color color = new Color(0.5f, 0.5f, 0.5f, 0.5f);
            texture2D2.SetPixels(new Color[4]
            {
              color,
              color,
              color,
              color
            });
            texture2D2.Apply();
            if (_normalTextures != null)
            {
              temporary1.DiscardContents();
              SimplygonTerrainConvert.Blit((Texture) texture2D1, temporary1, false);
              for (int index = 0; index < texturesInfo; ++index)
              {
                if (index % 4 == 0)
                  mat.SetTexture("_V_T2M_Control", (Texture) alphamapTextures[index / 4]);
                mat.SetFloat("_V_T2M_ChannelIndex", 0.5f + (float) (index % 4));
                if ((UnityEngine.Object) _normalTextures[index] == (UnityEngine.Object) null)
                {
                  Debug.LogWarning((object) ("Terrain '" + _terrain.name + "' is missing normal texture " +
                                             (object) index));
                  mat.SetTexture("_V_T2M_Splat_N", (Texture) texture2D2);
                  mat.SetVector("_V_T2M_Splat_uvScale", Vector4.one);
                }
                else
                {
                  mat.SetTexture("_V_T2M_Splat_N", (Texture) _normalTextures[index]);
                  mat.SetVector("_V_T2M_Splat_uvScale",
                    new Vector4(_uvScale[index].x, _uvScale[index].y, _uvOffset[index].x, _uvOffset[index].y));
                }

                temporary2.DiscardContents();
                SimplygonTerrainConvert.Blit((Texture) temporary1, temporary2, mat, sRGB, 1);
                temporary1.DiscardContents();
                SimplygonTerrainConvert.Blit((Texture) temporary2, temporary1, sRGB);
              }

              RenderTexture.active = temporary1;
              _normalMap = new Texture2D(_width, _height, TextureFormat.ARGB32, true);
              _normalMap.ReadPixels(new Rect(0.0f, 0.0f, (float) _width, (float) _height), 0, 0);
              _normalMap.Apply();
              _normalMap.wrapMode = TextureWrapMode.Clamp;
            }

            RenderTexture.active = (RenderTexture) null;
            if (Application.isPlaying)
              RenderTexture.active = active;
            UnityEngine.Object.DestroyImmediate((UnityEngine.Object) mat);
            UnityEngine.Object.DestroyImmediate((UnityEngine.Object) texture2D1);
            UnityEngine.Object.DestroyImmediate((UnityEngine.Object) texture2D2);
            RenderTexture.ReleaseTemporary(temporary1);
            RenderTexture.ReleaseTemporary(temporary2);
          }
        }
      }
    }

    public static Texture2D ExtractHeightmap(Terrain _terrain, bool _remap)
    {
      if ((UnityEngine.Object) _terrain == (UnityEngine.Object) null ||
          (UnityEngine.Object) _terrain.terrainData == (UnityEngine.Object) null)
        return (Texture2D) null;
      int heightmapResolution1 = _terrain.terrainData.heightmapResolution;
      int heightmapResolution2 = _terrain.terrainData.heightmapResolution;
      Texture2D texture2D = new Texture2D(heightmapResolution1, heightmapResolution2, TextureFormat.ARGB32, true);
      float[,] heights = _terrain.terrainData.GetHeights(0, 0, heightmapResolution1, heightmapResolution2);
      float from1 = 1f;
      float to1 = 0.0f;
      if (_remap)
      {
        for (int index1 = 0; index1 < texture2D.height; ++index1)
        {
          for (int index2 = 0; index2 < texture2D.width; ++index2)
          {
            float num = heights[index1, index2];
            if ((double) num < (double) from1)
              from1 = num;
            if ((double) num > (double) to1)
              to1 = num;
          }
        }
      }

      for (int y = 0; y < texture2D.height; ++y)
      {
        for (int x = 0; x < texture2D.width; ++x)
        {
          float num = heights[y, x];
          if (_remap)
            num = SimplygonTerrainConvert.Remap(num, from1, to1, 0.0f, 1f);
          Color color = new Color(num, num, num, 1f);
          texture2D.SetPixel(x, y, color);
        }
      }

      texture2D.Apply();
      return texture2D;
    }

    public static Texture2D ExtractHeightmap(
      Terrain _terrain,
      int _width,
      int _height,
      bool _remap)
    {
      Texture2D heightmap = SimplygonTerrainConvert.ExtractHeightmap(_terrain, _remap);
      Texture2D dstTexture;
      SimplygonTerrainConvert.ResizePro((Texture) heightmap, _width, _height, out dstTexture, false);
      UnityEngine.Object.DestroyImmediate((UnityEngine.Object) heightmap);
      return dstTexture;
    }

    public static int ExtractTexturesInfo(
      Terrain _terrain,
      out Texture2D[] _diffuseTextures,
      out Texture2D[] _normalTextures,
      out Vector2[] _uvScale,
      out Vector2[] _uvOffset,
      out float[] _metalic,
      out float[] _smoothness)
    {
      _diffuseTextures = (Texture2D[]) null;
      _normalTextures = (Texture2D[]) null;
      _uvScale = (Vector2[]) null;
      _uvOffset = (Vector2[]) null;
      _metalic = (float[]) null;
      _smoothness = (float[]) null;
      if ((UnityEngine.Object) _terrain == (UnityEngine.Object) null ||
          (UnityEngine.Object) _terrain.terrainData == (UnityEngine.Object) null ||
          (_terrain.terrainData.terrainLayers == null || _terrain.terrainData.terrainLayers.Length == 0))
        return 0;
      int length = _terrain.terrainData.terrainLayers.Length;
      _diffuseTextures = new Texture2D[length];
      _normalTextures = new Texture2D[length];
      _uvScale = new Vector2[length];
      _uvOffset = new Vector2[length];
      _metalic = new float[length];
      _smoothness = new float[length];
      for (int index = 0; index < length; ++index)
      {
        TerrainLayer terrainLayer = _terrain.terrainData.terrainLayers[index];
        if ((UnityEngine.Object) terrainLayer == (UnityEngine.Object) null)
        {
          _diffuseTextures[index] = (Texture2D) null;
          _normalTextures[index] = (Texture2D) null;
          _uvScale[index] = Vector2.one;
          _uvOffset[index] = Vector2.zero;
          _metalic[index] = 0.0f;
          _smoothness[index] = 0.0f;
        }
        else
        {
          _diffuseTextures[index] = !((UnityEngine.Object) terrainLayer.diffuseTexture == (UnityEngine.Object) null)
            ? terrainLayer.diffuseTexture
            : (Texture2D) null;
          _normalTextures[index] = !((UnityEngine.Object) terrainLayer.normalMapTexture == (UnityEngine.Object) null)
            ? terrainLayer.normalMapTexture
            : (Texture2D) null;
          float x1 = (double) terrainLayer.tileSize.x == 0.0
            ? 0.0f
            : _terrain.terrainData.size.x / terrainLayer.tileSize.x;
          float y1 = (double) terrainLayer.tileSize.y == 0.0
            ? 0.0f
            : _terrain.terrainData.size.z / terrainLayer.tileSize.y;
          float x2 = (double) terrainLayer.tileSize.x == 0.0
            ? 0.0f
            : terrainLayer.tileOffset.x / terrainLayer.tileSize.x;
          float y2 = (double) terrainLayer.tileSize.y == 0.0
            ? 0.0f
            : terrainLayer.tileOffset.y / terrainLayer.tileSize.y;
          _uvScale[index] = new Vector2(x1, y1);
          _uvOffset[index] = new Vector2(x2, y2);
          _metalic[index] = terrainLayer.metallic;
          _smoothness[index] = terrainLayer.smoothness;
        }
      }

      bool flag1 = false;
      for (int index = 0; index < _diffuseTextures.Length; ++index)
      {
        if ((UnityEngine.Object) _diffuseTextures[index] != (UnityEngine.Object) null)
          flag1 = true;
      }

      if (!flag1)
        _diffuseTextures = (Texture2D[]) null;
      bool flag2 = false;
      for (int index = 0; index < _normalTextures.Length; ++index)
      {
        if ((UnityEngine.Object) _normalTextures[index] != (UnityEngine.Object) null)
          flag2 = true;
      }

      if (!flag2)
        _normalTextures = (Texture2D[]) null;
      if (!flag1 && !flag2)
      {
        _uvScale = (Vector2[]) null;
        _uvOffset = (Vector2[]) null;
        length = 0;
      }

      return length;
    }

    public static GameObject ExtractTrees(Terrain _terrain)
    {
      if ((UnityEngine.Object) _terrain == (UnityEngine.Object) null ||
          (UnityEngine.Object) _terrain.terrainData == (UnityEngine.Object) null ||
          _terrain.terrainData.treePrototypes == null)
        return (GameObject) null;
      TerrainData terrainData = _terrain.terrainData;
      GameObject gameObject1 = new GameObject(_terrain.name + "_Trees");
      gameObject1.transform.position = Vector3.zero;
      gameObject1.transform.rotation = Quaternion.identity;
      GameObject[] gameObjectArray = new GameObject[terrainData.treePrototypes.Length];
      for (int index = 0; index < terrainData.treePrototypes.Length; ++index)
      {
        if (terrainData.treePrototypes[index] != null &&
            !((UnityEngine.Object) terrainData.treePrototypes[index].prefab == (UnityEngine.Object) null))
        {
          gameObjectArray[index] = new GameObject(terrainData.treePrototypes[index].prefab.name);
          gameObjectArray[index].transform.position = Vector3.zero;
          gameObjectArray[index].transform.rotation = Quaternion.identity;
          gameObjectArray[index].transform.parent = gameObject1.transform;
        }
      }

      for (int index = 0; index < terrainData.treeInstances.Length; ++index)
      {
        TreeInstance treeInstance = terrainData.treeInstances[index];
        if (terrainData.treePrototypes[treeInstance.prototypeIndex] != null &&
            !((UnityEngine.Object) terrainData.treePrototypes[treeInstance.prototypeIndex].prefab ==
              (UnityEngine.Object) null))
        {
          GameObject gameObject2 = UnityEngine.Object.Instantiate<GameObject>(
            terrainData.treePrototypes[treeInstance.prototypeIndex].prefab, Vector3.zero, Quaternion.identity);
          gameObject2.name = gameObject2.name.Replace("(Clone)", string.Empty);
          Vector3 position = treeInstance.position;
          position.x *= terrainData.size.x;
          position.y *= terrainData.size.y;
          position.z *= terrainData.size.z;
          gameObject2.transform.position = position;
          gameObject2.transform.localRotation = Quaternion.AngleAxis(57.29578f * treeInstance.rotation, Vector3.up);
          Vector3 localScale = terrainData.treePrototypes[treeInstance.prototypeIndex].prefab.transform.localScale;
          Vector3 one = Vector3.one;
          one.x *= treeInstance.widthScale * localScale.x;
          one.y *= treeInstance.heightScale * localScale.y;
          one.z *= treeInstance.widthScale * localScale.z;
          gameObject2.transform.localScale = one;
          gameObject2.transform.parent = gameObjectArray[treeInstance.prototypeIndex].transform;
        }
      }

      int num = 0;
      for (int index = gameObjectArray.Length - 1; index >= 0; --index)
      {
        if ((UnityEngine.Object) gameObjectArray[index] != (UnityEngine.Object) null)
        {
          int childCount = gameObjectArray[index].transform.childCount;
          if (childCount == 0)
          {
            UnityEngine.Object.DestroyImmediate((UnityEngine.Object) gameObjectArray[index]);
          }
          else
          {
            GameObject gameObject2 = gameObjectArray[index];
            gameObject2.name = gameObject2.name + " (" + (object) childCount + ")";
            num += childCount;
          }
        }
      }

      GameObject gameObject3 = gameObject1;
      gameObject3.name = gameObject3.name + " (" + (object) num + ")";
      return gameObject1;
    }

    public static string TerrainToOBJ(
      Terrain _terrain,
      int _vertexCountHorizontal,
      int _vertexCountVertical)
    {
      Vector3[] _vertices = (Vector3[]) null;
      int[] _trinagles = (int[]) null;
      Vector3[] _normals = (Vector3[]) null;
      Vector2[] _uvs = (Vector2[]) null;
      SimplygonTerrainConvert.GenerateTerrainOBJ(_terrain, _vertexCountHorizontal, _vertexCountVertical, ref _vertices,
        ref _trinagles, ref _normals, ref _uvs);
      StringBuilder stringBuilder = new StringBuilder();
      stringBuilder.Append("# TerrainToMesh OBJ File");
      stringBuilder.Append(Environment.NewLine);
      stringBuilder.Append("# File Created: " + DateTime.Now.ToString());
      stringBuilder.Append(Environment.NewLine + " ");
      stringBuilder.Append(Environment.NewLine);
      stringBuilder.Append("g " + _terrain.name);
      stringBuilder.Append(Environment.NewLine + " ");
      stringBuilder.Append(Environment.NewLine);
      for (int index = 0; index < _vertices.Length; ++index)
      {
        stringBuilder.Append("v ");
        stringBuilder.Append((_vertices[index].x * -1f).ToString()).Append(" ");
        stringBuilder.Append(_vertices[index].y.ToString()).Append(" ");
        stringBuilder.Append(_vertices[index].z.ToString());
        stringBuilder.Append(Environment.NewLine);
      }

      for (int index = 0; index < _uvs.Length; ++index)
      {
        stringBuilder.Append("vt ");
        stringBuilder.Append(_uvs[index].x.ToString()).Append(" ");
        stringBuilder.Append(_uvs[index].y.ToString());
        stringBuilder.Append(Environment.NewLine);
      }

      for (int index = 0; index < _normals.Length; ++index)
      {
        stringBuilder.Append("vn ");
        stringBuilder.Append((_normals[index].x * -1f).ToString()).Append(" ");
        stringBuilder.Append(_normals[index].y.ToString()).Append(" ");
        stringBuilder.Append(_normals[index].z.ToString());
        stringBuilder.Append(Environment.NewLine);
      }

      for (int index = 0; index < _trinagles.Length; index += 3)
      {
        stringBuilder.Append("f ");
        stringBuilder.Append(_trinagles[index + 2] + 1).Append("/").Append(_trinagles[index + 2] + 1).Append("/")
          .Append(_trinagles[index + 2] + 1).Append(" ");
        stringBuilder.Append(_trinagles[index + 1] + 1).Append("/").Append(_trinagles[index + 1] + 1).Append("/")
          .Append(_trinagles[index + 1] + 1).Append(" ");
        stringBuilder.Append(_trinagles[index] + 1).Append("/").Append(_trinagles[index] + 1).Append("/")
          .Append(_trinagles[index] + 1);
        stringBuilder.Append(Environment.NewLine);
      }

      return stringBuilder.ToString();
    }

    private static void GenerateTerrainOBJ(
      Terrain _terrain,
      int _vCountH,
      int vCountV,
      ref Vector3[] _vertices,
      ref int[] _trinagles,
      ref Vector3[] _normals,
      ref Vector2[] _uvs)
    {
      TerrainData terrainData = _terrain.terrainData;
      int num1 = vCountV - 1;
      int num2 = _vCountH - 1;
      if (num1 < 1)
        num1 = 1;
      if (num2 < 1)
        num2 = 1;
      double x = (double) terrainData.size.x;
      float z = terrainData.size.z;
      double num3 = (double) num2;
      float num4 = (float) (x / num3);
      float num5 = z / (float) num1;
      int num6 = num1 + 1;
      int num7 = num2 + 1;
      _vertices = new Vector3[num7 * num6];
      _trinagles = new int[num1 * num2 * 2 * 3];
      _normals = (Vector3[]) null;
      _uvs = new Vector2[num7 * num6];
      int index1 = -1;
      for (int index2 = 0; index2 < num6; ++index2)
      {
        for (int index3 = 0; index3 < num7; ++index3)
        {
          Vector3 worldPosition = new Vector3((float) index3 * num4, 0.0f, (float) index2 * num5);
          worldPosition.y = _terrain.SampleHeight(worldPosition);
          _vertices[++index1] = worldPosition;
          _uvs[index1] = new Vector2(Mathf.Clamp01(worldPosition.x / terrainData.size.x),
            Mathf.Clamp01(worldPosition.z / terrainData.size.z));
        }
      }

      int num8 = 0;
      int num9 = -1;
      for (int index2 = 0; index2 < num1; ++index2)
      {
        int num10 = index2 * num7;
        num8 += num7;
        for (int index3 = 0; index3 < num2; ++index3)
        {
          int num11;
          _trinagles[num11 = num9 + 1] = num10 + index3;
          int num12;
          _trinagles[num12 = num11 + 1] = num8 + index3;
          int num13;
          _trinagles[num13 = num12 + 1] = num8 + index3 + 1;
          int num14;
          _trinagles[num14 = num13 + 1] = num10 + index3 + 1;
          int num15;
          _trinagles[num15 = num14 + 1] = num10 + index3;
          _trinagles[num9 = num15 + 1] = num8 + index3 + 1;
        }
      }

      SimplygonTerrainConvert.GenerateNormals(_vertices, _trinagles, out _normals);
    }

    private static void GenerateTerrainBaseChunks(ref PreMesh[] _preMeshes)
    {
      _preMeshes = new PreMesh[SimplygonTerrainConvert.terrainConvertInfo.chunkCountHorizontal * SimplygonTerrainConvert.terrainConvertInfo.chunkCountVertical];
      TerrainData terrainData = SimplygonTerrainConvert.terrain.terrainData;
      int vertexCountVertical = SimplygonTerrainConvert.terrainConvertInfo.vertexCountVertical - 1;
      int vertexCountHorizontal = SimplygonTerrainConvert.terrainConvertInfo.vertexCountHorizontal - 1;
      if (vertexCountVertical < 1)
        vertexCountVertical = 1;
      if (vertexCountHorizontal < 1)
        vertexCountHorizontal = 1;
      int index1 = -1;
      for (int j = 0; j < SimplygonTerrainConvert.terrainConvertInfo.chunkCountVertical; j++)
      {
        for (int i = 0; i < SimplygonTerrainConvert.terrainConvertInfo.chunkCountHorizontal; i++)
        {
          float chunkSizeHorizontal = SimplygonTerrainConvert.terrain.terrainData.size.x / (float)SimplygonTerrainConvert.terrainConvertInfo.chunkCountHorizontal;
          float chunkOffsetHorizontal = (float)i * chunkSizeHorizontal;
          float chunkSizeVertical = SimplygonTerrainConvert.terrain.terrainData.size.z / (float)SimplygonTerrainConvert.terrainConvertInfo.chunkCountVertical;
          float chunkOffsetVertical = (float)j * chunkSizeVertical;
          float vertexSpaceHorizontal = chunkSizeHorizontal / (float)vertexCountHorizontal;
          float vertexSpaceVertical = chunkSizeVertical / (float)vertexCountVertical;
          int realVertexCountVertical = vertexCountVertical + 1 + 2;
          int realVertexCountHorizontal = vertexCountHorizontal + 1 + 2;
          
          List<Vector3> vertices = new List<Vector3>();
          List<Vector2> uvs = new List<Vector2>();
          List<int> triangles = new List<int>();
          for (int k = 0; k < realVertexCountVertical; k++)
          {
            for (int l = 0; l < realVertexCountHorizontal; l++)
            {
              float verticeY = SimplygonTerrainConvert.terrain.SampleHeight(new Vector3((chunkOffsetHorizontal + (float)l * vertexSpaceHorizontal - vertexSpaceHorizontal), 0f, (chunkOffsetVertical + (float)k * vertexSpaceVertical - vertexSpaceVertical)));
              Vector3 vertice = new Vector3((float)l * vertexSpaceHorizontal - vertexSpaceHorizontal, verticeY, (float)k * vertexSpaceVertical - vertexSpaceVertical);
              vertices.Add(vertice);
              Vector2 uv = new Vector2(Mathf.Clamp01((chunkOffsetHorizontal + (float)l * vertexSpaceHorizontal - vertexSpaceHorizontal) / terrainData.size.x), Mathf.Clamp01((chunkOffsetVertical + (float)k * vertexSpaceVertical - vertexSpaceVertical) / terrainData.size.z));
              uvs.Add(uv);
            }
          }

          int triangleAtTL = 0;
          int triangleAtBR = 0;
          for (int m = 0; m < vertexCountVertical + 2; m++)
          {
              triangleAtTL = m * realVertexCountHorizontal;
              triangleAtBR += realVertexCountHorizontal;
              for (int n = 0; n < vertexCountHorizontal + 2; n++)
              {
                  triangles.Add(triangleAtTL + n);
                  triangles.Add(triangleAtBR + n);
                  triangles.Add(triangleAtBR + n + 1);
                  triangles.Add(triangleAtTL + n + 1);
                  triangles.Add(triangleAtTL + n);
                  triangles.Add(triangleAtBR + n + 1);
              }
          }

          ++index1;
          _preMeshes[index1] = new PreMesh();
          _preMeshes[index1].name =
            (string.IsNullOrEmpty(SimplygonTerrainConvert.terrain.terrainData.name)
              ? "TerrainID_" + (object) SimplygonTerrainConvert.terrain.GetInstanceID()
              : SimplygonTerrainConvert.terrain.terrainData.name) +
            string.Format("_x{0}_y{1}", (object) i, (object) j);
          _preMeshes[index1].vertices = vertices.ToArray();
          _preMeshes[index1].uv = uvs.ToArray();
          _preMeshes[index1].uv2 = uvs.ToArray();
          _preMeshes[index1].triangles = triangles.ToArray();
          SimplygonTerrainConvert.GenerateNormals(_preMeshes[index1].vertices, _preMeshes[index1].triangles, out _preMeshes[index1].normals);
          SimplygonTerrainConvert.GenerateTangents(_preMeshes[index1].vertices, _preMeshes[index1].triangles, _preMeshes[index1].normals, _preMeshes[index1].uv, out _preMeshes[index1].tangents);
        }
      }
    }

    private static Mesh GenerateTerrainMainChunks(
      ref PreMesh _preMesh,
      float _chunkH_Width,
      float _chunkH_StartOffset,
      float _chunkV_Length,
      float _chunkV_StartOffset,
      bool _normalizeUV)
    {
      int num1 = SimplygonTerrainConvert.terrainConvertInfo.vertexCountVertical - 1;
      if (SimplygonTerrainConvert.terrainConvertInfo.vertexCountHorizontal - 1 < 1)
      {
        
      }
      List<Vector3> vector3List1 = new List<Vector3>((IEnumerable<Vector3>) _preMesh.vertices);
      List<int> intList1 = new List<int>();
      List<int> intList2 = new List<int>((IEnumerable<int>) _preMesh.triangles);
      List<Vector3> vector3List2 = new List<Vector3>((IEnumerable<Vector3>) _preMesh.vertices);
      int[] numArray1 = new int[2 * SimplygonTerrainConvert.terrainConvertInfo.vertexCountHorizontal +
                                2 * SimplygonTerrainConvert.terrainConvertInfo.vertexCountVertical + 4];
      int num2 = 0;
      for (int index = 0; index < SimplygonTerrainConvert.terrainConvertInfo.vertexCountHorizontal + 2; ++index)
        numArray1[num2++] = index;
      int num3 = SimplygonTerrainConvert.terrainConvertInfo.vertexCountHorizontal + 1;
      for (int index1 = 0; index1 < SimplygonTerrainConvert.terrainConvertInfo.vertexCountVertical; ++index1)
      {
        int num4 = num3 + 1;
        int[] numArray2 = numArray1;
        int index2 = num2;
        int num5 = index2 + 1;
        int num6 = num4;
        numArray2[index2] = num6;
        num3 = num4 + (SimplygonTerrainConvert.terrainConvertInfo.vertexCountHorizontal + 1);
        int[] numArray3 = numArray1;
        int index3 = num5;
        num2 = index3 + 1;
        int num7 = num3;
        numArray3[index3] = num7;
      }

      for (int index = 1; index < SimplygonTerrainConvert.terrainConvertInfo.vertexCountHorizontal + 3; ++index)
        numArray1[num2++] = num3 + index;
      int[] numArray4 = new int[4 * (SimplygonTerrainConvert.terrainConvertInfo.vertexCountHorizontal - 1) +
                                4 * (SimplygonTerrainConvert.terrainConvertInfo.vertexCountVertical - 1) + 8];
      int num8 = 0;
      int num9 = 0;
      for (int index1 = 0; index1 < SimplygonTerrainConvert.terrainConvertInfo.vertexCountHorizontal + 1; ++index1)
      {
        int[] numArray2 = numArray4;
        int index2 = num8;
        int num4 = index2 + 1;
        int num5 = num9;
        int num6 = num5 + 1;
        numArray2[index2] = num5;
        int[] numArray3 = numArray4;
        int index3 = num4;
        num8 = index3 + 1;
        int num7 = num6;
        num9 = num7 + 1;
        numArray3[index3] = num7;
      }

      for (int index1 = 0; index1 < SimplygonTerrainConvert.terrainConvertInfo.vertexCountVertical - 1; ++index1)
      {
        int[] numArray2 = numArray4;
        int index2 = num8;
        int num4 = index2 + 1;
        int num5 = num9;
        int num6 = num5 + 1;
        numArray2[index2] = num5;
        int[] numArray3 = numArray4;
        int index3 = num4;
        int num7 = index3 + 1;
        int num10 = num6;
        int num11 = num10 + 1;
        numArray3[index3] = num10;
        int num12 = num11 + 2 * (SimplygonTerrainConvert.terrainConvertInfo.vertexCountHorizontal - 1);
        int[] numArray5 = numArray4;
        int index4 = num7;
        int num13 = index4 + 1;
        int num14 = num12;
        int num15 = num14 + 1;
        numArray5[index4] = num14;
        int[] numArray6 = numArray4;
        int index5 = num13;
        num8 = index5 + 1;
        int num16 = num15;
        num9 = num16 + 1;
        numArray6[index5] = num16;
      }

      for (int index1 = 0; index1 < SimplygonTerrainConvert.terrainConvertInfo.vertexCountHorizontal + 1; ++index1)
      {
        int[] numArray2 = numArray4;
        int index2 = num8;
        int num4 = index2 + 1;
        int num5 = num9;
        int num6 = num5 + 1;
        numArray2[index2] = num5;
        int[] numArray3 = numArray4;
        int index3 = num4;
        num8 = index3 + 1;
        int num7 = num6;
        num9 = num7 + 1;
        numArray3[index3] = num7;
      }

      for (int index = numArray4.Length - 1; index >= 0; --index)
      {
        int num4 = numArray4[index];
        intList2.RemoveAt(num4 * 3 + 2);
        intList2.RemoveAt(num4 * 3 + 1);
        intList2.RemoveAt(num4 * 3);
      }

      int num17 = SimplygonTerrainConvert.terrainConvertInfo.vertexCountHorizontal + 3;
      int num18 = -1;
      for (int index1 = 0; index1 < SimplygonTerrainConvert.terrainConvertInfo.vertexCountVertical - 1; ++index1)
      {
        for (int index2 = 0; index2 < SimplygonTerrainConvert.terrainConvertInfo.vertexCountHorizontal - 1; ++index2)
        {
          int index3;
          intList2[index3 = num18 + 1] -= num17;
          int num4 = intList2[index3];
          int num5;
          intList2[num5 = index3 + 1] = num4 + SimplygonTerrainConvert.terrainConvertInfo.vertexCountHorizontal;
          int num6;
          intList2[num6 = num5 + 1] = num4 + SimplygonTerrainConvert.terrainConvertInfo.vertexCountHorizontal + 1;
          int num7;
          intList2[num7 = num6 + 1] = num4 + 1;
          int num10;
          intList2[num10 = num7 + 1] = num4;
          intList2[num18 = num10 + 1] = num4 + SimplygonTerrainConvert.terrainConvertInfo.vertexCountHorizontal + 1;
        }

        num17 += 2;
      }

      List<Vector2> vector2List1 = new List<Vector2>((IEnumerable<Vector2>) _preMesh.uv);
      List<Vector2> vector2List2 = new List<Vector2>((IEnumerable<Vector2>) _preMesh.uv2);
      List<Vector3> vector3List3 = new List<Vector3>((IEnumerable<Vector3>) _preMesh.normals);
      List<Vector4> vector4List = new List<Vector4>((IEnumerable<Vector4>) _preMesh.tangents);
      for (int index = numArray1.Length - 1; index >= 0; --index)
        vector3List2.RemoveAt(numArray1[index]);
      for (int index = numArray1.Length - 1; index >= 0; --index)
        vector2List1.RemoveAt(numArray1[index]);
      for (int index = numArray1.Length - 1; index >= 0; --index)
        vector2List2.RemoveAt(numArray1[index]);
      for (int index = numArray1.Length - 1; index >= 0; --index)
        vector3List3.RemoveAt(numArray1[index]);
      for (int index = numArray1.Length - 1; index >= 0; --index)
        vector4List.RemoveAt(numArray1[index]);
      _preMesh.Clear();
      Mesh mesh = new Mesh();
      mesh.name = _preMesh.name;
      mesh.vertices = vector3List2.ToArray();
      mesh.triangles = intList2.ToArray();
      mesh.uv = _normalizeUV ? SimplygonTerrainConvert.NormalizeUV(vector2List1.ToArray()) : vector2List1.ToArray();
      mesh.uv2 = vector2List2.ToArray();
      mesh.normals = vector3List3.ToArray();
      mesh.tangents = vector4List.ToArray();
      return mesh;
    }

    private static Mesh GenerateTerrain()
    {
      TerrainData terrainData = SimplygonTerrainConvert.terrain.terrainData;
      int num1 = SimplygonTerrainConvert.terrainConvertInfo.vertexCountVertical - 1;
      int num2 = SimplygonTerrainConvert.terrainConvertInfo.vertexCountHorizontal - 1;
      if (num1 < 1)
        num1 = 1;
      if (num2 < 1)
        num2 = 1;
      double x = (double) terrainData.size.x;
      float z = terrainData.size.z;
      double num3 = (double) num2;
      float num4 = (float) (x / num3);
      float num5 = z / (float) num1;
      int num6 = num1 + 1;
      int num7 = num2 + 1;
      Vector3[] _vertices = new Vector3[num7 * num6];
      int[] _triangles = new int[num1 * num2 * 2 * 3];
      Vector2[] _texcoords = new Vector2[num7 * num6];
      int index1 = -1;
      for (int index2 = 0; index2 < num6; ++index2)
      {
        for (int index3 = 0; index3 < num7; ++index3)
        {
          Vector3 worldPosition = new Vector3((float) index3 * num4, 0.0f, (float) index2 * num5);
          worldPosition.y = SimplygonTerrainConvert.terrain.SampleHeight(worldPosition);
          _vertices[++index1] = worldPosition;
          _texcoords[index1] = new Vector2(Mathf.Clamp01(worldPosition.x / terrainData.size.x),
            Mathf.Clamp01(worldPosition.z / terrainData.size.z));
        }
      }

      int num8 = 0;
      int num9 = -1;
      for (int index2 = 0; index2 < num1; ++index2)
      {
        int num10 = index2 * num7;
        num8 += num7;
        for (int index3 = 0; index3 < num2; ++index3)
        {
          int num11;
          _triangles[num11 = num9 + 1] = num10 + index3;
          int num12;
          _triangles[num12 = num11 + 1] = num8 + index3;
          int num13;
          _triangles[num13 = num12 + 1] = num8 + index3 + 1;
          int num14;
          _triangles[num14 = num13 + 1] = num10 + index3 + 1;
          int num15;
          _triangles[num15 = num14 + 1] = num10 + index3;
          _triangles[num9 = num15 + 1] = num8 + index3 + 1;
        }
      }

      Mesh mesh = new Mesh();
      mesh.name = string.IsNullOrEmpty(SimplygonTerrainConvert.terrain.terrainData.name)
        ? "TerrainID_" + (object) SimplygonTerrainConvert.terrain.GetInstanceID()
        : SimplygonTerrainConvert.terrain.terrainData.name;
      // mesh.hideFlags = HideFlags.HideAndDontSave;
      mesh.vertices = _vertices;
      mesh.triangles = _triangles;
      mesh.RecalculateBounds();
      mesh.uv = _texcoords;
      mesh.uv2 = _texcoords;
      mesh.RecalculateNormals();
      Vector3[] normals = mesh.normals;
      Vector4[] _tangents = (Vector4[]) null;
      SimplygonTerrainConvert.GenerateTangents(_vertices, _triangles, normals, _texcoords, out _tangents);
      mesh.tangents = _tangents;
      return mesh;
    }

    private static void AddSkirt(Mesh _sourceMesh)
    {
      if ((UnityEngine.Object) _sourceMesh == (UnityEngine.Object) null)
        return;
      int vertexCountHorizontal = SimplygonTerrainConvert.terrainConvertInfo.vertexCountHorizontal;
      int vertexCountVertical = SimplygonTerrainConvert.terrainConvertInfo.vertexCountVertical;
      int length = 2 * (vertexCountHorizontal + vertexCountVertical);
      int[] numArray1 = new int[length];
      int index1 = 0;
      for (int index2 = 0; index2 < vertexCountHorizontal; ++index2)
      {
        numArray1[index1] = index2;
        numArray1[index1 + vertexCountHorizontal] = vertexCountHorizontal * (vertexCountVertical - 1) + index2;
        ++index1;
      }

      int index3 = 2 * vertexCountHorizontal;
      for (int index2 = 0; index2 < vertexCountVertical; ++index2)
      {
        numArray1[index3] = vertexCountHorizontal * index2;
        numArray1[index3 + vertexCountVertical] = vertexCountHorizontal * (index2 + 1) - 1;
        ++index3;
      }

      Vector3[] vertices = _sourceMesh.vertices;
      int[] triangles = _sourceMesh.triangles;
      Vector2[] uv = _sourceMesh.uv;
      Vector2[] uv2 = _sourceMesh.uv2;
      Vector3[] normals = _sourceMesh.normals;
      Vector4[] tangents = _sourceMesh.tangents;
      Vector3[] vector3Array1 = new Vector3[length];
      int[] numArray2 = new int[(length - 1) * 6];
      Vector2[] vector2Array1 = new Vector2[length];
      Vector2[] vector2Array2 = new Vector2[length];
      Vector3[] vector3Array2 = new Vector3[length];
      Vector4[] vector4Array1 = new Vector4[length];
      for (int index2 = 0; index2 < length; ++index2)
      {
        int index4 = numArray1[index2];
        vector3Array1[index2] = vertices[index4];
        vector3Array1[index2].y = SimplygonTerrainConvert.terrainConvertInfo.skirtGroundLevel;
        vector2Array1[index2] = uv[index4];
        vector2Array2[index2] = uv2[index4];
        vector3Array2[index2] = normals[index4];
        vector4Array1[index2] = tangents[index4];
      }

      int num1 = 0;
      for (int index2 = 0; index2 < SimplygonTerrainConvert.terrainConvertInfo.vertexCountHorizontal - 1; ++index2)
      {
        int[] numArray3 = numArray2;
        int index4 = num1;
        int num2 = index4 + 1;
        int num3 = _sourceMesh.vertexCount + index2;
        numArray3[index4] = num3;
        int[] numArray4 = numArray2;
        int index5 = num2;
        int num4 = index5 + 1;
        int num5 = numArray1[index2];
        numArray4[index5] = num5;
        int[] numArray5 = numArray2;
        int index6 = num4;
        int num6 = index6 + 1;
        int num7 = _sourceMesh.vertexCount + 1 + index2;
        numArray5[index6] = num7;
        int[] numArray6 = numArray2;
        int index7 = num6;
        int num8 = index7 + 1;
        int num9 = numArray1[index2 + 1];
        numArray6[index7] = num9;
        int[] numArray7 = numArray2;
        int index8 = num8;
        int num10 = index8 + 1;
        int num11 = _sourceMesh.vertexCount + 1 + index2;
        numArray7[index8] = num11;
        int[] numArray8 = numArray2;
        int index9 = num10;
        num1 = index9 + 1;
        int num12 = numArray1[index2];
        numArray8[index9] = num12;
      }

      for (int index2 = vertexCountHorizontal; index2 < 2 * vertexCountHorizontal - 1; ++index2)
      {
        int[] numArray3 = numArray2;
        int index4 = num1;
        int num2 = index4 + 1;
        int num3 = numArray1[index2];
        numArray3[index4] = num3;
        int[] numArray4 = numArray2;
        int index5 = num2;
        int num4 = index5 + 1;
        int num5 = _sourceMesh.vertexCount + index2;
        numArray4[index5] = num5;
        int[] numArray5 = numArray2;
        int index6 = num4;
        int num6 = index6 + 1;
        int num7 = _sourceMesh.vertexCount + 1 + index2;
        numArray5[index6] = num7;
        int[] numArray6 = numArray2;
        int index7 = num6;
        int num8 = index7 + 1;
        int num9 = _sourceMesh.vertexCount + 1 + index2;
        numArray6[index7] = num9;
        int[] numArray7 = numArray2;
        int index8 = num8;
        int num10 = index8 + 1;
        int num11 = numArray1[index2 + 1];
        numArray7[index8] = num11;
        int[] numArray8 = numArray2;
        int index9 = num10;
        num1 = index9 + 1;
        int num12 = numArray1[index2];
        numArray8[index9] = num12;
      }

      for (int index2 = 2 * vertexCountHorizontal;
        index2 < 2 * vertexCountHorizontal + vertexCountVertical - 1;
        ++index2)
      {
        int[] numArray3 = numArray2;
        int index4 = num1;
        int num2 = index4 + 1;
        int num3 = numArray1[index2];
        numArray3[index4] = num3;
        int[] numArray4 = numArray2;
        int index5 = num2;
        int num4 = index5 + 1;
        int num5 = _sourceMesh.vertexCount + index2;
        numArray4[index5] = num5;
        int[] numArray5 = numArray2;
        int index6 = num4;
        int num6 = index6 + 1;
        int num7 = _sourceMesh.vertexCount + 1 + index2;
        numArray5[index6] = num7;
        int[] numArray6 = numArray2;
        int index7 = num6;
        int num8 = index7 + 1;
        int num9 = _sourceMesh.vertexCount + 1 + index2;
        numArray6[index7] = num9;
        int[] numArray7 = numArray2;
        int index8 = num8;
        int num10 = index8 + 1;
        int num11 = numArray1[index2 + 1];
        numArray7[index8] = num11;
        int[] numArray8 = numArray2;
        int index9 = num10;
        num1 = index9 + 1;
        int num12 = numArray1[index2];
        numArray8[index9] = num12;
      }

      for (int index2 = 2 * vertexCountHorizontal + vertexCountVertical;
        index2 < 2 * vertexCountHorizontal + 2 * vertexCountVertical - 1;
        ++index2)
      {
        int[] numArray3 = numArray2;
        int index4 = num1;
        int num2 = index4 + 1;
        int num3 = _sourceMesh.vertexCount + index2;
        numArray3[index4] = num3;
        int[] numArray4 = numArray2;
        int index5 = num2;
        int num4 = index5 + 1;
        int num5 = numArray1[index2];
        numArray4[index5] = num5;
        int[] numArray5 = numArray2;
        int index6 = num4;
        int num6 = index6 + 1;
        int num7 = _sourceMesh.vertexCount + 1 + index2;
        numArray5[index6] = num7;
        int[] numArray6 = numArray2;
        int index7 = num6;
        int num8 = index7 + 1;
        int num9 = numArray1[index2 + 1];
        numArray6[index7] = num9;
        int[] numArray7 = numArray2;
        int index8 = num8;
        int num10 = index8 + 1;
        int num11 = _sourceMesh.vertexCount + 1 + index2;
        numArray7[index8] = num11;
        int[] numArray8 = numArray2;
        int index9 = num10;
        num1 = index9 + 1;
        int num12 = numArray1[index2];
        numArray8[index9] = num12;
      }

      Vector3[] vector3Array3 = new Vector3[_sourceMesh.vertexCount + length];
      int[] numArray9 = new int[_sourceMesh.triangles.Length + (length - 1) * 6];
      Vector2[] vector2Array3 = new Vector2[_sourceMesh.vertexCount + length];
      Vector2[] vector2Array4 = new Vector2[_sourceMesh.vertexCount + length];
      Vector3[] vector3Array4 = new Vector3[_sourceMesh.vertexCount + length];
      Vector4[] vector4Array2 = new Vector4[_sourceMesh.vertexCount + length];
      vertices.CopyTo((Array) vector3Array3, 0);
      vector3Array1.CopyTo((Array) vector3Array3, _sourceMesh.vertexCount);
      triangles.CopyTo((Array) numArray9, 0);
      numArray2.CopyTo((Array) numArray9, _sourceMesh.triangles.Length);
      uv.CopyTo((Array) vector2Array3, 0);
      vector2Array1.CopyTo((Array) vector2Array3, _sourceMesh.vertexCount);
      uv2.CopyTo((Array) vector2Array4, 0);
      vector2Array2.CopyTo((Array) vector2Array4, _sourceMesh.vertexCount);
      normals.CopyTo((Array) vector3Array4, 0);
      vector3Array2.CopyTo((Array) vector3Array4, _sourceMesh.vertexCount);
      tangents.CopyTo((Array) vector4Array2, 0);
      vector4Array1.CopyTo((Array) vector4Array2, _sourceMesh.vertexCount);
      _sourceMesh.vertices = vector3Array3;
      _sourceMesh.triangles = numArray9;
      _sourceMesh.uv = vector2Array3;
      _sourceMesh.uv2 = vector2Array4;
      _sourceMesh.normals = vector3Array4;
      _sourceMesh.tangents = vector4Array2;
    }

    private static void GenerateNormals(Vector3[] _vertices, int[] _trinagles, out Vector3[] _normals)
    {
      _normals = new Vector3[_vertices.Length];
      List<List<Vector3>> list = new List<List<Vector3>>();
      for (int i = 0; i < _vertices.Length; ++i)
        list.Add(new List<Vector3>());
      for (int j = 0; j < _trinagles.Length; j += 3)
      {
        Vector3 vertex1 = _vertices[_trinagles[j]];
        Vector3 vertex2 = _vertices[_trinagles[j + 1]];
        Vector3 vertex3 = _vertices[_trinagles[j + 2]];
        Vector3 lhs = vertex2 - vertex1;
        Vector3 rhs = vertex3 - vertex1;
        Vector3 vector3_2 = Vector3.Cross(lhs, rhs);
        vector3_2.Normalize();
        list[_trinagles[j]].Add(vector3_2);
        list[_trinagles[j + 1]].Add(vector3_2);
        list[_trinagles[j + 2]].Add(vector3_2);
      }

      for (int k = 0; k < _vertices.Length; ++k)
      {
        Vector3 vector = Vector3.zero;
        for (int l = 0; l < list[k].Count; ++l)
          vector += list[k][l];
        _normals[k] = vector / (float) list[k].Count;
      }
    }

    private static void GenerateTangents(Vector3[] _vertices, int[] _triangles, Vector3[] _normals, Vector2[] _texcoords, out Vector4[] _tangents)
    {
        int num = _vertices.Length;
        int num2 = _triangles.Length / 3;
        Vector3[] array = new Vector3[num];
        Vector3[] array2 = new Vector3[num];
        int num3 = 0;
        for (int i = 0; i < num2; i++)
        {
            int num4 = _triangles[num3];
            int num5 = _triangles[num3 + 1];
            int num6 = _triangles[num3 + 2];
            Vector3 vector = _vertices[num4];
            Vector3 vector2 = _vertices[num5];

            Vector3 vector3 = _vertices[num6];
            Vector2 vector4 = _texcoords[num4];
            Vector2 vector5 = _texcoords[num5];
            Vector2 vector6 = _texcoords[num6];
            float num7 = vector2.x - vector.x;
            float num8 = vector3.x - vector.x;
            float num9 = vector2.y - vector.y;
            float num10 = vector3.y - vector.y;
            float num11 = vector2.z - vector.z;
            float num12 = vector3.z - vector.z;
            float num13 = vector5.x - vector4.x;
            float num14 = vector6.x - vector4.x;
            float num15 = vector5.y - vector4.y;
            float num16 = vector6.y - vector4.y;
            float num17 = 0.0001f;
            if (num13 * num16 - num14 * num15 != 0f)
            {
                num17 = 1f / (num13 * num16 - num14 * num15);
            }
            Vector3 vector7 = new Vector3((num16 * num7 - num15 * num8) * num17, (num16 * num9 - num15 * num10) * num17, (num16 * num11 - num15 * num12) * num17);
            Vector3 vector8 = new Vector3((num13 * num8 - num14 * num7) * num17, (num13 * num10 - num14 * num9) * num17, (num13 * num12 - num14 * num11) * num17);
            array[num4] += vector7;
            array[num5] += vector7;
            array[num6] += vector7;
            array2[num4] += vector8;
            array2[num5] += vector8;
            array2[num6] += vector8;
            num3 += 3;
        }
        _tangents = new Vector4[num];
        for (int j = 0; j < num; j++)
        {
            Vector3 vector9 = _normals[j];
            Vector3 vector10 = array[j];
            Vector3.OrthoNormalize(ref vector9, ref vector10);
            _tangents[j].x = vector10.x;
            _tangents[j].y = vector10.y;
            _tangents[j].z = vector10.z;
            _tangents[j].w = ((Vector3.Dot(Vector3.Cross(vector9, vector10), array2[j]) < 0f) ? -1f : 1f);
        }
    }

    private static float Remap(float value, float from1, float to1, float from2, float to2) =>
      (float) (((double) value - (double) from1) / ((double) to1 - (double) from1) * ((double) to2 - (double) from2)) +
      from2;

    private static Vector2[] NormalizeUV(Vector2[] _oldUVs)
    {
      Vector2[] vector2Array = new Vector2[_oldUVs.Length];
      int index1 = 0;
      for (int index2 = 0; index2 < SimplygonTerrainConvert.terrainConvertInfo.vertexCountVertical; ++index2)
      {
        for (int index3 = 0; index3 < SimplygonTerrainConvert.terrainConvertInfo.vertexCountHorizontal; ++index3)
        {
          vector2Array[index1] =
            new Vector2((float) index3 / (float) (SimplygonTerrainConvert.terrainConvertInfo.vertexCountHorizontal - 1),
              (float) index2 / (float) (SimplygonTerrainConvert.terrainConvertInfo.vertexCountVertical - 1));
          ++index1;
        }
      }

      return vector2Array;
    }

    private static void ResizePro(
      Texture texture,
      int width,
      int height,
      out Texture2D dstTexture,
      bool sRGB,
      bool hasMipMap = true)
    {
      if ((UnityEngine.Object) texture == (UnityEngine.Object) null)
      {
        dstTexture = (Texture2D) null;
      }
      else
      {
        if (width < 4)
          width = 4;
        if (height < 4)
          height = 4;
        RenderTexture temporary = RenderTexture.GetTemporary(width, height);
        temporary.Create();
        RenderTexture active = RenderTexture.active;
        RenderTexture.active = temporary;
        temporary.DiscardContents();
        SimplygonTerrainConvert.Blit(texture, temporary, sRGB);
        dstTexture = new Texture2D(width, height, TextureFormat.ARGB32, hasMipMap);
        dstTexture.name = texture.name;
        dstTexture.filterMode = FilterMode.Bilinear;
        dstTexture.ReadPixels(new Rect(0.0f, 0.0f, (float) width, (float) height), 0, 0);
        dstTexture.Apply();
        RenderTexture.active = Application.isPlaying ? active : (RenderTexture) null;
        RenderTexture.ReleaseTemporary(temporary);
      }
    }

    public static void Blit(Texture source, RenderTexture dest, bool sRGB) =>
      SimplygonTerrainConvert.Blit(source, dest, (Material) null, sRGB);

    public static void Blit(
      Texture source,
      RenderTexture dest,
      Material mat,
      bool sRGB,
      int pass = -1)
    {
      GL.sRGBWrite = sRGB;
      if ((UnityEngine.Object) mat == (UnityEngine.Object) null)
        Graphics.Blit(source, dest);
      else
        Graphics.Blit(source, dest, mat, pass);
    }

    public static RenderTexture CreateRenderTexture(int width, int height, bool sRGB) => new RenderTexture(width,
      height, 24, RenderTextureFormat.ARGB32, sRGB ? RenderTextureReadWrite.sRGB : RenderTextureReadWrite.Default);

    public static RenderTexture CreateTemporaryRenderTexture(
      int width,
      int height,
      bool sRGB)
    {
      return RenderTexture.GetTemporary(width, height, 24, RenderTextureFormat.ARGB32,
        sRGB ? RenderTextureReadWrite.sRGB : RenderTextureReadWrite.Default);
    }

    private static void RenderTextureToTexture2D(
      RenderTexture renderTexture,
      TextureFormat format,
      ref Texture2D texture2D)
    {
      RenderTexture active = RenderTexture.active;
      RenderTexture.active = renderTexture;
      if ((UnityEngine.Object) texture2D == (UnityEngine.Object) null)
        texture2D = new Texture2D(renderTexture.width, renderTexture.height, format, true);
      else if (texture2D.width != renderTexture.width || texture2D.height != renderTexture.height)
        texture2D.Reinitialize(renderTexture.width, renderTexture.height, format, true);
      texture2D.ReadPixels(new Rect(0.0f, 0.0f, (float) renderTexture.width, (float) renderTexture.height), 0, 0);
      texture2D.Apply();
      RenderTexture.active = Application.isPlaying ? active : (RenderTexture) null;
    }

    private static Texture2D Get2x2Texture2D(
      Color fillColor,
      TextureFormat format,
      bool linear)
    {
      Texture2D texture2D = new Texture2D(2, 2, format, linear);
      texture2D.SetPixels(new Color[4]
      {
        fillColor,
        fillColor,
        fillColor,
        fillColor
      });
      texture2D.Apply();
      return texture2D;
    }

    private static Texture2D GetBlackTexture(int width, int height)
    {
      if (width < 2)
        width = 2;
      if (height < 2)
        height = 2;
      Texture2D texture2D1 = SimplygonTerrainConvert.Get2x2Texture2D(Color.clear, TextureFormat.RGBA32, false);
      RenderTexture temporary = RenderTexture.GetTemporary(width, height, 0);
      temporary.Create();
      Graphics.Blit((Texture) texture2D1, temporary);
      Texture2D texture2D2 = (Texture2D) null;
      SimplygonTerrainConvert.RenderTextureToTexture2D(temporary, TextureFormat.RGBA32, ref texture2D2);
      RenderTexture.ReleaseTemporary(temporary);
      if (Application.isEditor)
        UnityEngine.Object.DestroyImmediate((UnityEngine.Object) texture2D1);
      else
        UnityEngine.Object.Destroy((UnityEngine.Object) texture2D1);
      return texture2D2;
    }

    public delegate void ProgressFunction(string _name, float _value);

  }

  [Serializable]
  public class TerrainConvertInfo
  {
    public const int maxVertexCount = 65000;
    public int chunkCountHorizontal;
    public int chunkCountVertical;
    public int vertexCountHorizontal;
    public int vertexCountVertical;
    public bool generateSkirt;
    public float skirtGroundLevel;

    public TerrainConvertInfo() => this.Reset();

    public TerrainConvertInfo(TerrainConvertInfo _right)
    {
      this.chunkCountHorizontal = _right.chunkCountHorizontal;
      this.chunkCountVertical = _right.chunkCountVertical;
      this.vertexCountHorizontal = _right.vertexCountHorizontal;
      this.vertexCountVertical = _right.vertexCountVertical;
      this.generateSkirt = _right.generateSkirt;
      this.skirtGroundLevel = _right.skirtGroundLevel;
    }

    public void Reset()
    {
      this.chunkCountHorizontal = 1;
      this.chunkCountVertical = 1;
      this.vertexCountHorizontal = 25;
      this.vertexCountVertical = 25;
      this.generateSkirt = false;
      this.skirtGroundLevel = 0.0f;
    }

    public int GetChunkCount()
    {
      if (this.chunkCountHorizontal < 1)
        this.chunkCountHorizontal = 1;
      if (this.chunkCountVertical < 1)
        this.chunkCountVertical = 1;
      return this.chunkCountHorizontal * this.chunkCountVertical;
    }

    public int GetVertexCountPerChunk()
    {
      int num = 0;
      if (this.generateSkirt)
        num = 2 * (this.vertexCountHorizontal + this.vertexCountVertical);
      return this.vertexCountHorizontal * this.vertexCountVertical + num;
    }

    public int GetVertexCountTotal() => this.GetChunkCount() * this.GetVertexCountPerChunk();

    public int GetTriangleCountPerChunk()
    {
      int num = 0;
      if (this.generateSkirt)
        num = 2 * (this.vertexCountHorizontal + this.vertexCountVertical) * 2;
      return (this.vertexCountHorizontal - 1) * (this.vertexCountVertical - 1) * 2 + num;
    }

    public int GetTriangleCountTotal() => this.GetChunkCount() * this.GetTriangleCountPerChunk();
  }

  internal class MeshConvertInfo
  {
    public Mesh mesh;

    public int chunkCountHorizontal;
    public int chunkCountVertical;

    public int chunkIndexHorizontal;
    public int chunkIndexVertical;

    public float chunkSizeHorizontal;
    public float chunkSizeVertical;

    public MeshConvertInfo()
    {

    }

    public float GetChunkOffsetHorizontal()
    {
      return this.chunkIndexHorizontal * this.chunkSizeHorizontal;
    }

    public float GetChunkOffsetVertical()
    {
      return this.chunkIndexVertical * this.chunkSizeVertical;
    }
  }

  internal class PreMesh
  {
    public string name = string.Empty;
    public Vector3[] vertices;
    public int[] triangles;
    public Vector2[] uv;
    public Vector2[] uv2;
    public Vector3[] normals;
    public Vector4[] tangents;

    public void Clear()
    {
      this.vertices = (Vector3[]) null;
      this.triangles = (int[]) null;
      this.uv = (Vector2[]) null;
      this.uv2 = (Vector2[]) null;
      this.normals = (Vector3[]) null;
      this.tangents = (Vector4[]) null;
    }
  }
}