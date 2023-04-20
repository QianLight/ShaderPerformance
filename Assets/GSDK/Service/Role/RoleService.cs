using System.Collections.Generic;
using System;
using System.Collections;
using System.Runtime.InteropServices;
using UNBridgeLib.LitJson;
using UnityEngine;

namespace GSDK
{
    public class RoleService : IRoleService
    {
        private GMSDK.MainSDK gsdk = GMSDKMgr.instance.SDK;

       
        
        public void FetchZonesList(string gameVersion, FetchZonesListDelegate fetchZonesListCallback, double timeout = double.NaN, string extraInfo = "")
        {
#if UNITY_STANDALONE_WIN  && !GMEnderOn
            ZonesListCallback = fetchZonesListCallback;
            ServerRoleFetchZonesList(gameVersion, ZonesListFunc);
#else            
            IAccountService accountService = ServiceProvider.Instance.GetService(ServiceType.Account,"Bytedance") as IAccountService;
            string accessToken = accountService.GetLoginRecord().Token;
            if (string.IsNullOrEmpty(accessToken))
            {
                GLog.LogError("AccessToken is null or empty in RoleService");
                OnGetZoneListCallback(
                    new Result(ErrorCode.AccountAccessTokenInvalid, "AccessToken is null or empty in RoleService"),
                    null,
                    fetchZonesListCallback
                );
            }
            else
            {
                gsdk.SdkGetZonesList(accessToken, gameVersion, (GMSDK.ZonesListResult zoneListResult) =>
                {
                    if (zoneListResult == null)
                    {
                        return;
                    }
                    OnGetZoneListCallback(
                        RoleInnerTools.ConvertRoleError(zoneListResult),
                        RoleInnerTools.Convert(zoneListResult),
                        fetchZonesListCallback
                    );
                }, timeout, extraInfo);
            }
#endif 
        }

        void OnGetZoneListCallback(Result result, List<ZoneInfo> zones, FetchZonesListDelegate fetchZonesListCallback)
        {
            if (fetchZonesListCallback != null)
            {
                try
                {
                    fetchZonesListCallback(result, zones);
                }
                catch (Exception ex)
                {
                    GLog.LogException(ex);
                }
            }
        }


        public void FetchRolesList(FetchRolesListDelegate fetchRolesListCallback, double timeout = double.NaN)
        {
#if UNITY_STANDALONE_WIN  && !GMEnderOn
            RoleListCallback = fetchRolesListCallback;
            ServerRoleFetchRolesList(RoleListFunc);
#else
            
            IAccountService accountService = ServiceProvider.Instance.GetService(ServiceType.Account,"Bytedance") as IAccountService;
            string accessToken = accountService.GetLoginRecord().Token;
            if (string.IsNullOrEmpty(accessToken))
            {
                GLog.LogError("AccessToken is null or empty in RoleService");
                OnGetRolesListCallback(
                    new Result(ErrorCode.AccountAccessTokenInvalid, "AccessToken is null or empty in RoleService"),
                    null, 
                    fetchRolesListCallback);
            }
            else
            {
                gsdk.SdkGetRolesList(accessToken, (GMSDK.RolesListResult rolesListResult) =>
                {
                    if (rolesListResult == null)
                    {
                        return;
                    }
                    OnGetRolesListCallback(
                        RoleInnerTools.ConvertRoleError(rolesListResult),
                        RoleInnerTools.Convert(rolesListResult),
                        fetchRolesListCallback);
                }, timeout);
            }
#endif
        }

        void OnGetRolesListCallback(Result result, List<RoleInfo> roles, FetchRolesListDelegate fetchRolesListCallback)
        {
            if (fetchRolesListCallback != null)
            {
                try
                {
                    fetchRolesListCallback(result, roles);
                }
                catch (Exception ex)
                {
                    GLog.LogException(ex);
                }
            }
        }

       
        public void FetchZonesAndRolesList(string gameVersion, FetchZonesAndRolesListDelegate fetchZonesAndRolesListCallback, double timeout = double.NaN, string extraInfo = "")
        {
#if UNITY_STANDALONE_WIN  && !GMEnderOn
            ZonesAndRolesCallback = fetchZonesAndRolesListCallback;
            ServerRoleFetchZonesAndRolesList(gameVersion, ZonesAndRolesFunc);
#else            
            
            IAccountService accountService = ServiceProvider.Instance.GetService(ServiceType.Account,"Bytedance") as IAccountService;
            string accessToken = accountService.GetLoginRecord().Token;
            if (string.IsNullOrEmpty(accessToken))
            {
                GLog.LogError("AccessToken is null or empty in RoleService");
                OnGetZonesAndRolesListCallback(
                    new Result(ErrorCode.RoleUnknownError, "AccessToken is null or empty in RoleService"),
                    null,
                    null, 
                    fetchZonesAndRolesListCallback
                    );
            }
            else
            {
                gsdk.SdkGetServersAndRolesList(accessToken, gameVersion, (GMSDK.ServersAndRolesResult serversAndRolesResult) =>
                {
                    if (serversAndRolesResult == null)
                    {
                        return;
                    }
                    OnGetZonesAndRolesListCallback(
                        RoleInnerTools.ConvertRoleError(serversAndRolesResult),
                        RoleInnerTools.Convert(serversAndRolesResult.serverAndRole != null
                            ? serversAndRolesResult.serverAndRole.zones
                            : null),
                        RoleInnerTools.Convert(serversAndRolesResult.serverAndRole != null
                            ? serversAndRolesResult.serverAndRole.roles
                            : null),
                        fetchZonesAndRolesListCallback
                    );
                }, timeout, extraInfo);
            }
#endif
        }

