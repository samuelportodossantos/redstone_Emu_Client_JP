using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RedStoneLib.Packets.RSPacket.PitchmanShop
{
    /// <summary>
    /// 露店メッセージ出力
    /// </summary>
    public class PitchmanShopMessagePacket : Packet
    {
        public enum PitchmanShopResult : ushort
        {
            Need_Trading = 2,//	取引人の称号が必要
            Valid_Position = 3,//	位置が適切ではない
            TooMany_ShopNum = 4,//	露店の数が多すぎ
            ShopOpen_failed = 5,//	露店の開店に失敗しました
            AlreadyRegistered = 6,//	既に登録されてるアイテムです
            EmptySlot1 = 7,//	空きスロットです
            NotOpenShop1 = 8,//	露天を開店した状態ではありません
            EmptySlot2 = 9,//	空きスロットです
            NotOpenShop2 = 10,//	露天を開店した状態ではありません
            ChangedInformation = 11,//	既にアイテムが売れているか，情報が変わりました
            NotEnoughMoney = 12,//	そのアイテムを買うにはお金が足りません
            FullInventory = 13,//	インベントリに空きがありません
            FullMoney_Master = 14,//	商店の主人のお金が多すぎ
            Preparation = 15,//	開店準備中の露店です
            CantTradeItem = 17,//	そのアイテムは取引できないアイテムです
            CantUseName = 19,//	露天商の名前に使うことができない名前です
            NotEnoughIngot = 20,//	金のインゴットが足りません
            NotEnoughInventory_Master = 21,//	販売者のインベントリが不足しています
            FullMoney = 22,//	これ以上の金額を所持できません
            UnableOpenShop = 23,//	現在のフィールドでは露店を開けません
        }

        PitchmanShopResult Result;
        
        public PitchmanShopMessagePacket(PitchmanShopResult result)
        {
            Result = result;
        }

        public override byte[] Build()
        {
            var writer = new PacketWriter();

            writer.Write((ushort)Result);

            packetSize = (ushort)writer.ToArray().Count();
            return writer.ToArray();
        }

        public override PacketHeader GetHeader()
        {
            return new PacketHeader(packetSize, 0x120E);
        }
    }
}
