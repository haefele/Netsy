using System;

namespace Netsy.Atom
{
    public class AtomChannelDisconnectedEventArgs : EventArgs
    {
        public AtomChannel Channel { get; }

        public AtomChannelDisconnectedEventArgs(AtomChannel channel)
        {
            Guard.NotNull(channel, nameof(channel));

            this.Channel = channel;
        }
    }
}