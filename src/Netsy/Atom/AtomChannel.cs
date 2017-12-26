using System;
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
        private readonly NetworkStream _stream;
        private readonly SemaphoreSlim _lock;
        private readonly Task _readFromChannelTask;

        public event EventHandler<AtomChannelMessageReceivedEventArgs> MessageReceived;

        internal AtomChannel(AtomServer parent, TcpClient channel)
        {
            Guard.NotNull(parent, nameof(parent));
            Guard.NotNull(channel, nameof(channel));

            this._parent = parent;
            this._channel = channel;
            this._stream = this._channel.GetStream();
            this._lock = new SemaphoreSlim(1, 1);

            this._readFromChannelTask = Task.Factory.StartNew(this.ReadFromChannel, TaskCreationOptions.LongRunning);
        }

        public async Task SendMessageAsync(AtomMessage message)
        {
            Guard.NotNull(message, nameof(message));

            await this._lock.WaitAsync();

            try
            {
                await this._stream.WriteRawMessageAsync(message.Data);
            }
            finally
            {
                this._lock.Release();
            }
        }

        public void Disconnect()
        {
            if (this._channel.Connected)
            {
                this._parent.RemoveChannel(this);
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
                        this.Disconnect();
                        break;
                    }

                    var message = AtomMessage.Incoming(data);
                    this.MessageReceived?.Invoke(this, new AtomChannelMessageReceivedEventArgs(message));
                }
            }
            catch
            {
                this.Disconnect();
            }
        }
    }
}