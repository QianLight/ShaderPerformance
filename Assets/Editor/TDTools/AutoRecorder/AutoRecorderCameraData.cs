using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Cinemachine;

namespace TDTools
{
    public class AutoRecorderCameraData
    {
        public static readonly string[] CameraTypeStr = new string[]
        {
            "Small",
            "Middle",
            "Middle+",
            "Middle++",
            "Large",
            "Large+",
            "Titanic",
            "Titanic-Kaxi",
            "Custom",
        };

        public static Vector3[] CameraTypeTrackedOffsets = new Vector3[]
        {
            // Small
            new Vector3(0, 0, 0),
            // Middle
            new Vector3(0, 0, 0),
            // Middle+
            new Vector3(0, 0.5f, 0),
            // Middle++
            new Vector3(0, 0.5f, 0),
            // Large
            new Vector3(0, 0.5f, 0),
            // Large+
            new Vector3(0, 1.0f, 0),
            // Titanic
            new Vector3(0, 1.5f, 0),
            // Titanic-Kaxi
            new Vector3(0, -5f, 0),
            // Custom
            new Vector3(0, 0, 0),
        };

        public static readonly float[] CameraDirectionValue = new float[]
        {
            0f,
            -90f,
            90f,
            180f,
        };

        public static CinemachineFreeLook.Orbit[][] CameraTypeOrbits = new CinemachineFreeLook.Orbit[][]
        {
            // Small
            new CinemachineFreeLook.Orbit[]
            {
                new CinemachineFreeLook.Orbit(3f, 3f),
                new CinemachineFreeLook.Orbit(1f, 3f),
                new CinemachineFreeLook.Orbit(0.5f, 3f),
            },
            // Middle
            new CinemachineFreeLook.Orbit[]
            {
                new CinemachineFreeLook.Orbit(4f, 5f),
                new CinemachineFreeLook.Orbit(2f, 5f),
                new CinemachineFreeLook.Orbit(0.5f, 4f),
            },
            // Middle+
            new CinemachineFreeLook.Orbit[]
            {
                new CinemachineFreeLook.Orbit(4f, 7f),
                new CinemachineFreeLook.Orbit(4f, 7f),
                new CinemachineFreeLook.Orbit(0.5f, 4f),
            },
            // Middle++
            new CinemachineFreeLook.Orbit[]
            {
                new CinemachineFreeLook.Orbit(4f, 9f),
                new CinemachineFreeLook.Orbit(4f, 9f),
                new CinemachineFreeLook.Orbit(0.5f, 9f),
            },
            // Large
            new CinemachineFreeLook.Orbit[]
            {
                new CinemachineFreeLook.Orbit(5f, 12f),
                new CinemachineFreeLook.Orbit(5f, 12f),
                new CinemachineFreeLook.Orbit(5f, 12f),
            },
            // Large+
            new CinemachineFreeLook.Orbit[]
            {
                new CinemachineFreeLook.Orbit(6f, 17f),
                new CinemachineFreeLook.Orbit(6f, 17f),
                new CinemachineFreeLook.Orbit(6f, 17f),
            },
            // Titanic
            new CinemachineFreeLook.Orbit[]
            {
                new CinemachineFreeLook.Orbit(10f, 31f),
                new CinemachineFreeLook.Orbit(10f, 31f),
                new CinemachineFreeLook.Orbit(10f, 31f),
            },
            // Tatanic-Kaxi
            new CinemachineFreeLook.Orbit[]
            {
                new CinemachineFreeLook.Orbit(8f, 25f),
                new CinemachineFreeLook.Orbit(8f, 25f),
                new CinemachineFreeLook.Orbit(8f, 25f),
            },
            // Custom
            new CinemachineFreeLook.Orbit[]
            {
                new CinemachineFreeLook.Orbit(4f, 7f),
                new CinemachineFreeLook.Orbit(4f, 7f),
                new CinemachineFreeLook.Orbit(0.5f, 4f),
            },
        };

        public int CameraType = 0;
        public int CameraDirection = 0;

        public float CustomHeight = 4f, CustomRadius = 7f, CustomOffset = 0f;

        public void SetCustomParam(int index = -1, CinemachineFreeLook.Orbit[] orbit = null, float offset = float.MinValue)
        {
            if (index == -1)
                index = CameraType;
            if (offset.Equals(float.MinValue))
                offset = CustomOffset;
            if (orbit == null)
            {
                CameraTypeOrbits[index] = new CinemachineFreeLook.Orbit[]
                {
                    new CinemachineFreeLook.Orbit(CustomHeight, CustomRadius),
                    new CinemachineFreeLook.Orbit(CustomHeight, CustomRadius),
                    new CinemachineFreeLook.Orbit(CustomHeight, CustomRadius),
                };
                CameraTypeTrackedOffsets[index] = new Vector3(0, offset, 0);
            }
            else
            {
                CameraTypeOrbits[index] = orbit;
                CameraTypeTrackedOffsets[index] = new Vector3(0, offset, 0);
            }
        }

        public void SetCameraParam(GameObject camera, int direction = -1)
        {
            var freeLook = camera?.GetComponent<CinemachineFreeLook>();
            if (freeLook == null)
            {
                Debug.Log("没有找到FreeLook_skillEditor！！！");
                return;
            }
            if (direction != -1)
                CameraDirection = direction;
            freeLook.m_BindingMode = CinemachineTransposer.BindingMode.WorldSpace;
            freeLook.m_XAxis.Value = CameraDirectionValue[CameraDirection];

            freeLook.m_Orbits = CameraTypeOrbits[CameraType];
            var transposer = freeLook.GetRig(1).GetCinemachineComponent<CinemachineComposer>();
            transposer.m_TrackedObjectOffset = CameraTypeTrackedOffsets[CameraType];
        }

        public void ResetCameraParam(GameObject camera)
        {
            CameraType = 0;
            CameraDirection = 0;
            CustomHeight = 4f;
            CustomRadius = 7f;
            CustomOffset = 0f;
            int index = CameraTypeStr.Length - 1;
            CameraTypeOrbits[index] = new CinemachineFreeLook.Orbit[]
            {
                new CinemachineFreeLook.Orbit(CustomHeight, CustomRadius),
                new CinemachineFreeLook.Orbit(CustomHeight, CustomRadius),
                new CinemachineFreeLook.Orbit(CustomHeight, CustomRadius),
            };
            CameraTypeTrackedOffsets[index] = new Vector3(0, CustomOffset, 0);
            SetCameraParam(camera, 0);
        }
    }
}
