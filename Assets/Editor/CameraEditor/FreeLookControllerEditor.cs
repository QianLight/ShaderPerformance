using CFClient;
using System;
using System.Linq;
using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using TableUtils;
using UnityEditor;
using CFUtilPoolLib;
using UnityEngine;
[CustomEditor(typeof(FreeLookController))]
public class FreeLookControllerEditor : Editor
{
    FreeLookController fc;
    TableData presentationTable;

    // 自动计算相机参数
    static readonly GUIContent autoParamLabel = new GUIContent("自动计算中轨参数", "根据角色身高、俯角和人物占高比计算中轨参数");
    static readonly GUIContent autoParamLabelSingle = new GUIContent("自动计算中轨参数（当前角色）", "刷新当前角色的中轨参数");
    static readonly GUIContent autoParamLabelBatch = new GUIContent("自动计算中轨参数（所有角色）", "刷新所有角色的中轨参数，不要随便点！");
    static readonly GUIContent isApplyBiasRatioLabel = new GUIContent("是否应用看向点偏移相对高度比");
    private bool autoParamExpand = false;
    private bool autoParamExpandSingle = false;
    private bool autoParamExpandBatch = false;
    [SerializeField] float pitchSingle = 15f;
    [SerializeField] float heightRatioSingle = 0.33f;
    [SerializeField] float pitchBatch = 15f;
    [SerializeField] float heightRatioBatch = 0.33f;

    [SerializeField] float lookAtBias;
    [SerializeField] float lookAtBiasRatio = 0.66f;
    [SerializeField] bool isApplyBiasRatio = false;
    float roleHeight = -1f;


    // 导出配置文本
    static readonly GUIContent configLabel = new GUIContent("导出PartnerCamera的CameraParam配置", "导出PartnerCamera的CameraParam配置");
    private bool configExpand = false;

    private void OnEnable()
    {
        fc = target as FreeLookController;
        ReadTable (Application.dataPath + "/Table/PartnerCamera.txt");
        if (presentationTable == null)
            presentationTable = TableManager.ReadTable (Application.dataPath + "/Table/XEntityPresentation.txt");
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        DrawTableActionEditor();
        DrawAutoParmEditor();
    }

    private void GetRect(out Rect rect, out Rect rectLabel, bool requireLabelWidth = true)
    {
        rect = EditorGUILayout.GetControlRect(true);
        rectLabel = new Rect(rect.x, rect.y, EditorGUIUtility.labelWidth, rect.height);
        rect.width -= rectLabel.width;
        rect.x += rectLabel.width;
    }

    private void DivideRect (out Rect rect, float divisor)
    {
        rect = EditorGUILayout.GetControlRect(true);
        rect.width /= divisor; 
    }

    void DrawAutoParmEditor ()
    {
        Rect rect;

        rect = EditorGUILayout.GetControlRect(true);
        var foldoutLabelWidth = GUI.skin.label.CalcSize(autoParamLabel).x;

        autoParamExpand = EditorGUI.Foldout(
            new Rect(rect.x, rect.y, foldoutLabelWidth, rect.height),
            autoParamExpand,
            autoParamLabel,
            true);

        if (autoParamExpand)
        {
            DrawAutoParamSubEditor(ref autoParamExpandSingle, autoParamLabelSingle, true);
            DrawAutoParamSubEditor(ref autoParamExpandBatch, autoParamLabelBatch, false);
        }
    }

