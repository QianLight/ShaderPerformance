//#if UNITY_EDITOR
//using UnityEditor;
//using UnityEngine;
//using UnityEditor.SceneManagement;
//using UnityEngine.SceneManagement;
//using System.IO;
//using System.Xml.Serialization;
//using CFUtilPoolLib;
//using System;

//namespace XEditor
//{
//    public class XReactDataBuilder : XSingleton<XReactDataBuilder>
//    {
//        public static GameObject hoster = null;
//        public static DateTime Time;
//        public static string prefixPath = "";

//        public void Load(string pathwithname)
//        {
//            try
//            {
//                XReactConfigData conf = XDataIO<XReactConfigData>.singleton.DeserializeData(XEditorPath.GetCfgFromSkp(pathwithname));
//                GameObject prefab = XAnimationLibrary.GetDummy((uint)conf.Player);

//                if (prefab == null)
//                {
//                    Debug.Log("<color=red>Prefab not found by id: " + conf.Player + "</color>");
//                }

//                ColdBuild(prefab, conf);

//                prefixPath = pathwithname.Substring(0, pathwithname.IndexOf("/ReactPackage"));
//                Time = File.GetLastWriteTime(pathwithname);
//            }
//            catch (Exception e)
//            {
//                Debug.Log("<color=red>Error occurred during loading config file: " + pathwithname + " with error " + e.Message + "</color>");
//            }
//        }

//        public void ColdBuild(GameObject prefab, XReactConfigData conf)
//        {
//            if (hoster != null) GameObject.DestroyImmediate(hoster);

//            hoster = UnityEngine.Object.Instantiate(prefab, Vector3.zero, Quaternion.identity) as GameObject;
//            hoster.transform.localScale = Vector3.one * XAnimationLibrary.AssociatedAnimations((uint)conf.Player).Scale;
//            XDestructionLibrary.AttachDress((uint)conf.Player, hoster);
//            hoster.AddComponent<XReactHoster>();

//            CharacterController cc = hoster.GetComponent<CharacterController>();
//            if (cc != null) cc.enabled = false;

//            UnityEngine.AI.NavMeshAgent agent = hoster.GetComponent<UnityEngine.AI.NavMeshAgent>();
//            if (agent != null) agent.enabled = false;

//            XReactHoster component = hoster.GetComponent<XReactHoster>();

//            string directory = conf.Directory[conf.Directory.Length - 1] == '/' ? conf.Directory.Substring(0, conf.Directory.Length - 1) : conf.Directory;
//            string path = XEditorPath.GetPath("ReactPackage" + "/" + directory);

//            component.ConfigData = conf;
//            component.ReactData = XDataIO<XReactData>.singleton.DeserializeData(path + conf.ReactName + ".bytes");

//            component.ReactDataExtra.ScriptPath = path;
//            component.ReactDataExtra.ScriptFile = conf.ReactName;

//            component.ReactDataExtra.ReactClip = RestoreClip(conf.ReactClip, conf.ReactClipName);

//            if (component.ReactData.Time == 0 && component.ReactDataExtra.ReactClip != null)
//                component.ReactData.Time = component.ReactDataExtra.ReactClip.length;

//            HotBuildEx(component, conf);

//            EditorGUIUtility.PingObject(hoster);
//            Selection.activeObject = hoster;
//        }

//        public void HotBuildEx(XReactHoster hoster, XReactConfigData conf)
//        {
//            XReactDataExtra edata = hoster.ReactDataExtra;
//            XReactData data = hoster.ReactData;

//            edata.Fx.Clear();
//            edata.Audio.Clear();

//            if (data.Fx != null)
//            {
//                foreach (XFxData fx in data.Fx)
//                {
//                    XFxDataExtra fxe = new XFxDataExtra();
//                    fxe.Fx = XResourceHelper.LoadEditorResourceAtBundleRes<GameObject>(fx.Fx);
//                    if (fx.Bone != null && fx.Bone.Length > 0)
//                    {
//                        Transform attachPoint = hoster.gameObject.transform.Find(fx.Bone);
//                        if (attachPoint != null)
//                        {
//                            fxe.BindTo = attachPoint.gameObject;
//                        }
//                        else
//                        {
//                            int index = fx.Bone.LastIndexOf("/");
//                            if (index >= 0)
//                            {
//                                string bone = fx.Bone.Substring(index + 1);
//                                attachPoint = hoster.gameObject.transform.Find(bone);
//                                if (attachPoint != null)
//                                {
//                                    fxe.BindTo = attachPoint.gameObject;
//                                }
//                            }

//                        }
//                    }

//                    fxe.Ratio = fx.At / data.Time;
//                    fxe.End_Ratio = fx.End / data.Time;

//                    edata.Fx.Add(fxe);
//                }
//            }

//            if (data.Audio != null)
//            {
//                foreach (XAudioData au in data.Audio)
//                {
//                    XAudioDataExtra aue = new XAudioDataExtra();
//                    aue.audio = XResourceHelper.LoadEditorResourceAtBundleRes<AudioClip>(au.Clip);
//                    aue.Ratio = au.At / data.Time;

