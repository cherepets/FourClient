using System;

namespace FourClient.Data
{
    public class WebServiceException : Exception
    {
        internal WebServiceException (string message)
            : base(message) { }
    }
}