    void DrawAutoParamSubEditor (ref bool expand, GUIContent label, bool isSingle)
    {
        EditorGUI.indentLevel++;

        Rect rect, rectLabel;

        rect = EditorGUILayout.GetControlRect(true);
        var foldoutLabelWidth = GUI.skin.label.CalcSize(autoParamLabel).x;

        expand = EditorGUI.Foldout(
            new Rect(rect.x, rect.y, foldoutLabelWidth, rect.height),
            expand,
            label,
            true);

        if (expand)
        {
            // Present pitch
            GetRect(out rect, out rectLabel);
            GUI.Label(rectLabel, "俯角");
            if (isSingle)
                pitchSingle = EditorGUI.FloatField(rect, pitchSingle);
            else
                pitchBatch = EditorGUI.FloatField(rect, pitchBatch);

            // Present height ratio
            GetRect(out rect, out rectLabel);
            GUI.Label(rectLabel, "人物占高比");
            if (isSingle)
                heightRatioSingle = EditorGUI.FloatField(rect, heightRatioSingle);
            else
                heightRatioBatch = EditorGUI.FloatField(rect, heightRatioBatch);

            // Present parms only for Single
            if (isSingle && fc.LookAt != null)
            {
                // Present LookAtBias
                if (fc.LookAt != null)
                    lookAtBias = fc.LookAt.localPosition.y;
                GetRect(out rect, out rectLabel);
                GUI.Label(rectLabel, "看向点偏移");
                lookAtBias = EditorGUI.FloatField(rect, lookAtBias);
                if (fc.LookAt != null)
                    fc.LookAt.localPosition = new Vector3(fc.LookAt.localPosition.x, lookAtBias, fc.LookAt.localPosition.z);

                // Present role height
                GetRect(out rect, out rectLabel);
                GUI.Label(rectLabel, "角色高度");
                GUI.Label(rect, roleHeight.ToString());

                // Present camera distance to the LookAt point
                float midDistance = CalculateCameraDistance(roleHeight, pitchSingle, heightRatioSingle);
                GetRect(out rect, out rectLabel);
                GUI.Label(rectLabel, "相机到看向点距离");
                GUI.Label(rect, midDistance.ToString());

                // Present the mid rig radius
                float midRadius = CalculateRigRadius(midDistance, pitchSingle);
                GetRect(out rect, out rectLabel);
                GUI.Label(rectLabel, "相机中轨半径");
                GUI.Label(rect, midRadius.ToString());

                // Present the mid rig height
                float midHeight = CalculateRigHeight(midRadius, pitchSingle, lookAtBias);
                GetRect(out rect, out rectLabel);
                GUI.Label(rectLabel, "相机中轨高度");
                GUI.Label(rect, midHeight.ToString());

                // Read role height
                GetRect(out rect, out rectLabel);
                GUI.Label(rectLabel, "读取角色高度");
                if (GUI.Button(rect, "读取角色高度", "button"))
                {
                    uint ID = XCombatPlayer.Player.Attributes.TemplateID;
                    roleHeight = GetRoleHeight (ID);
                }

                // Apply the params
                GetRect(out rect, out rectLabel);
                GUI.Label(rectLabel, "应用中轨参数");
                if (GUI.Button(rect, "应用中轨参数", "button"))
                {
                    fc.FreeLook.m_Orbits[1].m_Height = midHeight * fc.CameraHeight;
                    fc.FreeLook.m_Orbits[1].m_Radius = midRadius * fc.CameraDistance;
                }
            } 
            else if (!isSingle && fc.LookAt != null)
            {
                rect = EditorGUILayout.GetControlRect (false);
                isApplyBiasRatio = EditorGUI.ToggleLeft (
                    rect,
                    isApplyBiasRatioLabel,
                    isApplyBiasRatio
                );

                if (isApplyBiasRatio)
                {
                    // Present the role height ratio
                    GetRect(out rect, out rectLabel);
                    GUI.Label(rectLabel, "看向点偏移相对高度比");
                    lookAtBiasRatio = EditorGUI.FloatField (rect, lookAtBiasRatio);
                    lookAtBiasRatio = Mathf.Clamp01 (lookAtBiasRatio);
                }
            }

            // Set the params
            GetRect(out rect, out rectLabel);
            GUI.Label(rectLabel, "刷新中轨参数");
            if (GUI.Button(rect, "刷新! 请确认后再点击", "Button"))
            {
                if (isSingle)
                {
                    SetParam ();
                    SaveParam (Application.dataPath + "/Table/PartnerCamera.txt");
                }
                else
                {
                    if (fc == null)
                    {
                        XDebug.singleton.AddErrorLog("FreeLookController组件为空");
                        return; 
                    }

                    int IDCol = (fc.tableClass as TableData).titleIndex["ID"];
                    for (int i = 0; i < (fc.tableClass as TableData).dataStoreByRow.Count; ++i)
                    {
                        uint ID = uint.Parse ((fc.tableClass as TableData).dataStoreByRow[i][IDCol]);
                        float currentRoleHeight = GetRoleHeight (ID);
                        if (currentRoleHeight != -1)
                        {
                            string paramString = (fc.tableClass as TableData).GetconfigByKey("ID", ID.ToString(), "CameraParam");
                            if (!String.IsNullOrEmpty (paramString))
                            {
                                float[] currentCameraParams = GetCameraParamFromString(paramString, '|');
                                
                                if (isApplyBiasRatio && !Mathf.Approximately (0f, lookAtBiasRatio))
                                    currentCameraParams[0] = currentRoleHeight * lookAtBiasRatio;

                                ApplyParamToCam(currentCameraParams);
                                float midDistance = CalculateCameraDistance(currentRoleHeight, pitchBatch, heightRatioBatch);
                                float midRadius = CalculateRigRadius(midDistance, pitchBatch);
                                float midHeight = CalculateRigHeight(midRadius, pitchBatch, currentCameraParams[0]);

                                fc.FreeLook.m_Orbits[1].m_Height = midHeight * fc.CameraHeight;
                                fc.FreeLook.m_Orbits[1].m_Radius = midRadius * fc.CameraDistance;
                                
                                SetParam (ID);
                            }
                        }
                    }
                    SaveParam (Application.dataPath + "/Table/PartnerCamera.txt");
                }
            }
        }

        EditorGUI.indentLevel--;
    }

