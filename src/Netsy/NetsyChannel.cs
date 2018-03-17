using System;
using System.Threading.Tasks;
using Netsy.Atom;

namespace Netsy
{
    public class NetsyChannel
    {
        private readonly NetsyConnectionHelper _connectionHelper;

        public AtomChannel AtomChannel { get; }

        internal NetsyChannel(AtomChannel atomChannel, NetsyConnectionHelper connectionHelper)
        {
            this._connectionHelper = connectionHelper;

            this.AtomChannel = atomChannel;
            this.AtomChannel.MessageReceived += this.AtomChannelOnMessageReceived;
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

        private async void AtomChannelOnMessageReceived(object sender, AtomChannelMessageReceivedEventArgs e)
        {
            await this._connectionHelper.HandleAtomMessage(e.Message);
        }
    }
}