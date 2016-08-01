using FourToolkit.UI;
using FourToolkit.UI.Extensions;
using System.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Animation;

namespace FourClient
{
    public class GridViewFlipper : UserControl, IFlipper
    {
        public IList ItemsSource
        {
            get
            {
                return _view.ItemsSource as IList;
            }
            set
            {
                _view.ItemsSource = value;
            }
        }

        public int SelectedIndex
        {
            get
            {
                return _view.SelectedIndex;
            }
            set
            {
                _view.SelectedIndex = value;
            }
        }

        public object SelectedItem => ItemsSource?[SelectedIndex];
        
        public ItemsPanelTemplate ItemsPanel
        {
            get
            {
                return _view.ItemsPanel;
            }
            set
            {
                _view.ItemsPanel = value;
            }
        }

        public DataTemplate ItemTemplate
        {
            get
            {
                return _view.ItemTemplate;
            }
            set
            {
                _view.ItemTemplate = value;
            }
        }
        
        public Style ItemContainerStyle
        {
            get
            {
                return _view.ItemContainerStyle;
            }
            set
            {
                _view.ItemContainerStyle = value;
            }
        }

        public TransitionCollection ItemContainerTransitions
        {
            get
            {
                return _view.ItemContainerTransitions;
            }
            set
            {
                _view.ItemContainerTransitions = value;
            }
        }

        public ScrollBarVisibility VerticalScrollBarVisibility
        {
            get
            {
                var scroll = _view.GetScrollViewer();
                return scroll?.VerticalScrollBarVisibility ?? default(ScrollBarVisibility);
            }
            set
            {
                var scroll = _view.GetScrollViewer();
                if (scroll != null) scroll.VerticalScrollBarVisibility = value;
            }
        }

        private Windows.UI.Xaml.Controls.GridView _view = new Windows.UI.Xaml.Controls.GridView();

        public GridViewFlipper()
        {
            Content = _view;
        }
    }
}
