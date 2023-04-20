using UnityEngine;

namespace GSDK.RNU {
    /**
    * ReactRawText 代表一个 字符串
    * <Text>aa</Text>  ---> <Text>
    *                           <RawText text="aa"/>
    *                       </Text>
    * 
    *  <Text>                               <Text>
    *      aa                     --->            <RawText text="aa"/>
    *      <Text>bb</Text>                        <Text> <RawText text="bb"/></Text>
    *   </Text>                             </Text> 
    *
    */
    public class ReactRawTextManager: BaseViewManager {
        override public string GetName() {
            return "RCTRawText";
        }

        protected override BaseView createViewInstance() {
            return null;
        }

        public override ReactSimpleShadowNode CreateShadowNodeInstance(int tag) {
            return new ReactRawTextShadowNode(tag);
        }
    }
}