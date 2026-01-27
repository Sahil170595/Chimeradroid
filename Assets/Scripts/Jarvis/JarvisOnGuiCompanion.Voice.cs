using System;
using System.Collections;
using Chimeradroid.Jarvis;

namespace Chimeradroid
{
    public sealed partial class JarvisOnGuiCompanion
    {
        private string _voiceText = "hey jarvis say hello in one short sentence";
        private string _voiceStatus = "";
        private string _voiceLast = "";
        private string _voiceMode = "whisper_piper";

        private IEnumerator VoiceConverse()
        {
            var baseUrl = JarvisWeb.NormalizeBaseUrl(BaseUrl);
            if (string.IsNullOrEmpty(baseUrl))
            {
                _voiceStatus = "missing base url";
                yield break;
            }

            if (string.IsNullOrEmpty(_deviceKey))
            {
                _voiceStatus = "missing device key";
                yield break;
            }

            var text = (_voiceText ?? "").Trim();
            if (string.IsNullOrEmpty(text))
            {
                _voiceStatus = "missing text";
                yield break;
            }

            _voiceStatus = "conversing...";
            var url = $"{baseUrl}/jarvis/v2/voice/converse";
            var payload = new VoiceConverseRequest
            {
                SessionId = string.IsNullOrWhiteSpace(_sessionId) ? null : _sessionId,
                Text = text,
                Mode = "sync",
                WaitMs = 20000,
                VoiceMode = _voiceMode,
            };

            VoiceConverseResponse resp = null;
            string err = null;
            yield return JarvisWeb.PostJson<VoiceConverseRequest, VoiceConverseResponse>(
                url,
                payload,
                JarvisWeb.DeviceKeyHeader,
                _deviceKey,
                r => resp = r,
                e => err = e
            );

            if (!string.IsNullOrEmpty(err) || resp == null)
            {
                _voiceStatus = err ?? "voice failed";
                yield break;
            }

            if (!string.IsNullOrWhiteSpace(resp.SessionId))
            {
                _sessionId = resp.SessionId;
                UnityEngine.PlayerPrefs.SetString("jarvis.session_id", _sessionId);
                UnityEngine.PlayerPrefs.Save();
            }

            _lastTurnId = resp.TurnId ?? _lastTurnId;
            _voiceLast = resp.ResponseText ?? "";
            _voiceStatus =
                $"ok={resp.Ok} ignored={resp.Ignored} mode={resp.VoiceModeEffective ?? resp.VoiceModeRequested}";

            if (!string.IsNullOrWhiteSpace(resp.ResponseAudioB64))
            {
                PlayWavBase64(resp.ResponseAudioB64);
            }
        }
    }
}

