using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
namespace CFEngine
{

    [Serializable]
    public struct Activedata
    {
        public string exstring;
        public Transform _targetgroup;
    }

    [ExecuteInEditMode]
    public class AnimationControl : MonoBehaviour
    {
        public List<Activedata> _activedatalist = new List<Activedata>();
        private List<string> _tempkeys = new List<string>();
        // Start is called before the first frame update
        void OnEnable()
        {
#if UNITY_EDITOR
            GetDATA();
#endif
        }
#if UNITY_EDITOR
        private void GetDATA()
        {
            ActiveObject[] _acteveobject = transform.Find("MainScene").GetComponentsInChildren<ActiveObject>();
            if (_acteveobject != null && _acteveobject.Length > 0)
            {
                for (int i = 0; i < _acteveobject.Length; i++)
                {
                    bool havedata = false;
                    if (_activedatalist.Count == 0)
                    {
                        Activedata _activedata = new Activedata();
                        _activedata._targetgroup = _acteveobject[i].animationTargetGroup;
                        _activedata.exstring = _acteveobject[i].exString;
                        _activedatalist.Add(_activedata);
                    }
                    else
                    {
                        for (int x = 0; x < _activedatalist.Count; x++)
                        {
                            if (_activedatalist[x].exstring.Equals(_acteveobject[i].exString) || _activedatalist[x]._targetgroup == _acteveobject[i].animationTargetGroup)
                            {
                                havedata = true;
                                break;
                            }
                        }
                    }
                    if (!havedata)
                    {
                        Activedata _activedata = new Activedata();
                        _activedata._targetgroup = _acteveobject[i].animationTargetGroup;
                        _activedata.exstring = _acteveobject[i].exString;
                        _activedatalist.Add(_activedata);
                    }
                }
            }
        }
#endif
        private void SetdataActive(string name, bool isshow)
        {
            if (_activedatalist.Count > 0)
            {
                for (int i = 0; i < _activedatalist.Count; i++)
                {
                    if (_activedatalist[i].exstring.Equals(name))
                    {
                        if (_activedatalist[i]._targetgroup != null)
                            _activedatalist[i]._targetgroup.gameObject.SetActive(isshow);
                        break;
                    }
                }
            }
        }

        private void Update()
        {
            if (SceneDynamicObjectSystem.Showhidelist.Count > 0)
            {
                _tempkeys.Clear();
                foreach (var item in SceneDynamicObjectSystem.Showhidelist)
                {
                    SetdataActive(item.Key, item.Value);
                    _tempkeys.Add(item.Key);
                }
                for (int i = 0; i < _tempkeys.Count; i++)
                {
                    SceneDynamicObjectSystem.Showhidelist.Remove(_tempkeys[i]);
                }
                _tempkeys.Clear();
            }
        }
    }
}
