using System;
using System.Collections;
using System.Text;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;

namespace Chimeradroid.Jarvis
{
    public static class JarvisWeb
    {
        public const string MasterKeyHeader = "X-Jarvis-Key";
        public const string DeviceKeyHeader = "X-Jarvis-Device-Key";

        private const int DefaultTimeoutSeconds = 20;
        private const int DefaultRetryCount429 = 4;

        public static string NormalizeBaseUrl(string baseUrl)
        {
            if (string.IsNullOrWhiteSpace(baseUrl))
            {
                return "";
            }

            var trimmed = baseUrl.Trim().TrimEnd('/');

            if (!trimmed.StartsWith("http://", StringComparison.OrdinalIgnoreCase)
                && !trimmed.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                return "";
            }

            return trimmed;
        }

        public static IEnumerator PostJson<TRequest, TResponse>(
            string url,
            TRequest payload,
            string headerName,
            string headerValue,
            Action<TResponse> onOk,
            Action<string> onError
        )
        {
            var json = JsonConvert.SerializeObject(payload);
            var bytes = Encoding.UTF8.GetBytes(json);

            for (var attempt = 0; attempt < DefaultRetryCount429; attempt++)
            {
                using (var req = new UnityWebRequest(url, "POST"))
                {
                    req.uploadHandler = new UploadHandlerRaw(bytes);
                    req.downloadHandler = new DownloadHandlerBuffer();
                    req.timeout = DefaultTimeoutSeconds;
                    req.SetRequestHeader("Content-Type", "application/json");
                    if (!string.IsNullOrEmpty(headerName) && !string.IsNullOrEmpty(headerValue))
                    {
                        req.SetRequestHeader(headerName, headerValue);
                    }

                    yield return req.SendWebRequest();

                    if (req.responseCode == 429 && attempt < DefaultRetryCount429 - 1)
                    {
                        var retryAfter = ParseRetryAfterSeconds(req);
                        yield return new WaitForSecondsRealtime(retryAfter);
                        continue;
                    }

                    if (req.result != UnityWebRequest.Result.Success)
                    {
                        onError?.Invoke(BuildError(req));
                        yield break;
                    }

                    try
                    {
                        var obj = JsonConvert.DeserializeObject<TResponse>(req.downloadHandler.text);
                        onOk?.Invoke(obj);
                    }
                    catch (Exception e)
                    {
                        onError?.Invoke($"Failed to parse JSON: {e.Message}\n{req.downloadHandler.text}");
                    }
                }

                yield break;
            }
        }

        public static IEnumerator GetJson<TResponse>(
            string url,
            string headerName,
            string headerValue,
            Action<TResponse> onOk,
            Action<string> onError
        )
        {
            for (var attempt = 0; attempt < DefaultRetryCount429; attempt++)
            {
                using (var req = UnityWebRequest.Get(url))
                {
                    req.timeout = DefaultTimeoutSeconds;
                    if (!string.IsNullOrEmpty(headerName) && !string.IsNullOrEmpty(headerValue))
                    {
                        req.SetRequestHeader(headerName, headerValue);
                    }

                    yield return req.SendWebRequest();

                    if (req.responseCode == 429 && attempt < DefaultRetryCount429 - 1)
                    {
                        var retryAfter = ParseRetryAfterSeconds(req);
                        yield return new WaitForSecondsRealtime(retryAfter);
                        continue;
                    }

                    if (req.result != UnityWebRequest.Result.Success)
                    {
                        onError?.Invoke(BuildError(req));
                        yield break;
                    }

                    try
                    {
                        var obj = JsonConvert.DeserializeObject<TResponse>(req.downloadHandler.text);
                        onOk?.Invoke(obj);
                    }
                    catch (Exception e)
                    {
                        onError?.Invoke($"Failed to parse JSON: {e.Message}\n{req.downloadHandler.text}");
                    }
                }

                yield break;
            }
        }

        public static IEnumerator PatchJson<TRequest, TResponse>(
            string url,
            TRequest payload,
            string headerName,
            string headerValue,
            Action<TResponse> onOk,
            Action<string> onError
        )
        {
            var json = JsonConvert.SerializeObject(payload);
            var bytes = Encoding.UTF8.GetBytes(json);

            for (var attempt = 0; attempt < DefaultRetryCount429; attempt++)
            {
                using (var req = new UnityWebRequest(url, "PATCH"))
                {
                    req.uploadHandler = new UploadHandlerRaw(bytes);
                    req.downloadHandler = new DownloadHandlerBuffer();
                    req.timeout = DefaultTimeoutSeconds;
                    req.SetRequestHeader("Content-Type", "application/json");
                    if (!string.IsNullOrEmpty(headerName) && !string.IsNullOrEmpty(headerValue))
                    {
                        req.SetRequestHeader(headerName, headerValue);
                    }

                    yield return req.SendWebRequest();

                    if (req.responseCode == 429 && attempt < DefaultRetryCount429 - 1)
                    {
                        var retryAfter = ParseRetryAfterSeconds(req);
                        yield return new WaitForSecondsRealtime(retryAfter);
                        continue;
                    }

                    if (req.result != UnityWebRequest.Result.Success)
                    {
                        onError?.Invoke(BuildError(req));
                        yield break;
                    }

                    try
                    {
                        var obj = JsonConvert.DeserializeObject<TResponse>(req.downloadHandler.text);
                        onOk?.Invoke(obj);
                    }
                    catch (Exception e)
                    {
                        onError?.Invoke($"Failed to parse JSON: {e.Message}\n{req.downloadHandler.text}");
                    }
                }

                yield break;
            }
        }

        public static IEnumerator PostQuery(
            string url,
            string headerName,
            string headerValue,
            Action<string> onOkBody,
            Action<string> onError
        )
        {
            for (var attempt = 0; attempt < DefaultRetryCount429; attempt++)
            {
                using (var req = new UnityWebRequest(url, "POST"))
                {
                    req.downloadHandler = new DownloadHandlerBuffer();
                    req.timeout = DefaultTimeoutSeconds;
                    if (!string.IsNullOrEmpty(headerName) && !string.IsNullOrEmpty(headerValue))
                    {
                        req.SetRequestHeader(headerName, headerValue);
                    }

                    yield return req.SendWebRequest();

                    if (req.responseCode == 429 && attempt < DefaultRetryCount429 - 1)
                    {
                        var retryAfter = ParseRetryAfterSeconds(req);
                        yield return new WaitForSecondsRealtime(retryAfter);
                        continue;
                    }

                    if (req.result != UnityWebRequest.Result.Success)
                    {
                        onError?.Invoke(BuildError(req));
                        yield break;
                    }

                    onOkBody?.Invoke(req.downloadHandler.text ?? "");
                }

                yield break;
            }
        }

        private static string BuildError(UnityWebRequest req)
        {
            var body = "";
            try
            {
                body = req.downloadHandler?.text ?? "";
            }
            catch
            {
                // DownloadHandler may be disposed
            }

            return $"HTTP {(long)req.responseCode}: {req.error}\n{body}";
        }

        private static float ParseRetryAfterSeconds(UnityWebRequest req)
        {
            try
            {
                var h = req.GetResponseHeader("Retry-After");
                if (!string.IsNullOrEmpty(h) && int.TryParse(h, out var seconds))
                {
                    return Mathf.Clamp(seconds, 1, 30);
                }
            }
            catch
            {
                // header access can fail on some platforms
            }

            return 1f;
        }
    }
}
