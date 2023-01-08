using System;
using System.Collections.Generic;

namespace RedStoneEmu.Database.MastodonEF
{
    public partial class Accounts
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string Domain { get; set; }
        public string Secret { get; set; }
        public string PrivateKey { get; set; }
        public string PublicKey { get; set; }
        public string RemoteUrl { get; set; }
        public string SalmonUrl { get; set; }
        public string HubUrl { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string Note { get; set; }
        public string DisplayName { get; set; }
        public string Uri { get; set; }
        public string Url { get; set; }
        public string AvatarFileName { get; set; }
        public string AvatarContentType { get; set; }
        public int? AvatarFileSize { get; set; }
        public DateTime? AvatarUpdatedAt { get; set; }
        public string HeaderFileName { get; set; }
        public string HeaderContentType { get; set; }
        public int? HeaderFileSize { get; set; }
        public DateTime? HeaderUpdatedAt { get; set; }
        public string AvatarRemoteUrl { get; set; }
        public DateTime? SubscriptionExpiresAt { get; set; }
        public bool Silenced { get; set; }
        public bool Suspended { get; set; }
        public bool Locked { get; set; }
        public string HeaderRemoteUrl { get; set; }
        public int StatusesCount { get; set; }
        public int FollowersCount { get; set; }
        public int FollowingCount { get; set; }
        public DateTime? LastWebfingeredAt { get; set; }
    }
}
