using CFUtilPoolLib;
using System.IO;
using UnityEngine;
using UnityEngine.CFEventSystems;
using UnityEngine.CFUI;
using XLua;
using CFClient.UI;

[LuaCallCSharp]
public class Hotfix
{

    private static ILuaExtion m_luaExtion = null;

    public static ILuaExtion luaExtion
    {
        get
        {
            if (null == m_luaExtion || m_luaExtion.Deprecated)
                m_luaExtion = XInterfaceMgr.singleton.GetInterface<ILuaExtion>(XCommon.singleton.XHash("ILuaExtion"));
            return m_luaExtion;
        }
    } 

    public static object GetDocument(string doc)
    {
        return luaExtion.GetDocument(doc);
    }

    public static void SetDocumentMember(string doc, string key, object value, bool isPublic, bool isField)
    {
        luaExtion.SetDocumentMember(doc, key, value, isPublic, isField);
    }

    public static object GetDocumentMember(string doc, string key, bool isPublic, bool isField)
    {
        return luaExtion.GetDocumentMember(doc, key, isPublic, isField);
    }

    public static string GetGetDocumentLongMember(string doc, string key, bool isPublic, bool isField)
    {
        return GetDocumentMember(doc, key, isPublic, isField).ToString();
    }
    public static object GetDocumentStaticMember(string doc, string key, bool isPublic, bool isField)
    {
        return luaExtion.GetDocumentStaticMember(doc, key, isPublic, isField);
    }

    public static object CallDocumentMethod(string doc, bool isPublic, string method, params object[] args)
    {
        return luaExtion.CallDocumentMethod(doc, isPublic, method, args);
    }

    public static string CallDocumentLongMethod(string doc, bool isPublic, string method, params object[] args)
    {
        return luaExtion.CallDocumentMethod(doc, isPublic, method, args).ToString();
    }

    public static object CallDocumentStaticMethod(string doc, bool isPublic, string method, params object[] args)
    {
        return luaExtion.CallDocumentStaticMethod(doc, isPublic, method, args);
    }

    public static object GetSingle(string className)
    {
        return luaExtion.GetSingle(className);
    }

    public static object GetGSDKManager()
    {
        return luaExtion.GetGSDKManager();
    }

    public static object GetIRuGameAdvancedInjectionManager()
    {
        return luaExtion.GetIRuGameAdvancedInjectionManager();
    }

    public static object JsonDecode(string jsonStr)
    {
        return luaExtion.JsonDecode(jsonStr);
    }

    public static object GetSingleMember(string className, string key, bool isPublic, bool isField, bool isStatic)
    {
        return luaExtion.GetSingleMember(className, key, isPublic, isField, isStatic);
    }

    public static string GetSingleLongMember(string className, string key, bool isPublic, bool isField, bool isStatic)
    {
        return GetSingleMember(className, key, isPublic, isField, isStatic).ToString();
    }

    public static void SetSingleMember(string className, string key, object value, bool isPublic, bool isField, bool isStatic)
    {
        luaExtion.SetSingleMember(className, key, value, isPublic, isField, isStatic);
    }

    public static object CallSingleMethod(string className, bool isPublic, bool isStatic, string methodName, params object[] args)
    {
        return luaExtion.CallSingleMethod(className, isPublic, isStatic, methodName, args);
    }

    public static string CallSingleLongMethod(string className, bool isPublic, bool isStatic, string methodName, params object[] args)
    {
        return luaExtion.CallSingleMethod(className, isPublic, isStatic, methodName, args).ToString();
    }

    public static object GetEnumType(string classname, string value)
    {
        return luaExtion.GetEnumType(classname, value);
    }

    public static string GetStringTable(string key, params object[] args)
    {
        return luaExtion.GetStringTable(key, args);
    }
    public static string GetObjectString(object o, string name)
    {
        return PublicExtensions.GetPublicField(o, name).ToString();
    }

