using CFEngine;
using CFUtilPoolLib;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

public class RSignal
{
    //signal
    public static byte SignalType_QteClick = DirectorSignalEmmiter.EngineSignalCount;
    public static byte SignalType_QteContinuous = (byte)(SignalType_QteClick + 1);
    public static byte SignalType_LongPress = (byte)(SignalType_QteContinuous + 1);
    public static byte SignalType_UI = (byte)(SignalType_LongPress + 1);
    public static byte SignalType_SpeedLine = (byte)(SignalType_UI + 1);
    public static byte SignalType_Custom = (byte)(SignalType_SpeedLine + 1);
    public static byte SignalType_Layer = (byte)(SignalType_Custom + 1);
    public static byte ActionType_Layer = (byte)(SignalType_Layer + 1);
    public static byte FMod_Layer = (byte)(ActionType_Layer + 1);
    public static byte AutoFade_Layer = (byte)(FMod_Layer + 1);
    public static byte Movie_Layer = (byte)(AutoFade_Layer + 1);
    public static byte HideObject_Layer = (byte)(Movie_Layer + 1);
    public static byte SceneSignal_Layer = (byte)(HideObject_Layer + 1);
    public static byte Signal_QTE = (byte)(SceneSignal_Layer + 1);
    public static byte CameraTransition_Signal = (byte)(Signal_QTE + 1);
    public static byte HideLensflare_Signal = (byte)(CameraTransition_Signal + 1);
    public static byte SmartShadow_Signal = (byte)(HideLensflare_Signal + 1);

    private static PlayableDirector director { get { return EngineContext.director; } }

    public static IXTimeline Ixtl { get { return RTimeline.Ixtl; } }


    public static void InnerInit()
    {
        if (DirectorHelper.createSignal == null)
        {
            DirectorHelper.createSignal = CreateSignal;
        }
        if (DirectorHelper.realeaseSignal == null)
        {
            DirectorHelper.realeaseSignal = ReleaseSignal;
        }
    }

    static DirectorSignalEmmiter CreateSignal(byte type)
    {
        if (type == SignalType_QteClick)
        {
            return DirectorHelper.CreateSignal<QteClickSignal>();
        }
        //else if (type == SignalType_QteContinuous)
        //{
        //    return DirectorHelper.CreateSignal<QteContinuousSignal>();
        //}
        //else if (type == SignalType_LongPress)
        //{
        //    return DirectorHelper.CreateSignal<QteLongPressSignal>();
        //}
        //else
        if (type == SignalType_UI)
        {
            return DirectorHelper.CreateSignal<UISignal>();
        }
        else if (type == SignalType_SpeedLine)
        {
            return DirectorHelper.CreateSignal<SpeedLineSignal>();
        }
        else if (type == SignalType_Custom)
        {
            return DirectorHelper.CreateSignal<CustomSignal>();
        }
        else if (type == ActionType_Layer)
        {
            return DirectorHelper.CreateSignal<ActionSignal>();
        }
        else if (type == FMod_Layer)
        {
            return DirectorHelper.CreateSignal<FModSignal>();
        }
        else if (type == AutoFade_Layer)
        {
            return DirectorHelper.CreateSignal<AutofadeSignal>();
        }
        else if (type == Movie_Layer)
        {
            return DirectorHelper.CreateSignal<Mp4Signal>();
        }
        else if (type == HideObject_Layer)
        {
            return DirectorHelper.CreateSignal<HideObjectSignal>();
        }
        else if (type == SceneSignal_Layer)
        {
            return DirectorHelper.CreateSignal<SceneSignal>();
        }
        else if (type == Signal_QTE)
        {
            return DirectorHelper.CreateSignal<QteClickSignal>();
        }
        else if (type == CameraTransition_Signal)
        {
            return DirectorHelper.CreateSignal<CameraTransitionSignal>();
        }
        else if (type == HideLensflare_Signal)
        {
            return DirectorHelper.CreateSignal<SwitchLensflareStateSignal>();
        }
        else if (type == SmartShadow_Signal)
        {
            return DirectorHelper.CreateSignal<SmartShadowSignal>();
        }
        return null;
    }

