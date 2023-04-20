using System.Collections.Generic;

namespace GSDK.RNU {
    public class ViewAtIndex {
        public int tag;
        public int index;
        public ViewAtIndex(int tag, int index) {
            this.tag = tag;
            this.index = index;
        }
    }

    public class ViewAtIndexOrder : Comparer<ViewAtIndex>
    {
        // Call CaseInsensitiveComparer.Compare with the parameters reversed.
        public override int Compare(ViewAtIndex lhs, ViewAtIndex rhs)
        {
           return lhs.index - rhs.index;
        }
    }
}