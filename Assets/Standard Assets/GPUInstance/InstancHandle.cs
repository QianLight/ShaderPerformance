using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class InstanceHandld
{
	private bool noURP = false;
	private bool init = true;
	private List<InstanceItemBlock> blockList = null;
	private List<GameObject> storeGameObjects = new List<GameObject>();

	public InstanceHandld()
    {
		StaticInstanceMgr.Init();
		noURP = GraphicsSettings.renderPipelineAsset == null;
	}

	public bool Update(Camera cam)
    {
		if(init && cam == null)
        {
			return false;
        }
		if (init && noURP)
        {
			StaticInstanceMgr.Update(null);
		}
		return true;
	}

	public void SetInstance(Transform root, Camera cam, List<string> shaderList, float cullingDistance, InstanceType type = InstanceType.Mid)
	{
		if (init && cam != null)
		{
			init = false;
			blockList = StaticInstanceMgr.GetRender(root, cam, shaderList, cullingDistance, type, ref storeGameObjects);
		}
	}
	public void ClearInstance(bool back)
    {
		StaticInstanceMgr.Clear(blockList);
		if (back)
        {
			StaticInstanceMgr.SetActive(storeGameObjects, true);
		}
		init = true;
	}
}
