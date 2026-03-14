using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Chimeradroid.Jarvis
{
    [Serializable]
    public sealed class WorkflowInboxResponse
    {
        [JsonProperty("workflows")] public WorkflowContinuation[] Workflows;
        [JsonProperty("next_cursor")] public string NextCursor;
    }

    [Serializable]
    public sealed class WorkflowContinuation
    {
        [JsonProperty("schema_version")] public string SchemaVersion;
        [JsonProperty("workflow_id")] public string WorkflowId;
        [JsonProperty("turn_id")] public string TurnId;
        [JsonProperty("session_id")] public string SessionId;
        [JsonProperty("title")] public string Title;
        [JsonProperty("status")] public string Status;
        [JsonProperty("workflow_version")] public int WorkflowVersion;
        [JsonProperty("requires_attention")] public bool RequiresAttention;
        [JsonProperty("pending_actions")] public WorkflowPendingAction[] PendingActions;
        [JsonProperty("pending_approvals")] public WorkflowPendingApproval[] PendingApprovals;
        [JsonProperty("capture_requests")] public WorkflowCaptureRequest[] CaptureRequests;
        [JsonProperty("recent_events")] public WorkflowRecentEvent[] RecentEvents;
    }

    [Serializable]
    public sealed class WorkflowPendingAction
    {
        [JsonProperty("action_kind")] public string ActionKind;
        [JsonProperty("label")] public string Label;
        [JsonProperty("step_idx")] public int? StepIdx;
    }

    [Serializable]
    public sealed class WorkflowPendingApproval
    {
        [JsonProperty("step_idx")] public int StepIdx;
        [JsonProperty("intent")] public string Intent;
        [JsonProperty("tool_name")] public string ToolName;
        [JsonProperty("agent_alias")] public string AgentAlias;
    }

    [Serializable]
    public sealed class WorkflowCaptureRequest
    {
        [JsonProperty("step_idx")] public int StepIdx;
        [JsonProperty("intent")] public string Intent;
        [JsonProperty("agent_alias")] public string AgentAlias;
    }

    [Serializable]
    public sealed class WorkflowRecentEvent
    {
        [JsonProperty("event_id")] public string EventId;
        [JsonProperty("workflow_version")] public int WorkflowVersion;
        [JsonProperty("event_type")] public string EventType;
        [JsonProperty("payload")] public object Payload;
        [JsonProperty("created_at")] public string CreatedAt;
    }

    [Serializable]
    public sealed class MobileSyncActionRequest
    {
        [JsonProperty("schema_version")] public string SchemaVersion = "jarvis.mobile_sync_action.v1";
        [JsonProperty("action_id")] public string ActionId;
        [JsonProperty("workflow_id")] public string WorkflowId;
        [JsonProperty("device_id")] public string DeviceId;
        [JsonProperty("step_idx")] public int? StepIdx;
        [JsonProperty("action_type")] public string ActionType;
        [JsonProperty("occurred_at")] public string OccurredAt;
        [JsonProperty("payload")] public Dictionary<string, object> Payload = new Dictionary<string, object>();
    }

    [Serializable]
    public sealed class MobileSyncActionResponse
    {
        [JsonProperty("workflow_id")] public string WorkflowId;
        [JsonProperty("action_id")] public string ActionId;
        [JsonProperty("status")] public string Status;
        [JsonProperty("reason_code")] public string ReasonCode;
        [JsonProperty("workflow_version")] public int? WorkflowVersion;
    }

    [Serializable]
    public sealed class MobileSyncBatchRequest
    {
        [JsonProperty("actions")] public MobileSyncActionRequest[] Actions;
        [JsonProperty("client_cursor")] public string ClientCursor;
    }

    [Serializable]
    public sealed class MobileSyncBatchResponse
    {
        [JsonProperty("results")] public MobileSyncActionResult[] Results;
    }

    [Serializable]
    public sealed class MobileSyncActionResult
    {
        [JsonProperty("action_id")] public string ActionId;
        [JsonProperty("status")] public string Status;
        [JsonProperty("reason_code")] public string ReasonCode;
        [JsonProperty("workflow_version")] public int? WorkflowVersion;
    }

    [Serializable]
    public sealed class CaptureArtifactRequest
    {
        [JsonProperty("schema_version")] public string SchemaVersion = "jarvis.capture_artifact.v1";
        [JsonProperty("artifact_id")] public string ArtifactId;
        [JsonProperty("workflow_id")] public string WorkflowId;
        [JsonProperty("step_idx")] public int? StepIdx;
        [JsonProperty("device_id")] public string DeviceId;
        [JsonProperty("artifact_kind")] public string ArtifactKind;
        [JsonProperty("retention_class")] public string RetentionClass;
        [JsonProperty("semantic_candidate")] public bool SemanticCandidate;
        [JsonProperty("mime")] public string Mime;
        [JsonProperty("payload")] public Dictionary<string, object> Payload = new Dictionary<string, object>();
        [JsonProperty("captured_at")] public string CapturedAt;
    }

    [Serializable]
    public sealed class CaptureArtifactResponse
    {
        [JsonProperty("workflow_id")] public string WorkflowId;
        [JsonProperty("artifact_id")] public string ArtifactId;
        [JsonProperty("status")] public string Status;
        [JsonProperty("metadata")] public CaptureArtifactMetadata Metadata;
    }

    [Serializable]
    public sealed class CaptureArtifactMetadata
    {
        [JsonProperty("artifact_kind")] public string ArtifactKind;
        [JsonProperty("retention_class")] public string RetentionClass;
        [JsonProperty("source_device_id")] public string SourceDeviceId;
        [JsonProperty("semantic_candidate")] public bool SemanticCandidate;
        [JsonProperty("captured_at")] public string CapturedAt;
    }

    [Serializable]
    public sealed class LearningEventRequest
    {
        [JsonProperty("schema_version")] public string SchemaVersion = "jarvis.learning_event.v1";
        [JsonProperty("event_id")] public string EventId;
        [JsonProperty("workflow_id")] public string WorkflowId;
        [JsonProperty("session_id")] public string SessionId;
        [JsonProperty("turn_id")] public string TurnId;
        [JsonProperty("device_id")] public string DeviceId;
        [JsonProperty("event_type")] public string EventType;
        [JsonProperty("payload")] public Dictionary<string, object> Payload = new Dictionary<string, object>();
        [JsonProperty("occurred_at")] public string OccurredAt;
    }

    [Serializable]
    public sealed class LearningEventResponse
    {
        [JsonProperty("learning_event_id")] public string LearningEventId;
        [JsonProperty("status")] public string Status;
    }

    [Serializable]
    public sealed class CompanionLocalState
    {
        [JsonProperty("last_sync_at")] public string LastSyncAt;
        [JsonProperty("last_drain_at")] public string LastDrainAt;
        [JsonProperty("selected_workflow_id")] public string SelectedWorkflowId;
        [JsonProperty("workflows")] public List<WorkflowContinuation> Workflows = new List<WorkflowContinuation>();
        [JsonProperty("queued_actions")] public List<MobileSyncActionRequest> QueuedActions = new List<MobileSyncActionRequest>();
        [JsonProperty("staged_captures")] public List<CaptureArtifactRequest> StagedCaptures = new List<CaptureArtifactRequest>();
        [JsonProperty("emitted_learning_events")] public List<LearningEventRequest> EmittedLearningEvents = new List<LearningEventRequest>();
    }
}
