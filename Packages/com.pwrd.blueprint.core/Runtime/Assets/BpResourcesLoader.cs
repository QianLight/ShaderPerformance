using Blueprint.Actor;
using UnityEngine;
using System;

namespace Blueprint.Asset
{
    public class BpResourcesLoader : IAssetLoader
    {
        public bool Inited()
        {
            return true;
        }

        public T SpawnActor<T>(string path) where T : ActorBase
        {
            ActorBase ab = SpawnActor(path);
            return ab != null ? (T)ab : null;
        }

        public T SpawnActor<T>(string path, Transform parent = null) where T : ActorBase
        {
            ActorBase ab = SpawnActor(path, parent);
            return ab != null ? (T)ab : null;
        }

        public ActorBase SpawnActor(string path)
        {
            return SpawnActor(path, null);
        }

        public ActorBase SpawnActor(string path, Transform parent = null)
        {
            GameObject obj = Resources.Load<GameObject>(path);

            if (obj != null && obj.GetComponent<BlueprintActor>())
            {
                GameObject actor = UnityEngine.Object.Instantiate(obj) as GameObject;
                ActorBase ab = actor.GetComponent<BlueprintActor>().BpActor;

                if (parent != null)
                {
                    actor.transform.SetParent(parent);
                }

                return ab;
            }
            else 
            {
                Debug.LogWarning("Can't find actor at path:" + path);
            }

            return null;
        }

        public T LoadAsset<T>(string path) where T : UnityEngine.Object
        {
            return Resources.Load<T>(path);
        }

        public T LoadAsset<T>(string path, Action release) where T : UnityEngine.Object
        {
            return Resources.Load<T>(path);
        }
    }

}
