using System;
using System.Collections.Generic;

namespace UnityEditor.Recorder
{
    [Serializable]
    public abstract class RecorderInputSettings
    {
        protected internal abstract Type InputType { get; }
        protected internal abstract bool ValidityCheck(List<string> errors);
    }
}