//                    edata.Audio.Add(aue);
//                }
//            }

//            if (data.BoneShakeData != null)
//            {
//                foreach (XBoneShakeData fx in data.BoneShakeData)
//                {
//                    XBoneShakeExtra fxe = new XBoneShakeExtra();
//                    if (fx.Bone != null && fx.Bone.Length > 0)
//                    {
//                        Transform attachPoint = hoster.gameObject.transform.Find(fx.Bone);
//                        if (attachPoint != null)
//                        {
//                            fxe.BindTo = attachPoint.gameObject;
//                        }
//                        else
//                        {
//                            int index = fx.Bone.LastIndexOf("/");
//                            if (index >= 0)
//                            {
//                                string bone = fx.Bone.Substring(index + 1);
//                                attachPoint = hoster.gameObject.transform.Find(bone);
//                                if (attachPoint != null)
//                                {
//                                    fxe.BindTo = attachPoint.gameObject;
//                                }
//                            }

//                        }
//                    }

//                    fxe.Ratio = fx.At / data.Time;

//                    edata.BoneShake.Add(fxe);
//                }
//            }

//            if (!string.IsNullOrEmpty(data.AvatarMask))
//            {
//                edata.Mask = RestoreAvatarMask(data.AvatarMask);
//            }
//        }

//        private AvatarMask RestoreAvatarMask(string path)
//        {
//            if (path == null) return null;

//            return AssetDatabase.LoadAssetAtPath("Assets/BundleRes/"+path+".mask", typeof(AvatarMask)) as AvatarMask;
//        }

//        private AnimationClip RestoreClip(string path, string name)
//        {
//            if (path == null || name == null || path == "" || name == "") return null;

//            int last = path.LastIndexOf('.');
//            string subfix = path.Substring(last, path.Length - last).ToLower();

//            if (subfix == ".fbx")
//            {
//                UnityEngine.Object[] objs = AssetDatabase.LoadAllAssetsAtPath(path);
//                foreach (UnityEngine.Object obj in objs)
//                {
//                    AnimationClip clip = obj as AnimationClip;
//                    if (clip != null && clip.name == name)
//                        return clip;
//                }
//            }
//            else if (subfix == ".anim")
//            {
//                return AssetDatabase.LoadAssetAtPath(path, typeof(AnimationClip)) as AnimationClip;

//            }
//            else
//                return null;

//            return null;
//        }

//        public void Update(XReactHoster hoster)
//        {
//            string pathwithname = hoster.ReactDataExtra.ScriptPath + hoster.ConfigData.ReactName + ".txt";

//            DateTime time = File.GetLastWriteTime(pathwithname);

//            if (Time == default(DateTime)) Time = time;

//            if (time != Time)
//            {
//                Time = time;

//                if (EditorUtility.DisplayDialog("WARNING!",
//                                            "Skill has been Modified outside, Press 'OK' to reload file or 'Ignore' to maintain your change. (Make sure the '.config' file for skill script has been well synchronized)",
//                                            "Ok", "Ignore"))
//                {
//                    hoster.ConfigData = XDataIO<XReactConfigData>.singleton.DeserializeData(XEditorPath.GetCfgFromSkp(pathwithname));
//                    hoster.ReactData = XDataIO<XReactData>.singleton.DeserializeData(pathwithname);

//                    HotBuildEx(hoster, hoster.ConfigData);
//                }
//            }
//        }
//    }

//    public class XReactEditor : MonoBehaviour
//    {
//        [MenuItem(@"XEditor/Create React")]
//        static void ReactCreater()
//        {
//            EditorWindow.GetWindow<XReactWindow>(@"React Editor");
//        }

//        [MenuItem(@"XEditor/Open React ")]
//        static void ReactOpener()
//        {
//            XReactWindow.OpenSkill();
//        }

//    }

//    internal class XReactWindow : EditorWindow
//    {
//        private string _name = "default_skill";

//        private int _id = 0;

//        private string _directory = "/";
//        private GameObject _prefab = null;

//        public AnimationClip _skillClip = null;

//        private GUIStyle _labelstyle = null;

        

//        void OnGUI()
//        {
//            if (_labelstyle == null)
//            {
//                _labelstyle = new GUIStyle(EditorStyles.boldLabel);
//                _labelstyle.fontSize = 13;
//            }

//            GUILayout.Label(@"Create and edit a new skill:", _labelstyle);

//            _name = EditorGUILayout.TextField("*Skill Name:", _name);
//            _directory = EditorGUILayout.TextField("*Directory:", _directory);
//            if (_directory.Length > 0 && _directory[_directory.Length - 1] == '/') _directory.Remove(_directory.Length - 1);
//            EditorGUILayout.Space();

//            EditorGUI.BeginChangeCheck();
//            _id = EditorGUILayout.IntField("*Dummy ID", _id);
//            if (EditorGUI.EndChangeCheck())
//            {
//                _prefab = XAnimationLibrary.GetDummy((uint)_id);
//            }