    static void ReleaseSignal(DirectorSignalEmmiter signal)
    {
        switch (signal)
        {
            case QteClickSignal _:
                DirectorHelper.ReleaseSignal<QteClickSignal>(signal);
                break;
            //case QteContinuousSignal _:
            //    DirectorHelper.ReleaseSignal<QteContinuousSignal>(signal);
            //    break;
            //case QteLongPressSignal _:
            //    DirectorHelper.ReleaseSignal<QteLongPressSignal>(signal);
            //    break;
            case UISignal _:
                DirectorHelper.ReleaseSignal<UISignal>(signal);
                break;
            case SpeedLineSignal _:
                DirectorHelper.ReleaseSignal<SpeedLineSignal>(signal);
                break;
            case CustomSignal _:
                DirectorHelper.ReleaseSignal<CustomSignal>(signal);
                break;
            case ActionSignal _:
                DirectorHelper.ReleaseSignal<ActionSignal>(signal);
                break;
            case FModSignal _:
                DirectorHelper.ReleaseSignal<FModSignal>(signal);
                break;
            case AutofadeSignal _:
                DirectorHelper.ReleaseSignal<AutofadeSignal>(signal);
                break;
            case Mp4Signal _:
                DirectorHelper.ReleaseSignal<Mp4Signal>(signal);
                break;
            case HideObjectSignal _:
                DirectorHelper.ReleaseSignal<HideObjectSignal>(signal);
                break;
            case SceneSignal _:
                DirectorHelper.ReleaseSignal<SceneSignal>(signal);
                break;
            case TimelineBgmSignal _:
                DirectorHelper.ReleaseSignal<TimelineBgmSignal>(signal);
                break;
            //case QteClickSignal _:
            //    DirectorHelper.ReleaseSignal<QteClickSignal>(signal);
            //    break;
            case CameraTransitionSignal _:
                DirectorHelper.ReleaseSignal<CameraTransitionSignal>(signal);
                break;
            case SwitchLensflareStateSignal _:
                DirectorHelper.ReleaseSignal<SwitchLensflareStateSignal>(signal);
                break;
            case SmartShadowSignal _:
                DirectorHelper.ReleaseSignal<SmartShadowSignal>(signal);
                break;
            default:
                DebugLog.AddErrorLog2("unknow signal type:{0}", signal.GetType().Name);
                break;
        }
    }

