#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
namespace CFEngine
{
    [System.Serializable]
    public enum ColliderType
    {
        None,
        Box,
        Sphere,
        Capsule,
        Mesh
    }

    [System.Serializable]
    public class ColliderData : BaseSceneData<ColliderData>
    {
        public int objID = -1;
        public ColliderType colliderType = ColliderType.None;
        public Vector3 center;
        public Vector3 size;
        public float radius;
        public float height;
        public int direction;
        public Mesh sharedMesh;
        public bool convex;
        public MeshColliderCookingOptions cookingOptions;
        // public string exString;

        public override void Copy (ColliderData src)
        {
            colliderType = src.colliderType;
            center = src.center;
            size = src.size;
            radius = src.radius;
            height = src.height;
            direction = src.direction;
            sharedMesh = src.sharedMesh;
            convex = src.convex;
            cookingOptions = src.cookingOptions;
            // exString = src.exString;
        }
        public static Collider Copy (Collider src)
        {
            Collider c = null;

            if (src is BoxCollider)
            {
                var go = new GameObject (src.name);
                BoxCollider bc = src as BoxCollider;
                var des = go.AddComponent<BoxCollider> ();
                des.center = bc.center;
                des.size = bc.size;
                c = des;
            }
            else if (src is SphereCollider)
            {
                var go = new GameObject (src.name);
                SphereCollider sc = src as SphereCollider;
                var des = go.AddComponent<SphereCollider> ();
                des.center = sc.center;
                des.radius = sc.radius;
                c = des;
            }
            else if (src is CapsuleCollider)
            {
                var go = new GameObject (src.name);
                CapsuleCollider cc = src as CapsuleCollider;
                var des = go.AddComponent<CapsuleCollider> ();
                des.center = cc.center;
                des.radius = cc.radius;
                des.height = cc.height;
                des.direction = cc.direction;
                c = des;
            }
            else if (src is MeshCollider)
            {
                MeshCollider mc = src as MeshCollider;
                if (mc.sharedMesh != null)
                {
                    string meshPath = string.Format ("{0}{1}.asset", LoadMgr.singleton.editorResPath, mc.sharedMesh.name);
                    if (File.Exists (meshPath))
                    {
                        var mesh = AssetDatabase.LoadAssetAtPath<Mesh> (meshPath);
                        if (mesh != null && mesh.vertexCount > 1000)
                        {
                            DebugLog.AddErrorLog2 ("collider mesh too many vertex:{0}", mesh.vertexCount.ToString ());
                        }
                        else
                        {
                            var go = new GameObject (src.name);
                            var des = go.AddComponent<MeshCollider> ();
                            des.sharedMesh = mesh;
                            des.convex = mc.convex;
                            des.cookingOptions = mc.cookingOptions;
                            c = des;
                        }
                    }
                    else
                    {

                        DebugLog.AddWarningLog2 ("collider mesh not find:{0}", meshPath);
                    }
                }
            }
            return c;
        }
        public void Save (Collider c)
        {
            if (c is BoxCollider)
            {
                BoxCollider bc = c as BoxCollider;
                colliderType = ColliderType.Box;
                center = bc.center;
                size = bc.size;
            }
            else if (c is SphereCollider)
            {
                SphereCollider sc = c as SphereCollider;
                colliderType = ColliderType.Sphere;
                center = sc.center;
                radius = sc.radius;
            }
            else if (c is CapsuleCollider)
            {
                CapsuleCollider cc = c as CapsuleCollider;
                colliderType = ColliderType.Capsule;
                center = cc.center;
                radius = cc.radius;
                height = cc.height;
                direction = cc.direction;
            }
            else if (c is MeshCollider)
            {
                MeshCollider mc = c as MeshCollider;
                colliderType = ColliderType.Mesh;
                sharedMesh = mc.sharedMesh;
                convex = mc.convex;
                cookingOptions = mc.cookingOptions;
            }
        }

        public void Load (GameObject go)
        {
            switch (colliderType)
            {
                case ColliderType.Box:
                    {
                        BoxCollider bc = go.AddComponent<BoxCollider> ();
                        bc.center = center;
                        bc.size = size;
                    }
                    break;
                case ColliderType.Sphere:
                    {
                        SphereCollider sc = go.AddComponent<SphereCollider> ();
                        sc.center = center;
                        sc.radius = radius;
                    }
                    break;
                case ColliderType.Capsule:
                    {
                        CapsuleCollider cc = go.AddComponent<CapsuleCollider> ();
                        cc.center = center;
                        cc.radius = radius;
                        cc.height = height;
                        cc.direction = direction;
                    }
                    break;
                case ColliderType.Mesh:
                    {
                        MeshCollider mc = go.AddComponent<MeshCollider> ();
                        mc.sharedMesh = sharedMesh;
                        mc.convex = convex;
                        mc.cookingOptions = cookingOptions;
                    }
                    break;
            }
        }
    }
}
#endif