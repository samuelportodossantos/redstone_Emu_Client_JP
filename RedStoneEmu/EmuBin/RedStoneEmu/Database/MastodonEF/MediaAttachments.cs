using System;
using System.Collections.Generic;

namespace RedStoneEmu.Database.MastodonEF
{
    public partial class MediaAttachments
    {
        public int Id { get; set; }
        public long? StatusId { get; set; }
        public string FileFileName { get; set; }
        public string FileContentType { get; set; }
        public int? FileFileSize { get; set; }
        public DateTime? FileUpdatedAt { get; set; }
        public string RemoteUrl { get; set; }
        public int? AccountId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string Shortcode { get; set; }
        public int Type { get; set; }
        public string FileMeta { get; set; }
    }
}
