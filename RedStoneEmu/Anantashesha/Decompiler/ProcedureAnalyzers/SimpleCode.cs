using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Anantashesha.Decompiler.Disassemble.Disassembler;
using static Anantashesha.Decompiler.ProcedureAnalyzers.SimpleData;

namespace Anantashesha.Decompiler.ProcedureAnalyzers
{
    /// <summary>
    /// 操作用
    /// </summary>
    class SimpleCode : IData
    {
        public uint ip { get; set; }

        /// <summary>
        /// コマンド
        /// </summary>
        public CodeCmd Cmd;

        /// <summary>
        /// コマンド特徴
        /// </summary>
        public CodeOP Feature;

        public IData[] OPs;

        SDataType _DataType;
        SDataSign _DataSign;

        /// <summary>
        /// IF文用の式
        /// </summary>
        public SimpleExpression Expression = null;

        /// <summary>
        /// 未対応のコマンド名
        /// </summary>
        string UnkCmdName = null;
        
        /// <summary>
        /// 複製
        /// </summary>
        /// <returns></returns>
        public object Clone()
        {
            SimpleCode clone = new SimpleCode(ip, Cmd, Feature, OPs.Select(t => (IData)t?. Clone()).ToArray());
            clone.UnkCmdName = UnkCmdName;
            clone._DataSign = _DataSign;
            clone._DataType = _DataType;
            return clone;
        }

        public IData OP1
        {
            get => OPs[0];
            set => OPs[0] = value;
        }
        public IData OP2
        {
            get => OPs[1];
            set => OPs[1] = value;
        }
        public IData OP3
        {
            get => OPs[2];
            set => OPs[2] = value;
        }
        public IData OP4
        {
            get => OPs[3];
            set => OPs[3] = value;
        }

        /// <summary>
        /// ポインタの型もしくは式内部のデータ型
        /// </summary>
        public SDataType DataType
        {
            get
            {
                if (Cmd == CodeCmd.Assign) return OP2.HasDataType ? OP2.DataType : OP1.DataType;//代入文はOP2の型を優先
                else if (HasDataType) return _DataType;
                else if (TryGetNode(out IData dataHasDataType, t => t.HasDataType)) return dataHasDataType.DataType;
                else return SDataType.Unknown;
            }
            set => _DataType = value;
        }

        /// <summary>
        /// ポインタの符号もしくは式内部のデータ符号
        /// </summary>
        public SDataSign DataSign
        {
            get => _DataSign;
            set => _DataSign = value;
        }

        public bool IsPointer => Feature.HasFlag(CodeOP.Pointer);

        public bool HasDataType
            => _DataType == SDataType.BYTE || _DataType == SDataType.CHAR || _DataType == SDataType.WORD || _DataType == SDataType.DWORD;

        public int DataSize
            => GetDataSizeFromDataType(DataType);

        public SimpleCode(uint ip, CodeCmd cmd, CodeOP feature, params IData[] ops)
        {
            this.ip = ip;
            Cmd = cmd;
            Feature = feature;
            OPs = ops;
        }

        public SimpleCode(uint ip, CodeCmd cmd, params IData[] ops)
            : this(ip, cmd, CodeOP.None, ops) { }

        public SimpleCode(CodeCmd cmd, CodeOP feature, params IData[] ops)
            : this(0, cmd, feature, ops) { }

        public SimpleCode(CodeCmd cmd, params IData[] ops)
            : this(0, cmd, CodeOP.None, ops) { }

        /// <summary>
        /// コード全体変換
        /// </summary>
        /// <param name="codeBlock"></param>
        /// <param name="successors"></param>
        /// <returns></returns>
        public static Dictionary<uint, List<SimpleCode>> CreateCodes(Dictionary<uint, List<t_disasm>> codeBlock)
        {
            //simple Code化
            Dictionary<uint, List<SimpleCode>> simpleBlock =
                codeBlock.ToDictionary(t => t.Key, t => t.Value.Select(u => CreateCode(u)).ToList());

            //If調整
            SimpleCode prevExpr = null;
            foreach (var ifBlockIP in simpleBlock.Keys
                .Where(t => simpleBlock[t].Last() is SimpleCode scode && scode.Cmd == CodeCmd.If))
            {
                prevExpr = SimpleExpression.StructIf(simpleBlock[ifBlockIP], prevExpr);
            }

            return simpleBlock;
        }

