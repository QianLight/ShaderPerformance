using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CFEngine.Editor;
using CFUtilPoolLib;
using UnityEditor;
using UnityEngine;
using VirtualSkill;

namespace XEditor
{
    #region HugeEditor
    public class HugeEditor:EditorWindow
    {
        public const string COMMENT_PATH = "Assets/BundleRes/ReactPackage/HugeColliderComment/";
        private static uint  mPresentID;
        private static string _name;
        private static Dictionary<uint, XEntityPresentation.RowData> mHugeDict;
        private static Dictionary<uint,string> mCommentDict;

        private static HugeDataEditor mHugeDataEditor;
        private static string mChangedString;

        [MenuItem(@"XEditor/HugeExport")]
        static void OpenHugeEditor()
        {
            EditorWindow.GetWindow<HugeEditor>("Huge Boss Collider Exporter", true);
        }
        private void OnEnable()
        {
            //SkillHoster.GetHoster.RefreshHitScript();
            Init();
        }
        private static void Init()
        {
            mPresentID = 76020;
            _name = "";
            mHugeDict = new Dictionary<uint, XEntityPresentation.RowData>();
            mCommentDict = null;
            LoadTable();

        }
        private void OnDisable()
        {
            mHugeDict = null;
            mCommentDict = null;
            mHugeDataEditor = null;
            mChangedString = null;
        }
        private static void LoadTable()
        {
            //读取表格
            mHugeDict.Clear();
            XEntityPresentationReader.Reload();
            var table = XEntityPresentationReader.Presentations.Table;
            int length = table.Length;
            for (int i = 0; i < length; i++)
            {
                if (table[i].Huge)
                    mHugeDict.Add(table[i].PresentID, table[i]);
            }
        }
        private void OnGUI()
        {
            uint.TryParse(EditorGUITool.TextField("Present ID: ",mPresentID.ToString()),out mPresentID); 
            //mAnimation = EditorGUITool.TextField("Aniamtion: ","[未实现]");
            //_animation = EditorGUITool.TextField("Aniamtion: ",_animation);

            //var result = XEntityPresentationReader.GetData(_presentID);
            //    _name = result == null?"none": result.PrefabShow;//暂时不管非Huge怪
            if (mHugeDict.TryGetValue(mPresentID, out var targetHuge))
                _name = targetHuge.PrefabShow;
            else
                _name = "";
            EditorGUITool.LabelField("Name: " + _name);
            if (GUILayout.Button("Open"))
            {
                Open(mPresentID);
            }
            //if (GUILayout.Button("Create[未实现]"))
            //{
            //    Create();
            //}
        }
        public static void Open(uint pid)
        {
            if (mHugeDict == null)
                Init();

            //参数
            HugeInfo info = new HugeInfo();
            if (mHugeDict.TryGetValue(pid, out var targetHuge))
            {
                info.presentID = targetHuge.PresentID;
                info.skillLocation = targetHuge.SkillLocation;
                info.idle = targetHuge.Idle;
                info.hugeMonsterColliders = mChangedString == null?targetHuge.HugeMonsterColliders.ToString():mChangedString;
                info.prefabShow = targetHuge.PrefabShow;
            }
            else
            {
                //this.ShowNotification(new GUIContent("不存在对应的HugeMonster数据"));
                Debug.LogError("不存在对应的HugeMonster数据");
                return;
            }

            
            info.entity = null;
            if (SkillHoster.GetHoster != null)//skillHoster内对应的Entity
            {
                var pairs = SkillHoster.GetHoster.EntityDic.ToArray();
                for (int i = 0; i < pairs.Length; i++)
                    if (pairs[i].Value.presentData?.PresentID == pid)
                    {
                        Debug.Log("找到啦~");
                        info.entity = pairs[i].Value;
                        break;
                    }
            }

            //读取评论
            var path = COMMENT_PATH + pid + ".bytes";
            mCommentDict = new Dictionary<uint, string>(); 
            string[] str; string[] kv;
            if (File.Exists(path))
            {
                str = DataIO.DeserializeData<string>(path).Split('|');
                for (int i = 0; i < str.Length; i++)
                {
                    kv = str[i].Split('=');
                    mCommentDict[uint.Parse(kv[0])] = kv[1];
                }
            }
            info.comment = mCommentDict;

            //打开Editor界面
            HugeDataEditor.OpenHugeData(info,ref mHugeDataEditor);
        }
        private void Create()
        {
            if (SkillHoster.GetHoster == null || SkillHoster.GetHoster.EntityDic==null)
            {
                Debug.Log("empty");
            }
            else
            {
                Debug.Log("dict"+ SkillHoster.GetHoster.EntityDic.Count);
                var pair = SkillHoster.GetHoster.EntityDic.ToList();
                for (int i = 0; i < pair.Count; i++)
                {
                if(pair[i].Value.presentData!=null)
                    Debug.Log("dict: ("+pair[i].Key + ": " + pair[i].Value.presentData.PresentID+")");
                }
            }
        }
    }
    #endregion

