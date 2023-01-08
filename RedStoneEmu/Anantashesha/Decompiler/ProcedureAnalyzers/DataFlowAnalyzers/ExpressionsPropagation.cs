using Anantashesha.Decompiler.ProcedureAnalyzers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Anantashesha.Decompiler.Disassemble.Disassembler;

namespace Anantashesha.Decompiler.ProcedureAnalyzers.DataFlowAnalyzers
{
    class ExpressionsPropagation
    {
        public static void Analyze(Dictionary<uint, List<SimpleCode>> blocks, Dictionary<uint, uint[]> successors, uint startIP)
        {
            //データ表現グラフ
            var liveOut = DataRepresentation.ComputeFromCFG(blocks, successors);

            //ブロックごとのREG内部
            var savedRegs = blocks.ToDictionary(t => t.Key, _ => new IData[NREG]);
            //regのblockとcode ip
            var savedPos = blocks.ToDictionary(t => t.Key, _ => new(uint blockIP, uint codeIP)[NREG]);

            //thisセット
            savedRegs[startIP][(int)REG.ECX] = new SimpleData(SimpleData.DataOP.This);

            Stack<uint> workList = new Stack<uint>();
            List<uint> workedList = new List<uint>();
            uint currentIP;
            workList.Push(startIP);

            List<int> usedRegList = new List<int>();
            while (workList.Count > 0)
            {
                currentIP = workList.Pop();
                workedList.Add(currentIP);

                foreach (var code in blocks[currentIP].ToArray())
                {
                    if (code.OPs.Length == 0) continue;

                    IData tmpSrc = code.OPs.Length >= 2 ? code.OP2 : null;
                    for (int i = 1; i < code.OPs.Length; i++)
                    {
                        //被代入オペランドを先に伝搬
                        code.OPs[i] = replaceUses(code.OPs[i], code.ip);
                    }
                    if (code.Cmd == SimpleCode.CodeCmd.Assign && code.OP1 is SimpleData dst)
                    {
                        switch (dst.Type)
                        {
                            case SimpleData.DataOP.Register:
                                //戻る前に削除
                                removeSavedRegs(code.ip);

                                //残ってたら削除
                                if (savedRegs[currentIP][(int)dst.Reg] != null)
                                {
                                    usedRegList.Add((int)dst.Reg);
                                    removeSavedRegs(code.ip, true);
                                }

                                //代入するOP記憶
                                savedRegs[currentIP][(int)dst.Reg] = code.OP2;
                                savedPos[currentIP][(int)dst.Reg] = (currentIP, code.ip);
                                continue;
                            case SimpleData.DataOP.Argment:
                            case SimpleData.DataOP.LocalVar:
                                if (tmpSrc is SimpleData src && src.Type == SimpleData.DataOP.Register && savedRegs[currentIP][(int)src.Reg] != null)
                                {
                                    savedRegs[currentIP][(int)src.Reg] = dst;
                                }
                                continue;
                        }
                    }

                    //1OP目が非代入の伝搬
                    code.OPs[0] = replaceUses(code.OPs[0], code.ip);
                    //IF文の中身
                    if (code.Expression != null)
                    {
                        code.Expression.SetOP(replaceUses(code.Expression.OP1, code.ip), replaceUses(code.Expression.OP2, code.ip));
                    }
                    removeSavedRegs(code.ip);
                }

                //伝搬
                foreach (var succ in successors[currentIP].Where(t => !workList.Contains(t) && !workedList.Contains(t)))
                {
                    workList.Push(succ);
                    savedRegs[succ] = (IData[])savedRegs[currentIP].Clone();
                    savedPos[succ] = ((uint, uint)[])savedPos[currentIP].Clone();
                }

                //保存されたレジスタの削除
                void removeSavedRegs(uint codeip, bool force=false)
                {
                    foreach (var reg in usedRegList.Distinct())
                    {
                        //使用した非活性を除去
                        if (!liveOut[codeip][reg] || force)
                        {
                            if (reg == (int)REG.ECX && savedRegs[currentIP][reg] is SimpleData sdata && sdata.Type == SimpleData.DataOP.This)
                            {
                                //thisは元命令削除しない
                                savedRegs[currentIP][reg] = null;
                                continue;
                            }
                            savedRegs[currentIP][reg] = null;
                            (var removeBlockIP, var removeCodeIP) = savedPos[currentIP][reg];
                            blocks[removeBlockIP].RemoveAll(t => t.ip == removeCodeIP);
                            savedPos[currentIP][reg] = default;
                        }
                    }
                    usedRegList.Clear();
                }

                //記録したop返す
                IData replaceUses(IData data, uint codeip)
                {
                    if (data == null) return null;
                    switch (data)
                    {
                        case SimpleCode scode:
                            for (int i = 0; i < scode.OPs.Length; i++)
                                scode.OPs[i] = replaceUses(scode.OPs[i], codeip);
                            if (scode.Expression != null)//IF文
                            {
                                scode.Expression.SetOP(replaceUses(scode.Expression.OP1, codeip), replaceUses(scode.Expression.OP2, codeip));
                            }
                            break;
                        case SimpleData sdata:
                            if (sdata.Type == SimpleData.DataOP.Register && savedRegs[currentIP][(int)sdata.Reg] != null)
                            {
                                int reg = (int)sdata.Reg;
                                usedRegList.Add(reg);
                                data = savedRegs[currentIP][reg];
                            }
                            break;
                    }
                    return data;
                }
            }


        }

