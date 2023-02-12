using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Anantashesha.Decompiler.Disassemble.Disassembler;

namespace Anantashesha.Decompiler.ProcedureAnalyzers.DataFlowAnalyzers
{
    static class FrameAnalyzer
    {
        public enum AnalyzeResult:ulong
        {
            /// <summary>
            /// 成功
            /// </summary>
            Success,

            /// <summary>
            /// ローカルサイズ取得失敗
            /// </summary>
            DetectLocalSizeFailed,

            /// <summary>
            /// ローカルサイズに未知のalloc関数使用を検出 上位8ビットに関数呼び出しのアドレスセット
            /// </summary>
            DetectUseUnkAllocFunc,

            /// <summary>
            /// スタックポインタにエイリアス検出
            /// </summary>
            DetectSPAlies,

            /// <summary>
            /// フレームレス関数のパラメータ調整失敗
            /// </summary>
            AdjustFramelessParamaterFailed,
        }

        /// <summary>
        /// ローカル変数・引数を解析
        /// </summary>
        /// <param name="codeBlock"></param>
        /// <param name="successors"></param>
        /// <param name="startIP"></param>
        /// <returns></returns>
        public static AnalyzeResult AnalyzeLocalParamater(Dictionary<uint, List<t_disasm>> codeBlock, Dictionary<uint, uint[]> successors, uint startIP)
        {
            var exitCodes = codeBlock.Where(t => successors[t.Key].Length==0 && t.Value.Last().cmdtypeKind==D.RET).ToDictionary(t => t.Key, t => t.Value);

            //フレーム検出
            bool isFrame = TryRemoveFrame(codeBlock[startIP], exitCodes);

            //フレームポインタ
            int FP;
            if (isFrame) FP = (int)REG.EBP;
            else FP = (int)REG.ESP;

            bool getFrameLocalSizeResult = true;
            AnalyzeResult getFramelessAdjustResult = AnalyzeResult.Success;

            int localSize;
            if (isFrame)//frame サイズ検出
                getFrameLocalSizeResult = TryGetLocalSizeWithFrame(codeBlock[startIP], exitCodes, out localSize);
            else//frameless ずれ調整
                getFramelessAdjustResult = AdjustFramelessParamater(codeBlock, successors, startIP, out localSize);

            //ローカルサイズ取得失敗
            if (isFrame && !getFrameLocalSizeResult || !isFrame && getFramelessAdjustResult == AnalyzeResult.DetectLocalSizeFailed)
            {
                var call = codeBlock[startIP].FirstOrDefault(t => t.cmdtypeKind == D.CALL);
                if (call.cmdtypeKind == D.CALL)
                {
                    //関数検出
                    return (AnalyzeResult)((ulong)AnalyzeResult.DetectUseUnkAllocFunc | (call.ip << 0x20));
                }

                if (codeBlock[startIP].Exists(t => t.op[0].IsRegNotIncludedMemory((REG)FP) && t.op[1].IsRegIncludedMemory() &&
                 (t.cmdtypeKind == D.MOV || t.cmdtypeKind == D.CMD)))
                {
                    //エイリアス検出の可能性
                    return AnalyzeResult.DetectSPAlies;
                }
            }
            else if (!isFrame && getFramelessAdjustResult != AnalyzeResult.Success)
                return getFramelessAdjustResult;//ずれ調整その他の失敗

            //引数・局所変数検出
            var vars = codeBlock.Values.SelectMany(codes =>
                codes.Select(code => code.TryGetParamaterOPIndex((REG)FP, out int index) ? code.op[index].SignedOpconst : 0)
            ).Where(t => t != 0).Distinct().ToArray();

            //引数・局所変数のサイズ構築
            Dictionary<int, (int num, uint size)> argList = new Dictionary<int, (int num, uint size)>();
            Dictionary<int, (int num, uint size)> lvarList = new Dictionary<int, (int num, uint size)>();
            int argSize = localSize = 0;
            foreach (var arg in vars.Where(t => t > 0).OrderBy(t => t).Select((v, i) => new { v, i }))
            {
                argList[arg.v] = (arg.i+1, (uint)(arg.v - argSize));
                argSize = arg.v;
            }
            foreach (var lvar in vars.Where(t => t < 0).OrderByDescending(t => t).Select((v, i) => new { v, i }))
            {
                lvarList[lvar.v] = (-lvar.i-1, (uint)(localSize - lvar.v));
                localSize = lvar.v;
            }

            //オペランド変更
            uint currentIP;
            Stack<uint> workList = new Stack<uint>();
            List<uint> workedIP = new List<uint>();
            workList.Push(startIP);

            while (workList.Count > 0)
            {
                currentIP = workList.Pop();
                workedIP.Add(currentIP);

                codeBlock[currentIP] = codeBlock[currentIP].Select(code =>
                {
                    if (code.TryGetParamaterOPIndex((REG)FP, out int index))
                    {
                        //取得
                        int num;uint size;
                        if (code.op[index].SignedOpconst > 0) (num, size) = argList[code.op[index].SignedOpconst];
                        else (num, size) = lvarList[code.op[index].SignedOpconst];

                        //変更
                        var oldGran = code.op[index].granularity;
                        code.op[index] = t_operand.New();
                        code.op[index].arg = num > 0 ? B.ARG : B.LVAR;
                        code.op[index].varnum = num;
                        code.op[index].opsize = size;
                        code.op[index].granularity = oldGran;
                    }
                    return code;
                }).ToList();

                foreach (var succ in successors[currentIP].Where(t => !workedIP.Contains(t) && !workList.Contains(t)))
                {
                    workList.Push(succ);
                }
            }

            return AnalyzeResult.Success;
        }

