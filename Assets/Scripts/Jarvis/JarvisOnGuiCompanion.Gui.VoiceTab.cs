using UnityEngine;

namespace Chimeradroid
{
    public sealed partial class JarvisOnGuiCompanion
    {
        private Vector2 _voiceScroll;

        private void RenderVoiceTab()
        {
            GUILayout.Label("Voice (server-side whisper+piper):");
            GUILayout.BeginHorizontal();
            GUILayout.Label("mode", GUILayout.Width(50));
            _voiceMode = GUILayout.TextField(_voiceMode);
            GUILayout.EndHorizontal();

            GUILayout.Label("text:");
            _voiceText = GUILayout.TextArea(_voiceText, GUILayout.Height(80));
            if (GUILayout.Button("Converse"))
            {
                StartCoroutine(VoiceConverse());
            }

            if (!string.IsNullOrEmpty(_voiceStatus))
            {
                GUILayout.Label(_voiceStatus);
            }

            _voiceScroll = GUILayout.BeginScrollView(_voiceScroll, GUILayout.Height(420));
            GUILayout.TextArea(string.IsNullOrEmpty(_voiceLast) ? "(no response)" : _voiceLast);
            GUILayout.EndScrollView();
        }
    }
}

