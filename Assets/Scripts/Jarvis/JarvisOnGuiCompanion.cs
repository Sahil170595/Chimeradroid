using System;
using System.Collections;
using Chimeradroid.Jarvis;
using Chimeradroid.Jarvis.Audio;
using UnityEngine;
using UnityEngine.Events;

namespace Chimeradroid
{
    public sealed partial class JarvisOnGuiCompanion : MonoBehaviour
    {
        private const string PrefKeyBaseUrl = "jarvis.base_url";
        private const string PrefKeyDeviceKey = "jarvis.device_key";
        private const string PrefKeySessionId = "jarvis.session_id";

        [Header("Connection")]
        public string BaseUrl = "http://localhost:8400";

#if UNITY_EDITOR
        [Tooltip("Editor-only. Used once to register a device key, then discarded at build time.")]
        public string MasterKey = "jarvis-demo-key";

        [Header("Device")]
        public string DeviceName = "chimeradroid";
#endif

        [Header("Chat")]
        [TextArea(2, 6)]
        public string Message = "Hello, Jarvis.";

        public bool SpeakResponses = false;
        public bool UseAndroidAsrIfPresent = true;

        private string _deviceKey;
        private string _sessionId;
        private string _lastTurnId;
        private string _status = "idle";
        private string _lastResponse = "";
        private Vector2 _scroll;

        private object _tts;
        private object _asr;
        private bool _asrSearchDone;
        private bool _ttsSearchDone;
        private JarvisStreamClient _stream;
        private string _streamStatus = "disconnected";
        private AudioSource _audioSource;

        private string _handoffToken = "";
        private string _timeline = "";

        private string _toolsStatus = "";
        private bool _chatStreamOnSend = true;

        private bool _prefsDirty;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void EnsureLoaded()
        {
            if (FindFirstObjectByType<JarvisOnGuiCompanion>() != null)
            {
                return;
            }

            var go = new GameObject("JarvisOnGuiCompanion");
            DontDestroyOnLoad(go);
            go.AddComponent<JarvisOnGuiCompanion>();
        }

        private void Awake()
        {
            BaseUrl = PlayerPrefs.GetString(PrefKeyBaseUrl, BaseUrl);
            _deviceKey = PlayerPrefs.GetString(PrefKeyDeviceKey, "");
            _sessionId = PlayerPrefs.GetString(PrefKeySessionId, "");
            _stream = new JarvisStreamClient();
            _stream.OnRawMessage += OnStreamMessage;
            _audioSource = GetComponent<AudioSource>();
            if (_audioSource == null)
            {
                _audioSource = gameObject.AddComponent<AudioSource>();
            }
            _audioSource.playOnAwake = false;
        }

        private void Update()
        {
            _stream?.Pump();
            StreamTick();

            if (UseAndroidAsrIfPresent && _asr == null && !_asrSearchDone)
            {
                _asr = FindEmbardimentComponent("Google.XR.Embardiment.AndroidAsr");
                _asrSearchDone = true;
                if (_asr != null)
                {
                    var onComplete = _asr.GetType().GetField("OnComplete")?.GetValue(_asr);
                    if (onComplete != null)
                    {
                        var addListener = onComplete.GetType().GetMethod("AddListener");
                        if (addListener != null)
                        {
                            UnityAction<string> handler = OnAsrComplete;
                            addListener.Invoke(onComplete, new object[] { handler });
                        }
                    }
                }
            }

            if (_tts == null && !_ttsSearchDone)
            {
                _tts = FindEmbardimentComponent("Google.XR.Embardiment.AndroidTts")
                       ?? FindEmbardimentComponent("Google.XR.Embardiment.GeminiTts");
                _ttsSearchDone = true;
            }

            FlushPrefs();
        }

        private void OnDestroy()
        {
            FlushPrefsImmediate();

            try
            {
                _streamCts?.Cancel();
                _streamCts?.Dispose();
            }
            catch
            {
                // ignore
            }

            _stream?.Dispose();
        }

