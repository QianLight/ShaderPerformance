/*******************************************************************
* Copyright © 2017—2022 Perfect World.Co.Ltd. All rights reserved.
* author: 移动项目支持部 ZEUS FRAMEWORK TEAM
********************************************************************/
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Zeus.Framework.Asset;

public static class DecodeHelper
{
    private static string Md4Hash(string input)
    {
        byte[] bytesRead = null;
        try
        {
            bytesRead = System.Text.Encoding.Default.GetBytes(input);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }

        List<byte> bytes = new List<byte>();
        foreach (var by in bytesRead)
        {
            bytes.Add(by);
        }

        uint bitCount = (uint) (bytes.Count) * 8;

        bytes.Add(128);
        while (bytes.Count % 64 != 56)
        {
            bytes.Add(0);
        }

        var uints = new List<uint>();
        for (int i = 0; i + 3 < bytes.Count; i += 4)
        {
            uints.Add(bytes[i] | (uint) bytes[i + 1] << 8 | (uint) bytes[i + 2] << 16 | (uint) bytes[i + 3] << 24);
        }

        uints.Add(bitCount);
        uints.Add(0);

        uint A = 0x67452301;
        uint B = 0xefcdab89;
        uint C = 0x98badcfe;
        uint D = 0x10325476;

        Func<uint, uint, uint, uint> F = (x, y, z) => (x & y) | (~x & z);
        Func<uint, uint, uint, uint> G = (x, y, z) => (x & y) | (x & z) | (y & z);
        Func<uint, uint, uint, uint> H = (x, y, z) => x ^ y ^ z;

        Func<uint, uint, uint> leftRotate = (x, y) => x << (int) y | x >> 32 - (int) y;

        Func<uint, uint, uint, uint, uint, ushort, uint> ROUND1 = (a, b, c, d, x, m) => A = leftRotate((a + F(b, c, d) + x), m);
        Func<uint, uint, uint, uint, uint, ushort, uint> ROUND11 = (d, a, b, c, x, m) => D = leftRotate((d + F(a, b, c) + x), m);
        Func<uint, uint, uint, uint, uint, ushort, uint> ROUND12 = (c, d, a, b, x, m) => C = leftRotate((c + F(d, a, b) + x), m);
        Func<uint, uint, uint, uint, uint, ushort, uint> ROUND13 = (b, c, d, a, x, m) => B = leftRotate((b + F(c, d, a) + x), m);

        Func<uint, uint, uint, uint, uint, ushort, uint> ROUND2 = (a, b, c, d, x, m) => A = leftRotate((a + G(b, c, d) + x + (uint) 0x5a827999), m);
        Func<uint, uint, uint, uint, uint, ushort, uint> ROUND21 = (d, a, b, c, x, m) => D = leftRotate((d + G(a, b, c) + x + (uint) 0x5a827999), m);
        Func<uint, uint, uint, uint, uint, ushort, uint> ROUND22 = (c, d, a, b, x, m) => C = leftRotate((c + G(d, a, b) + x + (uint) 0x5a827999), m);
        Func<uint, uint, uint, uint, uint, ushort, uint> ROUND23 = (b, c, d, a, x, m) => B = leftRotate((b + G(c, d, a) + x + (uint) 0x5a827999), m);

        Func<uint, uint, uint, uint, uint, ushort, uint> ROUND3 = (a, b, c, d, x, m) => A = leftRotate((a + H(b, c, d) + x + (uint) 0x6ed9eba1), m);
        Func<uint, uint, uint, uint, uint, ushort, uint> ROUND31 = (d, a, b, c, x, m) => D = leftRotate((d + H(a, b, c) + x + (uint) 0x6ed9eba1), m);
        Func<uint, uint, uint, uint, uint, ushort, uint> ROUND32 = (c, d, a, b, x, m) => C = leftRotate((c + H(d, a, b) + x + (uint) 0x6ed9eba1), m);
        Func<uint, uint, uint, uint, uint, ushort, uint> ROUND33 = (b, c, d, a, x, m) => B = leftRotate((b + H(c, d, a) + x + (uint) 0x6ed9eba1), m);

        for (int i = 0; i < uints.Count() / 16; i++)
        {
            uint[] X = new uint[16];
            for (int j = 0; j < 16; j++)
            {
                X[j] = uints[i * 16 + j];
            }

            uint AA = A;
            uint BB = B;
            uint CC = C;
            uint DD = D;

            ROUND1(A, B, C, D, X[0], 3);
            ROUND11(D, A, B, C, X[1], 7);
            ROUND12(C, D, A, B, X[2], 11);
            ROUND13(B, C, D, A, X[3], 19);
            ROUND1(A, B, C, D, X[4], 3);
            ROUND11(D, A, B, C, X[5], 7);
            ROUND12(C, D, A, B, X[6], 11);
            ROUND13(B, C, D, A, X[7], 19);
            ROUND1(A, B, C, D, X[8], 3);
            ROUND11(D, A, B, C, X[9], 7);
            ROUND12(C, D, A, B, X[10], 11);
            ROUND13(B, C, D, A, X[11], 19);
            ROUND1(A, B, C, D, X[12], 3);
            ROUND11(D, A, B, C, X[13], 7);
            ROUND12(C, D, A, B, X[14], 11);
            ROUND13(B, C, D, A, X[15], 19);

            ROUND2(A, B, C, D, X[0], 3);
            ROUND21(D, A, B, C, X[4], 5);
            ROUND22(C, D, A, B, X[8], 9);
            ROUND23(B, C, D, A, X[12], 13);
            ROUND2(A, B, C, D, X[1], 3);
            ROUND21(D, A, B, C, X[5], 5);
            ROUND22(C, D, A, B, X[9], 9);
            ROUND23(B, C, D, A, X[13], 13);
            ROUND2(A, B, C, D, X[2], 3);
            ROUND21(D, A, B, C, X[6], 5);
            ROUND22(C, D, A, B, X[10], 9);
            ROUND23(B, C, D, A, X[14], 13);
            ROUND2(A, B, C, D, X[3], 3);
            ROUND21(D, A, B, C, X[7], 5);
            ROUND22(C, D, A, B, X[11], 9);
            ROUND23(B, C, D, A, X[15], 13);

            ROUND3(A, B, C, D, X[0], 3);
            ROUND31(D, A, B, C, X[8], 9);
            ROUND32(C, D, A, B, X[4], 11);
            ROUND33(B, C, D, A, X[12], 15);
            ROUND3(A, B, C, D, X[2], 3);
            ROUND31(D, A, B, C, X[10], 9);
            ROUND32(C, D, A, B, X[6], 11);
            ROUND33(B, C, D, A, X[14], 15);
            ROUND3(A, B, C, D, X[1], 3);
            ROUND31(D, A, B, C, X[9], 9);
            ROUND32(C, D, A, B, X[5], 11);
            ROUND33(B, C, D, A, X[13], 15);
            ROUND3(A, B, C, D, X[3], 3);
            ROUND31(D, A, B, C, X[11], 9);
            ROUND32(C, D, A, B, X[7], 11);
            ROUND33(B, C, D, A, X[15], 15);

            A += AA;
            B += BB;
            C += CC;
            D += DD;
        }

        byte[] exportBytes = new[] {A, B, C, D}.SelectMany(BitConverter.GetBytes).ToArray();
        return BitConverter.ToString(exportBytes).Replace("-", "").ToLower();
    }

