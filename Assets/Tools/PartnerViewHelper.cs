#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.CFUI; 

public class PartnerViewHelper : MonoBehaviour
{
    [ContextMenu("PrintColor")]
    public void PrintColor()
    {
        string color = string.Empty;
        try
        {
            color += ColorUtility.ToHtmlStringRGB(this.transform.Find("Color1_0").GetComponent<CFImage>().color);
            color += "=" + ColorUtility.ToHtmlStringRGB(this.transform.Find("Logo").GetComponent<CFRawImage>().color);
            color += "=" + ColorUtility.ToHtmlStringRGB(this.transform.Find("Color3").GetComponent<CFImage>().color);
            color += "=" + ColorUtility.ToHtmlStringRGB(this.transform.Find("Color4").GetComponent<CFRawImage>().color);
            GUIUtility.systemCopyBuffer = color;

            Debug.Log(color);
        }
        catch (System.Exception e)
        {
            Debug.LogError("发生错误，请检查 Color1_0,Logo,Color3,Color4节点和其身上挂载的CFImage组件是否存在");
        }
    }

}
#endif