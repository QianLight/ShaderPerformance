using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Reflection;
using System.IO;
using UNBridgeLib;

public class InvokeClassStaticMethodResult {
	public string Namespace;
	public string Class;
	public string Method;
	public object Return;
	public bool Void;
	public Exception Exception;
	//缓存
	static private ArrayList arrCacheType = new ArrayList();

	public static Assembly getAssemblyOfClass(string className) {
		// var results = new List<InvokeClassStaticMethodResult>();
		bool find = false;
		foreach (Type _t in arrCacheType) {
			if((_t.FullName == className) && _t.IsClass) {
				return _t.Assembly;
			}
		}
		if (!find) {
			foreach (Assembly _a in AppDomain.CurrentDomain.GetAssemblies()) {
				foreach (Type _t in _a.GetTypes()) {
					if ((_t.FullName == className) && _t.IsClass) {

						arrCacheType.Add (_t);
						return _t.Assembly;
					}
				}
			}
		}
		return null;
	}

	public static bool existClass(string className) {
		// var results = new List<InvokeClassStaticMethodResult>();
		bool find = false;
		foreach (Type _t in arrCacheType) {
			if((_t.FullName == className) && _t.IsClass) {
				find = true;
//				LogUtils.D ("class exist in cache:" + className);
				break;
			}
		}
		if (!find) {
			foreach (Assembly _a in AppDomain.CurrentDomain.GetAssemblies()) {
				foreach (Type _t in _a.GetTypes()) {
					if ((_t.FullName == className) && _t.IsClass) {

						arrCacheType.Add (_t);
						find = true;
						LogUtils.D ("class exist:" , className);
						break;
					}
				}
			}
		}
		return find;
	}

	public static InvokeClassStaticMethodResult[] InvokeMethod(string className, object obj, string methodName, Type[] argsType, bool throwExceptions, params object[] parameters) {
		var results = new List<InvokeClassStaticMethodResult>();
		Type findType = className.GetType();
		bool find = false;
		foreach (Type _t in arrCacheType) {
			if((_t.FullName == className) && _t.IsClass) {
				findType = _t;
				find = true;
//				LogUtils.D ("find class in cache:" + findType.FullName);
				break;
			}
		}
		if (!find) {
			foreach (Assembly _a in AppDomain.CurrentDomain.GetAssemblies()) {
				foreach (Type _t in _a.GetTypes()) {
					if ((_t.FullName == className) && _t.IsClass) {

						arrCacheType.Add (_t);
						findType = _t;
						find = true;
						LogUtils.D ("find class:" , findType.FullName);
						break;
					}
				}
			}
		}

		if (find) {
			MethodInfo method_t;
			if (argsType != null) {
				method_t = findType.GetMethod (methodName, argsType);
			} else {
				method_t = findType.GetMethod (methodName);
			}
			if (method_t == null) {
				PropertyInfo pInfo = findType.GetProperty (methodName);
				if (pInfo != null) {
					method_t = pInfo.GetGetMethod ();
				}
			}
			if ((method_t != null) && method_t.IsPublic) {
				var details_t = new InvokeClassStaticMethodResult ();
				details_t.Namespace = findType.Namespace;
				details_t.Class = findType.Name;
				details_t.Method = method_t.Name;
				try {
					if (method_t.ReturnType == typeof(void)) {
						if (method_t.IsStatic) {
							method_t.Invoke (null, parameters);
						} else if (obj != null) {
							method_t.Invoke (obj, parameters);
						}
						details_t.Void = true;
					} else {
						if (method_t.IsStatic) {
							details_t.Return = method_t.Invoke (null, parameters);
						} else if (obj != null) {
							details_t.Return = method_t.Invoke (obj, parameters);
						}

					}
				} catch (Exception ex) {
					if (throwExceptions) {
						throw;
					} else {
						details_t.Exception = ex;
					}
				}
				results.Add (details_t);
			} else {
				LogUtils.E ("invoke method failed:" + methodName);
			}
		} else {
			LogUtils.E ("not find class:" + className);
		}

		return results.ToArray();
	}

}