    private static string Md5Hash(string input)
    {
        byte[] hash;
        using (var md5 = MD5.Create())
        {
            var bytes = Encoding.ASCII.GetBytes(input);
            hash = md5.ComputeHash(bytes);
        }

        var internalFile = BitConverter.ToString(hash, 0).ToLower().Replace("-", "");
        return string.Format(internalFile);
    }


    public static HashSet<string> FindBundleNames()
    {
        var bundles = new HashSet<string>();
        
        foreach (var directory in Directory.GetDirectories("AssetBundleCache"))
        {
            var manifestFile = Path.Combine(directory, Path.GetDirectoryName(directory) + ".menifest");
            if (File.Exists(manifestFile))
            {
                foreach (var bundle in AssetBundleUtils.LoadDependenciesForManifestFile(manifestFile).Keys)
                {
                    bundles.Add(bundle);
                }
            }
        }
        
        foreach (var directory in Directory.GetDirectories("Bundles"))
        {
            var manifestFile = Path.Combine(directory, Path.GetDirectoryName(directory) + ".menifest");
            if (File.Exists(manifestFile))
            {
                foreach (var bundle in AssetBundleUtils.LoadDependenciesForManifestFile(manifestFile).Keys)
                {
                    bundles.Add(bundle);
                }
            }
        }

        var files = Directory.GetFiles("AssetBundleCache", "*.data", SearchOption.AllDirectories);
        foreach (var file in files)
        {
            bundles.Add(Path.GetFileName(file));
        }
        
        files = Directory.GetFiles("Bundles", "*.data", SearchOption.AllDirectories);
        foreach (var file in files)
        {
            bundles.Add(Path.GetFileName(file));
        }

        return bundles;
    }
    
    public static Dictionary<string, string> GetDictionary(IEnumerable<string> bundles)
    {
        var dictionary = new Dictionary<string, string>();
        foreach (var bundle in bundles)
        {
            dictionary.Add(Md4Hash(bundle), bundle);
            dictionary.Add(Md5Hash(bundle), bundle);
            dictionary.Add(Md4Hash(Path.GetFileNameWithoutExtension(bundle)), bundle);
            dictionary.Add(Md5Hash(Path.GetFileNameWithoutExtension(bundle)), bundle);
        }

        return dictionary;
    }
    
    [UnityEditor.MenuItem("Zeus/Asset/打印解密信息", false, 10)]
    public static void Print()
    {
        var dictionary = GetDictionary(FindBundleNames());
        var fileStream = File.Create("DecodeDic.txt");
        var sw = new StreamWriter(fileStream);
        foreach (var pair in dictionary)
        {
            sw.Write(pair.Key);
            sw.Write("  ");
            sw.Write(pair.Value);
            sw.Write("\n");
        }
        sw.Flush();
        sw.Close();
    }
}
