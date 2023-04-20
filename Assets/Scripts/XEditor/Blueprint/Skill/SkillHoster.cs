#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using CFClient;
using CFEngine;
using CFUtilPoolLib;
using Cinemachine;
using ClientEcsData;
using EcsData;
using EditorEcs;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using XEcsGamePlay;
using FxData = XEcsGamePlay.FxData;
using Object = UnityEngine.Object;
using XSkillData = EcsData.XSkillData;
using XWarningData = EcsData.XWarningData;

namespace VirtualSkill
{
    public class Entity
    {
        public Entity(GameObject go, Animator animator, ulong index)
        {
            id = index;
            obj = go;
            ator = animator;
            startY = go.transform.position.y;

            posXZ = GameObject.Instantiate<GameObject>(AssetDatabase.LoadAssetAtPath("Assets/BundleRes/Prefabs/empty.prefab", typeof(GameObject)) as GameObject, null);
            posXZ.name = go.name + "_posXZ";
            mHeightOffset = 0;
            InitDynamicBone(go);
        }

        private void InitDynamicBone(GameObject go)
        {
            List<CFDynamicBone> bones = new List<CFDynamicBone>();
            go.GetComponentsInChildren<CFDynamicBone>(true, bones);
            dynamicBone.Init(bones);
        }

        public Transform Find(string name)
        {
            if (!string.IsNullOrEmpty(name))
            {
                return obj.transform.Find(name);
            }
            return obj.transform;
        }

        public void Update(float deltaTime)
        {
            UpdateMultipleDir(deltaTime);
            UpdatePosXZ();
            dynamicBone.Update(deltaTime);
            if (previewContext != null)
            {
                previewContext.Update();
            }
            if (fmod != null && obj != null)
            {
                //fmod.Update3DAttributes(obj.transform.position, m_audioChannel);
                foreach (var item in m_followDict)
                {
                    if (item.Value)
                    {
                        fmod.Update3DAttributes(obj.transform.position, item.Key);
                    }
                }
            }

        }

        #region TransformSkin
        public void TransformSkin(int presentID)
        {
            XEntityPresentation.RowData data = XEntityPresentationReader.GetData((uint)presentID);
            if (data == null) return;
            GameObject prefabObject = XEntityPresentationReader.GetDummy((uint)presentID);
            GameObject entity = GameObject.Instantiate(prefabObject);
            GameObject preObj = obj;
            Animator preAtor = ator;
            obj = entity;
            entity.transform.position = XCommon.Far_Far_Away;
            entity.name = "Player";
            entity.AddComponent<SkillResultSceneUI>();
            entity.AddComponent<SkillTargetSelectSceneUI>();

            DynamicCaster dynamicCaster = entity.AddComponent<DynamicCaster>();
            dynamicCaster.dynamicCastType = EShadowCasterType.MainCaster;
            ator = entity.GetComponent<Animator>();

            InitDynamicBone(obj);

            SkillHoster.GetHoster.PlayerPresentID = presentID;
            SkillHoster.GetHoster.EntityAtorDic[1] = SkillHoster.GetHoster.BuildOverride(preObj, obj);
            SkillHoster.GetHoster.cameraComponent.Hoster = obj;
            SkillHoster.GetHoster.PresentDic[1] = (uint)presentID; ;
            SkillHoster.GetHoster.EntityDic[1].presentData = XEntityPresentationReader.GetData(SkillHoster.GetHoster.PresentDic[1]);
            SkillHoster.GetHoster.EntityDic[1].collidersStr = SkillHoster.GetHoster.EntityDic[1].presentData.HugeMonsterColliders.ToString();
            SkillHoster.GetHoster.EntityDic[1].statisticsData = XEntityStatisticsReader.GetData(SkillHoster.GetHoster.PresentDic[1]);
            SkillHoster.GetHoster.EntityDic[1].previewContext.entityInit = false;

            float layer0Time;
            float layer1Time;
            int layer0NameHash;
            int layer1NameHash;

            if (preAtor.IsInTransition(0))
            {
                layer0Time = GetNextTime(preAtor, 0, true);
                layer0NameHash = GetNextStateHash(preAtor, 0);
            }
            else
            {
                layer0Time = GetPlayedTime(preAtor, 0, true);
                layer0NameHash = GetPlayingStateHash(preAtor, 0);
            }

            if (preAtor.IsInTransition(1))
            {
                layer1Time = GetNextTime(preAtor, 1, true);
                layer1NameHash = GetNextStateHash(preAtor, 1);
            }
            else
            {
                layer1Time = GetPlayedTime(preAtor, 1, true);
                layer1NameHash = GetPlayingStateHash(preAtor, 1);
            }

            ator.Play(layer0NameHash, 0, layer0Time);
            ator.Play(layer1NameHash, 1, layer1Time);
            ator.Update(0);

            Debug.Log(presentID);
            SkillHoster.GetHoster.GetEntity();
            SkillHoster.GetHoster.EntityDic[1].previewContext.xGameObject.Ator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
            SkillHoster.GetHoster.EntityDic[1].previewContext.xGameObject.SetUpdateWhenOffscreen(true, uint.MaxValue, true);
            SFXMgr.singleton.ChangeSfxOwner(1, previewContext.xGameObject);
            XTimerMgr.singleton.SetTimer(0f,
                (object o) =>
                {
                    SkillHoster.GetHoster.PrepareAnim(id, "idle", "Idle");
                    SkillHoster.GetHoster.PrepareAnim(id, "run", "Run");
                    entity.transform.SetPositionAndRotation(preObj.transform.position, preObj.transform.rotation);
                    SkillHoster.GetHoster.ResetCinemachine();
                    //preObj.transform.position = XCommon.Far_Far_Away;
                    preObj.SetActive(false);
                }, null);
            XTimerMgr.singleton.SetTimer(1f, (object o) => { GameObject.DestroyImmediate(preObj); }, null);
        }

        private int GetPlayingStateHash(Animator m_Ator, int layer)
        {
            if (m_Ator != null)
            {
                AnimatorStateInfo animationState = m_Ator.GetCurrentAnimatorStateInfo(layer);
                return animationState.fullPathHash;
            }
            else
            {
                return 0;
            }
        }

        private int GetNextStateHash(Animator m_Ator, int layer)
        {
            if (m_Ator != null)
            {
                AnimatorStateInfo animationState = m_Ator.GetNextAnimatorStateInfo(layer);
                return animationState.fullPathHash;
            }
            else
            {
                return 0;
            }
        }

        private float GetPlayedTime(Animator m_Ator, int layer, bool ignoreCurrentClip = false)
        {
            if (m_Ator != null)
            {
                if (ignoreCurrentClip)
                {
                    AnimatorStateInfo animationState = m_Ator.GetCurrentAnimatorStateInfo(layer);
                    return animationState.normalizedTime;
                }
                else
                {
                    List<AnimatorClipInfo> clips = CFEngine.ListPool<AnimatorClipInfo>.Get();
                    AnimatorStateInfo animationState = m_Ator.GetCurrentAnimatorStateInfo(layer);
                    m_Ator.GetCurrentAnimatorClipInfo(layer, clips);
                    float time = clips.Count > 0 ? clips[0].clip.length * animationState.normalizedTime : 0;
                    CFEngine.ListPool<AnimatorClipInfo>.Release(ref clips);
                    return time;
                }


            }
            else
            {
                return 0;
            }
        }

        private float GetNextTime(Animator m_Ator, int layer, bool ignoreCurrentClip = false)
        {
            if (m_Ator != null)
            {
                if (ignoreCurrentClip)
                {
                    AnimatorStateInfo animationState = m_Ator.GetNextAnimatorStateInfo(layer);
                    return animationState.normalizedTime;
                }
                else
                {
                    List<AnimatorClipInfo> clips = CFEngine.ListPool<AnimatorClipInfo>.Get();
                    AnimatorStateInfo animationState = m_Ator.GetNextAnimatorStateInfo(layer);
                    m_Ator.GetNextAnimatorClipInfo(layer, clips);
                    float time = clips.Count > 0 ? clips[0].clip.length * animationState.normalizedTime : 0;
                    CFEngine.ListPool<AnimatorClipInfo>.Release(ref clips);
                    return time;
                }


            }
            else
            {
                return 0;
            }
        }
        #endregion

        #region huge

        private List<List<float>> GetColliderByString(string str)
        {
            List<List<float>> col = new List<List<float>>(8);
            if (str.Length == 0) return col;

            var ids = str.Split('|');
            for (int i = 0; i < ids.Length; i++)
            {
                col.Add(new List<float>());
                var box = ids[i].Split('=');
                for (int j = 0; j < box.Length; j++)
                    col[i].Add(System.Convert.ToSingle(box[j]));
            }
            return col;
        }

        public void OnDrawGizmos()
        {
            if (presentData == null) return;
            if (!presentData.Huge) return;
            if (colliders == null) colliders = GetColliderByString(collidersStr);
            int count = -1;//累积，确认选中的盒子序号
            for (int i = 0; i < colliders.Count; ++i)
            {
                if ((int)colliders[i][0] == mBoxID) count++;
                else continue;
                Gizmos.color = count == mSelectedBoxIdx ? mSelectedColor : mNormalColor;//颜色
                Transform trans = obj.transform;
                Vector3 offset = trans.rotation * (new Vector3(colliders[i][1], colliders[i][2], colliders[i][3]) * presentData.Scale);
                float radius = colliders[i][4] * presentData.Scale;
                float height = colliders[i][5] * 0.5f * presentData.Scale;
                Vector3 pos = trans.position + offset;
                DrawCylinder(pos, radius, height);
            }
        }

        private void DrawCylinder(Vector3 center, float radius, float height)
        {
            DrawCircle(center + new Vector3(0, -height, 0), radius);
            DrawCircle(center + new Vector3(0, height, 0), radius);
            Gizmos.DrawLine(center + new Vector3(radius, -height, 0), center + new Vector3(radius, height, 0));
            Gizmos.DrawLine(center + new Vector3(-radius, -height, 0), center + new Vector3(-radius, height, 0));
            Gizmos.DrawLine(center + new Vector3(0, -height, -radius), center + new Vector3(0, height, -radius));
            Gizmos.DrawLine(center + new Vector3(0, -height, radius), center + new Vector3(0, height, radius));
        }

        private void DrawCircle(Vector3 _center, float _radius, int _lineNum = 60)
        {
            Vector3 forwardLine = Vector3.forward * _radius;
            Vector3 curPos = _center + forwardLine;
            Vector3 prePos = curPos;
            for (int i = 0; i < _lineNum; i++)
            {
                forwardLine = _radius * XCommon.singleton.HorizontalRotateVetor3(forwardLine, 360f / _lineNum);
                curPos = forwardLine + _center;
                Gizmos.DrawLine(prePos, curPos);
                prePos = curPos;
            }
        }
        #endregion

        #region CheckBlock
        public bool CheckBlock(ref float x, ref float y, ref float z, Vector3 oPos)
        {
            bool ischeck = false;

            if (!SkillHoster.GetHoster.SkillBlock) return false;

            if (Xuthus_VirtualServer.getState(id) != EditorEcs.XStateType.Skill)
            {
                return false;
            }

            Vector3 pos = new Vector3(x, y, z);
            for (ulong i = 1; i <= SkillHoster.GetHoster.Target; ++i)
            {
                if (i == id) continue;
                if (!SkillHoster.GetHoster.EntityDic.ContainsKey(i)) continue;

                Entity oppo = SkillHoster.GetHoster.EntityDic[i];

                if (id != 1 && i != 1) continue;

                if (colliders == null) colliders = GetColliderByString(collidersStr);
                if (colliders.Count != 0)
                {
                    int nState = Xuthus_VirtualServer.getCollisionType(i);

                    bool bStopped = false;
                    for (int k = 0; k < colliders.Count; ++k)
                    {
                        if ((int)colliders[k][0] != Xuthus_VirtualServer.getCollisionType(id)) continue;

                        Transform trans = oppo.obj.transform;
                        Vector3 oppoOffset = trans.rotation * (new Vector3(colliders[k][1], colliders[k][2], colliders[k][3]) * oppo.presentData.Scale);
                        float oppoRadius = colliders[k][4] * oppo.presentData.Scale;
                        float oppoHeight = colliders[k][5] * oppo.presentData.Scale;
                        Vector3 center = trans.position + oppoOffset;
                        if (center.y - oppoHeight * 0.5f > y + presentData.BoundHeight * presentData.Scale) continue;

                        int ret = CheckA(obj.transform.position, pos, center, presentData.BoundRadius * presentData.Scale, oppoRadius);
                        if (ret == 1)
                        {
                            x = pos.x;
                            y = pos.y;
                            z = pos.z;
                            ischeck = true;
                        }
                        else if (ret == 2)
                        {
                            Vector3 ori_pos = oPos;
                            x = ori_pos.x;
                            z = ori_pos.z;
                            ischeck = true;
                            bStopped = true;
                            break;
                        }
                    }
                    if (bStopped) break;
                }
                else
                {
                    int ret = CheckA(obj.transform.position, pos, oppo.obj.transform.position, presentData.BoundRadius * presentData.Scale, oppo.presentData.BoundRadius * oppo.presentData.Scale);
                    if (ret == 1)
                    {
                        x = pos.x;
                        y = pos.y;
                        z = pos.z;
                        ischeck = true;
                    }
                    else if (ret == 2)
                    {
                        Vector3 ori_pos = oPos;
                        x = ori_pos.x;
                        z = ori_pos.z;
                        ischeck = true;
                        break;
                    }
                }
            }

            return ischeck;
        }

