/*******************************************************************
* Copyright © 2017—2021 Perfect World.Co.Ltd. All rights reserved.
* author: 移动项目支持部 ZEUS FRAMEWORK TEAM
********************************************************************/
using System.Collections.Generic;
using UnityEngine;
using System.Collections;
using System;

namespace Zeus.Framework
{
    public static class UnityExtensions
    {

        public delegate void VoidDelegate();

        /// <summary>
        /// 查找所有
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="trans"></param>
        /// <returns></returns>
        public static List<T> FindAllChild<T>(this Transform trans) where T : Component
        {
            var list = new List<T>();
            FindChilds<T>(trans, ref list);
            return list;
        }

        private static void FindChilds<T>(Transform trans, ref List<T> result) where T : Component
        {
            var root = trans.GetComponent<T>();
            if(root != null)
            {
                result.Add(root);
            }
            for(int i = 0; i < trans.childCount; i++)
            {
                FindChilds<T>(trans.GetChild(i), ref result);
            }
        }

        /// <summary>
        /// Extension for Easy call
        /// </summary>
        /// <param name="comp"></param>
        /// <param name="duration"></param>
        /// <param name="del"></param>
        public static void WaitForSecondsExt(this MonoBehaviour comp, float duration, VoidDelegate del)
        {
            comp.StartCoroutine(DoWaitForSecondsExt(comp, duration, del, null));
        }

        /// <summary>
        /// Extension for Easy call
        /// </summary>
        /// <param name="comp"></param>
        /// <param name="frameCnt"></param>
        /// <param name="del"></param>
        public static void WaitForNextFramesExt(this MonoBehaviour comp, int frameCnt, VoidDelegate del)
        {
            comp.StartCoroutine(DoWaitForNextFrameExt(comp, frameCnt, del, null));
        }

        /// <summary>
        /// Extension for Easy call
        /// </summary>
        /// <param name="comp"></param>
        /// <param name="duration"></param>
        /// <param name="del"></param>
        /// <param name="update"></param>
        /// <returns></returns>
        public static IEnumerator DoWaitForSecondsExt(this MonoBehaviour comp, float duration, VoidDelegate del, VoidDelegate update)
        {
            while(duration > 0)
            {
                duration -= Time.deltaTime;
                if(update != null) update();
                yield return null;
            }

            if(del != null) del();
        }

        /// <summary>
        /// Extension for Easy call
        /// </summary>
        /// <param name="comp"></param>
        /// <param name="frameCnt"></param>
        /// <param name="del"></param>
        /// <param name="update"></param>
        /// <returns></returns>
        public static IEnumerator DoWaitForNextFrameExt(this MonoBehaviour comp, int frameCnt, VoidDelegate del, VoidDelegate update)
        {
            while(frameCnt > 0)
            {
                frameCnt--;
                if(update != null) update();
                yield return null;
            }

            if(del != null) del();
        }

        public static T UniqueAddComponent<T>(GameObject gobj) where T : Component
        {
            T t = gobj.GetComponent<T>() as T;
            if(null == t)
            {
                t = gobj.AddComponent<T>();
            }
            else
            {
                if(t is Behaviour && !(t as Behaviour).enabled)
                {
                    (t as Behaviour).enabled = true;
                }
            }

            return t;
        }

        public static void IdentityTransform(Transform parent, Transform target, bool checkParentChanged = true)
        {
            target.SetParent(parent);
            target.localPosition = Vector3.zero;
            target.localRotation = Quaternion.identity;
            target.localScale = Vector3.one;
        }

        public static bool IsInScreen(Transform t)
        {
            return IsInScreen(t.position);
        }

        public static bool IsInScreen(Vector3 p)
        {
            return Mathf.Abs(p.x) < Screen.width / 2 && Mathf.Abs(p.y) < Screen.height / 2;
        }


        public static void DestroyObject(UnityEngine.Object obj, float delay = 0.0f)
        {
            if(obj != null)
            {
                if(Application.isPlaying)
                {
                    if(delay <= 0f)
                    {
                        UnityEngine.Object.Destroy(obj);
                    }
                    else
                    {
                        UnityEngine.Object.Destroy(obj, delay);
                    }
                }
                else
                {
                    Debug.Log(obj.name);
                    UnityEngine.Object.DestroyImmediate(obj);
                }
            }
        }
        public static void TransformSortChildsByName(Transform tf, bool AscendingOrder)
        {
            List<Transform> childTransformList = new List<Transform>(tf.childCount);
            for(int i = 0; i < tf.childCount; i++)
            {
                Transform child = tf.GetChild(i);
                childTransformList.Add(child);
            }
            if (AscendingOrder)
            {
                childTransformList.Sort((left, right) => { return left.name.CompareTo(right.name); });
            }
            else
            {
                childTransformList.Sort((left, right) => { return right.name.CompareTo(left.name); });
            }
            for(int i = 0; i < childTransformList.Count; i++)
            {
                childTransformList[i].SetSiblingIndex(i);
            }
        }
    }
}
