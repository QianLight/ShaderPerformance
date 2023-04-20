using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

namespace TDTools.ResourceScanner {

    [System.Serializable]
    public class TableChangeRecord {

        public string[] newKeys;
        public string[] newValues;

        public string[] oldKeys;
        public string[] oldVlaues;

        public string TableName;
        public string IDColumn;
        public int IDIndex;
        public bool Selected;

        public string DateTime;

        public TableChangeRecord() {
        }

        public TableChangeRecord(Dictionary<string, string> newRow, string tableName, string id) {
            TableName = tableName;
            IDColumn = id;
            newKeys = new string[newRow.Count];
            newValues = new string[newRow.Count];
            newRow.Keys.CopyTo(newKeys, 0);
            newRow.Values.CopyTo(newValues, 0);

            for (int i = 0; i < newKeys.Length; i++)
                if (newKeys[i].Equals(id)) {
                    IDIndex = i;
                    break;
                }

            var table = TableDatabase.Instance.GetTable(tableName, id);
            table.Reload();
            var oldRows = table.GetRowByID(newRow[id]);
            oldKeys = new string[oldRows.Count];
            oldVlaues = new string[oldRows.Count];
            oldRows.Keys.CopyTo(oldKeys, 0);
            oldRows.Values.CopyTo(oldVlaues, 0);

            Selected = false;
            DateTime = System.DateTime.Now.ToString();
            DateTime = DateTime.Substring(DateTime.IndexOf("/") + 1);
        }

        public string ID {
            get {
                return newValues[IDIndex];
            }
        }
    }
}