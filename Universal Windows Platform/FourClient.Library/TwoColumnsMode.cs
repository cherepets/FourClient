using System;
using System.Collections.Generic;
using System.Linq;

namespace FourClient.Library
{
    public enum TwoColumnsMode
    {
        Default, Never, Always
    }

    public static class TwoColumnsModeExt
    {
        public static string GetName(this TwoColumnsMode type)
        {
            var dict = GetDictionary();
            return dict.ContainsKey(type) ? dict[type] : null;
        }

        public static Dictionary<TwoColumnsMode, string> GetDictionary()
        {
            if (_dictionary == null)
                _dictionary = Enum.GetValues(typeof(TwoColumnsMode))
                .Cast<TwoColumnsMode>()
                .ToDictionary(s => s, s => Localize(s));
            return _dictionary;
        }
        private static Dictionary<TwoColumnsMode, string> _dictionary;

        private static string Localize(TwoColumnsMode type)
        {
            switch (type)
            {
                case TwoColumnsMode.Default:
                    return "По умолчанию";
                case TwoColumnsMode.Always:
                    return "Всегда";
                case TwoColumnsMode.Never:
                    return "Никогда";
            }
            return null;
        }
    }
}
