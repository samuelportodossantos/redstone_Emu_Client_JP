using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace RedStoneEmu.NetWork
{
    /// <summary>
    /// ソケット単体が送信や受信をするクラス
    /// </summary>
    public class SocketClient
    {
        /// <summary>
        /// ソケット実態
        /// </summary>
        public TcpClient Socket { get; private set; }

        /// <summary>
        /// サーバー実態
        /// </summary>
        public readonly SocketServer Server;

        /// <summary>
        /// 通信用バッファ
        /// </summary>
        private readonly byte[] ReadBuffer, WriteBuffer;

        /// <summary>
        /// 書き込み位置
        /// </summary>
        private int WritePosition = 0;

        public delegate void ConnectionLostDelegate();
        public delegate void DisConnectionDelegate(Exception ex);
        public delegate void DataReceivedDelegate(byte[] data, int size);

        /// <summary>
        /// データ受信用デリゲート
        /// </summary>
        public event DataReceivedDelegate DataReceived;

        /// <summary>
        /// 切断時用デリゲート
        /// </summary>
        public event ConnectionLostDelegate ConnectionLost;

        /// <summary>
        /// エラー切断用デリゲート
        /// </summary>
        public event DisConnectionDelegate SendDisConnection;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="server"></param>
        /// <param name="socket"></param>
        public SocketClient(SocketServer server, TcpClient socket)
        {
            Server = server;
            Socket = socket;
            
            ReadBuffer = new byte[1024 * 32];
            WriteBuffer = new byte[1024 * 1024]; 
        }

        /// <summary>
        /// 送信の必要性
        /// </summary>
        public bool NeedsToWrite { get { return (WritePosition > 0); } }

        /// <summary>
        /// クライアント削除
        /// </summary>
        private void RemoveClient()
        {
            ConnectionLost();
            Server.NotifyConnectionClosed(this);
        }

        /// <summary>
        /// 強制切断
        /// </summary>
        public void Disconnect()
        {
            RemoveClient();
            Socket.Dispose();
        }

        /// <summary>
        /// 受信
        /// </summary>
        /// <returns></returns>
        public bool OnReadable()
        {
            try
            {
                //受信
                var read = Socket.Client.Receive(ReadBuffer);
                if (read == 0)
                {
                    //受信失敗
                    RemoveClient();
                    return false;
                }

                DataReceived(ReadBuffer, read);

                return true;
            }
            catch (SocketException)
            {
                RemoveClient();
                return false;
            }
            catch (Exception ex)
            {
                //切断
                SendDisConnection(ex);
                Disconnect();
                RemoveClient();
                return false;
            }
        }

        /// <summary>
        /// 送信
        /// </summary>
        /// <returns></returns>
        public bool OnWritable()
        {
            try
            {
                var write = Socket.Client.Send(WriteBuffer, 0, WritePosition, SocketFlags.None);
                if (write == 0)
                {
                    // Connection failed, presumably
                    ConnectionLost();
                    Server.NotifyConnectionClosed(this);
                    return false;
                }

                Array.Copy(WriteBuffer, write, WriteBuffer, 0, WritePosition - write);
                WritePosition -= write;

                return true;
            }
            catch (SocketException)
            {
                RemoveClient();
                return false;
            }
        }

        public void Write(byte[] blob)
        {
            if ((WritePosition + blob.Length) > WriteBuffer.Length)
            {
                // Buffer exceeded!
                throw new Exception("too much data in write queue");
            }

            Array.Copy(blob, 0, WriteBuffer, WritePosition, blob.Length);
            WritePosition += blob.Length;
        }
    }
}
