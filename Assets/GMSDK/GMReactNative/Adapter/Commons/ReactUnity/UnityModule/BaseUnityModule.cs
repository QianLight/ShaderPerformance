using System.Collections;

namespace GSDK.RNU
{
    public interface BaseUnityModule : NativeModule
    {

        /**
           * @return a map of constants this module exports to JS. Supports JSON types.
           */
         Hashtable GetConstants();

         void Destroy();

    };
}