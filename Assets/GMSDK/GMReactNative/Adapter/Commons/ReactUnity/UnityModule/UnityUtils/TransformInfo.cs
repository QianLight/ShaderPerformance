using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GSDK.RNU
{
    public class TransformInfo
    {
        // x, y, z
        public Vector3 Rotate;
        public Vector3 Translate;
        public Vector3 Scale;

        private TransformInfo(Vector3 r, Vector3 t, Vector3 s)
        {
            Rotate = r;
            Translate = t;
            Scale = s;
        }

        public static TransformInfo InitTransformInfoByJsList(ArrayList jsList)
        {
            if (jsList.Count == 0)
            {
                return null;
            }

            var ti = new TransformInfo(Vector3.zero, Vector3.zero, Vector3.one);
            // 由于现在是围绕中心点 做平移，旋转，缩放，故和执行顺序无关，可以如此。
            // TODO 当不是围绕中心点的时候， 就和执行顺序有关，不可如此。
            foreach (Dictionary<string, object> item in jsList)
            {
                // translate
                if (item.ContainsKey("translateX"))
                {
                    ti.Translate.x = Convert.ToSingle(item["translateX"]);
                    continue;
                }

                if (item.ContainsKey("translateY"))
                {
                    ti.Translate.y = Convert.ToSingle(item["translateY"]);
                    continue;
                }

                // scale
                if (item.ContainsKey("scale"))
                {
                    ti.Scale.y = ti.Scale.x = Convert.ToSingle(item["scale"]);
                    continue;
                }

                if (item.ContainsKey("scaleX"))
                {
                    ti.Scale.x = Convert.ToSingle(item["scaleX"]);
                    continue;
                }

                if (item.ContainsKey("scaleY"))
                {
                    ti.Scale.y = Convert.ToSingle(item["scaleY"]);
                    continue;
                }

                // rotate
                if (item.ContainsKey("rotate"))
                {
                    ti.Rotate.z = JsValueToDegree((string) item["rotate"]);
                    continue;
                }

                if (item.ContainsKey("rotateX"))
                {
                    ti.Rotate.x = JsValueToDegree((string) item["rotateX"]);
                    continue;
                }

                if (item.ContainsKey("rotateY"))
                {
                    ti.Rotate.y = JsValueToDegree((string) item["rotateY"]);
                    continue;
                }

                if (item.ContainsKey("rotateZ"))
                {
                    ti.Rotate.z = JsValueToDegree((string) item["rotateZ"]);
                    continue;
                }
            }

            return ti;
        }

        // 注意这里取负，因为方向相反
        private static float JsValueToDegree(string jsV)
        {
            if (jsV.EndsWith("deg"))
            {
                var deg = jsV.Substring(0, jsV.Length - 3);
                Util.Log("deg: {0}", Convert.ToSingle(deg));
                return -Convert.ToSingle(deg);
            }

            if (jsV.EndsWith("rad"))
            {
                var rad = jsV.Substring(0, jsV.Length - 3);
                //  1RAD = rad *  180 / PI
                return -Convert.ToSingle(Convert.ToSingle(rad) *  (180 / Math.PI));
            }

            return 0;
        }
    }
}