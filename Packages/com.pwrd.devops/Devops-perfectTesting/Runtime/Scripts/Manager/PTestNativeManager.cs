using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PTestNativeManager : MonoBehaviour
{
    // Start is called before the first frame update
    public AndroidJavaClass CTUtils;
    void Start()
    {
        CTUtils = new AndroidJavaClass("com.pwrd.upsdk.CaptureUtils");
    }

    public void StarServer()
    {
        CTUtils.Call("GetMemoryData");

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
