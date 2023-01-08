using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace RedStoneLib.Packets.RSPacket.BCS
{
    /// <summary>
    /// ブロードキャストのデータ？
    /// </summary>
    public class BCSDataPacket : Packet
    {

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct CustomData
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 430)]
            public byte[] Unknown_1;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 10)]
            public Friend[] FriendList;
            
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4296)]
            public byte[] Unknown_2;

            [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
            public struct Friend
            {
                uint Unknown_1;

                [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 14)]
                string GroupName;

                public Friend(uint unk, string name)
                {
                    Unknown_1 = unk;
                    GroupName = name;
                }
            }
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="ipAddr"></param>
        /// <param name="port"></param>
        public BCSDataPacket()
        {
        }

        public override byte[] Build()
        {
            var writer = new PacketWriter();

            writer.Write((uint)1);//setCustomDataFlag
            writer.Write((uint)0x1a75);//junk

            CustomData cd = new CustomData
            {
                Unknown_1 = Enumerable.Range(0, 430).Select(_ => (byte)0xFF).ToArray(),
                FriendList = Enumerable.Range(0, 10).Select(t => new CustomData.Friend(0, string.Format("Group#{0:00}", t))).ToArray(),
                Unknown_2 = new byte[4296]
            };
            writer.WriteStruct(cd);
            writer.Write((uint)0x7013);//junk
            writer.Write((ushort)0xCCCC);//junk


            //0x2A61316, EBX+1C1330
            //writer.Write((ushort)1);

            packetSize = (ushort)writer.ToArray().Count();
            return writer.ToArray();
        }

        public override PacketHeader GetHeader()
        {
            return new PacketHeader(packetSize, 0x7008);
        }
    }
}
