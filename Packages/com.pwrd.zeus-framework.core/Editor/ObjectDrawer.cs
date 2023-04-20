/*******************************************************************
* Copyright © 2017—2021 Perfect World.Co.Ltd. All rights reserved.
* author: 移动项目支持部 ZEUS FRAMEWORK TEAM
********************************************************************/
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Zeus.Attributes;


namespace Zeus
{
    public static class ObjectDrawer
    {
        private static Dictionary<DrawerPencilType, ICustomGetSet> _dictionaryOwnerObjectType2CustomGetSet = new Dictionary<DrawerPencilType, ICustomGetSet>();
        private static Dictionary<string, ICustomGetSet> _dictionaryOwnerObjectGenericType2CustomGetSet = new Dictionary<string, ICustomGetSet>();
        private static Dictionary<DrawerPencilType, IPencil> _dictionaryObjectType2Pencil = new Dictionary<DrawerPencilType, IPencil>();
        private static Dictionary<string, IPencil> _dictionaryGenericType2Pencil = new Dictionary<string, IPencil>();
        private static Dictionary<Type, IPencil> _dictionaryPencilType2Pencil = new Dictionary<Type, IPencil>();

        private static string _fucusingControlName;

        static ObjectDrawer()
        {
            RegisterPencil<ObjectPencil>(DrawerPencilType.ObjectPencil);
            RegisterPencil<CachePencil>(DrawerPencilType.IntPencil);
            RegisterPencil<CachePencil>(DrawerPencilType.FloatPencil);
            RegisterPencil<CachePencil>(DrawerPencilType.DoublePencil);
            RegisterPencil<CachePencil>(DrawerPencilType.StringPencil);
            RegisterPencil<CachePencil>(DrawerPencilType.PathPencil);
            RegisterPencil<ListPencil>("Zeus.Core.Collections.ObservableList`1");
        }        

        public static void RegisterPencil<TPencil>(DrawerPencilType pencilType)
            where TPencil : IPencil, new()
        {
            IPencil pencil = null;
            if (!_dictionaryPencilType2Pencil.TryGetValue(typeof(TPencil), out pencil))
            {
                pencil = new TPencil();
                _dictionaryPencilType2Pencil.Add(typeof(TPencil), pencil);

                ICustomGetSet customGetSet = pencil as ICustomGetSet;
                if (customGetSet != null)
                {
                    _dictionaryOwnerObjectType2CustomGetSet.Add(pencilType, customGetSet);
                }
            }

            _dictionaryObjectType2Pencil[pencilType] = pencil;
        }

        public static void RegisterPencil<TPencil>(string genericType)
            where TPencil : IPencil, new()
        {
            IPencil pencil = null;
            if (!_dictionaryGenericType2Pencil.TryGetValue(genericType, out pencil))
            {
                pencil = new TPencil();
                _dictionaryGenericType2Pencil.Add(genericType, pencil);

                ICustomGetSet customGetSet = pencil as ICustomGetSet;
                if (customGetSet != null)
                {
                    _dictionaryOwnerObjectGenericType2CustomGetSet.Add(genericType, customGetSet);
                }
            }

            _dictionaryGenericType2Pencil[genericType] = pencil;
        }

        public static void Draw(object drawingObject, DrawerPencilType pencilType = DrawerPencilType.ObjectPencil, string title = null)
        {
            if (drawingObject == null)
            {
                throw new ZeusException("drawingObject can't be null!!!");
            }

            Draw(drawingObject, drawingObject.GetType(),pencilType, null, null, title);
        }

        /// <summary>
        /// 画对象包括标题和值
        /// </summary>
        public static void Draw(object drawingObject, Type objectType, DrawerPencilType pencilType, string title = null)
        {
            Draw(drawingObject, objectType,pencilType, null, null, title);
        }

        /// <summary>
        /// 只画对象的值
        /// </summary>
        public static void DrawValue(object drawingObject, Type objectType, DrawerPencilType pencilType)
        {
            IPencil pencil = _GetPencil(objectType,pencilType);
            _DrawValue(drawingObject, objectType, pencilType, null, 0, pencil);
        }

        public static bool IsValueValidInOwner(object ownerObject, DrawerPencilType pencilType, object memberIdentity)
        {
            ICustomGetSet customGetSet = GetCustomGetSet(ownerObject,pencilType);
            if (customGetSet != null)
            {
                return customGetSet.IsValidValue(ownerObject, memberIdentity);
            }

            return true;
        }

