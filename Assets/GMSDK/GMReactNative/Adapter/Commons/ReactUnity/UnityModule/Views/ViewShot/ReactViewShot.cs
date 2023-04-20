using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;
using Object = UnityEngine.Object;
using Random = System.Random;

namespace GSDK.RNU
{
    public class ReactViewShot : SimpleBaseView
    {
        private GameObject realGameObject; // 挂在原有节点下
        private GameObject rootObject; // 新建一个根节点，用于指定摄像机
        private GameObject cameraObject; // 摄像机Object
        private GameObject rootViewObject; // 挂在根节点rootObject下，用于设置ShotView的宽高和位置
        private Camera cam; // 新建摄像机
        private Rect shotRect; // 截图区域

        public ReactViewShot(string name)
        {
            realGameObject = new GameObject(name);
            rootObject = new GameObject(name + "Shot");
            rootViewObject = new GameObject(name + "ShotView");
            rootViewObject.transform.SetParent(rootObject.transform, false);
            cameraObject = new GameObject(name + "Camera");
            SetCamera();
            SetActive(false);
        }

        /*
         * 新建相机cam
         * cam深度指定为-99，不渲染到屏幕
         * cam设置为正交投影
         * rootObject添加Canvas，并指定渲染相机为cam
         */
        private void SetCamera()
        {
            cam = cameraObject.AddComponent<Camera>();
            cam.depth = -99;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.orthographic = true;
            Canvas canvas = rootObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceCamera;
            canvas.worldCamera = cam;
        }

        public override GameObject GetGameObject()
        {
            return realGameObject;
        }

        public override GameObject GetContentObject()
        {
            return rootViewObject;
        }

        public override void Destroy()
        {
            base.Destroy();
            Object.Destroy(rootObject);
            Object.Destroy(cameraObject);
            Object.Destroy(rootViewObject);
            rootObject = null;
            cameraObject = null;
            rootViewObject = null;
        }

        private void SetActive(bool active)
        {
            rootObject.SetActive(active);
            cameraObject.SetActive(active);
            rootViewObject.SetActive(active);
        }

        public void SetCameraDepth(float depth)
        {
            cam.depth = depth;
        }
        
        public void SetCameraPosition(Dictionary<string, object> position)
        {
            var x = Convert.ToSingle(position["x"]);
            var y = Convert.ToSingle(position["y"]);
            var z = Convert.ToSingle(position["z"]);
            cam.transform.position = new Vector3(x, y, z);
        }
        
        public void SetCameraRotation(Dictionary<string, object> rotation)
        {
            var x = Convert.ToInt32(rotation["x"]);
            var y = Convert.ToInt32(rotation["y"]);
            var z = Convert.ToInt32(rotation["z"]);
            cam.transform.rotation = Quaternion.Euler(x, y, z);
        }

        /*
         * 设置shotView的宽高
         * 设置shotView的位置为左下角
         * 设置截shotRect的目标区域为shotView
         */
        public override void SetLayout(int x, int y, int width, int height)
        {
            GameObject panel = GetContentObject();
            RectTransform rectTransform = panel.GetComponent<RectTransform>();
            if (rectTransform == null)
            {
                rectTransform = panel.AddComponent<RectTransform>();
            }
            
            rectTransform.anchorMin = new Vector2(0, 0);
            rectTransform.anchorMax = new Vector2(0, 0);
            rectTransform.offsetMin = new Vector2(0, 0);
            rectTransform.offsetMax = new Vector2(width, height);
            rectTransform.pivot = new Vector2(0.5F, 0.5F);
            shotRect = new Rect(0, 0, width, height);
        }

        private string ScreenNameGenerate()
        {
            long cur = DateTime.Now.Ticks / 1000;
            Random random = new Random((int) cur);
            int num = random.Next(100000, 999999);

            return cur.ToString() + "_" + num.ToString() + ".png";
        }

        /*
         * 使用新建的相机进行截图
         */
        public IEnumerator CaptureCamera(Promise promise)
        {
            SetActive(true);
            yield return null;
            if (rootObject == null)
            {
                yield break;
            }
            RectTransform rectTransform = rootObject.GetComponent<RectTransform>();
            Rect reanderRect = new Rect(0, 0, rectTransform.sizeDelta.x, rectTransform.sizeDelta.y);
            // 创建一个RenderTexture对象，大小为rootObject的canvas的大小  
            RenderTexture rt = new RenderTexture((int) reanderRect.width, (int) reanderRect.height, 0);
            GameExtraOp(cam, rt, (int)reanderRect.width, (int)reanderRect.height);
            // 临时设置相关相机的targetTexture为rt, 并手动渲染相关相机
            cam.targetTexture = rt;
            cam.Render();
            // 激活这个rt, 并从中中读取像素。
            RenderTexture.active = rt;
            Texture2D screenShot =
                new Texture2D((int) shotRect.width, (int) shotRect.height, TextureFormat.RGBA32, false);
            screenShot.ReadPixels(shotRect, 0, 0); // 从rt中读取像素
            screenShot.Apply();
            
            byte[] bytes = screenShot.EncodeToPNG();
            string filePath = "/" + ScreenNameGenerate();
            string filename = Application.persistentDataPath + filePath;
            File.WriteAllBytes(filename, bytes);
            
            cam.targetTexture = null;
            RenderTexture.active = null;
            Object.Destroy(rt);
            Object.Destroy(screenShot);
            SetActive(false);
            
            promise.Resolve(filename);
        }
        
        //TODO 不应依赖游戏的逻辑
        //Hgame 需要的额外操作，
        private void GameExtraOp(Camera cam, RenderTexture rt, int width, int height)
        {
            //  LuaHelper.AddUICardSetup(cam, rt, (int)reanderRect.width, (int)reanderRect.height);
            var luaHelperType = Type.GetType("LuaHelper");
            if (luaHelperType != null)
            {
                var method = luaHelperType.GetMethod("AddUICardSetup", new Type[] { cam.GetType(), rt.GetType(), typeof(int), typeof(int) });

                if (method != null)
                {
                    var parameters = new object[] { cam, rt, width, height };
                    method.Invoke(null, parameters);
                }
            }
        }
        
    }
}