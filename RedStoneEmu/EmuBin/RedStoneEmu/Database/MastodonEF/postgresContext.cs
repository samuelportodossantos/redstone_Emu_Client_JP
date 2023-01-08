using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace RedStoneEmu.Database.MastodonEF
{
    public partial class postgresContext : DbContext
    {
        public virtual DbSet<Accounts> Accounts { get; set; }
        public virtual DbSet<ArInternalMetadata> ArInternalMetadata { get; set; }
        public virtual DbSet<Blocks> Blocks { get; set; }
        public virtual DbSet<DomainBlocks> DomainBlocks { get; set; }
        public virtual DbSet<Favourites> Favourites { get; set; }
        public virtual DbSet<FollowRequests> FollowRequests { get; set; }
        public virtual DbSet<Follows> Follows { get; set; }
        public virtual DbSet<Imports> Imports { get; set; }
        public virtual DbSet<MediaAttachments> MediaAttachments { get; set; }
        public virtual DbSet<Mentions> Mentions { get; set; }
        public virtual DbSet<Mutes> Mutes { get; set; }
        public virtual DbSet<Notifications> Notifications { get; set; }
        public virtual DbSet<OauthAccessGrants> OauthAccessGrants { get; set; }
        public virtual DbSet<OauthAccessTokens> OauthAccessTokens { get; set; }
        public virtual DbSet<OauthApplications> OauthApplications { get; set; }
        public virtual DbSet<PreviewCards> PreviewCards { get; set; }
        public virtual DbSet<Reports> Reports { get; set; }
        public virtual DbSet<SchemaMigrations> SchemaMigrations { get; set; }
        public virtual DbSet<Settings> Settings { get; set; }
        public virtual DbSet<Statuses> Statuses { get; set; }
        public virtual DbSet<StreamEntries> StreamEntries { get; set; }
        public virtual DbSet<Subscriptions> Subscriptions { get; set; }
        public virtual DbSet<Tags> Tags { get; set; }
        public virtual DbSet<Users> Users { get; set; }
        public virtual DbSet<WebSettings> WebSettings { get; set; }

        // Unable to generate entity type for table 'public.statuses_tags'. Please see the warning messages.

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseNpgsql(string.Format("Host={0};User Id={1};Password={2};Database={3};Port={4}",
                       RedStoneApp.LoginServer.Config.DatabaseHost,
                       RedStoneApp.LoginServer.Config.DatabaseUsername,
                       RedStoneApp.LoginServer.Config.DatabasePassword,
                       "postgres",
                       RedStoneApp.LoginServer.Config.DatabasePort));
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Accounts>(entity =>
            {
                entity.ToTable("accounts");

                entity.HasIndex(e => e.AvatarFileName)
                    .HasName("index_accounts_on_url");

                entity.HasIndex(e => new { e.Username, e.Domain })
                    .HasName("index_accounts_on_username_and_domain")
                    .IsUnique();

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.AvatarContentType)
                    .HasColumnName("avatar_content_type")
                    .HasColumnType("varchar");

                entity.Property(e => e.AvatarFileName)
                    .HasColumnName("avatar_file_name")
                    .HasColumnType("varchar");

                entity.Property(e => e.AvatarFileSize).HasColumnName("avatar_file_size");

                entity.Property(e => e.AvatarRemoteUrl)
                    .HasColumnName("avatar_remote_url")
                    .HasColumnType("varchar");

                entity.Property(e => e.AvatarUpdatedAt).HasColumnName("avatar_updated_at");

                entity.Property(e => e.CreatedAt).HasColumnName("created_at");

                entity.Property(e => e.DisplayName)
                    .IsRequired()
                    .HasColumnName("display_name")
                    .HasColumnType("varchar")
                    .HasDefaultValueSql("''::character varying");

                entity.Property(e => e.Domain)
                    .IsRequired()
                    .HasColumnName("domain")
                    .HasColumnType("varchar");

                entity.Property(e => e.FollowersCount)
                    .HasColumnName("followers_count")
                    .HasDefaultValueSql("0");

                entity.Property(e => e.FollowingCount)
                    .HasColumnName("following_count")
                    .HasDefaultValueSql("0");

                entity.Property(e => e.HeaderContentType)
                    .HasColumnName("header_content_type")
                    .HasColumnType("varchar");

                entity.Property(e => e.HeaderFileName)
                    .HasColumnName("header_file_name")
                    .HasColumnType("varchar");

                entity.Property(e => e.HeaderFileSize).HasColumnName("header_file_size");

                entity.Property(e => e.HeaderRemoteUrl)
                    .IsRequired()
                    .HasColumnName("header_remote_url")
                    .HasColumnType("varchar")
                    .HasDefaultValueSql("''::character varying");

                entity.Property(e => e.HeaderUpdatedAt).HasColumnName("header_updated_at");

                entity.Property(e => e.HubUrl)
                    .IsRequired()
                    .HasColumnName("hub_url")
                    .HasColumnType("varchar")
                    .HasDefaultValueSql("''::character varying");

                entity.Property(e => e.LastWebfingeredAt).HasColumnName("last_webfingered_at");

                entity.Property(e => e.Locked)
                    .HasColumnName("locked")
                    .HasDefaultValueSql("false");

                entity.Property(e => e.Note)
                    .IsRequired()
                    .HasColumnName("note")
                    .HasDefaultValueSql("''::text");

                entity.Property(e => e.PrivateKey).HasColumnName("private_key");

                entity.Property(e => e.PublicKey)
                    .IsRequired()
                    .HasColumnName("public_key")
                    .HasDefaultValueSql("''::text");

                entity.Property(e => e.RemoteUrl)
                    .IsRequired()
                    .HasColumnName("remote_url")
                    .HasColumnType("varchar")
                    .HasDefaultValueSql("''::character varying");

                entity.Property(e => e.SalmonUrl)
                    .IsRequired()
                    .HasColumnName("salmon_url")
                    .HasColumnType("varchar")
                    .HasDefaultValueSql("''::character varying");

                entity.Property(e => e.Secret)
                    .IsRequired()
                    .HasColumnName("secret")
                    .HasColumnType("varchar")
                    .HasDefaultValueSql("''::character varying");

                entity.Property(e => e.Silenced)
                    .HasColumnName("silenced")
                    .HasDefaultValueSql("false");

                entity.Property(e => e.StatusesCount)
                    .HasColumnName("statuses_count")
                    .HasDefaultValueSql("0");

                entity.Property(e => e.SubscriptionExpiresAt).HasColumnName("subscription_expires_at");

                entity.Property(e => e.Suspended)
                    .HasColumnName("suspended")
                    .HasDefaultValueSql("false");

                entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");

                entity.Property(e => e.Uri)
                    .IsRequired()
                    .HasColumnName("uri")
                    .HasColumnType("varchar")
                    .HasDefaultValueSql("''::character varying");

                entity.Property(e => e.Url)
                    .HasColumnName("url")
                    .HasColumnType("varchar");

                entity.Property(e => e.Username)
                    .IsRequired()
                    .HasColumnName("username")
                    .HasColumnType("varchar")
                    .HasDefaultValueSql("''::character varying");
            });

            modelBuilder.Entity<ArInternalMetadata>(entity =>
            {
                entity.HasKey(e => e.Key)
                    .HasName("PK_ar_internal_metadata");

                entity.ToTable("ar_internal_metadata");

                entity.Property(e => e.Key)
                    .HasColumnName("key")
                    .HasColumnType("varchar");

                entity.Property(e => e.CreatedAt).HasColumnName("created_at");

                entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");

                entity.Property(e => e.Value)
                    .HasColumnName("value")
                    .HasColumnType("varchar");
            });

            modelBuilder.Entity<Blocks>(entity =>
            {
                entity.ToTable("blocks");

                entity.HasIndex(e => new { e.AccountId, e.TargetAccountId })
                    .HasName("index_blocks_on_account_id_and_target_account_id")
                    .IsUnique();

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.AccountId).HasColumnName("account_id");

                entity.Property(e => e.CreatedAt).HasColumnName("created_at");

                entity.Property(e => e.TargetAccountId).HasColumnName("target_account_id");

                entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
            });

            modelBuilder.Entity<DomainBlocks>(entity =>
            {
                entity.ToTable("domain_blocks");

                entity.HasIndex(e => e.Domain)
                    .HasName("index_domain_blocks_on_domain")
                    .IsUnique();

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.CreatedAt).HasColumnName("created_at");

                entity.Property(e => e.Domain)
                    .IsRequired()
                    .HasColumnName("domain")
                    .HasColumnType("varchar")
                    .HasDefaultValueSql("''::character varying");

                entity.Property(e => e.RejectMedia).HasColumnName("reject_media");

                entity.Property(e => e.Severity)
                    .HasColumnName("severity")
                    .HasDefaultValueSql("0");

                entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
            });

            modelBuilder.Entity<Favourites>(entity =>
            {
                entity.ToTable("favourites");

                entity.HasIndex(e => e.StatusId)
                    .HasName("index_favourites_on_status_id");

                entity.HasIndex(e => new { e.AccountId, e.StatusId })
                    .HasName("index_favourites_on_account_id_and_status_id")
                    .IsUnique();

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.AccountId).HasColumnName("account_id");

                entity.Property(e => e.CreatedAt).HasColumnName("created_at");

                entity.Property(e => e.StatusId).HasColumnName("status_id");

                entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
            });

            modelBuilder.Entity<FollowRequests>(entity =>
            {
                entity.ToTable("follow_requests");

                entity.HasIndex(e => new { e.AccountId, e.TargetAccountId })
                    .HasName("index_follow_requests_on_account_id_and_target_account_id")
                    .IsUnique();

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.AccountId).HasColumnName("account_id");

                entity.Property(e => e.CreatedAt).HasColumnName("created_at");

                entity.Property(e => e.TargetAccountId).HasColumnName("target_account_id");

                entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
            });

            modelBuilder.Entity<Follows>(entity =>
            {
                entity.ToTable("follows");

                entity.HasIndex(e => new { e.AccountId, e.TargetAccountId })
                    .HasName("index_follows_on_account_id_and_target_account_id")
                    .IsUnique();

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.AccountId).HasColumnName("account_id");

                entity.Property(e => e.CreatedAt).HasColumnName("created_at");

                entity.Property(e => e.TargetAccountId).HasColumnName("target_account_id");

                entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
            });

            modelBuilder.Entity<Imports>(entity =>
            {
                entity.ToTable("imports");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.AccountId).HasColumnName("account_id");

                entity.Property(e => e.Approved).HasColumnName("approved");

                entity.Property(e => e.CreatedAt).HasColumnName("created_at");

                entity.Property(e => e.DataContentType)
                    .HasColumnName("data_content_type")
                    .HasColumnType("varchar");

                entity.Property(e => e.DataFileName)
                    .HasColumnName("data_file_name")
                    .HasColumnType("varchar");

                entity.Property(e => e.DataFileSize).HasColumnName("data_file_size");

                entity.Property(e => e.DataUpdatedAt).HasColumnName("data_updated_at");

                entity.Property(e => e.Type).HasColumnName("type");

                entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
            });

            modelBuilder.Entity<MediaAttachments>(entity =>
            {
                entity.ToTable("media_attachments");

                entity.HasIndex(e => e.Shortcode)
                    .HasName("index_media_attachments_on_shortcode")
                    .IsUnique();

                entity.HasIndex(e => e.StatusId)
                    .HasName("index_media_attachments_on_status_id");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.AccountId).HasColumnName("account_id");

                entity.Property(e => e.CreatedAt).HasColumnName("created_at");

                entity.Property(e => e.FileContentType)
                    .HasColumnName("file_content_type")
                    .HasColumnType("varchar");

                entity.Property(e => e.FileFileName)
                    .HasColumnName("file_file_name")
                    .HasColumnType("varchar");

                entity.Property(e => e.FileFileSize).HasColumnName("file_file_size");

                entity.Property(e => e.FileMeta)
                    .HasColumnName("file_meta")
                    .HasColumnType("json");

                entity.Property(e => e.FileUpdatedAt).HasColumnName("file_updated_at");

                entity.Property(e => e.RemoteUrl)
                    .IsRequired()
                    .HasColumnName("remote_url")
                    .HasColumnType("varchar")
                    .HasDefaultValueSql("''::character varying");

                entity.Property(e => e.Shortcode)
                    .IsRequired()
                    .HasColumnName("shortcode")
                    .HasColumnType("varchar");

                entity.Property(e => e.StatusId).HasColumnName("status_id");

                entity.Property(e => e.Type)
                    .HasColumnName("type")
                    .HasDefaultValueSql("0");

                entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
            });

            modelBuilder.Entity<Mentions>(entity =>
            {
                entity.ToTable("mentions");

                entity.HasIndex(e => e.StatusId)
                    .HasName("index_mentions_on_status_id");

                entity.HasIndex(e => new { e.AccountId, e.StatusId })
                    .HasName("index_mentions_on_account_id_and_status_id")
                    .IsUnique();

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.AccountId)
                    .IsRequired()
                    .HasColumnName("account_id");

                entity.Property(e => e.CreatedAt).HasColumnName("created_at");

                entity.Property(e => e.StatusId)
                    .IsRequired()
                    .HasColumnName("status_id");

                entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
            });

            modelBuilder.Entity<Mutes>(entity =>
            {
                entity.ToTable("mutes");

                entity.HasIndex(e => new { e.AccountId, e.TargetAccountId })
                    .HasName("index_mutes_on_account_id_and_target_account_id")
                    .IsUnique();

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.AccountId).HasColumnName("account_id");

                entity.Property(e => e.CreatedAt).HasColumnName("created_at");

                entity.Property(e => e.TargetAccountId).HasColumnName("target_account_id");

                entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
            });

            modelBuilder.Entity<Notifications>(entity =>
            {
                entity.ToTable("notifications");

                entity.HasIndex(e => new { e.ActivityId, e.ActivityType })
                    .HasName("index_notifications_on_activity_id_and_activity_type");

                entity.HasIndex(e => new { e.AccountId, e.ActivityId, e.ActivityType })
                    .HasName("account_activity")
                    .IsUnique();

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.AccountId)
                    .IsRequired()
                    .HasColumnName("account_id");

                entity.Property(e => e.ActivityId)
                    .IsRequired()
                    .HasColumnName("activity_id");

                entity.Property(e => e.ActivityType)
                    .IsRequired()
                    .HasColumnName("activity_type")
                    .HasColumnType("varchar");

                entity.Property(e => e.CreatedAt).HasColumnName("created_at");

                entity.Property(e => e.FromAccountId).HasColumnName("from_account_id");

                entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
            });

            modelBuilder.Entity<OauthAccessGrants>(entity =>
            {
                entity.ToTable("oauth_access_grants");

                entity.HasIndex(e => e.Token)
                    .HasName("index_oauth_access_grants_on_token")
                    .IsUnique();

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.ApplicationId).HasColumnName("application_id");

                entity.Property(e => e.CreatedAt).HasColumnName("created_at");

                entity.Property(e => e.ExpiresIn).HasColumnName("expires_in");

                entity.Property(e => e.RedirectUri)
                    .IsRequired()
                    .HasColumnName("redirect_uri");

                entity.Property(e => e.ResourceOwnerId).HasColumnName("resource_owner_id");

                entity.Property(e => e.RevokedAt).HasColumnName("revoked_at");

                entity.Property(e => e.Scopes)
                    .HasColumnName("scopes")
                    .HasColumnType("varchar");

                entity.Property(e => e.Token)
                    .IsRequired()
                    .HasColumnName("token")
                    .HasColumnType("varchar");
            });

            modelBuilder.Entity<OauthAccessTokens>(entity =>
            {
                entity.ToTable("oauth_access_tokens");

                entity.HasIndex(e => e.RefreshToken)
                    .HasName("index_oauth_access_tokens_on_refresh_token")
                    .IsUnique();

                entity.HasIndex(e => e.ResourceOwnerId)
                    .HasName("index_oauth_access_tokens_on_resource_owner_id");

                entity.HasIndex(e => e.Token)
                    .HasName("index_oauth_access_tokens_on_token")
                    .IsUnique();

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.ApplicationId).HasColumnName("application_id");

                entity.Property(e => e.CreatedAt).HasColumnName("created_at");

                entity.Property(e => e.ExpiresIn).HasColumnName("expires_in");

                entity.Property(e => e.RefreshToken)
                    .IsRequired()
                    .HasColumnName("refresh_token")
                    .HasColumnType("varchar");

                entity.Property(e => e.ResourceOwnerId).HasColumnName("resource_owner_id");

                entity.Property(e => e.RevokedAt).HasColumnName("revoked_at");

                entity.Property(e => e.Scopes)
                    .HasColumnName("scopes")
                    .HasColumnType("varchar");

                entity.Property(e => e.Token)
                    .IsRequired()
                    .HasColumnName("token")
                    .HasColumnType("varchar");
            });

            modelBuilder.Entity<OauthApplications>(entity =>
            {
                entity.ToTable("oauth_applications");

                entity.HasIndex(e => e.Uid)
                    .HasName("index_oauth_applications_on_uid")
                    .IsUnique();

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.CreatedAt).HasColumnName("created_at");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasColumnName("name")
                    .HasColumnType("varchar");

                entity.Property(e => e.RedirectUri)
                    .IsRequired()
                    .HasColumnName("redirect_uri");

                entity.Property(e => e.Scopes)
                    .IsRequired()
                    .HasColumnName("scopes")
                    .HasColumnType("varchar")
                    .HasDefaultValueSql("''::character varying");

                entity.Property(e => e.Secret)
                    .IsRequired()
                    .HasColumnName("secret")
                    .HasColumnType("varchar");

                entity.Property(e => e.Superapp)
                    .HasColumnName("superapp")
                    .HasDefaultValueSql("false");

                entity.Property(e => e.Uid)
                    .IsRequired()
                    .HasColumnName("uid")
                    .HasColumnType("varchar");

                entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");

                entity.Property(e => e.Website)
                    .HasColumnName("website")
                    .HasColumnType("varchar");
            });

            modelBuilder.Entity<PreviewCards>(entity =>
            {
                entity.ToTable("preview_cards");

                entity.HasIndex(e => e.StatusId)
                    .HasName("index_preview_cards_on_status_id")
                    .IsUnique();

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.AuthorName)
                    .IsRequired()
                    .HasColumnName("author_name")
                    .HasColumnType("varchar")
                    .HasDefaultValueSql("''::character varying");

                entity.Property(e => e.AuthorUrl)
                    .IsRequired()
                    .HasColumnName("author_url")
                    .HasColumnType("varchar")
                    .HasDefaultValueSql("''::character varying");

                entity.Property(e => e.CreatedAt).HasColumnName("created_at");

                entity.Property(e => e.Description)
                    .HasColumnName("description")
                    .HasColumnType("varchar");

                entity.Property(e => e.Height)
                    .HasColumnName("height")
                    .HasDefaultValueSql("0");

                entity.Property(e => e.Html)
                    .IsRequired()
                    .HasColumnName("html")
                    .HasDefaultValueSql("''::text");

                entity.Property(e => e.ImageContentType)
                    .HasColumnName("image_content_type")
                    .HasColumnType("varchar");

                entity.Property(e => e.ImageFileName)
                    .HasColumnName("image_file_name")
                    .HasColumnType("varchar");

                entity.Property(e => e.ImageFileSize).HasColumnName("image_file_size");

                entity.Property(e => e.ImageUpdatedAt).HasColumnName("image_updated_at");

                entity.Property(e => e.ProviderName)
                    .IsRequired()
                    .HasColumnName("provider_name")
                    .HasColumnType("varchar")
                    .HasDefaultValueSql("''::character varying");

                entity.Property(e => e.ProviderUrl)
                    .IsRequired()
                    .HasColumnName("provider_url")
                    .HasColumnType("varchar")
                    .HasDefaultValueSql("''::character varying");

                entity.Property(e => e.StatusId)
                    .IsRequired()
                    .HasColumnName("status_id");

                entity.Property(e => e.Title)
                    .HasColumnName("title")
                    .HasColumnType("varchar");

                entity.Property(e => e.Type)
                    .HasColumnName("type")
                    .HasDefaultValueSql("0");

                entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");

                entity.Property(e => e.Url)
                    .IsRequired()
                    .HasColumnName("url")
                    .HasColumnType("varchar")
                    .HasDefaultValueSql("''::character varying");

                entity.Property(e => e.Width)
                    .HasColumnName("width")
                    .HasDefaultValueSql("0");
            });

            modelBuilder.Entity<Reports>(entity =>
            {
                entity.ToTable("reports");

                entity.HasIndex(e => e.AccountId)
                    .HasName("index_reports_on_account_id");

                entity.HasIndex(e => e.TargetAccountId)
                    .HasName("index_reports_on_target_account_id");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.AccountId).HasColumnName("account_id");

                entity.Property(e => e.ActionTaken)
                    .HasColumnName("action_taken")
                    .HasDefaultValueSql("false");

                entity.Property(e => e.ActionTakenByAccountId).HasColumnName("action_taken_by_account_id");

                entity.Property(e => e.Comment)
                    .IsRequired()
                    .HasColumnName("comment")
                    .HasDefaultValueSql("''::text");

                entity.Property(e => e.CreatedAt).HasColumnName("created_at");

                entity.Property(e => e.StatusIds)
                    .IsRequired()
                    .HasColumnName("status_ids")
                    .HasDefaultValueSql("'{}'::integer[]");

                entity.Property(e => e.TargetAccountId).HasColumnName("target_account_id");

                entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
            });

            modelBuilder.Entity<SchemaMigrations>(entity =>
            {
                entity.HasKey(e => e.Version)
                    .HasName("PK_schema_migrations");

                entity.ToTable("schema_migrations");

                entity.Property(e => e.Version)
                    .HasColumnName("version")
                    .HasColumnType("varchar");
            });

            modelBuilder.Entity<Settings>(entity =>
            {
                entity.ToTable("settings");

                entity.HasIndex(e => new { e.ThingType, e.ThingId, e.Var })
                    .HasName("index_settings_on_thing_type_and_thing_id_and_var")
                    .IsUnique();

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.CreatedAt).HasColumnName("created_at");

                entity.Property(e => e.ThingId)
                    .IsRequired()
                    .HasColumnName("thing_id");

                entity.Property(e => e.ThingType)
                    .IsRequired()
                    .HasColumnName("thing_type")
                    .HasColumnType("varchar");

                entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");

                entity.Property(e => e.Value).HasColumnName("value");

                entity.Property(e => e.Var)
                    .IsRequired()
                    .HasColumnName("var")
                    .HasColumnType("varchar");
            });

            modelBuilder.Entity<Statuses>(entity =>
            {
                entity.ToTable("statuses");

                entity.HasIndex(e => e.AccountId)
                    .HasName("index_statuses_on_account_id");

                entity.HasIndex(e => e.InReplyToId)
                    .HasName("index_statuses_on_in_reply_to_id");

                entity.HasIndex(e => e.ReblogOfId)
                    .HasName("index_statuses_on_reblog_of_id");

                entity.HasIndex(e => e.Uri)
                    .HasName("index_statuses_on_uri")
                    .IsUnique();

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.AccountId).HasColumnName("account_id");

                entity.Property(e => e.ApplicationId).HasColumnName("application_id");

                entity.Property(e => e.CreatedAt).HasColumnName("created_at");

                entity.Property(e => e.FavouritesCount)
                    .HasColumnName("favourites_count")
                    .HasDefaultValueSql("0");

                entity.Property(e => e.InReplyToAccountId).HasColumnName("in_reply_to_account_id");

                entity.Property(e => e.InReplyToId).HasColumnName("in_reply_to_id");

                entity.Property(e => e.Language)
                    .IsRequired()
                    .HasColumnName("language")
                    .HasColumnType("varchar")
                    .HasDefaultValueSql("'en'::character varying");

                entity.Property(e => e.ReblogOfId).HasColumnName("reblog_of_id");

                entity.Property(e => e.ReblogsCount)
                    .HasColumnName("reblogs_count")
                    .HasDefaultValueSql("0");

                entity.Property(e => e.Reply)
                    .HasColumnName("reply")
                    .HasDefaultValueSql("false");

                entity.Property(e => e.Sensitive)
                    .HasColumnName("sensitive")
                    .HasDefaultValueSql("false");

                entity.Property(e => e.SpoilerText)
                    .IsRequired()
                    .HasColumnName("spoiler_text")
                    .HasDefaultValueSql("''::text");

                entity.Property(e => e.Text)
                    .IsRequired()
                    .HasColumnName("text")
                    .HasDefaultValueSql("''::text");

                entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");

                entity.Property(e => e.Uri)
                    .IsRequired()
                    .HasColumnName("uri")
                    .HasColumnType("varchar");

                entity.Property(e => e.Url)
                    .HasColumnName("url")
                    .HasColumnType("varchar");

                entity.Property(e => e.Visibility)
                    .HasColumnName("visibility")
                    .HasDefaultValueSql("0");

                entity.HasOne(d => d.ReblogOf)
                    .WithMany(p => p.InverseReblogOf)
                    .HasForeignKey(d => d.ReblogOfId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("fk_rails_256483a9ab");
            });

            modelBuilder.Entity<StreamEntries>(entity =>
            {
                entity.ToTable("stream_entries");

                entity.HasIndex(e => e.AccountId)
                    .HasName("index_stream_entries_on_account_id");

                entity.HasIndex(e => new { e.ActivityId, e.ActivityType })
                    .HasName("index_stream_entries_on_activity_id_and_activity_type");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.AccountId).HasColumnName("account_id");

                entity.Property(e => e.ActivityId).HasColumnName("activity_id");

                entity.Property(e => e.ActivityType)
                    .HasColumnName("activity_type")
                    .HasColumnType("varchar");

                entity.Property(e => e.CreatedAt).HasColumnName("created_at");

                entity.Property(e => e.Hidden)
                    .HasColumnName("hidden")
                    .HasDefaultValueSql("false");

                entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
            });

            modelBuilder.Entity<Subscriptions>(entity =>
            {
                entity.ToTable("subscriptions");

                entity.HasIndex(e => new { e.CallbackUrl, e.AccountId })
                    .HasName("index_subscriptions_on_callback_url_and_account_id")
                    .IsUnique();

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.AccountId).HasColumnName("account_id");

                entity.Property(e => e.CallbackUrl)
                    .IsRequired()
                    .HasColumnName("callback_url")
                    .HasColumnType("varchar")
                    .HasDefaultValueSql("''::character varying");

                entity.Property(e => e.Confirmed)
                    .HasColumnName("confirmed")
                    .HasDefaultValueSql("false");

                entity.Property(e => e.CreatedAt).HasColumnName("created_at");

                entity.Property(e => e.ExpiresAt).HasColumnName("expires_at");

                entity.Property(e => e.LastSuccessfulDeliveryAt).HasColumnName("last_successful_delivery_at");

                entity.Property(e => e.Secret)
                    .HasColumnName("secret")
                    .HasColumnType("varchar");

                entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
            });

            modelBuilder.Entity<Tags>(entity =>
            {
                entity.ToTable("tags");

                entity.HasIndex(e => e.Name)
                    .HasName("index_tags_on_name")
                    .IsUnique();

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.CreatedAt).HasColumnName("created_at");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasColumnName("name")
                    .HasColumnType("varchar")
                    .HasDefaultValueSql("''::character varying");

                entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
            });

            modelBuilder.Entity<Users>(entity =>
            {
                entity.ToTable("users");

                entity.HasIndex(e => e.AccountId)
                    .HasName("index_users_on_account_id");

                entity.HasIndex(e => e.AllowedLanguages)
                    .HasName("index_users_on_allowed_languages");

                entity.HasIndex(e => e.ConfirmationToken)
                    .HasName("index_users_on_confirmation_token")
                    .IsUnique();

                entity.HasIndex(e => e.Email)
                    .HasName("index_users_on_email")
                    .IsUnique();

                entity.HasIndex(e => e.ResetPasswordToken)
                    .HasName("index_users_on_reset_password_token")
                    .IsUnique();

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.AccountId).HasColumnName("account_id");

                entity.Property(e => e.Admin)
                    .HasColumnName("admin")
                    .HasDefaultValueSql("false");

                entity.Property(e => e.AllowedLanguages)
                    .IsRequired()
                    .HasColumnName("allowed_languages")
                    .HasColumnType("_varchar")
                    .HasDefaultValueSql("'{}'::character varying[]");

                entity.Property(e => e.ConfirmationSentAt).HasColumnName("confirmation_sent_at");

                entity.Property(e => e.ConfirmationToken)
                    .IsRequired()
                    .HasColumnName("confirmation_token")
                    .HasColumnType("varchar");

                entity.Property(e => e.ConfirmedAt).HasColumnName("confirmed_at");

                entity.Property(e => e.ConsumedTimestep).HasColumnName("consumed_timestep");

                entity.Property(e => e.CreatedAt).HasColumnName("created_at");

                entity.Property(e => e.CurrentSignInAt).HasColumnName("current_sign_in_at");

                entity.Property(e => e.CurrentSignInIp).HasColumnName("current_sign_in_ip");

                entity.Property(e => e.Email)
                    .IsRequired()
                    .HasColumnName("email")
                    .HasColumnType("varchar")
                    .HasDefaultValueSql("''::character varying");

                entity.Property(e => e.EncryptedOtpSecret)
                    .HasColumnName("encrypted_otp_secret")
                    .HasColumnType("varchar");

                entity.Property(e => e.EncryptedOtpSecretIv)
                    .HasColumnName("encrypted_otp_secret_iv")
                    .HasColumnType("varchar");

                entity.Property(e => e.EncryptedOtpSecretSalt)
                    .HasColumnName("encrypted_otp_secret_salt")
                    .HasColumnType("varchar");

                entity.Property(e => e.EncryptedPassword)
                    .IsRequired()
                    .HasColumnName("encrypted_password")
                    .HasColumnType("varchar")
                    .HasDefaultValueSql("''::character varying");

                entity.Property(e => e.LastEmailedAt).HasColumnName("last_emailed_at");

                entity.Property(e => e.LastSignInAt).HasColumnName("last_sign_in_at");

                entity.Property(e => e.LastSignInIp).HasColumnName("last_sign_in_ip");

                entity.Property(e => e.Locale)
                    .HasColumnName("locale")
                    .HasColumnType("varchar");

                entity.Property(e => e.OtpBackupCodes)
                    .HasColumnName("otp_backup_codes")
                    .HasColumnType("_varchar");

                entity.Property(e => e.OtpRequiredForLogin).HasColumnName("otp_required_for_login");

                entity.Property(e => e.RememberCreatedAt).HasColumnName("remember_created_at");

                entity.Property(e => e.ResetPasswordSentAt).HasColumnName("reset_password_sent_at");

                entity.Property(e => e.ResetPasswordToken)
                    .IsRequired()
                    .HasColumnName("reset_password_token")
                    .HasColumnType("varchar");

                entity.Property(e => e.SignInCount)
                    .HasColumnName("sign_in_count")
                    .HasDefaultValueSql("0");

                entity.Property(e => e.UnconfirmedEmail)
                    .HasColumnName("unconfirmed_email")
                    .HasColumnType("varchar");

                entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
            });

            modelBuilder.Entity<WebSettings>(entity =>
            {
                entity.ToTable("web_settings");

                entity.HasIndex(e => e.UserId)
                    .HasName("index_web_settings_on_user_id")
                    .IsUnique();

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.CreatedAt).HasColumnName("created_at");

                entity.Property(e => e.Data)
                    .HasColumnName("data")
                    .HasColumnType("json");

                entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");

                entity.Property(e => e.UserId)
                    .IsRequired()
                    .HasColumnName("user_id");
            });
        }
    }
}