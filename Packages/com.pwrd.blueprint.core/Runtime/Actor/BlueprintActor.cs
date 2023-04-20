using UnityEngine;
using System.Collections.Generic;
using System.Reflection;
using System;
using System.Linq;
#if UNITY_EDITOR
#if UNITY_2021_2_OR_NEWER
    using UnityEditor.SceneManagement;
#else 
    using UnityEditor.Experimental.SceneManagement;
#endif
using UnityEditor;
#endif

namespace Blueprint.Actor
{
    using Blueprint;

    [DisallowMultipleComponent]
    public class BlueprintActor : MonoBehaviour
    {

        public ActorBase BpActor;

        public List<BlueprintActorGoParam> blueprintActorGoParams = new List<BlueprintActorGoParam>();

        public bool IsStart = false;

        /// <summary>
        /// actor导出类的类名
        /// </summary>
        public string bpClassExportName = string.Empty;

        /// <summary>
        /// actor对应根物体名称
        /// </summary>
        public string bpClassName = string.Empty;

        private BPClassManager BPClassManager => BPClassManager.Instance;

        /// <summary>
        /// 该prefab所有gameobject对应的param字典
        /// </summary>
        /// <typeparam name="GameObject"></typeparam>
        /// <typeparam name="BlueprintActorParam"></typeparam>
        /// <returns></returns>
        public Dictionary<GameObject, BlueprintActorGoParam> actorParamDic = new Dictionary<GameObject, BlueprintActorGoParam>();

        public bool IsCanStart = false;

        void Awake()
        {
            if (Application.isPlaying)
            {
                if (BPClassManager.Instance == null)
                {
                    UnityEngine.Debug.LogError("BPClassManager没有初始化,首先请检查actor prfab上有BPInit脚本,然后检查PlayerSetting中是否将BPInit执行顺序设置为-2或0以下");
                }

                ConstructorInfo constructorInfo = BPClassManager.GetEmptyConstructorInfo("Blueprint.UnityLogic." + bpClassExportName);
                if (constructorInfo != null)
                {
                    BpActor = constructorInfo.Invoke(null) as ActorBase;
                    BpActor.SetBlueprintActor(this);

                    SetField(BpActor);

                    BpActor.Awake();
                }
                else
                {
                    Debug.LogError("Can't find actor class: " + bpClassExportName);
                }
            }
        }

        private void SetField(ActorBase BpActor)
        {
            ActorFieldBase fieldComp = gameObject.GetComponent<ActorFieldBase>() as ActorFieldBase;

            if (fieldComp == null)
            {
                return ;
            }

            fieldComp.actor = BpActor;
            var fieldRuntimeData = BpActor.GetType().GetFields();
            var fieldData = fieldComp.GetType().GetFields(BindingFlags.NonPublic|BindingFlags.Instance);

            foreach (var runtimeData in fieldRuntimeData)
            {
                var data = fieldData.Where(t=>t.Name==runtimeData.Name).FirstOrDefault();
                if(data!=null)
                    runtimeData.SetValue(BpActor,data.GetValue(fieldComp));
            }
        }

        public void Start()
        {
            if (Application.isPlaying && BpActor != null)
            {
                BpActor.Start();
                IsStart = true;
            }
        }

        void Update()
        {
            if (Application.isPlaying && BpActor != null)
            {
                if (IsStart && IsCanStart)
                {
                    BpActor.Start();
                    IsCanStart = false;
                }
                BpActor.Update();
                BpActor.InvokeUpdate(Time.deltaTime);
            }
        }

        void OnDestroy()
        {
            if (Application.isPlaying && BpActor != null)
            {
                BpActor.OnDestroy();
            }
        }

        void FixedUpdate()
        {
            if (Application.isPlaying && BpActor != null)
            {
                BpActor.FixedUpdate();
            }
        }

        void LateUpdate()
        {
            if (Application.isPlaying && BpActor != null)
            {
                BpActor.LateUpdate();
            }
        }

        void OnApplicationFocus(bool hasFocus)
        {
            if (Application.isPlaying && BpActor != null)
            {
                BpActor.OnApplicationFocus(hasFocus);
            }
        }

