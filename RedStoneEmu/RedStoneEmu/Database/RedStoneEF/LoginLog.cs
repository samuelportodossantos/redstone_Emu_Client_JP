using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RedStoneEmu.Database.RedStoneEF
{
    [Table("login_log")]
    public class LoginLog
    {
        /// <summary>
        /// プライマリキー
        /// </summary>
        [Key]
        [Column("id")]
        public int Id { get; set; }

        /// <summary>
        /// 日時
        /// </summary>
        [Column("datetime")]
        public DateTime Datetime { get; set; }


        [Column("username")]
        public string UserName { get; set; }

        /// <summary>
        /// IP
        /// </summary>
        [Column("ip_address")]
        public string IPAddress { get; set; }

        /// <summary>
        /// MACアドレス
        /// </summary>
        [Column("mac_address")]
        public string MACAddress { get; set; }
    }
}
