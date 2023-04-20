using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Devops.Core
{
    public class FunctionsPanel : MonoBehaviour
    {
        static FunctionsPanel instance;
        public static FunctionsPanel Instance()
        {
            return instance;
        }

        private void Awake()
        {
            instance = this;
            gameObject.SetActive(false);
        }

        public Button mClose;
        public Button[] mFunctionButtons;

        public void SetEnable(bool enable)
        {
            gameObject.SetActive(enable);
        }
        // Start is called before the first frame update
        void Start()
        {
            mClose?.onClick.AddListener(() => {
                SetEnable(false);
                EntrancePanel.Instance().SetEnable(true);
            });
            for(int i =0; i < mFunctionButtons.Length; i++)
            {
                if(DevopsUI.RegisterFunctions.ContainsKey(i))
                {
                    RegisterUIFunction(DevopsUI.RegisterFunctions[i].function, i, DevopsUI.RegisterFunctions[i].text);
                }
                else
                {
                    mFunctionButtons[i].gameObject.SetActive(false);
                }
            }
        }

        public void RegisterUIFunction(Action function, int nIndex, string text)
        {
            if (nIndex >= 0 && nIndex < mFunctionButtons.Length && mFunctionButtons[nIndex] != null)
            {
                mFunctionButtons[nIndex].onClick.RemoveAllListeners();
                mFunctionButtons[nIndex].onClick.AddListener(() => {
                    OnFunctionButtonClick(nIndex);
                });
                if(text != null)
                {
                    mFunctionButtons[nIndex].transform.Find("Text").GetComponent<Text>().text = text;
                }
                mFunctionButtons[nIndex].gameObject.SetActive(true);
            }
        }

        public void UnRegisterUIFunction(int nIndex)
        {
            if (nIndex >= 0 && nIndex < mFunctionButtons.Length && mFunctionButtons[nIndex] != null)
            {
                if(mFunctionButtons[nIndex] != null)
                {
                    mFunctionButtons[nIndex].onClick.RemoveAllListeners();
                    mFunctionButtons[nIndex].gameObject.SetActive(false);
                }
            }
        }

        void OnFunctionButtonClick(int index)
        {
            if(DevopsUI.RegisterFunctions.ContainsKey(index))
            {
                DevopsUI.RegisterFunctions[index].function.Invoke();
                SetEnable(false);
                EntrancePanel.Instance().SetEnable(true);
            }
        }
    }

}