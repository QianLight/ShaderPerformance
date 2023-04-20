using System;
//using System.Buffers.Binary;
using System.IO;
using UnityEngine;

namespace AssetStudio
{
    public class EndianBinaryReader : BinaryReader
    {
        private readonly byte[] buffer;

        public EndianType Endian;

        public EndianBinaryReader(Stream stream, EndianType endian = EndianType.BigEndian) : base(stream)
        {
            Endian = endian;
            buffer = new byte[8];
        }

        public long Position
        {
            get => BaseStream.Position;
            set => BaseStream.Position = value;
        }

        public override short ReadInt16()
        {
            if (Endian == EndianType.BigEndian)
            {
                Read(buffer, 0, 2);
                return ReadInt16BigEndian(buffer);
            }
            return base.ReadInt16();
        }

        public short ReadInt16BigEndian(byte[] bytes)
        {
            short num = (short) (bytes[1] | bytes[0] << 8);
            // if (BitConverter.IsLittleEndian)
            // {
            //     num = ReverseEndianness(num);
            // }
            return num;
        }
        
        public override int ReadInt32()
        {
            if (Endian == EndianType.BigEndian)
            {
                Read(buffer, 0, 4);
                return ReadInt32BigEndian(buffer);
            }
            return base.ReadInt32();
        }

        public int ReadInt32BigEndian(byte[] bytes)
        {
            int num = bytes[3] |
                      bytes[2] << 8 |
                      bytes[1] << 16 |
                      bytes[0] << 24;

            // if (BitConverter.IsLittleEndian)
            // {
            //     num = ReverseEndianness(num);
            // }

            return num;
        }
        
        public override long ReadInt64()
        {
            if (Endian == EndianType.BigEndian)
            {
                Read(buffer, 0, 8);
                return ReadInt64BigEndian(buffer);
            }
            return base.ReadInt64();
        }

        public long ReadInt64BigEndian(byte[] bytes)
        {
            long num= (long)bytes[7] |
                (long)bytes[6] << 8 |
                (long)bytes[5] << 16 |
                (long)bytes[4] << 24 |
                (long)bytes[3] << 32 |
                (long)bytes[2] << 40 |
                (long)bytes[1] << 48 |
                (long)bytes[0] << 56;
            
            // if (BitConverter.IsLittleEndian)
            // {
            //     num = ReverseEndianness(num);
            // }

            return num;
        }


        public override ushort ReadUInt16()
        {
            if (Endian == EndianType.BigEndian)
            {
                Read(buffer, 0, 2);
                return ReadUInt16BigEndian(buffer);
            }
            return base.ReadUInt16();
        }
        
        public ushort ReadUInt16BigEndian(byte[] bytes)
        {
            ushort num = (ushort) (bytes[1] | bytes[0] << 8);
            // if (BitConverter.IsLittleEndian)
            // {
            //     num = ReverseEndianness(num);
            // }
            return num;
        }
        
        

        public override uint ReadUInt32()
        {
            if (Endian == EndianType.BigEndian)
            {
                Read(buffer, 0, 4);
                return ReadUInt32BigEndian(buffer);
            }
            return base.ReadUInt32();
        }

        public uint ReadUInt32BigEndian(byte[] bytes)
        {
            uint num = (uint)bytes[3] |
                       (uint)bytes[2] << 8 |
                       (uint)bytes[1] << 16 |
                       (uint)bytes[0] << 24;

            // if (BitConverter.IsLittleEndian)
            // {
            //     num = ReverseEndianness(num);
            // }

            return num;
        }
        
        public override ulong ReadUInt64()
        {
            if (Endian == EndianType.BigEndian)
            {
                Read(buffer, 0, 8);
                return ReadUInt64BigEndian(buffer);
            }
            return base.ReadUInt64();
        }

        
        public ulong ReadUInt64BigEndian(byte[] bytes)
        {
            ulong num= (ulong)bytes[7] |
                       (ulong)bytes[6] << 8 |
                       (ulong)bytes[5] << 16 |
                       (ulong)bytes[4] << 24 |
                       (ulong)bytes[3] << 32 |
                       (ulong)bytes[2] << 40 |
                       (ulong)bytes[1] << 48 |
                       (ulong)bytes[0] << 56;
            
            // if (BitConverter.IsLittleEndian)
            // {
            //     num = ReverseEndianness(num);
            // }

            return num;
        }
        
        public override float ReadSingle()
        {
            if (Endian == EndianType.BigEndian)
            {
                
                Debug.LogError("ReadSingle");
                
                Read(buffer, 0, 4);
                Array.Reverse(buffer, 0, 4);
                return base.ReadSingle();
            }
            return base.ReadSingle();
        }


        public override double ReadDouble()
        {
            if (Endian == EndianType.BigEndian)
            {
                Debug.LogError("ReadDouble");
                Read(buffer, 0, 8);
                Array.Reverse(buffer);
                return base.ReadDouble();
            }

            return base.ReadDouble();
        }

        public static sbyte ReverseEndianness(sbyte value)
        {
            return value;
        }
        public static short ReverseEndianness(short value)
        {
            return (short)((value & 0x00FF) << 8 | (value & 0xFF00) >> 8);
        }
        public static int ReverseEndianness(int value) => (int)ReverseEndianness((uint)value);


        public static long ReverseEndianness(long value) => (long)ReverseEndianness((ulong)value);
        
        public static byte ReverseEndianness(byte value)
        {
            return value;
        }
        
        public static ushort ReverseEndianness(ushort value)
        {
            // Don't need to AND with 0xFF00 or 0x00FF since the final
            // cast back to ushort will clear out all bits above [ 15 .. 00 ].
            // This is normally implemented via "movzx eax, ax" on the return.
            // Alternatively, the compiler could elide the movzx instruction
            // entirely if it knows the caller is only going to access "ax"
            // instead of "eax" / "rax" when the function returns.

            return (ushort)((value >> 8) + (value << 8));
        }
        
        public static uint ReverseEndianness(uint value)
        {
            // This takes advantage of the fact that the JIT can detect
            // ROL32 / ROR32 patterns and output the correct intrinsic.
            //
            // Input: value = [ ww xx yy zz ]
            //
            // First line generates : [ ww xx yy zz ]
            //                      & [ 00 FF 00 FF ]
            //                      = [ 00 xx 00 zz ]
            //             ROR32(8) = [ zz 00 xx 00 ]
            //
            // Second line generates: [ ww xx yy zz ]
            //                      & [ FF 00 FF 00 ]
            //                      = [ ww 00 yy 00 ]
            //             ROL32(8) = [ 00 yy 00 ww ]
            //
            //                (sum) = [ zz yy xx ww ]
            //
            // Testing shows that throughput increases if the AND
            // is performed before the ROL / ROR.

            uint mask_xx_zz = (value & 0x00FF00FFU);
            uint mask_ww_yy = (value & 0xFF00FF00U);
            return ((mask_xx_zz >> 8) | (mask_xx_zz << 24))
                + ((mask_ww_yy << 8) | (mask_ww_yy >> 24));
        }
        
        public static ulong ReverseEndianness(ulong value)
        {
            // Operations on 32-bit values have higher throughput than
            // operations on 64-bit values, so decompose.

            return ((ulong)ReverseEndianness((uint)value) << 32)
                + ReverseEndianness((uint)(value >> 32));
        }
    }
}
