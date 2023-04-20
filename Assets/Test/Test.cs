#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngineEditor = UnityEditor.Editor;
using CFEngine;
using UnityEditor;
using UnityEngine.Profiling;

[ExecuteInEditMode]
public class Test : MonoBehaviour
{
	public bool test = false;

	//rotate move
	[Range (-10, 10)]

	public float moveSpeed = 0;
	[HideInInspector]
	public Vector2 moveDirXZ = new Vector2 (1, 0);

	[Range (0, 360)]
	public float angle = 0;
	[Range (10, 100)]
	public float width = 10;
	public Transform startPoint = null;
	public Transform endPoint = null;

	private Vector3 normal = Vector3.forward;

	public List<ObjectCache> objects = new List<ObjectCache> ();

	public Material mat0;
	public Material mat1;
	private MaterialPropertyBlock mpb0;
	private MaterialPropertyBlock mpb1;
	public Shader shader0;
	public Shader shader1;

	public Transform c;
	public Transform t;
	public Renderer r;

	public bool result0 = false;
	public bool result1 = false;


	public ParticleSystem ps;
	public float time = 1;
	// private float accT = 0;
	private CommandBuffer cb;

	public Camera uiCamera;

	public string abPath;
	public string assetName;
	private AssetBundle ab;
	private UnityEngine.Object obj;


	// Use this for initialization
	void Start ()
	{
        //AnimatorUtility.OptimizeTransformHierarchy(this.gameObject, null);
    }

    private void OnDestroy()
    {
		AssetBundle.UnloadAllAssetBundles(true);
	}
    // Update is called once per frame
    void Update()
    {
        if (mat0 != null && r != null)
        {
            if (cb == null)
            {
                cb = new CommandBuffer();
            }
            if (mpb0 == null)
            {
                mpb0 = new MaterialPropertyBlock();
            }
            mpb0.SetColor(ShaderManager._Color0, new Color(1, 0, 0, 0.5f));
            r.SetPropertyBlock(mpb0);
            cb.DrawRenderer(r, mat0);
            if (mpb1 == null)
            {
                mpb1 = new MaterialPropertyBlock();
            }
			mpb1.SetColor(ShaderManager._Color0, new Color(0, 0, 1, 0.5f));
            r.SetPropertyBlock(mpb1);
            cb.DrawRenderer(r, mat0);
            Graphics.ExecuteCommandBuffer(cb);
			cb.Clear();


        }
        if (ab == null && System.IO.File.Exists(abPath))
        {
            ab = AssetBundle.LoadFromFile(abPath);
        }
        if (ab != null && !string.IsNullOrEmpty(assetName))
        {
            obj = ab.LoadAsset(assetName);
            if (obj != null)
            {
				ab.Unload(false);
                DebugLog.AddErrorLog2("ab asset:{0}", obj.name);
                if (obj is GameObject)
                {
                    GameObject.DestroyImmediate(obj, true);
                }
                else
                {
					Resources.UnloadAsset(obj);
				}

				obj = null;
			}
        }
			//DebugLog.AddEngineLog2("world pos:{0} local:{1}", this.transform.position,this.transform.localPosition);
			//if (uiCamera != null)
			//	DebugLog.AddEngineLog2("screen pos:{0}", RectTransformUtility.WorldToScreenPoint(uiCamera, this.transform.position));
			//if (test)
			//{
			//	test = false;
			//}
			//UpdateTest0 ();
			//UpdateTest1 ();
			//UpdateTest2 ();
			//if (ps != null)
			//{
			//	if (accT > time)
			//	{
			//		ps.Play (true);
			//		accT = 0;
			//	}
			//	else
			//	{
			//		accT += Time.deltaTime;
			//	}
			//}
			//if (shader0 != null && shader1 != null)
			//      {
			//	if(r==null)

			//          {
			//		this.gameObject.TryGetComponent<Renderer>(out r);
			//          }
			//	if(mat0==null)
			//          {
			//		mat0 = new Material(shader0);
			//              if (r != null)
			//              {
			//                  r.sharedMaterial = mat0;
			//              }
			//          }
			//	else
			//          {
			//		Profiler.BeginSample("switch shader");
			//              int frame = Time.frameCount % 2;
			//              if (frame == 0)
			//              {
			//                  mat0.shader = shader0;
			//              }
			//              else
			//              {
			//                  mat0.shader = shader1;
			//              }

			//              Profiler.EndSample();

			//	}
			//      }
			if (t != null)
        {
            if (t.TryGetComponent<MeshRenderer>(out var mr) && t.TryGetComponent<MeshFilter>(out var mf))
            {
                Graphics.DrawMesh(
                    mf.sharedMesh,
                    t.localToWorldMatrix,
                    mr.sharedMaterial,
                    0,
                null,

                                           0,
                    null,
                    false,
                    false,
                    false);
            }
        }
    }
    void UpdateTest0()
    {
        moveDirXZ.x = Mathf.Sin(Mathf.Deg2Rad * angle);
        moveDirXZ.y = Mathf.Cos(Mathf.Deg2Rad * angle);
        normal = new Vector3(moveDirXZ.x, 0, moveDirXZ.y);
        if (Mathf.Abs(moveSpeed) > 0.0001f && endPoint != null)
        {
            float dist = Vector3.Dot(normal, endPoint.position);
            float dist2 = Vector3.Dot(normal, startPoint.position);
            float delta = Mathf.Abs(dist2 - dist);
            for (int i = 0; i < objects.Count; ++i)
            {
                var obj = objects[i];
                if (obj.t != null)
                {
                    obj.t.position += moveSpeed * normal;
                    if ((Vector3.Dot(normal, obj.t.position) - dist) > 0)
                    {
                        obj.t.position -= normal * delta;
                    }
                }

            }
        }
    }