        /// <summary>
        /// 単一コード構築
        /// </summary>
        /// <param name="blockIP"></param>
        /// <param name="code"></param>
        /// <param name="successors"></param>
        /// <returns></returns>
        static SimpleCode CreateCode(t_disasm code)
        {
            var args = code.op.Select(t => CreateData(code.ip, t)).Where(t => t != null).ToArray();
            switch (code.cmdtypeKind)
            {
                case D.MOV:
                    return new SimpleCode(code.ip, CodeCmd.Assign, args);
                case D.MOVC:
                    throw new NotImplementedException();
                case D.PUSH:
                    return new SimpleCode(code.ip, CodeCmd.Push, args);
                case D.POP:
                    return OperateOwn(code.ip, CodeCmd.Pop, args);
                case D.JMP:
                    return new SimpleCode(code.ip, CodeCmd.Goto, args);
                case D.JMC:
                    var ifcode = new SimpleCode(code.ip, CodeCmd.If, new SimpleCode(CodeCmd.Goto, args));
                    ifcode.Expression = SimpleExpression.CreateExpression(code);
                    return ifcode;
                case D.RET:
                    if (args.Length >= 1)
                        return new SimpleCode(code.ip, CodeCmd.Retn, null,
                            new SimpleCode(CodeCmd.Sub, new SimpleData(REG.ESP), args[0]));
                    else return new SimpleCode(code.ip, CodeCmd.Retn);
                case D.TEST:
                    switch (code.cmdname)
                    {
                        case "AND":
                            return OperateOwn(code.ip, CodeCmd.And, CodeOP.Test | CodeOP.Operation, args);
                        case "TEST":
                            return new SimpleCode(code.ip, CodeCmd.And, CodeOP.Test, args);
                        case "CMP":
                            return new SimpleCode(code.ip, CodeCmd.Sub, CodeOP.Test, args);
                    }
                    goto default;
                case D.CMD:
                    switch (code.cmdname)
                    {
                        case "ADD":
                        case "ADC":
                            if (args[1] is SimpleData addData && addData.Type == DataOP.Const && addData.Opconst < 0)
                            {
                                addData.Opconst *= -1;
                                return OperateOwn(code.ip, CodeCmd.Sub, CodeOP.Operation, args);
                            }
                            else
                                return OperateOwn(code.ip, CodeCmd.Add, CodeOP.Operation, args);
                        case "SBB":
                            ((SimpleData)args[0]).DataSign = SDataSign.Signed;
                            goto case "SUB";
                        case "SUB":
                            return OperateOwn(code.ip, CodeCmd.Sub, CodeOP.Operation, args);
                        case "MUL":
                        case "IMUL":
                            return OperateOwn(code.ip, CodeCmd.Mul, CodeOP.Operation, args);
                        case "DIV":
                        case "IDIV":
                            return OperateOwn(code.ip, CodeCmd.Div, CodeOP.Operation, args);
                        case "INC":
                            return OperateOwn(code.ip, CodeCmd.Add, CodeOP.Operation, args[0], new SimpleData(1));
                        case "DEC":
                            return OperateOwn(code.ip, CodeCmd.Sub, CodeOP.Operation, args[0], new SimpleData(1));
                        case "NEG":
                            ((SimpleData)args[0]).DataSign = SDataSign.Signed;
                            return OperateOwn(code.ip, CodeCmd.Mul, CodeOP.Operation, args[0], new SimpleData(-1));
                        case "OR":
                            return OperateOwn(code.ip, CodeCmd.Or, CodeOP.Operation, args);
                        case "XOR":
                            if (CompareData(args[0], args[1])) return new SimpleCode(code.ip, CodeCmd.Assign, args[0], new SimpleData(0));
                            else return OperateOwn(code.ip, CodeCmd.Div, CodeOP.Operation, args);
                        case "NOT":
                            ((SimpleData)args[0]).DataSign = SDataSign.Unsigned;
                            return OperateOwn(code.ip, CodeCmd.Not, CodeOP.Operation, args);
                        case "SAL":
                            ((SimpleData)args[0]).DataSign = SDataSign.Signed;
                            goto case "SHL";
                        case "SHL":
                            CodeCmd shlType = CodeCmd.Mul;
                            if (args[1] is SimpleData shlSrc && shlSrc.Type == DataOP.Const)
                                shlSrc.Opconst = (int)Math.Pow(2, shlSrc.Opconst);//srcが定数なら掛け算
                            else
                                shlType = CodeCmd.LShift;
                            return OperateOwn(code.ip, shlType, CodeOP.Operation, args);
                        case "SAR":
                            ((SimpleData)args[0]).DataSign = SDataSign.Signed;
                            goto case "SHR";
                        case "SHR":
                            CodeCmd shrType = CodeCmd.Div;
                            if (args[1] is SimpleData shrSrc && shrSrc.Type == DataOP.Const)
                                shrSrc.Opconst = (int)Math.Pow(2, shrSrc.Opconst);//srcが定数なら割り算
                            else
                                shrType = CodeCmd.RShift;
                            return OperateOwn(code.ip, shrType, CodeOP.Operation, args);
                        case "RCR":
                            ((SimpleData)args[0]).DataSign = SDataSign.Signed;
                            goto case "ROR";
                        case "ROR":
                            return OperateOwn(code.ip, CodeCmd.LRoll, CodeOP.Operation, args);
                        case "RCL":
                            ((SimpleData)args[0]).DataSign = SDataSign.Signed;
                            goto case "ROL";
                        case "ROL":
                            return OperateOwn(code.ip, CodeCmd.RRoll, CodeOP.Operation, args);
                        case "LEA":
                            if (args[1] is SimpleCode ptr && ptr.Feature.HasFlag(CodeOP.Pointer))
                            {
                                ptr.Feature &= ~CodeOP.Pointer;
                                return new SimpleCode(code.ip, CodeCmd.Assign, args[0], ptr);
                            }
                            else
                            {
                                ((SimpleData)args[1]).NeedPtr = true;
                                return new SimpleCode(code.ip, CodeCmd.Assign, args);
                            }
                        default:
                            SimpleCode unkCode;

                            var pargs = code.op.Select(t => CreateData(code.ip, t, true)).Where(t => t != null).ToArray();
                            if ((code.op[0].arg & B.PSEUDO) != 0)
                                unkCode = new SimpleCode(code.ip, CodeCmd.CMD, pargs);
                            else
                                unkCode = OperateOwn(code.ip, CodeCmd.CMD, pargs);
                            unkCode.UnkCmdName = code.cmdname;
                            return unkCode;

                    }
                default:
                    var tpargs = code.op.Select(t => CreateData(code.ip, t, true)).Where(t => t != null).ToArray();
                    var trueUnkCode = new SimpleCode(code.ip, CodeCmd.CMD, tpargs);
                    trueUnkCode.UnkCmdName = $"{code.cmdname}({code.cmdtypeKind})";
                    return trueUnkCode;
            }
        }