        void OnApplicationPause(bool pauseStatus)
        {
            if (Application.isPlaying && BpActor != null)
            {
                BpActor.OnApplicationPause(pauseStatus);
            }
        }

        void OnApplicationQuit()
        {
            if (Application.isPlaying && BpActor != null)
            {
                BpActor.OnApplicationQuit();
            }
        }

        /// <summary>
        /// 根据唯一名称获取物体
        /// </summary>
        /// <param name="uniqueName"></param>
        /// <returns></returns>
        public GameObject GetGameObject(string uniqueName)
        {
            return GetBlueprintActorGoParam(uniqueName)?.gameObject;
        }

        /// <summary>
        /// 根据唯一名称获取actorparam
        /// </summary>
        /// <param name="uniqueName"></param>
        /// <returns></returns>
        public BlueprintActorGoParam GetBlueprintActorGoParam(string uniqueName)
        {
            foreach (var bapp in blueprintActorGoParams)
            {
                if (bapp.uniqueName == uniqueName)
                {
                    return bapp;
                }
            }

            return null;
        }

        public void OnRecycle()
        {
            if (BpActor != null)
            {
                BpActor.ClearAllInvokeList();
                DelayControl.Instance.RemoveAllDelay(BpActor);
            }
        }

        /// <summary>
        /// 设置GoParam中组件数据
        /// </summary>
        /// <param name="actorParam"></param>
        private void SetGoParamInfos(BlueprintActorGoParam actorParam)
        {
            actorParam.IsActive = actorParam.gameObject.activeSelf;
            List<Component> comps = new List<Component>();
            actorParam.gameObject.GetComponents(comps);
            foreach (var comp in comps)
            {
                bool ret = true;
                if (comp is Behaviour behaviour)
                {
                    ret = behaviour.isActiveAndEnabled;
                }
                else if (comp is Collider collider)
                {
                    ret = collider.enabled;
                }
                else if (comp is Renderer renderer)
                {
                    ret = renderer.enabled;
                }
                else if (comp is Cloth cloth)
                {
                    ret = cloth.enabled;
                }
                int instanceID = comp.GetInstanceID();
                actorParam.commponentInstanceIDs.Add(instanceID, ret);
            }
        }
        /// <summary>
        /// 填充ActorParamDic
        /// </summary>
        public void FillActorParamDic()
        {
            if (actorParamDic.Count != 0)
                return;
            foreach (var goParam in blueprintActorGoParams)
            {
                actorParamDic.Add(goParam.gameObject, goParam);
                SetGoParamInfos(goParam);
            }
        }
        /// <summary>
        /// 重置物体与组件Active状态
        /// </summary>
        public void ResetActiveStatus()
        {
            foreach (var goParam in blueprintActorGoParams)
            {
                goParam.gameObject.SetActive(actorParamDic[goParam.gameObject].IsActive);
                RestComponentActiveStatus(goParam);
            }
        }    
        public void RestComponentActiveStatus(BlueprintActorGoParam actorParam)
        {
            List<Component> comps = new List<Component>();
            actorParam.gameObject.GetComponents(comps);
            foreach (var comp in comps)
            {
                int instanceID = comp.GetInstanceID();
                bool ret = actorParam.commponentInstanceIDs[instanceID];
                if (comp is Behaviour behaviour)
                {
                    behaviour.enabled = ret;
                }
                else if (comp is Collider collider)
                {
                    collider.enabled = ret;
                }
                else if (comp is Renderer renderer)
                {
                    renderer.enabled = ret;
                }
                else if (comp is Cloth cloth)
                {
                    cloth.enabled = ret;
                }
            }
        }
        /// <summary>
        /// 检查是否有组件被移除
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public bool CheckComponent(GameObject obj)
        {
            if (obj == null)
                return true;
            if (!actorParamDic.ContainsKey(obj))
                return false;
            List<Component> comps = new List<Component>();
            obj.GetComponents(comps);
            var compIDs = actorParamDic[obj].commponentInstanceIDs.Keys;
            int count = 0;
            foreach (Component item in comps)
            {
                if (!compIDs.Contains(item.GetInstanceID()))
                {
                    DestroyImmediate(item);
                }
                else
                    count++;
            }
            if (count != compIDs.Count)
                return false;
            if (obj.transform.childCount != 0)
            {
                #pragma warning disable CS0162
                for(int i = 0; i < obj.transform.childCount; i++)
                #pragma warning restore CS0162
                {
                    if (!CheckComponent(obj.transform.GetChild(i).gameObject))
                        return false;
                }
            }
            return true;
        }

#if UNITY_EDITOR
        public void RefreshActorParam()
        {
            if (Application.isPlaying)
            {
                return ;
            }

            actorParamDic.Clear();

            if (blueprintActorGoParams.Count == 0)
            {
                BlueprintActorGoParam goParam = new BlueprintActorGoParam()
                {
                    gameObject = gameObject,
                    name = gameObject.name,
                    uniqueName = GetUniqueName(gameObject),
                };

                blueprintActorGoParams.Add(goParam);
            }

        
            ClearInvalidParams();
            GenerateParamDic();
            GenerateActorParam(this.gameObject);
            CalculateInherit(this.gameObject, false);
            CheckNameRepeat();

            GameObject parent = PrefabUtility.GetCorrespondingObjectFromSource(this.gameObject);
            if (parent != null)
            {
                UpdateUniqueName(this.gameObject, parent.GetComponent<BlueprintActor>());
            }
        }

