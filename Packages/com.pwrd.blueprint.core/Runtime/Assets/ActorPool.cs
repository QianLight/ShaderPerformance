using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using UnityEngine.SceneManagement;
using Blueprint.Asset;

namespace Blueprint
{
    /// <summary>
    /// 单例模板
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class Singleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        private static T instance;
        public static T Instance
        {
            get
            {
                return instance;
            }
            set
            {
                instance = value;
            }
        }

        public static void Init()
        {
            if (Instance != null)
            {
                return;
            }
            if (!Application.isPlaying)
                return;

            var obj = new GameObject("ObjectPool");
            Instance = obj.AddComponent<T>();

            DontDestroyOnLoad(obj);
        }
        protected virtual void Awake()
        {
            if (instance == null)
            {
                instance = this as T;
            }
        }
    }

    /// <summary>
    /// 单个缓存池对象,既一个抽屉
    /// </summary>
    public class PoolData
    {
        public Transform ParentTransform; //该缓存池的父物体,既柜子的某个抽屉

        public Queue<GameObject> ObjectPool; //存储创建的所有物体,既ParentObject抽屉中的所有文件

        GameObject Asset = null; //持有的GameObject

        public Action ClearPoolAction;

        /// <summary>
        /// 使用构造函数创建对象池
        /// </summary>
        /// <param name="obj">要放入池子中的物体</param>
        /// <param name="poolObj">该池子的父物体</param>
        /// <param name="count">需要初始化池子的大小</param>
        public PoolData(string path, int count, Transform parentTransform = null)
        {
            Asset = BpAssetManager.LoadAsset<GameObject>(path);
            if (Asset != null)
            {
                ParentTransform = parentTransform;
                ObjectPool = new Queue<GameObject>();
                for (int i = 0; i < count; i++)
                {
                    GameObject objClone = GameObject.Instantiate(Asset) as GameObject;//克隆Prefab
                    objClone.transform.SetParent(ParentTransform);//为克隆出来的物体指定父物体
                    objClone.name = objClone.name + "_" + i.ToString();//更改克隆出来的名字,为其加上序号
                    objClone.SetActive(false);//状态设为false
                    ObjectPool.Enqueue(objClone);//将克隆出来的物体加入对象池
                }
            }
        }

        public PoolData(string path, Transform parentTransform = null)
        {
            ObjectPool = new Queue<GameObject>();
            ParentTransform = parentTransform;
            Asset = BpAssetManager.LoadAsset<GameObject>(path);
        }

        /// <summary>
        /// 从对象池中获取物体
        /// </summary>
        /// <returns></returns>
        public GameObject GetObject(Transform parentTransform)
        {
            GameObject ret;
            if (ObjectPool.Count > 0)//对象池中还有物体
            {
                ret = ObjectPool.Dequeue();//移除并返回位于 Queue 开始处的对象
            }
            else//对象池中没有物体
            {
                ret = GameObject.Instantiate(Asset) as GameObject;//从Prefab复制一个出来
            }
            ret.transform.SetParent(parentTransform);//将取出的物体的父物体设为null
            ret.SetActive(true);
            return ret;
        }

        /// <summary>
        /// 设置池子大小
        /// </summary>
        /// <param name="size"></param>
        public void SetSize(int size)
        {
            while (ObjectPool.Count > size)
            {
                GameObject ret = ObjectPool.Dequeue();
                GameObject.Destroy(ret);
            }
            while (ObjectPool.Count < size)
            {
                GameObject objClone = GameObject.Instantiate(Asset) as GameObject;//克隆Prefab
                objClone.transform.SetParent(ParentTransform);//为克隆出来的物体指定父物体
                objClone.SetActive(false);//状态设为false
                ObjectPool.Enqueue(objClone);//将克隆出来的物体加入对象池
            }
        }

        /// <summary>
        /// 将物体放回池子中
        /// </summary>
        /// <param name="obj">将要放回的物体</param>
        public void PushObject(GameObject obj)
        {
            obj.SetActive(false);
            ObjectPool.Enqueue(obj);
            obj.transform.SetParent(ParentTransform);//为放进池子的物体指定父物体
        }

        /// <summary>
        /// 获取池子的大小
        /// </summary>
        /// <returns></returns>
        public int GetPoolSize()
        {
            return ObjectPool.Count;
        }

        /// <summary>
        /// 清空池子
        /// </summary>
        public void Clear()
        {
            while (ObjectPool.Count > 0)
            {
                GameObject.Destroy(ObjectPool.Dequeue());
            }
            ClearPoolAction?.Invoke();
        }
    }

    /// <summary>
    /// 缓存池加载进度
    /// </summary>
    public class PoolLoadInfo
    {
        private int m_AllValue = 0;

        private bool m_Complete = false;

        private int m_CompleteValue = 0;

        private Action m_CompleteAction;

        private string m_PrePath = string.Empty;

        private List<string> m_LoadingObjects = new List<string>();

        // 总生成数量
        public int TotalNumber {
            get
            {
                return m_AllValue;
            }
        }
        // 是否完成
        public bool IsComplete {
            get
            {
                return m_Complete;
            }
        }
        // 已完成数量
        public int LoadedNumber {
            get
            {
                return m_CompleteValue;
            }
        }
        // 完成回调
        [NotReflectAttribute]
        public Action CompleteAction {
            get
            {
                return m_CompleteAction;
            }
        }
        // 当前加载
        [NotReflectAttribute]
        public string PrePath {
            get
            {
                return m_PrePath;
            }
        }
        // 加载列表
        [NotReflectAttribute]
        public List<string> LoadingObjects {
            get
            {
                return m_LoadingObjects;
            }
        }

        [NotReflectAttribute]
        public void SetAllValue(int val)
        {
            m_AllValue = val;
        }

        [NotReflectAttribute]
        public void SetComplete(bool val)
        {
            m_Complete = val;
        }

        [NotReflectAttribute]
        public void SetCompleteValue(int val)
        {
            m_CompleteValue = val;
        }

        [NotReflectAttribute]
        public void SetCompleteAction(Action action)
        {
            m_CompleteAction = action;
        }

        [NotReflectAttribute]
        public void SetPrePath(string val)
        {
            m_PrePath = val;
        }
    }

    /// <summary>
    /// 缓存池管理器
    /// </summary>
    public class PoolManager : Singleton<PoolManager>
    {
        protected override void Awake()
        {
            base.Awake();
        }

        private GameObject Asset = null; //持有的对象

        private Dictionary<string, PoolData> PoolDic = new Dictionary<string, PoolData>();//<Prefab路径,生成的缓存池>,既整个柜子

        public List<PoolLoadInfo> LoadingInfos= new List<PoolLoadInfo>();

        /// <summary>
        /// 同步创建缓存池，既根据path创建对应名字的抽屉
        /// </summary>
        /// <param name="path"></param>
        public void CreatePoolByMap(Dictionary<string,int> loadObject, Transform parentTransform = null)
        {
            GameObject parentObj;
            if (parentTransform == null)
            {
                parentObj = this.transform.gameObject;
            }
            else
                parentObj = parentTransform.gameObject;
            foreach (var item in loadObject)
            {
                var path = (BPClassManager.Instance.GetClass("Blueprint.UnityLogic." + item.Key).GetCustomAttributes(false).FirstOrDefault() as LoadPathAttribute).path;
                if (!PoolDic.ContainsKey(path))
                {
                    PoolData poolData = new PoolData(path, item.Value, parentObj.transform);
                    PoolDic.Add(path, poolData);
                }
            }
        }

        public PoolLoadInfo AsyncCreatePoolByMap(Dictionary<string, int> loadObject, Action action = null, Transform parentTransform = null)
        {
            GameObject parentObj;
            if (parentTransform == null)
            {
                parentObj = this.transform.gameObject;
            }
            else
                parentObj = parentTransform.gameObject;
            PoolLoadInfo info = new PoolLoadInfo();
            LoadingInfos.Add(info);
            info.SetCompleteAction(action);
            info.SetAllValue(0);
            info.SetCompleteValue(0);
            foreach(var item in loadObject)
            {
                var classInstance = BPClassManager.Instance.GetClass("Blueprint.UnityLogic." + item.Key);
                if (classInstance == null)
                    continue;
                var path = (classInstance.GetCustomAttributes(false).FirstOrDefault() as LoadPathAttribute).path;
                info.SetAllValue(info.TotalNumber + item.Value);
                int index = item.Value;
                while (index != 0)
                {
                    index--;
                    info.LoadingObjects.Add(path);
                }
                if (!PoolDic.ContainsKey(path))
                {
                    PoolData poolData = new PoolData(path, parentObj.transform);
                    PoolDic.Add(path, poolData);
                }
            }
            return info;
        }

        /// <summary>
        /// 同步创建缓存池，既根据path创建对应名字的抽屉
        /// </summary>
        /// <param name="path"></param>
        public void CreatePool(Dictionary<string, int> loadObject, Transform parentTransform = null)
        {
            GameObject parentObj;
            if (parentTransform == null)
            {
                parentObj = this.transform.gameObject;
            }
            else
                parentObj = parentTransform.gameObject;
            foreach (var item in loadObject)
            {
                if (!PoolDic.ContainsKey(item.Key))
                {
                    PoolData poolData = new PoolData(item.Key, item.Value, parentObj.transform);
                    PoolDic.Add(item.Key, poolData);
                }
            }
        }

        public PoolLoadInfo AsyncCreatePool(Dictionary<string, int> loadObject, Action action = null, Transform parentTransform = null)
        {
            GameObject parentObj;
            if (parentTransform == null)
            {
                parentObj = this.transform.gameObject;
            }
            else
                parentObj = parentTransform.gameObject;
            PoolLoadInfo info = new PoolLoadInfo();
            LoadingInfos.Add(info);
            info.SetCompleteAction(action);
            info.SetAllValue(0);
            info.SetCompleteValue(0);
            foreach (var item in loadObject)
            {
                info.SetAllValue(info.TotalNumber + item.Value);
                int index = item.Value;
                while (index != 0)
                {
                    index--;
                    info.LoadingObjects.Add(item.Key);
                }
                if (!PoolDic.ContainsKey(item.Key))
                {
                    PoolData poolData = new PoolData(item.Key, parentObj.transform);
                    PoolDic.Add(item.Key, poolData);
                }
            }
            return info;
        }

        public void Update()
        {
            int i = 100;
            while (--i >= 0)
            {
                if (LoadingInfos.Count != 0)
                {
                    PoolLoadInfo info = LoadingInfos[0];
                    if (info.LoadingObjects.Count != 0)
                    {
                        var path = info.LoadingObjects[0];
                        if (info.PrePath != path)
                        {
                            Asset = BpAssetManager.LoadAsset<GameObject>(path);
                            info.SetPrePath(path);
                        }
                        GameObject objClone = GameObject.Instantiate(Asset) as GameObject;
                        PoolDic[path].PushObject(objClone);
                        info.SetCompleteValue(info.LoadedNumber + 1);
                        info.LoadingObjects.RemoveAt(0);
                    }
                    else
                    {
                        info.SetComplete(true);
                        OnComplete(info);
                        LoadingInfos.RemoveAt(0);
                    }
                }
            }
        }
        private void OnComplete(PoolLoadInfo info)
        {
            info.CompleteAction?.Invoke();
        }

        /// <summary>
        /// 从整个柜子中取物体,根据传入path自动决定从哪个抽屉中取
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public GameObject GetObject(string path, Transform parentTransform = null)
        {
            if (!PoolDic.ContainsKey(path))//未创建的话先创建后取
            {
                GameObject parentObj;
                if (parentTransform == null)
                {
                    parentObj = this.transform.gameObject;
                }
                else
                    parentObj = parentTransform.gameObject;
                PoolData poolData = new PoolData(path, 1, parentObj.transform);
                PoolDic.Add(path, poolData);
            }
            GameObject ret = PoolDic[path].GetObject(parentTransform);
            if(parentTransform == null)
                SceneManager.MoveGameObjectToScene(ret, SceneManager.GetActiveScene());
            return ret;
        }

        /// <summary>
        /// 将物体放入池子中，根据path自动决定放入哪个
        /// </summary>
        /// <param name="path">物体路径，需要哪个抽屉</param>
        /// <param name="obj">物体，放入的物体</param>
        public void Recycle(string path, GameObject obj)
        {
            if (!PoolDic.ContainsKey(path))
            {
                PoolData poolData = new PoolData(path, 0, this.gameObject.transform);
                PoolDic.Add(path, poolData);
            }
            if (PoolDic[path].ParentTransform == null)
                PoolDic[path].ParentTransform = this.gameObject.transform;
            PoolDic[path].PushObject(obj);
        }

        /// <summary>
        /// 获取某个池子的大小，既抽屉中当前物体数
        /// </summary>
        /// <param name="path">路径，既抽屉名</param>
        /// <returns></returns>
        public int GetPoolSize(string path)
        {
            if (PoolDic.ContainsKey(path))
            {
                return PoolDic[path].GetPoolSize();
            }
            return 0;
        }

        /// <summary>
        /// 设置池子大小
        /// </summary>
        /// <param name="path"></param>
        /// <param name="size"></param>
        public void SetPoolSize(string path, int size)
        {
            if (PoolDic.ContainsKey(path))
            {
                PoolDic[path].SetSize(size);
            }
        }

        /// <summary>
        /// 清空某个池子，既清空抽屉中的物体
        /// </summary>
        /// <param name="path"></param>
        public void ClearPool(string path)
        {
            if (PoolDic.ContainsKey(path))
            {
                PoolDic[path].Clear();
            }
        }

        /// <summary>
        /// Destory掉某个池子，既直接将path的抽屉扔掉
        /// </summary>
        /// <param name="path">路径</param>
        public void DestroyPool(string path)
        {
            if (PoolDic.ContainsKey(path))
            {
                PoolDic[path].Clear();
                PoolDic.Remove(path);
            }
        }

        /// <summary>
        /// 清空所有池子
        /// </summary>
        public void DestroyAllPool()
        {
            foreach (var pool in PoolDic)
            {
                pool.Value.Clear();
            }
            PoolDic.Clear();
        }
    }

}