using RedStoneLib.Model;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace RedStoneLib.Algorithm
{
    public class JumpPointSearch
    {
        /// <summary>
        /// カベ：false 移動可能:true
        /// </summary>
        public bool[,] Grid;

        /// <summary>
        /// grid生成
        /// </summary>
        /// <param name="map"></param>
        public JumpPointSearch(Map map)
        {
            //grid カベ：false 移動可能:true
            Grid = new bool[map.Size.Width, map.Size.Height];
            for (int x = 0; x < map.Size.Width; x++)
            {
                for (int y = 0; y < map.Size.Height; y++)
                {
                    Grid[x, y] = map.GetBlock(x, y) == 0;
                }
            }
        }

        public JumpPointSearch(bool[,] grid)
            => Grid = grid;

        /// <summary>
        /// MAPからサーチャー
        /// </summary>
        /// <param name="map"></param>
        /// <param name="path"></param>
        /// <param name="startX"></param>
        /// <param name="startY"></param>
        /// <param name="goalX"></param>
        /// <param name="goalY"></param>
        /// <param name="step"></param>
        /// <param name="skip"></param>
        /// <returns></returns>
        public bool FindPath(out Point<int>[] path,
            int startX, int startY, int goalX, int goalY, int step = 0, int skip = 0)
        {
            bool startTmp = Grid[startX, startY];
            bool goalTmp = Grid[goalX, goalY];
            Grid[startX, startY] = true;
            Grid[goalX, goalY] = true;
            Searcher search = new Searcher(Grid);
            search.SetSkip(skip);
            bool found = search.FindPath(out path, startX, startY, goalX, goalY, step);

            Grid[startX, startY] = startTmp;
            Grid[goalX, goalY] = goalTmp;
            return found;
        }
        
        /// <summary>
        /// 画像出力
        /// </summary>
        /// <param name="fname"></param>
        /// <param name="path"></param>
        public void OutputImage(string fname, Point<int>[] path = null)
        {
            Bitmap img = new Bitmap(Grid.GetLength(0), Grid.GetLength(1));
            for (int x = 0; x < Grid.GetLength(0); x++)
            {
                for (int y = 0; y < Grid.GetLength(1); y++)
                {
                    if (Grid[x, y]) img.SetPixel(x, y, Color.White);
                    else img.SetPixel(x, y, Color.Black);
                }
            }
            if (path != null && path.Count() > 0)
            {
                if (path.Count() <= 1)
                {
                    img.SetPixel(path[0].X, path[0].Y, Color.Red);
                }
                else
                {
                    using (Graphics g = Graphics.FromImage(img))
                    {
                        g.DrawLines(Pens.Red, path.Select(t => new System.Drawing.Point(t.X, t.Y)).ToArray());
                    }
                }
            }
            img.Save(fname);
        }

        private class Node
        {
            //position
            public int X;
            public int Y;
            public float G;
            public float F;

            public Node Parent = null;

            public bool IsOpen { get; private set; }

            public bool IsClosed { get; private set; }

            public Node(int x, int y)
            {
                X = x;
                Y = y;
                F = 0;
                G = 0;
                IsOpen = false;
                IsClosed = false;
            }

            public void SetOpen()
                => IsOpen = true;

            public void SetClosed()
                => IsClosed = true;

            public override string ToString()
                => $"({X}, {Y})F:{F}, G:{G}";
        }

        private class PriorityQueue
        {
            private Node[] _heap;
            private int _sz = 0;

            private int _count = 0;

            /// <summary>
            /// Priority Queue
            /// </summary>
            public PriorityQueue(int maxSize = 500)
            {
                _heap = new Node[maxSize];
            }

            private float Compare(Node x, Node y)
            {
                return x.F - y.F;
            }

            public void Push(Node x)
            {
                _count++;

                //node number
                var i = _sz++;

                while (i > 0)
                {
                    //parent node number
                    var p = (i - 1) / 2;

                    if (Compare(_heap[p], x) <= 0) break;

                    _heap[i] = _heap[p];
                    i = p;
                }

                _heap[i] = x;
            }

            public Node Pop()
            {
                _count--;

                Node ret = _heap[0];
                Node x = _heap[--_sz];

                int i = 0;
                while (i * 2 + 1 < _sz)
                {
                    //children
                    int a = i * 2 + 1;
                    int b = i * 2 + 2;

                    if (b < _sz && Compare(_heap[b], _heap[a]) < 0) a = b;

                    if (Compare(_heap[a], x) >= 0) break;

                    _heap[i] = _heap[a];
                    i = a;
                }

                _heap[i] = x;

                return ret;
            }

            public void Fixup()
            {
                PriorityQueue tmp = new PriorityQueue(_heap.Length);
                foreach (Node node in _heap.Where(t => t != null))
                {
                    tmp.Push(node);
                }
                _heap = tmp._heap;
                _sz = tmp._sz;
            }

            /// <summary>
            /// 取得試行
            /// </summary>
            /// <param name="x"></param>
            /// <param name="y"></param>
            /// <param name="result"></param>
            /// <returns></returns>
            public bool TryPeek(int x, int y, out Node result)
            {
                result = _heap.Where(t => t != null).FirstOrDefault(t => t.X == x && t.Y == y);
                return result != null;
            }

            public int Count()
            {
                return _count;
            }

            public Node Peek()
            {
                return _heap[0];
            }

            public bool Contains(Node x)
            {
                for (int i = 0; i < _sz; i++) if (x.Equals(_heap[i])) return true;
                return false;
            }

            public void Clear()
            {
                while (this.Count() > 0) this.Pop();
            }
        }

        private class Searcher
        {
            /// <summary>
            /// path operation
            /// </summary>
            enum Result
            {
                NO_PATH = 0,
                FOUND_PATH = 1,
                NEED_MORE_STEPS = 2
            }

            PriorityQueue Open = new PriorityQueue();
            Node EndNode;

            Point<int> NPos = new Point<int> { X = -1, Y = -1 };

            //ステップ残り
            int StepsRemain;
            int StepsDone;

            int Skip;

            /// <summary>
            /// カベ：false 移動可能:true
            /// </summary>
            bool[,] Grid;

            /// <summary>
            /// コンストラクタ
            /// </summary>
            /// <param name="grid"></param>
            public Searcher(bool[,] grid)
            {
                Grid = grid;
                EndNode = null;
                Skip = 1;
                StepsRemain = 0;
                StepsDone = 0;
            }

            /// <summary>
            /// skipセット
            /// </summary>
            /// <param name="skip"></param>
            public void SetSkip(int skip)
                => Skip = Math.Max(1, skip);

            Point<int> JumpP(Point<int> p, int srcX, int srcY)
            {
                int dx = p.X - srcX;
                int dy = p.Y - srcY;

                if (dx != 0 && dy != 0) return JumpD(p, dx, dx);
                else if (dx != 0) return JumpX(p, dx);
                else if (dy != 0) return JumpY(p, dy);
                else return NPos;
            }

            Point<int> JumpD(Point<int> p, int dx, int dy)
            {
                int endPosX = EndNode.X;
                int endPosY = EndNode.Y;
                int steps = 0;

                while (true)
                {
                    if (p.X == endPosX && p.Y == endPosY)
                        break;
                    ++steps;
                    int x = p.X, y = p.Y;

                    if ((Grid[x - dx, y + dy] && !Grid[x - dx, y]) || (Grid[x + dx, y - dy] && !Grid[x, y - dy]))
                        break;

                    bool gdx = Grid[x + dx, y];
                    bool gdy = Grid[x, y + dy];

                    if (gdx && JumpX(new Point<int> { X = x + dx, Y = y }, dx).X != -1)//is valid
                        break;
                    if (gdy && JumpY(new Point<int> { X = x, Y = y + dy }, dy).X != -1)//is valid
                        break;

                    if ((gdx || gdy) && Grid[x + dx, y + dy])
                    {
                        p.X += dx;
                        p.Y += dy;
                    }
                    else
                    {
                        p = NPos;
                        break;
                    }
                }
                StepsDone += steps;
                StepsRemain -= steps;
                return p;
            }

            Point<int> JumpX(Point<int> p, int dx)
            {
                int y = p.Y;
                int endPosX = EndNode.X;
                int endPosY = EndNode.Y;
                int skip = Skip;
                int steps = 0;

                uint a = ~((uint)(Grid[p.X, y + skip] ? 1 : 0) | (uint)((Grid[p.X, y - skip] ? 1 : 0) << 1));

                while (true)
                {
                    int xx = p.X + dx;
                    uint b = (uint)(Grid[xx, y + skip] ? 1 : 0) | (uint)((Grid[xx, y - skip] ? 1 : 0) << 1);

                    if ((b & a) != 0 || (p.X == endPosX && p.Y == endPosY))
                        break;
                    if (!Grid[xx, y])
                    {
                        p = NPos;
                        break;
                    }

                    p.X += dx;
                    a = ~b;
                    ++steps;
                }
                StepsDone += steps;
                StepsRemain -= steps;
                return p;
            }

            Point<int> JumpY(Point<int> p, int dy)
            {
                int x = p.X;
                int endPosX = EndNode.X;
                int endPosY = EndNode.Y;
                int skip = Skip;
                int steps = 0;

                uint a = ~((uint)(Grid[x + skip, p.Y] ? 1 : 0) | (uint)((Grid[x - skip, p.Y] ? 1 : 0) << 1));

                while (true)
                {
                    int yy = p.Y + dy;
                    uint b = (uint)(Grid[x + skip, yy] ? 1 : 0) | (uint)((Grid[x - skip, yy] ? 1 : 0) << 1);

                    if ((b & a) != 0 || (p.X == endPosX && p.Y == endPosY))
                        break;
                    if (!Grid[x, yy])
                    {
                        p = NPos;
                        break;
                    }

                    p.Y += dy;
                    a = ~b;
                    ++steps;
                }
                StepsDone += steps;
                StepsRemain -= steps;
                return p;
            }
            
            /// <summary>
            /// 隣接点探す
            /// </summary>
            /// <param name="n"></param>
            /// <returns></returns>
            (int, Point<int>[]) FindNeighbors(Node n)
            {
                Point<int>[] w = new Point<int>[8];
                int x = n.X;
                int y = n.Y;
                int wIndex = 0;

                bool JPS_CHECKGRID(int dx_, int dy_) => Grid[x + (dx_), y + (dy_)];
                void JPS_ADDPOS(int dx_, int dy_)
                {
                    w[wIndex].X = x + dx_;
                    w[wIndex++].Y = y + dy_;
                }
                void JPS_ADDPOS_CHECK(int dx_, int dy_)
                {
                    if (JPS_CHECKGRID(dx_, dy_))
                        JPS_ADDPOS(dx_, dy_);
                }
                void JPS_ADDPOS_NO_TUNNEL(int dx_, int dy_)
                {
                    if (Grid[x + (dx_), y] || Grid[x, y + (dy_)])
                        JPS_ADDPOS_CHECK(dx_, dy_);
                }

                if (n.Parent == null)
                {
                    // straight moves
                    JPS_ADDPOS_CHECK(-Skip, 0);
                    JPS_ADDPOS_CHECK(0, -Skip);
                    JPS_ADDPOS_CHECK(0, Skip);
                    JPS_ADDPOS_CHECK(Skip, 0);

                    // diagonal moves + prevent tunneling
                    JPS_ADDPOS_NO_TUNNEL(-Skip, -Skip);
                    JPS_ADDPOS_NO_TUNNEL(-Skip, Skip);
                    JPS_ADDPOS_NO_TUNNEL(Skip, -Skip);
                    JPS_ADDPOS_NO_TUNNEL(Skip, Skip);

                    return (wIndex, w);
                }

                // jump directions (both -1, 0, or 1)
                int dx = x - n.Parent.X;
                dx /= Math.Max(Math.Abs(dx), 1);
                dx *= Skip;
                int dy = y - n.Parent.Y;
                dy /= Math.Max(Math.Abs(dy), 1);
                dy *= Skip;

                if (dx != 0 && dy != 0)
                {
                    // diagonal
                    // natural neighbors
                    bool walkX = false;
                    bool walkY = false;
                    if ((walkX = Grid[x + dx, y]))
                    {
                        w[wIndex].X = x + dx;
                        w[wIndex++].Y = y;
                    }
                    if ((walkY = Grid[x, y + dy]))
                    {
                        w[wIndex].X = x;
                        w[wIndex++].Y = y + dy;
                    }

                    if (walkX || walkY)
                        JPS_ADDPOS_CHECK(dx, dy);

                    // forced neighbors
                    if (walkY && !JPS_CHECKGRID(-dx, 0))
                        JPS_ADDPOS_CHECK(-dx, dy);

                    if (walkX && !JPS_CHECKGRID(0, -dy))
                        JPS_ADDPOS_CHECK(dx, -dy);
                }
                else if (dx != 0)
                {
                    // along X axis
                    if (JPS_CHECKGRID(dx, 0))
                    {
                        JPS_ADDPOS(dx, 0);

                        // Forced neighbors (+ prevent tunneling)
                        if (!JPS_CHECKGRID(0, Skip))
                            JPS_ADDPOS_CHECK(dx, Skip);
                        if (!JPS_CHECKGRID(0, -Skip))
                            JPS_ADDPOS_CHECK(dx, -Skip);
                    }


                }
                else if (dy != 0)
                {
                    // along Y axis
                    if (JPS_CHECKGRID(0, dy))
                    {
                        JPS_ADDPOS(0, dy);

                        // Forced neighbors (+ prevent tunneling)
                        if (!JPS_CHECKGRID(Skip, 0))
                            JPS_ADDPOS_CHECK(Skip, dy);
                        if (!JPS_CHECKGRID(-Skip, 0))
                            JPS_ADDPOS_CHECK(-Skip, dy);
                    }
                }
                return (wIndex, w);
            }

            /// <summary>
            /// 後続を特定
            /// </summary>
            /// <param name="n"></param>
            void IdentifySuccessors(Node n)
            {
                float Euclidean(float ax, float ay, float bx, float by)
                {
                    float fx = ax - bx;
                    float fy = ay - by;
                    return (float)Math.Sqrt(fx * fx + fy * fy);
                }

                int Manhattan(int ax, int ay, int bx, int by)
                    => Math.Abs(ax - bx) + Math.Abs(ay - by);

                (int num, Point<int>[] buf) = FindNeighbors(n);
                for (int i = num - 1; i >= 0; --i)
                {
                    // Invariant:対応するグリッド位置が移動可能(JumpPでアサートされる)である場合、ノードは有効なネイバーのみです。
                    Point<int> jp = JumpP(buf[i], n.X, n.Y);

                    if (jp.X == -1)
                        continue;

                    // グリッド位置が確実に有効なジャンプポイントであることがわかったので、実際のノードを作成しなければなりません。
                    Node jn = Open.TryPeek(jp.X, jp.Y, out var node) ? node : new Node(jp.X, jp.Y);
                    if (!jn.IsClosed)
                    {
                        float extraG = Euclidean(jn.X, jn.Y, n.X, n.Y);
                        float newG = n.G + extraG;
                        if (!jn.IsOpen || newG < jn.G)
                        {
                            jn.G = newG;
                            jn.F = jn.G + Manhattan(jn.X, jn.Y, EndNode.X, EndNode.Y);
                            jn.Parent = n;
                            if (!jn.IsOpen)
                            {
                                Open.Push(jn);
                                jn.SetOpen();
                            }
                            else
                            {
                                //ヒープ修正
                                Open.Fixup();
                            }
                        }
                    }
                }
            }

            Result FindPathStep(int limit)
            {
                StepsRemain = limit;
                do
                {
                    if (Open.Count() <= 0)
                        return Result.NO_PATH;
                    Node n = Open.Pop();
                    n.SetClosed();
                    if (n.X == EndNode.X && n.Y == EndNode.Y)
                    {
                        EndNode = n;
                        return Result.FOUND_PATH;
                    }
                    IdentifySuccessors(n);
                } while (StepsRemain >= 0);
                return Result.NEED_MORE_STEPS;
            }

            /// <summary>
            /// パス生成
            /// </summary>
            /// <param name="path"></param>
            /// <param name="step"></param>
            /// <returns></returns>
            bool FindPathFinish(out Point<int>[] path, int step)
            {
                path = null;
                List<Point<int>> result = new List<Point<int>>();
                if (step == 0)
                {
                    Node next = EndNode;
                    if (next.Parent == null) return false;
                    do
                    {
                        result.Add(new Point<int> { X = next.X, Y = next.Y });
                        next = next.Parent;
                    } while (next.Parent != null);
                }
                else
                {
                    throw new NotImplementedException();
                }
                result.Reverse();
                path = result.ToArray();
                return true;
            }

            /// <summary>
            /// パス見つける
            /// </summary>
            /// <param name="path"></param>
            /// <param name="start"></param>
            /// <param name="end"></param>
            /// <param name="step"></param>
            /// <returns></returns>
            public bool FindPath(out Point<int>[] path, int startX, int startY, int goalX, int goalY, int step)
            {
                Result res = FindPathInit(startX, startY, goalX, goalY);

                // If this is true, the resulting path is empty (findPathFinish() would fail, so this needs to be checked before)
                if (res == Result.FOUND_PATH)
                {
                    path = new Point<int>[0];
                    return true;
                }

                while (true)
                {
                    switch (res)
                    {
                        case Result.NEED_MORE_STEPS:
                            res = FindPathStep(0);
                            break; // the switch

                        case Result.FOUND_PATH:
                            path = null;
                            return FindPathFinish(out path, step);

                        case Result.NO_PATH:
                        default:
                            path = null;
                            return false;
                    }
                }
            }

            /// <summary>
            /// パラメータ初期化
            /// </summary>
            /// <param name="startX"></param>
            /// <param name="startY"></param>
            /// <param name="goalX"></param>
            /// <param name="goalY"></param>
            /// <returns></returns>
            Result FindPathInit(int startX, int startY, int goalX, int goalY)
            {
                /* for (NodeGrid::iterator it = nodegrid.begin(); it != nodegrid.end(); ++it)
                     it->second.clearState();*/
                Open.Clear();
                EndNode = null;
                StepsDone = 0;

                // If skip is > 1, make sure the points are aligned so that the search will always hit them
                startX = (startX / Skip) * Skip;
                startY = (startY / Skip) * Skip;
                goalX = (goalX / Skip) * Skip;
                goalY = (goalY / Skip) * Skip;

                if (startX == goalX && startY == goalY)
                {
                    // There is only a path if this single position is walkable.
                    // But since the starting position is omitted, there is nothing to do here.
                    return Grid[goalX, goalY] ? Result.FOUND_PATH : Result.NO_PATH;
                }

                // If start or end point are obstructed, don't even start
                if (!Grid[startX, startY] || !Grid[goalX, goalY])
                    return Result.NO_PATH;

                Open.Push(new Node(startX, startY));
                EndNode = new Node(goalX, goalY);

                return Result.NEED_MORE_STEPS;
            }
        }
    }
}
