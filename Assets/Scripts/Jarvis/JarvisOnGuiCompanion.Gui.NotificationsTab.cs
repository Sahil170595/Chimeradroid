using UnityEngine;

namespace Chimeradroid
{
    public sealed partial class JarvisOnGuiCompanion
    {
        private Vector2 _notificationsScroll;

        private void RenderNotificationsTab()
        {
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Refresh notifications"))
            {
                StartCoroutine(LoadNotifications(limit: 50));
            }
            if (GUILayout.Button("Refresh proactive"))
            {
                StartCoroutine(LoadProactiveStatus());
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Proactive enable"))
            {
                StartCoroutine(SetProactiveEnabled(enabled: true));
            }
            if (GUILayout.Button("Proactive disable"))
            {
                StartCoroutine(SetProactiveEnabled(enabled: false));
            }
            if (GUILayout.Button("Save consents"))
            {
                StartCoroutine(ApplyProactiveConsents());
            }
            GUILayout.EndHorizontal();

            if (!string.IsNullOrEmpty(_proactiveStatus))
            {
                GUILayout.Label(_proactiveStatus);
            }

            if (_proactive != null && _proactive.Triggers != null)
            {
                GUILayout.Label("Consents:");
                foreach (var t in _proactive.Triggers)
                {
                    if (string.IsNullOrWhiteSpace(t))
                    {
                        continue;
                    }

                    var enabled = true;
                    if (_proactiveDraft != null && _proactiveDraft.TryGetValue(t, out var cur))
                    {
                        enabled = cur;
                    }

                    var next = GUILayout.Toggle(enabled, t);
                    if (_proactiveDraft != null)
                    {
                        _proactiveDraft[t] = next;
                    }
                }
            }

            if (!string.IsNullOrEmpty(_notificationsStatus))
            {
                GUILayout.Label(_notificationsStatus);
            }

            _notificationsScroll = GUILayout.BeginScrollView(_notificationsScroll, GUILayout.Height(380));
            if (_notifications == null || _notifications.Length == 0)
            {
                GUILayout.Label("(no notifications)");
            }
            else
            {
                foreach (var n in _notifications)
                {
                    if (n == null || string.IsNullOrWhiteSpace(n.NotificationId))
                    {
                        continue;
                    }

                    GUILayout.BeginVertical("box");
                    GUILayout.Label($"{n.TriggerType} @ {n.CreatedAt}");
                    GUILayout.Label(n.Content);
                    if (!string.IsNullOrWhiteSpace(n.ReadAt))
                    {
                        GUILayout.Label($"read_at: {n.ReadAt}");
                    }
                    else
                    {
                        if (GUILayout.Button("Mark read"))
                        {
                            StartCoroutine(MarkNotificationRead(n.NotificationId));
                        }
                    }
                    GUILayout.EndVertical();
                }
            }
            GUILayout.EndScrollView();
        }
    }
}

