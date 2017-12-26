namespace Netsy.Atom
{
    public enum AtomClientState
    {
        Connecting,
        Connected,
        Disconnecting,
        Disconnected,
    }

    public static class AtomClientStateExtensions
    {
        public static bool CanConnect(this AtomClientState self)
        {
            Guard.NotInvalidEnum(self, nameof(self));

            return self == AtomClientState.Disconnected;
        }

        public static bool CanDisconnect(this AtomClientState self)
        {
            Guard.NotInvalidEnum(self, nameof(self));

            return self == AtomClientState.Connected;
        }
    }
}