using System;
using System.IO;
using System.Collections.Generic;
using CFEngine;
using UnityEditor;
using UnityEngine;
using System.Text.RegularExpressions;
using CFEngine.Editor;

namespace XEditor
{
    public class PartyEditor : EditorWindow
    {
        public const string ConfigName = "/BundleRes/PartyConfig/PartyConfig.bytes";
        public const string ConfigDirName = "/BundleRes/PartyConfig/";
        public const string PARTYEDITOR = "PartyEditor";
        public const string PLAYER = "Player";
        public const string GROUND = "Ground";
        public const string DESK = "Desk";
        public const string GRABAREA = "GrabArea";
        public const string FOODS = "Foods";
        public const string RANDOMPOINTS = "RandomPoints";
        public const string NPCSPOSITION = "NpcPosition";

        private string ConfigDir;
        private string ConfigPath;

        private Dictionary<int, DishesConfiger> dishesConfig;
        private PartyAllConfigs config;
        private Vector2 sceneListScroll = Vector2.zero;
        private Vector2 npcListScroll = Vector2.zero;
        private bool EditorMode = false;
        protected GUIContent[] toolIcons = null;
        protected int toolIndex = 0;
        private string dishPath;
        private Vector3 modelSize;
        private Vector3 dishCollider;
        private int drawMode = 0;
        private int lastestLoadedTemplate;
        private int FPosition;

        private GameObject root;
        private GameObject player;
        private GameObject desk;
        private GameObject grabArea;
        private GameObject foods;
        private GameObject randomPoints;
        private GameObject npcsPosition;

        // 延迟创建 为了编辑器下可以选中创建的物体
        private int clickTime = 0;
        private GameObject clickObject;
        private Vector3 clickPosition;
        private int deleteIndex = -1;
        private bool isDirty { get; set; }

        MaterialPropertyBlock prop = null;

        [MenuItem(@"XEditor/PartyEditor")]
        public static void MenuInitEditorWindow()
        {
            PartyEditor window = EditorWindow.GetWindow<PartyEditor>("PartyEditor", true);
            window.name = "PartyEditor";
            window.Show();
        }

        private void OnEnable()
        {
            ConfigPath = Application.dataPath + ConfigName;
            ConfigDir = Application.dataPath + ConfigDirName;
#if UNITY_2019_1_OR_NEWER
            SceneView.duringSceneGui -= OnSceneGUI;
            SceneView.duringSceneGui += OnSceneGUI;
#else
            SceneView.onSceneGUIDelegate -= OnSceneGUI;
            SceneView.onSceneGUIDelegate += OnSceneGUI;
#endif

            EditorMode = false;
            toolIndex = 0;
            lastestLoadedTemplate = -1;
            dishesConfig = new Dictionary<int, DishesConfiger>();
            clickTime = 0;
            clickObject = null;
            isDirty = false;
            prop = new MaterialPropertyBlock();
            TextAsset content = AssetDatabase.LoadAssetAtPath("Assets/Table/PartyDishes.txt", typeof(TextAsset)) as TextAsset;
            string regexStr = @"(?<=\r\n)\d*\s.*?(?=(\s))\s.*?\s.*?\s";
            MatchCollection matches = Regex.Matches(content.text, regexStr);
            foreach (Match NextMatch in matches)
            {
                string[] value = NextMatch.Value.Split('\t');
                string[] value1 = value[2].Split('|');
                Vector3 size1 = Vector3.one;
                if (value1!=null&& value1.Length==3)
                  size1 = new Vector3(float.Parse(value1[0]), float.Parse(value1[1]), float.Parse(value1[2]));

                string[] value2 = value[3].Split('|');
                Vector3 size2 = Vector3.one;
                if (value2 != null && value2.Length == 3)
                    size2 = new Vector3(float.Parse(value2[0]), float.Parse(value2[1]), float.Parse(value2[2]));

                DishesConfiger con = new DishesConfiger() { name = value[1], size = size1 ,modelSize = size2 };
                dishesConfig.Add(int.Parse(value[0]), con);
            }


            if (toolIcons == null)
            {
                toolIcons = new GUIContent[dishesConfig.Count + 2];
                int index = 0;
                foreach (var v in dishesConfig)
                {
                    toolIcons[index++] = new GUIContent(v.Key.ToString());
                }
                toolIcons[index] = new GUIContent("Random");
                toolIcons[index + 1] = new GUIContent("Npc");

            }
        }

