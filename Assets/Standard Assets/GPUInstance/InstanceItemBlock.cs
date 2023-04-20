using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//[System.Serializable]
public class InstanceItemBlock
{
	const string INSTANCE_HIGHT_KEY = "_INSTANCING_HIGH";
	public InstanceItemBlock(Camera cam, Mesh mesh, Material mat, float far, InstanceType type)
	{
		Cam = cam;
		InstanceMesh = mesh;
		Mat = mat;
		Type = type;
		cullingGroup = new CullingGroup();
		Mat.enableInstancing = true;
		//Mat.renderQueue = 2000;
		if(Type == InstanceType.High)
        {
			Mat.EnableKeyword(INSTANCE_HIGHT_KEY);
		}
		cullingDistancs = far;

		if(Cam != null)
        {
			cullingGroup.targetCamera = Cam;
			cullingGroup.SetDistanceReferencePoint(Cam.transform);
		}
	}
	public void AddItem(Transform item)
	{
		if (matrix4x4sList == null)
		{
			matrix4x4sList = new List<InstanceItem>();
		}
		Bounds bounds = InstanceMesh.bounds;
		float r = StaticInstanceMgr.GetBoundsR(bounds, item.transform.localScale);
		matrix4x4sList.Add(new InstanceItem(r, item.position, bounds, item.localToWorldMatrix));
		Length = matrix4x4sList.Count;

	}
	private float cullingDistancs = 20;
	private CullingGroup cullingGroup;
	private List<InstanceItem> matrix4x4sList = null;
	private Camera Cam;

	private Matrix4x4[] matrix4x4sStatic;
	private int[] cullingIds;

	private uint[] argsArray = new uint[5] { 0, 0, 0, 0, 0 };

	public Mesh InstanceMesh;
	public Material Mat;
	public int Length = 0;
	public int RenderLength = 0;
	public InstanceType Type = InstanceType.Mid;
	public ComputeBuffer Matrix4x4Buffer, ArgsBuffer;

	public void Culling()
	{
		if (matrix4x4sStatic == null)
		{
			//first init
			matrix4x4sStatic = new Matrix4x4[matrix4x4sList.Count];
			cullingIds = new int[matrix4x4sStatic.Length];
			BoundingSphere[] bs = new BoundingSphere[matrix4x4sStatic.Length];
			Matrix4x4s = new Matrix4x4[matrix4x4sStatic.Length];


			for (int i = 0; i < matrix4x4sList.Count; i++)
			{
				InstanceItem item = matrix4x4sList[i];
				matrix4x4sStatic[i] = item.Matrix;
				bs[i] = item.Sphere;
				Matrix4x4s[i] = item.Matrix;
			}
			Length = matrix4x4sStatic.Length;
			RenderLength = matrix4x4sStatic.Length;
			//Debug.LogError(InstanceMesh.name + "," + InstanceMesh.triangles.Length + "," + matrix4x4s.Length + "," + Mat.renderQueue + "," + Mat.shader.name);

			if (Type == InstanceType.High)
			{
				Matrix4x4Buffer = new ComputeBuffer(Length, 64);
				ArgsBuffer = new ComputeBuffer(1, argsArray.Length * sizeof(uint), ComputeBufferType.IndirectArguments);

				int subMeshIndex = 0;
				argsArray[0] = (uint)InstanceMesh.GetIndexCount(subMeshIndex);
				argsArray[1] = (uint)Length;
				argsArray[2] = (uint)InstanceMesh.GetIndexStart(subMeshIndex);
				argsArray[3] = (uint)InstanceMesh.GetBaseVertex(subMeshIndex);

				Matrix4x4Buffer.SetData(Matrix4x4s);
				ArgsBuffer.SetData(argsArray);

				Mat.SetBuffer(StaticInstanceMgr.Matrix4x4BufferID, Matrix4x4Buffer);
			}

			cullingGroup.SetBoundingSpheres(bs);
			cullingGroup.SetBoundingSphereCount(bs.Length);
			cullingGroup.SetBoundingDistances(new float[] { cullingDistancs });

			matrix4x4sList.Clear();
			matrix4x4sList = null;
		}
		else
		{
			//int index = 0;
			//for (int i = 0; i < matrix4x4s.Length; i++)
			//         {
			//	InstanceItem item = matrix4x4s[i];
			//	//if(!FastCullingXZ(Cam, item.Sphere))
			//	//            {
			//	//	Matrix4x4s[index] = item.Matrix;
			//	//	index++;
			//	//}
			//	if(cullingGroup.IsVisible(i))
			//             {
			//		Matrix4x4s[index] = item.Matrix;
			//		index++;
			//	}
			//}

			int len = cullingGroup.QueryIndices(true, cullingIds, 0);
			RenderLength = len;
			if (Type == InstanceType.High)
			{
				copyMatrix4x4s(cullingIds, len);

				Matrix4x4Buffer.SetData(Matrix4x4s);
				Mat.SetBuffer(StaticInstanceMgr.Matrix4x4BufferID, Matrix4x4Buffer);

				argsArray[1] = (uint)len;
				ArgsBuffer.SetData(argsArray);

			}
			else
			{
				copyMatrix4x4s(cullingIds, len);
			}
		}
	}

	void copyMatrix4x4s(int[] cullingIds, int len)
    {
		unsafe
		{
			//var pixelSize = System.Runtime.InteropServices.Marshal.SizeOf(typeof(Matrix4x4));
			//var arrayOfMatrices4x4 = (byte*)(UnsafeUtility.SizeOf<Matrix4x4>() * nativeArrayOfMatrices4x4.Length);
			//Unity.Collections.LowLevel.Unsafe.UnsafeUtility.MemCpy(arrayOfMatrices4x4, nativeArrayOfMatrices4x4.GetUnsafePtr<Matrix4x4>(), nativeArrayOfMatrices4x4.Length);

			fixed (Matrix4x4* ptrFrom = matrix4x4sStatic)
			{
				fixed (Matrix4x4* ptrTo = Matrix4x4s)
				{
					for (int i = 0; i < len; i++)
					{
						*(ptrTo + i) = *(ptrFrom + cullingIds[i]);
					}
				}
			}
		}
    }
	public void Clear()
	{
		matrix4x4sStatic = null;
		Matrix4x4s = null;
		matrix4x4sList = null;
		cullingGroup.Dispose();
		cullingGroup = null;

		if (Matrix4x4Buffer != null) Matrix4x4Buffer.Release();

		if (Type == InstanceType.High)
		{
			Mat.DisableKeyword(INSTANCE_HIGHT_KEY);
		}
	}
	//   bool FastCullingXZ(Camera cam, BoundingSphere sphere)
	//{
	//	Vector3 localPos = cam.transform.InverseTransformPoint(sphere.position);

	//	if (localPos.z > (far + sphere.radius) || localPos.z < (cam.nearClipPlane - sphere.radius) || Mathf.Abs(localPos.x) > ((localPos.z * Mathf.Tan(angle)) + (sphere.radius / Mathf.Sin(angle1))))
	//	{
	//		return true;
	//	}
	//	return false;
	//}
	public Matrix4x4[] Matrix4x4s;
}
//[System.Serializable]
public class InstanceItem
{
	public InstanceItem(float r, Vector3 pos, Bounds bou, Matrix4x4 matrix)
	{
		Pos = pos;
		Sphere = new BoundingSphere(pos, r);
		Matrix = matrix;
		Bounds = bou;

	}
	public BoundingSphere Sphere;
	public Bounds Bounds;
	public Matrix4x4 Matrix;
	public Vector4 Pos;
}