    public static void OnSignal(DirectorSignalEmmiter signal)
    {
        //engine emmiter
        if (signal.signalType == DirectorSignalEmmiter.SignalType_Active)
        {
            Transform tf = signal.parentTrack != null ? signal.parentTrack.BindTransform : null;
            if (tf)
            {
                tf.gameObject.SetActive(signal.HasFlag(DirectorSignalEmmiter.Flag_Active));
            }
        }
        else if (signal.signalType == DirectorSignalEmmiter.SignalType_Jump)
        {
            director.time = signal.jumpTime;
        }
        else if (signal.signalType == DirectorSignalEmmiter.SignalType_Slow)
        {
            DirectorAsset.instance.SetSpeed(signal.slowRate);
        }
        else if (signal is QteClickSignal)
        {
            QteClickSignal s = signal as QteClickSignal;
            if (!s.m_enable) return;
            if (s.m_qteType == QTEType.Click)
            {
                if (s.m_isLoop) //循环的单击qte
                {
                    TimelineQteClickLoopUI.singleton.Show(signal as QteClickSignal);
                }
                else
                {
                    TimelineQteClickUI.singleton.Show(signal as QteClickSignal);
                }
            }
            else if (s.m_qteType == QTEType.ContinueClick)
            {
                TimelineQteContinueClickUI.singleton.Show(signal as QteClickSignal);
            }
        }
        else if (signal is SpeedLineSignal)
        {
            TimelineSpeedLineUI.singleton.Show(signal as SpeedLineSignal);
        }
        else if (signal is CustomSignal)
        {
            CustomSignal cs = signal as CustomSignal;
            CustomSignalProcess.singleton.OnProcess(cs.type);
        }
        else if (signal is UISignal)
        {
            UISignal uiSignal = signal as UISignal;
            TimelineSignalUI.ShowSign(uiSignal);
        }
        else if (signal is LayerSignal)
        {

            LayerSignal layer = (signal as LayerSignal);

            GameObjectLayerHelper.HideLayerWithMask(layer.layerMask, false);

            if (layer.BForceEnableRender)
                GameObjectLayerHelper.HideLayerWithSkinMesh_RecoverByLayer(layer.layerMask);

            GameObjectLayerHelper.Push(true, layer.BIgnorelayerStateStack);
        }

        else if (signal is ActionSignal actionSignal)
        {
            ActionSignal sig = actionSignal;
            if (Ixtl != null && Application.isPlaying) Ixtl.ReactPlay(sig.StatisticID, sig.Anim);
        }
        else if (signal is FModSignal fs)
        {
            string name = fs.clip;
            var _fmod_dic = RTimeline.singleton.Fmod_dic;
            if (_fmod_dic.ContainsKey(name))
            {
                _fmod_dic[name].SetParam(AudioChannel.Action, fs.key, fs.param);
            }
        }
        else if (signal is AutofadeSignal autofadeSignal)
        {
            AutofadeSignal sig = autofadeSignal;
            Ixtl?.FadeOut2In(sig.Out, sig.In, sig.Free);
        }
        else if (signal is Mp4Signal mp4Signal)
        {
            Mp4Signal sig = mp4Signal;
            TimelineMovieUI.singleton.Show(sig);
        }
        else if (signal is HideObjectSignal hideObjectSignal)
        {
            HideObjectSignal sig = hideObjectSignal;
            sig.PlayAnimation();
        }
        else if (signal is SceneSignal sceneSignal)
        {
            SceneSignal sig = sceneSignal;
            sig.LoadOrUnLoadScene();
        }
        else if (signal is TimelineBgmSignal timelineBgmSignal)
        {
            TimelineBgmSignal sig = timelineBgmSignal;
            RTimeline.singleton.ControlBgm(sig);
            if (sig.m_isLastSignal)
            {
                RTimeline.singleton.m_lastBgmSignalExecuted = true;
            }
        }
        else if (signal is JumpTimelineSignal jumpTimelineSignal)
        {
            JumpTimelineSignal sig = jumpTimelineSignal;
            RTimeline.Ixtl.JumpTimeline(sig.m_timelineName);
        }
        else if (signal is MuteTrackSignal muteTrackSignal)
        {
            if (RTimeline.singleton.Director.state == PlayState.Playing)
            {
                TrackAsset track = null;
                foreach (var pb in RTimeline.singleton.Director.playableAsset.outputs)
                {
                    if (pb.sourceObject is TrackAsset obj)
                    {
                        if (obj.name == muteTrackSignal.trackName)
                        {
                            track = obj;
                            break;
                        }
                    }
                }
                RTimeline.singleton.SetMute(track, muteTrackSignal.mute);
            }
        }
        else if (signal is CameraTransitionSignal)
        {
            RTimeline.singleton.GrabScreen();
        }
        else if (signal is SwitchLensflareStateSignal)
        {
            MFLensFlare.TimelineAvoid = !MFLensFlare.TimelineAvoid;
        }
        else if (signal is SmartShadowSignal smartShadowSignal)
        {
            SmartShadowSignal sig = smartShadowSignal;
            if(SmartShadow.Instance != null)
            {
                SmartShadow.Instance.SetEnable(sig.m_enable);
            }
        }
    }
}
