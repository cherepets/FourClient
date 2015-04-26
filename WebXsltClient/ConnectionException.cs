using System;

namespace WebServiceClient
{
    /// <summary>
    /// Exception that represents bad internet connection
    /// </summary>
    public class ConnectionException : Exception
    {
        internal ConnectionException()
            : base("Bad internet connection") { }
    }
}
