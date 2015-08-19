using Windows.UI.Xaml.Controls;

namespace FourClient.Extensions
{
    public static class ListBoxExt
    {
        public static string CurrentValue(this ListBox listBox)
        {
            if (listBox.SelectedItem == null) listBox.SelectedItem = listBox.Items[0];
            return (listBox.SelectedItem as ListBoxItem).Content.ToString();
        }
    }
}
