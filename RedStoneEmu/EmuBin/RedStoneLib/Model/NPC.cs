using RedStoneLib.Packets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RedStoneLib.Model;
using System.Runtime.InteropServices;
using RedStoneLib.Karmas;
using RedStoneLib.Model.Base;

namespace RedStoneLib.Model
{
    public sealed class NPC : Base.Actor
    {
        /// <summary>
        /// Actorの種類
        /// </summary>
        [SimpleActorInfo(0, 0x02)]
        [VerySimpleActorInfo(0, 0x02)]
        protected override byte ActorType => 1;

        /// <summary>
        /// NPCのタイプ
        /// </summary>
        [SimpleActorInfo(1, 0x06, type: typeof(ushort))]
        [VerySimpleActorInfo(6, 0x06, type: typeof(ushort))]
        public MapActorSingle.CType NPCType { get; set; }

        /// <summary>
        /// 方向
        /// </summary>
        [SimpleActorInfo(2, 0x03, type: typeof(ushort))]
        [VerySimpleActorInfo(3, 0x03, type: typeof(ushort))]
        public override ActorDirect Direct { get; set; }

        /// <summary>
        /// CharID
        /// </summary>
        [SimpleActorInfo(3, 0x0B)]
        [VerySimpleActorInfo(1, 0x0B)]
        public override ushort CharID { get; set; }

        /// <summary>
        /// NPCのカーソルフラグ
        /// </summary>
        [SimpleActorInfo(4, 0x02)]
        public byte CursorFlag { get; private set; } = 2;

        /// <summary>
        /// SAI用BodySize
        /// </summary>
        [SimpleActorInfo(5, 0x08)]
        [VerySimpleActorInfo(8, type: typeof(byte))]
        private ushort BodyWidth { get => BodySize.Width; }

        /// <summary>
        /// 画像のIndex
        /// </summary>
        [SimpleActorInfo(6, 0x0B)]
        [VerySimpleActorInfo(2, 0x0B)]
        public ushort Image { get; set; }

        /// <summary>
        /// クリックフラグ
        /// </summary>
        [SimpleActorInfo(7, 0x01, type: typeof(byte))]
        [VerySimpleActorInfo(4, 0x01, type: typeof(byte))]
        public bool CanClick { get => Events != null && Events.Length != 0; }

        /// <summary>
        /// 名前無しフラグ
        /// </summary>
        [SimpleActorInfo(8, 0x04, 0x01, type: typeof(byte))]
        [VerySimpleActorInfo(5, 0x04, 0x01, type: typeof(byte))]
        public bool NoName { get; private set; } = false;

        /// <summary>
        /// X座標
        /// </summary>
        [SimpleActorInfo(9)]
        [VerySimpleActorInfo(9)]
        public override ushort PosX { get; set; }

        /// <summary>
        /// Y座標
        /// </summary>
        [SimpleActorInfo(10)]
        [VerySimpleActorInfo(10)]
        public override ushort PosY { get; set; }

        /// <summary>
        /// 名前
        /// </summary>
        [SimpleActorInfo(11)]
        public override string Name { get; set; }

        /// <summary>
        /// ステータス
        /// </summary>
        public override ActorStatus Status { get; set; }

        /// <summary>
        /// 最大HP
        /// </summary>
        public override uint MaxHP { get; set; }

        /// <summary>
        /// 現在HP
        /// </summary>
        public override uint NowHP { get; set; }
        
        /// <summary>
        /// 最大HP
        /// </summary>
        public override uint MaxCP { get; set; }

        /// <summary>
        /// 現在HP
        /// </summary>
        public override int NowCP { get; set; }

        /// <summary>
        /// 攻撃力
        /// </summary>
        public override Func<double, double, Scale<double>> Attack { get; set; }

        /// <summary>
        /// 防御力
        /// </summary>
        public override double Defence { get; set; }

        /// <summary>
        /// 魔法抵抗
        /// </summary>
        public override Magic MagicResistance { get; set; }

        /// <summary>
        /// ボディサイズ
        /// </summary>
        public override Size<ushort> BodySize { get; set; }

        /// <summary>
        /// レベル
        /// </summary>
        public override ushort Level { get; set; }

        /// <summary>
        /// 攻撃可能
        /// </summary>
        [VerySimpleActorInfo(7, 0x02, type: typeof(byte))]
        public byte CanAttack { get; private set; } = 0;

        /// <summary>
        /// junk
        /// </summary>
        [VerySimpleActorInfo(11)]
        private ushort Junk { get; set; } = 0;

        /// <summary>
        /// 種族
        /// </summary>
        public override Breed.ActorRace Race
            => ABase.Race;

        /// <summary>
        /// 職業ID
        /// </summary>
        public override Player.JOB Job { get; set; }
        
        /// <summary>
        /// 取りうるレベルのスケール
        /// </summary>
        public override Scale<ushort> LevelScale { get; set; }

        /// <summary>
        /// イベント
        /// </summary>
        public Event[] Events { get; set; }

        public NPC(MapActorSingle single, MapActorGroup group, ushort charID)
            :base(single, group, charID)
        {
            Image = group.ImageID;
            NPCType = single.CharType;
            Events = single.Events;

            //レベルセット
            Level = (ushort)Helper.StaticRandom.Next(group.MinLevel, group.MaxLevel);
            SetStatusByLevel(Level);
        }
    }
}
