using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Netsy.Atom;

namespace Netsy
{
    public class NetsyClient
    {
        private readonly ConcurrentDictionary<Type, ConcurrentBag<Action<object>>> _packageHandlers;
        private readonly ConcurrentDictionary<Guid, Action<object>> _outgoingRequests;
        private readonly ConcurrentDictionary<Type, Func<object, object>> _incomingRequestHandlers;
        
        public AtomClient AtomClient { get; }
        public IPackageSerializer PackageSerializer { get; }

        public NetsyClient(AtomClient atomClient, IPackageSerializer packageSerializer)
        {
            this._packageHandlers = new ConcurrentDictionary<Type, ConcurrentBag<Action<object>>>();
            this._outgoingRequests = new ConcurrentDictionary<Guid, Action<object>>();
            this._incomingRequestHandlers = new ConcurrentDictionary<Type, Func<object, object>>();

            this.AtomClient = atomClient;
            this.PackageSerializer = packageSerializer;

            this.AtomClient.MessageReceived += this.AtomClientOnMessageReceived;
        }

        public async Task SendPackageAsync(object package)
        {
            var data = this.PackageSerializer.Serialize(package);
            await this.SendMessageAsync(NetsyMessage.Package(data));
        }

        public async Task<TResponse> SendRequestAsync<TResponse>(object request)
        {
            var data = this.PackageSerializer.Serialize(request);
            var message = NetsyMessage.Request(data);

            var result = new TaskCompletionSource<TResponse>();
            Action<object> responseHandler = (response) => result.SetResult((TResponse) response);

            this._outgoingRequests.AddOrUpdate(message.RequestId, responseHandler, (_, __) => responseHandler);

            await this.SendMessageAsync(message);

            return await result.Task;
        }

        public async Task SendMessageAsync(NetsyMessage message)
        {
            await this.AtomClient.SendMessageAsync(message.ToAtomMessage());
        }

        public void OnPackageReceived<T>(Action<T> handler)
        {
            this._packageHandlers.GetOrAdd(typeof(T), _ => new ConcurrentBag<Action<object>>()).Add(f => handler((T)f));
        }

        public void OnRequestReceived<TRequest, TResponse>(Func<TRequest, TResponse> handler)
        {
            Func<object, object> wrappedHandler = f => (object) handler((TRequest) f);
            this._incomingRequestHandlers.AddOrUpdate(typeof(TRequest), wrappedHandler, (_, __) => wrappedHandler);
        }

        private async void AtomClientOnMessageReceived(object sender, AtomClientMessageReceivedEventArgs e)
        {
            var netsyMessage = NetsyMessage.FromAtomMessage(e.Message.Data);
            var package = this.PackageSerializer.Deserialize(netsyMessage.Data);

            switch (netsyMessage.Kind)
            {
                case NetsyMessageKind.SendPackage:
                {
                    if (this._packageHandlers.TryGetValue(package.GetType(), out var handlers))
                    {
                        foreach (var handler in handlers)
                        {
                            handler.Invoke(package);
                        }
                    }
                }
                break;

                case NetsyMessageKind.Request:
                {
                    if (this._incomingRequestHandlers.TryGetValue(package.GetType(), out var handler))
                    {
                        var response = handler(package);
                        var responseData = this.PackageSerializer.Serialize(response);

                        var responseMessage = NetsyMessage.ResponseFor(responseData, netsyMessage.RequestId);
                        await this.SendMessageAsync(responseMessage);
                    }
                }
                break;

                case NetsyMessageKind.Response:
                {
                    if (this._outgoingRequests.TryGetValue(netsyMessage.RequestId, out var handler))
                    {
                        handler(package);
                    }
                }
                break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }

    public struct NetsyMessage
    {
        public byte[] Data { get;  }
        public NetsyMessageKind Kind { get; }
        public Guid RequestId { get; }

        public static NetsyMessage Package(byte[] data)
        {
            return new NetsyMessage(data, NetsyMessageKind.SendPackage, Guid.Empty);
        }

        public static NetsyMessage Request(byte[] data)
        {
            return new NetsyMessage(data, NetsyMessageKind.Request, Guid.NewGuid());
        }

        public static NetsyMessage ResponseFor(byte[] data, Guid requestId)
        {
            return new NetsyMessage(data, NetsyMessageKind.Response, requestId);
        }

        public static NetsyMessage FromAtomMessage(byte[] data)
        {
            var kind = (NetsyMessageKind) data[0];

            var requestId = Guid.Empty;
            int bytesToSkip = 0;

            switch (kind)
            {
                case NetsyMessageKind.Request:
                case NetsyMessageKind.Response:
                    var requestIdBytes = new byte[16];
                    Array.Copy(data, 1, requestIdBytes, 0, 16);
                    requestId = new Guid(requestIdBytes);
                    bytesToSkip = 16;

                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            var actualData = new byte[data.Length - 1 - bytesToSkip];
            Array.Copy(data, 1 + bytesToSkip, actualData, 0, actualData.Length);

            return new NetsyMessage(actualData, kind, requestId);
        }

        private NetsyMessage(byte[] data, NetsyMessageKind kind, Guid requestId)
        {
            this.Data = data;
            this.Kind = kind;
            this.RequestId = requestId;
        }

        public AtomMessage ToAtomMessage()
        {
            return AtomMessage.FromData(this.GetData());
        }

        private byte[] GetData()
        {
            var result = new byte[this.GetDataLength()];
            result[0] = (byte) this.Kind;

            switch (this.Kind)
            {
                case NetsyMessageKind.Request:
                case NetsyMessageKind.Response:
                    Array.Copy(this.RequestId.ToByteArray(), 0, result, 1, 16);
                    break;
            }

            Array.Copy(this.Data, 0, result, result.Length - this.Data.Length, this.Data.Length);
            return result;
        }

        private int GetDataLength()
        {
            int length = this.Data.Length;
            length += 1; //NetsyMessageKind
            switch (this.Kind)
            {
                case NetsyMessageKind.SendPackage:
                    break;
                case NetsyMessageKind.Request:
                    length += 16; //RequestId
                    break;
                case NetsyMessageKind.Response:
                    length += 16; //RequestId
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            return length;
        }
    }

    public enum NetsyMessageKind
    {
        SendPackage = 0,
        Request = 1,
        Response = 2
    }

    public interface IPackageSerializer
    {
        byte[] Serialize(object package);
        object Deserialize(byte[] data);
    }
}