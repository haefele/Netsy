using System;
using System.Runtime.Serialization;

namespace Netsy
{
    [Serializable]
    public class NetsyException : Exception
    {
        public NetsyException()
        {
        }

        public NetsyException(string message) 
            : base(message)
        {
            Guard.NotNullOrWhiteSpace(message, nameof(message));
        }

        public NetsyException(string message, Exception inner)
            : base(message, inner)
        {
            Guard.NotNullOrWhiteSpace(message, nameof(message));
            Guard.NotNull(inner, nameof(inner));
        }

        protected NetsyException(SerializationInfo info, StreamingContext context) 
            : base(info, context)
        {
            Guard.NotNull(info, nameof(info));
            Guard.NotNull(context, nameof(context));
        }
    }
}