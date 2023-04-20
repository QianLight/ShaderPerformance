using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GSDK.RNU
{

    /*
    * 模块管理，所有模块注册在这里， 单例
    */
    public class ModuleManager
    {
        private List<ModuleHolder> allModules = new List<ModuleHolder>();

        public ModuleManager(RNUMainCore rnuContext)
        {

            // init all native module

            MainPackage mainPackage = new MainPackage();

            // init ui manager
            List<ViewManager> viewManagers = mainPackage.CreateViewManagers();
            UiManagerModule uiManagerModule = new UiManagerModule(viewManagers);
            allModules.Add(new ModuleHolder(uiManagerModule));
            
             // init api module
            List<BaseUnityModule> modules = mainPackage.CreateNativeModules(rnuContext);
            foreach (BaseUnityModule module in modules) {
                allModules.Add(new ModuleHolder(module));
            }
        }

        public List<ModuleHolder> GetAllModules() {
            return allModules;
        }

        public void CallNativeModule(int moduleId, int methodId, ArrayList args)
        {
            ModuleHolder mh = allModules[moduleId];
            mh.invoke(methodId, args);
        }
        public void CallEndOfBatch()
        {

           UiManagerModule uiManagerModule= GetUiManagerModule();
           uiManagerModule.OnBatchComplete();
        }


        public UiManagerModule GetUiManagerModule() {
            return (UiManagerModule) (allModules[0].GetInnerUnityModule());
        }

        public void Destroy() {
            foreach(ModuleHolder mh in allModules) {
               BaseUnityModule bum =  mh.GetInnerUnityModule();
               bum.Destroy();
            }
        }
    }
}
