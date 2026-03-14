using System;
using System.Collections.Generic;
using Chimeradroid.Jarvis;
using UnityEngine;

namespace Chimeradroid
{
    public sealed partial class JarvisOnGuiCompanion
    {
        private Vector2 _workflowInboxScroll;
        private Vector2 _workflowDetailScroll;
        private string _workflowNoteDraft = "";
        private string _workflowImageRefDraft = "";
        private string _workflowAudioRefDraft = "";

        private void RenderWorkflowsTab()
        {
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Refresh Inbox"))
            {
                StartCoroutine(RefreshWorkflowInbox());
            }
            if (!string.IsNullOrWhiteSpace(_companionState?.SelectedWorkflowId) && GUILayout.Button("Refresh Selected"))
            {
                StartCoroutine(LoadWorkflowContinuation(_companionState.SelectedWorkflowId));
            }
            if (GUILayout.Button("Drain Queue Now"))
            {
                StartCoroutine(DrainWorkflowQueue());
            }
            GUILayout.EndHorizontal();

            if (!string.IsNullOrEmpty(_workflowsStatus))
            {
                GUILayout.Label(_workflowsStatus);
            }

            GUILayout.Label(
                $"queued_actions={_companionState?.QueuedActions?.Count ?? 0}  "
                + $"staged_captures={_companionState?.StagedCaptures?.Count ?? 0}  "
                + $"learning_events={_companionState?.EmittedLearningEvents?.Count ?? 0}"
            );

            GUILayout.BeginHorizontal();

            GUILayout.BeginVertical(GUILayout.Width(220));
            GUILayout.Label("Workflow Inbox");
            _workflowInboxScroll = GUILayout.BeginScrollView(_workflowInboxScroll, GUILayout.Height(620));
            if (_companionState == null || _companionState.Workflows == null || _companionState.Workflows.Count == 0)
            {
                GUILayout.Label("(no workflows cached)");
            }
            else
            {
                foreach (var workflow in _companionState.Workflows)
                {
                    if (workflow == null || string.IsNullOrWhiteSpace(workflow.WorkflowId))
                    {
                        continue;
                    }

                    GUILayout.BeginVertical("box");
                    GUILayout.Label(workflow.Title ?? workflow.WorkflowId);
                    GUILayout.Label($"status: {workflow.Status}");
                    GUILayout.Label($"version: {workflow.WorkflowVersion}");
                    if (workflow.RequiresAttention)
                    {
                        GUILayout.Label("requires attention");
                    }
                    if (GUILayout.Button("Open"))
                    {
                        _companionState.SelectedWorkflowId = workflow.WorkflowId;
                        MarkCompanionStateDirty();
                        StartCoroutine(LoadWorkflowContinuation(workflow.WorkflowId));
                    }
                    GUILayout.EndVertical();
                }
            }
            GUILayout.EndScrollView();
            GUILayout.EndVertical();

            GUILayout.BeginVertical();
            var workflowDetail = SelectedWorkflow;
            if (workflowDetail == null)
            {
                GUILayout.Label("Select a workflow from the inbox.");
            }
            else
            {
                GUILayout.Label($"Workflow: {workflowDetail.Title ?? workflowDetail.WorkflowId}");
                GUILayout.Label($"workflow_id: {workflowDetail.WorkflowId}");
                GUILayout.Label($"status: {workflowDetail.Status} | version: {workflowDetail.WorkflowVersion}");
                if (!string.IsNullOrWhiteSpace(workflowDetail.SessionId))
                {
                    GUILayout.Label($"session_id: {workflowDetail.SessionId}");
                }
                if (!string.IsNullOrWhiteSpace(workflowDetail.TurnId))
                {
                    GUILayout.Label($"turn_id: {workflowDetail.TurnId}");
                }

                _workflowDetailScroll = GUILayout.BeginScrollView(_workflowDetailScroll, GUILayout.Height(560));

                GUILayout.Label("Pending actions:");
                if (workflowDetail.PendingActions == null || workflowDetail.PendingActions.Length == 0)
                {
                    GUILayout.Label("(none)");
                }
                else
                {
                    foreach (var action in workflowDetail.PendingActions)
                    {
                        GUILayout.BeginHorizontal("box");
                        GUILayout.Label(action.Label);
                        if (GUILayout.Button("Queue", GUILayout.Width(80)))
                        {
                            QueueWorkflowAction(
                                workflowDetail.WorkflowId,
                                action.ActionKind,
                                action.StepIdx,
                                new Dictionary<string, object>()
                            );
                        }
                        GUILayout.EndHorizontal();
                    }
                }

                GUILayout.Space(8);
                GUILayout.Label("Pending approvals:");
                if (workflowDetail.PendingApprovals == null || workflowDetail.PendingApprovals.Length == 0)
                {
                    GUILayout.Label("(none)");
                }
                else
                {
                    foreach (var approval in workflowDetail.PendingApprovals)
                    {
                        GUILayout.BeginVertical("box");
                        GUILayout.Label($"step {approval.StepIdx}: {approval.Intent}");
                        if (!string.IsNullOrWhiteSpace(approval.ToolName))
                        {
                            GUILayout.Label($"tool: {approval.ToolName}");
                        }
                        GUILayout.BeginHorizontal();
                        if (GUILayout.Button("Approve"))
                        {
                            QueueWorkflowAction(workflowDetail.WorkflowId, "approval.accept", approval.StepIdx);
                        }
                        if (GUILayout.Button("Defer"))
                        {
                            QueueWorkflowAction(workflowDetail.WorkflowId, "approval.defer", approval.StepIdx);
                            QueueLearningEvent(
                                workflowDetail.WorkflowId,
                                "approval_deferred",
                                new Dictionary<string, object> { { "step_idx", approval.StepIdx } }
                            );
                        }
                        if (GUILayout.Button("Reject"))
                        {
                            QueueWorkflowAction(workflowDetail.WorkflowId, "approval.reject", approval.StepIdx);
                            QueueLearningEvent(
                                workflowDetail.WorkflowId,
                                "approval_rejected",
                                new Dictionary<string, object> { { "step_idx", approval.StepIdx } }
                            );
                        }
                        GUILayout.EndHorizontal();
                        GUILayout.EndVertical();
                    }
                }

                GUILayout.Space(8);
                GUILayout.Label("Capture requests:");
                if (workflowDetail.CaptureRequests == null || workflowDetail.CaptureRequests.Length == 0)
                {
                    GUILayout.Label("(none)");
                }
                else
                {
                    foreach (var capture in workflowDetail.CaptureRequests)
                    {
                        GUILayout.Label($"step {capture.StepIdx}: {capture.Intent}");
                    }
                }

                GUILayout.Space(8);
                GUILayout.Label("Quick capture: note");
                _workflowNoteDraft = GUILayout.TextArea(_workflowNoteDraft, GUILayout.Height(70));
                if (GUILayout.Button("Stage Note Capture"))
                {
                    StageCapture(
                        workflowDetail.WorkflowId,
                        "note",
                        workflowDetail.CaptureRequests != null && workflowDetail.CaptureRequests.Length > 0
                            ? workflowDetail.CaptureRequests[0].StepIdx
                            : (int?)null,
                        "retained",
                        new Dictionary<string, object> { { "note_text", _workflowNoteDraft } }
                    );
                    _workflowNoteDraft = "";
                }

                GUILayout.Label("Quick capture: image ref");
                _workflowImageRefDraft = GUILayout.TextField(_workflowImageRefDraft);
                if (GUILayout.Button("Stage Image Capture"))
                {
                    StageCapture(
                        workflowDetail.WorkflowId,
                        "image",
                        workflowDetail.CaptureRequests != null && workflowDetail.CaptureRequests.Length > 0
                            ? workflowDetail.CaptureRequests[0].StepIdx
                            : (int?)null,
                        "semantic_candidate",
                        new Dictionary<string, object> { { "image_ref", _workflowImageRefDraft } }
                    );
                    _workflowImageRefDraft = "";
                }

                GUILayout.Label("Quick capture: audio ref");
                _workflowAudioRefDraft = GUILayout.TextField(_workflowAudioRefDraft);
                if (GUILayout.Button("Stage Audio Capture"))
                {
                    StageCapture(
                        workflowDetail.WorkflowId,
                        "audio",
                        workflowDetail.CaptureRequests != null && workflowDetail.CaptureRequests.Length > 0
                            ? workflowDetail.CaptureRequests[0].StepIdx
                            : (int?)null,
                        "retained",
                        new Dictionary<string, object> { { "audio_ref", _workflowAudioRefDraft } }
                    );
                    _workflowAudioRefDraft = "";
                }

                if (GUILayout.Button("Stage Location Snapshot"))
                {
                    StageCapture(
                        workflowDetail.WorkflowId,
                        "location",
                        workflowDetail.CaptureRequests != null && workflowDetail.CaptureRequests.Length > 0
                            ? workflowDetail.CaptureRequests[0].StepIdx
                            : (int?)null,
                        "retained",
                        new Dictionary<string, object>
                        {
                            { "latitude", Input.location.lastData.latitude },
                            { "longitude", Input.location.lastData.longitude },
                            { "altitude", Input.location.lastData.altitude }
                        }
                    );
                }

                GUILayout.Space(8);
                GUILayout.Label("Recent events:");
                if (workflowDetail.RecentEvents == null || workflowDetail.RecentEvents.Length == 0)
                {
                    GUILayout.Label("(none)");
                }
                else
                {
                    foreach (var evt in workflowDetail.RecentEvents)
                    {
                        GUILayout.BeginVertical("box");
                        GUILayout.Label($"{evt.EventType} @ v{evt.WorkflowVersion}");
                        GUILayout.Label(evt.CreatedAt);
                        GUILayout.EndVertical();
                    }
                }

                GUILayout.EndScrollView();
            }
            GUILayout.EndVertical();

            GUILayout.EndHorizontal();
        }
    }
}
