using System;
using System.Collections.Generic;
using System.Linq;

namespace FourClient
{
    public enum StartUpType
    {
        Interesting, Sources, Feed, Collection
    }

    public static class StartUpTypeExt
    {
        public static string GetName(this StartUpType type)
        {
            var dict = GetDictionary();
            return dict.ContainsKey(type) ? dict[type] : null;
        }

        public static Dictionary<StartUpType, string> GetDictionary()
        {
            if (_dictionary == null)
                _dictionary =  Enum.GetValues(typeof(StartUpType))
                .Cast<StartUpType>()
                .ToDictionary(s => s, s => Localize(s));
            return _dictionary;
        }
        private static Dictionary<StartUpType, string> _dictionary;

        private static string Localize(StartUpType type)
        {
            switch (type)
            {
                case StartUpType.Interesting:
                    return "Интересное";
                case StartUpType.Sources:
                    return "Источники";
                case StartUpType.Feed:
                    return "Новостная лента";
                case StartUpType.Collection:
                    return "Коллекция";
            }
            return null;
        }
    }
}