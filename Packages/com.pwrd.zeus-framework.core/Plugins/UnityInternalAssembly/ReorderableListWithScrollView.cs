/*******************************************************************
* Copyright © 2017—2021 Perfect World.Co.Ltd. All rights reserved.
* author: 移动项目支持部 ZEUS FRAMEWORK TEAM
********************************************************************/
#if UNITY_EDITOR
using System;
using UnityEditorInternal;
using UnityEngine;

namespace UnityEditor
{
    internal class ReorderableListWithScrollView : ReorderableListWithRenameAndScrollView
    {
        public ReorderableListWithScrollView(ReorderableList list, State state) : base(list, state)
        {
            onGetNameAtIndex = _Internal_GetNameAtIndex_DoNothing;
        }

        public ReorderableListWithScrollView(ReorderableList list, Vector2 scrollPos) : base(list, new State() { m_ScrollPos = scrollPos })
        {
            onGetNameAtIndex = _Internal_GetNameAtIndex_DoNothing;
        }

        private State m_NewState;

        public new State State
        {
            get
            {
                if (m_NewState == null)
                {
                    var field = typeof(ReorderableListWithRenameAndScrollView).GetField("m_State", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    m_NewState = field.GetValue(this) as State;
                }
                return m_NewState;
            }
        }

        public Vector2 ScrollPos
        {
            get
            {
                return m_NewState.m_ScrollPos;
            }
            set
            {
                m_NewState.m_ScrollPos = value;
            }
        }

        public new void OnGUI(Rect rect)
        {
            //  该处try-catch纯属为了消除DrawScrollDropShadow处实例为null的错误
            try
            {
                base.OnGUI(rect);
                var d = State;
            }
            catch (Exception e)
            {
                if (!e.StackTrace.Contains("DrawScrollDropShadow"))
                {
                    Debug.LogError(e.StackTrace);
                }
            }
        }

        //  该处纯属为了消除 没有onGetNameAtIndex回调时的错误
        private string _Internal_GetNameAtIndex_DoNothing(int index)
        {
            return string.Empty;
        }
    }
}
#endif
