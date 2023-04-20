using System;
using System.Collections;
using System.Collections.Generic;

namespace GSDK.RNU
{

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class ReactProp : Attribute
    {
        public string name;
        public double defaultDouble = 0.0;
        public float defaultFloat = 0.0f;
        public int defaultInt = 0;
        public long defaultLong = 0;
        public string defaultString = "";
        public bool defaultBoolean = false;
        public Hashtable defaultMap = new Hashtable();
        public ArrayList defaultArray = new ArrayList();
        public Dictionary<string, object> defaultDictionary = new Dictionary<string, object>();
    }
}