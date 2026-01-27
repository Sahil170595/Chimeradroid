using System;
using Newtonsoft.Json;

namespace Chimeradroid.Jarvis
{
    [Serializable]
    public sealed class DeviceRegisterRequest
    {
        [JsonProperty("name")] public string Name;
    }

    [Serializable]
    public sealed class DeviceRegisterResponse
    {
        [JsonProperty("device_id")] public string DeviceId;
        [JsonProperty("name")] public string Name;
        [JsonProperty("device_key")] public string DeviceKey;
        [JsonProperty("created_at")] public string CreatedAt;
    }

    [Serializable]
    public sealed class ChatRequest
    {
        [JsonProperty("idempotency_key")] public string IdempotencyKey;
        [JsonProperty("message")] public string Message;
        [JsonProperty("session_id")] public string SessionId;
        [JsonProperty("mode")] public string Mode = "sync";
        [JsonProperty("wait_ms")] public int? WaitMs = 10000;
    }

    [Serializable]
    public sealed class ChatResponse
    {
        [JsonProperty("turn_id")] public string TurnId;
        [JsonProperty("session_id")] public string SessionId;
        [JsonProperty("status")] public string Status;
        [JsonProperty("turn_status")] public string TurnStatus;
        [JsonProperty("stream_url")] public string StreamUrl;
        [JsonProperty("final_response")] public string FinalResponse;
        [JsonProperty("still_running")] public bool? StillRunning;
    }

    [Serializable]
    public sealed class TurnResponse
    {
        [JsonProperty("turn_id")] public string TurnId;
        [JsonProperty("session_id")] public string SessionId;
        [JsonProperty("status")] public string Status;
        [JsonProperty("current_step")] public string CurrentStep;
        [JsonProperty("eta_seconds")] public int? EtaSeconds;
        [JsonProperty("final_response")] public string FinalResponse;
    }

    [Serializable]
    public sealed class SessionHandoffRedeemRequest
    {
        [JsonProperty("token")] public string Token;
    }

    [Serializable]
    public sealed class SessionHandoffRedeemResponse
    {
        [JsonProperty("handoff_id")] public string HandoffId;
        [JsonProperty("session_id")] public string SessionId;
        [JsonProperty("stream_url")] public string StreamUrl;
    }

    [Serializable]
    public sealed class SessionTimelineResponse
    {
        [JsonProperty("session")] public TimelineSession Session;
        [JsonProperty("stream_url")] public string StreamUrl;
        [JsonProperty("turns")] public TimelineTurn[] Turns;
        [JsonProperty("messages")] public TimelineMessage[] Messages;
    }

    [Serializable]
    public sealed class SessionsListResponse
    {
        [JsonProperty("sessions")] public SessionSummary[] Sessions;
    }

    [Serializable]
    public sealed class SessionSummary
    {
        [JsonProperty("session_id")] public string SessionId;
        [JsonProperty("created_at")] public string CreatedAt;
        [JsonProperty("last_activity_at")] public string LastActivityAt;
        [JsonProperty("stream_url")] public string StreamUrl;
        [JsonProperty("last_turn_id")] public string LastTurnId;
        [JsonProperty("last_turn_status")] public string LastTurnStatus;
        [JsonProperty("turns_count")] public int TurnsCount;
        [JsonProperty("messages_count")] public int MessagesCount;
    }

    [Serializable]
    public sealed class TimelineSession
    {
        [JsonProperty("session_id")] public string SessionId;
        [JsonProperty("created_at")] public string CreatedAt;
    }

    [Serializable]
    public sealed class TimelineTurn
    {
        [JsonProperty("turn_id")] public string TurnId;
        [JsonProperty("status")] public string Status;
        [JsonProperty("current_step")] public string CurrentStep;
        [JsonProperty("message")] public string Message;
        [JsonProperty("final_response")] public string FinalResponse;
        [JsonProperty("created_at")] public string CreatedAt;
        [JsonProperty("updated_at")] public string UpdatedAt;
    }

