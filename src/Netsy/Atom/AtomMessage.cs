using System;

namespace Netsy.Atom
{
    public class AtomMessage
    {
        public byte[] Data { get; private set; }

        #region Factory Methods
        internal static AtomMessage Incoming(byte[] data)
        {
            Guard.NotNull(data, nameof(data));

            return new AtomMessage
            {
                Data = data
            };
        }

        public static AtomMessage FromData(byte[] data)
        {
            Guard.NotNull(data, nameof(data));

            return new AtomMessage
            {
                Data = data
            };
        }

        private AtomMessage()
        {

        }
        #endregion
    }
}