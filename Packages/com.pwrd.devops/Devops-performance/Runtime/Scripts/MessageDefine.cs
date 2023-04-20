
using PENet;
using System;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;

namespace Devops.Performance
{
    //[Serializable]
    //[DataContract]
    //public class NetMsg : PEMsg
    //{
    //    [DataMember]
    //    public EMessageType msgType;

    //    public NetMsg()
    //    { }

    //    public NetMsg(EMessageType inMsgType)
    //    {
    //        msgType = inMsgType;
    //    }
    //}

    public enum EMessageType : Int32
    {
        Connect = 1,
        Disconnect = 2,
        MemorySnapshot = 3,
        ObjectMemorySnapshot = 4,
        ScreenShot = 5,
        ScreenShotBase64 = 6,
        HeartBeat = 7,
        MemoryInfo = 8,
        ScreenShotBuffer = 9,
        ScreenShotTextureString = 10,
        ScreenShotTextureBuffer = 11,
        Tag = 12,
        ScreenShotTextureInfo = 13,
    }
    public class ClientSystemInfo/* : NetMsg*/
    {
        public string deviceName;
        public string deviceModel;
        public string graphicsDeviceID;
        public string graphicsDeviceName;
        public string graphicsDeviceType;
        public string graphicsDeviceVendor;
        public string graphicsDeviceVendorID;
        public string operatingSystem;
        public string unityVersion;
        public string reportWebAddress;
        public string remarkInfo;
        public string buildTimestamp;
        public string versionId;
        public string platform;
        public string processorType;
        public string screenResolution;
        public string[] tags;
        public bool auto;
        public string packageVersion;
        public string buildRunId;
        public string deviceUuid;
        public string reportId;
        public string testPlan;


        static NetMsgSender sender = new NetMsgSender((int)EMessageType.Connect);
        public NetMsgSender GetMsgSender()
        {
            sender.Reset();
            sender.WriteString(deviceName);
            sender.WriteString(deviceModel);
            sender.WriteString(graphicsDeviceID);
            sender.WriteString(graphicsDeviceName);
            sender.WriteString(graphicsDeviceType);
            sender.WriteString(graphicsDeviceVendor);
            sender.WriteString(graphicsDeviceVendorID);
            sender.WriteString(operatingSystem);
            sender.WriteString(unityVersion);
            sender.WriteString(reportWebAddress);
            sender.WriteString(remarkInfo);
            sender.WriteString(buildTimestamp);
            sender.WriteString(versionId);
            sender.WriteString(platform);
            sender.WriteString(processorType);
            sender.WriteString(screenResolution);
            sender.WriteStringArray(tags);
            sender.WriteBool(auto);
            sender.WriteString(packageVersion);
            sender.WriteString(buildRunId);
            sender.WriteString(deviceUuid);
            sender.WriteString(reportId);
            sender.WriteString(testPlan);
            sender.WriteEnd();
            return sender;
        }

        public void Receiver(byte[] buffer)
        {
            NetMsgReceiver receiver = new NetMsgReceiver(buffer);
            deviceName = receiver.ReadString();
            deviceModel = receiver.ReadString();
            graphicsDeviceID = receiver.ReadString();
            graphicsDeviceName = receiver.ReadString();
            graphicsDeviceType = receiver.ReadString();
            graphicsDeviceVendor = receiver.ReadString();
            graphicsDeviceVendorID = receiver.ReadString();
            operatingSystem = receiver.ReadString();
            unityVersion = receiver.ReadString();
            reportWebAddress = receiver.ReadString();
            remarkInfo = receiver.ReadString();
            buildTimestamp = receiver.ReadString();
            versionId = receiver.ReadString();
            platform = receiver.ReadString();
            processorType = receiver.ReadString();
            screenResolution = receiver.ReadString();
            tags = receiver.ReadStringArray();
            auto = receiver.ReadBool();
            packageVersion = receiver.ReadString();
            buildRunId = receiver.ReadString();
            deviceUuid = receiver.ReadString();
            reportId = receiver.ReadString();
            testPlan = receiver.ReadString();
        }
    }

    public enum eScreenShotReason : Int32
    {
        None = 0,
        ObjectSnapShot = 1,
    }

    public class ScreenShotTextureInfoString
    {
        public int frame;
        public string imageInfoBase64;
        public bool isAuto;
        public eScreenShotReason reason;

        static NetMsgSender sender = new NetMsgSender((int)EMessageType.ScreenShotTextureString);
        public NetMsgSender GetMsgSender()
        {
            sender.Reset();
            sender.WriteInt(frame);
            sender.WriteString(imageInfoBase64);
            sender.WriteBool(isAuto);
            sender.WriteInt((int)reason);
            sender.WriteEnd();
            return sender;
        }

