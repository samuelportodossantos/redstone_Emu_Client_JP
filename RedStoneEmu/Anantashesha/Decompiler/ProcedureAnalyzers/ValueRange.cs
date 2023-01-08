using Anantashesha.Decompiler.Disassemble;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Anantashesha.Decompiler.Disassemble.Disassembler;
using static Anantashesha.Decompiler.ProcedureAnalyzers.SimpleData;

namespace Anantashesha.Decompiler.ProcedureAnalyzers
{
    /// <summary>
    /// 値が取りうる範囲
    /// </summary>
    class ValueRange
    {
        /// <summary>
        /// And Or考慮した範囲
        /// </summary>
        struct RangeAware
        {
            //型が取る範囲（この範囲は超えられない）
            long DataTypeMin;
            long DataTypeMax;

            //実態
            long _Min;
            long _Max;

            //Not
            List<long> Not;

            //Or範囲(Any or Not)
            List<long> OrMin;
            List<long> OrMax;
            List<long> OrNot;

            /// <summary>
            /// 最小値
            /// </summary>
            public long Min
            {
                get => _Min;
                private set => _Min = Math.Max(value, _Min);
            }

            /// <summary>
            /// 最大値
            /// </summary>
            public long Max
            {
                get => _Max;
                private set => _Max = Math.Min(value, _Max);
            }

            /// <summary>
            /// データアドレス取得
            /// </summary>
            /// <param name="pointer"></param>
            /// <param name="index"></param>
            /// <returns></returns>
            public IEnumerable<(uint Addr, int Case)> GetDataAddress(SimpleDisassembler disasm, SimpleCode pointer, IData index)
            {
                List<int> visited = new List<int>();
                var mynot = Not;
                for (long i = _Min, cnt=0; i < _Max + 1; i++, cnt++)
                {
                    if (cnt > 1000)
                        throw new ArgumentOutOfRangeException(pointer.ToString());
                    if (!pointer.TryAssignAndExecute(index, (int)i, out int result, GetPointerData)) break;

                    if (mynot.Contains(i))
                        continue;
                    if (visited.Contains(result))
                        if (visited.Max() > result) break;
                        else continue;
                    yield return ((uint)result, (int)i);
                    visited.Add(result);
                }

                //ポインタの値取得
                uint GetPointerData(uint ip, int size)
                {
                    switch (size)
                    {
                        case 4: return disasm.GetUInt32(ip);
                        case 2: return disasm.GetUInt16(ip);
                        case 1: return disasm.GetUInt8(ip);
                        default:
                            throw new NotImplementedException(nameof(size));
                    }
                }
            }
            
            /// <summary>
            /// 範囲追加
            /// </summary>
            /// <param name="expType"></param>
            /// <param name="listType"></param>
            /// <param name="value"></param>
            public void Add(SimpleExpression.ExpType expType, SDataSign exprSign, SimpleExpression.ExpressionListType listType, int value)
            {
                if (exprSign == SDataSign.Unsigned && DataTypeMin < 0)
                {
                    //型の符号更新
                    DataTypeMin = 0;
                    Min = 0;
                }
                switch (listType)
                {
                    case SimpleExpression.ExpressionListType.None:
                    case SimpleExpression.ExpressionListType.And:
                        switch (expType)
                        {
                            case SimpleExpression.ExpType.EQ:
                                if (value < DataTypeMin || value > DataTypeMax) break;
                                _Min = _Max = value;
                                break;
                            case SimpleExpression.ExpType.NEQ://not equalはcross
                                if (value < DataTypeMin || value > DataTypeMax) break;
                                Not.Add(value);
                                break;
                            case SimpleExpression.ExpType.GT:
                                Min = value + 1;
                                break;
                            case SimpleExpression.ExpType.GE:
                                Min = value;
                                break;
                            case SimpleExpression.ExpType.LT:
                                Max = value - 1;
                                break;
                            case SimpleExpression.ExpType.LE:
                                Max = value;
                                break;
                            default:
                                throw new NotImplementedException();
                        }
                        break;
                    case SimpleExpression.ExpressionListType.Or:
                        switch (expType)
                        {
                            case SimpleExpression.ExpType.EQ:
                                if (value < DataTypeMin || value > DataTypeMax) break;
                                OrMin.Add(value);
                                OrMax.Add(value);
                                break;
                            case SimpleExpression.ExpType.NEQ://not equalはcross
                                if (value < DataTypeMin || value > DataTypeMax) break;
                                OrNot.Add(value);
                                break;
                            case SimpleExpression.ExpType.GT:
                                if (value + 1 < DataTypeMin) break;
                                OrMin.Add(value + 1);
                                break;
                            case SimpleExpression.ExpType.GE:
                                if (value < DataTypeMin) break;
                                OrMin.Add(value);
                                break;
                            case SimpleExpression.ExpType.LT:
                                if (value - 1 > DataTypeMax) break;
                                OrMax.Add(value - 1);
                                break;
                            case SimpleExpression.ExpType.LE:
                                if (value > DataTypeMax) break;
                                OrMax.Add(value);
                                break;
                            default:
                                throw new NotImplementedException();
                        }
                        break;
                    default:
                        throw new NotImplementedException();
                }
            }

