using UnityEngine;

namespace Chimeradroid
{
    public sealed partial class JarvisOnGuiCompanion
    {
        private int _tabIndex = 0;
        private bool _showSecrets = false;

        private void OnGUI()
        {
            float dpiScale = Mathf.Max(1f, Screen.dpi / 160f);
            int pad = (int)(10 * dpiScale);
            int w = Mathf.Min((int)(560 * dpiScale), Screen.width - pad * 2);
            int h = Mathf.Min((int)(820 * dpiScale), Screen.height - pad * 2);

            GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(dpiScale, dpiScale, 1f));
            w = (int)(w / dpiScale);
            h = (int)(h / dpiScale);
            pad = (int)(pad / dpiScale);

            var rect = new Rect(pad, pad, w, h);
            GUI.Box(rect, "JARVIS Companion (Chimeradroid)");
            GUILayout.BeginArea(new Rect(pad + 10, pad + 25, w - 20, h - 35));

            _tabIndex = GUILayout.Toolbar(
                _tabIndex,
                new[]
                {
                    "Chat",
                    "Workflows",
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
                    RenderWorkflowsTab();
                    break;
                case 2:
                    RenderSessionsTab();
                    break;
                case 3:
                    RenderVoiceTab();
                    break;
                case 4:
                    RenderMeshTab();
                    break;
                case 5:
                    RenderNotificationsTab();
                    break;
                case 6:
                    RenderToolsTab();
                    break;
                case 7:
                    RenderHandoffTab();
                    break;
                case 8:
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
            GUI.matrix = Matrix4x4.identity;
        }
    }
}
