# Chimeradroid

Unity Android client ("body") for your JARVIS AI stack (Banterpacks). A production-ready mobile companion that connects Android devices to the JARVIS v2 AI Gateway, enabling voice interactions, tool approvals, session management, and real-time streaming.

[![Unity](https://img.shields.io/badge/Unity-6000.0+-blue.svg)](https://unity.com/)
[![License](https://img.shields.io/badge/license-See%20LICENSE-lightgrey.svg)](LICENSE)
[![Platform](https://img.shields.io/badge/platform-Android-green.svg)](https://www.android.com/)

---

## Table of Contents

- [Overview](#overview)
- [Architecture](#architecture)
- [Features](#features)
- [Prerequisites](#prerequisites)
- [Quick Start](#quick-start)
- [Setup & Installation](#setup--installation)
- [Configuration](#configuration)
- [Usage](#usage)
- [API Integration](#api-integration)
- [Embardiment Integration](#embardiment-integration)
- [Development](#development)
- [Troubleshooting](#troubleshooting)
- [Related Projects](#related-projects)

---

## Overview

**Chimeradroid** is a Unity-based Android client that serves as the mobile interface ("body") for the JARVIS AI stack. It provides a complete mobile companion experience with:

- **Voice interactions** via Embardiment's Android ASR/TTS
- **Real-time streaming** of AI responses
- **Tool approval workflows** for secure AI tool execution
- **Session management** for conversation continuity
- **Mesh networking** for cross-device visibility
- **Proactive notifications** for AI-driven alerts

### Project Context

Chimeradroid is part of a larger ecosystem:

1. **Banterpacks** (`../Banterpacks`) - The JARVIS v2 AI Gateway backend
   - Unified AI orchestration platform
   - Voice pipeline (STT/TTS) with barge-in support
   - Tool execution via TDD005 sandbox
   - Memory & fact management
   - Proactive notification system
   - Real-time event streaming via WebSocket
   - Production-ready with 95.5% test pass rate

2. **Embardiment** (`../embardiment`) - Unity package for Android XR AI
   - Apache-2.0 licensed Unity package
   - Native Android ASR (Automatic Speech Recognition)
   - Native Android TTS (Text-to-Speech)
   - Cloud Gemini API integration
   - OCR capabilities
   - LLM integration

3. **Chimeradroid** (this project) - The mobile client
   - Based on Embardiment's `examples/android-mobile` template
   - Connects to JARVIS v2 Gateway at `/jarvis/v2/*`
   - Auto-spawns IMGUI companion UI
   - Full JARVIS API integration

---

## Architecture

```text
┌─────────────────────────────────────────────────────────────┐
│                    Chimeradroid (Android)                    │
│  ┌──────────────────────────────────────────────────────┐   │
│  │  JarvisOnGuiCompanion (Unity MonoBehaviour)          │   │
│  │  ├── Chat Tab (text/voice input)                    │   │
│  │  ├── Voice Tab (ASR/TTS integration)                 │   │
│  │  ├── Sessions Tab (conversation history)             │   │
│  │  ├── Tools Tab (approval workflows)                 │   │
│  │  ├── Handoff Tab (session handoff)                  │   │
│  │  ├── Mesh Tab (cross-device events)                 │   │
│  │  ├── Notifications Tab (proactive alerts)           │   │
│  │  └── Settings Tab (configuration)                   │   │
│  └──────────────────────────────────────────────────────┘   │
│  ┌──────────────────────────────────────────────────────┐   │
│  │  Embardiment Integration (Optional)                  │   │
│  │  ├── AndroidAsr (native speech recognition)          │   │
│  │  ├── AndroidTts (native text-to-speech)              │   │
│  │  └── GeminiTts (cloud TTS fallback)                   │   │
│  └──────────────────────────────────────────────────────┘   │
│  ┌──────────────────────────────────────────────────────┐   │
│  │  Network Layer                                        │   │
│  │  ├── JarvisWeb (HTTP client with retry logic)        │   │
│  │  └── JarvisStreamClient (WebSocket via NativeWebSocket)│   │
│  └──────────────────────────────────────────────────────┘   │
└─────────────────────────────────────────────────────────────┘
                            │
                            │ HTTP/WebSocket
                            │ (Device Key Auth)
                            ▼
┌─────────────────────────────────────────────────────────────┐
│              JARVIS v2 Gateway (Banterpacks)                  │
│  ┌──────────────────────────────────────────────────────┐   │
│  │  Gateway (port 8400)                                  │   │
│  │  ├── /jarvis/v2/chat (conversation)                  │   │
│  │  ├── /jarvis/v2/stream (real-time events)            │   │
│  │  ├── /jarvis/v2/devices/register (device auth)       │   │
│  │  ├── /jarvis/v2/tools/* (tool approvals)            │   │
│  │  ├── /jarvis/v2/sessions/* (session management)      │   │
│  │  ├── /jarvis/v2/mesh/* (cross-device events)         │   │
│  │  └── /jarvis/v2/voice/* (voice pipeline)            │   │
│  └──────────────────────────────────────────────────────┘   │
│  ┌──────────────────────────────────────────────────────┐   │
│  │  Backend Services                                     │   │
│  │  ├── LLM Providers (OpenAI, Anthropic, Google, etc.) │   │
│  │  ├── TTS/STT Services                                 │   │
│  │  ├── Tool Execution (TDD005 sandbox)                 │   │
│  │  ├── Memory Store (SQLite)                            │   │
│  │  └── Proactive Scheduler                              │   │
│  └──────────────────────────────────────────────────────┘   │
└─────────────────────────────────────────────────────────────┘
```

---

## Features

### Core Capabilities

- ✅ **Device Registration** - One-time bootstrap with master key, persistent device key storage
- ✅ **Chat Interface** - Text and voice-based conversations with JARVIS
- ✅ **Real-time Streaming** - WebSocket-based event streaming for live responses
- ✅ **Session Management** - Browse, resume, and handoff conversations
- ✅ **Tool Approvals** - Secure approval workflow for AI tool execution
- ✅ **Voice Integration** - Optional Embardiment ASR/TTS for hands-free interaction
- ✅ **Mesh Networking** - Push/pull events for cross-device visibility
- ✅ **Proactive Notifications** - Receive AI-driven alerts and suggestions
- ✅ **Timeline View** - Complete conversation history with turns and messages
- ✅ **Handoff Support** - Transfer sessions between devices via tokens

### UI Components

The companion UI is split into maintainable partials:

- **`JarvisOnGuiCompanion.Gui.cs`** - Main tabbed UI framework
- **`JarvisOnGuiCompanion.Gui.ChatTab.cs`** - Chat input/output
- **`JarvisOnGuiCompanion.Gui.VoiceTab.cs`** - Voice controls
- **`JarvisOnGuiCompanion.Gui.SessionsTab.cs`** - Session browser
- **`JarvisOnGuiCompanion.Gui.ToolsTab.cs`** - Tool approval interface
- **`JarvisOnGuiCompanion.Gui.HandoffTab.cs`** - Session handoff
- **`JarvisOnGuiCompanion.Gui.MeshTab.cs`** - Mesh event debug
- **`JarvisOnGuiCompanion.Gui.NotificationsTab.cs`** - Proactive alerts
- **`JarvisOnGuiCompanion.Gui.SettingsTab.cs`** - Configuration

---

## Prerequisites

### Required

- **Unity Hub** and **Unity 6000.0+**
- **Android SDK** (via Unity Hub)
- **Git** (for UPM Git dependencies)
- **Banterpacks JARVIS Gateway** running (see [Quick Start](#quick-start))

### Optional

- **Embardiment** package (for native ASR/TTS)
- **Gemini API Key** (for cloud TTS fallback)

---

## Quick Start

### 1. Start JARVIS Backend

From the Banterpacks repository:

```bash
cd ../Banterpacks
npm run jarvis:up
```

This starts:
- JARVIS Gateway on `http://localhost:8400`
- Registry on port 8001
- Studio on port 5173
- All supporting services

### 2. Open in Unity

1. **Unity Hub** → **Add** → **Add project from disk** → select this folder
2. **File** → **Build Settings** → Switch platform to **Android**
3. Open scene: `Assets/Scenes/Message Relay with Android.unity`
4. Press **Play**: `JarvisOnGuiCompanion` auto-spawns

### 3. Configure Connection

1. In the Unity Editor, find the `JarvisOnGuiCompanion` GameObject
2. Set **Base URL** to your machine's LAN IP (e.g., `http://192.168.1.10:8400`)
   - ⚠️ Android cannot use `localhost` - use your actual LAN IP
   - For remote access, use a VPN (Tailscale/WireGuard) and point to VPN IP
3. Set **Master Key** (Editor-only, for one-time device registration)
4. Click **Register Device** in the Settings tab
5. Device key is stored in PlayerPrefs for all future requests

### 4. Start Chatting

- Use the **Chat** tab to send messages
- Use the **Voice** tab for hands-free interaction (if Embardiment ASR is present)
- View conversation history in the **Sessions** tab

---

## Setup & Installation

### Unity Project Setup

1. **Clone the repository:**
   ```bash
   git clone <repository-url>
   cd Chimeradroid
   ```

2. **Open in Unity Hub:**
   - Unity Hub → Add → Add project from disk
   - Select the `Chimeradroid` folder

3. **Install dependencies:**
   - Unity will automatically fetch packages from `Packages/manifest.json`
   - Embardiment: `com.google.xr.embardiment` via OpenUPM
   - NativeWebSocket: Git dependency from `endel/NativeWebSocket`

4. **If Git packages fail to load:**
   - Install Git: https://git-scm.com/downloads
   - Restart Unity Hub and reopen the project

### Android Build Configuration

1. **Switch to Android platform:**
   - File → Build Settings → Android → Switch Platform

2. **Configure Android settings:**
   - Edit → Project Settings → Player
   - Set **Package Name** (e.g., `com.yourcompany.chimeradroid`)
   - Set **Minimum API Level** (API 24+ recommended)
   - Enable **Internet Access** permission

3. **Build APK:**
   - File → Build Settings → Build
   - Select output location and build

### Embardiment Integration (Optional)

For native Android ASR/TTS:

1. **Embardiment is already included** via `Packages/manifest.json`
2. **Add ASR/TTS prefabs to scene:**
   - From `Packages/Embardiment/Runtime/Prefabs/`
   - Add `AndroidAsr` and `AndroidTts` to your scene
   - `JarvisOnGuiCompanion` will auto-detect them

3. **Configure Gemini API Key (for cloud TTS):**
   - Create `Assets/Resources/` folder
   - Right-click → Create → Scriptable Objects → Gemini Key
   - Paste your API key from [Google AI Studio](https://aistudio.google.com/app/apikey)
   - ⚠️ Add `GeminiKey.asset` to `.gitignore`

---

## Configuration

### PlayerPrefs Storage

Chimeradroid stores configuration in Unity PlayerPrefs:

- `jarvis.base_url` - JARVIS Gateway base URL
- `jarvis.device_key` - Registered device key (persistent)
- `jarvis.session_id` - Current session ID (persistent)

### Inspector Settings

Configure in Unity Inspector on `JarvisOnGuiCompanion`:

| Setting | Description | Default |
|---------|-------------|---------|
| **Base URL** | JARVIS Gateway URL | `http://localhost:8400` |
| **Master Key** | One-time device registration key | `jarvis-demo-key` |
| **Device Name** | Device identifier | `chimeradroid` |
| **Message** | Default chat message | `Hello, Jarvis.` |
| **Speak Responses** | Enable TTS for responses | `false` |
| **Use Android Asr If Present** | Auto-detect Embardiment ASR | `true` |

### Network Configuration

**For Local Development:**
- Use your machine's LAN IP: `http://192.168.1.10:8400`
- Ensure Android device and development machine are on same network

**For Remote Access:**
- Use VPN (Tailscale/WireGuard): `http://100.x.x.x:8400`
- Or use port forwarding/ngrok for testing

**For Production:**
- Use HTTPS endpoint: `https://jarvis.yourdomain.com`
- Remove master key from build (Editor-only)

---

## Usage

### Chat Interface

1. **Open Chat Tab** in the companion UI
2. **Type a message** or use voice input
3. **Click Send** or press Enter
4. **View response** in the output area
5. **Enable streaming** for real-time responses

### Voice Interaction

1. **Ensure Embardiment ASR is in scene** (optional)
2. **Open Voice Tab**
3. **Click Start Listening** to begin speech recognition
4. **Speak your message** - it will auto-send when complete
5. **Enable TTS** to hear responses aloud

### Session Management

1. **Open Sessions Tab**
2. **Click Refresh** to load recent sessions
3. **Select a session** to view details
4. **Click Connect Stream** to resume real-time streaming
5. **View Timeline** for complete conversation history

### Tool Approvals

1. **Open Tools Tab**
2. **View pending approvals** from stream events
3. **Review tool details** (name, args, risk tier)
4. **Approve or Cancel** tool execution
5. **Monitor execution status** in real-time

### Handoff

1. **Open Handoff Tab**
2. **Enter handoff token** from another device
3. **Click Redeem** to transfer session
4. **Automatically connect** to stream URL

### Mesh Events

1. **Open Mesh Tab**
2. **Push events** to share with other devices
3. **Pull events** to see cross-device activity
4. **View whoami** to see current device identity

### Proactive Notifications

1. **Open Notifications Tab**
2. **Enable proactive mode** in settings
3. **Receive alerts** when AI has suggestions
4. **Mark as read** to dismiss notifications

---

## API Integration

Chimeradroid integrates with the JARVIS v2 API. All endpoints use device key authentication (except device registration which uses master key).

### Device Management

- **`POST /jarvis/v2/devices/register`** - Register device (one-time, master key)
  - Request: `{ "name": "chimeradroid" }`
  - Response: `{ "device_id": "...", "device_key": "...", ... }`

### Chat & Streaming

- **`POST /jarvis/v2/chat`** - Send chat message
  - Request: `{ "message": "...", "session_id": "...", "mode": "async" }`
  - Response: `{ "turn_id": "...", "session_id": "...", "stream_url": "...", ... }`

- **`GET /jarvis/v2/turns/{turn_id}`** - Poll turn status
  - Response: `{ "status": "...", "final_response": "...", ... }`

- **`WebSocket /jarvis/v2/stream`** - Real-time event stream
  - Connect with device key as token
  - Receives: `turn.progress`, `turn.complete`, `tool.approval_required`, etc.

### Session Management API

- **`GET /jarvis/v2/sessions`** - List recent sessions
  - Response: `{ "sessions": [...] }`

- **`GET /jarvis/v2/sessions/{session_id}/timeline`** - Get session timeline
  - Response: `{ "session": {...}, "turns": [...], "messages": [...] }`

- **`POST /jarvis/v2/sessions/handoff/redeem`** - Redeem handoff token
  - Request: `{ "token": "..." }`
  - Response: `{ "session_id": "...", "stream_url": "..." }`

### Tool Execution

- **`GET /jarvis/v2/tools/approvals`** - List pending approvals
- **`POST /jarvis/v2/tools/approve`** - Approve tool execution
  - Request: `{ "approval_id": "...", "tool_run_id": "..." }`
- **`POST /jarvis/v2/tools/execute`** - Execute approved tool
  - Request: `{ "tool_run_id": "...", "approval_id": "..." }`
- **`POST /jarvis/v2/tools/cancel`** - Cancel tool execution

### Voice Pipeline

- **`POST /jarvis/v2/voice/converse`** - Voice conversation
  - Request: `{ "text": "...", "voice_mode": "..." }`
  - Response: `{ "response_audio_b64": "...", "transcribed_text": "...", ... }`

### Mesh Networking

- **`POST /jarvis/v2/mesh/push`** - Push event to mesh
  - Request: `{ "event_type": "...", "event_data": {...} }`
- **`GET /jarvis/v2/mesh/pull`** - Pull events from mesh
  - Response: `{ "events": [...] }`
- **`GET /jarvis/v2/mesh/whoami`** - Get device identity
  - Response: `{ "device_id": "...", "user_id": "...", ... }`

### Proactive Notifications API

- **`GET /jarvis/v2/notifications`** - List notifications
- **`PATCH /jarvis/v2/notifications/{id}/read`** - Mark as read
- **`GET /jarvis/v2/proactive/status`** - Get proactive status
- **`PATCH /jarvis/v2/proactive/consents`** - Update consents

### System

- **`GET /jarvis/v2/system/status`** - System health check
- **`GET /jarvis/v2/system/control-room`** - Control room dashboard

---

## Embardiment Integration

Chimeradroid optionally integrates with Embardiment for native Android AI capabilities.

### Automatic Detection

`JarvisOnGuiCompanion` automatically detects Embardiment components in the scene:

```csharp
// Auto-detects AndroidAsr
var asr = FindEmbardimentComponent("Google.XR.Embardiment.AndroidAsr");

// Auto-detects AndroidTts or GeminiTts
var tts = FindEmbardimentComponent("Google.XR.Embardiment.AndroidTts")
       ?? FindEmbardimentComponent("Google.XR.Embardiment.GeminiTts");
```

### ASR Integration

1. **Add AndroidAsr prefab** to scene
2. **Enable "Use Android Asr If Present"** in inspector
3. **Start listening** via Voice tab
4. **OnComplete event** automatically sends recognized text to chat

### TTS Integration

1. **Add AndroidTts or GeminiTts** prefab to scene
2. **Enable "Speak Responses"** in inspector
3. **Responses are automatically spoken** after receiving from JARVIS
4. **Audio playback** via Unity AudioSource component

### Embardiment Features

Embardiment provides:

- **Android ASR** - On-device speech recognition (no cloud required)
- **Android TTS** - On-device text-to-speech (no cloud required)
- **Gemini TTS** - Cloud-based TTS fallback (requires API key)
- **OCR** - Optical character recognition (not used by Chimeradroid)
- **LLM** - Large language model integration (JARVIS handles this)

See [Embardiment README](../embardiment/README.md) for full documentation.

---

## Development

### Project Structure

```text
Chimeradroid/
├── Assets/
│   ├── Scripts/
│   │   └── Jarvis/
│   │       ├── JarvisOnGuiCompanion.cs          # Main companion class
│   │       ├── JarvisOnGuiCompanion.Gui.*.cs    # UI partials (tabs)
│   │       ├── JarvisOnGuiCompanion.*.cs         # Feature partials
│   │       ├── JarvisWeb.cs                      # HTTP client
│   │       ├── JarvisStreamClient.cs             # WebSocket client
│   │       ├── JarvisModels.cs                   # API models
│   │       └── Audio/
│   │           └── WavReader.cs                 # Audio utilities
│   ├── Scenes/
│   │   └── Message Relay with Android.unity      # Main scene
│   └── Resources/                                # Optional: GeminiKey.asset
├── Packages/
│   └── manifest.json                             # Dependencies
└── README.md                                     # This file
```

### Code Organization

- **`JarvisOnGuiCompanion.cs`** - Core MonoBehaviour, auto-spawns on scene load
- **`JarvisOnGuiCompanion.Gui.*.cs`** - UI rendering (IMGUI)
- **`JarvisOnGuiCompanion.*.cs`** - Feature implementations (chat, voice, tools, etc.)
- **`JarvisWeb.cs`** - HTTP client with retry logic and rate limit handling
- **`JarvisStreamClient.cs`** - WebSocket client using NativeWebSocket
- **`JarvisModels.cs`** - C# data models matching JARVIS API schemas

### Adding New Features

1. **Create partial class** in `Assets/Scripts/Jarvis/`
2. **Add UI tab** in `JarvisOnGuiCompanion.Gui.cs`
3. **Implement API calls** using `JarvisWeb` helper methods
4. **Handle stream events** in `JarvisOnGuiCompanion.Stream.cs`

### WebSocket Implementation

Chimeradroid uses `NativeWebSocket` (Git UPM dependency) instead of Unity's `ClientWebSocket` to avoid IL2CPP compatibility issues on Android.

```csharp
// Connection
var ws = new WebSocket(streamUrl);
ws.Connect();

// Send hello
ws.SendText(JsonConvert.SerializeObject(new StreamHello {
    Type = "client.hello",
    Token = deviceKey,
    SessionId = sessionId
}));

// Receive messages
ws.OnMessage += (bytes) => {
    var text = Encoding.UTF8.GetString(bytes);
    var evt = JsonConvert.DeserializeObject<StreamEvent>(text);
    // Handle event...
};
```

### Testing

1. **Editor Testing:**
   - Run in Unity Editor with Play mode
   - Use `http://localhost:8400` if JARVIS is on same machine
   - Test all UI tabs and API integrations

2. **Android Testing:**
   - Build APK and install on device
   - Use LAN IP for Base URL
   - Test voice features with Embardiment ASR/TTS

3. **Integration Testing:**
   - Verify device registration
   - Test chat with streaming
   - Test tool approvals workflow
   - Test session handoff

---

## Troubleshooting

### Common Issues

**"Missing base url" error:**
- Ensure Base URL is set in Settings tab
- Use LAN IP, not `localhost` (Android can't reach localhost)
- Check that JARVIS Gateway is running: `npm run jarvis:up` in Banterpacks

**"Register failed" error:**
- Verify master key is correct
- Check JARVIS Gateway is accessible
- Ensure network connectivity

**"ASR not found in scene":**
- Add Embardiment `AndroidAsr` prefab to scene
- Or disable "Use Android Asr If Present" to use text input only

**WebSocket connection fails:**
- Verify device key is registered and stored
- Check stream URL is valid
- Ensure network allows WebSocket connections

**Git packages not loading:**
- Install Git: https://git-scm.com/downloads
- Restart Unity Hub
- Check internet connectivity

**Audio playback issues:**
- Ensure AudioSource component exists (auto-created)
- Check TTS component is in scene
- Verify audio permissions on Android

### Debug Tips

1. **Check Unity Console** for errors
2. **View status messages** in companion UI
3. **Test API directly** with curl:
   ```bash
   curl -H "X-Jarvis-Device-Key: your-key" http://localhost:8400/jarvis/v2/system/status
   ```
4. **Enable logging** in JARVIS Gateway for server-side debugging

---

## Related Projects

### Upstream Dependencies

- **Embardiment** (`../embardiment`)
  - Apache-2.0 licensed Unity package
   - Source: <https://github.com/google/embardiment> (or your fork)
  - Provides: Android ASR, Android TTS, Gemini integration

- **Banterpacks** (`../Banterpacks`)
  - JARVIS v2 AI Gateway backend
  - Production-ready with 95.5% test pass rate
  - Full documentation: See `../Banterpacks/README.md`

### Package Dependencies

- **`com.google.xr.embardiment`** (v1.0.0)
  - Installed via OpenUPM registry
  - Source: `package.openupm.com`

- **`com.endel.nativewebsocket`**
  - Git UPM dependency
   - Source: <https://github.com/endel/NativeWebSocket.git#upm>
  - Required for Android WebSocket support

---

## License

See [LICENSE](LICENSE) file for details.

---

## Acknowledgments

- **Embardiment Team** - For the excellent Unity Android AI toolkit
- **Banterpacks Team** - For the production-ready JARVIS Gateway
- **NativeWebSocket** - For Android WebSocket compatibility

---

## Status

✅ **Production Ready** - Fully functional JARVIS v2 client

**Last Updated:** January 2025  
**Unity Version:** 6000.0+  
**Platform:** Android  
**JARVIS API:** v2

---

For more information:
- **JARVIS Gateway Docs:** `../Banterpacks/docs/JARVIS_V1_COMPLETE.md`
- **Embardiment Docs:** `../embardiment/README.md`
- **Banterpacks README:** `../Banterpacks/README.md`
