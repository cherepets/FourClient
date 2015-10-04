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

        public static Color Lighten(this Color color) => Color.FromArgb(color.A, Lighter(color.R), Lighter(color.G), Lighter(color.B));
        public static Color Darken(this Color color) => Color.FromArgb(color.A, Darker(color.R), Darker(color.G), Darker(color.B));

        private static byte Lighter(int b) => (byte)((b + byte.MaxValue) / 2);
        private static byte Darker(int b) => (byte)(b / 2);
    }
}
