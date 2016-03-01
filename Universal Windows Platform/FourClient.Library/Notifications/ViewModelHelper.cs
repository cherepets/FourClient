using System.Reflection;

namespace FourClient.Library.Notifications
{
    public static class ViewModelHelper
    {
        public static T GenerateDummy<T>(T obj)
        {
            var type = obj.GetType();
            var props = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (var prop in props)
                prop.SetValue(obj, $"[{prop.Name}]");
            return obj;
        }

        public static string FillXml<T>(T obj, string xml)
        {
            var type = obj.GetType();
            var props = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (var prop in props)
            {
                var val = prop.GetValue(obj) as string;
                xml = xml.Replace($"[{prop.Name}]", val);
            }
            return xml;
        }
    }
}
