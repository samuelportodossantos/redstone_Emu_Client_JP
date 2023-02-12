#define PARALLEL 

using Anantashesha.Decompiler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Anantashesha
{
    public class BayesianNetwork
    {
        /// <summary>
        /// ノード番号
        /// </summary>
        public int Index;

        /// <summary>
        /// 開始アドレス
        /// </summary>
        public uint Address;

        /// <summary>
        /// シンボル名
        /// </summary>
        public string Symbol;

        /// <summary>
        /// 元ノードからの距離
        /// </summary>
        public int Layer;

        /// <summary>
        /// 命令の量
        /// </summary>
        public int InstructionLength;

        /// <summary>
        /// 子ノードのインデックス
        /// </summary>
        public int[] Children;

        /// <summary>
        /// 親ノードのインデックス
        /// </summary>
        public int[] Parents;

        /// <summary>
        /// 静的変数のインデックス
        /// </summary>
        public int[] Variables;

        /// <summary>
        /// 確率
        /// </summary>
        public double[] Probs;

        public BayesianNetwork(int index, uint address, string symbol, int instructionLength)
        {
            Index = index;
            Address = address;
            Symbol = symbol;
            InstructionLength = instructionLength;
        }

        public BayesianNetwork(int index, uint address, string symbol, int[] parents)
            :this(index, address, symbol, 0)
        {
            Parents = parents;
        }

        public BayesianNetwork() { }

        public override string ToString()
            => Symbol ?? $"sub_{Address:X8}";

        /// <summary>
        /// 尤度の最大化
        /// 尤度が最大のtrain-ansペアを探す
        /// train-ansペアは消す（今後候補に出せないため）
        /// 以上をansかtrainが尽きるまで行う
        /// </summary>
        /// <param name="trainParents"></param>
        /// <param name="answerParents"></param>
        /// <param name="getLikehood"></param>
        /// <returns></returns>
        static double likehoodMaximum(int[] trainParents, int[] answerParents, Func<int, int, double> getLikehood)
        {
            double value = 0;
            int lkCount = 0;
            List<int> tmpTrainParents = new List<int>(trainParents);
            List<int> tmpAnsParents = new List<int>(answerParents);

            while (tmpTrainParents.Count > 0 && tmpAnsParents.Count > 0)// ansかtrainが尽きるまで
            {
                double max = -1;
                int maxTrainIndex = 0;
                int maxAnsIndex = 0;

                // 尤度が最大のtrain-ansペアを探す
                foreach (var trainFuncParent in tmpTrainParents)
                {
                    double innerMax = -1;//trainParentが最も近いanspの尤度
                    int ansParent = 0;//trainParentが最も近いanspのindex
                    foreach (var ansp in tmpAnsParents)
                    {
                        if (innerMax < getLikehood(trainFuncParent, ansp))
                        {
                            innerMax = getLikehood(trainFuncParent, ansp);
                            ansParent = ansp;
                        }
                    }

                    if (max < innerMax)
                    {
                        max = innerMax;
                        maxAnsIndex = ansParent;
                        maxTrainIndex = trainFuncParent;
                    }
                }
                if (!tmpTrainParents.Remove(maxTrainIndex)) throw new InvalidOperationException($"maxTrainIndex {maxTrainIndex} not exist");
                if (!tmpAnsParents.Remove(maxAnsIndex)) throw new InvalidOperationException($"maxAnsIndex {maxAnsIndex} not exist");

                value += max;
                lkCount++;
            }
            return value / (lkCount == 0 ? 1 : lkCount);
        }

        public static void Fit(double varRate = 0.2)
        {
            (var funcAnswer, var varAnswer) = new ProcedureFinder(@"C:\Program Files (x86)\GameON\RED STONE\RedStoneLocal.exe").MakeTree();
            (var funcTrain, var varTrain) = new ProcedureFinder(@"C:\Users\daigo\Documents\redstone_new2\Red Stone_dump.exe").MakeTree(funcAnswer.Length);

            //変数index to 添字
            Dictionary<int, int> variableAnsIndexToSubscript = new Dictionary<int, int>();
            for (int i = 0; i < varAnswer.Length; i++)
            {
                variableAnsIndexToSubscript[varAnswer[i].Index] = i;
            }

            //vartrainのprob初期化
            for (int i = 0; i < varTrain.Length; i++)
            {
                varTrain[i].Probs = new double[varAnswer.Length];
            }
            
            var funcAnswerDic = funcAnswer.ToDictionary(t => t.Index, t => t);
            var funcTrainDic = funcTrain.ToDictionary(t => t.Index, t => t);
            var varTrainDic = varTrain.ToDictionary(t => t.Index, t => t);
            

            //確定したIndex
            List<int> confirmFuncIndex = funcTrain.Select(t => Array.FindIndex(t.Probs, u => u >= 1.0)).Where(t => t >= 0).ToList();
            List<int> confirmVarIndex = new List<int>();
            object locker = new object();

            Dictionary<int, (int left, int top)> curDic = new Dictionary<int, (int left, int top)>();
            foreach (var layer in funcTrain.GroupBy(t => t.Layer))
            {
                Console.Write($"Layer{layer.Key:D2}:");
                curDic[layer.Key] = (Console.CursorLeft, Console.CursorTop);
                Console.WriteLine("");
            }

            //レイヤ毎に更新
            Console.Write("epochs:");
            int left = Console.CursorLeft, top = Console.CursorTop;
            int setCounter = 1;
            for (int epochs = 0; ; epochs++)
            {
                Console.SetCursorPosition(left, top);
                Console.Write($"{epochs + 1:D3}");
                foreach (var layer in funcTrain.GroupBy(t => t.Layer))
                {
                    int maxFuncIndex = -1, maxFuncAns = -1;
                    double maxFuncProb = -1;
#if PARALLEL
                    Parallel.ForEach(layer.Select(t => t.Index).Where(t => !funcTrainDic[t].Probs.Any(prob => prob >= 1.0)), index =>
#else
                    foreach (var index in layer.Select(t => t.Index).Where(t => !trainDic[t].Probs.Any(prob => prob >= 1.0)))
#endif
                    {
                        //各々の尤度を計算
                        foreach (var likehoodIndex in Enumerable.Range(0, funcAnswer.Length).Where(t => !confirmFuncIndex.Contains(t)))
                        {
                            // 尤度の最大化
                            /*int[] tmpTrainParents = funcTrainDic[index].Parents;
                            int[] tmpAnsParents = funcAnswerDic[likehoodIndex].Parents;*/

                            int[] tmpTrainParents;
                            int[] tmpAnsParents;
                            if (epochs % 2 == 0)
                            {
                                tmpTrainParents = funcTrainDic[index].Parents;
                                tmpAnsParents = funcAnswerDic[likehoodIndex].Parents;
                            }
                            else
                            {
                                tmpTrainParents = funcTrainDic[index].Children;
                                tmpAnsParents = funcAnswerDic[likehoodIndex].Children;
                            }
                            funcTrainDic[index].Probs[likehoodIndex] = likehoodMaximum(tmpTrainParents, tmpAnsParents, (t, a) => funcTrainDic[t].Probs[a]);
                            if (funcTrainDic[index].Variables != null && funcAnswerDic[likehoodIndex].Variables != null)
                            {
                                //呼び出し変数から推定
                                funcTrainDic[index].Probs[likehoodIndex] = (1 - varRate) * funcTrainDic[index].Probs[likehoodIndex] +
                                    varRate * likehoodMaximum(funcTrainDic[index].Variables, funcAnswerDic[likehoodIndex].Variables, (t, a) => varTrainDic[t].Probs[variableAnsIndexToSubscript[a]]);
                            }

                            //命令長を考慮
                            funcTrainDic[index].Probs[likehoodIndex] *=
                                1.0 / Math.Pow(1.0 + Math.Abs(funcTrainDic[index].InstructionLength - funcAnswerDic[likehoodIndex].InstructionLength), 0.1);
                        }

                        //尤度の合計で割る
                        var likehoodSum = funcTrainDic[index].Probs.Sum();
                        if (likehoodSum != 0)
                        {
                            funcTrainDic[index].Probs = funcTrainDic[index].Probs.Select(t => t / likehoodSum).ToArray();
                        }

                        lock (locker)
                        {
                            if (funcTrainDic[index].Probs.Max() > maxFuncProb)
                            {
                                maxFuncProb = funcTrainDic[index].Probs.Max();
                                maxFuncAns = Array.FindIndex(funcTrainDic[index].Probs, t => t == maxFuncProb);
                                maxFuncIndex = index;
                            }
                        }
                    }
#if PARALLEL
                    );
#endif
                    Console.SetCursorPosition(curDic[layer.Key].left, curDic[layer.Key].top);
                    if (maxFuncProb < 0) Console.Write("NaN");
                    else
                    {
                        Console.Write($"0x{funcTrainDic[maxFuncIndex].Address:X8} is {funcAnswerDic[maxFuncAns].ToString()}  ");
                        Console.SetCursorPosition(70, curDic[layer.Key].top);
                        Console.Write($"  Prob:{maxFuncProb:00.000%} ");
                    }
                }

                //変数更新
                int maxVarIndex = -1, maxVarAns = -1;
                double maxVarProb = -1;
#if PARALLEL
                Parallel.ForEach(varTrainDic.Keys.Where(t => !varTrainDic[t].Probs.Any(prob => prob >= 1.0)), index =>
#else
                foreach (var index in varTrainDic.Keys.Where(t => !varTrainDic[t].Probs.Any(prob => prob >= 1.0)))
#endif
                {
                    //各々の尤度を計算
                    foreach (var likehoodIndex in Enumerable.Range(0, varAnswer.Length).Where(t => !confirmVarIndex.Contains(t)))
                    {
                        // 尤度の最大化
                        varTrainDic[index].Probs[likehoodIndex] = 
                            likehoodMaximum(varTrainDic[index].Parents, varAnswer[likehoodIndex].Parents, (t, a) => funcTrainDic[t].Probs[a]);
                    }

                    //尤度の合計で割る
                    var likehoodSum = varTrainDic[index].Probs.Sum();
                    if (likehoodSum != 0)
                    {
                        varTrainDic[index].Probs = varTrainDic[index].Probs.Select(t => t / likehoodSum).ToArray();
                    }

                    lock (locker)
                    {
                        if (varTrainDic[index].Probs.Max() > maxVarProb)
                        {
                            maxVarProb = varTrainDic[index].Probs.Max();
                            maxVarAns = Array.FindIndex(varTrainDic[index].Probs, t => t == maxVarProb);
                            maxVarIndex = index;
                        }
                    }
                }
#if PARALLEL
                );
#endif
                Console.SetCursorPosition(0, top + 1);
                Console.Write($"Variable: 0x{varTrainDic[maxVarIndex].Address:X8} is {varAnswer[maxVarAns].ToString()}  ");
                Console.SetCursorPosition(70, top + 1);
                Console.Write($"  Prob:{maxVarProb:00.000%} ");

                //強制
                /*List<(double max, double sub)> subs = new List<(double max, double sub)>();
                Parallel.ForEach(funcTrainDic.Keys.Where(t => !funcTrainDic[t].Probs.Any(prob => prob >= 1.0)), index =>
                {
                    var max = funcTrainDic[index].Probs.Max();
                    if (funcTrainDic[index].Probs.Count(t => t == max) > 1) return;

                    double sub = max - funcTrainDic[index].Probs.OrderByDescending(t => t).Skip(1).First();

                    lock (locker)
                    {
                        subs.Add((max, sub));
                        //if (sub < 0.1) return;
                        if (max < 0.2) return;

                        int maxIndex = Array.FindIndex(funcTrainDic[index].Probs, t => t == max);
                        if (confirmFuncIndex.Contains(maxIndex)) return;

                        confirmFuncIndex.Add(maxIndex);
                        for (int i = 0; i < funcTrainDic[index].Probs.Length; i++)
                        {
                            funcTrainDic[index].Probs[i] = 0.0;
                        }
                        funcTrainDic[index].Probs[maxIndex] = 1.0;

                        Console.SetCursorPosition(0, top + (++setCounter));
                        Console.Write($"{funcAnswerDic[maxIndex].Symbol} is :0x{funcTrainDic[index].Address:X8} sub:{sub}");
                    }
                });*/

                var sendPacketTrain = funcTrain.First(t => t.Address == 0x5C3E70);
                var sendPacketAns = funcAnswer.First(t => t.Symbol.Contains("sendPacket"));
                var trainMain = sendPacketTrain.Probs.Select((v, i) => new { v, i }).OrderByDescending(t => t.v).First();
                Console.SetCursorPosition(0, top + 2);
                Console.Write($"Prob:{sendPacketTrain.Probs[sendPacketAns.Index]:00.000%}, Max:{funcAnswer[trainMain.i].Symbol}, MaxProb:{trainMain.v:00.000%}");

                /*var maxsub = subs.OrderByDescending(t => t.max).First();
                Console.SetCursorPosition(0, top + 1);
                Console.Write($"max:{maxsub.max} sub:{maxsub.sub}");*/
            }
        }
    }
}
