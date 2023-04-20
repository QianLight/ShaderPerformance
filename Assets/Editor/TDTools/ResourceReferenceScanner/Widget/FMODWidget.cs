using FMODUnity;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace TDTools.ResourceScanner {
    public class FMODWidget : InspectorElement {
        

        public FMODWidget(ResourceReferenceInspector owner, Vector2 position, string FMODPath) : base(owner, position, FMODPath) {
            TitleLabel.text = FMODPath;
            ID = FMODPath;
            InspectorNodeType = NodeType.FMOD;
        }

        public override void BuildContent() {
            Container.Clear();

            Button buttonPlay = new Button();
            buttonPlay.text = "开始播放";
            buttonPlay.clicked += () => {
                if (ID == "")
                    return;

                EditorEventRef evt = EventManager.EventFromPath(ID);
                if (evt == null) return;
                var para = new Dictionary<string, float>();
                foreach (EditorParamRef paramRef in evt.Parameters) {
                    para[paramRef.name.Substring(paramRef.name.LastIndexOf("/") + 1)] = paramRef.Default;
                }
                FMODUnity.EditorUtils.PreviewEvent(evt, para);
                
            };
            Container.Add(buttonPlay);

            Button buttonStop = new Button();
            buttonStop.text = "停止播放";
            buttonStop.clicked += () => {
                FMODUnity.EditorUtils.PreviewStop();
            };
            Container.Add(buttonStop);
        }

        public override void Close() {
            base.Close();
            FMODUnity.EditorUtils.PreviewStop();
        }
    }
}