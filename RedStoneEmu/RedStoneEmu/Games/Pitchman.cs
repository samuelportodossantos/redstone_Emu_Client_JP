using RedStoneLib.Model;
using RedStoneLib.Packets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static RedStoneLib.Packets.RSPacket.PitchmanShop.ChangePitchmanShopInfoPacket;

namespace RedStoneEmu.Games
{
    /// <summary>
    /// 露店
    /// </summary>
    class Pitchman
    {
        /// <summary>
        /// ショップID
        /// </summary>
        public readonly uint ShopID;

        /// <summary>
        /// プレイヤの名前
        /// </summary>
        public readonly string PlayerName;

        /// <summary>
        /// 場所
        /// </summary>
        public readonly ushort PosX;
        public readonly ushort PosY;

        /// <summary>
        /// 露店主にパケット送信
        /// </summary>
        public readonly Action<Packet> SendPacket;

        /// <summary>
        /// 露店状態
        /// </summary>
        public ShopState State;

        /// <summary>
        /// 露店広告内容
        /// </summary>
        public string Advertising;

        /// <summary>
        /// アイテムスロット
        /// </summary>
        public (int slot, Item item, uint price, byte isIngot)[] ItemSlots { get; set; }

        /// <summary>
        /// プレイヤ本体
        /// </summary>
        Player PitchmanPlayer;

        
        public Pitchman(ushort mapSerial, Player player, ushort posX, ushort posY, Action<Packet> sendPacket)
        {
            ShopID = GetUnusedShopID(mapSerial);
            PosX = posX;
            PosY = posY;
            PitchmanPlayer = player;
            PlayerName = player.Name;

            ItemSlots = new(int slot, Item item, uint price, byte isIngot)[9];
            for (int i = 0; i < 9; i++)
            {
                ItemSlots[i].slot = -1;
                ItemSlots[i].item = new Item();
                ItemSlots[i].price = 0;
                ItemSlots[i].isIngot = 0;
            }

            SendPacket = sendPacket;
            State = ShopState.Preparing;
        }

        /// <summary>
        /// 未使用ShopID探す
        /// </summary>
        /// <param name="mapSerial"></param>
        /// <returns></returns>
        uint GetUnusedShopID(ushort mapSerial)
        {
            MAPServer map = MAPServer.AllMapServers[mapSerial];
            
            return (uint)Enumerable.Range(0, int.MaxValue).First(t => !map.PitchmanShops.Select(u => u.ShopID).Contains((uint)t));
        }

        /// <summary>
        /// プレイヤ取得
        /// </summary>
        /// <returns></returns>
        public Player GetPlayer() => PitchmanPlayer;

        /// <summary>
        /// パケット用外見データ
        /// </summary>
        /// <returns></returns>
        public byte[] OutwardData()
        {
            var writer = new PacketWriter();
            writer.Write(ShopID);
            writer.Write((ushort)(0x70 | (ushort)State));
            writer.Write(PosX);
            writer.Write(PosY);
            writer.WriteSjis(Advertising);
            writer.Write((byte)0);
            return writer.ToArray();
        }

        /// <summary>
        /// パケット用内面データ
        /// </summary>
        /// <returns></returns>
        public byte[] InnerData()
        {
            var writer = new PacketWriter();
            writer.Write((ushort)0);
            writer.Write(ShopID);
            writer.Write((byte)(ItemSlots.Count(t => t.slot != -1) << 4));
            writer.Write((byte)0xCC);
            writer.WriteSjis(Advertising, 0x3E);
            writer.WriteSjis(PlayerName, 0x12);

            writer.Write((ushort)0xCCCC);
            foreach (var v in ItemSlots.Select((slot, i) => new { slot, i }).Where(t => t.slot.slot != -1))
            {
                writer.Write(v.slot.item);
                writer.Write((ushort)1);
                writer.Write(v.slot.price);
                writer.Write((ushort)v.i);
                writer.Write(v.slot.isIngot);
                writer.Write((byte)0xCC);
            }

            return writer.ToArray();
        }
    }
}
