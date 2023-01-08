using System;
using System.Collections.Generic;

namespace RedStoneEmu.Database.MastodonEF
{
    public partial class Statuses
    {
        public long Id { get; set; }
        public string Uri { get; set; }
        public int AccountId { get; set; }
        public string Text { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public long? InReplyToId { get; set; }
        public long? ReblogOfId { get; set; }
        public string Url { get; set; }
        public bool? Sensitive { get; set; }
        public int Visibility { get; set; }
        public int? InReplyToAccountId { get; set; }
        public int? ApplicationId { get; set; }
        public string SpoilerText { get; set; }
        public bool? Reply { get; set; }
        public int FavouritesCount { get; set; }
        public int ReblogsCount { get; set; }
        public string Language { get; set; }

        public virtual Statuses ReblogOf { get; set; }
        public virtual ICollection<Statuses> InverseReblogOf { get; set; }
    }
}
