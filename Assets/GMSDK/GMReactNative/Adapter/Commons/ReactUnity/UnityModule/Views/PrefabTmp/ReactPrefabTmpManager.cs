using UnityEngine;

namespace GSDK.RNU {
    public class ReactPrefabTmpManager: BaseViewManager
    {
        public static string ReactViewName = "RNUPrefabTmp";

        public override string GetName() {
            return ReactViewName;
        }

        protected override BaseView createViewInstance() {
            return new ReactPrefabTmp();
        }
        
        [ReactProp(name = "prefabName")]
        public void SetPrefabName(BaseView view, string prefabName)
        {
            ((ReactPrefabTmp)view).SetPrefabName(prefabName);
        }
    }
}