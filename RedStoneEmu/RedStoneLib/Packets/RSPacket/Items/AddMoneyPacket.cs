using RedStoneLib.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedStoneLib.Packets.RSPacket.Items
{
    /// <summary>
    /// お金取得
    /// </summary>
    public class AddMoneyPacket : Packet
    {
        public enum MoneyGetType : ushort
        {
            getMoney = 0,//通常の入手
            pushGBank = 1,//GHに入れる
            putGBank = 2//GHから取る
        }

        MoneyGetType Gettype;
        uint Money;

        public AddMoneyPacket(uint money, MoneyGetType gettype)
        {
            Money = money;
            Gettype = gettype;
        }

        public override byte[] Build()
        {
            var writer = new PacketWriter();

            writer.Write((ushort)0);
            writer.Write(Money);
            writer.Write((ushort)Gettype);

            packetSize = (ushort)writer.ToArray().Count();
            return writer.ToArray();
        }

        public override PacketHeader GetHeader()
        {
            return new PacketHeader(packetSize, 0x11A3);
        }
    }
}
