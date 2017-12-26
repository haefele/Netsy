using System;

namespace Netsy.Atom
{
    public class AtomChannelConnectedEventArgs : EventArgs
    {
        public AtomChannel Channel { get; }

        public AtomChannelConnectedEventArgs(AtomChannel channel)
        {
            Guard.NotNull(channel, nameof(channel));

            this.Channel = channel;
        }
    }
}