        public static object GetValueInOwner(object ownerObject, DrawerPencilType pencilType, object memberIdentity)
        {
            ICustomGetSet customGetSet = GetCustomGetSet(ownerObject,pencilType);
            if (customGetSet != null)
            {
                return customGetSet.GetValue(ownerObject, memberIdentity);
            }

            MemberInfo memberInfo = (MemberInfo)memberIdentity;
            if (memberInfo.MemberType == System.Reflection.MemberTypes.Field)
            {
                return ((FieldInfo)memberInfo).GetValue(ownerObject);
            }
            else
            {
                PropertyInfo propertyInfo = memberInfo as PropertyInfo;
                if (!propertyInfo.CanRead || !propertyInfo.CanWrite)
                {
                    throw new ZeusException("Drawable property must have both get and set!");
                }

                return propertyInfo.GetValue(ownerObject, null);
            }
        }
    
        public static void SetValueInOwner(object ownerObject, DrawerPencilType pencilType, object memberIdentity, object value)
        {
            ICustomGetSet customGetSet = GetCustomGetSet(ownerObject,pencilType);
            if (customGetSet != null)
            {
                customGetSet.SetValue(ownerObject, memberIdentity, value);
                return;
            }

            MemberInfo memberInfo = (MemberInfo)memberIdentity;
            if (memberInfo.MemberType == System.Reflection.MemberTypes.Field)
            {
                ((FieldInfo)memberInfo).SetValue(ownerObject, value);
            }
            else
            {
                PropertyInfo propertyInfo = memberInfo as PropertyInfo;
                if (!propertyInfo.CanRead || !propertyInfo.CanWrite)
                {
                    throw new ZeusException("Drawable property must have both get and set!");
                }

                propertyInfo.SetValue(ownerObject, value, null);
            }
        }
        internal static void Draw(object drawingObject, Type objectType,DrawerPencilType pencilType, object ownerObject, object memberIdentity, string title)
        {
            IPencil pencil = _GetPencil(objectType,pencilType);
            pencil.DrawTitle(objectType, pencilType, title);
            _DrawValue(drawingObject, objectType, pencilType, ownerObject, memberIdentity, pencil);
            pencil.DrawEnd(objectType, pencilType);
        }

        internal static void DrawValue(object drawingObject, Type objectType, DrawerPencilType pencilType, object ownerObject, object memberIdentity)
        {
            IPencil pencil = _GetPencil(objectType,pencilType);
            _DrawValue(drawingObject, objectType, pencilType, ownerObject, memberIdentity, pencil);
        }

        internal static string ProcessTitle(string title)
        {
            int index = title.IndexOf("_");
            if (index != -1)
            {
                title = title.Substring(index + 1);
            }


            return title.Substring(0, 1).ToUpper() + title.Substring(1);
        }

        private static void _DrawValue(object drawingObject, Type objectType,DrawerPencilType pencilType, object ownerObject, object memberIdentity, IPencil pencil)
        {
            pencil.Draw(drawingObject, objectType, pencilType, ownerObject, memberIdentity);
        }

        private static IPencil _GetPencil(Type objectType,DrawerPencilType pencilType)
        {
            IPencil pencil = null;
            if (objectType.IsGenericType)
            {
                foreach (KeyValuePair<string, IPencil> pair in _dictionaryGenericType2Pencil)
                {
                    if (objectType.FullName.StartsWith(pair.Key))
                    {
                        pencil = pair.Value;
                        break;
                    }
                }
            }
            else
            {
                _dictionaryObjectType2Pencil.TryGetValue(pencilType, out pencil);
            }

            if (pencil == null)
            {
                pencil = _dictionaryObjectType2Pencil[DrawerPencilType.ObjectPencil];
            }
            return pencil;
        }


        private static ICustomGetSet GetCustomGetSet(object ownerObject,DrawerPencilType pencilType)
        {
            Type ownerType = ownerObject.GetType();
            ICustomGetSet customGetSet = null;
            if (ownerType.IsGenericType)
            {
                foreach (KeyValuePair<string, ICustomGetSet> pair in _dictionaryOwnerObjectGenericType2CustomGetSet)
                {
                    if (ownerType.FullName.StartsWith(pair.Key))
                    {
                        customGetSet = pair.Value;
                        break;
                    }
                }
            }
            else
            {
                _dictionaryOwnerObjectType2CustomGetSet.TryGetValue(pencilType, out customGetSet);
            }

            return customGetSet;
        }

