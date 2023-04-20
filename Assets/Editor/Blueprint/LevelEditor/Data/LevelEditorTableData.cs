using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using CFUtilPoolLib;

namespace LevelEditor
{
    public class LevelEditorTableData
    {
        private static XEntityStatistics _monster_data_info = null;
        private static XEntityPresentation _monster_present_info = null;
        private static MapList _map_info = null;
        private static DropObject _drop_info = null;

        public static XEntityStatistics MonsterInfo
        {
            get { return _monster_data_info; }
        }

        public static XEntityPresentation MonsterPresent
        {
            get { return _monster_present_info; }
        }

        public static MapList MapInfo
        {
            get { return _map_info; }
        }

        public static DropObject DropInfo
        {
            get { return _drop_info; }
        }

        public static void ReadTable()
        {
            _monster_data_info = new XEntityStatistics();

            if (!XTableReader.ReadFile("Table/XEntityStatistics", _monster_data_info))
            {
                Debug.Log("<color=red>Error occurred when loading associate data file.</color>");
            }

            _monster_present_info = new XEntityPresentation();
            if(!XTableReader.ReadFile("Table/XEntityPresentation", _monster_present_info))
            {
                Debug.Log("<color=red>Error occurred when loading associate data file.</color>");
            }

            _map_info = new MapList();
            if(!XTableReader.ReadFile("Table/MapList",_map_info))
            {
                Debug.Log("<color=red>Error occurred when loading associate data file.</color>");
            }

            _drop_info = new DropObject();
            if (!XTableReader.ReadFile("Table/DropObject", _drop_info))
            {
                Debug.Log("<color=red>Error occurred when loading associate data file.</color>");
            }
        }
        
    }
}
