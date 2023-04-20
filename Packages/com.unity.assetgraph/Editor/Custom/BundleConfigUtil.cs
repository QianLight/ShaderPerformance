using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.AssetGraph;

public class ColumnFilter
{
    public string ColumnTitle;
    public Func<string, bool> Filter;
}

public enum ColumnSelectorFilterLink
{
    And,
    Or,
}

public class ColumnSelector
{
    public IEnumerable<ColumnFilter> Filters;
    public string Column;
    public ColumnSelectorFilterLink FilterLink = ColumnSelectorFilterLink.And;
}

public static class BundleConfigUtil
{
    public static HashSet<string> SelectIds(Dictionary<string, Dictionary<string, List<string>>> allColumnConfigs,
        string table, IEnumerable<ColumnFilter> columnFilters)
    {
        var rmIndex = new HashSet<int>();
        var columnConfig = allColumnConfigs[table];
        foreach (var columnFilter in columnFilters)
        {
            var title = columnFilter.ColumnTitle;
            var filter = columnFilter.Filter;
            var columnVals = columnConfig[title];
            for (var i = 0; i < columnVals.Count; i++)
            {
                if (null != filter && !filter(columnVals[i]))
                {
                    rmIndex.Add(i);
                }
            }
        }
        var idColumn = columnConfig.Values.First();
        var result = new HashSet<string>();
        for (var i = 0; i < idColumn.Count; i++)
        {
            if (rmIndex.Contains(i))
            {
                continue;
            }
            result.Add(idColumn[i]);
        }
        return result;
    }

    public static HashSet<string> SelectVals(Dictionary<string, Dictionary<string, List<string>>> allColumnConfigs,
        string table, ColumnSelector selector, IEnumerable<string> rows = null)
    {
        var columnConfig = allColumnConfigs[table];
        var filters = selector.Filters;
        var rmIndices = new HashSet<int>();
        var idColumn = columnConfig.Values.First();
        if(null != rows)
        {
            for(var i = 0; i < idColumn.Count; i++)
            {
                if(!rows.Contains(idColumn[i]))
                {
                    rmIndices.Add(i);
                }
            }
        }
        if (null != filters)
        {
            switch (selector.FilterLink)
            {
                case ColumnSelectorFilterLink.And:
                    foreach (var filter in filters)
                    {
                        if (!columnConfig.TryGetValue(filter.ColumnTitle, out var column))
                        {
                            Debug.LogError($"{filter.ColumnTitle} doesn't exist in {table}");
                            return null;
                        }
                        for (var i = 0; i < column.Count; i++)
                        {
                            if (null != filter.Filter && !filter.Filter(column[i]))
                            {
                                rmIndices.Add(i);
                            }
                        }
                    }
                    break;
                case ColumnSelectorFilterLink.Or:
                    var baseRmIndices = new HashSet<int>();
                    bool isBase = true;
                    foreach (var filter in filters)
                    {
                        if (!columnConfig.TryGetValue(filter.ColumnTitle, out var column))
                        {
                            Debug.LogError($"{filter.ColumnTitle} doesn't exist in {table}");
                            return null;
                        }
                        for (var i = 0; i < column.Count; i++)
                        {
                            if (null != filter.Filter && !filter.Filter(column[i]))
                            {
                                if(isBase)
                                {
                                    baseRmIndices.Add(i);
                                }
                            }
                            else
                            {
                                baseRmIndices.Remove(i);
                            }
                        }
                        isBase = false;
                    }
                    foreach(var id in baseRmIndices)
                    {
                        rmIndices.Add(id);
                    }
                    break;
            }
        }
        var selectedColumn = columnConfig[selector.Column];
        var result = new HashSet<string>();
        for (var i = 0; i < selectedColumn.Count; i++)
        {
            if (rmIndices.Contains(i))
            {
                continue;
            }
            result.Add(selectedColumn[i]);
        }
        return result;
    }

    public static HashSet<string> SelectVals(Dictionary<string, Dictionary<string, List<string>>> allColumnConfigs,
        string table, IEnumerable<string> rows, IEnumerable<string> columns, Func<string, bool> filter = null)
    {
        var list = new HashSet<string>();
        var columnConfig = allColumnConfigs[table];
        var idColumn = columnConfig.Values.First();
        foreach (var column in columns)
        {
            if (!columnConfig.TryGetValue(column, out var columnVals))
            {
                Debug.LogError($"{column} doesn't exist in {table}");
                return null;
            }
            for (var rowIndex = 0; rowIndex < idColumn.Count; rowIndex++)
            {
                if (rows.Contains(idColumn[rowIndex]))
                {
                    if (rowIndex > columnVals.Count - 1)
                    {
                        Debug.LogError($"selectVals from {table} failed: illegal rowIndex {rowIndex} for columnVals with count {columnVals.Count} in {column}");
                        return null;
                    }
                    var val = columnVals[rowIndex];
                    if (null == filter || filter(val))
                    {
                        list.Add(val);
                    }
                }
            }
        }
        return list;
    }