        /// <summary>
        /// 从父prefab中获取继承物体的唯一名称
        /// </summary>
        private void UpdateUniqueName(GameObject obj, BlueprintActor parent)
        {
            if (parent == null)
            {
                return ;
            }

            GameObject parentObj = PrefabUtility.GetCorrespondingObjectFromSource(obj);
            if (parentObj == null)
            {
                return ;
            }

            if (actorParamDic[obj].isInherit)
            {
                actorParamDic[obj].uniqueName = parent.GetObjUniqueName(parent, parentObj);
            }

            foreach (Transform tran in obj.transform)
            {
                UpdateUniqueName(tran.gameObject, parent);
            }
        }

        private string GetObjUniqueName(BlueprintActor blueprintActor, GameObject obj)
        {
            foreach (var goParam in blueprintActor.blueprintActorGoParams)
            {
                if (goParam.gameObject == obj)
                {
                    return goParam.uniqueName;
                }
            }

            return string.Empty;
        }

        private void GenerateActorParam(GameObject gameObject)
        {
            BlueprintActorGoParam p = null;
            if (!actorParamDic.TryGetValue(gameObject, out p))
            {
                p = new BlueprintActorGoParam();
                p.gameObject = gameObject;
                p.uniqueName = GetUniqueName(gameObject);
                p.name = p.gameObject.name;
                p.index = blueprintActorGoParams.Count;
                
                blueprintActorGoParams.Add(p);
                actorParamDic.Add(gameObject, p);
            }

            p.name = gameObject.name;
            p.childIndex.Clear();
            p.components = GetActorComponents(gameObject);

            foreach (Transform tran in gameObject.transform)
            {
                GenerateActorParam(tran.gameObject);
            }

            foreach (Transform tran in gameObject.transform)
            {
                var goParam = actorParamDic[tran.gameObject];
                goParam.parentIndex = p.index;
                p.childIndex.Add(goParam.index);
            }
        }

        private void ClearInvalidParams()
        {
            for (int i = blueprintActorGoParams.Count - 1; i >= 0; i--)
            {
                if (blueprintActorGoParams[i].gameObject == null)
                {
                    blueprintActorGoParams.RemoveAt(i);
                }
            }

            for (int i = 0; i < blueprintActorGoParams.Count; i++)
            {
                blueprintActorGoParams[i].index = i;
            }
        }

        private void GenerateParamDic()
        {
            int index = 0;
            foreach (var goParam in blueprintActorGoParams)
            {
                if (goParam.gameObject != null)
                {
                    actorParamDic.Add(goParam.gameObject, goParam);
                }
                else 
                {
                    Debug.Log(index);
                }
                index ++;
            }
        }