        void OnGUI()
        {
            if (!File.Exists(ConfigPath))
            {
                config = null;
                if (GUILayout.Button("Create", GUILayout.MaxWidth(120)))
                {
                    CreateConfig();
                    Debug.Log("create success at path: " + ConfigPath);
                    AssetDatabase.Refresh();
                }
                return;
            }
            if (config == null)
            {
                if (GUILayout.Button("Load Config", GUILayout.MaxWidth(120)))
                {
                    config = new PartyAllConfigs();
                    config.Load(ConfigPath);
                    InitScene();

                }
                return;
            }
            if (deleteIndex != -1)
            {
                config.configs.RemoveAt(deleteIndex);
                deleteIndex = -1;
                DeleteUnAttachFile();
                isDirty = true;
            }

            GUILayout.BeginHorizontal();

            if (GUILayout.Button("SaveGlobalConfig", GUILayout.MaxWidth(120)))
            {
                config.SaveGlobalConfig(ConfigPath);
                isDirty = false;
            }
            if (isDirty)
            {
                GUILayout.Label(EditorGUIUtility.IconContent("lightMeter/redLight"), GUILayout.Width(20), GUILayout.Height(20));
            }
            EditorGUILayout.LabelField("Click button when red icon appears, or desk/grabArea changed", GUILayout.Width(400f));

            GUILayout.EndHorizontal();
            DrawConfigList();

            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("EditorMode", GUILayout.Width(120f));
            EditorGUI.BeginChangeCheck();
            EditorMode = EditorGUILayout.Toggle(EditorMode, GUILayout.Width(120f));
            if (EditorGUI.EndChangeCheck())
            {
                if (EditorMode)
                {
                    toolIndex = 0;
                    ChangeDishPath(toolIndex);

                }
            }
            GUILayout.EndHorizontal();

            if (EditorMode)
            {
                GUILayout.Label("ctrl + Click ", GUILayout.MaxWidth(300));
                EditorGUI.BeginChangeCheck();
                //toolIndex = GUILayout.Toolbar(toolIndex, toolIcons, "button", GUILayout.MaxWidth(500));
                toolIndex = GUILayout.SelectionGrid(toolIndex, toolIcons, 4);
                if (EditorGUI.EndChangeCheck())
                {
                    ChangeDishPath(toolIndex);
                }
                string path = string.Empty;
                if (drawMode == 0)
                    path = dishPath;
                else if (drawMode == 1)
                    path = "random";
                else if (drawMode == 2)
                    path = "npc";

                GUILayout.Label("selected prefab:" + path);
                GUILayout.Label("RandomPoint   total:" + randomPoints.transform.childCount.ToString(), GUILayout.MaxWidth(150));
                GUILayout.Label("Npc   total:" + npcsPosition.transform.childCount.ToString(), GUILayout.MaxWidth(150));

            }

        }

