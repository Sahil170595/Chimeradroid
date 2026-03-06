using System;
using System.Collections.Concurrent;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NativeWebSocket;
using Newtonsoft.Json;
using UnityEngine;

namespace Chimeradroid.Jarvis
{
    public sealed class JarvisStreamClient : IDisposable
    {
        private readonly ConcurrentQueue<string> _incoming = new ConcurrentQueue<string>();
        private WebSocket _ws;
        private CancellationTokenSource _cts;
        private Task _pingTask;

        public bool IsConnected => _ws != null && _ws.State == WebSocketState.Open;
        public volatile string LastError;

        public event Action<string> OnRawMessage;

        public async Task ConnectAsync(
            string wsUrl,
            StreamHello hello,
            CancellationToken cancellationToken,
            int? recoverSinceSeq = null
        )
        {
            await DisconnectAsync(CancellationToken.None);

            LastError = null;
            _cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            _ws = new WebSocket(wsUrl);

            _ws.OnError += msg =>
            {
                LastError = msg;
                Debug.LogWarning($"JarvisStreamClient error: {msg}");
            };
            _ws.OnClose += code =>
            {
                LastError = $"closed: {code}";
            };
            _ws.OnMessage += bytes =>
            {
                try
                {
                    _incoming.Enqueue(Encoding.UTF8.GetString(bytes));
                }
                catch (Exception e)
                {
                    LastError = e.Message;
                }
            };

            await _ws.Connect();
            _cts.Token.ThrowIfCancellationRequested();

            await SendTextAsync(JsonConvert.SerializeObject(hello), _cts.Token);

            if (recoverSinceSeq.HasValue && recoverSinceSeq.Value > 0)
            {
                await SendTextAsync(
                    JsonConvert.SerializeObject(
                        new { type = "client.recover", since_seq = recoverSinceSeq.Value }
                    ),
                    _cts.Token
                );
            }

            _pingTask = Task.Run(() => PingLoop(_cts.Token), _cts.Token);
        }

        public async Task DisconnectAsync(CancellationToken cancellationToken)
        {
            try
            {
                if (_cts != null && !_cts.IsCancellationRequested)
                {
                    _cts.Cancel();
                }
            }
            catch
            {
                // ignore
            }

            if (_ws != null)
            {
                try
                {
                    if (_ws.State == WebSocketState.Open)
                    {
                        var closeTask = _ws.Close();
                        var done = await Task.WhenAny(
                            closeTask,
                            Task.Delay(TimeSpan.FromSeconds(2), cancellationToken)
                        );
                        if (done == closeTask)
                        {
                            await closeTask;
                        }
                    }
                }
                catch
                {
                    // ignore
                }
                finally
                {
                    _ws = null;
                }
            }

            _cts?.Dispose();
            _cts = null;
            _pingTask = null;
        }

        public void Pump()
        {
            try
            {
                _ws?.DispatchMessageQueue();
            }
            catch
            {
                // ignore
            }

            while (_incoming.TryDequeue(out var msg))
            {
                OnRawMessage?.Invoke(msg);
            }
        }

        public void Dispose()
        {
            try
            {
                if (_cts != null && !_cts.IsCancellationRequested)
                {
                    _cts.Cancel();
                }
            }
            catch
            {
                // ignore
            }

            try
            {
                _cts?.Dispose();
                _cts = null;
            }
            catch
            {
                // ignore
            }

            _ws = null;
            _pingTask = null;
        }

        private async Task SendTextAsync(string text, CancellationToken cancellationToken)
        {
            if (_ws == null)
            {
                return;
            }

            cancellationToken.ThrowIfCancellationRequested();
            await _ws.SendText(text ?? "");
        }

        private async Task PingLoop(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    await Task.Delay(TimeSpan.FromSeconds(20), cancellationToken);
                    if (!IsConnected)
                    {
                        continue;
                    }

                    await SendTextAsync("{\"type\":\"client.ping\"}", cancellationToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception e)
                {
                    LastError = e.Message;
                    break;
                }
            }
        }
    }
}
