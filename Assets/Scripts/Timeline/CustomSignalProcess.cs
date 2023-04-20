using CFUtilPoolLib;
using System.Collections.Generic;
using UnityEngine;

public class CustomSignalProcess : XSingleton<CustomSignalProcess>
{


    public void OnProcess(SignalType type)
    {
        switch (type)
        {
            case SignalType.Capture:
                OnCapture();
                break;
            default:
                XDebug.singleton.AddErrorLog("unknown signal ", type.ToString());
                break;
        }
    }

    private void OnCapture()
    {
        XDebug.singleton.AddGreenLog("********* capture *********");
    }

}
