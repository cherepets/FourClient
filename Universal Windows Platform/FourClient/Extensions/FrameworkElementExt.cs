using System;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media.Animation;

namespace FourClient.Extensions
{
    public static class FrameworkElementExt
    {
        public static void Animate(this FrameworkElement element)
        {
            var storyboard = new Storyboard();
            var anim = new DoubleAnimationUsingKeyFrames();
            Storyboard.SetTarget(anim, element);
            Storyboard.SetTargetProperty(anim, "Opacity");
            anim.KeyFrames.Add(new EasingDoubleKeyFrame
            {
                KeyTime = KeyTime.FromTimeSpan(TimeSpan.Zero),
                Value = 0
            });
            anim.KeyFrames.Add(new EasingDoubleKeyFrame
            {
                KeyTime = KeyTime.FromTimeSpan(TimeSpan.FromSeconds(0.2)),
                Value = 1
            });
            storyboard.Children.Add(anim);
            storyboard.Begin();
        }

        public static Point GetPosition(this FrameworkElement element) => element
            .TransformToVisual(Window.Current.Content).TransformPoint(new Point());
    }
}
