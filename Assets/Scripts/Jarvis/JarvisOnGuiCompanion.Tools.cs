using System;
using System.Collections;
using System.Collections.Generic;
using Chimeradroid.Jarvis;
using Newtonsoft.Json;

namespace Chimeradroid
{
    public sealed partial class JarvisOnGuiCompanion
    {
        private readonly List<PendingToolRun> _pendingTools = new List<PendingToolRun>();
        private ApprovalSummary[] _controlRoomPendingApprovals;
        private ToolRunSummary[] _controlRoomExecutingTools;
        private AuditSummary[] _controlRoomRecentAudits;

        private IEnumerator RefreshControlRoom()
        {
            _toolsStatus = "loading control room...";
            var baseUrl = JarvisWeb.NormalizeBaseUrl(BaseUrl);
            if (string.IsNullOrEmpty(baseUrl))
            {
                _toolsStatus = "missing base url";
                yield break;
            }

            if (string.IsNullOrEmpty(_deviceKey))
            {
                _toolsStatus = "missing device key";
                yield break;
            }

            var url = $"{baseUrl}/jarvis/v2/system/control-room?limit=50";
            ControlRoomResponse resp = null;
            string err = null;
            yield return JarvisWeb.GetJson<ControlRoomResponse>(
                url,
                JarvisWeb.DeviceKeyHeader,
                _deviceKey,
                r => resp = r,
                e => err = e
            );

            if (!string.IsNullOrEmpty(err) || resp == null)
            {
                _toolsStatus = err ?? "control room failed";
                yield break;
            }

            _controlRoomPendingApprovals = resp.PendingApprovals ?? Array.Empty<ApprovalSummary>();
            _controlRoomExecutingTools = resp.ExecutingToolRuns ?? Array.Empty<ToolRunSummary>();
            _controlRoomRecentAudits = resp.RecentAudits ?? Array.Empty<AuditSummary>();
            _toolsStatus =
                $"control room: approvals={_controlRoomPendingApprovals.Length} "
                + $"executing={_controlRoomExecutingTools.Length} audits={_controlRoomRecentAudits.Length}";
        }

        private IEnumerator ApproveTool(string toolRunId)
        {
            if (string.IsNullOrWhiteSpace(toolRunId))
            {
                _toolsStatus = "missing tool_run_id";
                yield break;
            }

            _toolsStatus = "approving...";
            var baseUrl = JarvisWeb.NormalizeBaseUrl(BaseUrl);
            if (string.IsNullOrEmpty(baseUrl))
            {
                _toolsStatus = "missing base url";
                yield break;
            }
            if (string.IsNullOrEmpty(_deviceKey))
            {
                _toolsStatus = "missing device key";
                yield break;
            }
            var url = $"{baseUrl}/jarvis/v2/tools/approve";

            var payload = new ToolApproveRequest { ToolRunId = toolRunId };
            ToolApproveResponse resp = null;
            string err = null;
            yield return JarvisWeb.PostJson<ToolApproveRequest, ToolApproveResponse>(
                url,
                payload,
                JarvisWeb.DeviceKeyHeader,
                _deviceKey,
                r => resp = r,
                e => err = e
            );

            if (!string.IsNullOrEmpty(err) || resp == null || string.IsNullOrWhiteSpace(resp.ApprovalId))
            {
                _toolsStatus = err ?? "approve failed";
                yield break;
            }

            for (var i = 0; i < _pendingTools.Count; i++)
            {
                if (_pendingTools[i].ToolRunId == toolRunId)
                {
                    _pendingTools[i].ApprovalId = resp.ApprovalId;
                    break;
                }
            }
            _toolsStatus = $"approved: {resp.ApprovalId}";
        }

        private IEnumerator ExecuteTool(string toolRunId, string approvalId)
        {
            if (string.IsNullOrWhiteSpace(toolRunId))
            {
                _toolsStatus = "missing tool_run_id";
                yield break;
            }
            if (string.IsNullOrWhiteSpace(approvalId))
            {
                _toolsStatus = "missing approval_id";
                yield break;
            }

            _toolsStatus = "executing...";
            var baseUrl = JarvisWeb.NormalizeBaseUrl(BaseUrl);
            if (string.IsNullOrEmpty(baseUrl))
            {
                _toolsStatus = "missing base url";
                yield break;
            }
            if (string.IsNullOrEmpty(_deviceKey))
            {
                _toolsStatus = "missing device key";
                yield break;
            }
            var url = $"{baseUrl}/jarvis/v2/tools/execute";

            var payload = new ToolExecuteRequest
                { ToolRunId = toolRunId, ApprovalId = approvalId };
            ToolExecuteResponse resp = null;
            string err = null;
            yield return JarvisWeb.PostJson<ToolExecuteRequest, ToolExecuteResponse>(
                url,
                payload,
                JarvisWeb.DeviceKeyHeader,
                _deviceKey,
                r => resp = r,
                e => err = e
            );

            if (!string.IsNullOrEmpty(err) || resp == null)
            {
                _toolsStatus = err ?? "execute failed";
                yield break;
            }
            _toolsStatus = $"executed: {resp.Status}";
        }

        private IEnumerator CancelTool(string toolRunId)
        {
            if (string.IsNullOrWhiteSpace(toolRunId))
            {
                _toolsStatus = "missing tool_run_id";
                yield break;
            }

            _toolsStatus = "cancelling...";
            var baseUrl = JarvisWeb.NormalizeBaseUrl(BaseUrl);
            if (string.IsNullOrEmpty(baseUrl))
            {
                _toolsStatus = "missing base url";
                yield break;
            }
            if (string.IsNullOrEmpty(_deviceKey))
            {
                _toolsStatus = "missing device key";
                yield break;
            }
            var url =
                $"{baseUrl}/jarvis/v2/tools/cancel?tool_run_id={Uri.EscapeDataString(toolRunId)}";
            string okBody = null;
            string err = null;
            yield return JarvisWeb.PostQuery(
                url,
                JarvisWeb.DeviceKeyHeader,
                _deviceKey,
                body => okBody = body,
                e => err = e
            );

            _toolsStatus = !string.IsNullOrEmpty(err) ? err : (okBody ?? "cancelled");
        }

        private sealed class PendingToolRun
        {
            public string ToolRunId;
            public string ToolName;
            public string RiskTier;
            public string ArgsJson;
            public string ApprovalId;
        }
    }
}
