using UnityEngine;

namespace GMSDK
{
    public abstract class GMSDKObject
    {
        private static int _id = 1;
        private float _updateTimeLeft;
        private bool _removable;

        public int ID { get; private set; }

        // 构造一个不会移除的GSDKObject，如果需要销毁需要手动调用GSDKManager.Instance.RemoveObject
        protected GMSDKObject()
        {
            ID = _id ++;
            _removable = false;
        }
        
        // 构造一个定时的GSDKObject，在updateTime后自动销毁
        protected GMSDKObject(float updateTime)
        {
            _updateTimeLeft = updateTime;
            ID = _id ++;
            _removable = true;
        }
        
        #region Need Be Implement

        // 每一帧都会触发
        protected virtual void OnUpdate(float deltaTime){}
        
        // 使用定时的GSDKObject时，超出时间后触发
        protected virtual void OnTimeOut(){}

        #region Mono Events
        
        // 应用暂停时会触发
        public virtual void OnApplicationPause(bool pauseStatus){}
        
        // 应用退出时会触发
        public virtual void OnApplicationQuit(){}
        
        // 应用触发Disable生命周期时触发
        public virtual void OnDisable(){}

        #endregion
        
        #endregion

        public void Update()
        {
            var deltaTime = Time.deltaTime;
            OnUpdate(deltaTime);
            if (_updateTimeLeft > 0 && _removable)
            {
                _updateTimeLeft -= deltaTime;
                if (_updateTimeLeft <= 0)
                {
                    OnTimeOut();
                    GMSDKManager.Instance.RemoveObject(this);
                }
            }
        }
    }
}