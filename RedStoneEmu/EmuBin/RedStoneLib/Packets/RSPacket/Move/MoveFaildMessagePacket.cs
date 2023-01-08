using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedStoneLib.Packets.RSPacket.Move
{
    public class MoveFaildMessagePacket : Packet
    {
        ushort flags, posX, posY;

        [Flags]
        public enum MoveFaildMessage : ushort
        {
            /// <summary>
            /// 現在移動ができない状態です
            /// </summary>
            ImpossibleToMove = 0x3000,

            /// <summary>
            /// なにもせず，ただOnlyCastIcyStalagmiteする
            /// </summary>
            Nothing_OnlyCastIcyStalagmite = 0x800,

            /// <summary>
            /// 位置合わせだけ
            /// </summary>
            OnlySetPos=0,
        }

        /// <summary>
        /// なにもしない
        /// </summary>
        /// <param name="message"></param>
        public MoveFaildMessagePacket()
        {
            flags = (ushort)MoveFaildMessage.Nothing_OnlyCastIcyStalagmite;
            posX = 0;
            posY = 0;
        }

        /// <summary>
        /// 位置合わせ
        /// </summary>
        /// <param name="message"></param>
        /// <param name="posX"></param>
        /// <param name="posY"></param>
        /// <param name="MoveSpeed"></param>
        /// <param name="IsRun"></param>
        public MoveFaildMessagePacket(MoveFaildMessage message, ushort posX, ushort posY, ushort MoveSpeed, bool IsRun)
        {
            flags = (ushort)((IsRun ? 1 : 0) | ((MoveSpeed << 1) & 0x3FF) | (ushort)message);
            this.posX = posX;
            this.posY = posY;
        }
        public override byte[] Build()
        {

            var writer = new PacketWriter();

            writer.Write(flags);
            writer.Write(posX);
            writer.Write(posY);

            packetSize = (ushort)writer.ToArray().Count();
            return writer.ToArray();
        }

        public override PacketHeader GetHeader()
        {
            return new PacketHeader(packetSize, 0x118A);
        }
    }
}
