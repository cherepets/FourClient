using System;

namespace FourClient
{
    public class Article
    {
        public string Prefix { get; set; }
        public string Title { get; set; }
        public string Image { get; set; }
        public string Link { get; set; }
        public string Avatar { get; set; }
        public string FullLink { get; set; }
        public string CommentLink { get; set; }
        public string Html { get; set; }
        public bool InCollection { get; set; }
        public DateTime CreatedOn { get; set; }
    }
}