    #region HugeInfo数据类
    class HugeInfo {
        public uint presentID;
        public string skillLocation;
        public string idle;
        public string hugeMonsterColliders;
        public string prefabShow;
        public Dictionary<uint,string> comment;
        public Entity entity;
        public HugeInfo() { }
        public HugeInfo(uint id, string skillLocation, string idle, string hugeColliders,string prefabShow, Dictionary<uint, string> comment, Entity entity)
        {
            this.presentID = id;
            this.skillLocation = skillLocation;
            this.idle = idle;
            this.hugeMonsterColliders = hugeColliders;
            this.prefabShow = prefabShow;
            this.comment = comment;
            this.entity = entity;
        }
    }
    #endregion

    #region HugeData
    class HugeDataEditor :EditorWindow
    {
        const int MAX_ID = 32;
        const int MAX_BOX_COUNT = 8;

        static uint mCurrentID;
        static string mBufferStr;
        static string mOriginStr;
        static HugeInfo mHugeInfo;
        static Dictionary<uint, List<BoxData>> mDatas;
        static Dictionary<uint, string> mComments;

        static List<GameObject> selectedGOs;
        static Vector2 scrollView;

        static bool OnPlayMode=false;
        static bool AutoUpdateID = false;
        static ulong idInRuntime=ulong.MaxValue;

        static bool DrawInBox = false;
        class BoxData
        {
            public Vector3 _XYZ;
            public float _radius;
            public float _height;
            public BoxData(Vector3 xYZ, float radius, float height)
            {
                _XYZ = xYZ;
                _radius = radius;
                _height = height;
            }
        }
        internal static void OpenHugeData(HugeInfo info,ref HugeDataEditor huge)
        {
            mHugeInfo = info;
            mComments = info.comment;
            mBufferStr = info.hugeMonsterColliders;
            mOriginStr = mBufferStr;

            huge= EditorWindow.GetWindow<HugeDataEditor>("Huge Boss Collider Data", true);
        }
        private void OnEnable()
        {
            mCurrentID = 0;
            LoadData(mBufferStr);
        }
        private void OnDisable()
        {
            mHugeInfo = null;
            mDatas = null;
            mComments = null;
            ClearBoxEntity();
        }
        private void OnGUI()
        {
            if (mDatas == null)
                LoadData(mBufferStr);

            //顶部栏
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Save"))
            {
                SaveData();
            }
            if (GUILayout.Button("Revert"))
            {
                RevertData();
            }
            GUILayout.EndHorizontal();

            EditorGUILayout.Space();

            //gizmos画图方式
            DrawInBox = EditorGUILayout.Toggle("绘制立方体", DrawInBox);

            //角色名称
            EditorGUILayout.LabelField("当前角色："+mHugeInfo.prefabShow);

            //自动更新id
            AutoUpdateID= EditorGUILayout.Toggle("随状态自动更新ID", AutoUpdateID);

            //if (mHugeInfo.entity != null&&AutoUpdateID)
            //{

            //    var id = Xuthus.getCollisionType(mHugeInfo.entity.id);
            //    if (id > -1 && id <= MAX_ID)
            //        mCurrentID = (uint)id;
            //    Debug.Log(mCurrentID);
            //}


            ////搜索栏
            //GUILayout.BeginHorizontal();
            //EditorGUITool.LabelField("新增状态ID: ");
            //if (uint.TryParse(EditorGUITool.TextField("Search: ", mCurrentID.ToString()), out mCurrentID))
            //{
            //    if (mCurrentID > MAX_ID)
            //    {
            //        ShowNotification(new GUIContent("状态ID不能大于" + MAX_ID.ToString()),2f);
            //        mCurrentID = 0;
            //    }
            //}

            GUIStyle notEmptyStyle = new GUIStyle();
            notEmptyStyle.fontStyle = FontStyle.Bold;



            GUILayout.BeginHorizontal();
            bool isNotEmpty;
            for (uint i = 0; i <= MAX_ID; i++)
            {
                isNotEmpty = mDatas.ContainsKey(i) && mDatas[i].Count > 0;
                if(isNotEmpty?  
                    GUILayout.Button(new GUIContent(i.ToString())):
                    GUILayout.Button(new GUIContent(i.ToString()),notEmptyStyle)
                    )
                {
                    mCurrentID = i;
                }                
            }
            GUILayout.EndHorizontal();

            EditorGUILayout.Space();

            DrawBoxGizmos();

            DrawInfoByID();

            DrawCopyStr();
        }