            /// <summary>
            /// 新規作成
            /// </summary>
            /// <param name="type"></param>
            /// <param name="sign"></param>
            /// <param name="expType"></param>
            /// <param name="listType"></param>
            /// <param name="value"></param>
            /// <returns></returns>
            public RangeAware(SDataType type, SDataSign sign, 
                SimpleExpression.ExpType expType, SimpleExpression.ExpressionListType listType, int value)
            {
                Not = new List<long>();
                OrMin = new List<long>();
                OrMax = new List<long>();
                OrNot = new List<long>();

                //型の範囲セット
                (DataTypeMin, DataTypeMax) = GetDataRange(type, sign);
                _Min = DataTypeMin;
                _Max = DataTypeMax;

                //初期範囲セット
                Add(expType, sign, listType, value);
            }
        }

        /// <summary>
        /// 範囲保存
        /// </summary>
        Dictionary<string, RangeAware> Ranges = new Dictionary<string, RangeAware>();

        /// <summary>
        /// コピー
        /// </summary>
        /// <returns></returns>
        private ValueRange(ValueRange src)
        {
            if (src != null)
            {
                Ranges = new Dictionary<string, RangeAware>(src.Ranges);
            }
        }

        /// <summary>
        /// 範囲をセット
        /// </summary>
        /// <param name="key"></param>
        /// <param name="expType"></param>
        /// <param name="listType"></param>
        /// <param name="value"></param>
        void SetRange(IData key, SDataSign exprSign, SimpleExpression.ExpType expType, SimpleExpression.ExpressionListType listType, int value)
        {
            string strKey = key.ToString();
            if (Ranges.TryGetValue(strKey, out var targetRange))
            {
                targetRange.Add(expType, exprSign, listType, value);
                Ranges[strKey] = targetRange;
            }
            else Ranges[strKey] = new RangeAware(key.DataType, exprSign, expType, listType, value);
        }

        /// <summary>
        /// 範囲取得
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        RangeAware GetRange(IData data)
            => Ranges[data.ToString()];

        /// <summary>
        /// 式を解きながらデータセット
        /// </summary>
        /// <param name="cmpee"></param>
        /// <param name="cmper"></param>
        /// <param name="setRange"></param>
        bool TakeInRangeWithOperation(IData cmpee, IData cmper, SDataSign exprSign, SimpleExpression.ExpType expType, SimpleExpression.ExpressionListType listType)
        {
            if (cmpee is SimpleCode scode && !cmpee.IsPointer)
            {
                if (scode.Cmd == SimpleCode.CodeCmd.And && cmper is SimpleData cmperData && cmperData.Type == DataOP.Const && cmperData.Opconst == 0 &&
                    scode.OP2 is SimpleData andSrc && andSrc.Type == DataOP.Const)
                {
                    //AND比較
                    if (expType == SimpleExpression.ExpType.NEQ)
                    {
                        //該当ビット以外をNOT
                        foreach(var not in Enumerable.Range(0, 0x10).Select(t => 1 << t).Where(t => (t & andSrc.Opconst) == 0))
                        {
                            TakeInRangeWithOperation(scode.OP1, new SimpleData(not), exprSign, SimpleExpression.ExpType.NEQ, listType);
                        }
                        int minimum = Enumerable.Range(0, andSrc.Opconst).Select(t => 1 << t).First(t => (andSrc.Opconst & t) != 0);
                        return TakeInRangeWithOperation(scode.OP1, new SimpleData(minimum), exprSign, SimpleExpression.ExpType.GE, listType);
                    }
                    else if (expType == SimpleExpression.ExpType.EQ)
                    {
                        //該当ビットをNOT
                        foreach (var not in Enumerable.Range(0, 0x10).Select(t => 1 << t).Where(t => (t & andSrc.Opconst) != 0))
                        {
                            TakeInRangeWithOperation(scode.OP1, new SimpleData(not), exprSign, SimpleExpression.ExpType.NEQ, listType);
                        }
                        if (scode.OP1.DataSign != SDataSign.Signed)
                        {
                            //nand
                            int minimum = Enumerable.Range(0, andSrc.Opconst).Select(t => 1 << t).First(t => (~andSrc.Opconst & t) != 0);
                            return TakeInRangeWithOperation(scode.OP1, new SimpleData(minimum), exprSign, SimpleExpression.ExpType.GE, listType);
                        }
                    }
                }
                else if (SimpleCode.TryGetNegateOperationCmd(scode.Cmd, out var negateCmd) &&
                    new SimpleCode(negateCmd, SimpleCode.CodeOP.Operation, cmper, scode.OP2).TryEval(out var resultData))
                {
                    //演算可能
                    return TakeInRangeWithOperation(scode.OP1, resultData, exprSign, expType, listType);
                }
            }
            if (cmper is SimpleData value &&//右辺が式はとりあえず無視
                value.Type == DataOP.Const)//右辺が定数以外（a1>=a2）などもとりあえず無視
            {
                SetRange(cmpee, exprSign, expType, listType, value.Opconst);
                return true;
            }
            return false;
        }

