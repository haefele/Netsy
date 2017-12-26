using System;

namespace Netsy.Atom
{
    public class AtomChannelDisconnectedEventArgs : EventArgs
    {
        public AtomChannel Channel { get; }

        public AtomChannelDisconnectedEventArgs(AtomChannel channel)
        {
            this.Channel = channel;
        }
    }
}