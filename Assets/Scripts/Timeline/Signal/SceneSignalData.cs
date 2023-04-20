using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CFEngine
{
    public class SceneSignalData : MonoBehaviour
    {
        public GameObject _EditorScene;
        public bool _IsLoadSameScene = false;
        public string m_SceneName;

        public bool LoadScene_TryCheckIsSameScene(string sceneName)
        {
            m_SceneName = sceneName;
            Scene scene = SceneManager.GetSceneByName(sceneName);

            _IsLoadSameScene = scene.IsValid();

            return _IsLoadSameScene;
        }

        public void HiddenSceneRenders(bool isHidden)
        {
            if (!isHidden) return;

            _EditorScene = GameObject.Find("EditorScene");

            if (_EditorScene)
                _EditorScene.SetActive(false);
        }
        
        
        public bool UnLoadScene_TryCheckIsSameScene()
        {
            if (_IsLoadSameScene)
            {
                _IsLoadSameScene = false;
                return true;
            }
        
            return false;
        }

        public bool UnLoadScene_CheckLastSceneNameSame(string sceneName)
        {
            if (string.IsNullOrEmpty(m_SceneName)) return false;
            
            return m_SceneName == sceneName;
        }

        public void ShowSceneRenders()
        {
            if(_EditorScene==null) return;
            
            _EditorScene.SetActive(true);
            _EditorScene = null;
        
        }

        public void Clear()
        {
            _EditorScene = null;
            _IsLoadSameScene = false;
            m_SceneName = "";
        }
    }
}
