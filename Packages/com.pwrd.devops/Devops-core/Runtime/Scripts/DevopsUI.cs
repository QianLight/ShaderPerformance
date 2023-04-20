using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Devops.Core
{
    public class RegisterFunctionInfo
    {
        public Action function;
        public string text;
    }

    public class DevopsUI : MonoBehaviour
    {
        public static Dictionary<int, RegisterFunctionInfo> RegisterFunctions = new Dictionary<int, RegisterFunctionInfo>();
        public static void RegisterFunction(Action function, int nIndex, string text = null)
        {
            if (RegisterFunctions.ContainsKey(nIndex))
            {
                RegisterFunctions[nIndex].function = function;
                RegisterFunctions[nIndex].text = text;
            }
            else
            {
                RegisterFunctions.Add(nIndex, new RegisterFunctionInfo() { function = function, text = text });
            }

            if (FunctionsPanel.Instance() != null)
            {
                FunctionsPanel.Instance().RegisterUIFunction(function, nIndex, text);
            }
        }

        public static void UnRegisterFunction(int nIndex)
        {
            if (RegisterFunctions.ContainsKey(nIndex))
                RegisterFunctions.Remove(nIndex);
            if (FunctionsPanel.Instance() != null)
            {
                FunctionsPanel.Instance().UnRegisterUIFunction(nIndex);
            }
        }


        public Transform PanelParent;

        static DevopsUI instance;
        public static DevopsUI Instance()
        {
            return instance;
        }

        private void Awake()
        {
            instance = this;
        }

        private void Start()
        {
            DontDestroyOnLoad(gameObject);
        }
    }
}