using System;
using System.Collections.Concurrent;
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
        private readonly NetsyConnectionHelperHandlerRegistry _handlerRegistry;
        private readonly NetsyConnectionHelper _connectionHelper;

        public AtomClient AtomClient { get; }
        public IPackageSerializer PackageSerializer { get; }

        public NetsyClient(IPEndPoint address, IPackageSerializer packageSerializer, IAtomClientPlugin plugin = null)
            : this(new AtomClient(address, plugin), packageSerializer)
        {
            
        }

        public NetsyClient(AtomClient atomClient, IPackageSerializer packageSerializer)
        {
            this.AtomClient = atomClient;
            this.PackageSerializer = packageSerializer;

            this._handlerRegistry = new NetsyConnectionHelperHandlerRegistry();
            this._connectionHelper = new NetsyConnectionHelper(packageSerializer, f => this.AtomClient.SendMessageAsync(f), this._handlerRegistry);

            this.AtomClient.MessageReceived += this.AtomClientOnMessageReceived;
        }

        public Task ConnectAsync()
        {
            return this.AtomClient.ConnectAsync();
        }

        public Task DisconnectAsync()
        {
            return this.AtomClient.DisconnectAsync();
        }

        public Task SendPackageAsync(object package)
        {
            return this._connectionHelper.SendPackageAsync(package);
        }

        public Task<TResponse> SendRequestAsync<TResponse>(object request)
        {
            return this._connectionHelper.SendRequestAsync<TResponse>(request);
        }

        public Task SendMessageAsync(NetsyMessage message)
        {
            return this._connectionHelper.SendMessageAsync(message);
        }

        public void OnPackageReceived<T>(Action<T> handler)
        {
            this._handlerRegistry.OnPackageReceived(handler);
        }

        public void OnRequestReceived<TRequest, TResponse>(Func<TRequest, TResponse> handler)
        {
            this._handlerRegistry.OnRequestReceived(handler);
        }

        private async void AtomClientOnMessageReceived(object sender, AtomClientMessageReceivedEventArgs e)
        {
            await this._connectionHelper.HandleAtomMessage(e.Message);
        }
    }

    internal class NetsyConnectionHelperHandlerRegistry
    {
        private readonly ConcurrentDictionary<Type, ConcurrentBag<Action<object>>> _packageHandlers;
        private readonly ConcurrentDictionary<Guid, Action<object>> _outgoingRequests;
        private readonly ConcurrentDictionary<Type, Func<object, object>> _incomingRequestHandlers;

        public NetsyConnectionHelperHandlerRegistry()
        {
            this._packageHandlers = new ConcurrentDictionary<Type, ConcurrentBag<Action<object>>>();
            this._outgoingRequests = new ConcurrentDictionary<Guid, Action<object>>();
            this._incomingRequestHandlers = new ConcurrentDictionary<Type, Func<object, object>>();
        }

        public void OnPackageReceived<T>(Action<T> handler)
        {
            this._packageHandlers.GetOrAdd(typeof(T), _ => new ConcurrentBag<Action<object>>()).Add(f => handler((T)f));
        }

        public void OnRequestReceived<TRequest, TResponse>(Func<TRequest, TResponse> handler)
        {
            Func<object, object> wrappedHandler = f => (object)handler((TRequest)f);
            this._incomingRequestHandlers.AddOrUpdate(typeof(TRequest), wrappedHandler, (_, __) => wrappedHandler);
        }

        public void AddOutgoingRequest(Guid requestId, Action<object> responseHandler)
        {
            this._outgoingRequests.AddOrUpdate(requestId, responseHandler, (_, __) => responseHandler);
        }

        public void HandlePackage(object package)
        {
            if (this._packageHandlers.TryGetValue(package.GetType(), out var handlers))
            {
                foreach (var handler in handlers)
                {
                    handler.Invoke(package);
                }
            }
        }

        public (bool, object) HandleIncomingRequest(object package)
        {
            if (this._incomingRequestHandlers.TryGetValue(package.GetType(), out var handler))
            {
                return (true, handler(package));
            }

            return (false, null);
        }

        public void HandleResponse(Guid requestId, object package)
        {
            if (this._outgoingRequests.TryGetValue(requestId, out var handler))
            {
                handler(package);
            }
        }
    }

    internal class NetsyConnectionHelper
    {
        private readonly IPackageSerializer _packageSerializer;
        private readonly Func<AtomMessage, Task> _sendAtomMessage;
        private readonly NetsyConnectionHelperHandlerRegistry _handlerRegistry;

        public NetsyConnectionHelper(IPackageSerializer packageSerializer, Func<AtomMessage, Task> sendAtomMessage, NetsyConnectionHelperHandlerRegistry handlerRegistry)
        {
            this._packageSerializer = packageSerializer;
            this._sendAtomMessage = sendAtomMessage;
            this._handlerRegistry = handlerRegistry;
        }

        public async Task SendPackageAsync(object package)
        {
            var data = this._packageSerializer.Serialize(package);
            await this.SendMessageAsync(NetsyMessage.Package(data));
        }


        public async Task<TResponse> SendRequestAsync<TResponse>(object request)
        {
            var data = this._packageSerializer.Serialize(request);
            var message = NetsyMessage.Request(data);

            var result = new TaskCompletionSource<TResponse>();
            Action<object> responseHandler = (response) => result.SetResult((TResponse)response);

            this._handlerRegistry.AddOutgoingRequest(message.RequestId, responseHandler);

            await this.SendMessageAsync(message);

            return await result.Task;
        }

        public async Task SendMessageAsync(NetsyMessage message)
        {
            await this._sendAtomMessage(message.ToAtomMessage());
        }

        public async Task HandleAtomMessage(AtomMessage message)
        {
            var netsyMessage = NetsyMessage.FromAtomMessage(message.Data);
            var package = this._packageSerializer.Deserialize(netsyMessage.Data);

            switch (netsyMessage.Kind)
            {
                case NetsyMessageKind.SendPackage:
                    this._handlerRegistry.HandlePackage(package);
                    break;

                case NetsyMessageKind.Request:
                    var (success, response) = this._handlerRegistry.HandleIncomingRequest(package);
                    if (success)
                    {
                        var responseData = this._packageSerializer.Serialize(response);

                        var responseMessage = NetsyMessage.ResponseFor(responseData, netsyMessage.RequestId);
                        await this.SendMessageAsync(responseMessage);
                    }
                    break;

                case NetsyMessageKind.Response:
                    this._handlerRegistry.HandleResponse(netsyMessage.RequestId, package);
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}