    [Serializable]
    public sealed class TimelineMessage
    {
        [JsonProperty("message_id")] public string MessageId;
        [JsonProperty("turn_id")] public string TurnId;
        [JsonProperty("role")] public string Role;
        [JsonProperty("content")] public string Content;
        [JsonProperty("created_at")] public string CreatedAt;
    }

    [Serializable]
    public sealed class ToolApproveRequest
    {
        [JsonProperty("approval_id")] public string ApprovalId;
        [JsonProperty("tool_run_id")] public string ToolRunId;
    }

    [Serializable]
    public sealed class ToolApproveResponse
    {
        [JsonProperty("approval_id")] public string ApprovalId;
        [JsonProperty("tool_name")] public string ToolName;
        [JsonProperty("risk_tier")] public string RiskTier;
        [JsonProperty("expires_at")] public string ExpiresAt;
        [JsonProperty("approved")] public bool Approved;
    }

    [Serializable]
    public sealed class ToolExecuteRequest
    {
        [JsonProperty("tool_run_id")] public string ToolRunId;
        [JsonProperty("approval_id")] public string ApprovalId;
    }

    [Serializable]
    public sealed class ToolExecuteResponse
    {
        [JsonProperty("tool_run_id")] public string ToolRunId;
        [JsonProperty("status")] public string Status;
        [JsonProperty("ok")] public bool? Ok;
        [JsonProperty("exit_code")] public int? ExitCode;
        [JsonProperty("stdout")] public string Stdout;
        [JsonProperty("stderr")] public string Stderr;
    }

    [Serializable]
    public sealed class VoiceConverseRequest
    {
        [JsonProperty("session_id")] public string SessionId;
        [JsonProperty("text")] public string Text;
        [JsonProperty("mode")] public string Mode = "sync";
        [JsonProperty("wait_ms")] public int? WaitMs = 20000;
        [JsonProperty("voice_mode")] public string VoiceMode;
    }

    [Serializable]
    public sealed class VoiceConverseResponse
    {
        [JsonProperty("ok")] public bool Ok;
        [JsonProperty("ignored")] public bool? Ignored;
        [JsonProperty("session_id")] public string SessionId;
        [JsonProperty("turn_id")] public string TurnId;
        [JsonProperty("stream_url")] public string StreamUrl;
        [JsonProperty("interrupt_epoch")] public int? InterruptEpoch;
        [JsonProperty("still_running")] public bool? StillRunning;
        [JsonProperty("transcribed_text")] public string TranscribedText;
        [JsonProperty("message_text")] public string MessageText;
        [JsonProperty("response_text")] public string ResponseText;
        [JsonProperty("response_audio_b64")] public string ResponseAudioB64;
        [JsonProperty("voice_mode_requested")] public string VoiceModeRequested;
        [JsonProperty("voice_mode_effective")] public string VoiceModeEffective;
    }

    [Serializable]
    public sealed class WhoamiResponse
    {
        [JsonProperty("auth_type")] public string AuthType;
        [JsonProperty("user_id")] public string UserId;
        [JsonProperty("device_id")] public string DeviceId;
    }

    [Serializable]
    public sealed class MeshPushRequest
    {
        [JsonProperty("event_type")] public string EventType;
        [JsonProperty("event_data")] public object EventData;
    }

    [Serializable]
    public sealed class MeshPushResponse
    {
        [JsonProperty("seq")] public int Seq;
    }

    [Serializable]
    public sealed class MeshEvent
    {
        [JsonProperty("seq")] public int Seq;
        [JsonProperty("source_device_id")] public string SourceDeviceId;
        [JsonProperty("event_type")] public string EventType;
        [JsonProperty("event_data")] public object EventData;
        [JsonProperty("created_at")] public string CreatedAt;
    }

    [Serializable]
    public sealed class MeshPullResponse
    {
        [JsonProperty("events")] public MeshEvent[] Events;
    }

