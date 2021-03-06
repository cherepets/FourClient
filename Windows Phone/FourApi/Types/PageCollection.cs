﻿using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using WebServiceClient;
using Windows.Foundation;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace FourAPI.Types
{
    public class PageCollection : ObservableCollection<FourItem>, ISupportIncrementalLoading
    {
        /// <summary>
        /// Source
        /// </summary>
        public FourSource Source { get; set; }
        /// <summary>
        /// Is it a search
        /// </summary>
        public bool SearchMode { get; set; }
        /// <summary>
        /// Type of news
        /// </summary>
        public string NewsType { get; set; }
        /// <summary>
        /// Search query
        /// </summary>
        public string SearchQuery { get; set; }

        private int _pageNumber = 1;

        public delegate void LoadFailedEventHandler();
        /// <summary>
        /// Fires when loading of new items failed
        /// </summary>
        public event LoadFailedEventHandler LoadFailed;
        public delegate void LoadStartedEventHandler();
        /// <summary>
        /// Fires when loading of new items starts
        /// </summary>
        public event LoadStartedEventHandler LoadStarted;
        public delegate void LoadCompletedEventHandler();
        /// <summary>
        /// Fires when loading of new items completes
        /// </summary>
        public event LoadCompletedEventHandler LoadCompleted;
        public delegate void ServiceExceptionHandler(Exception ex);
        /// <summary>
        /// Fires when service exception in thrown
        /// </summary>
        public event ServiceExceptionHandler ServiceExceptionThrown;
        public delegate void ConnectionExceptionHandler(Exception ex);
        /// <summary>
        /// Fires when connection exception is thrown
        /// </summary>
        public event ConnectionExceptionHandler ConnectionExceptionThrown;

        public bool HasMoreItems
        {
            get { return _hasMoreItems; }
            set { _hasMoreItems = true; }
        }

        private bool _hasMoreItems = true;

        public IAsyncOperation<LoadMoreItemsResult> LoadMoreItemsAsync(uint count)
        {
            var coreDispatcher = Window.Current.Dispatcher;
            return Task.Run(async () =>
            {
                try
                {
                    await coreDispatcher.RunAsync(CoreDispatcherPriority.Normal,
                    () => LoadStarted?.Invoke());
                    var newPages = SearchMode ?
                        await Source.SearchPageAsync(SearchQuery, _pageNumber) :
                        await Source.GetItemsAsync(NewsType, _pageNumber);
                    _pageNumber++;
                    if (newPages.Count == 0) throw new Exception("No items");
                    await coreDispatcher.RunAsync(CoreDispatcherPriority.Normal,
                    () =>
                    {
                        foreach (var page in newPages)
                            if (!this.Any(p => p.Link == page.Link)) Add(page);
                    });
                    return new LoadMoreItemsResult() { Count = (uint)newPages.Count };
                }
                catch (ServiceException se)
                {
                    await coreDispatcher.RunAsync(CoreDispatcherPriority.Normal,
                    () => ServiceExceptionThrown?.Invoke(se));
                    _hasMoreItems = false;
                    return new LoadMoreItemsResult() { Count = 0 };
                }
                catch (ConnectionException se)
                {
                    await coreDispatcher.RunAsync(CoreDispatcherPriority.Normal,
                    () => ConnectionExceptionThrown?.Invoke(se));
                    _hasMoreItems = false;
                    return new LoadMoreItemsResult() { Count = 0 };
                }
                catch
                {
                    if (Count == 0)
                    {
                        await coreDispatcher.RunAsync(CoreDispatcherPriority.Normal,
                        () => LoadFailed?.Invoke());
                    }
                    _hasMoreItems = false;
                    return new LoadMoreItemsResult() { Count = 0 };
                }
                finally
                {
                    await coreDispatcher.RunAsync(CoreDispatcherPriority.Normal,
                    () => LoadCompleted?.Invoke());
                }
            }).AsAsyncOperation();
        }
    }
}
