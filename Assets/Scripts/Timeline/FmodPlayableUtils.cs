#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEngine;
using FMODUnity;
using System.IO;
// using UnityEditor;

public class FmodPlayableUtils : MonoBehaviour 
{
    static FMOD.Studio.System system;

    const string CacheAssetName = "FMODStudioCache";
    const string CacheAssetFullName = "Assets/" + CacheAssetName + ".asset";
    const string StringBankExtension = "strings.bank";
    const string BankExtension = "bank";

    //static private bool _inited = false;


    public FMOD.Studio.EventInstance Play(string clip, int misecond)
    {
        if (!system.isValid())
            Init();

        return PlayablePreview(clip, misecond);
    }

    public static FMOD.Studio.System System
    {
        get
        {
            if (!system.isValid())
            {
                CreateSystem();
            }
            return system;
        }
    }

    public static void CheckResult(FMOD.RESULT result)
    {
        if (result != FMOD.RESULT.OK)
        {
            UnityEngine.Debug.LogError(string.Format("FMOD Studio: Encounterd Error: {0} {1}", result, FMOD.Error.String(result)));
        }
    }

    private static void CreateSystem()
    {
        UnityEngine.Debug.Log("FMOD Studio: Creating editor system instance");
        RuntimeUtils.EnforceLibraryOrder();

        FMOD.RESULT result = FMOD.Debug.Initialize(FMOD.DEBUG_FLAGS.LOG, FMOD.DEBUG_MODE.FILE, null, "fmod_editor.log");
        if (result != FMOD.RESULT.OK)
        {
            UnityEngine.Debug.LogWarning("FMOD Studio: Cannot open fmod_editor.log. Logging will be disabled for importing and previewing");
        }

        CheckResult(FMOD.Studio.System.create(out system));

        FMOD.System lowlevel;
        CheckResult(system.getCoreSystem(out lowlevel));

        // Use play-in-editor speaker mode for event browser preview and metering
        //lowlevel.setSoftwareFormat(0, (FMOD.SPEAKERMODE)Settings.Instance.GetSpeakerMode(FMODPlatform.Default), 0);
        lowlevel.setSoftwareFormat(0, (FMOD.SPEAKERMODE)Settings.Instance.GetEditorSpeakerMode(), 0);


        CheckResult(system.initialize(256, FMOD.Studio.INITFLAGS.ALLOW_MISSING_PLUGINS | FMOD.Studio.INITFLAGS.SYNCHRONOUS_UPDATE, FMOD.INITFLAGS.NORMAL, IntPtr.Zero));

        FMOD.ChannelGroup master;
        CheckResult(lowlevel.getMasterChannelGroup(out master));
        FMOD.DSP masterHead;
        CheckResult(master.getDSP(FMOD.CHANNELCONTROL_DSP_INDEX.HEAD, out masterHead));
        CheckResult(masterHead.setMeteringEnabled(false, true));
    }

    private FMOD.Studio.EventDescription eventDescription;

    private FMOD.Studio.EventInstance instance;

    private bool Init()
    {
        //if (!_inited)
        {
            string defaultBankFolder = null;
            defaultBankFolder = GetBankDirectory();

            List<String> stringBanks = new List<string>(0);
            var files = Directory.GetFiles(defaultBankFolder, "*." + StringBankExtension);
            stringBanks = new List<string>(files);

            stringBanks.RemoveAll((x) => Path.GetFileName(x).StartsWith("._"));
            //stringBanks.Sort((a, b) => File.GetLastWriteTime(b).CompareTo(File.GetLastWriteTime(a)));
            string stringBankPath = stringBanks[0];

            FMOD.Studio.Bank stringBank;
            CheckResult(System.loadBankFile(stringBankPath, FMOD.Studio.LOAD_BANK_FLAGS.NORMAL, out stringBank));
            int stringCount;
            stringBank.getStringCount(out stringCount);
            List<string> bankFileNames = new List<string>();
            for (int stringIndex = 0; stringIndex < stringCount; stringIndex++)
            {
                string currentString;
                Guid currentGuid;
                stringBank.getStringInfo(stringIndex, out currentGuid, out currentString);
                const string BankPrefix = "bank:/";
                int BankPrefixLength = BankPrefix.Length;
                if (currentString.StartsWith(BankPrefix))
                {
                    string bankFileName = currentString.Substring(BankPrefixLength) + "." + BankExtension;
                    if (!bankFileName.Contains(StringBankExtension)) // filter out the strings bank
                    {
                        bankFileNames.Add(bankFileName);
                    }
                }
            }

            string[] folderContents = Directory.GetFiles(defaultBankFolder);

            foreach (string bankFileName in bankFileNames)
            {
                //string bankPath = ArrayUtility.Find(folderContents, x => (string.Equals(bankFileName, Path.GetFileName(x), StringComparison.CurrentCultureIgnoreCase)));
                string bankPath = defaultBankFolder + "\\" + bankFileName;

                Debug.Log(bankPath);
                FileInfo bankFileInfo = new FileInfo(bankPath);

                FMOD.Studio.Bank bank;
                FMOD.RESULT LoadResult = System.loadBankFile(bankFileInfo.FullName, FMOD.Studio.LOAD_BANK_FLAGS.NORMAL, out bank);

                if (LoadResult == FMOD.RESULT.ERR_EVENT_ALREADY_LOADED)
                {
                    System.getBank(bankFileName, out bank);
                    bank.unload();
                    LoadResult = System.loadBankFile(bankFileInfo.FullName, FMOD.Studio.LOAD_BANK_FLAGS.NORMAL, out bank);
                }
            }

            //_inited = true;
        }

        return true;
    }
    private FMOD.Studio.EventInstance PlayablePreview(string clip, int misecond)
    {
        Guid guid = PathToGUID(clip);

        CheckResult(System.getEventByID(guid, out eventDescription));
        CheckResult(eventDescription.createInstance(out instance));
        instance.setTimelinePosition(misecond);
        instance.start();
        return instance;
    }

    const string BuildFolder = "Build";
    private static string GetBankDirectory()
    {
        if (Settings.Instance.HasSourceProject && !String.IsNullOrEmpty(Settings.Instance.SourceProjectPath))
        {
            string projectPath = Settings.Instance.SourceProjectPath;
            string projectFolder = Path.GetDirectoryName(projectPath);
            return Path.Combine(projectFolder, BuildFolder);
        }
        else if (!String.IsNullOrEmpty(Settings.Instance.SourceBankPath))
        {
            return Settings.Instance.SourceBankPath;
        }
        return null;
    }

    private static Guid PathToGUID(string path)
    {
        Guid guid = Guid.Empty;
        if (path.StartsWith("{"))
        {
            FMOD.Studio.Util.parseID(path, out guid);
        }
        else
        {
            var result = System.lookupID(path, out guid);
            if (result == FMOD.RESULT.ERR_EVENT_NOTFOUND)
            {
                return new Guid();
            }
        }
        return guid;
    }
    
    public void Update()
    {
        if (system.isValid())
        {
            CheckResult(system.update());
        }
    }

    public void OnDestroy()
    {
        if (system.isValid())
        {
            UnityEngine.Debug.Log("FMOD Studio: Destroying editor system instance");
            system.release();
            system.clearHandle();
        }
    }

    public static void Destroy()
    {
        if (system.isValid())
        {
            UnityEngine.Debug.Log("FMOD Studio: Destroying editor system instance");
            system.release();
            system.clearHandle();
        }
    }

}
#endif