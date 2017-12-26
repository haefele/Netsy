using System;

namespace Netsy.Atom
{
    public class AtomClientMessageReceivedEventArgs : EventArgs
    {
        public AtomMessage Message { get; }

        public AtomClientMessageReceivedEventArgs(AtomMessage message)
        {
            Guard.NotNull(message, nameof(message));

            this.Message = message;
        }
    }
}