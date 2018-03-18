using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Netsy.Atom;
using Netsy.Atom.Extensibility;

namespace Netsy
{
    public class NetsyClient
    {
        private readonly AtomClient _atomClient;
        private readonly IPackageSerializer _packageSerializer;
        private readonly NetsyConnectionHelper.HandlerRegistry _handlerRegistry;
        private readonly NetsyConnectionHelper _connectionHelper;

        public IPEndPoint Address => this._atomClient.Address;
        public AtomClientState State => this._atomClient.State;
        public IDictionary<string, object> Data { get; }

        public NetsyClient(IPEndPoint address, IPackageSerializer packageSerializer, IAtomClientPlugin plugin = null)
        {
            Guard.NotNull(address, nameof(address));
            Guard.NotNull(packageSerializer, nameof(packageSerializer));

            this.Data = new Dictionary<string, object>();

            this._atomClient = new AtomClient(address, plugin);
            this._packageSerializer = packageSerializer;

            this._handlerRegistry = new NetsyConnectionHelper.HandlerRegistry();
            this._connectionHelper = new NetsyConnectionHelper(packageSerializer, f => this._atomClient.SendMessageAsync(f), this._handlerRegistry);

            this._atomClient.MessageReceived += this.AtomClientOnMessageReceived;
        }

        public Task ConnectAsync()
        {
            return this._atomClient.ConnectAsync();
        }

        public Task DisconnectAsync()
        {
            return this._atomClient.DisconnectAsync();
        }

        public Task SendPackageAsync(object package)
        {
            Guard.NotNull(package, nameof(package));

            return this._connectionHelper.SendPackageAsync(package);
        }

        public Task<TResponse> SendRequestAsync<TResponse>(object request)
        {
            Guard.NotNull(request, nameof(request));

            return this._connectionHelper.SendRequestAsync<TResponse>(request);
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

        private async void AtomClientOnMessageReceived(object sender, AtomClientMessageReceivedEventArgs e)
        {
            await this._connectionHelper.HandleAtomMessage(e.Message);
        }
    }
}