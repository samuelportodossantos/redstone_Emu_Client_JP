using System;
using System.Collections.Generic;

namespace RedStoneEmu.Database.MastodonEF
{
    public partial class Reports
    {
        public int Id { get; set; }
        public int AccountId { get; set; }
        public int TargetAccountId { get; set; }
        public long[] StatusIds { get; set; }
        public string Comment { get; set; }
        public bool ActionTaken { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public int? ActionTakenByAccountId { get; set; }
    }
}
