using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

public class MatchInfo
{
    public int m_startLineIndexLeft;
    public int m_startLineIndexRight;
    public int m_matchCount;
}

/// <summary>
/// 会对AssetGraph进行处理，取消编码，方便进行修改查看
/// 同时也是一个通用的比较工具
/// </summary>
public class GraphComparerWindow : EditorWindow
{

    [MenuItem("Zeus/AssetGraph/ComparerWindow")]
    public static void Open()
    {
        var window = GetWindow<GraphComparerWindow>();
        window.minSize = new Vector2(1024, 768);
        var style = window.m_textAreaStyle = new GUIStyle() { richText = true};
        style.normal.textColor = Color.white;
        style.normal.textColor = Color.white;
    }

    private int m_currentViewLineIndex;
    private string[] m_leftLines = new string[0];
    private string m_leftContent;
    private string m_leftLightContent;
    private string m_filterContent;
    private bool m_searchPrev;
    private string[] m_rightLines = new string[0];
    private int m_currentMatchLine;
    private string m_rightLightContent;
    private string m_rightContent;
    private Vector2 m_scrollViewPosition;
    private GUIStyle m_textAreaStyle;
    private SearchOptions m_lastSearchOptions;

    private class SearchOptions
    {
        public string Key;
        public bool IsLeft;
        public bool IsPrev;

        public override bool Equals(object obj)
        {
            var options = obj as SearchOptions;
            if(null == options)
            {
                return false;
            }
            return options.Key == Key && options.IsLeft == IsLeft && options.IsPrev == IsPrev;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }

