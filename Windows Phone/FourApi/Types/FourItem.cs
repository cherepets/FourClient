namespace FourAPI.Types
{
    /// <summary>
    /// Item of the list of articles
    /// </summary>
    public struct FourItem
    {
        /// <summary>
        /// Article title
        /// </summary>
        public string Title { get; set; }
        /// <summary>
        /// Article thumbnail url
        /// </summary>
        public string Image { get; set; }
        /// <summary>
        /// Article address that used within FourAPI
        /// </summary>
        public string Link { get; set; }
        /// <summary>
        /// Source thumbnail url
        /// </summary>
        public string Avatar { get; set; }
        /// <summary>
        /// Article web address
        /// </summary>
        public string FullLink { get; set; }
        /// <summary>
        /// Comments web address
        /// </summary>
        public string CommentLink { get; set; }
    }
}
