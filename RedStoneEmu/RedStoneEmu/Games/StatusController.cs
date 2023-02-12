using RedStoneLib;
using RedStoneLib.Model;
using RedStoneLib.Packets;
using RedStoneLib.Packets.RSPacket.Action;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static RedStoneLib.Model.Player;

namespace RedStoneEmu.Games
{
    /// <summary>
    /// ステータス処理全般を扱う
    /// </summary>
    static class StatusController
    {
        /// <summary>
        /// 次のレベルアップに必要なポイント
        /// </summary>
        public static List<ulong> NeedEXPtable { get; private set; } = new List<ulong>();

        /// <summary>
        /// レベルアップ合計値
        /// </summary>
        public static List<ulong> SummationEXPTable { get; private set; } = new List<ulong>();

        /// <summary>
        /// スキルアップに必要なポイント
        /// </summary>
        public static List<double> NeedSPTable { get; private set; } = new List<double>();

        /// <summary>
        /// 経験値もらった時の手続き
        /// </summary>
        /// <param name="player"></param>
        /// <param name="monster"></param>
        /// <returns></returns>
        public static void EXPProcedure(ref Player player, Monster monster, SendPacketDelegate sendPacket, Action<Packet> sendPacketNearPlayer)
        {

            //EXP取得前のSPゲージの場所
            var beforeEXP = player.EXP;
            int spIndex = Enumerable.Range(0, NeedSPTable.Count).First(i => NeedSPTable[i] > beforeEXP);

            //EXP計算
            var exp = GainEXP(monster, player, RedStoneApp.GameServer.Config.EXPrate);

            //EXP受け取り
            GaveEXP(exp, ref player, sendPacket, sendPacketNearPlayer);
        }

        /// <summary>
        /// EXPを受け取る
        /// </summary>
        /// <param name="exp"></param>
        /// <param name="player"></param>
        /// <param name="sendPacket"></param>
        /// <param name="sendPacketNearPlayer"></param>
        public static void GaveEXP(uint exp, ref Player player, SendPacketDelegate sendPacket, Action<Packet> sendPacketNearPlayer)
        {
            //EXP取得
            player.EXP += exp;

            //合計EXP
            var sumEXP = (player.Level == 1 ? 0 : SummationEXPTable[player.Level - 2]) + player.EXP;

            //スキルポイントチェック
            var sumSP = PlayerSkill.SummationSP(player);
            while (NeedSPTable[(int)(sumSP + player.SkillPoint)] <= sumEXP)
                player.SkillPoint++;
            sendPacket(new UpdateEXP(player.Level, player.EXP, player.SkillPoint), true);

            //レベルチェック
            int increaseLevel = 0;
            for (; NeedEXPtable[player.Level + increaseLevel - 1] < player.EXP; increaseLevel++) ;
            if (increaseLevel > 0)
            {
                //EXP引く
                player.EXP -= (uint)NeedEXPtable[player.Level - 1];
                //レベル上昇
                player.Level += (ushort)increaseLevel;
                //ステータス上昇
                for (int i = 0; i < increaseLevel; i++)
                {
                    UpdateStatusByLevelUp(ref player);
                }
            }

            //SPorLevelUP通知
            if (increaseLevel > 0)
            {
                sendPacket(new LevelUP(player.Level, player.EXP, player.SkillPoint, player.BaseStatus, (ushort)player.StatusPoint), true);

                //周りに通知
                sendPacketNearPlayer(new LevelUPOther(player.CharID, player.Level));
            }
        }

        /// <summary>
        /// レベルアップによるステータスアップデート
        /// </summary>
        /// <param name="status"></param>
        static void UpdateStatusByLevelUp(ref Player player) 
        {
            player.StatusPoint += 4;
            ActorStatus status = player.BaseStatus;
            switch (player.Job)
            {
                case JOB.Swordsman:
                case JOB.Warrior:
                    status.Strength += 1;
                    break;
                case JOB.Wizard:
                case JOB.Wolfman:
                    status.Inteligence += 1;
                    break;
                case JOB.Bishop:
                case JOB.Angel:
                    status.Charisma += 1;
                    break;
                case JOB.Thief:
                case JOB.Monk:
                    status.Luckey += 1;
                    break;
                case JOB.Lancer:
                case JOB.Archer:
                    status.Agility += 1;
                    break;
                case JOB.Tamer:
                case JOB.Summoner:
                    status.Wisdom += 1;
                    break;
                case JOB.Princess:
                case JOB.LittleWitch:
                    status.Condition += 1;
                    break;
                default:
                    player.StatusPoint += 1;
                    break;
            }
            player.BaseStatus = status;
        }


        /// <summary>
        /// 取得EXP
        /// </summary>
        /// <param name="player"></param>
        /// <param name="expRate"></param>
        /// <returns></returns>
        public static uint GainEXP(Monster monster, Player player, double expRate)
        {
            double levelSum = Math.Abs(monster.Level - player.Level);
            double levelTimesMonsterEXP = levelSum < 75 ? (1.0 - levelSum / 75.0) * monster.ABase.DefaultEXP : 1.0;//レベル差補正
            double resultEXP = levelTimesMonsterEXP * (1.0 + (Helper.StatusToBonus(player.Status.Wisdom)) / 100.0);

            return (uint)(Math.Max(resultEXP, 1.0) * expRate);
        }

        /// <summary>
        /// テーブル計算
        /// </summary>
        /// <returns></returns>
        public static void CalcEXPTables(int levelCap)
        {
            object locker = new object();

            //次のレベルアップに必要なポイント計算
            Parallel.For(1, levelCap, level =>
            {
                ulong X = (ulong)((Math.Sqrt(4 * level - 3) + 1) / 2);
                ulong exp = (ulong)((level + 4) * (-2 * Math.Pow(X, 3.0) + (6 * (ulong)level + 2) * X + 9) * 10.0 / 3.0);
                lock (locker)
                {
                    NeedEXPtable.Add(exp);
                }
            });
            NeedEXPtable.Sort();

            //合計値計算
            ulong expSum = 0;
            foreach (var exp in NeedEXPtable)
            {
                expSum += exp;
                SummationEXPTable.Add(expSum);
            }

            //スキルポイント必要値計算
            foreach (var exp in NeedEXPtable.Select((val, level) => new { val, level }))
            {
                for (int i = 0; i < Math.Min(100, exp.level + 2); i++)
                {
                    NeedSPTable.Add((double)exp.val / Math.Min(100, exp.level + 2) + NeedSPTable.LastOrDefault());
                }
            }
        }
    }
}
