using System;
using System.Collections.Generic;
using CFEngine;
using UnityEditor;
using UnityEngine;
using UnityEngine.CFUI;
using UnityEngine.SceneManagement;

namespace CFEngine.Editor
{
    public partial class AssetsConfigTool : BaseConfigTool<AssetsConfig>
    {
        public override void OnInit ()
        {
            base.OnInit ();
            config = AssetsConfig.instance;
            InitConst ();
            InitMeshTex ();
        }

        protected override void OnConfigGui (ref Rect rect)
        {
            MeshTexGUI (ref rect);
            ConstValuesGUI ();
        }
    }
}