using System;
using System.IO;
using System.Runtime.InteropServices;
using Simplygon;

namespace Athena.MeshSimplify
{
    public class SimplygonLoader
    {
        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool SetDllDirectory(string lpPathName);

        public static ISimplygon InitSimplygon(out EErrorCodes errorCode, out string errorMessage)
        {
            return SimplygonLoader.InitSimplygonInternal((string)null, (string)null, out errorCode, out errorMessage);
        }

        public static ISimplygon InitSimplygon(
          string sdkPath,
          out EErrorCodes errorCode,
          out string errorMessage)
        {
            return SimplygonLoader.InitSimplygonInternal(sdkPath, (string)null, out errorCode, out errorMessage);
        }

        public static ISimplygon InitSimplygon(
          string sdkPath,
          string licenseDataText,
          out EErrorCodes errorCode,
          out string errorMessage)
        {
            return SimplygonLoader.InitSimplygonInternal(sdkPath, licenseDataText, out errorCode, out errorMessage);
        }

        private static ISimplygon InitSimplygonInternal(
          string sdkPath,
          string licenseDataText,
          out EErrorCodes errorCode,
          out string errorMessage)
        {
            if (string.IsNullOrWhiteSpace(sdkPath))
                sdkPath = SimplygonLoader.GetSDKPath();
            errorCode = EErrorCodes.NoError;
            errorMessage = string.Empty;
            if (!File.Exists(sdkPath))
            {
                errorCode = EErrorCodes.DLLOrDependenciesNotFound;
                errorMessage = "Simplygon.dll not found";
                return (ISimplygon)null;
            }
            try
            {
                ISimplygon isimplygon = string.IsNullOrWhiteSpace(licenseDataText) ? global::Simplygon.Simplygon.InitializeSimplygon(sdkPath) : global::Simplygon.Simplygon.InitializeSimplygon(sdkPath, licenseDataText);
                errorCode = global::Simplygon.Simplygon.GetLastInitializationError();
                if (errorCode != EErrorCodes.NoError)
                    throw new Exception(string.Format("Failed to load Simplygon from {0}\nErrorCode: {1}", (object)sdkPath, (object)errorCode));
                isimplygon.SendTelemetry(nameof(SimplygonLoader), "C#", "", "{}");
                return isimplygon;
            }
            catch (NotSupportedException ex)
            {
                errorCode = EErrorCodes.AlreadyInitialized;
                string str = string.Format(string.Format("Failed to load Simplygon from {0}\nErrorCode: {1}\nMessage: {2}", (object)sdkPath, (object)errorCode, (object)ex.Message), (object[])Array.Empty<object>());
                Console.Error.WriteLine(str);
                errorMessage = str;
            }
            catch (SEHException ex)
            {
                string str = string.Format(string.Format("Failed to load Simplygon from {0}\nErrorCode: {1}\nMessage: {2}", (object)sdkPath, (object)errorCode, (object)ex.Message), (object[])Array.Empty<object>());
                Console.Error.WriteLine(str);
                if (errorCode == EErrorCodes.NoError)
                    errorCode = EErrorCodes.DLLOrDependenciesNotFound;
                errorMessage = str;
            }
            catch (Exception ex)
            {
                if (errorCode == EErrorCodes.NoError)
                    errorCode = EErrorCodes.DLLOrDependenciesNotFound;
                errorMessage = ex.Message;
                Console.Error.WriteLine(errorMessage);
            }
            finally
            {
                if (errorCode != EErrorCodes.NoError)
                    Console.Error.WriteLine(string.Format("Failed to load Simplygon from {0}\nErrorCode: {1}", (object)sdkPath, (object)errorCode));
            }
            return (ISimplygon)null;
        }

        private static string scriptPath = Path.GetDirectoryName(new System.Diagnostics.StackTrace(true).GetFrame(0).GetFileName());
        private static string GetSDKPath()
        {
            string str = scriptPath.Replace("\\", "/") + "/Plugins";
            SimplygonLoader.SetDllDirectory(str);
            return Path.Combine(str, "Simplygon.dll");
        }

        public static void DisposeSimplygon()
        {
            global::Simplygon.Simplygon.DeinitializeSimplygonThread();
            // string str = scriptPath.Replace("\\", "/") + "/Plugins";
            
            foreach (System.Diagnostics.ProcessModule module in System.Diagnostics.Process.GetCurrentProcess().Modules)
            {
                if (module.ModuleName.Equals("SimplygonCWrapper.dll") || module.ModuleName.Equals("Simplygon.Unity.EditorPlugin"))
                {
                    module.Dispose();
                }
            }
        }
    }
}
