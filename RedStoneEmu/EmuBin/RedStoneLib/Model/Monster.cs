using RedStoneLib.Karmas;
using RedStoneLib.Model.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedStoneLib.Model
{
    /// <summary>
    /// 敵
    /// </summary>
    public class Monster : Base.Actor
    {
        /// <summary>
        /// Actorの種類
        /// </summary>
        [SimpleActorInfo(0, 0x02)]
        [VerySimpleActorInfo(0, 0x02)]
        protected override byte ActorType => 2;

        /// <summary>
        /// 応急処置 0xC0
        /// </summary>
        [SimpleActorInfo(1, 0x0A)]
        public ushort AidHP { get; set; }

        /// <summary>
        /// レベル
        /// </summary>
        [SimpleActorInfo(2, 0x0A)]
        public override ushort Level { get; set; }

        /// <summary>
        /// BodySize
        /// </summary>
        [SimpleActorInfo(3, 0x08)]
        [VerySimpleActorInfo(3, type:typeof(byte))]
        private ushort BodySizeWidth => BodySize.Width;

        /// <summary>
        /// カーソルタイプ 以上 s_uint32
        /// </summary>
        [SimpleActorInfo(4, 0x02)]
        [VerySimpleActorInfo(4, 0x03, mask: 0x02)]
        public byte CursorType { get; private set; } = 0;


        /// <summary>
        /// 方向
        /// </summary>
        [SimpleActorInfo(5, 0x04, type: typeof(ushort))]
        [VerySimpleActorInfo(1, 0x03, type: typeof(ushort))]
        public override ActorDirect Direct { get; set; }

        /// <summary>
        /// CharID
        /// </summary>
        [SimpleActorInfo(6, 0x0B)]
        [VerySimpleActorInfo(2, 0x0B)]
        public override ushort CharID { get; set; }

        /// <summary>
        /// ペットフラグ
        /// </summary>
        [SimpleActorInfo(7, 0x01, type: typeof(byte))]
        [VerySimpleActorInfo(5, 0x01, type: typeof(byte))]
        public bool IsPet { get; private set; }

        /// <summary>
        /// 隠れる
        /// </summary>
        [SimpleActorInfo(8, 0x01, type: typeof(byte))]
        [VerySimpleActorInfo(6, 0x01, type: typeof(byte))]
        public bool IsHide { get; private set; }

        /// <summary>
        /// 不明 以上 s_uint32
        /// </summary>
        [SimpleActorInfo(9, 0x0F, 0x7FF)]
        public ushort Unknown_0xEC { get; private set; }

        /// <summary>
        /// 死にフラグ 以上 vs_uint32
        /// </summary>
        [VerySimpleActorInfo(7, 0x03, mask: 0x01, type: typeof(byte))]
        public bool IsDead => NowHP == 0;


        /// <summary>
        /// 現在HP
        /// </summary>
        [SimpleActorInfo(10)]
        private byte SaiNowHP
            => Convert.ToByte(NowHP * 255.0 / (MaxHP & 0xFFFFFF));

        /// <summary>
        /// 最大HP 以上 s_uint16
        /// </summary>
        [SimpleActorInfo(11, 0x18)]
        public override uint MaxHP { get; set; }


        /// <summary>
        /// 画像のIndex
        /// </summary>
        [SimpleActorInfo(12, 0x0B)]
        [VerySimpleActorInfo(8, 0x0B)]
        public ushort Image { get; set; }

        /// <summary>
        /// ActorColor NameBarBlueと0x80のみ適応 以上 s_uint16 vs_uint16
        /// </summary>
        [SimpleActorInfo(13, 0x05)]
        [VerySimpleActorInfo(9, 0x05)]
        private ushort SmallActorColor => (ushort)((ushort)color >> 5);


        /// <summary>
        /// X座標
        /// </summary>
        [SimpleActorInfo(14)]
        [VerySimpleActorInfo(10)]
        public override ushort PosX { get; set; }

        /// <summary>
        /// Y座標
        /// </summary>
        [SimpleActorInfo(15)]
        [VerySimpleActorInfo(11)]
        public override ushort PosY { get; set; }

        /// <summary>
        /// 主にスキル用のアニメ +0x2F0
        /// </summary>
        [SimpleActorInfo(18, type: typeof(uint))]
        [VerySimpleActorInfo(14, type: typeof(uint))]
        public override ActorAnim1 Anim1 { get; set; }

        /// <summary>
        /// +0x2F4
        /// </summary>
        [SimpleActorInfo(19, type: typeof(uint))]
        [VerySimpleActorInfo(15, type: typeof(uint))]
        public override ActorAnim2 Anim2 { get; set; }

        /// <summary>
        /// +0x2F8
        /// </summary>
        [SimpleActorInfo(20, type: typeof(uint))]
        [VerySimpleActorInfo(16, type: typeof(uint))]
        public override ActorAnim3 Anim3 { get; set; }

        /// <summary>
        /// +0x2FC
        /// </summary>
        [SimpleActorInfo(21, type: typeof(uint))]
        [VerySimpleActorInfo(17, type: typeof(uint))]
        public override ActorAnim4 Anim4 { get; set; }

        /// <summary>
        /// +0x300
        /// </summary>
        [SimpleActorInfo(22, type: typeof(uint))]
        [VerySimpleActorInfo(18, type: typeof(uint))]
        public override ActorAnim5 Anim5 { get; set; }

        /// <summary>
        /// 名前
        /// </summary>
        [SimpleActorInfo(23)]
        public override string Name { get; set; }

        /// <summary>
        /// ステータス
        /// </summary>
        public override ActorStatus Status { get; set; }

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
        /// 種族
        /// </summary>
        public override Breed.ActorRace Race
            => ABase.Race;

        /// <summary>
        /// 職業のインデックス
        /// </summary>
        public override Player.JOB Job { get; set; }

        /// <summary>
        /// 取りうるレベルのスケール
        /// </summary>
        public override Scale<ushort> LevelScale { get; set; }

        /// <summary>
        /// 湧き時間
        /// </summary>
        public uint PopSpeed { get; set; }

        /// <summary>
        /// リスポーン済み
        /// </summary>
        public bool Respawned { get; set; }

        /// <summary>
        /// 色　NameBarBlue未満は使用不可
        /// </summary>
        public ActorColor color = ActorColor.BodyNormal;

        [SimpleActorInfo(16)]
        [VerySimpleActorInfo(12)]
        private uint junk0 { get; set; }

        [SimpleActorInfo(17)]
        [VerySimpleActorInfo(13)]
        private ushort junk1 { get; set; }

        [VerySimpleActorInfo(19)]
        private uint junk2 { get; set; }

        /// <summary>
        /// イベント
        /// </summary>
        public readonly Event[] Events;

        /// <summary>
        /// タイミングによって発動するKarma
        /// </summary>
        public MapActorGroup.MapEnemyKarmaInfo[] EnemyKarmaInfo;

        /// <summary>
        /// Regenコンストラクタ　レベル抽選
        /// </summary>
        /// <param name="single"></param>
        /// <param name="group"></param>
        /// <param name="charID"></param>
        public Monster(MapActorSingle single, MapActorGroup group, ushort charID)
            : base(single, group, charID)
        {
            Image = group.ImageID;
            Events = single.Events;
            PopSpeed = single.PopSpeed;
            EnemyKarmaInfo = group.EnemyKarmaInfos;
            Direct = (ActorDirect)((ushort)single.Direct + Helper.StaticRandom.Next(8));
            Respawned = false;

            //レベルセット
            Level = (ushort)Helper.StaticRandom.Next(group.MinLevel, group.MaxLevel);
            SetStatusByLevel(Level);
        }
    }
}
