using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace CFEngine
{
    public class CameraBlockGroup : MonoBehaviour
#if UNITY_EDITOR
        , ICheckableComponent
#endif
    {
        public enum BlockType
        {
            SceneObject,
            RoleOrMonster,
        }
        
        [HideInInspector]
        public CameraBlockConfig config = new CameraBlockConfig(0f, 0.2f, 0f, 0.3f);
        public List<Renderer> renderers = new List<Renderer>();
        public List<Collider> colliders = new List<Collider>();
        
        [Header("是否覆盖全局淡入淡出和透明度配置")]
        public bool isOverrideMiscConfig = false;

        public BlockType blockType = BlockType.SceneObject;

        private FlagArray flagArray;

        private void OnEnable()
        {
            if (!flagArray.IsCreated())
            {
                flagArray = new FlagArray(colliders.Count);
                for (int i = 0; i < colliders.Count; i++)
                {
                    Collider c = colliders[i];
                    if (c)
                    {
                        flagArray.SetFlag(i, c.enabled);
                    }
                }
            }

            CameraAvoidBlock.RegisterGroup(this);
            for (int i = 0; i < colliders.Count; i++)
            {
                Collider c = colliders[i];
                if (c)
                {
                    c.gameObject.layer = CameraAvoidBlock.Layer;
                }
            }
        }

        private void OnDisable()
        {
            CameraAvoidBlock.UnregisterGroup(this);
            for (int i = 0; i < colliders.Count; i++)
            {
                Collider c = colliders[i];
                if (c)
                {
                    c.enabled = flagArray.HasFlag(i);
                }
            }
        }

#if UNITY_EDITOR
        public bool Process()
        {
            renderers.RemoveNullElements();
            colliders.RemoveNullElements();
            return true;
        }
#endif
    }
}