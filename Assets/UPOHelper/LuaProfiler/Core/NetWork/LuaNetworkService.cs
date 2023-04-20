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
* Filename: NetWorkClient
* Created:  2018/7/13 14:29:22
* Author:   エル・プサイ・コングリィ
* Purpose:  
* ==============================================================================
*/
#if ENABLE_UPO && ENABLE_UPO_LUA
using System.Linq;
using UnityEngine;
using UPOHelper.Network;
using UPOHelper.Utils;


namespace UPOLuaProfiler
{
    using System;
    using System.IO;
    
    public static class LuaNetworkService
    {
        private const int PACK_HEAD = 0x23333333;
        private static int m_frameCount = 0;

        #region public

        public static void SendMessage(NetBase sample)
        {
            if (!NetworkServer.IsConnected())
            {
                sample.Restore();
                return;
            }
            
            // add pack_head
            byte[] head = BitConverter.GetBytes(PACK_HEAD);
            if (!BitConverter.IsLittleEndian)
            {
                Array.Reverse(head);
            }

            int type = -1;
            if (sample is Sample)
            {
                type = 0;
            } else if (sample is LuaRefInfo)
            {
                type = 1;
            } else if (sample is LuaDiffInfo)
            {
                type = 2;
            }

            byte[] result = head.Concat(UPOTools.ConvertLittleEndian(type)).Concat(sample.Serialize()).ToArray();
            NetworkServer.SendMessage(UPOMessageType.Lua, result);
        }

        public static void OnReceiveMessage(UPOMessage message)
        {
            Debug.Log("receive cmd@LuaNetworkService:OnReceiveMessage");
            byte[] rawData = message.GetRaw();

            MemoryStream stream = new MemoryStream(rawData);
            BinaryReader reader = new BinaryReader(stream);
            int header = reader.ReadInt32(); // 0x23333333
            if (header != PACK_HEAD)
            {
                Debug.LogError("wrong pack header " + header);
                return;
            }

            int cmd = reader.ReadInt32();

            Debug.Log("receive cmd" + cmd);
            switch (cmd)
            {
                case 0:
                    LuaProfiler.SendAllRef();
                    break;
                case 1:
                    HookLuaSetup.RegisterAction(() => { LuaHook.Record(); });
                    break;
                case 2:
                    HookLuaSetup.RegisterAction(LuaHook.DiffServer);
                    break;
                case 3:
                    HookLuaSetup.RegisterAction(() => { LuaHook.RecordStatic(); });
                    break;
            }
        }

        #endregion

        #region private

        private static int m_key = 0;

        public static int GetUniqueKey()
        {
            return m_key++;
        }

        #endregion
    }
}

#endif