        void OnGetZonesAndRolesListCallback(Result result, List<ZoneInfo> zones, List<RoleInfo> roles, FetchZonesAndRolesListDelegate fetchZonesAndRolesListCallback)
        {
            if (fetchZonesAndRolesListCallback != null)
            {
                try
                {
                    fetchZonesAndRolesListCallback(result, zones, roles);
                }
                catch (Exception ex)
                {
                    GLog.LogException(ex);
                }
            }
        }

        public void PingServerList(List<ServerInfo> servers, PingServerListDelegate pingServerListCallback, double timeout = double.NaN)
        {
            gsdk.SdkPingServerList(RoleInnerTools.Convert(servers), (GMSDK.PingServersResult pingServersResult) =>
            {
                if (pingServersResult == null)
                {
                    return;
                }
                OnPingServerListCallback(
                    RoleInnerTools.ConvertRoleError(pingServersResult), 
                    RoleInnerTools.Convert(pingServersResult.server),
                    pingServersResult.finish,
                    pingServerListCallback);
            }, timeout);
        }

        void OnPingServerListCallback(Result result, ServerInfo serverInfo, bool isFinished, PingServerListDelegate pingServerListCallback)
        {
            if (pingServerListCallback != null)
            {
                try
                {
                    pingServerListCallback(result, serverInfo, isFinished);
                }
                catch (Exception ex)
                {
                    GLog.LogException(ex);
                }
            }
        }
        
        
#if UNITY_STANDALONE_WIN && !GMEnderOn
        public FetchZonesListDelegate ZonesListCallback;
        [MonoPInvokeCallback(typeof(Callback2P))]
        private static void ZonesListFunc(string resultStr, string callbackStr)
        {
            Debug.Log("resultStr:" + resultStr + ",callbackStr:" + callbackStr);
            Result result = JsonMapper.ToObject<Result>(resultStr);
            List<ZoneInfo> zones = new List<ZoneInfo>();
            if (callbackStr != null && !callbackStr.Equals("null"))
            {
                JsonData jsonData = JsonMapper.ToObject(callbackStr);
                for (int i = 0; i < jsonData.Count; i++)
                {
                    string zonestr = JsonMapper.ToJson(jsonData[i]);
                    Debug.Log("zonestr " + zonestr);
                    ZoneInfo zone = JsonMapper.ToObject<ZoneInfo>(zonestr);
                    zones.Add(zone);
                }
            }
            RoleService roleService = Role.Service as RoleService;
            if (roleService.ZonesListCallback != null)
            {
                roleService.ZonesListCallback(result, zones);
            }
        }
        
        public FetchRolesListDelegate RoleListCallback;
        [MonoPInvokeCallback(typeof(Callback2P))]
        private static void RoleListFunc(string resultStr, string callbackStr)
        {
            Debug.Log("resultStr:" + resultStr + ",callbackStr:" + callbackStr);
            Result result = JsonMapper.ToObject<Result>(resultStr);
            List<RoleInfo> roles = new List<RoleInfo>();
            if (callbackStr != null && !callbackStr.Equals("null"))
            {
                JsonData jsonData = JsonMapper.ToObject(callbackStr);
                for (int i = 0; i < jsonData.Count; i++)
                {
                    string rolestr = JsonMapper.ToJson(jsonData[i]);
                    Debug.Log("rolestr " + rolestr);
                    RoleInfo role = JsonMapper.ToObject<RoleInfo>(rolestr);
                    roles.Add(role);
                }
            }

            RoleService roleService = Role.Service as RoleService;
            if (roleService.RoleListCallback != null)
            {
                roleService.RoleListCallback(result, roles);
            }
        }
        public FetchZonesAndRolesListDelegate ZonesAndRolesCallback;
        [MonoPInvokeCallback(typeof(Callback3P))]
        private static void ZonesAndRolesFunc(string resultStr, string zonesStr, string rolesStr)
        {
            Debug.Log("resultStr:" + resultStr + ",zonesStr:" + zonesStr + ", rolestr:" + rolesStr);
            Result result = JsonMapper.ToObject<Result>(resultStr);
            List<ZoneInfo> zones = new List<ZoneInfo>();
            if (zonesStr != null && !zonesStr.Equals("null"))
            {
                JsonData jsonData = JsonMapper.ToObject(zonesStr);
                for (int i = 0; i < jsonData.Count; i++)
                {
                    string zonestr = JsonMapper.ToJson(jsonData[i]);
                    Debug.Log("zonestr " + zonestr);
                    ZoneInfo zone = JsonMapper.ToObject<ZoneInfo>(zonestr);
                    zones.Add(zone);
                }
            }

            List<RoleInfo> roles = new List<RoleInfo>();
            if (rolesStr != null && !rolesStr.Equals("null"))
            {
                JsonData roleJsonData = JsonMapper.ToObject(rolesStr);
                for (int i = 0; i < roleJsonData.Count; i++)
                {
                    string rolestr = JsonMapper.ToJson(roleJsonData[i]);
                    Debug.Log("rolestr " + rolestr);
                    RoleInfo role = JsonMapper.ToObject<RoleInfo>(rolestr);
                    roles.Add(role);
                }
            }
            RoleService roleService = Role.Service as RoleService;
            if (roleService.ZonesAndRolesCallback != null)
            {
                roleService.ZonesAndRolesCallback(result, zones, roles);
            }
        }
        
        
        [DllImport(PluginName.GSDK)]
        private static extern void ServerRoleFetchZonesList(string gameVersion, Callback2P callback);

        [DllImport(PluginName.GSDK)]
        private static extern void ServerRoleFetchRolesList(Callback2P callback);
        
        [DllImport(PluginName.GSDK)]
        private static extern void ServerRoleFetchZonesAndRolesList(string gameVersion, Callback3P callback);
        
#endif        
    }
}