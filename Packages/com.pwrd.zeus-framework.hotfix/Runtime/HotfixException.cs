/*******************************************************************
* Copyright © 2017—2021 Perfect World.Co.Ltd. All rights reserved.
* author: 移动项目支持部 ZEUS FRAMEWORK TEAM
********************************************************************/
using System;
namespace Zeus.Framework.Hotfix
{
    public class HotfixException : Exception
    {
        public HotfixError errorType = HotfixError.Exception;
        public object param;
        
        public HotfixException()
        {

        }

        public HotfixException(string message) : base(message)
        {

        }

        public HotfixException(HotfixError error)
        {
            errorType = error;
        }

        public HotfixException(string message, HotfixError errorType) : base(message)
        {
            this.errorType = errorType;
        }

        public HotfixException(string message, HotfixError errorType,object param) : base(message)
        {
            this.errorType = errorType;
            this.param = param;
        }

        public override string Message
        {
            get
            {
                return "[" + errorType.ToString() + "] " + base.Message;
            }
        }

        public override string ToString()
        {
            return Message;
        }
    }


}