    private void OnGUI()
    {
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("打开左侧文件"))
        {
            var graphPath = EditorUtility.OpenFilePanel("OpenGraph", Application.dataPath, "asset");
            m_leftLines = GraphComparerUtility.PrettyGraph(graphPath);
            var sb = new StringBuilder();
            for (var i = 0; i < m_leftLines.Length; i++)
            {
                sb.AppendLine(m_leftLines[i]);
            }
            m_leftContent = sb.ToString();
        }
        if (GUILayout.Button("打开右侧文件"))
        {
            var graphPath = EditorUtility.OpenFilePanel("OpenGraph", Application.dataPath, "asset");
            m_rightLines = GraphComparerUtility.PrettyGraph(graphPath);
            var sb = new StringBuilder();
            for (var i = 0; i < m_rightLines.Length; i++)
            {
                sb.AppendLine(m_rightLines[i]);
            }
            m_rightContent = sb.ToString();
        }
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();

        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("比较"))
        {
            Diff();
        }
        if (GUILayout.Button("上一个不同点"))
        {
            JumpToPrevDiff();
        }
        if (GUILayout.Button("下一个不同点"))
        {
            JumpToNextDiff();
        }
        if (GUILayout.Button("重置"))
        {
            m_leftContent = "";
            m_leftLightContent = "";
            m_leftLines = null;
            m_rightContent = "";
            m_rightLightContent = "";
            m_rightLines = null;
            m_scrollViewPosition = Vector2.zero;
        }
        GUILayout.EndHorizontal();
        m_scrollViewPosition = GUILayout.BeginScrollView(m_scrollViewPosition, true, true);
        m_currentViewLineIndex = (int)(m_scrollViewPosition.y / m_textAreaStyle.lineHeight);
        GUILayout.BeginHorizontal();
        if(!string.IsNullOrEmpty(m_leftLightContent))
        {
            GUILayout.TextArea(m_leftLightContent, m_textAreaStyle, GUILayout.Width(Screen.width * 0.5f));
        }
        else
        {
            GUILayout.TextArea(m_leftContent, m_textAreaStyle, GUILayout.Width(Screen.width * 0.5f));
        }
        if(!string.IsNullOrEmpty(m_rightLightContent))
        {
            GUILayout.TextArea(m_rightLightContent, m_textAreaStyle, GUILayout.Width(Screen.width * 0.5f));
        }
        else
        {
            GUILayout.TextArea(m_rightContent, m_textAreaStyle, GUILayout.Width(Screen.width * 0.5f));
        }
        GUILayout.EndHorizontal();
        GUILayout.EndScrollView();
        GUILayout.BeginHorizontal();
        m_filterContent = GUILayout.TextField(m_filterContent);
        m_searchPrev = GUILayout.Toggle(m_searchPrev, "向上查找" );
        if (GUILayout.Button("查找左边"))
        {
            if (!string.IsNullOrEmpty(m_filterContent))
            {
                if(null == m_lastSearchOptions || !m_lastSearchOptions.IsLeft || m_lastSearchOptions.Key != m_filterContent)
                {
                    m_leftLightContent = m_leftContent.Replace(m_filterContent, WithBlueColor(m_filterContent));
                    m_rightLightContent = "";
                }

                if (m_searchPrev)
                {
                    for (var i = m_leftLines.Length - 1; i >= 0; i--)
                    {
                        if (SearchKeyAtIndex(i, m_leftLines, false))
                        {
                            break;
                        }
                    }
                }
                else
                {
                    for (var i = 0; i < m_leftLines.Length; i++)
                    {
                        if (SearchKeyAtIndex(i, m_leftLines, true))
                        {
                            break;
                        }
                    }
                }
                if(null == m_lastSearchOptions)
                {
                    m_lastSearchOptions = new SearchOptions();
                }
                m_lastSearchOptions.Key = m_filterContent;
                m_lastSearchOptions.IsLeft = true;

            }
        }
        if (GUILayout.Button("查找右边"))
        {
            if(null == m_lastSearchOptions || m_lastSearchOptions.IsLeft || m_lastSearchOptions.Key != m_filterContent)
            {
                m_rightLightContent = m_rightContent.Replace(m_filterContent, WithBlueColor(m_filterContent));
                m_leftLightContent = "";
            }

            if (!string.IsNullOrEmpty(m_filterContent))
            {
                if (m_searchPrev)
                {
                    for (var i = m_rightLines.Length - 1; i >= 0; i--)
                    {
                        if (SearchKeyAtIndex(i, m_rightLines, false))
                        {
                            break;
                        }
                    }
                }
                else
                {
                    for (var i = 0; i < m_rightLines.Length; i++)
                    {
                        if (SearchKeyAtIndex(i, m_rightLines, true))
                        {
                            break;
                        }
                    }
                }
                if(null == m_lastSearchOptions)
                {
                    m_lastSearchOptions = new SearchOptions();
                }
                m_lastSearchOptions.Key = m_filterContent;
                m_lastSearchOptions.IsLeft = false;
            }
        }
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("粘贴到左边"))
        {
            m_leftContent = ProcessContent(GUIUtility.systemCopyBuffer, ref m_leftLines);
        }
        if (GUILayout.Button("粘贴到右边"))
        {
            m_rightContent = ProcessContent(GUIUtility.systemCopyBuffer, ref m_rightLines);
        }
        GUILayout.EndHorizontal();
    }

    private bool SearchKeyAtIndex(int i, string[] lines, bool next)
    {
        if (lines[i].Contains(m_filterContent))
        {
            if(next && i <= m_currentMatchLine)
            {
                return false;
            }
            else if(!next && i >= m_currentMatchLine)
            {
                return false;
            }
            Debug.Log($"jump to right line {i} with content {lines[i]}");
            JumpToLine(i);
            m_currentMatchLine = i;
            return true;
        }
        return false;
    }


    private string ProcessContent(string formatedContent, ref string[] lines )
    {
        formatedContent = GraphComparerUtility.ConvertLineEndings(formatedContent);
        var tmpLines = formatedContent.Split('\n');
        lines = GraphComparerUtility.PrettyLines(tmpLines);
        var sb = new StringBuilder();
        for(var i = 0; i < lines.Length; i++)
        {
            sb.AppendLine(lines[i]);
        }
        return sb.ToString();
    }

    private void JumpToLine(int viewLineIndex)
    {
        m_scrollViewPosition.y = viewLineIndex * m_textAreaStyle.lineHeight;
    }

    private void JumpToPrevDiff()
    {
        var currentY = m_scrollViewPosition.y;
        var prevLineIndex = GetPrevDiffLine(currentY, m_opList);
        if(prevLineIndex.HasValue)
        {
            JumpToLine(prevLineIndex.Value);
        }
    }

    private int? GetPrevDiffLine(float currentY, List<GraphComparerUtility.Op> opList)
    {
        for(var i = 0; i < opList.Count; i++)
        {
            var lineY = GetLinePositionY(opList[i].m_index);
            if (lineY < currentY)
            {
                return opList[i].m_index;
            }
        }
        return null;
    }

    private int? GetNextDiffLine(float currentY, List<GraphComparerUtility.Op> opList)
    {
        for(var i = opList.Count - 1; i >= 0; i--)
        {
            var lineY = GetLinePositionY(opList[i].m_index);
            if (lineY > currentY)
            {
                return opList[i].m_index;
            }
        }
        return null;
    }

    private void JumpToNextDiff()
    {
        var currentY = m_scrollViewPosition.y;
        var nextLineIndex = GetNextDiffLine(currentY, m_opList);
        if(nextLineIndex.HasValue)
        {
            JumpToLine(nextLineIndex.Value);
        }
    }

    private float GetLinePositionY(int lineIndex)
    {
        return lineIndex * m_textAreaStyle.lineHeight;
    }
    
    private List<GraphComparerUtility.Op> m_opList = new List<GraphComparerUtility.Op>();
    private void Diff()
    {
        var leftlist = new List<string>(m_leftLines);
        var rightList = new List<string>(m_rightLines);
        var rightContentList = new List<string>(m_leftLines);
        var opList = GraphComparerUtility.EditList(leftlist, rightList);
        m_opList = opList;
        for (var i = 0; i < opList.Count; i++)
        {
            var index = opList[i].m_index;
            switch(opList[i].m_opType)
            {
                case GraphComparerUtility.OpType.Delete:
                    rightContentList[index] = WithRedColor("     ");
                    leftlist[index] = WithRedColor(leftlist[index]);
                    break;
                case GraphComparerUtility.OpType.Insert:
                    rightContentList.Insert(index, WithRedColor(opList[i].m_content));
                    leftlist.Insert(index, "");
                    break;
                case GraphComparerUtility.OpType.Replace:
                    rightContentList[index] = WithRedColor(opList[i].m_content);
                    leftlist[index] = WithRedColor(leftlist[index]);
                    break;
            }
        }
        var leftStringBuilder = new StringBuilder();
        var rightStringBuilder = new StringBuilder();
        for(var i = 0; i < leftlist.Count; i++)
        {
            leftStringBuilder.AppendLine(i.ToString() + ":" + leftlist[i]);
        }
        m_leftContent = leftStringBuilder.ToString();
        for(var i = 0; i < rightContentList.Count; i++)
        {
            rightStringBuilder.AppendLine(rightContentList[i]);
        }
        m_rightContent = rightStringBuilder.ToString();
    }

    private string WithRedColor(string src)
    {
        return $"<color=#ff0000>{src}</color>";
    }

    private string WithBlueColor(string src)
    {
        return $"<color=#0000ff>{src}</color>";
    }

    private string WithGreenColor(string src)
    {
        return $"<color=#00ff00>{src}</color>";
    }

    private string WithWhiteColor(string src)
    {
        return $"<color=#ffffff>{src}</color>";
    }
}
