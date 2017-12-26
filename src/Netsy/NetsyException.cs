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
        }

        public NetsyException(string message, Exception inner)
            : base(message, inner)
        {
        }

        protected NetsyException(SerializationInfo info, StreamingContext context) 
            : base(info, context)
        {
        }
    }
}