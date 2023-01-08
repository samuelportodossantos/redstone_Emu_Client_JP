using System;
using System.Collections.Generic;

namespace RedStoneEmu.Database.MastodonEF
{
    public partial class DomainBlocks
    {
        public int Id { get; set; }
        public string Domain { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public int? Severity { get; set; }
        public bool? RejectMedia { get; set; }
    }
}
