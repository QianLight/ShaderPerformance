using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IDeviceLevel
{
   void SetDeviceLevelHandler(System.Action<int> onDeviceLevel);
}
