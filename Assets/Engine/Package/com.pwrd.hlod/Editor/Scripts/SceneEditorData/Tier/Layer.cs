using System;
using System.Collections.Generic;

namespace com.pwrd.hlod.editor
{
    [Serializable]
    public class Layer
    {
        public int index;
        public List<Cluster> clusters = new List<Cluster>();
        
        //override
        public bool useOverrideSetting = false;
        public bool firstChangeOverrideState = true;
        public LayerSetting layerSetting = new LayerSetting();
        
        public bool ignoreGenerator;

        public void UpdateIgnoreGenerator(bool ignoreGenerator)
        {
            this.ignoreGenerator = ignoreGenerator;
            foreach (var cluster in clusters)
            {
                cluster.UpdateIgnoreGenerator(ignoreGenerator);
            }
        }
    }

}