using System;

namespace Netsy.Atom
{
    public class AtomChannelConnectedEventArgs : EventArgs
    {
        public AtomChannel Channel { get; }

        public AtomChannelConnectedEventArgs(AtomChannel channel)
        {
            this.Channel = channel;
        }
    }
}