using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using GSDK;
using UNBridgeLib.LitJson;
using UnityEditor;
using UnityEngine;

public abstract class BaseSDKConfigSetting : ScriptableObject
{
    public static string SDKSettingConfigName = "sdkDomesticConfig.json"; //新json文件name
    protected const string IOSOldSettingConfigName = "ConfigSettings.json"; //ios原json文件name
    private const string SDKSettingConfigNameGSDKHub = "sdkDomesticConfigHub.json";  //GSDK-Hub下发的json
    protected const string AndroidOldSettingConfigName = "domesticConfig.json"; //andorid原json文件name
    

    protected static GMSDKEnv env = GMSDKEnv.Instance;
    public SDKConfigModule _sdkConfigModule;
    private static BaseSDKConfigSetting _instance;
    private static string SAVE_CONFIG_FILE = Path.Combine(env.PATH_CONFIG_SETTINGS, SDKSettingConfigName);
    
    protected AppConfig androidAppConfig = null;
    protected ConfigModel iOSConfigModel = null;

    public static BaseSDKConfigSetting Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = ScriptableObject.CreateInstance<DomesticConfigSetting>();
            }

            return _instance;
        }
        set
        {
            _instance = value;
        }
    }

    //解析json文件
    public void initPanelData()
    {
        string properPath = env.PATH_CONFIG_SETTINGS;
        if (!Directory.Exists(properPath))
        {
            Directory.CreateDirectory(properPath);
        }

        string fullPath = Path.Combine(env.PATH_CONFIG_SETTINGS, SDKSettingConfigName);
        if (System.IO.File.Exists(fullPath))
        {
            Debug.Log("sdkconfigsettings file exist, fullPath:" + fullPath);
            string configSettingJson = File.ReadAllText(fullPath);
            //3702版本新增一个输入框，两套参数的兜底策略，针对int和long类型的需要转成string，不然没法做兜底"｜"
            configSettingJson = dealConfigSettingJson(configSettingJson);
            //将configSettingJson转成object对象
            _sdkConfigModule = JsonMapper.ToObject<SDKConfigModule>(configSettingJson);
        }
        else
        {
            //判断是否存在domesticConfig.json/ConfigSettings.json文件
            string oldAndroidFullPath = Path.Combine(env.PATH_CONFIG_SETTINGS, AndroidOldSettingConfigName);
            string oldIosFullPath = Path.Combine(env.PATH_CONFIG_SETTINGS, IOSOldSettingConfigName);
            if (!System.IO.File.Exists(oldAndroidFullPath) && !System.IO.File.Exists(oldIosFullPath))
            {
                //使用默认配置
                _sdkConfigModule = new SDKConfigModule();
                Debug.Log(
                    "sdkconfigsettings file not exist, old configsettings file not exist, read default settings");
            }
            else
            {
                _sdkConfigModule = new SDKConfigModule();
                if (System.IO.File.Exists(oldAndroidFullPath))
                {
                    //解析android端内容
                    parseAndroidConfigJsonData(oldAndroidFullPath);
                    File.Delete(oldAndroidFullPath);
                }

                if (System.IO.File.Exists(oldIosFullPath))
                {
                    //解析ios端内容
                    string configSettingJson = File.ReadAllText(oldIosFullPath);
                    iOSConfigModel = JsonMapper.ToObject<ConfigModel>(configSettingJson);
                    File.Delete(oldIosFullPath);
                }
                initConfigModule();
            }
        }
        //将GSDK_Hub返回的json文件和本地json文件进行merge
        CombinParamManager.ParamCombin(parseGSDKHubConfig(), _sdkConfigModule);
        SaveConfigSettings();
    }

    protected abstract void parseAndroidConfigJsonData(string oldAndroidFullPath);

    protected virtual void initConfigModule()
    {
        initCommonModule(androidAppConfig, iOSConfigModel);
        initPushModule(androidAppConfig, iOSConfigModel);
        initIMModule(androidAppConfig, iOSConfigModel);
        initPrivacySecurityConfigModule(androidAppConfig, iOSConfigModel);
        initRatingConfigModule(iOSConfigModel);
        initRTCConfigModule(androidAppConfig, iOSConfigModel);
        initShareConfigModule(androidAppConfig, iOSConfigModel);
    }


    #region 通用配置

    public string app_id
    {
        get
        {
            return _sdkConfigModule.commonModule.app_id;
        }
        set
        {
            if (_sdkConfigModule.commonModule.app_id != value)
            {
                _sdkConfigModule.commonModule.app_id = value;
                DirtyEditor();
                SaveConfigSettings();
            }
        }
    }

    public string app_name
    {
        get
        {
            return _sdkConfigModule.commonModule.app_name;
        }
        set
        {
            if (_sdkConfigModule.commonModule.app_name != value)
            {
                _sdkConfigModule.commonModule.app_name = value;
                DirtyEditor();
                SaveConfigSettings();
            }
        }
    }

    public string init_timeout
    {
        get
        {
            return string.IsNullOrEmpty(_sdkConfigModule.commonModule.init_timeout) ? "0" : _sdkConfigModule.commonModule.init_timeout;
        }
        set
        {
            if (_sdkConfigModule.commonModule.init_timeout != value)
            {
                _sdkConfigModule.commonModule.init_timeout = value;
                DirtyEditor();
                SaveConfigSettings();
            }
        }
    }


    public bool init_optimization_enable
    {
        get
        {
            return _sdkConfigModule.commonModule.drOptimization;
        }
        set
        {
            if (_sdkConfigModule.commonModule.drOptimization != value)
            {
                _sdkConfigModule.commonModule.drOptimization = value;
                DirtyEditor();
                SaveConfigSettings();
            }
        }
    }
    
    public string account_init_timeout
    {
        get
        {
            return string.IsNullOrEmpty(_sdkConfigModule.commonModule.account_init_timeout) ? "5000" : _sdkConfigModule.commonModule.account_init_timeout;
        }
        set
        {
            if (_sdkConfigModule.commonModule.account_init_timeout != value)
            {
                _sdkConfigModule.commonModule.account_init_timeout = value;
                DirtyEditor();
                SaveConfigSettings();
            }
        }
    }

    public int init_dr_retryCount
    {
        get
        {
            return _sdkConfigModule.commonModule.drRetryCount;
        }
        set
        {
            if (_sdkConfigModule.commonModule.drRetryCount != value)
            {
                _sdkConfigModule.commonModule.drRetryCount = value;
                DirtyEditor();
                SaveConfigSettings();
            }
        }
    }



    public bool SecurityUsePacketChannelEnable
    {
        get
        {
            return _sdkConfigModule.commonModule.use_packet_channel;
        }
        set
        {
            if (_sdkConfigModule.commonModule.use_packet_channel != value)
            {
                _sdkConfigModule.commonModule.use_packet_channel = value;
                DirtyEditor();
                SaveConfigSettings();
            }
        }
    }

    public bool needGSDKModifyInfoPlist
    {
        get
        {
            return _sdkConfigModule.commonModule.needModifyInfoPlist;
        }
        set
        {
            if (_sdkConfigModule.commonModule.needModifyInfoPlist != value)
            {
                _sdkConfigModule.commonModule.needModifyInfoPlist = value;
                DirtyEditor();
                SaveConfigSettings();
            }
        }
    }

    public bool gsdkModifyInfoPlistReplaceWhenMetSame
    {
        get
        {
            return _sdkConfigModule.commonModule.modifyInfoPlistReplaceWhenMetSame;
        }
        set
        {
            if (_sdkConfigModule.commonModule.modifyInfoPlistReplaceWhenMetSame != value)
            {
                _sdkConfigModule.commonModule.modifyInfoPlistReplaceWhenMetSame = value;
                DirtyEditor();
                SaveConfigSettings();
            }
        }
    }
    
    public String download_source
    {
        get
        {
            return _sdkConfigModule.commonModule.download_source;
        }
        set
        {
            if (_sdkConfigModule.commonModule.download_source != value)
            {
                _sdkConfigModule.commonModule.download_source = value;
                DirtyEditor();
                SaveConfigSettings();
            }
        }
    }
    
    
    public Skin skin
    {
        get
        {
            try
            {
                return (Skin) Enum.Parse(typeof(Skin), _sdkConfigModule.commonModule.skin);
            }
            catch
            {
                //ignore
            }

            return Skin.purple;
        }
        set
        {
            string skin = Enum.GetName(typeof(Skin), value);
            if (_sdkConfigModule.commonModule.skin != skin)
            {
                _sdkConfigModule.commonModule.skin = skin;
                DirtyEditor();
                SaveConfigSettings();
            }
        }
    }

    public bool is_need_anti_addiction
    {
        get
        {
            return _sdkConfigModule.commonModule.is_need_anti_addiction;
        }
        set
        {
            if (_sdkConfigModule.commonModule.is_need_anti_addiction != value)
            {
                _sdkConfigModule.commonModule.is_need_anti_addiction = value;
                DirtyEditor();
                SaveConfigSettings();
            }
        }
    }
    
    public bool is_need_service_mouth
    {
        get
        {
            return _sdkConfigModule.commonModule.is_need_service_mouth;
        }
        set
        {
            if (_sdkConfigModule.commonModule.is_need_service_mouth != value)
            {
                _sdkConfigModule.commonModule.is_need_service_mouth = value;
                DirtyEditor();
                SaveConfigSettings();
            }
        }
    }
    

    
    #region Android独有
    
    public string android_package_name
    {
        get
        {
            return _sdkConfigModule.commonModule.package_name;
        }
        set
        {
            if (_sdkConfigModule.commonModule.package_name != value)
            {
                _sdkConfigModule.commonModule.package_name = value;
                DirtyEditor();
                SaveConfigSettings();
            }
        }
    }
    public AndroidScreenOrientation screenOrientation
    {
        get
        {
            try
            {
                return (AndroidScreenOrientation) Enum.Parse(typeof(AndroidScreenOrientation),
                    _sdkConfigModule.commonModule.android_screen_orientation);
            }
            catch
            {
                //ignore
            }

            return AndroidScreenOrientation.sensorLandscape;
        }

        set
        {
            string orientation = Enum.GetName(typeof(AndroidScreenOrientation), value);
            if (_sdkConfigModule.commonModule.android_screen_orientation != orientation)
            {
                _sdkConfigModule.commonModule.android_screen_orientation = orientation;
                DirtyEditor();
                SaveConfigSettings();
            }
        }
    }

    public string android_main_activity
    {
        get
        {
            return _sdkConfigModule.commonModule.android_main_activity;
        }
        set
        {
            if (_sdkConfigModule.commonModule.android_main_activity != value)
            {
                _sdkConfigModule.commonModule.android_main_activity = value;
                DirtyEditor();
                SaveConfigSettings();
            }
        }
    }

    public bool android_has_splash
    {
        get
        {
            return _sdkConfigModule.commonModule.android_has_splash;
        }
        set
        {
            if (_sdkConfigModule.commonModule.android_has_splash != value)
            {
                _sdkConfigModule.commonModule.android_has_splash = value;
                DirtyEditor();
                SaveConfigSettings();
            }
        }
    }

    public int android_splash_interval
    {
        get
        {
            return _sdkConfigModule.commonModule.android_splash_interval;
        }
        set
        {
            if (_sdkConfigModule.commonModule.android_splash_interval != value)
            {
                _sdkConfigModule.commonModule.android_splash_interval = value;
                DirtyEditor();
                SaveConfigSettings();
            }
        }
    }

    public bool android_open_launch_trace
    {
        get
        {
            return _sdkConfigModule.commonModule.android_open_launch_trace;
        }
        set
        {
            if (_sdkConfigModule.commonModule.android_open_launch_trace != value)
            {
                _sdkConfigModule.commonModule.android_open_launch_trace = value;
                DirtyEditor();
                SaveConfigSettings();
            }
        }
    }

    public bool android_clean_app
    {
        get
        {
            return _sdkConfigModule.commonModule.android_clean_app;
        }
        set
        {
            if (_sdkConfigModule.commonModule.android_clean_app != value)
            {
                _sdkConfigModule.commonModule.android_clean_app = value;
                DirtyEditor();
                SaveConfigSettings();
            }
        }
    }
    
    public bool android_request_cloud_no_db_default
    {
        get
        {
            return _sdkConfigModule.commonModule.android_request_cloud_no_db_default;
        }
        set
        {
            if (_sdkConfigModule.commonModule.android_request_cloud_no_db_default != value)
            {
                _sdkConfigModule.commonModule.android_request_cloud_no_db_default = value;
                DirtyEditor();
                SaveConfigSettings();
            }
        }
    }

    #endregion
    
    #region iOS独有

    public string iOS_bundleId
    {
        get
        {
            return _sdkConfigModule.commonModule.iOS_bundleId;
        }
        set
        {
            if (_sdkConfigModule.commonModule.iOS_bundleId != value)
            {
                _sdkConfigModule.commonModule.iOS_bundleId = value;
                DirtyEditor();
                SaveConfigSettings();
            }
        }
    }
    public string iOS_app_display_name
    {
        get
        {
            return _sdkConfigModule.commonModule.iOS_app_display_name;
        }
        set
        {
            if (_sdkConfigModule.commonModule.iOS_app_display_name != value)
            {
                _sdkConfigModule.commonModule.iOS_app_display_name = value;
                DirtyEditor();
                SaveConfigSettings();
            }
        }
    }

    public bool iOS_import_magicbox
    {
        get
        {
            return _sdkConfigModule.commonModule.iOS_import_magicbox;
        }
        set
        {
            if (_sdkConfigModule.commonModule.iOS_import_magicbox != value)
            {
                _sdkConfigModule.commonModule.iOS_import_magicbox = value;
                DirtyEditor();
                SaveConfigSettings();
            }
        }
    }

    public bool iOS_dsym_auto_upload
    {
        get
        {
            return _sdkConfigModule.commonModule.iOS_dsym_auto_upload;
        }
        set
        {
            if (_sdkConfigModule.commonModule.iOS_dsym_auto_upload != value)
            {
                _sdkConfigModule.commonModule.iOS_dsym_auto_upload = value;
                DirtyEditor();
                SaveConfigSettings();
            }
        }
    }

    public string iOS_dsym_upload_key
    {
        get
        {
            return _sdkConfigModule.commonModule.iOS_dsym_upload_key;
        }
        set
        {
            if (_sdkConfigModule.commonModule.iOS_dsym_upload_key != value)
            {
                _sdkConfigModule.commonModule.iOS_dsym_upload_key = value;
                DirtyEditor();
                SaveConfigSettings();
            }
        }
    }

    public string iOS_project_target_name
    {
        get
        {
            return _sdkConfigModule.commonModule.iOS_project_target_name;
        }
        set
        {
            if (_sdkConfigModule.commonModule.iOS_project_target_name != value)
            {
                _sdkConfigModule.commonModule.iOS_project_target_name = value;
                DirtyEditor();
                SaveConfigSettings();
            }
        }
    }

    public string iOS_framework_target_name
    {
        get
        {
            return _sdkConfigModule.commonModule.iOS_framework_target_name;
        }
        set
        {
            if (_sdkConfigModule.commonModule.iOS_framework_target_name != value)
            {
                _sdkConfigModule.commonModule.iOS_framework_target_name = value;
                DirtyEditor();
                SaveConfigSettings();
            }
        }
    }

    public int iOS_associated_domains_list_size
    {
        get
        {
            return _sdkConfigModule.commonModule.iOS_associated_domains_list_size;
        }
        set
        {
            if (_sdkConfigModule.commonModule.iOS_associated_domains_list_size != value)
            {
                _sdkConfigModule.commonModule.iOS_associated_domains_list_size = value;
                DirtyEditor();
                SaveConfigSettings();
            }
        }
    }

    // 因为数组遍历校验会造成卡顿，所以数组类型在set方法里不做 != value 的校验，其余类型做校验
    public List<string> iOS_associated_domains_list
    {
        get
        {
            return _sdkConfigModule.commonModule.iOS_associated_domains_list;
        }
        set
        {
            if (IsEqual(_sdkConfigModule.commonModule.iOS_associated_domains_list, value)) return;
            _sdkConfigModule.commonModule.iOS_associated_domains_list = value;
            DirtyEditor();
            SaveConfigSettings();
        }
    }

    public int iOS_own_url_scheme_list_size
    {
        get
        {
            return _sdkConfigModule.commonModule.iOS_own_url_scheme_list_size;
        }
        set
        {
            if (_sdkConfigModule.commonModule.iOS_own_url_scheme_list_size != value)
            {
                _sdkConfigModule.commonModule.iOS_own_url_scheme_list_size = value;
                DirtyEditor();
                SaveConfigSettings();
            }
        }
    }

    public List<string> iOS_own_url_scheme_list
    {
        get
        {
            return _sdkConfigModule.commonModule.iOS_own_url_scheme_list;
        }
        set
        {
            if (IsEqual(_sdkConfigModule.commonModule.iOS_own_url_scheme_list, value)) return;
            _sdkConfigModule.commonModule.iOS_own_url_scheme_list = value;
            DirtyEditor();
            SaveConfigSettings();
        }
    }

    public int iOS_application_queries_schemes_list_size
    {
        get
        {
            return _sdkConfigModule.commonModule.iOS_application_queries_schemes_list_size;
        }
        set
        {
            if (_sdkConfigModule.commonModule.iOS_application_queries_schemes_list_size != value)
            {
                _sdkConfigModule.commonModule.iOS_application_queries_schemes_list_size = value;
                DirtyEditor();
                SaveConfigSettings();
            }
        }
    }

    public List<string> iOS_application_queries_schemes_list
    {
        get
        {
            return _sdkConfigModule.commonModule.iOS_application_queries_schemes_list;
        }
        set
        {
            if (IsEqual(_sdkConfigModule.commonModule.iOS_application_queries_schemes_list, value)) return;
            _sdkConfigModule.commonModule.iOS_application_queries_schemes_list = value;
            DirtyEditor();
            SaveConfigSettings();
        }
    }
    

    public bool iOS_cert_success
    {
        get
        {
            return _sdkConfigModule.commonModule.iOS_cert_success;
        }
        set
        {
            if (_sdkConfigModule.commonModule.iOS_cert_success != value)
            {
                _sdkConfigModule.commonModule.iOS_cert_success = value;
                DirtyEditor();
                SaveConfigSettings();
            }
        }
    }

    public string iOS_provisioning_guid
    {
        get
        {
            return _sdkConfigModule.commonModule.iOS_provisioning_guid;
        }
        set
        {
            if (_sdkConfigModule.commonModule.iOS_provisioning_guid != value)
            {
                _sdkConfigModule.commonModule.iOS_provisioning_guid = value;
                DirtyEditor();
                SaveConfigSettings();
            }
        }
    }

    public string iOS_team_ID
    {
        get
        {
            return _sdkConfigModule.commonModule.iOS_team_ID;
        }
        set
        {
            if (_sdkConfigModule.commonModule.iOS_team_ID != value)
            {
                _sdkConfigModule.commonModule.iOS_team_ID = value;
                DirtyEditor();
                SaveConfigSettings();
            }
        }
    }

    public string iOS_cert_name
    {
        get
        {
            return _sdkConfigModule.commonModule.iOS_cert_name;
        }
        set
        {
            if (_sdkConfigModule.commonModule.iOS_cert_name != value)
            {
                _sdkConfigModule.commonModule.iOS_cert_name = value;
                DirtyEditor();
                SaveConfigSettings();
            }
        }
    }

    public bool iOS_debug_cert_success
    {
        get
        {
            return _sdkConfigModule.commonModule.iOS_debug_cert_success;
        }
        set
        {
            if (_sdkConfigModule.commonModule.iOS_debug_cert_success != value)
            {
                _sdkConfigModule.commonModule.iOS_debug_cert_success = value;
                DirtyEditor();
                SaveConfigSettings();
            }
        }
    }

    public string iOS_debug_provisioning_guid
    {
        get
        {
            return _sdkConfigModule.commonModule.iOS_debug_provisioning_guid;
        }
        set
        {
            if (_sdkConfigModule.commonModule.iOS_debug_provisioning_guid != value)
            {
                _sdkConfigModule.commonModule.iOS_debug_provisioning_guid = value;
            }
            DirtyEditor();
            SaveConfigSettings();
        }
    }

    public string iOS_debug_team_ID
    {
        get
        {
            return _sdkConfigModule.commonModule.iOS_debug_team_ID;
        }
        set
        {
            if (_sdkConfigModule.commonModule.iOS_debug_team_ID != value)
            {
                _sdkConfigModule.commonModule.iOS_debug_team_ID = value;
                DirtyEditor();
                SaveConfigSettings();
            }
        }
    }

    public string iOS_debug_cert_name
    {
        get
        {
            return _sdkConfigModule.commonModule.iOS_debug_cert_name;
        }
        set
        {
            if (_sdkConfigModule.commonModule.iOS_debug_cert_name != value)
            {
                _sdkConfigModule.commonModule.iOS_debug_cert_name = value;
                DirtyEditor();
                SaveConfigSettings();
            }
        }
    }

    #endregion
    
    #endregion

    
    #region 隐私安全配置

    public bool privacySecurity_is_show_permission_toast
    {
        get
        {
            return _sdkConfigModule.privacySecurityConfigModule.is_show_permission_toast;
        }
        set
        {
            if (_sdkConfigModule.privacySecurityConfigModule.is_show_permission_toast != value)
            {
                _sdkConfigModule.privacySecurityConfigModule.is_show_permission_toast = value;
                DirtyEditor();
                SaveConfigSettings();
            }
        }
    }
    
    #region Android独有

    public bool android_privacySecurity_is_need_privacy_protection
    {
        get
        {
            return _sdkConfigModule.privacySecurityConfigModule.android_is_need_privacy_protection;
        }
        set
        {
            if (_sdkConfigModule.privacySecurityConfigModule.android_is_need_privacy_protection != value)
            {
                _sdkConfigModule.privacySecurityConfigModule.android_is_need_privacy_protection = value;
                DirtyEditor();
                SaveConfigSettings();
            }
        }
    }

    #endregion
    
    #region iOS独有

    public bool iOS_privacy_microphone_usage_available
    {
        get
        {
            return _sdkConfigModule.privacySecurityConfigModule.iOS_privacy_microphone_usage_available;
        }
        set
        {
            if (_sdkConfigModule.privacySecurityConfigModule.iOS_privacy_microphone_usage_available != value)
            {
                _sdkConfigModule.privacySecurityConfigModule.iOS_privacy_microphone_usage_available = value;
                DirtyEditor();
                SaveConfigSettings();
            }
        }
    }

    public string iOS_privacy_microphone_usage_desc
    {
        get
        {
            return _sdkConfigModule.privacySecurityConfigModule.iOS_privacy_microphone_usage_desc;
        }
        set
        {
            if (_sdkConfigModule.privacySecurityConfigModule.iOS_privacy_microphone_usage_desc != value)
            {
                _sdkConfigModule.privacySecurityConfigModule.iOS_privacy_microphone_usage_desc = value;
                DirtyEditor();
                SaveConfigSettings();
            }
        }
    }
    
    public bool iOS_privacy_photo_usage_available
    {
        get
        {
            return _sdkConfigModule.privacySecurityConfigModule.iOS_privacy_photo_usage_available;
        }
        set
        {
            if (_sdkConfigModule.privacySecurityConfigModule.iOS_privacy_photo_usage_available != value)
            {
                _sdkConfigModule.privacySecurityConfigModule.iOS_privacy_photo_usage_available = value;
                DirtyEditor();
                SaveConfigSettings();
            }
        }
    }

    public string iOS_privacy_photo_usage_desc
    {
        get
        {
            return _sdkConfigModule.privacySecurityConfigModule.iOS_privacy_photo_usage_desc;
        }
        set
        {
            if (_sdkConfigModule.privacySecurityConfigModule.iOS_privacy_photo_usage_desc != value)
            {
                _sdkConfigModule.privacySecurityConfigModule.iOS_privacy_photo_usage_desc = value;
                DirtyEditor();
                SaveConfigSettings();
            }
        }
    }

    public bool iOS_privacy_photo_additions_usage_available
    {
        get
        {
            return _sdkConfigModule.privacySecurityConfigModule.iOS_privacy_photo_additions_usage_available;
        }
        set
        {
            if (_sdkConfigModule.privacySecurityConfigModule.iOS_privacy_photo_additions_usage_available != value)
            {
                _sdkConfigModule.privacySecurityConfigModule.iOS_privacy_photo_additions_usage_available = value;
                DirtyEditor();
                SaveConfigSettings();
            }
        }
    }

    public string iOS_privacy_photo_additions_usage_desc
    {
        get
        {
            return _sdkConfigModule.privacySecurityConfigModule.iOS_privacy_photo_additions_usage_desc;
        }
        set
        {
            if (_sdkConfigModule.privacySecurityConfigModule.iOS_privacy_photo_additions_usage_desc != value)
            {
                _sdkConfigModule.privacySecurityConfigModule.iOS_privacy_photo_additions_usage_desc = value;
                DirtyEditor();
                SaveConfigSettings();
            }
        }
    }

    public bool iOS_privacy_camera_usage_available
    {
        get
        {
            return _sdkConfigModule.privacySecurityConfigModule.iOS_privacy_camera_usage_available;
        }
        set
        {
            if (_sdkConfigModule.privacySecurityConfigModule.iOS_privacy_camera_usage_available != value)
            {
                _sdkConfigModule.privacySecurityConfigModule.iOS_privacy_camera_usage_available = value;
                DirtyEditor();
                SaveConfigSettings();
            }
        }
    }

    public string iOS_privacy_camera_usage_desc
    {
        get
        {
            return _sdkConfigModule.privacySecurityConfigModule.iOS_privacy_camera_usage_desc;
        }
        set
        {
            if (_sdkConfigModule.privacySecurityConfigModule.iOS_privacy_camera_usage_desc != value)
            {
                _sdkConfigModule.privacySecurityConfigModule.iOS_privacy_camera_usage_desc = value;
                DirtyEditor();
                SaveConfigSettings();
            }
        }
    }

    public bool iOS_privacy_contacts_usage_available
    {
        get
        {
            return _sdkConfigModule.privacySecurityConfigModule.iOS_privacy_contacts_usage_available;
        }
        set
        {
            if (_sdkConfigModule.privacySecurityConfigModule.iOS_privacy_contacts_usage_available != value)
            {
                _sdkConfigModule.privacySecurityConfigModule.iOS_privacy_contacts_usage_available = value;
                DirtyEditor();
                SaveConfigSettings();
            }
        }
    }

    public string iOS_privacy_contacts_usage_desc
    {
        get
        {
            return _sdkConfigModule.privacySecurityConfigModule.iOS_privacy_contacts_usage_desc;
        }
        set
        {
            if (_sdkConfigModule.privacySecurityConfigModule.iOS_privacy_contacts_usage_desc != value)
            {
                _sdkConfigModule.privacySecurityConfigModule.iOS_privacy_contacts_usage_desc = value;
                DirtyEditor();
                SaveConfigSettings();
            }
        }
    }

    public bool iOS_privacy_app_tracking_usage_available
    {
        get
        {
            return _sdkConfigModule.privacySecurityConfigModule.iOS_privacy_app_tracking_usage_available;
        }
        set
        {
            if (_sdkConfigModule.privacySecurityConfigModule.iOS_privacy_app_tracking_usage_available != value)
            {
                _sdkConfigModule.privacySecurityConfigModule.iOS_privacy_app_tracking_usage_available = value;
                DirtyEditor();
                SaveConfigSettings();
            }
        }
    }

    public string iOS_privacy_app_tracking_usage_desc
    {
        get
        {
            return _sdkConfigModule.privacySecurityConfigModule.iOS_privacy_app_tracking_usage_desc;
        }
        set
        {
            if (_sdkConfigModule.privacySecurityConfigModule.iOS_privacy_app_tracking_usage_desc != value)
            {
                _sdkConfigModule.privacySecurityConfigModule.iOS_privacy_app_tracking_usage_desc = value;
                DirtyEditor();
                SaveConfigSettings();
            }
        }
    }

    public bool iOS_privacy_location_usage_available
    {
        get
        {
            return _sdkConfigModule.privacySecurityConfigModule.iOS_privacy_location_usage_available;
        }
        set
        {
            if (_sdkConfigModule.privacySecurityConfigModule.iOS_privacy_location_usage_available != value)
            {
                _sdkConfigModule.privacySecurityConfigModule.iOS_privacy_location_usage_available = value;
                DirtyEditor();
                SaveConfigSettings();
            }
        }
    }

    public string iOS_privacy_location_usage_desc
    {
        get
        {
            return _sdkConfigModule.privacySecurityConfigModule.iOS_privacy_location_usage_desc;
        }
        set
        {
            if (_sdkConfigModule.privacySecurityConfigModule.iOS_privacy_location_usage_desc != value)
            {
                _sdkConfigModule.privacySecurityConfigModule.iOS_privacy_location_usage_desc = value;
                DirtyEditor();
                SaveConfigSettings();
            }
        }
    }

    public bool iOS_privacy_location_always_usage_available
    {
        get
        {
            return _sdkConfigModule.privacySecurityConfigModule.iOS_privacy_location_always_usage_available;
        }
        set
        {
            if (_sdkConfigModule.privacySecurityConfigModule.iOS_privacy_location_always_usage_available != value)
            {
                _sdkConfigModule.privacySecurityConfigModule.iOS_privacy_location_always_usage_available = value;
                DirtyEditor();
                SaveConfigSettings();
            }
        }
    }

    public string iOS_privacy_location_always_usage_desc
    {
        get
        {
            return _sdkConfigModule.privacySecurityConfigModule.iOS_privacy_location_always_usage_desc;
        }
        set
        {
            if (_sdkConfigModule.privacySecurityConfigModule.iOS_privacy_location_always_usage_desc != value)
            {
                _sdkConfigModule.privacySecurityConfigModule.iOS_privacy_location_always_usage_desc = value;
                DirtyEditor();
                SaveConfigSettings();
            }
        }
    }

    public bool iOS_privacy_calendars_usage_available
    {
        get
        {
            return _sdkConfigModule.privacySecurityConfigModule.iOS_privacy_calendars_usage_available;
        }
        set
        {
            if (_sdkConfigModule.privacySecurityConfigModule.iOS_privacy_calendars_usage_available != value)
            {
                _sdkConfigModule.privacySecurityConfigModule.iOS_privacy_calendars_usage_available = value;
                DirtyEditor();
                SaveConfigSettings();
            }
        }
    }

    public string iOS_privacy_calendars_usage_desc
    {
        get
        {
            return _sdkConfigModule.privacySecurityConfigModule.iOS_privacy_calendars_usage_desc;
        }
        set
        {
            if (_sdkConfigModule.privacySecurityConfigModule.iOS_privacy_calendars_usage_desc != value)
            {
                _sdkConfigModule.privacySecurityConfigModule.iOS_privacy_calendars_usage_desc = value;
                DirtyEditor();
                SaveConfigSettings();
            }
        }
    }

    public bool iOS_privacy_reminders_usage_available
    {
        get
        {
            return _sdkConfigModule.privacySecurityConfigModule.iOS_privacy_reminders_usage_available;
        }
        set
        {
            if (_sdkConfigModule.privacySecurityConfigModule.iOS_privacy_reminders_usage_available != value)
            {
                _sdkConfigModule.privacySecurityConfigModule.iOS_privacy_reminders_usage_available = value;
                DirtyEditor();
                SaveConfigSettings();
            }
        }
    }

    public string iOS_privacy_reminders_usage_desc
    {
        get
        {
            return _sdkConfigModule.privacySecurityConfigModule.iOS_privacy_reminders_usage_desc;
        }
        set
        {
            if (_sdkConfigModule.privacySecurityConfigModule.iOS_privacy_reminders_usage_desc != value)
            {
                _sdkConfigModule.privacySecurityConfigModule.iOS_privacy_reminders_usage_desc = value;
                DirtyEditor();
                SaveConfigSettings();
            }
        }
    }

    public bool iOS_privacy_speech_recognition_usage_available
    {
        get
        {
            return _sdkConfigModule.privacySecurityConfigModule.iOS_privacy_speech_recognition_usage_available;
        }
        set
        {
            if (_sdkConfigModule.privacySecurityConfigModule.iOS_privacy_speech_recognition_usage_available != value)
            {
                _sdkConfigModule.privacySecurityConfigModule.iOS_privacy_speech_recognition_usage_available = value;
                DirtyEditor();
                SaveConfigSettings();
            }
        }
    }

    public string iOS_privacy_speech_recognition_usage_desc
    {
        get
        {
            return _sdkConfigModule.privacySecurityConfigModule.iOS_privacy_speech_recognition_usage_desc;
        }
        set
        {
            if (_sdkConfigModule.privacySecurityConfigModule.iOS_privacy_speech_recognition_usage_desc != value)
            {
                _sdkConfigModule.privacySecurityConfigModule.iOS_privacy_speech_recognition_usage_desc = value;
                DirtyEditor();
                SaveConfigSettings();
            }
        }
    }

    public bool iOS_privacy_siri_usage_available
    {
        get
        {
            return _sdkConfigModule.privacySecurityConfigModule.iOS_privacy_siri_usage_available;
        }
        set
        {
            if (_sdkConfigModule.privacySecurityConfigModule.iOS_privacy_siri_usage_available != value)
            {
                _sdkConfigModule.privacySecurityConfigModule.iOS_privacy_siri_usage_available = value;
                DirtyEditor();
                SaveConfigSettings();
            }
        }
    }

    public string iOS_privacy_siri_usage_desc
    {
        get
        {
            return _sdkConfigModule.privacySecurityConfigModule.iOS_privacy_siri_usage_desc;
        }
        set
        {
            if (_sdkConfigModule.privacySecurityConfigModule.iOS_privacy_siri_usage_desc != value)
            {
                _sdkConfigModule.privacySecurityConfigModule.iOS_privacy_siri_usage_desc = value;
                DirtyEditor();
                SaveConfigSettings();
            }
        }
    }

    public bool iOS_privacy_face_ID_usage_available
    {
        get
        {
            return _sdkConfigModule.privacySecurityConfigModule.iOS_privacy_face_ID_usage_available;
        }
        set
        {
            if (_sdkConfigModule.privacySecurityConfigModule.iOS_privacy_face_ID_usage_available != value)
            {
                _sdkConfigModule.privacySecurityConfigModule.iOS_privacy_face_ID_usage_available = value;
                DirtyEditor();
                SaveConfigSettings();
            }
        }
    }

    public string iOS_privacy_face_ID_usage_desc
    {
        get
        {
            return _sdkConfigModule.privacySecurityConfigModule.iOS_privacy_face_ID_usage_desc;
        }
        set
        {
            if (_sdkConfigModule.privacySecurityConfigModule.iOS_privacy_face_ID_usage_desc != value)
            {
                _sdkConfigModule.privacySecurityConfigModule.iOS_privacy_face_ID_usage_desc = value;
                DirtyEditor();
                SaveConfigSettings();
            }
        }
    }


    #region ATT idfa弹窗

    public bool iOS_is_ATT_available
    {
        get
        {
            return _sdkConfigModule.privacySecurityConfigModule.iOS_is_ATT_available;
        }
        set
        {
            if (_sdkConfigModule.privacySecurityConfigModule.iOS_is_ATT_available != value)
            {
                _sdkConfigModule.privacySecurityConfigModule.iOS_is_ATT_available = value;
                DirtyEditor();
                SaveConfigSettings();
            }
        }
    }

    public bool iOS_ATT_is_need_DIY_call
    {
        get
        {
            return _sdkConfigModule.privacySecurityConfigModule.iOS_is_need_DIY_call;
        }
        set
        {
            if (_sdkConfigModule.privacySecurityConfigModule.iOS_is_need_DIY_call != value)
            {
                _sdkConfigModule.privacySecurityConfigModule.iOS_is_need_DIY_call = value;
                DirtyEditor();
                SaveConfigSettings();
            }
        }
    }

    public bool iOS_ATT_is_use_CP_DIY_view
    {
        get
        {
            return _sdkConfigModule.privacySecurityConfigModule.iOS_is_use_CP_DIY_view;
        }
        set
        {
            if (_sdkConfigModule.privacySecurityConfigModule.iOS_is_use_CP_DIY_view != value)
            {
                if (value)
                {
                    this.iOS_ATT_is_need_gsdk_defalut_view = false;
                    this.iOS_ATT_is_need_DIY_call = true;
                }
                _sdkConfigModule.privacySecurityConfigModule.iOS_is_use_CP_DIY_view = value;
                DirtyEditor();
                SaveConfigSettings();
            }
        }
    }

    public bool iOS_ATT_is_need_gsdk_defalut_view
    {
        get
        {
            return _sdkConfigModule.privacySecurityConfigModule.iOS_is_need_gsdk_defalut_view;
        }
        set
        {
            if (_sdkConfigModule.privacySecurityConfigModule.iOS_is_need_gsdk_defalut_view != value)
            {
                if (value)
                {
                    this.iOS_ATT_is_use_CP_DIY_view = false;
                }
                _sdkConfigModule.privacySecurityConfigModule.iOS_is_need_gsdk_defalut_view = value;
                DirtyEditor();
                SaveConfigSettings();
            }
        }
    }

    public string iOS_ATT_gsdk_defalut_view_cancel_title_color
    {
        get
        {
            return _sdkConfigModule.privacySecurityConfigModule.iOS_gsdk_defalut_view_cancel_title_color;
        }
        set
        {
            if (_sdkConfigModule.privacySecurityConfigModule.iOS_gsdk_defalut_view_cancel_title_color != value)
            {
                _sdkConfigModule.privacySecurityConfigModule.iOS_gsdk_defalut_view_cancel_title_color = value;
                DirtyEditor();
                SaveConfigSettings();
            }
        }
    }

    public string iOS_ATT_gsdk_defalut_view_cancel_color
    {
        get
        {
            return _sdkConfigModule.privacySecurityConfigModule.iOS_gsdk_defalut_view_cancel_color;
        }
        set
        {
            if (_sdkConfigModule.privacySecurityConfigModule.iOS_gsdk_defalut_view_cancel_color != value)
            {
                _sdkConfigModule.privacySecurityConfigModule.iOS_gsdk_defalut_view_cancel_color = value;
                DirtyEditor();
                SaveConfigSettings();
            }
        }
    }

    public string iOS_ATT_gsdk_defalut_view_cancel_title
    {
        get
        {
            return _sdkConfigModule.privacySecurityConfigModule.iOS_gsdk_defalut_view_cancel_title;
        }
        set
        {
            if (_sdkConfigModule.privacySecurityConfigModule.iOS_gsdk_defalut_view_cancel_title != value)
            {
                _sdkConfigModule.privacySecurityConfigModule.iOS_gsdk_defalut_view_cancel_title = value;
                DirtyEditor();
                SaveConfigSettings();
            }
        }
    }

    public string iOS_ATT_gsdk_defalut_view_confirm_title_color
    {
        get
        {
            return _sdkConfigModule.privacySecurityConfigModule.iOS_gsdk_defalut_view_confirm_title_color;
        }
        set
        {
            if (_sdkConfigModule.privacySecurityConfigModule.iOS_gsdk_defalut_view_confirm_title_color != value)
            {
                _sdkConfigModule.privacySecurityConfigModule.iOS_gsdk_defalut_view_confirm_title_color = value;
                DirtyEditor();
                SaveConfigSettings();
            }
        }
    }

    public string iOS_ATT_gsdk_defalut_view_confirm_color
    {
        get
        {
            return _sdkConfigModule.privacySecurityConfigModule.iOS_gsdk_defalut_view_confirm_color;
        }
        set
        {
            if (_sdkConfigModule.privacySecurityConfigModule.iOS_gsdk_defalut_view_confirm_color != value)
            {
                _sdkConfigModule.privacySecurityConfigModule.iOS_gsdk_defalut_view_confirm_color = value;
                DirtyEditor();
                SaveConfigSettings();
            }
        }
    }

    public string iOS_ATT_gsdk_defalut_view_confirm_title
    {
        get
        {
            return _sdkConfigModule.privacySecurityConfigModule.iOS_gsdk_defalut_view_confirm_title;
        }
        set
        {
            if (_sdkConfigModule.privacySecurityConfigModule.iOS_gsdk_defalut_view_confirm_title != value)
            {
                _sdkConfigModule.privacySecurityConfigModule.iOS_gsdk_defalut_view_confirm_title = value;
                DirtyEditor();
                SaveConfigSettings();
            }
        }
    }

    public string iOS_ATT_gsdk_defalut_view_content
    {
        get
        {
            return _sdkConfigModule.privacySecurityConfigModule.iOS_gsdk_defalut_view_content;
        }
        set
        {
            if (_sdkConfigModule.privacySecurityConfigModule.iOS_gsdk_defalut_view_content != value)
            {
                _sdkConfigModule.privacySecurityConfigModule.iOS_gsdk_defalut_view_content = value;
                DirtyEditor();
                SaveConfigSettings();
            }
        }
    }

    public string iOS_ATT_gsdk_defalut_view_title
    {
        get
        {
            return _sdkConfigModule.privacySecurityConfigModule.iOS_gsdk_defalut_view_title;
        }
        set
        {
            if (_sdkConfigModule.privacySecurityConfigModule.iOS_gsdk_defalut_view_title != value)
            {
                _sdkConfigModule.privacySecurityConfigModule.iOS_gsdk_defalut_view_title = value;
                DirtyEditor();
                SaveConfigSettings();
            }
        }
    }
    
    #endregion

    #endregion

    #endregion
    
    
    

    
    #region 支付配置
    #region Android独有
    #endregion
    #endregion

    
    #region 分享配置

    public bool share_dim_amount_available
    {
        get
        {
            return _sdkConfigModule.shareConfigModule.share_dim_amount_available;
        }
        set
        {
            if (_sdkConfigModule.shareConfigModule.share_dim_amount_available != value)
            {
                _sdkConfigModule.shareConfigModule.share_dim_amount_available = value;
                DirtyEditor();
                SaveConfigSettings();
            }
        }
    }

    public string share_dim_amount
    {
        get { return _sdkConfigModule.shareConfigModule.share_dim_amount; }
        set
        {
            if (_sdkConfigModule.shareConfigModule.share_dim_amount != value)
            {
                _sdkConfigModule.shareConfigModule.share_dim_amount = value;
                DirtyEditor();
                SaveConfigSettings();
            }
        }
    }

    #region Android独有
    public string andriod_share_panel_id
    {
        get
        {
            return _sdkConfigModule.shareConfigModule.android_share_panel_id;
        }
        set
        {
            if (_sdkConfigModule.shareConfigModule.android_share_panel_id != value)
            {
                _sdkConfigModule.shareConfigModule.android_share_panel_id = value;
                DirtyEditor();
                SaveConfigSettings();
            }
        }
    }
    #endregion

    #region iOS独有
    public string iOS_share_panel_id
    {
        get
        {
            return _sdkConfigModule.shareConfigModule.iOS_share_panel_id;
        }
        set
        {
            if (_sdkConfigModule.shareConfigModule.iOS_share_panel_id != value)
            {
                _sdkConfigModule.shareConfigModule.iOS_share_panel_id = value;
                DirtyEditor();
                SaveConfigSettings();
            }
        }
    }
    #endregion


    #region Android独有

    public bool android_share_qq_available
    {
        get
        {
            return _sdkConfigModule.shareConfigModule.android_qq_available;
        }
        set
        {
            if (_sdkConfigModule.shareConfigModule.android_qq_available != value)
            {
                _sdkConfigModule.shareConfigModule.android_qq_available = value;
                DirtyEditor();
                SaveConfigSettings();
            }
        }
    }

    public string android_share_qq_key
    {
        get
        {
            return _sdkConfigModule.shareConfigModule.android_qq_key;
        }
        set
        {
            if (_sdkConfigModule.shareConfigModule.android_qq_key != value)
            {
                _sdkConfigModule.shareConfigModule.android_qq_key = value;
                DirtyEditor();
                SaveConfigSettings();
            }
        }
    }

    public bool android_share_wx_available
    {
        get
        {
            return _sdkConfigModule.shareConfigModule.android_wx_available;
        }
        set
        {
            if (_sdkConfigModule.shareConfigModule.android_wx_available != value)
            {
                _sdkConfigModule.shareConfigModule.android_wx_available = value;
                DirtyEditor();
                SaveConfigSettings();
            }
        }
    }

    public string android_share_wx_key
    {
        get
        {
            return _sdkConfigModule.shareConfigModule.android_wx_key;
        }
        set
        {
            if (_sdkConfigModule.shareConfigModule.android_wx_key != value)
            {
                _sdkConfigModule.shareConfigModule.android_wx_key = value;
                DirtyEditor();
                SaveConfigSettings();
            }
        }
    }

    public bool android_share_weibo_available
    {
        get
        {
            return _sdkConfigModule.shareConfigModule.android_weibo_available;
        }
        set
        {
            if (_sdkConfigModule.shareConfigModule.android_weibo_available != value)
            {
                _sdkConfigModule.shareConfigModule.android_weibo_available = value;
                DirtyEditor();
                SaveConfigSettings();
            }
        }
    }

    public string android_share_weibo_key
    {
        get
        {
            return _sdkConfigModule.shareConfigModule.android_weibo_key;
        }
        set
        {
            if (_sdkConfigModule.shareConfigModule.android_weibo_key != value)
            {
                _sdkConfigModule.shareConfigModule.android_weibo_key = value;
                DirtyEditor();
                SaveConfigSettings();
            }
        }
    }

    public string android_share_weibo_url
    {
        get
        {
            return _sdkConfigModule.shareConfigModule.android_weibo_url;
        }
        set
        {
            if (_sdkConfigModule.shareConfigModule.android_weibo_url != value)
            {
                _sdkConfigModule.shareConfigModule.android_weibo_url = value;
                DirtyEditor();
                SaveConfigSettings();
            }
        }
    }
    
    public bool android_share_awe_available
    {
        get
        {
            return _sdkConfigModule.shareConfigModule.android_awe_available;
        }
        set
        {
            if (_sdkConfigModule.shareConfigModule.android_awe_available != value)
            {
                _sdkConfigModule.shareConfigModule.android_awe_available = value;
                DirtyEditor();
                SaveConfigSettings();
            }
        }
    }

    public string android_share_awe_key
    {
        get
        {
            return _sdkConfigModule.shareConfigModule.android_awe_key;
        }
        set
        {
            if (_sdkConfigModule.shareConfigModule.android_awe_key != value)
            {
                _sdkConfigModule.shareConfigModule.android_awe_key = value;
                DirtyEditor();
                SaveConfigSettings();
            }
        }
    }

    #endregion

    #region iOS独有

    public bool iOS_share_qq_available
    {
        get
        {
            return _sdkConfigModule.shareConfigModule.iOS_qq_available;
        }
        set
        {
            if (_sdkConfigModule.shareConfigModule.iOS_qq_available != value)
            {
                _sdkConfigModule.shareConfigModule.iOS_qq_available = value;
                DirtyEditor();
                SaveConfigSettings();
            }
        }
    }

    public string iOS_share_qq_key
    {
        get
        {
            return _sdkConfigModule.shareConfigModule.iOS_qq_key;
        }
        set
        {
            if (_sdkConfigModule.shareConfigModule.iOS_qq_key != value)
            {
                _sdkConfigModule.shareConfigModule.iOS_qq_key = value;
                DirtyEditor();
                SaveConfigSettings();
            }
        }
    }

    public string iOS_share_qq_universal_link
    {
        get
        {
            return _sdkConfigModule.shareConfigModule.iOS_qq_universal_link;
        }
        set
        {
            if (_sdkConfigModule.shareConfigModule.iOS_qq_universal_link != value)
            {
                _sdkConfigModule.shareConfigModule.iOS_qq_universal_link = value;
                DirtyEditor();
                SaveConfigSettings();
            }
        }
    }

    public bool iOS_share_wx_available
    {
        get
        {
            return _sdkConfigModule.shareConfigModule.iOS_wx_available;
        }
        set
        {
            if (_sdkConfigModule.shareConfigModule.iOS_wx_available != value)
            {
                _sdkConfigModule.shareConfigModule.iOS_wx_available = value;
                DirtyEditor();
                SaveConfigSettings();
            }
        }
    }

    public string iOS_share_wx_key
    {
        get
        {
            return _sdkConfigModule.shareConfigModule.iOS_wx_key;
        }
        set
        {
            if (_sdkConfigModule.shareConfigModule.iOS_wx_key != value)
            {
                _sdkConfigModule.shareConfigModule.iOS_wx_key = value;
                DirtyEditor();
                SaveConfigSettings();
            }
        }
    }

    public string iOS_share_wx_universal_link
    {
        get
        {
            return _sdkConfigModule.shareConfigModule.iOS_wx_universal_link;
        }
        set
        {
            if (_sdkConfigModule.shareConfigModule.iOS_wx_universal_link != value)
            {
                _sdkConfigModule.shareConfigModule.iOS_wx_universal_link = value;
                DirtyEditor();
                SaveConfigSettings();
            }
        }
    }

    public bool iOS_share_weibo_available
    {
        get
        {
            return _sdkConfigModule.shareConfigModule.iOS_weibo_available;
        }
        set
        {
            if (_sdkConfigModule.shareConfigModule.iOS_weibo_available != value)
            {
                _sdkConfigModule.shareConfigModule.iOS_weibo_available = value;
                DirtyEditor();
                SaveConfigSettings();
            }
        }
    }

    public string iOS_share_weibo_key
    {
        get
        {
            return _sdkConfigModule.shareConfigModule.iOS_weibo_key;
        }
        set
        {
            if (_sdkConfigModule.shareConfigModule.iOS_weibo_key != value)
            {
                _sdkConfigModule.shareConfigModule.iOS_weibo_key = value;
                DirtyEditor();
                SaveConfigSettings();
            }
        }
    }
    
    public string iOS_share_weibo_universal_link
    {
        get
        {
            return _sdkConfigModule.shareConfigModule.iOS_weibo_universal_link;
        }
        set
        {
            if (_sdkConfigModule.shareConfigModule.iOS_weibo_universal_link != value)
            {
                _sdkConfigModule.shareConfigModule.iOS_weibo_universal_link = value;
                DirtyEditor();
                SaveConfigSettings();
            }
        }
    }
    
    

    public bool iOS_share_awe_available
    {
        get
        {
            return _sdkConfigModule.shareConfigModule.iOS_awe_available;
        }
        set
        {
            if (_sdkConfigModule.shareConfigModule.iOS_awe_available != value)
            {
                _sdkConfigModule.shareConfigModule.iOS_awe_available = value;
                DirtyEditor();
                SaveConfigSettings();
            }
        }
    }

    public string iOS_share_awe_key
    {
        get
        {
            return _sdkConfigModule.shareConfigModule.iOS_awe_key;
        }
        set
        {
            if (_sdkConfigModule.shareConfigModule.iOS_awe_key != value)
            {
                _sdkConfigModule.shareConfigModule.iOS_awe_key = value;
                DirtyEditor();
                SaveConfigSettings();
            }
        }
    }

    #endregion
    
    
    #endregion

    
    #region push配置

    public bool push_enable
    {
        get { return _sdkConfigModule.pushConfigModule.push_enable; }
        set
        {
            if (_sdkConfigModule != null && _sdkConfigModule.pushConfigModule.push_enable!=value)
            {
                _sdkConfigModule.pushConfigModule.push_enable = value;
                DirtyEditor();
                SaveConfigSettings();
            }
        }
    }
    #region Android独有
    public string android_push_app_name
    {
        get { return _sdkConfigModule.pushConfigModule.android_push_app_name; }
        set
        {
            if (_sdkConfigModule != null && _sdkConfigModule.pushConfigModule.android_push_app_name!=value)
            {
                _sdkConfigModule.pushConfigModule.android_push_app_name = value;
                DirtyEditor();
                SaveConfigSettings();
            }
        }
    }
    public string android_push_huawei_appid
    {
        get { return _sdkConfigModule.pushConfigModule.android_push_huawei_appid; }
        set
        {
            if (_sdkConfigModule != null && _sdkConfigModule.pushConfigModule.android_push_huawei_appid != value)
            {
                _sdkConfigModule.pushConfigModule.android_push_huawei_appid = value;
                DirtyEditor();
                SaveConfigSettings();
            }
        }
    }
    
    public string android_push_mi_app_id
    {
        get { return _sdkConfigModule.pushConfigModule.android_push_mi_app_id; }
        set
        {
            if (_sdkConfigModule != null && _sdkConfigModule.pushConfigModule.android_push_mi_app_id != value)
            {
                _sdkConfigModule.pushConfigModule.android_push_mi_app_id = value;
                DirtyEditor();
                SaveConfigSettings();
            }
        }
    }
    
    public string android_push_mi_app_key
    {
        get { return _sdkConfigModule.pushConfigModule.android_push_mi_app_key; }
        set
        {
            if (_sdkConfigModule != null && _sdkConfigModule.pushConfigModule.android_push_mi_app_key != value)
            {
                _sdkConfigModule.pushConfigModule.android_push_mi_app_key = value;
                DirtyEditor();
                SaveConfigSettings();
            }
        }
    }
    
    public string android_push_meizu_app_id
    {
        get { return _sdkConfigModule.pushConfigModule.android_push_meizu_app_id; }
        set
        {
            if (_sdkConfigModule != null && _sdkConfigModule.pushConfigModule.android_push_meizu_app_id != value)
            {
                _sdkConfigModule.pushConfigModule.android_push_meizu_app_id = value;
                DirtyEditor();
                SaveConfigSettings();
            }
        }
    }
    
    public string android_push_meizu_app_key
    {
        get { return _sdkConfigModule.pushConfigModule.android_push_meizu_app_key; }
        set
        {
            if (_sdkConfigModule != null && _sdkConfigModule.pushConfigModule.android_push_meizu_app_key != value)
            {
                _sdkConfigModule.pushConfigModule.android_push_meizu_app_key = value;
                DirtyEditor();
                SaveConfigSettings();
            }
        }
    }
    
    public string android_push_oppo_app_key
    {
        get { return _sdkConfigModule.pushConfigModule.android_push_oppo_app_key; }
        set
        {
            if (_sdkConfigModule != null && _sdkConfigModule.pushConfigModule.android_push_oppo_app_key != value)
            {
                _sdkConfigModule.pushConfigModule.android_push_oppo_app_key = value;
                DirtyEditor();
                SaveConfigSettings();
            }
        }
    }
    
    public string android_push_umeng_app_key
    {
        get { return _sdkConfigModule.pushConfigModule.android_push_umeng_app_key; }
        set
        {
            if (_sdkConfigModule != null && _sdkConfigModule.pushConfigModule.android_push_umeng_app_key != value)
            {
                _sdkConfigModule.pushConfigModule.android_push_umeng_app_key = value;
                DirtyEditor();
                SaveConfigSettings();
            }
        }
    }
    
    public string android_push_umeng_app_secret
    {
        get { return _sdkConfigModule.pushConfigModule.android_push_umeng_app_secret; }
        set
        {
            if (_sdkConfigModule != null && _sdkConfigModule.pushConfigModule.android_push_umeng_app_secret != value)
            {
                _sdkConfigModule.pushConfigModule.android_push_umeng_app_secret = value;
                DirtyEditor();
                SaveConfigSettings();
            }
        }
    }
    
    public string android_push_oppo_app_secret
    {
        get { return _sdkConfigModule.pushConfigModule.android_push_oppo_app_secret; }
        set
        {
            if (_sdkConfigModule != null && _sdkConfigModule.pushConfigModule.android_push_oppo_app_secret != value)
            {
                _sdkConfigModule.pushConfigModule.android_push_oppo_app_secret = value;
                DirtyEditor();
                SaveConfigSettings();
            }
        }
    }

    #endregion
    
    #endregion

    
    #region 广告归因配置
    
    #endregion

    
    #region 应用商店评价配置

    #region iOS独有

    public string iOS_rating_app_store_id
    {
        get
        {
            return _sdkConfigModule.ratingConfigModule.iOS_rating_app_store_id;
        }
        set
        {
            if (_sdkConfigModule.ratingConfigModule.iOS_rating_app_store_id != value)
            {
                _sdkConfigModule.ratingConfigModule.iOS_rating_app_store_id = value;
                DirtyEditor();
                SaveConfigSettings();
            }
        }
    }

    #endregion

    #endregion

    
    #region DeepLink配置
    public bool deeplink_enable
    {
        get
        {
            return _sdkConfigModule.deepLinkConfigModule.enable;
        }
        set
        {
            if (_sdkConfigModule.deepLinkConfigModule.enable != value)
            {
                _sdkConfigModule.deepLinkConfigModule.enable = value;
                DirtyEditor();
                SaveConfigSettings();
            }
        }
    }
    
    public int deeplink_schema_listsize
    {
        get
        {
            return _sdkConfigModule.deepLinkConfigModule.schema_listsize;
        }
        set
        {
            if (_sdkConfigModule.deepLinkConfigModule.schema_listsize != value)
            {
                _sdkConfigModule.deepLinkConfigModule.schema_listsize = value;
                DirtyEditor();
                SaveConfigSettings();
            }
        }
    }
    
    public List<string> deeplink_schema_list
    {
        get
        {
            return _sdkConfigModule.deepLinkConfigModule.schema_list;
        }
        set
        {
            if (IsEqual(deeplink_schema_list, value)) return;
            _sdkConfigModule.deepLinkConfigModule.schema_list = value;
            DirtyEditor();
            SaveConfigSettings();
        }
    }
    
    public string deeplink_baseurl_for_fission
    {
        get { return _sdkConfigModule.deepLinkConfigModule.baseurl_for_fission; }
        set
        {
            if (_sdkConfigModule.deepLinkConfigModule.baseurl_for_fission != value)
            {
                _sdkConfigModule.deepLinkConfigModule.baseurl_for_fission = value;
                DirtyEditor();
                SaveConfigSettings();
            }
        }
    }
    
    #region Android独有
    public int android_deeplink_delay_millis
    {
        get
        {
            return _sdkConfigModule.deepLinkConfigModule.android_delay_millis;
        }
        set
        {
            if (_sdkConfigModule.deepLinkConfigModule.android_delay_millis != value)
            {
                _sdkConfigModule.deepLinkConfigModule.android_delay_millis = value;
                DirtyEditor();
                SaveConfigSettings();
            }
        }
    }

    public int android_deeplink_host_listsize
    {
        get
        {
            return _sdkConfigModule.deepLinkConfigModule.android_host_listsize;
        }
        set
        {
            if (_sdkConfigModule.deepLinkConfigModule.android_host_listsize != value)
            {
                _sdkConfigModule.deepLinkConfigModule.android_host_listsize = value;
                DirtyEditor();
                SaveConfigSettings();
            }
        }
    }
    
    public List<string> android_deeplink_host_list
    {
        get
        {
            return _sdkConfigModule.deepLinkConfigModule.android_host_list;
        }
        set
        {
            if (IsEqual(_sdkConfigModule.deepLinkConfigModule.android_host_list, value)) return;
            _sdkConfigModule.deepLinkConfigModule.android_host_list = value;
            DirtyEditor();
            SaveConfigSettings();
        }
    }

    public List<string> android_default_token_regex
    {
        get
        {
            return _sdkConfigModule.deepLinkConfigModule.android_default_token_regex;
        }
        set
        {
            if (IsEqual(_sdkConfigModule.deepLinkConfigModule.android_default_token_regex, value)) return;
            _sdkConfigModule.deepLinkConfigModule.android_default_token_regex = value;
            DirtyEditor();
            SaveConfigSettings();
        }
    }

    #endregion
    #endregion

    #region IM配置

    public bool im_enable
    {
        get { return _sdkConfigModule.imConfigModule.enable; }
        set
        {
            if (_sdkConfigModule.imConfigModule.enable != value)
            {
                _sdkConfigModule.imConfigModule.enable = value;
                DirtyEditor();
                SaveConfigSettings();
            }
        }
    }

    public string im_appid
    {
        get
        {
            return string.IsNullOrEmpty(_sdkConfigModule.imConfigModule.aid) ? "0" : _sdkConfigModule.imConfigModule.aid;
        }
        set
        {
            if (_sdkConfigModule.imConfigModule.aid != value)
            {
                _sdkConfigModule.imConfigModule.aid = value;
                DirtyEditor();
                SaveConfigSettings();
            }
        }
    }

    public string im_frontier_method_key
    {
        get
        {
            return string.IsNullOrEmpty(_sdkConfigModule.imConfigModule.method) ? "0" : _sdkConfigModule.imConfigModule.method;
        }
        set
        {
            if (_sdkConfigModule.imConfigModule.method != value)
            {
                _sdkConfigModule.imConfigModule.method = value;
                DirtyEditor();
                SaveConfigSettings();
            }
        }
    }
    
    public string im_frontier_service_key
    {
        get
        {
            return string.IsNullOrEmpty(_sdkConfigModule.imConfigModule.service) ? "0" : _sdkConfigModule.imConfigModule.service;
        }
        set
        {
            if (_sdkConfigModule.imConfigModule.service != value) {
                _sdkConfigModule.imConfigModule.service = value;
                DirtyEditor ();
                SaveConfigSettings ();
            }
        }
    }

    public string im_frontier_boe_baseurl
    {
        get { return _sdkConfigModule.imConfigModule.ws_host_boe; }
        set
        {
            if (_sdkConfigModule.imConfigModule.ws_host_boe != value)
            {
                _sdkConfigModule.imConfigModule.ws_host_boe = value;
                DirtyEditor();
                SaveConfigSettings();
            }
        }
    }
    
    public string im_frontier_baseurl
    {
        get { return _sdkConfigModule.imConfigModule.ws_host; }
        set
        {
            if (_sdkConfigModule.imConfigModule.ws_host != value) {
                _sdkConfigModule.imConfigModule.ws_host = value;
                DirtyEditor ();
                SaveConfigSettings ();
            }
        }

    }

    public List<int> im_inboxes
    {
        get { return _sdkConfigModule.imConfigModule.inboxes; }
        set
        {
            if (IsEqual(_sdkConfigModule.imConfigModule.inboxes, value)) return;
            _sdkConfigModule.imConfigModule.inboxes = value;
            DirtyEditor();
            SaveConfigSettings();
        }
    }

    public string im_frontier_id
    {
        get
        {
            return string.IsNullOrEmpty(_sdkConfigModule.imConfigModule.fpid) ? "0" : _sdkConfigModule.imConfigModule.fpid;
        }
        set
        {
            if (_sdkConfigModule.imConfigModule.fpid != value)
            {
                _sdkConfigModule.imConfigModule.fpid = value;
                DirtyEditor();
                SaveConfigSettings();
            }
        }
    }

    public string im_http_baseurl
    {
        get { return _sdkConfigModule.imConfigModule.http_host; }
        set
        {
            if (_sdkConfigModule.imConfigModule.http_host != value)
            {
                _sdkConfigModule.imConfigModule.http_host = value;
                DirtyEditor();
                SaveConfigSettings();
            }
        }
    }

    public string im_http_boe_baseurl
    {
        get { return _sdkConfigModule.imConfigModule.http_host_boe; }
        set
        {
            if (_sdkConfigModule.imConfigModule.http_host_boe != value)
            {
                _sdkConfigModule.imConfigModule.http_host_boe = value;
                DirtyEditor();
                SaveConfigSettings();
            }
        }
    }

    public string im_frontier_app_key
    {
        get { return _sdkConfigModule.imConfigModule.app_key; }
        set
        {
            if (_sdkConfigModule.imConfigModule.app_key != value)
            {
                _sdkConfigModule.imConfigModule.app_key = value;
                DirtyEditor();
                SaveConfigSettings();
            }
        }
    }

    #endregion


    #region 直播配置
    public bool live_enable
    {
        get { return _sdkConfigModule.liveConfigModule.enable; }
        set
        {
            if (_sdkConfigModule.liveConfigModule.enable != value)
            {
                _sdkConfigModule.liveConfigModule.enable = value;
                DirtyEditor();
                SaveConfigSettings();
            }
        }
    }

    #region Android独有

    public string android_douyin_key
    {
        get { return _sdkConfigModule.liveConfigModule.android_douyin_key; }
        set
        {
            if (_sdkConfigModule.liveConfigModule.android_douyin_key != value)
            {
                _sdkConfigModule.liveConfigModule.android_douyin_key = value;
                DirtyEditor();
                SaveConfigSettings();
            }
        }
    }

    #endregion
    
    #region iOS独有

    public string iOS_live_scheme
    {
        get { return _sdkConfigModule.liveConfigModule.iOS_live_scheme; }
        set
        {
            if (_sdkConfigModule.liveConfigModule.iOS_live_scheme != value)
            {
                _sdkConfigModule.liveConfigModule.iOS_live_scheme = value;
                DirtyEditor();
                SaveConfigSettings();
            }
        }
    }
    
    public string iOS_kh_id
    {
        get { return _sdkConfigModule.liveConfigModule.iOS_kh_id; }
        set
        {
            if (_sdkConfigModule.liveConfigModule.iOS_kh_id != value)
            {
                _sdkConfigModule.liveConfigModule.iOS_kh_id = value;
                DirtyEditor();
                SaveConfigSettings();
            }
        }
    }
    
    public string iOS_kh_schema
    {
        get { return _sdkConfigModule.liveConfigModule.iOS_kh_scheme; }
        set
        {
            if (_sdkConfigModule.liveConfigModule.iOS_kh_scheme != value)
            {
                _sdkConfigModule.liveConfigModule.iOS_kh_scheme = value;
                DirtyEditor();
                SaveConfigSettings();
            }
        }
    }

    #endregion

