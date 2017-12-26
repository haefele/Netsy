using System;

namespace Netsy.Atom
{
    public class AtomChannelMessageReceivedEventArgs : EventArgs
    {
        public AtomMessage Message { get; }

        public AtomChannelMessageReceivedEventArgs(AtomMessage message)
        {
            Guard.NotNull(message, nameof(message));

            this.Message = message;
        }
    }
}