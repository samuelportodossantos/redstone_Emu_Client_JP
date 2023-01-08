using System;
using System.Collections.Generic;

namespace RedStoneEmu.Database.MastodonEF
{
    public partial class Imports
    {
        public int Id { get; set; }
        public int AccountId { get; set; }
        public int Type { get; set; }
        public bool? Approved { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string DataFileName { get; set; }
        public string DataContentType { get; set; }
        public int? DataFileSize { get; set; }
        public DateTime? DataUpdatedAt { get; set; }
    }
}
