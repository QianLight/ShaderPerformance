/*******************************************************************
* Copyright © 2017—2021 Perfect World.Co.Ltd. All rights reserved.
* author: 移动项目支持部 ZEUS FRAMEWORK TEAM
********************************************************************/
using System;


namespace Zeus
{
    public struct ActionExtra<TExtra>
    {
        public Action<TExtra> Dele;
        public TExtra Extra;

        public ActionExtra(Action<TExtra> dele, TExtra extra)
        {
            Dele = dele;
            Extra = extra;
        }

        public void Invoke()
        {
            Dele(Extra);
        }
    }


    public struct ActionExtra<TData, TExtra>
    {
        public Action<TData, TExtra> Dele;
        public TExtra Extra;

        public ActionExtra(Action<TData, TExtra> dele, TExtra extra)
        {
            Dele = dele;
            Extra = extra;
        }

        public void Invoke(TData data)
        {
            Dele(data, Extra);
        }
    }
}
