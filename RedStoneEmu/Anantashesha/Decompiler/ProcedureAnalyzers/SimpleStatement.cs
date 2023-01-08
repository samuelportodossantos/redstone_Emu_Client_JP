using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Anantashesha.Decompiler.Disassemble.Disassembler;
using static Anantashesha.Decompiler.ProcedureAnalyzers.SimpleData;

namespace Anantashesha.Decompiler.ProcedureAnalyzers
{
    abstract class SimpleStatement : IDisasm
    {
        public uint ip { get ; set ; }

        /// <summary>
        /// ステートメントの式
        /// </summary>
        public SimpleExpression Expression;

        protected SimpleStatement(uint ip)
            => this.ip = ip;
    }

    /// <summary>
    /// ループ文
    /// </summary>
    class SimpleLoop : SimpleStatement
    {
        /// <summary>
        /// ループタイプ
        /// </summary>
        public WhileType Type;

        /// <summary>
        /// 式が真の時のコード
        /// </summary>
        public Dictionary<uint, IDisasm> TrueCodes;

        /// <summary>
        /// breakのIP
        /// </summary>
        public uint BreakIP;

        /// <summary>
        /// ContinueのIP
        /// </summary>
        public uint ContinueIP;

        public SimpleLoop(uint ip, WhileType type)
            : base(ip) => Type = type;

        public override string ToString()
        {
            switch (Type)
            {
                case WhileType.DoWhile:
                    return $"Do{{{TrueCodes.First().Value}...}}While({Expression})";
                case WhileType.While:
                    return $"While({Expression}){{{TrueCodes.First().Value}...}}";
                default:
                    throw new NotImplementedException();
            }
        }

        public enum WhileType
        {
            None = 0,
            DoWhile, While, For
        }
    }

    /// <summary>
    /// IF文
    /// </summary>
    class SimpleIf : SimpleStatement
    {
        /// <summary>
        /// 式が真の時のコード
        /// </summary>
        public Dictionary<uint, IDisasm> TrueCodes;

        /// <summary>
        /// 式が偽の時のコード
        /// </summary>
        public Dictionary<uint, IDisasm> FalseCodes;

        public SimpleIf(uint ip) : base(ip) { }

        public override string ToString()
            => $"If({Expression}){{{TrueCodes.First().Value}...}}";
    }

    /// <summary>
    /// switch文
    /// </summary>
    class SimpleSwitch : SimpleStatement
    {
        public SimpleSwitch(uint ip) : base(ip){ }

        /// <summary>
        /// ジャンプ用の式orData
        /// </summary>
        public IData EvalCode;

        /// <summary>
        /// Case文
        /// </summary>
        public Dictionary<int, Dictionary<uint, IDisasm>> CaseCodes
            = new Dictionary<int, Dictionary<uint, IDisasm>>();
    }
}