    public static void ExtractSharedAssets(Dictionary<string, HashSet<string>> groupAssetMap)
    {
        var assetGroupMap = new Dictionary<string, List<string>>();
        foreach (var pair in groupAssetMap)
        {
            foreach (var asset in pair.Value)
            {
                if (!assetGroupMap.TryGetValue(asset, out var list))
                {
                    assetGroupMap[asset] = list = new List<string>();
                }
                if (!list.Contains(pair.Key))
                {
                    list.Add(pair.Key);
                }
            }
        }
        var sharedSet = new HashSet<string>();
        foreach (var assetGroupPair in assetGroupMap)
        {
            var asset = assetGroupPair.Key;
            var groups = assetGroupPair.Value;
            //只在一个bundle里的不处理
            if (groups.Count < 2)
            {
                continue;
            }
            var sharedAssetKey = "Shared";
            sharedSet.Clear();
            //在多个Bundle里，提出来
            foreach (var group in groups)
            {
                if (!groupAssetMap[group].Remove(asset))
                {
                    Debug.LogError($"no asset {asset} in group {group}");
                }
                sharedAssetKey += "_" + group;
            }
            if (!groupAssetMap.TryGetValue(sharedAssetKey, out var list))
            {
                //groupAssetMap["shared"+StringToHash(sharedAssetKey)] = list = new List<string>();
                groupAssetMap[sharedAssetKey] = list = new HashSet<string>();
            }
            list.Add(asset);
        }
    }
    public static string StringToHash(string input)
    {
        return StringUtility.GetHashCode(input).ToString("X4").ToLower();
    }

    public static string SerializeConfigDic(Dictionary<string, Dictionary<string, List<string>>> columnConfigs)
    {
        var configs = new TableConfigs();
        var tableConfigList = new List<TableConfig>();
        foreach (var tablePair in columnConfigs)
        {
            var config = new TableConfig();
            config.TableName = tablePair.Key;
            config.Items = new List<TableConfigItem>();
            foreach (var columnPair in tablePair.Value)
            {
                var item = new TableConfigItem();
                item.ColumnTitle = columnPair.Key;
                item.ColumnVals = columnPair.Value;
                config.Items.Add(item);
            }
            tableConfigList.Add(config);
        }
        configs.Items = tableConfigList;
        return JsonUtility.ToJson(configs);
    }

    public static Dictionary<string, Dictionary<string, List<string>>> DeserializeConfigDic(string json)
    {
        var result = new Dictionary<string, Dictionary<string, List<string>>>();
        var tableConfigs = JsonUtility.FromJson<TableConfigs>(json);
        foreach (var tableConfig in tableConfigs.Items)
        {
            var columnDic = new Dictionary<string, List<string>>();
            result[tableConfig.TableName] = columnDic;
            foreach (var item in tableConfig.Items)
            {
                columnDic[item.ColumnTitle] = item.ColumnVals;
            }
        }
        return result;
    }

    public static void ParseTxtTable(string configFile, out Dictionary<string, List<string>> columnConfigs, int startLineIndex)
    {
        var lines = File.ReadAllLines(configFile);
        var titleLine = lines[0];
        var titles = titleLine.Split('\t');
        columnConfigs = new Dictionary<string, List<string>>();

        for (var i = 0; i < titles.Length; i++)
        {
            columnConfigs[titles[i]] = new List<string>();
        }

        for (var lineIndex = startLineIndex; lineIndex < lines.Length; lineIndex++)
        {
            var vals = lines[lineIndex].Split('\t');
            if(string.IsNullOrEmpty(vals[0]) || string.IsNullOrWhiteSpace(vals[0]) || vals[0].StartsWith("#"))
            {
                continue;
            }
            for (var i = 0; i < vals.Length; i++)
            {
                columnConfigs[titles[i]].Add(vals[i]);
            }
        }
    }
}

[Serializable]
public class TableConfigs
{
    public List<TableConfig> Items = new List<TableConfig>();
}

[Serializable]
public class TableConfig
{
    public string TableName;
    public List<TableConfigItem> Items = new List<TableConfigItem>();
}

[Serializable]
public class TableConfigItem
{
    public string ColumnTitle;
    public List<string> ColumnVals = new List<string>();
}

