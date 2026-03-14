using System;
using System.Threading;
using System.Threading.Tasks;
using Chimeradroid.Jarvis;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Chimeradroid
{
    public sealed partial class JarvisOnGuiCompanion
    {
        private CancellationTokenSource _streamCts;
        private int _lastSeq = 0;
        private float _nextReconnectAt = 0f;
        private int _reconnectAttempts = 0;
        private bool _autoReconnect = true;
        private string _deltaTurnId;

        private const float ReconnectBaseDelay = 2f;
        private const float ReconnectMaxDelay = 60f;

        private void StartStream()
        {
            StartStreamWithUrl(null);
        }

        private void StartStreamWithUrl(string streamUrl)
        {
            var baseUrl = JarvisWeb.NormalizeBaseUrl(BaseUrl);
            if (string.IsNullOrEmpty(baseUrl))
            {
                _streamStatus = "missing base url";
                return;
            }
            if (string.IsNullOrEmpty(_deviceKey))
            {
                _streamStatus = "missing device key";
                return;
            }

            var wsUrl = streamUrl;
            if (string.IsNullOrWhiteSpace(wsUrl))
            {
                var wsBase = baseUrl.StartsWith("https://")
                    ? baseUrl.Replace("https://", "wss://")
                    : baseUrl.Replace("http://", "ws://");
                wsUrl = $"{wsBase}/jarvis/v2/stream";
                if (!string.IsNullOrWhiteSpace(_sessionId))
                {
                    wsUrl = $"{wsUrl}?session_id={Uri.EscapeDataString(_sessionId)}";
                }
            }
            else
            {
                if (wsUrl.StartsWith("https://"))
                {
                    wsUrl = wsUrl.Replace("https://", "wss://");
                }
                else if (wsUrl.StartsWith("http://"))
                {
                    wsUrl = wsUrl.Replace("http://", "ws://");
                }
            }

            _streamCts?.Cancel();
            _streamCts?.Dispose();
            _streamCts = new CancellationTokenSource();
            _streamStatus = "connecting...";

            var hello = new StreamHello
            {
                Token = _deviceKey,
                SessionId = string.IsNullOrWhiteSpace(_sessionId) ? null : _sessionId
            };
            _ = ConnectStreamAsync(wsUrl, hello, _streamCts.Token);
        }

        private async Task ConnectStreamAsync(
            string wsUrl,
            StreamHello hello,
            CancellationToken cancellationToken
        )
        {
            try
            {
                await _stream.ConnectAsync(wsUrl, hello, cancellationToken, recoverSinceSeq: _lastSeq);
                _streamStatus = "connected";
                _reconnectAttempts = 0;
            }
            catch (OperationCanceledException)
            {
                _streamStatus = "disconnected";
            }
            catch (Exception e)
            {
                _streamStatus = $"connect failed: {e.Message}";
            }
        }

        private void StopStream()
        {
            _reconnectAttempts = 0;
            try
            {
                _streamCts?.Cancel();
            }
            catch
            {
                // ignore
            }
            _streamStatus = "disconnecting...";
            _ = DisconnectStreamAsync();
        }

        private async Task DisconnectStreamAsync()
        {
            try
            {
                await _stream.DisconnectAsync(CancellationToken.None);
            }
            catch (Exception e)
            {
                Debug.LogWarning($"Stream disconnect error: {e.Message}");
            }
            _streamStatus = "disconnected";
        }

        private void OnStreamMessage(string raw)
        {
            try
            {
                var obj = JObject.Parse(raw);
                var t = (string)obj["type"];
                var turnId = (string)(obj["turn_id"] ?? obj["payload"]?["turn_id"]);
                if (!string.IsNullOrWhiteSpace(turnId) && _lastTurnId != turnId)
                {
                    _lastTurnId = turnId;
                }
                var seqToken = obj["seq"];
                if (seqToken != null && int.TryParse(seqToken.ToString(), out var seq))
                {
                    if (seq > _lastSeq) _lastSeq = seq;
                }
                if (t == "assistant.delta")
                {
                    if (!string.IsNullOrWhiteSpace(turnId) && _deltaTurnId != turnId)
                    {
                        _deltaTurnId = turnId;
                        _lastResponse = "";
                    }
                    var delta = (string)(obj["delta"] ?? obj["payload"]?["delta"]);
                    if (!string.IsNullOrEmpty(delta))
                    {
                        _lastResponse += delta;
                    }
                }
                else if (t == "assistant.final")
                {
                    var text = (string)(obj["text"] ?? obj["payload"]?["text"]);
                    if (!string.IsNullOrEmpty(text))
                    {
                        _deltaTurnId = turnId ?? _deltaTurnId;
                        _lastResponse = text;
                        MaybeSpeak(text);
                    }
                }
                else if (t == "turn.status")
                {
                    var status = (string)(obj["status"] ?? obj["payload"]?["status"]);
                    if (!string.IsNullOrWhiteSpace(status))
                    {
                        _status = status;
                    }
                }
                else if (t == "server.hello")
                {
                    var sess = (string)obj["session_id"];
                    if (!string.IsNullOrWhiteSpace(sess))
                    {
                        _sessionId = sess;
                        SetPref(PrefKeySessionId, _sessionId);
                    }
                }
                else if (t == "tool.approval_required")
                {
                    var toolRunId = (string)(
                        obj["tool_run_id"] ?? obj["payload"]?["tool_run_id"]
                    );
                    var tool = obj["tool"] ?? obj["payload"]?["tool"];
                    var toolName = (string)tool?["name"];
                    var risk = (string)tool?["risk_tier"];
                    var args = tool?["args"]?.ToString(Newtonsoft.Json.Formatting.None) ?? "{}";
                    if (!string.IsNullOrWhiteSpace(toolRunId))
                    {
                        foreach (var existing in _pendingTools)
                        {
                            if (existing.ToolRunId == toolRunId)
                            {
                                return;
                            }
                        }
                        _pendingTools.Add(
                            new PendingToolRun
                            {
                                ToolRunId = toolRunId,
                                ToolName = toolName ?? "",
                                RiskTier = risk ?? "",
                                ArgsJson = args
                            }
                        );
                    }
                }
                else if (t == "tool.approved")
                {
                    var toolRunId = (string)(
                        obj["tool_run_id"] ?? obj["payload"]?["tool_run_id"]
                    );
                    var approvalId = (string)(
                        obj["approval_id"] ?? obj["payload"]?["approval_id"]
                    );
                    if (!string.IsNullOrWhiteSpace(toolRunId) && !string.IsNullOrWhiteSpace(approvalId))
                    {
                        foreach (var p in _pendingTools)
                        {
                            if (p.ToolRunId == toolRunId)
                            {
                                p.ApprovalId = approvalId;
                                break;
                            }
                        }
                    }
                }
                else if (t == "tool.result")
                {
                    var toolRunId = (string)(
                        obj["tool_run_id"] ?? obj["payload"]?["tool_run_id"]
                    );
                    if (!string.IsNullOrWhiteSpace(toolRunId))
                    {
                        for (var i = _pendingTools.Count - 1; i >= 0; i--)
                        {
                            if (_pendingTools[i].ToolRunId == toolRunId)
                            {
                                _pendingTools.RemoveAt(i);
                            }
                        }
                    }
                }
                else if (!string.IsNullOrWhiteSpace(t) && t.StartsWith("workflow.", StringComparison.Ordinal))
                {
                    var workflowId = (string)(obj["workflow_id"] ?? obj["payload"]?["workflow_id"]);
                    if (!string.IsNullOrWhiteSpace(workflowId))
                    {
                        if (_companionState != null && _companionState.SelectedWorkflowId == workflowId)
                        {
                            StartCoroutine(LoadWorkflowContinuation(workflowId));
                        }
                        else
                        {
                            StartCoroutine(RefreshWorkflowInbox());
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning($"Jarvis stream parse error: {e.Message}");
            }
        }

        private void StreamTick()
        {
            if (!_autoReconnect)
            {
                return;
            }

            if (Time.realtimeSinceStartup < _nextReconnectAt)
            {
                return;
            }

            if (_stream == null || _stream.IsConnected)
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(BaseUrl) || string.IsNullOrWhiteSpace(_deviceKey))
            {
                return;
            }

            if (_streamStatus != null && _streamStatus.StartsWith("connecting"))
            {
                return;
            }

            _reconnectAttempts++;
            var delay = Mathf.Min(
                ReconnectBaseDelay * Mathf.Pow(2f, _reconnectAttempts - 1),
                ReconnectMaxDelay
            );
            _nextReconnectAt = Time.realtimeSinceStartup + delay;
            _streamStatus = $"reconnecting (attempt {_reconnectAttempts}, next in {delay:F0}s)...";
            StartStream();
        }
    }
}
