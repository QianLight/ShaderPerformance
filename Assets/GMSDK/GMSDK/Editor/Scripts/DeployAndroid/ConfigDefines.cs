using System;
using System.Collections.Generic;

/// <summary>
/// SDK 全局配置
/// </summary>
public class AppConfig
{
    public string app_id = "";
    public string channel = "";
    public long gsdk_init_timeout = 30 * 1000;
    public bool device_register_optimization = false;
    public int device_register_retry_count = 100;
    public long gsdk_account_init_timeout = 5 * 1000;
    public string screen_orientation = "sensorLandscape";
    public string package_name = "";
    public string main_activity = "";
    public bool open_launch_trace;

    public string app_name = "";

    public bool is_debug = false;
  
    // public bool is_private;

    public string download_source = "";
   
    public bool has_splash;
    public int splash_interval;
    public string skin = "";
    public bool is_need_privacy_protection;
    public bool is_need_anti_addiction;
    public bool is_need_service_mouth;
    public bool is_need_visitor;
    public bool clean_app;
    public bool is_show_toast = true;

    public bool use_packet_channel = true;

    public ShareConfig share = new ShareConfig();

    public PushConfig push = new PushConfig();

    public ImConfig im = new ImConfig();

    public RTCConfig rtc = new RTCConfig();
    
    public LiveConfig live = new LiveConfig();
    public DeepLinkConfig deeplink = new DeepLinkConfig();
    public PangleConfig pangle = new PangleConfig();
    public ThanosConfig thanos = new ThanosConfig();
    public bool gsdk_request_cloud_no_db_default = false;
}

public class IronSourceConfig
{
    public bool enable = false;
    public string app_key = "";
}
public class ShareConfig
{
    public string panelId = "";

    public string dimAmount = "";

    public ShareKeys share_keys = new ShareKeys();

    public class ShareKeys
    {
        public string wechat = "";

        public string dingding = "";

        public string qq = "";

        public string douyin = "";

        public string tiktok = "";

        public string messenger = "";

        public WeiBoShare weibo = new WeiBoShare();

        public Twitter twitter = new Twitter();

    }

    public class WeiBoShare
    {
        public string key = "";

        public string direct_url = "";
    }

    public class Twitter
    {
        public string key = "";

        public string secret = "";
    }
}

public class PushConfig
{
    public bool enable = false;

    public string push_channel_id;

    public string push_channel_name;

    public string push_app_name;

    public string push_black_list;
}

public class ImConfig
{
    public bool enable = false;
    public int aid = 0;
	public int method = 0;
	public int service = 0;
	public string wsHostBoe = "";
	public string wsHost = "";
	public List<int> inboxes = null;
    
	public int fpid = 0;
	public string httpHost = "";
	public string httpHostBoe = "";
	public string appKey = "";
}

public class LiveConfig
{
    public bool enable = false;
    public string douyin_key;
}

public class DeepLinkConfig
{
    public bool enable;
    public int schemaListSize;
    public List<string> schemaList= new List<string>();
    public int delayMillis;
    public int hostListSize;
    public List<string> hostList= new List<string>();
    public string baseUrlForFission;
    public List<string> defaultTokenRegex = new List<string>();
}

public class RTCConfig
{
    public bool enable;
    public string rtcAppId;
}

public class PangleConfig
{
    public bool enable;
    public string key;
}

public class ThanosConfig
{
    public bool saveSubChannel = false;
}

public class BaseChannelConfig
{

}