using System.Collections.Generic;

namespace FourClient.Data.Interfaces
{
    public interface ISourceSelector
    {
        IEnumerable<Source> Sources { get; }
    }
}
