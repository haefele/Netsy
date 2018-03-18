using System;

namespace Netsy
{
    public class NetsyChannelDisconnectedEventArgs : EventArgs
    {
        public NetsyChannel Channel { get; }

        public NetsyChannelDisconnectedEventArgs(NetsyChannel channel)
        {
            Guard.NotNull(channel, nameof(channel));

            this.Channel = channel;
        }
    }
}