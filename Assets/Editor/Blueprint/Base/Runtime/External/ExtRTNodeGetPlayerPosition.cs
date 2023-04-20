using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEditor;

namespace BluePrint
{
    class ExtRTNodeGetPlayerPosition : BlueprintRuntimeDataNode<ExtGetPlayerPositionData>
    {
        GameObject fakePlayer;

        //public ExtRTNodeGetPlayerPosition(BlueprintRuntimeGraph e) : base(e) { }

        public override void Init(ExtGetPlayerPositionData data, bool AutoStreamPin = true)
        {
            base.Init(data, false);
            BlueprintRuntimeValuedPin pinDataOut = new BlueprintRuntimeValuedPin(this, 1, PinType.Data, PinStream.Out, VariantType.Var_Vector3);
            pinDataOut.SetValueSource(GetValue);
            AddPin(pinDataOut);

            // simulation player position
            GameObject prefab = (GameObject)AssetDatabase.LoadAssetAtPath("Assets/Editor Default Resources/Tmp/dabai.prefab", typeof(GameObject));
            fakePlayer = GameObject.Instantiate(prefab);
            fakePlayer.name = "SimulationPlayer";

            Ray ray = SceneView.lastActiveSceneView.camera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 1.0f));
            int layerMask = (1 << 9 | 1);
            RaycastHit hitInfo;
            if (Physics.Raycast(ray, out hitInfo, Mathf.Infinity, layerMask))
            {
                fakePlayer.transform.position = hitInfo.point + new Vector3(0, 0.5f, 0);
            }
        }

        protected BPVariant GetValue()
        {
            BPVariant ret = default;
            ret.type = VariantType.Var_Vector3;

            if(fakePlayer != null)
                ret.val._vec3 = fakePlayer.transform.position;
            return ret;
        }

        public override void UnInit()
        {
            if (fakePlayer != null)
            {
                UnityEngine.Object.DestroyImmediate(fakePlayer);
                fakePlayer = null;
            }

            base.UnInit();
        }

        public override void OnEndSimulation()
        {
            UnInit();
        }
    }

    class ExtRTNodeGetPlayerHP : BlueprintRuntimeDataNode<BluePrintNodeBaseData>
    {
        public override void Init(BluePrintNodeBaseData data, bool AutoStreamPin = true)
        {
            base.Init(data, false);
            BlueprintRuntimeValuedPin pinDataOut = new BlueprintRuntimeValuedPin(this, 1, PinType.Data, PinStream.Out, VariantType.Var_Float);
            pinDataOut.SetValueSource(GetValue);
            AddPin(pinDataOut);
        }

        protected BPVariant GetValue()
        {
            BPVariant ret = default;
            ret.type = VariantType.Var_Float;
            ret.val._float = 100.0f;
            return ret;
        }
    }

    class ExtRTNodeGetPartnerAttr : BlueprintRuntimeDataNode<ExtGetPartnerAttrData>
    {
        public override void Init(ExtGetPartnerAttrData data, bool AutoStreamPin = true)
        {
            base.Init(data, false);
            BlueprintRuntimeValuedPin pinDataOut = new BlueprintRuntimeValuedPin(this, 1, PinType.Data, PinStream.Out, VariantType.Var_Float);
            pinDataOut.SetValueSource(GetValue);
            AddPin(pinDataOut);
        }

        protected BPVariant GetValue()
        {
            BPVariant ret = default;
            ret.type = VariantType.Var_Float;
            ret.val._float = 100.0f;
            return ret;
        }
    }
}
