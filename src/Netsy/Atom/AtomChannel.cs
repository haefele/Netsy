using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Netsy.Extensions;

namespace Netsy.Atom
{
    public class AtomChannel
    {
        private readonly AtomServer _parent;
        private readonly TcpClient _channel;
        private readonly Stream _stream;
        private readonly SemaphoreSlim _lock;
        private readonly Task _readFromChannelTask;

        public IDictionary<string, object> Data { get; }

        public event EventHandler<AtomChannelMessageReceivedEventArgs> MessageReceived;

        internal AtomChannel(AtomServer parent, TcpClient channel)
        {
            Guard.NotNull(parent, nameof(parent));
            Guard.NotNull(channel, nameof(channel));

            this._parent = parent;
            this._channel = channel;
            this._lock = new SemaphoreSlim(1, 1);
            this.Data = new Dictionary<string, object>();
            this._stream = this._parent.OnChannelCreatingStream(this, this._channel.GetStream());

            this._readFromChannelTask = Task.Factory.StartNew(this.ReadFromChannel, TaskCreationOptions.LongRunning);
        }

        public async Task SendMessageAsync(AtomMessage message)
        {
            Guard.NotNull(message, nameof(message));

            await this._lock.WaitAsync();

            try
            {
                await this._parent.OnChannelSendingMessageAsync(this, message);
                await this._stream.WriteRawMessageAsync(message.Data);
            }
            finally
            {
                this._lock.Release();
            }
        }

        public async Task DisconnectAsync()
        {
            if (this._channel.Connected)
            {
                await this._parent.RemoveChannelAsync(this);
                this._stream.Dispose();
                this._channel.Dispose();
            }
        }

        private async void ReadFromChannel()
        {
            try
            {
                while (this._channel.Connected)
                {
                    var data = await this._stream.ReadRawMessageAsync();

                    if (data == null)
                    {
                        await this.DisconnectAsync();
                        break;
                    }

                    var message = AtomMessage.Incoming(data);
                    await this._parent.OnChannelMessageReceivedAsync(this, message);
                    this.MessageReceived?.Invoke(this, new AtomChannelMessageReceivedEventArgs(message));
                }
            }
            catch
            {
                await this.DisconnectAsync();
            }
        }
    }
}