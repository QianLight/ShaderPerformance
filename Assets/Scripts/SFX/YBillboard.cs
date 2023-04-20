//not use any more


// using UnityEngine;
// using CFUtilPoolLib;

// public class YBillboard : MonoBehaviour
// {
//     private IAssociatedCamera _camera = null;
//     private Transform cacheTrans = null;
//     private Transform cacheCameraTrans = null;
//     public static bool IsUpdate = true;

//     public bool UsePlayerDirection = false;
//     // Use this for initialization
//     //void Awake () {
//     //       cacheTrans = this.transform;
//     //       if (Camera.main != null)
//     //           cacheCameraTrans = Camera.main.transform;
//     //   }

//     // Update is called once per frame
//     void LateUpdate()
//     {
//         if (cacheTrans == null)
//             cacheTrans = this.transform;
//         if (_camera == null || _camera.Deprecated) _camera = XInterfaceMgr.singleton.GetInterface<IAssociatedCamera>(XCommon.singleton.XHash("IAssociatedCamera"));
//         if (cacheCameraTrans == null && _camera != null)
//         {
//             Camera c = _camera.Get();
//             if (c != null)
//                 cacheCameraTrans = c.transform;
//         }

// 		IsUpdate = XInterfaceMgr.IsBillboardUpdate;

// 		if (IsUpdate && cacheCameraTrans != null && cacheTrans != null)
//         {
//             Vector3 eularAngle = cacheTrans.rotation.eulerAngles;
//             Vector3 targetEularAngle = Vector3.zero;
//             if(!UsePlayerDirection)
//             {
//                 targetEularAngle = cacheCameraTrans.rotation.eulerAngles;
//             }
//             else
//             {
//                 targetEularAngle = _camera.PlayerDir();
//             }

//             cacheTrans.rotation = Quaternion.Euler(eularAngle.x, targetEularAngle.y, eularAngle.z);
            
//         }
//     }
// }