    void UpdateTest1 ()
	{
		if (mat0 != null && mat1 != null)
		{
			Color c = mat0.GetColor ("_Color");
			Vector4 v = new Vector4 (c.r, c.g, c.b, c.a);
			mat1.SetColor ("_Color", new Color (v.x, v.y, v.z, v.w));
		}
	}

	void UpdateTest2 ()
	{
		if (c != null && r != null && t != null)
		{
			var cameraPlane = c.right;
			var cameraPos = c.position;
			var cameraPlaneDist = -EngineUtility.Dot (ref cameraPos, ref cameraPlane);
			AABB aabb = AABB.Create (r.bounds);
			var lookAtPos = t.position;
			var c2t = lookAtPos - cameraPos;
			var camera2TargetDir = c2t.normalized;
			CFEngine.Ray2D camera2Target = new CFEngine.Ray2D ();
			EngineUtility.Ray2D (c2t.x, c2t.z, cameraPos.x, cameraPos.z, ref camera2Target.normalXZ);
			EngineUtility.Ray2D (c2t.z, c2t.y, cameraPos.z, cameraPos.y, ref camera2Target.normalYZ);
			EngineUtility.Ray2D (c2t.x, c2t.y, cameraPos.x, cameraPos.y, ref camera2Target.normalXY);

			result0 = false;
			Profiler.BeginSample ("test aabb 0");
			for (int i = 0; i < 10000; ++i)
			{
				if (EngineUtility.FastTestRayAABB (ref cameraPlane, cameraPlaneDist, ref aabb))
				{
					if (EngineUtility.FastTestRayAABB (
							ref cameraPos,
							ref lookAtPos,
							ref camera2Target,
							ref aabb, out var rayAABB))
					{
						if (EngineUtility.FastTestPointPlane (
								ref cameraPos,
								ref lookAtPos,
								ref aabb)) { result0 = true; }
					}
				}
			}
			Profiler.EndSample ();

			result1 = false;
			Profiler.BeginSample ("test aabb 1");
			for (int i = 0; i < 10000; ++i)
			{
				if (EngineUtility.FastTestRayAABB (ref cameraPlane, cameraPlaneDist, ref aabb))
				{
					if (EngineUtility.FastTestRayAABB (
							ref cameraPos,
							ref camera2TargetDir,
							ref aabb))
					{
						if (EngineUtility.FastTestPointPlane (
								ref cameraPos,
								ref lookAtPos,
								ref aabb)) { result1 = true; }
					}
				}
			}
			Profiler.EndSample ();
		}

	}
	public void DoTest0 ()
	{

	}

	public void OnDrawGizmos ()
	{
		var c = Gizmos.color;

		if (startPoint != null)
		{
			Gizmos.color = Color.green;
			RuntimeUtilities.DrawPlane (startPoint.position, normal, Vector3.up, width, width * 0.5f);
			Gizmos.color = Color.yellow;
			Gizmos.DrawLine (startPoint.position, startPoint.position + normal * 5);
		}
		if (endPoint != null)
		{
			Gizmos.color = Color.red;
			RuntimeUtilities.DrawPlane (endPoint.position, normal, Vector3.up, width, width * 0.5f);
		}
		Gizmos.color = Color.green;
		for (int i = 0; i < objects.Count; ++i)
		{
			var obj = objects[i];
			if (obj.r != null)
			{
				Gizmos.DrawWireCube (obj.r.bounds.center, obj.r.bounds.size);
			}

		}
		Gizmos.color = c;
	}
}

[CustomEditor (typeof (Test))]
public class TestEditor : UnityEngineEditor
{
	public override void OnInspectorGUI ()
	{
		base.OnInspectorGUI ();
		Test t = target as Test;
		EditorGUI.BeginChangeCheck ();
		Transform tran = null;
		tran = EditorGUILayout.ObjectField ("Folder", tran, typeof (Transform), true) as Transform;
		if (EditorGUI.EndChangeCheck ())
		{
			if (tran != null)
			{
				for (int i = 0; i < tran.childCount; ++i)
				{
					var child = tran.GetChild (i);
					Renderer r;
					if (child.TryGetComponent (out r))
					{
						t.objects.Add (new ObjectCache ()
						{
							t = child,
								r = r
						});
					}

				}
			}
		}
		if (GUILayout.Button ("TestUpdate"))
		{
			(target as Test).test = true;
		}
		if (GUILayout.Button ("Test0"))
		{
			(target as Test).DoTest0 ();
		}

	}
}
#endif