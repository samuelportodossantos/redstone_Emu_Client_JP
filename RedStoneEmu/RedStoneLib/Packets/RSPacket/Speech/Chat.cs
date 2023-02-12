using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedStoneLib.Packets.RSPacket.Speech
{
    public class Chat : Packet
    {
        [BitField(0, 0x0B)]
        ushort CharID { get; set; }

        [BitField(1, 0x0B)]
        ushort Unknown { get; set; }

        [BitField(2, 0x05, type:typeof(ushort))]
        ChatType CType { get; set; }

        [BitField(3, 0x05)]
        byte IsGM { get; set; }

        string Name, Message;

        public enum ChatType : byte
        {
            normalChat = 0,
            wisperChat = 1,
            partyChat = 2,
            guildChat = 3,
            shoutChat = 4,
            noticeChat = 5,//鯖内の全員
            red_unknownChat = 6,
            monsterShoutChat = 7,
        }

        public Chat(ushort charID, ChatType chatType, string name, string message, bool isGM = false)
        {
            CharID = charID;
            CType = chatType;
            IsGM = isGM ? (byte)1 : (byte)0;
            Name = name;
            Message = message;
        }

        public override byte[] Build()
        {
            var writer = new PacketWriter();

            writer.Write((ushort)0);
            var test = BitField.ToBytes<Chat, BitField>(this, GetType());
            writer.Write(test);
            writer.WriteSjis(Name);
            writer.WriteSjis(Message, 140);

            packetSize = (ushort)writer.ToArray().Count();
            return writer.ToArray();
        }

        public override PacketHeader GetHeader()
        {
            return new PacketHeader(packetSize, 0x1158);
        }
    }
}