        #region 顶部栏
        //private void OpenData()
        //{
        //    //todo
        //}
        private void SaveData()
        {
            GenerateStr();
            SaveComment();
            ShowNotification(new GUIContent("更改已保存至缓存。"));
        }
        private void BuildData()
        {
            if (Application.isPlaying)
            {
                ShowNotification(new GUIContent("仅在编辑器状态下使用"));
                return;
            }
            else//在编辑器状态下打开场景
            {
                //GenerateStr();
                Close();
                EditorApplication.ExecuteMenuItem("Edit/Play");
            }
            //if (SkillHoster.GetHoster == null)
            //{
            //    ShowNotification(new GUIContent("游戏场景中仅能预览碰撞盒"));
            //    return;
            //}
            //else
            //{
            //    if (!Application.isPlaying)//"编辑器下
            //    {
            //        EditorApplication.ExecuteMenuItem("Edit/Play");
            //        GenerateStr();

            //        foreach (var pair in SkillHoster.GetHoster.EntityDic.ToArray())
            //        {
            //            if (pair.Value.presentData.PresentID == mCurrentID)
            //            {
            //                mHugeInfo.entity = pair.Value;
            //            }
            //        }
            //        if (mHugeInfo.entity != null)
            //            mHugeInfo.entity.collidersStr = mBufferStr;
            //        else 
            //            ShowNotification(new GUIContent("出现错误，需要重新开启页面"));
            //    }
            //    else//编辑器运行
            //    {
            //        GenerateStr();
            //        mHugeInfo.entity.collidersStr = mBufferStr;
            //    }
            //}
        }

        private void RevertData()
        {
            LoadData(mOriginStr);
            mBufferStr = mOriginStr;
            ShowNotification(new GUIContent("已重新加载数据"));
        }
        #endregion

        #region 加载
        /// <summary>
        /// 根据字符加载
        /// </summary>
        private void LoadData(string str)
        {
            OnPlayMode = mHugeInfo.entity != null;//Gizmos描绘方式
            
            mDatas = new Dictionary<uint, List<BoxData>>();//读取参数
            if (str == null)
                return;

            string[] boxes = str.Split('|');
            BoxData boxData;
            for (int i = 0; i < boxes.Length; i++)
            {
                var value = boxes[i].Split('=');
                if (value.Length < 6) return;
                uint id = uint.Parse(value[0]);
                boxData = new BoxData(new Vector3(float.Parse(value[1]), float.Parse(value[2]), float.Parse(value[3])), float.Parse(value[4]), float.Parse(value[5]));
                if (!mDatas.ContainsKey(id))
                    mDatas.Add(id, new List<BoxData>());
                mDatas[id].Add(boxData);
            }
        }

        /// <summary>
        /// 生成字符
        /// </summary>
        private void GenerateStr()
        {
            if (mDatas == null) return;
            StringBuilder str = new StringBuilder("");
            uint key;
            List<BoxData> boxes;
            foreach (var boxData in mDatas)
            {
                key = boxData.Key;
                boxes = boxData.Value;
                foreach (var box in boxes)
                {
                    str.Append(key);
                    str.Append("=");
                    str.Append(box._XYZ.x);
                    str.Append("=");
                    str.Append(box._XYZ.y);
                    str.Append("=");
                    str.Append(box._XYZ.z);
                    str.Append("=");
                    str.Append(box._radius);
                    str.Append("=");
                    str.Append(box._height);
                    str.Append("|");
                }
            }
            if (str.Length > 0) str.Remove(str.Length - 1, 1);
            mBufferStr = str.ToString();
        }
        private void SaveComment()
        {
            if (mComments == null) return;//comment的保存方法
            var path = HugeEditor.COMMENT_PATH + mHugeInfo.presentID + ".bytes";
            var str = new StringBuilder("");
            foreach (var kv in mComments)
            {
                if (kv.Value.Length == 0) continue;
                str.Append(kv.Key);
                str.Append("=");
                str.Append(kv.Value);
                str.Append("|");
            }
            if (str.Length > 0) str.Remove(str.Length - 1, 1);
            if(str.Length>0) DataIO.SerializeData(path, str.ToString());
        }
        #endregion

