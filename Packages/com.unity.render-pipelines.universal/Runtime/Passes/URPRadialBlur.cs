using System;
using System.Collections.Generic;

namespace UnityEngine.Rendering.Universal
{
    [Serializable]
    public struct RadialBlurParam
    {
        public bool active;
        public Vector3 center;
        public float size;
        public float innerRadius;
        public float innerFadeOut;
        public float outerRadius;
        public float outerFadeOut;
        public float intensity;
        public bool useScreenPos;

        public static RadialBlurParam GetDefualtValue()
        {
            RadialBlurParam param = new RadialBlurParam();
            param.active = false;
            param.center = new Vector3(0.5f, 0.5f, 10f);
            param.size = 1f;
            param.innerRadius = 0.0f;
            param.innerFadeOut = 0.2f;
            param.outerRadius = 1.0f;
            param.outerFadeOut = 0.2f;
            param.intensity = 0.3f;
            param.useScreenPos = true;
            return param;
        }

        public static RadialBlurParam InitValue(object[] array)
        {
            RadialBlurParam param = new RadialBlurParam();
            param.active = (bool)array[0];
            param.center = (Vector3)array[1];
            param.size = (float)array[2];
            param.innerRadius= (float)array[3];
            param.innerFadeOut= (float)array[4]; 
            param.outerRadius= (float)array[5]; ;
            param.outerFadeOut= (float)array[6]; ;
            param.intensity= (float)array[7]; ;
            param.useScreenPos = (bool)array[8];
            return param;
        }
    }

    /// <summary>
    /// 径向模糊的功能优先级列表。
    /// </summary>
    public enum URPRadialBlurSource : ushort
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

    /// <summary>
    /// 可以保存多个按照优先级排序好的参数。
    /// </summary>
    public class OrderedParams<TParam>
    {
        private struct Operation
        {
            public int id;
            public uint priority;
            public TParam param;
        }

        private const int ERROR_COUNT = 100;
        private readonly List<Operation> operations = new List<Operation>();
        private static int increasingID;
        
        public bool GetValue(out TParam param)
        {
            if (operations.Count == 0)
            {
                param = default;
                return false;
            }

            param = operations[operations.Count - 1].param;
            return true;
        }
        
        public int Add(TParam param, uint priority)
        {
            // Create operation.
            Operation operation = new Operation();
            operation.id = increasingID++;
            operation.param = param;
            operation.priority = priority;

            // Insert operation.
            operations.Add(default);
            int index = operations.Count - 2;
            while (index >= 0 && operations[index].priority > priority)
            {
                operations[index + 1] = operations[index];
                index--;
            }
            operations[index + 1] = operation;
            
            #if UNITY_EDITOR
            if (operations.Count > ERROR_COUNT)
            {
                Debug.LogError($"{GetType()}.{nameof(Add)}: Param count too much! threshold = ({ERROR_COUNT}), current count = {operations.Count}");
            }
            #endif
            
            return operation.id;
        }

        public bool Modify(int id, TParam param)
        {
            for (int i = 0; i < operations.Count; i++)
            {
                var operation = operations[i];
                if (operation.id == id)
                {
                    operation.param = param;
                    operations[i] = operation;
                    return true;
                }
            }

            Debug.LogError($"URPRadialBlur.ModifyOperation: Modify operation fail, target operation not exist, id = {id}");
            return false;
        }

        public bool Remove(int id)
        {
            for (int i = 0; i < operations.Count; i++)
            {
                Operation op = operations[i];
                if (op.id == id)
                {
                    operations.RemoveAt(i);
                    return true;
                }
            }

            Debug.LogError($"{GetType()}.{nameof(Remove)}: The param that try to remove not exist, id = {id}");
            return false;
        }

        public bool Contains(int id)
        {
            for (int i = 0; i < operations.Count; i++)
            {
                Operation op = operations[i];
                if (op.id == id)
                {
                    return true;
                }
            }

            return false;
        }
    }

    [Serializable]
    public class URPRadialBlur
    {
        private readonly OrderedParams<RadialBlurParam> @params = new OrderedParams<RadialBlurParam>();
        
        public static URPRadialBlur instance { get; } = new URPRadialBlur();

        public const int BLUR_SAMPLE_COUNT = 4;
        
        public URPRadialBlur()
        {
            uint order = CalculateOrder(URPRadialBlurSource.DefualtValue, 0);
            @params.Add(RadialBlurParam.GetDefualtValue(), order);
        }

        public bool GetValue(out RadialBlurParam param)
        {
            return @params.GetValue(out param);
        }

        public int AddParam(RadialBlurParam param, URPRadialBlurSource source, int order)
        {
            uint uintOrder = CalculateOrder(source, order);
            return @params.Add(param, uintOrder);
        }

        public bool RemoveParam(int id)
        {
            return @params.Remove(id);
        }

        public bool ModifyParam(int id, RadialBlurParam param)
        {
            return @params.Modify(id, param);
        }

        private static uint CalculateOrder(URPRadialBlurSource source, int order)
        {
            ushort ushortOrder = (ushort)(order + short.MaxValue);
            return ((uint)source << 16) + ushortOrder;
        }
    }
}