    [Serializable]
    public sealed class Notification
    {
        [JsonProperty("notification_id")] public string NotificationId;
        [JsonProperty("trigger_type")] public string TriggerType;
        [JsonProperty("content")] public string Content;
        [JsonProperty("read_at")] public string ReadAt;
        [JsonProperty("created_at")] public string CreatedAt;
    }

    [Serializable]
    public sealed class NotificationsListResponse
    {
        [JsonProperty("notifications")] public Notification[] Notifications;
    }

    [Serializable]
    public sealed class ProactiveStatusResponse
    {
        [JsonProperty("enabled")] public bool Enabled;
        [JsonProperty("triggers")] public string[] Triggers;
        [JsonProperty("consents")] public System.Collections.Generic.Dictionary<string, bool> Consents;
    }

    [Serializable]
    public sealed class ProactiveConsentsPatchRequest
    {
        [JsonProperty("consents")] public System.Collections.Generic.Dictionary<string, bool> Consents;
    }

    [Serializable]
    public sealed class ApprovalSummary
    {
        [JsonProperty("approval_id")] public string ApprovalId;
        [JsonProperty("session_id")] public string SessionId;
        [JsonProperty("turn_id")] public string TurnId;
        [JsonProperty("tool_name")] public string ToolName;
        [JsonProperty("tool_args")] public object ToolArgs;
        [JsonProperty("required_scopes")] public string[] RequiredScopes;
        [JsonProperty("risk_tier")] public string RiskTier;
        [JsonProperty("template_version")] public string TemplateVersion;
        [JsonProperty("interrupt_epoch")] public int? InterruptEpoch;
        [JsonProperty("status")] public string Status;
        [JsonProperty("created_at")] public string CreatedAt;
        [JsonProperty("expires_at")] public string ExpiresAt;
        [JsonProperty("revoked_at")] public string RevokedAt;
        [JsonProperty("requested_by")] public string RequestedBy;
        [JsonProperty("approved_by")] public string ApprovedBy;
        [JsonProperty("approved_at")] public string ApprovedAt;
        [JsonProperty("trace_id")] public string TraceId;
    }

    [Serializable]
    public sealed class ApprovalsListResponse
    {
        [JsonProperty("approvals")] public ApprovalSummary[] Approvals;
    }

    [Serializable]
    public sealed class ToolRunSummary
    {
        [JsonProperty("tool_run_id")] public string ToolRunId;
        [JsonProperty("session_id")] public string SessionId;
        [JsonProperty("turn_id")] public string TurnId;
        [JsonProperty("tool_name")] public string ToolName;
        [JsonProperty("tool_args")] public object ToolArgs;
        [JsonProperty("status")] public string Status;
        [JsonProperty("created_at")] public string CreatedAt;
        [JsonProperty("updated_at")] public string UpdatedAt;
    }

    [Serializable]
    public sealed class AuditSummary
    {
        [JsonProperty("audit_id")] public string AuditId;
        [JsonProperty("session_id")] public string SessionId;
        [JsonProperty("turn_id")] public string TurnId;
        [JsonProperty("event_type")] public string EventType;
        [JsonProperty("event_data")] public object EventData;
        [JsonProperty("created_at")] public string CreatedAt;
    }

    [Serializable]
    public sealed class ControlRoomResponse
    {
        [JsonProperty("pending_approvals")] public ApprovalSummary[] PendingApprovals;
        [JsonProperty("executing_tool_runs")] public ToolRunSummary[] ExecutingToolRuns;
        [JsonProperty("recent_audits")] public AuditSummary[] RecentAudits;
        [JsonProperty("trace_id")] public string TraceId;
        [JsonProperty("traceparent")] public string Traceparent;
    }

    [Serializable]
    public sealed class SystemStatusResponse
    {
        [JsonProperty("status")] public string Status;
        [JsonProperty("degraded_states")] public string[] DegradedStates;
        [JsonProperty("dependencies")] public object Dependencies;
    }

    [Serializable]
    public sealed class StreamHello
    {
        [JsonProperty("type")] public string Type = "client.hello";
        [JsonProperty("token")] public string Token;
        [JsonProperty("session_id")] public string SessionId;
    }
}
