using RedStoneLib.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedStoneLib.Packets.RSPacket.Action
{
    /// <summary>
    /// 通常攻撃
    /// </summary>
    public class BasicAttackPacket : Packet
    {
        Point<ushort> FromPos;
        ushort ToCharID, FromCharID, SkillID;
        int PhysicDamage, MagicDamage;

        byte NotMiss, Fatal = 0;//致命打

        public BasicAttackPacket(Point<ushort> fromPos, ushort fromCharID, ushort toCharID, ushort skillID, int physicDamage, Magic magicDamage, bool isAvoid)
        {
            FromPos = fromPos;
            ToCharID = toCharID;
            FromCharID = fromCharID;
            SkillID = skillID;
            PhysicDamage = physicDamage;
            MagicDamage = magicDamage.Fire + magicDamage.Water + magicDamage.Wind + magicDamage.Earth + magicDamage.Light + magicDamage.Dark;
            NotMiss = isAvoid ? (byte)0 : (byte)1;
        }

        public override byte[] Build()
        {
            var writer = new PacketWriter();

            //アクターとスキルID
            var actorAndSkillInfo = (FromCharID & 0x7FF) | ((ToCharID & 0x7FF) << 0x0B) | (SkillID << 0x16);

            //ダメージフラグ
            var damageFlags = NotMiss | (Fatal << 1) | 0x50;

            writer.Write(FromPos.X);
            writer.Write(FromPos.Y);
            writer.Write((ushort)0x56);
            writer.Write(actorAndSkillInfo);
            writer.Write(MagicDamage);//黄ダメ
            writer.Write(PhysicDamage);//白ダメ
            writer.Write(damageFlags);
            writer.Write((ushort)0);

            packetSize = (ushort)writer.ToArray().Count();
            return writer.ToArray();
        }

        public override PacketHeader GetHeader()
        {
            return new PacketHeader(packetSize, 0x1130);
        }
    }
}
