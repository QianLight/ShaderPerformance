/*******************************************************************
* Copyright © 2017—2022 Perfect World.Co.Ltd. All rights reserved.
* author: 移动项目支持部 ZEUS FRAMEWORK TEAM
********************************************************************/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;


public class MemoryElement : IComparable<MemoryElement>
{
    public string name;
    public long totalMemory;
    public List<MemoryElement> children = new List<MemoryElement>();

    private MemoryElement()
    {
    }

    public static MemoryElement Create(Reflector srcMemoryElement)
    {
        if (srcMemoryElement == null) return null;
        var dstMemoryElement = new MemoryElement();
        Reflector.CopyFrom(dstMemoryElement, srcMemoryElement.InnerObject,
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.GetField);

        var srcChildren = srcMemoryElement.PublicInstanceField<IList>("children");
        if (srcChildren == null) return dstMemoryElement;
        foreach (var srcChild in srcChildren)
        {
            var memoryElement = Create(new Reflector(srcChild));
            dstMemoryElement.children.Add(memoryElement);
        }
        dstMemoryElement.children.Sort();
        return dstMemoryElement;
    }


    public override string ToString()
    {
        var text = string.IsNullOrEmpty(name) ? "-" : name;
        var text2 = "KB";
        var num = totalMemory / 1024f;
        if (num > 512f)
        {
            num /= 1024f;
            text2 = "MB";
        }

        var resultString = $" {text}\t{num}{text2}";
        return resultString;
    }

    public MemoryElement GetChildByName(string target)
    {
        foreach (var child in children)
        {
            if (child.name == target)
            {
                return child;
            }
        }
        return null;
    }

    public int CompareTo(MemoryElement other)
    {
        if (other.totalMemory != totalMemory)
        {
            return (int) (other.totalMemory - totalMemory);
        }

        if (string.IsNullOrEmpty(name) && string.IsNullOrEmpty(other.name))
        {
            return 0;
        }
        if (string.IsNullOrEmpty(name)) return -1;
        return !string.IsNullOrEmpty(other.name) ? string.Compare(name, other.name, StringComparison.Ordinal) : 1;

    }
}