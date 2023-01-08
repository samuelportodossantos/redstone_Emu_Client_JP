using System;
using System.Collections.Generic;

namespace RedStoneEmu.Database.MastodonEF
{
    public partial class Subscriptions
    {
        public int Id { get; set; }
        public string CallbackUrl { get; set; }
        public string Secret { get; set; }
        public DateTime? ExpiresAt { get; set; }
        public bool Confirmed { get; set; }
        public int AccountId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public DateTime? LastSuccessfulDeliveryAt { get; set; }
    }
}
