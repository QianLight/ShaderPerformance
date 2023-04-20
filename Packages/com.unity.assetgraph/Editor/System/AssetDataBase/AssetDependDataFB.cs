// <auto-generated>
//  automatically generated by the FlatBuffers compiler, do not modify
// </auto-generated>

using global::System;
using global::System.Collections.Generic;
using global::ZeusFlatBuffers;

public struct AssetDependDataFB : IFlatbufferObject
{
  private Table __p;
  public ByteBuffer ByteBuffer { get { return __p.bb; } }
  public static void ValidateVersion() { FlatBufferConstants.FLATBUFFERS_1_12_0(); }
  public static AssetDependDataFB GetRootAsAssetDependDataFB(ByteBuffer _bb) { return GetRootAsAssetDependDataFB(_bb, new AssetDependDataFB()); }
  public static AssetDependDataFB GetRootAsAssetDependDataFB(ByteBuffer _bb, AssetDependDataFB obj) { return (obj.__assign(_bb.GetInt(_bb.Position) + _bb.Position, _bb)); }
  public void __init(int _i, ByteBuffer _bb) { __p = new Table(_i, _bb); }
  public AssetDependDataFB __assign(int _i, ByteBuffer _bb) { __init(_i, _bb); return this; }

  public DependDataFB? Data(int j) { int o = __p.__offset(4); return o != 0 ? (DependDataFB?)(new DependDataFB()).__assign(__p.__indirect(__p.__vector(o) + j * 4), __p.bb) : null; }
  public int DataLength { get { int o = __p.__offset(4); return o != 0 ? __p.__vector_len(o) : 0; } }
  public string AssetPath(int j) { int o = __p.__offset(6); return o != 0 ? __p.__string(__p.__vector(o) + j * 4) : null; }
  public int AssetPathLength { get { int o = __p.__offset(6); return o != 0 ? __p.__vector_len(o) : 0; } }

  public static Offset<AssetDependDataFB> CreateAssetDependDataFB(FlatBufferBuilder builder,
      VectorOffset dataOffset = default(VectorOffset),
      VectorOffset assetPathOffset = default(VectorOffset)) {
    builder.StartTable(2);
    AssetDependDataFB.AddAssetPath(builder, assetPathOffset);
    AssetDependDataFB.AddData(builder, dataOffset);
    return AssetDependDataFB.EndAssetDependDataFB(builder);
  }

  public static void StartAssetDependDataFB(FlatBufferBuilder builder) { builder.StartTable(2); }
  public static void AddData(FlatBufferBuilder builder, VectorOffset dataOffset) { builder.AddOffset(0, dataOffset.Value, 0); }
  public static VectorOffset CreateDataVector(FlatBufferBuilder builder, Offset<DependDataFB>[] data) { builder.StartVector(4, data.Length, 4); for (int i = data.Length - 1; i >= 0; i--) builder.AddOffset(data[i].Value); return builder.EndVector(); }
  public static VectorOffset CreateDataVectorBlock(FlatBufferBuilder builder, Offset<DependDataFB>[] data) { builder.StartVector(4, data.Length, 4); builder.Add(data); return builder.EndVector(); }
  public static void StartDataVector(FlatBufferBuilder builder, int numElems) { builder.StartVector(4, numElems, 4); }
  public static void AddAssetPath(FlatBufferBuilder builder, VectorOffset assetPathOffset) { builder.AddOffset(1, assetPathOffset.Value, 0); }
  public static VectorOffset CreateAssetPathVector(FlatBufferBuilder builder, StringOffset[] data) { builder.StartVector(4, data.Length, 4); for (int i = data.Length - 1; i >= 0; i--) builder.AddOffset(data[i].Value); return builder.EndVector(); }
  public static VectorOffset CreateAssetPathVectorBlock(FlatBufferBuilder builder, StringOffset[] data) { builder.StartVector(4, data.Length, 4); builder.Add(data); return builder.EndVector(); }
  public static void StartAssetPathVector(FlatBufferBuilder builder, int numElems) { builder.StartVector(4, numElems, 4); }
  public static Offset<AssetDependDataFB> EndAssetDependDataFB(FlatBufferBuilder builder) {
    int o = builder.EndTable();
    return new Offset<AssetDependDataFB>(o);
  }
  public static void FinishAssetDependDataFBBuffer(FlatBufferBuilder builder, Offset<AssetDependDataFB> offset) { builder.Finish(offset.Value); }
  public static void FinishSizePrefixedAssetDependDataFBBuffer(FlatBufferBuilder builder, Offset<AssetDependDataFB> offset) { builder.FinishSizePrefixed(offset.Value); }
};

