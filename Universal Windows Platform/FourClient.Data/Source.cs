using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace FourClient.Data
{
    public class Source
    {
        public Feed MainFeed => new Feed
        {
            NewsType = NewsTypes.First().Key,
            SearchMode = false,
            Source = this
        };

        public string MySources { get; set; }
        public string Name { get; set; }
        public string Prefix { get; set; }
        public string ImageUrl { get; set; }
        public string Base64Types
        {
            get { return _base64types; }
            set
            {
                if (!string.IsNullOrEmpty(value))
                {
                    _base64types = value;
                    var bytes = Convert.FromBase64String(_base64types);
                    var decoded = Encoding.UTF8.GetString(bytes, 0, bytes.Length);
                    var xml = XDocument.Parse(decoded);
                    var pairs = xml.Descendants().Where(d => d.Name.LocalName == "Pair");
                    NewsTypes = new Dictionary<string, string>();
                    foreach (var pair in pairs)
                    {
                        NewsTypes.Add(
                            pair.Attribute("key").Value,
                            pair.Attribute("value").Value);
                        Base64Types = string.Empty;
                    }
                }
            }
        }
        public bool Enabled { get; set; }
        public string Availability
        {
            get { return Enabled.ToString(); }
            set
            {
                Enabled = value == "true";
            }
        }

        public bool Searchable { get; set; }
        public string Searchability
        {
            get { return Searchable.ToString(); }
            set
            {
                Searchable = value == "true";
            }
        }
        public Dictionary<string, string> NewsTypes { get; set; }

        private string _base64types;
        
        public override bool Equals(object obj)
        {
            var source = obj as Source;
            if (source == null) return false;
            if (Name != source.Name) return false;
            if (Prefix != source.Prefix) return false;
            if (ImageUrl != source.ImageUrl) return false;
            if (Base64Types != source.Base64Types) return false;
            if (Availability != source.Availability) return false;
            if (Searchability != source.Searchability) return false;
            return true;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