        int CheckA(Vector3 src, Vector3 dst, Vector3 oppo, float r1, float r2)
        {
            Vector3 v1 = dst - oppo;
            v1.y = 0.0f;

            float r = r1 + r2;
            float d1 = Vector3.SqrMagnitude(v1);
            if (d1 - r * r < -0.001f)
            {
                return 2;
            }
            else
            {
                float coef = 0.7f;
                Vector3 v2 = dst - src;
                v2.y = 0.0f;
                float d2 = Vector3.SqrMagnitude(v2);
                if (d2 <= r * r * coef)
                {
                    return 0;
                }

                Vector3 dir = XCommon.singleton.Horizontal(v2);
                float len = Mathf.Sqrt(d2);
                float rot = dir == Vector3.zero ? 0.0f : Vector3.Angle(Vector3.right, dir);
                rot = (rot == 0.0f) ? 0.0f : (XCommon.singleton.Clockwise(Vector3.right, dir) ? -rot : rot);

                Vector3 rect_center = src + dir * (len * 0.5f);
                Vector3 v_rc1 = new Vector3(oppo.x - rect_center.x, 0.0f, oppo.z - rect_center.z);
                Vector3 v_rc2 = XCommon.singleton.HorizontalRotateVetor3(v_rc1, rot, false);
                if (XCommon.singleton.IsRectCycleCross(len * 0.5f, r1, v_rc2, r2))
                {
                    return 2;
                }
            }
            return 0;
        }
        #endregion

        private void UpdateMultipleDir(float deltaTime)
        {
            if (MoveFactor > 0)
            {
                MoveFactor = Mathf.Max(0, MoveFactor - Time.deltaTime * 10);
                SetMultipleDirParam(MoveDir.x * MoveFactor, MoveDir.z * MoveFactor);
            }
            else if (hasDir)
            {
                MoveDir = Vector3.zero;
                hasDir = false;
            }
        }

        public float mHeightOffset = 0;
        private void UpdatePosXZ()
        {
            posXZ.transform.position = new Vector3(obj.transform.position.x, startY, obj.transform.position.z) + Vector3.up * mHeightOffset;
        }

        public void CalMultipleDir(Vector3 dir, float face)
        {
            if (dir.x == 0 && dir.z == 0) return;

            if (useMultipleDir) MoveFactor = 2.0f;
            float t = ator.speed;
            t = t < 1 ? t * t : 1;
            MoveDir = Vector3.Lerp(MoveDir, XCommon.singleton.FloatToAngle(XCommon.singleton.AngleWithSign(XCommon.singleton.FloatToAngle(face), dir)), t);
            SetMultipleDirParam(MoveDir.x * MoveFactor, MoveDir.z * MoveFactor);
            hasDir = true;
        }

        public void StartMultipleDir()
        {
            MoveFactor = 2.0f;
            useMultipleDir = true;
        }

        public void SetMultipleDirParam(float x, float z)
        {
            if (useMultipleDir)
            {
                ator.SetFloat("MultipleDirFactorX", x);
                ator.SetFloat("MultipleDirFactorZ", z);
            }
        }

        public void EndMultipleDirParam(bool reset = false)
        {
            if (reset)
            {
                MoveFactor = 0.0f;
                ator.SetFloat("MultipleDirFactorX", 0.0f);
                ator.SetFloat("MultipleDirFactorZ", 0.0f);
            }

            useMultipleDir = false;
        }

        public ulong id = 0;
        private bool hasDir = false;
        private bool useMultipleDir = false;
        public Vector3 MoveDir = Vector3.zero;
        public float MoveFactor = 0.0f;


        private float startY;
        public GameObject obj;
        public GameObject posXZ;
        public Animator ator;
        public EffectPreviewContext previewContext;

        public XEntityPresentation.RowData presentData;
        public XEntityStatistics.RowData statisticsData;

        public string collidersStr { get { return colStr; } set { colStr = value; colliders = null; } }
        private string colStr;
        //用于在编辑器状态下读写collier（因为seqlistref是只读的）, ugly but work.
        List<List<float>> colliders;

        public XRuntimeFmod fmod = null;
        public CFDynamicBoneCore dynamicBone = new CFDynamicBoneCore();
        //private AudioChannel m_audioChannel;
        //private bool m_followRole;

        private Dictionary<AudioChannel, bool> m_followDict = new Dictionary<AudioChannel, bool>();


        //Gizmos画图用
        public static Color mNormalColor = new Color(0f, 1f, 0f, 1f);
        public static Color mSelectedColor = new Color(0f, 0f, 1f, 1f);
        public int mSelectedBoxIdx;
        public int mBoxID;
        public void PlaySound(AudioChannel channel, string eventName, bool followRole = true)
        {
            if (fmod == null)
            {
                fmod = XRuntimeFmod.GetFMOD();
                fmod.Init(obj, null);
            }
            fmod.StartEvent(eventName, channel, channel == AudioChannel.Skill);
            //m_audioChannel = channel;
            //m_followRole = followRole;
            m_followDict[channel] = followRole;
        }

        public void OnGUI()
        {
            using (new GUILayout.VerticalScope("box"))
            {
                if (GUILayout.Button($"Entity : {obj.name}"))
                {
                    Selection.activeGameObject = obj;
                }
                GUILayout.Label($"DynamicBone : {dynamicBone.weight:0.000}");
            }
        }
    }

    public class SkillEditorKeyboardData
    {
        [SerializeField]
        public List<int> PresentList = new List<int>();
        [SerializeField]
        public List<List<string>> SkillList = new List<List<string>>();

        [SerializeField]
        public List<string> MapKey_Skill = new List<string>();
        [SerializeField]
        public List<List<float>> MapValue_TriggerTime = new List<List<float>>();
        [SerializeField]
        public List<List<string>> MapValue_TriggerSkill = new List<List<string>>();

        public void CacheData()
        {
            string path = Application.dataPath + "/BundleRes/SkillPackage/SkillEditorKeyboard.ecfg";

            DataIO.SerializeData<SkillEditorKeyboardData>(path, this);
        }

        public static SkillEditorKeyboardData LoadData()
        {
            string path = Application.dataPath + "/BundleRes/SkillPackage/SkillEditorKeyboard.ecfg";

            return DataIO.DeserializeData<SkillEditorKeyboardData>(path);
        }

        public bool GetCombatSkill(string skill, ref List<float> triggerTime, ref List<string> triggerSkill)
        {
            for (int i = 0; i < MapKey_Skill.Count; ++i)
            {
                if (MapKey_Skill[i] == skill)
                {
                    triggerTime = MapValue_TriggerTime[i];
                    triggerSkill = MapValue_TriggerSkill[i];
                    return true;
                }
            }
            return false;
        }
    }

    public class SkillHoster : MonoBehaviour
    {
        private delegate void ActionDataCallBack<T>(T data) where T : EcsData.XConfigData;

        public static string PrefabPath = "Assets/BundleRes/Prefabs/";
        public static string ResourecePath = "Assets/BundleRes/";
        public static string CurvePath = "Assets/Editor/EditorResources/Server/";

        public Dictionary<ulong, uint> PresentDic = new Dictionary<ulong, uint>();
        public Dictionary<ulong, Entity> EntityDic = new Dictionary<ulong, Entity>();

        public List<EffectPreviewContext> DummyEntityDic = new List<EffectPreviewContext>();

        public Dictionary<ulong, AnimatorOverrideController> EntityAtorDic = new Dictionary<ulong, AnimatorOverrideController>();
        public Dictionary<uint, EcsData.XConfigData> ScriptDic = new Dictionary<uint, EcsData.XConfigData>();
        public Dictionary<uint, string> HitScriptHash = new Dictionary<uint, string>();
        private Dictionary<ulong, List<TimerData>> EntityTimers = new Dictionary<ulong, List<TimerData>>();
        private Dictionary<ulong, float> EntityRatio = new Dictionary<ulong, float>();
        private Dictionary<ulong, List<XEcsGamePlay.FxData>> FxDic = new Dictionary<ulong, List<XEcsGamePlay.FxData>>();

        public Queue<ulong> Targets = new Queue<ulong>();
        public static ulong PlayerIndex = 1;
        public static string MobSkillName = "";
        ulong _index = 1;
        public ulong Target { get { return _index == 2 ? 0 : (EntityDic.ContainsKey(_index - 1) ? _index - 1 : 0); } }

        public SkillCamera cameraComponent;
        public FreeLookController CFreeLook;
        public LevelCameraController CLevel;
        public SoloModeCameraController CSoloMix;
        public ulong SoloFollowIndex = 0;
        public MotionController CMotion;

        public bool SkillGraphInited = false;
        public bool HitGraphInited = false;
        public Queue<SkillDebugData> debugQueue = new Queue<SkillDebugData>();

        public float GroundHeight = 0.0f;
        public string partTag = "";

        //freelook相机控制，模拟战斗状态的变化
        private static bool isFighting = true;

        #region Net
        public string[] NetStr = new string[] { "Editor_0ms", "Low_50ms", "Middle_100ms", "High_200ms" };
        public int LagType = 0;
        public int FluctuationsType = 0;
        public float Lag = 0.0f;
        public float Fluctuations = 0.0f;
        #endregion
#if UNITY_EDITOR
        public delegate void AutoFireSkill();

        public AutoFireSkill AutoFire;
#endif

        public static void BuildSkillScene(string file = "")
        {
            if (string.IsNullOrEmpty(file))
            {
                EditorSceneManager.OpenScene("Assets/Scenes/Scenelib/Role_screencapture/Role_screencapture.unity");

                //EditorSceneManager.NewScene (NewSceneSetup.DefaultGameObjects);

                //GameObject plane = GameObject.CreatePrimitive (PrimitiveType.Plane);
                //plane.name = "Ground";
                //plane.layer = LayerMask.NameToLayer ("Terrain");
                //plane.transform.position = new Vector3 (0, -0.01f, 0);
                //plane.transform.localScale = new Vector3 (1000, 1, 1000);

                //plane.GetComponent<Renderer> ().sharedMaterial = AssetDatabase.LoadAssetAtPath<Material> ("Assets/Editor/EditorResources/PlaneMat.mat");
                //plane.GetComponent<Renderer> ().sharedMaterial.SetColor ("_Color", new Color (90 / 255.0f, 90 / 255.0f, 90 / 255.0f));
            }
            else
            {
                EditorSceneManager.OpenScene(file);
            }
            if (GameObject.Find("GamePoint") == null)
            {
                GameObject root = CFEngine.XGameObject.GetGameObject();
                root.name = "GamePoint";
                root.AddComponent<XFmodBus>();
            }
        }

        public delegate void GetExSkill(ref List<string> skillList, ref List<float> timeList, ref List<string> combatList);

        public static void CreateEntity(GameObject obj, string name, ulong id, Vector3 pos, int presentID)
        {
            XEntityPresentation.RowData data = XEntityPresentationReader.GetData((uint)presentID);
            GameObject entity = GameObject.Instantiate<GameObject>(obj, null);
            entity.name = name;
            entity.AddComponent<SkillResultSceneUI>();
            entity.AddComponent<SkillTargetSelectSceneUI>();
            entity.transform.position = pos;
            entity.transform.localScale = Vector3.one * data.Scale;

            DynamicCaster dynamicCaster = entity.AddComponent<DynamicCaster>();
            dynamicCaster.dynamicCastType = EShadowCasterType.MainCaster;

            var host = GetHosterAndCreate;
            host.EntityDic[id] = new Entity(entity, entity.GetComponent<Animator>(), id);

            host.PlayerPresentID = presentID;
        }

        public static void RefreshSkill(GetExSkill handler, bool create = true)
        {
            var host = create ? GetHosterAndCreate : GetHoster;
            if (host != null)
                handler(ref host._keyboard_skill, ref host._combat_time, ref host._combat_skill);
        }

