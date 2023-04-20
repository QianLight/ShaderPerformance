using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SmartFootAO : MonoBehaviour
{
    public GameObject LeftFootShadow, RightFootShadow;
    public void ShowFootShadow(bool show = false)
    {
        if (LeftFootShadow != null && RightFootShadow != null)
        {
            LeftFootShadow.SetActive(show);
            RightFootShadow.SetActive(show);
        }
    }

#if UNITY_EDITOR
    private void OnEnable()
    {
        gameObject.transform.position = Vector3.zero;
        gameObject.transform.rotation = Quaternion.identity;
    }
    public string[] PartName = new string[] { "Bip001 L Toe0", "Bip001 R Toe0" };
    public string[] RenderName = new string[] { "_shoes_normal", "_cloth" };
    public SkinnedMeshRenderer LeftFootRender, RightFootRender;
    public Texture3D Left3D, Right3D;

    public float SlicePPM = 0.01f;
    public float FootHeight = 0.01f;
    public float FootAdjust = 0.01f;
    [Range(0, 4)]
    public float AOScale = 2.5f;
    [Range(0, 1)]
    public float AOMaxStength = 0.5f;
    public Vector3 AddScale = new Vector3(1.2f, 1.8f, 1.1f);
    public bool UseLastPrefab = true;
    public bool RightUseLeft = true;
    public bool ShowDebug = false;
    public bool AutoGetRender = true;
    public Bounds8Point LeftBound, LeftMaxBound, RightBound, RightMaxBound;

    public Bounds8Point LocalBounds;
    public List<Vector3> DebugList = new List<Vector3>();

    private void OnDrawGizmos()
    {
        if (!ShowDebug)
            return;
        if (LeftFootRender != null)
        {
            Gizmos.color = Color.red;
            Bounds8Point.DrawGizmos(LocalBounds);
            //Gizmos.color = Color.black;
            //Gizmos.DrawWireCube(LeftFootRender.bounds.center, LeftFootRender.bounds.size);
            Gizmos.color = Color.green;
            Bounds8Point.DrawGizmos(LeftMaxBound);
            Gizmos.color = Color.blue;
            Bounds8Point.DrawGizmos(LeftBound);

            Gizmos.color = Color.green;
            Bounds8Point.DrawGizmos(RightMaxBound);
            Gizmos.color = Color.blue;
            Bounds8Point.DrawGizmos(RightBound);

            if (DebugList != null && DebugList.Count > 0)
            {
                foreach (Vector3 v in DebugList)
                {
                    Gizmos.DrawSphere(v, 0.01f);
                }
            }
        }
    }
    public const string SliceShaderShader = "Assets/Standard Assets/SmartAO/Shader/SliceShader.mat";
    public const string FootAOBoxMeshPath = "Assets/Engine/Runtime/Shaders/SmartAO/FootAOBoxMesh.asset";
    public const string FootAOPlaneMeshPath = "Assets/Engine/Runtime/Shaders/SmartAO/FootAOPlaneMesh.asset";
    public const string FootAOShader = "Assets/Engine/Runtime/Shaders/SmartAO/SmartAO.shader";
    public const string FootAoObjName = "FootAO";
    public const string SmartAOPath = "Assets/BundleRes/SmartAO/";
    public static string LastLeftPrefabPath = null;

    public string SmartAbsPath = "Assets/BundleRes/SmartAO/luffy/";
    public string PrefabPathLeft = "FootAoLeft.prefab";
    public string PrefabPathRight = "FootAoRight.prefab";
    public string FootAoMatLeft = "My3DTextureLeft.mat";
    public string FootAoMatRight = "My3DTextureRight.mat";
    public string FootAoTexLeft = "My3DTextureLeft.asset";
    public string FootAoTexRight = "My3DTextureRight.asset";

    public void CreateSmartAO()
    {
        string partName0 = PartName[0];
        string partName1 = PartName[1];
        Transform root = transform;
        Transform part0 = SmartFootAOEditorHelp.FindObj(root, partName0);
        Transform part1 = SmartFootAOEditorHelp.FindObj(root, partName1);
        if (part0 == null)
        {
            Debug.LogError("PartName \"" + partName0 + "\" can't find!");
        }
        if (part1 == null)
        {
            Debug.LogError("PartName \"" + partName1 + "\" can't find!");
        }
        Vector3 mid = root.InverseTransformPoint((part0.position + part1.position) / 2.0f);

        Bounds bound0 = new Bounds(), bound1 = new Bounds();

        if (AutoGetRender)
        {
            SkinnedMeshRenderer render = GetComponent<SkinnedMeshRenderer>();
            List<SkinnedMeshRenderer> rList = new List<SkinnedMeshRenderer>();
            rList.Add(render);
            rList.AddRange(GetComponentsInChildren<SkinnedMeshRenderer>());
            //for (int i = 0; i < rList.Count; i++)
            //{
            //    byte boundCheck = 0;
            //    SkinnedMeshRenderer tmpRender = rList[i];
            //    if (tmpRender != null)
            //    {
            //        Bounds tmpLocalBound = tmpRender.localBounds;
            //        Bounds tmpBound = tmpRender.bounds;
            //        if (tmpBound.Contains(part0.position))
            //        {
            //            bound0 = tmpLocalBound;
            //            LeftFootRender = tmpRender;
            //            boundCheck++;
            //        }
            //        if (tmpBound.Contains(part1.position))
            //        {
            //            bound1 = tmpLocalBound;
            //            RightFootRender = tmpRender;
            //            boundCheck++;
            //        }
            //    }
            //    if (boundCheck > 1)
            //    {
            //        break;
            //    }
            //}
            bool fined = false;
            for (int j = 0; j < RenderName.Length; j++)
            {
                for (int i = 0; i < rList.Count; i++)
                {
                    SkinnedMeshRenderer tmpRender = rList[i];
                    if (tmpRender != null)
                    {
                        bool nameCheck = tmpRender.name.Contains(RenderName[j]);
                        Bounds tmpLocalBound = tmpRender.localBounds;
                        if (nameCheck)
                        {
                            bound0 = tmpLocalBound;
                            LeftFootRender = tmpRender;

                            bound1 = tmpLocalBound;
                            RightFootRender = tmpRender;
                            fined = true;
                        }
                    }
                    if (fined)
                    {
                        break;
                    }
                }
                if (fined)
                {
                    break;
                }
            }
        }
        else
        {

        }

        if (LeftFootRender == null)
        {
            Debug.LogError("Can't find bounds contains \"" + partName0);
            return;
        }
        else
        {
            //Debug.Log("LeftFootRender:" + LeftFootRender);
        }
        if (RightFootRender == null)
        {
            Debug.LogError("Can't find bounds contains \"" + partName1);
        }
        else
        {
            //Debug.Log("RightFootRender:" + RightFootRender);
        }
        DebugList.Clear();
        float floorHeight = 0;

        if (UseLastPrefab)
        {
            if(LastLeftPrefabPath == null)
                LastLeftPrefabPath = SmartAbsPath + PrefabPathLeft;
            GameObject lastPrefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(LastLeftPrefabPath);
            if(lastPrefab == null)
            {
                //---------------------和下面一样-----------
                SmartFootAOEditorCamera.SetRenderVision(root, LeftFootRender, SliceShaderShader, true);
                LocalBounds = SmartFootAOEditorHelp.InitFromSkin(LeftFootRender);
                floorHeight = LeftFootRender.transform.position.y + FootHeight * LeftFootRender.transform.lossyScale.y;
                LeftMaxBound = SmartFootAOEditorHelp.GetFootMaxBounds(LeftFootRender.transform, LocalBounds, mid, floorHeight, true);
                LeftBound = SmartFootAOEditorHelp.GetSliceBounds8(LeftFootRender, LeftMaxBound, SlicePPM, DebugList);
                SmartFootAOEditorCamera.CreateCamera("LightShadowCamera", LeftBound);

                Left3D = SmartFootAOEditorCamera.Create3DTexture(SmartAbsPath + FootAoTexLeft, LeftBound, SlicePPM);
                LeftFootShadow = SmartFootAOEditorCamera.CreateGameObject(FootAoObjName, SmartAbsPath, FootAOBoxMeshPath, FootAOShader, part0, LeftBound, Left3D, FootAdjust, AddScale);
                SmartFootAOEditorCamera.ClearRenderVision(root, LeftFootRender);

                UnityEditor.PrefabUtility.SaveAsPrefabAsset(LeftFootShadow, LastLeftPrefabPath);
                UnityEditor.AssetDatabase.Refresh();
                if (RightUseLeft)
                {
                    LocalBounds = SmartFootAOEditorHelp.InitFromSkin(RightFootRender);
                    floorHeight = RightFootRender.transform.position.y + FootHeight * RightFootRender.transform.lossyScale.y;
                    RightMaxBound = SmartFootAOEditorHelp.GetFootMaxBounds(RightFootRender.transform, LocalBounds, mid, floorHeight, false);
                    RightBound = SmartFootAOEditorHelp.GetSliceBounds8(RightFootRender, RightMaxBound, SlicePPM, DebugList);
                    Right3D = Left3D;
                    RightFootShadow = SmartFootAOEditorCamera.CopyGameObject(FootAoObjName, LeftFootShadow, part1, RightBound, FootAdjust, AddScale);
                }
                else
                {
                    SmartFootAOEditorCamera.SetRenderVision(root, RightFootRender, SliceShaderShader, false);
                    LocalBounds = SmartFootAOEditorHelp.InitFromSkin(RightFootRender);
                    floorHeight = RightFootRender.transform.position.y + FootHeight * RightFootRender.transform.lossyScale.y;
                    RightMaxBound = SmartFootAOEditorHelp.GetFootMaxBounds(RightFootRender.transform, LocalBounds, mid, floorHeight, false);
                    RightBound = SmartFootAOEditorHelp.GetSliceBounds8(RightFootRender, RightMaxBound, SlicePPM, DebugList);
                    SmartFootAOEditorCamera.CreateCamera("LightShadowCamera", RightBound);
                    Right3D = SmartFootAOEditorCamera.Create3DTexture(SmartAbsPath + FootAoTexRight, RightBound, SlicePPM);
                    RightFootShadow = SmartFootAOEditorCamera.CreateGameObject(FootAoObjName, SmartAbsPath, FootAOBoxMeshPath, FootAOShader, part1, RightBound, Right3D, FootAdjust, AddScale);
                    SmartFootAOEditorCamera.ClearRenderVision(root, RightFootRender);
                }
                SmartFootAOEditorCamera.ClearCamera();
            }
            else
            {
                LocalBounds = SmartFootAOEditorHelp.InitFromSkin(LeftFootRender);
                floorHeight = LeftFootRender.transform.position.y + FootHeight * LeftFootRender.transform.lossyScale.y;
                LeftMaxBound = SmartFootAOEditorHelp.GetFootMaxBounds(LeftFootRender.transform, LocalBounds, mid, floorHeight, true);
                LeftBound = SmartFootAOEditorHelp.GetSliceBounds8(LeftFootRender, LeftMaxBound, SlicePPM, DebugList);
                LeftFootShadow = SmartFootAOEditorCamera.CopyGameObject(FootAoObjName, lastPrefab, part0, LeftBound, FootAdjust, AddScale);

                LocalBounds = SmartFootAOEditorHelp.InitFromSkin(RightFootRender);
                floorHeight = RightFootRender.transform.position.y + FootHeight * RightFootRender.transform.lossyScale.y;
                RightMaxBound = SmartFootAOEditorHelp.GetFootMaxBounds(RightFootRender.transform, LocalBounds, mid, floorHeight, false);
                RightBound = SmartFootAOEditorHelp.GetSliceBounds8(RightFootRender, RightMaxBound, SlicePPM, DebugList);
                RightFootShadow = SmartFootAOEditorCamera.CopyGameObject(FootAoObjName, lastPrefab, part1, RightBound, FootAdjust, AddScale);
            }
        }
        else
        {
            SmartFootAOEditorCamera.SetRenderVision(root, LeftFootRender, SliceShaderShader, true);
            LocalBounds = SmartFootAOEditorHelp.InitFromSkin(LeftFootRender);
            floorHeight = LeftFootRender.transform.position.y + FootHeight * LeftFootRender.transform.lossyScale.y;
            LeftMaxBound = SmartFootAOEditorHelp.GetFootMaxBounds(LeftFootRender.transform, LocalBounds, mid, floorHeight, true);
            LeftBound = SmartFootAOEditorHelp.GetSliceBounds8(LeftFootRender, LeftMaxBound, SlicePPM, DebugList);
            SmartFootAOEditorCamera.CreateCamera("LightShadowCamera", LeftBound);

            Left3D = SmartFootAOEditorCamera.Create3DTexture(SmartAbsPath + FootAoTexLeft, LeftBound, SlicePPM);
            LeftFootShadow = SmartFootAOEditorCamera.CreateGameObject(FootAoObjName, SmartAbsPath, FootAOBoxMeshPath, FootAOShader, part0, LeftBound, Left3D, FootAdjust, AddScale);
            SmartFootAOEditorCamera.ClearRenderVision(root, LeftFootRender);

            if (RightUseLeft)
            {
                LocalBounds = SmartFootAOEditorHelp.InitFromSkin(RightFootRender);
                floorHeight = RightFootRender.transform.position.y + FootHeight * RightFootRender.transform.lossyScale.y;
                RightMaxBound = SmartFootAOEditorHelp.GetFootMaxBounds(RightFootRender.transform, LocalBounds, mid, floorHeight, false);
                RightBound = SmartFootAOEditorHelp.GetSliceBounds8(RightFootRender, RightMaxBound, SlicePPM, DebugList);
                Right3D = Left3D;
                RightFootShadow = SmartFootAOEditorCamera.CopyGameObject(FootAoObjName, LeftFootShadow, part1, RightBound, FootAdjust, AddScale);
            }
            else
            {
                SmartFootAOEditorCamera.SetRenderVision(root, RightFootRender, SliceShaderShader, false);
                LocalBounds = SmartFootAOEditorHelp.InitFromSkin(RightFootRender);
                floorHeight = RightFootRender.transform.position.y + FootHeight * RightFootRender.transform.lossyScale.y;
                RightMaxBound = SmartFootAOEditorHelp.GetFootMaxBounds(RightFootRender.transform, LocalBounds, mid, floorHeight, false);
                RightBound = SmartFootAOEditorHelp.GetSliceBounds8(RightFootRender, RightMaxBound, SlicePPM, DebugList);
                SmartFootAOEditorCamera.CreateCamera("LightShadowCamera", RightBound);
                Right3D = SmartFootAOEditorCamera.Create3DTexture(SmartAbsPath + FootAoTexRight, RightBound, SlicePPM);
                RightFootShadow = SmartFootAOEditorCamera.CreateGameObject(FootAoObjName, SmartAbsPath, FootAOBoxMeshPath, FootAOShader, part1, RightBound, Right3D, FootAdjust, AddScale);
                SmartFootAOEditorCamera.ClearRenderVision(root, RightFootRender);
            }
            SmartFootAOEditorCamera.ClearCamera();
        }
        if(LeftFootRender != null && LeftFootShadow != null)
        {
            SmartFootAOFollow follow = LeftFootRender.gameObject.GetComponent<SmartFootAOFollow>();
            if(follow == null)
            {
                follow = LeftFootRender.gameObject.AddComponent<SmartFootAOFollow>();
            }
            follow.LeftFootShadow = LeftFootShadow;
            follow.RightFootShadow = RightFootShadow;
        }
    }
    public void ClearLastFootAO()
    {
        LastLeftPrefabPath = null;
    }
    public void CreateSmartFootAO(string roleName)
    {
        if(string.IsNullOrEmpty(roleName))
        {
            Debug.LogError("Create Smart Foot AO:" + roleName);
            return;
        }
        SmartAbsPath = SmartAOPath + roleName + "/";
        if(!System.IO.Directory.Exists(SmartAbsPath))
        {
            System.IO.Directory.CreateDirectory(SmartAbsPath);
            UnityEditor.AssetDatabase.Refresh();
        }
        PrefabPathLeft = roleName + "_left.prefab";
        FootAoMatLeft = roleName + "_left.mat";
        FootAoMatRight = roleName + "_right.mat";
        FootAoTexLeft = roleName + "_left.asset";
        FootAoTexRight = roleName + "_right.asset";
        CreateSmartAO();
    }
#endif

}
