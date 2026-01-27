using System;
using System.Collections;
using Chimeradroid.Jarvis;
using Newtonsoft.Json;

namespace Chimeradroid
{
    public sealed partial class JarvisOnGuiCompanion
    {
        private string _whoami = "";
        private string _meshStatus = "";
        private int _meshSinceSeq = 0;
        private string _meshEventType = "chimeradroid.ping";
        private string _meshEventJson = "{\"at\":0}";
        private string _meshLog = "";

        private IEnumerator LoadWhoami()
        {
            var baseUrl = JarvisWeb.NormalizeBaseUrl(BaseUrl);
            if (string.IsNullOrEmpty(baseUrl))
            {
                _whoami = "missing base url";
                yield break;
            }

            if (string.IsNullOrEmpty(_deviceKey))
            {
                _whoami = "missing device key";
                yield break;
            }

            var url = $"{baseUrl}/jarvis/v2/whoami";
            WhoamiResponse resp = null;
            string err = null;
            yield return JarvisWeb.GetJson<WhoamiResponse>(
                url,
                JarvisWeb.DeviceKeyHeader,
                _deviceKey,
                r => resp = r,
                e => err = e
            );

            if (!string.IsNullOrEmpty(err) || resp == null)
            {
                _whoami = err ?? "whoami failed";
                yield break;
            }

            _whoami = $"auth_type={resp.AuthType}\nuser_id={resp.UserId}\ndevice_id={resp.DeviceId}";
        }

        private IEnumerator MeshPush()
        {
            var baseUrl = JarvisWeb.NormalizeBaseUrl(BaseUrl);
            if (string.IsNullOrEmpty(baseUrl))
            {
                _meshStatus = "missing base url";
                yield break;
            }

            if (string.IsNullOrEmpty(_deviceKey))
            {
                _meshStatus = "missing device key";
                yield break;
            }

            object dataObj = null;
            try
            {
                dataObj = JsonConvert.DeserializeObject<object>(_meshEventJson);
            }
            catch
            {
                _meshStatus = "invalid event_data json";
                yield break;
            }

            _meshStatus = "pushing mesh event...";
            var url = $"{baseUrl}/jarvis/v2/mesh/push";
            var payload = new MeshPushRequest { EventType = _meshEventType, EventData = dataObj };

            MeshPushResponse resp = null;
            string err = null;
            yield return JarvisWeb.PostJson<MeshPushRequest, MeshPushResponse>(
                url,
                payload,
                JarvisWeb.DeviceKeyHeader,
                _deviceKey,
                r => resp = r,
                e => err = e
            );

            if (!string.IsNullOrEmpty(err) || resp == null)
            {
                _meshStatus = err ?? "mesh push failed";
                yield break;
            }

            _meshStatus = $"pushed seq={resp.Seq}";
            if (resp.Seq > _meshSinceSeq)
            {
                _meshSinceSeq = Math.Max(0, resp.Seq - 1);
            }
        }

        private IEnumerator MeshPull(bool includeSelf)
        {
            var baseUrl = JarvisWeb.NormalizeBaseUrl(BaseUrl);
            if (string.IsNullOrEmpty(baseUrl))
            {
                _meshStatus = "missing base url";
                yield break;
            }

            if (string.IsNullOrEmpty(_deviceKey))
            {
                _meshStatus = "missing device key";
                yield break;
            }

            _meshStatus = "pulling mesh events...";
            var url =
                $"{baseUrl}/jarvis/v2/mesh/pull?since_seq={_meshSinceSeq}&limit=50&include_self={(includeSelf ? "true" : "false")}";

            MeshPullResponse resp = null;
            string err = null;
            yield return JarvisWeb.GetJson<MeshPullResponse>(
                url,
                JarvisWeb.DeviceKeyHeader,
                _deviceKey,
                r => resp = r,
                e => err = e
            );

            if (!string.IsNullOrEmpty(err) || resp == null || resp.Events == null)
            {
                _meshStatus = err ?? "mesh pull failed";
                yield break;
            }

            var events = resp.Events;
            Array.Sort(events, (a, b) => a.Seq.CompareTo(b.Seq));
            var sb = new System.Text.StringBuilder();
            var maxSeq = _meshSinceSeq;
            foreach (var e in events)
            {
                if (e == null)
                {
                    continue;
                }
                maxSeq = Math.Max(maxSeq, e.Seq);
                sb.AppendLine($"[{e.Seq}] {e.EventType} ({e.SourceDeviceId}) {e.CreatedAt}");
                if (e.EventData != null)
                {
                    sb.AppendLine(JsonConvert.SerializeObject(e.EventData, Formatting.None));
                }
            }
            _meshLog = sb.ToString();
            _meshSinceSeq = maxSeq;
            _meshStatus = $"pulled {events.Length} events (since_seq={_meshSinceSeq})";
        }
    }
}

