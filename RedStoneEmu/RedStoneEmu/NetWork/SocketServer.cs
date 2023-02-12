using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace RedStoneEmu.NetWork
{
    /// <summary>
    /// 全ソケットを管理するクラス
    /// ・ソケット受け入れ
    /// ・OnReadable実行
    /// ・OnWriteable実行
    /// </summary>
    public class SocketServer
    {
        /// <summary>
        /// listenするポート
        /// </summary>
        private int Port;

        /// <summary>
        /// リスナー
        /// </summary>
        private readonly TcpListener Listener;

        /// <summary>
        /// ソケットのリスト
        /// </summary>
        private readonly List<SocketClient> Clients = new List<SocketClient>();

        /// <summary>
        /// ソケットとソケットクライアントの辞書
        /// </summary>

        private readonly Dictionary<Socket, SocketClient> SocketMap = new Dictionary<Socket, SocketClient>();

        /// <summary>
        /// 受信（読み込み）可能ソケットリスト（チェック用）
        /// </summary>
        private readonly List<Socket> ReadableSockets = new List<Socket>();

        /// <summary>
        /// 送信（書き込み）可能ソケットリスト（チェック用）
        /// </summary>
        private readonly List<Socket> WritableSockets = new List<Socket>();

        /// <summary>
        /// クライアント新規受入時のサーバー側デリゲート
        /// </summary>
        public event NewClientDelegate NewClient;

        public delegate void NewClientDelegate(SocketClient client);

        /// <summary>
        /// ソケットワーバーコンストラクタ
        /// </summary>
        /// <param name="port"></param>
        public SocketServer(int port)
        {
            this.Port = port;

            //TCPリスナの開始
            Listener = new TcpListener(IPAddress.Any, port);
            Listener.Start();
        }

        /// <summary>
        /// ソケットサーバー処理
        /// ・新ソケット受け入れ
        /// ・ソケット送受信処理
        /// </summary>
        public void Run()
        {
            try
            {
                //チェック用ソケットクリア
                ReadableSockets.Clear();
                WritableSockets.Clear();

                //新規接続を読み込み用に加える
                ReadableSockets.Add(Listener.Server);

                //新規接続以外を加える
                ReadableSockets.AddRange(Clients.Select(t=>t.Socket.Client));
                WritableSockets.AddRange(Clients.Where(t => t.NeedsToWrite).Select(t => t.Socket.Client));

                //読み込み可能・書き込み可能なソケットを選別
                Socket.Select(ReadableSockets, WritableSockets, null, 1000000);

                //受信処理
                foreach (var socket in ReadableSockets)
                {
                    if(socket == Listener.Server)
                    {
                        //新規接続受け入れ
                        var newTcpClient = Listener.AcceptTcpClient();
                        Logger.WriteInternal("[New] {0}", newTcpClient.Client.RemoteEndPoint);

                        //ソケットクライアント作成・登録
                        var c = new SocketClient(this, newTcpClient);
                        Clients.Add(c);
                        SocketMap.Add(c.Socket.Client, c);

                        //クライアントサーバー側で登録
                        NewClient(c);
                    }
                    else
                    {
                        //パケット受信
                        if (socket.Connected)
                            SocketMap[socket].OnReadable();
                    }
                }

                //送信処理
                foreach (var socket in WritableSockets)
                {
                    if (socket.Connected)
                        SocketMap[socket].OnWritable();
                }
            }
            catch (Exception ex)
            {
                Logger.WriteException("A socket error occurred", ex);
            }
        }

        /// <summary>
        /// 辞書とソケットクライアントリストから消す
        /// </summary>
        /// <param name="client"></param>
        internal void NotifyConnectionClosed(SocketClient client)
        {
            Logger.Write("Connection closed");

            SocketMap.Remove(client.Socket.Client);
            Clients.Remove(client);
        }
    }
}
