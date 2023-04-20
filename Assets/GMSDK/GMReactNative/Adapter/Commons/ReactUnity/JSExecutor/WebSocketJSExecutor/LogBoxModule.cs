using System.Collections;
using UnityEngine;

namespace GSDK.RNU
{
    public class LogBoxModule : SimpleUnityModule
    {
        private static string NAME = "LogBox";
        private static string LogRootName = "LogBox";
        private readonly RNUMainCore rnuContext;

        private GameObject logBoxGo; 

        public override string GetName()
        {
            return NAME;
        }

        public LogBoxModule(RNUMainCore context)
        {
            rnuContext = context;
        }
        
        [ReactMethod]
        public void show()
        {
            if (logBoxGo == null)
            {
                StaticCommonScript.StaticStartCoroutine(InitLogBox());
                return;
            }
            
            logBoxGo.SetActive(true);
        }
        
        private IEnumerator InitLogBox()
        {
            yield return null;
            rnuContext.RunApplication(LogRootName, "");
        }

        [ReactMethod]
        public void hide() {
            var go = GameObject.Find(LogRootName);
            if (go == null) return;

            if (logBoxGo == null)
            {
                logBoxGo = go;
            }
            
            logBoxGo.SetActive(false);
        }
        
    }
}