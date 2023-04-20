using System;
using System.Collections.Generic;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using UnityEngine;

namespace com.pwrd.hlod.editor
{
    [Serializable]
    public class SceneNode
    {
        public string sceneName;
        public string scenePath;
        public GameObject targetParent;
        public List<GameObject> roots = new List<GameObject>();
        public List<Layer> layers = new List<Layer>();
        // public List<HLODResultData> resultList = new List<HLODResultData>();
        public List<HLODDecalTag> decalTagList = new List<HLODDecalTag>();
        
        //override
        public bool useOverrideSetting = false;
        public bool firstChangeOverrideState = true;
        public SceneSetting sceneSetting = new SceneSetting();
        
        public bool ignoreGenerator;

        public Scene scene
        {
            get
            {
                var s = SceneManager.GetSceneByPath(scenePath);
                if (!s.isLoaded)
                {
                    s = EditorSceneManager.OpenScene(scenePath);
                }
                return s;
            }
        }

        public void UpdateIgnoreGenerator(bool ignoreGenerator)
        {
            this.ignoreGenerator = ignoreGenerator;
            foreach (var layer in layers)
            {
                layer.UpdateIgnoreGenerator(ignoreGenerator);
            }
        }
    }
}