        /// <summary>
        /// 自分自身に操作
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="cmd"></param>
        /// <param name="op1"></param>
        /// <returns></returns>
        private static SimpleCode OperateOwn(uint ip, CodeCmd cmd, params IData[] ops)
            => OperateOwn(ip, cmd, CodeOP.None, ops);

        /// <summary>
        /// 自分自身に操作
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="cmd"></param>
        /// <param name="feature"></param>
        /// <param name="ops"></param>
        /// <returns></returns>
        private static SimpleCode OperateOwn(uint ip, CodeCmd cmd, CodeOP feature, params IData[] ops)
        {
            if (ops.Length > 0) return new SimpleCode(ip, CodeCmd.Assign, feature, ops[0], new SimpleCode(0, cmd, feature, ops));
            else return new SimpleCode(ip, cmd, feature, ops);
        }
        
        /// <summary>
        /// ポインタ作成
        /// </summary>
        /// <param name="op"></param>
        /// <returns></returns>
        public static SimpleCode CreatePointer(uint ip, t_operand op)
        {
            if ((op.features & OP.INDEXED) != 0)//SIB付き
            {
                return CreateSib(ip, op);
            }

            IData op1, op2 = null;
            CodeCmd feature = CodeCmd.None;
            if (op.reg != REG.UNDEF && op.opconst != 0)
            {
                op1 = new SimpleData(op.reg);
                op2 = new SimpleData(op.SignedOpconst);
                feature = CodeCmd.Add;
            }
            else if (op.reg != REG.UNDEF)
            {
                op1 = new SimpleData(op.reg);
            }
            else
            {
                op1 = new SimpleData(op.SignedOpconst);
            }
            var result = new SimpleCode(ip, feature, CodeOP.Pointer | (feature != CodeCmd.None ? CodeOP.Operation : 0), op1, op2);
            result.DataType = SizeToDataType(op.granularity);
            return result;
        }

