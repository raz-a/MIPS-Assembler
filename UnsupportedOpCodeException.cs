using System;
using System.Runtime.Serialization;

namespace MIPSAssembler
{
    [Serializable]
    internal class UnsupportedOpCodeException : Exception
    {
        public UnsupportedOpCodeException()
        {
        }

        public UnsupportedOpCodeException(string message) : base(message)
        {
        }

        public UnsupportedOpCodeException(string message, Exception innerException) : base(message, innerException)
        {
        }

        public UnsupportedOpCodeException(string v, uint address) : base("Unsupported OpCode [" + v + "] at line " + address)
        {
        }

        protected UnsupportedOpCodeException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}