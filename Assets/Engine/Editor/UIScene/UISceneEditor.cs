using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngineEditor = UnityEditor.Editor;

namespace CFEngine.Editor
{
    [CustomEditor (typeof (UIScene))]
    public class UISceneEditor : UnityEngineEditor
    {


        private UIScene uiScene;
        private Transform uiSceneTf;
        private void OnEnable()
        {
            uiScene = target as UIScene;
            uiSceneTf = uiScene.transform;
        }

        private Transform cameraTf;
        private void GetDefaultCamera()
        {
            cameraTf = uiSceneTf.Find("Camera");

            if (cameraTf == null)
            {
                GameObject newObj = new GameObject("Camera");
                cameraTf = newObj.transform;
                cameraTf.SetParent(uiSceneTf);
                cameraTf.SetAsFirstSibling();
                SetCameraDefault();
            }
        }

        private void SetCameraDefault()
        {
            cameraTf.localScale = Vector3.one;
            cameraTf.position = new Vector3(0, -1000, 0);
            cameraTf.rotation = Quaternion.identity;
        }

        public override void OnInspectorGUI()
        {

            if (GUILayout.Button("Camera Show"))
            {
                GetDefaultCamera();

                if (Camera.main.IsEmpty()) return;

                Camera.main.transform.position = cameraTf.position;
                Camera.main.transform.rotation = cameraTf.rotation;
                
                if(uiScene.IsNoRT) return;
                
                Selection.activeGameObject = Camera.main.gameObject;

                UniversalAdditionalCameraData mainUacd = Camera.main.GetComponent<UniversalAdditionalCameraData>();
                if (mainUacd.IsEmpty()) return;

                Canvas canvas = GameObject.FindObjectOfType<Canvas>();
                if (canvas.IsEmpty() || canvas.worldCamera.IsEmpty()) return;

                Selection.activeGameObject = canvas.worldCamera.gameObject;

                UniversalAdditionalCameraData cameraUGUIUacd =
                    canvas.worldCamera.GetComponent<UniversalAdditionalCameraData>();
                if (cameraUGUIUacd == null) return;

                mainUacd.renderType = CameraRenderType.Base;
                mainUacd.cameraStack.Clear();



                Urp3DUIBackGround urp3DUIBackGround = GameObject.FindObjectOfType<Urp3DUIBackGround>();
                if (urp3DUIBackGround.IsNotEmpty())
                {
                    urp3DUIBackGround.Awake();
                    urp3DUIBackGround.OnEnable();
                }

                UrpCameraStackTag cameraUGUI = canvas.worldCamera.gameObject.GetComponent<UrpCameraStackTag>();
                if (cameraUGUI.IsEmpty())
                {
                    cameraUGUI = canvas.worldCamera.gameObject.AddComponent<UrpCameraStackTag>();
                }

                cameraUGUIUacd.renderType = CameraRenderType.Base;


                cameraUGUI.tag = UrpCameraTag.UGUI;
                cameraUGUI.Awake();

                UrpCameraStackTag cameraUIScene = Camera.main.gameObject.GetComponent<UrpCameraStackTag>();

                if (cameraUIScene.IsEmpty())
                {
                    cameraUIScene = Camera.main.gameObject.AddComponent<UrpCameraStackTag>();
                }

                cameraUIScene.tag = UrpCameraTag.UIScene;
                cameraUIScene.Awake();


                Selection.activeGameObject = uiSceneTf.gameObject;
            }


            if (GUILayout.Button("Camera DefaultPos"))
            {
                GetDefaultCamera();
                SetCameraDefault();
            }


            if (GUILayout.Button("Save"))
            {
                PrefabUtility.ApplyPrefabInstance(uiScene.gameObject, InteractionMode.UserAction);
                Save(uiScene);
            }


            uiScene.IsNoRT = GUILayout.Toggle(uiScene.IsNoRT, "不启用Rt");
        }

        public static void Save(UIScene uiScene)
        {
            try
            {
                string path = string.Format("{0}/UI/OPsystemprefab/UIScene/{1}.prefab",
                    AssetsConfig.instance.ResourcePath,
                    uiScene.name);

                GameObject newObject = GameObject.Instantiate(uiScene.gameObject);
                newObject.name = uiScene.gameObject.name;

                Transform canvasTf = newObject.transform.Find("Canvas");
                if (canvasTf != null)
                    DestroyImmediate(canvasTf.gameObject);


                List<Transform> previewTransforms = EditorRolePreview.FindUIScenePreviewRoles(newObject);
                foreach (Transform previewTransform in previewTransforms)
                    DestroyImmediate(previewTransform.gameObject);

                PrefabUtility.SaveAsPrefabAsset(newObject, path);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                DestroyImmediate(newObject);
            }
            catch (Exception e)
            {
                DebugLog.AddErrorLog(e.StackTrace);
            }
        }

        [MenuItem (@"Assets/Tool/UI_SaveAllUIScene")]
        private static void UI_SaveAllUIScene ()
        {
            CommonAssets.enumPrefab.cb = (prefab, path, context) =>
            {
                GameObject go = PrefabUtility.InstantiatePrefab (prefab) as GameObject;
                go.hideFlags = HideFlags.DontSave;
                UIScene uiScene = go.GetComponent<UIScene> ();
                if (uiScene != null)
                {
                    Save (uiScene);
                }
                GameObject.DestroyImmediate (go);
            };
            CommonAssets.EnumAsset<GameObject> (CommonAssets.enumPrefab, "SaveAllUIScene", string.Format("{0}UIScene", LoadMgr.singleton.editorResPath));

        }
    }

    public partial class BuildUIScene : PreBuildPreProcess
    {
        public override string Name { get { return "UIScene"; } }
        public override int Priority
        {
            get
            {
                return 1;
            }
        }

        public override void PreProcess ()
        {
            base.PreProcess ();
            //ProcessFolder("scene/uiscene", "uisceneloadlist");
        }
    }
}