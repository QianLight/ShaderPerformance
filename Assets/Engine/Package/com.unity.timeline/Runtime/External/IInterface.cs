using UnityEngine.Playables;


namespace UnityEngine.Timeline
{

    public interface IInterface
    {

        //GameObject CreateFromPrefab(string location, Vector3 position, Quaternion quaternion, bool usePool = true, bool dontDestroy = false);

        //void UnSafeDestroy(UnityEngine.Object o, bool returnPool = true, bool destroyImm = false);
        
        INotificationReceiver FetchGlobalReceiver();

    }

}