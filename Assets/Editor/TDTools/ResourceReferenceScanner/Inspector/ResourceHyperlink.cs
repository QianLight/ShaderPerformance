using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace TDTools.ResourceScanner {

    [System.Serializable]
    public enum NodeType { 
        None,
        XEntitySkill,
        Buff,
        Enemy,
        AI,
        Partner,
        PartnerSkill,
        XEntityPresentation,
        Texture2D,
        BeHit,
        Folder,
        Asset,
        Animation,
        BehaviourTree,
        Map,
        FMOD,
        Scene,
        LevelScript,
        DropObject,
        SkillGraph,
        EmemyModeState,
        EmemyStage
    }

    public class ResourceHyperlink {
        public VisualElement Root;
        ResourceReferenceInspector _inspectorWindow;
        InspectorElement _owner;
        NodeType _type;
        string _id;

        public InspectorElement LinkedTo;
        public Connection Connection;
        public VisualElement Pin;

        public string LinkedID;

        public ResourceHyperlink(InspectorElement owner, NodeType type, string titleText, string linkText, string tooltip, string linkValue = "", string linkedID = "") {
            _type = type;
            _owner = owner;
            _inspectorWindow = ResourceReferenceInspector.Instance;
            LinkedID = linkedID;
            if (linkValue == "")
                _id = linkText;
            else
                _id = linkValue;

            Root = new VisualElement();
            Root.style.flexDirection = FlexDirection.Row;
            Root.style.justifyContent = Justify.SpaceBetween;
            SetBorderColor(Color.gray);
            Label title = new Label(titleText);
            Label link = new Label($"{linkText} ");

            title.style.unityTextAlign = TextAnchor.MiddleLeft;
            if (type == NodeType.Folder) {
                link.style.color = Color.yellow;
            } else if (type == NodeType.Asset || type == NodeType.Texture2D || type == NodeType.Animation) {
                link.style.color = Color.green;
            } else {
                link.style.color = Color.cyan;
            }

            link.style.unityTextAlign = TextAnchor.MiddleLeft;

            VisualElement pinLinkContainer = new VisualElement();
            pinLinkContainer.style.flexDirection = FlexDirection.Row;
            pinLinkContainer.Add(link);

            Image image = new Image();
            image.image = new Texture2D(12, 12);
            image.style.marginLeft = 2f;
            image.style.marginRight = 2f;
            image.scaleMode = ScaleMode.ScaleToFit;
            Pin = image;
            pinLinkContainer.Add(image);

            Root.Add(title);
            Root.Add(pinLinkContainer);

            Root.RegisterCallback<MouseEnterEvent>(e => {
                ContextToolTip.Instance.SetContent(new Label(tooltip));
                ContextToolTip.Instance.Show();
                owner.CurrnetMouseOver = _id;
                owner.CurrentColumn = titleText;
                SetBorderColor(Color.yellow);
            });

            Root.RegisterCallback<MouseLeaveEvent>(e => {
                ContextToolTip.Instance.Hide();
                SetBorderColor(Color.gray);
                owner.CurrnetMouseOver = "";
            });


            link.RegisterCallback<PointerDownEvent>(e => {
                if (e.button == 0) {
                    CreateElement();
                }
            });
        }

        void CreateElement() {
            if (LinkedTo != null) {
                _inspectorWindow.SelectNode(LinkedTo);
                return;
            }
            InspectorElement node;
            Vector2 position = Root.worldBound.position + Root.worldBound.size / 2 + new Vector2(5, 5) - _inspectorWindow.Offsets;
            node = InspectorElement.CreateElement(_inspectorWindow, _type, position, _id, LinkedID);
            if (node == null)
                return;
            LinkedTo = node;
            node.LinkedFrom = this;
            (Pin as Image).tintColor = Color.yellow;
            (node.Pin as Image).tintColor = Color.yellow;
            Connection = new Connection(this, node);
            node.ConnectionFrom = Connection;
            _owner.Connections.Add(Connection);
            _inspectorWindow.AddConnection(Connection);
            _inspectorWindow.AddNode(node);
        }

        void SetBorderColor(Color color) {
            Root.style.borderTopWidth = 1f;
            Root.style.borderTopColor = color;
            Root.style.borderBottomWidth = 1f;
            Root.style.borderBottomColor = color;
        }
    }
}