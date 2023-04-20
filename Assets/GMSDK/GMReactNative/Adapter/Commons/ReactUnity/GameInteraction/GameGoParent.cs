/*
 * @Author: hexiaonuo
 * @Date: 2021-10-15
 * @Description: Game set parentGo, for ReactUnity action panel
 * @FilePath: ReactUnity/GameInteraction/GameGoParent.cs
 */

using System.Collections.Generic;
using UnityEngine;

namespace GSDK.RNU
{
    
    /*
     * 游戏设置父节点
     * 父节点要求：适配游戏的全屏大小
     * 在初始化 RNUMin 之后，游戏可设置九尾活动面板挂载的父节点信息
     */
    partial class GameInteraction
    {
        private static GameObject GameParentGo;

        public static void SetGameGoParent(string parentGoName)
        {
            if (parentGoName.Length <= 0)
            {
                Util.LogAndReport("set GameGoParent failed for name is not correct");
                return;
            }

            GameParentGo = GameObject.Find(parentGoName);
        }

        public static void SetGameGoParent(GameObject gameObject)
        {
            GameParentGo = gameObject;
        }
        

        public static GameObject GetGameGoParent()
        {
            Util.Log("GetGameParentGo name is : {0}", GameParentGo==null ? "null" : GameParentGo.name);
            return GameParentGo;
        }


        public static void SetRNUTouchIgnore(GameObject gameObject, bool isTouchIgnore)
        {
            if (gameObject == null)
            {
                Util.Log("set ugui touch ignore failed, for gameObject is nil");
                return;
            }

            if (isTouchIgnore)
            {
                gameObject.AddComponent<UGUITouchIgnore>();
                Util.Log("set {0} isTouchIgnore", gameObject.name);
            }
            else
            {
                GameObject.Destroy(gameObject.GetComponent<UGUITouchIgnore>());
                Util.Log("destroy {0} isTouchIgnore", gameObject.name);
            }
        }
        
        
    }
}