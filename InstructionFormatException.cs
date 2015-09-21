using System;
using System.Runtime.Serialization;

namespace MIPSAssembler
{
    [Serializable]
    internal class InstructionFormatException : Exception
    {
        public InstructionFormatException()
        {
        }

        public InstructionFormatException(string message) : base(message)
        {
        }

        public InstructionFormatException(string message, Exception innerException) : base(message, innerException)
        {
        }

        public InstructionFormatException(string v, uint address) : base(v + " at " + address)
        {
        }

        protected InstructionFormatException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}