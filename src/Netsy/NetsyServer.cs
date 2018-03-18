using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Netsy.Atom;
using Netsy.Atom.Extensibility;

namespace Netsy
{
    public class NetsyServer
    {
        private readonly ConcurrentDictionary<Guid, NetsyChannel> _channels;
        private readonly NetsyConnectionHelper.HandlerRegistry _handlerRegistry;
        private readonly AtomServer _atomServer;
        private readonly IPackageSerializer _packageSerializer;

        public IPEndPoint Address => this._atomServer.Address;
        public AtomServerState State => this._atomServer.State;
        public IDictionary<string, object> Data { get; }
        public IReadOnlyCollection<NetsyChannel> Channels => new ReadOnlyCollection<NetsyChannel>(this._channels.Values.ToList());

        public event EventHandler<NetsyChannelConnectedEventArgs> ChannelConnected;
        public event EventHandler<NetsyChannelDisconnectedEventArgs> ChannelDisconnected;

        public NetsyServer(IPEndPoint address, IPackageSerializer packageSerializer, IAtomServerPlugin plugin = null)
        {
            Guard.NotNull(address, nameof(address));
            Guard.NotNull(packageSerializer, nameof(packageSerializer));

            this.Data = new Dictionary<string, object>();

            this._channels = new ConcurrentDictionary<Guid, NetsyChannel>();
            this._handlerRegistry = new NetsyConnectionHelper.HandlerRegistry();

            this._atomServer = new AtomServer(address, plugin);
            this._packageSerializer = packageSerializer;

            this._atomServer.ChannelConnected += this.AtomServerOnChannelConnected;
            this._atomServer.ChannelDisconnected += this.AtomServerOnChannelDisconnected;
        }

        public Task StartAsync()
        {
            return this._atomServer.StartAsync();
        }

        public Task StopAsync()
        {
            return this._atomServer.StopAsync();
        }

        public void OnPackageReceived<T>(Action<T> handler)
        {
            Guard.NotNull(handler, nameof(handler));

            this._handlerRegistry.OnPackageReceived(handler);
        }

        public void OnRequestReceived<TRequest, TResponse>(Func<TRequest, TResponse> handler)
        {
            Guard.NotNull(handler, nameof(handler));

            this._handlerRegistry.OnRequestReceived(handler);
        }

        public void OnRequestReceived<TRequest, TResponse>(Func<TRequest, Task<TResponse>> handler)
        {
            Guard.NotNull(handler, nameof(handler));

            this._handlerRegistry.OnRequestReceived(handler);
        }

        const string NetsyServerChannelId = "NetsyServer.Channel.Id";
        private void AtomServerOnChannelConnected(object sender, AtomChannelConnectedEventArgs e)
        {
            var id = Guid.NewGuid();

            e.Channel.Data[NetsyServerChannelId] = id;
            var netsyChannel = new NetsyChannel(e.Channel, this._packageSerializer, this._handlerRegistry);

            this._channels.TryAdd(id, netsyChannel);

            this.ChannelConnected?.Invoke(this, new NetsyChannelConnectedEventArgs(netsyChannel));
        }

        private void AtomServerOnChannelDisconnected(object sender, AtomChannelDisconnectedEventArgs e)
        {
            if (this._channels.TryRemove((Guid) e.Channel.Data[NetsyServerChannelId], out var netsyChannel))
            {
                this.ChannelDisconnected?.Invoke(this, new NetsyChannelDisconnectedEventArgs(netsyChannel));
            }
        }
    }
}