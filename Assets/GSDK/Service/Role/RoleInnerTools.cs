using System.Collections.Generic;
using GMSDK;

namespace GSDK
{
    public class RoleInnerTools
    {
        public static Result ConvertRoleError(CallbackResult result)
        {
            if (result.code == 0)
            {
                return new Result( 0, result.message);
            }

            int code = result.code;
            string msg = result.message;
            int extraCode = result.extraErrorCode;
            string extraMsg = result.extraErrorMessage;
            string additionalInfo = result.additionalInfo;
            return new Result(code, msg,extraCode,extraMsg,additionalInfo);
        }
        
        public static List<ZoneInfo> Convert(GMSDK.ZonesListResult listRet)
        {
            List<ZoneInfo> zoneInfos = new List<ZoneInfo>();
            if (listRet.zoneList != null)
            {
                foreach (var oldZone in listRet.zoneList)
                {
                    zoneInfos.Add(Convert(oldZone));
                }
            }
            return zoneInfos;
        }

        public static ZoneInfo Convert(GMSDK.GMZoneModel zoneModel)
        {
            ZoneInfo zoneInfo = new ZoneInfo();
            zoneInfo.ZoneName = zoneModel.zoneName;
            zoneInfo.ZoneID = zoneModel.zoneId;
            zoneInfo.ChannelID = zoneModel.channelId;
            zoneInfo.ExtraInfo = zoneModel.extraInfo;
            zoneInfo.ExtraKV = zoneModel.extraKV;

            List<ServerInfo> serverInfos = new List<ServerInfo>();
            if (zoneModel.servers != null)
            {
                foreach (var oldServer in zoneModel.servers)
                {
                    serverInfos.Add(Convert(oldServer));
                }
            }
            zoneInfo.Servers = serverInfos;
            
            return zoneInfo;
        }

        public static ServerInfo Convert(GMSDK.GMServerModel serverModel)
        {
            ServerInfo serverInfo = new ServerInfo();
            serverInfo.ServerId = serverModel.serverId;
            serverInfo.ServerName = serverModel.serverName;
            serverInfo.ServerType = ConvertServerType(serverModel.serverType);
            serverInfo.ServerEntry = serverModel.serverEntry;
            serverInfo.ServerStatus = ConvertOpStatus(serverModel.opStatus);
            serverInfo.RealServerId = serverModel.realServerId;
            serverInfo.IsMerged = serverModel.isMerged;
            serverInfo.OnlineLoad = ConvertOnlineLoad(serverModel.onlineLoad);
            serverInfo.ExtraInfo = serverModel.extraInfo;
            serverInfo.OpenTimestamp = serverModel.openTimestamp;
            serverInfo.ExtraKV = serverModel.extraKV;
            serverInfo.PingAddr = serverModel.pingAddr;
            serverInfo.Time = serverModel.time;
            
            List<ServerTag> serverTags = new List<ServerTag>();
            if (serverModel.tags != null)
            {
                foreach (var oldTag in serverModel.tags)
                {
                    serverTags.Add(Convert(oldTag));
                }
            }
            serverInfo.Tags = serverTags;
            
            return serverInfo;
        }

        public static ServerType ConvertServerType(int oldServerType)
        {
            switch (oldServerType)
            {
                case 1:
                    return ServerType.Formal;
                case 2:
                    return ServerType.Experience;
                case 3:
                    return ServerType.Audit;
                case 4:
                    return ServerType.Test;
                default:
                    return ServerType.None;
            }
        }

        public static ServerStatus ConvertOpStatus(int oldOpStatus)
        {
            switch (oldOpStatus)
            {
                case 1:
                    return ServerStatus.Online;
                case 2:
                    return ServerStatus.InMaintenance;
                case 3:
                    return ServerStatus.Offline;
                default:
                    return ServerStatus.None;
            }
        }

        public static ServerOnlineLoad ConvertOnlineLoad(int oldOnlineLoad)
        {
            switch (oldOnlineLoad)
            {
                case 1:
                    return ServerOnlineLoad.Idle;
                case 2:
                    return ServerOnlineLoad.Busy;
                case 3:
                    return ServerOnlineLoad.Full;
                case 9:
                    return ServerOnlineLoad.Auto;
                default:
                    return ServerOnlineLoad.Auto;
            }
        }