    void DrawTableActionEditor()
    {
        Rect rect, rectLabel;

        rect = EditorGUILayout.GetControlRect(true);

        var lensLabelWidth = GUI.skin.label.CalcSize(configLabel).x;
        var foldoutLabelWidth = lensLabelWidth;

        configExpand = EditorGUI.Foldout(
            new Rect(rect.x, rect.y, foldoutLabelWidth, rect.height),
            configExpand, configLabel, true);

        if (configExpand) 
        {                    
            GetRect(out rect, out rectLabel);
            GUI.Label(rectLabel, "说明");
            if (GUI.Button(rect, "打印在log里了", "Button"))
            {
                XDebug.singleton.AddLog("1.使用前把现在的配置文本复制到文本对比工具里");
                XDebug.singleton.AddLog("2.使用时先点击 \"读取配置文件\" 按钮");
                XDebug.singleton.AddLog("3.更改需要改的参数");
                XDebug.singleton.AddLog("4.点击 \"设置当前角色的参数\" 按钮");
                XDebug.singleton.AddLog("5.全部更改完毕后点击 \"更改了不要忘了保存\" 按钮");
            }      
            if (fc.tableClass is TableData)
            {
                #region 查询模块
                /*
                GetRect(out rect, out rectLabel);
                GUI.Label(rectLabel, "查询");
                if (GUI.Button(rect, "查询", "Button"))
                {
                    string str = (fc.tableClass as TableData).GetconfigByKey("ShipRefuseWorkWords", "别想指使我。", "Name");
                    XDebug.singleton.AddErrorLog(str);
                }
                */
                #endregion
                GetRect(out rect, out rectLabel);
                GUI.Label(rectLabel, "设置当前角色的参数");
                if (GUI.Button(rect, "设置当前角色的参数", "Button"))
                    SetParam ();

                GetRect(out rect, out rectLabel);
                GUI.Label(rectLabel, "保存配置文件");
                if (GUI.Button(rect, "更改了不要忘了保存", "Button"))
                    SaveParam (Application.dataPath + "/Table/PartnerCamera.txt");
            }
            else
            {
                GetRect(out rect, out rectLabel);
                GUI.Label(rectLabel, "读取配置文件");
                if (GUI.Button(rect, "读取配置文件", "Button"))
                    ReadTable (Application.dataPath + "/Table/PartnerCamera.txt");
            }
        }
    }

