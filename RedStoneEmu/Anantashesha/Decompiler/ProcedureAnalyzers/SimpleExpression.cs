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
    /// 式
    /// </summary>
    class SimpleExpression : IDisasm, IEnumerable<(SimpleExpression, SimpleExpression.ExpressionListType)>
    {
        public uint ip { get; set; }

        public ExpType Type;

        public SDataSign Sign;

        IData _OP1;
        IData _OP2;

        /// <summary>
        /// 次の式
        /// </summary>
        public SimpleExpression Next = null;

        /// <summary>
        /// 次に繋がる場合のタイプ
        /// </summary>
        public ExpressionListType ListType = ExpressionListType.None;

        public IData OP1
        {
            get => _OP1;
            private set => _OP1 = SetSign(value);
        }

        public IData OP2
        {
            get => _OP2;
            private set => _OP2 = SetSign(value);
        }

        private SimpleExpression(uint ip, ExpType type, SDataSign sign)
        {
            this.ip = ip; Type = type; Sign = sign;
        }

        /// <summary>
        /// チェーン用
        /// </summary>
        /// <param name="baseExpr"></param>
        /// <param name="listType"></param>
        public SimpleExpression(SimpleExpression baseExpr, ExpressionListType listType)
            : this(baseExpr.ip, baseExpr.Type, baseExpr.Sign)
        {
            OP1 = baseExpr.OP1;
            OP2 = baseExpr.OP2;
            ListType = listType;
        }

        /// <summary>
        /// OPセット
        /// </summary>
        /// <param name="op1"></param>
        /// <param name="op2"></param>
        public void SetOP(IData op1, IData op2)
        {
            if(op1 is SimpleCode scode && (scode.Cmd ==SimpleCode.CodeCmd.And|| scode.Cmd ==SimpleCode.CodeCmd.Sub) && SimpleData.CompareData(scode.OP2, op2))
            {
                //比較が継承
                OP1 = scode.OP1;
                OP2 = op2;
            }
            else
            {
                OP1 = op1;
                OP2 = op2;
            }
            /*OP1 = (IData)op1.Clone();
            OP2 = (IData)op2.Clone();*/
        }

        /// <summary>
        /// 符号をセット
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        IData SetSign(IData value)
        {
            if (value is SimpleData || value.IsPointer)
            {
                value = SetSign(value);
            }
            else if (value is SimpleCode scode && scode.TryGetNode(out SimpleData targetData, t => (t.Type == DataOP.Argment || t.Type == DataOP.LocalVar) && t.DataSign==SDataSign.Unknown))
            {
                //式内部に符号セット
                targetData = (SimpleData)SetSign(targetData);
            }
            return value;
            IData SetSign(IData datavalue)
            {
                if (datavalue.DataSign != SDataSign.Unknown && Sign == SDataSign.Unknown) return datavalue;
                datavalue.DataSign = Sign;
                return datavalue;
            }
        }

        /// <summary>
        /// Ifが最後に存在するブロックに対して式の構築
        /// </summary>
        /// <param name="codes"></param>
        public static SimpleCode StructIf(List<SimpleCode> codes, SimpleCode prevExpr)
        {
            SimpleCode targetExpr = null;
            foreach (var code in codes.OrderByDescending(t => t.ip).Skip(1))
            {
                if (code.TryGetNode(out targetExpr, t => (t.Cmd == SimpleCode.CodeCmd.And || t.Cmd == SimpleCode.CodeCmd.Sub)))
                {
                    if (code.Cmd != SimpleCode.CodeCmd.Assign)
                    {
                        codes.RemoveAll(t => t.ip == code.ip);//TEST演算は削除
                    }
                    break;
                }
            }
            SimpleExpression expr = codes.Last().Expression;
            if (targetExpr == null)
                targetExpr = prevExpr;
            else
            {
                //符号セット
                ((SimpleData)targetExpr.OP1).DataSign = expr.Sign;
                ((SimpleData)targetExpr.OP2).DataSign = expr.Sign;
            }

            if (targetExpr.Cmd == SimpleCode.CodeCmd.And)
                expr.SetAnd(targetExpr.OP1, targetExpr.OP2);
            else if (targetExpr.Cmd == SimpleCode.CodeCmd.Sub)
                expr.SetSub(targetExpr.OP1, targetExpr.OP2);
            else
                throw new NotImplementedException();

            return targetExpr;
        }

        /// <summary>
        /// AND(TEST)でセット
        /// </summary>
        /// <param name="op1"></param>
        /// <param name="op2"></param>
        private void SetAnd(IData op1, IData op2)
        {
            if (CompareData(op1, op2))
            {
                //op1,op2が一致してる
                switch (Type)
                {
                    case ExpType.EQ:
                    case ExpType.NEQ:
                        OP1 = op1;
                        OP2 = new SimpleData(0);
                        break;
                    case ExpType.Sign:
                        OP1 = op1;
                        OP2 = new SimpleData(0);
                        Type = ExpType.LT;
                        break;
                    case ExpType.NotSign:
                        OP1 = op1;
                        OP2 = new SimpleData(0);
                        Type = ExpType.GE;
                        break;
                    default:
                        throw new NotImplementedException();
                }
            }
            else
            {
                OP1 = new SimpleCode(SimpleCode.CodeCmd.And, op1, op2);
                OP2 = new SimpleData(0);
            }
        }

        /// <summary>
        /// SUB(CMP)でセット
        /// </summary>
        /// <param name="op1"></param>
        /// <param name="op2"></param>
        private void SetSub(IData op1, IData op2)
        {
            switch (Type)
            {
                case ExpType.Sign:
                    Type = ExpType.GT;
                    break;
                case ExpType.NotSign:
                    Type = ExpType.LE;
                    break;
            }
            OP1 = op1;
            OP2 = op2;
        }

        /// <summary>
        /// 式作成
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        public static SimpleExpression CreateExpression(t_disasm code)
        {
            ExpType type;
            SDataSign sign;
            switch (code.cmdname)
            {
                case "JA":
                case "JNBE":
                    sign = SDataSign.Unsigned;
                    type = ExpType.GT;
                    break;
                case "JG":
                case "JNLE":
                    sign = SDataSign.Signed;
                    type = ExpType.GT;
                    break;
                case "JNB":
                case "JNC":
                case "JAE":
                    sign = SDataSign.Unsigned;
                    type = ExpType.GE;
                    break;
                case "JGE":
                case "JNL":
                    sign = SDataSign.Signed;
                    type = ExpType.GE;
                    break;
                case "JB":
                case "JC":
                case "JNAE":
                    sign = SDataSign.Unsigned;
                    type = ExpType.LT;
                    break;
                case "JL":
                case "JNGE":
                    sign = SDataSign.Signed;
                    type = ExpType.LT;
                    break;
                case "JBE":
                case "JNA":
                    sign = SDataSign.Unsigned;
                    type = ExpType.LE;
                    break;
                case "JLE":
                case "JNG":
                    sign = SDataSign.Signed;
                    type = ExpType.LE;
                    break;
                case "JE":
                case "JZ":
                    sign = SDataSign.Unknown;
                    type = ExpType.EQ;
                    break;
                case "JNE":
                case "JNZ":
                    sign = SDataSign.Unknown;
                    type = ExpType.NEQ;
                    break;
                case "JS":
                    sign = SDataSign.Signed;
                    type = ExpType.Sign;
                    break;
                case "JNS":
                    sign = SDataSign.Signed;
                    type = ExpType.NotSign;
                    break;
                default:
                    sign = SDataSign.Unknown;
                    type = ExpType.Unk;
                    break;
            }
            return new SimpleExpression(code.ip, type, sign);
        }

        /// <summary>
        /// ExpType逆転
        /// </summary>
        /// <param name="Type"></param>
        /// <returns></returns>
        static ExpType NegateType(ExpType Type)
        {
            switch (Type)
            {
                case ExpType.EQ:
                    return ExpType.NEQ;
                case ExpType.NEQ:
                    return ExpType.EQ;
                case ExpType.GT:
                    return ExpType.LE;
                case ExpType.LE:
                    return ExpType.GT;
                case ExpType.LT:
                    return ExpType.GE;
                case ExpType.GE:
                    return ExpType.LT;
                case ExpType.Sign:
                    return ExpType.NotSign;
                case ExpType.NotSign:
                    return ExpType.Sign;
                default:
                    throw new NotImplementedException();
            }
        }

        /// <summary>
        /// 式逆転
        /// </summary>
        public void Negate()
            => Type = NegateType(Type);

        /// <summary>
        /// 全ての式逆転
        /// </summary>
        public void NegateAll()
        {
            foreach ((var expr, ExpressionListType type) in this)
            {
                expr.Negate();
                switch (type)
                {
                    case ExpressionListType.And:
                        expr.ListType = ExpressionListType.Or;
                        break;
                    case ExpressionListType.Or:
                        expr.ListType = ExpressionListType.And;
                        break;
                }
            }
        }
        
        /// <summary>
        /// 反転した式を得る
        /// </summary>
        /// <returns></returns>
        public SimpleExpression GetNegate()
        {
            var result = new SimpleExpression(ip, NegateType(Type), Sign);
            result._OP1 = _OP1;
            result._OP2 = _OP2;
            return result;
        }

        public override string ToString()
        {
            string baseStr;
            string op1 = OP1?.ToString() ?? "";
            string op2 = OP2?.ToString() ?? "";
            switch (Type)
            {
                case ExpType.EQ:
                    baseStr= $"{op1}=={op2}"; break;
                case ExpType.NEQ:
                    baseStr = $"{op1}!={op2}"; break;
                case ExpType.GT:
                    baseStr = $"{op1}>{op2}"; break;
                case ExpType.GE:
                    baseStr = $"{op1}>={op2}"; break;
                case ExpType.LT:
                    baseStr = $"{op1}<{op2}"; break;
                case ExpType.LE:
                    baseStr = $"{op1}<={op2}"; break;
                case ExpType.Sign:
                    baseStr = $"{op1}<0"; break;
                case ExpType.NotSign:
                    baseStr = $"{op1}>=0"; break;
                default:
                    baseStr = $"{op1}?={op2}";break;
            }

            if (Next == null) return baseStr;
            else
            {
                switch (ListType)
                {
                    case ExpressionListType.And:
                        return $"{baseStr} && {Next}";
                    case ExpressionListType.Or:
                        return $"{baseStr} || {Next}";
                    case ExpressionListType.None:
                        return baseStr;
                    default:
                        throw new NotImplementedException();
                }
            }
        }
        
        /// <summary>
        /// Enumerator
        /// </summary>
        /// <returns></returns>
        public IEnumerator<(SimpleExpression, ExpressionListType)> GetEnumerator()
        {
            for (var currentExpr = this; currentExpr != null; currentExpr = currentExpr.Next)
            {
                yield return (currentExpr, currentExpr.ListType);
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
            => GetEnumerator();

        public enum ExpType
        {
            Unk = 0,

            /// <summary>
            /// 等しい（＝）
            /// </summary>
            EQ,

            /// <summary>
            /// 等しくない（≠）
            /// </summary>
            NEQ,

            /// <summary>
            /// 超（＞）
            /// </summary>
            GT,

            /// <summary>
            /// 以上（≧）
            /// </summary>
            GE,

            /// <summary>
            /// 未満（＜）
            /// </summary>
            LT,

            /// <summary>
            /// 以下（≦）
            /// </summary>
            LE,

            //符号チェック
            Sign, NotSign
        }

        public enum ExpressionListType
        {
            None, And, Or
        }
    }
}
