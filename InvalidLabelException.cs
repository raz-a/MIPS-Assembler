using System;
using System.Runtime.Serialization;

namespace MIPSAssembler
{
    [Serializable]
    internal class InvalidLabelException : Exception
    {
        public InvalidLabelException()
        {
        }

        public InvalidLabelException(string message) : base("Invalid Label: " + message)
        {
        }

        public InvalidLabelException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected InvalidLabelException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}