        /// <summary>
        /// 获取唯一名称
        /// </summary>
        /// <param name="gameObject"></param>
        /// <returns></returns>
        private string GetUniqueName(GameObject gameObject)
        {
            Guid guid = Guid.NewGuid();
            var bytes = guid.ToByteArray();
            int uid = (int)(bytes[0] | (bytes[1] << 8) | bytes[2] << 16);
            return gameObject.name + "_" + uid.ToString();
        }


        private List<ActorComp> GetActorComponents(GameObject obj)
        {
            List<Component> comps = new List<Component>();
            obj.GetComponents(comps);
            List<ActorComp> ret = new List<ActorComp>();

            if(CheckInherit(obj))
            {
                var removedComps = PrefabUtility.GetRemovedComponents(obj);
                if (removedComps.Count!=0)
                {
                    foreach(var removedComp in removedComps)
                    {
                        //屏蔽BlueprintActor，BPInit，ActorField脚本
                        if (removedComp == null
                            || removedComp.assetComponent == null
                            || removedComp.containingInstanceGameObject != obj
                            || removedComp.assetComponent is BlueprintActor
                            || removedComp.assetComponent is ActorFieldBase) continue;
                        var comp = removedComp.assetComponent.GetType();
                        ActorComp ac = new ActorComp(){
                            Name = comp.Name,
                            FullName = comp.FullName,
                            IsAdded = false,
                            IsRemoveComp = true,
                        };
                        ret.Add(ac);
                    }
                }
            }

            foreach (var comp in comps)
            {
                //屏蔽BlueprintActor，BPInit，ActorField脚本
                if (comp == null||comp is BlueprintActor||comp is ActorFieldBase||comp.GetType().Name.Equals("BPInit")) continue;

                ActorComp ac = new ActorComp()
                {
                    Name = comp.GetType().Name,
                    FullName = comp.GetType().FullName,
                    IsAdded = PrefabUtility.IsAddedComponentOverride(comp),
                    IsRemoveComp = false,
                };
                if (comp is Transform)
                {
                    var tran = comp as Transform;
                    ac.Position = tran.position;
                    ac.Rotation = tran.rotation.eulerAngles;
                    ac.Scale = tran.localScale;
                }

                ret.Add(ac);
            }
            return ret;
        }

        /// <summary>
        /// 检测该物体是否是继承的
        /// </summary>
        /// <param name="gameObject"></param>
        /// <returns></returns>
        private bool CheckInherit(GameObject gameObject)
        {
            PrefabStage stage = PrefabStageUtility.GetCurrentPrefabStage();
            GameObject sourceObj = PrefabUtility.GetCorrespondingObjectFromOriginalSource(gameObject);

            if (stage != null && stage.IsPartOfPrefabContents(gameObject))
            {
                return sourceObj != null;
            }
            else
            {
                return gameObject != sourceObj;
            }
        }

        /// <summary>
        /// 计算继承关系
        /// </summary>
        /// <param name="gameObject"></param>
        /// <param name="isInInnerPrefabRoot">是否在内嵌的prefab root下</param>
        /// <returns></returns>
        private void CalculateInherit(GameObject gameObject, bool isInInnerPrefabRoot)
        {
            if (gameObject != this.gameObject && PrefabUtility.IsOutermostPrefabInstanceRoot(gameObject))
            {
                actorParamDic[gameObject].isInherit = false;
                isInInnerPrefabRoot = true;
            }else
            {
                if (isInInnerPrefabRoot)
                {
                    actorParamDic[gameObject].isInherit = false;
                }
                else
                {
                    actorParamDic[gameObject].isInherit = CheckInherit(gameObject);
                }
            }

            foreach (Transform tran in gameObject.transform)
            {
                CalculateInherit(tran.gameObject, isInInnerPrefabRoot);
            }
        }

        private void CheckNameRepeat()
        {
            Dictionary<string, int> checkDic = new Dictionary<string, int>();

            foreach (BlueprintActorGoParam p in actorParamDic.Values)
            {
                if (checkDic.ContainsKey(p.uniqueName))
                {
                    Debug.LogError("actor 出现内部重名物体, 为蓝图actor运行时bug");
                    continue;
                }

                checkDic.Add(p.uniqueName, 0);
            }
        }
#endif

    }
}