        public static void BindCameraToEntity(bool newCamera = true, bool soloCamera = false, string cameraPath = "")
        {
            GameObject camera;
            if (newCamera)
            {
                camera = GameObject.Find("MainCamera");
                if (camera) DestroyImmediate(camera);

                camera = AssetDatabase.LoadAssetAtPath("Assets/Engine/Test/MainCamera.prefab", typeof(GameObject)) as GameObject;
                camera = GameObject.Instantiate<GameObject>(camera, null);
                camera.name = "MainCamera";

                camera = camera.transform.Find("Main Camera").gameObject;
            }
            else
            {
                camera = GameObject.Find("Main Camera");
            }
            camera.name = "Main Camera";
            camera.transform.localPosition = new Vector3(0, 3, -4);
            camera.transform.localEulerAngles = new Vector3(10, 0, 0);
            if (!camera.TryGetComponent<FMODUnity.StudioListener>(out var listener))
                camera.AddComponent<FMODUnity.StudioListener>();
            var pipelineRenderData = camera.GetComponent<UniversalAdditionalCameraData>();
            pipelineRenderData.renderPostProcessing = true;
            //---FXAA设置已经修改：请联系程庆宝
            //pipelineRenderData.antialiasing = AntialiasingMode.SubpixelMorphologicalAntiAliasing;
            camera.GetComponent<Camera>().allowHDR = true;
            camera.GetComponent<Camera>().allowMSAA = true;
            BuildCinemachine(soloCamera, cameraPath, camera);
        }

        private static void BuildCinemachine(bool soloCamera, string cameraPath, GameObject camera)
        {
            camera.AddComponent<CinemachineBrain>();
            if (!soloCamera)
            {
                GameObject freeLook = GameObject.Find("FreeLook_skillEditor(Clone)");
                if (freeLook) DestroyImmediate(freeLook);
                //                 if (!string.IsNullOrEmpty(cameraPath))
                //                 {
                //                     freeLook = AssetDatabase.LoadAssetAtPath(cameraPath, typeof(GameObject)) as GameObject;
                //                 }
                //                 else
                {
                    freeLook = AssetDatabase.LoadAssetAtPath("Assets/BundleRes/Prefabs/Cinemachine/FreeLook/FreeLook_skillEditor.prefab", typeof(GameObject)) as GameObject;
                }
                GameObject.Instantiate<GameObject>(freeLook, null);
            }
            else
            {
                GameObject soloMix = GameObject.Find("SoloMix(Clone)");
                if (soloMix) DestroyImmediate(soloMix);
                if (!string.IsNullOrEmpty(cameraPath))
                {
                    soloMix = AssetDatabase.LoadAssetAtPath(cameraPath, typeof(GameObject)) as GameObject;
                }
                else
                {
                    soloMix = AssetDatabase.LoadAssetAtPath("Assets/BundleRes/Prefabs/Cinemachine/Solo/SoloMix.prefab", typeof(GameObject)) as GameObject;
                }

                GameObject.Instantiate<GameObject>(soloMix, null).name = "SoloMix(Clone)";
            }

            GameObject motion = GameObject.Find("VirtualCamera(Clone)");
            if (motion) DestroyImmediate(motion);
            motion = AssetDatabase.LoadAssetAtPath("Assets/BundleRes/Prefabs/Cinemachine/VirtualCamera.prefab", typeof(GameObject)) as GameObject;
            motion = GameObject.Instantiate<GameObject>(motion, null);
            motion.transform.localPosition = new Vector3(0, 3, -4);
            motion.transform.localEulerAngles = new Vector3(10, 0, 0);
        }

        public ulong CreatePuppet(int presentID, float face = 0, float x = 0, float y = 0, float z = 1, ulong index = 0, int lod = 0)
        {
            index = index == 0 ? _index : index;
            GameObject obj = XEntityPresentationReader.GetDummy((uint)presentID);
            XEntityPresentation.RowData data = XEntityPresentationReader.GetData((uint)presentID);
            if (obj == null) return 0;
            obj = GetLodObject(obj, lod);

            GameObject entity = GameObject.Instantiate<GameObject>(obj, null);
            if (face == 0 && x == 0 && y == 0 && z == 1)
            {
                x = EntityDic[PlayerIndex].obj.transform.position.x;
                y = EntityDic[PlayerIndex].obj.transform.position.y;
                z = EntityDic[PlayerIndex].obj.transform.position.z + 3;
            }
            entity.transform.position = new Vector3(x, y, z);
            entity.transform.rotation = XCommon.singleton.FloatToQuaternion(face);
            entity.transform.localScale = Vector3.one * data.Scale;

            var newEntity = new Entity(entity, entity.GetComponent<Animator>(), index);
            EntityDic.Add(index, newEntity);
            EntityAtorDic.Add(index, BuildOverride(EntityDic[index].obj));
            PresentDic[index] = (uint)presentID;

            XEcsScriptEntity.CreatePuppet(index, face, x, y, z);

            EntityDic[index].presentData = data;
            EntityDic[index].collidersStr = EntityDic[index].presentData.HugeMonsterColliders.ToString();

            EntityDic[index].statisticsData = XEntityStatisticsReader.GetData((uint)presentID);
            for (int i = 0; i < data.BeHit.Count; ++i)
            {
                uint hash = XCommon.singleton.XHash(data.BeHit[i, 1]);
                XEcsScriptEntity.BindHit(index, int.Parse(data.BeHit[i, 0]), hash);
                HitScriptHash[hash] = Application.dataPath + "/BundleRes/HitPackage/" + data.BehitLocation + data.BeHit[i, 1] + ".bytes";
                if (File.Exists(HitScriptHash[hash]))
                {
                    ScriptDic[hash] = DataIO.DeserializeEcsData<ClientEcsData.XHitData>(GetHitScriptPath(hash));
                }
            }

            if (CSoloMix != null)
            {
                newEntity.mHeightOffset = data.BoundHeight * 0.72f;
                //EntityDic[1].mHeightOffset = 0;
                CSoloMix.LookAt = EntityDic[index].posXZ.transform;
            }

            // string[] mobName = MobSkillName.Split(',');
            // foreach (var i in mobName)
            // {
            //     string[] temp = i.Split('|');
            //     if (temp.Length >= 2)
            //     {
            //         int _presentID = int.Parse(temp[0]);
            //         Debug.LogError(index);

            //         if (_presentID == EntityDic[index].presentData.PresentID)
            //         {
            //             FireSkill("/BundleRes/SkillPackage/" + EntityDic[index].presentData.SkillLocation + temp[1] + ".bytes", index);
            //         }
            //     }
            // }

            return index == _index ? _index++ : index;
        }


        private GameObject GetLodObject(GameObject obj, int currentLod = 0)
        {
            string path = AssetDatabase.GetAssetPath(obj);
            path = path.Replace(".prefab", "");
            switch (currentLod)
            {
                case 0:
                    return obj;
                case 1:
                    path += "_lod1";
                    break;
                case 2:
                    path += "_lod2";
                    break;
            }

            UnityEngine.Object newObject = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path + ".prefab");
            if (newObject == null) return obj;
            return newObject as GameObject;
        }

        private bool need_refresh = false;
        public void RefreshHitScript()
        {
            if (!Application.isPlaying) return;
            if (!PresentDic.ContainsKey(Target)) return;
            if (XEcs.singleton.CurrentState(Target) == XStateType.Hit)
            {
                need_refresh = true;
                return;
            }

            need_refresh = false;
            XEntityPresentation.RowData data = XEntityPresentationReader.GetData(PresentDic[Target]);
            for (int i = 0; i < data.BeHit.Count; ++i)
            {
                uint hash = XCommon.singleton.XHash(data.BeHit[i, 1]);
                XEcsScriptEntity.Reload(Target, hash);
                ScriptDic[hash] = DataIO.DeserializeEcsData<ClientEcsData.XHitData>(GetHitScriptPath(hash));
            }
        }

        public void SetActionRatio(float ratio)
        {
            XEcsScriptEntity.SetActionRatio(PlayerIndex, ratio);
        }

        public float TryGetTerrainY(ref Vector3 pos)
        {
            float posY = pos.y;

            return GroundHeight;
        }

        bool isLoadGameAtHere = false;
        private ClientEcsData.SkillSFXContext sfxContext;
        void Start()
        {
            EngineContext.IsRunning = false;
            var c = Camera.main;
            if (c != null && c.TryGetComponent(out EnvironmentExtra env))
            {
                isLoadGameAtHere = env.loadGameAtHere;
                if (isLoadGameAtHere)
                    return;
            }
            isLoadGameAtHere = false;
            SFXMgr.GetSceneY = TryGetTerrainY;
            Application.targetFrameRate = EnvironmentExtra.frameRate.Value;
            // XFxMgr.singleton.Init ();
            InitEcs();
            InitPlayer();
            InitCinemachine();

            InitHandler();
            XCommon.InitFModBus();

            sfxContext.Init();
            sfxContext.getAudio = GetAudioEvent;
            sfxContext.getSpeedRatio = GetEntityRatio;
            sfxContext.getTargetTrans = GetTargetTrans;
            SFXAnimationEventManager.getEntity = GetPreviewContext;
            SFXAnimationEventManager.SetPartActiveState = SkillHelper.PlayActiveHideEffect;
            SkillHelper.saContext.Init();
            SFXMgr.GetEngineSceneType = () => 0;
            ComputeBufferMgr.singleton.OnEnterScene();
            GetEntity();
        }

        private void InitCinemachine()
        {
            GameObject FLobj = GameObject.Find("FreeLook_skillEditor(Clone)");
            if (FLobj != null)
            {
                CFreeLook = GameObject.Find("FreeLook_skillEditor(Clone)").GetComponent<FreeLookController>();
                CFreeLook.transform.position = Vector3.zero;
                CFreeLook.Init();
                CFreeLook.Follow = EntityDic[PlayerIndex].obj.transform;
                CFreeLook.LookAt = EntityDic[PlayerIndex].obj.transform;
                var data = EntityDic[PlayerIndex].presentData;
                CFreeLook.ChangeLookat(null);
                CFreeLook.BackToFollow(0);
                //CFreeLook.SetTableParamData(EntityDic[PlayerIndex].presentData.CameraScaleParam);
                CFreeLook.IsFighting = isFighting;

                CMotion = GameObject.Find("VirtualCamera(Clone)").GetComponent<MotionController>();
                CMotion.Init();
                CMotion.name = "MotionCamera";
                CMotion.Enable = false;
            }

            GameObject Soloobj = GameObject.Find("SoloMix(Clone)");
            if (Soloobj != null)
            {
                CSoloMix = GameObject.Find("SoloMix(Clone)").GetComponent<SoloModeCameraController>();
                CSoloMix.Init();

                //target.localPosition = new Vector3(0, EntityDic[PlayerIndex].presentData.BoundHeight*0.72f, 0);
                CSoloMix.Follow = EntityDic[PlayerIndex].posXZ.transform;
                CSoloMix.LookAt = EntityDic[PlayerIndex].posXZ.transform;

                SoloFollowIndex = PlayerIndex;
                //CSoloMix.SetTableParamData(EntityDic[SoloFollowIndex].presentData.CameraScaleParam);

                if (CMotion == null)
                {
                    CMotion = GameObject.Find("VirtualCamera(Clone)").GetComponent<MotionController>();
                    CMotion.Init();
                    CMotion.name = "MotionCamera";
                    CMotion.Enable = false;
                }
            }

            GameObject Aerialobj = GameObject.Find("Aerial_skillEditor(Clone)");
            if (Aerialobj != null)
            {
                CLevel = Aerialobj.GetComponent<LevelCameraController>();
                CLevel.Init();
                CLevel.Follow = EntityDic[PlayerIndex].obj.transform;
            }
        }

        public void ResetCinemachine()
        {
            if (CFreeLook != null)
            {
                CFreeLook.Follow = EntityDic[PlayerIndex].obj.transform;
                CFreeLook.LookAt = EntityDic[PlayerIndex].obj.transform;
            }
            if (CSoloMix != null)
            {
                CSoloMix.Follow = EntityDic[PlayerIndex].posXZ.transform;
            }
            if (CLevel != null)
            {
                CLevel.Follow = EntityDic[PlayerIndex].obj.transform;
            }
        }

        private void InitPlayer()
        {
            EntityDic.Add(_index, new Entity(GameObject.Find("Player"), GameObject.Find("Player").GetComponent<Animator>(), _index));
            EntityAtorDic.Add(_index, BuildOverride(EntityDic[_index].obj));
            cameraComponent = new SkillCamera(EntityDic[PlayerIndex].obj);
            SkillHashMap[XCommon.singleton.XHash("SkillBackup")] = "/Editor Default Resources/SkillBackup/SkillBackup.bytesRT";
            PresentDic.Add(_index, (uint)DataIO.DeserializeEcsData<ClientEcsData.XSkillData>(GetScriptPath(XCommon.singleton.XHash("SkillBackup"))).PresentID);
            EntityDic[_index].presentData = XEntityPresentationReader.GetData(PresentDic[_index]);
            EntityDic[_index].collidersStr = EntityDic[_index].presentData.HugeMonsterColliders.ToString();
            EntityDic[_index].statisticsData = XEntityStatisticsReader.GetData(PresentDic[_index]);

            XEcsScriptEntity.Create(_index, XCommon.singleton.XHash("SkillBackup"), 0,
                EntityDic[_index].obj.transform.position.x,
                EntityDic[_index].obj.transform.position.y,
                EntityDic[_index].obj.transform.position.z);
            GroundHeight = EntityDic[_index].obj.transform.position.y;
            _index++;
        }

        private static void InitEcs()
        {
            XVirtualNet.singleton.Init();
            XEcsScriptEntity.Start();
            XEcsScriptEntity.SetDebug(true);
        }

