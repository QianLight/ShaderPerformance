using System.Collections;

namespace GSDK.RNU
{
    public abstract class SimpleUnityModule : BaseUnityModule
    {
        public abstract string GetName();

        /**
           * @return a map of constants this module exports to JS. Supports JSON types.
           */
         virtual public Hashtable GetConstants() {
             return null;
         }

         public virtual void Destroy() {
             // no-op
         }

    };
}