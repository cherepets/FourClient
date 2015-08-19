using Windows.UI;

namespace FourClient.Extensions
{
    public static class ColorExt
    {
        public static string ToRGBString(this Color color)
        {
            var argb = color.ToString();
            var rgb = argb.Remove(1, 2);
            return rgb;
        }
    }
}