        /// <summary>
        /// SIB作成
        /// </summary>
        /// <param name="op"></param>
        /// <returns></returns>
        private static SimpleCode CreateSib(uint ip, t_operand op)
        {
            CodeOP codeop = CodeOP.Pointer;
            CodeCmd codecmd = CodeCmd.Add;

            //全Index検出
            IData[] indexes = Enumerable.Range(0, NREG).Where(t => op.scale[t] > 0)
                .Select(reg =>
                {
                    var myreg = new SimpleData((REG)reg);
                    if (op.scale[reg] == 1) return myreg;
                    else return (IData)new SimpleCode(CodeCmd.Mul, CodeOP.Operation, myreg, new SimpleData(op.scale[reg]));
                }).ToArray();

            //index
            IData joinedIndex;
            if (indexes.Length <= 0) throw new ArgumentException(nameof(op.scale));
            else if (indexes.Length == 1) joinedIndex = indexes[0];
            else
            {
                //index連結
                joinedIndex = new SimpleCode(CodeCmd.Add, CodeOP.Operation, indexes.First(), null);
                SimpleCode currentCode = (SimpleCode)joinedIndex;
                for (int i = 0; i < indexes.Length - 2; i++)
                {
                    currentCode.OP2 = new SimpleCode(CodeCmd.Add, CodeOP.Operation, indexes[i + 1], null);
                    currentCode = (SimpleCode)currentCode.OP2;
                }
                currentCode.OP2 = indexes.Last();
                codeop |= CodeOP.Operation;
            }

            //reg & const
            SimpleData regdata = op.reg != REG.UNDEF ? new SimpleData(op.reg) : null;
            SimpleData constdata = op.opconst != 0 ? new SimpleData(op.SignedOpconst) : null;
            IData regconst;
            if (regdata != null && constdata != null)
            {
                regconst = new SimpleCode(CodeCmd.Add, CodeOP.Operation, regdata, constdata);
                codeop |= CodeOP.Operation;
            }
            else if (regdata != null || constdata != null)
            {
                regconst = regdata ?? constdata;
                codeop |= CodeOP.Operation;
            }
            else if (indexes.Length == 1)//reg=const=0
            {
                codeop &= ~CodeOP.Operation;
                codecmd = CodeCmd.None;
                regconst = null;
            }
            else//execute考慮
                regconst = new SimpleData(0);

            var result = new SimpleCode(ip, codecmd, codeop, joinedIndex, regconst);
            result.DataType = SizeToDataType(op.granularity);
            return result;
        }

        /// <summary>
        /// 条件に適合するノード取得
        /// </summary>
        /// <param name="predicate"></param>
        /// <param name="code"></param>
        /// <returns></returns>
        public bool TryGetNode<T>(out T code, Predicate<T> predicate=null)
            where T : IData
        {
            return _tryGetNode(this, out code);

            bool _tryGetNode(IData incode, out T outcode)
            {
                if (incode is T t_incode && (predicate == null || predicate(t_incode)))
                {
                    outcode = t_incode;
                    return true;
                }
                switch (incode)
                {
                    case SimpleCode scode:
                        foreach (var op in scode.OPs)
                        {
                            if (_tryGetNode(op, out outcode)) return true;
                        }
                        if (scode.Expression != null)
                        {
                            if (_tryGetNode(scode.Expression.OP1, out outcode)) return true;
                            if (_tryGetNode(scode.Expression.OP2, out outcode)) return true;
                        }
                        break;
                    case SimpleExpression sexpr:
                        break;
                }
                outcode = default;
                return false;
            }
        }
        
        /// <summary>
        /// 逆の操作コマンド取得
        /// </summary>
        /// <param name="cmd"></param>
        /// <param name="negateCmd"></param>
        /// <returns></returns>
        public static bool TryGetNegateOperationCmd(CodeCmd cmd, out CodeCmd negateCmd)
        {
            switch (cmd)
            {
                case CodeCmd.Add:
                    negateCmd = CodeCmd.Sub;
                    return true;
                case CodeCmd.Sub:
                    negateCmd = CodeCmd.Add;
                    return true;
                case CodeCmd.Mul:
                    negateCmd = CodeCmd.Div;
                    return true;
                case CodeCmd.Div:
                    negateCmd = CodeCmd.Mul;
                    return true;
                case CodeCmd.RShift:
                    negateCmd = CodeCmd.LShift;
                    return true;
                case CodeCmd.LShift:
                    negateCmd = CodeCmd.RShift;
                    return true;
                case CodeCmd.RRoll:
                    negateCmd = CodeCmd.LRoll;
                    return true;
                case CodeCmd.LRoll:
                    negateCmd = CodeCmd.RRoll;
                    return true;
                default:
                    negateCmd = CodeCmd.Unknown;
                    return false;
            }
        }

