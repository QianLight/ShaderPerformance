#if ENABLE_UPO && ENABLE_UPO_CUSTOMIZE
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using UPOHelper.Network;
using UPOHelper.Utils;

namespace UPOHelper.Customize
{
    public class UpoCustomizedData
    {
        private static StringBuilder _builder = new StringBuilder();
        private static DateTime epochStart = new DateTime(1970, 1, 1, 0, 0, 0, System.DateTimeKind.Utc);

        class UpoCustomData
        {
            public string tab;
            public string chartName;
            public string chartType;
            public string data;
            public int frameIndex;
            public int timestamp;

        }

        public static bool SendCustomizedData(String subjectName, String groupName, String chartType,
            Dictionary<string, string> data)
        {
            Debug.Log("get customized data " + subjectName + groupName + chartType + data.ToString());
            
            _builder = new StringBuilder();
            _builder.Append("{");
            
            foreach (var item in data)
            {
                _builder.AppendFormat(" \"{0}\":\"{1}\",", item.Key, item.Value);
            }

            _builder.Remove(_builder.Length - 1, 1); // remove ,(comma)
            _builder.Append("}");
            // Debug.Log(_builder.ToString());		
            
            UpoCustomData result = new UpoCustomData();
            result.tab = subjectName;
            result.chartName = groupName;
            result.chartType = chartType;
            result.data = _builder.ToString();
            result.frameIndex = Time.frameCount;
            result.timestamp = (int) (DateTime.UtcNow - epochStart).TotalSeconds;
            string resultJson = JsonUtility.ToJson(result);
            byte[] rawBytes = UPOTools.DeserializeString(resultJson);
            NetworkServer.SendMessage(UPOMessageType.CustomizedData, rawBytes);

            return true;
        }
    }
}
#endif