    public static string GetObjectString(object o, string name, bool isPublic, bool isField)
    {
        if (isPublic)
        {
            if (isField)
                return o.GetPublicField(name).ToString();
            else
                return PublicExtensions.GetPublicProperty(o, name).ToString();
        }
        else
        {
            if (isField)
                return o.GetPrivateField(name).ToString();
            else
                return PrivateExtensions.GetPrivateProperty(o, name).ToString();
        }
    }
    
    public static uint LuaWait(int delay, LuaFunction cb)
    {
        return XTimerMgr.singleton.SetTimer(delay, (x) => { cb.Call(x); }, 1);
    }

    public static uint LuaLoop(int delay, int loop, LuaFunction cb)
    {
        return XTimerMgr.singleton.SetTimer(delay, (x) => { cb.Call(x); }, loop);
    }

    public static void RemoveTimer(uint seq)
    {
        XTimerMgr.singleton.KillTimer(seq);
    }

    public static bool SendLuaPtc(uint type, MemoryStream stream, int len)
    {
        return LuaRoute.LuaNetwork.LuaSendPtc(type, stream, len);
    }

    public static void SendLuaRPC(uint type, MemoryStream stream, int len, LuaRpcRespond onRes)
    {
        LuaRoute.LuaNetwork.LuaSendRPC(type, stream, len, onRes);
    }


 
    
    public static AttributeTagSkin CreateAttributeTagSkin(uint prefabID, Transform trans, bool active)
    {
        IUISystem system = XUITools.GetSpecificView(prefabID);
        return system != null ? XSkin.CreateInstance<AttributeTagSkin>() : null;
    }
    public static void RecycleInstanceTagSkin( AttributeTagSkin skin)
    {
        XSkin.RecycleInstance<AttributeTagSkin>(skin);
    }

    public static OpItemSkinGroup CreateOpItemSkinGroup(uint prefabID, Transform trans, bool active)
    {
        IUISystem system = XUITools.GetSpecificView(prefabID);
        return XSkin.CreateInstance<OpItemSkinGroup>();
    }
    public static void RecycleInstanceTagSkin( OpItemSkinGroup skin)
    {
        XSkin.RecycleInstance<OpItemSkinGroup>(skin);
    }
    public static void RecycleInstanceTagSkin(OpItemSkinGroup skin, bool recycle)
    {
        XSkin.RecycleInstance<OpItemSkinGroup>(skin, recycle);
    }

    public static CFText GetText(Transform tf, string path)
    {
        return UIBehaviour.Get<CFText>(tf, path);
    }
    
    public static CFButton GetButton(Transform tf, string path)
    {
        return UIBehaviour.Get<CFButton>(tf, path);
    }
    
    public static CFImage GetImage(Transform tf, string path)
    {
        return CFImage.Get<CFImage>(tf, path);
    }
    
    public static CFRawImage GetRawImage(Transform tf, string path)
    {
        return CFRawImage.Get<CFRawImage>(tf, path);
    }
    
    public static Empty4DragRaycast GetEmpty4DragRaycast(Transform tf, string path)
    {
        return Empty4DragRaycast.Get<Empty4DragRaycast>(tf, path);
    }
    
    public static Empty4Raycast GetEmpty4Raycast(Transform tf, string path)
    {
        return Empty4Raycast.Get<Empty4Raycast>(tf, path);
    }
    
    public static CFSlider GetCFSlider(Transform tf, string path)
    {
        return CFSlider.Get<CFSlider>(tf, path);
    }
    
    public static CFAnimation GetAnimation(Transform tf, string path)
    {
        return CFAnimation.Get<CFAnimation>(tf, path);
    }
    
    public static CFInput GetInput(Transform tf, string path)
    {
        return CFInput.Get<CFInput>(tf, path);
    }

    public static CFWrapContent GetWrapContent(Transform tf, string path)
    {
        return CFWrapContent.Get<CFWrapContent>(tf, path);
    }
    
    public static CFSpectorEffect GetSpectorEffect(Transform tf, string path)
    {
        return CFSpectorEffect.Get<CFSpectorEffect>(tf, path);
    }
}



[LuaCallCSharp]
[ReflectionUse]
public static class UnityEngineObjectExtention
{
    public static bool IsNull(this UnityEngine.Object o) 
    {
        return o == null;
    }
}

