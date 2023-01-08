using System;
using System.Collections.Generic;

namespace RedStoneEmu.Database.MastodonEF
{
    public partial class StreamEntries
    {
        public int Id { get; set; }
        public int? AccountId { get; set; }
        public long? ActivityId { get; set; }
        public string ActivityType { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public bool Hidden { get; set; }
    }
}
