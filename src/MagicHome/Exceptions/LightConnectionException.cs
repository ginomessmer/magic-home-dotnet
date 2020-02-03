using System;

namespace MagicHome.Exceptions
{
    public class LightConnectionException : Exception
    {
        public override string Message { get; }

        public LightConnectionException(string message)
        {
            Message = message;
        }
    }
}