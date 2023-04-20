using System;
using System.Collections.Generic;
using System.Reflection;

namespace Blueprint
{
    public static class BpGlobalEvent
    {
        public static Action<string, List<string>> OnBpAction;

        /// <summary>
        /// 移除事件的所有回调
        /// https://stackoverflow.com/questions/91778/how-to-remove-all-event-handlers-from-an-event/91853#91853
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="eventName"></param>
        public static void ClearEventInvocations(this object obj, string eventName)
        {
            var fi = obj.GetType().GetEventField(eventName);
            if (fi == null) return;
            fi.SetValue(obj, null);
        }

        private static FieldInfo GetEventField(this Type type, string eventName)
        {
            FieldInfo field = null;
            while (type != null)
            {
                /* Find events defined as field */
                field = type.GetField(eventName, BindingFlags.Static | BindingFlags.Instance | BindingFlags.NonPublic);
                if (field != null && (field.FieldType == typeof(MulticastDelegate) || field.FieldType.IsSubclassOf(typeof(MulticastDelegate))))
                    break;

                /* Find events defined as property { add; remove; } */
                field = type.GetField("EVENT_" + eventName.ToUpper(), BindingFlags.Static | BindingFlags.Instance | BindingFlags.NonPublic);
                if (field != null)
                    break;
                type = type.BaseType;
            }
            return field;
        }
    }
}