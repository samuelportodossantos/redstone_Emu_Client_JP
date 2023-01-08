using System;
using System.Collections.Generic;

namespace RedStoneEmu.Database.MastodonEF
{
    public partial class Favourites
    {
        public int Id { get; set; }
        public int AccountId { get; set; }
        public int StatusId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
