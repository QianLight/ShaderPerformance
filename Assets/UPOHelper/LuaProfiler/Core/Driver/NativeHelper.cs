/*
               #########                       
              ############                     
              #############                    
             ##  ###########                   
            ###  ###### #####                  
            ### #######   ####                 
           ###  ########## ####                
          ####  ########### ####               
         ####   ###########  #####             
        #####   ### ########   #####           
       #####   ###   ########   ######         
      ######   ###  ###########   ######       
     ######   #### ##############  ######      
    #######  #####################  ######     
    #######  ######################  ######    
   #######  ###### #################  ######   
   #######  ###### ###### #########   ######   
   #######    ##  ######   ######     ######   
   #######        ######    #####     #####    
    ######        #####     #####     ####     
     #####        ####      #####     ###      
      #####       ###        ###      #        
        ###       ###        ###               
         ##       ###        ###               
__________#_______####_______####______________
                我们的未来没有BUG                
* ==============================================================================
* Filename: NativeHelper
* Created:  2018/7/13 14:29:22
* Author:   エル・プサイ・コングリィ
* Purpose:  
* ==============================================================================
*/

#if ENABLE_UPO && ENABLE_UPO_LUA
namespace UPOLuaProfiler
{
#if UNITY_5_5_OR_NEWER
    using UnityEngine.Profiling;

#endif

    public static class NativeHelper
    {
        public static int GetPass()
        {
#if UNITY_5_5_OR_NEWER
            return (int) Profiler.GetTotalAllocatedMemoryLong();
#else
            return (int)Profiler.GetTotalAllocatedMemory();
#endif
        }

        public static float GetBatteryLevel()
        {
            float result = 100;
            return result;
        }
    }
}
#endif