using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Netsy.Extensions;

namespace Netsy.Atom
{
    public class AtomClient
    {
        private readonly SemaphoreSlim _lock;

        private TcpClient _client;
        private NetworkStream _stream;
        private Task _readFromClientTask;

        public IPEndPoint Address { get; }
        public AtomClientState State { get; private set; }

        public event EventHandler<AtomClientMessageReceivedEventArgs> MessageReceived;

        public AtomClient(IPEndPoint address)
        {
            this.Address = address;
            this.State = AtomClientState.Disconnected;

            this._lock = new SemaphoreSlim(1, 1);
        }

        public async Task ConnectAsync()
        {
            await this._lock.WaitAsync();
            try
            {
                if (this.State.CanConnect() == false)
                    return;

                this.State = AtomClientState.Connecting;

                var client = new TcpClient();
                client.Connect(this.Address);

                this._client = client;
                this._stream = client.GetStream();
                this._readFromClientTask = Task.Factory.StartNew(this.ReadFromClient, TaskCreationOptions.LongRunning);

                this.State = AtomClientState.Connected;
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

                this._stream.Dispose();
                this._stream = null;

                this._client.Dispose();
                this._client = null;

                this._readFromClientTask = null;

                this.State = AtomClientState.Disconnected;
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