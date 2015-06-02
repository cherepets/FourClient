using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Animation;

namespace FourClient.Extensions
{
    public static class FrameworkElementExt
    {
        public static void Animate(this FrameworkElement grid)
        {
            var storyboard = new Storyboard();
            var anim = new DoubleAnimationUsingKeyFrames();
            Storyboard.SetTarget(anim, grid);
            Storyboard.SetTargetProperty(anim, "Opacity");
            anim.KeyFrames.Add(new EasingDoubleKeyFrame
            {
                KeyTime = KeyTime.FromTimeSpan(TimeSpan.Zero),
                Value = 0
            });
            anim.KeyFrames.Add(new EasingDoubleKeyFrame
            {
                KeyTime = KeyTime.FromTimeSpan(TimeSpan.FromSeconds(1)),
                Value = 1
            });
            storyboard.Children.Add(anim);
            storyboard.Begin();
        }
    }
}
