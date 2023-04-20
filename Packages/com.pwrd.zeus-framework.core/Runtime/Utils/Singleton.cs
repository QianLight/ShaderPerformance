/*******************************************************************
* Copyright © 2017—2021 Perfect World.Co.Ltd. All rights reserved.
* author: 移动项目支持部 ZEUS FRAMEWORK TEAM
********************************************************************/
using System;

namespace Zeus
{
    public class Singleton<T> where T : new()
    {
        public static T Instance
        {
            get { return InstanceCreator.instance; }
        }

        class InstanceCreator
        {
            InstanceCreator()
            {

            }
            internal static readonly T instance = new T();
        }

        protected Singleton()
        {
            if (Instance != null)
            {
                throw (new Exception(
                 "You have tried to create a new singleton class where you should have instanced it. Replace your \"new class()\" with \"class.Instance\""));
            }
        }
    }
}

