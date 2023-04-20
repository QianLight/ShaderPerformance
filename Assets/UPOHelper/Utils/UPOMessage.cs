#if ENABLE_UPO
using System;
using System.IO;
using System.Linq;
using UnityEngine;

namespace UPOHelper.Utils
{
    public enum UPOMessageType
    {
        Lua = 1,
        Mono = 2,
        Il2cpp = 3,
        CustomizedData = 4,
        Overdraw = 5,
        Mipmap = 6
    }

    public class UPOMessage
    {
        private long _magic = 0x55504f5f53444b5f; // UPO_SDK_
        private UPOMessageType _type;
        private int _length;
        private byte[] _rawData;

        public UPOMessage()
        {
        }

        public UPOMessage(UPOMessageType type, byte[] raw)
        {
            _type = type;
            _rawData = raw;
        }

        // 序列化成byte数组
        public byte[] Serialize()
        {
            // Debug.Log("serialize upoMessage");
            byte[] result;

            byte[] magic = BitConverter.GetBytes(_magic);
            if (!BitConverter.IsLittleEndian)
                Array.Reverse(magic);

            byte[] type = BitConverter.GetBytes((int) _type);
            if (!BitConverter.IsLittleEndian)
                Array.Reverse(type);

            byte[] length = BitConverter.GetBytes(_rawData.Length);
            if (!BitConverter.IsLittleEndian)
                Array.Reverse(length);

            result = magic.Concat(type).Concat(length).Concat(_rawData).ToArray();
            return result;
        }

        // 反序列化
        public void Deserialize(byte[] raw)
        {
            Debug.Log("deserialize upoMessage");
            MemoryStream stream = new MemoryStream(raw);
            BinaryReader reader = new BinaryReader(stream);
            Deserialize(reader);
        }

        public void Deserialize(BinaryReader reader)
        {
            _magic = reader.ReadInt64();
            _type = (UPOMessageType) reader.ReadInt32();
            _length = reader.ReadInt32();
            _rawData = reader.ReadBytes(_length);
        }

        public void SetRaw(byte[] raw)
        {
            _rawData = raw;
        }

        public byte[] GetRaw()
        {
            return _rawData;
        }

        public UPOMessageType GetMessageType()
        {
            return _type;
        }
    }
}
#endif