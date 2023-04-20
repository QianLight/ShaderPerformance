/*******************************************************************
* Copyright © 2017—2021 Perfect World.Co.Ltd. All rights reserved.
* author: 移动项目支持部 ZEUS FRAMEWORK TEAM
********************************************************************/
using System;


namespace Zeus
{
	public class ZeusException : ApplicationException
	{
        public ZeusException() { }

	    public ZeusException(string msg)
	        : base(msg)
	    {
	    }
	}
}