    private float GetRoleHeight (uint ID = 0)
    {
        if (ID == 0)
        {
            XDebug.singleton.AddErrorLog ("ID错误！");
        }
        string presentID = (fc.tableClass as TableData).GetconfigByKey ("ID", ID.ToString(), "PresentId");
        string roleHeightString = presentationTable.GetconfigByKey ("PresentID", presentID, "BoundHeight");
        if (String.IsNullOrEmpty (roleHeightString))
        {
            XDebug.singleton.AddErrorLog ($"角色{ID}/Presentation表的角色{presentID}的角色高度错误！");
            return -1f;
        }
        return float.Parse (roleHeightString);
    }

    private float GetRoleHeightByRow (int row, int column, uint ID)
    {
        string roleHeightString = (fc.tableClass as TableData).dataStoreByRow[row][column];
        if (String.IsNullOrEmpty (roleHeightString))
        {
            XDebug.singleton.AddErrorLog ($"角色{ID.ToString()}的角色高度没有填写！");
            return -1f;
        }
        return float.Parse (roleHeightString);
    }

    private void ApplyParamToCam (float[] param)
    {
        if (fc.LookAt != null)
            fc.LookAt.localPosition = new Vector3(fc.LookAt.localPosition.x, param[0], fc.LookAt.localPosition.z);
        for (int i = 0; i < 3; ++i)
        {
            fc.FreeLook.m_Orbits[i].Scale = 1;
            fc.FreeLook.m_Orbits[i].m_Height = param[i + 1] * fc.CameraHeight;
            fc.FreeLook.m_Orbits[i].m_Radius = param[i + 4] * fc.CameraDistance;
            fc.FreeLook.GetRig(i).GetCinemachineComponent<CinemachineComposer>().ScreenY = param[i + 7] + fc.screenYOffset;
        }
    }

    private void SetParam (uint ID = 0)
    {
        string setValue = GetCompactGameraParam();
        if (setValue == null)
        {
            XDebug.singleton.AddErrorLog("组合参数失败");
            return;
        }
        XDebug.singleton.AddLog(setValue);
        
        if (ID == 0)
            ID = XCombatPlayer.Player.Attributes.TemplateID;
        bool setResult = (fc.tableClass as TableData).SetconfigByKey("ID", ID.ToString(), "CameraParam", setValue);
        XDebug.singleton.AddLog(setResult ? "设置成功" : "设置失败");
    }

    private void SaveParam (string path)
    {
        if (fc != null)
            TableManager.WriteTable(path, fc.tableClass as TableData);
        else
        {
            XDebug.singleton.AddErrorLog("FreeLookController组件为空");
            return;
        }
    }

    private void ReadTable (string path)
    {
        if (fc != null)
            fc.tableClass = TableManager.ReadTable(path);
        else
        {
            XDebug.singleton.AddErrorLog("FreeLookController组件为空");
            return;
        }
    }

    private float[] GetCameraParam ()
    {
        if (fc == null)
        {
            XDebug.singleton.AddErrorLog("FreeLookController组件为空");
            return null; 
        }
        return fc.DecodeParam(fc.GetCurrentParam());
    }

    private float[] GetCameraParamFromString (string s, char separator)
    {
        string[] subs = s.Split (separator);
        float[] param = subs.Select (x => float.Parse(x)).ToArray<float>();
        return param;
    }

    private string GetCompactGameraParam()
    {
        float[] param = GetCameraParam();
        return TableManager.CompactParam(new string[] 
            { 
                fc.LookAt.localPosition.y.ToString(),
                TableManager.CompactParam(param, "|") 
            }, "|");            
    }

    private float CalculateCameraDistance (float roleHeight, float pitch, float heightRatio = 0.33f)
    {
        float fov = fc.FreeLook.m_Lens.FieldOfView;
        return roleHeight / (2 * heightRatio) * Mathf.Cos(Mathf.Deg2Rad * pitch) / Mathf.Tan(Mathf.Deg2Rad * fov / 2);
    }

    private float CalculateRigRadius (float distance, float pitch)
    {
        return Mathf.Cos(Mathf.Deg2Rad * pitch) * distance;
    }

    private float CalculateRigHeight (float radius, float pitch, float lookAtBias)
    {
        return Mathf.Tan(Mathf.Deg2Rad * pitch) * radius + lookAtBias;
    }
}
