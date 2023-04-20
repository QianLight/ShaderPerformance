using System;
using UnityEngine;
using System.Collections.Generic;
using System.Collections;

namespace Blueprint
{

    [NotReflectConstructorAttribute]
    public class DelayHandle{
        /// <summary>
        /// 用来计时的时间
        /// </summary>
        [NotReflectAttribute]
        public float time;
        /// <summary>
        /// 间隔时间
        /// </summary>
        [NotReflectAttribute]
        public float repeatTime;
        /// <summary>
        /// 执行方法
        /// </summary>
        [NotReflectAttribute]
        public Action action;
        /// <summary>
        /// 执行index++
        /// </summary>
        [NotReflectAttribute]
        public Action indexAction;
        /// <summary>
        /// 重复次数, -1表示无限循环
        /// </summary>
        [NotReflectAttribute]
        public int repeat;
        /// <summary>
        /// completedAcion
        /// </summary>
        [NotReflectAttribute]
        public Action completedAcion;
        /// <summary>
        /// 启动改delay的对象
        /// </summary>
        [NotReflectAttribute]
        public object delayObject;
        /// <summary>
        /// 该delay的唯一名称
        /// </summary>
        [NotReflectAttribute]
        public string uniqueName;

        [DisplayName("Delay Stop")]
        public static void DelayStop(DelayHandle handle)
        {
            DelayControl.Instance.RemoveDelay(handle);
        }
    }
    public class DelayControl : MonoBehaviour
    {

        public static DelayControl Instance;
        private Dictionary<System.Object,Dictionary<string,DelayHandle>> scheduledList = new Dictionary<object, Dictionary<string, DelayHandle>>();
        private Dictionary<System.Object,List<string>> removeSourceList = new Dictionary<object, List<string>>();
        private List<Action> delayActionList = new List<Action>();
        private List<Action> nextFrameActions = new List<Action>();
        private List<Action> completedActions = new List<Action>();

        public static void Init()
        {
            if (Instance != null)
            {
                return;
            }
            if (!Application.isPlaying)
                return;

            var obj = new GameObject("DelayControl");
            Instance = obj.AddComponent<DelayControl>();

            DontDestroyOnLoad(obj);
        }

        /// <summary>
        /// 删除该object中的所有delay
        /// </summary>
        /// <param name="uniqueObject"></param>
        public void RemoveAllDelay(System.Object uniqueObject)
        {
            if (scheduledList.ContainsKey(uniqueObject))
                scheduledList.Remove(uniqueObject);
        }

        /// <summary>
        /// 删除指定delay
        /// </summary>
        /// <param name="delay"></param>
        public bool RemoveDelay(DelayHandle delay)
        {
            scheduledList.TryGetValue(delay.delayObject, out var handleDic);
            
            if (handleDic != null && handleDic.ContainsKey(delay.uniqueName))
            {
                var handle = handleDic[delay.uniqueName];
                handle.completedAcion?.Invoke();
                handleDic.Remove(delay.uniqueName);
                return true;
            }

            return false;
        }

        public void Delay(System.Object uniqueObject,string uniqueFunctionName, Action action, float duration)
        {
            DelayHandle delayItem = new DelayHandle{
                time = duration,
                action = action,
                repeat = 1,
                delayObject = uniqueObject,
                uniqueName = uniqueFunctionName,
            };
            if (scheduledList.ContainsKey(uniqueObject))
            {
                if(scheduledList[uniqueObject].ContainsKey(uniqueFunctionName))
                {
                    return;
                }
                else
                {
                    scheduledList[uniqueObject].Add(uniqueFunctionName,delayItem);
                }
            }
            else
            {
                scheduledList.Add(uniqueObject,new Dictionary<string, DelayHandle>());
                scheduledList[uniqueObject].Add(uniqueFunctionName,delayItem);
            }
        }
        public DelayHandle DelayLoop(System.Object uniqueObject,string uniqueFunctionName, Action action, float duration, int repeat, Action indexAdd, bool includeFirst,Action completedAction)
        {
            if(repeat == 0)
            {
                completedAction.Invoke();
                return null;
            }
            else if (repeat < 0) 
            {
                repeat = -1;
            }

            DelayHandle delayItem = new DelayHandle{
                action = action,
                repeat = repeat,
                repeatTime = duration,
                indexAction = indexAdd,
                completedAcion = completedAction,
                delayObject = uniqueObject,
                uniqueName = uniqueFunctionName,
            };
            if(includeFirst)
                delayItem.time = duration;

            if (scheduledList.ContainsKey(uniqueObject))
            {
                if(scheduledList[uniqueObject].ContainsKey(uniqueFunctionName))
                {
                    return scheduledList[uniqueObject][uniqueFunctionName];
                }
                else
                {
                    scheduledList[uniqueObject].Add(uniqueFunctionName,delayItem);
                }
            }
            else
            {
                scheduledList.Add(uniqueObject,new Dictionary<string, DelayHandle>());
                scheduledList[uniqueObject].Add(uniqueFunctionName,delayItem);
            }

            return delayItem;
        }

        void Update()
        {
            if(scheduledList == null)
            {
                return;
            }

            if (nextFrameActions.Count > 0)
            {
                foreach (var item in nextFrameActions)
                {
                    item.Invoke();
                }
                nextFrameActions.Clear();
            }

            foreach (var objectScheduledList in scheduledList)
            {
                if(objectScheduledList.Value.Count != 0)
                {
                    foreach (var item in objectScheduledList.Value)
                    { 
                        item.Value.time -= Time.deltaTime;
                        if(item.Value.time <= 0)
                        {
                            delayActionList.Add(item.Value.action);
                            item.Value.indexAction?.Invoke();

                            if(item.Value.repeat == 1){
                                if(removeSourceList.ContainsKey(objectScheduledList.Key))
                                    removeSourceList[objectScheduledList.Key].Add(item.Key);
                                else
                                {
                                    List<string> addList = new List<string>();
                                    addList.Add(item.Key);
                                    removeSourceList.Add(objectScheduledList.Key,addList);
                                }
                                if(item.Value.completedAcion != null)
                                    delayActionList.Add(item.Value.completedAcion);
                            }
                            else 
                            {
                                item.Value.repeat -= 1;

                                // 如果是负数，则表示无限循环
                                if (item.Value.repeat < 0)
                                {
                                    item.Value.repeat = -1;
                                }
                            }
                            item.Value.time = item.Value.repeatTime;
                        }
                    }
                }
            }

            //添加到下一帧执行队列中
            if (delayActionList.Count > 0)
            {
                lock (delayActionList)
                {
                    nextFrameActions.AddRange(delayActionList);
                    delayActionList.Clear();
                }
            }

            //移除已执行事件 
            foreach (var item in removeSourceList)
            {
                foreach (var removeNode in item.Value)
                {
                    scheduledList[item.Key].Remove(removeNode);
                }
            }
            removeSourceList.Clear();
        }
        public void OnDestroy()
        {
            scheduledList.Clear();
            delayActionList.Clear();
        }

    }
}