        private void DrawConfigList()
        {
            GUILayout.Space(8);
            GUILayout.BeginVertical();
            GUILayout.BeginHorizontal();
            GUILayout.Label("TemplateList   total:" + config.configs.Count.ToString(), GUILayout.MaxWidth(150));
            GUILayout.Label("Last loaded Template :" + lastestLoadedTemplate.ToString(), GUILayout.MaxWidth(300));
            GUILayout.EndHorizontal();
            if (GUILayout.Button("Add Template", GUILayout.MaxWidth(160)))
            {
                PartySingleConfig template = new PartySingleConfig("default" + config.configs.Count);
                template.isDirty = true;
                config.configs.Add(template);
                isDirty = true;
            }
            EditorCommon.BeginScroll(ref sceneListScroll, config.configs.Count, 15, -1, 550);
            for (int i = 0; i < config.configs.Count; ++i)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label(i.ToString(), GUILayout.MaxWidth(15));

                var template = config.configs[i];
                EditorGUI.BeginChangeCheck();
                string newName = EditorGUILayout.TextField("", config.configs[i].name, GUILayout.MaxWidth(160));
                if (EditorGUI.EndChangeCheck())
                {
                    bool vaild = CheckNameVaild(newName);
                    if (vaild)
                    {
                        config.configs[i].name = newName;
                        config.configs[i].isDirty = true;
                    }
                    else
                        Debug.LogError("Name Repeated");
                }
                if (GUILayout.Button("Delete", GUILayout.MaxWidth(80)))
                {
                    deleteIndex = i;
                }
                if (GUILayout.Button("Load", GUILayout.MaxWidth(80)))
                {
                    lastestLoadedTemplate = i;
                    bool loadSuccess = config.ReadSingleTemplate(i);
                    if (loadSuccess)
                        LoadTemplateScene(i);
                }
                if (GUILayout.Button("Save", GUILayout.MaxWidth(80)))
                {
                    config.SaveSingleTemplate(i);
                    config.configs[i].isDirty = false;

                }
                if (config.configs[i].isDirty)
                    GUILayout.Label(EditorGUIUtility.IconContent("lightMeter/redLight"), GUILayout.Width(20), GUILayout.Height(20));

                GUILayout.EndHorizontal();
            }
            EditorCommon.EndScroll();
            GUILayout.EndVertical();
        }


        public void LoadTemplateScene(int index)
        {
            if (config != null && config.configs.Count > index)
            {
                ClearFoodAndRandomPoint();
                PartySingleConfig singleConfig = config.configs[index];
                for (int i = 0; i < singleConfig.food.Count; i++)
                {
                    GameObject dish = CreateDish(dishesConfig[singleConfig.food[i].dishID].name, dishesConfig[singleConfig.food[i].dishID].size,dishesConfig[singleConfig.food[i].dishID].modelSize);
                    dish.transform.position = singleConfig.food[i].positon;
                    dish.transform.eulerAngles = singleConfig.food[i].eulerAngle;
                    dish.name = singleConfig.food[i].dishID.ToString();
                }
                for (int i = 0; i < singleConfig.randomPoint.Count; i++)
                {
                    GameObject randomPoint = CreateRandomPoint();
                    randomPoint.transform.position = singleConfig.randomPoint[i];
                    randomPoint.transform.rotation = Quaternion.identity;
                }
                for (int i = 0; i < singleConfig.npcPosition.Count; i++)
                {
                    GameObject npc = CreateNpcObject();
                    npc.transform.position = singleConfig.npcPosition[i].npcPosition;
                    npc.transform.eulerAngles = singleConfig.npcPosition[i].npcAngle;
                    Transform food = npc.transform.GetChild(0);
                    food.position = singleConfig.npcPosition[i].foodPosition;
                    food.eulerAngles = singleConfig.npcPosition[i].foodAngle;
                }
            }
        }

        private void ClearFoodAndRandomPoint()
        {
            while (foods.transform.childCount > 0)
            {
                DestroyImmediate(foods.transform.GetChild(0).gameObject);
            }
            while (randomPoints.transform.childCount > 0)
            {
                DestroyImmediate(randomPoints.transform.GetChild(0).gameObject);
            }
            while (npcsPosition.transform.childCount > 0)
            {
                DestroyImmediate(npcsPosition.transform.GetChild(0).gameObject);
            }
        }

        private void OnDestroy()
        {
#if UNITY_2019_1_OR_NEWER
            SceneView.duringSceneGui -= OnSceneGUI;
#else
            SceneView.onSceneGUIDelegate -= OnSceneGUI; 
#endif
        }

        private void OnSceneGUI(SceneView sceneView)
        {
            if (EditorMode && clickTime == 0 && Event.current.type == EventType.MouseDown && Event.current.control)
            {
                Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
                RaycastHit hitInfo;
                if (Physics.Raycast(ray, out hitInfo))
                {
                    Debug.DrawLine(ray.origin, hitInfo.point);
                    GameObject gameObj = hitInfo.collider.gameObject;
                    Debug.Log("click object name is " + gameObj.name);
                    if (gameObj.name == DESK || gameObj.name == GROUND)
                    {
                        clickPosition = hitInfo.point;
                        clickTime = 30;
                        clickObject = gameObj;
                    }
                }
            }
            if (clickTime > 0)
            {
                clickTime--;
                if (clickTime == 0)
                {
                    Selection.activeGameObject = null;
                    Selection.objects = null;
                    OnDeskClick(drawMode, clickObject);
                }
            }
        }

