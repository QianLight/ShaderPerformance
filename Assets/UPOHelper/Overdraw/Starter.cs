#if ENABLE_UPO && ENABLE_UPO_OVERDRAW
using UnityEngine;

public class Starter : MonoBehaviour
{
    // Start is called before the first frame update
    // void Start()
    // {
    //     Debug.Log("overdraw start");
    //     MyOverdraw a = MyOverdraw.Instance;
    // }

    #if UNITY_2018_2_OR_NEWER
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    public static void OnStartGame()
    {
        Debug.Log("overdraw start");
        MyOverdraw a = MyOverdraw.Instance;
    }
    #endif
    
}
#endif