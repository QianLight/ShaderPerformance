using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CFEngine
{
    public class SceneLoadMgr : MonoBehaviour
    {

        void Awake()
        {
            SceneManager.activeSceneChanged += ActiveSceneChanged;
        }

        void ActiveSceneChanged(Scene scene1, Scene scene2)
        {
#if UNITY_EDITOR
            Debug.Log("ActiveSceneChanged  scene1:" + scene1.name + "  scene2:" + scene2.name);
#endif
            LightmapVolumn.LoadRenderLightmaps();
            LightProbes.Tetrahedralize();
        }

        void OnDestroy()
        {
            SceneManager.activeSceneChanged -= ActiveSceneChanged;
        }
    }
}
