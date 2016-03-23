using System;
using System.Collections.ObjectModel;
using System.Linq.Expressions;

namespace FourClient.Collections.Decoration.ObservableCollection
{
    public static class ObservableCollectionExt
    {
        public static ObservableCollection<T> AttachFilter<T>(this ObservableCollection<T> collection, Expression<Func<T, bool>> filter)
            => new FilterDecorator<T>(collection, filter);
    }
}
