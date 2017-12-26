using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace Netsy.Atom
{
    public class AtomServer
    {
        private readonly ConcurrentDictionary<Guid, AtomChannel> _channels;
        private readonly SemaphoreSlim _lock;

        private TcpListener _server;
        private Task _acceptClientsTask;

        public IPEndPoint Address { get; }
        public AtomServerState State { get; private set; }
        public IReadOnlyCollection<AtomChannel> Channels => new ReadOnlyCollection<AtomChannel>(this._channels.Values.ToList());

        public event EventHandler<AtomChannelConnectedEventArgs> ChannelConnected;
        public event EventHandler<AtomChannelDisconnectedEventArgs> ChannelDisconnected;

        public AtomServer(IPEndPoint address)
        {
            this.Address = address;
            this.State = AtomServerState.Stopped;
            this._channels = new ConcurrentDictionary<Guid, AtomChannel>();
            this._lock = new SemaphoreSlim(1, 1);
        }

        public async Task StartAsync()
        {
            await this._lock.WaitAsync();

            try
            {
                if (this.State.CanStart() == false)
                    return;

                this.State = AtomServerState.Starting;

                var server = new TcpListener(this.Address);
                server.Start();

                this._server = server;
                this._acceptClientsTask = Task.Factory.StartNew(this.AcceptClients, TaskCreationOptions.LongRunning);

                this.State = AtomServerState.Started;
            }
            catch (Exception exception)
            {
                throw new NetsyException("Error while starting the AtomServer.", exception);
            }
            finally
            {
                if (this.State == AtomServerState.Starting)
                    this.State = AtomServerState.Stopped;

                this._lock.Release();
            }
        }

        public async Task StopAsync()
        {
            await this._lock.WaitAsync();

            try
            {
                if (this.State.CanStop() == false)
                    return;

                this.State = AtomServerState.Stopping;

                foreach (var client in this._channels)
                {
                    client.Value.Disconnect();
                }
                
                this._server.Stop();
                this._server = null;
                this._acceptClientsTask = null;

                this.State = AtomServerState.Stopped;
            }
            catch (Exception exception)
            {
                throw new NetsyException("Error while stopping the AtomServer.", exception);
            }
            finally
            {
                if (this.State == AtomServerState.Stopping) //TODO: Not sure about this one yet
                    this.State = AtomServerState.Started;

                this._lock.Release();
            }
        }

        internal void RemoveChannel(AtomChannel channel)
        {
            var pair = this._channels.FirstOrDefault(f => f.Value == channel);

            if (pair.Key == default)
                return;

            if (this._channels.TryRemove(pair.Key, out _))
                this.ChannelDisconnected?.Invoke(this, new AtomChannelDisconnectedEventArgs(channel));
        }

        private void AcceptClients()
        {
            //TODO: Thread.CurrentThread.Name = "AtomServer.AcceptClients";

            while (this.State == AtomServerState.Started)
            {
                var client = this._server.AcceptTcpClient();

                var channel = new AtomChannel(this, client);
                this._channels.TryAdd(Guid.NewGuid(), channel);

                this.ChannelConnected?.Invoke(this, new AtomChannelConnectedEventArgs(channel));
            }
        }
    }
}