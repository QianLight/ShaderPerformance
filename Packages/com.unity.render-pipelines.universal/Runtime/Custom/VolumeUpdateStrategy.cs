using System.Collections.Generic;

namespace UnityEngine.Rendering
{
    public class VolumeUpdateStrategy
    {
        private static VolumeUpdateStrategy _instance = new VolumeUpdateStrategy();
        public static VolumeUpdateStrategy instance => _instance;
        
        public delegate void UpdateFunc();

        public UpdateFunc UpdateFuncParam = () => { };

        private VolumeUpdateStrategy()
        {
            VolumeManager.instance.VolumeStrategicUpadte = StrategicUpadte;
        }

        public void Regist(VolumeComponent volume)
        {
            UpdateFuncParam += volume.OnOverrideFinish;
        }
        
        public void Unregist(VolumeComponent volume)
        {
            UpdateFuncParam -= volume.OnOverrideFinish;
        }
        void Upadate(int perFrame)
        {
            if (Time.frameCount % perFrame == 0)
            {
                UpdateFuncParam(); 
            }
        }

        public void StrategicUpadte(Camera camera)
        {
            int layerMask = camera.gameObject.layer;
            
            switch (layerMask)
            {
                case 0: // SceneCamera
                    Upadate(1);break;
                case 8: // UISceneCamera
                    Upadate(1);break;
                default:
                    Upadate(1);break;
            }
        }
    }

    public interface IRenderBinding
    {
        
    }
}