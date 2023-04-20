using System.Collections.Generic;
using UnityEngine;

namespace com.pwrd.hlod.editor
{
    public interface IHLODBuilder
    {
        List<HLODResultData> RunAggregate(List<(List<Renderer>, AggregateParam)> aggregateList);
    }
}