        XTimerMgr.ElapsedEventHandler killFxHandler;
        RatioChangeEventHandler fxRatioChangedHandler;
        ValidEventHandler fxValidHandler;

        XTimerMgr.ElapsedEventHandler killBulletHandler;

        XTimerMgr.ElapsedEventHandler stopAudioHandler;
        RatioChangeEventHandler audioRatioChangedHandler;

        private void InitHandler()
        {
            killFxHandler = KillFx;
            fxRatioChangedHandler = OnFxRatioChanged;
            fxValidHandler = FxValid;

            killBulletHandler = KillBullet;

            stopAudioHandler = StopAudio;
            audioRatioChangedHandler = OnAudioRatioChanged;
        }
        Vector3 preMouse = Vector3.zero;
        bool Draging = false;

        static bool pauseAtNext = false;
        public static bool targetSelectSceneUI = false;
        public static bool hideSceneUI = false;
        public static bool aiMode = false;
        public static bool targetControl = false;
        public static bool useAerialCamera = false;
        public static float aerialDistance = 15;
        private static bool autoFirer = false;
        [SerializeField]
        public float autoFirerTime = 1;
        private static bool fireSkill = false;

        static bool run = false;
        Vector3 forward;
        Vector3 right;
        int frameCount = 0;
        float totalTime = 0f;
        // Update is called once per frame
        void Update()
        {
            if (isLoadGameAtHere)
                return;

            ++frameCount;
            totalTime += Time.deltaTime;

            UpdateEcsSkillControl();

#if UNITY_EDITOR
            if (AutoFire != null) AutoFire();
            UpdateKeyBoardSkills();
#else
            UpdateKeyBoardSkills();
#endif

            XEcsScriptEntity.Update(Time.deltaTime);

            if (Input.GetKeyDown(KeyCode.P))
            {
                pauseAtNext = !pauseAtNext;
                RenderContext.pausePostProcess = pauseAtNext;
            }
            if (Input.GetKeyDown(KeyCode.R))
                useAerialCamera = !useAerialCamera;
            if (Input.GetKeyDown(KeyCode.H))
                hideSceneUI = !hideSceneUI;
            if (Input.GetKeyDown(KeyCode.I))
                aiMode = !aiMode;
            if (Input.GetKeyDown(KeyCode.RightBracket))
                targetControl = !targetControl;

            UpdateEcsMoveControl();

            UpdateFreeLook();

            XTimerMgr.singleton.Update(Time.deltaTime);

            CheckEditorEntityRemove();

            UpdateEntity();

            SkillHelper.UpdateEnvEffect();
        }

        private void OnDrawGizmos()
        {
            foreach (Entity e in EntityDic.Values)
            {
                e.OnDrawGizmos();
            }
        }

        private void CheckEditorEntityRemove()
        {
            bool flag;
            do
            {
                flag = false;
                foreach (ulong key in EntityDic.Keys)
                {
                    if (EntityDic[key].obj == null)
                    {
                        DestoryEntity(key);

                        flag = true;
                        break;
                    }
                }
            } while (flag);
        }

        private void UpdateEntity()
        {
            foreach (Entity e in EntityDic.Values)
            {
                e.Update(Time.deltaTime);
            }
            foreach (var e in DummyEntityDic)
            {
                e.Update();
            }
        }

        private void UpdateFreeLook()
        {
            //Debug.Log(1);
            if (CFreeLook)
            {
                if (Input.GetMouseButtonDown(0)) Draging = true;
                if (Input.GetMouseButtonUp(0)) Draging = false;
                if (Draging)
                {
                    if (preMouse != Vector3.zero)
                    {
                        Vector3 delta = Input.mousePosition - preMouse;
                        CFreeLook.xInputAxisValue = delta.x;
                        CFreeLook.yInputAxisValue = delta.y;
                    }
                    preMouse = Input.mousePosition;
                }
                else
                {
                    preMouse = Vector3.zero;
                    CFreeLook.xInputAxisValue = 0;
                    CFreeLook.yInputAxisValue = 0;
                }
            }
            else if (CSoloMix)
            {
                if (CSoloMix.HasPOV())
                {
                    if (Input.GetMouseButtonDown(0)) Draging = true;
                    if (Input.GetMouseButtonUp(0)) Draging = false;
                    if (Draging)
                    {
                        if (preMouse != Vector3.zero)
                        {
                            Vector3 delta = Input.mousePosition - preMouse;
                            CSoloMix.xInputAxisValue = delta.x;
                            CSoloMix.yInputAxisValue = delta.y;
                        }
                        preMouse = Input.mousePosition;
                    }
                    else
                    {
                        preMouse = Vector3.zero;
                        CSoloMix.xInputAxisValue = 0;
                        CSoloMix.yInputAxisValue = 0;
                    }
                }
            }

            // Adjust Aerial camera's distance
            if (CLevel != null)
            {
                var framingTransposer = CLevel.gameObject.GetComponent<CinemachineVirtualCamera>().GetCinemachineComponent<CinemachineFramingTransposer>();
                if (framingTransposer != null)
                    framingTransposer.m_CameraDistance = aerialDistance;
            }

            // Switch between FreeLook camera and Aerial camera
            if (CLevel != null && useAerialCamera)
            {
                CLevel.gameObject.GetComponent<CinemachineVirtualCamera>().Priority = 6;
            }
            else if (CLevel != null && !useAerialCamera)
            {
                CLevel.gameObject.GetComponent<CinemachineVirtualCamera>().Priority = 4;
            }
        }

        private void UpdateEcsMoveControl()
        {
            int nv = 0;
            int nh = 0;
            bool prerun = run;
            run = false;
            if (Input.GetKey(KeyCode.W))
            {
                run = true;
                nv = 1;
            }
            if (Input.GetKey(KeyCode.D))
            {
                nh = 1;
                run = true;
            }
            if (Input.GetKey(KeyCode.S))
            {
                nv = -1;
                run = true;
            }
            if (Input.GetKey(KeyCode.A))
            {
                nh = -1;
                run = true;
            }

            //if (prerun ^ run)
            if (cameraComponent != null && cameraComponent.CameraObject)
            {
                forward = cameraComponent.CameraObject.transform.forward;
                right = cameraComponent.CameraObject.transform.right;
                forward.y = 0;
                right.y = 0;
                forward.Normalize();
                right.Normalize();
            }

            if (run) XEcsScriptEntity.OnMove(
                targetControl ? Target : PlayerIndex,
                XCommon.singleton.AngleToFloat(nv * forward + nh * right),
                EntityDic[targetControl ? Target : PlayerIndex].obj.transform.position);

            else if (run ^ prerun) XEcsScriptEntity.OnStop(
                targetControl ? Target : PlayerIndex,
                XCommon.singleton.AngleToFloat(nv * forward + nh * right),
                EntityDic[targetControl ? Target : PlayerIndex].obj.transform.position);
        }

        private void UpdateEcsSkillControl()
        {
            if (Input.GetKeyDown(PlayerSkillKey) || Input.GetKeyDown(KeyCode.C) || fireSkill)
            {
                if (XEcs.singleton.CurrentState(PlayerIndex) != XStateType.Hit)
                {
                    var player = GameObject.Find("Player");
                    EngineContext.instance.mainRoleTrans = player != null ? player.transform : null;
                    fireSkill = false;
                    XEcsScriptEntity.EndSkill(PlayerIndex);
                    if (pauseAtNext) EditorApplication.ExecuteMenuItem("Edit/Pause");
                    XEcsScriptEntity.Reload(PlayerIndex, XCommon.singleton.XHash("SkillBackup"));
                    if (need_refresh) RefreshHitScript();
                    ScriptDic[XCommon.singleton.XHash("SkillBackup")] = DataIO.DeserializeEcsData<ClientEcsData.XSkillData>(GetScriptPath(XCommon.singleton.XHash("SkillBackup")));
                    SkillGraphInited = false;
                    debugQueue.Clear();
                    XEcsScriptEntity.BindSkill(PlayerIndex, XCommon.singleton.XHash("SkillBackup"));
                    XEcsScriptEntity.OnSkill(PlayerIndex, aiMode ? Target : ulong.MaxValue);
                    AddSelection(player, true);
                    SkillResultEditor.resultData = null;
                    frameCount = 0;
                    totalTime = 0;
#if UNITY_EDITOR
                    if (OverdrawMonitor.isOn)
                    {
                        ScriptDic.TryGetValue(XCommon.singleton.XHash("SkillBackup"), out XConfigData data);
                        if (data != null) OverdrawMonitor.Instance.StartObserveProfile(1, data.Name);
                    }
#endif

                    if (AssistCount > 1)
                    {
                        DestoryEntity(1001);
                        CreatePuppet(assistPresentid1, 0, EntityDic[PlayerIndex].obj.transform.position.x + 7.04f, 0, EntityDic[PlayerIndex].obj.transform.position.z + 5.04f, 1001);
                        FireSkill("/BundleRes/SkillPackage/" + EntityDic[1001].presentData.SkillLocation +
                            EntityDic[1001].presentData.SkillLocation.Remove(EntityDic[1001].presentData.SkillLocation.Length - 1) + "_combo.bytes", 1001);
                    }
                    if (AssistCount > 2)
                    {
                        DestoryEntity(1002);
                        CreatePuppet(assistPresentid2, 0, EntityDic[PlayerIndex].obj.transform.position.x - 7.53f, 0, EntityDic[PlayerIndex].obj.transform.position.z + 5.27f, 1002);
                        FireSkill("/BundleRes/SkillPackage/" + EntityDic[1002].presentData.SkillLocation +
                            EntityDic[1002].presentData.SkillLocation.Remove(EntityDic[1002].presentData.SkillLocation.Length - 1) + "_combo.bytes", 1002);
                    }

                    if (Input.GetKeyDown(KeyCode.C)) TriggerCombatSkill();
                }
            }
        }

        KeyCode TargetDashKey { get { return targetControl ? KeyCode.Space : KeyCode.T; } }
        KeyCode PlayerSkillKey { get { return targetControl ? KeyCode.T : KeyCode.Space; } }

        #region SkillEx
        public Dictionary<uint, string> SkillHashMap = new Dictionary<uint, string>();
        public List<string> _keyboard_skill = new List<string>();
        public List<float> _combat_time = new List<float>();
        public List<string> _combat_skill = new List<string>();
        public int PlayerPresentID = 0;

        private string _combat = null;
        private void UpdateKeyBoardSkills()
        {
#if UNITY_EDITOR
            for (int i = 0; i < _keyboard_skill.Count; ++i)
                UpdateKeyBoardSkill(KeyCode.Alpha1 + i);
#else
            for (int i = 0; i < _keyboard_skill.Count; ++i)
                UpdateKeyBoardSkill(KeyCode.Alpha1 + i);
#endif
            if (_combat != null)
            {
                FireSkill(_combat, PlayerIndex);
                _combat = null;
            }

            if (Input.GetKeyDown(TargetDashKey))
            {
                if (Target != 0)
                    FireSkill("/BundleRes/SkillPackage/" + EntityDic[Target].presentData.SkillLocation +
                        EntityDic[Target].presentData.SkillLocation.Remove(EntityDic[Target].presentData.SkillLocation.Length - 1) + "_dash.bytes", Target);
            }
        }

        private void UpdateKeyBoardSkill(KeyCode key)
        {
            if (Input.GetKeyDown(key))
            {
                int index = key - KeyCode.Alpha1;
                FireSkill(_keyboard_skill[index], PlayerIndex);
            }
        }

        public bool LoadSkillData(string skill)
        {
            uint hash = XCommon.singleton.XHash(Path.GetFileNameWithoutExtension(skill));
            if (hash == 0) return false;
            SkillHashMap[hash] = skill;
            ScriptDic[hash] = DataIO.DeserializeEcsData<ClientEcsData.XSkillData>(GetScriptPath(hash));
            if (ScriptDic[hash] == null) return false;
            return true;
        }

        public void FireSkill(string skill, ulong id)
        {
            if (!LoadSkillData(skill)) return;

            XEcsScriptEntity.EndSkill(id);
            XEcsScriptEntity.BindSkill(id, XCommon.singleton.XHash(Path.GetFileNameWithoutExtension(skill)));
            XEcsScriptEntity.OnSkill(id, (id == PlayerIndex && aiMode) ? Target : ulong.MaxValue);
#if UNITY_EDITOR
            if (OverdrawMonitor.isOn)
            {
                OverdrawMonitor.Instance.StartObserveProfile(1, skill);
            }
#endif
        }

        private void SetCombat(object obj)
        {
            _combat = (string)obj;
        }

        List<uint> combatToken = new List<uint>();
        private void TriggerCombatSkill()
        {
            for (int i = 0; i < combatToken.Count; ++i) XTimerMgr.singleton.KillTimer(combatToken[i]);
            combatToken.Clear();
            for (int i = 0; i < _combat_time.Count; ++i)
            {
                combatToken.Add(XTimerMgr.singleton.SetTimer(_combat_time[i], SetCombat, _combat_skill[i]));
            }
        }
        #endregion

