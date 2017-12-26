using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Netsy.Atom.Extensibility;
using Netsy.Extensions;

namespace Netsy.Atom
{
    public class AtomClient
    {
        private readonly SemaphoreSlim _lock;

        private readonly IAtomClientPlugin _plugin;
        private TcpClient _client;
        private Stream _stream;
        private Task _readFromClientTask;

        public IPEndPoint Address { get; }
        public AtomClientState State { get; private set; }
        public IDictionary<string, object> Data { get; }

        public event EventHandler<AtomClientMessageReceivedEventArgs> MessageReceived;

        public AtomClient(IPEndPoint address, IAtomClientPlugin plugin = null)
        {
            Guard.NotNull(address, nameof(address));

            this.Address = address;
            this.State = AtomClientState.Disconnected;
            this.Data = new Dictionary<string, object>();
            this._plugin = plugin ?? new NullAtomClientPlugin();
            this._lock = new SemaphoreSlim(1, 1);

            this._plugin.OnAppliedToClient(this);
        }

        public async Task ConnectAsync()
        {
            await this._lock.WaitAsync();
            try
            {
                if (this.State.CanConnect() == false)
                    return;

                this.State = AtomClientState.Connecting;
                await this._plugin.OnConnectingAsync();

                var client = new TcpClient();
                client.Connect(this.Address);

                this._client = client;
                this._stream =  this._plugin.OnCreatingStream(client.GetStream());
                this._readFromClientTask = Task.Factory.StartNew(this.ReadFromClient, TaskCreationOptions.LongRunning);

                this.State = AtomClientState.Connected;
                await this._plugin.OnConnectedAsync();
            }
            catch (Exception exception)
            {
                throw new NetsyException("Error while connecting as the AtomClient.", exception);
            }
            finally
            {
                if (this.State == AtomClientState.Connecting)
                    this.State = AtomClientState.Disconnected;

                this._lock.Release();
            }
        }

        public async Task DisconnectAsync()
        {
            await this._lock.WaitAsync();

            try
            {
                if (this.State.CanDisconnect() == false)
                    return;

                this.State = AtomClientState.Disconnecting;
                await this._plugin.OnDisconnectingAsync();

                this._stream.Dispose();
                this._stream = null;

                this._client.Dispose();
                this._client = null;

                this._readFromClientTask = null;

                this.State = AtomClientState.Disconnected;
                await this._plugin.OnDisconnectedAsync();
            }
            catch (Exception exception)
            {
                throw new NetsyException("Error while disconnecting as the AtomClient.", exception);
            }
            finally
            {
                if (this.State == AtomClientState.Disconnecting) //TODO: Not sure about this one yet
                    this.State = AtomClientState.Connected;

                this._lock.Release();
            }
        }

        public async Task SendMessageAsync(AtomMessage message)
        {
            Guard.NotNull(message, nameof(message));

            await this._lock.WaitAsync();

            try
            {
                await this._plugin.OnSendingMessageAsync(message);
                await this._stream.WriteRawMessageAsync(message.Data);
            }
            finally
            {
                this._lock.Release();
            }
        }
        
        private async void ReadFromClient()
        {
            try
            {
                while (this._client.Connected)
                {
                    var data = await this._stream.ReadRawMessageAsync();

                    if (data == null)
                    {
                        await this.DisconnectAsync();
                        break;
                    }

                    var message = AtomMessage.Incoming(data);
                    await this._plugin.OnMessageReceivedAsync(message);
                    this.MessageReceived?.Invoke(this, new AtomClientMessageReceivedEventArgs(message));
                }
            }
            catch
            {
                await this.DisconnectAsync();
            }
        }
    }
}