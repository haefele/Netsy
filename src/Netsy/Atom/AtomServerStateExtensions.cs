namespace Netsy.Atom
{
    public static class AtomServerStateExtensions
    {
        public static bool CanBeStarted(this AtomServerState self)
        {
            return self == AtomServerState.Stopped;
        }

        public static bool CanBeStopped(this AtomServerState self)
        {
            return self == AtomServerState.Started;
        }
    }
}