        /// <summary>
        /// 式から範囲リスト作成
        /// </summary>
        /// <param name="expressions"></param>
        void TakeInRangeFromExpressions(SimpleExpression expressions)
        {
            var exprSign = expressions.Select(t => t.Item1.Sign).FirstOrDefault(t => t != SDataSign.Unknown);
            foreach ((var expr, var listType) in expressions)
            {
                //比較されるデータが式の場合の処理
                TakeInRangeWithOperation(expr.OP1, expr.OP2, exprSign, expr.Type, listType);
            }
        }

        /// <summary>
        /// Switch文のJMP先アドレス取得
        /// </summary>
        /// <param name="codes"></param>
        /// <param name="disasm"></param>
        /// <returns></returns>
        public static (Dictionary<uint, uint[]> addrList, Dictionary<uint, int[]> caseList) GetSwitchLabels(IEnumerable<IDisasm> codes, SimpleDisassembler disasm)
        {
            Dictionary<uint, uint[]> addrResult = new Dictionary<uint, uint[]>();
            Dictionary<uint, int[]> caseResult = new Dictionary<uint, int[]>();

            //workSet
            IEnumerable<IDisasm> currentCodes;
            ValueRange currentRange;
            Stack<(IEnumerable<IDisasm> codes, ValueRange range)> codesWorkList
                = new Stack<(IEnumerable<IDisasm> codes, ValueRange range)>();
            codesWorkList.Push((codes, null));

            while (codesWorkList.Count > 0)
            {
                (currentCodes, currentRange) = codesWorkList.Pop();
                foreach (var code in currentCodes.Select((v, i) => new { v, i }))
                {
                    switch (code.v)
                    {
                        case SimpleCode scode:
                            if (scode.TryGetNode(out SimpleCode unsigMinus, t => t.Cmd == SimpleCode.CodeCmd.Sub &&
                             t.OP2 is SimpleData value && value.Type == DataOP.Const &&
                             t.OP1 is SimpleData var && var.DataSign == SDataSign.Unsigned && (var.Type == DataOP.Argment || var.Type == DataOP.LocalVar)&&
                             (!currentRange.Ranges.TryGetValue(var.ToString(), out var tmprange)||(tmprange.Max > value.Opconst && tmprange.Min < var.Opconst))))
                            {
                                //unsigned var - valueの範囲設定
                                SimpleData value = (SimpleData)unsigMinus.OP2;
                                SimpleData var = (SimpleData)unsigMinus.OP1;
                                currentRange.SetRange(var, var.DataSign, SimpleExpression.ExpType.GE, SimpleExpression.ExpressionListType.And, value.Opconst);
                            }
                            if (scode.Cmd == SimpleCode.CodeCmd.Goto && scode.OP1.IsPointer &&//ポインタ
                                scode.TryGetNode(out SimpleData addr, t => t.Type == DataOP.Const && disasm.IsInCode((uint)t.Opconst)) &&//アドレス取得
                                scode.TryGetNode(out IData index, t =>  currentRange.Ranges.ContainsKey(t.ToString())))//インデックス取得
                            {
                                //値取得
                                uint baseAddr = (uint)addr.Opconst;
                                var range = currentRange.GetRange(index);

                                //アドレス・case取得
                                var addrAndCase = range.GetDataAddress(disasm, (SimpleCode)scode.OP1, index).ToArray();
                                addrResult[code.v.ip] = addrAndCase.Select(t => t.Addr).ToArray();
                                caseResult[code.v.ip] = addrAndCase.Select(t => t.Case).ToArray();

                                //サイズ
                                disasm.TakeInSkipListForData(baseAddr, (uint)(baseAddr + scode.OP1.DataSize * addrResult[code.v.ip].Length));
                            }
                            break;
                        case SimpleStatement stmt:
                            //範囲計算
                            var TrueRange = new ValueRange(currentRange);
                            TrueRange.TakeInRangeFromExpressions(stmt.Expression);

                            //currentCodesの続きをPush
                            codesWorkList.Push((currentCodes.Skip(code.i + 1), currentRange));
                            switch (stmt)
                            {
                                case SimpleLoop sloop:
                                    codesWorkList.Push((sloop.TrueCodes.Values, TrueRange));
                                    goto foreachBreak;
                                case SimpleIf sif:
                                    if (sif.FalseCodes != null)
                                    {
                                        //false計算
                                        var FalseRange = new ValueRange(currentRange);
                                        sif.Expression.NegateAll();//反転
                                        FalseRange.TakeInRangeFromExpressions(sif.Expression);
                                        codesWorkList.Push((sif.FalseCodes.Values, FalseRange));
                                        sif.Expression.NegateAll();//反転戻す
                                    }
                                    codesWorkList.Push((sif.TrueCodes.Values, TrueRange));
                                    goto foreachBreak;
                            }
                            break;
                        default:
                            throw new NotImplementedException();
                    }
                }
                foreachBreak:;
            }
            return (addrResult, caseResult);
        }
    }
}
