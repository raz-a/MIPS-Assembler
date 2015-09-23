using System;
using System.Runtime.Serialization;

namespace MIPSAssembler
{
    [Serializable]
    internal class InvalidRegisterException : Exception
    {
        public InvalidRegisterException()
        {
        }

        public InvalidRegisterException(string message) : base("Invalid register " + message)
        {
        }

        public InvalidRegisterException(string message, Exception innerException) : base(message, innerException)
        {
        }

        public InvalidRegisterException(string v, uint address) : base("Invalid register " + v + " at address " + address + ".")
        {
        }

        protected InvalidRegisterException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}