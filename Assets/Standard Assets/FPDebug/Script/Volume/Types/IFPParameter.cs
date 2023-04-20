using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class IFPParameter
{
    public string ParaName;
    public string ValueString;

    public abstract object Value();
    public abstract bool OnGUI();
    public abstract string Object2String(object obj);
    public abstract void SetValue(string vlaueStr);
}