        /// <summary>
        /// 计算childType距离parentType的继承链上有多远
        /// </summary>
        /// <param name="childType"></param>
        /// <param name="parentType"></param>
        /// <returns>0代表无继承关系</returns>
        private static int _GetTypeHierarchyCount(Type childType, Type parentType)
        {
            if (!parentType.IsAssignableFrom(childType))
            {
                return 0;
            }

            Type baseType = childType.BaseType;
            if (baseType == parentType)
            {
                return 1;
            }

            foreach (Type interfaceType in childType.GetInterfaces())
            {
                if (interfaceType == parentType)
                {
                    return 1;
                }
                else
                {
                    int countTmp = _GetTypeHierarchyCount(interfaceType, parentType);
                    if (countTmp != 0)
                    {
                        ++countTmp;
                        return countTmp;
                    }
                }
            }

            int count = _GetTypeHierarchyCount(baseType, parentType);
            if (count != 0)
            {
                ++count;
                return count;
            }

            return 0;
        }
    }


    public interface IPencil
    {
        void DrawTitle(Type objectType,DrawerPencilType pencilType, string title);
        object Draw(object drawingObject, Type objectType, DrawerPencilType pencilType, object ownerObject, object memberIdentity);
        void DrawEnd(Type objectType, DrawerPencilType pencilType);
    }

    public interface ICustomGetSet
    {
        object GetValue(object ownerObject, object memberIdentity);
        void SetValue(object ownerObject, object memberIdentity, object value);
        bool IsValidValue(object ownerObject, object memberIdentity);
    }

    internal class ObjectPencil : IPencil
    {
        public void DrawTitle(Type objectType, DrawerPencilType pencilType, string title)
        {
            if (!string.IsNullOrEmpty(title))
            {
                EditorGUILayout.LabelField(title, GUILayout.MinWidth(20));
            }
        }

        public void DrawEnd(Type objectType, DrawerPencilType pencilType) { }

        public object Draw(object drawingObject, Type objectType, DrawerPencilType pencilType, object ownerObject, object memberIdentity)
        {
            if (drawingObject == null)
            {
                return null;
            }

            var members = objectType.GetMembers(
                System.Reflection.BindingFlags.Instance |
                System.Reflection.BindingFlags.Public |
                System.Reflection.BindingFlags.NonPublic);

            bool drawAny = false;
            foreach (var memberInfo in members)
            {
                var drawableMemberAttr = memberInfo.GetCustomAttribute<DrawableMemberAttribute>(false);
                if (drawableMemberAttr != null)
                {
                    if (drawAny)
                    {
                        EditorGUILayout.Separator();
                    }

                    Type memberType = _GetMemberType(memberInfo);
                    string title = drawableMemberAttr.m_labelName != null ? drawableMemberAttr.m_labelName : ObjectDrawer.ProcessTitle(memberInfo.Name);
                    DrawerPencilType memberPencilType = drawableMemberAttr.m_PencilType;
                    object memberValue = ObjectDrawer.GetValueInOwner(drawingObject, memberPencilType, memberInfo);
                    ObjectDrawer.Draw(memberValue, memberType, memberPencilType, drawingObject, memberInfo, title);

                    drawAny = true;
                }
            }

            return drawingObject;
        }

        private Type _GetMemberType(MemberInfo memberInfo)
        {
            if (memberInfo.MemberType == System.Reflection.MemberTypes.Field)
            {
                return ((FieldInfo)memberInfo).FieldType;
            }
            else
            {
                return ((PropertyInfo)memberInfo).PropertyType;
            }
        }
    }


    internal class CachePencil : IPencil
    {
        private struct ObjectCaching
        {
            public object m_ownerObject;
            public object m_memberIdentity;
            public DrawerPencilType m_PencilType;

            public ObjectCaching(object ownerObject, object memberIdentity, DrawerPencilType pencilType)
            {
                m_ownerObject = ownerObject;
                m_memberIdentity = memberIdentity;
                m_PencilType = pencilType;
            }
        }

        private Dictionary<DrawerPencilType, IPencil> _dictionaryObjectType2Pencil = new Dictionary<DrawerPencilType, IPencil>();
        private Dictionary<string, ObjectCaching> _dictionaryControlName2ObjectCaching = new Dictionary<string, ObjectCaching>();
        private string _focusingControlName;
        private object _inputCacheValue;
        private int _controlNameIndex;

        public CachePencil()
        {
            _dictionaryObjectType2Pencil.Add(DrawerPencilType.IntPencil, new IntPencil());
            _dictionaryObjectType2Pencil.Add(DrawerPencilType.DoublePencil, new DoublePencil());
            _dictionaryObjectType2Pencil.Add(DrawerPencilType.FloatPencil, new FloatPencil());
            _dictionaryObjectType2Pencil.Add(DrawerPencilType.StringPencil, new StringPencil());
            _dictionaryObjectType2Pencil.Add(DrawerPencilType.PathPencil, new PathPencil());
        }

