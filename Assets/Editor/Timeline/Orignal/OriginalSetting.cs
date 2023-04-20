using UnityEditor;
using UnityEngine;

public class OriginalSetting
{
    internal const string str_ch = "timeline_Role_Root";
    internal const string str_fx = "timeline_Fx_Root";
    internal const string str_audio = "timeline_Audio";

    internal static readonly string LIB = "Assets/BundleRes/Timeline/";
    internal static readonly string m_unusedTimelineDir = "Assets/BundleRes/TimelineUnused/";
    internal const string iconPat = "Assets/Editor/Timeline/StyleSheets/ico/Cutscene Icon.png";
    internal const string setPat = "Assets/Editor/Timeline/StyleSheets/ico/settings.png";
    internal const string stcPat = "Assets/Editor/Timeline/StyleSheets/ico/gesture.png";
    internal const string lipPat = "Assets/Editor/Timeline/StyleSheets/ico/lip.png";
    internal const string logoPat = "Assets/Editor/Timeline/StyleSheets/ico/pwrd.png";
    internal const string headPat = "Assets/Editor/Timeline/StyleSheets/ico/header.png";
    internal const string head2Pat = "Assets/Editor/Timeline/StyleSheets/ico/header_dark.png";
    internal const string docPat = "docs/drama_v1.pptx";
    internal const string resFolder = "Assets/Editor/Timeline/res/";
    internal const string linePat = "Assets/Editor/Timeline/res/lines.txt";
    internal const string fadePat = "Assets/Editor/Timeline/res/fade.txt";
    internal const string dataPat = "Assets/Editor/Timeline/res/OriginalCSData.asset";
    internal const string recdPat = "Assets/Editor/Timeline/res/recd.asset";
    internal const string tmpPat = "Assets/Editor/Timeline/res/Orignal_tmp.playable";
    internal const string tmpAvgPat = "Assets/BundleRes/TimelineUnused/Orignal_AVGsample.playable";
    internal const string vcPath = "Assets/Editor/Timeline/res/vcs/";
    internal const string EditorVC = "Edit_VirtualCam";

    internal readonly static string mergeVc = "合并虚拟相机，删除每个分镜对应录制的动画，以提高性能";
    internal readonly static string findMissing = "查找prefab中已经删除导致Missing的节点";

    // 注意事项
    internal const string tipUrl = "https://doc.weixin.qq.com/ww/doc?docid=e33c86fb87914b1e8040a26c71524b53_ww&scode=gysc19bb5dfcf8de2246c5caa1d67702a207&type=0";

    // 动态加载文档
    internal const string apiUrl = "https://doc.weixin.qq.com/txdoc/word?scode=AHYA5gfqAAYqiiirhXABEAbAYmAHI&docid=w2_ABEAbAYmAHIZzTCRV12SaiRiH6jWt&type=0";

    // 口型
    internal const string lipUrl = "https://doc.weixin.qq.com/txdoc/word?scode=AHYA5gfqAAY7jfskHuABEAbAYmAHI&docid=w2_ABEAbAYmAHI5Y6peN49QCWpxgpmRx&type=0";

    //对照表链接
    internal const string m_timelineUrl = "https://docs.qq.com/sheet/DV0NKWGtnaGhOblZX?tab=e3vv7e";

    private static Texture2D icon, set, statistic, logo;

    private static Texture2D header, lip;

    internal static Texture2D Icon
    {
        get
        {
            if (icon == null)
            {
                icon = AssetDatabase.LoadAssetAtPath<Texture2D>(iconPat);
            }
            return icon;
        }
    }

    internal static Texture2D Set
    {
        get
        {
            if (set == null)
            {
                set = AssetDatabase.LoadAssetAtPath<Texture2D>(setPat);
            }
            return set;
        }
    }

    internal static Texture2D Stc
    {
        get
        {
            if (statistic == null)
            {
                statistic = AssetDatabase.LoadAssetAtPath<Texture2D>(stcPat);
            }
            return statistic;
        }
    }

    internal static Texture2D Logo
    {
        get
        {
            if (logo == null)
            {
                logo = AssetDatabase.LoadAssetAtPath<Texture2D>(logoPat);
            }
            return logo;
        }
    }

    internal static Texture2D Header
    {
        get
        {
            if (header == null)
            {
                string pat = headPat;
                if (EditorGUIUtility.isProSkin)
                {
                    pat = head2Pat;
                }
                header = AssetDatabase.LoadAssetAtPath<Texture2D>(pat);
            }
            return header;
        }
    }

    internal static Texture2D Lip
    {
        get
        {
            if (lip == null)
            {
                lip = AssetDatabase.LoadAssetAtPath<Texture2D>(lipPat);
            }
            return lip;
        }
    }

    public static void OpenDoc()
    {
        try
        {
            EditorUtility.OpenWithDefaultApp(docPat);
        }
        catch (System.IO.IOException e)
        {
            EditorUtility.DisplayDialog("tip", "PROCCESS IS BUSY \n" + e.Message, "ok");
        }
    }


    public static void OpenApi()
    {
        Application.OpenURL(apiUrl);
    }

    public static void OpenLipDoc()
    {
        Application.OpenURL(lipUrl);
    }

    public static void OpenTimelineTableDoc()
    {
        Application.OpenURL(m_timelineUrl);
    }

    internal static void DrawLogo(EditorWindow w)
    {
        float x = w.position.width - 200;
        float y = w.position.height - 80;
        Rect rect = new Rect(x, y, 190, 65);
        GUI.DrawTexture(rect, Logo);
    }

}