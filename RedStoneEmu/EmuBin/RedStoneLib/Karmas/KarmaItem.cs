using RedStoneLib.Packets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedStoneLib.Karmas
{

    /// <summary>
    /// 条件によるKarmaItem
    /// </summary>
    public class KarmaItemCondition : KarmaItem
    {
        public KarmaItemCondition(PacketReader pr)
            : base(pr) { }

        /// <summary>
        /// 条件
        /// </summary>
        public ushort Index { get => (ushort)(_karmaItem & 0xFFFF); }

        /// <summary>
        /// Override
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            string result = ((KarmaCondition)Index).ToString();
            if (Message != null) result += string.Format("({0})", Message);
            return result;
        }

        /// <summary>
        /// 条件の値
        /// </summary>
        public enum KarmaCondition : ushort
        {
            not_condition = 0,//条件なし
            level_lower_than = 0x01,
            have_item_range = 3,
            have_quest_item = 0x04,
            have_money = 0x06,
            have_not_title = 0x08,//称号持ってる
            have_item = 0x0f,//アイテム持ってるか
            bag_space = 0x10,
            level = 0x0D,
            gm = 0x12,
            have_premium_item = 0x26,//スフィア持ってる
            notEnd_quest = 0x132,
            process_quest = 0x133,
            valuecount_quest = 0x134,//クエスト変数をカウント
            void_quest = 0x135,//クエストの空き
            isend_quest = 0x136,
            year = 0x193,//現在年が指定時以下？
            country = 0x19C,
            season_variable = 0x201
        }
    }

    /// <summary>
    /// 実行によるKarmaItem
    /// </summary>
    public class KarmaItemCommand : KarmaItem
    {
        public KarmaItemCommand(PacketReader pr)
            : base(pr)
        {
        }

        /// <summary>
        /// 実行
        /// </summary>
        public ushort Index { get => (ushort)(_karmaItem & 0xFFFF); }

        /// <summary>
        /// Override
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            string result = ((KarmaExecute)Index).ToString();
            if (Message != null) result += string.Format("({0})", Message);
            return result;
        }

        /// <summary>
        /// 実行の値
        /// </summary>
        public enum KarmaExecute : ushort
        {
            skip_nowspeech = 0,
            close_dialog = 1,
            open_shop = 2,
            opeb_bank = 3,
            move_field = 0x6A,
            send_message = 0x6C,
            add_quest = 0x6F,
            update_quest = 0x70,
            value_quest = 0x72,//クエストで変数を使う（露店クエ用）
            cancel_quest = 0x74,
            gain_exp = 0x75,
            complete_quest = 0x77,
            give_money = 0xc9,
            give_item = 0xca,
            drop_item = 0xcd,
            drop_gift_item = 0xE1,
            give_quest_item = 0xd1,
            teleport_to_actor = 0xd3,
            say_monster = 0x12E,
            heal_monster = 0x12F,
            add_title = 0x132,
            ingot_dialog = 0x1B2,
            add_box = 0x1B6,//プレゼントボックス？
        }

    }

    public class KarmaItem
    {
        /// <summary>
        /// 条件もしくは実行値
        /// </summary>
        protected uint _karmaItem;
        
        /// <summary>
        /// 素のIndex
        /// </summary>
        public ushort _Index{ get => (ushort)(_karmaItem & 0xFFFF); }

        public readonly uint[] Value; 

        public readonly uint Unknown_0;
        public readonly uint Unknown_1;
        public readonly bool Unknown_2;
        public readonly string Message;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="pr"></param>
        public KarmaItem(PacketReader pr)
        {
            using (PacketReader baseReader = new PacketReader(
                pr.EncryptionReads<byte>(0x20)))
            {
                //条件or実行
                _karmaItem = baseReader.ReadUInt32();

                //値
                Value = new uint[4];
                for (int i = 0; i < Value.Length; i++)
                {
                    Value[i] = baseReader.ReadUInt32();
                }

                //unknown
                Unknown_0 = baseReader.ReadUInt32();
                Unknown_1 = baseReader.ReadUInt32();

                //メッセージ
                uint messageFlags = baseReader.ReadUInt16();
                int messageLength = (int)(messageFlags & 0x7FFF);
                Unknown_2 = ((messageFlags >> 15) & 1) == 1;
                if (Unknown_2)
                {
                    Console.WriteLine("whats");
                }

                if (!Unknown_2 && messageLength > 0)
                {
                    Message = Helper.SjisByteToString(pr.EncryptionReads<byte>(messageLength));
                }
                else
                {
                    Message = null;
                }
            }
        }
    }
}
