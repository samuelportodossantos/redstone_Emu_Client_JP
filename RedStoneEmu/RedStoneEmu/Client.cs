using RedStoneEmu.Database.RedStoneEF;
using RedStoneLib.Model;
using RedStoneLib.Model.Base;
using RedStoneEmu.NetWork;
using RedStoneLib.Packets;
using RedStoneEmu.Packets.Handlers;
using RedStoneLib.Packets.RSPacket.Other;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedStoneEmu
{
    /// <summary>
    /// ログイン用
    /// </summary>
    public class LoginClient : Client
    {
        protected override char Prefix => 'L';

        /// <summary>
        /// アバター
        /// </summary>
        public List<Player> Avatars { get; set; } = null;

        public LoginClient(SocketClient socket) : base(socket) { }

        protected override void HandlePacket(uint type, ushort protectCode, byte[] data, uint position, uint size)
        {
            if (type != 0x1000)
            {
                if (PacketHandlers.HandlerNames.ContainsKey(type))
                {
#if DEBUG
                    //既知のパケットを表示
                    Logger.Write($"[{Prefix}][-->] Packet {PacketHandlers.HandlerNames[type]} ({size + 8} bytes)");
#endif
                }
                else
                {
                    //未知のパケットを表示
                    Logger.WriteWarning($"[{Prefix}][-->] UNIMPLEMENTED PACKET {type:X} -  ({size + 8} bytes)");
                    return;
                }
            }

            //bufferから該当パケットのデータ部を抽出
            var packet = new byte[size];
            if (data != null)
                Array.Copy(data, position, packet, 0, size);

            //typeにマッチするハンドラの取得
            var handler = PacketHandlers.GetHandlerFor(type);

            try
            {
                if (handler != null)
                {
                    //ログイン関係パケットは暗号化されている
                    byte[] decrypted = PacketCrypt.DecryptPacket(packet, size, type);
                    handler.HandlePacket(this, new PacketReader(decrypted), size);
                }
            }
            catch (Exception ex)
            {
                Logger.WriteException("[HandlePacketError]", ex);
                Disconnect();
            }
        }
    }

    /// <summary>
    /// ゲーム用
    /// </summary>
    public class GameClient : Client
    {
        /// <summary>
        /// ゲームサーバーログイン時の更新完了フラグ
        /// </summary>
        public bool IsUpdateReady { get; set; } = false;

        /// <summary>
        /// DBに追加するべきアイテム
        /// </summary>
        public List<Item> AddDBItems { get; set; } = new List<Item>();
        
        /// <summary>
        /// DBから削除するべきアイテム
        /// </summary>
        public List<Item> RemoveDBItems { get; set; } = new List<Item>();

        protected override char Prefix => 'G';

        public GameClient(SocketClient socket) : base(socket)
        {
            socket.ConnectionLost += Save;
        }

        /// <summary>
        /// データ保存
        /// </summary>
        public void Save()
        {
            using (var db = new gameContext())
            {
                try
                {
                    db.Players.Remove(User);
                    db.SaveChanges();
                    db.Players.Add(User);
                    db.Items.AddRange(AddDBItems);
                    db.Items.RemoveRange(RemoveDBItems);
                    db.SaveChanges();
                    AddDBItems.Clear();
                    RemoveDBItems.Clear();
                }
                catch (Exception ex)
                {
                    Logger.WriteException("[save]", ex);
                }
            }
        }

        protected override void HandlePacket(uint type, ushort protectCode, byte[] data, uint position, uint size)
        {
            if (type != 0x1000)
            {
                if (PacketHandlers.HandlerNames.ContainsKey(type))
                {
#if DEBUG
                    if (IsUpdateReady && type == 0x1024)
                    {
                        //アップデート完了してる場合はSTOP
                        Logger.Write($"[{Prefix}][-->] Packet {nameof(Packets.Handlers.Move.Stop)} -  ({size + 8} bytes)");
                    }
                    else
                    {
                        //既知のパケットを表示
                        Logger.Write($"[{Prefix}][-->] Packet {PacketHandlers.HandlerNames[type]} ({size + 8} bytes)");
                    }
#endif
                }
                else
                {
                    //未知のパケットを表示
                    Logger.WriteWarning($"[{Prefix}][-->] UNIMPLEMENTED PACKET {type:X} -  ({size + 8} bytes)");
                    return;
                }
            }

            //bufferから該当パケットのデータ部を抽出
            var packet = new byte[size];
            if (data != null)
                Array.Copy(data, position, packet, 0, size);

            //typeにマッチするハンドラの取得
            var handler = PacketHandlers.GetHandlerFor(type);

            try
            {
                if (handler != null)
                {
                    //ゲーム内パケットは暗号化されていない
                    if (IsUpdateReady && type == 0x1024)
                    {
                        //アップデート完了してる場合はSTOP
                        new Packets.Handlers.Move.Stop().HandlePacket(this, new PacketReader(packet), size);
                    }
                    else
                    {
                        handler.HandlePacket(this, new PacketReader(packet), size);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.WriteException("[HandlePacketError]", ex);
                Disconnect();
            }
        }
    }

    /// <summary>
    /// コミュニティ用
    /// </summary>
    public class CommunityClient : Client
    {
        protected override char Prefix => 'C';

        public CommunityClient(SocketClient socket) : base(socket) { }

        protected override void HandlePacket(uint type, ushort protectCode, byte[] data, uint position, uint size)
        {
            if (type != 0x1000)
            {
                if (PacketHandlers.HandlerNames.ContainsKey(type))
                {
#if DEBUG
                    //既知のパケットを表示
                    Logger.Write($"[{Prefix}][-->] Packet {PacketHandlers.HandlerNames[type]} ({size + 8} bytes)");
#endif
                }
                else
                {
                    //未知のパケットを表示
                    Logger.WriteWarning($"[{Prefix}][-->] UNIMPLEMENTED PACKET {type:X} -  ({size + 8} bytes)");
                    return;
                }
            }

            //bufferから該当パケットのデータ部を抽出
            var packet = new byte[size];
            if (data != null)
                Array.Copy(data, position, packet, 0, size);

            //typeにマッチするハンドラの取得
            var handler = PacketHandlers.GetHandlerFor(type);

            try
            {
                if (handler != null)
                {
                    handler.HandlePacket(this, new PacketReader(packet), size);
                }
            }
            catch (Exception ex)
            {
                Logger.WriteException("[HandlePacketError]", ex);
                Disconnect();
            }
        }
    }


    /// <summary>
    /// プレイヤのためのクラス
    /// </summary>
    public abstract class Client
    {
        /// <summary>
        /// クライアント終了
        /// </summary>
        public bool IsClosed { get; private set; }

        /// <summary>
        /// コンソール接頭辞
        /// </summary>
        protected abstract char Prefix { get; }

        /// <summary>
        /// 自分のソケットクライアント
        /// </summary>
        public SocketClient Socket { get; private set; }

        /// <summary>
        /// 現在のプレイヤ
        /// </summary>
        public Player User { get; set; } = null;

        /// <summary>
        /// サーバID
        /// </summary>
        public int ServerID { get; set; }

        /// <summary>
        /// ヒートビートが来た時間
        /// </summary>
        public DateTime BeatTime;

        /// <summary>
        /// 受信バッファ
        /// </summary>
        private readonly byte[] ReadBuffer;

        /// <summary>
        /// 受信バッファサイズ
        /// </summary>
        private uint ReadBufferSize;

        /// <summary>
        /// パケットのセキュリティコード
        /// </summary>
        public ushort PacketSecurityCode;

        /// <summary>
        /// GetActorsデリゲート
        /// </summary>
        /// <returns></returns>
        public delegate List<Actor> GetActors(ushort? IgnoreCharID = null);

        /// <summary>
        /// マップのActors取得
        /// </summary>
        public GetActors GetMapActors;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="socket"></param>
        public Client(SocketClient socket)
        {
            //各種ハンドラ
            socket.DataReceived += HandleDataReceived;
            socket.ConnectionLost += HandleConnectionLost;
            socket.SendDisConnection += SendClose;

            Socket = socket;

            ReadBuffer = new byte[1024 * 64];
            ReadBufferSize = 0;

            //ヒートビート時間設定
            BeatTime = DateTime.Now;
        }

        /// <summary>
        /// 切断
        /// </summary>
        public void Disconnect()
        {
            SendPacket(new DisconnectPacket());
            Socket.Disconnect();
        }

        /// <summary>
        /// パケット送信
        /// </summary>
        /// <param name="blob"></param>
        /// <param name="name"></param>
        public void SendPacket(byte[] blob, string name, bool flush = false)
        {
            var type = blob[2] | ((uint)blob[3] << 8);

#if DEBUG
            if (name == null)
                Logger.WriteGreen($"{Prefix}[<--] Packet {type:X} ({blob.Length} bytes)");
            else
                Logger.WriteGreen($"{Prefix}[<--] Packet {name} ({blob.Length} bytes)");
#endif

            try
            {
                Socket.Write(blob);
                if (flush)
                {
                    Socket.OnWritable();
                }
            }
            catch (Exception ex)
            {
                Logger.WriteException("Error sending packet", ex);
                if (ex.InnerException != null)
                    Logger.WriteException("[Inner]", ex.InnerException);
            }
        }

        /// <summary>
        /// パケット送信
        /// </summary>
        /// <param name="type"></param>
        /// <param name="data"></param>
        /// <param name="name"></param>
        public void SendPacket(UInt32 type, byte[] data, string name = null, bool flush = false)
        {
            var packet = new byte[6 + data.Length];

            // TODO: Use BinaryWriter here maybe?
            var dataLen = (uint)data.Length + 6;
            packet[0] = (byte)(dataLen & 0xFF);
            packet[1] = (byte)((dataLen >> 8) & 0xFF);
            packet[2] = (byte)(type & 0xFF);
            packet[3] = (byte)((type >> 8) & 0xFF);
            packet[4] = (byte)((type >> 16) & 0xFF);
            packet[5] = (byte)((type >> 24) & 0xFF);

            Array.Copy(data, 0, packet, 6, data.Length);

            SendPacket(packet, name, flush);
        }

        /// <summary>
        /// パケット送信
        /// </summary>
        /// <param name="packet"></param>
        public void SendPacket(Packet packet, bool flush = false)
        {
            string packetName = packet.GetType().Name;
            var h = packet.GetHeader();
            SendPacket(h.Type, packet.Build(), packetName, flush);
        }

        /// <summary>
        /// 受信
        /// </summary>
        /// <param name="data"></param>
        /// <param name="size"></param>
        private void HandleDataReceived(byte[] data, int size)
        {
            if ((ReadBufferSize + size) > ReadBuffer.Length)
            {
                //バッファオーバーフロー
                Logger.WriteError("[Buffer Over Flow] last pos is {0} , but max is {1} byte", ReadBufferSize + size, ReadBuffer.Length);
                return;
            }

            Array.Copy(data, 0, ReadBuffer, ReadBufferSize, size);

            ReadBufferSize += (uint)size;

            // Process ALL the packets
            uint position = 0;

            while ((position + 6) <= ReadBufferSize)
            {
                var packetSize =
                    ReadBuffer[position] |
                    ((uint)ReadBuffer[position + 1] << 8);

                // Minimum size, just to avoid possible infinite loops etc
                if (packetSize < 6)
                    packetSize = 6;

                // If we don't have enough data for this one...
                if (packetSize > 0x1000000 || (packetSize + position) > ReadBufferSize)
                    break;

                // Now handle this one
                UInt32 packetHeader = (
                    ReadBuffer[position + 2] |
                    ((uint)ReadBuffer[position + 3] << 8) |
                    ((uint)ReadBuffer[position + 4] << 16) |
                    ((uint)ReadBuffer[position + 5] << 24));
                UInt32 packetType = packetHeader & 0xFFFF;
                ushort protectCode = (ushort)(packetHeader >> 0x10);
                HandlePacket(
                    packetType, protectCode,
                    ReadBuffer, position + 6, packetSize - 6);

                // If the connection was closed, we have no more business here
                if (IsClosed)
                    break;

                position += packetSize;
            }

            // Wherever 'position' is up to, is what was successfully processed
            if (position > 0)
            {
                if (position >= ReadBufferSize)
                    ReadBufferSize = 0;
                else
                {
                    Array.Copy(ReadBuffer, position, ReadBuffer, 0, ReadBufferSize - position);
                    ReadBufferSize -= position;
                }
            }
        }

        /// <summary>
        /// 正常終了
        /// </summary>
        public void HandleConnectionLost()
        {
            // :(
            if (IsClosed) return;
            Logger.Write("[BYE] Connection lost. :(");
            IsClosed = true;
        }

        /// <summary>
        /// エラーで終了
        /// </summary>
        /// <param name="ex"></param>
        public void SendClose(Exception ex = null)
        {
            /*if (ex != null)
            {
                SendPacket(new ChatPacket(ChatPacket.ChatType.monsterShoutChat, 0xFFFF, "error", ex.Message));
                foreach (string str in ex.ToString().Split('\n').Skip(1))
                    SendPacket(new EventMessagePacket(str, EventMessagePacket.EventMessageType.WhiteChat1, 1));
            }
            SendPacket(new DisconnectPacket());*/
        }

        /// <summary>
        /// パケットのハンドラ
        /// </summary>
        /// <param name="type"></param>
        /// <param name="protectCode"></param>
        /// <param name="data"></param>
        /// <param name="position"></param>
        /// <param name="size"></param>
        protected abstract void HandlePacket(uint type, ushort protectCode, byte[] data, uint position, uint size);
        
        /// <summary>
        /// サーバータイプ接頭語
        /// </summary>
        private static Dictionary<ServerType, string> ServerTypePrefix
            = new Dictionary<ServerType, string>
            {
                {ServerType.Community , "[C]"},
                {ServerType.Game , "[G]"},
                {ServerType.Login , "[L]"},
            };
        private static string SetPrefix(ServerType st) => RedStoneApp.isDebug ? ServerTypePrefix[st] : "";
    }
}
