using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class StaticInstanceMgr
{
	private static bool initOK = true;
	public static void Init()
	{
		if(initOK)
        {
			initOK = false;
			if(SystemInfo.supportsInstancing)
            {
				MaxType = InstanceType.Mid;
				//---mali ��֧�� SSBO
				if (SystemInfo.supportsComputeShaders && SystemInfo.maxComputeBufferInputsVertex > 0)
                {
					MaxType = InstanceType.High;
				}
			}
			//far = Mathf.Min(Cam.farClipPlane, CullDistance);
			//angle = ((cam.fieldOfView / 2.0f) * (Mathf.PI)) / 180;
			//angle1 = (((90 - cam.fieldOfView) / 2.0f) * (Mathf.PI)) / 180;
		}
	}
	public static void Clear(List<InstanceItemBlock> blockList)
	{
		if(blockList != null)
        {
			for (int i = 0; i < blockList.Count; i++)
			{
				InstanceItemBlock block = blockList[i];
				block.Clear();
				if (InstanceBlock.Contains(block))
				{
					InstanceBlock.Remove(block);
				}
			}
		}

        if(InstanceBlock.Count == 0)
        {
			backCMD();
		}
	}

	public static void SetActive(List<GameObject> objects, bool active)
	{
		if (objects == null || objects.Count == 0)
			return;

		foreach (GameObject o in objects)
		{ 
			o.SetActive(active);
		}
	}
	public static void Update(CommandBuffer cmd, int pass = 0)
	{
		if (cmd == null)
		{
			getCMD();
			drawRender(cmdBufffer, pass);
		}
		else
		{
			drawRender(cmd, pass);
		}
	}

	private static int fatchIndex = 0; 
	public static void InitRender()
	{
		fatchIndex = 0;
	}
	public static bool FatchRender(CommandBuffer cmd, int pass = 0)
	{
		int taskCount = InstanceBlock.Count;
		if (fatchIndex < taskCount)
        {
			InstanceItemBlock block = InstanceBlock[fatchIndex];
			if (block.InstanceMesh != null && block.Mat != null)
			{
				//block.Culling();
				//Debug.LogError("drawRender:" + block.RenderLength + "," + block.Type);
				//if (block.Type == InstanceType.High)
				//{
				//	cmd.DrawMeshInstancedIndirect(block.InstanceMesh, 0, block.Mat, pass, block.ArgsBuffer);
				//}
				//else
				{
					cmd.DrawMeshInstanced(block.InstanceMesh, 0, block.Mat, pass, block.Matrix4x4s, block.RenderLength);
				}
			}
		}
		fatchIndex++;
		return fatchIndex < taskCount;
	}

	public static bool NeedWork()
	{
		return InstanceBlock.Count > 0;

	}
	public static InstanceType MaxType = InstanceType.None;
	public static float CullDistance = 30;
	public static int Matrix4x4BufferID = Shader.PropertyToID("matrix4x4Buffer");

	static CommandBuffer cmdBufffer;
	static float far;

	static CommandBuffer getCMD()
	{
		if (cmdBufffer == null)
		{
			cmdBufffer = new CommandBuffer();
			cmdBufffer.name = "DrawMeshInstanced";
			Camera cam = Camera.main;
			if (cam != null)
				cam.AddCommandBuffer(CameraEvent.AfterForwardOpaque, cmdBufffer);
		}
		cmdBufffer.Clear();
		return cmdBufffer;
	}
	static void backCMD()
	{
		if (cmdBufffer != null)
		{
			Camera cam = Camera.main;
			if (cam != null)
				cam.RemoveCommandBuffer(CameraEvent.AfterForwardOpaque, cmdBufffer);
			cmdBufffer.Release();
			cmdBufffer = null;
		}
	}

	static void drawRender(CommandBuffer cmd, int pass = 0)
	{
		if (cmd != null)
		{
			for (int i = 0; i < InstanceBlock.Count; i++)
			{
				InstanceItemBlock block = InstanceBlock[i];
				if (block.InstanceMesh != null && block.Mat != null)
				{
                    block.Culling();
                    //Debug.Log("drawRender:" + block.Mat + "," + block.Mat.GetHashCode());
                    if (block.Type == InstanceType.High)
                    {
                        cmd.DrawMeshInstancedIndirect(block.InstanceMesh, 0, block.Mat, pass, block.ArgsBuffer);
                    }
                    else
                    {
						cmd.DrawMeshInstanced(block.InstanceMesh, 0, block.Mat, pass, block.Matrix4x4s, block.RenderLength);
					}
				}
			}
		}
	}
	static string getMeshHash(Mesh mesh)
	{
		return mesh.name + "_" + mesh.vertexCount;
	}
	static int maxDrawCount = 1023;
	static List<InstanceItemBlock> InstanceBlock = new List<InstanceItemBlock>();
	static Dictionary<string, List<InstanceItemBlock>> storeMeshList = new Dictionary<string, List<InstanceItemBlock>>();
	static List<GameObject> storeGameObjectList = new List<GameObject>();
	static void getRender(Transform trans, Camera cam, List<string> shaderList, float cullingDistance, List<GameObject> storeGameObjectList, InstanceType instanceType)
	{
		if (trans == null || !trans.gameObject.activeSelf)
			return;
		GameObject g = trans.gameObject;
		Renderer render = g.GetComponent<Renderer>();
		MeshFilter filter = g.GetComponent<MeshFilter>();
		if (render != null && filter != null)
		{
			Material mat =StaticInstance.Instance.GetRenderMaterial(render.sharedMaterial);
			Mesh mesh = filter.sharedMesh;
			if (mat != null && mesh != null && mat.shader.name != "URP/Scene/Grass" && (shaderList.Count == 0 || shaderList.Contains(mat.shader.name)))
			{
				storeGameObjectList.Add(g);

				string hash = getMeshHash(mesh);
				List<InstanceItemBlock> blockList = null;
				InstanceItemBlock block = null;
				if (storeMeshList.ContainsKey(hash))
				{
					blockList = storeMeshList[hash];
					block = blockList[blockList.Count - 1];
					if ((MaxType ==  InstanceType.Mid || instanceType == InstanceType.Mid) && block.Length >= maxDrawCount)
					{
						block = new InstanceItemBlock(cam, mesh, mat, cullingDistance, instanceType);
						blockList.Add(block);
					}
				}
				else
				{
					blockList = new List<InstanceItemBlock>();
					block = new InstanceItemBlock(cam, mesh, mat, cullingDistance, instanceType);
					blockList.Add(block);
					storeMeshList[hash] = blockList;
				}
				block.AddItem(trans);
				trans.gameObject.SetActive(false);
			}
		}
		foreach (Transform o in trans)
		{
			getRender(o, cam, shaderList, cullingDistance, storeGameObjectList, instanceType);
		}
	}
	public static List<InstanceItemBlock> GetRender(Transform trans, Camera cam, List<string> shaderList, float cullingDistance, InstanceType type, ref List<GameObject> storeGameobjects)
	{
		if (MaxType == 0 || type == 0)
			return null;
		if (type > MaxType)
			type = MaxType;
		getRender(trans, cam, shaderList, cullingDistance, storeGameobjects, type);
		List<InstanceItemBlock> list = new List<InstanceItemBlock>();
		foreach (KeyValuePair<string, List<InstanceItemBlock>> kv in storeMeshList)
		{
			foreach (InstanceItemBlock k in kv.Value)
			{
				k.Culling();
				list.Add(k);
			}
		}
		InstanceBlock.AddRange(list);
		storeMeshList.Clear();
		return list;
	}

	public static float GetBoundsR(Bounds bounds, Vector3 scale)
	{
		Vector3 center = bounds.center;
		Vector3 ext = bounds.extents;

		float deltaX = Mathf.Abs(ext.x * scale.x);
		float deltaY = Mathf.Abs(ext.y * scale.y);
		float deltaZ = Mathf.Abs(ext.z * scale.z);
		return Vector3.Distance(Vector3.zero, center + new Vector3(deltaX, deltaY, deltaZ));
		//return Mathf.Max(Vector3.Distance(Vector3.zero, center + new Vector3(deltaX, deltaY, deltaZ)), Vector3.Distance(Vector3.zero, center + new Vector3(-deltaX, -deltaY, -deltaZ)));

		//Vector3[] tmpBoundsPoints = new Vector3[8];
		//Vector3 center = bounds.center;
		//Vector3 ext = bounds.extents;

		//float deltaX = Mathf.Abs(ext.x * scale.x);
		//float deltaY = Mathf.Abs(ext.y * scale.y);
		//float deltaZ = Mathf.Abs(ext.z * scale.z);

		//tmpBoundsPoints[0] = center + new Vector3(-deltaX, deltaY, -deltaZ);
		//tmpBoundsPoints[1] = center + new Vector3(deltaX, deltaY, -deltaZ);
		//tmpBoundsPoints[2] = center + new Vector3(deltaX, deltaY, deltaZ);
		//tmpBoundsPoints[3] = center + new Vector3(-deltaX, deltaY, deltaZ);

		//tmpBoundsPoints[4] = center + new Vector3(-deltaX, -deltaY, -deltaZ);
		//tmpBoundsPoints[5] = center + new Vector3(deltaX, -deltaY, -deltaZ);
		//tmpBoundsPoints[6] = center + new Vector3(deltaX, -deltaY, deltaZ);
		//tmpBoundsPoints[7] = center + new Vector3(-deltaX, -deltaY, deltaZ);

		//float result = 0;
		//for (int i = 0; i < tmpBoundsPoints.Length; i++)
		//{
		//    float tmp = Vector3.Distance(Vector3.zero, tmpBoundsPoints[i]);
		//    if (tmp > result)
		//        result = tmp;
		//}
		//return result;
	}

	

}
public enum InstanceType
{
	None,
	Mid,
	High
}