        /// <summary>
        /// 数値同士の演算を評価
        /// </summary>
        /// <param name="result"></param>
        /// <returns></returns>
        public bool TryEval(out SimpleData result, Func<uint, int, uint> getPointerData = null)
        {
            result = null;
            if ((IsPointer && getPointerData == null) || !Feature.HasFlag(CodeOP.Operation)) return false;//ポインタかthisが演算じゃない

            if (!getNumFromOP(OP1, out var numData1) || !getNumFromOP(OP2, out var numData2)) return false;//取得失敗
            int num1 = numData1.Opconst;
            int num2 = numData2.Opconst;

            //演算
            int numresult;
            switch (Cmd)
            {
                case CodeCmd.Add:
                    numresult = num1 + num2; break;
                case CodeCmd.Sub:
                    numresult = num1 - num2; break;
                case CodeCmd.Mul:
                    numresult = num1 * num2; break;
                case CodeCmd.Div:
                    numresult = num1 / num2; break;
                case CodeCmd.And:
                    numresult = num1 & num2; break;
                case CodeCmd.Or:
                    numresult = num1 | num2; break;
                case CodeCmd.Xor:
                    numresult = num1 ^ num2; break;
                case CodeCmd.Not:
                    numresult = ~num1; break;
                case CodeCmd.RShift:
                    numresult = num1 >> num2; break;
                case CodeCmd.LShift:
                    numresult = num1 << num2; break;
                case CodeCmd.RRoll:
                    numresult = ((num1 >> num2) | (num1 << (int)(Math.Pow(2, numData1.DataSize) - num2)));
                    numresult &= (int)GetDataRange(numData1.DataType, numData1.DataSign).max;
                    if (num1 < 0) numresult *= -1;
                    break;
                case CodeCmd.LRoll:
                    int num1bitnum = (int)Math.Pow(2, numData1.DataSize);//ビット長
                    long tmpresult = num1 << num2;
                    numresult = (int)tmpresult | (int)((tmpresult & (uint.MaxValue << num1bitnum)) >> num1bitnum);//余分を下位ビットに持ってくる
                    numresult &= (int)GetDataRange(numData1.DataType, numData1.DataSign).max;
                    if (num1 < 0) numresult *= -1;
                    break;
                default:
                    return false;
            }

            //結果セット
            if (IsPointer)
                result = new SimpleData((int)getPointerData((uint)numresult, DataSize), (uint)DataSize);//ポインタから取得
            else
            {
                result = new SimpleData(numresult);

                result.DataType = numData1.DataType;//OP1優先
                if (numData1.DataSign == SDataSign.Unsigned || numData2.DataSign == SDataSign.Unsigned)
                    result.DataSign = SDataSign.Unsigned;//unsigned優先
                else
                    result.DataSign = numData1.DataSign != SDataSign.Unknown ? numData1.DataSign : numData2.DataSign;
            }
            return true;

            //opから数値取得
            bool getNumFromOP(IData op, out SimpleData numResult)
            {
                numResult = default;
                if (op == null) return true;//OPが要らない演算
                else if (op is SimpleCode scode1 && scode1.TryEval(out var result1, getPointerData)) numResult = result1;//演算可能な式
                else if (op is SimpleData sdata1 && sdata1.Type == DataOP.Const) numResult = sdata1;//数値
                else return false;
                return true;
            }
        }

        /// <summary>
        /// dstにsrcを代入
        /// </summary>
        /// <param name="dst"></param>
        /// <param name="src"></param>
        /// <returns></returns>
        public SimpleCode TryAssignData(IData dst, IData src)
        {
            string dststr = dst.ToString();
            return (SimpleCode)_tryAssignData(this);
            IData _tryAssignData(IData target)
            {
                if (target == null) return target;
                if (CompareData(target, dst) || target.ToString() == dststr)
                    return src;
                switch (target)
                {
                    case SimpleCode scode:
                        for (int i = 0; i < scode.OPs.Length; i++)
                        {
                            scode.OPs[i] = _tryAssignData(scode.OPs[i]);
                        }
                        if (scode.Expression != null)//IF文
                        {
                            scode.Expression.SetOP(_tryAssignData(scode.Expression.OP1), _tryAssignData(scode.Expression.OP2));
                        }
                        return scode;
                    case SimpleData sdata:
                        return sdata;
                    default:
                        throw new NotImplementedException();
                }
            }
        }

