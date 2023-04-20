using System.IO;
using System.Text;

namespace AssetCheck
{
    public class WEMFile
    {
        private uint Size = 0xFFFFFFFF;

        public ushort Channels;
        public uint SampleRate;
        public uint AverageBytesPerSecond;
        public string AudioFormat;

        public WEMFile(string fileLocation)
        {
            var localFile = File.OpenRead(fileLocation);

            using (BinaryReader br = new BinaryReader(localFile, Encoding.UTF8, true))
            {
                br.ReadBytes(4); // riff
                Size = (uint)br.ReadInt32();
                br.ReadBytes(4); // wave
                br.ReadBytes(4); // "fmt "
                br.ReadInt32(); //sub chunk size

                var audioFormat = br.ReadInt16();

                if (audioFormat == 12352)
                {
                    AudioFormat = "opus";
                }
                else if (audioFormat == 12353)
                {
                    AudioFormat = "wem opus";
                }
                else if (audioFormat == -1)
                {
                    AudioFormat = "vorbis";
                }
                else if (audioFormat == -2)
                {
                    AudioFormat = "pcm";
                }
                else if (audioFormat == -31983)
                {
                    AudioFormat = "adpcm";
                }
                else
                {
                    AudioFormat = "unknown";
                }

                Channels = br.ReadUInt16();
                SampleRate = br.ReadUInt32();
                AverageBytesPerSecond = br.ReadUInt32();
            }
        }

        public uint MediaLength()
        {
            return (uint)UnityEngine.Mathf.CeilToInt((float)Size / (float)AverageBytesPerSecond);
        }

        public uint MediaFileSize()
        {
            return (uint)UnityEngine.Mathf.CeilToInt((float)Size / 1024);
        }
    }
}