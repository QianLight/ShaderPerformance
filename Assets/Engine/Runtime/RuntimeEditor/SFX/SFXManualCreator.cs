using System;
using System.Collections;
using System.Collections.Generic;
using CFEngine;
using UnityEditor;
using UnityEngine;

public class SFXManualCreator : MonoBehaviour
{
    public string name;

    public int performanceLevel;
    private int _performanceLevel;

    private bool _isActive;

    private SFX _target;
    // Update is called once per frame
    void Update()
    {
        if (_performanceLevel != performanceLevel)
        {
            _performanceLevel = performanceLevel;
            SFXMgr.performanceLevel = _performanceLevel;
        }
    }

    public void Play()
    {
        _target = SFXMgr.singleton.Create(name, 0, default, null, false, SFXMgr.fxOwner.CombatPlayer);
    }

    public void SetActive()
    {
        _isActive = !_isActive;
        _target.SetActive(_isActive);
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(SFXManualCreator))]
public class SFXManualCreatorEditor : Editor
{
    private SFXManualCreator _target;

    private void OnEnable()
    {
        _target = target as SFXManualCreator;
    }

    public override void OnInspectorGUI()
    {
        // base.OnInspectorGUI();
        if (GUILayout.Button("Play"))
        {
            _target.Play();
        }

        if (GUILayout.Button("ChangeActive"))
        {
            _target.SetActive();
        }

        _target.name = EditorGUILayout.TextField("技能名", _target.name);
        _target.performanceLevel = EditorGUILayout.IntField("等级", _target.performanceLevel);
    }
}
#endif