        /// <summary>
        /// データ表現
        /// </summary>
        class DataRepresentation
        {
            /// <summary>
            /// 命令で定義されたレジスタ
            /// </summary>
            byte Defs = 0;

            /// <summary>
            /// 命令で使用されたレジスタ
            /// </summary>
            byte Use = 0;

            /// <summary>
            /// 命令を実行する前に生きている変数のセット
            /// </summary>
            byte LiveIn = 0;

            /// <summary>
            /// 命令を実行したあとに生きている変数のセット
            /// </summary>
            byte LiveOut = 0;

            public bool this[int index] 
                => Get(LiveOut, index);

            public override string ToString()
                    => Helper.BitArrayToRegStr(("D", Defs), ("U", Use), ("I", LiveIn), ("O", LiveOut));

            private static bool Get(byte b, int index)
                => ((b >> index) & 1) == 1;

            private static void Set(ref byte b, int index, bool set = true)
            {
                if (set) b |= (byte)(1 << index);
                else b &= (byte)(~(1 << index));
            }

            private static void Set(ref byte b, t_operand op, bool set = true)
                => Set(ref b, (int)op.reg, set);

            /// <summary>
            /// データフロー分析のヘルパー
            /// </summary>
            private class Helper
            {
                public byte Input = 0;
                public byte Output = 0;
                public byte Dead = 0;

                public static string BitArrayToRegStr(params (string name, byte ba)[] objects)
                {
                    return string.Join(",", innerProc());
                    IEnumerable<string> innerProc()
                    {
                        foreach (var obj in objects)
                        {
                            if (Enumerable.Range(0, NREG).All(i => !Get(obj.ba, i))) continue;
                            yield return $"{obj.name}:{string.Join(",", Enumerable.Range(0, NREG).Where(t => Get(obj.ba, t)).Select(t => REGNAME[t]))}";
                        }
                    }
                }

                public override string ToString()
                    => BitArrayToRegStr(("I", Input), ("O", Output), ("D", Dead));
            }

