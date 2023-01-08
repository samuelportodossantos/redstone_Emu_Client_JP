using System;
using System.Collections.Generic;

namespace RedStoneEmu.Database.MastodonEF
{
    public partial class Notifications
    {
        public int Id { get; set; }
        public int? AccountId { get; set; }
        public long? ActivityId { get; set; }
        public string ActivityType { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public int? FromAccountId { get; set; }
    }
}
