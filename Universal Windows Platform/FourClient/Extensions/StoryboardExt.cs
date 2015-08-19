using System.Threading.Tasks;
using Windows.UI.Xaml.Media.Animation;

namespace FourClient.Extensions
{
    public static class StoryboardExt
    {
        public static async Task PlayAsync(this Storyboard storyboard)
        {
            storyboard.Begin();
            while(storyboard.GetCurrentState() == ClockState.Active)
            {
                await Task.Delay(100);
            }
        }
    }
}