        #region 展示栏

        /// <summary>
        /// 策划填表用，方便复制
        /// </summary>
        private void DrawCopyStr()
        {
            EditorGUILayout.Space();
            //var style = EditorStyles.label;
            //style.wordWrap = true;
            //EditorGUILayout.LabelField(str, style);
            EditorGUILayout.LabelField(mBufferStr);
            if (GUILayout.Button("复制"))
            {
                GUIUtility.systemCopyBuffer = mBufferStr;
            }
            EditorGUILayout.Space();
        }
        private bool refresh = false;
        private void DrawInfoByID()
        {
            if (mDatas==null) return;
            if (!mDatas.ContainsKey(mCurrentID))
                mDatas.Add(mCurrentID, new List<BoxData>());

            //展示栏
            GUILayout.BeginHorizontal();
            EditorGUITool.LabelField("当前状态ID: " + mCurrentID);
            if (GUILayout.Button("Add New Box"))
            {
                AddBox();
            }
            if (GUILayout.Button("Delete ID"))
            {
                DeleteID(mCurrentID);
            }
            GUILayout.EndHorizontal();

            if (mComments.TryGetValue(mCurrentID, out var com))//备注
                mComments[mCurrentID] = EditorGUILayout.TextField("评论备注: ", mComments[mCurrentID]);
            else
                mComments[mCurrentID] = EditorGUILayout.TextField("评论备注: ","");

            scrollView = EditorGUILayout.BeginScrollView(scrollView);
            GUILayout.BeginVertical();
            int count = mDatas[mCurrentID].Count;
            refresh = false;
            Vector3 tempV3 = Vector3.zero; ;float tempR=0f, tempH=0f;
            for (int i = 0; i < count; i++)
            {
                EditorGUILayout.Space();

                GUILayout.BeginHorizontal();
                GUILayout.BeginVertical();

                if (!refresh)
                {
                    tempV3 = mDatas[mCurrentID][i]._XYZ;
                    tempR = mDatas[mCurrentID][i]._radius;
                    tempH = mDatas[mCurrentID][i]._height;
                }
                mDatas[mCurrentID][i]._XYZ= EditorGUILayout.Vector3Field("XYZ: ", mDatas[mCurrentID][i]._XYZ);
                mDatas[mCurrentID][i]._radius=EditorGUITool.FloatField("Radius: ", mDatas[mCurrentID][i]._radius);
                mDatas[mCurrentID][i]._height=EditorGUITool.FloatField("Height: ", mDatas[mCurrentID][i]._height);
                if (!refresh)
                {
                    refresh= (tempV3 == mDatas[mCurrentID][i]._XYZ)||(tempR == mDatas[mCurrentID][i]._radius)||(tempH == mDatas[mCurrentID][i]._height);
                }
                GUILayout.EndVertical();
                if (GUILayout.Button("Delete Box"))
                {
                    DeleteBox(i);
                }
                GUILayout.EndHorizontal();
                EditorGUILayout.Space();
            }
            GUILayout.EndVertical();
            GUILayout.EndScrollView();
        }

