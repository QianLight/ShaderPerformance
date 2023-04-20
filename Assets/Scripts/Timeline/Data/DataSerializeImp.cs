//This file is auto generate by Assets/Tool/Director_CreateDataSerialize

using System;
using System.IO;
using CFEngine;
using CFUtilPoolLib;
using UnityEngine;


public partial class DataSerialize
{
#if UNITY_EDITOR


    // public static void Save_DirectorData (BinaryWriter bw, TimelineData td, ref DirectorData data)
    // {
    //     bw.Write(data.flag);
	// 	bw.Write(data.hideRenderLayer);
	// 	bw.Write(data.animCount);
    // }

    // public static void Save_TrackTargetData (BinaryWriter bw, TimelineData td, ref TrackTargetData data)
    // {
    //     bw.Write(data.targetType);
	// 	bw.Write(data.id);
	// 	EditorCommon.WriteVector(bw, data.pos);
	// 	EditorCommon.WriteVector(bw, data.rot);
	// 	EditorCommon.WriteVector(bw, data.scale);
	// 	bw.Write(data.flag);
    // }

    // public static void Save_AnimClipData (BinaryWriter bw, TimelineData td, ref AnimClipData data)
    // {
    //     short pathIndex = (short)Array.IndexOf(td.strPool, data.path);
	// 	bw.Write(pathIndex);
    // }

    // public static void Save_FxClipData (BinaryWriter bw, TimelineData td, ref FxClipData data)
    // {
    //     short pathIndex = (short)Array.IndexOf(td.strPool, data.path);
	// 	bw.Write(pathIndex);
	// 	short boneNameIndex = (short)Array.IndexOf(td.strPool, data.boneName);
	// 	bw.Write(boneNameIndex);
	// 	EditorCommon.WriteVector(bw, data.pos);
	// 	EditorCommon.WriteVector(bw, data.rot);
	// 	EditorCommon.WriteVector(bw, data.scale);
    // }

    // public static void Save_IndexTrackData (BinaryWriter bw, TimelineData td, ref IndexTrackData data)
    // {
    //     bw.Write(data.dataType);
	// 	EditorCommon.WriteVector(bw, data.clipIndex);
    // }


#endif


    // public static void Load_DirectorData (CFBinaryReader reader, TimelineData td, ref DirectorData data)
    // {
    //     data.flag = reader.ReadUInt32();
	// 	data.hideRenderLayer = reader.ReadInt32();
	// 	data.animCount = reader.ReadInt32();
    // }

    // public static void Load_TrackTargetData (CFBinaryReader reader, TimelineData td, ref TrackTargetData data)
    // {
    //     data.targetType = reader.ReadByte();
	// 	data.id = reader.ReadUInt32();
	// 	reader.ReadVector(ref data.pos);
	// 	reader.ReadVector(ref data.rot);
	// 	reader.ReadVector(ref data.scale);
	// 	data.flag = reader.ReadUInt32();
    // }

    // public static void Load_AnimClipData (CFBinaryReader reader, TimelineData td, ref AnimClipData data)
    // {
    //     data.path = GetStr(td.strPool, reader.ReadInt16());
    // }

    // public static void Load_FxClipData (CFBinaryReader reader, TimelineData td, ref FxClipData data)
    // {
    //     data.path = GetStr(td.strPool, reader.ReadInt16());
	// 	data.boneName = GetStr(td.strPool, reader.ReadInt16());
	// 	reader.ReadVector(ref data.pos);
	// 	reader.ReadVector(ref data.rot);
	// 	reader.ReadVector(ref data.scale);
    // }

    // public static void Load_IndexTrackData (CFBinaryReader reader, TimelineData td, ref IndexTrackData data)
    // {
    //     data.dataType = reader.ReadByte();
	// 	reader.ReadVector(ref data.clipIndex);
    // }

}