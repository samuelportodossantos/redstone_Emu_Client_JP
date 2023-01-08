using Anantashesha.Decompiler.ProcedureAnalyzers.DataFlowAnalyzers;
using Anantashesha.Decompiler.Disassemble;
using RedStoneLib.Packets;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using static Anantashesha.Decompiler.Disassemble.Disassembler;

namespace Anantashesha.Decompiler.ProcedureAnalyzers
{
    partial class Procedure
    {

        /// <summary>
        /// 解析結果のコード
        /// </summary>
        public IDisasm[] Codes;

        /// <summary>
        /// 基本ブロック
        /// </summary>
        public Dictionary<uint, BasicBlock> BasicBlocks;

        /// <summary>
        /// 入り口
        /// </summary>
        public uint EntryIP;

        /// <summary>
        /// フレーム関数フラグ
        /// </summary>
        public bool IsFrame;

        /// <summary>
        /// ローカル変数のサイズ
        /// </summary>
        public int SizeOfLocals;

        public static Procedure CreateProcedure(SimpleDisassembler disasm, uint entryIP)
        {
            //disasm保存
            disasm.StateSave();

            //proc本体
            Procedure proc = new Procedure { EntryIP = entryIP };

            //スイッチ文のアドレス
            Dictionary<uint, uint[]> switchAddr = new Dictionary<uint, uint[]>();
            Dictionary<uint, int[]> caseList = new Dictionary<uint, int[]>();
            int swithAddrCount = 0;
            
            //成功するまでループ
            while (!BasicBlock.TryCreateBasicBlocks(disasm, entryIP, out proc.BasicBlocks, switchAddr))
            {
                if (swithAddrCount != 0)
                {

                }
                if (!TryAnalyze(proc.BasicBlocks, entryIP, out var tmpAnalyzed, caseList))//解析
                    throw new InvalidOperationException();
                (switchAddr, caseList) = ValueRange.GetSwitchLabels(tmpAnalyzed, disasm);
                if (switchAddr.Count == swithAddrCount)//変化なし
                    throw new ArgumentException();
                else
                    swithAddrCount = switchAddr.Count;
            }
            if (!TryAnalyze(proc.BasicBlocks, entryIP, out proc.Codes, caseList))
                throw new InvalidOperationException();
            
            //disasm回復
            disasm.StateLoad();

            return proc;
        }

        /// <summary>
        /// 解析を試行
        /// </summary>
        /// <param name="blocks"></param>
        /// <param name="startIP"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        public static bool TryAnalyze(Dictionary<uint, BasicBlock> blocks, uint startIP, out IDisasm[] result, Dictionary<uint, int[]> caseList)
        {
            //基本ブロックをコードのみに変更
            var codeBlock = blocks.ToDictionary(t => t.Key, t => t.Value.Codes.ToList());
            var successors = blocks.ToDictionary(t => t.Key, t => t.Value.Successors);
            var predecessors = blocks.ToDictionary(t => t.Key, t => t.Value.Predecessors);

            if (FrameAnalyzer.AnalyzeLocalParamater(codeBlock, successors, startIP) != FrameAnalyzer.AnalyzeResult.Success)
            {
                //ローカルパラメータ解析失敗
                result = null;
                return false;
            }
            AdjustHeaderAndSuccessors(codeBlock, successors, predecessors, ref startIP);

            //simple Code化
            Dictionary<uint, List<SimpleCode>> simpleBlock = SimpleCode.CreateCodes(codeBlock);
            AdjustHeaderAndSuccessors(simpleBlock, successors, predecessors, ref startIP);

            //式伝搬
            ExpressionsPropagation.Analyze(simpleBlock, successors, startIP);
            AdjustHeaderAndSuccessors(codeBlock, successors, predecessors, ref startIP);

            //ステートメント
            var st = new StatementAnalyzer(simpleBlock, successors, predecessors, caseList, startIP);
            result = st.Analyze();
            return true;
        }