        public void DestoryEntity(ulong id)
        {
            if (!EntityDic.ContainsKey(id)) return;
            if (EntityDic[id].obj != null)
            {
                GameObject.DestroyImmediate(EntityDic[id].obj);
            }
            XEcsScriptEntity.EndSkill(id);
            XEcsScriptEntity.Destroy(id);
            EntityDic.Remove(id);
            EntityAtorDic.Remove(id);
        }

        public bool SkillBlock = false;
        public bool showGUI = true;
        bool assist = false;
        int assistPresentid1 = 0;
        int assistPresentid2 = 0;
        public int AssistCount
        {
            get
            {
                int count = 1;
                count += SkillHoster.GetHoster.assistPresentid1 != 0 ? 1 : 0;
                count += SkillHoster.GetHoster.assistPresentid2 != 0 ? 1 : 0;
                return count;
            }
        }
        public float moveSpeed = 0;
        public string msStr = "";
        public float rotateSpeed = 0;
        public string rsStr = "";
        public bool UsePhoneAnimLoad = false;
        private void OnGUI()
        {
            showGUI = GUILayout.Toggle(showGUI, "");
            if (showGUI)
            {
                GUILayout.TextArea("StatisticsID: " + ((EntityDic[PlayerIndex].statisticsData == null) ? "NULL" : EntityDic[PlayerIndex].statisticsData.ID.ToString()));
                GUILayout.TextArea("DeltaTime: " + ((int)(Time.deltaTime * 1000) / 1000f).ToString());
                GUILayout.TextArea("Version: " + EditorEcs.Xuthus_VirtualServer._FW_VERSION_);
                GUILayout.TextArea("Frame: " + (frameCount).ToString());
                GUILayout.TextArea("TotalTime(" + ((int)(totalTime / 0.03333f)).ToString() + "): " + ((int)(totalTime * 1000) / 1000f).ToString());
                pauseAtNext = GUILayout.Toggle(pauseAtNext, "PausePlay (Key: P)");
                hideSceneUI = GUILayout.Toggle(hideSceneUI, "HideSceneUI (Key: H)");
                targetSelectSceneUI = GUILayout.Toggle(targetSelectSceneUI, "TargetSelectSceneUI");
                aiMode = GUILayout.Toggle(aiMode, "AiMode (Key: I)");
                targetControl = GUILayout.Toggle(targetControl, "TargetControl (Key: ])");
                useAerialCamera = GUILayout.Toggle(useAerialCamera, "AerialCamera (Key: R)");
                if (useAerialCamera) aerialDistance = GUILayout.HorizontalSlider(aerialDistance, 5, 40);
                SkillBlock = GUILayout.Toggle(SkillBlock, "SkillBlock");
                if (Target == 0) targetControl = false;
                autoFirer = GUILayout.Toggle(autoFirer, "AutoFirer");

                assist = GUILayout.Toggle(assist, "Assist");
                if (assist)
                {
                    try
                    {
                        assistPresentid1 = int.Parse(GUILayout.TextArea((assistPresentid1).ToString()));
                        assistPresentid2 = int.Parse(GUILayout.TextArea((assistPresentid2).ToString()));
                        if (assistPresentid1 == 0) assistPresentid2 = 0;
                    }
                    catch { }
                }
                UsePhoneAnimLoad = GUILayout.Toggle(UsePhoneAnimLoad, "UsePhoneAnimLoad");

                foreach (Entity e in EntityDic.Values)
                {
                    e.OnGUI();
                }

                GUILayout.BeginHorizontal();
                msStr = GUILayout.TextArea(msStr);
                if (GUILayout.Button("SetMoveSpeed"))
                {
                    try
                    {
                        moveSpeed = float.Parse(msStr == "" ? "0" : msStr);
                    }
                    catch
                    {

                    }
                }
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                rsStr = GUILayout.TextArea(rsStr);
                if (GUILayout.Button("SetRotateSpeed"))
                {
                    try
                    {
                        rotateSpeed = float.Parse(rsStr == "" ? "0" : rsStr);
                    }
                    catch
                    {

                    }
                }
                GUILayout.EndHorizontal();

                attackRange = GUILayout.TextArea(attackRange);

                if (GUILayout.Button("PullNPushCamera"))//freelook推拉镜头
                {
                    isFighting = !isFighting;
                    if (CFreeLook != null)
                        CFreeLook.IsFighting = isFighting;
                }


                if (GUILayout.Button("Freelook参数切换"))//freelook三档参数
                {
                    if (CFreeLook != null)
                    {
                        paramIndex = (paramIndex + 1) % 3;
                        CFreeLook.ChangeLookat(param[paramIndex]);
                    }
                }
            }
        }
        private int paramIndex = 0;
        private float[][] param ={
            new float[]{1.2f,4,2,1,6,6,3,0.5f,0.5f,0.5f},
            new float[]{3,5,3,1,8,8,3,0.5f,0.5f,0.5f},
            new float[]{7,10,8,2,20,20,3,0.5f,0.5f,0.5f},
        };
        public string attackRange = "AttackRange";

        void LateUpdate()
        {
            if (cameraComponent != null)
            {
                cameraComponent.PostUpdate(Time.deltaTime);
                XEcsScriptEntity.PostUpdate();
                ComputeBufferMgr.singleton.PostUpdate();
            }

            var context = EngineContext.instance;
            if (context != null)
            {
                RenderEffectSystem.Update(context);
                SplitScreenSystem.OnUpdate(context);
            }
        }

        void OnApplicationQuit()
        {
            for (ulong i = PlayerIndex; i < _index; ++i)
                XEcsScriptEntity.Destroy(i);
            _index = PlayerIndex;

            XEcsScriptEntity.Quit();
        }

        public static SkillHoster GetHosterAndCreate
        {
            get
            {
                GameObject camera = GameObject.Find("Main Camera");
                if (camera == null) return null;
                var host = camera.GetComponent<SkillHoster>();
                if (host == null)
                {
                    host = camera.gameObject.AddComponent<SkillHoster>();
                }
                return host;
            }
        }

        public static SkillHoster GetHoster
        {
            get
            {
                GameObject camera = GameObject.Find("Main Camera");
                if (camera == null) return null;
                return camera.GetComponent<SkillHoster>();
            }
        }

        public Entity GetEntity()
        {
            return GetEntity(PlayerIndex);
        }

        private Entity GetEntity(ulong id)
        {
            if (EntityDic.TryGetValue(id, out Entity e))
            {
                if (e.previewContext == null)
                {
                    e.previewContext = new EffectPreviewContext();
                }

                if (!e.previewContext.entityInit)
                {
                    EffectConfig.InitEffect(e.previewContext, e.obj, EffectConfig.instance);
                }

                e.previewContext.fromId = id;
                EffectConfig.PostInit(e.previewContext, partTag);

                return e;
            }
            return null;
        }

        public EffectPreviewContext GetPreviewContext(ulong id)
        {
            return GetEntity(id)?.previewContext;
        }

        public EffectPreviewContext GetPlayerPreviewContext()
        {
            return GetPreviewContext(PlayerIndex);
        }

        public void OnNodeChange(ulong id, uint hash, int index)
        {
            debugQueue.Enqueue(new SkillDebugData() { hash = hash, index = index });

        }

        public AnimatorOverrideController BuildOverride(GameObject obj)
        {

            Animator _ator = obj.GetComponent<Animator>();
            if (_ator == null)
            {
                _ator = obj.AddComponent<Animator>();
                _ator.runtimeAnimatorController = AssetDatabase.LoadAssetAtPath<UnityEditor.Animations.AnimatorController>("Assets/BundleRes/Controller/XAnimator.controller");
            }
            if (_ator.runtimeAnimatorController is AnimatorOverrideController)
            {

            }
            else
            {
                AnimatorOverrideController oVerrideController = new AnimatorOverrideController();
                oVerrideController.runtimeAnimatorController = _ator.runtimeAnimatorController;
                _ator.runtimeAnimatorController = oVerrideController;
            }

            return _ator.runtimeAnimatorController as AnimatorOverrideController;
        }

        public AnimatorOverrideController BuildOverride(GameObject from, GameObject to)
        {
            Animator _ator = to.GetComponent<Animator>();
            if (_ator == null)
            {
                _ator = to.AddComponent<Animator>();
                _ator.runtimeAnimatorController = AssetDatabase.LoadAssetAtPath<UnityEditor.Animations.AnimatorController>("Assets/BundleRes/Controller/XAnimator.controller");
            }
            if (_ator.runtimeAnimatorController is AnimatorOverrideController)
            {

            }
            else
            {
                AnimatorOverrideController oVerrideController = new AnimatorOverrideController();
                oVerrideController.runtimeAnimatorController = _ator.runtimeAnimatorController;
                _ator.runtimeAnimatorController = from.GetComponent<Animator>().runtimeAnimatorController;
            }

            return _ator.runtimeAnimatorController as AnimatorOverrideController;
        }

        public void SyncPos(ulong id, Vector3 pos, float face)
        {
            EntityDic[id].CalMultipleDir(pos - EntityDic[id].obj.transform.position, face);

            EntityDic[id].obj.transform.position = pos;
            EntityDic[id].obj.transform.eulerAngles = new Vector3(0, face, 0);
        }

        private void FireMainSkill(object o)
        {
            fireSkill = true;
        }

        public void OnSkillBegin(ulong id)
        {
        }

        public void OnSkillEnd(ulong id)
        {

            if (id == PlayerIndex && autoFirer)
            {
                XTimerMgr.singleton.SetTimer(autoFirerTime, FireMainSkill, null);
            }

            // if (CFreeLook != null) CFreeLook.Enable = true;
            if (CSoloMix != null) CSoloMix.Enable = true;
            if (_fov_id == id) cameraComponent.Reset(id, true, _fov_end_blend_time);

            List<TimerData> timerList;
            if (EntityTimers.TryGetValue(id, out timerList))
            {
                for (int i = timerList.Count - 1; i >= 0; --i)
                {
                    XTimerMgr.singleton.FireTimer(timerList[i].token);
                    if (!timerList[i].Valid())
                    {
                        CommonObject<TimerData>.Release(timerList[i]);
                        timerList.RemoveAt(i);
                    }
                }
            }

            List<FxData> fxList;
            if (FxDic.TryGetValue(id, out fxList))
            {
                for (int i = fxList.Count - 1; i >= 0; --i)
                {
                    if (!fxList[i].Valid)
                    {
                        fxList.RemoveAt(i);
                    }
                }
            }

            Entity e;
            if (EntityDic.TryGetValue(id, out e))
            {
                e.EndMultipleDirParam();
            }
            SkillHelper.OnEndSkill(id, e != null ? e.previewContext : null);
            RenderContext.pausePostProcess = false;
            DummyEntityDic.Clear();
#if UNITY_EDITOR
            if (OverdrawMonitor.isOn)
            {
                OverdrawMonitor.Instance.EndObserveProfile(1);
            }
#endif
        }

        public AnimationClip LoadPhoneAnimClip(string path)
        {
            return AssetDatabase.LoadAssetAtPath(ResourecePath + path + ".anim", typeof(AnimationClip)) as AnimationClip;
        }

        public void OverrideAnimClip(ulong id, string motion, string clip)
        {
            AnimatorOverrideController controller = EntityAtorDic[id];
            if (!UsePhoneAnimLoad)
            {
                AnimtionWrap wrap = AssetDatabase.LoadAssetAtPath(ResourecePath + "Editor" + clip + ".asset", typeof(CFEngine.AnimtionWrap)) as AnimtionWrap;
                if (wrap != null)
                    controller[motion] = wrap.clip;
            }
            else
            {
                controller[motion] = LoadPhoneAnimClip(clip);
            }
        }

        public void OverrideAnimClip(ulong id, string motion, uint hash, int index)
        {
            EcsData.XConfigData data = null;
            ScriptDic.TryGetValue(hash, out data);
            if (data != null)
            {
                if (data is ClientEcsData.XSkillData) OverrideAnimClip(id, motion, (data as ClientEcsData.XSkillData).AnimationData, index);
                else OverrideAnimClip(id, motion, (data as ClientEcsData.XHitData).AnimationData, index);
            }
        }

