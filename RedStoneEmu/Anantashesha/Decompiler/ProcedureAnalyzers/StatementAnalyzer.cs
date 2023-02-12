using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Anantashesha.Decompiler.Disassemble.Disassembler;

namespace Anantashesha.Decompiler.ProcedureAnalyzers
{
    /// <summary>
    /// ステートメント解析
    /// </summary>
    class StatementAnalyzer
    {
        /// <summary>
        /// 入力コード
        /// </summary>
        Dictionary<uint, List<SimpleCode>> CodeBlocks;

        /// <summary>
        /// ステートメントリスト
        /// </summary>
        Dictionary<uint, SimpleStatement> Statements = new Dictionary<uint, SimpleStatement>();

        Dictionary<uint, uint[]> Successors;
        Dictionary<uint, uint[]> Predecessors;
        Dictionary<uint, uint[]> Dominators;
        Dictionary<uint, int[]> CaseList;

        uint EntryIP;

        /// <summary>
        /// 疑似IFステートメント
        /// </summary>
        Dictionary<uint, (uint trueBlockIP, uint falseBlockIP)> TemporaryIfBlock
            = new Dictionary<uint, (uint trueBlockIP, uint falseBlockIP)>();

        /// <summary>
        /// 疑似Whileブロック
        /// </summary>
        Dictionary<uint, uint[]> TemporaryWhileBlock = new Dictionary<uint, uint[]>();

        /// <summary>
        /// 疑似Switch-Caseブロック
        /// </summary>
        Dictionary<uint, (int caselabel, uint caseIP)[]> TemporarySwitchCaseBlock
            = new Dictionary<uint, (int caselabel, uint caseIP)[]>();


        public StatementAnalyzer(Dictionary<uint, List<SimpleCode>> codeBlocks,
            Dictionary<uint, uint[]> successors, Dictionary<uint, uint[]> predecessors, Dictionary<uint, int[]> caseList, uint entryIP)
        {
            CodeBlocks = codeBlocks;
            Successors = successors;
            Predecessors = predecessors;
            CaseList = caseList;
            EntryIP = entryIP;
        }


        public IDisasm[] Analyze()
        {
            //ループ計算
            ComputeNaturalLoops();

            //switch計算
            ComputeSwitchCase();

            //if計算
            ComputeNaturalIf();

            //統合
            return MargeStatements();
        }

