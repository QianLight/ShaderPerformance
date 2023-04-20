using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace CFEngine
{
   public class MultiLayerCulling
   {
      public MeshFilter[] _meshFilters;
      public MeshRenderer[] _meshRenderers;
      CullingGroup m_CullingGroup;

      public MultiLayer m_MultiLayer;
      public Mesh _Mesh;
      private MultiLayerJob m_MultiLayerJob;
      public MultiLayerCulling(GameObject splitObj, MultiLayer layer)
      {
         m_MultiLayer = layer;
         m_MultiLayerJob = new MultiLayerJob();
         _meshFilters = splitObj.GetComponentsInChildren<MeshFilter>();
         _meshRenderers = splitObj.GetComponentsInChildren<MeshRenderer>();
         m_MultiLayerJob.Init(_meshFilters);
         
         ShowRenderState(false);
         
         if (!Application.isPlaying)
         {
            //lastShowMeshFilters.AddRange(_meshFilters);
            Mesh mesh = m_MultiLayerJob.ShowAllMesh();;
            m_MultiLayer.SetRenderMesh(mesh);
            _Mesh = mesh;
         }
         else
         {
            InitCulling();
         }
         
      }

      private void InitCulling()
      {
         m_CullingGroup = new CullingGroup();
         BoundingSphere[] spheres = new BoundingSphere[_meshFilters.Length];
         for (int i = 0; i < _meshFilters.Length; i++)
         {
            MeshRenderer mr = _meshRenderers[i];

            GetMeshRendererSphere(mr, out Vector3 position, out float radius);
            spheres[i] = new BoundingSphere(position, radius / 2);
         }


         Camera cam = EngineUtility.GetMainCamera();
         m_CullingGroup.targetCamera = cam;
         m_CullingGroup.SetDistanceReferencePoint(cam.transform);
         m_CullingGroup.SetBoundingSpheres(spheres);
         m_CullingGroup.SetBoundingSphereCount(spheres.Length);
         m_CullingGroup.onStateChanged = OnChange;
         m_CullingGroup.enabled = true;
      }

      public void ToggleRender()
      {
         bool b = !_meshRenderers[0].enabled;
         ShowRenderState(b);
      }

      private void ShowRenderState(bool b)
      {
         for (int i = 0; i < _meshRenderers.Length; i++)
         {
            _meshRenderers[i].enabled = b;
         }
      }

      private void GetMeshRendererSphere(MeshRenderer mr, out Vector3 pos, out float radius)
      {
         Vector3 size = mr.bounds.size;
         radius = Mathf.Max(Mathf.Max(size.x, size.y), size.z);
         pos = mr.bounds.center;
      }

      //public List<MeshFilter> lastShowMeshFilters = new List<MeshFilter>();



      private void OnChange(CullingGroupEvent evt)
      {
         // Debug.Log("CullingGroupEvent: index:" + evt.index + "  isVisible:" + evt.isVisible + "  wasVisible:" +
         //           evt.wasVisible + "  hasBecomeInvisible:" + evt.hasBecomeInvisible + "  hasBecomeVisible:" +
         //           evt.hasBecomeVisible+"  "+_meshFilters[evt.index],_meshFilters[evt.index]);

         if (evt.hasBecomeVisible)
         {
            RefreshMesh(evt.index, true);
         }

         if (evt.hasBecomeInvisible)
         {
            RefreshMesh(evt.index, false);
         }

         
      }

      public void UpdateData()
      {
         if (!lastFrameHasChange) return;

         lastFrameHasChange = false;
         m_MultiLayer.SetRenderMesh(m_MultiLayerJob.GetCombineMesh());

         //Debug.Log("m_MultiLayerJob UpdateData GetCombineMesh");
      }

      private bool lastFrameHasChange = false;

      private void RefreshMesh(int nIndex, bool bState)
      {
         m_MultiLayerJob.SetRefreshMesh(nIndex, bState);
         lastFrameHasChange = true;
      }

      public void OnDrawGizmos()
      {
         for (int i = 0; i < _meshFilters.Length; i++)
         {
            MeshRenderer mr = _meshRenderers[i];
            GetMeshRendererSphere(mr, out Vector3 position, out float radius);
            Gizmos.DrawWireSphere(position, radius / 2);
         }
      }

      public void OnDestory()
      {
         if (m_CullingGroup != null)
            m_CullingGroup.Dispose();
         m_CullingGroup = null;
         ClearJob();
      }

      public void ClearJob()
      {
         if (m_MultiLayerJob != null)
            m_MultiLayerJob.OnDestory();
         m_MultiLayerJob = null;
      }
   }
}