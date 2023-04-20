using System;
using GMSDK;

namespace GSDK
{
    public class SystemService : ISystemService
    {
        readonly MainSDK _gsdk;
        private bool _isRegisterCharging;
        private bool _isRegisterHeadset;

        public SystemService()
        {
            _gsdk = GMSDKMgr.instance.SDK;
        }

        private event SystemFetchChargingStatusEventHandler chargingEvent;

        public event SystemFetchChargingStatusEventHandler ChargingEvent
        {
            add
            {
                chargingEvent += value;
                if (!_isRegisterCharging)
                {
                    RegisterChargingReceiver();
                    _isRegisterCharging = true;
                }
            }

            remove
            {
                chargingEvent -= value;
                if (chargingEvent == null)
                {
                    UnRegisterChargingReceiver();
                    _isRegisterCharging = false;
                }
            }
        }

        private event SystemFetchHeadsetStatusEventHandler headsetEvent;

        public event SystemFetchHeadsetStatusEventHandler HeadsetEvent
        {
            add
            {
                headsetEvent += value;
                if (!_isRegisterHeadset)
                {
                    RegisterHeadsetReceiver();
                    _isRegisterHeadset = true;
                }
            }

            remove
            {
                headsetEvent -= value;
                if (headsetEvent == null)
                {
                    UnRegisterHeadsetReceiver();
                    _isRegisterHeadset = false;
                }
            }
        }

        public NetworkState NetworkState
        {
            get { return (NetworkState) _gsdk.SdkFetchNetState(); }
        }

        public bool Charging
        {
            get { return _gsdk.SdkIsCharging(); }
        }

        public double Electricity
        {
            get { return _gsdk.SdkBatteryLevel(); }
        }

        public bool HeadsetPlugging
        {
            get { return _gsdk.SdkIsHeadsetPlugged(); }
        }

        public double ScreenBrightness
        {
            get { return _gsdk.SdkScreenBrightness(); }

            set
            {
                if (value < 0 || value > 1)
                {
                    throw new ArgumentOutOfRangeException("value");
                }

                _gsdk.SdkSetScreenBrightness((float) value);
            }
        }

        public SystemScreenType ScreenType
        {
            get
            {
                var type = _gsdk.SdkIsScreenAnomalous();
                return (SystemScreenType) type;
            }
        }

        public double CurrentWindowBrightness
        {
            get { return _gsdk.SdkCurrentWindowBrightness(); }

            set
            {
                if (value < 0 || value > 1)
                {
                    throw new ArgumentOutOfRangeException("value");
                }

                _gsdk.SdkSetCurrentWindowBrightness((float) value);
            }
        }

#if UNITY_ANDROID
        public void CheckEmulator(SystemCheckEmulatorDelegate emulatorDelegate)
        {
            if (emulatorDelegate == null)
            {
                GLog.LogError("Please initialize SystemCheckEmulatorDelegate.");
                return;
            }

            _gsdk.SdkDeviceIsEmulator(res =>
            {
                InnerTools.SafeInvoke(() =>
                {
                    emulatorDelegate(SystemInnerTools.ConvertSystemError(res), res.isEmulator,
                        res.emulatorBrand);
                });
            });
        }
#endif

        private void RegisterHeadsetReceiver()
        {
            if (headsetEvent == null)
            {
                GLog.LogError("FetchHeadsetStatusEvent is null at RegisterHeadsetReceiver");
                return;
            }

            _gsdk.SdkHeadsetRegisterReceiver(callbackResult =>
            {
                InnerTools.SafeInvoke(() => { headsetEvent(callbackResult.status); });
            });
        }

        private void UnRegisterHeadsetReceiver()
        {
            _gsdk.SdkHeadsetUnregisterReceiver();
        }

        private void RegisterChargingReceiver()
        {
            if (chargingEvent == null)
            {
                GLog.LogError("FetchChargingStatusEvent is null at RegisterChargingReceiver");
                return;
            }

            _gsdk.SdkBatteryRegisterReceiver(callbackResult =>
            {
                InnerTools.SafeInvoke((() => { chargingEvent(callbackResult.status); }));
            });
        }

        private void UnRegisterChargingReceiver()
        {
            _gsdk.SdkBatteryUnregisterReceiver();
        }
    }
}