public struct DependDataFB : IFlatbufferObject
{
  private Table __p;
  public ByteBuffer ByteBuffer { get { return __p.bb; } }
  public static void ValidateVersion() { FlatBufferConstants.FLATBUFFERS_1_12_0(); }
  public static DependDataFB GetRootAsDependDataFB(ByteBuffer _bb) { return GetRootAsDependDataFB(_bb, new DependDataFB()); }
  public static DependDataFB GetRootAsDependDataFB(ByteBuffer _bb, DependDataFB obj) { return (obj.__assign(_bb.GetInt(_bb.Position) + _bb.Position, _bb)); }
  public void __init(int _i, ByteBuffer _bb) { __p = new Table(_i, _bb); }
  public DependDataFB __assign(int _i, ByteBuffer _bb) { __init(_i, _bb); return this; }

  public int Index { get { int o = __p.__offset(4); return o != 0 ? __p.bb.GetInt(o + __p.bb_pos) : (int)0; } }
  public string Hash { get { int o = __p.__offset(6); return o != 0 ? __p.__string(o + __p.bb_pos) : null; } }
#if ENABLE_SPAN_T
  public Span<byte> GetHashBytes() { return __p.__vector_as_span<byte>(6, 1); }
#else
  public ArraySegment<byte>? GetHashBytes() { return __p.__vector_as_arraysegment(6); }
#endif
  public byte[] GetHashArray() { return __p.__vector_as_array<byte>(6); }
  public int DepIndices(int j) { int o = __p.__offset(8); return o != 0 ? __p.bb.GetInt(__p.__vector(o) + j * 4) : (int)0; }
  public int DepIndicesLength { get { int o = __p.__offset(8); return o != 0 ? __p.__vector_len(o) : 0; } }
#if ENABLE_SPAN_T
  public Span<int> GetDepIndicesBytes() { return __p.__vector_as_span<int>(8, 4); }
#else
  public ArraySegment<byte>? GetDepIndicesBytes() { return __p.__vector_as_arraysegment(8); }
#endif
  public int[] GetDepIndicesArray() { return __p.__vector_as_array<int>(8); }

  public static Offset<DependDataFB> CreateDependDataFB(FlatBufferBuilder builder,
      int index = 0,
      StringOffset hashOffset = default(StringOffset),
      VectorOffset depIndicesOffset = default(VectorOffset)) {
    builder.StartTable(3);
    DependDataFB.AddDepIndices(builder, depIndicesOffset);
    DependDataFB.AddHash(builder, hashOffset);
    DependDataFB.AddIndex(builder, index);
    return DependDataFB.EndDependDataFB(builder);
  }

  public static void StartDependDataFB(FlatBufferBuilder builder) { builder.StartTable(3); }
  public static void AddIndex(FlatBufferBuilder builder, int index) { builder.AddInt(0, index, 0); }
  public static void AddHash(FlatBufferBuilder builder, StringOffset hashOffset) { builder.AddOffset(1, hashOffset.Value, 0); }
  public static void AddDepIndices(FlatBufferBuilder builder, VectorOffset depIndicesOffset) { builder.AddOffset(2, depIndicesOffset.Value, 0); }
  public static VectorOffset CreateDepIndicesVector(FlatBufferBuilder builder, int[] data) { builder.StartVector(4, data.Length, 4); for (int i = data.Length - 1; i >= 0; i--) builder.AddInt(data[i]); return builder.EndVector(); }
  public static VectorOffset CreateDepIndicesVectorBlock(FlatBufferBuilder builder, int[] data) { builder.StartVector(4, data.Length, 4); builder.Add(data); return builder.EndVector(); }
  public static void StartDepIndicesVector(FlatBufferBuilder builder, int numElems) { builder.StartVector(4, numElems, 4); }
  public static Offset<DependDataFB> EndDependDataFB(FlatBufferBuilder builder) {
    int o = builder.EndTable();
    return new Offset<DependDataFB>(o);
  }
};

