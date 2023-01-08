using Microsoft.EntityFrameworkCore;
using RedStoneLib.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using static RedStoneLib.Model.Player;

namespace RedStoneEmu.Database.RedStoneEF
{
    /// <summary>
    /// ゲーム用のcontext
    /// </summary>
    public class gameContext : DbContext
    {
        /// <summary>
        /// プレイヤ情報
        /// </summary>
        public DbSet<Player> Players { get; set; }

        /// <summary>
        /// アイテム情報
        /// </summary>
        public DbSet<Item> Items { get; set; }

        /// <summary>
        /// 銀行
        /// </summary>
        public DbSet<Bank> Banks { get; set; }

        /// <summary>
        /// ゲームサーバーの設定ファイル
        /// </summary>
        private static ServerConfig Config = null;

        /// <summary>
        /// 設定ファイル読み込み
        /// </summary>
        static gameContext()
        {
            if (RedStoneApp.GameServer != null)
            {
                Config = RedStoneApp.GameServer.Config;
            }
            else
            {
                Config = new ServerConfig(ServerType.Game);
                if (!Config.Load()) throw new InvalidOperationException("ゲームサーバーの設定ファイルが読み込めませんでした．");
            }
        }
        
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseNpgsql(string.Format("Host={0};User Id={1};Password={2};Database={3};Port={4}",
                "192.168.11.18",
                "dmarch",
                "hourai",
                "redstone", 5432));
        }
        
        /// <summary>
        /// ここでuniqueなどを設定
        /// </summary>
        /// <param name="modelBuilder"></param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Player>()
                 .HasIndex(u => u.Name)
                 .IsUnique();

            //private
            modelBuilder.Entity<Player>()
            .Property(typeof(short), "Level_DB");
            modelBuilder.Entity<Player>()
            .Property(typeof(short), "StateHPCPBonus_DB");
            modelBuilder.Entity<Player>()
            .Property(typeof(short), "LevelHPCPBobuns_DB");
            modelBuilder.Entity<Player>()
            .Property(typeof(string), "Status_DB");
            modelBuilder.Entity<Player>()
            .Property(typeof(short), "MaxPower_DB");
            modelBuilder.Entity<Player>()
            .Property(typeof(short), "MinPower_DB");
            modelBuilder.Entity<Player>()
            .Property(typeof(short), "Defence_DB");
            modelBuilder.Entity<Player>()
            .Property(typeof(string), "Mresistance_DB");
            modelBuilder.Entity<Player>()
            .Property(typeof(string), "Caresistance_DB");
            modelBuilder.Entity<Player>()
            .Property(typeof(string), "EquipmentItem_DB");
            modelBuilder.Entity<Player>()
            .Property(typeof(string), "BeltItem_DB");
            modelBuilder.Entity<Player>()
            .Property(typeof(string), "InventoryItem_DB");
            modelBuilder.Entity<Player>()
            .Property(typeof(short), "MapSerial_DB");
            modelBuilder.Entity<Player>()
            .Property(typeof(short), "PosX_DB");
            modelBuilder.Entity<Player>()
            .Property(typeof(short), "PosY_DB");
            modelBuilder.Entity<Player>()
            .Property(typeof(short), "Direct_DB");
            modelBuilder.Entity<Player>()
            .Property(typeof(short), "GuildIndex_DB");
            modelBuilder.Entity<Player>()
            .Property(typeof(string), "Skills_DB");
            modelBuilder.Entity<Player>()
            .Property(typeof(string), "ProgressQuests_DB");
            modelBuilder.Entity<Player>()
            .Property(typeof(string), "Titles_DB");

            base.OnModelCreating(modelBuilder);
        }

        /// <summary>
        /// プレイヤー削除
        /// </summary>
        /// <param name="targetAvatar"></param>
        public void RemovePlayer(Player targetAvatar)
        {
            //アイテム削除
            foreach (var toRemove in new IEnumerable<Item>[] {
                targetAvatar.EquipmentItems.Where(t => !t.IsEmpty),//装備
                targetAvatar.BeltItems.Where(t => !t.IsEmpty),//ベルト
                targetAvatar.InventoryItems.Where(t => !t.IsEmpty), //インベ
            })
            {
                Items.RemoveRange(toRemove);
            }

            Bank bank = Banks.SingleOrDefault(t => t.UserID == targetAvatar.UserID);
            if (bank != null)
            {
                //銀行アイテム削除
                Items.RemoveRange(bank.UniqueItemString.Split(',')
                    .Where(t => t != "0")
                    .Select(t => Items.Single(u => u.UniqueID == Convert.ToInt32(t))));

                //銀行削除
                Banks.Remove(bank);
            }

            //プレイヤー削除
            Players.Remove(targetAvatar);
        }

        /// <summary>
        /// キャラ内のアイテム読み込み
        /// </summary>
        /// <param name="db"></param>
        /// <param name="area">操作対象</param>
        public void LoadItems(Player player, TargetItemArea area)
        {
            //文字列からロード
            Item[] LoadItems(string str)
            {
                //UniqueIDのリスト（ゼロ以外）
                List<int> uids = str.Split(',').Select(t => Convert.ToInt32(t)).Where(t => t != 0).ToList();

                var items = Items.ToList();

                //0以外を引き出して辞書化
                var task = Items.Where(t => uids.Contains(t.UniqueID)).ToDictionaryAsync(t => t.UniqueID, t => t);
                task.Wait();
                var itemsWithoutZero = task.Result;

                //0を設定
                itemsWithoutZero[0] = new Item();

                //統合して返す
                return str.Split(',').Select(t => itemsWithoutZero[Convert.ToInt32(t)]).ToArray();
            }

            //アイテムのロード
            if (area.HasFlag(TargetItemArea.EquipmentItem))
            {
                player.EquipmentItems = new EquipmentItemCollection(player, LoadItems(player.UniqueIdStringsFor.EquipmentItem));
                player.EquipmentItems.OnChangeEffect += player.HandleChangeEffect;
            }
            if (area.HasFlag(TargetItemArea.BeltItem))
                player.BeltItems = (ItemCollection)LoadItems(player.UniqueIdStringsFor.BeltItem);
            if (area.HasFlag(TargetItemArea.InventoryItem))
            {
                player.InventoryItems = (ItemCollection)LoadItems(player.UniqueIdStringsFor.InventoryItem);
                player.InventoryItems.OnChangeEffect += player.HandleChangeEffect;
            }
        }


        /// <summary>
        /// 操作対象のアイテムエリア
        /// </summary>
        [Flags]
        public enum TargetItemArea : byte
        {
            ALL = byte.MaxValue,
            EquipmentItem = 1,
            BeltItem = 2,
            InventoryItem = 4,
        }
    }
}
