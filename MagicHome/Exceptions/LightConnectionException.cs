using System;

namespace MagicHome
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