        public static ServerTag Convert(GMSDK.GMServerTagModel serverTagModel)
        {
            ServerTag serverTag = new ServerTag();
            serverTag.TagName = serverTagModel.tagName;
            serverTag.TagValue = serverTagModel.tagValue;
            return serverTag;
        }

        public static List<RoleInfo> Convert(GMSDK.RolesListResult rolesListRet)
        {
            List<RoleInfo> roleInfos = new List<RoleInfo>();
            if (rolesListRet.roleList != null)
            {
                foreach (var oldRole in rolesListRet.roleList)
                {
                    roleInfos.Add(Convert(oldRole));
                }
            }
            return roleInfos;
        }
        
        public static RoleInfo Convert(GMSDK.GMRoleModel roleModel)
        {
            RoleInfo roleInfo = new RoleInfo();
            roleInfo.RoleId = roleModel.roleId;        
            roleInfo.RoleName = roleModel.roleName;
            roleInfo.RoleLevel = roleModel.roleLevel;     
            roleInfo.RealServerId = roleModel.realServerId;     
            roleInfo.ServerName = roleModel.serverName;    
            roleInfo.LoginTime = roleModel.loginTime;       
            roleInfo.AvatarUrl = roleModel.avatarUrl;
            roleInfo.Job = roleModel.job;           
            roleInfo.ExtraInfo = roleModel.extra;

            return roleInfo;
        }

        public static List<ZoneInfo> Convert(List<GMSDK.GMZoneModel> zoneModels)
        {
            List<ZoneInfo> zoneInfos = new List<ZoneInfo>();
            if (zoneModels != null)
            {
                foreach (var oldZoneModel in zoneModels)
                {
                    zoneInfos.Add(Convert(oldZoneModel));
                }
            }

            return zoneInfos;
        }

        public static List<RoleInfo> Convert(List<GMSDK.GMRoleModel> roleModels)
        {
            List<RoleInfo> roleInfos = new List<RoleInfo>();
            if (roleModels != null)
            {
                foreach (var oldRoleModel in roleModels)
                {
                    roleInfos.Add(Convert(oldRoleModel));
                }
            }

            return roleInfos;
        }

        
        // ping服务器需要用到GMServerModel，所以需要有个转换回去的函数
        public static List<GMSDK.GMServerModel> Convert(List<ServerInfo> infos)
        {
            List<GMSDK.GMServerModel> retList = new List<GMSDK.GMServerModel>();
            if (infos != null)
            {
                foreach (var info in infos)
                {
                    retList.Add(Convert(info));
                }
            }

            return retList;
        }

        // ping服务器需要用到GMServerModel，所以需要有个转换回去的函数
        public static GMSDK.GMServerModel Convert(ServerInfo info)
        {
            GMSDK.GMServerModel model = new GMSDK.GMServerModel();
            model.serverId = info.ServerId;
            model.serverName = info.ServerName;
            model.serverType = (int)info.ServerType;
            model.serverEntry = info.ServerEntry;
            model.opStatus = (int)info.ServerStatus;
            model.realServerId = info.RealServerId;
            model.isMerged = info.IsMerged;
            model.onlineLoad = (int)info.OnlineLoad;
            model.extraInfo = info.ExtraInfo;
            model.openTimestamp = info.OpenTimestamp;
            model.extraKV = info.ExtraKV;
            model.pingAddr = info.PingAddr;
            model.time = info.Time;
            
            List<GMSDK.GMServerTagModel> serverTags = new List<GMSDK.GMServerTagModel>();
            if (info.Tags != null)
            {
                foreach (var tag in info.Tags)
                {
                    serverTags.Add(Convert(tag));
                }
            }
            model.tags = serverTags;
            
            return model;
        }

        public static GMSDK.GMServerTagModel Convert(ServerTag tag)
        {
            GMSDK.GMServerTagModel model = new GMSDK.GMServerTagModel();
            model.tagName = tag.TagName;
            model.tagValue = tag.TagValue;
            return model;
        }
    }
}