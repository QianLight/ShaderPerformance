using System;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace UnityEngine.Rendering.Universal
{
    [Serializable]
    public struct DOFParam
    {
        public bool active;
        public bool EasyMode;
        public float FocusDistance;
        public float BokehRangeFar;
        public float FocusRangeFar;
        [Range(0f,2f)]
        public float BlurRadius;
        [Range(0f,2f)]
        public float Intensity;
        public static DOFParam GetDefaultValue()
        {
            DOFParam param = new DOFParam();
            param.active = false;
            param.EasyMode = true;
            param.FocusDistance = 5;
            param.BokehRangeFar = 5;
            param.FocusRangeFar = 30;
            param.BlurRadius = 1;
            param.Intensity = 1;
            return param;
        }
    }
    
    public enum URPDOFSource : ushort
    {
        /// <summary>
        /// 一定要设置优先级
        /// </summary>
        Error = 0,
        /// <summary>
        /// 默认设置（关闭的参数）。
        /// </summary>
        DefualtValue,
        /// <summary>
        /// Timeline，还没迁移。
        /// </summary>
        Timeline,
        /// <summary>
        /// 技能节点
        /// </summary>
        SkillHelper,
        /// <summary>
        /// 编辑器预览。
        /// </summary>
        EditorPreview,
    }
    
    [Serializable]
    public class URPDepthOfField
    {
        private readonly OrderedParams<DOFParam> _params = new OrderedParams<DOFParam>();
        public static URPDepthOfField instance { get; } = new URPDepthOfField();
        public bool overrideByTimeline = false;
        private DOFParam _timelineParam = new DOFParam();
        public URPDepthOfField()
        {
            uint order = CalculateOrder(URPDOFSource.DefualtValue, 0);
            _params.Add(DOFParam.GetDefaultValue(), order);
        }

        public bool GetValue(out DOFParam param)
        {
            if (overrideByTimeline)
            {
                param = new DOFParam();
                param = _timelineParam;
                // _timelineParam = default;
                return true;
            }
            else
            {
                return _params.GetValue(out param);
            }
            
        }

        public int AddParam(DOFParam param, URPDOFSource source, int order)
        {
            uint uintOrder = CalculateOrder(source, order);
            return _params.Add(param, uintOrder);
        }
    
        public bool RemoveParam(int id)
        {
            return _params.Remove(id);
        }

        public bool ModifyParam(int id, DOFParam param)
        {
            return _params.Modify(id, param);
        }

        public bool ModifyTimelineParam(DOFParam param)
        {
            overrideByTimeline = true;
            _timelineParam = param;
            return true;
        }

        public void ResetParam()
        {
            overrideByTimeline = false;
        }
        private static uint CalculateOrder(URPDOFSource source, int order)
        {
            ushort ushortOrder = (ushort)(order + short.MaxValue);
            return ((uint)source << 16) + ushortOrder;
        }
        
    }
}