        private void CreateConfig()
        {
            if (File.Exists(ConfigPath))
                File.Delete(ConfigPath);
            FileStream fs = new FileStream(ConfigPath, FileMode.CreateNew);
            fs.Close();
        }

        private void OnDeskClick(int drawMode, GameObject obj)
        {
            GameObject dish = null;
            if (drawMode == 0 && obj.name == DESK)
            {
                dish = CreateDish(dishPath,dishCollider, modelSize);

            }
            else if (drawMode == 1 && obj.name == DESK)
            {
                dish = CreateRandomPoint();

            }
            else if (drawMode == 2 && obj.name == GROUND)
            {
                dish = CreateNpcObject();
            }
            if (dish == null)
                return;
            dish.transform.position = clickPosition;
            dish.transform.rotation = Quaternion.identity;
            Selection.objects = null;
            Selection.activeGameObject = dish;
        }


        private GameObject CreateDish(string dishName,Vector3 colliderSize, Vector3 modelSize)
        {
            GameObject dish = null;
            GameObject dishHandle = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/BundleRes/Prefabs/" + dishName + ".prefab");
            dish = PrefabUtility.InstantiatePrefab(dishHandle) as GameObject;
            dish.transform.SetParent(foods.transform);
            dish.transform.localScale = modelSize;
            dish.name = toolIcons[toolIndex].text;
            BoxCollider col = dish.AddComponent<BoxCollider>();
            col.size = colliderSize;
            return dish;

        }

        private GameObject CreatePlayer()
        {
            GameObject player0 = null;
            player0 = GameObject.CreatePrimitive(PrimitiveType.Cube);
            player0.transform.SetParent(root.transform);
            player0.transform.localScale = new Vector3(0.5f, 4f, 0.25f);
            player0.name = PLAYER;
            SetGameObjectColor(player0, Color.red);

            return player0;
        }


        private GameObject CreateRandomPoint()
        {
            GameObject dish = null;
            dish = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            dish.transform.SetParent(randomPoints.transform);
            dish.transform.localScale = new Vector3(1f, 0.2f, 1f);
            dish.name = toolIcons[toolIndex].text;
            SetGameObjectColor(dish, Color.cyan);

            return dish;
        }

        private GameObject CreateNpcObject()
        {
            string suffix = npcsPosition.transform.childCount.ToString();
            GameObject npc = GameObject.CreatePrimitive(PrimitiveType.Cube);
            npc.name = "npc_" + suffix;
            npc.transform.SetParent(npcsPosition.transform);
            npc.transform.localScale = new Vector3(0.5f, 4f, 0.25f);
            SetGameObjectColor(npc, Color.green);

            GameObject food = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            food.name = "food_" + suffix;
            food.transform.SetParent(npc.transform);
            food.transform.localScale = new Vector3(2f, 0.25f * 0.2f, 4f);
            food.GetComponent<MeshRenderer>().sharedMaterial.shader = Shader.Find("Custom/Scene/Uber");

            SetGameObjectColor(food, Color.yellow);
            return npc;
        }

        private void SetGameObjectColor(GameObject obj, Color color)
        {
            Renderer render = obj.GetComponent<Renderer>();
            if (render != null)
            {
                render.GetPropertyBlock(prop);
                prop.SetColor("_Color0", color);
                render.SetPropertyBlock(prop);
            }
        }

