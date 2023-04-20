#include "FinishCallback.h"
#include "CheckPermissionCallback.h"

#ifdef __cplusplus
extern "C"
{
#endif
    void SetNetworkStatusObseverHost(char* host);
    void SetTempStr(char* tempExtension,char* tempMarkStr);
    void RegistFinishCallback(FinishCallback callback);

    void GenerateiOSDownloader(unsigned long long identifier,char* arg);
    void DeleteiOSDownloader(unsigned long long identifier);
    void Refresh(unsigned long long oriIdentifier,char* newArg);
    void StartDownload(unsigned long long identifier);

    long long GetAvgDownloadSpeed(unsigned long long identifier);
    long long GetDownloadTime(unsigned long long identifier);
    long long GetTotalReceive(unsigned long long identifier);
    long long GetRealNeedDownloadSize(unsigned long long identifier);
    bool GetIsAbort(unsigned long long identifier);
    bool GetIsAborting(unsigned long long identifier);

    void SetDownloadSpeedLimit(long long limitSpeed);
    void EnableLimitDownloadSpeed(unsigned long long identifier,bool enable);
    void SetOverrideRetryCountLimit(unsigned long long identifier,int times);
    void SetIsMultiThread(unsigned long long identifier,bool isMultiThread);
    void SetThreadLimit(unsigned long long identifier,int threadLimit);
    void Abort(unsigned long long identifier);
    long long NativeCalcRealDownloadSize(char* destPath,long long targetSize);
    void SetAllowDownloadInBackground(unsigned long long identifier,bool allow);
    void SetAllowCarrierDataNetworkDownload(unsigned long long identifier,bool allow);
    void SetSucNotificationStr(unsigned long long identifier, char* suc);
    void SetFailNotificationStr(unsigned long long identifier, char* fail);
    void NativeShowNotification(char* str);
    bool NativeIsCarrierDataNetwork();
    void CheckNotificationPermission(CheckPermissionCallback callback);
    void GoToNotificationPermissionSetting();

#ifdef __cplusplus
}
#endif
