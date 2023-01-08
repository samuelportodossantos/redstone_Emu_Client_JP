using RedStoneLib;
using RedStoneLib.Algorithm;
using RedStoneLib.Model;
using RedStoneLib.Model.Base;
using RedStoneLib.Packets.RSPacket.Move;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RedStoneEmu.Games
{
    static class EnemyAI
    {
        /// <summary>
        /// マップのGRID
        /// </summary>
        static Dictionary<ushort, JumpPointSearch> AllGrids = new Dictionary<ushort, JumpPointSearch>();

        /// <summary>
        /// Path見つける
        /// </summary>
        /// <param name="map"></param>
        /// <param name="startX"></param>
        /// <param name="startY"></param>
        /// <param name="goalX"></param>
        /// <param name="goalY"></param>
        /// <param name="skillRange"></param>
        /// <returns>精密な座標</returns>
        static Point<int>[] FindPath(Map map, bool[,] movingGrid, int startX, int startY, int goalX, int goalY, float skillRange)
        {
            //skill range point
            Point<int> skillRangePoint(int x, int y)
            {
                var rad = Math.Atan2(y - goalY, x - goalX);
                int x_sum = (int)Math.Round(skillRange * Math.Cos(rad) * 64);
                int y_sum = (int)Math.Round(skillRange * Math.Sin(rad) * 32);
                return new Point<int> { X = x_sum + goalX, Y = y_sum + goalY };
            }

            //GRID存在しない
            if (!AllGrids.ContainsKey(map.SerialID))
            {
                //構築
                AllGrids[map.SerialID] = new JumpPointSearch(movingGrid);
            }

            //障害物チェック
            if (!map.IsBlockedWay(startX, startY, goalX, goalY))
            {
                //障害物なし
                if(Helper.GetDistance(startX/64, startY/32, goalX/64, goalY/32) <= skillRange)
                {
                    //射程圏内
                    return new Point<int>[0];
                }
                else
                {
                    //射程内まで移動
                    return new Point<int>[] { skillRangePoint(startX, startY) };
                }
            }

            //path取得
            if(AllGrids[map.SerialID].FindPath(out var path_row, 
                (int)Math.Floor(startX/64.0), (int)Math.Floor(startY / 32.0), (int)Math.Floor(goalX / 64.0), (int)Math.Floor(goalY / 32.0)))
            {
                if (path_row.Length <= 0)
                {
                    //射程内まで移動
                    return new Point<int>[] { skillRangePoint(startX, startY) };
                }

                //長過ぎたら中止
                var tmpGrid = path_row.First();
                var totalDist = path_row.Select(pos =>
                {
                    var dist = Helper.GetDistance(pos.X, pos.Y, tmpGrid.X, tmpGrid.Y);
                    tmpGrid = pos;
                    return dist;
                }).Sum();
                if (totalDist > 100)
                    return null;

                //スタート・ゴールは飛ばす
                /*List<Point<int>> path = path_row.Take(path_row.Length > 1 ? path_row.Length - 1 : path_row.Length)
                    .Select(t => new Point<int> { X = t.X * 64 + 32, Y = t.Y * 32 + 16 }).ToList();*/
                List<Point<int>> path = path_row.Select(t => new Point<int> { X = t.X * 64 + 32, Y = t.Y * 32 + 16 }).ToList();

                //射程内に移動
                path.Add(skillRangePoint(path.Last().X, path.Last().Y));

                return path.ToArray();
            }
            return null;
        }

        /// <summary>
        /// 追跡
        /// </summary>
        /// <param name="map"></param>
        /// <param name="monster"></param>
        /// <param name="player"></param>
        /// <param name="skillRange"></param>
        /// <returns></returns>
        static bool Chase(MAPServer map, Monster monster, Player player, float skillRange, int separateWaitTime = 10)
        {
            refind:
            //経路
            var pathes = FindPath(map.MAP, map.MovingMap, monster.PosX, monster.PosY, player.PosX, player.PosY, skillRange);
            if (pathes == null)
            {
                /*try
                {
                    AllGrids[map.MAP.SerialID].OutputImage(map.MAP.Name.Replace("/", "") + ".png",
                        new Point<int>[] { new Point<int> { X = monster.PosX / 64, Y = monster.PosY / 32 }, new Point<int> { X = player.PosX / 64, Y = player.PosY / 32 } });
                }
                catch { }*/
                return false;
            }

            //描画
            /*var drawingPath = ((Point<int>[])pathes.Clone()).ToList();
            drawingPath.Insert(0, new Point<int> { X = monster.PosX, Y = monster.PosY });
            try
            {
                AllGrids[map.MAP.SerialID].OutputImage(map.MAP.Name.Replace("/", "") + ".png",
                    drawingPath.Select(t => new Point<int> { X = t.X / 64, Y = t.Y / 32 }).ToArray());
            }
            catch { }*/

            //経路区切る
            var startPt = new Point<int> { X = monster.PosX, Y = monster.PosY };
            const int splitNum = 256;
            List<Point<int>> splitedPath = new List<Point<int>>();//区切られた後のPath
            foreach(var goalPt in pathes.ToArray())
            {
                if(map.MAP.IsBlockedWay(startPt.X, startPt.Y, goalPt.X, goalPt.Y))
                {
                    //throw new Exception("blocked");
                }
                double dist = Helper.GetDistance(startPt, goalPt);
                if (dist > splitNum)
                {
                    double rad = Math.Atan2(goalPt.Y - startPt.Y, goalPt.X - startPt.X);
                    for(int i = 0; i <Math.Floor(dist / splitNum); i++)
                    {
                        splitedPath.Add(new Point<int>
                        {
                            X = (int)Math.Round(startPt.X + splitNum * (i + 1) * Math.Cos(rad)),
                            Y = (int)Math.Round(startPt.Y + splitNum * (i + 1) * Math.Sin(rad))
                        });
                    }
                }
                splitedPath.Add(goalPt);

                startPt = goalPt;
            }
            pathes = splitedPath.ToArray();

            //移動
            Point<ushort> playerPos = (Point<ushort>)player.Pos.Clone();
            foreach (var to in pathes)
            {
                map.SendPacketsToPlayersOnTheMap(new MovePacket(monster.CharID, monster.PosX, monster.PosY, (ushort)to.X, (ushort)to.Y, monster.MoveSpeed));
                //待機
                double dist = Helper.GetDistance(monster.PosX / 64, monster.PosY / 32, to.X / 64, to.Y / 32);
                double waittime = DistanceToWaitSec(dist, monster.MoveSpeed);

                int numWait = (int)Math.Round(waittime / separateWaitTime);//wait回数

                //単位移動量
                double unitX = (double)(player.PosX - monster.PosX) / numWait;
                double unitY = (double)(player.PosY - monster.PosY) / numWait;

                //精密な座標
                double preciseX = monster.PosX;
                double preciseY = monster.PosY;
                for (int i = 0; i < numWait; i++)
                {
                    //プレイヤー移動チェック
                    if (playerPos.X / 64 != player.PosX / 64 || playerPos.Y / 32 != player.PosY / 32)
                    {
                        //プレイヤー移動
                        playerPos = (Point<ushort>)player.Pos.Clone();
                        map.SendPacketsToPlayersOnTheMap(new StopPacket(monster));
                        goto refind;
                    }

                    //終了チェック
                    if (player.NowHP == 0 || monster.NowHP == 0)
                    {
                        map.SendPacketsToPlayersOnTheMap(new StopPacket(monster));
                        goto end;
                    }

                    Thread.Sleep(separateWaitTime);
                    preciseX += unitX;
                    preciseY += unitY;

                    if (map.MAP.GetBlock((int)Math.Round(preciseX) / 64, (int)Math.Round(preciseY) / 32) == 0)
                    {
                        monster.PosX = (ushort)Math.Round(preciseX);
                        monster.PosY = (ushort)Math.Round(preciseY);
                    }
                }
                //移動完了
                monster.PosX = (ushort)to.X;
                monster.PosY = (ushort)to.Y;
            }
            end:
            return true;
        }

        /// <summary>
        /// 反応する
        /// </summary>
        /// <param name="monster"></param>
        /// <param name="player"></param>
        public static void ReactsTo(this Monster monster, Player player)
        {
            if (monster.IsButtleNow) return;
            monster.IsButtleNow = true;

            MAPServer map = MAPServer.AllMapServers[monster.MapSerial];

            //スキルレベル
            int slv = (int)Math.Round(Math.Sqrt(monster.Status.Inteligence / 3.0));

            //最も強い攻撃スキル選択
            var targetSkill = monster.Skills.Values.Select(skill =>
            {
                bool hasPhysic = Action.TryGetPhysicDamageToActor(skill, out var pyDmg, out var _, slv, monster, player, true, true);
                bool hasMagic = Action.TryGetMagicDamage(skill, out var mgDmg, slv, monster, player, pyDmg != 0, true);
                if (hasPhysic || hasMagic)
                {
                    return new { dmg = pyDmg + mgDmg.SumDouble, skill = skill };
                }
                else
                {
                    return new { dmg = (double)0, skill = skill };
                }
            }).OrderByDescending(t => t.dmg).First().skill;

            //スキル範囲
            var skillRange = targetSkill.MaxEnableRange[slv];

            Task.Run(() =>
            {
                while (player.NowHP > 0 && monster.NowHP > 0 && monster.IsButtleNow)
                {
                    //射程内or間にブロックないなら攻撃
                    while (player.NowHP > 0 && monster.NowHP > 0 &&
                    monster.GetDistance(player, true) <= skillRange && !map.MAP.IsBlockedWay(monster.PosX, monster.PosY, player.PosX, player.PosY) &&
                    monster.IsButtleNow)
                    {
                        Action.AttackToActor(targetSkill, null, out double totalDamage, slv, monster, player);
                    }

                    //移動
                    if (!Chase(map, monster, player, skillRange)) return;
                }
            }).ContinueWith(t =>
            {
                if (t.IsFaulted)
                    throw t.Exception;
                monster.IsButtleNow = false;
            });
        }

        /// <summary>
        /// 移動時間
        /// </summary>
        /// <param name="distance"></param>
        /// <param name="moveSpeed"></param>
        /// <returns></returns>
        public static double DistanceToWaitSec(double distance, int moveSpeed)
        {
            return distance * 250000.0 / moveSpeed;
        }
    }
}
