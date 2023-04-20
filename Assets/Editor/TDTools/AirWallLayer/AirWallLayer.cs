using UnityEngine;
using UnityEditor;

public class AirwallLayer:MonoBehaviour
{
    [MenuItem("GameObject/TDTools/�ؿ���ع���/AriwallLayer", false, 0)]
    static void test()
    {
        Transform[] AirwallLayers = Selection.activeTransform.GetComponentsInChildren<Transform>();
        foreach (Transform child in AirwallLayers)
        {
            if(child.gameObject.layer == LayerMask.NameToLayer("Default")&&child.gameObject.GetComponent<Collider>())
            {
                child.gameObject.layer = 25;
            }
        }
    }
}


