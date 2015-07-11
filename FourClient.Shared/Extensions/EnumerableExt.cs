using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace FourClient.Extensions
{
    internal static class EnumerableExt
    {
        public static void ForEach<T>(this IEnumerable<T> enumerable, Action<T> action)
        {
            foreach (var item in enumerable)
                action(item);
        }

        public static ObservableCollection<T> ToObservable<T>(this IEnumerable<T> enumerable)
        {
            var observable = new ObservableCollection<T>();
            enumerable.ForEach(observable.Add);
            return observable;
        }
    }
}
