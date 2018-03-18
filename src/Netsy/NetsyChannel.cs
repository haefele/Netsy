using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Netsy.Atom;

namespace Netsy
{
    public class NetsyChannel
    {
        private readonly AtomChannel _atomChannel;
        private readonly NetsyConnectionHelper _connectionHelper;

        public IDictionary<string, object> Data { get; }

        internal NetsyChannel(AtomChannel atomChannel, IPackageSerializer packageSerializer, NetsyConnectionHelper.HandlerRegistry handlerRegistry)
        {
            Guard.NotNull(atomChannel, nameof(atomChannel));
            Guard.NotNull(packageSerializer, nameof(packageSerializer));
            Guard.NotNull(handlerRegistry, nameof(handlerRegistry));

            this._atomChannel = atomChannel;
            this._atomChannel.MessageReceived += this.AtomChannelOnMessageReceived;
            this._connectionHelper = new NetsyConnectionHelper(packageSerializer, atomChannel.SendMessageAsync, handlerRegistry);

            this.Data = new Dictionary<string, object>();
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

        public Task DisconnectAsync()
        {
            return this._atomChannel.DisconnectAsync();
        }

        private async void AtomChannelOnMessageReceived(object sender, AtomChannelMessageReceivedEventArgs e)
        {
            await this._connectionHelper.HandleAtomMessage(this, e.Message);
        }
    }
}