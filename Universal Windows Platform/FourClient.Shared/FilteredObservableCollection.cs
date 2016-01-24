using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace FourClient
{
    public class FilteredObservableCollection<T> : ObservableCollection<T>
    {
        private Func<T, bool> _filter;
        private ObservableCollection<T> _collection;

        public FilteredObservableCollection(ObservableCollection<T> collection, Func<T, bool> filter)
        {
            _filter = filter;
            _collection = collection;
            Recheck();
            collection.CollectionChanged += Collection_CollectionChanged;
        }

        public void Recheck()
        {
            foreach (var item in _collection)
            {
                AddChecked(item);
                RemoveChecked(item);
            }
        }

        private void Collection_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    foreach (var item in e.NewItems)
                        AddChecked((T)item);
                    break;
                case NotifyCollectionChangedAction.Remove:
                    foreach (var item in e.OldItems)
                        RemoveChecked((T)item);
                    break;
                case NotifyCollectionChangedAction.Reset:
                    Clear();
                    break;
                default:
                    throw new NotImplementedException();
            }
        }

        private void AddChecked(T item)
        {
            if (_filter(item) && !Contains(item))
                Add(item);
        }

        private void RemoveChecked(T item)
        {
            if (!_filter(item) && Contains(item))
                Remove(item);
        }
    }
}
