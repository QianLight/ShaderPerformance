using System;
using System.Collections.Generic;
using Unity.Profiling;
using UnityEngine;

namespace RuntimeProfile
{
    [Serializable]
    public class UserData
    {
        [SerializeField] public String UserId;
        [SerializeField] public string MobilePhone;
        [SerializeField] public int AllMemory;
    }

    [Serializable]
    public class SceneData
    {
        [SerializeField] public uint SceneId;
        [SerializeField] public string SceneName;
        [SerializeField] public string SceneEntryTime;
    }

    [Serializable]
    public class SceneRecordDataRequest
    {
        [SerializeField] public UserData UserData;
        [SerializeField] public SceneData SceneData;
    }

    [Serializable]
    public struct ProfilerData
    {
        public uint TickId;
        public uint SceneDataId;

        public String Time;

        // render
        public long TriangleCount;
        public long VertexCount;
        public long DrawCallCount;
        public long SetPassCount;
        public long TotalBatchCount;

        public double FrameTime;

        // memory
        public long SystemUsedMemory;
        public long TotalUseMemory;
        public long GCUsedMemory;
        public long TotalReservedMemory;
        public long GCReservedMemory;
    }

    public class StatInfo
    {
        public ProfilerCategory Cat;
        public string Name;
        public ProfilerMarkerDataUnit Unit;
        public ProfilerRecorder Recorder;
    }

    [Serializable]
    public class SlowestProfile
    {
        [NonSerialized] public RenderTexture SlowestTex;
        [SerializeField] public ProfilerData data;

        public void Clear()
        {
            data = default;
        }

        public SlowestProfile(Vector2Int picsize)
        {
            SlowestTex = new RenderTexture(picsize.x, picsize.y, 0);
        }

        public void Release()
        {
            SlowestTex.Release();
            SlowestTex = null;
        }
    }

    [Serializable]
    public class ProfilerDataEndRequest
    {
        [SerializeField] public List<SlowestProfile> SlowestProfiles;
        [SerializeField] public List<SlowestProfile> MaxMemoryProfiles;
        [SerializeField] public int MaxRecordNum;
        [SerializeField] public uint SceneDataId;


        public void Clear()
        {
            foreach (var data in SlowestProfiles)
            {
                data.Clear();
            }

            foreach (var data in MaxMemoryProfiles)
            {
                data.Clear();
            }
        }

        public ProfilerDataEndRequest(int maxRecordNum, Vector2Int picsize)
        {
            MaxRecordNum = maxRecordNum;
            SlowestProfiles = new List<SlowestProfile>(maxRecordNum);
            MaxMemoryProfiles = new List<SlowestProfile>(maxRecordNum);
            for (int i = 0; i < maxRecordNum; i++)
            {
                SlowestProfiles.Add(new SlowestProfile(picsize));
                MaxMemoryProfiles.Add(new SlowestProfile(picsize));
            }
        }

        public void Release()
        {
            for (int i = 0; i < SlowestProfiles.Count; i++)
            {
                SlowestProfiles[i]?.Release();
            }
            for (int i = 0; i < MaxMemoryProfiles.Count; i++)
            {
                MaxMemoryProfiles[i]?.Release();
            }
            SlowestProfiles.Clear();
            MaxMemoryProfiles.Clear();
        }
    }

    public static class ProfilerProperties
    {
        public static readonly String SystemUsedMemory = "System Used Memory";
        public static readonly String TotalReservedMemory = "Total Reserved Memory";
        public static readonly String TotalUsedMemory = "Total Used Memory";
        public static readonly String GCReservedMemory = "GC Reserved Memory";
        public static readonly String GCUsedMemory = "GC Used Memory";

        public static readonly String DrawCallsCount = "Draw Calls Count";
        public static readonly String SetPassCount = "SetPass Calls Count";
        public static readonly String TotalBatchCount = "Batches Count";
        public static readonly String TriangleCount = "Triangles Count";
        public static readonly String VertexCount = "Vertices Count";
        public static readonly String MainThread = "Main Thread";
    }

    [Serializable]
    public class NeedSendToServerData<T>
    {
        [NonSerialized] private int _listNum;
        [NonSerialized] private int _nowProfilerDataNum;
        [SerializeField] private T[] ProfilerDatas;

        public NeedSendToServerData(int listNum)
        {
            _listNum = listNum;
            ProfilerDatas = new T[_listNum];
            _nowProfilerDataNum = 0;
        }

        public T[] GetDatas()
        {
            return ProfilerDatas;
        }
        
        public bool IsFull()
        {
            return _listNum == _nowProfilerDataNum;
        }

        public void Clear()
        {
            for (int i = 0; i < ProfilerDatas.Length; i++)
            {
                ProfilerDatas[i] = default;
            }

            _nowProfilerDataNum = 0;
        }

        public void AddToList(T data)
        {
            if (IsFull()) return;
            ProfilerDatas[_nowProfilerDataNum] = data;
            _nowProfilerDataNum++;
        }
    }

    [Serializable]
    public class StartProfilerResp
    {
        public int Code;
        public string Msg;
        public uint SceneDataId;
    }

}