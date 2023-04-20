using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEditor;

namespace CFEngine.ShaderVariantsStripper
{
    public class ShaderVariantsStripperConditionAExistAndBNotExist : ShaderVariantsStripperCondition
    {
        private enum AccessLevel
        {
            Global,
            Local
        }

        public List<string> keywordA = new List<string>();
        public List<string> keywordB= new List<string>();
        public bool BHasAnyOne = false;
        
        private AccessLevel _accessLevel = AccessLevel.Global;
        private int _selectedKeywordIndex;
        private string _inputKeyword;

        public bool Completion(Shader shader, ShaderVariantsData data)
        {
            bool valid = keywordA.Count > 0 && keywordB.Count > 0;

            bool AExist = true;

            for (int i = 0; i < keywordA.Count; i++)
            {
                string strItm = keywordA[i];

                if (data.shaderKeywordSet.IsEnabled(new ShaderKeyword(strItm)) ||
                    data.shaderKeywordSet.IsEnabled(new ShaderKeyword(shader, strItm)))
                {

                }
                else
                {
                    AExist = false;
                    break;
                }
            }



            if (!BHasAnyOne)
            {
                bool BNotExist = false;
                for (int i = 0; i < keywordB.Count; i++)
                {
                    string strItm = keywordB[i];

                    if (data.shaderKeywordSet.IsEnabled(new ShaderKeyword(strItm)) ||
                        data.shaderKeywordSet.IsEnabled(new ShaderKeyword(shader, strItm)))
                    {

                    }
                    else
                    {
                        BNotExist = true;
                        break;
                    }
                }

                return valid && AExist && BNotExist;
            }
            else
            {
                bool BHasAnyOne = false;
                for (int i = 0; i < keywordB.Count; i++)
                {
                    string strItm = keywordB[i];

                    if (data.shaderKeywordSet.IsEnabled(new ShaderKeyword(strItm)) ||
                        data.shaderKeywordSet.IsEnabled(new ShaderKeyword(shader, strItm)))
                    {
                        BHasAnyOne = true;
                        break;
                    }
                }

                return valid && AExist && BHasAnyOne;
            }

        }

        public bool EqualTo(ShaderVariantsStripperCondition other)
        {
            
            if (other.GetType() != typeof(ShaderVariantsStripperConditionAExistAndBNotExist))
            {
                return false;
            }

            ShaderVariantsStripperConditionAExistAndBNotExist otherCondition =
                other as ShaderVariantsStripperConditionAExistAndBNotExist;

            if (this.keywordA.Count != otherCondition.keywordA.Count&&this.keywordB.Count != otherCondition.keywordB.Count)
            {
                return false;
            }
            
            var set1 = new HashSet<string>(this.keywordA);
            var set2 = new HashSet<string>(otherCondition.keywordA);
            
            var set1B = new HashSet<string>(this.keywordB);
            var set2B = new HashSet<string>(otherCondition.keywordB);
            return set1.SetEquals(set2)&&set1B.SetEquals(set2B);
            
        }

#if UNITY_EDITOR
        public string Overview()
        {
            return $"当Keyword<{string.Join(",", keywordA)}>存在，且<{string.Join(",", keywordB)}>不存在时";
        }

        public void OnGUI(ShaderVariantsStripperConditionOnGUIContext context)
        {
            
            EditorGUILayout.BeginVertical();
            BHasAnyOne = EditorGUILayout.Toggle("B组合包含任意一个", BHasAnyOne);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space(20);
          
            
            EditorGUILayout.BeginVertical();
            if (context.shader != null)
            {
                #region 选择添加
                EditorGUILayout.BeginHorizontal();
                float width = 
                    EditorGUIUtility.currentViewWidth * 0.25f;
                
                AccessLevel newAccessLevel = (AccessLevel)EditorGUILayout.Popup((int)_accessLevel, new string[] { "Global", "Local" }, GUILayout.Width(width));

                if (newAccessLevel != _accessLevel)
                {
                    _selectedKeywordIndex = 0;
                    _accessLevel = newAccessLevel;
                }

                if (_accessLevel == AccessLevel.Global && context.globalKeywords.Length > 0 ||
                    _accessLevel == AccessLevel.Local && context.localKeywords.Length > 0)
                {
                    _selectedKeywordIndex = EditorGUILayout.Popup(_selectedKeywordIndex,
                        _accessLevel == AccessLevel.Global ? context.globalKeywords : context.localKeywords,
                        GUILayout.Width(width));

                    string selectedKeyword = _accessLevel == AccessLevel.Global
                        ? context.globalKeywords[_selectedKeywordIndex]
                        : context.localKeywords[_selectedKeywordIndex];

                    if (GUILayout.Button("设置为A", GUILayout.Width(width)))
                    {
                        if (!keywordA.Contains(selectedKeyword))
                            keywordA.Add(selectedKeyword);
                    }

                    if (GUILayout.Button("设置为B", GUILayout.Width(width)))
                    {
                        if (!keywordB.Contains(selectedKeyword))
                            keywordB.Add(selectedKeyword);
                    }


                }

                EditorGUILayout.EndHorizontal();
                #endregion
                
                EditorGUILayout.Space(20);
            }
            
            EditorGUILayout.BeginHorizontal();
            
            if (GUILayout.Button("重置A", GUILayout.Width(200)))
            {
                keywordA.Clear();
            }
            
            if (GUILayout.Button("重置B", GUILayout.Width(200)))
            {
                keywordB.Clear();
            }
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.Space(20);
            
            #region 输入添加
            EditorGUILayout.BeginHorizontal();

            _inputKeyword = EditorGUILayout.TextField("输入Keyword", _inputKeyword, GUILayout.Width(EditorGUIUtility.currentViewWidth * 0.5f));
            if (GUILayout.Button("设置为A", GUILayout.Width(EditorGUIUtility.currentViewWidth * 0.25f)) && _inputKeyword != null)
            {
                string[] inputKeywords = _inputKeyword.Split(' ');

                foreach (var keyword in inputKeywords)
                {
                    if (keyword != "" || _inputKeyword == "")
                    {
                        if (!keywordA.Contains(_inputKeyword))
                            keywordA.Add(_inputKeyword);
                    }
                }
            }
            if (GUILayout.Button("设置为B", GUILayout.Width(EditorGUIUtility.currentViewWidth * 0.25f)) && _inputKeyword != null)
            {
                string[] inputKeywords = _inputKeyword.Split(' ');

                foreach (var keyword in inputKeywords)
                {
                    if (keyword != "" || _inputKeyword == "")
                    {
                        if (!keywordB.Contains(_inputKeyword))
                            keywordB.Add(_inputKeyword);
                    }
                }
            }
            
            EditorGUILayout.EndHorizontal();
            #endregion
            
            EditorGUILayout.Space(20);
            
            #region 显示Keyword
            EditorGUILayout.LabelField($"当包含Keyword<{string.Join(",", keywordA)}>,却不包含Keyword<{string.Join(",", keywordB)}>时");
            #endregion
            EditorGUILayout.EndVertical();
        }

        public string GetName()
        {
            return "KeywordA 存在 且 KeywordB不存在";
        }
#endif
    }
}