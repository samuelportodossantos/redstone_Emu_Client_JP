using System;
using System.Collections.Generic;

namespace RedStoneEmu.Database.MastodonEF
{
    public partial class OauthApplications
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Uid { get; set; }
        public string Secret { get; set; }
        public string RedirectUri { get; set; }
        public string Scopes { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public bool Superapp { get; set; }
        public string Website { get; set; }
    }
}