        int artHash = Animator.StringToHash("Attack Layer.Art");
        private void OverrideAnimClip(ulong id, string motion, List<EcsData.XAnimationData> list, int index)
        {
            Entity entity = EntityDic[id];

            if (SkillHelper.GetDataIndex<EcsData.XAnimationData>(list, ref index))
            {
                EcsData.XAnimationData data = list[index];

                if (entity.ator == null) return;

                int nextHash = entity.ator.GetNextAnimatorStateInfo(1).fullPathHash;
                int curHash = entity.ator.GetCurrentAnimatorStateInfo(1).fullPathHash;
                int stateHash = nextHash == 0 ? curHash : nextHash;
                bool artSkill = stateHash != artHash;
                motion = motion == "Art" ? (artSkill ? "Art" : "Art1") : motion;

                OverrideAnimClip(id, motion, data.ClipPath);
                if (data.MultipleDirection)
                {
                    OverrideAnimClip(id, artSkill ? "Forward" : "Forward1", data.Forward);
                    OverrideAnimClip(id, artSkill ? "RightForward" : "RightForward1", data.RightForward);
                    OverrideAnimClip(id, artSkill ? "Right" : "Right1", data.Right);
                    OverrideAnimClip(id, artSkill ? "RightBack" : "RightBack1", data.RightBack);
                    OverrideAnimClip(id, artSkill ? "Back" : "Back1", data.Back);
                    OverrideAnimClip(id, artSkill ? "LeftBack" : "LeftBack1", data.LeftBack);
                    OverrideAnimClip(id, artSkill ? "Left" : "Left1", data.Left);
                    OverrideAnimClip(id, artSkill ? "LeftForward" : "LeftForward1", data.LeftForward);

                    entity.StartMultipleDir();
                    entity.SetMultipleDirParam(entity.MoveDir.x * entity.MoveFactor, entity.MoveDir.z * entity.MoveFactor);
                }
                else
                {
                    entity.EndMultipleDirParam(true);
                }
                EffectPreviewContext context = GetPreviewContext(id);
                if (context != null)
                    SkillHelper.SetupAnim(data, context);
            }
        }

        public SFX PlayFx(ulong id, uint hash, int index, float hitDirection)
        {
            EcsData.XConfigData data = null;
            ScriptDic.TryGetValue(hash, out data);
            if (data != null)
            {
                if (data is ClientEcsData.XSkillData) return PlayFx(data, (data as ClientEcsData.XSkillData).FxData, id, index, hitDirection, false, false, Vector3.zero, 0, fxRatioChangedHandler);
                else return PlayFx(data, (data as ClientEcsData.XHitData).FxData, id, index, hitDirection, true, false, Vector3.zero, 0);
            }

            return null;
        }

        public SFX PlayFx(ulong id, uint hash, int index, Vector3 pos, float dir)
        {
            EcsData.XConfigData data = null;
            ScriptDic.TryGetValue(hash, out data);
            if (data != null)
            {
                if (data is ClientEcsData.XSkillData) return PlayFx(data, (data as ClientEcsData.XSkillData).FxData, id, index, -1000, false, true, pos, dir, fxRatioChangedHandler);
                else return PlayFx(data, (data as ClientEcsData.XHitData).FxData, id, index, -1000, true, true, pos, dir);
            }

            return null;
        }

        private SFX PlayFx(EcsData.XConfigData data, List<EcsData.XFxData> list, ulong id, int index, float hitDirection, bool isHitFx, bool global, Vector3 gPos, float gRot, RatioChangeEventHandler handler = null)
        {
            if (SkillHelper.GetDataIndex<EcsData.XFxData>(list, ref index))
            {
                EcsData.XFxData fd = list[index];

                if (fd.AttachTarget)
                {
                    ulong targetid = GetEntityTarget(id);
                    if (targetid != 0)
                    {
                        id = targetid;
                        global = false;
                    }
                    else return null;
                }

                Transform parent = EntityDic[id].obj.transform;
                // if (!string.IsNullOrEmpty (data.Bone))
                //     parent = EntityDic[id].Find (data.Bone);
                // else parent = EntityDic[id].obj.transform;

                Vector3 offset;
                Vector3 fxScale = new Vector3(fd.ScaleX, fd.ScaleY, fd.ScaleZ);
                if (hitDirection > -360)
                {
                    isHitFx = true;
                    XEntityPresentation.RowData pdata = XEntityPresentationReader.GetData(PresentDic[id]);
                    hitDirection += 180;
                    offset = XCommon.singleton.HorizontalRotateVetor3(Vector3.forward * pdata.BoundRadius * pdata.Scale + new Vector3(fd.OffsetX, 0, fd.OffsetZ),
                        hitDirection, false);
                    offset = XCommon.singleton.HorizontalRotateVetor3(offset, -EntityDic[id].obj.transform.eulerAngles.y, false);
                    Vector3 pos = global ? gPos : EntityDic[id].obj.transform.position;
                    if (GroundHeight + 0.5f > pos.y)
                        offset.y = fd.OffsetY;

                    fxScale.x *= pdata.HitFxScale[0] == 0 ? 1 : pdata.HitFxScale[0];
                    fxScale.y *= pdata.HitFxScale[1] == 0 ? 1 : pdata.HitFxScale[1];
                    fxScale.z *= pdata.HitFxScale[2] == 0 ? 1 : pdata.HitFxScale[2];
                    handler = null;
                }
                else
                {
                    if (!global)
                        offset = new Vector3(fd.OffsetX, fd.OffsetY, fd.OffsetZ);
                    else
                    {
                        Vector3 deltaPos = gPos - EntityDic[id].obj.transform.position;
                        offset = deltaPos + XCommon.singleton.HorizontalRotateVetor3(new Vector3(fd.OffsetX, fd.OffsetY, fd.OffsetZ), gRot, false);
                        offset = XCommon.singleton.HorizontalRotateVetor3(offset, -EntityDic[id].obj.transform.eulerAngles.y, false);
                    }
                }

                sfxContext.id = (fd.Flag & EcsData.XFxData.Flag_NotKillWithEntity) != 0 ? ulong.MaxValue : id;
                sfxContext.parent = parent;
                sfxContext.offset = offset;
                sfxContext.scale = fxScale;
                sfxContext.isHitFx = isHitFx;
                sfxContext.hitDirection = hitDirection;
                sfxContext.isGlobalRot = global;
                sfxContext.globalRot = gRot;
                sfxContext.flag = 0;

                SFXMgr.singleton.RemoveEcsTagSfx(id, fd.Tag);
                SFX fx = SkillHelper.PlaySFx(data, fd, null, SFXMgr.fxOwner.CombatPlayer, SFXMgr.fxOwner.CombatPlayer, ref sfxContext);
                fx.ecsTag = fd.Tag;
                return fx;
            }
            return null;
        }

        private bool OnFxRatioChanged(TimerData tData, float changeRatio)
        {
            FxData data = tData.GetObj<FxData>();

            if (data.Valid)
            {
                // float timeLeft = (float) XTimerMgr.singleton.TimeLeft (data.sfx.Token);
                // if (timeLeft > 0)
                // {
                //     XTimerMgr.singleton.Delay (tData.GetRatioTimeDelay (timeLeft, tData.ratio, changeRatio), data.fx.Token);
                // }
                // data.sfx.Duration = data.sfx.Duration * tData.ratio / changeRatio;

                data.sfx.SetSpeedRatio(changeRatio);
            }
            else return false;

            return true;
        }

        private bool FxValid(TimerData tData)
        {
            FxData data = tData.GetObj<FxData>();
            return data.Valid;
        }

        private void KillFx(object o)
        {
            FxData data = (FxData)o;
            if (data.Valid)
            {
                SFXMgr.singleton.Destroy(ref data.sfx);
            }
            // XFxMgr.singleton.DestroyFx (data.sfx, false);
        }

        public SFX ProjectBullet(ulong id, ulong bullet, uint hash, int index, Vector3 pos, float face)
        {
            EcsData.XConfigData data = null;
            ScriptDic.TryGetValue(hash, out data);
            if (data != null)
            {
                if (data is ClientEcsData.XSkillData) return ProjectBullet((data as ClientEcsData.XSkillData), id, bullet, hash, index, pos, face);
            }
            return null;
        }

        SkillBulletSceneUI skillBulletScene;
        private SFX ProjectBullet(ClientEcsData.XSkillData data, ulong id, ulong bullet, uint hash, int index, Vector3 pos, float face)
        {
            var list = data.BulletData;
            SFX sfx;
            if (SkillHelper.GetDataIndex(list, ref index))
            {
                EcsData.XBulletData bd = list[index];

                sfxContext.id = id;
                sfxContext.hash = hash;
                sfxContext.offset = pos;
                sfxContext.globalRot = face;
                sfxContext.flag = 0;

                sfx = SkillHelper.PlayBullet(data, bd, null, SFXMgr.fxOwner.CombatPlayer, SFXMgr.fxOwner.CombatPlayer, ref sfxContext);
                if (bd.BulletShakeIndex != -1) playCameraShake((data as ClientEcsData.XSkillData).CameraShakeData, hash, bd.BulletShakeIndex, pos);
                var trans = sfx.Find("");
                if (trans != null)
                {

                    if (skillBulletScene == null && !SkillBulletSceneUI.Root.TryGetComponent(out skillBulletScene))
                    {
                        skillBulletScene = SkillBulletSceneUI.Root.AddComponent<SkillBulletSceneUI>();
                    }
                    skillBulletScene.dataList.Add(bd);
                    skillBulletScene.transList.Add(trans);
                    skillBulletScene.idList.Add(bullet);
                    skillBulletScene.createTime.Add(Time.time);
                    trans.position = pos;
                    skillBulletScene.posList.Add(pos);
                    skillBulletScene.AttackRangeObjList.Add(new List<GameObject>());
                    skillBulletScene.initList.Add(false);

                }
#if UNITY_EDITOR
                if (OverdrawMonitor.isOn)
                {
                    OverdrawMonitor.Instance.StartObserveProfile(2, data.Name);
                }
#endif
                return sfx;
            }
            return null;
        }

        public void EditorBulletSync(ulong id, ulong e, float face, float x, float y, float z)
        {
            if (skillBulletScene == null && !SkillBulletSceneUI.Root.TryGetComponent(out skillBulletScene))
            {
                skillBulletScene = SkillBulletSceneUI.Root.AddComponent<SkillBulletSceneUI>();
            }
            for (int i = 0; i < skillBulletScene.transList.Count; ++i)
            {
                if (skillBulletScene.idList[i] == id && skillBulletScene.dataList[i].CurveDataIndex != -1)
                {
                    for (int j = 0; j < skillBulletScene.AttackRangeObjList[i].Count; ++j)
                    {
                        skillBulletScene.AttackRangeObjList[i][j].transform.position = new Vector3(x, y, z);
                    }
                }
            }
        }

        private string GetAudioEvent(ulong id, uint hash, int index)
        {
            if (index == -1) return null;

            EcsData.XConfigData data = null;
            ScriptDic.TryGetValue(hash, out data);
            if (data != null)
            {
                List<EcsData.XAudioData> list;
                if (data is ClientEcsData.XSkillData)
                    list = (data as ClientEcsData.XSkillData).AudioData;
                else
                    list = (data as ClientEcsData.XHitData).AudioData;
                if (SkillHelper.GetDataIndex<EcsData.XAudioData>(list, ref index))
                {
                    return list[index].AudioName;
                }
            }

            return null;
        }

        private void KillBullet(object o) { }

        public SFX ProjectWarning(ulong id, uint hash, int index, Vector3 pos, float face)
        {
            EcsData.XConfigData data = null;
            ScriptDic.TryGetValue(hash, out data);
            if (data != null)
            {
                if (data is ClientEcsData.XSkillData) return ProjectWarning((data as ClientEcsData.XSkillData).WarningData, id, index, pos, face);
            }

            return null;
        }

