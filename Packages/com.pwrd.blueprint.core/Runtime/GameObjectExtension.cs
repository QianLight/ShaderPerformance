using System;
using UnityEngine;

namespace Blueprint
{

    public static class GameObjectExtension
    {
        public static T GetOrAddComponent<T>(this GameObject obj) where T : Component
        {
            var comp = obj.GetComponent<T>();

            if (comp == null)
            {
                comp = obj.AddComponent<T>();
            }

            return comp;
        }
        public static Component GetOrAddComponent(this GameObject obj,Type componentType)
        {
            var comp = obj.GetComponent(componentType);

            if (comp == null)
            {
                comp = obj.AddComponent(componentType);
            }

            return comp;
        }
    }

}