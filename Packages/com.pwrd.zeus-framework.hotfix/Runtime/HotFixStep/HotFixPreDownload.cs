/*******************************************************************
* Copyright © 2017—2021 Perfect World.Co.Ltd. All rights reserved.
* author: 移动项目支持部 ZEUS FRAMEWORK TEAM
********************************************************************/
using System.IO;
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Zeus.Framework.Http.UnityHttpDownloader;

namespace Zeus.Framework.Hotfix
{
    public class HotFixPreDownload : MonoBehaviour
    {
        private enum State
        {
            None,
            Init,
            WaitingToDownload,
            Downloading,
            WaitLocalAreaNetwork,
            DownloadError,
            Pause,
            Complete,
        }

        private static HotFixPreDownload _instance;
        public static HotFixPreDownload Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new GameObject("HotFixPreDownload").AddComponent<HotFixPreDownload>();
                    DontDestroyOnLoad(_instance.gameObject);
                }
                return _instance;
            }
        }

        volatile State _state = State.None;
        HttpDownloader _downloader;
        List<HFPatchConfigData> _hFPatchDatas;
        int index = 0;
        HotfixLocalConfig _localConfig;
        string[] _serverUrls;
        int retryCount = 2;

        public void Init(List<HFPatchConfigData> HFPatchDatas, HotfixLocalConfig LocalConfig, string[] ServerUrls)
        {
            Debug.Assert(_state == State.None || _state == State.Complete, "state wrong:" + _state);
            _state = State.Init;
            _hFPatchDatas = HFPatchDatas;
            index = 0;
            _localConfig = LocalConfig;
            _serverUrls = ServerUrls;
            retryCount = 2;
            _downloader = null;
        }

        public void Add(List<HFPatchConfigData> HFPatchDatas)
        {
            for (int i = 0; i < HFPatchDatas.Count; i++)
            {
                _hFPatchDatas.Add(HFPatchDatas[i]);
            }
        }

        public bool Run()
        {
            long needDownloadSize = 0;
            for (int i = 0; i < _hFPatchDatas.Count; i++)
            {
                string patchPath = _hFPatchDatas[i].GetPatchSavePath();
                long patchSize = _hFPatchDatas[i].GetPatchSize();
                //因为文件名包含MD5，并且在下载完后会校验MD5，所以此处不需要校验MD5，只需要检测文件是否存在以及文件大小即可
                if (!System.IO.File.Exists(patchPath) || new System.IO.FileInfo(patchPath).Length != patchSize)
                {
                    needDownloadSize += HttpDownloader.CalcRealDownloadSize(patchPath, patchSize);
                }
            }
            if (needDownloadSize >= DiskUtils.CheckAvailableSpaceBytes())
            {
                HotfixLogger.LogError("Can not start predownload, harddisk is full.");
                return false;
            }
            Debug.Assert(_state == State.Init, "state wrong:" + _state);
            this._state = State.WaitingToDownload;
            return true;
        }

        float _updateTimer = 0f;
        private void Update()
        {
            if (_state == State.None || _state == State.Complete || _state == State.Pause)
                return;

            _updateTimer += Time.deltaTime;
            if (_updateTimer < 3)
            {
                return;
            }
            _updateTimer = 0;

            switch (_state)
            {
                case State.WaitingToDownload:
                    ProcessWaitingToDwonload();
                    break;
                case State.Downloading:
                    ProcessDownloading();
                    break;
                case State.WaitLocalAreaNetwork:
                    ProcessWaitLocalAreaNetwork();
                    break;
                case State.DownloadError:
                    ProcessDownloadError();
                    break;
            }
        }

        private void ProcessWaitingToDwonload()
        {
            switch (Application.internetReachability)
            {
                case NetworkReachability.ReachableViaCarrierDataNetwork:
                    {
                        if (_localConfig.allowPreDownloadOnCarrierDataNetwork)
                        {
                            StartDownload();
                        }
                    }
                    break;
                case NetworkReachability.ReachableViaLocalAreaNetwork:
                    {
                        StartDownload();
                    }
                    break;
            }
        }

        public void UpdateNetworkStatusObseverUrls(string[] urls)
        {
            if (Application.platform == RuntimePlatform.IPhonePlayer)
            {
                string observerAddress = "";
                foreach (string url in urls)
                {
                    Uri uri = new Uri(url);
                    if (string.IsNullOrEmpty(observerAddress))
                    {
                        observerAddress = uri.Host;
                    }
                    else
                    {
                        observerAddress += ";" + uri.Host;
                    }
                }
                HttpDownloader.SetNetworkStatusObseverUrls(observerAddress);
            }
        }

        private void StartDownload()
        {
            if (_downloader == null)
            {
                _downloader.Dispose();
                _downloader = null;
                string savePath = _hFPatchDatas[index].GetPatchSavePath();
                string[] urls = _hFPatchDatas[index].GetUrls(_serverUrls);
                UpdateNetworkStatusObseverUrls(urls);
                //限速下载
                DownloadArg arg = new DownloadArg(DownloadType.WholeFile);
                arg.sourceUrls = urls;
                arg.destPath = savePath;
                arg.isMultiThread = false;
                arg.threadLimit = 1;
                arg.targetSize = _hFPatchDatas[index].GetPatchSize();
                arg.enableSpeedLimit = true;
                arg.checkAlgorithm = new CheckAlgorithm(CheckAlgorithmType.Md5, _hFPatchDatas[index].GetPatchMd5());
                arg.callback = OnDownloadFinish;
                _downloader = new HttpDownloader(arg);
                _downloader.SetAllowDownloadInBackground(true);
                _downloader.SetAllowCarrierDataNetworkDownload(_localConfig.allowPreDownloadOnCarrierDataNetwork);
            }
            HttpDownloader.SetLimitSpeed(_localConfig.preDownloadSpeedLimit * 1024L);
            _state = State.Downloading;
            _downloader.StartDownLoad();
        }

        private void OnDownloadFinish(bool suc)
        {
            if (suc)
            {
                index++;
                if (index < _hFPatchDatas.Count)
                {
                    StartDownload();
                }
                else
                {
                    _state = State.Complete;
                    Clear();
                }
            }
            else
            {
                _state = State.DownloadError;
            }
        }

        private void Clear()
        {
            _hFPatchDatas = null;
            _localConfig = null;
            _serverUrls = null;
            if (_downloader != null)
            {
                _downloader.Dispose();
                _downloader = null;
            }
        }

        private void ProcessDownloading()
        {
            Debug.Assert(_state == State.Downloading, "state wrong:" + _state);
            if (!_localConfig.allowPreDownloadOnCarrierDataNetwork &&
                Application.internetReachability == NetworkReachability.ReachableViaCarrierDataNetwork)
            {
                //移动网络不允许下载
                WaitLocalAreaNetwork();
            }
        }

        private void ProcessWaitLocalAreaNetwork()
        {
            Debug.Assert(_state == State.WaitLocalAreaNetwork, "state wrong:" + _state);
            if (Application.internetReachability == NetworkReachability.ReachableViaLocalAreaNetwork)
            {
                //WIFI恢复下载
                StartDownload();
            }
        }

        private void ProcessDownloadError()
        {
            Debug.Assert(_state == State.DownloadError, "state wrong:" + _state);
            if (Application.internetReachability == NetworkReachability.ReachableViaLocalAreaNetwork ||
                (_localConfig.allowPreDownloadOnCarrierDataNetwork && Application.internetReachability == NetworkReachability.ReachableViaCarrierDataNetwork))
            {
                switch (_downloader.TopPriorityError)
                {
                    case ErrorType.NetError:
                        StartDownload();
                        break;
                    case ErrorType.IOException:
                    case ErrorType.Exception:
                    case ErrorType.CombineFile:
                    case ErrorType.MissingFile:
                    case ErrorType.HardDiskFull:
                    case ErrorType.CheckFail:
                        {
                            if (retryCount > 0)
                            {
                                retryCount--;
                                StartDownload();
                            }
                            else
                            {
                                _state = State.None;
                                Clear();
                            }
                        }
                        break;
                }
            }
        }

        private void WaitLocalAreaNetwork()
        {
            _state = State.WaitLocalAreaNetwork;
            _downloader.Abort();
        }

        /// <summary>
        /// 停止后台下载热更文件（停止后不可恢复）
        /// </summary>
        public void StopDownload()
        {
            if (IsDownloading())
            {
                _state = State.None;
                if (_downloader != null)
                {
                    _downloader.Abort();
                    _downloader = null;
                }
            }
        }
        /// <summary>
        /// 是否正在后台下载热更文件(后台下载处于暂停状态也会返回true)
        /// </summary>
        /// <returns></returns>
        public bool IsDownloading()
        {
            return (_state != State.None || _state != State.Complete);
        }
        /// <summary>
        /// 后台下载是否处于暂停状态
        /// </summary>
        /// <returns></returns>
        public bool IsPause()
        {
            return _state != State.Pause;
        }
        /// <summary>
        /// 暂停后台下载（可恢复）
        /// </summary>
        /// <returns></returns>
        public void PauseDownload()
        {
            if (IsDownloading())
            {
                _state = State.Pause;
                if (_downloader != null)
                {
                    _downloader.Abort();
                }
            }
        }
        /// <summary>
        /// 恢复后台下载
        /// </summary>
        /// <returns></returns>
        public void ResumeDownload()
        {
            if (_state == State.Pause)
            {
                _state = State.WaitingToDownload;
            }
        }

        public List<string> GetDownloadingVersions()
        {
            if (IsDownloading() && _hFPatchDatas != null)
            {
                List<string> list = new List<string>();
                for (int i = 0; i < _hFPatchDatas.Count; i++)
                {
                    list.Add(_hFPatchDatas[i].TargetVersion);
                }
                return list;
            }
            return null;
        }

        private void OnApplicationQuit()
        {
            StopDownload();
        }

    }
}