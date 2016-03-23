using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;

namespace FourClient
{
    public class FilteredObservableCollection<T> : ObservableCollection<T>
    {
        private Func<T, bool> _filter;
        private ObservableCollection<T> _collection;

        public FilteredObservableCollection(ObservableCollection<T> collection, Expression<Func<T, bool>> filter)
        {
            _filter = filter.Compile();
            _collection = collection;
            Recheck();
            collection.CollectionChanged += Collection_CollectionChanged;
            foreach (var member in new MemberDetector().Detect(filter))
            {
                var propertyNotifier = member as INotifyPropertyChanged;
                if (propertyNotifier != null)
                    propertyNotifier.PropertyChanged += (s, a) => Recheck();
            }
        }

        private void Recheck()
        {
            foreach (var item in _collection)
            {
                AddChecked(item);
                RemoveChecked(item);
            }
            Sort();
        }

        private void Sort()
        {
            var ordered = this.OrderBy(e => _collection.IndexOf(e)).ToList();
            foreach (var item in ordered)
                Move(IndexOf(item), ordered.IndexOf(item));
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

        #region MemberDetector
        class MemberDetector : ExpressionVisitor
        {
            private List<string> _visited;
            private List<object> _detected;

            private object _lock = new object();

            public List<object> Detect(Expression expression)
            {
                // Prevent mixed states
                lock (_lock)
                {
                    _visited = new List<string>();
                    _detected = new List<object>();
                    Visit(expression);
                    return _detected;
                }
            }

            protected override Expression VisitMember(MemberExpression node)
            {
                if (!_visited.Contains(node.Member.Name))
                {
                    _visited.Add(node.Member.Name);
                    var value = GetValue(node);
                    if (value != null) _detected.Add(value);
                }
                return base.VisitMember(node);
            }

            private object GetValue(MemberExpression member)
            {
                try
                {
                    return Expression.Lambda<Func<object>>(Expression.Convert(member, typeof(object)))
                      .Compile().Invoke();
                }
                catch
                {
                    return null;
                }
            }
        }
        #endregion
    }
}
