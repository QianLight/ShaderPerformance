/********************************************************************
	created:	2021/08/02  11:09
	file base:	SceneSignal
	author:		c a o   f e n g
	
	purpose:	多场景数据
*********************************************************************/

using System;
using System.IO;
using CFEngine;
using UnityEngine;
using UnityEngine.Timeline;
using UnityEngine.SceneManagement;
using System.Collections.Generic;


#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;

[MarkerAttribute(TrackType.MARKER)]
[CSDiscriptor("多场景")]
#endif

public class SceneSignal : DirectorSignalEmmiter
{
    public bool m_UnLoadScene;
    public string m_SceneName;
    public bool m_IsHiddenActiveScene;

    [HideInInspector] public bool m_executed = false;

    public override PropertyName id
    {
        get { return new PropertyName("SceneSignal"); }
    }
    

    public void LoadOrUnLoadScene()
    {
        if (m_executed || String.IsNullOrEmpty(m_SceneName)) return;
        m_executed = true;
        
        if (!m_UnLoadScene)
            LoadScene();
        else
            UnLoadScene();
    }

    #region load

    private void LoadScene()
    {
        AddSceneSignalData();

        _SceneSignalData = FindObjectOfType<SceneSignalData>();

        if (_SceneSignalData.LoadScene_TryCheckIsSameScene(m_SceneName)) return;

        _SceneSignalData.HiddenSceneRenders(m_IsHiddenActiveScene);

        
        //LightmapVolumn.ResetState();
        //Scene activeScene = SceneManager.GetActiveScene();
        EngineUtility.SetAddScene(true);
        ZeusAssetManager.singleton.LoadScene(m_SceneName, LoadSceneMode.Additive);
        //SceneManager.SetActiveScene(activeScene); 
        
        // 
        // ZeusAssetManager.singleton.LoadSceneAsync(m_SceneName, LoadSceneMode.Additive,
        //     (bool flag, float precent, object o) => { });
    }

    #endregion
    
    

    private SceneSignalData _SceneSignalData;
    private void AddSceneSignalData()
    {
        GameObject sceneSignalObj = GameObject.Find("SceneSignal");
        if (sceneSignalObj == null)
        {
            sceneSignalObj = new GameObject("SceneSignal");
            sceneSignalObj.AddComponent<SceneSignalData>();
        }
    }
    
    
    #region Unload
    private void UnLoadScene()
    {
        _SceneSignalData = FindObjectOfType<SceneSignalData>();
        if (_SceneSignalData.IsNull()) return;

        if (_SceneSignalData.UnLoadScene_TryCheckIsSameScene()) return;

        if (!_SceneSignalData.UnLoadScene_CheckLastSceneNameSame(m_SceneName)) return;

        EngineUtility.SetAddScene(false);
        ZeusAssetManager.singleton.UnloadSceneAsync(m_SceneName,
            (bool flag, float precent, object o) =>
            {
                
                if(flag) LightmapVolumn.ResetState(true);//add场景 避免释放的时候重新进入计算lightmap
                
            }, null);
        _SceneSignalData.ShowSceneRenders();
        
        _SceneSignalData.Clear();
    }

    #endregion
    

    public override void Load(CFBinaryReader reader)
    {
        base.Load(reader);
        m_UnLoadScene = reader.ReadBoolean();
        m_SceneName = reader.ReadString();
        m_IsHiddenActiveScene = reader.ReadBoolean();
    }



#if UNITY_EDITOR
    public override void Save(BinaryWriter bw, ref SignalSaveContext context)
    {
        base.Save(bw, ref context);
        if (!context.presave)
        {
            bw.Write(m_UnLoadScene);
            bw.Write(m_SceneName);
            bw.Write(m_IsHiddenActiveScene);
        }
    }

    public override byte GetSignalType()
    {
        return RSignal.SceneSignal_Layer;
    }
#endif
}
