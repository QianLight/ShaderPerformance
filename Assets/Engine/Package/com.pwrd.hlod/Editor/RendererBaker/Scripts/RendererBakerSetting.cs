using System;

namespace com.pwrd.hlod.editor
{
    [Serializable]
    public class RendererBakerSetting
    {
        public bool useCustomBakeLayerMask = false;
        public int bakeLayerMask = 22;  //默认设置较高层级，防止重复
        public BakeRTSize bakeRTSize = BakeRTSize._2048;
        
        public RendererBakerSetting Clone()
        {
            return new RendererBakerSetting()
            {
                useCustomBakeLayerMask = this.useCustomBakeLayerMask,
                bakeLayerMask = this.bakeLayerMask,
                bakeRTSize = this.bakeRTSize,
            };
        }
    }
}