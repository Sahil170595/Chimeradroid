using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Chimeradroid.Jarvis;
using UnityEngine;

namespace Chimeradroid
{
    public sealed partial class JarvisOnGuiCompanion
    {
        private string _workflowsStatus = "";
        private float _nextWorkflowDrainAt = 0f;
        private const float WorkflowDrainIntervalSeconds = 5f;

        private WorkflowContinuation SelectedWorkflow
        {
            get
            {
                if (_companionState == null || string.IsNullOrWhiteSpace(_companionState.SelectedWorkflowId))
                {
                    return null;
                }

                return _companionState.Workflows.FirstOrDefault(
                    workflow => workflow != null && workflow.WorkflowId == _companionState.SelectedWorkflowId
                );
            }
        }

        private void UpsertWorkflow(WorkflowContinuation workflow)
        {
            if (_companionState == null || workflow == null || string.IsNullOrWhiteSpace(workflow.WorkflowId))
            {
                return;
            }

            var existing = _companionState.Workflows.FindIndex(
                item => item != null && item.WorkflowId == workflow.WorkflowId
            );
            if (existing >= 0)
            {
                _companionState.Workflows[existing] = workflow;
            }
            else
            {
                _companionState.Workflows.Add(workflow);
            }

            _companionState.Workflows = _companionState.Workflows
                .Where(item => item != null)
                .OrderByDescending(item => item.RequiresAttention)
                .ThenBy(item => item.Title ?? item.WorkflowId)
                .ToList();
            MarkCompanionStateDirty();
        }

        private string EffectiveDeviceId =>
            !string.IsNullOrWhiteSpace(_deviceId)
                ? _deviceId
                : "chimeradroid";

        private IEnumerator RefreshWorkflowInbox()
        {
            var baseUrl = JarvisWeb.NormalizeBaseUrl(BaseUrl);
            if (string.IsNullOrEmpty(baseUrl))
            {
                _workflowsStatus = "missing base url";
                yield break;
            }

            if (string.IsNullOrEmpty(_deviceKey))
            {
                _workflowsStatus = "missing device key";
                yield break;
            }

            _workflowsStatus = "loading workflows...";
            WorkflowInboxResponse resp = null;
            string err = null;
            yield return JarvisWeb.GetJson<WorkflowInboxResponse>(
                $"{baseUrl}/jarvis/v2/mobile/workflows/inbox",
                JarvisWeb.DeviceKeyHeader,
                _deviceKey,
                r => resp = r,
                e => err = e
            );

            if (!string.IsNullOrEmpty(err) || resp == null)
            {
                _workflowsStatus = err ?? "workflow inbox failed";
                yield break;
            }

            _companionState.Workflows = resp.Workflows != null
                ? resp.Workflows.Where(item => item != null).ToList()
                : new List<WorkflowContinuation>();
            if (string.IsNullOrWhiteSpace(_companionState.SelectedWorkflowId) && _companionState.Workflows.Count > 0)
            {
                _companionState.SelectedWorkflowId = _companionState.Workflows[0].WorkflowId;
            }
            _companionState.LastSyncAt = DateTime.UtcNow.ToString("o");
            MarkCompanionStateDirty();
            _workflowsStatus = $"workflows loaded: {_companionState.Workflows.Count}";
        }

        private IEnumerator LoadWorkflowContinuation(string workflowId)
        {
            workflowId = (workflowId ?? "").Trim();
            if (string.IsNullOrEmpty(workflowId))
            {
                _workflowsStatus = "missing workflow_id";
                yield break;
            }

            var baseUrl = JarvisWeb.NormalizeBaseUrl(BaseUrl);
            if (string.IsNullOrEmpty(baseUrl))
            {
                _workflowsStatus = "missing base url";
                yield break;
            }

            if (string.IsNullOrEmpty(_deviceKey))
            {
                _workflowsStatus = "missing device key";
                yield break;
            }

            _workflowsStatus = "loading continuation...";
            WorkflowContinuation resp = null;
            string err = null;
            yield return JarvisWeb.GetJson<WorkflowContinuation>(
                $"{baseUrl}/jarvis/v2/mobile/workflows/{Uri.EscapeDataString(workflowId)}/continuation",
                JarvisWeb.DeviceKeyHeader,
                _deviceKey,
                r => resp = r,
                e => err = e
            );

            if (!string.IsNullOrEmpty(err) || resp == null)
            {
                _workflowsStatus = err ?? "continuation failed";
                yield break;
            }

            _companionState.SelectedWorkflowId = resp.WorkflowId;
            UpsertWorkflow(resp);
            _companionState.LastSyncAt = DateTime.UtcNow.ToString("o");
            MarkCompanionStateDirty();
            _workflowsStatus = $"loaded workflow {resp.WorkflowId}";
        }

