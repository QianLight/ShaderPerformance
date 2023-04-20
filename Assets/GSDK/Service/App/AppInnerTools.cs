using System.Collections.Generic;
using GMSDK;
namespace GSDK
{
    public class AppInnerTools
    {
        public static PermissionInfo ConvertPermissionResult(PermissionRequestResult permissionRequestResult)
        {
            PermissionInfo permissionInfo = new PermissionInfo();
            permissionInfo.GrantResults = new Dictionary<string, PermissionCode>();
            permissionInfo.IsAllGranted = permissionRequestResult.isAllGranted;
            for (int i = 0; i < permissionRequestResult.permissions.Count; i++)
            {
                permissionInfo.GrantResults[permissionRequestResult.permissions[i]] =
                    (PermissionCode) permissionRequestResult.grantResults[i];
            }

            return permissionInfo;
        }
    }
}