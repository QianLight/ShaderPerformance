using System;
using System.Collections;
using System.Collections.Generic;
using CFClient.React;
using UnityEngine;

public class WeatherTest : MonoBehaviour
{
   public float Clock;
   public float TransitionTime;
   public int targetVolumeIndex;
   
   [ContextMenu("Test")]
   public void Test()
   {
      WeatherSystemExtend.WeatherParams @params = new WeatherSystemExtend.WeatherParams()
      {
         Clock = Clock,
         TransitionTime = TransitionTime,
         targetVolumeIndex = targetVolumeIndex
      };
      WeatherSystemExtend.Instance.SetWeatherParams(@params);
   }

   private void Update()
   {
      if (Input.GetKeyUp(KeyCode.K))
      {
         Test();
      }
   }
}
