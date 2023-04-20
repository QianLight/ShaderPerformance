using System.Collections.Generic;
using CFEngine;
using UnityEngine;

[DisallowMultipleComponent]
public class SceneAnimation : MonoBehaviour, IAnimationObject
{
    public virtual void Play(bool state, float speed)
    {
        isPlay = state;
    }

    public string exString = "";
    public float duration = -1;
    public bool autoPlay = true;
    [System.NonSerialized] public bool isPlay = false;

    public SceneAnimationData profile;
    private uint hash;

    private void Awake()
    {
        hash = EngineUtility.XHashLowerRelpaceDot(0, exString);
    }

    protected virtual void OnEnable()
    {
        if (EngineContext.IsRunning)
        {
            Dictionary<uint, IAnimationObject> map = EngineContext.instance.animationObjects;
            if (map.ContainsKey(hash))
            {
                Debug.LogError($"Add animation object already exist, exString={exString}, hash={hash}");
            }
            else
            {
                map[hash] = this;
            }
        }
    }

    protected virtual void OnDisable()
    {
        if (EngineContext.IsRunning)
        {
            EngineContext.instance.animationObjects.Remove(hash);
        }
    }

    private static void UpdateObjects(Transform t, List<ObjectCache> list)
    {
        if (t != null)
        {
            if (t.TryGetComponent(out Renderer r))
            {
                list.Add(new ObjectCache()
                {
                    t = t,
                    r = r
                });
            }

            for (int i = 0; i < t.childCount; ++i)
            {
                var child = t.GetChild(i);
                UpdateObjects(child, list);
            }
        }
    }

    public void GetObjects(Transform t, List<ObjectCache> list)
    {
        list.Clear();
        UpdateObjects(t, list);
    }

    public virtual SceneAnimationObject Serialize()
    {
        return null;
    }
}