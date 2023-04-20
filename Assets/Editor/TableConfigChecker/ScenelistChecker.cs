

namespace TDTools
{
    public class ScenelistChecker : BaseChecker
    {
        //public ScenelistChecker()
        //{
            //CheckList.Add(new MethordInfo("SceneList", "ID", 1, EmptyCheck));  暂时没用
        //}
        public void PassMethords()     
        {
            bool exist_1 = false;
            bool exist_2 = false;
            bool exist_3 = false;
            MethordInfo AimMethord_1 = new MethordInfo("Scenelist", "EmptyCheck", "筛选列内空行", 1, EmptyCheck);
            MethordInfo AimMethord_2 = new MethordInfo("Scenelist", "RepeatedCheck", "筛选列内重复行", 1, RepeatedCheck);
            MethordInfo AimMethord_3 = new MethordInfo("Scenelist", "ForeignkeyCheck", "筛选没有在目标列中出现的内容", 1, ForeignkeyCheck);
            foreach (MethordInfo m in TableConfigCheckerUI.AllMethords)
            {
                if ((m.ObjectName + m.MethordName) == (AimMethord_1.ObjectName + AimMethord_1.MethordName))
                    exist_1 = true;
                if ((m.ObjectName + m.MethordName) == (AimMethord_2.ObjectName + AimMethord_2.MethordName))
                    exist_2 = true;
                if ((m.ObjectName + m.MethordName) == (AimMethord_3.ObjectName + AimMethord_3.MethordName))
                    exist_3 = true;
            }
            if(!exist_1)
                TableConfigCheckerUI.AllMethords.Add(AimMethord_1);
            if (!exist_2)
                TableConfigCheckerUI.AllMethords.Add(AimMethord_2);
            if (!exist_3)
                TableConfigCheckerUI.AllMethords.Add(AimMethord_3);
        }
    }
}
