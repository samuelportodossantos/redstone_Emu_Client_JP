using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedStoneEmu.Database.PhpbbEF
{
    public class Users
    {
        [Key]
        public int user_id { get; set; }
        public int user_type { get; set; }
        public int group_id { get; set; }
        public string user_permissions { get; set; }
        public int user_perm_from { get; set; }
        public string user_ip { get; set; }
        public int user_regdate { get; set; }
        public string username { get; set; }
        public string username_clean { get; set; }
        public string user_password { get; set; }
        public int user_passchg { get; set; }
        public string user_email { get; set; }
        public int user_email_hash { get; set; }
        public string user_birthday { get; set; }
        public int user_lastvisit { get; set; }
        public int user_lastmark { get; set; }
        public int user_lastpost_time { get; set; }
        public string user_lastpage { get; set; }
        public string user_last_confirm_key { get; set; }
        public int user_last_search { get; set; }
        public int user_warnings { get; set; }
        public int user_last_warning { get; set; }
        public int user_login_attempts { get; set; }
        public int user_inactive_reason { get; set; }
        public int user_inactive_time { get; set; }
        public int user_posts { get; set; }
        public string user_lang { get; set; }
        public string user_timezone { get; set; }
        public string user_dateformat { get; set; }
        public int user_style { get; set; }
        public int user_rank { get; set; }
        public string user_colour { get; set; }
        public int user_new_privmsg { get; set; }
        public int user_unread_privmsg { get; set; }
        public int user_last_privmsg { get; set; }
        public int user_message_rules { get; set; }
        public int user_full_folder { get; set; }
        public int user_emailtime { get; set; }
        public int user_topic_show_days { get; set; }
        public string user_topic_sortby_type { get; set; }
        public string user_topic_sortby_dir { get; set; }
        public int user_post_show_days { get; set; }
        public string user_post_sortby_type { get; set; }
        public string user_post_sortby_dir { get; set; }
        public int user_notify { get; set; }
        public int user_notify_pm { get; set; }
        public int user_notify_type { get; set; }
        public int user_allow_pm { get; set; }
        public int user_allow_viewonline { get; set; }
        public int user_allow_viewemail { get; set; }
        public int user_allow_massemail { get; set; }
        public int user_options { get; set; }
        public string user_avatar { get; set; }
        public string user_avatar_type { get; set; }
        public int user_avatar_width { get; set; }
        public int user_avatar_height { get; set; }
        public string user_sig { get; set; }
        public string user_sig_bbcode_uid { get; set; }
        public string user_sig_bbcode_bitfield { get; set; }
        public string user_jabber { get; set; }
        public string user_actkey { get; set; }
        public string user_newpasswd { get; set; }
        public string user_form_salt { get; set; }
        public int user_new { get; set; }
        public int user_reminded { get; set; }
        public int user_reminded_time { get; set; }
    }
}
