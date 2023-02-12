using System;
using System.IO;
using Npgsql;

namespace RedStoneEmu
{
    class EnviromentController
    {
        public void build()
        {
            string[] lines = File.ReadAllLines(".env");
            foreach (string line in lines)
            {
                if (line.StartsWith("#"))
                {
                    continue;
                }
                string[] keyValue = line.Split('=');
                string key = keyValue[0];
                string value = keyValue[1];
                if (value != null)
                {
                    Environment.SetEnvironmentVariable(key, value);
                }
            }
        }

        public void setupDatabases()
        {
            ServerConfig config = new ServerConfig();
            var connectionString = string.Format("Host={0};User Id={1};Password={2};Database={3};Port={4}",
                config.DatabaseHost,
                config.DatabaseUsername,
                config.DatabasePassword,
                config.DatabaseName,
                config.DatabasePort);

            using (var conn = new NpgsqlConnection(connectionString))
            {
                conn.Open();

                // CREATING banks TABLE
                using (var cmd = new NpgsqlCommand("CREATE TABLE IF NOT EXISTS public.\"Banks\" (\"BankKey\" integer NOT NULL GENERATED BY DEFAULT AS IDENTITY ( INCREMENT 1 START 1 MINVALUE 1 MAXVALUE 2147483647 CACHE 1 ), \"UserID\" text COLLATE pg_catalog.\"default\" NOT NULL, \"UniqueItemString\" text COLLATE pg_catalog.\"default\" NOT NULL, \"Gold\" bigint NOT NULL, \"Unknown\" bigint NOT NULL, CONSTRAINT \"PK_Banks\" PRIMARY KEY (\"BankKey\"))", conn))
                {
                    cmd.ExecuteNonQuery();
                }

                // CREATING game_server_info TABLE
                using (var command = new NpgsqlCommand())
                {
                    command.Connection = conn;
                    command.CommandText = @"
                        -- Table: public.game_server_info

                        DROP TABLE IF EXISTS public.game_server_info;

                        CREATE TABLE IF NOT EXISTS public.game_server_info
                        (
                            ""Id"" integer NOT NULL GENERATED BY DEFAULT AS IDENTITY ( INCREMENT 1 START 1 MINVALUE 1 MAXVALUE 2147483647 CACHE 1 ),
                            ""Enable"" boolean NOT NULL,
                            ""ServerId"" integer NOT NULL,
                            ""ServerName"" text COLLATE pg_catalog.""default"",
                            ""Host"" text COLLATE pg_catalog.""default"",
                            ""ServerType"" integer NOT NULL,
                            CONSTRAINT ""PK_game_server_info"" PRIMARY KEY (""Id"")
                        )

                        TABLESPACE pg_default;

                        DROP INDEX IF EXISTS public.""IX_game_server_info_Host"";

                        CREATE UNIQUE INDEX IF NOT EXISTS ""IX_game_server_info_Host""
                            ON public.game_server_info USING btree
                            (""Host"" COLLATE pg_catalog.""default"" ASC NULLS LAST)
                            TABLESPACE pg_default;

                        DROP INDEX IF EXISTS public.""IX_game_server_info_ServerId"";

                        CREATE UNIQUE INDEX IF NOT EXISTS ""IX_game_server_info_ServerId""
                            ON public.game_server_info USING btree
                            (""ServerId"" ASC NULLS LAST)
                            TABLESPACE pg_default;

                        DROP INDEX IF EXISTS public.""IX_game_server_info_ServerName"";

                        CREATE UNIQUE INDEX IF NOT EXISTS ""IX_game_server_info_ServerName""
                            ON public.game_server_info USING btree
                            (""ServerName"" COLLATE pg_catalog.""default"" ASC NULLS LAST)
                            TABLESPACE pg_default;
                    ";

                    command.ExecuteNonQuery();
                }

                // CREATING phpbb_users TABLE
                using (var command = new NpgsqlCommand())
                {
                    command.Connection = conn;
                    command.CommandText = @"-- Table: public.phpbb_users

                    -- DROP TABLE IF EXISTS public.phpbb_users;

                    CREATE TABLE IF NOT EXISTS public.phpbb_users
                    (
                        user_id integer NOT NULL GENERATED BY DEFAULT AS IDENTITY ( INCREMENT 1 START 1 MINVALUE 1 MAXVALUE 2147483647 CACHE 1 ),
                        user_type integer NOT NULL,
                        group_id integer NOT NULL,
                        user_permissions text COLLATE pg_catalog.""default"" NOT NULL,
                        user_perm_from integer NOT NULL,
                        user_ip text COLLATE pg_catalog.""default"" NOT NULL,
                        user_regdate integer NOT NULL,
                        username text COLLATE pg_catalog.""default"" NOT NULL,
                        username_clean text COLLATE pg_catalog.""default"" NOT NULL,
                        user_password text COLLATE pg_catalog.""default"" NOT NULL,
                        user_passchg integer NOT NULL,
                        user_email text COLLATE pg_catalog.""default"" NOT NULL,
                        user_email_hash integer NOT NULL,
                        user_birthday text COLLATE pg_catalog.""default"" NOT NULL,
                        user_lastvisit integer NOT NULL,
                        user_lastmark integer NOT NULL,
                        user_lastpost_time integer NOT NULL,
                        user_lastpage text COLLATE pg_catalog.""default"" NOT NULL,
                        user_last_confirm_key text COLLATE pg_catalog.""default"" NOT NULL,
                        user_last_search integer NOT NULL,
                        user_warnings integer NOT NULL,
                        user_last_warning integer NOT NULL,
                        user_login_attempts integer NOT NULL,
                        user_inactive_reason integer NOT NULL,
                        user_inactive_time integer NOT NULL,
                        user_posts integer NOT NULL,
                        user_lang text COLLATE pg_catalog.""default"" NOT NULL,
                        user_timezone text COLLATE pg_catalog.""default"" NOT NULL,
                        user_dateformat text COLLATE pg_catalog.""default"" NOT NULL,
                        user_style integer NOT NULL,
                        user_rank integer NOT NULL,
                        user_colour text COLLATE pg_catalog.""default"" NOT NULL,
                        user_new_privmsg integer NOT NULL,
                        user_unread_privmsg integer NOT NULL,
                        user_last_privmsg integer NOT NULL,
                        user_message_rules integer NOT NULL,
                        user_full_folder integer NOT NULL,
                        user_emailtime integer NOT NULL,
                        user_topic_show_days integer NOT NULL,
                        user_topic_sortby_type text COLLATE pg_catalog.""default"" NOT NULL,
                        user_topic_sortby_dir text COLLATE pg_catalog.""default"" NOT NULL,
                        user_post_show_days integer NOT NULL,
                        user_post_sortby_type text COLLATE pg_catalog.""default"" NOT NULL,
                        user_post_sortby_dir text COLLATE pg_catalog.""default"" NOT NULL,
                        user_notify integer NOT NULL,
                        user_notify_pm integer NOT NULL,
                        user_notify_type integer NOT NULL,
                        user_allow_pm integer NOT NULL,
                        user_allow_viewonline integer NOT NULL,
                        user_allow_viewemail integer NOT NULL,
                        user_allow_massemail integer NOT NULL,
                        user_options integer NOT NULL,
                        user_avatar text COLLATE pg_catalog.""default"" NOT NULL,
                        user_avatar_type text COLLATE pg_catalog.""default"" NOT NULL,
                        user_avatar_width integer NOT NULL,
                        user_avatar_height integer NOT NULL,
                        user_sig text COLLATE pg_catalog.""default"" NOT NULL,
                        user_sig_bbcode_uid text COLLATE pg_catalog.""default"" NOT NULL,
                        user_sig_bbcode_bitfield text COLLATE pg_catalog.""default"" NOT NULL,
                        user_jabber text COLLATE pg_catalog.""default"" NOT NULL,
                        user_actkey text COLLATE pg_catalog.""default"" NOT NULL,
                        user_newpasswd text COLLATE pg_catalog.""default"" NOT NULL,
                        user_form_salt text COLLATE pg_catalog.""default"" NOT NULL,
                        user_new integer NOT NULL,
                        user_reminded integer NOT NULL,
                        user_reminded_time integer NOT NULL,
                        CONSTRAINT ""PK_phpbb_users"" PRIMARY KEY (user_id)
                    )

                    TABLESPACE pg_default;";

                    command.ExecuteNonQuery();
                }



                using (var command = new NpgsqlCommand())
                {
                    command.Connection = conn;
                    command.CommandText = @"-- Table: public.Players

                        -- DROP TABLE IF EXISTS public.""Players"";

                        CREATE TABLE IF NOT EXISTS public.""Players""
                        (
                            ""PlayerId"" integer NOT NULL GENERATED BY DEFAULT AS IDENTITY ( INCREMENT 1 START 1 MINVALUE 1 MAXVALUE 2147483647 CACHE 1 ),
                            ""EXP"" bigint NOT NULL,
                            ""SkillPoint"" bigint NOT NULL,
                            ""NowHP"" bigint NOT NULL,
                            ""BaseHP"" bigint NOT NULL,
                            ""NowCP"" integer NOT NULL,
                            ""BaseCP"" bigint NOT NULL,
                            ""Tendency"" smallint NOT NULL,
                            ""UserID"" text COLLATE pg_catalog.""default"" NOT NULL,
                            ""Name"" text COLLATE pg_catalog.""default"" NOT NULL,
                            ""Job"" smallint NOT NULL,
                            ""Gold"" bigint NOT NULL,
                            ""StatusPoint"" bigint NOT NULL,
                            ""GMLevel"" integer NOT NULL,
                            ""DeathPenarty"" integer NOT NULL,
                            ""IsRun"" boolean NOT NULL,
                            ""MiniPet1"" smallint NOT NULL,
                            ""MiniPet2"" smallint NOT NULL,
                            ""RebornNumber"" smallint NOT NULL,
                            ""Level"" smallint NOT NULL,
                            ""StateHPCPBonus"" smallint NOT NULL,
                            ""LevelHPCPBobuns"" smallint NOT NULL,
                            ""Status"" text COLLATE pg_catalog.""default"" NOT NULL,
                            ""MaxPower"" smallint NOT NULL,
                            ""MinPower"" smallint NOT NULL,
                            ""Defence"" smallint NOT NULL,
                            ""MResistance"" text COLLATE pg_catalog.""default"" NOT NULL,
                            ""CAResistance"" text COLLATE pg_catalog.""default"" NOT NULL,
                            ""EquipmentItem"" text COLLATE pg_catalog.""default"" NOT NULL,
                            ""BeltItem"" text COLLATE pg_catalog.""default"" NOT NULL,
                            ""InventoryItem"" text COLLATE pg_catalog.""default"" NOT NULL,
                            ""PosX"" smallint NOT NULL,
                            ""PosY"" smallint NOT NULL,
                            ""Direct"" smallint NOT NULL,
                            ""GuildIndex"" smallint NOT NULL,
                            ""MapSerial"" smallint NOT NULL,
                            ""Skills"" text COLLATE pg_catalog.""default"" NOT NULL,
                            ""ProgressQuests"" text COLLATE pg_catalog.""default"" NOT NULL,
                            ""Titles"" text COLLATE pg_catalog.""default"" NOT NULL,
                            CONSTRAINT ""PK_Players"" PRIMARY KEY (""PlayerId"")
                        )

                        TABLESPACE pg_default;
                        
                        -- Index: IX_Players_Name

                        -- DROP INDEX IF EXISTS public.""IX_Players_Name"";

                        CREATE UNIQUE INDEX IF NOT EXISTS ""IX_Players_Name""
                            ON public.""Players"" USING btree
                            (""Name"" COLLATE pg_catalog.""default"" ASC NULLS LAST)
                            TABLESPACE pg_default;";

                    command.ExecuteNonQuery();
                }



                using (var command = new NpgsqlCommand())
                {
                    command.Connection = conn;
                    command.CommandText = @"-- Table: public.Items

                    -- DROP TABLE IF EXISTS public.""Items"";

                    CREATE TABLE IF NOT EXISTS public.""Items""
                    (
                        ""UniqueID"" integer NOT NULL GENERATED BY DEFAULT AS IDENTITY ( INCREMENT 1 START 1 MINVALUE 1 MAXVALUE 2147483647 CACHE 1 ),
                        ""ItemIndex"" smallint NOT NULL,
                        ""Count"" smallint NOT NULL,
                        ""Endurance"" smallint NOT NULL,
                        ""Values"" bytea NOT NULL,
                        ""OPs"" bytea NOT NULL,
                        unk_bool1 boolean NOT NULL,
                        unk_bool2 boolean NOT NULL,
                        ""StackableFlag"" smallint NOT NULL,
                        unk_bool3 boolean NOT NULL,
                        unk_flag1 smallint NOT NULL,
                        unk_flag2 smallint NOT NULL,
                        unk_flag3 smallint NOT NULL,
                        unk_flag4 smallint NOT NULL,
                        unk_flag5 smallint NOT NULL,
                        CONSTRAINT ""PK_Items"" PRIMARY KEY (""UniqueID"")
                    )

                    TABLESPACE pg_default;";

                    command.ExecuteNonQuery();
                }



                using (var command = new NpgsqlCommand())
                {
                    command.Connection = conn;
                    command.CommandText = @"-- Table: public.login_log

                    -- DROP TABLE IF EXISTS public.login_log;

                    CREATE TABLE IF NOT EXISTS public.login_log
                    (
                        ""Id"" integer NOT NULL GENERATED BY DEFAULT AS IDENTITY ( INCREMENT 1 START 1 MINVALUE 1 MAXVALUE 2147483647 CACHE 1 ),
                        ""Datetime"" timestamp with time zone NOT NULL,
                        ""UserName"" text COLLATE pg_catalog.""default"" NOT NULL,
                        ""IPAddress"" text COLLATE pg_catalog.""default"" NOT NULL,
                        ""MACAddress"" text COLLATE pg_catalog.""default"" NOT NULL,
                        CONSTRAINT ""PK_login_log"" PRIMARY KEY (""Id"")
                    )

                    TABLESPACE pg_default;
                    
                    -- Index: IX_login_log_Datetime

                    -- DROP INDEX IF EXISTS public.""IX_login_log_Datetime"";

                    CREATE UNIQUE INDEX IF NOT EXISTS ""IX_login_log_Datetime""
                        ON public.login_log USING btree
                        (""Datetime"" ASC NULLS LAST)
                        TABLESPACE pg_default;";

                    command.ExecuteNonQuery();
                }

                using (var command = new NpgsqlCommand())
                {
                    command.Connection = conn;


                    command.CommandText = @"
                        SELECT count(*) FROM public.phpbb_users WHERE username = 'temp';
                    ";

                    int count = Convert.ToInt32(command.ExecuteScalar());
                    if (count == 0)
                    {
                        command.CommandText = @"
                            INSERT INTO public.phpbb_users(
                            user_id, user_type, group_id, user_permissions, user_perm_from, user_ip, user_regdate, username, username_clean, user_password, user_passchg, user_email, user_email_hash, user_birthday, user_lastvisit, user_lastmark, user_lastpost_time, user_lastpage, user_last_confirm_key, user_last_search, user_warnings, user_last_warning, user_login_attempts, user_inactive_reason, user_inactive_time, user_posts, user_lang, user_timezone, user_dateformat, user_style, user_rank, user_colour, user_new_privmsg, user_unread_privmsg, user_last_privmsg, user_message_rules, user_full_folder, user_emailtime, user_topic_show_days, user_topic_sortby_type, user_topic_sortby_dir, user_post_show_days, user_post_sortby_type, user_post_sortby_dir, user_notify, user_notify_pm, user_notify_type, user_allow_pm, user_allow_viewonline, user_allow_viewemail, user_allow_massemail, user_options, user_avatar, user_avatar_type, user_avatar_width, user_avatar_height, user_sig, user_sig_bbcode_uid, user_sig_bbcode_bitfield, user_jabber, user_actkey, user_newpasswd, user_form_salt, user_new, user_reminded, user_reminded_time)
                            VALUES (1, 1, 1, 'all', 1, '127.0.0.1', 1, 'temp', 'temp', '$2a$12$JQe2v25dAFizIQEi2i/Ll.H3LJaLaoPRuzPo7oiVLuVoi3luIjyCO', 1, 'temp@mail.com', 1, 'notext', 1, 1, 1, '1', '1', '1', '1', '1', '1', '1', '1', '1', '1', '1', '1', '1', '1', '1', '1', '1', '1', '1', '1', '1', '1', '1', '1', '1', '1', '1', '1', '1', '1', '1', '1', '1', '1', '1', '1', '1', '1', '1', '1', '1', '1', '1', '1', '1', '1', '1', '1', '1');
                            ";
                        command.ExecuteNonQuery();
                    }
                }
            }

        }
    }

}