using RedStoneLib.Model;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Threading.Tasks;

namespace RedStoneLib.Algorithm
{
    /// <summary>
    /// 移動用マップ
    /// </summary>
    public static class MovingMap
    {
        // 細線化用の８つのフィルタ
        static readonly short[][,] Filters = new short[][,]
            {
                new short[,] { { 0, 0, 0 }, {-1, 1,-1 }, { 1, 1, 1 } },
                new short[,] { {-1, 0, 0 }, { 1, 1, 0 }, {-1, 1,-1 } },
                new short[,] { { 1,-1, 0 }, { 1, 1, 0 }, { 1,-1, 0 } },
                new short[,] { {-1, 1,-1 }, { 1, 1, 0 }, {-1, 0, 0 } },
                new short[,] { { 1, 1, 1 }, {-1, 1,-1 }, { 0, 0, 0 } },
                new short[,] { {-1, 1,-1 }, { 0, 1, 1 }, { 0, 0,-1 } },
                new short[,] { { 0,-1, 1 }, { 0, 1, 1 }, { 0,-1, 1 } },
                new short[,] { { 0, 0,-1 }, { 0, 1, 1 }, {-1, 1,-1 } },
            };

        /// <summary>
        /// 移動用マップに変形
        /// </summary>
        /// <param name="grid"></param>
        /// <param name="outputName"></param>
        /// <returns></returns>
        public static bool Transform(ref bool[,] grid, string outputName = null)
        {
            int w = grid.GetLength(0);
            int h = grid.GetLength(1);

            bool[,] originalGrid = (bool[,])grid.Clone();

            //細線化
            var thinningGrid = Thinning(grid);

            //細線対角線埋め
            for (int x = 0; x < w; x++)
            {
                for (int y = 0; y < h; y++)
                {
                    thinningGrid[x, y] = !thinningGrid[x, y];
                }
            }
            SetFillDiagonal(ref thinningGrid, originalGrid);
            for (int x = 0; x < w; x++)
            {
                for (int y = 0; y < h; y++)
                {
                    thinningGrid[x, y] = !thinningGrid[x, y];
                }
            }

            //膨張
            grid = Expansion2(grid);

            //減算
            for (int x = 0; x < w; x++)
            {
                for (int y = 0; y < h; y++)
                {
                    if (!grid[x, y] && thinningGrid[x, y])
                        grid[x, y] = true;
                }
            }

            //対角線埋め
            SetFillDiagonal(ref grid);

            //オリジナルで通行不可の場所は通行不可にする
            for (int x = 0; x < w; x++)
            {
                for (int y = 0; y < h; y++)
                {
                    if (!originalGrid[x, y] && grid[x, y])
                        grid[x, y] = false;
                }
            }
            
            if (outputName != null)
            {
                Bitmap img = new Bitmap(w, h);
                for (int x = 0; x < w; x++)
                {
                    for (int y = 0; y < h; y++)
                    {
                        if (grid[x, y]) img.SetPixel(x, y, Color.White);
                        else img.SetPixel(x, y, Color.Black);
                    }
                }
                img.Save(outputName);
            }

            return true;
        }

        /// <summary>
        /// 細線化
        /// </summary>
        /// <param name="grid"></param>
        /// <param name="iterations"></param>
        /// <returns></returns>
        static bool[,] Thinning(bool[,] grid, int iterations = 20)
        {
            int gridWidth = grid.GetLength(0);
            int gridHeight = grid.GetLength(1);

            //端を追加したGrid
            bool[,] extraGrid = new bool[gridWidth + 2, gridHeight + 2];

            //細線化実行
            for (int i = 0; i < iterations; i++)
            {
                //extraGrid作成
                for (int x = 0; x < extraGrid.GetLength(0); x++)
                {
                    for (int y = 0; y < extraGrid.GetLength(1); y++)
                    {
                        if (x < gridWidth && y < gridHeight)
                        {
                            //元GRID範囲内
                            extraGrid[x, y] = grid[x, y];
                        }
                        else
                        {
                            //範囲外
                            extraGrid[x, y] = false;
                        }
                    }
                }
                bool[,] tmpGrid = (bool[,])grid.Clone();

                //細線化
                Parallel.For(0, gridWidth, x =>
                {
                    for (int y = 0; y < gridHeight; y++)
                    {
                        if (!extraGrid[x + 1, y + 1]) continue;
                        foreach (var filter in Filters)
                        {
                            //マッチング
                            for (int xx = 0; xx < 3; xx++)
                            {
                                for (int yy = 0; yy < 3; yy++)
                                {
                                    if (xx == 1 && yy == 1) continue;
                                    bool gridResult = extraGrid[x + xx, y + yy];
                                    switch (filter[xx, yy])
                                    {
                                        case 0:
                                            if (!gridResult) break;
                                            else goto notMatch;
                                        case 1:
                                            if (gridResult) break;
                                            else goto notMatch;
                                        default:
                                            break;
                                    }
                                }
                            }
                            //マッチング
                            tmpGrid[x + 1, y + 1] = false;
                            break;

                            notMatch:;
                        }
                    }
                });
                grid = tmpGrid;
            }
            return grid;
        }

