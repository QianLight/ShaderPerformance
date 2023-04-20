/*
 * @Author: hexiaonuo
 * @Date: 2021-10-15
 * @Description: Game set gameData, for ReactUnity action
 * @FilePath: ReactUnity/GameInteraction/GameData.cs
 */

using System.Collections.Generic;

namespace GSDK.RNU
{
    /*
     * 游戏设置数据信息相关
     * 为了方便扩展设置资源信息，设置 Dict 为 string，object
     */
    partial class GameInteraction
    {
        private static Dictionary<string, object> gameDataDic = new Dictionary<string, object>();

        
        // 游戏信息设置
        // 可以包含玩家信息，游戏版本信息，其他信息等
        public static void SetGameData(Dictionary<string, object> gameData)
        {
            if (gameData == null || gameData.Count == 0)
            {
                Util.LogAndReport("game set gamedata is failed, for gamedata is null or count is 0");
                return;
            }
            gameDataDic = gameData;
        }

        public static object GetGameData(string name)
        {
            if (gameDataDic != null && gameDataDic.ContainsKey(name))
            {
                return gameDataDic[name];
            }

            return null;
        }

        public static Dictionary<string, object> GetGameDataDic()
        {
            Util.Log("-----getgameData");
            return gameDataDic;
        }
    }
}