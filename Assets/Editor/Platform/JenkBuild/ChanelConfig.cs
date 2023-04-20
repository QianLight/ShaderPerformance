using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.IO;


/// <summary>
/// 渠道配置信息
/// </summary>
[InitializeOnLoad]
public class ChanelConfig
{

    static ChanelConfig()
    {
        Add(TChanel.Internal, "10.253.17.49", 25001, "外网服");
        Add(TChanel.Out,   "180.167.106.237", 25001, "公网服");

        //add other channel login server here
    }


    public enum TChanel
    {
        /// <summary>
        /// 公司内部渠道
        /// </summary>
        Internal,

        /// <summary>
        /// 对外渠道 如OB(4G)
        /// </summary>
        Out,
    }


    public struct ChannelInfo
    {
        public string ip;
        public int port;
        public string desc;

        public ChannelInfo(string _ip, int _port,string _desc)
        {
            this.ip = _ip;
            this.port = _port;
            this.desc = _desc + " ip: " + ip + ":" + _port;
        }
    }


    static Dictionary<TChanel, ChannelInfo> channels = new Dictionary<TChanel, ChannelInfo>();

    
    public static string GetDesc(TChanel chanel)
    {
        return channels[chanel].desc;
    }

    static void Add(TChanel chanel, string ip, int port,string desc)
    {
        if (!channels.ContainsKey(chanel))
        {
            ChannelInfo info = new ChannelInfo(ip, port, desc);
            channels.Add(chanel, info);
        }
    }

    public static void Make(TChanel chanel)
    {
        string path = Path.Combine(Application.dataPath, "Resources/loginserver.txt");
        File.WriteAllText(path, channels[chanel].ip + ":" + channels[chanel].port);
    }

}