        /// <summary>
        /// 膨張
        /// </summary>
        /// <param name="grid"></param>
        /// <returns></returns>
        static bool[,] Expansion(bool[,] grid)
        {
            int w = grid.GetLength(0);
            int h = grid.GetLength(1);
            bool[,] gridTmp = (bool[,])grid.Clone();
            for (int x = 0; x < w; x++)
            {
                for (int y = 0; y < h; y++)
                {
                    if (!grid[x, y])
                    {
                        if (x > 0 && grid[x - 1, y]) gridTmp[x - 1, y] = false;
                        if (x < w - 1 && grid[x + 1, y]) gridTmp[x + 1, y] = false;
                        if (y > 0 && grid[x, y - 1]) gridTmp[x, y - 1] = false;
                        if (y < h - 1 && grid[x, y + 1]) gridTmp[x, y + 1] = false;
                    }
                }
            }
            return gridTmp;
        }

        static bool[,] Expansion2(bool[,] grid)
        {
            int w = grid.GetLength(0);
            int h = grid.GetLength(1);
            bool[,] gridTmp = (bool[,])grid.Clone();
            for (int x = 1; x < w-1; x++)
            {
                for (int y = 1; y < h-1; y++)
                {
                    if (!grid[x, y]) continue;
                    if ( !grid[x - 1, y]|| !grid[x + 1, y]|| !grid[x, y - 1]||!grid[x, y + 1])
                    {
                        gridTmp[x, y] = false;
                    }
                }
            }
            return gridTmp;
        }

        /// <summary>
        /// 対角埋め
        /// </summary>
        /// <param name="grid"></param>
        /// <param name="orgMap"></param>
        static void SetFillDiagonal(ref bool[,] grid, bool [,] orgMap=null)
        {
            int w = grid.GetLength(0);
            int h = grid.GetLength(1);

            //埋める座標
            bool useX = Helper.StaticRandom.Next(2) == 1;

            for (int x = 0; x < w; x++)
            {
                for (int y = 0; y < h; y++)
                {
                    if (x == 0 || y == 0) continue;

                    //対角が空いてる（オリジナルマップが存在する場合、現メソッドで潰した点も考慮）
                    if (!grid[x, y] && !grid[x - 1, y - 1] && grid[x - 1, y] && grid[x, y - 1])
                    {
                        if (orgMap == null)
                        {
                            if (useX) grid[x - 1, y] = false;
                            else grid[x, y - 1] = false;
                        }
                        else
                        {
                            //オリジナルマップの空き状況から対角線を決める
                            if (orgMap[x - 1, y]) grid[x - 1, y] = false;
                            else if (orgMap[x, y - 1]) grid[x, y - 1] = false;
                            else if (useX) grid[x - 1, y] = false;
                            else grid[x, y - 1] = false;
                        }
                    }

                    //対角続き
                    else if (grid[x, y] && grid[x - 1, y - 1] && !grid[x - 1, y] && !grid[x, y - 1])
                    {
                        if (orgMap == null)
                        {
                            if (useX) grid[x, y] = false;
                            else grid[x - 1, y - 1] = false;
                        }
                        else
                        {
                            //オリジナルマップの空き状況から対角線を決める
                            if (orgMap[x, y]) grid[x, y] = false;
                            else if (orgMap[x - 1, y - 1]) grid[x - 1, y - 1] = false;
                            else if (useX) grid[x - 1, y] = false;
                            else grid[x, y - 1] = false;
                        }
                    }
                }
            }
        }
    }
}
