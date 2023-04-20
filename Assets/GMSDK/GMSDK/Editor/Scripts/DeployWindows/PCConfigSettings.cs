using System.IO;
using System.Text;
using UNBridgeLib.LitJson;
using UnityEditor;
using UnityEngine;

namespace GSDK.UnityEditor
{
    public class PCConfigSettings : ScriptableObject
    {
        public static readonly string CONFIG_FILE_FOLDER = Path.Combine(GMSDKEnv.Instance.PATH_CONFIG_SETTINGS, "PC");
        public static readonly string CONFIG_FILE = Path.Combine(CONFIG_FILE_FOLDER, "config.json");
        private static PCConfigSettings _instance;
        private PCConfig config;

        class PCConfig
        {
            public AppConfig app;
            public PCConfig()
            {
                app = new AppConfig();
            }
        }

        struct AppConfig
        {
            public string app_id;
            public string app_name;
            public string display_name;
            public string package_name;
        }

        internal PCConfigSettings()
        {
            config = new PCConfig();
            if (File.Exists(CONFIG_FILE))
            {
                string json = File.ReadAllText(CONFIG_FILE);
                config = JsonMapper.ToObject<PCConfig>(json);
            }
        }

        public static PCConfigSettings Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = CreateInstance<PCConfigSettings>();
                }
                return _instance;
            }
        }

 
        public string AppID
        {
            set
            {
                config.app.app_id = value;
                DirtyEditor();
                SavePCConfig();
            }
            get
            {
                return config.app.app_id;
            }
        }

        public string AppName
        {
            set
            {
                config.app.app_name = value;
                DirtyEditor();
                SavePCConfig();
            }
            get
            {
                return config.app.app_name;
            }
        }

        public string Displayname
        {
            set
            {
                config.app.display_name = value;
                DirtyEditor();
                SavePCConfig();
            }
            get
            {
                return config.app.display_name;
            }
        }

        public string PackageName
        {

            set
            {
                config.app.package_name = value;
                DirtyEditor();
                SavePCConfig();
            }
            get
            {
                return config.app.package_name;
            }
   
        }

        private void DirtyEditor()
        {
            EditorUtility.SetDirty(Instance);
        }

        private void SavePCConfig()
        {
            if (!Directory.Exists(CONFIG_FILE_FOLDER))
            {
                Directory.CreateDirectory(CONFIG_FILE_FOLDER);
            }
            StringBuilder sb = new StringBuilder();
            JsonWriter writer = new JsonWriter(sb);
            writer.PrettyPrint = true;
            writer.IndentValue = 4;
            JsonMapper.ToJson(config, writer);
            File.WriteAllText(CONFIG_FILE, sb.ToString());
            
        }
    }
}