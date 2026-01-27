using System;
using System.Collections;
using Chimeradroid.Jarvis;

namespace Chimeradroid
{
    public sealed partial class JarvisOnGuiCompanion
    {
        private SessionSummary[] _sessions = Array.Empty<SessionSummary>();
        private string _sessionsStatus = "";

        private IEnumerator LoadSessions(int limit, int offset)
        {
            var baseUrl = JarvisWeb.NormalizeBaseUrl(BaseUrl);
            if (string.IsNullOrEmpty(baseUrl))
            {
                _sessionsStatus = "missing base url";
                yield break;
            }

            if (string.IsNullOrEmpty(_deviceKey))
            {
                _sessionsStatus = "missing device key";
                yield break;
            }

            limit = Math.Max(1, Math.Min(limit, 200));
            offset = Math.Max(0, offset);

            _sessionsStatus = "loading sessions...";
            var url = $"{baseUrl}/jarvis/v2/sessions?limit={limit}&offset={offset}";

            SessionsListResponse resp = null;
            string err = null;
            yield return JarvisWeb.GetJson<SessionsListResponse>(
                url,
                JarvisWeb.DeviceKeyHeader,
                _deviceKey,
                r => resp = r,
                e => err = e
            );

            if (!string.IsNullOrEmpty(err) || resp == null || resp.Sessions == null)
            {
                _sessionsStatus = err ?? "sessions list failed";
                yield break;
            }

            _sessions = resp.Sessions ?? Array.Empty<SessionSummary>();
            _sessionsStatus = $"sessions loaded: {_sessions.Length}";
        }
    }
}

