#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartGame : MonoBehaviour
{
    public static StartGame Instance;
    private string uiRootPath = "uiRoot";
    private GameObject uiRoot;
    [HideInInspector]
	public Joystick joystick;

    private void OnEnable()
    {
        if (Instance != null)
            return;
        Instance = this;
        Object prefab = Resources.Load(uiRootPath, typeof(GameObject));
        uiRoot = Instantiate(prefab) as GameObject;
        uiRoot.name = "UIRoot";
        joystick = uiRoot.GetComponentInChildren<Joystick>();
        uiRoot.transform.SetParent(this.transform);
        GameObject startPoint = GameObject.Find("startPoint");
		Transform player = GameObject.Find("Player").transform;
        if(startPoint != null)
        {
            player.position = startPoint.transform.position;
            player.localEulerAngles = startPoint.transform.localEulerAngles;
        }
        else
        {
            player.position = Vector3.zero;
            player.localEulerAngles = Vector3.zero;
        }

        joystick.model = player; 
		CameraController cameraController = Camera.main.GetComponent<CameraController>();
        if(cameraController == null)
        {
            cameraController = Camera.main.gameObject.AddComponent<CameraController>();
        }
        joystick.cameraController = cameraController;
		cameraController.player = player;
        cameraController.Init();
		//DontDestroyOnLoad(this);
    }
}
#endif