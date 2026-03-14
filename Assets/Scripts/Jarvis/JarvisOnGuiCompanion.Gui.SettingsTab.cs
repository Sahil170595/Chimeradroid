using UnityEngine;

namespace Chimeradroid
{
    public sealed partial class JarvisOnGuiCompanion
    {
        private void RenderSettingsTab()
        {
            GUILayout.Label("Base URL (phone cannot use localhost):");
            BaseUrl = GUILayout.TextField(BaseUrl);
            if (GUILayout.Button("Save Base URL"))
            {
                BaseUrl = Jarvis.JarvisWeb.NormalizeBaseUrl(BaseUrl);
                SetPref(PrefKeyBaseUrl, BaseUrl);
            }

            if (Application.isMobilePlatform && BaseUrl != null && BaseUrl.Contains("localhost"))
            {
                GUILayout.Label(
                    "Warning: on device, use your PC LAN IP (e.g. http://192.168.1.10:8400)."
                );
            }

            GUILayout.Space(8);
            _showSecrets = GUILayout.Toggle(_showSecrets, "Show secrets");

            GUILayout.Label("Device Key:");
            _deviceKey = _showSecrets
                ? GUILayout.TextField(_deviceKey)
                : GUILayout.PasswordField(_deviceKey, '*');
            GUILayout.Label($"Device ID: {(_deviceId ?? "")}");
            if (GUILayout.Button("Save Device Key"))
            {
                SetPref(PrefKeyDeviceKey, _deviceKey);
            }
            if (GUILayout.Button("Clear Device Key"))
            {
                _deviceKey = "";
                DeletePref(PrefKeyDeviceKey);
            }

            GUILayout.Space(8);
            GUILayout.Label("Session ID:");
            _sessionId = GUILayout.TextField(_sessionId);
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Save Session"))
            {
                SetPref(PrefKeySessionId, _sessionId);
            }
            if (GUILayout.Button("New Session"))
            {
                _sessionId = "";
                SetPref(PrefKeySessionId, _sessionId);
            }
            GUILayout.EndHorizontal();

            if (GUILayout.Button("Wipe Local Settings"))
            {
                _deviceKey = "";
                _deviceId = "";
                _sessionId = "";
                BaseUrl = "http://localhost:8400";
                DeletePref(PrefKeyBaseUrl);
                DeletePref(PrefKeyDeviceKey);
                DeletePref(PrefKeyDeviceId);
                DeletePref(PrefKeySessionId);
                Jarvis.JarvisCompanionStateStore.Delete();
                _companionState = new Jarvis.CompanionLocalState();
            }

            GUILayout.Space(8);
            SpeakResponses = GUILayout.Toggle(
                SpeakResponses,
                "Speak responses (if Embardiment TTS present)"
            );
            UseAndroidAsrIfPresent = GUILayout.Toggle(
                UseAndroidAsrIfPresent,
                "Use Embardiment Android ASR (if present)"
            );

            if (GUILayout.Button("Start ASR (push-to-talk)"))
            {
                StartAsr();
            }

            GUILayout.Space(10);
#if UNITY_EDITOR
            GUILayout.Label("One-time device registration (Editor only):");
            MasterKey = _showSecrets
                ? GUILayout.TextField(MasterKey)
                : GUILayout.PasswordField(MasterKey, '*');
            GUILayout.BeginHorizontal();
            GUILayout.Label("Name", GUILayout.Width(60));
            DeviceName = GUILayout.TextField(DeviceName);
            GUILayout.EndHorizontal();
            if (GUILayout.Button("Register Device"))
            {
                StartCoroutine(RegisterDevice());
            }
#else
            GUILayout.Label("Register on desktop; paste device key above.");
#endif

            GUILayout.Space(10);
            GUILayout.Label("Backend:");
            RenderSystemStatusPanel(height: 220);
        }
    }
}
