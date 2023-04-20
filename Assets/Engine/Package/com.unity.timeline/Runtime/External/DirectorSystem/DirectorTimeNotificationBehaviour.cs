using UnityEngine.Playables;

namespace UnityEngine.Timeline
{
    public class NotifyContext
    {
        public double time;
    }

    public class DirectorTimeNotificationBehaviour : PlayableBehaviour
    {
        struct NotificationEntry
        {
            public double time;
            public INotification payload;
            public bool notificationFired;
            public NotificationFlags flags;

            public bool triggerInEditor
            {
                get { return (flags & NotificationFlags.TriggerInEditMode) != 0; }
            }
            public bool prewarm
            {
                get { return (flags & NotificationFlags.Retroactive) != 0; }
            }
            public bool triggerOnce
            {
                get { return (flags & NotificationFlags.TriggerOnce) != 0; }
            }
        }

        NotificationEntry[] notifications = new NotificationEntry[32];
        int notifiCount = 0;
        double m_PreviousTime;
        // bool m_NeedSortNotifications;
        static NotifyContext context = new NotifyContext ();

        public static void CreateNotificationsPlayable (
            ref ScriptPlayable<DirectorTimeNotificationBehaviour> notificationPlayable,
            PlayableGraph graph, short signalStart, short signalEnd)
        {

            if (DirectorHelper.singleton.signalCount > 0)
            {
                var director = DirectorHelper.GetDirector ();
                if (notificationPlayable.IsNull ())
                    notificationPlayable = ScriptPlayable<DirectorTimeNotificationBehaviour>.Create (graph);
                notificationPlayable.SetDuration (DirectorAsset.instance.duration);
                notificationPlayable.SetTimeWrapMode (director.extrapolationMode);
                notificationPlayable.SetPropagateSetTime (true);
                var behaviour = notificationPlayable.GetBehaviour ();
                behaviour.notifiCount = 0;
                var signals = DirectorHelper.singleton.signals;
                for (int i = 0; i < DirectorHelper.singleton.signalCount; ++i)
                {
                    var signal = signals[i];
                    if (signal != null)
                    {
                        var time = (DiscreteTime) signal.T;
                        var tlDuration = (DiscreteTime) DirectorAsset.instance.duration;
                        if (time >= tlDuration && time <= tlDuration.OneTickAfter () && tlDuration != 0)
                        {
                            time = tlDuration.OneTickBefore ();
                        }
                        var notificationOptionProvider = signal as INotificationOptionProvider;
                        if (notificationOptionProvider != null)
                        {
                            behaviour.AddNotification ((double) time, signal, notificationOptionProvider.flags);
                        }
                        else
                        {
                            behaviour.AddNotification ((double) time, signal);
                        }
                    }
                }
            }

        }

        public void Reset ()
        {
            notifiCount = 0;
        }

        public void AddNotification (double time, INotification payload,
            NotificationFlags flags = NotificationFlags.Retroactive)
        {
            notifications[notifiCount++] = new NotificationEntry
            {
            time = time,
            payload = payload,
            flags = flags
            };
            // m_NeedSortNotifications = true;
        }

        public override void OnGraphStart (Playable playable)
        {
            //SortNotifications ();
            for (int i = 0; i < notifiCount; i++)
            {
                ref var notification = ref notifications[i];
                notification.notificationFired = false;
            }
            m_PreviousTime = playable.GetTime ();
        }

        public override void OnBehaviourPause (Playable playable, FrameData info)
        {
            if (playable.IsDone ())
            {
                //SortNotifications ();
                for (int i = 0; i < notifiCount; i++)
                {
                    ref var notifi = ref notifications[i];
                    if (!notifi.notificationFired)
                    {
                        var duration = playable.GetDuration ();
                        var canTrigger = m_PreviousTime <= notifi.time && notifi.time <= duration;
                        if (canTrigger)
                        {
                            Trigger_internal (info.output, ref notifi);
                        }
                    }
                }
            }
        }

        public override void PrepareFrame (Playable playable, FrameData info)
        {
            // Never trigger on scrub
            if (info.evaluationType == FrameData.EvaluationType.Evaluate)
            {
                return;
            }
            double duration = playable.GetDuration ();
            if (DirectorAsset.instance.directorPlayable.IsValid ())
            {
                duration = DirectorAsset.instance.directorPlayable.GetDuration ();
            }
            // SyncDurationWithExternalSource (playable);
            // SortNotifications ();
            var currentTime = playable.GetTime ();
            context.time = currentTime;
            if (info.timeLooped)
            {
                TriggerNotificationsInRange (m_PreviousTime, duration, info, playable, true);
                double dx = duration - m_PreviousTime;
                int nFullTimelines = (int) ((info.deltaTime * info.effectiveSpeed - dx) / duration);
                for (int i = 0; i < nFullTimelines; i++)
                {
                    TriggerNotificationsInRange (0, duration, info, playable, false);
                }
                TriggerNotificationsInRange (0, currentTime, info, playable, false);
            }
            else
            {
                double pt = playable.GetTime ();
                TriggerNotificationsInRange (m_PreviousTime, pt, info,
                    playable, true);
            }

            for (int i = 0; i < notifiCount; i++)
            {
                ref var notifi = ref notifications[i];
                if (notifi.notificationFired && CanRestoreNotification (ref notifi, ref info, currentTime, m_PreviousTime))
                {
                    Restore_internal (ref notifi);
                }
            }

            m_PreviousTime = playable.GetTime ();
        }

        void SortNotifications ()
        {
            // if (m_NeedSortNotifications)
            // {
            //     m_Notifications.Sort ((x, y) => x.time.CompareTo (y.time));
            //     m_NeedSortNotifications = false;
            // }
        }

        static bool CanRestoreNotification (ref NotificationEntry e, ref FrameData info, double currentTime, double previousTime)
        {
            if (e.triggerOnce)
                return false;
            if (info.timeLooped)
                return true;

            //case 1111595: restore the notification if the time is manually set before it
            return previousTime > currentTime && currentTime <= e.time;
        }

        void TriggerNotificationsInRange (double start, double end, FrameData info, Playable playable, bool checkState)
        {
            if (start <= end)
            {
                var playMode = Application.isPlaying;
                for (int i = 0; i < notifiCount; i++)
                {
                    ref var notifi = ref notifications[i];
                    if (notifi.notificationFired && (checkState || notifi.triggerOnce))
                        continue;

                    var notificationTime = notifi.time;
                    if (notifi.prewarm && notificationTime < end && (notifi.triggerInEditor || playMode))
                    {
                        Trigger_internal (info.output, ref notifi);
                    }
                    else
                    {
                        if (notificationTime < start || notificationTime > end)
                            continue;

                        if (notifi.triggerInEditor || playMode)
                        {
                            Trigger_internal (info.output, ref notifi);
                        }
                    }
                }
            }
        }

        void SyncDurationWithExternalSource (Playable playable)
        {
            // if (m_TimeSource.IsValid ())
            // {
            //     playable.SetDuration (m_TimeSource.GetDuration ());
            //     playable.SetTimeWrapMode (m_TimeSource.GetTimeWrapMode ());
            // }
        }

        static void Trigger_internal (PlayableOutput output, ref NotificationEntry e)
        {
            var receiver = DirectorHelper.singleton.globalReceiver;
            if (receiver != null)
            {
                e.notificationFired = true;
                receiver.OnNotify (Playable.Null, e.payload, context);
            }

        }

        static void Restore_internal (ref NotificationEntry e)
        {
            e.notificationFired = false;
        }
    }
}