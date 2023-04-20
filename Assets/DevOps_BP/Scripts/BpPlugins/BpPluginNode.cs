using CFClient.Utility;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.CFEventSystems;

#region  Test To Client

public class BpPluginNode : MonoBehaviour
{
    public static BpPluginNode instance;
    private void Awake()
    {
        instance = this;
    }

    public static GameObject LoadGameObject(string path)
    {
        UnityEngine.Object obj = Resources.Load(path, typeof(GameObject));
        GameObject actor = UnityEngine.Object.Instantiate(obj) as GameObject;
        return actor;
    }

    public static GameObject LoadGameObject(string path, string name)
    {
        UnityEngine.Object obj = Resources.Load(path, typeof(GameObject));
        GameObject actor = UnityEngine.Object.Instantiate(obj) as GameObject;
        actor.name = name;
        return actor;
    }

    public static GameObject LoadGameObject(string path, Transform parent)
    {
        UnityEngine.Object obj = Resources.Load(path, typeof(GameObject));
        GameObject actor = UnityEngine.Object.Instantiate(obj, parent) as GameObject;
        return actor;
    }

    public static GameObject LoadGameObject(string path, Transform parent, string name)
    {
        UnityEngine.Object obj = Resources.Load(path, typeof(GameObject));
        GameObject actor = UnityEngine.Object.Instantiate(obj, parent) as GameObject;
        actor.name = name;
        return actor;
    }

    public static Sprite LoadSprite(string path)
    {
        UnityEngine.Sprite image = Resources.Load<Sprite>("Image/" + path);
        return image;
    }

    public static RuntimeAnimatorController LoadAnimator(string path)
    {
        UnityEngine.RuntimeAnimatorController anima = Resources.Load<RuntimeAnimatorController>("Animator/" + path);
        return anima;
    }

    public static Vector2 Vector2Multiply(float num, Vector2 V2)
    {
        return num * V2;
    }

    public static string[] StringSplite(string s)
    {
        string[] sl = s.Split('_');
        return sl;
    }

    public static string[] StringSplitecomma(string s)
    {
        string[] sl = s.Split(',');
        return sl;
    }
    public static Vector3 V2ToV3(Vector2 vector2)
    {
        return vector2;
    }

    public static Vector2 V3ToV2(Vector3 vector3)
    {
        return vector3;
    }

    public static string StringAdd(string a, string b)
    {
        return a + b;
    }


    public void FindGameobjLooper(string name, int outTime, Action failed, Action<GameObject> action = null)
    {
        StartCoroutine(_FindGameobjLooper(name, outTime, failed, action));
    }

    IEnumerator _FindGameobjLooper(string name, int outTime, Action failed, Action<GameObject> action = null)
    {
        Debug.Log("AutoTest:Finding GameObject：" + name);
        GameObject obj = GameObject.Find(name);
        int timer = 0;
        while (obj == null && timer < outTime)
        {
            timer++;
            yield return new WaitForSecondsRealtime(1);
            obj = GameObject.Find(name);
            Debug.Log("AutoTest:Finding GameObject：" + name);
        }
        if (obj != null) action?.Invoke(obj);
        else failed?.Invoke();
    }

    public void Looper(int outTime, Func<bool> checkFunc, Action successed, Action failed)
    {
        StartCoroutine(_Looper(outTime, checkFunc, successed, failed));
    }


    public IEnumerator _Looper(int outTime, Func<bool> checkFunc, Action successed, Action failed)
    {
        int timer = 0;
        bool check = !checkFunc.Invoke();
        while (check && timer < outTime)
        {
            timer++;

            yield return new WaitForSecondsRealtime(1);
            check = !checkFunc.Invoke();
        }

        if (check)
        {
            successed?.Invoke();

        }
        else
        {
            failed?.Invoke();
        }

    }

    public static void CallGM(string cmd)
    {
        AutoTestingInterface.InputGM(cmd);
    }

    public static void UIClick(string uiPath)
    {
        AutoTestingInterface.UIClick(uiPath);
    }

    public static Action<string, List<string>> BPReCall;
    public static void Register()
    {
        AutoTestingInterface.OnBpAction = BPReCall;
    }

    public static bool IsSceneReady()
    {

        return AutoTestingInterface.IsSceneReady();
    }

    public static void WalkToPos(float x, float y, float z)
    {
        AutoTestingInterface.WalkToPos(x, y, z);
    }

    public static void PlayerRotation(float x, float y)
    {
        AutoTestingInterface.PlayerRotation(x, y);
    }

}
#endregion