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
            return self == AtomClientState.Disconnected;
        }

        public static bool CanDisconnect(this AtomClientState self)
        {
            return self == AtomClientState.Connected;
        }
    }
}