        /// <summary>
        /// ステートメント統合
        /// </summary>
        /// <param name="codes"></param>
        /// <returns></returns>
        IDisasm[] MargeStatements()
        {
            if (CodeBlocks.Keys.First() != EntryIP)
                throw new NotImplementedException();

            //使用済みブロックIP
            List<uint> usedBlockIP = new List<uint>();

            return _MargeStatements(CodeBlocks.Keys).ToArray();
            
            IEnumerable<IDisasm>_MargeStatements(IEnumerable<uint> blockIPs, uint stmtIP=0)
            {
                foreach(var blockIP in blockIPs)
                {
                    if (usedBlockIP.Contains(blockIP)) continue;

                    foreach (var code in CodeBlocks[blockIP])
                    {
                        uint ip = code.ip;
                        if (ip != stmtIP && Statements.ContainsKey(ip))
                        {
                            switch (Statements[ip])
                            {
                                case SimpleLoop sl:
                                    sl.TrueCodes = _MargeStatements(TemporaryWhileBlock[ip].OrderBy(t => t), ip).ToDictionary(t => t.ip, t => t);
                                    usedBlockIP.AddRange(TemporaryWhileBlock[ip]);
                                    yield return sl;
                                    break;
                                case SimpleIf sif:
                                    (var tblock, var fblock) = TemporaryIfBlock[ip];
                                    sif.TrueCodes = _MargeStatements(new uint[] { tblock }).ToDictionary(t => t.ip, t => t);
                                    usedBlockIP.Add(tblock);
                                    if (fblock != 0)
                                    {
                                        sif.FalseCodes = _MargeStatements(new uint[] { fblock }).ToDictionary(t => t.ip, t => t);
                                        usedBlockIP.Add(fblock);
                                    }
                                    yield return sif;
                                    break;
                                case SimpleSwitch ssw:
                                    foreach((var caselabel, var caseIP) in TemporarySwitchCaseBlock[ip])
                                    {
                                        ssw.CaseCodes[caselabel] = _MargeStatements(new uint[] { caseIP}).ToDictionary(t => t.ip, t => t);
                                        usedBlockIP.Add(caseIP);
                                    }
                                    break;
                                default:
                                    throw new NotImplementedException();
                            }
                        }
                        else
                        {
                            yield return code;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Switch-Case文を計算
        /// </summary>
        void ComputeSwitchCase()
        {
            foreach (var switchIP in BlockIPInOrder().Where(t => Successors[t].Length > 2))
            {
                var switchGotoCode = CodeBlocks[switchIP].Last();

                //lastがgotoチェック
                if (switchGotoCode.Cmd != SimpleCode.CodeCmd.Goto)
                    throw new ArgumentException(switchGotoCode.ToString());

                SimpleSwitch Switch = new SimpleSwitch(switchGotoCode.ip)
                {
                    EvalCode = ((SimpleCode)((SimpleCode)switchGotoCode.OP1).OP1).OP1
                };

                //擬似コード追加
                TemporarySwitchCaseBlock[switchGotoCode.ip] = Enumerable.Range(0, CaseList[switchGotoCode.ip].Length)
                    .Select(t => (MightEvalCode(CaseList[switchGotoCode.ip][t]), Successors[switchIP][t])).ToArray();

                Statements[switchGotoCode.ip] = Switch;

                //case調整
                int MightEvalCode(int label)
                {
                    if (Switch.EvalCode is SimpleCode eval &&
                        eval.TryGetNode(out SimpleData arg, t => t.Type == SimpleData.DataOP.Argment || t.Type == SimpleData.DataOP.LocalVar) &&
                        eval.TryAssignAndExecute(arg, label, out int result, null)) return result;
                    else return label;
                }
            }
        }

        /// <summary>
        /// ブロックオーダー順番
        /// </summary>
        /// <returns></returns>
        IEnumerable<uint> BlockIPInOrder()
        {
            Stack<uint> workList = new Stack<uint>();
            List<uint> workedList = new List<uint>();
            workList.Push(EntryIP);
            uint currentIP;
            while (workList.Count > 0)
            {
                currentIP = workList.Pop();
                yield return currentIP;
                workedList.Add(currentIP);
                foreach (var succ in Successors[currentIP].Where(t => !workedList.Contains(t) && !workList.Contains(t)))
                {
                    workList.Push(succ);
                }
            }
        }

        /// <summary>
        /// if文計算
        /// </summary>
        void ComputeNaturalIf()
        {
            uint nextBlock = 0;
            foreach(var blockIP in CodeBlocks.Keys)
            {
                if (nextBlock != 0)
                {
                    //next blockになるまでループ
                    if (blockIP == nextBlock) nextBlock = 0;
                    else continue;
                }
                var ifCode = CodeBlocks[blockIP].Last();
                if (ifCode.Cmd != SimpleCode.CodeCmd.If || Statements.ContainsKey(ifCode.ip)) continue;

                uint trueBlockIp = Successors[blockIP][0],
                    falseBlockIp = Successors[blockIP][1];

                if (StructureIfElse(ifCode, trueBlockIp, falseBlockIp)) continue;
                if (StructureIfElse2(ifCode, trueBlockIp, falseBlockIp)) continue;
                if (StructureNegateIf(ifCode, trueBlockIp, falseBlockIp)) continue;
                nextBlock = StructureIfs(ifCode, blockIP, trueBlockIp, falseBlockIp);

                if (nextBlock == 0)
                {
                    //通常のIF文
                    SimpleIf If = new SimpleIf(ifCode.ip);
                    If.Expression = ifCode.Expression;
                    If.Expression.Negate();
                    uint elseIP = 0;
                    if (IsNotJMPorJMC(trueBlockIp) || Successors[trueBlockIp].All(t => IsNotJMPorJMC(t)))
                    {
                        //else構築
                        elseIP = trueBlockIp;
                    }

                    TemporaryIfBlock[ifCode.ip] = (falseBlockIp, elseIP);
                    Statements[ifCode.ip] = If;
                }
            }
        }

        /// <summary>
        /// その他If構築
        /// </summary>
        /// <param name="ifIp"></param>
        /// <param name="trueBlockIp"></param>
        /// <param name="falseBlockIp"></param>
        /// <returns></returns>
        uint StructureIfs(SimpleCode ifCode, uint ifBlockIP, uint trueBlockIp, uint falseBlockIp)
        {
            //BlockIP:TrueIP
            Dictionary<uint, uint> blockTrueIPList = new Dictionary<uint, uint> { { ifBlockIP, trueBlockIp } };
            uint realTrueBlockIP = 0;

            //ブロックIP収集
            if (!collectBlockIP(ifBlockIP) || blockTrueIPList.Count <= 1)
            {
                //構築失敗
                return 0;
            }

            //式チェーンを左から優先度順に並べるためにスタックを使う
            Stack<(SimpleExpression expr, SimpleExpression.ExpressionListType type)> exprList
                = new Stack<(SimpleExpression expr, SimpleExpression.ExpressionListType type)>();

            //最初の式・タイプ
            var currentExpr = ifCode.Expression;
            if (trueBlockIp != realTrueBlockIP) currentExpr.Negate();
            var currentType = trueBlockIp == realTrueBlockIP ? 
                SimpleExpression.ExpressionListType.Or : SimpleExpression.ExpressionListType.And;

            //初期push
            exprList.Push((currentExpr, currentType));
            
            //スタック作成
            foreach (var blockIP in blockTrueIPList.Keys.Skip(1))
            {
                //次のタイプ
                if (Successors[blockIP][1] != realTrueBlockIP)//ラスト以外を変更
                {
                    if (Successors[blockIP][0] == realTrueBlockIP)
                        currentType = SimpleExpression.ExpressionListType.Or;
                    else
                        currentType = SimpleExpression.ExpressionListType.And;
                }

                currentExpr = CodeBlocks[blockIP].First().Expression;

                //条件反転
                if (Successors[blockIP][0] != realTrueBlockIP) currentExpr.Negate();
                
                //式・タイプ確定
                exprList.Push((currentExpr, currentType));
                CodeBlocks[blockIP].RemoveAt(0);//削除
            }


            SimpleIf If = new SimpleIf(ifCode.ip);
            TemporaryIfBlock[ifCode.ip] = (realTrueBlockIP, 0);

            //チェーン作成
            (currentExpr, currentType) = exprList.Pop();
            If.Expression = new SimpleExpression(currentExpr, currentType);
            SimpleExpression currentExprList = If.Expression;
            while (exprList.Count > 0)
            {
                (currentExpr, currentType) = exprList.Pop();
                currentExprList.Next = new SimpleExpression(currentExpr, currentType);
                currentExprList = currentExprList.Next;
            }

            Statements[ifCode.ip] = If;

            //real false block返す
            return realTrueBlockIP;


            bool collectBlockIP(uint blockIP)
            {
                if (Successors[blockIP].Length == 2)
                {
                    //現在がif文
                    uint nextBlockIP = Successors[blockIP][1];
                    if (Successors[nextBlockIP].Length == 2 && CodeBlocks[nextBlockIP].First().Cmd==SimpleCode.CodeCmd.If)
                    {
                        //false blockもif文
                        uint nextTrueBlockIP = Successors[nextBlockIP][0];
                        uint nextFalseBlockIP = Successors[nextBlockIP][1];

                        if (IsNotJMPorJMC(nextTrueBlockIP) && IsNotJMPorJMC(nextFalseBlockIP))
                        {
                            //ここでIF文終了
                            realTrueBlockIP = nextBlockIP;
                            return true;
                        }

                        if (blockTrueIPList.Values.Distinct().Count() >= 2 && !blockTrueIPList.ContainsValue(nextTrueBlockIP))
                        {
                            //trueIPのバリエーションが2種類超える
                            if (blockTrueIPList.ContainsValue(nextFalseBlockIP))
                            {
                                //or項が発生
                                realTrueBlockIP = nextFalseBlockIP;
                            }
                            else
                            {
                                realTrueBlockIP = nextBlockIP;
                            }
                            return true;
                        }

                        //追加
                        blockTrueIPList[nextBlockIP] = nextTrueBlockIP;

                        //next falseが既に登録されている場合はorで終了
                        if (blockTrueIPList.ContainsValue(nextFalseBlockIP))
                        {
                            //or項が発生
                            realTrueBlockIP = nextFalseBlockIP;
                            return true;
                        }
                        else
                        {
                            //再帰
                            return collectBlockIP(nextBlockIP);
                        }
                    }
                    else
                    {
                        realTrueBlockIP = nextBlockIP;
                        return true;
                    }
                }
                return false;
            }
        }

        bool StructureNegateIf(SimpleCode ifCode, uint trueBlockIp, uint falseBlockIp)
        {
            if (Successors[trueBlockIp].Length == 1 && Successors[trueBlockIp][0] == falseBlockIp)
            {
                SimpleIf If = new SimpleIf(ifCode.ip);
                If.Expression = ifCode.Expression;
                If.Expression.Negate();

                removeLastGoto(trueBlockIp);
                removeLastGoto(falseBlockIp);
                TemporaryIfBlock[ifCode.ip] = (trueBlockIp, 0);
                Statements[ifCode.ip] = If;
                return true;
            }
            return false;
        }

        /// <summary>
        /// If-Else構築
        /// </summary>
        /// <param name="ifIp"></param>
        /// <returns></returns>
        bool StructureIfElse(SimpleCode ifCode, uint trueBlockIp, uint falseBlockIp)
        {
            //true, falseの後任がjmpでジャンプ先ノードが一致している必要がある
            if (Successors[trueBlockIp].Length != 1 || Successors[falseBlockIp].Length != 1
                || Successors[falseBlockIp][0] != Successors[trueBlockIp][0]) return false;

            SimpleIf If = new SimpleIf(ifCode.ip);
            If.Expression = ifCode.Expression;
            If.Expression.Negate();

            removeLastGoto(trueBlockIp);
            removeLastGoto(falseBlockIp);
            TemporaryIfBlock[ifCode.ip] = (trueBlockIp, falseBlockIp);
            Statements[ifCode.ip] = If;
            return true;
        }

        /// <summary>
        /// Switch Return用If-Else構築
        /// </summary>
        /// <param name="ifIp"></param>
        /// <returns></returns>
        bool StructureIfElse2(SimpleCode ifCode, uint trueBlockIp, uint falseBlockIp)
        {
            //truesucc = falsesucc = 0 or >2
            if (IsNotJMPorJMC(trueBlockIp) && IsNotJMPorJMC(falseBlockIp))
            {
                SimpleIf If = new SimpleIf(ifCode.ip);
                If.Expression = ifCode.Expression;
                
                TemporaryIfBlock[ifCode.ip] = (trueBlockIp, falseBlockIp);
                Statements[ifCode.ip] = If;
                return true;
            }
            return false;

        }

        /// <summary>
        /// jmpでもjmcでもない
        /// </summary>
        /// <param name="blockIP"></param>
        /// <returns></returns>
        bool IsNotJMPorJMC(uint blockIP)
            => Successors[blockIP].Length == 0 || Successors[blockIP].Length > 2;

        /// <summary>
        /// ブロック最後のGOTOを削除
        /// </summary>
        /// <param name="blockIP"></param>
        void removeLastGoto(uint blockIP)
        {
            if (CodeBlocks[blockIP].Last().Cmd == SimpleCode.CodeCmd.Goto)
            {
                CodeBlocks[blockIP].RemoveAt(CodeBlocks[blockIP].Count - 1);
            }
        }

        /// <summary>
        /// ループ計算
        /// </summary>
        void ComputeNaturalLoops()
        {
            //ドミネータ計算
            Dominators = ComputeDominators(CodeBlocks, EntryIP, Predecessors);
            
            //ループ検出
            List<Loop> loopSet = new List<Loop>();
            foreach (var enterIP in CodeBlocks.Keys)
            {
                if (enterIP == EntryIP) continue;

                foreach(var succIP in Successors[enterIP])
                {
                    if (Dominators[enterIP].Contains(succIP))
                    {
                        loopSet.Add(Loop.NaturalLoopForEdge(Predecessors, succIP, enterIP));
                    }
                }
            }

            if (loopSet.Count == 0) return;
            
            //ポストドミネーター
            var postDominators = ComputePostDominators(CodeBlocks, EntryIP, Successors);

            //ループ構造化 do-while
            foreach (var loop in Loop.SortFromInnermostLoopToOutermostLoop(loopSet.ToArray()))//最内ループから最外ループへのループセットのソート
            {
                SimpleLoop doWhile = new SimpleLoop(loop.Header, SimpleLoop.WhileType.DoWhile);

                //式を探す（jmpの可能性）
                foreach (var blockIP in loop.Blocks.Reverse())
                {
                    foreach (var code in CodeBlocks[blockIP].OrderByDescending(t => t.ip))
                    {
                        if (code.TryGetNode(out SimpleCode exprCode, t => t.Expression != null))
                        {
                            //式を決定
                            doWhile.Expression = exprCode.Expression;

                            //コードから削除
                            CodeBlocks[blockIP].RemoveAll(t => t.ip == code.ip);
                            goto foundedExpr;
                        }
                    }
                }
                foundedExpr:

                //ブロックを仮代入
                TemporaryWhileBlock[doWhile.ip] = loop.Blocks;

                //break 全てのループノードを後支配してる後継ノード
                bool foundBreak = false;
                foreach (var breakCandidate in loop.Blocks.SelectMany(t => Successors[t]).Distinct()
                    .Where(t => !loop.Blocks.Contains(t)))
                {
                    if (loop.Blocks.All(t => postDominators[t].Contains(breakCandidate)))
                    {
                        doWhile.BreakIP = breakCandidate;
                        foundBreak = true;
                        break;
                    }
                }
                if (!foundBreak) throw new InvalidOperationException("Breakが見つからない");

                //breakからwhile構造推定
                foreach(var breakPred in Predecessors[doWhile.BreakIP])
                {
                    if(Successors[breakPred].Length == 2 &&
                        Successors[breakPred][0] == doWhile.BreakIP &&
                        Successors[breakPred][1] == loop.Header)
                    {
                        //while化
                        doWhile.Type = SimpleLoop.WhileType.While;

                        //取得して修正（WhileのIPは変えなくてOK）
                        var whileHeader = CodeBlocks[breakPred].Last();
                        Successors[breakPred] = new uint[] { loop.Header };

                        //whileのヘッダIF削除
                        CodeBlocks[breakPred].RemoveAll(t => t.ip == whileHeader.ip);
                        break;
                    }
                }

                //continue
                doWhile.ContinueIP = loop.Blocks.Last();
                if (CodeBlocks[doWhile.ContinueIP].Last().Cmd != SimpleCode.CodeCmd.If ||
                    (Successors[doWhile.ContinueIP].Length == 1 && Successors[doWhile.ContinueIP][0] != loop.Header))
                {
                    doWhile.ContinueIP = 0;
                }
                
                //breakとcontinueセット
                foreach (var blockIP in TemporaryWhileBlock[doWhile.ip])
                {
                    CodeBlocks[blockIP][CodeBlocks[blockIP].Count-1] =
                        StructureBreakContinue(blockIP, CodeBlocks[blockIP].Last(), doWhile.ContinueIP, doWhile.BreakIP);
                }

                Statements[doWhile.ip] = doWhile;
            }
        }

        /// <summary>
        /// break continue構築
        /// </summary>
        /// <param name="targetBlock"></param>
        /// <param name="contBlock"></param>
        /// <param name="breakBlock"></param>
        SimpleCode StructureBreakContinue(uint blockIP, SimpleCode code, uint contBlock, uint breakBlock)
        {
            switch (code.Cmd)
            {
                case SimpleCode.CodeCmd.Goto:
                    uint jmpto = Successors[blockIP][0];
                    if (jmpto == contBlock)
                        return new SimpleCode(code.ip, SimpleCode.CodeCmd.Continue);
                    else if (jmpto == breakBlock)
                        return new SimpleCode(code.ip, SimpleCode.CodeCmd.Break);
                    break;
                case SimpleCode.CodeCmd.If:
                    code.OP1 = StructureBreakContinue(blockIP, (SimpleCode)code.OP1, contBlock, breakBlock);
                    break;
            }
            return code;
        }

        class Loop
        {
            public uint Header;
            public uint[] Blocks;

            private Loop(uint header)
                => Header = header;

            /// <summary>
            /// 最も内側のループから最も外側のループにソート
            /// </summary>
            /// <param name="loops"></param>
            public static IEnumerable<Loop> SortFromInnermostLoopToOutermostLoop(Loop[] loops)
            {
                //ループの親リスト
                Dictionary<int, int> loopPredList = new Dictionary<int, int>();
                
                //ループの内包関係
                foreach (var pLoop in loops.Select((v, i) => new { v, i }))
                {
                    if (loopPredList.ContainsKey(pLoop.i)) continue;
                    foreach (var cLoop in loops.Select((v, i) => new { v, i }).Where(t => t.i != pLoop.i))
                    {
                        bool? checkInclude = pLoop.v.checkIncludeLoop(cLoop.v);
                        if (!checkInclude.HasValue) continue;

                        if (checkInclude.Value)
                        {
                            //cLoop⊆pLoop
                            loopPredList[cLoop.i] = pLoop.i;
                        }
                        else
                        {
                            //cLoop⊆pLoop
                            loopPredList[pLoop.i] = cLoop.i;
                        }
                    }
                }

                //返してないループのインデックス
                List<int> notReturnLoopIndex = Enumerable.Range(0, loops.Length).ToList();

                //ループの最内たち
                foreach(var cLoop in loops.Select((v, i) => new { v, i })
                    .Where(t => loopPredList.ContainsKey(t.i) && !loopPredList.ContainsValue(t.i)))
                {
                    //最内返す
                    yield return cLoop.v;
                    notReturnLoopIndex.Remove(cLoop.i);

                    //親を順に返していく
                    for(int pLoopIndex = loopPredList[cLoop.i]; loopPredList.ContainsKey(pLoopIndex);
                        pLoopIndex = loopPredList[pLoopIndex])
                    {
                        yield return loops[pLoopIndex];
                        notReturnLoopIndex.Remove(pLoopIndex);
                    }
                }

                //1重ループ返す
                foreach(var i in notReturnLoopIndex)
                {
                    yield return loops[i];
                }
            }

            /// <summary>
            /// ループ部分集合チェック
            /// </summary>
            /// <param name="loop"></param>
            /// <returns>true: arg⊆this, false: this⊆arg, null: thisとargは独立</returns>
            bool? checkIncludeLoop(Loop loop)
            {
                //loopAはloopB含む
                bool thisIncludeArg = Blocks.All(t => loop.Blocks.Contains(t));
                //loopBはloopA含む
                bool argIncludeThis = loop.Blocks.All(t => Blocks.Contains(t));
                if (thisIncludeArg && !argIncludeThis)//arg⊆this
                    return true;
                else if (!thisIncludeArg && argIncludeThis)//this⊆arg
                    return false;
                else if (!thisIncludeArg && !argIncludeThis)//thisとargは独立
                    return null;
                else throw new InvalidOperationException("ループの集合が等しい");
            }

            /// <summary>
            /// 基本ループ
            /// </summary>
            /// <param name="headerIP"></param>
            /// <param name="tailIP"></param>
            /// <returns></returns>
            public static Loop NaturalLoopForEdge(Dictionary<uint, uint[]> predecessors, uint headerIP, uint tailIP)
            {
                Loop loop = new Loop(headerIP);
                List<uint> blocks = new List<uint> { headerIP };
                Stack<uint> workList = new Stack<uint>();

                if (headerIP != tailIP)
                {
                    blocks.Add(tailIP);
                    workList.Push(tailIP);
                }
                while (workList.Count() > 0)
                {
                    var enterIP = workList.Pop();
                    foreach (var pred in predecessors[enterIP])
                    {
                        if (!blocks.Contains(pred))
                        {
                            blocks.Add(pred);
                            workList.Push(pred);
                        }
                    }
                }
                blocks.RemoveAt(0);
                blocks.Add(headerIP);
                blocks.Reverse();
                loop.Blocks = blocks.ToArray();
                return loop;
            }
        }

        /// <summary>
        /// ポストドミネーター計算
        /// </summary>
        /// <param name="codeBlocks"></param>
        /// <param name="startIP"></param>
        /// <param name="successors"></param>
        /// <returns></returns>
        public static Dictionary<uint, uint[]> ComputePostDominators(Dictionary<uint, List<SimpleCode>> codeBlocks, uint startIP, Dictionary<uint, uint[]> successors)
            => ComputeDominators(codeBlocks, startIP, successors, post: true);

        /// <summary>
        /// ドミネータ計算
        /// </summary>
        /// <param name="codeBlocks"></param>
        /// <param name="startIP"></param>
        /// <param name="predecessors"></param>
        /// <returns></returns>
        public static Dictionary<uint, uint[]> ComputeDominators(Dictionary<uint, List<SimpleCode>> codeBlocks, uint startIP, Dictionary<uint, uint[]> predecessors)
            => ComputeDominators(codeBlocks, startIP, predecessors, post: false);

        /// <summary>
        /// ドミネータ計算
        /// dominators計算:predOrSucc=Predecessors, post=false
        /// postDomina計算:predOrSucc=Successors, post=True
        /// </summary>
        /// <param name="codeBlocks"></param>
        /// <param name="predOrSucc"></param>
        /// <param name="startIP"></param>
        /// <param name="post"></param>
        /// <returns></returns>
        static Dictionary<uint, uint[]> ComputeDominators(Dictionary<uint, List<SimpleCode>> codeBlocks, uint startIP, 
            Dictionary<uint, uint[]> predOrSucc, bool post = false)
        {
            //初期化
            Dictionary<uint, BitArray> bitDominators = new Dictionary<uint, BitArray>();
            var blockEntrys = codeBlocks.Keys.ToArray();
            int numofBlock = blockEntrys.Length;

            //ipからbitの場所
            Dictionary<uint, int> ipToBitPos = Enumerable.Range(0, numofBlock).ToDictionary(t => blockEntrys[t], t => t);

            //bitDominator初期化
            foreach (var enterIP in blockEntrys)
            {
                if ((!post && enterIP == startIP) || (post && codeBlocks[enterIP].Last().Cmd==SimpleCode.CodeCmd.Retn))
                {
                    //dominatorの場合はentryBlockを，postDomの場合はexitBlockを自己支配のみにセット
                    bitDominators[enterIP] = new BitArray(numofBlock, false);
                    bitDominators[enterIP].Set(ipToBitPos[enterIP], true);
                }
                else
                {
                    //全支配
                    bitDominators[enterIP] = new BitArray(numofBlock, true);
                }
            }

            //探索
            bool changed;
            do
            {
                changed = false;
                foreach (var enterIP in codeBlocks.Keys)
                {
                    if (enterIP == startIP) continue;

                    var beforeBA = new BitArray(bitDominators[enterIP]);
                    foreach (var predDom in predOrSucc[enterIP].Select(t => bitDominators[t]))
                    {
                        bitDominators[enterIP].And(predDom);
                    }
                    bitDominators[enterIP].Set(ipToBitPos[enterIP], true);
                    if (Enumerable.Range(0, numofBlock).Any(pos => beforeBA[pos] != bitDominators[enterIP][pos]))
                        changed = true;
                }

            } while (changed);

            return bitDominators.ToDictionary(t => t.Key,
                t => Enumerable.Range(0, numofBlock).Where(pos => t.Value[pos]).Select(pos => blockEntrys[pos]).ToArray());
        }
    }
}
