/********************************************************************
	created:	2021/06/25  18:58
	file base:	Urp3DUIBackGround
	author:		c a o   f e n g
	
	purpose:	获取ugui上面设置的背景图片 传到shader全局参数进行使用
*********************************************************************/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.CFUI;

namespace CFEngine
{
    public class Urp3DUIBackGround : MonoBehaviour
    {
        public static int _BackGround = Shader.PropertyToID("_3DUIBackGround");
        private static string keyName = "_3DUIBACKGROUND";

        private CFRawImage BackgroundImage;

        public Texture backTexture;
        public bool renderPostProcessing = true;//是否启动相机后期



        public void Awake()
        {
            BackgroundImage = GetComponent<CFRawImage>();
        }

        public void OnEnable()
        {

            if (backTexture != null)
            {
                UrpCameraStackTag.renderPostProcessingWith3DUI = renderPostProcessing;
                Shader.EnableKeyword(keyName);
                Shader.SetGlobalTexture(_BackGround, backTexture);
                BackgroundImage.enabled = false;
            }

        }


        public void OnDisable()
        {
            if (backTexture != null)
            {
                UrpCameraStackTag.renderPostProcessingWith3DUI = false;
                BackgroundImage.enabled = true;
                Shader.DisableKeyword(keyName);
            }
        }


    }
}
