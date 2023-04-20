using System;
using System.Collections.Generic;
using UnityEngine.Playables;
using UnityEngine;
using Blueprint;

namespace Blueprint.CSharpReflection
{
    public static class CSharpReflectionSettings
    {
        //导出时强制做为静态类的类型(注意customTypeList 还要添加这个类型才能导出)
        //unity 有些类作为sealed class, 其实完全等价于静态类
        public static List<Type> staticClassTypes = new List<Type>
        {
        
        };

        public static BindType[] customClassList =
        {
             // Actor
            _GT(typeof(Blueprint.Actor.ActorBase)),
            _GT(typeof(Blueprint.Actor.BlueprintActor)),
            _GT(typeof(Blueprint.Actor.ActorUtil)),
            _GT(typeof(Blueprint.Actor.ActorInvoke)),
            _GT(typeof(Blueprint.PoolLoadInfo)),

            // Core
            _GT(typeof(Blueprint.Logic.SaveGame)),
            _GT(typeof(Blueprint.Logic.BP_Base)),
            _GT(typeof(Blueprint.DelayHandle)),

            // Util
            _GT(typeof(Blueprint.Util.ProjectileUtil)),
            _GT(typeof(Blueprint.Logic.JsonUtility)),

            _GT(typeof(BpGlobalEvent)),
        };

        public static BindType _GT(Type t)
        {
            return new BindType(t);
        }
    }

    public class Test
    {

        public static void TestFun(int a, [ParamType("function_return")] UnityEngine.Events.UnityAction act)
        {

        }

        [Code("Convert.ToString((num|num))")]
        public static string FloatToString(float num)
        {
            return Convert.ToString(num);
        }
    }
}

public class TestReflection
{
    public void TestCallBack([ParamType("function")] System.Action act)
    {

    }

    public void TestCallBack2([ParamType("function_return")] System.Action act)
    {

    }

    //public void TestParams([ParamType("IndefiniteParam(int)")] params int[] values)
    //{

    //}

    public void TestProtocol([ParamType("ProtocolMessage")] System.Type t)
    {

    }

    //public void TestEnum([ParamType("Enum")] SomeEnum t)
    //{

    //}

    public void TestResource([ParamType("prefab(name)")] string r)
    {

    }

    public void TestEvent([ParamType("BlueprintEvent")] string e)
    {

    }

    [MethodDescription("datanode")]
    public static int TestDatanode(int i)
    {
        return 0;
    }

    [MethodDescription("datanode express")]
    public static int TestExpress(int i)
    {
        return 0;
    }

    //[Comment("函数注释")]
    //[return: Comment("返回值注释")]
    //public int TestCommonet([Comment("参数注释")]int i)
    //{
    //    return 0;
    //}

}