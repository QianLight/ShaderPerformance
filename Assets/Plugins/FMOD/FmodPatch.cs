using System.IO;
using UnityEngine;


namespace FMODUnity
{
    public partial class RuntimeManager
    {

        private static bool CheckBankPatch(string bankName, bool loadSamples = false)
        {
            string path = Path.Combine(Application.persistentDataPath, bankName + ".bank");
            try
            {
                if (File.Exists(path))
                {
                    LoadExternalBank(path, bankName, loadSamples);
                    return true;
                }
            }
            catch (IOException e)
            {
                Debug.LogError("Fmod error:" + e.Message);
            }
            return false;
        }



        private static void LoadExternalBank(string bankPath, string bankName, bool loadSamples = false)
        {
            LoadedBank loadedBank = new LoadedBank();
            FMOD.RESULT loadResult;
            loadResult = Instance.studioSystem.loadBankFile(bankPath, FMOD.Studio.LOAD_BANK_FLAGS.NORMAL, out loadedBank.Bank);
            Instance.loadedBankRegister(loadedBank, bankPath, bankName, loadSamples, loadResult);
        }

    }

}