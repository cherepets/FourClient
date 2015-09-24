using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace FourClient.HtmlRender
{
    public static class HtmlRenderFactory
    {
        public static List<string> Renders => Types.Keys.ToList();

        public static IHtmlRender GetRender(string render = null)
            => Activator.CreateInstance(GetRenderType(render)) as IHtmlRender;

        private static Type GetRenderType(string render)
            => Types.ContainsKey(render ?? SettingsService.Render)
            ? Types[render ?? SettingsService.Render]
            : Types.Values.First();

        private static Dictionary<string, Type> Types
        {
            get
            {
                if (_types == null)
                    _types = GetDerivedTypes(typeof(IHtmlRender).GetTypeInfo())
                        .ToDictionary(t => t.Name, t => t.AsType());
                return _types;
            }
        }
        private static Dictionary<string, Type> _types;

        private static List<TypeInfo> GetDerivedTypes(TypeInfo typeInfo)
        {
            var assembly = typeInfo.Assembly;
            return assembly
                .DefinedTypes
                .Where(t => t != typeInfo && typeInfo.IsAssignableFrom(t))
                .ToList();
        }
    }
}
