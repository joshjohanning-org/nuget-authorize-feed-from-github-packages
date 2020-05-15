﻿using SocketIOClient;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TehGM.Wolfringo.Messages;
using TehGM.Wolfringo.Messages.Serialization;
using TehGM.Wolfringo.Utilities;

namespace TehGM.Wolfringo
{
    public class WolfClient : IWolfClient, IDisposable
    {
        public const string DefaultUrl = "wss://v3-rc.palringo.com:3051";
        public const string DefaultDevice = "bot";

        public string Url { get; }
        public string Token { get; }
        public string Device { get; }
        public bool ThrowMissingSerializer { get; set; } = true;

        public event Action Connected;
        public event Action Disconnected;
        public event Action<IWolfMessage> MessageReceived;
        public event Action PingSent;
        public event Action<TimeSpan> PongReceived;

        private readonly SocketIO _client;
        private readonly IDictionary<string, IMessageSerializer> _serializers;

        public WolfClient(string url, string token, string device = DefaultDevice)
        {
            this.Url = url;
            this.Token = token;
            this.Device = device;

            this._serializers = GetDefaultMessageSerializers();
            this._client = new SocketIO(this.Url);
            this._client.Parameters = new Dictionary<string, string>()
            {
                { "token", this.Token },
                { "device", this.Device }
            };

            this._client.OnReceivedEvent += _client_OnReceivedEvent;
            this._client.OnConnected += () => this.Connected?.Invoke();
            this._client.OnClosed += _ => this.Disconnected?.Invoke();
            this._client.OnPing += () => this.PingSent?.Invoke();
            this._client.OnPong += ts => this.PongReceived?.Invoke(ts);
        }

        public WolfClient(string token, string device = DefaultDevice)
            : this(DefaultUrl, token, device) { }

        public WolfClient(string device = DefaultDevice)
            : this(DefaultUrl, new DefaultWolfTokenProvider().GenerateToken(18), device) { }

        public Task ConnectAsync()
            => _client.ConnectAsync();

        public Task DisconnectAsync()
            => _client.CloseAsync();

        public void Dispose()
            => _client.Dispose();

        private void _client_OnReceivedEvent(string command, SocketIOClient.Arguments.ResponseArgs payload)
        {
            if (!_serializers.TryGetValue(command, out IMessageSerializer serializer))
            {
                if (ThrowMissingSerializer)
                    throw new KeyNotFoundException($"Serializer for command {command} not found");
                return;
            }

            IWolfMessage msg = serializer.Deserialize(command, payload.Text, payload.Buffers);
            this.MessageReceived?.Invoke(msg);
        }

        protected virtual Dictionary<string, IMessageSerializer> GetDefaultMessageSerializers()
        {
            return new Dictionary<string, IMessageSerializer>(StringComparer.OrdinalIgnoreCase)
            {
                { MessageCommands.Welcome, new JsonMessageSerializer<WelcomeMessage>() }
            };
        }
    }
}
