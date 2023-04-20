using System;
using System.Collections.Generic;
using GMSDK;
using UNBridgeLib.LitJson;

namespace GSDK.IMVoice
{
    internal static class VoiceInnerTools
    {
        private static readonly Dictionary<PlayMode, int> _playModeDictionary = new Dictionary<PlayMode, int>()
        {
            {PlayMode.Default, 0},
            {PlayMode.Speaker, 1}
        };

        public static int Convert(PlayMode mode)
        {
            return _playModeDictionary[mode];
        }

        public static FinishRecordInfo Convert(FinishRecordResult result)
        {
            return new FinishRecordInfo()
            {
                VoiceID = result.voiceId,
                VoiceDuration = result.voiceDuration
            };
        }

        public static FinishRecordNotUploadInfo Convert(FinishRecordNotUploadResult result)
        {
            return new FinishRecordNotUploadInfo()
            {
                VoiceDuration = result.voiceDuration,
                UniqueId = result.uniqueId
            };
        }

        public static PlayInfo Convert(StartPlayResult result)
        {
            return new PlayInfo()
            {
                VoiceID = result.voiceId
            };
        }

        public static PlayInfo Convert(FinishPlayResult result)
        {
            return new PlayInfo()
            {
                VoiceID = result.voiceId
            };
        }

        public static PlayInfo Convert(UploadVoiceResult result)
        {
            return new PlayInfo()
            {
                VoiceID = result.voiceId
            };
        }

        public static TranscribeInfo Convert(VoiceTranslateResult result)
        {
            return new TranscribeInfo()
            {
                VoiceID = result.voiceId,
                VoiceContent = result.voiceContent
            };
        }

        public static TranscribeLocalInfo Convert(LocalVoiceTranslateResult result)
        {
            return new TranscribeLocalInfo()
            {
                VoiceContent = result.voiceContent
            };
        }

        public static VoiceFileInfo Convert(FetchLocalPathResult result)
        {
            return new VoiceFileInfo()
            {
                VoiceID = result.voiceId,
                VoiceLocalFilePath = result.localPath
            };
        }

        public static Result ConvertVoiceIMRecordError(CallbackResult result)
        {
            if (result.code == 0)
            {
                return new Result( 0, result.message);
            }

            int code = result.code;
            string msg = result.message;
            int extraCode = result.extraErrorCode;
            string extraMsg = result.extraErrorMessage;
            string additionalInfo = result.additionalInfo;
            return new Result(code, msg,extraCode,extraMsg,additionalInfo);
        }

        public static Result ConvertVoiceIMPlayError(CallbackResult result)
        {
            if (result.code == 0)
            {
                return new Result(0, result.message);
            }

            int code = result.code;
            string msg = result.message;
            int extraCode = result.extraErrorCode;
            string extraMsg = result.extraErrorMessage;
            string additionalInfo = result.additionalInfo;
            return new Result(code, msg,extraCode,extraMsg,additionalInfo);
        }
        
        public static Result ConvertVoiceIMTranscribeError(CallbackResult result)
        {
            if (result.code == 0)
            {
                return new Result( 0, result.message);
            }
            int code = result.code;
            string msg = result.message;
            int extraCode = result.extraErrorCode;
            string extraMsg = result.extraErrorMessage;
            string additionalInfo = result.additionalInfo;
            return new Result(code, msg, extraCode, extraMsg,additionalInfo);
        }
    }
}

namespace GSDK.ASRVoice
{
    internal static class VoiceInnerTools
    {
        private static readonly Dictionary<PlayMode, int> _playModeDictionary = new Dictionary<PlayMode, int>()
        {
            {PlayMode.Default, 0},
            {PlayMode.Speaker, 1}
        };

        private static readonly Dictionary<TargetLanguage, int> _languageDictionary = new Dictionary<TargetLanguage, int>()
        {
            {TargetLanguage.Chinese, 0},
            {TargetLanguage.English, 1}
        };

        public static int Convert(PlayMode playMode)
        {
            return _playModeDictionary[playMode];
        }

        public static int Convert(TargetLanguage targetLanguage)
        {
            return _languageDictionary[targetLanguage];
        }

        public static PlayInfo Convert(StartPlayResult startPlayResult)
        {
            return new PlayInfo()
            {
                VoiceID = startPlayResult.voiceId
            };
        }
        
        public static PlayInfo Convert(FinishPlayResult finishPlayResult)
        {
            return new PlayInfo()
            {
                VoiceID = finishPlayResult.voiceId
            };
        }

        public static TranscribingInfo ConvertToPartialTranscribeInfo(ASRTranslateResult asrTranscribeResult)
        {
            return new TranscribingInfo()
            {
                TranscribingText = asrTranscribeResult.partialResult
            };
        }

        public static FinishTranscribeInfo ConvertToFinishTranscribeInfo(ASRTranslateResult asrTranscribeResult)
        {
            return new FinishTranscribeInfo()
            {
                FinishTranscribeText = asrTranscribeResult.translateResult
            };
        }

        public static FinishTranscribeAndUploadInfo ConvertToFinishTranscribeAndUploadInfo(
            ASRTranslateResult asrTranscribeResult)
        {
            return new FinishTranscribeAndUploadInfo()
            {
                FinishTranscribeText = asrTranscribeResult.translateResult,
                VoiceID = asrTranscribeResult.voiceId,
                VoiceTime = asrTranscribeResult.voiceTime
            };
        }
        
        public static Result ConvertVoiceASRError(CallbackResult result)
        {
            if (result.code == 0)
            {
                return new Result(0, result.message);
            }
            
            int code = result.code;
            string msg = result.message;
            int extraCode = result.extraErrorCode;
            string extraMsg = result.extraErrorMessage;
            string additionalInfo = result.additionalInfo;
            return new Result(code, msg, extraCode, extraMsg, additionalInfo);
        }
    }
}