        /// <summary>
        /// HeaderとSuccessors調整
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="codeBlock"></param>
        /// <param name="successors"></param>
        static void AdjustHeaderAndSuccessors<T>(Dictionary<uint, List<T>> codeBlock,
            Dictionary<uint, uint[]> successors, Dictionary<uint, uint[]> predecessors, ref uint startIP)
            where T : IDisasm
        {
            //旧IP：新IP
            Dictionary<uint, uint> changeHeaderList = new Dictionary<uint, uint>();
            foreach (var headerIP in codeBlock.Keys)
            {
                uint realHeaderIP = codeBlock[headerIP].First().ip;
                if (realHeaderIP != headerIP) changeHeaderList[headerIP] = realHeaderIP;
            }
            foreach (var changeHeaderIP in changeHeaderList.Keys)
            {
                if (!codeBlock.ContainsKey(changeHeaderIP)) continue;

                //header succ更新
                var newHeaderIP = changeHeaderList[changeHeaderIP];
                var tmpCodes = codeBlock[changeHeaderIP];
                var tmpSucc = successors[changeHeaderIP];
                var tmpPred = predecessors[changeHeaderIP];
                codeBlock.Remove(changeHeaderIP);
                successors.Remove(changeHeaderIP);
                predecessors.Remove(changeHeaderIP);
                codeBlock[newHeaderIP] = tmpCodes;
                successors[newHeaderIP] = tmpSucc;
                predecessors[newHeaderIP] = tmpPred;

                //startIP
                if (changeHeaderIP == startIP)
                {
                    startIP = newHeaderIP;
                }

                //succの中身更新
                foreach (var succsIP in successors.Keys)
                {
                    int index = Array.FindIndex(successors[succsIP], t => t == changeHeaderIP);
                    if (index >= 0) successors[succsIP][index] = newHeaderIP;
                }
                //predの中身更新
                foreach (var predIP in predecessors.Keys)
                {
                    int index = Array.FindIndex(predecessors[predIP], t => t == changeHeaderIP);
                    if (index >= 0) predecessors[predIP][index] = newHeaderIP;
                }
            }
        }

        /// <summary>
        /// プロシージャ取得
        /// </summary>
        /// <param name="fname"></param>
        public unsafe static void ProcedureFinder(string fname)
        {
            Dictionary<uint, Procedure> Procedures = new Dictionary<uint, Procedure>();
            using (SimpleDisassembler disasm = new SimpleDisassembler(fname))
            {
                Stack<uint> callees = new Stack<uint>();

                //呼び出し純粋チェック
                bool legalCall(t_disasm code, out uint entryIP)
                {
                    if (code.cmdtypeKind == D.CALL &&
                        (code.op[0].features & OP.MEMORY) == 0 &&
                        (code.op[0].features & OP.REGISTER) == 0)
                    {
                        uint eip = code.op[0].opconst;
                        if (!Procedures.ContainsKey(eip) && !callees.Contains(eip))
                        {
                            entryIP = eip;
                            return true;
                        }
                    }
                    entryIP = default(uint);
                    return false;
                }

                disasm.CurrentIP = 0x4012DC;

                //コードを上からすべて見る
                foreach (var code in disasm)
                {
                    if ((code.exttype & DX.NOP) == 0)
                    {
                        disasm.Pause();
                        callees.Push(code.ip);
                        while (callees.Count > 0)
                        {
                            uint entryIP = callees.Pop();

                            //proc作成
                            var proc = CreateProcedure(disasm, entryIP);

                            //今より下のIPのみskipList追加
                            if (entryIP >= code.ip)
                                disasm.TakeInSkipList(proc.BasicBlocks.ToDictionary(t => t.Key, t => t.Value.Codes.ToArray()));

                            Procedures[entryIP] = proc;

                            //関数内の呼び出し調査
                            foreach (var procInCode in proc.BasicBlocks.Values.SelectMany(t => t.Codes))
                            {
                                if (legalCall(procInCode, out var procInEntryIP)) callees.Push(procInEntryIP);
                            }
                        }
                    }
                }
            }
        }
    }
}
