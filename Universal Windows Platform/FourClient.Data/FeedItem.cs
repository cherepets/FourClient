namespace FourClient.Data
{
    public class FeedItem
    {
        public string Title { get; set; }
        public string Image { get; set; }
        public string Link { get; set; }
        public string Avatar { get; set; }
        public string FullLink { get; set; }
        public string CommentLink { get; set; }

        public override bool Equals(object obj)
        {
            var item = obj as FeedItem;
            if (item == null) return false;
            if (!SafeCompare(Title, item.Title)) return false;
            if (!SafeCompare(Image, item.Image)) return false;
            if (!SafeCompare(Link, item.Link)) return false;
            if (!SafeCompare(Avatar, item.Avatar)) return false;
            if (!SafeCompare(FullLink, item.FullLink)) return false;
            if (!SafeCompare(CommentLink, item.CommentLink)) return false;
            return true;
        }

        private bool SafeCompare(string s1, string s2)
            => string.IsNullOrEmpty(s1) && string.IsNullOrEmpty(s2)
                ? true
                : s1 == s2;

        public override int GetHashCode() => base.GetHashCode();
    }
}
