using CFClient;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace TDTools.ResourceScanner {

    public enum GMCommandType { 
        Client,
        Server
    }

    public class GMButton {
        public VisualElement Root;

        public GMButton(GMCommandType type, string text, string command, string buttonText = "") {
            Root = new VisualElement();
            Label label = new Label(text);
            Button button = new Button();
            if (buttonText == "")
                button.text = command;
            else
                button.text = buttonText;
            button.style.height = 14;
#if USE_GM
            button.clicked += () => {
                if (type == GMCommandType.Server)
                    CFCommand.singleton.ProcessServerCommand(command);
                else
                    CFCommand.singleton.ProcessClientCommand(command);
            };
#endif
            button.style.color = new Color(1f, 0.5f, 0.5f, 1f);
            Root.style.justifyContent = Justify.SpaceBetween;
            Root.style.flexDirection = FlexDirection.Row;
            Root.Add(label);
            Root.Add(button);
            Root.style.borderTopWidth = 1f;
            Root.style.borderTopColor = Color.gray;
            Root.style.borderBottomWidth = 1f;
            Root.style.borderBottomColor = Color.gray;
        }
    }
}