using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Reflection;

namespace Blueprint.Actor
{
    using Blueprint;
    using Blueprint.Actor.EventSystem;
    public class ActorFieldBase : MonoBehaviour
    {
        // Start is called before the first frame update

        public ActorBase actor;
#if UNITY_EDITOR
        private void OnValidate()
        {
            if (Application.isPlaying && actor!=null)
            {
                var fieldRuntimeData = actor.GetType().GetFields();
                var fieldData = this.GetType().GetFields(BindingFlags.NonPublic|BindingFlags.Instance);
                foreach (var runtimeData in fieldRuntimeData)
                {
                    var data = fieldData.Where(t=>t.Name==runtimeData.Name).FirstOrDefault();
                    if(data!=null)
                        runtimeData.SetValue(actor,data.GetValue(this));
                }
            }
        }
        public void DestroyField() 
        {
            DestroyImmediate(this,true);
        }
#endif
    }

    [System.AttributeUsage(System.AttributeTargets.Class,AllowMultiple = false)]
    public class OrignBPClass : System.Attribute
    {
        string name;

        public OrignBPClass(string name)
        {
            this.name = name;
        }
        public string GetName()  
        {  
            return name;  
        }
    }

}