        private void InitScene()
        {
            GameObject partyRoot = GameObject.Find("PartyEditor");
            if (partyRoot == null)
            {
                partyRoot = new GameObject("PartyEditor");
            }
            root = partyRoot;

            GameObject player0 = GameObject.Find(PLAYER);
            if (player0 == null)
            {
                player0 = CreatePlayer();
            }
            player0.transform.SetParent(root.transform);
            player0.transform.position = config.playerPosition;
            player0.transform.eulerAngles = config.playerEulur;
            player = player0;

            GameObject partyGround = GameObject.Find(GROUND);
            if (partyGround == null)
            {
                partyGround = new GameObject(GROUND);
                partyGround.AddComponent<BoxCollider>();
            }
            partyGround.transform.SetParent(root.transform);
            BoxCollider collider0 = partyGround.GetComponent<BoxCollider>();
            partyGround.transform.position = Vector3.zero;
            collider0.size = new Vector3(500f, 0.5f, 500f);

            GameObject partydesk = GameObject.Find(DESK);
            if (partydesk == null)
            {
                partydesk = new GameObject(DESK);
                partydesk.AddComponent<BoxCollider>();
            }
            partydesk.transform.SetParent(root.transform);
            desk = partydesk;
            BoxCollider collider1 = partydesk.GetComponent<BoxCollider>();
            collider1.size = config.deskSize;
            partydesk.transform.position = config.deskCenter;


            GameObject partyGrabArea = GameObject.Find(GRABAREA);
            if (partyGrabArea == null)
            {
                partyGrabArea = new GameObject(GRABAREA);
                partyGrabArea.AddComponent<BoxCollider>();
            }
            partyGrabArea.transform.SetParent(root.transform);
            grabArea = partyGrabArea;
            BoxCollider collider2 = grabArea.GetComponent<BoxCollider>();
            collider2.size = config.grapSize;
            grabArea.transform.position = config.grapCenter;

            GameObject partyFoods = GameObject.Find(FOODS);
            if (partyFoods == null)
            {
                partyFoods = new GameObject(FOODS);
            }
            partyFoods.transform.SetParent(root.transform);
            foods = partyFoods;

            GameObject randomPoint = GameObject.Find(RANDOMPOINTS);
            if (randomPoint == null)
            {
                randomPoint = new GameObject(RANDOMPOINTS);
            }
            randomPoint.transform.SetParent(root.transform);
            randomPoints = randomPoint;


            GameObject npcPos = GameObject.Find(NPCSPOSITION);
            if (npcPos == null)
            {
                npcPos = new GameObject(NPCSPOSITION);
            }
            npcPos.transform.SetParent(root.transform);
            npcsPosition = npcPos;

        }

        private void ChangeDishPath(int index)
        {
            dishPath = string.Empty;
            int i = 0;
            foreach (var o in dishesConfig)
            {
                if (index == i)
                {
                    dishPath = o.Value.name;
                    dishCollider = o.Value.size;
                    modelSize = o.Value.modelSize;
                    drawMode = 0;

                    return;
                }
                i++;
            }
            if (index == dishesConfig.Count)
            {
                drawMode = 1;
            }
            else if (index == dishesConfig.Count + 1)
            {
                drawMode = 2;
            }
        }

        private void DeleteUnAttachFile()
        {
            DirectoryInfo theFolder = new DirectoryInfo(ConfigDir);
            FileInfo[] thefileInfo = theFolder.GetFiles("*.bytes", SearchOption.TopDirectoryOnly);
            for (int i = thefileInfo.Length - 1; i > -1; i--)
            {
                string name = thefileInfo[i].Name;
                if (name == "PartyConfig.bytes")
                    continue;
                string[] names = name.Split('.');
                int index = config.configs.FindIndex(x => x.name == names[0]);
                if (index == -1)
                {
                    File.Delete(thefileInfo[i].FullName);
                }
            }
            AssetDatabase.Refresh();
        }

