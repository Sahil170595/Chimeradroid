using Newtonsoft.Json;
using UnityEngine;

namespace Chimeradroid
{
    public sealed partial class JarvisOnGuiCompanion
    {
        private Vector2 _toolsScroll;

        private void RenderToolsTab()
        {
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Refresh Control Room"))
            {
                StartCoroutine(RefreshControlRoom());
            }
            if (GUILayout.Button("Clear stream pending"))
            {
                _pendingTools.Clear();
            }
            GUILayout.EndHorizontal();

            if (!string.IsNullOrEmpty(_toolsStatus))
            {
                GUILayout.Label($"Tools: {_toolsStatus}");
            }

            _toolsScroll = GUILayout.BeginScrollView(_toolsScroll, GUILayout.Height(660));

            GUILayout.Label("Live pending tools (from stream):");
            if (_pendingTools.Count == 0)
            {
                GUILayout.Label("(none)");
            }
            else
            {
                for (var i = _pendingTools.Count - 1; i >= 0; i--)
                {
                    var p = _pendingTools[i];
                    GUILayout.BeginVertical("box");
                    GUILayout.Label($"tool_run_id: {p.ToolRunId}");
                    GUILayout.Label($"tool: {p.ToolName} ({p.RiskTier})");
                    GUILayout.Label($"args: {p.ArgsJson}");
                    if (!string.IsNullOrWhiteSpace(p.ApprovalId))
                    {
                        GUILayout.Label($"approval_id: {p.ApprovalId}");
                    }
                    GUILayout.BeginHorizontal();
                    if (string.IsNullOrWhiteSpace(p.ApprovalId))
                    {
                        if (GUILayout.Button("Approve"))
                        {
                            StartCoroutine(ApproveTool(p.ToolRunId));
                        }
                    }
                    else
                    {
                        if (GUILayout.Button("Execute"))
                        {
                            StartCoroutine(ExecuteTool(p.ToolRunId, p.ApprovalId));
                        }
                    }
                    if (GUILayout.Button("Cancel"))
                    {
                        StartCoroutine(CancelTool(p.ToolRunId));
                    }
                    GUILayout.EndHorizontal();
                    GUILayout.EndVertical();
                }
            }

            GUILayout.Space(10);
            GUILayout.Label("Control room: pending approvals (read-only; no tool_run_id here):");
            if (_controlRoomPendingApprovals == null || _controlRoomPendingApprovals.Length == 0)
            {
                GUILayout.Label("(none)");
            }
            else
            {
                foreach (var a in _controlRoomPendingApprovals)
                {
                    if (a == null || string.IsNullOrWhiteSpace(a.ApprovalId))
                    {
                        continue;
                    }

                    GUILayout.BeginVertical("box");
                    GUILayout.Label($"approval_id: {a.ApprovalId}");
                    if (!string.IsNullOrWhiteSpace(a.ToolName))
                    {
                        GUILayout.Label($"tool: {a.ToolName} ({a.RiskTier})");
                    }
                    if (a.ToolArgs != null)
                    {
                        GUILayout.Label($"args: {JsonConvert.SerializeObject(a.ToolArgs, Formatting.None)}");
                    }
                    if (!string.IsNullOrWhiteSpace(a.ExpiresAt))
                    {
                        GUILayout.Label($"expires_at: {a.ExpiresAt}");
                    }
                    GUILayout.EndVertical();
                }
            }

            GUILayout.Space(10);
            GUILayout.Label("Control room: executing tool runs:");
            if (_controlRoomExecutingTools == null || _controlRoomExecutingTools.Length == 0)
            {
                GUILayout.Label("(none)");
            }
            else
            {
                foreach (var r in _controlRoomExecutingTools)
                {
                    if (r == null || string.IsNullOrWhiteSpace(r.ToolRunId))
                    {
                        continue;
                    }

                    GUILayout.BeginVertical("box");
                    GUILayout.Label($"tool_run_id: {r.ToolRunId}");
                    GUILayout.Label($"tool: {r.ToolName} ({r.Status})");
                    if (r.ToolArgs != null)
                    {
                        GUILayout.Label($"args: {JsonConvert.SerializeObject(r.ToolArgs, Formatting.None)}");
                    }
                    if (GUILayout.Button("Cancel"))
                    {
                        StartCoroutine(CancelTool(r.ToolRunId));
                    }
                    GUILayout.EndVertical();
                }
            }

            GUILayout.Space(10);
            GUILayout.Label("Recent audits:");
            if (_controlRoomRecentAudits == null || _controlRoomRecentAudits.Length == 0)
            {
                GUILayout.Label("(none)");
            }
            else
            {
                foreach (var a in _controlRoomRecentAudits)
                {
                    if (a == null || string.IsNullOrWhiteSpace(a.AuditId))
                    {
                        continue;
                    }

                    GUILayout.BeginVertical("box");
                    GUILayout.Label($"{a.EventType} @ {a.CreatedAt}");
                    if (!string.IsNullOrWhiteSpace(a.SessionId))
                    {
                        GUILayout.Label($"session_id: {a.SessionId}");
                    }
                    if (!string.IsNullOrWhiteSpace(a.TurnId))
                    {
                        GUILayout.Label($"turn_id: {a.TurnId}");
                    }
                    if (a.EventData != null)
                    {
                        GUILayout.TextArea(
                            JsonConvert.SerializeObject(a.EventData, Formatting.None),
                            GUILayout.Height(60)
                        );
                    }
                    GUILayout.EndVertical();
                }
            }

            GUILayout.EndScrollView();
        }
    }
}

