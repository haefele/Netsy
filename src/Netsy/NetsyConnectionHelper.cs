using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Netsy.Atom;

namespace Netsy
{
    internal class NetsyConnectionHelper
    {
        private readonly IPackageSerializer _packageSerializer;
        private readonly Func<AtomMessage, Task> _sendAtomMessage;
        private readonly HandlerRegistry _handlerRegistry;

        public NetsyConnectionHelper(IPackageSerializer packageSerializer, Func<AtomMessage, Task> sendAtomMessage, HandlerRegistry handlerRegistry)
        {
            Guard.NotNull(packageSerializer, nameof(packageSerializer));
            Guard.NotNull(sendAtomMessage, nameof(sendAtomMessage));
            Guard.NotNull(handlerRegistry, nameof(handlerRegistry));

            this._packageSerializer = packageSerializer;
            this._sendAtomMessage = sendAtomMessage;
            this._handlerRegistry = handlerRegistry;
        }

        public async Task SendPackageAsync(object package)
        {
            Guard.NotNull(package, nameof(package));

            var data = this._packageSerializer.Serialize(package);
            await this.SendMessageAsync(NetsyMessage.Package(data));
        }
        
        public async Task<TResponse> SendRequestAsync<TResponse>(object request)
        {
            Guard.NotNull(request, nameof(request));

            var data = this._packageSerializer.Serialize(request);
            var message = NetsyMessage.Request(data);

            var result = new TaskCompletionSource<TResponse>();
            Action<object> responseHandler = (response) => result.SetResult((TResponse)response);

            this._handlerRegistry.AddOutgoingRequest(message.RequestId, responseHandler);

            await this.SendMessageAsync(message);

            return await result.Task;
        }

        public async Task HandleAtomMessage(NetsyChannel channel, AtomMessage atomMessage)
        {
            //channel can be null
            Guard.NotNull(atomMessage, nameof(atomMessage));

            var netsyMessage = NetsyMessage.FromAtomMessage(atomMessage.Data);
            var package = this._packageSerializer.Deserialize(netsyMessage.Data);

            switch (netsyMessage.Kind)
            {
                case NetsyMessageKind.SendPackage:
                    this._handlerRegistry.HandlePackage(channel, package);
                    break;

                case NetsyMessageKind.Request:
                    var (success, response) = await this._handlerRegistry.HandleIncomingRequest(channel, package);
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

        private async Task SendMessageAsync(NetsyMessage message)
        {
            Guard.NotNull(message, nameof(message));

            await this._sendAtomMessage(message.ToAtomMessage());
        }

        #region Handler Registry
        internal class HandlerRegistry
        {
            private readonly ConcurrentDictionary<Type, ConcurrentBag<Action<NetsyChannel, object>>> _packageHandlers;
            private readonly ConcurrentDictionary<Guid, Action<object>> _outgoingRequests;
            private readonly ConcurrentDictionary<Type, Func<NetsyChannel, object, Task<object>>> _incomingRequestHandlers;

            public HandlerRegistry()
            {
                this._packageHandlers = new ConcurrentDictionary<Type, ConcurrentBag<Action<NetsyChannel, object>>>();
                this._outgoingRequests = new ConcurrentDictionary<Guid, Action<object>>();
                this._incomingRequestHandlers = new ConcurrentDictionary<Type, Func<NetsyChannel, object, Task<object>>>();
            }

            public void OnPackageReceived<T>(Action<T> handler)
            {
                this.OnPackageReceived((NetsyChannel _, T package) => handler(package));
            }
            public void OnPackageReceived<T>(Action<NetsyChannel, T> handler)
            {
                Guard.NotNull(handler, nameof(handler));

                this._packageHandlers.GetOrAdd(typeof(T), _ => new ConcurrentBag<Action<NetsyChannel, object>>()).Add((channel, package) => handler(channel, (T)package));
            }

            public void OnRequestReceived<TRequest, TResponse>(Func<TRequest, TResponse> handler)
            {
                this.OnRequestReceived((NetsyChannel _, TRequest request) => handler(request));
            }
            public void OnRequestReceived<TRequest, TResponse>(Func<NetsyChannel, TRequest, TResponse> handler)
            {
                Guard.NotNull(handler, nameof(handler));

                this.OnRequestReceived<TRequest, TResponse>((channel, package) => Task.FromResult(handler(channel, package)));
            }

            public void OnRequestReceived<TRequest, TResponse>(Func<TRequest, Task<TResponse>> handler)
            {
                this.OnRequestReceived((NetsyChannel _, TRequest request) => handler(request));
            }
            public void OnRequestReceived<TRequest, TResponse>(Func<NetsyChannel, TRequest, Task<TResponse>> handler)
            {
                Guard.NotNull(handler, nameof(handler));

                Func<NetsyChannel, object, Task<object>> wrappedHandler = async (channel, request) => (object)(await handler(channel, (TRequest)request));
                this._incomingRequestHandlers.AddOrUpdate(typeof(TRequest), wrappedHandler, (_, __) => wrappedHandler);
            }

            public void AddOutgoingRequest(Guid requestId, Action<object> responseHandler)
            {
                Guard.NotInvalidGuid(requestId, nameof(requestId));
                Guard.NotNull(responseHandler, nameof(responseHandler));

                this._outgoingRequests.AddOrUpdate(requestId, responseHandler, (_, __) => responseHandler);
            }

            public void HandlePackage(NetsyChannel channel, object package)
            {
                Guard.NotNull(package, nameof(package));

                if (this._packageHandlers.TryGetValue(package.GetType(), out var handlers))
                {
                    foreach (var handler in handlers)
                    {
                        handler.Invoke(channel, package);
                    }
                }
            }

            public async Task<(bool, object)> HandleIncomingRequest(NetsyChannel channel, object package)
            {
                Guard.NotNull(package, nameof(package));

                if (this._incomingRequestHandlers.TryGetValue(package.GetType(), out var handler))
                {
                    var result = await handler(channel, package);
                    return (true, result);
                }

                return (false, null);
            }

            public void HandleResponse(Guid requestId, object package)
            {
                Guard.NotInvalidGuid(requestId, nameof(requestId));
                Guard.NotNull(package, nameof(package));

                if (this._outgoingRequests.TryGetValue(requestId, out var handler))
                {
                    handler(package);
                }
            }
        }
        #endregion
    }
}