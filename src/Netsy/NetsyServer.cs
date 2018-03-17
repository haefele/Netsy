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
        private readonly NetsyConnectionHelperHandlerRegistry _handlerRegistry;

        public IReadOnlyCollection<NetsyChannel> Channels => new ReadOnlyCollection<NetsyChannel>(this._channels.Values.ToList());
        public AtomServer AtomServer { get; }
        public IPackageSerializer PackageSerializer { get; }

        public NetsyServer(IPEndPoint address, IPackageSerializer packageSerializer, IAtomServerPlugin plugin = null)
            : this(new AtomServer(address, plugin), packageSerializer)
        {
            
        }

        public NetsyServer(AtomServer atomServer, IPackageSerializer packageSerializer)
        {
            this._channels = new ConcurrentDictionary<Guid, NetsyChannel>();
            this._handlerRegistry = new NetsyConnectionHelperHandlerRegistry();

            this.AtomServer = atomServer;
            this.PackageSerializer = packageSerializer;

            this.AtomServer.ChannelConnected += this.AtomServerOnChannelConnected;
            this.AtomServer.ChannelDisconnected += this.AtomServerOnChannelDisconnected;
        }

        public Task StartAsync()
        {
            return this.AtomServer.StartAsync();
        }

        public Task StopAsync()
        {
            return this.AtomServer.StopAsync();
        }

        public void OnPackageReceived<T>(Action<T> handler)
        {
            this._handlerRegistry.OnPackageReceived(handler);
        }

        public void OnRequestReceived<TRequest, TResponse>(Func<TRequest, TResponse> handler)
        {
            this._handlerRegistry.OnRequestReceived(handler);
        }

        const string NetsyServerChannelId = "NetsyServer.Channel.Id";

        private void AtomServerOnChannelConnected(object sender, AtomChannelConnectedEventArgs e)
        {
            var id = Guid.NewGuid();

            e.Channel.Data[NetsyServerChannelId] = id;
            this._channels.TryAdd(id, new NetsyChannel(e.Channel, new NetsyConnectionHelper(this.PackageSerializer, f => e.Channel.SendMessageAsync(f), this._handlerRegistry)));
        }

        private void AtomServerOnChannelDisconnected(object sender, AtomChannelDisconnectedEventArgs e)
        {
            this._channels.TryRemove((Guid) e.Channel.Data[NetsyServerChannelId], out _);
        }
    }
}