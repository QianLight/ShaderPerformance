using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

using System.IO;

public class SecretAgreementGenerator : Editor
{
    [MenuItem(@"Tools/Generator/SecretAgreementGenarator")]
    static void GenarateSecretAgreement()
    {
        string path = Application.dataPath + "/Table/SDK/UserConfidentialityAgreement.txt";
        string savePath = Application.dataPath+ "/BundleRes/Table/userconfidentialityagreement.bytes";
        if (File.Exists(path))
        {
            string content = File.ReadAllText(path);
            byte[] bytes = System.Text.Encoding.UTF8.GetBytes(content);
            File.WriteAllBytes(savePath, bytes);
            Debug.Log("SecretAgreement update success");
        }
        else
        {
            Debug.LogError(path + "not exist");
        }

    }
}
