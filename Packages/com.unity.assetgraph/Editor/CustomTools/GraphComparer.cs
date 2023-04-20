using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine.AssetGraph;
using Model=UnityEngine.AssetGraph.DataModel.Version2;

public class GraphComparerUtility
{
    public static string[] PrettyLines(string[] allLines)
    {
        bool modified = false;
        for(var i = 0; i < allLines.Length; i++)
        {
            var line = allLines[i];
            var header = Model.Settings.BASE64_IDENTIFIER;
            var headerindex = line.IndexOf(header);
            if(headerindex >= 0)
            {
                var encodedStr = line.Substring(headerindex, line.Length - headerindex).Replace("\"", "");
                var json =  CustomScriptUtility.DecodeString(encodedStr);
                var node = NiceJson.JsonNode.ParseJsonString(json);
                var prettyString = node.ToJsonPrettyPrintString();
                allLines[i] = line.Replace(encodedStr, prettyString);
                modified = true;
            }
        }
        var sb = new StringBuilder();
        for(var i = 0; i < allLines.Length; i++)
        {
            sb.AppendLine(allLines[i]);
        }
        var result = sb.ToString();
        allLines = result.Split('\n');
        if(modified)
        {
            return PrettyLines(allLines);
        }
        return allLines;
    }

    public static string ConvertLineEndings(string src)
    {
        return src.Replace("\r\n", "\n");
    }

    public static void ConvertLineEndings(string[] src)
    {
        for(var i = 0; i<src.Length; i++)
        {
            src[i] = src[i].Replace("\r\n", "\n");
        }
    }

    public static string[] PrettyGraph(string graphPath)
    {
        var allLines = File.ReadAllLines(graphPath);
        ConvertLineEndings(allLines);
        return PrettyLines(allLines);
    }

    public static void ProcessMatchInfos(Dictionary<int, MatchInfo> matchInfos)
    {
        if(matchInfos.Count > 0)
        {
            List<MatchInfo> processedMatchInfos = new List<MatchInfo>();
            List<MatchInfo> crossedMatchInfos = new List<MatchInfo>();
            List<int> indicesToRemove = new List<int>();
            foreach(var matchInfoPair in matchInfos)
            {
                var crossLineCount = 0;
                for(var i = 0; i < processedMatchInfos.Count; i++)
                {
                    if(processedMatchInfos[i].m_startLineIndexRight > matchInfoPair.Value.m_startLineIndexRight)
                    {
                        crossedMatchInfos.Add(processedMatchInfos[i]);
                        crossLineCount += processedMatchInfos[i].m_matchCount;
                    }
                }
                if(crossLineCount > matchInfoPair.Value.m_matchCount)
                {
                    indicesToRemove.Add(matchInfoPair.Key);
                }
                else
                {
                    for(var i = 0; i < crossedMatchInfos.Count; i++)
                    {
                        indicesToRemove.Add(crossedMatchInfos[i].m_startLineIndexLeft);
                    }
                }
                crossedMatchInfos.Clear();
                processedMatchInfos.Add(matchInfoPair.Value);
            }
            for(var i = 0; i < indicesToRemove.Count; i++)
            {
                matchInfos.Remove(indicesToRemove[i]);
            }
        }
    }

    public static void CalculateMinEidtDistance()
    {

    }

    internal enum OpType
    {
        Insert,
        Delete,
        Replace,
    }

    internal class Op
    {
        public OpType m_opType;
        public string m_content;
        public int m_index;
    }

    internal static List<Op> EditList(List<string> left, List<string> right)
    {
        var dp = new int[left.Count + 1, right.Count + 1];
        for (var i = 0; i < left.Count + 1; i++)
        {
            dp[i, 0] = i;
        }
        for (var i = 0; i < right.Count + 1; i++)
        {
            dp[0, i] = i;
        }
        for (var i = 1; i < dp.GetLength(0); i++)
        {
            for (var j = 1; j < dp.GetLength(1); j++)
            {
                var last = Math.Min(dp[i - 1, j - 1], Math.Min(dp[i - 1, j], dp[i, j - 1]));
                if (left[i - 1] == right[j - 1])
                {
                    dp[i, j] = last;
                }
                else
                {
                    dp[i, j] = last + 1;
                }
            }
        }
        //var stringBuilder = new StringBuilder();
        //for(var i = 0; i < dp.GetLength(0); i++)
        //{
        //    for(var j = 0; j < dp.GetLength(1); j++)
        //    {
        //        stringBuilder.Append(dp[i, j]);
        //        stringBuilder.Append(",");
        //    }
        //    stringBuilder.Append("\n");
        //}
        //UnityEngine.Debug.Log(stringBuilder.ToString());
        //File.WriteAllText("d:/dp.csv", stringBuilder.ToString());
        var list = new List<Op>();
        for (int x = dp.GetLength(0) - 1, y = dp.GetLength(1) - 1; x > 0 || y > 0;)
        {
            if (x > 0 && y > 0 && dp[x, y] == dp[x - 1, y - 1] + 1)
            {
                //stringBuilder.AppendLine($"replace {left[x - 1]} to {right[y - 1]}");
                list.Add(new Op
                {
                    m_index = x - 1,
                    m_opType = OpType.Replace,
                    m_content = right[y - 1],
                });
                x -= 1;
                y -= 1;
            }
            else if (y > 0 && dp[x, y] == dp[x, y - 1] + 1)
            {
                //stringBuilder.AppendLine($"insert {right[y - 1]}");
                list.Add(new Op
                {
                    m_index = x,
                    m_opType = OpType.Insert,
                    m_content = right[y - 1],
                });
                y -= 1;
            }
            else if (x > 0 && dp[x, y] == dp[x - 1, y] + 1)
            {
                //stringBuilder.AppendLine($"delete {left[x - 1]}");
                list.Add(new Op
                {
                    m_index = x - 1,
                    m_opType = OpType.Delete,
                    m_content = left[x - 1],
                });
                x -= 1;
            }
            if (x > 0 && y > 0 && dp[x, y] == dp[x - 1, y - 1])
            {
                x -= 1;
                y -= 1;
            }
            else if(x > 0 && y > 0 && dp[x, y] == dp[x, y - 1])
            {
                y -= 1;
            }
            else if(x > 0 && y > 0 && dp[x, y] == dp[x - 1, y])
            {
                x -= 1;
            }
        }
        return list;
    }

    public static void CollectEqualInfos(string[] leftLines, string[] rightLines, Dictionary<int, int> leftEqualMap, Dictionary<int, int> rightEqualMap, SortedDictionary<int, MatchInfo> leftMatchInfos)
    {
        leftEqualMap.Clear();
        rightEqualMap.Clear();
        var lastMathIndex = 0;
        MatchInfo lastMathInfo = null;
        for(var i = 0; i < leftLines.Length; i++)
        {
            var leftStart = i;
            for(var j = lastMathIndex; j < rightLines.Length; j++)
            {
                while(leftLines[i++] == rightLines[j++] && i < leftLines.Length && j < rightLines.Length)
                {
                    leftEqualMap[i] = j;
                    if(!leftEqualMap.ContainsKey(leftStart))
                    {
                        lastMathInfo = leftMatchInfos[leftStart] = new MatchInfo { m_matchCount = 1, m_startLineIndexRight = j, m_startLineIndexLeft = i };
                    }
                    else
                    {
                        lastMathInfo.m_matchCount += 1;
                    }
                }
                lastMathIndex = j;
                break;
            }
        }
    }
}