            /// <summary>
            /// Compute LiveIn and LiveOut
            /// </summary>
            /// <param name="blocks"></param>
            /// <returns></returns>
            public static Dictionary<uint, DataRepresentation> ComputeFromCFG(Dictionary<uint, List<SimpleCode>> blocks, Dictionary<uint, uint[]> successors)
            {
                //計算補助用
                var helper = blocks.Values.SelectMany(t => t.Select(c => c.ip)).ToDictionary(t => t, _ => new Helper());

                Dictionary<uint, DataRepresentation> result = new Dictionary<uint, DataRepresentation>();

                //ph0. 各命令にのみ依存するデータ表現（use, defs）を計算
                foreach (var entryIP in blocks.Keys)
                {
                    foreach (var code in blocks[entryIP])
                    {
                        DataRepresentation dr = new DataRepresentation();

                        for (int i = 0; i < code.OPs.Length; i++)
                        {
                            if (i == 0 && code.Cmd == SimpleCode.CodeCmd.Assign &&
                                code.OP1 is SimpleData sdata && sdata.Type == SimpleData.DataOP.Register)
                            {
                                //op1 && assign
                                Set(ref dr.Defs, (int)sdata.Reg);
                            }
                            else
                            {
                                dr.Use |= GetUses(code.OPs[i]);
                            }
                        }
                        if (code.Expression != null)
                        {
                            dr.Use |= GetUses(code.Expression.OP1);
                            dr.Use |= GetUses(code.Expression.OP2);
                        }
                        result[code.ip] = dr;
                    }
                }

                //ph1. 各基本ブロックの生存状況計算
                var blockResult = blocks.Keys.ToDictionary(t => t, _ => new DataRepresentation());
                foreach (var blockIndex in blocks.Keys)
                {
                    foreach (var i in blocks[blockIndex].Select(t=>t.ip).OrderByDescending(t=>t))
                    {
                        //使用regは実行後も生きている
                        result[i].LiveOut = result[i].Use;

                        //定義後に生きていないregは死んでる
                        helper[i].Dead = (byte)(result[i].Defs & (~result[i].LiveOut));

                        //ブロックで使用されていて現在死んでないreg，もしくは現在実行後生きているregはブロックで使用されている
                        blockResult[blockIndex].Use = (byte)(result[i].LiveOut | (blockResult[blockIndex].Use & (~helper[i].Dead)));

                        //ブロックで定義されていて現在実行後生きてないreg，もしくは現在死んでるregはブロックで定義されている
                        blockResult[blockIndex].Defs = (byte)(helper[i].Dead | (blockResult[blockIndex].Defs & (~result[i].LiveOut)));
                    }
                }

                //ph2. 各ブロックの生存情報をCFG内の他のすべてのブロックに伝播することにより、各ブロックの情報を処理
                bool changed;
                do
                {
                    changed = false;
                    foreach(var blockIndex in blocks.Keys)
                    {
                        var dr = blockResult[blockIndex];
                        var hlp = helper[blockIndex];
                        var @out = dr.Defs;
                        foreach(var succ in successors[blockIndex])
                        {
                            @out |= helper[succ].Input;
                        }
                        var @in = (byte)(dr.Use | (@out & (~dr.Defs)));

                        if (@in != hlp.Input || @out != hlp.Output)
                        {
                            changed = true;
                            hlp.Input = @in;
                            hlp.Output = @out;
                        }
                    }
                } while (changed);

                //ph3. 他のブロックから収集された情報を各ブロックに存在する各命令に追加
                foreach(var blockIndex in blocks.Keys)
                {
                    byte live = 0;
                    foreach(var succ in successors[blockIndex])
                    {
                        live |= helper[succ].Input;
                    }

                    foreach (var i in blocks[blockIndex].Select(t => t.ip).OrderByDescending(t => t))
                    {
                        var oldLive = result[i].LiveOut;
                        result[i].LiveOut = live;
                        live = (byte)(oldLive | (live & (~helper[i].Dead)));
                    }
                }
                return result;
            }

            static byte GetUses(IData code)
            {
                if (code == null) return 0;
                byte result = 0;
                switch (code)
                {
                    case SimpleCode scode:
                        foreach (var op in scode.OPs)
                            result |= GetUses(op);
                        if (scode.Expression != null)
                        {
                            result |= GetUses(scode.Expression.OP1);
                            result |= GetUses(scode.Expression.OP2);
                        }
                        break;
                    case SimpleData sdata when sdata.Type == SimpleData.DataOP.Register:
                        Set(ref result, (int)sdata.Reg);
                        break;
                }
                return result;
            }
        }
    }
}
