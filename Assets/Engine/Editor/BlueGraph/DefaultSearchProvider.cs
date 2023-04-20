﻿
using System;
using System.Collections.Generic;

namespace CFEngine.Editor
{
    class DefaultSearchProvider : ISearchProvider
    {
        public IEnumerable<SearchResult> GetSearchResults(SearchFilter filter)
        {
            foreach (var entry in NodeReflection.GetNodeTypes())
            {
                var node = entry.Value;
                if (IsCompatible(filter.sourcePort, node))
                {
                    yield return new SearchResult
                    {
                        name = node.name,
                        path = node.path,
                        userData = node,
                    };
                }
            }
        }

        public AbstractNode Instantiate(SearchResult result)
        {
            NodeReflectionData data = result.userData as NodeReflectionData;
            return data.CreateInstance();
        }
        
        bool IsCompatible(Port sourcePort, NodeReflectionData node)
        {
            if (sourcePort == null)
            {
                return true;
            }

            if (sourcePort.isInput)
            {
                return node.HasOutputOfType(sourcePort.Type);
            }

            return node.HasInputOfType(sourcePort.Type);
        }
    }
}
