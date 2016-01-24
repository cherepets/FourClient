using System.Collections.Generic;

namespace FourClient.Data.Interfaces
{
    public interface ICache<T>
    {
        void Put(List<T> values);
        List<T> Get();
    }
}
