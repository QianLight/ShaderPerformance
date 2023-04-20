#if ENABLE_UPO
using System;
using System.IO;
using System.Linq;
using System.Text;

namespace UPOHelper.Utils
{
    public static class UPOTools
    {
        public static byte[] DeserializeString(string str)
        {
            byte[] raw = Encoding.UTF8.GetBytes(str);
            byte[] len = BitConverter.GetBytes(raw.Length);
            if (!BitConverter.IsLittleEndian)
            {
                Array.Reverse(len);
            }

            byte[] result = len.Concat(raw).ToArray();
            return result;
        }
        
        public static byte[] ConvertLittleEndian(int value)
        {
            byte[] result = BitConverter.GetBytes(value);
            if (!BitConverter.IsLittleEndian)
            {
                Array.Reverse(result);
            }
            return result;
        }
        
        public static byte[] ConvertLittleEndian(float value)
        {
            byte[] result = BitConverter.GetBytes(value);
            if (!BitConverter.IsLittleEndian)
            {
                Array.Reverse(result);
            }
            return result;
        }
        
        public static byte[] ConvertLittleEndian(long value)
        {
            byte[] result = BitConverter.GetBytes(value);
            if (!BitConverter.IsLittleEndian)
            {
                Array.Reverse(result);
            }
            return result;
        }

        public static byte[] ReadAllBytes(BinaryReader reader)
        {
            const int bufferSize = 4096;
            using (var ms = new MemoryStream())
            {
                byte[] buffer = new byte[bufferSize];
                int count;
                while ((count = reader.Read(buffer, 0, buffer.Length)) != 0)
                    ms.Write(buffer, 0, count);
                return ms.ToArray();
            }
        }
    }
}
#endif