        private void MarkPrefsDirty()
        {
            _prefsDirty = true;
        }

        private void FlushPrefs()
        {
            if (!_prefsDirty) return;
            _prefsDirty = false;
            PlayerPrefs.Save();
        }

        private void FlushPrefsImmediate()
        {
            if (!_prefsDirty) return;
            _prefsDirty = false;
            PlayerPrefs.Save();
        }

        private void SetPref(string key, string value)
        {
            PlayerPrefs.SetString(key, value);
            MarkPrefsDirty();
        }

        private void DeletePref(string key)
        {
            PlayerPrefs.DeleteKey(key);
            MarkPrefsDirty();
        }

        private static object FindEmbardimentComponent(string fullTypeName)
        {
            var t = ResolveType(fullTypeName);
            return t == null ? null : FindAnyObjectByType(t, FindObjectsInactive.Include);
        }

        private static Type ResolveType(string fullTypeName)
        {
            var direct = Type.GetType(fullTypeName);
            if (direct != null)
            {
                return direct;
            }

            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                Type t;
                try
                {
                    t = asm.GetType(fullTypeName, throwOnError: false);
                }
                catch
                {
                    t = null;
                }

                if (t != null)
                {
                    return t;
                }
            }

            return null;
        }

        private void StartAsr()
        {
            if (_asr == null)
            {
                _status = "ASR not found in scene";
                return;
            }

            var method = _asr.GetType().GetMethod("OpenRecognitionStream", Type.EmptyTypes);
            if (method == null)
            {
                _status = "ASR method missing";
                return;
            }

            method.Invoke(_asr, null);
            _status = "listening...";
        }

        private void OnAsrComplete(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return;
            }