        /// <summary>
        /// フレームレス関数のパラメータずれ調整
        /// </summary>
        /// <param name="codeBlock"></param>
        /// <param name="successors"></param>
        /// <param name="startIP"></param>
        static AnalyzeResult AdjustFramelessParamater(Dictionary<uint, List<t_disasm>> codeBlock, Dictionary<uint, uint[]> successors, uint startIP, out int localSize)
        {
            localSize = 0;
            bool getLocalSize = false;
            int sp_adjust = 0;
            uint currentIP;
            Stack<(uint ip, int adjust)> workList = new Stack<(uint ip, int adjust)>();
            Dictionary<uint, int> adjustDic = new Dictionary<uint, int>();
            List<uint> removeLocalSizeAddSubIP = new List<uint>();//ローカルサイズ取得・調整命令削除用
            workList.Push((startIP, 0));

            while (workList.Count > 0)
            {
                //新規ブロック
                (currentIP, sp_adjust) = workList.Pop();

                //調整値チェック
                if (adjustDic.TryGetValue(currentIP, out int oldAdjust))
                {
                    //exitBlockでパラメータコードなしは除外
                    if (sp_adjust == oldAdjust ||
                        successors[currentIP].Length == 0 &&
                        codeBlock[currentIP].All(t => !t.TryGetParamaterOPIndex(REG.ESP, out var _))) continue;
                    return AnalyzeResult.AdjustFramelessParamaterFailed;
                }
                else if (currentIP != startIP)
                {
                    adjustDic[currentIP] = sp_adjust;
                }

                //調整
                foreach(var code in codeBlock[currentIP])
                {
                    switch (code.cmdtypeKind)
                    {
                        case D.PUSH:
                            sp_adjust += (int)code.op[0].opsize;
                            continue;
                        case D.POP:
                            sp_adjust -= (int)code.op[0].opsize;
                            continue;
                        default:
                            //パラメータの定数取得
                            if (!code.TryGetParamaterOPIndex(REG.ESP, out int index))
                                break;

                            //調整
                            code.op[index].opconst = (uint)(code.op[index].SignedOpconst - localSize - sp_adjust);
                            code.op[index].opconstsize = 4;
                            if (code.op[index].SignedOpconst == 0)
                                throw new ArgumentOutOfRangeException("パラメータがゼロです");
                            break;
                    }
                    switch (code.exttype & DX.TYPEMASK)
                    {
                        case DX.ADD:
                            if (code.TryGetREG_CONST(REG.ESP, out var addop))
                            {
                                sp_adjust += addop.SignedOpconst;
                                removeLocalSizeAddSubIP.Add(code.ip);
                            }
                            break;
                        case DX.SUB:
                            if (code.TryGetREG_CONST(REG.ESP, out var subop))
                            {
                                if (currentIP == startIP)
                                {   //ローカルサイズ取得
                                    localSize = (int)subop.opconst;
                                    getLocalSize = true;
                                }
                                else
                                {
                                    sp_adjust -= subop.SignedOpconst;
                                }
                                removeLocalSizeAddSubIP.Add(code.ip);
                            }
                            break;
                    }
                }

                //命令削除
                if (removeLocalSizeAddSubIP.Count > 0)
                {
                    codeBlock[currentIP].RemoveAll(t => removeLocalSizeAddSubIP.Contains(t.ip));
                    removeLocalSizeAddSubIP.Clear();
                }

                //push succ
                foreach(var succ in successors[currentIP])
                {
                    workList.Push((succ, sp_adjust));
                }
            }

            //ローカルサイズ取得失敗考慮
            if (getLocalSize) return AnalyzeResult.Success;
            else return AnalyzeResult.DetectLocalSizeFailed;
        }

