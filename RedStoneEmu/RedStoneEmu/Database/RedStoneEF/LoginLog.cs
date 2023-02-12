using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RedStoneEmu.Database.RedStoneEF
{
    [Table("login_log")]
    public class LoginLog
    {
        public int Id { get; set; }
        public DateTime Datetime { get; set; }
        public string UserName { get; set; }
        public string IPAddress { get; set; }
        public string MACAddress { get; set; }
    }
}
