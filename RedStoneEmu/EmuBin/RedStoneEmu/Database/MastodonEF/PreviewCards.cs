using System;
using System.Collections.Generic;

namespace RedStoneEmu.Database.MastodonEF
{
    public partial class PreviewCards
    {
        public int Id { get; set; }
        public long? StatusId { get; set; }
        public string Url { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string ImageFileName { get; set; }
        public string ImageContentType { get; set; }
        public int? ImageFileSize { get; set; }
        public DateTime? ImageUpdatedAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public int Type { get; set; }
        public string Html { get; set; }
        public string AuthorName { get; set; }
        public string AuthorUrl { get; set; }
        public string ProviderName { get; set; }
        public string ProviderUrl { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
    }
}
