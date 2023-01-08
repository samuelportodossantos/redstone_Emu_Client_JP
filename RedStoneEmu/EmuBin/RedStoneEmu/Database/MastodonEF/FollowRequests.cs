using System;
using System.Collections.Generic;

namespace RedStoneEmu.Database.MastodonEF
{
    public partial class FollowRequests
    {
        public int Id { get; set; }
        public int AccountId { get; set; }
        public int TargetAccountId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