        public void DrawTitle(Type objectType, DrawerPencilType pencilType, string title)
        {
            _dictionaryObjectType2Pencil[pencilType].DrawTitle(objectType, pencilType, title);
        }

        public void DrawEnd(Type objectType, DrawerPencilType pencilType)
        {
            _dictionaryObjectType2Pencil[pencilType].DrawEnd(objectType, pencilType);
        }

        public object Draw(object drawingObject, Type objectType, DrawerPencilType pencilType, object ownerObject, object memberIdentity)
        {
            string curFocusingControlName = GUI.GetNameOfFocusedControl();

            string curControlName = _GetControlName(ownerObject, pencilType, memberIdentity);
            IPencil pencil = _dictionaryObjectType2Pencil[pencilType];
            GUI.SetNextControlName(curControlName);
            ObjectCaching objectCaching;
            if (_dictionaryControlName2ObjectCaching.TryGetValue(curControlName, out objectCaching))
            {
                if (ObjectDrawer.IsValueValidInOwner(objectCaching.m_ownerObject, pencilType, objectCaching.m_memberIdentity))
                {
                    object oldValue = ObjectDrawer.GetValueInOwner(objectCaching.m_ownerObject, pencilType, objectCaching.m_memberIdentity);
                    _inputCacheValue = pencil.Draw(oldValue, objectType, pencilType, ownerObject, memberIdentity);
                    if (!object.Equals(oldValue, _inputCacheValue))
                    {
                        ObjectDrawer.SetValueInOwner(objectCaching.m_ownerObject, pencilType, objectCaching.m_memberIdentity, _inputCacheValue);
                    }
                }
            }

            return _inputCacheValue;
        }


        private string _GetControlName(object ownerObject,DrawerPencilType pencilType, object memberIdentity)
        {
            foreach (KeyValuePair<string, ObjectCaching> pair in _dictionaryControlName2ObjectCaching)
            {
                if (pair.Value.m_ownerObject == ownerObject && object.Equals(pair.Value.m_memberIdentity, memberIdentity))
                {
                    return pair.Key;
                }
            }

            string controlName = "__CachePencilController_" + _controlNameIndex++;
            _dictionaryControlName2ObjectCaching.Add(controlName, new ObjectCaching(ownerObject, memberIdentity, pencilType));
            return controlName;
        }
    }


    internal class IntPencil : IPencil
    {
        public void DrawTitle(Type objectType, DrawerPencilType pencilType, string title)
        {
            EditorGUILayout.BeginHorizontal();
            if (!string.IsNullOrEmpty(title))
            {
                EditorGUILayout.LabelField(title, GUILayout.MinWidth(20));
            }
        }

        public void DrawEnd(Type objectType, DrawerPencilType pencilType)
        {
            EditorGUILayout.EndHorizontal();
        }

        public virtual object Draw(object drawingObject, Type objectType, DrawerPencilType pencilType, object ownerObject, object memberIdentity)
        {
            return EditorGUILayout.IntField((int)drawingObject);
        }
    }


    internal class StringPencil : IntPencil
    {
        public override object Draw(object drawingObject, Type objectType, DrawerPencilType pencilType, object ownerObject, object memberIdentity)
        {
            return EditorGUILayout.TextField((string)drawingObject);
        }
    }

    internal class PathPencil : IntPencil
    {
        public override object Draw(object drawingObject, Type objectType, DrawerPencilType pencilType,object ownerObject, object memberIdentity)
        {
            GUILayout.BeginHorizontal();

            string returnStr = EditorGUILayout.TextField((string)drawingObject);
            if (GUILayout.Button("Browse", GUILayout.Width(80)))
            {
                string generateViewLuaPath = EditorUtility.OpenFolderPanel("Select Directory", Application.dataPath, "Select Directory");
                if (!string.IsNullOrEmpty(generateViewLuaPath))
                {
                    //筛选出相对路径                    
                    System.Uri dataPathUri = new System.Uri(Application.dataPath + "/");
                    System.Uri relativeUri = dataPathUri.MakeRelativeUri(new System.Uri(generateViewLuaPath));
                    string relativePath = System.Uri.UnescapeDataString(relativeUri.ToString());

                    returnStr = relativePath;
                    EditorGUILayout.TextField(returnStr);
                }
            }
            GUILayout.EndHorizontal();
            if (!string.IsNullOrEmpty(returnStr))
            {
                returnStr = returnStr.Replace("\\", "/");
                while (returnStr.EndsWith("/"))
                {
                    returnStr = returnStr.Substring(0, returnStr.Length - 1);
                }
            }
            return returnStr;
        }
    }


