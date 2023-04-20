#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using CFClient;
using CFEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using URPLighting = UnityEngine.Rendering.Universal.Lighting;

[Serializable]
public class SceneRoleData
{
    public Scene scene;
    public Transform root;
    public List<Role> roles;
}

[Serializable]
public class Role
{
    public Transform transform;
    public float viewSize;
    public RenderBinding binding;
}

public static class EditorRolePreview
{
    public static bool checkRolePath = true;

    public static List<SceneRoleData> datas = new List<SceneRoleData>();

    public const string RootName = "Role";

    private static MaterialPropertyBlock mpb;

    [InitializeOnLoadMethod]
    private static void RegisterEvents()
    {
        EditorSceneManager.sceneOpened += OnSceneOpened;
        EditorApplication.update += Update;
        EditorSceneManager.sceneClosed += OnSceneClosed;
        EditorApplication.playModeStateChanged += OnPlayModeChanged;
        UniversalRenderPipeline.setupPerCameraData += OnPrepareFrameData;
        EditorApplication.playModeStateChanged += OnPlayModeChagned;

        Initialize();
    }

    private static void OnPlayModeChagned(PlayModeStateChange stageChange)
    {
        if (stageChange == PlayModeStateChange.EnteredEditMode)
        {
            Initialize();
        }
    }

    private static void Initialize()
    {
        int sceneCount = EditorSceneManager.loadedSceneCount;
        Scene activeScene = EditorSceneManager.GetActiveScene();
        for (int i = 0; i < sceneCount; i++)
        {
            Scene scene = EditorSceneManager.GetSceneAt(i);
            OpenSceneMode mode = sceneCount == 1
                ? OpenSceneMode.Single
                : (scene == activeScene ? OpenSceneMode.Single : OpenSceneMode.Additive);
            OnSceneOpened(scene, mode);
        }
    }

    private static void OnPlayModeChanged(PlayModeStateChange stateChange)
    {
        if (stateChange == PlayModeStateChange.EnteredEditMode)
        {
            Initialize();
        }
    }

    private static void OnPrepareFrameData(ScriptableRenderContext context, Camera camera)
    {
        for (int i = 0; i < datas.Count; i++)
        {
            var data = datas[i];
            if (data == null)
                continue;

            List<Renderer> renderers = UnityEngine.Rendering.ListPool<Renderer>.Get();
            foreach (Role role in data.roles)
            {
                if (role == null || !role.transform)
                    continue;

                role.transform.GetComponentsInChildren(renderers);
                if (renderers.Count == 0)
                    continue;

                Bounds bounds = renderers[0].bounds;
                for (int rendererIndex = 1; rendererIndex < renderers.Count; rendererIndex++)
                    bounds.Encapsulate(renderers[rendererIndex].bounds);

                Vector4 param = EntityExtSystem.CalculateOutlineParam(camera, bounds);
                role.viewSize = param.x;

                mpb ??= new MaterialPropertyBlock();
                foreach (Renderer renderer in renderers)
                {
                    renderer.GetPropertyBlock(mpb);
                    mpb.SetVector(ShaderManager._OutlineParam, param);
                    renderer.SetPropertyBlock(mpb);
                }
            }

            UnityEngine.Rendering.ListPool<Renderer>.Release(renderers);
        }
    }

    private static void OnSceneClosed(Scene scene)
    {
        for (int i = 0; i < datas.Count; i++)
        {
            if (datas[i].scene == scene)
            {
                datas.RemoveAt(i);
                return;
            }
        }
    }

    private static void Update()
    {
        for (int i = 0; i < datas.Count; i++)
        {
            SceneRoleData data = datas[i];

            if (!data.scene.isLoaded)
            {
                datas.RemoveAt(i--);
                continue;
            }

            UpateRootReference(data);

            UpdateRoleReference(data);

            UpdateRootPos(data);
        }
    }

    private static void UpdateRootPos(SceneRoleData data)
    {
        mpb = mpb ?? new MaterialPropertyBlock();
        List<Renderer> renderers = UnityEngine.Rendering.ListPool<Renderer>.Get();
        foreach (Role role in data.roles)
        {
            renderers.Clear();
            role.transform.GetComponentsInChildren(renderers);
            Vector4 position = role.transform.position;
            Ray ray = new Ray(role.transform.position + Vector3.up * 1.5f, Vector3.down);
            bool hitSuccess = Physics.Raycast(ray, out var hi, 10f, LayerMask.GetMask("Default", "Terrain"));
            position.w = hitSuccess ? hi.point.y : role.transform.position.y - 10f;
            foreach (Renderer renderer in renderers)
            {
                if (!renderer.gameObject.activeInHierarchy || !renderer.enabled)
                    continue;
                mpb.Clear();
                renderer.GetPropertyBlock(mpb);
                // mpb.SetVector("_RootPosWS", position);
                renderer.SetPropertyBlock(mpb);
            }
        }

        UnityEngine.Rendering.ListPool<Renderer>.Release(renderers);
    }

    private static void UpateRootReference(SceneRoleData data)
    {
        if (!data.root)
            data.root = FindOrCreateRoleRoot(data.scene);
        if (data.root.name != RootName)
            data.root.name = RootName;
    }

