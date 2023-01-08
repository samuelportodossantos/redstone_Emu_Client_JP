using System;
using System.Collections.Generic;
using System.Net;

namespace RedStoneEmu.Database.MastodonEF
{
    public partial class Users
    {
        public int Id { get; set; }
        public string Email { get; set; }
        public int AccountId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string EncryptedPassword { get; set; }
        public string ResetPasswordToken { get; set; }
        public DateTime? ResetPasswordSentAt { get; set; }
        public DateTime? RememberCreatedAt { get; set; }
        public int SignInCount { get; set; }
        public DateTime? CurrentSignInAt { get; set; }
        public DateTime? LastSignInAt { get; set; }
        public IPAddress CurrentSignInIp { get; set; }
        public IPAddress LastSignInIp { get; set; }
        public bool? Admin { get; set; }
        public string ConfirmationToken { get; set; }
        public DateTime? ConfirmedAt { get; set; }
        public DateTime? ConfirmationSentAt { get; set; }
        public string UnconfirmedEmail { get; set; }
        public string Locale { get; set; }
        public string EncryptedOtpSecret { get; set; }
        public string EncryptedOtpSecretIv { get; set; }
        public string EncryptedOtpSecretSalt { get; set; }
        public int? ConsumedTimestep { get; set; }
        public bool? OtpRequiredForLogin { get; set; }
        public DateTime? LastEmailedAt { get; set; }
        public string[] OtpBackupCodes { get; set; }
        public string[] AllowedLanguages { get; set; }
    }
}