#endregion

    
    #region RTC配置

    public bool rtc_available
    {
        get
        {
            return _sdkConfigModule.rtcConfigModule.rtc_available;
        }
        set
        {
            if (_sdkConfigModule.rtcConfigModule.rtc_available != value)
            {
                _sdkConfigModule.rtcConfigModule.rtc_available = value;
                DirtyEditor();
                SaveConfigSettings();
            }
        }
    }

    public string rtc_app_id
    {
        get
        {
            return _sdkConfigModule.rtcConfigModule.rtc_app_id;
        }
        set
        {
            if (_sdkConfigModule.rtcConfigModule.rtc_app_id != value)
            {
                _sdkConfigModule.rtcConfigModule.rtc_app_id = value;
                DirtyEditor();
                SaveConfigSettings();
            }
        }
    }

    #endregion

    
    #region 广告付费配置

    public bool pangle_enable
    {
        get
        {
            return _sdkConfigModule.adConfigModule.pangleConfigModule.enable;
        }
        set
        {
            if (_sdkConfigModule.adConfigModule.pangleConfigModule.enable!=value)
            {
                _sdkConfigModule.adConfigModule.pangleConfigModule.enable = value;
                DirtyEditor();
                SaveConfigSettings();
            }
        }

    }

    public string pangle_appid
    {
        get { return _sdkConfigModule.adConfigModule.pangleConfigModule.pangle_appid; }
        set
        {
            if (_sdkConfigModule.adConfigModule.pangleConfigModule.pangle_appid != value)
            {
                _sdkConfigModule.adConfigModule.pangleConfigModule.pangle_appid = value;
                DirtyEditor();
                SaveConfigSettings();
            }
        }
    }
    #endregion

    
    #region Thanos配置
    #region Android独有

    public bool android_save_sub_channel
    {
        get
        {
            return _sdkConfigModule.thanosConfigModule.android_save_sub_channel;
        }
        
        set
        {
            if (_sdkConfigModule.thanosConfigModule.android_save_sub_channel != value)
            {
                _sdkConfigModule.thanosConfigModule.android_save_sub_channel = value;
                DirtyEditor();
                SaveConfigSettings();
            }
        }
    }

    #endregion
    #endregion


    //...
    protected void DirtyEditor()
    {
        EditorUtility.SetDirty(Instance);
    }

    public void SaveConfigSettings()
    {
        if (!Directory.Exists(env.PATH_CONFIG_SETTINGS))
        {
            Directory.CreateDirectory(env.PATH_CONFIG_SETTINGS);
        }

        StringBuilder sb = new StringBuilder();
        JsonWriter writer = new JsonWriter(sb);
        writer.PrettyPrint = true;
        writer.IndentValue = 4;
        JsonMapper.ToJson(_sdkConfigModule, writer);
        File.WriteAllText(SAVE_CONFIG_FILE, sb.ToString());
        
        androidSaveConfigToPluginAssets();
    }

    // 通用配置，旧module转新module
    protected void initCommonModule(AppConfig androidAppConfig, ConfigModel iOSConfigModel)
    {
        //通用
        string appId = _sdkConfigModule.commonModule.app_id;
        string appName = _sdkConfigModule.commonModule.app_name;
        string initTimeOut = _sdkConfigModule.commonModule.init_timeout;
        bool usePacketChannel = _sdkConfigModule.commonModule.use_packet_channel;
        string skin = _sdkConfigModule.commonModule.skin;
        bool isNeedAntiAddiction = _sdkConfigModule.commonModule.is_need_anti_addiction;
        bool isNeedServiceMouth = _sdkConfigModule.commonModule.is_need_service_mouth;
       
        if (androidAppConfig != null && iOSConfigModel != null)
        {
            appId = universalValueOpe(androidAppConfig.app_id, iOSConfigModel.appId);
            appName = universalValueOpe(androidAppConfig.app_name, iOSConfigModel.appName);
            initTimeOut = universalValueOpe(androidAppConfig.gsdk_init_timeout.ToString(),
                (iOSConfigModel.gsdk_init_timeout * 1000).ToString());
            usePacketChannel = androidAppConfig.use_packet_channel;
            skin = androidAppConfig.skin;
            isNeedAntiAddiction = androidAppConfig.is_need_anti_addiction;
            isNeedServiceMouth = androidAppConfig.is_need_service_mouth;
        }
        else if (androidAppConfig != null || iOSConfigModel != null)
        {
            appId = androidAppConfig != null ? androidAppConfig.app_id : iOSConfigModel.appId;
            appName = androidAppConfig != null ? androidAppConfig.app_name : iOSConfigModel.appName;
            initTimeOut = androidAppConfig != null
                ? androidAppConfig.gsdk_init_timeout.ToString()
                : (iOSConfigModel.gsdk_init_timeout * 1000).ToString();
            usePacketChannel = androidAppConfig != null
                ? androidAppConfig.use_packet_channel
                : iOSConfigModel.usePacketChannel;
            skin = androidAppConfig != null ? androidAppConfig.skin : iOSConfigModel.Skin;
            isNeedAntiAddiction = androidAppConfig != null ? androidAppConfig.is_need_anti_addiction : iOSConfigModel.IsNeedAntiAddiction;
            isNeedServiceMouth = androidAppConfig != null ? androidAppConfig.is_need_service_mouth : iOSConfigModel.IsNeedServiceMouth;
        }
        _sdkConfigModule.commonModule.app_id = appId;
        _sdkConfigModule.commonModule.app_name = appName;
        _sdkConfigModule.commonModule.init_timeout = initTimeOut;
        _sdkConfigModule.commonModule.use_packet_channel = usePacketChannel;
            _sdkConfigModule.commonModule.skin = skin;
            _sdkConfigModule.commonModule.is_need_anti_addiction = isNeedAntiAddiction;
            _sdkConfigModule.commonModule.is_need_service_mouth = isNeedServiceMouth;

        if (androidAppConfig != null)
        {
            //android特有
            _sdkConfigModule.commonModule.package_name = androidAppConfig.package_name;
            _sdkConfigModule.commonModule.android_screen_orientation = androidAppConfig.screen_orientation;
            _sdkConfigModule.commonModule.android_main_activity = androidAppConfig.main_activity;
            _sdkConfigModule.commonModule.android_open_launch_trace = androidAppConfig.open_launch_trace;
            _sdkConfigModule.commonModule.android_clean_app = androidAppConfig.clean_app;
            _sdkConfigModule.commonModule.android_has_splash = androidAppConfig.has_splash;
            _sdkConfigModule.commonModule.android_splash_interval = androidAppConfig.splash_interval;
        }

        if (iOSConfigModel != null)
        {
            //iOS特有
            _sdkConfigModule.commonModule.iOS_bundleId = iOSConfigModel.bundleId;
            _sdkConfigModule.commonModule.iOS_app_display_name = iOSConfigModel.appDisplayName;
            _sdkConfigModule.commonModule.iOS_import_magicbox = iOSConfigModel.importMagicBox;
            _sdkConfigModule.commonModule.iOS_dsym_auto_upload = iOSConfigModel.dsymAutoUpload;
            _sdkConfigModule.commonModule.iOS_dsym_upload_key = iOSConfigModel.dsymUploadKey;
            _sdkConfigModule.commonModule.iOS_project_target_name = iOSConfigModel.projectTargetName;
            _sdkConfigModule.commonModule.iOS_framework_target_name = iOSConfigModel.frameworkTargetName;
            _sdkConfigModule.commonModule.iOS_associated_domains_list_size = iOSConfigModel.AssociatedDomainsListSize;
            _sdkConfigModule.commonModule.iOS_associated_domains_list = iOSConfigModel.AssociatedDomainsList;
            _sdkConfigModule.commonModule.iOS_cert_success = iOSConfigModel.iOSCertSuccess;
            _sdkConfigModule.commonModule.iOS_provisioning_guid = iOSConfigModel.iOSProvisioningGUID;
            _sdkConfigModule.commonModule.iOS_team_ID = iOSConfigModel.iOSTeamID;
            _sdkConfigModule.commonModule.iOS_cert_name = iOSConfigModel.iOSCertName;
            _sdkConfigModule.commonModule.iOS_debug_cert_success = iOSConfigModel.iOSDebugCertSuccess;
            _sdkConfigModule.commonModule.iOS_debug_provisioning_guid = iOSConfigModel.iOSDebugProvisioningGUID;
            _sdkConfigModule.commonModule.iOS_debug_team_ID = iOSConfigModel.iOSDebugTeamID;
            _sdkConfigModule.commonModule.iOS_debug_cert_name = iOSConfigModel.iOSDebugCertName;
            _sdkConfigModule.commonModule.iOS_own_url_scheme_list = new List<string> {iOSConfigModel.ownURLScheme};
            _sdkConfigModule.commonModule.needModifyInfoPlist = iOSConfigModel.needModifyInfoPlist;
            _sdkConfigModule.commonModule.modifyInfoPlistReplaceWhenMetSame = iOSConfigModel.modifyInfoPlistReplaceWhenMetSame;
            if (string.IsNullOrEmpty(iOSConfigModel.ownURLScheme))
            {
                _sdkConfigModule.commonModule.iOS_own_url_scheme_list_size = 0;
            }
            else
            {
                _sdkConfigModule.commonModule.iOS_own_url_scheme_list_size = 1;
            }

            if (string.IsNullOrEmpty(iOSConfigModel.AppSchemeArray))
            {
                _sdkConfigModule.commonModule.iOS_application_queries_schemes_list_size = 0;
            }
            else
            {
                string[] strArray = iOSConfigModel.AppSchemeArray.Split (' ');
                List<string> strList = new List<string>();
                for (int i = 0; i < strArray.Length; i++)
                {
                    strList.Add(strArray[i]);
                }
                _sdkConfigModule.commonModule.iOS_application_queries_schemes_list = strList;
                _sdkConfigModule.commonModule.iOS_application_queries_schemes_list_size = strArray.Length;
            }
        }
    }

    // 隐私安全，旧module转新module
    protected void initPrivacySecurityConfigModule(AppConfig androidAppConfig, ConfigModel iOSConfigModel)
    {
        //通用
        if (androidAppConfig != null)
        {
            _sdkConfigModule.privacySecurityConfigModule.is_show_permission_toast = androidAppConfig.is_show_toast;
        }
        else if (iOSConfigModel != null)
        {
              _sdkConfigModule.privacySecurityConfigModule.is_show_permission_toast = iOSConfigModel.EnableSDKAuthPoping;
        }

        if (androidAppConfig != null)
        {
            //android特有
           
            _sdkConfigModule.privacySecurityConfigModule.android_is_need_privacy_protection = androidAppConfig.is_need_privacy_protection;
           
        }

        if (iOSConfigModel != null)
        {
            //iOS特有
            _sdkConfigModule.privacySecurityConfigModule.iOS_privacy_microphone_usage_available = iOSConfigModel.privacyMicrophoneUsageAvailable;
            _sdkConfigModule.privacySecurityConfigModule.iOS_privacy_microphone_usage_desc = iOSConfigModel.privacyMicrophoneUsageDesc;
            _sdkConfigModule.privacySecurityConfigModule.iOS_privacy_photo_usage_available = iOSConfigModel.privacyPhotoUsageAvailable;
            _sdkConfigModule.privacySecurityConfigModule.iOS_privacy_photo_usage_desc = iOSConfigModel.privacyPhotoUsageDesc;
            _sdkConfigModule.privacySecurityConfigModule.iOS_privacy_photo_additions_usage_available = iOSConfigModel.privacyPhotoAdditionsAvailable;
            _sdkConfigModule.privacySecurityConfigModule.iOS_privacy_photo_additions_usage_desc = iOSConfigModel.privacyPhotoAdditionsDesc;
            _sdkConfigModule.privacySecurityConfigModule.iOS_privacy_camera_usage_available = iOSConfigModel.privacyCameraUsageAvailable;
            _sdkConfigModule.privacySecurityConfigModule.iOS_privacy_camera_usage_desc = iOSConfigModel.privacyCameraUsageDesc;
            _sdkConfigModule.privacySecurityConfigModule.iOS_privacy_contacts_usage_available = iOSConfigModel.privacyContactsUsageAvailable;
            _sdkConfigModule.privacySecurityConfigModule.iOS_privacy_contacts_usage_desc = iOSConfigModel.privacyContactsUsageDesc;
            _sdkConfigModule.privacySecurityConfigModule.iOS_privacy_app_tracking_usage_available = iOSConfigModel.privacyAppTrackingUsageAvailable;
            _sdkConfigModule.privacySecurityConfigModule.iOS_privacy_app_tracking_usage_desc = iOSConfigModel.privacyAppTrackingUsageDesc;
            _sdkConfigModule.privacySecurityConfigModule.iOS_privacy_location_usage_available = iOSConfigModel.privacyLocationUsageAvailable;
            _sdkConfigModule.privacySecurityConfigModule.iOS_privacy_location_usage_desc = iOSConfigModel.privacyLocationUsageDesc;
            _sdkConfigModule.privacySecurityConfigModule.iOS_privacy_location_always_usage_available = iOSConfigModel.privacyLocationAlwaysUsageAvailable;
            _sdkConfigModule.privacySecurityConfigModule.iOS_privacy_location_always_usage_desc = iOSConfigModel.privacyLocationAlwaysUsageDesc;
            _sdkConfigModule.privacySecurityConfigModule.iOS_privacy_calendars_usage_available = iOSConfigModel.privacyCalendarsUsageAvailable;
            _sdkConfigModule.privacySecurityConfigModule.iOS_privacy_calendars_usage_desc = iOSConfigModel.privacyCalendarsUsageDesc;
            _sdkConfigModule.privacySecurityConfigModule.iOS_privacy_reminders_usage_available = iOSConfigModel.privacyRemindersUsageaAvailable;
            _sdkConfigModule.privacySecurityConfigModule.iOS_privacy_reminders_usage_desc = iOSConfigModel.privacyRemindersUsageDesc;
            _sdkConfigModule.privacySecurityConfigModule.iOS_privacy_speech_recognition_usage_available = iOSConfigModel.privacySpeechRecognitionUsageAvailable;
            _sdkConfigModule.privacySecurityConfigModule.iOS_privacy_speech_recognition_usage_desc = iOSConfigModel.privacySpeechRecognitionUsageDesc;
            _sdkConfigModule.privacySecurityConfigModule.iOS_privacy_siri_usage_available = iOSConfigModel.privacySiriUsageAvailable;
            _sdkConfigModule.privacySecurityConfigModule.iOS_privacy_siri_usage_desc = iOSConfigModel.privacySiriUsageDesc;
            _sdkConfigModule.privacySecurityConfigModule.iOS_privacy_face_ID_usage_available = iOSConfigModel.privacyFaceIDUsageAvailable;
            _sdkConfigModule.privacySecurityConfigModule.iOS_privacy_face_ID_usage_desc = iOSConfigModel.privacyFaceIDUsageDesc;
            

            #region ATT idfa弹窗
            
            _sdkConfigModule.privacySecurityConfigModule.iOS_is_ATT_available = iOSConfigModel.ATTAvailable;
            _sdkConfigModule.privacySecurityConfigModule.iOS_is_need_DIY_call = iOSConfigModel.needCustomCall;
            _sdkConfigModule.privacySecurityConfigModule.iOS_is_use_CP_DIY_view = iOSConfigModel.useCPPopup;
            _sdkConfigModule.privacySecurityConfigModule.iOS_is_need_gsdk_defalut_view = iOSConfigModel.needCustomDefalutView;
            _sdkConfigModule.privacySecurityConfigModule.iOS_gsdk_defalut_view_cancel_title_color = iOSConfigModel.CancelTitleColor;
            _sdkConfigModule.privacySecurityConfigModule.iOS_gsdk_defalut_view_cancel_color = iOSConfigModel.CancelColor;
            _sdkConfigModule.privacySecurityConfigModule.iOS_gsdk_defalut_view_cancel_title = iOSConfigModel.CancelTitle;
            _sdkConfigModule.privacySecurityConfigModule.iOS_gsdk_defalut_view_confirm_title_color = iOSConfigModel.ConfirmTitleColor;
            _sdkConfigModule.privacySecurityConfigModule.iOS_gsdk_defalut_view_confirm_color = iOSConfigModel.ConfirmColor;
            _sdkConfigModule.privacySecurityConfigModule.iOS_gsdk_defalut_view_confirm_title = iOSConfigModel.ConfirmTitle;
            _sdkConfigModule.privacySecurityConfigModule.iOS_gsdk_defalut_view_content = iOSConfigModel.Content;
            _sdkConfigModule.privacySecurityConfigModule.iOS_gsdk_defalut_view_title = iOSConfigModel.Title;

            #endregion
        }
    }

    // 应用商店评价，旧module转新module
    public void initRatingConfigModule(ConfigModel iOSConfigModel)
    {
        if (iOSConfigModel != null)
        {
            _sdkConfigModule.ratingConfigModule.iOS_rating_app_store_id = iOSConfigModel.RatingAppStoreID;
        }
    }
    
    protected void initPushModule(AppConfig androidAppConfig, ConfigModel iOSConfigModel)
    {
        //通用
        if (androidAppConfig != null || iOSConfigModel != null)
        {
            _sdkConfigModule.pushConfigModule.push_enable = androidAppConfig != null ? androidAppConfig.push.enable : iOSConfigModel.pushAvailable;
        }
        if (androidAppConfig != null)
        {
            //android特有
            _sdkConfigModule.pushConfigModule.android_push_app_name = androidAppConfig.push.push_app_name;
        }
    }

    protected void initIMModule(AppConfig androidAppConfig, ConfigModel iOSConfigModel)
    {
        //通用
        bool enable = _sdkConfigModule.imConfigModule.enable;
        string aid = _sdkConfigModule.imConfigModule.aid;
        string method = _sdkConfigModule.imConfigModule.method;
        string service = _sdkConfigModule.imConfigModule.service;
        string wsHostBoe = _sdkConfigModule.imConfigModule.ws_host_boe;
        string ws_host = _sdkConfigModule.imConfigModule.ws_host;
        string fpid = _sdkConfigModule.imConfigModule.fpid;
        string http_host = _sdkConfigModule.imConfigModule.http_host;
        string http_host_boe = _sdkConfigModule.imConfigModule.http_host_boe;
        string app_key = _sdkConfigModule.imConfigModule.app_key;
        List<int> inboxes = _sdkConfigModule.imConfigModule.inboxes;
        List<int> androidInbox = new List<int>();
        List<int> iOSInbox = new List<int>();
        if (androidAppConfig != null && iOSConfigModel != null)
        {
            enable = androidAppConfig.im.enable;
            aid = universalValueOpe(androidAppConfig.im.aid.ToString(), iOSConfigModel.IMAppID);
            method = universalValueOpe(androidAppConfig.im.method.ToString(), iOSConfigModel.IMFrontierMethodKey);
            service = universalValueOpe(androidAppConfig.im.service.ToString(), iOSConfigModel.IMFrontierServiceKey);
            wsHostBoe = universalValueOpe(androidAppConfig.im.wsHostBoe, iOSConfigModel.IMFrontierBOEBaseURL);
            ws_host = universalValueOpe(androidAppConfig.im.wsHost, iOSConfigModel.IMFrontierBaseURL);
            fpid = universalValueOpe(androidAppConfig.im.fpid.ToString(), iOSConfigModel.IMFrontierID);
            http_host = universalValueOpe(androidAppConfig.im.httpHost, iOSConfigModel.IMHTTPBaseURL);
            http_host_boe = universalValueOpe(androidAppConfig.im.httpHostBoe, iOSConfigModel.IMHTTPBOEBaseURL);
            app_key = universalValueOpe(androidAppConfig.im.appKey, iOSConfigModel.IMFrontierAppKey);
        }
        else if (androidAppConfig != null || iOSConfigModel != null)
        {
            enable = androidAppConfig != null ? androidAppConfig.im.enable : !string.IsNullOrEmpty(iOSConfigModel.IMAppID);
            aid = androidAppConfig != null ? androidAppConfig.im.aid.ToString() : iOSConfigModel.IMAppID;
            method = androidAppConfig != null ? androidAppConfig.im.method.ToString() : iOSConfigModel.IMFrontierMethodKey;
            service = androidAppConfig != null ? androidAppConfig.im.service.ToString() : iOSConfigModel.IMFrontierServiceKey;
            wsHostBoe = androidAppConfig != null ? androidAppConfig.im.wsHostBoe : iOSConfigModel.IMFrontierBOEBaseURL;
            ws_host =  androidAppConfig != null ? androidAppConfig.im.wsHost : iOSConfigModel.IMFrontierBaseURL;
            fpid = androidAppConfig != null ? androidAppConfig.im.fpid.ToString() : iOSConfigModel.IMFrontierID;
            http_host = androidAppConfig != null ? androidAppConfig.im.httpHost: iOSConfigModel.IMHTTPBaseURL;
            http_host_boe = androidAppConfig != null ? androidAppConfig.im.httpHostBoe: iOSConfigModel.IMHTTPBOEBaseURL;
            app_key = androidAppConfig != null ? androidAppConfig.im.appKey: iOSConfigModel.IMFrontierAppKey;
        }

        if (androidAppConfig != null)
        {
            androidInbox = androidAppConfig.im.inboxes != null ? androidAppConfig.im.inboxes : new List<int>();
        }
        if (iOSConfigModel != null)
        {
            string[] IMInboxesTempArray = iOSConfigModel.IMInboxes.Split (' ');
            List<string> IMInboxesTempList = new List<string>(IMInboxesTempArray);
            foreach (var tempString in IMInboxesTempList)
            {
                if (!string.IsNullOrEmpty(tempString))
                {
                    iOSInbox.Add(int.Parse(tempString));
                }
            }
        }

        if (androidInbox.Count != 0 && iOSInbox.Count == 0)
        {
            inboxes = androidInbox;
        }
        else if (androidInbox.Count == 0 && iOSInbox.Count != 0)
        {
            inboxes = iOSInbox;
        }
        else if (androidInbox.Count != 0 && iOSInbox.Count != 0)
        {
            inboxes = androidInbox.Union(iOSInbox).ToList<int>();
        }

        _sdkConfigModule.imConfigModule.enable = enable;
        _sdkConfigModule.imConfigModule.aid = aid;
        _sdkConfigModule.imConfigModule.method = method;
        _sdkConfigModule.imConfigModule.service = service;
        _sdkConfigModule.imConfigModule.ws_host_boe = wsHostBoe;
        _sdkConfigModule.imConfigModule.ws_host = ws_host;
        _sdkConfigModule.imConfigModule.fpid = fpid;
        _sdkConfigModule.imConfigModule.http_host = http_host;
        _sdkConfigModule.imConfigModule.http_host_boe = http_host_boe;
        _sdkConfigModule.imConfigModule.app_key = app_key;
        _sdkConfigModule.imConfigModule.inboxes = inboxes;
    }
    
    // RTC
    public void initRTCConfigModule(AppConfig androidAppConfig, ConfigModel iOSConfigModel)
    {
        string rtcAppId = _sdkConfigModule.rtcConfigModule.rtc_app_id;
        bool enable = _sdkConfigModule.rtcConfigModule.rtc_available;
        if (androidAppConfig != null && iOSConfigModel != null)
        {
            rtcAppId = universalValueOpe(androidAppConfig.rtc.rtcAppId, iOSConfigModel.RTCAppId);
            enable = androidAppConfig.rtc.enable;
        }
        else if (androidAppConfig != null || iOSConfigModel != null)
        {
            rtcAppId = androidAppConfig != null ? androidAppConfig.rtc.rtcAppId : iOSConfigModel.RTCAppId;
            enable = androidAppConfig != null ? androidAppConfig.rtc.enable : iOSConfigModel.RTCAvailable;
        }
        _sdkConfigModule.rtcConfigModule.rtc_available = enable;
        _sdkConfigModule.rtcConfigModule.rtc_app_id = rtcAppId;
    }
    
    // 分享
    public void initShareConfigModule(AppConfig androidAppConfig, ConfigModel iOSConfigModel)
    {
        //通用
        string shareDimAmount = _sdkConfigModule.shareConfigModule.share_dim_amount;
        bool shareDimAmountAvailable = _sdkConfigModule.shareConfigModule.share_dim_amount_available;
        if (androidAppConfig != null && iOSConfigModel != null)
        {
            shareDimAmount = universalValueOpe(androidAppConfig.share.dimAmount, iOSConfigModel.shareDimAmount);
            shareDimAmountAvailable = !string.IsNullOrEmpty(androidAppConfig.share.dimAmount);
        }
        else if (androidAppConfig != null || iOSConfigModel != null)
        {
            shareDimAmount = androidAppConfig != null ? androidAppConfig.share.dimAmount : iOSConfigModel.shareDimAmount;
            shareDimAmountAvailable = androidAppConfig != null ?  !string.IsNullOrEmpty(androidAppConfig.share.dimAmount) : iOSConfigModel.shareDimAmountAvailable;
        }

        _sdkConfigModule.shareConfigModule.share_dim_amount = shareDimAmount;
        _sdkConfigModule.shareConfigModule.share_dim_amount_available = shareDimAmountAvailable;

        if (androidAppConfig != null)
        {
            //android特有
            _sdkConfigModule.shareConfigModule.android_share_panel_id = androidAppConfig.share.panelId;
        }

        if (iOSConfigModel != null)
        {
            //iOS特有
            _sdkConfigModule.shareConfigModule.iOS_share_panel_id = iOSConfigModel.sharePanelID;
        }
    }

    public abstract void androidSaveConfigToPluginAssets();
    
    private bool IsEqual<T>(List<T> first, List<T> second)
    {
        if (first == null)
        {
            return second == null;
        }
        return second == null || first.SequenceEqual(second);
    }
    
    public bool login_panel_can_be_closed
    {
        get
        {
            return _sdkConfigModule.loginConfigModule.login_panel_can_be_closed;
        }
        set
        {
            if (_sdkConfigModule.loginConfigModule.login_panel_can_be_closed != value)
            {
                _sdkConfigModule.loginConfigModule.login_panel_can_be_closed = value;
                DirtyEditor();
                SaveConfigSettings();
            }
        }
    }
    
    public string panel_language
    {
        get
        {
            return _sdkConfigModule.pannelSettingsModule.panel_language;
        }
        set
        {
            if (_sdkConfigModule.pannelSettingsModule.panel_language != value)
            {
                _sdkConfigModule.pannelSettingsModule.panel_language = value;
                DirtyEditor();
                SaveConfigSettings();
            }
        }
    }

    /**
     * 非通用逻辑，就是为了3702版本一个输入框多套参数的兜底逻辑，将long和int转成string类型
     */
    private string dealConfigSettingJson(string configSettingsFile)
    {
        JsonData jsonData = JsonMapper.ToObject(configSettingsFile);
        if (jsonData.ContainsKey("commonModule") && jsonData["commonModule"].ContainsKey("init_timeout") &&
            !jsonData["commonModule"]["init_timeout"].IsString)
        {
            jsonData["commonModule"]["init_timeout"] = jsonData["commonModule"]["init_timeout"].ToString();
        }

        //share
        if (jsonData.ContainsKey("shareConfigModule"))
        {
            if (jsonData["shareConfigModule"].ContainsKey("share_dim_amount") &&
                !jsonData["shareConfigModule"]["share_dim_amount"].IsString)
            {
                jsonData["shareConfigModule"]["share_dim_amount"] =
                    jsonData["shareConfigModule"]["share_dim_amount"].ToString();
            }
        }
        
        //IM
        if (jsonData.ContainsKey("imConfigModule"))
        {
            if (jsonData["imConfigModule"].ContainsKey("aid") &&
                !jsonData["imConfigModule"]["aid"].IsString)
            {
                jsonData["imConfigModule"]["aid"] = jsonData["imConfigModule"]["aid"].ToString();
            }

            if (jsonData["imConfigModule"].ContainsKey("method") && !jsonData["imConfigModule"]["method"].IsString)
            {
                jsonData["imConfigModule"]["method"] = jsonData["imConfigModule"]["method"].ToString();
            }

            if (jsonData["imConfigModule"].ContainsKey("service") && !jsonData["imConfigModule"]["service"].IsString)
            {
                jsonData["imConfigModule"]["service"] = jsonData["imConfigModule"]["service"].ToString();
            }
            
            if (jsonData["imConfigModule"].ContainsKey("fpid") && !jsonData["imConfigModule"]["fpid"].IsString)
            {
                jsonData["imConfigModule"]["fpid"] = jsonData["imConfigModule"]["fpid"].ToString();
            }
        }

        return jsonData.ToJson();
    }

    /**
     * 判断通用值androidValue是否等于iOSValue,如果不相等则使用"｜"进行拼接
     */
    protected string universalValueOpe(string androidValue, string iOSValue)
    {
        if (!string.IsNullOrEmpty(androidValue) && !string.IsNullOrEmpty(iOSValue))
        {
            if (androidValue == iOSValue)
            {
                return androidValue;
            }

            return androidValue + ConfigSettingParamHandleUtils.Delimiter + iOSValue;
        }

        if (!string.IsNullOrEmpty(androidValue))
        {
            return androidValue;
        }

        return iOSValue;
    }
    
     //负责拉取GDev上的配置参数
   public void getParametersOnGDev(Action<PullResult> responseHandler)
   {
       //判断Assets/GSDK/gsdk.json文件是否存在
       string properPath = env.PATH_ASSETS_GSDK;
       if (Directory.Exists(properPath))
       {
           string fullPath = Path.Combine(properPath, "gsdk.json");
           if (File.Exists(fullPath))
           {
               Debug.Log("gsdk.json file exist");
               JsonData jsonData = JsonMapper.ToObject(File.ReadAllText(fullPath));
               if (jsonData == null)
               {
                   Debug.Log("gsdk.json parse failed");
                   responseHandler(new PullResult(PullResult.PullError, "gsdk.json parse failed", null));
                   return;
               }

               if (!jsonData.ContainsKey("appid"))
               {
                   if (string.IsNullOrEmpty(_sdkConfigModule.commonModule.app_id))
                   {
                       Debug.LogError("Please enter AppID");
                       responseHandler(new PullResult(PullResult.PullError, "Please enter AppID", null));
                       return;
                   }
                   jsonData["appid"] = _sdkConfigModule.commonModule.app_id;
               }

               PullGDevParamesTools.PullParams(jsonData.ToJson(),
                   responseHandler);
           }
           else
           {
               responseHandler(new PullResult(PullResult.PullError, "gsdk.json does not exist", null));
               Debug.Log("gsdk.json does not exist");
           }
       }
       else
       {
           responseHandler(new PullResult(PullResult.PullError, properPath + "does not exist", null));
           Debug.Log(properPath + "does not exist");
       }
   }

   //解析GSDK-Hub下发的参数
   private string parseGSDKHubConfig()
   {
       string sdkConfigModuleHub = null;
       try
       {
           string fullPath = Path.Combine(env.PATH_CONFIG_SETTINGS, SDKSettingConfigNameGSDKHub);
           if (System.IO.File.Exists(fullPath))
           {
               sdkConfigModuleHub = File.ReadAllText(fullPath);
           }

           //删除SDKSettingConfigNameGSDKHub
           File.Delete(fullPath);
       }
       catch (Exception exception)
       {
           Debug.LogError(exception.Message);
       }

       return sdkConfigModuleHub;
   }

}
