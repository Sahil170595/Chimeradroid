using UnityEngine;

namespace Chimeradroid
{
    public sealed partial class JarvisOnGuiCompanion
    {
        private void RenderChatTab()
        {
            GUILayout.Label("Message:");
            Message = GUILayout.TextArea(Message, GUILayout.Height(100));
            _chatStreamOnSend = GUILayout.Toggle(_chatStreamOnSend, "Stream on send (async + WebSocket)");
            if (GUILayout.Button("Send to /jarvis/v2/chat"))
            {
                StartCoroutine(SendChat(Message));
            }

            GUILayout.Space(8);
            _autoReconnect = GUILayout.Toggle(_autoReconnect, "Auto-reconnect stream");
            GUILayout.Label($"Stream: {_streamStatus}");
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Connect Stream"))
            {
                StartStream();
            }
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

