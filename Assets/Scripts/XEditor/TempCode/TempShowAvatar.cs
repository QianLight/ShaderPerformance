#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CFEngine;

public class TempShowAvatar : MonoBehaviour
{

    public Transform[] showTSs;
    public string[] buttonNames;
    Transform camTS;
    public float Width = 100;
    public float Height = 30;
    public float XOffset = 30;
    public float YOffset = 35;
    public float XOrigin = 30;
    public float YOrigin = 30;
    public float YBottom = 30;

    private void Awake()
    {
        var cam = GameObject.Find("Main Camera");
        if (cam != null)
        {
            camTS = cam.transform;
            EnvironmentExtra ee = GameObject.Find("Main Camera").GetComponent<EnvironmentExtra>();
            if (ee != null)
            {
                ee.forceIgnore = true;
            }
        }

    }

    // Use this for initialization
    void Start()
    {
        if (camTS != null && showTSs != null && showTSs.Length > 0)
        {
            SetCam(0);
        }

    }

    // Update is called once per frame
    void Update()
    {

    }

    private void OnGUI()
    {
        if (camTS == null || showTSs == null || showTSs.Length == 0) return;

        int count = showTSs.Length;
        int dx = 0, dy = 0;
        for (int i = 0; i < count; ++i)
        {
            float y = YOrigin + dy * YOffset + Height + YBottom;
            float x = XOrigin + dx * (XOffset + Width);

            if (y >= Screen.height)
            {
                dy = 0;
                dx++;
                x = XOrigin + dx * (XOffset + Width);
                y = YOrigin + dy * YOffset + Height;
            }
            else
            {
                y -= YBottom;
            }

            if (GUI.Button(new Rect(new Vector2(x, y), new Vector2(Width, Height)), i < buttonNames.Length ? buttonNames[i] : string.Format("Change:{0}", i)))
            {
                Debug.Log(i.ToString());
                SetCam(i);
            }

            dy++;
        }
    }

    void SetCam(int idx)
    {
        if (camTS != null)
        {
            camTS.SetParent(null);
        }
        for (int i = 0; i < showTSs.Length; ++i)
        {
            showTSs[i].gameObject.SetActive(i == idx);
        }
        var camRoot = showTSs[idx].Find("cam");

        if (camRoot != null)
        {
            camTS.SetParent(camRoot);
            camTS.localPosition = Vector3.zero;
            camTS.localRotation = Quaternion.identity;
        }


    }
}

#endif