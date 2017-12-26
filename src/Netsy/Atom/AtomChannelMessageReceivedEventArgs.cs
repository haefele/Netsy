using System;

namespace Netsy.Atom
{
    public class AtomChannelMessageReceivedEventArgs : EventArgs
    {
        public AtomMessage Message { get; }

        public AtomChannelMessageReceivedEventArgs(AtomMessage message)
        {
            this.Message = message;
        }
    }
}