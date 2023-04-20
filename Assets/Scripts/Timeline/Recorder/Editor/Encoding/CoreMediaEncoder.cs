using System;
using System.Collections.Generic;
using Unity.Collections;
using UnityEditor.Media;
using UnityEditor.Recorder;
using UnityEngine;

namespace Unity.Media
{
    internal class CoreMediaEncoderRegister : MediaEncoderRegister
    {
        internal override bool PerformsVerticalFlip => true;

        internal override unsafe MediaEncoderHandle Register(MediaEncoderManager mgr)
        {
            return mgr.Instantiate();
        }

        internal override string GetName()
        {
            return "CoreMediaEncoder";
        }

        internal override bool SupportsResolution(MovieRecorderSettings settings, int width, int height, out string errorMessage)
        {
            if (!base.SupportsResolution(settings, width, height, out errorMessage))
            {
                return false;
            }
            errorMessage = "";

            if (height % 2 != 0 || width % 2 != 0)
            {
                errorMessage = "The MP4 format does not support odd values in resolution";
                return false;
            }

            if (width > 4096 || height > 4096)
            {
                Debug.LogWarning(string.Format("MP4 format might not support resolutions bigger than 4096. Current resolution: {0} x {1}.", width, height));
            }

            return true;
        }

        internal override bool SupportsTransparency(MovieRecorderSettings settings, out string errorMessage)
        {
            base.SupportsTransparency(settings, out errorMessage);
            return false;
        }

        internal override string GetDefaultExtension() { return "mp4"; }
    }

    internal class CoreMediaEncoder : IDisposable
    {
        RefHandle<MediaEncoder> Encoder = new RefHandle<MediaEncoder>();

        public void Construct(string path, List<IMediaEncoderAttribute> attributes)
        {
            VideoTrackAttributes vAttr = new VideoTrackAttributes();
            List<AudioTrackAttributes> aAttrs = new List<AudioTrackAttributes>();

            int nVideoTracks = 0;
            foreach (var a in attributes)
            {
                Type t = a.GetType();
                if (t == typeof(VideoTrackMediaEncoderAttribute))
                {
                    nVideoTracks++;
                    var vmAttr = (VideoTrackMediaEncoderAttribute)a;
                    vAttr = vmAttr.Value;
                }
                else if (t == typeof(AudioTrackMediaEncoderAttribute))
                {
                    var amAttr = (AudioTrackMediaEncoderAttribute)a;
                    aAttrs.Add(amAttr.Value);
                }
            }

            Debug.Assert(nVideoTracks > 0, "No video track");
            if (aAttrs.Count == 0)
            {
                Construct(path, vAttr);
            }
            else
            {
                Construct(path, vAttr, aAttrs.ToArray()[0]);
            }
        }

        public void Construct(string path, VideoTrackAttributes vAttr)
        {
            Construct(path, vAttr, new AudioTrackAttributes[0]);
        }

        public void Construct(string path, VideoTrackAttributes vAttr, AudioTrackAttributes aAttr)
        {
            Construct(path, vAttr, new[] { aAttr });
        }

        void Construct(string path, VideoTrackAttributes vAttr, AudioTrackAttributes[] aAttr)
        {
            CoreMediaEncoderLog("Construct()");
            if (Encoder.IsCreated)
                throw new InvalidOperationException("CoreMediaEncoder already instantiated");

            Encoder.Target = new MediaEncoder(path, vAttr, aAttr);
        }

        public void Dispose()
        {
            CoreMediaEncoderLog("Dispose()");
            if (Encoder.IsCreated)
            {
                Encoder.Target.Dispose();
                Encoder.Dispose();
            }
        }

        public bool AddFrame(int width, int height, int rowBytes, TextureFormat format, NativeArray<byte> data)
        {
            CoreMediaEncoderLog("AddFrame(w,h,r,f,d)");
            if (Encoder.IsCreated)
                return Encoder.Target.AddFrame(width, height, rowBytes, format, data);
            return false;
        }

        public bool AddFrame(Texture2D texture)
        {
            CoreMediaEncoderLog("AddFrame(tex)");
            if (Encoder.IsCreated)
                return Encoder.Target.AddFrame(texture);
            return false;
        }

        public bool AddSamples(NativeArray<float> interleavedSamples)
        {
            CoreMediaEncoderLog("AddSamples(samples)");
            if (Encoder.IsCreated)
                return Encoder.Target.AddSamples(interleavedSamples);
            return false;
        }

        void CoreMediaEncoderLog(string log)
        {
#if COREMEDIAENCODER_TRACE_ENABLED
            Debug.Log("CoreMediaEncoder : " + log);
#endif
        }
    }
}
