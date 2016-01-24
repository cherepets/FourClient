using System;

namespace FourClient.Data
{
    public class ConnectionException : Exception
    {
        internal ConnectionException()
            : base("Bad internet connection") { }
    }
}
