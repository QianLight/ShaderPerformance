using System;
using UnityEngine;

namespace CFEngine
{ 
    public class StaticGaussianBlur: MonoBehaviour
    {
        [SerializeField]private Material GaussianBlurMaterial;
        [SerializeField][Range(0.001f,0.003f)] private float BlurIntensity = 0.003f;

        public enum Mode
         {
             Default,
             AVG
         }
         [SerializeField]private Mode mode = Mode.Default;

         private BlurCore _blurCore;
         private BlurContext _blurContext;

         private void Awake()
         {
             _blurContext = new BlurContext(gameObject, GaussianBlurMaterial, 3);

             if (mode == Mode.Default)
             {
                 _blurCore = new PauseBlur(_blurContext);
             }
             else if (mode == Mode.AVG)
             {
                 _blurCore = new AVGBackgroundBlur(_blurContext);
             }
         }

         private void OnEnable()
         {
             _blurContext.Clear();
             _blurContext.BlurIntensity = BlurIntensity;
             _blurCore.Excecute();
         }


         private void OnDisable()
         {
             _blurCore.OnClear();
             _blurContext.Clear();
         }
    }
}