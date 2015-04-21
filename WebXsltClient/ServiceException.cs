using System;

namespace WebXslt
{
    /// <summary>
    /// Exception that represents exception on the server-side
    /// </summary>
    public class ServiceException : Exception
    {
        internal ServiceException (string message)
            : base(message) { }
    }
}