        /// <summary>
        /// 代入して実行
        /// </summary>
        /// <param name="arg"></param>
        /// <param name="value"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        public bool TryAssignAndExecute(IData arg, int value, out int result, Func<uint, int, uint> getPointerData)
        {
            SimpleCode newCode = ((SimpleCode)Clone()).TryAssignData(arg, new SimpleData(value));

            if (!newCode.TryEval(out var resultData, getPointerData))
            {
                result = default;
                return false;
            }
            else
            {
                result = resultData.Opconst;
                return true;
            }
        }

        public override string ToString()
        {
            if (Feature.HasFlag(CodeOP.Pointer))
            {
                //ポインタの場合
                string ptrTypeStr = TypeAndSignToString(DataType, DataSign);
                if (ptrTypeStr != null) ptrTypeStr = $"({ptrTypeStr} *)";

                return $"*{ptrTypeStr ?? ""}({CmdString()})";
            }
            else
                return CmdString();

            //コマンドからStrへ
            string CmdString()
            {
                switch (Cmd)
                {
                    case CodeCmd.None:
                        if (OP2 != null) goto default;
                        else return OP1.ToString();
                    case CodeCmd.Assign:
                        return $"{OP1} = {OP2}";
                    case CodeCmd.Add:
                        return $"{OP1} + {OP2}";
                    case CodeCmd.Sub:
                        return $"{OP1} - {OP2}";
                    case CodeCmd.Mul:
                        return $"{OP1} * {OP2}";
                    case CodeCmd.Div:
                        return $"{OP1} / {OP2}";
                    case CodeCmd.And:
                        return $"{OP1} & {OP2}";
                    case CodeCmd.Or:
                        return $"{OP1} | {OP2}";
                    case CodeCmd.Xor:
                        return $"{OP1} ^ {OP2}";
                    case CodeCmd.Not:
                        return $"~{OP1}";
                    case CodeCmd.RShift:
                        return $"{OP1} >> {OP2}";
                    case CodeCmd.LShift:
                        return $"{OP1} << {OP2}";
                    case CodeCmd.RRoll:
                        return $"{OP1} >=> {OP2}";
                    case CodeCmd.LRoll:
                        return $"{OP1} <=< {OP2}";
                    case CodeCmd.Push:
                        return $"Push({OP1})";
                    case CodeCmd.Pop:
                        return $"Pop()";
                    case CodeCmd.Goto:
                        return $"GOTO:{OP1}";
                    case CodeCmd.If:
                        return $"If({Expression}) {OP1}";
                    case CodeCmd.Retn:
                        return $"Return";
                    case CodeCmd.Continue:
                        return "Continue";
                    case CodeCmd.Break:
                        return "Break";
                    case CodeCmd.CMD:
                        return $"{UnkCmdName}({string.Join(",", OPs.Select(t => t.ToString()))})";
                    default:
                        if (OP2 != null) return $"{OP1} ? {OP2}";
                        else return $"?{OP1}";
                }
            }
        }

        public bool Equals(IData x, IData y)
            => x.GetHashCode() == y.GetHashCode();

        public int GetHashCode(IData obj)
            => obj.GetHashCode();

        public override int GetHashCode()
            => ToString().GetHashCode();

        /// <summary>
        /// コード種類
        /// </summary>
        public enum CodeCmd
        {
            Unknown=0,
            None,
            Assign,//割当て
            Add,Sub,Mul,Div,//演算
            And, Or, Xor, Not,//論理演算
            RShift, LShift,RRoll, LRoll,
            Push, Pop,
            CMD,//その他割当コマンド
            Goto, If, Retn, Continue, Break,
        }

        /// <summary>
        /// コード特徴
        /// </summary>
        [Flags]
        public enum CodeOP
        {
            None=0,
            Operation=0x01,//演算
            Test=0x02,//テスト
            Pointer=0x04,//ポインタ
        }
    }

    /// <summary>
    /// データ用
    /// </summary>
    class SimpleData : IData
    {
        public uint ip { get; set; }

        /// <summary>
        /// データの特徴
        /// </summary>
        public DataOP Type;

        /// <summary>
        /// 型
        /// </summary>
        public SDataType DataType { get; set; }

        /// <summary>
        /// 符号
        /// </summary>
        public SDataSign DataSign { get; set; }

        /// <summary>
        /// 定数
        /// </summary>
        public int Opconst;

