namespace Netsy.Atom
{
    public static class AtomServerStateExtensions
    {
        public static bool CanStart(this AtomServerState self)
        {
            return self == AtomServerState.Stopped;
        }

        public static bool CanStop(this AtomServerState self)
        {
            return self == AtomServerState.Started;
        }
    }
}