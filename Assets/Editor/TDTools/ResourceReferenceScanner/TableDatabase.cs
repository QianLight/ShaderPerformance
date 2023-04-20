using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace TDTools.ResourceScanner {
    public class TableDatabase {

        public static TableDatabase Instance {
            get {
                if (_instance == null)
                    _instance = new TableDatabase();
                return _instance;
            }
        }
        private static TableDatabase _instance;

        Dictionary<string, TableNode> _tables;

        TableDatabase() {
            _tables = new Dictionary<string, TableNode>();
        }

        public TableNode GetTable(string path, string IDCol, string displayName = "") {
            if (_tables.ContainsKey(path)) {
                string currentTime = File.GetLastWriteTime($"{Application.dataPath}/table/{path}.txt").ToString();
                if (!_tables[path].LastModifiedTime.Equals(currentTime)) {
                    _tables[path] = new TableNode(path, IDCol, displayName);
                } else {
                    _tables[path].DisplayName = displayName;
                }
            } else {
                _tables[path] = new TableNode(path, IDCol, displayName);
            }
            return _tables[path];
        }

        public void ClearAllSet() {
            foreach (var table in _tables) {
                table.Value.Set.Clear();
            }
        }

        public void Reload() {
            _tables.Clear();
        }
    }
}