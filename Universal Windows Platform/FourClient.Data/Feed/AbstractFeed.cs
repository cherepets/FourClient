using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace FourClient.Data.Feed
{
    public class AbstractFeed : ObservableCollection<FeedItem>, ISupportIncrementalLoading
    {
        public Source Source { get; set; }
        public bool SearchMode { get; set; }
        public string Topic { get; set; }
        public string SearchTerm { get; set; }

        private int _pageNumber = 1;

        public delegate void LoadFailedEventHandler();
        public event LoadFailedEventHandler LoadFailed;
        public delegate void LoadStartedEventHandler();
        public event LoadStartedEventHandler LoadStarted;
        public delegate void LoadCompletedEventHandler();
        public event LoadCompletedEventHandler LoadCompleted;
        public delegate void ServiceExceptionHandler(Exception ex);
        public event ServiceExceptionHandler ServiceExceptionThrown;
        public delegate void ConnectionExceptionHandler(Exception ex);
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
                    var newPages = SearchMode
                        ? await Api.SearchPageAsync(Source.Prefix, SearchTerm, _pageNumber)
                        : await Api.GetItemsAsync(Source, Topic, _pageNumber);
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
                catch (WebServiceException se)
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

        public AbstractFeed Clone() => new ClonedFeed(this);
    }
}
