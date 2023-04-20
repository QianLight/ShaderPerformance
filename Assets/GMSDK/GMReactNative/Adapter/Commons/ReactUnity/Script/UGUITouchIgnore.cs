using UnityEngine;

public class UGUITouchIgnore : MonoBehaviour, ICanvasRaycastFilter
{
    public bool IsRaycastLocationValid(Vector2 screenPoint, Camera eventCamera)
    {

        return false;
    }
}
