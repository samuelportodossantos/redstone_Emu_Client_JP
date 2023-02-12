using Anantashesha.Decompiler.Disassemble;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Anantashesha.Decompiler.Disassemble.Disassembler;
using static Anantashesha.Decompiler.ProcedureAnalyzers.Procedure;

namespace Anantashesha.Decompiler.ProcedureAnalyzers
{
    class BasicBlock
    {
        /// <summary>
        /// 最初のIP
        /// </summary>
        public uint HeaderIP;

        /// <summary>
        /// 種類
        /// </summary>
        public BlockType Type;

        /// <summary>
        /// 前任ブロックリスト
        /// </summary>
        public uint[] Predecessors;

        /// <summary>
        /// 後任ブロックリスト
        /// </summary>
        public uint[] Successors;

        /// <summary>
        /// コード
        /// </summary>
        public List<t_disasm> Codes;

        //caseの値
        public Dictionary<uint, int[]> CaseList;

        public enum BlockType
        {
            None, IfGoto, Goto, GotoFailed, Retn, Switch
        }

        private BasicBlock(BlockType type, List<t_disasm> codes, uint[] succs = null)
        {
            HeaderIP = codes.Min(t => t.ip);
            Type = type;
            Codes = new List<t_disasm>(codes);
            Successors = succs ?? new uint[0];
        }

        /// <summary>
        /// 作成
        /// </summary>
        /// <param name="disasm"></param>
        /// <param name="startIP"></param>
        /// <returns></returns>
        public static Dictionary<uint, BasicBlock> CreateBasicBlocks(SimpleDisassembler disasm, uint startIP)
        {
            //結果
            Dictionary<uint, BasicBlock> result;

            //スイッチ文のアドレス
            Dictionary<uint, uint[]> switchAddr = new Dictionary<uint, uint[]>();
            Dictionary<uint, int[]> caseList = new Dictionary<uint, int[]>();
            int swithAddrCount = 0;

            //disasm保存
            disasm.StateSave();

            //成功するまでループ
            if (startIP == 0x546010)
            {

            }
            while (!TryCreateBasicBlocks(disasm, startIP, out result, switchAddr))
            {
                if (swithAddrCount != 0)
                {

                }
                if (!TryAnalyze(result, startIP, out var tmpAnalyzed, caseList))//解析
                    throw new InvalidOperationException();
                (switchAddr, caseList) = ValueRange.GetSwitchLabels(tmpAnalyzed, disasm);
                if (switchAddr.Count == swithAddrCount)//変化なし
                    throw new ArgumentException();
                else
                    swithAddrCount = switchAddr.Count;
            }

            //disasm回復
            disasm.StateLoad();

            return result;
        }

        /// <summary>
        /// ベーシックブロック作成試行
        /// 戻り値はswitch先検出成功
        /// </summary>
        /// <param name="disasm"></param>
        /// <param name="startIP"></param>
        /// <param name="blockList"></param>
        /// <param name="switchAddr"></param>
        /// <returns></returns>
        public static bool TryCreateBasicBlocks(SimpleDisassembler disasm, uint startIP, out Dictionary<uint, BasicBlock> blockList, Dictionary<uint, uint[]> switchAddr)
        {
            //結果
            bool successSwitch = true;

            //ブロックリスト
            blockList = new Dictionary<uint, BasicBlock>();

            //ラベル
            List<uint> labels = new List<uint>();
            
            //disasm保存
            disasm.StateSave();

            //スキップなし
            disasm.UseSkip = false;

            //ph1. ラベルをスキャン・ラベル数の変化がなくなるまでループ
            int labelCount = -1;
            while (labels.Count != labelCount)
            {
                disasm.CurrentIP = startIP;
                labelCount = labels.Count;
                foreach (var code in disasm)
                {
                    switch (code.cmdtypeKind)
                    {
                        case D.JMC://jmc記録
                            if (!disasm.MemoryOrReg(code.op[0]))
                                labels.Add(code.op[0].opconst);
                            break;
                        case D.JMP:
                            if (disasm.IsSwitchCaseLikeBlock(out uint dataAddr))//switch-case
                            {
                                if (switchAddr.TryGetValue(code.ip, out var addrs)) labels.AddRange(addrs);
                                else successSwitch = false;
                            }
                            else if (!disasm.MemoryOrReg(code.op[0]))
                            {
                                labels.Add(code.op[0].opconst);
                            }
                            goto case D.RET;
                        case D.INT:
                        case D.RET://最も近くの後方に移動
                            uint jmpTo = labels.Where(t => t > code.ip).OrderBy(t => t - code.ip).FirstOrDefault();
                            if (jmpTo == 0) goto endscan;
                            else disasm.CurrentIP = jmpTo;
                            break;
                    }
                }
                endscan:
                labels = labels.Distinct().ToList();
            }

            //ph2. ブロック作成
            Stack<uint> workList = new Stack<uint>();
            Dictionary<uint, List<uint>> predList = new Dictionary<uint, List<uint>>();
            BasicBlock block = default;
            List<t_disasm> codes = new List<t_disasm>();
            workList.Push(startIP);
            while (workList.Count > 0)
            {
                disasm.CurrentIP = workList.Pop();
                foreach (var code in disasm)
                {
                    if (labels.Contains(code.ip) && codes.Count > 0)
                    {
                        block = new BasicBlock(BlockType.None, codes, new uint[] { code.ip });
                        goto endBlock;
                    }
                    codes.Add(code);
                    switch (code.cmdtypeKind)
                    {
                        case D.JMC:
                            if (disasm.MemoryOrReg(code.op[0])) break;
                            block = new BasicBlock(BlockType.IfGoto, codes, new uint[] { code.op[0].opconst, code.ip + code.size });
                            goto endBlock;
                        case D.JMP:
                            if (disasm.IsSwitchCaseLikeBlock(out uint dataAddr))//switch-case
                            {
                                if (!switchAddr.TryGetValue(code.ip, out var switchSuccs)) successSwitch = false;
                                block = new BasicBlock(BlockType.Switch, codes, switchSuccs);
                            }
                            else if (disasm.MemoryOrReg(code.op[0])) break;
                            else block = new BasicBlock(BlockType.Goto, codes, new uint[] { code.op[0].opconst });
                            goto endBlock;
                        case D.INT:
                        case D.RET:
                            block = new BasicBlock(BlockType.Retn, codes);
                            goto endBlock;
                    }
                }
                endBlock:;
                blockList[block.HeaderIP] = block;
                foreach (var succ in block.Successors)
                {
                    //pred
                    if (!predList.ContainsKey(succ)) predList[succ] = new List<uint> { block.HeaderIP };
                    else if (!predList[succ].Contains(block.HeaderIP)) predList[succ].Add(block.HeaderIP);
                    //push
                    if (!blockList.ContainsKey(succ) && !workList.Contains(succ)) workList.Push(succ);
                }
                codes.Clear();
            }

            //ph3. pred付与
            foreach (var blockIp in blockList.Keys)
            {
                blockList[blockIp].Predecessors = predList.TryGetValue(blockIp, out var preds) ? preds.ToArray() : new uint[0];
            }
            blockList = blockList.OrderBy(t => t.Key).ToDictionary(t => t.Key, t => t.Value);
            
            //disasm回復
            disasm.StateLoad();

            return successSwitch;
        }
    }
}
