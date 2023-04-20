using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
namespace CFEngine
{
    [Serializable]
    public class GrassItem
    {
        public GameObject _gameobject;
        public Vector4 WorldPosOffset;
        public Texture2D LightMap;
    }
    public class GrassManager : MonoBehaviour
    {

        public static int _ParamKey = Shader.PropertyToID("_Param");
        public static int _CustomLightmap = Shader.PropertyToID("_CustomLightmap");
        public static int _ChunkOffsetKey = Shader.PropertyToID("_ChunkOffset");

        public static List<GrassManager> grassManagerList = new List<GrassManager>();
        public static void SearchGrass()
        {

#if UNITY_EDITOR
            foreach (GrassManager mgr in grassManagerList)
            {
                mgr.SearchGrass1();
            }
#endif

        }
        public List<GrassItem> GrassItems = new List<GrassItem>();
        private bool setState = true;
        private void Awake()
        {
            if (!grassManagerList.Contains(this))
            {
                grassManagerList.Add(this);
            }
        }
        private void Start()
        {
          //  SetGrass();
        }
        private void OnEnable()
        {
          //  SetGrass();
        }

        private void OnDisable()
        {
            
        }

        private void OnDestroy()
        {
            if(grassManagerList.Contains(this))
            {
                grassManagerList.Remove(this);
            }
        }

        private void SetGrass()
        {
            if (setState)
            {
                setState = false;
                for (int i = 0; i < GrassItems.Count; i++)
                {
                    SetGrassItem(GrassItems[i]);
                }
            }
        }
        private void SetGrassItem(GrassItem item)
        {
            if (item != null)
            {
                Renderer Ren = item._gameobject.GetComponent<Renderer>();
                Material[] materials = new Material[Ren.materials.Length];
                for (int i = 0; i < Ren.materials.Length; i++)
                {
                    materials[i] = Ren.materials[i];
                    if (item.LightMap != null)
                        materials[i].SetTexture(_CustomLightmap, item.LightMap);
                    materials[i].SetVector(_ChunkOffsetKey, item.WorldPosOffset);
                    materials[i].SetVector(_ParamKey, new Vector4(0, item.LightMap != null ? 1 : 0, 1, 0));

                }

                item._gameobject.GetComponent<Renderer>().materials = materials;
            }
        }

        private void Update()
        {
          //  SetGrass();
        }

#if UNITY_EDITOR
        public void SearchGrass1()
        {
            GrassItems.Clear();
            SearchGrassMat(transform);
        }

		void SearchGrassMat(Transform trans)
		{
			Renderer render = trans.gameObject.GetComponent<Renderer>();
			if (render != null)
			{
			//	Material mat = render;
				//if (mat != null)
				//{
                    InstanceObject instanceObj = trans.GetComponent<InstanceObject>();
                    if(instanceObj != null && instanceObj.lightmap != null)
                    {
                        GrassItem item = new GrassItem();
                        item.LightMap = instanceObj.lightmap;
                        item._gameobject = trans.gameObject;
                        item.WorldPosOffset = instanceObj.worldPosOffset;
                        GrassItems.Add(item);
                    }
              
			}
			foreach (Transform o in trans)
			{
                SearchGrassMat(o);
			}
		}
#endif

    }
}