        /// <summary>
        ///  碰撞盒可视化
        /// </summary>
        private void DrawBoxGizmos()
        {
            if (mDatas == null) return;
            if (selectedGOs == null) InitBoxEntity();

            if (AutoUpdateID&&idInRuntime!=ulong.MaxValue)
            {
                mCurrentID = (uint)Xuthus.getCollisionType(idInRuntime);
            }
            if (!mDatas.ContainsKey(mCurrentID)) return;

            //修改entity的值
            SeqListRef<float> colliders;
            var boxData = mDatas[mCurrentID];
            if (OnPlayMode)
            {
                colliders = mHugeInfo.entity.presentData.HugeMonsterColliders;
                if (colliders.count <= mCurrentID) return;
                mHugeInfo.entity.mBoxID = Convert.ToInt32(mCurrentID);//盒子id
                mHugeInfo.entity.mSelectedBoxIdx = -1;
                if (refresh)
                {
                    GenerateStr();
                    mHugeInfo.entity.collidersStr = mBufferStr;
                }
            }
            HugeBoxEntity boxEntity;
            for (int i = 0; i < selectedGOs.Count; i++)
            {
                if (i < boxData.Count)
                {
                    if (!OnPlayMode)
                    {
                        boxEntity = selectedGOs[i].GetComponent<HugeBoxEntity>();
                        //boxEntity.transform.localPosition = boxData[i]._XYZ;
                        boxEntity.mXYZ = boxData[i]._XYZ;
                        boxEntity.mRadius = boxData[i]._radius;
                        boxEntity.mHeight = boxData[i]._height;
                        boxEntity.mCurrentColor = Selection.activeGameObject == selectedGOs[i] ? HugeBoxEntity.mSelectedColor : HugeBoxEntity.mNormalColor;
                        boxEntity.DrawInBox = DrawInBox;
                    }
                    else
                    {
                        if (Selection.activeGameObject == selectedGOs[i])
                            mHugeInfo.entity.mSelectedBoxIdx = i;
                    }
                    selectedGOs[i].SetActive(true);
                }
                else
                    selectedGOs[i].SetActive(false);
            }
        }
        private void OnInspectorUpdate()
        {
            Repaint();
        }
        private void InitBoxEntity()
        {
            selectedGOs = new List<GameObject>(8);
            var hugeName = mHugeInfo.prefabShow.ToLower();
            var allGO = FindObjectsOfType(typeof(GameObject));
            GameObject player=null;
            string tempName;
            bool isPlaying = Application.isPlaying;
            bool isHoster = SkillHoster.GetHoster != null;
            for (int i = 0; i < allGO.Length; i++)
            {
                tempName = allGO[i].name.ToLower();
                if (isPlaying && !isHoster)//游戏内场景
                {
                    if (tempName.Contains("uid")
                        &&tempName.Contains(hugeName))
                    {
                        var split = tempName.Split('-');
                        if (idInRuntime == ulong.MaxValue)
                            idInRuntime = ulong.Parse(split[1].Split('_')[0]);
                        player = allGO[i] as GameObject;
                        break;
                    }
                }
                else
                {
                    if (isHoster)//编辑器场景
                    {
                        if (tempName == hugeName)
                        {
                            player = allGO[i] as GameObject;
                            break;
                        }
                        else if (tempName == "player")
                        {
                            player = allGO[i] as GameObject;
                            break;
                        }
                    }
                    else//其他情况, 临时使用
                    {
                        if (tempName == hugeName|| tempName == "player")
                        {
                            player = allGO[i] as GameObject;
                            break;
                        }
                    }
                }
            }
            if (player == null)
            {
                ShowNotification(new GUIContent($"场景中没找到'{mHugeInfo.prefabShow}'或'Player'"));
                return;
            }
            else if (player.GetComponent<SkinnedMeshRenderer>()!=null)
            {
                player = player.transform.parent.gameObject;
            }
            GameObject entity;
            for (int i = 0; i < MAX_BOX_COUNT; i++)
            {
                entity=GameObject.Instantiate<GameObject>(AssetDatabase.LoadAssetAtPath("Assets/BundleRes/Prefabs/empty.prefab",typeof(GameObject))as GameObject);
                entity.name = "HugeBox" +i;
                entity.transform.SetParent(player.transform);
                entity.transform.localPosition = Vector3.zero;
                entity.transform.localRotation = Quaternion.identity;
                entity.transform.localScale = Vector3.one;
                entity.AddComponent<HugeBoxEntity>().enabled = false;//在非技能编辑器运行状态下使用
                selectedGOs.Add(entity);
            }
        }               
        private static bool ClearBoxEntity()
        {
            for (int i = 0; i < selectedGOs.Count; i++)
                DestroyImmediate(selectedGOs[i]);
            selectedGOs = null;
            return true;
        }
        private void DeleteID(uint id)
        {
            if(mDatas.TryGetValue(id,out var idData))
            {
                for (int i = 0; i < idData.Count; i++)
                    idData[i] = null;
                //idData._comment = "";
                mDatas.Remove(id);
            }
            mComments.Remove(id);
        }
        private void AddBox()
        {
            if (!mDatas.ContainsKey(mCurrentID))
                mDatas.Add(mCurrentID, new List<BoxData>());
            if (mDatas[mCurrentID].Count > MAX_BOX_COUNT)
            {
                ShowNotification(new GUIContent("碰撞盒数量已达到上限" + MAX_BOX_COUNT.ToString()),2f);
            }
            else
            {
                mDatas[mCurrentID].Add(new BoxData(Vector3.one, 2f, 2f));
            }
        }
        private void DeleteBox(int i)
        {            
            mDatas[mCurrentID].RemoveAt(i);
        }        
        #endregion
    }
    #endregion
}