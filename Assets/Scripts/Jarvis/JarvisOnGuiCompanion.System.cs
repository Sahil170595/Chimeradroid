using System;
using System.Collections;
using Chimeradroid.Jarvis;
using Newtonsoft.Json;
using UnityEngine;

namespace Chimeradroid
{
    public sealed partial class JarvisOnGuiCompanion
    {
        private string _systemStatusStatus = "";
        private string _systemStatusJson = "";
        private Vector2 _systemStatusScroll;

        private IEnumerator LoadSystemStatus()
        {
            _systemStatusStatus = "loading...";
            var baseUrl = JarvisWeb.NormalizeBaseUrl(BaseUrl);
            if (string.IsNullOrEmpty(baseUrl))
            {
                _systemStatusStatus = "missing base url";
                yield break;
            }

            if (string.IsNullOrEmpty(_deviceKey))
            {
                _systemStatusStatus = "missing device key";
                yield break;
            }

            var url = $"{baseUrl}/jarvis/v2/system/status";
            SystemStatusResponse resp = null;
            string err = null;
            yield return JarvisWeb.GetJson<SystemStatusResponse>(
                url,
                JarvisWeb.DeviceKeyHeader,
                _deviceKey,
                r => resp = r,
                e => err = e
            );

            if (!string.IsNullOrEmpty(err) || resp == null)
            {
                _systemStatusStatus = err ?? "status failed";
                return;
            }

            _systemStatusStatus = resp.Status ?? "unknown";
            _systemStatusJson = JsonConvert.SerializeObject(resp, Formatting.Indented);
        }

        private void RenderSystemStatusPanel(float height)
        {
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Refresh system status"))
            {
                StartCoroutine(LoadSystemStatus());
            }
            if (GUILayout.Button("Open mobile web UI"))
            {
                var baseUrl = JarvisWeb.NormalizeBaseUrl(BaseUrl);
                if (!string.IsNullOrEmpty(baseUrl))
                {
                    Application.OpenURL($"{baseUrl}/jarvis/v2/mobile");
                }
            }
            GUILayout.EndHorizontal();

            if (!string.IsNullOrEmpty(_systemStatusStatus))
            {
                GUILayout.Label($"System: {_systemStatusStatus}");
            }

            _systemStatusScroll = GUILayout.BeginScrollView(_systemStatusScroll, GUILayout.Height(height));
            GUILayout.TextArea(string.IsNullOrEmpty(_systemStatusJson) ? "(no status loaded)" : _systemStatusJson);
            GUILayout.EndScrollView();
        }
    }
}