            Message = text;
            StartCoroutine(SendChat(text));
        }

        private IEnumerator RegisterDevice()
        {
#if !UNITY_EDITOR
            _status = "register only available in Editor";
            yield break;
#else
            var baseUrl = JarvisWeb.NormalizeBaseUrl(BaseUrl);
            if (string.IsNullOrEmpty(baseUrl))
            {
                _status = "missing base url";
                yield break;
            }

            _status = "registering device...";
            var url = $"{baseUrl}/jarvis/v2/devices/register";
            var payload = new DeviceRegisterRequest { Name = DeviceName };

            DeviceRegisterResponse resp = null;
            string err = null;
            yield return JarvisWeb.PostJson<DeviceRegisterRequest, DeviceRegisterResponse>(
                url,
                payload,
                JarvisWeb.MasterKeyHeader,
                MasterKey,
                r => resp = r,
                e => err = e
            );

            if (!string.IsNullOrEmpty(err) || resp == null || string.IsNullOrEmpty(resp.DeviceKey))
            {
                _status = "register failed";
                _lastResponse = err ?? "unknown error";
                yield break;
            }

            _deviceKey = resp.DeviceKey;
            SetPref(PrefKeyDeviceKey, _deviceKey);
            _status = $"registered device: {resp.DeviceId}";
            _lastResponse = $"device_id={resp.DeviceId}\nname={resp.Name}\ncreated_at={resp.CreatedAt}";
#endif
        }

        private IEnumerator SendChat(string text)
        {
            var baseUrl = JarvisWeb.NormalizeBaseUrl(BaseUrl);
            if (string.IsNullOrEmpty(baseUrl))
            {
                _status = "missing base url";
                yield break;
            }

            if (string.IsNullOrEmpty(_deviceKey))
            {
                _status = "missing device key (register first)";
                yield break;
            }

            _status = "sending chat...";
            _lastResponse = "";
            var url = $"{baseUrl}/jarvis/v2/chat";
            var payload = new ChatRequest
            {
                IdempotencyKey = Guid.NewGuid().ToString("N"),
                Message = text,
                SessionId = string.IsNullOrWhiteSpace(_sessionId) ? null : _sessionId,
                Mode = _chatStreamOnSend ? "async" : "sync",
                WaitMs = _chatStreamOnSend ? 2000 : 10000,
            };

            ChatResponse resp = null;
            string err = null;
            yield return JarvisWeb.PostJson<ChatRequest, ChatResponse>(
                url,
                payload,
                JarvisWeb.DeviceKeyHeader,
                _deviceKey,
                r => resp = r,
                e => err = e
            );

            if (!string.IsNullOrEmpty(err) || resp == null)
            {
                _status = "chat failed";
                _lastResponse = err ?? "unknown error";
                yield break;
            }

            _sessionId = resp.SessionId;
            _lastTurnId = resp.TurnId;
            SetPref(PrefKeySessionId, _sessionId);

            if (_chatStreamOnSend && !string.IsNullOrEmpty(resp.StreamUrl))
            {
                _status = resp.TurnStatus ?? "running";
                StartStreamWithUrl(resp.StreamUrl);
                yield break;
            }

            if (!string.IsNullOrEmpty(resp.FinalResponse))
            {
                _status = resp.TurnStatus;
                _lastResponse = resp.FinalResponse;
                MaybeSpeak(resp.FinalResponse);
                yield break;
            }

            if (resp.StillRunning == true)
            {
                yield return PollTurnUntilComplete(baseUrl, resp.TurnId, timeoutSeconds: 30f);
            }
            else
            {
                _status = resp.TurnStatus;
                _lastResponse = resp.FinalResponse ?? "";
                MaybeSpeak(_lastResponse);
            }
        }

        private IEnumerator PollTurnUntilComplete(string baseUrl, string turnId, float timeoutSeconds)
        {
            var started = Time.realtimeSinceStartup;
            while (Time.realtimeSinceStartup - started < timeoutSeconds)
            {
                _status = "polling...";
                var url = $"{baseUrl}/jarvis/v2/turns/{turnId}";
                TurnResponse resp = null;
                string err = null;
                yield return JarvisWeb.GetJson<TurnResponse>(
                    url,
                    JarvisWeb.DeviceKeyHeader,
                    _deviceKey,
                    r => resp = r,
                    e => err = e
                );

                if (!string.IsNullOrEmpty(err))
                {
                    _status = "turn poll failed";
                    _lastResponse = err;
                    yield break;
                }

                if (resp == null)
                {
                    yield return new WaitForSecondsRealtime(0.5f);
                    continue;
                }

                _status = resp.Status ?? "unknown";
                if (!string.IsNullOrEmpty(resp.FinalResponse))
                {
                    _lastResponse = resp.FinalResponse;
                    MaybeSpeak(resp.FinalResponse);
                    yield break;
                }

                if (resp.Status == "FAILED" || resp.Status == "CANCELLED")
                {
                    _lastResponse = resp.FinalResponse ?? resp.Status;
                    yield break;
                }

                yield return new WaitForSecondsRealtime(0.5f);
            }

            _status = "poll timeout";
        }

        private void MaybeSpeak(string text)
        {
            if (!SpeakResponses || string.IsNullOrWhiteSpace(text) || _tts == null)
            {
                return;
            }

            var speak = _tts.GetType().GetMethod("Speak", new[] { typeof(string) });
            speak?.Invoke(_tts, new object[] { text });
        }

        private void PlayWavBase64(string audioB64)
        {
            if (string.IsNullOrWhiteSpace(audioB64) || _audioSource == null)
            {
                return;
            }

            try
            {
                var bytes = Convert.FromBase64String(audioB64);
                var clip = WavReader.ToAudioClip(bytes, "jarvis-voice");
                _audioSource.Stop();
                _audioSource.clip = clip;
                _audioSource.Play();
            }
            catch (Exception e)
            {
                _status = $"audio decode failed: {e.Message}";
            }
        }
    }
}
