using System;
using System.Collections;
using System.Collections.Generic;
using Chimeradroid.Jarvis;

namespace Chimeradroid
{
    public sealed partial class JarvisOnGuiCompanion
    {
        private Notification[] _notifications = Array.Empty<Notification>();
        private string _notificationsStatus = "";
        private ProactiveStatusResponse _proactive;
        private string _proactiveStatus = "";
        private Dictionary<string, bool> _proactiveDraft = new Dictionary<string, bool>();

        private IEnumerator LoadNotifications(int limit)
        {
            var baseUrl = JarvisWeb.NormalizeBaseUrl(BaseUrl);
            if (string.IsNullOrEmpty(baseUrl))
            {
                _notificationsStatus = "missing base url";
                yield break;
            }

            if (string.IsNullOrEmpty(_deviceKey))
            {
                _notificationsStatus = "missing device key";
                yield break;
            }

            limit = Math.Max(1, Math.Min(limit, 200));
            var url = $"{baseUrl}/jarvis/v2/notifications?limit={limit}";
            _notificationsStatus = "loading...";

            NotificationsListResponse resp = null;
            string err = null;
            yield return JarvisWeb.GetJson<NotificationsListResponse>(
                url,
                JarvisWeb.DeviceKeyHeader,
                _deviceKey,
                r => resp = r,
                e => err = e
            );

            if (!string.IsNullOrEmpty(err) || resp == null || resp.Notifications == null)
            {
                _notificationsStatus = err ?? "notifications failed";
                yield break;
            }

            _notifications = resp.Notifications ?? Array.Empty<Notification>();
            _notificationsStatus = $"loaded: {_notifications.Length}";
        }

        private IEnumerator MarkNotificationRead(string notificationId)
        {
            var baseUrl = JarvisWeb.NormalizeBaseUrl(BaseUrl);
            if (string.IsNullOrEmpty(baseUrl))
            {
                _notificationsStatus = "missing base url";
                yield break;
            }

            if (string.IsNullOrEmpty(_deviceKey))
            {
                _notificationsStatus = "missing device key";
                yield break;
            }

            notificationId = (notificationId ?? "").Trim();
            if (string.IsNullOrEmpty(notificationId))
            {
                _notificationsStatus = "missing notification_id";
                yield break;
            }

            var url =
                $"{baseUrl}/jarvis/v2/notifications/{Uri.EscapeDataString(notificationId)}/mark-read";
            _notificationsStatus = "marking read...";

            string okBody = null;
            string err = null;
            yield return JarvisWeb.PostQuery(
                url,
                JarvisWeb.DeviceKeyHeader,
                _deviceKey,
                body => okBody = body,
                e => err = e
            );

            _notificationsStatus = !string.IsNullOrEmpty(err) ? err : (okBody ?? "ok");
        }

        private IEnumerator LoadProactiveStatus()
        {
            var baseUrl = JarvisWeb.NormalizeBaseUrl(BaseUrl);
            if (string.IsNullOrEmpty(baseUrl))
            {
                _proactiveStatus = "missing base url";
                yield break;
            }

            if (string.IsNullOrEmpty(_deviceKey))
            {
                _proactiveStatus = "missing device key";
                yield break;
            }

            _proactiveStatus = "loading...";
            var url = $"{baseUrl}/jarvis/v2/proactive/status";

            ProactiveStatusResponse resp = null;
            string err = null;
            yield return JarvisWeb.GetJson<ProactiveStatusResponse>(
                url,
                JarvisWeb.DeviceKeyHeader,
                _deviceKey,
                r => resp = r,
                e => err = e
            );

            if (!string.IsNullOrEmpty(err) || resp == null)
            {
                _proactiveStatus = err ?? "proactive status failed";
                yield break;
            }

            _proactive = resp;
            _proactiveDraft = resp.Consents != null
                ? new Dictionary<string, bool>(resp.Consents)
                : new Dictionary<string, bool>();
            _proactiveStatus = $"enabled={resp.Enabled}";
        }

        private IEnumerator SetProactiveEnabled(bool enabled)
        {
            var baseUrl = JarvisWeb.NormalizeBaseUrl(BaseUrl);
            if (string.IsNullOrEmpty(baseUrl))
            {
                _proactiveStatus = "missing base url";
                yield break;
            }

            if (string.IsNullOrEmpty(_deviceKey))
            {
                _proactiveStatus = "missing device key";
                yield break;
            }

            var url = enabled
                ? $"{baseUrl}/jarvis/v2/proactive/enable"
                : $"{baseUrl}/jarvis/v2/proactive/disable";
            _proactiveStatus = enabled ? "enabling..." : "disabling...";

            string okBody = null;
            string err = null;
            yield return JarvisWeb.PostQuery(
                url,
                JarvisWeb.DeviceKeyHeader,
                _deviceKey,
                body => okBody = body,
                e => err = e
            );

            _proactiveStatus = !string.IsNullOrEmpty(err) ? err : (okBody ?? "ok");
            yield return LoadProactiveStatus();
        }

        private IEnumerator ApplyProactiveConsents()
        {
            var baseUrl = JarvisWeb.NormalizeBaseUrl(BaseUrl);
            if (string.IsNullOrEmpty(baseUrl))
            {
                _proactiveStatus = "missing base url";
                yield break;
            }

            if (string.IsNullOrEmpty(_deviceKey))
            {
                _proactiveStatus = "missing device key";
                yield break;
            }

            _proactiveStatus = "saving consents...";
            var url = $"{baseUrl}/jarvis/v2/proactive/consents";
            var payload = new ProactiveConsentsPatchRequest { Consents = _proactiveDraft };

            object resp = null;
            string err = null;
            yield return JarvisWeb.PatchJson<ProactiveConsentsPatchRequest, object>(
                url,
                payload,
                JarvisWeb.DeviceKeyHeader,
                _deviceKey,
                r => resp = r,
                e => err = e
            );

            _proactiveStatus = !string.IsNullOrEmpty(err) ? err : "consents saved";
            yield return LoadProactiveStatus();
        }
    }
}

