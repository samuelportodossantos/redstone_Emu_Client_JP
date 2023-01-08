using System;
using System.Collections.Generic;

namespace RedStoneEmu.Database.MastodonEF
{
    public partial class Settings
    {
        public int Id { get; set; }
        public string Var { get; set; }
        public string Value { get; set; }
        public string ThingType { get; set; }
        public int? ThingId { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