        /// <summary>
        /// フレーム関数のローカル変数サイズ取得
        /// </summary>
        /// <param name="codeBlock"></param>
        /// <param name="entryIP"></param>
        /// <param name="exitIPs"></param>
        /// <returns></returns>
        static bool TryGetLocalSizeWithFrame(List<t_disasm> entryCodes, Dictionary<uint, List<t_disasm>> exitCodes, out int localSize)
        {
            //検出
            int subESPIndex = entryCodes.FindIndex(t => t.cmdname == "SUB" && t.op[0].reg == REG.ESP && t.op[1].features == OP.CONST);
            if (subESPIndex < 0)
            {
                localSize = default(int);
                return false;
            }
            uint uLocalSize = entryCodes[subESPIndex].op[1].opconst;

            //削除
            entryCodes.RemoveAt(subESPIndex);
            foreach (var exitIP in exitCodes.Keys)
            {
                var block = exitCodes[exitIP];
                int addIndex = block.FindLastIndex(t => t.cmdname == "ADD" && t.op[0].reg == REG.ESP && t.op[1].opconst == uLocalSize);
                if (addIndex >= 0)//leave考慮
                    block.RemoveAt(addIndex);
            }

            localSize = (int)uLocalSize;
            return true;
        }

        /// <summary>
        /// フレーム削除
        /// </summary>
        /// <param name="codeBlock"></param>
        /// <param name="entryIP"></param>
        /// <param name="exitIPs"></param>
        /// <returns></returns>
        static bool TryRemoveFrame(List<t_disasm> entryCodes, Dictionary<uint, List<t_disasm>> exitCodes)
        {
            //PUSH POP EBPチェック
            if (!TryGetTopAndBottoms(t => t.cmdname == "ENTER" || t.cmdtypeKind == D.PUSH && t.op[0].reg == REG.EBP,
                t => t.cmdname == "LEAVE" || t.cmdtypeKind == D.POP && t.op[0].reg == REG.EBP, out var enterPOPIndex, out var exitPUSHIndexes))
                return false;

            //enter leaveチェック
            bool isEnter = entryCodes[enterPOPIndex].cmdname == "ENTER";
            uint[] LeaveIPs = exitPUSHIndexes.Where(t => exitCodes[t.Key][t.Value].cmdname == "LEAVE").Select(t => t.Key).ToArray();

            //削除
            RemoveTopAndBottoms(enterPOPIndex, exitPUSHIndexes);

            //MOVチェック
            if (!TryGetTopAndBottoms(t => t.cmdtypeKind == D.MOV && t.op[0].reg == REG.EBP && t.op[1].reg == REG.ESP,
                t => t.cmdtypeKind == D.MOV && t.op[0].reg == REG.ESP && t.op[1].reg == REG.EBP, out var enterMOVIndex, out var exitMOVIndexes,
                !isEnter, LeaveIPs.Length > 0 ? LeaveIPs : null))
                return false;

            //削除
            RemoveTopAndBottoms(enterMOVIndex, exitMOVIndexes);

            return true;

            //entryとexitで含まれるコマンド取得試行
            bool TryGetTopAndBottoms(Predicate<t_disasm> enterFilter, Predicate<t_disasm> exitFilter,
                out int enterCodeIndex, out Dictionary<uint, int> exitCodeIndexes, bool isCheckEnter=true, uint[] uncheckIPs =null)
            {
                //entryチェック
                if (isCheckEnter) {
                    enterCodeIndex = entryCodes.FindIndex(enterFilter);;
                    if (enterCodeIndex < 0)
                    {
                        exitCodeIndexes = null;
                        return false;
                    }
                }
                else enterCodeIndex = Int32.MaxValue;

                //exitチェック
                var exitIndexes = exitCodes.Keys.Select(blockIP =>
                   {
                       if (uncheckIPs?.Contains(blockIP) ?? false) return new { blockIP = blockIP, targetIndex = Int32.MaxValue };
                       int targetIndex = exitCodes[blockIP].FindLastIndex(exitFilter);
                       return new { blockIP = blockIP, targetIndex = targetIndex };
                   }).ToArray();
                if (exitIndexes.Any(t => t.targetIndex < 0))
                {
                    exitCodeIndexes = null;
                    return false;
                }
                exitCodeIndexes = exitIndexes.ToDictionary(t => t.blockIP, t => t.targetIndex);
                return true;
            }

            //enterとexitで含まれるコマンドを探して削除
            void RemoveTopAndBottoms(int enterCodeIndex, Dictionary<uint, int> exitCodeIndexes)
            {
                //entry削除
                if (enterCodeIndex < Int32.MaxValue)
                    entryCodes.RemoveAt(enterCodeIndex);

                //exit削除
                foreach (var removeBlockIP in exitCodeIndexes.Keys)
                {
                    int removeIndex = exitCodeIndexes[removeBlockIP];
                    if (removeIndex == Int32.MaxValue) continue;
                    exitCodes[removeBlockIP].RemoveAt(removeIndex);
                }
            }
        }
    }
}