        private void QueueWorkflowAction(string workflowId, string actionType, int? stepIdx, Dictionary<string, object> payload = null)
        {
            workflowId = (workflowId ?? "").Trim();
            if (string.IsNullOrEmpty(workflowId))
            {
                _workflowsStatus = "missing workflow_id";
                return;
            }

            var action = new MobileSyncActionRequest
            {
                ActionId = Guid.NewGuid().ToString("N"),
                WorkflowId = workflowId,
                DeviceId = EffectiveDeviceId,
                StepIdx = stepIdx,
                ActionType = actionType,
                OccurredAt = DateTime.UtcNow.ToString("o"),
                Payload = payload ?? new Dictionary<string, object>(),
            };

            _companionState.QueuedActions.Add(action);
            MarkCompanionStateDirty();
            _workflowsStatus = $"queued action: {actionType}";
        }

        private void StageCapture(string workflowId, string artifactKind, int? stepIdx, string retentionClass, Dictionary<string, object> payload)
        {
            workflowId = (workflowId ?? "").Trim();
            if (string.IsNullOrEmpty(workflowId))
            {
                _workflowsStatus = "missing workflow_id";
                return;
            }

            var capture = new CaptureArtifactRequest
            {
                ArtifactId = Guid.NewGuid().ToString("N"),
                WorkflowId = workflowId,
                StepIdx = stepIdx,
                DeviceId = EffectiveDeviceId,
                ArtifactKind = artifactKind,
                RetentionClass = retentionClass,
                SemanticCandidate = string.Equals(retentionClass, "semantic_candidate", StringComparison.OrdinalIgnoreCase),
                Mime = "application/json",
                Payload = payload ?? new Dictionary<string, object>(),
                CapturedAt = DateTime.UtcNow.ToString("o"),
            };

            _companionState.StagedCaptures.Add(capture);
            MarkCompanionStateDirty();
            _workflowsStatus = $"staged capture: {artifactKind}";
        }

        private void QueueLearningEvent(string workflowId, string eventType, Dictionary<string, object> payload = null)
        {
            var evt = new LearningEventRequest
            {
                EventId = Guid.NewGuid().ToString("N"),
                WorkflowId = workflowId,
                SessionId = _sessionId,
                TurnId = _lastTurnId,
                DeviceId = EffectiveDeviceId,
                EventType = eventType,
                Payload = payload ?? new Dictionary<string, object>(),
                OccurredAt = DateTime.UtcNow.ToString("o"),
            };
            _companionState.EmittedLearningEvents.Add(evt);
            MarkCompanionStateDirty();
        }

        private void MaybeDrainWorkflowQueue()
        {
            if (_companionState == null || string.IsNullOrEmpty(_deviceKey))
            {
                return;
            }

            if ((_companionState.QueuedActions == null || _companionState.QueuedActions.Count == 0)
                && (_companionState.StagedCaptures == null || _companionState.StagedCaptures.Count == 0)
                && (_companionState.EmittedLearningEvents == null || _companionState.EmittedLearningEvents.Count == 0))
            {
                return;
            }

            if (Time.realtimeSinceStartup < _nextWorkflowDrainAt)
            {
                return;
            }

            _nextWorkflowDrainAt = Time.realtimeSinceStartup + WorkflowDrainIntervalSeconds;
            StartCoroutine(DrainWorkflowQueue());
        }