        private bool CheckNameVaild(string name)
        {
            for (int i = 0; i < config.configs.Count; i++)
            {
                if (config.configs[i].name == name)
                    return false;
            }
            return true;
        }

    }

    public class PartyAllConfigs
    {
        public Vector3 grapCenter;
        public Vector3 grapSize;
        public Vector3 deskCenter;
        public Vector3 deskSize;
        public Vector3 playerPosition;
        public Vector3 playerEulur;
        public List<PartySingleConfig> configs = new List<PartySingleConfig>();
        public void Load(string path)
        {
            if (File.Exists(path))
            {
                FileInfo fileInfo = new FileInfo(path);
                if (fileInfo.Length > 0)
                    ReadGlobalConfig(path);
            }
        }

        public void SaveGlobalConfig(string configPath)
        {
            GameObject Grab = GameObject.Find(PartyEditor.GRABAREA);
            GameObject Desk = GameObject.Find(PartyEditor.DESK);
            GameObject Player = GameObject.Find(PartyEditor.PLAYER);
            if (Grab == null || Desk == null)
            {
                Debug.LogError("GrabArea or Desk Can not be found!");
                return;
            }
            if (File.Exists(configPath))
                File.Delete(configPath);
            FileStream fs = new FileStream(configPath, FileMode.CreateNew);
            BinaryWriter bw = new BinaryWriter(fs);
            if (Grab != null)
            {
                BoxCollider collider = Grab.GetComponent<BoxCollider>();
                WriteVector3(bw, Grab.transform.position);
                WriteVector3(bw, collider.size);

            }
            else
            {
                WriteVector3(bw, Vector3.zero);
                WriteVector3(bw, Vector3.zero);
            }

            if (Desk != null)
            {
                BoxCollider collider = Desk.GetComponent<BoxCollider>();
                WriteVector3(bw, collider.transform.position);
                WriteVector3(bw, collider.size);

            }
            else
            {
                WriteVector3(bw,Vector3.zero);
                WriteVector3(bw, Vector3.zero);
            }

            if (Player != null)
            { 
                WriteVector3(bw, Player.transform.position);
                WriteVector3(bw, Player.transform.eulerAngles);

            }
            else
            {
                WriteVector3(bw, Vector3.zero);
                WriteVector3(bw, Vector3.zero);
            }

            bw.Write(configs.Count);
            for (int i = 0; i < configs.Count; i++)
            {
                //config must be low case
                bw.Write(configs[i].name.ToLower());
            }
            bw.Close();
            fs.Close();
            AssetDatabase.Refresh();
            Debug.Log("save success at path: " + configPath);

        }

        public bool ReadGlobalConfig(string configPath)
        {
            if (!File.Exists(configPath))
            {
                Debug.LogError("not Exist!:" + configPath);
                return false;
            }
            FileStream fs = new FileStream(configPath, FileMode.Open);
            BinaryReader br = new BinaryReader(fs);
            grapCenter = ReadVector3(br);
            grapSize = ReadVector3(br);
            deskCenter = ReadVector3(br);
            deskSize = ReadVector3(br);
            playerPosition = ReadVector3(br);
            playerEulur = ReadVector3(br);

            configs.Clear();
            int count = br.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                string name = br.ReadString();
                configs.Add(new PartySingleConfig(name));
            }
            br.Close();
            fs.Close();
            for (int i = configs.Count - 1; i > -1; i--)
            {
                string filePath = Application.dataPath + PartyEditor.ConfigDirName + configs[i].name + ".bytes";
                if (!File.Exists(filePath))
                {
                    configs.RemoveAt(i);
                }
            }
            return true;

        }

        public void SaveSingleTemplate(int index)
        {
            if (index < configs.Count)
            {
                PartySingleConfig config = configs[index];
                GameObject Food = GameObject.Find(PartyEditor.FOODS);
                GameObject RandomPoint = GameObject.Find(PartyEditor.RANDOMPOINTS);
                GameObject NpcsPosition = GameObject.Find(PartyEditor.NPCSPOSITION);
                if (Food == null && RandomPoint == null)
                {
                    Debug.LogError("Foods or RandomPoints can not be Found");
                    return;
                }
                string filPath = Application.dataPath + PartyEditor.ConfigDirName + config.name + ".bytes";
                if (File.Exists(filPath))
                    File.Delete(filPath);
                FileStream fs = new FileStream(filPath, FileMode.CreateNew);
                BinaryWriter bw = new BinaryWriter(fs);
                bw.Write(config.name);

                int foodCount = Food.transform.childCount;
                bw.Write(foodCount);
                for (int i = 0; i < Food.transform.childCount; i++)
                {
                    Transform trans = Food.transform.GetChild(i);
                    int id = 0;
                    bool s = int.TryParse(trans.name, out id);
                    bw.Write(id);
                    WriteVector3(bw, trans.position);
                    WriteVector3(bw, trans.eulerAngles);
                }
                int randomCount = RandomPoint.transform.childCount;
                bw.Write(randomCount);
                for (int i = 0; i < randomCount; i++)
                {
                    Transform trans = RandomPoint.transform.GetChild(i);
                    WriteVector3(bw, trans.position);
                }

                int npcCount = NpcsPosition.transform.childCount;
                bw.Write(npcCount);
                for (int i = 0; i < npcCount; i++)
                {
                    Transform trans = NpcsPosition.transform.GetChild(i);
                    WriteVector3(bw, trans.position);
                    WriteVector3(bw, trans.eulerAngles);
                    Transform food = trans.GetChild(0);
                    WriteVector3(bw, food.position);
                    WriteVector3(bw, food.eulerAngles);

                }

                bw.Close();
                fs.Close();
                AssetDatabase.Refresh();
                Debug.Log("save success at path: " + filPath);

            }
        }


        public bool ReadSingleTemplate(int index)
        {
            if (index < configs.Count)
            {
                string filPath = Application.dataPath + PartyEditor.ConfigDirName + configs[index].name + ".bytes";

                if (!File.Exists(filPath))
                {
                    Debug.LogError("Template not Exist!:" + filPath);
                    return false;
                }
                PartySingleConfig config = configs[index];
                config.Clear();
                FileStream fs = new FileStream(filPath, FileMode.Open);
                BinaryReader br = new BinaryReader(fs);
                config.name = br.ReadString();
                int count = br.ReadInt32();
                for (int i = 0; i < count; i++)
                {
                    PartySingleConfig.PartyFoodClass food = new PartySingleConfig.PartyFoodClass();
                    food.dishID = br.ReadInt32();
                    food.positon = ReadVector3(br);
                    food.eulerAngle = ReadVector3(br);
                    config.food.Add(food);
                }
                count = br.ReadInt32();
                for (int i = 0; i < count; i++)
                {
                    config.randomPoint.Add(ReadVector3(br));
                }
                count = br.ReadInt32();
                for (int i = 0; i < count; i++)
                {
                    PartySingleConfig.NpcPosition npc = new PartySingleConfig.NpcPosition();
                    npc.npcPosition = ReadVector3(br);
                    npc.npcAngle = ReadVector3(br);
                    npc.foodPosition = ReadVector3(br);
                    npc.foodAngle = ReadVector3(br);
                    config.npcPosition.Add(npc);
                }

                br.Close();
                fs.Close();
                return true;
            }
            return false;

        }

        private Vector3 ReadVector3(BinaryReader br)
        {
            Vector3 value;
            value.x = br.ReadSingle();
            value.y = br.ReadSingle();
            value.z = br.ReadSingle();
            return value;
        }

        private void WriteVector3(BinaryWriter bw, Vector3 value)
        {
            bw.Write(value.x);
            bw.Write(value.y);
            bw.Write(value.z);
        }

    }

    public class DishesConfiger 
    {
        public string name;
        public Vector3 size;
        public Vector3 modelSize;
    }

    public class PartySingleConfig
    {
        public string name;

        public List<PartyFoodClass> food = new List<PartyFoodClass>();
        public List<Vector3> randomPoint = new List<Vector3>();
        public List<NpcPosition> npcPosition = new List<NpcPosition>();
        public bool isDirty;

        public PartySingleConfig(string _name)
        {
            name = _name;
        }

        public void Clear()
        {
            name = string.Empty;
            food.Clear();
            randomPoint.Clear();
            npcPosition.Clear();
        }

        public class PartyFoodClass
        {
            public int dishID;
            public Vector3 positon;
            public Vector3 eulerAngle;
        }

        public class NpcPosition
        {
            public Vector3 npcPosition;
            public Vector3 npcAngle;
            public Vector3 foodPosition;
            public Vector3 foodAngle;
        }

    }

    public partial class BuildParty : PreBuildPreProcess
    {
        public override string Name { get { return "Party"; } }
        public override int Priority
        {
            get
            {
                return 1;
            }
        }

        public override void PreProcess()
        {
            base.PreProcess();
            ProcessFolder("partyconfig", "partylist");
        }
    }

}