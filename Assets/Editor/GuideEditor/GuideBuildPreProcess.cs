using System;
using System.Collections.Generic;
using System.IO;
using CFEngine;
using CFEngine.Editor;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;


namespace CFEngine.Editor
{

    public partial class BuildGuide : PreBuildPreProcess
    {
        public override string Name { get { return "Guide"; } }

        public override int Priority
        {
            get
            {
                return 5;
            }
        }
        public override void PreProcess()
        {
            base.PreProcess();
            ProcessFolder("guide", "guidelist");
        }

    }
}
