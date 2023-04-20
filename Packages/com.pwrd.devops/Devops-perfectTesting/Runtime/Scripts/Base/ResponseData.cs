using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResponseData 
{

}


public class ResponseConfigData
{
    public string env = "dev";//dev/test/pub
    [SerializeField]
    public CaseNetList data = new CaseNetList();
}


public class ResponseGetData
{
    public string env = "dev";//dev/test/pub
}