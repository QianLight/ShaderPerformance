#if UNITY_EDITOR
using UnityEngine;

[ExecuteInEditMode]
public class DIYVisualTool : MonoBehaviour
{
    [Header("球心")]
    public Transform center;
    [Header("半径")]
    public float radus = 21;

    [Header("起始点")]
    public Transform origTf;
    [Header("终止点")]
    public Transform targetTf;
    
    [Header("起始Fov")]
    public float origFov = 45;
    [Header("终止Fov")]
    public float targetFov = 40;
  
    [Header("采点个数")]
    public int max = 100;

    [Header("水平旋转")]
    [Range(-25, 25)]
    public float rotateX = 0;

    [Header("竖直旋转")]
    [Range(-25, 25)]
    public float rotateY = 0;

    [Header("编辑器预览")]
    [Range(0, 1)]
    public float slider = 0;

    private Vector3 midPos;

    private Vector3[] points;

    [ContextMenu("CreateEnv")]
    private void CreateEnv()
    {
        GameObject diy = new GameObject("DIY");
        diy.transform.localPosition = Vector3.zero;
        diy.transform.localEulerAngles = Vector3.zero;
        diy.transform.localScale = Vector3.one;

        GameObject centerGo = new GameObject("center");
        centerGo.transform.parent = diy.transform;
        centerGo.transform.position = new Vector3(58, 8, 51);
        center = centerGo.transform;

        GameObject p1 = new GameObject("p1");
        p1.transform.position = new Vector3(71, 8, 69);
        p1.transform.parent = diy.transform;
        origTf = p1.transform;
        origTf.localEulerAngles = new Vector3(4, -138, 0);

        GameObject p2 = new GameObject("p2");
        p2.transform.position = new Vector3(63, 8, 39);
        p2.transform.parent = diy.transform;
        targetTf = p2.transform;
        targetTf.localEulerAngles = new Vector3(0, 30, 0);

        GenPoints();
    }


    private void GenPoints()
    {
        if (points == null || max != points.Length)
        {
            points = new Vector3[max];
        }
        CaluteMidPoint();
        for (int i = 0; i < max; i++)
        {
            float t = i / (float)max;
            points[i] = DrawBezierCurve(t, origTf.position, midPos, targetTf.position);
        }
    }

    private void CaluteMidPoint()
    {
        Vector3 v1 = (origTf.position - center.position).normalized;
        Vector3 v2 = (targetTf.position - center.position).normalized;

        Vector3 midV = (v1 + v2).normalized;
        midPos = center.position + midV * radus;
    }

    private void OnEnable()
    {
        if (center != null)
        {
            GenPoints();
        }
        var env = Camera.main.GetComponent<CFEngine.EnvironmentExtra>();
        env.loadGameAtHere = false;
    }


    private float startTime;
    private bool play;

    private void Update()
    {
        if (Application.isPlaying)
        {
            if (points != null && play)
            {
                float offset = Time.time - startTime;
                float t = offset / 2.0f;
                UpdateCamera(t);
                if (t >= 1) play = false;
            }
            if (Input.GetKeyUp(KeyCode.Space))
            {
                startTime = Time.time;
                play = true;
            }
        }
        else
        {
            if (origTf)
            {
                UpdateCamera(slider);
                UpdateCamX();
                UpdateCamY();
            }
        }
    }

    private void UpdateCamera(float t)
    {
        var cam = Camera.main;
        var tf = cam.transform;
        tf.position = DrawBezierCurve(t, origTf.position, midPos, targetTf.position);
        tf.rotation = Quaternion.Lerp(origTf.rotation, targetTf.rotation, t);
        cam.fieldOfView = Mathf.Lerp(origFov, targetFov, t);
    }

    private void UpdateCamX()
    {
        var cam = Camera.main;
        var tf = cam.transform;
        float t = rotateX / 90.0f;
        if (t > 0)
            tf.forward = Vector3.Lerp(tf.forward, tf.right, t);
        else
            tf.forward = Vector3.Lerp(tf.forward, -tf.right, -t);
    }

    private void UpdateCamY()
    {
        var cam = Camera.main;
        var tf = cam.transform;
        float t = rotateY / 90.0f;
        if (t > 0)
            tf.forward = Vector3.Lerp(tf.forward, tf.up, t);
        else
            tf.forward = Vector3.Lerp(tf.forward, -tf.up, -t);
    }


    public void OnDrawGizmos()
    {
        if (center != null)
        {
            GenPoints();
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(center.position, radus);
            Gizmos.DrawSphere(center.position, 0.2f);

            Gizmos.color = Color.green;
            Gizmos.DrawSphere(origTf.position, 0.1f);
            Gizmos.DrawSphere(midPos, 0.2f);
            Gizmos.DrawSphere(targetTf.position, 0.1f);

            Gizmos.color = Color.red;
            for (int i = 0; i < max - 1; i++)
            {
                if (points[i] != null) Gizmos.DrawLine(points[i], points[i + 1]);
            }
            Gizmos.color = Color.white;
            var cam = Camera.main;
            Gizmos.matrix = cam.transform.localToWorldMatrix;
            Gizmos.DrawFrustum(Vector3.zero, cam.fieldOfView, cam.farClipPlane, cam.nearClipPlane, cam.aspect);
        }
    }


    /// <summary>
    /// https://blog.csdn.net/ZHENZHEN9310/article/details/101062335
    /// </summary>
    private Vector3 DrawBezierCurve(float t, Vector3 p0, Vector3 p1, Vector3 p2)
    {
        float a = (1 - t) * (1 - t);
        float b = 2 * t * (1 - t);
        float c = t * t;
        return a * p0 + b * p1 + c * p2;
    }
}

#endif