        /// <summary>
        /// &Any
        /// </summary>
        public bool NeedPtr = false;

        /// <summary>
        /// 複製
        /// </summary>
        /// <returns></returns>
        public object Clone()
        {
            SimpleData clone = new SimpleData(Type);
            clone.ip = ip;
            clone.DataType = DataType;
            clone.DataSign = DataSign;
            clone.Opconst = Opconst;
            clone.NeedPtr = NeedPtr;
            return clone;
        }

        /// <summary>
        /// レジスタ
        /// </summary>
        public REG Reg
            => Type == DataOP.Register ? (REG)Opconst : REG.UNDEF;

        /// <summary>
        /// ポインタフラグ
        /// </summary>
        public bool IsPointer => false;

        /// <summary>
        /// 型を持っているか
        /// </summary>
        public bool HasDataType
            => DataType == SDataType.BYTE || DataType == SDataType.CHAR || DataType == SDataType.WORD || DataType == SDataType.DWORD;

        /// <summary>
        /// データ作成
        /// </summary>
        /// <param name="op"></param>
        /// <returns></returns>
        public static IData CreateData(uint ip, t_operand op, bool pseudo = false)
        {
            if (op.arg == B.NONE) return null;
            if (!pseudo && (op.arg & B.PSEUDO) != 0) return null;//pseudoOP未許可

            //ポインタ
            if ((op.features & OP.MEMORY) != 0) return SimpleCode.CreatePointer(ip, op);

            //引数orローカル変数
            if (op.varnum != 0)
            {
                if (op.varnum > 0) return new SimpleData(DataOP.Argment, op.varnum, op.granularity);
                else return new SimpleData(DataOP.LocalVar, -op.varnum, op.granularity);
            }

            if ((op.features & OP.SOMEREG) == OP.REGISTER)
            {
                //レジスタ
                return new SimpleData(op.reg, op.granularity);
            }
            if ((op.features & OP.CONST) != 0)
            {
                //定数
                var result = new SimpleData(op.SignedOpconst, op.granularity);
                if((op.arg & B.ARGMASK) == B.SXTCONST)
                {
                    //符号付き8ビット
                    result.DataType = SDataType.BYTE;
                    result.DataSign = SDataSign.Signed;
                }
                return result;
            }

            throw new NotImplementedException();
        }

        /// <summary>
        /// サイズから型推定
        /// </summary>
        /// <param name="size"></param>
        /// <returns></returns>
        public static SDataType SizeToDataType(uint size)
        {
            switch (size)
            {
                case 4: return SDataType.DWORD;
                case 2: return SDataType.WORD;
                case 1: return SDataType.BYTE;
                default: return SDataType.Unknown;
            }
        }

        /// <summary>
        /// TypeとSignから型名を取得
        /// </summary>
        /// <param name="type"></param>
        /// <param name="sign"></param>
        /// <returns></returns>
        public static string TypeAndSignToString(SDataType type, SDataSign sign)
        {
            switch (type)
            {
                case SDataType.DWORD: return getName("int");
                case SDataType.WORD: return getName("short");
                case SDataType.BYTE: return getName("byte", "sbyte");
                default: return null;
            }
            string getName(string signedName, string unsignedName = null)
            {
                switch (sign)
                {
                    case SDataSign.Signed: return signedName;
                    case SDataSign.Unsigned: return unsignedName ?? "u" + signedName;
                    default: return type.ToString();
                }
            }
        }

        /// <summary>
        /// 一致
        /// </summary>
        /// <param name="data1"></param>
        /// <returns></returns>
        public static bool CompareData(SimpleData data1, SimpleData data2)
        {
            if (data1 == null || data2 == null) return false;
            if (data1.Type != data2.Type) return false;
            if (data1.Opconst != data2.Opconst) return false;
            return true;
        }

        /// <summary>
        /// データ比較
        /// </summary>
        /// <param name="data1"></param>
        /// <param name="data2"></param>
        /// <returns></returns>
        public static bool CompareData(IDisasm data1, IDisasm data2)
            => CompareData(data1 as SimpleData, data2 as SimpleData);

        private SimpleData(uint size)
            => DataType = SizeToDataType(size);

        /// <summary>
        /// データサイズ
        /// </summary>
        public int DataSize
            => GetDataSizeFromDataType(DataType);

