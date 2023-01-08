using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace RedStoneEmu.Database.SMFEF
{
    public partial class Members
    {
        [Key]
        public int id_member { get; set; }
        public string member_name { get; set; }
        public int date_registered { get; set; }
        public int posts { get; set; }
        public int id_group { get; set; }
        public string lngfile { get; set; }
        public int last_login { get; set; }
        public string real_name { get; set; }
        public int instant_messages { get; set; }
        public int unread_messages { get; set; }
        public int new_pm { get; set; }
        public int alerts { get; set; }
        public string buddy_list { get; set; }
        public string pm_ignore_list { get; set; }
        public int pm_prefs { get; set; }
        public string mod_prefs { get; set; }
        public string passwd { get; set; }
        public string email_address { get; set; }
        public string personal_text { get; set; }
        public DateTime birthdate { get; set; }
        public string website_title { get; set; }
        public string website_url { get; set; }
        public int show_online { get; set; }
        public string time_format { get; set; }
        public string signature { get; set; }
        public double time_offset { get; set; }
        public string avatar { get; set; }
        public string usertitle { get; set; }
        public IPAddress member_ip { get; set; }
        public IPAddress member_ip2 { get; set; }
        public string secret_question { get; set; }
        public string secret_answer { get; set; }
        public int id_theme { get; set; }
        public int is_activated { get; set; }
        public string validation_code { get; set; }
        public int id_msg_last_visit { get; set; }
        public string additional_groups { get; set; }
        public string smiley_set { get; set; }
        public int id_post_group { get; set; }
        public int total_time_logged_in { get; set; }
        public string password_salt { get; set; }
        public string ignore_boards { get; set; }
        public int warning { get; set; }
        public string passwd_flood { get; set; }
        public int pm_receive_from { get; set; }
        public string timezone { get; set; }
        public string tfa_secret { get; set; }
        public string tfa_backup { get; set; }
    }
}
