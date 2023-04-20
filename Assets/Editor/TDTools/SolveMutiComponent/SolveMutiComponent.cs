using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEditor;

namespace TDTools
{
    class SolveMutiComponent : MonoBehaviour
    {
        [MenuItem("GameObject/TDTools/关卡相关工具/CheckAndDestoryMuti")]
        public static void CheckAndDestory()
        {
            var obj = Selection.activeGameObject;
            var components = obj.GetComponentsInChildren<LevelEditor.IAmDynamicWall>();
            List<int> instanceID = new List<int>();
            foreach(var item in components)
            {
                if(instanceID.Contains(item.transform.GetInstanceID()))
                {
                    DestroyImmediate(item);
                }
                else
                {
                    instanceID.Add(item.transform.GetInstanceID());
                }
            }
        }
    }
}
