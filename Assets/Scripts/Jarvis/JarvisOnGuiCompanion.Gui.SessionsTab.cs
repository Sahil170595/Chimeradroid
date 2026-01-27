using UnityEngine;

namespace Chimeradroid
{
    public sealed partial class JarvisOnGuiCompanion
    {
        private Vector2 _sessionsScroll;

        private void RenderSessionsTab()
        {
            if (GUILayout.Button("Refresh Sessions"))
            {
                StartCoroutine(LoadSessions(limit: 25, offset: 0));
            }

            if (!string.IsNullOrEmpty(_sessionsStatus))
            {
                GUILayout.Label(_sessionsStatus);
            }

            if (_sessions == null || _sessions.Length == 0)
            {
                GUILayout.Label("(no sessions loaded)");
                return;
            }

            _sessionsScroll = GUILayout.BeginScrollView(_sessionsScroll, GUILayout.Height(640));
            foreach (var s in _sessions)
            {
                if (s == null || string.IsNullOrEmpty(s.SessionId))
                {
                    continue;
                }

                GUILayout.BeginVertical("box");
                GUILayout.Label($"session_id: {s.SessionId}");
                if (!string.IsNullOrEmpty(s.LastActivityAt))
                {
                    GUILayout.Label($"last_activity_at: {s.LastActivityAt}");
                }
                if (!string.IsNullOrEmpty(s.LastTurnStatus))
                {
                    GUILayout.Label($"last_turn_status: {s.LastTurnStatus}");
                }
                GUILayout.Label($"turns: {s.TurnsCount} | messages: {s.MessagesCount}");

                GUILayout.BeginHorizontal();
                if (GUILayout.Button("Use"))
                {
                    _sessionId = s.SessionId;
                    PlayerPrefs.SetString(PrefKeySessionId, _sessionId);
                    PlayerPrefs.Save();
                }
                if (GUILayout.Button("Timeline"))
                {
                    _sessionId = s.SessionId;
                    PlayerPrefs.SetString(PrefKeySessionId, _sessionId);
                    PlayerPrefs.Save();
                    StartCoroutine(LoadTimeline());
                }
                if (GUILayout.Button("Stream"))
                {
                    _sessionId = s.SessionId;
                    PlayerPrefs.SetString(PrefKeySessionId, _sessionId);
                    PlayerPrefs.Save();
                    StartStreamWithUrl(s.StreamUrl);
                }
                GUILayout.EndHorizontal();
                GUILayout.EndVertical();
            }
            GUILayout.EndScrollView();
        }
    }
}