        private IEnumerator DrainWorkflowQueue()
        {
            var baseUrl = JarvisWeb.NormalizeBaseUrl(BaseUrl);
            if (string.IsNullOrEmpty(baseUrl) || string.IsNullOrEmpty(_deviceKey))
            {
                yield break;
            }

            if (_companionState.QueuedActions != null && _companionState.QueuedActions.Count > 0)
            {
                MobileSyncBatchResponse batchResp = null;
                string batchErr = null;
                yield return JarvisWeb.PostJson<MobileSyncBatchRequest, MobileSyncBatchResponse>(
                    $"{baseUrl}/jarvis/v2/mobile/sync/batch",
                    new MobileSyncBatchRequest
                    {
                        Actions = _companionState.QueuedActions.ToArray(),
                        ClientCursor = null,
                    },
                    JarvisWeb.DeviceKeyHeader,
                    _deviceKey,
                    r => batchResp = r,
                    e => batchErr = e
                );

                if (string.IsNullOrEmpty(batchErr) && batchResp != null && batchResp.Results != null)
                {
                    var acceptedOrDuplicate = new HashSet<string>(
                        batchResp.Results
                            .Where(result => result != null && (result.Status == "accepted" || result.Status == "duplicate"))
                            .Select(result => result.ActionId)
                    );
                    _companionState.QueuedActions.RemoveAll(action => acceptedOrDuplicate.Contains(action.ActionId));
                    MarkCompanionStateDirty();
                }
            }

            if (_companionState.StagedCaptures != null && _companionState.StagedCaptures.Count > 0)
            {
                var completedCaptureIds = new List<string>();
                foreach (var capture in _companionState.StagedCaptures)
                {
                    CaptureArtifactResponse captureResp = null;
                    string captureErr = null;
                    yield return JarvisWeb.PostJson<CaptureArtifactRequest, CaptureArtifactResponse>(
                        $"{baseUrl}/jarvis/v2/mobile/workflows/{Uri.EscapeDataString(capture.WorkflowId)}/captures",
                        capture,
                        JarvisWeb.DeviceKeyHeader,
                        _deviceKey,
                        r => captureResp = r,
                        e => captureErr = e
                    );

                    if (string.IsNullOrEmpty(captureErr) && captureResp != null
                        && (captureResp.Status == "attached" || captureResp.Status == "duplicate"))
                    {
                        completedCaptureIds.Add(capture.ArtifactId);
                    }
                }

                if (completedCaptureIds.Count > 0)
                {
                    _companionState.StagedCaptures.RemoveAll(
                        capture => completedCaptureIds.Contains(capture.ArtifactId)
                    );
                    MarkCompanionStateDirty();
                }
            }

            if (_companionState.EmittedLearningEvents != null && _companionState.EmittedLearningEvents.Count > 0)
            {
                var completedEventIds = new List<string>();
                foreach (var evt in _companionState.EmittedLearningEvents)
                {
                    LearningEventResponse resp = null;
                    string err = null;
                    yield return JarvisWeb.PostJson<LearningEventRequest, LearningEventResponse>(
                        $"{baseUrl}/jarvis/v2/learning/events",
                        evt,
                        JarvisWeb.DeviceKeyHeader,
                        _deviceKey,
                        r => resp = r,
                        e => err = e
                    );

                    if (string.IsNullOrEmpty(err) && resp != null
                        && (resp.Status == "accepted" || resp.Status == "duplicate"))
                    {
                        completedEventIds.Add(evt.EventId);
                    }
                }

                if (completedEventIds.Count > 0)
                {
                    _companionState.EmittedLearningEvents.RemoveAll(
                        evt => completedEventIds.Contains(evt.EventId)
                    );
                    MarkCompanionStateDirty();
                }
            }

            _companionState.LastDrainAt = DateTime.UtcNow.ToString("o");
            MarkCompanionStateDirty();

            if (!string.IsNullOrWhiteSpace(_companionState.SelectedWorkflowId))
            {
                yield return LoadWorkflowContinuation(_companionState.SelectedWorkflowId);
            }
            else
            {
                yield return RefreshWorkflowInbox();
            }
        }
    }
}
