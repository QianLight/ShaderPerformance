using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using B;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;
using Unity.EditorCoroutines.Editor;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace UnityEditor.Recorder
{
    public class CFAudioRecorder
    {
        private static string AudioTmpPath
        {
            get { return EditorPrefs.GetString("PREF_RECD", Application.dataPath) + "/tmpAudio.wav"; }
        }
        
        
        private static string MovieTmpPath
        {
            get { return EditorPrefs.GetString("PREF_RECD", Application.dataPath) + "/tmpMovie.mp4"; }
        }


        // public static bool RecordAudioExeState
        // {
        //     get { return PlayerPrefs.GetInt("RecordAudioExeState") == 1; }
        //
        //     set { PlayerPrefs.SetInt("RecordAudioExeState", value ? 1 : 0); }
        // }

        [MenuItem("Tools/引擎/音频/获取设备列表")]
        public static void GetRecordAudioDevicesList()
        {
            Debug.Log("Microphone.devices:" + Microphone.devices.Length);

            var allSounds = GetSoundDevices();

            Debug.Log("allSounds:" + allSounds.Length);
            foreach (var sd in allSounds)
            {
                Debug.Log("sd:" + sd);
            }
        }

        [DllImport("winmm.dll", SetLastError=true)]
        static extern uint waveOutGetNumDevs();

        [DllImport("winmm.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern uint waveOutGetDevCaps(uint hwo,ref WAVEOUTCAPS pwoc,uint cbwoc);

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        public struct WAVEOUTCAPS {
            public ushort wMid;
            public ushort wPid;
            public uint vDriverVersion;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
            public string szPname;
            public uint dwFormats;
            public ushort wChannels;
            public ushort wReserved1;
            public uint dwSupport;
        }

        public static string [] GetSoundDevices() {
            uint devices = waveOutGetNumDevs();
            string [] result = new string[devices];
            WAVEOUTCAPS caps = new WAVEOUTCAPS();

            for(uint i = 0; i < devices; i++) {
                waveOutGetDevCaps(i, ref caps, (uint)Marshal.SizeOf(caps));
                result[i] = caps.szPname;
            }
            return result;
        }

        [MenuItem("Tools/引擎/音频/初始化")]
        public static void InitRecordAudio()
        {
            if (ProcessAudio != null) return;
            Debug.Log("StartRecordAudio:" + Time.realtimeSinceStartup + "  " + AudioTmpPath);

            Start("captura-cli.exe", string.Format("start --source none --speaker {2} --file {0} --y true --msg {1}",
                AudioTmpPath,
                LibraryFolder, AudioDeviceIndex));
        }

        private static Process ProcessAudio
        {
            get
            {
                Process[] allProcesses = Process.GetProcessesByName("captura-cli");
                if (allProcesses == null || allProcesses.Length == 0) return null;

                Process pro = allProcesses[0];
                return pro;
            }
        }



        private static string m_MoviePath = "";

        public static void SetMoviePath(string path)
        {
            m_MoviePath = path;

            Debug.Log("CFAudioRecorder:" + m_MoviePath);
        }



        [MenuItem("Tools/引擎/音频/开始")]
        public static void StartRecordAudio()
        {
            if (ProcessAudio == null) return;

            WriteMsg("1");
            //ProcessAudio.StandardInput.WriteLine('g');
            Debug.Log("  :" + Time.realtimeSinceStartup + "  " + m_MoviePath);
        }

        [MenuItem("Tools/引擎/音频/停止")]
        public static void StopRecordAudio()
        {
            if (ProcessAudio == null) return;

            EditorCoroutineUtility.StartCoroutineOwnerless(StopRecordAudioEnumerator());
        }

        static IEnumerator StopRecordAudioEnumerator()
        {
            yield return new EditorWaitForSeconds(1);
            WriteMsg("2");
            //ProcessAudio.StandardInput.WriteLine('s');
            Debug.Log("StopRecordAudio:" + Time.realtimeSinceStartup);

            string videoTime = "";
            Process ffprobe = Start("ffprobe.exe", string.Format("-v error -show_entries format=duration " +
               
                                                                 "-of default=noprint_wrappers=1:nokey=1 {0}", m_MoviePath),
                str =>
                {
                    videoTime = str;
                });

            ffprobe.WaitForExit();
            yield return new EditorWaitForSeconds(1);
            
            Debug.Log("ffprobe video time:"+videoTime);
            
            if(File.Exists(MovieTmpPath)) File.Delete(MovieTmpPath);
            
            Process ffmpeg = Start("ffmpeg.exe", string.Format("-i {0} -i {1} -c:v copy -c:a aac -t {2} {3}", m_MoviePath,
                AudioTmpPath, videoTime, MovieTmpPath));
            ffmpeg.WaitForExit();
            
            Debug.Log("ffmpeg merge video and audio:" + Time.realtimeSinceStartup);
            
            File.Copy(MovieTmpPath,m_MoviePath,true);
            File.Delete(MovieTmpPath);
            File.Delete(AudioTmpPath);
        }

        private static void KillAllProcess()
        {
            Process[] allProcesses = Process.GetProcessesByName("captura-cli");
            for (int i = 0; i < allProcesses.Length; i++)
            {
                allProcesses[i].Kill();
            }
        }


        [MenuItem("Tools/引擎/音频/结束")]
        public static void DestoryRecordAudio()
        {
            if (ProcessAudio == null) return;
            
            KillAllProcess();
            // ProcessAudio.StandardInput.WriteLine('q');
            // ProcessAudio.WaitForExit();
            Debug.Log("DestoryRecordAudio:" + Time.realtimeSinceStartup);
        }

        private static string ExeFolder
        {
            get
            {
                var path = Application.dataPath.Replace("/Assets", "/Shell/Captura/");
                //var path = "D:/Project/Captura/src/Captura.Console/bin/Debug/";
                return path;
            }
        }

        private static string LibraryFolder
        {
            get
            {
                var path = Application.dataPath.Replace("/Assets", "/Library/"); //  //ExeFolder + "msg.txt";

                // Debug.Log("msgtxt:" + path);
                
                return path;
            }
        }


        private static string MsgTxt
        {
            get
            {
                var path = LibraryFolder + "msg.txt";
                return path;
            }
        }


        private static string AudioDevicesTxt
        {
            get
            {
                var path = LibraryFolder + "audioDevices.txt";
                return path;
            }
        }


        private static string[] allDevices;

        public static string[] GetAllDevices
        {
            get
            {
                if (allDevices == null && File.Exists(AudioDevicesTxt))
                {
                    allDevices = File.ReadAllLines(AudioDevicesTxt);
                }

                return allDevices;
            }
        }

        public static int AudioDeviceIndex
        {
            get { return PlayerPrefs.GetInt("AudioDeviceIndex", 0); }

            set
            {
                int nLastValue = AudioDeviceIndex;
                if (value == nLastValue) return;
                PlayerPrefs.SetInt("AudioDeviceIndex", value);
                EditorCoroutineUtility.StartCoroutineOwnerless(ReopenCapturaCli());
            }
        }

        
        static IEnumerator ReopenCapturaCli()
        {
            DestoryRecordAudio();
            yield return new EditorWaitForSeconds(1);
            while (ProcessAudio != null)
            {
                yield return new EditorWaitForSeconds(1);
            }

            InitRecordAudio();
        }



        static Process Start( string exeName, string Arguments,Action<string> callback=null)
        {
            var path = ExeFolder +exeName;

            var process = new Process
            {
                StartInfo =
                {
                    FileName = path,
                    Arguments = Arguments,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardInput = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                },
                EnableRaisingEvents = true
            };

            process.Start();

            process.BeginErrorReadLine();
            process.BeginOutputReadLine();

            void Write(string Data, string Prefix)
            {
                if (string.IsNullOrWhiteSpace(Data))
                    return;

                UnityEngine.Debug.Log($"{Prefix}: {Data}");

                if (callback != null) callback(Data);
            }

            process.ErrorDataReceived += (S, E) => Write(E.Data, "Err");
            process.OutputDataReceived += (S, E) => Write(E.Data, "Out");

            return process;
        }


        private static void WriteMsg(string msg)
        {
            string txt = MsgTxt;
            File.WriteAllText(txt, msg);
        }
    }


}