        /// <summary>
        /// データ型からサイズ取得
        /// </summary>
        /// <param name="dataType"></param>
        /// <returns></returns>
        public static int GetDataSizeFromDataType(SDataType dataType)
        {
            switch (dataType)
            {
                case SDataType.BYTE:
                case SDataType.CHAR:
                    return 1;
                case SDataType.WORD:
                    return 2;
                case SDataType.DWORD:
                    return 4;
                default:
                    throw new NotImplementedException();
            }
        }

        /// <summary>
        /// データ範囲を取得
        /// </summary>
        /// <param name="type"></param>
        /// <param name="sign"></param>
        /// <returns></returns>
        public static (long min, long max) GetDataRange(SDataType type, SDataSign sign)
        {
            switch (type)
            {
                case SDataType.BYTE:
                case SDataType.CHAR:
                    return getRange(sbyte.MinValue, sbyte.MaxValue, byte.MinValue, byte.MaxValue);
                case SDataType.WORD:
                    return getRange(short.MinValue, short.MaxValue, ushort.MinValue, ushort.MaxValue);
                case SDataType.DWORD:
                    return getRange(int.MinValue, int.MaxValue, uint.MinValue, uint.MaxValue);
                default:
                    throw new NotImplementedException();
            }

            //signからtypeの取りうる範囲
            (long, long) getRange(long signedMinValue, long signedMaxValue, long unsignedMinValue, long unsignedMaxValue)
            {
                switch (sign)
                {
                    case SDataSign.Unknown:
                    case SDataSign.Signed:
                        return (signedMinValue, signedMaxValue);
                    case SDataSign.Unsigned:
                        return (unsignedMinValue, unsignedMaxValue);
                    default:
                        throw new NotImplementedException();
                }
            }
        }

        /// <summary>
        /// レジスタ
        /// </summary>
        /// <param name="reg"></param>
        public SimpleData(REG reg, uint size = 0)
            : this(size)
        {
            Type = DataOP.Register;
            Opconst = (int)reg;
        }

        /// <summary>
        /// 定数
        /// </summary>
        /// <param name="value"></param>
        public SimpleData(int value, uint size = 0)
            : this(size)
        {
            Type = DataOP.Const;
            Opconst = value;
        }

        /// <summary>
        /// タイプと値
        /// </summary>
        /// <param name="value"></param>
        public SimpleData(DataOP feature, int value, uint size = 0)
            : this(size)
        {
            Type = feature;
            Opconst = value;
        }

        /// <summary>
        /// タイプのみ
        /// </summary>
        /// <param name="value"></param>
        public SimpleData(DataOP feature, uint size = 0)
            : this(size)
        {
            Type = feature;
            Opconst = 0;
        }

        public override string ToString()
        {
            string result;
            switch (Type)
            {
                case DataOP.Const:
                    result = Opconst.ToString("X"); break;
                case DataOP.Register:
                    result = REGNAME[Opconst]; break;
                case DataOP.LocalVar:
                    result = $"v{Opconst - 1}"; break;
                case DataOP.Argment:
                    result = $"a{Opconst - 1}"; break;
                case DataOP.This:
                    result = "this"; break;
                default:
                    result = "UNK"; break;
            }
            if (NeedPtr) result = "&" + result;
            return result;
        }

        public bool Equals(IData x, IData y)
            => x.GetHashCode() == y.GetHashCode();

        public int GetHashCode(IData obj)
            => obj.GetHashCode();

        public override int GetHashCode()
            => ToString().GetHashCode();

        public enum DataOP
        {
            None = 0,
            Const,//定数
            Register,//レジスタ

            LocalVar,//ローカル変数
            Argment,//引数
            This,//thiscall index
        }

        public enum SDataType
        {
            Unknown = 0,
            BYTE, CHAR, WORD, DWORD,
        }

        public enum SDataSign
        {
            Unknown = 0,
            Unsigned, Signed
        }

    }

    /// <summary>
    /// データ用インターフェイス
    /// </summary>
    interface IData : IEqualityComparer<IData>, ICloneable, IDisasm
    {
        /// <summary>
        /// 型
        /// </summary>
        SDataType DataType { get; set; }

        /// <summary>
        /// 符号
        /// </summary>
        SDataSign DataSign { get; set; }

        /// <summary>
        /// ポインタ
        /// </summary>
        bool IsPointer { get; }

        bool HasDataType { get; }

        /// <summary>
        /// 値のサイズ
        /// </summary>
        int DataSize { get; }

        /// <summary>
        /// ハッシュコード
        /// </summary>
        /// <returns></returns>
        int GetHashCode();
    }
}
