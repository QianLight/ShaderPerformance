using UnityEditor;

namespace XEditor
{
    [CustomEditor(typeof(LodData))]
    public class LodDataEditor : XDataEditor<LodData>
    {
        internal const string dataPat = "Assets/Editor/LOD/LodData.asset";

       // [MenuItem("Assets/LodData")]
        static void CreateLodData()
        {
            CreateAsset(dataPat);
        }

        private void OnEnable()
        {
            string iconPat = "Assets/Editor/LOD/ico.png";
            base.Init(iconPat, "Lod Data");
        }

        protected override void InnerGUI()
        {
            LodNode action = null;
            if (Data.nodes != null)
            {
                for (int i = 0; i < Data.nodes.Length; i++)
                {
                    var node = Data.nodes[i];
                    if (node.Match(search))
                    {
                        var op = node.GUI();
                        if (op == LodOP.DELETE)
                        {
                            Data.nodes = XEditorUtil.Remv<LodNode>(Data.nodes, i);
                            break;
                        }
                        else if (op == LodOP.DETAIL)
                        {
                            action = node;
                        }
                    }
                }
            }
            if (action != null) OpenLodWin(action);
        }
        

        protected override void OnAdd()
        {
            XEditorUtil.Add<LodNode>(ref odData.nodes, new LodNode("role"));
        }

        protected override void OnSave()
        {
            odData.Save();
        }

        private void OpenLodWin(LodNode lod)
        {
            if (LodUtil.MakeLodEnv())
            {
                var win = LodWindow.LodShow();
                win.LoadRole(lod);
                win.UpdateInfo();
            }
        }
    }

}