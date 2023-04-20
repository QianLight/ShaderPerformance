using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace TDTools.ResourceScanner {
    public class ImageWidget : AssetWidget {
        public ImageWidget(ResourceReferenceInspector owner, Vector2 position, string imagePath) : base(owner, position, imagePath) {
            TitleLabel.text = imagePath.Substring(imagePath.LastIndexOf('/') + 1, imagePath.LastIndexOf('.') - imagePath.LastIndexOf('/') - 1);
            InspectorNodeType = NodeType.Texture2D;
        }

        public override void BuildContent() {
            Container.Clear();
            Image image = new Image();
            Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>($"Assets/{ID}");
            image.image = texture;
            image.scaleMode = ScaleMode.ScaleToFit;
            Container.Add(image);
        }
    }
}