//            EditorGUILayout.ObjectField("*Dummy Prefab:", _prefab, typeof(GameObject), true);

//            AnimationClip skillClip = EditorGUILayout.ObjectField("*Skill Animation:", _skillClip, typeof(AnimationClip), true) as AnimationClip;

//            if (skillClip != null)
//            {
//                string location = AssetDatabase.GetAssetPath(skillClip);
//                if (skillClip == null || location.IndexOf(".anim") != -1)
//                    _skillClip = skillClip;
//                else
//                {
//                    EditorUtility.DisplayDialog("Confirm your selection.",
//                            "Please select extracted clip!",
//                            "Ok");
//                }
//            }

//            EditorGUILayout.Space();

//            EditorGUILayout.BeginHorizontal();
//            if (GUILayout.Button("Done"))
//            {
//                if (_prefab != null)
//                {
//                    Scene scene = EditorSceneManager.GetActiveScene();
//                    //string current = EditorApplication.currentScene;
//                    if (scene.name.Length > 0 && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
//                    {
//                        Close();
//                    }
//                    else
//                    {
//                        EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects);
//                        //EditorApplication.NewScene();
//                        OnSkillPreGenerationDone();
//                        return;
//                    }
//                }
//                else
//                {
//                    EditorUtility.DisplayDialog("Confirm your selection.",
//                        "Please select prefab and clip!",
//                        "Ok");
//                }
//            }
//            else if (GUILayout.Button("Cancel"))
//            {
//                Close();
//            }
//            EditorGUILayout.EndHorizontal();

//            EditorGUILayout.Space();
//            GUILayout.Label("* means this section can not be empty.", EditorStyles.foldout);
//        }

//        private void OnSkillPreGenerationDone()
//        {
//            XReactDataBuilder.singleton.Load(PreStoreData());
//        }

//        private string PreStoreData()
//        {
//            string skp = XEditorPath.GetPath("ReactPackage" + "/" + _directory) + _name + ".bytes";
//            string config = XEditorPath.GetEditorBasedPath("ReactPackage" + "/" + _directory) + _name + ".config";

//            XReactConfigData conf = new XReactConfigData();

//            if (_skillClip != null)
//            {
//                conf.ReactClip = AssetDatabase.GetAssetPath(_skillClip);
//                conf.ReactClipName = _skillClip.name;
//            }
            

//            conf.Player = _id;
//            conf.Directory = _directory;
//            conf.ReactName = _name;

//            XReactData data = new XReactData();
//            data.Name = _name;

//            if (_skillClip != null)
//            {
//                data.ClipName = conf.ReactClip.Remove(conf.ReactClip.LastIndexOf('.'));
//                data.ClipName = data.ClipName.Remove(0, 17);
//            }
                
//            //using (FileStream writer = new FileStream(skp, FileMode.Create))
//            {
//                XDataIO<XReactData>.singleton.SerializeData(skp, data);
//            }

//            using (FileStream writer = new FileStream(config, FileMode.Create))
//            {
//                //IFormatter formatter = new BinaryFormatter();
//                XmlSerializer formatter = new XmlSerializer(typeof(XReactConfigData));
//                formatter.Serialize(writer, conf);
//            }

//            AssetDatabase.Refresh();
//            return skp;
//        }


//        public static void OpenSkill()
//        {
//            string file = EditorUtility.OpenFilePanel("Select react file", XEditorPath.Rtp, "bytes");

//            if (file.Length != 0)
//            {
//                Scene scene = EditorSceneManager.GetActiveScene();
//                //string current = EditorApplication.currentScene;

//                if (!string.IsNullOrEmpty(scene.name) && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
//                {
//                    return;
//                }
//                else
//                {

//                    EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects);

//                    GameObject plane = GameObject.CreatePrimitive(PrimitiveType.Plane);
//                    plane.name = "Ground";
//                    plane.layer = LayerMask.NameToLayer("Terrain");
//                    plane.transform.position = new Vector3(0, -0.01f, 0);
//                    plane.transform.localScale = new Vector3(1000, 1, 1000);
//                    //Material m = new Material(Shader.Find("Diffuse"));
//                    plane.GetComponent<Renderer>().sharedMaterial = new Material(Shader.Find("Diffuse"));
//                    plane.GetComponent<Renderer>().sharedMaterial.SetColor("_Color", new Color(90 / 255.0f, 90 / 255.0f, 90 / 255.0f));

//                    XReactDataBuilder.singleton.Load(file);

//                    DestroyImmediate(GameObject.Find("Main Camera"));
//                    GameObject camera = AssetDatabase.LoadAssetAtPath("Assets/Editor/EditorResources/Main Camera.prefab", typeof(GameObject)) as GameObject;
//                    camera = GameObject.Instantiate<GameObject>(camera, null);
//                    camera.name = "Main Camera";
//                    camera.transform.position = new Vector3(0f, 2f, -6f);
//                }
//            }
//        }
//    }
//}
//#endif