    internal class FloatPencil : IntPencil
    {
        public override object Draw(object drawingObject, Type objectType, DrawerPencilType pencilType,object ownerObject, object memberIdentity)
        {
            return EditorGUILayout.FloatField((float)drawingObject);
        }
    }

    internal class DoublePencil : IntPencil
    {
        public override object Draw(object drawingObject, Type objectType, DrawerPencilType pencilType,object ownerObject, object memberIdentity)
        {
            return EditorGUILayout.DoubleField((double)drawingObject);
        }
    }

    internal class ListPencil : IPencil, ICustomGetSet
    {
        private object[] _oneParams = new object[1];
        private object[] _twoParams = new object[2];
        private List<int> _needRemoves = new List<int>();
        private List<object> _listMemberIdentity = new List<object>();
        private int _maxMemberIdentity;

        public void DrawTitle(Type objectType, DrawerPencilType pencilType,string title)
        {
            EditorGUILayout.LabelField(title, GUILayout.MinWidth(20));
        }

        public object Draw(object list, Type objectType, DrawerPencilType pencilType,object ownerObject, object memberIdentity)
        {
            var type = list.GetType();
            int count = (int)type.GetProperty("Count").GetValue(list, null);

            var getFunc = type.GetMethod("get_Item", BindingFlags.Instance | BindingFlags.Public);
            var removeAt = type.GetMethod("RemoveAt", BindingFlags.Instance | BindingFlags.Public);

            Type itemType = null;
            if (objectType.IsGenericType)
            {
                itemType = objectType.GetGenericArguments()[0];
            }

            _needRemoves.Clear();
            for (int i = 0; i < count; ++i)
            {
                EditorGUILayout.BeginHorizontal();

                _oneParams[0] = i;
                object item = getFunc.Invoke(list, _oneParams);

                object curMemberIdentity = null;
                if (i >= _listMemberIdentity.Count)
                {
                    curMemberIdentity = _maxMemberIdentity++;
                    _listMemberIdentity.Add(curMemberIdentity);
                }
                else
                {
                    curMemberIdentity = _listMemberIdentity[i];
                }

                if (itemType != null)
                {
                    ObjectDrawer.DrawValue(item, itemType, pencilType, list, curMemberIdentity);
                }
                else 
                {
                    if (item == null)
                    {
                        ObjectDrawer.DrawValue(item,typeof(string),DrawerPencilType.StringPencil, list, curMemberIdentity);
                    }
                    else
                    {
                        ObjectDrawer.DrawValue(item, item.GetType(), pencilType, list, curMemberIdentity);                        
                    }
                }

                if (GUILayout.Button("Delete", GUILayout.Width(50)))
                {
                    _needRemoves.Add(i);
                }

                EditorGUILayout.EndHorizontal();
            }

            for (int i=0; i<_needRemoves.Count; ++i)
            {
                _oneParams[0] = _needRemoves[i];
                removeAt.Invoke(list, _oneParams);
                _listMemberIdentity.RemoveAt(_needRemoves[i]);
            }

            var addFunc = list.GetType().GetMethod("Add", BindingFlags.Instance | BindingFlags.Public);

            if (GUILayout.Button("New"))
            {
                _oneParams[0] = null;
                addFunc.Invoke(list, _oneParams);

                _listMemberIdentity.Add(_maxMemberIdentity++);
            }

            return list;
        }

        public void DrawEnd(Type objectType, DrawerPencilType pencilType) { }

        public object GetValue(object list, object memberIdentity) 
        {
            var type = list.GetType();
            var getFunc = type.GetMethod("get_Item", BindingFlags.Instance | BindingFlags.Public);
            _oneParams[0] = _GetIndex(memberIdentity);
            return getFunc.Invoke(list, _oneParams);
        }

        public void SetValue(object list, object memberIdentity, object value)
        {
            var type = list.GetType();
            var setFunc = type.GetMethod("set_Item", BindingFlags.Instance | BindingFlags.Public);
            _twoParams[0] = _GetIndex(memberIdentity);
            _twoParams[1] = value;
            setFunc.Invoke(list, _twoParams);
        }

        private int _GetIndex(object memberIdentity)
        {
            for (int i=0; i<_listMemberIdentity.Count; ++i)
            {
                if (object.Equals(_listMemberIdentity[i], memberIdentity))
                {
                    return i;
                }
            }

            return -1;
        }

        public bool IsValidValue(object ownerObject, object memberIdentity)
        {
            return _GetIndex(memberIdentity) >= 0;
        }
    }
}