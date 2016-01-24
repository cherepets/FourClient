using FourClient.Data.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace FourClient.Data
{
    public class CollectionWithCache<T> : ObservableCollection<T>
    {
        public event EventHandler<List<T>> LoadCompleted;
        public event EventHandler<Exception> LoadFailed;

        public ICache<T> Cache { get; }

        public CollectionWithCache(ICache<T> cache, Task<List<T>> dataTask)
        {
            Cache = cache;
            var cached = Cache?.Get();
            if (!dataTask.IsCompleted && cached != null)
                foreach (var item in cached)
                {
                    Add(item);
                }
            WaitForData(dataTask);
        }

        private async void WaitForData(Task<List<T>> dataTask)
        {
            try
            {
                var data = await dataTask;
                foreach (var item in data)
                {
                    var index = data.IndexOf(item);
                    if (index >= Count) Add(item);
                    else
                    if (!this[index].Equals(item))
                    {
                        RemoveAt(index);
                        Insert(index, item);
                    }
                }
                Cache?.Put(data);
                LoadCompleted?.Invoke(this, data);
            }
            catch (Exception exception)
            {
                LoadFailed?.Invoke(this, exception);
            }
        }
    }
}
