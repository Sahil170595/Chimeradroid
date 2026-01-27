using UnityEngine;

namespace Chimeradroid
{
    public sealed partial class JarvisOnGuiCompanion
    {
        private int _tabIndex = 0;
        private bool _showSecrets = false;

        private void OnGUI()
        {
            const int pad = 10;
            const int w = 560;
            const int h = 820;

            var rect = new Rect(pad, pad, w, h);
            GUI.Box(rect, "JARVIS Companion (Chimeradroid)");
            GUILayout.BeginArea(new Rect(pad + 10, pad + 25, w - 20, h - 35));

            _tabIndex = GUILayout.Toolbar(
                _tabIndex,
                new[]
                {
                    "Chat",
                    "Sessions",
                    "Voice",
                    "Mesh",
                    "Notifications",
                    "Tools",
                    "Handoff",
                    "Settings"
                }
            );

            GUILayout.Space(8);

            switch (_tabIndex)
            {
                case 0:
                    RenderChatTab();
                    break;
                case 1:
                    RenderSessionsTab();
                    break;
                case 2:
                    RenderVoiceTab();
                    break;
                case 3:
                    RenderMeshTab();
                    break;
                case 4:
                    RenderNotificationsTab();
                    break;
                case 5:
                    RenderToolsTab();
                    break;
                case 6:
                    RenderHandoffTab();
                    break;
                case 7:
                    RenderSettingsTab();
                    break;
            }

            GUILayout.Space(8);
            GUILayout.Label($"Status: {_status}");
            if (!string.IsNullOrEmpty(_lastTurnId))
            {
                GUILayout.Label($"Turn: {_lastTurnId}");
            }

            GUILayout.EndArea();
        }
    }
}

