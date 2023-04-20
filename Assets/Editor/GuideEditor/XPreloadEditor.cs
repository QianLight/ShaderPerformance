using System.IO;
using System.Runtime.InteropServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.CFUI;
using CFUtilPoolLib;

public class XPreloadEditor
{
    private static readonly string PATH_FIX = "Assets/BundleRes/UI/";
    private static Dictionary<int,string[]> GetPreloadList(){
        Dictionary<int,string[]> predic = new Dictionary<int,string[]>();
        string[] items =  {
            "OPsystemprefab/Common/CommonPerfab/CommonItem/CommonItem_Base",
            "OPsystemprefab/Common/CommonPerfab/CommonItem/CommonItem_chip",
            "OPsystemprefab/Common/CommonPerfab/CommonItem/CommonItem_available",
            "OPsystemprefab/Common/CommonPerfab/CommonItem/CommonItem_tag",
            "OPsystemprefab/Common/CommonPerfab/Common_multi_choose",
            "OPsystemprefab/Common/CommonPerfab/Common_select",
            "OPsystemprefab/Common/CommonPerfab/Common_mask",
            "OPsystemprefab/Common/CommonPerfab/Common_new",
            "OPsystemprefab/Common/CommonItemNone",
            "OPsystemprefab/Common/CommonItemBaseBg",
            "OPsystemprefab/Common/CommonPerfab/CommonCharacter/Commoncharacter_base",
            "OPsystemprefab/Character/CharacterHandler/CharacterCardItem",
            "OPsystemprefab/Will/Will_New/WillItem_New",
            "OPsystemprefab/Skill/SkillCommonBase",
            "OPsystemprefab/Common/CommonTitle"
        };
        predic.Add(-1 , items);

        string[] middle ={
        "OPsystemprefab/Battlepass/BattlepassBg",
        "OPsystemprefab/Battlepass/BattlepassTaskDaily",
        "OPsystemprefab/Battlepass/BattlepassTaskWeek",
        "OPsystemprefab/Battlepass/BattlepassPowerValue",    
        "OPsystemprefab/Backpack/BackpackInfo",
        "OPsystemprefab/Character/CharacterMainView",
        "OPsystemprefab/Skill/SkillSystem",
        "OPsystemprefab/Task/TaskView",
        // "OPsystemprefab/Task/OPBountyHandler",
        "OPsystemprefab/Fleet/FleetBoss/FleetBossMainView",
        };

        predic.Add(3,middle);
        return predic;
    } 


    [MenuItem("Tools/UI/UI ExportDependent")]
    public static void ExportDependentFile()
    {
        UIDependenceConfig config = new UIDependenceConfig();
        Dictionary<int,string[]> prelist= GetPreloadList();
        foreach (KeyValuePair<int, string[]> kv in prelist)
        {
              for (int i = 0; i < kv.Value.Length; i++) 
              {
                string filename = PATH_FIX + kv.Value[i] + ".prefab";
                GameObject go = AssetDatabase.LoadAssetAtPath<GameObject>(filename);
                if (go == null) continue;
                UIDependenceNode node = null;
                if(CreateDependenceNode(go, kv.Value[i],ref node)) {
                    node.layer = kv.Key;
                    config.dep_configs.Add(node);
                }
            }
        }
        string url = "Assets/BundleRes/Guide/preloadconfig.bytes";
        DataIO.SerializeData<UIDependenceConfig>(url, config);
        EditorUtility.DisplayDialog("export", string.Format("Export {0} sucess!", url), "ok");
    }

    private static List<MaskableGraphic> graphics = new List<MaskableGraphic>();
    private static bool CreateDependenceNode( GameObject prefab,string prefabName,ref UIDependenceNode node )
    {
        graphics.Clear();
        prefab.GetComponentsInChildren<MaskableGraphic>(true, graphics);
        if (graphics.Count == 0) return false;
        node = new UIDependenceNode();
        node.prefab = prefabName;
        for (int i = 0; i < graphics.Count; i++)
        {
            if (graphics[i] is CFImage)
            {
                CFImage image = graphics[i] as CFImage;
                if (!image.m_StaticSrc || string.IsNullOrEmpty(image.atlasName) || string.IsNullOrEmpty(image.spriteName)) continue;
                if (node.dep_atlas.Contains(image.atlasName)) continue;
                node.dep_atlas.Add(image.atlasName);
            }else if(graphics[i] is CFRawImage)
            {
                CFRawImage rawimage = graphics[i] as CFRawImage;
                if (!rawimage.StaticSrc || string.IsNullOrEmpty(rawimage.m_TexPath)) continue;
                if (node.dep_texture.Contains(rawimage.m_TexPath)) continue;
                node.dep_texture.Add(rawimage.m_TexPath);
            }    
        }
        return true;
    }
}

