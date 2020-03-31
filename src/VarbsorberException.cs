using System;
using System.Runtime.Serialization;

namespace Varbsorb
{
    [Serializable]
    internal class VarbsorberException : Exception
    {
        public VarbsorberException()
        {
        }

        public VarbsorberException(string message)
            : base(message)
        {
        }

        public VarbsorberException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        protected VarbsorberException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
