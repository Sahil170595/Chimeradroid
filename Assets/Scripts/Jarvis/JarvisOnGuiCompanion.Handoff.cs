using System;
using System.Collections;
using Chimeradroid.Jarvis;

namespace Chimeradroid
{
    public sealed partial class JarvisOnGuiCompanion
    {
        private IEnumerator RedeemHandoff(string token)
        {
            var baseUrl = JarvisWeb.NormalizeBaseUrl(BaseUrl);
            if (string.IsNullOrEmpty(baseUrl))
            {
                _status = "missing base url";
                yield break;
            }

            if (string.IsNullOrEmpty(_deviceKey))
            {
                _status = "missing device key";
                yield break;
            }

            token = (token ?? "").Trim();
            if (string.IsNullOrEmpty(token))
            {
                _status = "missing token";
                yield break;
            }

            var url = $"{baseUrl}/jarvis/v2/sessions/handoff/redeem";
            var payload = new SessionHandoffRedeemRequest { Token = token };

            SessionHandoffRedeemResponse resp = null;
            string err = null;
            yield return JarvisWeb.PostJson<SessionHandoffRedeemRequest, SessionHandoffRedeemResponse>(
                url,
                payload,
                JarvisWeb.DeviceKeyHeader,
                _deviceKey,
                r => resp = r,
                e => err = e
            );

            if (
                !string.IsNullOrEmpty(err) || resp == null || string.IsNullOrWhiteSpace(resp.SessionId)
            )
            {
                _status = "handoff redeem failed";
                _lastResponse = err ?? "unknown error";
                yield break;
            }

            _sessionId = resp.SessionId;
            SetPref(PrefKeySessionId, _sessionId);
            _status = "handoff redeemed";
            _lastResponse = $"session_id={resp.SessionId}\nstream_url={resp.StreamUrl}";
        }

        private IEnumerator LoadTimeline()
        {
            var baseUrl = JarvisWeb.NormalizeBaseUrl(BaseUrl);
            if (string.IsNullOrEmpty(baseUrl))
            {
                _status = "missing base url";
                yield break;
            }

            if (string.IsNullOrEmpty(_deviceKey))
            {
                _status = "missing device key";
                yield break;
            }

            if (string.IsNullOrWhiteSpace(_sessionId))
            {
                _status = "missing session id";
                yield break;
            }

            var url =
                $"{baseUrl}/jarvis/v2/sessions/{Uri.EscapeDataString(_sessionId)}/timeline?limit_turns=50&limit_messages=100";
            SessionTimelineResponse resp = null;
            string err = null;
            yield return JarvisWeb.GetJson<SessionTimelineResponse>(
                url,
                JarvisWeb.DeviceKeyHeader,
                _deviceKey,
                r => resp = r,
                e => err = e
            );

            if (!string.IsNullOrEmpty(err) || resp == null || resp.Messages == null)
            {
                _status = "timeline failed";
                _lastResponse = err ?? "unknown error";
                yield break;
            }

            var sb = new System.Text.StringBuilder();
            sb.AppendLine($"session_id: {resp.Session?.SessionId}");
            if (!string.IsNullOrWhiteSpace(resp.StreamUrl))
            {
                sb.AppendLine($"stream_url: {resp.StreamUrl}");
            }
            sb.AppendLine();

            foreach (var m in resp.Messages)
            {
                if (m == null) continue;
                var ts = string.IsNullOrWhiteSpace(m.CreatedAt) ? "" : $"[{m.CreatedAt}] ";
                sb.AppendLine($"{ts}{m.Role}: {m.Content}");
            }
            _timeline = sb.ToString();
            _status = "timeline loaded";
        }
    }
}
