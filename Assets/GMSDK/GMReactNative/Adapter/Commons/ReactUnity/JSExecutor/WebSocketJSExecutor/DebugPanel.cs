using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace GSDK.RNU
{
    public class DebugPanel
    {

        private static readonly Regex AnsiRegex = new Regex("\\[[0-9]+m");
        private static readonly string DefaultTip = "Bundling: 0/0";
        private bool enableHot = true;

        private GameObject debugRoot;
        private Text tipText;
        private GameObject tipContainer;
        private GameObject errorContainer;
        private RawImage debugRootRi;
        private GameObject closeBtn;
        private RNUMainCore rnuContent;

        
        public DebugPanel(RNUMainCore content)
        {
            rnuContent = content;
            
            InitDebugUi();
        }

        public void Destory()
        {
            Object.Destroy(debugRoot);
        }
        
        public void ReloadJs()
        {
            tipContainer.SetActive(true);
            debugRootRi.raycastTarget = true;
            StaticCommonScript.StaticStartCoroutine(DownLoadJsBundle());
        }

        public void ShowError(string errorMsg)
        {
            errorContainer.SetActive(true);
            debugRootRi.raycastTarget = true;
            closeBtn.SetActive(true);
            var errorText = errorContainer.GetComponentInChildren<Text>();
            
            errorText.text = "红屏：错误修复之后，请点击右下角Reload刷新！" + "\n\n" + errorMsg;
        }

        public void HideError()
        {
            errorContainer.SetActive(false);
            debugRootRi.raycastTarget = false;
            closeBtn.SetActive(false);
            var errorText = errorContainer.GetComponentInChildren<Text>();
            errorText.text = "";
        } 
        
        private IEnumerator DownLoadJsBundle()
        {
            var form = new WWWForm();
            var uwr = UnityWebRequest.Post("http://"+ rnuContent.GetDebugIp() +":" + rnuContent.GetDebugPort() +  "/index.bundle?platform=android&dev=true&minify=false&app=com.rnlearn2&modulesOnly=false&runModule=true", form);
            uwr.SetRequestHeader("Accept", "multipart/mixed");
            uwr.downloadHandler = new BundleDownHandler(BundleProgressHandler, BundleCompleteHandler, BundleErrorHandler);
            yield return uwr.SendWebRequest();
        }
        
        private void BundleProgressHandler(BundleChunkInfo info, Dictionary<string, string> headers)
        {
            tipText.text = "Bundling: " + info.done + "/" + info.total;
        }

        private void BundleErrorHandler(BundleErrorChunkInfo errorInfo)
        {

            var errorMessage = AnsiRegex.Replace(errorInfo.message, "")
                .Replace(" ", "  ");
            ShowError(errorMessage);
        }

        private void BundleCompleteHandler(long hasDown, long total)
        {
            if (hasDown == total)
            {
                tipText.text = "Downloading: 100%";
                tipContainer.SetActive(false);
                debugRootRi.raycastTarget = false;
                return;
            }
            
            // 保证2位精度
            // ReSharper disable once PossibleLossOfFraction
            var percent = (hasDown * 10000 / total) / 100f + "%";
            
            tipText.text = "Downloading: " + percent;
        }
        
        private void InitDebugUi()
        {
            debugRoot = new GameObject("DebugRoot");
            var rectTransform = debugRoot.AddComponent<RectTransform>();

            debugRootRi = debugRoot.AddComponent<RawImage>();
            debugRootRi.color = Color.clear;
            
            var pageParentGo = RNUMainCore.GetPageParentGo();
            var pageParentRect = RNUMainCore.GetGameGoParentRect();
            var h = pageParentRect == Rect.zero ? Screen.height : pageParentRect.height;
            var w = pageParentRect == Rect.zero ? Screen.width : pageParentRect.width;
            
            SetXYWHLayout(debugRoot, 0, 0, w, h);
            
            rectTransform.SetParent(pageParentGo.transform, false);
            
            // Top Text
            var tipTextGo = DefaultControls.CreateText(new DefaultControls.Resources());
            tipText = tipTextGo.GetComponent<Text>();
            tipText.text = DefaultTip;
            tipText.alignment = TextAnchor.MiddleCenter;
            tipText.color = Color.white;
            tipText.fontStyle = FontStyle.Italic;
            tipText.alignByGeometry = true;
            tipText.fontSize = (int) ((w / 1280) * 30);
            
            var tRectTransform = tipTextGo.GetComponent<RectTransform>();
            SetXYWHLayout(tipTextGo, 0, 0 , w, tipText.fontSize * 2);
            // Top Text container
            tipContainer = new GameObject();
            var tcImage = tipContainer.AddComponent<Image>();
            tcImage.color = Color.gray;
            SetXYWHLayout(tipContainer, 0, 0, w, tipText.fontSize * 2);
            var tipContainerRectTransform = tipContainer.GetComponent<RectTransform>();
            tRectTransform.SetParent(tipContainerRectTransform.transform, false);
            tipContainerRectTransform.SetParent(debugRoot.transform, false);
            
            
            // error msg
            var errorTextGo = DefaultControls.CreateText(new DefaultControls.Resources());
            var errorText = errorTextGo.GetComponent<Text>();
            errorText.text = "";
            errorText.alignment = TextAnchor.UpperLeft;
            errorText.color = Color.white;
            errorText.fontStyle = FontStyle.Bold;
            errorText.alignByGeometry = true;
            errorText.fontSize = (int) ((w / 1280) * 30);
            SetXYWHLayout(errorTextGo, 40, 40, w - 80, h - 40);
            // error container
            errorContainer = new GameObject();
            var ecImage = errorContainer.AddComponent<Image>();
            ecImage.color = Color.red;
            SetXYWHLayout(errorContainer, 0, 0, w,  h);
            AppendChild(debugRoot, errorContainer);
            AppendChild(errorContainer, errorTextGo);
            errorContainer.SetActive(false);

            // hot-reload
            var hotReloadToggle = DefaultControls.CreateToggle(new DefaultControls.Resources());

            SetXYWHLayout(hotReloadToggle, w - 100, h - 90, 50, 50);
            AppendChild(debugRoot, hotReloadToggle);
            var hotText = hotReloadToggle.GetComponentInChildren<Text>();
            hotText.text = "Hot";
            var hotToggle = hotReloadToggle.GetComponent<Toggle>();
            hotToggle.graphic.color = Color.green;
            hotToggle.onValueChanged.AddListener(EnableHotReload);
            
            
            
            // reload
            var refreshBtn = DefaultControls.CreateButton(new DefaultControls.Resources());
            SetXYWHLayout(refreshBtn, w - 100, h - 150, 50, 50);
            
            AppendChild(debugRoot, refreshBtn);
            var btn = refreshBtn.GetComponent<Button>();
            btn.onClick.AddListener(ReloadHandle);
            var btnImage = refreshBtn.GetComponent<Image>();
            btnImage.color = Color.green;
            var refreshText = refreshBtn.GetComponentInChildren<Text>();
            refreshText.text = "Reload";
            
            // close btn
            closeBtn = DefaultControls.CreateButton(new DefaultControls.Resources());
            closeBtn.SetActive(false);
            SetXYWHLayout(closeBtn, w - 100, tipText.fontSize * 2, 50, 50);
            
            AppendChild(debugRoot, closeBtn);
            var cbtn = closeBtn.GetComponent<Button>();
            cbtn.onClick.AddListener(() =>
            {
                HideError();
                RNUMain.Close();
            });
            var closeBtnImage = closeBtn.GetComponent<Image>();
            closeBtnImage.color = Color.green;
            var closeText = closeBtn.GetComponentInChildren<Text>();
            closeText.text = "关闭";
            
        }


        public void ReloadHandle()
        {
            tipText.text = DefaultTip;
            HideError();

            // do some clear and restart
            this.rnuContent.RestartJs(enableHot);
        }

        private void EnableHotReload(bool enable)
        {
            enableHot = enable;
            
            if (enable)
            {
                HMRClient.enable();
            }
            else
            {
                HMRClient.disable();
            }
        }

        private static void SetXYWHLayout(GameObject go, float x, float y, float w, float h)
        {
            var rectTransform = go.GetComponent<RectTransform>();
            rectTransform.anchorMin = new Vector2(0.0F, 1.0F);
            rectTransform.anchorMax = new Vector2(0.0F, 1.0F);
            rectTransform.offsetMin = new Vector2(x, -(y + h));
            rectTransform.offsetMax = new Vector2(x + w, -y);
            rectTransform.pivot = new Vector2(0.5F, 0.5F);
        }

        private static void AppendChild(GameObject parent, GameObject child)
        {
            child.transform.SetParent(parent.transform, false);
        }



    }
}