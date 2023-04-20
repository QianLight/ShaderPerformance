using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace GSDK.RNU
{
    public class ModuleHolder
    {
        private BaseUnityModule moduleInner;
        private ArrayList methods = new ArrayList();
        private new HashSet<int> promiseMethods = new HashSet<int>();//new ArrayList();
        private ArrayList syncMethods = new ArrayList();

        private Dictionary<string, MethodInfo> methodsCache = new Dictionary<string, MethodInfo>();

        public ModuleHolder(BaseUnityModule module)
        {
            this.moduleInner = module;

            Type typz = module.GetType();
            MethodInfo[] allMethodsInfo = typz.GetMethods();
            foreach (var m in allMethodsInfo)
            {
                foreach (Attribute attr in m.GetCustomAttributes(false))
                {
                    if (attr is ReactMethod)
                    {
                        ReactMethod rm = (ReactMethod) attr;
                        
                        if (rm.isPromise)
                        {
                            promiseMethods.Add(methods.Count);
                        }
                        
                        methods.Add(m.Name);
                        methodsCache.Add(m.Name, m);
                        break;
                    }
                }
            }
        }

        public ArrayList GetMethods()
        {
            return methods;
        }
        public ArrayList GetPromiseMethods()
        {
            return new ArrayList(promiseMethods.ToList());
        }
        public ArrayList GetSyncMethods()
        {
            return syncMethods;
        }

        public string GetName()
        {
            return this.moduleInner.GetName();
        }

        public BaseUnityModule GetInnerUnityModule() {
            return this.moduleInner;
        }

        public Hashtable GetConstants()
        {
            Hashtable constants = this.moduleInner.GetConstants();
            if (constants == null)
            {
                return new Hashtable();
            }
            return constants;

        }

        public void invoke(int methodId, ArrayList argsFinal) {
            MethodInfo method = methodsCache[(string)methods[methodId]];

            if (promiseMethods.Contains(methodId))
            {
                // for some unkown reason ! succCall 应该是argsFinal.Count - 1 的位置。 but React/ReactNative在设计这里的时候，
                // 有问题
                int succCallID = (int) argsFinal[argsFinal.Count - 2];
                int failCallID = (int) argsFinal[argsFinal.Count - 1];
                argsFinal.RemoveRange(argsFinal.Count - 2, 2);

                Callback failCall = new Callback(failCallID);
                Callback sucCall = new Callback(succCallID);

                Promise p = new Promise(sucCall, failCall);
                argsFinal.Add(p);
            }
            
            object[] argsObjArr = argsFinal.ToArray();
            method.Invoke(moduleInner, argsObjArr);
        }
    }

}
