using RedStoneLib.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RedStoneEmu.Games
{
    class Trade
    {
        /// <summary>
        /// 全てのトレード
        /// </summary>
        public static Dictionary<int, Trade> AllTrades { get; private set; }
            = new Dictionary<int, Trade>();

        /// <summary>
        /// 登録
        /// </summary>
        /// <param name="player1">取引申請側</param>
        /// <param name="player2">取引承認側</param>
        /// <param name="sendPacket1"></param>
        /// <param name="sendPacket2"></param>
        public static bool Register(Player player1, Player player2, SendPacketDelegate sendPacket1, SendPacketDelegate sendPacket2)
        {
            if (AllTrades.Count > 500) return false;

            //取引作成
            int tradeID = Enumerable.Range(0, Int32.MaxValue).First(t => !AllTrades.ContainsKey(t));
            Trade trade = new Trade(tradeID, player1, player2, sendPacket1, sendPacket2);

            //登録
            AllTrades[tradeID] = trade;
            player1.TradeID = tradeID;
            player2.TradeID = tradeID;

            return true;
        }

        private Trade(int tradeID, Player player1, Player player2, SendPacketDelegate sendPacket1, SendPacketDelegate sendPacket2)
        {
            TradeID = tradeID;
            Player1 = player1;
            Player2 = player2;
            SendPacket1 = sendPacket1;
            SendPacket2 = sendPacket2;
        }

        /// <summary>
        /// パートナー取得
        /// </summary>
        /// <param name="myName"></param>
        /// <returns></returns>
        public (Player, SendPacketDelegate) GetPartner(string myName)
        {
            if (Player1.Name == myName)
            {
                return (Player2, SendPacket2);
            }
            else
            {
                return (Player1, SendPacket1);
            }
        }

        /// <summary>
        /// 自分と相手のSendPacket取得
        /// </summary>
        /// <param name="myName"></param>
        /// <returns></returns>
        public (SendPacketDelegate me, SendPacketDelegate mate) GetSendPackets(string myName)
        {
            if (Player1.Name == myName)
            {
                return (SendPacket1, SendPacket2);
            }
            else
            {
                return (SendPacket2, SendPacket1);
            }
        }

        /// <summary>
        /// 自分のPlayers取得
        /// </summary>
        /// <param name="myName"></param>
        /// <returns></returns>
        public Player GetMe(string myName)
        {
            if (Player1.Name == myName)
            {
                return Player1;
            }
            else
            {
                return Player2;
            }
        }

        /// <summary>
        /// 相手のPlayers取得
        /// </summary>
        /// <param name="myName"></param>
        /// <returns></returns>
        public Player GetMate(string myName)
        {
            if (Player1.Name == myName)
            {
                return Player2;
            }
            else
            {
                return Player1;
            }
        }

        /// <summary>
        /// アイテムリスト取得
        /// </summary>
        /// <param name="myName"></param>
        /// <returns></returns>
        public List<(int inveSlot, int count, Item item)> MyItems(string myName)
        {
            if (Player1.Name == myName)
            {
                return Player1Items;
            }
            else
            {
                return Player2Items;
            }
        }

        /// <summary>
        /// ゴールドをセット
        /// </summary>
        /// <param name="myName"></param>
        /// <returns></returns>
        public void SetGold(string myName, uint gold)
        {
            if (Player1.Name == myName)
            {
                Gold1 = gold;
            }
            else
            {
                Gold2 = gold;
            }
        }

        /// <summary>
        /// 渡すゴールド取得
        /// </summary>
        /// <param name="myName"></param>
        /// <returns></returns>
        public uint GetGiveGold(string myName)
        {
            if (Player1.Name == myName)
            {
                return Gold1;
            }
            else
            {
                return Gold2;
            }
        }
        
        /// <summary>
        /// 取引準備OK
        /// </summary>
        /// <param name="myName"></param>
        /// <returns></returns>
        public void SetReady(string myName)
        {
            if (Player1.Name == myName)
            {
                Ready1 = true;
            }
            else
            {
                Ready2 = true;
            }
        }

        /// <summary>
        /// 取引準備リセット
        /// </summary>
        public void ResetReady()
        {
            Ready1 = false;
            Ready2 = false;
            Permit = false;
        }

        /// <summary>
        /// トレードID
        /// </summary>
        public int TradeID { get; private set; }

        /// <summary>
        /// プレイヤ1（申請側）
        /// </summary>
        public Player Player1 { get; set; }

        /// <summary>
        /// プレイヤ2
        /// </summary>
        public Player Player2 { get; set; }

        /// <summary>
        /// プレイヤ1SendPacket
        /// </summary>
        public SendPacketDelegate SendPacket1 { get; set; }

        /// <summary>
        /// プレイヤ2SendPacket
        /// </summary>
        public SendPacketDelegate SendPacket2 { get; set; }

        /// <summary>
        /// プレイヤ1のアイテム
        /// </summary>
        List<(int inveSlot, int count, Item item)> Player1Items { get; set; }
            = new List<(int inveSlot, int count, Item item)>();

        /// <summary>
        /// プレイヤ2のアイテム
        /// </summary>
        List<(int inveSlot, int count, Item item)> Player2Items { get; set; }
            = new List<(int inveSlot, int count, Item item)>();

        /// <summary>
        /// プレイヤ1のゴールド
        /// </summary>
        uint Gold1;

        /// <summary>
        /// プレイヤ2のゴールド
        /// </summary>
        uint Gold2;

        /// <summary>
        /// プレイヤ1取引準備OK
        /// </summary>
        bool Ready1 = false;

        /// <summary>
        /// プレイヤ2取引準備OK
        /// </summary>
        bool Ready2 = false;

        /// <summary>
        /// 取引承認
        /// </summary>
        public bool Permit { get; set; } = false;
    }
}
