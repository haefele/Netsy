namespace Netsy.Atom
{
    public enum AtomServerState
    {
        Starting,
        Started,
        Stopping,
        Stopped
    }

    public static class AtomServerStateExtensions
    {
        public static bool CanStart(this AtomServerState self)
        {
            Guard.NotInvalidEnum(self, nameof(self));

            return self == AtomServerState.Stopped;
        }

        public static bool CanStop(this AtomServerState self)
        {
            Guard.NotInvalidEnum(self, nameof(self));

            return self == AtomServerState.Started;
        }
    }
}