        public void Receiver(byte[] buffer)
        {
            NetMsgReceiver receiver = new NetMsgReceiver(buffer);
            frame = receiver.ReadInt();
            imageInfoBase64 = receiver.ReadString();
            isAuto = receiver.ReadBool();
            reason = (eScreenShotReason)receiver.ReadInt();
        }
    }

    public class ScreenShotTextureInfoBuffer
    {
        public int frame;
        public byte[] imageBuffer;
        public bool isAuto;
        public eScreenShotReason reason;

        static NetMsgSender sender = new NetMsgSender((int)EMessageType.ScreenShotTextureBuffer);
        public NetMsgSender GetMsgSender()
        {
            sender.Reset();
            sender.WriteInt(frame);
            sender.WriteByteArray(imageBuffer);
            sender.WriteBool(isAuto);
            sender.WriteInt((int)reason);
            sender.WriteEnd();
            return sender;
        }

        public void Receiver(byte[] buffer)
        {
            NetMsgReceiver receiver = new NetMsgReceiver(buffer);
            frame = receiver.ReadInt();
            imageBuffer = receiver.ReadByteArray();
            isAuto = receiver.ReadBool();
            reason = (eScreenShotReason)receiver.ReadInt();
        }
    }

    public class ScreenShotTextureInfo
    {
        public int frame;
        public string textureName;
        public bool isAuto;
        public eScreenShotReason reason;

        static NetMsgSender sender = new NetMsgSender((int)EMessageType.ScreenShotTextureInfo);
        public NetMsgSender GetMsgSender()
        {
            sender.Reset();
            sender.WriteInt(frame);
            sender.WriteString(textureName);
            sender.WriteBool(isAuto);
            sender.WriteInt((int)reason);
            sender.WriteEnd();
            return sender;
        }

        public void Receiver(byte[] buffer)
        {
            NetMsgReceiver receiver = new NetMsgReceiver(buffer);
            frame = receiver.ReadInt();
            textureName = receiver.ReadString();
            isAuto = receiver.ReadBool();
            reason = (eScreenShotReason)receiver.ReadInt();
        }
    }

    public class HeartBeatInfo/* : NetMsg*/
    {
        public string remark;

        static NetMsgSender sender = new NetMsgSender((int)EMessageType.HeartBeat);
        public NetMsgSender GetMsgSender()
        {
            sender.Reset();
            sender.WriteString(remark);
            sender.WriteEnd();
            return sender;
        }

        public void Receiver(byte[] buffer)
        {
            NetMsgReceiver receiver = new NetMsgReceiver(buffer);
            remark = receiver.ReadString();
        }
    }

    public class SDKMemory
    {
        public int vss;
        public int uss;
        public int pss;
        public int rss;

        static NetMsgSender sender = new NetMsgSender((int)EMessageType.MemoryInfo);
        public NetMsgSender GetMsgSender()
        {
            sender.Reset();
            sender.WriteInt(vss);
            sender.WriteInt(uss);
            sender.WriteInt(pss);
            sender.WriteInt(rss);
            sender.WriteEnd();
            return sender;
        }

        public void Receiver(byte[] buffer)
        {
            NetMsgReceiver receiver = new NetMsgReceiver(buffer);
            vss = receiver.ReadInt();
            uss = receiver.ReadInt();
            pss = receiver.ReadInt();
            rss = receiver.ReadInt();
        }
    }

    public class MemoryInfo/* : NetMsg*/
    {
        public int vss;
        public int uss;
        public int pss;
        public int rss;
        static NetMsgSender sender = new NetMsgSender((int)EMessageType.MemoryInfo);
        public NetMsgSender GetMsgSender()
        {
            sender.Reset();
            sender.WriteInt(vss);
            sender.WriteInt(uss);
            sender.WriteInt(pss);
            sender.WriteInt(rss);
            sender.WriteEnd();
            return sender;
        }

        public void Receiver(byte[] buffer)
        {
            NetMsgReceiver receiver = new NetMsgReceiver(buffer);
            vss = receiver.ReadInt();
            uss = receiver.ReadInt();
            pss = receiver.ReadInt();
            rss = receiver.ReadInt();
        }
    }

    public class TagFrameInfo
    {
        public int frame = 0;
        public string tag = string.Empty;
        static NetMsgSender sender = new NetMsgSender((int)EMessageType.Tag);
        public NetMsgSender GetMsgSender()
        {
            sender.Reset();
            sender.WriteInt(frame);
            sender.WriteString(tag);
            sender.WriteEnd();
            return sender;
        }

        public void Receiver(byte[] buffer)
        {
            NetMsgReceiver receiver = new NetMsgReceiver(buffer);
            frame = receiver.ReadInt();
            tag = receiver.ReadString();
        }
    }
}