    private static void UpdateRoleReference(SceneRoleData data)
    {
        #region Role/...

        // Collect exist role transforms.
        HashSet<Transform> transforms = new HashSet<Transform>();
        Transform rootTransform = data.root.transform;
        int childCount = rootTransform.childCount;
        for (int childIndex = 0; childIndex < childCount; childIndex++)
            transforms.Add(rootTransform.GetChild(childIndex));

        for (int roleIndex = 0; roleIndex < data.roles.Count; roleIndex++)
        {
            Role role = data.roles[roleIndex];

            // Destroyed.
            if (!role.transform)
            {
                data.roles.RemoveAt(roleIndex--);
                continue;
            }

            // Move out of root in edit mode, move back.
            if (!transforms.Remove(role.transform))
            {
                GameObject prefab = PrefabUtility.GetNearestPrefabInstanceRoot(role.transform);
                if (!prefab)
                {
                    role.transform.SetParent(data.root);
                    EditorSceneManager.MarkSceneDirty(data.scene);
                }
            }
        }

        // New roles.
        foreach (Transform transform in transforms)
        {
            if (IsRole(transform.gameObject))
            {
                AddRole(data, transform);
            }
        }

        #endregion

        #region UIScene/Playres/...

        UIScene[] uiScenes = UnityEngine.Object.FindObjectsOfType<UIScene>();
        foreach (UIScene scene in uiScenes)
        {
            List<Transform> uiRoles = FindUIScenePreviewRoles(scene.gameObject);
            foreach (Transform transform in uiRoles)
            {
                AddRole(data, transform);
            }
        }
        
        #endregion
    }

    private static void AddRole(SceneRoleData data, Transform transform)
    {
        Role role = new Role
        {
            transform = transform
        };
        role.binding = transform.GetComponent<RenderBinding>();
        data.roles ??= new List<Role>();
        foreach (Role r in data.roles)
        {
            if (transform == r.transform)
            {
                return;
            }
        }
        data.roles.Add(role);
    }

    private static void OnSceneOpened(Scene scene, OpenSceneMode mode)
    {
        if (!Application.isPlaying && checkRolePath)
        {
            if (string.IsNullOrEmpty(scene.path)
                || !scene.path.StartsWith("Assets/Scenes/Scenelib/"))
                return;

            SceneRoleData sceneRoleData = new SceneRoleData();
            sceneRoleData.scene = scene;
            sceneRoleData.root = FindOrCreateRoleRoot(scene);
            sceneRoleData.roles = FindRoles(sceneRoleData);
            datas.Add(sceneRoleData);

            MoveRolesToRoot(sceneRoleData);
        }
    }

    private static List<Role> FindRoles(SceneRoleData sceneRoleData)
    {
        GameObject[] roots = sceneRoleData.scene.GetRootGameObjects();
        List<Role> roles = new List<Role>();
        foreach (GameObject root in roots)
        {
            List<Animator> animators = UnityEngine.Rendering.ListPool<Animator>.Get();
            root.GetComponentsInChildren(true, animators);
            foreach (Animator animator in animators)
            {
                if (!IsRole(animator.gameObject))
                    continue;

                AddRole(sceneRoleData, animator.transform);
            }

            UnityEngine.Rendering.ListPool<Animator>.Release(animators);
        }
        
        UIScene[] uiScenes = UnityEngine.Object.FindObjectsOfType<UIScene>();
        foreach (UIScene uiScene in uiScenes)
        {
            List<Transform> previewRoles = FindUIScenePreviewRoles(uiScene.gameObject);
            foreach (Transform transform in previewRoles)
            {
                AddRole(sceneRoleData, transform);
            }
        }

        return roles;
    }

    private static bool IsRole(GameObject gameObject)
    {
        if (!gameObject)
            return false;

        bool isPrefabRoot = gameObject == PrefabUtility.GetOutermostPrefabInstanceRoot(gameObject);
        if (!isPrefabRoot)
            return false;

        string name = gameObject.name;
        if (!name.StartsWith("Role_") && !name.StartsWith("Monster_"))
            return false;

        // EditorScene下面的可能有其他用途，不做控制。
        Transform parent = gameObject.transform;
        while (parent.parent)
            parent = parent.parent;
        if (parent.name == "EditorScene")
            return false;

        return true;
    }

    private static Transform FindOrCreateRoleRoot(Scene scene)
    {
        Transform roleRoot = null;
        GameObject[] roots = scene.GetRootGameObjects();
        foreach (GameObject root in roots)
        {
            if (root.name == RootName)
            {
                roleRoot = root.transform;
            }
        }

        if (!roleRoot)
        {
            roleRoot = new GameObject(RootName).transform;
            roleRoot.SetSiblingIndex(0);
        }

        return roleRoot;
    }

    public static List<Transform> FindUIScenePreviewRoles(GameObject uiScene)
    {
        List<Transform> previewTransforms = new List<Transform>();
        Transform playersTf = uiScene.transform.Find("Players");
        if (playersTf.IsNotEmpty())
        {
            for (int i = 0; i < playersTf.childCount; i++)
            {
                Transform childTf = playersTf.GetChild(i);
                if (!childTf.name.Contains("Player")) continue;
                for (int j = childTf.childCount - 1; j >= 0; j--)
                {
                    Transform childTf1 = childTf.GetChild(j);
                    previewTransforms.Add(childTf1);
                }
            }
        }

        return previewTransforms;
    }

    private static void MoveRolesToRoot(SceneRoleData sceneRoleData)
    {
        List<Role> outOfRootRoles = new List<Role>();
        foreach (Role role in sceneRoleData.roles)
        {
            if (!role.transform.transform.IsChildOf(sceneRoleData.root)
                && role.transform.parent)
            {
                outOfRootRoles.Add(role);
            }
        }

        foreach (Role role in outOfRootRoles)
        {
            role.transform.SetParent(sceneRoleData.root, true);
            EditorSceneManager.MarkSceneDirty(sceneRoleData.scene);
        }

        if (outOfRootRoles.Count > 0)
            EditorUtility.DisplayDialog("打开场景检查", $"有{outOfRootRoles.Count}个角色不在角色根节点下，可能导致部分效果无法正确预览，已经自动迁移到Role根节点下。",
                "OK");
    }
}
#endif