        private SFX ProjectWarning(List<EcsData.XWarningData> list, ulong id, int index, Vector3 pos, float face)
        {
            if (SkillHelper.GetDataIndex<EcsData.XWarningData>(list, ref index))
            {
                EcsData.XWarningData data = list[index];
                ulong originID = id;

                if (data.NeedTarget)
                {
                    ulong targetid = GetEntityTarget(id);
                    if (targetid != 0)
                    {
                        id = targetid;
                    }
                }

                SFX sfx;
                if (data.NotRealWarning)
                {
                    sfx = SFXMgr.singleton.Create(data.FxPath, 0, id, null, false, SkillHelper.OwnerType(id));
                }
                else
                {
                    sfx = SFXMgr.singleton.Create(data.FxPath, 0, id);
                }
                if (sfx != null)
                {
                    /// ID1012515
                    float parentScale = 1;

                    if (data.FollowTime > 0)
                    {
                        sfx.SetParent(EntityDic[id].obj.transform);
                        parentScale = EntityDic[id].obj.transform.localScale.x;
                    }
                    sfx.flag.SetFlag(SFX.Flag_Follow, data.FollowTime > 0);
                    sfx.flag.SetFlag(SFX.Flag_NeedTransByTransPartner, data.FollowTime > 0);
                    sfx.SetFollowTime(data.FollowTime);
                    sfx.FollowTime = data.FollowTime;

                    if (data.FollowTime == 0) sfx.SetRot(0, face, 0);
                    else sfx.SetRot(0, data.Angle, 0);

                    if (data.StickOnGround)
                    {
                        sfx.SetStickGround();
                    }
                    if (data.FollowTime > 0)
                    {
                        pos = new Vector3(data.OffsetX, data.OffsetY, data.OffsetZ);
                        pos = XCommon.singleton.HorizontalRotateVetor3(pos, data.Angle, false);
                    }
                    sfx.SetPos(ref pos);
                    var scale = new Vector3(data.ScaleX / parentScale, Mathf.Clamp(data.ScaleY, 0.2f, 2f), data.ScaleZ / parentScale);
                    sfx.SetScale(ref scale);
                    sfx.InitDuration = data.LifeTime;
                    if (data.PrefabType == 1 || data.PrefabType == 2) sfx.InitDuration += data.HighLightTime;
                    sfx.flag.SetFlag(SFX.Flag_SpeedChange, true);

                    if (data.PrefabType != 0)
                    {
                        sfx.goCache.TryGetComponent(out SFXWarningZone controller);
                        if (controller == null)
                        {
                            Debug.LogWarning("缺少预警圈控制器");
                        }
                        else
                        {
                            //Normal: X:Scale
                            //Circle: X:scale
                            //Square: X:weight Z:length
                            //Arc: X:scale Z: arc
                            if (data.PrefabType == 1)
                            {
                                //sfx.SetSpeedRatio(1.0f / (data.LifeTime - 0.06f));
                                sfx.SetAnimatorSpeed = 1.0f / (data.LifeTime - 0.06f);
                                controller.Init(data.ScaleX, data.LifeTime - 0.06f, data.Arc, data.WarningAllLow, data.HighLightTime, Mathf.Clamp(data.VanishDistance, 8, 1000), data.noRot);
                            }
                            // else if(data.PrefabType == 3)
                            // {
                            //     sfx.SetAnimatorSpeed = 1.0f / (data.LifeTime - 0.06f);
                            //     controller.Init(data.ScaleX, data.LifeTime - 0.06f,data.minLoop,data.maxLoop,data.WarningAllLow);
                            // }
                            else
                            {
                                sfx.SetAnimatorSpeed = 1.0f / (data.LifeTime - 0.06f);
                                //controller.Init(data.ScaleX, data.LifeTime - 0.06f);
                                controller.Init(data.ScaleX, data.LifeTime - 0.06f, data.WarningAllLow, data.HighLightTime, Mathf.Clamp(data.VanishDistance, 8, 1000), data.noRot);
                            }
                        }
                    }
                    sfx.Play();

                    // XFx fx = XFxMgr.singleton.CreateAndPlay (
                    //     data.FxPath,
                    //     pos,
                    //     new Vector3 (data.ScaleX, data.ScaleY, data.ScaleZ),
                    //     1, data.LifeTime
                    // );

                    if (data.EndWithSkill)
                        AddTimer(originID, 100, StopWarning, new FxData()
                        {
                            fx_token = sfx.uid,
                            sfx = sfx,
                        });
                }
                return sfx;
            }

            return null;
        }

        private void StopWarning(object o)
        {
            FxData fx = (FxData)o;
            if (fx.Valid)
                SFXMgr.singleton.Destroy(ref fx.sfx);
        }

        public static void AddSelection(Object obj, bool clear = false)
        {
            if (hideSceneUI) return;

            Object[] objs = Selection.objects;
            Object[] newObjs = new Object[clear ? 1 : (objs.Length + 1)];
            if (!clear) objs.CopyTo(newObjs, 0);
            newObjs[newObjs.Length - 1] = obj;
            Selection.objects = newObjs;
        }

        public float GetAnimSpeed(ulong id)
        {
            return EntityDic[id].obj.GetComponent<Animator>().speed;
        }

        public void SetAnimSpeed(ulong id, float speed)
        {
            EntityDic[id].obj.GetComponent<Animator>().speed = speed;
        }

        public void SetAnimTrigger(ulong id, string trigger)
        {
            EntityDic[id].obj.GetComponent<Animator>().SetTrigger(trigger);
        }

        public string GetScriptPath(uint hash)
        {
            if (!SkillHashMap.ContainsKey(hash))
            {
                bool flag = false;
                SkillListForEnemy.RowData[] list = XSkillReader.EnemySkill.Table;
                for (int i = 0; i < list.Length; i++)
                {
                    if (XCommon.singleton.XHash(list[i].SkillScript) == hash)
                    {
                        LoadSkillData("/BundleRes/SkillPackage/" + EntityDic[PlayerIndex].presentData.SkillLocation + list[i].SkillScript + ".bytes");
                        flag = true;
                        break;
                    }
                }
                if (!flag)
                {
                    SkillListForRole.RowData[] rolelist = XSkillReader.RoleSkill.Table;
                    for (int i = 0; i < rolelist.Length; i++)
                    {
                        if (XCommon.singleton.XHash(rolelist[i].SkillScript) == hash)
                        {
                            LoadSkillData("/BundleRes/SkillPackage/" + EntityDic[PlayerIndex].presentData.SkillLocation + rolelist[i].SkillScript + ".bytes");
                            break;
                        }
                    }
                }
            }
            if (!SkillHashMap.ContainsKey(hash))
            {
                Debug.LogError(hash + " not find in skilltable");
                return null;
            }

            return Application.dataPath + SkillHashMap[hash];
        }

        public string GetHitScriptPath(uint hash)
        {
            return HitScriptHash[hash];
        }

        public string GetHitHeaderScriptPath(ulong id)
        {
            XEntityPresentation.RowData data = XEntityPresentationReader.GetData(PresentDic[id]);
            string path = Application.dataPath + "/BundleRes/HitPackage/" + data.BehitLocation + data.Prefab + "_Hit_Header.bytes";
            return File.Exists(path) ? path : string.Empty;
        }

        public uint GetHitHeaderScriptHash(ulong id)
        {
            XEntityPresentation.RowData data = XEntityPresentationReader.GetData(PresentDic[id]);
            return XCommon.singleton.XHash(data.Prefab + "_Hit_Header");
        }

        public void PrepareAnim(ulong id, string state, string motion)
        {
            XEntityPresentation.RowData data = XEntityPresentationReader.GetData(PresentDic[id]);
            switch (state)
            {
                case "idle":
                    {
                        OverrideAnimClip(id, motion, "Animation/" + data.AnimLocation + data.AttackIdle);
                        OverrideAnimClip(id, "Brake", "Animation/" + data.AnimLocation + (string.IsNullOrEmpty(data.Brake) ? data.AttackIdle : data.Brake));
                    }
                    break;
                case "run":
                    {
                        OverrideAnimClip(id, motion, "Animation/" + data.AnimLocation + data.AttackRun);
                    }
                    break;
            }
        }

        public void playCameraLayerMask(ulong id, uint hash, int index)
        {
            EcsData.XConfigData data = null;
            ScriptDic.TryGetValue(hash, out data);
            if (data != null)
            {
                playCameraLayerMask((data as ClientEcsData.XSkillData).CameraLayerMaskData, id, index);
            }
        }

        private void playCameraLayerMask(List<EcsData.XCameraLayerMaskData> list, ulong id, int index)
        {
            //if (SkillHelper.GetDataIndex<EcsData.XCameraLayerMaskData>(list, ref index))
            //{
            //    EcsData.XCameraLayerMaskData data = list[index];
            //    cameraComponent.PlayCameraLayerMask(data.LifeTime, data.Mask);
            //}
        }

        public void playCameraMotion(ulong id, uint hash, int index)
        {
            EcsData.XConfigData data = null;
            ScriptDic.TryGetValue(hash, out data);
            if (data != null)
            {
                playCameraMotion((data as ClientEcsData.XSkillData).CameraMotionData, id, index);
            }
        }

        private uint motionToken = 0;
        private void playCameraMotion(List<EcsData.XCameraMotionData> list, ulong id, int index)
        {
            if (SkillHelper.GetDataIndex<EcsData.XCameraMotionData>(list, ref index))
            {
                EcsData.XCameraMotionData data = list[index];
                cameraComponent.PlayCameraMotion("Effect", data);
                cameraComponent.AnchorBased = data.AnchorBased;

                CFEngine.AssetHandler res = null;
                AnimationClip animClip = null;
                if (!UsePhoneAnimLoad)
                {
                    CFEngine.EngineUtility.LoadAnim(data.MotionPath, ref res);
                    animClip = res.obj as AnimationClip;
                }
                else
                {
                    animClip = LoadPhoneAnimClip(data.MotionPath);
                }
                if (animClip != null)
                {
                    AddTimer(id, animClip.length, EndCameraMotion, ++motionToken, OnCameraMotionRatioChanged);
                }
                LoadMgr.singleton.Destroy(ref res);
            }
        }

        private void EndCameraMotion(object o)
        {
            if (motionToken != (uint)o) return;

            cameraComponent._motion.ResetStatus(false);
        }

        private bool OnCameraMotionRatioChanged(TimerData tData, float changeRatio)
        {
            cameraComponent.Speed = changeRatio;
            return true;
        }

        public void playCameraPostEffect(ulong id, uint hash, int index)
        {
            EcsData.XConfigData data = null;
            ScriptDic.TryGetValue(hash, out data);
            if (data != null)
            {
                playCameraPostEffect((data as ClientEcsData.XSkillData).CameraPostEffectData, id, index);
            }
        }
        private void playCameraPostEffect(List<EcsData.XCameraPostEffectData> list, ulong id, int index)
        {
            var context = EngineContext.instance;
            if (context != null)
            {
                if (SkillHelper.GetDataIndex<EcsData.XCameraPostEffectData>(list, ref index))
                {
                    EcsData.XCameraPostEffectData data = list[index];
                    if (!string.IsNullOrEmpty(data.FxPath))
                    {
                        string sfxName = EngineUtility.GetFileName(data.FxPath);
                        if (!string.IsNullOrEmpty(sfxName))
                        {
                            if (!data.AssistEffect)
                            {
                                SFX sfx = SFXMgr.singleton.Create(sfxName, SFXMgr.Flag_Async, id);
                                sfx.SetParent(context.CameraTransCache);
                                sfx.flag.SetFlag(SFX.Flag_Follow, true);
                                sfx.flag.SetFlag(SFX.Flag_EndWithHoster, true);
                                sfx.Duration = data.LifeTime;
                                sfx.Play();
                            }
                            else
                            {
                                int slot = GetVirtualCameraSlot(id);
                                if (slot >= 0) SplitScreenSystem.SetSlotSFX(context, data.FxPath, slot);
                            }
                            // AddTimer (id, data.LifeTime, killFxHandler,
                            //     AddFx (id, new FxData ()
                            //     {
                            //         type = FxType.CameraFx,
                            //             id = id,
                            //             fx_token = sfx.uid,
                            //             sfx = sfx
                            //     }));
                        }

                        // XFx fx = XFxMgr.singleton.CreateAndPlay (data.FxPath,
                        //     cameraComponent.CameraObject.transform,
                        //     Vector3.zero, Vector3.one, 1, true, data.LifeTime);

                        // AddTimer (id, data.LifeTime, KillFx,
                        //     new FxData () { type = FxType.CameraFx, id = id, fx_token = fx._instanceID, fx = fx });
                    }
                    //else
                    //{
                    //    EnvHelp.OverrideEnvEffectParam (context, data.EffectType, data);
                    //    XTimerMgr.singleton.SetTimer (data.LifeTime, ResumeCameraPostEffect, null);
                    //}
                    return;
                }
            }
        }

        public void playCameraShake(ulong id, uint hash, int index)
        {
            EcsData.XConfigData data = null;
            ScriptDic.TryGetValue(hash, out data);
            if (data != null)
            {
                playCameraShake((data as ClientEcsData.XSkillData).CameraShakeData, id, index, Vector3.zero);
            }
        }

        private void playCameraShake(List<EcsData.XCameraShakeData> list, ulong id, int index, Vector3 pos)
        {
            if (SkillHelper.GetDataIndex<EcsData.XCameraShakeData>(list, ref index))
            {
                EcsData.XCameraShakeData data = list[index];
                int impulse = cameraComponent.PlayCameraShake(data.Path, data.Amplitude, data.Frequency, data.LifeTime, data.AttackTime, data.DecayTime, pos == Vector3.zero ? EntityDic[id].obj.transform.position : pos, data.ImpactRadius);
                if (data.StopAtEnd && impulse >= 0) AddTimer(id, data.LifeTime, StopCameraShake, impulse);
            }
        }

        private void StopCameraShake(object o)
        {
            cameraComponent.StopCameraShake((int)o);
        }

        public void playCameraStretch(ulong id, uint hash, int index)
        {
            EcsData.XConfigData data = null;
            ScriptDic.TryGetValue(hash, out data);
            if (data != null)
            {
                playCameraStretch((data as ClientEcsData.XSkillData).CameraStretchData, id, index);
            }
        }

