using RedStoneLib.Karmas;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedStoneLib.Packets.RSPacket.KarmaPacket
{
    /// <summary>
    /// NPCがスピーチする
    /// </summary>
    public class ComplexSpeechPacket : Packet
    {
        ushort CharID;
        Event @Event;

        /// <summary>
        /// NPCがスピーチする
        /// </summary>
        /// <param name="charID">NPCのcharID</param>
        /// <param name="event"></param>
        public ComplexSpeechPacket(ushort charID, Event @event)
        {
            CharID = charID;
            Event = @event;
        }

        public override byte[] Build()
        {
            var writer = new PacketWriter();

            //EventCharID
            writer.Write(CharID);
            writer.Write((ushort)0xFFFF);

            //イベント書き込み
            @Event.Write(writer);

            packetSize = (ushort)writer.ToArray().Count();
            return writer.ToArray();
        }

        public override PacketHeader GetHeader()
        {
            return new PacketHeader(packetSize, 0x115C);
        }
    }
}
