using System;

namespace CFEngine
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public sealed class EnvEditorAttribute : Attribute
    {
        public readonly Type settingsType;

        public EnvEditorAttribute(Type settingsType)
        {
            this.settingsType = settingsType;
        }
    }
}