        private ulong _fov_id = 0;
        private float _fov_origin;
        private float _fov_end_blend_time;
        private void playCameraStretch(List<EcsData.XCameraStretchData> list, ulong id, int index)
        {
            if (SkillHelper.GetDataIndex<EcsData.XCameraStretchData>(list, ref index))
            {
                EcsData.XCameraStretchData data = list[index];
                if (data.Type == 0)
                {
                    if (data.UsingFov)
                    {
                        _fov_id = id;
                        _fov_end_blend_time = data.EndBlendTime;
                        if (data.LifeTime == 0)
                            cameraComponent.SetFov(data.Fov, data.FOVFadeinTime, data.UseFovFadeInCurve ? data.FovFadeInCurve : null, data.FOVLastTime, data.FOVFadeOutTime, data.UseFovFadeOutCurve ? data.FovFadeOutCurve : null, data.isMotion);
                        else
                            cameraComponent.SetFov(data.Fov, data.LifeTime, data.UseFovFadeInCurve ? data.FovFadeInCurve : null, 0, -1, data.UseFovFadeOutCurve ? data.FovFadeOutCurve : null, data.isMotion);
                    }

                }
                else if (data.Type == 1)
                {
                    //todo
                    _fov_id = id;
                    cameraComponent.SetDamping(data.DampingLastTime, data.DampingCurve);
                }
                else if (data.Type == 2) 
                {
                    cameraComponent.StartRotate(SkillHoster.GetHoster.EntityDic[Target]?.obj?.transform, data.targetAngles, data.rotTime, data.rotAcceleration);
                }
            }
        }

        public void playSpecialAction(ulong id, uint hash, int index)
        {
            EcsData.XConfigData data = null;
            ScriptDic.TryGetValue(hash, out data);
            if (data != null)
            {
                playSpecialAction((data as ClientEcsData.XSkillData).SpecialActionData, id, index);
            }
        }

        private void playSpecialAction(List<EcsData.XSpecialActionData> list, ulong id, int index)
        {
            if (SkillHelper.GetDataIndex(list, ref index))
            {
                var sad = list[index] as EcsData.XSpecialActionData;
                XSpecialActionType type = (XSpecialActionType)sad.Type;
                switch (type)
                {
                    case XSpecialActionType.XSkybox:
                        {
                            SkillHelper.PlaySkyBox(sad);
                        }
                        break;
                    case XSpecialActionType.XCriticalAttack:
                    case XSpecialActionType.XSpaceTimeLockEffect:
                    case XSpecialActionType.XSceneEffect:
                        {
                            if (sad.SubType == EcsData.XSpecialActionData.SceneMisc)
                            {
                                EffectPreviewContext epc = GetPlayerPreviewContext();
                                EngineContext context = EngineContext.instance;
                                if (context != null)
                                {
                                    context.mainRole = epc.xGameObject;
                                }
                            }

                            SkillHelper.PlayEnvEffect(sad, id);
                        }
                        break;
                    case XSpecialActionType.XCinemachineControl:
                        {
                            switch ((ClientEcsData.XScriptCinemachineControlType)sad.SubType)
                            {
                                case XScriptCinemachineControlType.DisableSimpleFollow:
                                    {
                                        cameraComponent.SetSimpleFollowState(false);

                                        AddTimer(id, sad.LifeTime, ResumeCinemachineControl, sad);
                                    }
                                    break;
                            }
                        }
                        break;
                    case XSpecialActionType.XPlayCameraCurve:
                        {
                            PlayCameraCurve(id, sad);
                        }
                        break;
                    case XSpecialActionType.XDynamicBone:
                        {
                            GetEntity()?.dynamicBone.Execute(sad);
                        }
                        break;
                }
            }
        }

        private void PlayCameraCurve(ulong id, EcsData.XSpecialActionData sad)
        {
            var context = EngineContext.instance;
            EffectPreviewContext epc = GetPreviewContext(id);
            if (id == PlayerIndex)
            {
                switch (sad.IntParameter1)
                {
                    case 0:
                    case 3:
                        {
                            SplitScreenSystem.SetPlayerCount(3);
                        }
                        break;
                    default:
                        {
                            SplitScreenSystem.SetPlayerCount(sad.IntParameter1);
                        }
                        break;
                }
                Vector3 scale = Vector3.one;
                SplitScreenSystem.EnableSpitScreen(context, true, ref scale);
                AddTimer(id, sad.LifeTime, StopCameraCurve, null);
                EditorVirtualCameraData cameraData = new EditorVirtualCameraData(epc.xGameObject, sad.StringParameter1, id);

                virtualCameraList.Insert(0, cameraData);
                for (int i = 0; i < 3 && i < virtualCameraList.Count; ++i)
                {
                    int slot = i;
                    slot = (slot == 0 ? 1 : (slot == 1 ? 0 : slot));
                    SplitScreenSystem.SetSlotObject(context, virtualCameraList[i].go, slot, virtualCameraList[i].cb, sad.FloatParameter1);
                }
            }
            else
            {
                EditorVirtualCameraData cameraData = new EditorVirtualCameraData(epc.xGameObject, sad.StringParameter1, id);

                virtualCameraList.Add(cameraData);
                if (virtualCameraList.Count < 4)
                {
                    int slot = virtualCameraList.Count - 1;
                    slot = (slot == 0 ? 1 : (slot == 1 ? 0 : slot));
                    SplitScreenSystem.SetSlotObject(context, cameraData.go, slot, cameraData.cb, sad.FloatParameter1);
                }
            }
        }

        private class EditorVirtualCameraData : VirtualCameraData
        {
            public EditorVirtualCameraData(XGameObject obj, string path, ulong id) : base(obj, path, id) { }
            protected override Matrix4x4 CameraMatrix
            {
                get { return GetHoster.cameraComponent.DummyObject.transform.localToWorldMatrix; }
            }
            protected override Vector3 CameraPosition
            {
                get { return GetHoster.cameraComponent.DummyObject.transform.position; }
            }
        }

        List<EditorVirtualCameraData> virtualCameraList = new List<EditorVirtualCameraData>();

        private int GetVirtualCameraSlot(ulong id)
        {
            if (virtualCameraList.Count > 0)
            {
                for (int i = 0; i < virtualCameraList.Count; ++i)
                {
                    if (virtualCameraList[i].ID == id)
                    {
                        switch (i)
                        {
                            case 0:
                                return 1;
                            case 1:
                                return 0;
                            default:
                                return i;
                        }
                    }
                }
            }
            return -1;

        }

        private void StopCameraCurve(object o)
        {
            var context = EngineContext.instance;
            Vector3 scale = Vector3.one;
            SplitScreenSystem.EnableSpitScreen(context, false, ref scale);
            for (int i = 0; i < virtualCameraList.Count; ++i)
            {
                virtualCameraList[i].UnInit();
            }
            virtualCameraList.Clear();
        }

        public void ResumeCinemachineControl(object o)
        {
            var data = o as EcsData.XSpecialActionData;

            switch ((ClientEcsData.XScriptCinemachineControlType)data.SubType)
            {
                case XScriptCinemachineControlType.DisableSimpleFollow:
                    {
                        cameraComponent.SetSimpleFollowState(true);
                    }
                    break;
            }
        }

        public void playShaderEffect(ulong id, uint hash, int index)
        {
            EcsData.XConfigData data = null;
            ScriptDic.TryGetValue(hash, out data);
            if (data != null)
            {
                playShaderEffect(data, id, hash, index);
            }
        }

        private void playShaderEffect(EcsData.XConfigData skillData, ulong id, uint hash, int index)
        {
            EffectPreviewContext context = GetPreviewContext(id);
            if (context != null)
            {
                SkillHelper.PlayMatEffect(skillData, index, context);
            }
        }

        public void PlayAudio(ulong id, uint hash, int index)
        {
            EcsData.XConfigData data = null;
            ScriptDic.TryGetValue(hash, out data);
            if (data != null)
            {
                if (data is ClientEcsData.XSkillData) PlayAudio((data as ClientEcsData.XSkillData).AudioData, id, index);
                else PlayAudio((data as ClientEcsData.XHitData).AudioData, id, index);
            }
        }

        private void PlayAudio(List<EcsData.XAudioData> list, ulong id, int index)
        {
            if (SkillHelper.GetDataIndex<EcsData.XAudioData>(list, ref index))
            {
                EcsData.XAudioData data = list[index];

                EntityDic[id].PlaySound((AudioChannel)data.ChannelID, "event:/" + data.AudioName, data.Follow);

                AddTimer(id, 100, stopAudioHandler, new AudioData() { id = id, audioChannel = (AudioChannel)data.ChannelID, StopAtSkillEnd = data.StopAtSkillEnd }, OnAudioRatioChanged);
            }
        }

        private struct AudioData
        {
            public ulong id;
            public AudioChannel audioChannel;
            public bool StopAtSkillEnd;
        }

        private void StopAudio(object o)
        {
            AudioData data = (AudioData)o;
            if (!data.StopAtSkillEnd) return;
            EntityDic[data.id].fmod.Stop(data.audioChannel, STOP_MODE.ALLOWFADEOUT);
        }

        private bool OnAudioRatioChanged(TimerData tData, float changeRatio)
        {
            AudioData data = tData.GetObj<AudioData>();

            if (EntityDic[data.id].fmod != null)
            {
                EntityDic[data.id].fmod.SetSpeedAndVolume(data.audioChannel, Xuthus.getactionratio(data.id), 1);
            }
            else return false;

            return true;
        }

        public void OnSpaceTimeLock(ulong id, float time, float ratio, int mask)
        {
            if (id != PlayerIndex) return;

            if ((mask & 0x0001) != 0)
                EditorEcs.Xuthus_VirtualServer.setActionRatio(PlayerIndex, ratio);
            if ((mask & 0x0008) != 0)
            {
                for (ulong i = 2; i <= Target; ++i)
                    EditorEcs.Xuthus_VirtualServer.setActionRatio(i, ratio);
            }

            XTimerMgr.singleton.SetTimer(time, ResumeSpaceTimeLock, mask);
        }

        private void ResumeSpaceTimeLock(object o)
        {
            int mask = (int)o;

            if ((mask & 0x0001) != 0)
                EditorEcs.Xuthus_VirtualServer.setActionRatio(PlayerIndex, 1);
            if ((mask & 0x0008) != 0)
            {
                for (ulong i = 2; i <= Target; ++i)
                    EditorEcs.Xuthus_VirtualServer.setActionRatio(i, 1);
            }
        }

        private List<TimerData> AddTimer(ulong id, float time, XTimerMgr.ElapsedEventHandler handler, object param, RatioChangeEventHandler ratioHandler = null, ValidEventHandler validHandler = null)
        {
            List<TimerData> timeList = null;
            if (!EntityTimers.TryGetValue(id, out timeList))
            {
                timeList = CFEngine.ListPool<TimerData>.Get();
                EntityTimers.Add(id, timeList);
            }

            TimerData data = CommonObject<TimerData>.Get();
            data.Init(XTimerMgr.singleton.SetTimer(time, handler, param), 1, ratioHandler, param, validHandler);
            timeList.Add(data);
            float scale = GetEntityRatio(id);
            if (scale != 1) data.OnRatioChange(scale);

            return timeList;
        }

        private XEcsGamePlay.FxData AddFx(ulong id, XEcsGamePlay.FxData data)
        {
            if (!FxDic.ContainsKey(id)) FxDic.Add(id, CFEngine.ListPool<XEcsGamePlay.FxData>.Get());
            FxDic[id].Add(data);
            return data;
        }

        public void OnActionRatioChanged(ulong id, float ratio)
        {
            ChangeEntityRatio(id, ratio);
        }

        private void ChangeEntityRatio(ulong id, float ratio)
        {
            EntityRatio[id] = ratio <= 0 ? 0.00001f : ratio;

            List<TimerData> timerList;
            if (EntityTimers.TryGetValue(id, out timerList))
            {
                for (int i = timerList.Count - 1; i >= 0; --i)
                {
                    timerList[i].OnRatioChange(GetEntityRatio(id));
                }
            }

            SFXMgr.singleton.ChangeSpeedRatio(id, ratio, false);
            SkillHelper.ChangeSpeedRatio(id, ratio);
        }

        public float GetEntityRatio(ulong id)
        {
            float scale = 1;
            if (EntityRatio.TryGetValue(id, out scale))
                return scale;
            return 1;
        }
        public Transform GetTargetTrans(ulong id, string targetBone)
        {
            var target = EntityDic[XEcs.singleton.GetSkillTarget(id)];
            if (target != null)
            {
                return target.Find(targetBone);
            }
            return null;
        }

        public ulong GetEntityTarget(ulong id)
        {
            return XEcs.singleton.GetSkillTarget(id);
        }
        #region FOR_RECORD
        public void CheckRemoveEntity()
        {
            var list = new List<ulong>();
            foreach (ulong key in EntityDic.Keys)
            {
                if (EntityDic[key].id != PlayerIndex)
                {
                    list.Add(EntityDic[key].id);
                }
            }
            foreach (ulong key in list)
            {
                DestoryEntity(key);
            }
        }

        public void FireSkillForRecord(string skill, ulong id)
        {
            if (!LoadSkillData(skill)) return;

            XEcsScriptEntity.EndSkill(id);
            XEcsScriptEntity.BindSkill(id, XCommon.singleton.XHash(Path.GetFileNameWithoutExtension(skill)));
            XEcsScriptEntity.OnSkill(id, aiMode ? Target : ulong.MaxValue);
        }
        #endregion

        #region VerifyPos
        public bool VerifyPos(ulong id, ref float x, ref float y, ref float z, float rx, float ry, float rz)
        {
            return EntityDic[id].CheckBlock(ref x, ref y, ref z, new Vector3(rx, ry, rz));
        }
        #endregion
    }
}
#endif