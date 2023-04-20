using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BluePrint
{
    class ExtRTNodeInRange : BlueprintRuntimeDataNode<ExtInRangeData>
    {
        BlueprintRuntimeValuedPin _InPin_Vec3;
        BlueprintRuntimeValuedPin _OutPin_Bool;

        //public ExtRTNodeInRange(BlueprintRuntimeGraph e) : base(e)
        //{
        //}

        public override void Init(ExtInRangeData data, bool AutoStreamPin = true)
        {
            base.Init(data, false);
            AddPin(_InPin_Vec3 = new BlueprintRuntimeValuedPin(this, 1, PinType.Data, PinStream.In, VariantType.Var_Vector3));

            _OutPin_Bool = new BlueprintRuntimeValuedPin(this, 2, PinType.Data, PinStream.Out, VariantType.Var_Bool);
            _OutPin_Bool.SetValueSource(GetValue);
            AddPin(_OutPin_Bool);
        }

        public BPVariant GetValue()
        {
            BPVariant ret = default;
            ret.type = VariantType.Var_Bool;

            BPVariant data1 = new BPVariant();
            if (_InPin_Vec3.GetPinValue(ref data1))
            {
                if(data1.type == VariantType.Var_Vector3)
                {
                    float d = Vector3.Distance(data1.val._vec3, HostData.Center);
                    if (d <= HostData.Radius)
                        ret.val._bool = true;
                }
            }

            return ret;
        }
    }
}
