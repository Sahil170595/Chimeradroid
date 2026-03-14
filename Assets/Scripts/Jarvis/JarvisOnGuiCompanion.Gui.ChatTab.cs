using Chimeradroid.Jarvis;
using UnityEngine;

namespace Chimeradroid
{
    public sealed partial class JarvisOnGuiCompanion
    {
        private void RenderChatTab()
        {
            var ready = !string.IsNullOrEmpty(_deviceKey)
                        && !string.IsNullOrEmpty(JarvisWeb.NormalizeBaseUrl(BaseUrl));

            if (!ready)
            {
                GUILayout.Label("Configure Base URL and register a device key in Settings first.");
            }

            GUILayout.Label("Message:");
            Message = GUILayout.TextArea(Message, GUILayout.Height(100));
            _chatStreamOnSend = GUILayout.Toggle(_chatStreamOnSend, "Stream on send (async + WebSocket)");

            GUI.enabled = ready;
            if (GUILayout.Button("Send to /jarvis/v2/chat"))
            {
                StartCoroutine(SendChat(Message));
            }
            GUI.enabled = true;

            GUILayout.Space(8);
            _autoReconnect = GUILayout.Toggle(_autoReconnect, "Auto-reconnect stream");
            GUILayout.Label($"Stream: {_streamStatus}");
            GUILayout.BeginHorizontal();
            GUI.enabled = ready;
            if (GUILayout.Button("Connect Stream"))
            {
                StartStream();
            }
            GUI.enabled = true;
            if (GUILayout.Button("Disconnect Stream"))
            {
                StopStream();
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(8);
            _scroll = GUILayout.BeginScrollView(_scroll, GUILayout.Height(260));
            GUILayout.TextArea(_lastResponse);
            GUILayout.EndScrollView();
        }
    }
}
