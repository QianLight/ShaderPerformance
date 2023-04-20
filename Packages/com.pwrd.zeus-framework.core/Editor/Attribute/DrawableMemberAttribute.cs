/*******************************************************************
* Copyright © 2017—2021 Perfect World.Co.Ltd. All rights reserved.
* author: 移动项目支持部 ZEUS FRAMEWORK TEAM
********************************************************************/
using System;
using Zeus;

namespace Zeus.Attributes
{
    //画笔类型
    public enum DrawerPencilType
    {
        None,
        ObjectPencil,
        IntPencil,
        FloatPencil,
        DoublePencil,
        StringPencil,
        PathPencil,
        ListPencil
    }

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
	public class DrawableMemberAttribute : Attribute
	{
        public string m_labelName;
        public DrawerPencilType m_PencilType;

        public DrawableMemberAttribute()
        {
        }

        public DrawableMemberAttribute(string labelName,DrawerPencilType pencilType)
        {
            m_labelName = labelName;
            m_PencilType = pencilType;
        }
    }
}
