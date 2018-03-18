using System;

namespace Netsy
{
    public class NetsyChannelConnectedEventArgs : EventArgs
    {
        public NetsyChannel Channel { get; }

        public NetsyChannelConnectedEventArgs(NetsyChannel channel)
        {
            Guard.NotNull(channel, nameof(channel));

            this.Channel = channel;
        }
    }
}