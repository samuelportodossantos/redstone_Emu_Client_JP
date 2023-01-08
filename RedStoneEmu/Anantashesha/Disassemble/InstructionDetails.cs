using RedStoneLib.Packets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Anantashesha.Disassemble.InstructionDetail;

namespace Anantashesha.Disassemble
{
    /// <summary>
    /// 命令値構成子Interface
    /// </summary>
    interface IIVConstituent
    {
        //即値の取得
        object GetImmediate();

        string GetString();
    }

    //Data DWORD
    struct DD : IIVConstituent
    {
        uint DataDWORD;

        public DD(uint dataDword) => DataDWORD = dataDword;

        public object GetImmediate() => null;

        public string GetString() => DataDWORD.ToString("X8");
    }

    //Data WORD
    struct DW : IIVConstituent
    {
        ushort DataWORD;

        public DW(ushort dataWord) => DataWORD = dataWord;

        public object GetImmediate() => null;

        public string GetString() => DataWORD.ToString("X4");
    }

    //Data BYTE
    struct DB : IIVConstituent
    {
        byte DataByte;

        public DB(byte dataByte) => DataByte = dataByte;

        public object GetImmediate() => null;

        public string GetString() => DataByte.ToString("X2");
    }

    //値2ペア
    struct PairIVC : IIVConstituent
    {
        public IIVConstituent IVC1;
        public IIVConstituent IVC2;

        public PairIVC(IIVConstituent ivc1, IIVConstituent ivc2)
        { IVC1 = ivc1; IVC2 = ivc2; }

        public object GetImmediate() => IVC2.GetImmediate() ?? IVC1.GetImmediate();

        public string GetString() => $"{IVC1.GetString()}, {IVC2.GetString()}";
    }

    //値3ペア
    struct TupleIVC : IIVConstituent
    {
        public IIVConstituent IVC1;
        public IIVConstituent IVC2;
        public IIVConstituent IVC3;

        public TupleIVC(IIVConstituent ivc1, IIVConstituent ivc2, IIVConstituent ivc3)
        { IVC1 = ivc1; IVC2 = ivc2;IVC3 = ivc3; }

        public object GetImmediate() => IVC3.GetImmediate() ?? IVC2.GetImmediate() ?? IVC2.GetImmediate();

        public string GetString() => $"{IVC1.GetString()}, {IVC2.GetString()}, {IVC3.GetString()}";
    }

    //ポインタ
    struct PtrIVC : IIVConstituent
    {
        public enum PtrType
        {
            PTR = 0x08,
            BYTE_PTR = 0x10 | PTR,
            WORD_PTR = 0x20 | PTR,
            DWORD_PTR = 0x40 | PTR,
            FWORD_PTR = 0x80 | PTR,
            QWORD_PTR = 0x100 | PTR,
        }

        public PtrType _PtrType;

        public IIVConstituent IVC;

        public PtrIVC(PtrType ptrType, IIVConstituent ivc)
        { _PtrType = ptrType; IVC = ivc; }

        public object GetImmediate() => IVC.GetImmediate();

        public string GetString() => $"{_PtrType.ToString()}:[{IVC.GetString()}]";
    }

    //ST,ST(i)
    struct STi :IIVConstituent
    {
        public byte I;

        public STi(int i)=>I=(byte)i;

        public object GetImmediate() => null;

        public string GetString() => $"ST,ST{(I != 0 ? $"({I})" : "")}";
    }

    //ptr 16:16
    struct Ptr1616 : IIVConstituent
    {
        public ushort Ptr16_0;
        public ushort Ptr16_1;

        public Ptr1616(ushort ptr16_0, ushort ptr16_1)
        { Ptr16_0 = ptr16_0; Ptr16_1 = ptr16_1; }

        public object GetImmediate() => Ptr16_0;

        public string GetString() => $"{Ptr16_0:X4}:{Ptr16_1:X4}";
    }

    //ptr 16:32
    struct Ptr1632 : IIVConstituent
    {
        public uint Ptr32;
        public ushort Ptr16;

        public Ptr1632(uint ptr32, ushort ptr16)
        { Ptr32 = ptr32; Ptr16 = ptr16; }

        public object GetImmediate() => Ptr32;

        public string GetString() => $"{Ptr16:X4}:{Ptr32:X8}";
    }

    //8bit offset
    struct Rel8 : IIVConstituent
    {
        public sbyte _Rel8;

        public Rel8(sbyte rel8) => _Rel8 = rel8;

        public object GetImmediate() => _Rel8;

        public string GetString() => $"{(_Rel8 < 0 ? "-" : "+")}{Math.Abs(_Rel8 + 2):X2}";
    }

    //16bit offset
    struct Rel16 : IIVConstituent
    {
        public short _Rel16;

        public Rel16(short rel16) => _Rel16 = rel16;

        public object GetImmediate() => _Rel16;

        public string GetString() => $"{(_Rel16 < 0 ? "-" : "+")}{Math.Abs(_Rel16 + 3):X4}";
    }

    //32bit offset
    struct Rel32 : IIVConstituent
    {
        public int _Rel32;

        public Rel32(int rel32) => _Rel32 = rel32;

        public object GetImmediate() => _Rel32;

        public string GetString() => $"{(_Rel32 < 0 ? "-" : "+")}{Math.Abs(_Rel32 + 6):X8}";
    }

    //only disp8
    struct Disp8 : IIVConstituent
    {
        public sbyte _Disp8;

        public Disp8(sbyte disp8) => _Disp8 = disp8;

        public object GetImmediate() => null;

        public string GetString() => $"{_Disp8:X2}";
    }

    //only disp16
    struct Disp16 : IIVConstituent
    {
        public ushort _Disp16;

        public Disp16(ushort disp16) => _Disp16 = disp16;

        public object GetImmediate() => _Disp16;

        public string GetString() => $"{_Disp16:X4}";
    }

    //only disp32
    struct Disp32 : IIVConstituent
    {
        public uint _Disp32;

        public Disp32(uint disp32) => _Disp32 = disp32;

        public object GetImmediate() => _Disp32;

        public string GetString() => $"{_Disp32:X8}";
    }

    //only REG
    struct Register : IIVConstituent
    {
        public RegType Reg;

        public Register(RegType reg)=>Reg=reg;

        public object GetImmediate() => null;

        public string GetString() => Reg.ToString();
    }

    //REG+disp8
    struct RegisterPlusDisp8 : IIVConstituent
    {
        public Register Reg;

        public Disp8 Disp8;

        public RegisterPlusDisp8(RegType reg, sbyte disp8)
        { Reg = new Register(reg); Disp8 = new Disp8(disp8); }

        public object GetImmediate() => null;

        public string GetString() => $"{Reg.Reg.ToString()}{(Disp8._Disp8 < 0 ? "-" : "+")}{Math.Abs((int)Disp8._Disp8):X2}";
    }

    //REG+disp32
    struct RegisterPlusDisp32 : IIVConstituent
    {
        public Register Reg;

        public Disp32 Disp32;

        public RegisterPlusDisp32(RegType reg, uint disp32)
        { Reg = new Register(reg); Disp32 = new Disp32(disp32); }

        public object GetImmediate() => null;

        public string GetString() => $"{Reg.Reg.ToString()}+{Disp32._Disp32:X}";
    }

    //SIB
    struct Sib : IIVConstituent
    {
        public IIVConstituent Base;

        public RegType Index;

        public byte Scale;

        public Sib(IIVConstituent @base, RegType index, byte scale)
        { Base = @base;Index = index;Scale = scale; }

        public object GetImmediate() => Base.GetImmediate();

        public string GetString() => $"{Index}*{Scale}+{Base.GetString()}";
    }

    struct InstructionDetail
    {
        /// <summary>
        /// オペコード
        /// </summary>
        public InstructionType Instruction;
        public IIVConstituent Operand;
        
        public enum InstructionType
        {
            NULL = 0,
            DB,DW,DD,

            ADD,XADD,
            OR,
            ADC,
            SBB,
            AND,
            SUB,
            XOR,
            CMP,CMPS,CMPXCHG,
            PUSH,POP,
            IMUL,MUL,
            IDIV,DIV,
            
            INC,DEC,
            CALL,JMP,

            DAA,DAS,AAA,AAS,AAM,AAD,

            BOUND,
            ARPL,

            INS,OUTS,IN,OUT,

            JO,JNO,JB,JAE,JZ,JNZ,JBE,JA,JS,JNS,JP,JNP,JL,JGE,JLE,JG,

            TEST,NOT,NEG,
            XCHG,
            MOV,LEA,MOVS,MOVZX,MOVSX,MOVAPS,MOVAPD,MOVD,MOVQ,
            NOP,
            CWDE,CBW,
            CDQ,CWD,

            WAIT,
            SAHF,LAHF,
            STOS,LODS,SCAS,
            ROL,ROR,RCL,RCR,SAL,SAR,SHL,SHR,

            RETN,RETF,
            LES,LDS,LSS,LFS,LGS,
            ENTER,LEAVE,
            INT3,INT,INTO,INT1,//Call to Interrupt Procedure
            IRETW,IRETD,
            HLT,CMC,
            CLC,STC,CLI,STI,CLD,STD,

            SLDT,STR,LLDT,LTR,VERR,VERW,

            LOOPNE,LOOPNZ,LOOPE,LOOPZ,LOOP,
            JCXZ,JECXZ,//0x67実装したら
            UD1,
            BTR,BT,BTS,BTC,BSF,BSR,
            SETO,SETNO,SETB,SETAE,SETZ,SETNZ,SETBE,SETA,SETS,SETNS,SETP,SETNP,SETL,SETGE,SETLE,SETG,

            FLD,FST,FSTP,FSTOR,FNSAVE,FNSTSW,FFREE,FUCOM,FUCOMP,FLDENV,FLDCW,FNSTENV,FNSTCW,

            FIADD,FIMUL,FICOM,FICOMP,FISUB,FISUBR,FIDIV,FIDIVR,
            FCMOVB,FCMOVE,FCMOVBE,FCMOVBU,FUCOMPP,

            FILD,FIST,FISTP,FBLD,FNCLEX,FNINIT,FUCOMI,FCOMI,

            FADD,FMUL,FCOM,FCOMP,FSUB,FSUBR,FDIV,FDIVR,
            FADDP,FMULP,FCOMPP,FSUBRP,FSUBP,FDIVRP,FDIVP,

            FBSTP,FUCOMIP,FCOMIP,
            FXCH,
            
            SHRD,XLAT,SETALC,
            CVTPI2PS,CVTTPS2PI,CVTPS2PI,UCOMISS,COMISS,
            FXRSTOR,LDMXCSR,STMXCSR,XSAVE,XSTOR,
            PSRLQ, PSLLQ,


        }

        [Flags]
        public enum RegType
        {
            NULL = 0x00,
            EF = 0x01,//EFlags
            R16 = 0x02,//下位16bit
            H = 0x04,//上位8bit
            L = 0x08,//下位8bit

            //汎用データレジスタ
            EAX = 0x10, ECX = 0x20, EDX = 0x40, EBX = 0x80, ESP = 0x100, EBP = 0x200, ESI = 0x400, EDI = 0x800,

            //セグメントレジスタ
            ES = 0x1000, CS = 0x2000, SS = 0x4000, DS = 0x8000, FS = 0x10000, GS = 0x20000, SEG6 = 0x40000, SEG7 = 0x80000,

            //制御レジスタ(CR0~CR4)
            CR1 = 0x100000,

            //デバッグレジスタ（DR0~DR7）
            DR1 = 0x200000,

            FD = 0x2000000,//EFlags全て
            FW = FD | R16,

            AD = EAX | ECX | EDX | EBX | ESP | EBP | ESI | EDI,
            A = AD | R16,

            AX = EAX | R16, CX = ECX | R16, DX = EDX | R16, BX = EBX | R16,//16bit
            SP = ESP | R16, BP = EBP | R16, SI = ESI | R16, DI = EDI | R16,

            AH = AX | H, CH = CX | H, DH = DX | H, BH = BX | H,//high 8bit
            AL = AX | L, CL = CX | L, DL = DX | L, BL = BX | L,//low 8bit
        }
        
        public enum OperandType
        {
            NULL=0,
            rm32, rm16, rm8,//r
            m3232, m1632,//PTR QWORD xS:[any]
            r32, r16, r8,//レジスタ
            sreg,//セグメントレジスタ
            rel8, rel32,//ジャンプ長
            ptr1632, ptr1616,//ジャンプポインタ
            imm8,imm16,imm32,//即値
            moffs8, moffs32,//レジスタ対ポインタ
            one,//整数1
            m16int,m32int,m64fp,//rm系のszなし
        }

        //3OP
        public InstructionDetail(InstructionType instruction, OperandType dstType, OperandType srcType1, OperandType srcType2, PacketReader br, int sz = 0)
        {
            Instruction = instruction;

            byte? modrm = null;
            Operand = new TupleIVC(GetIVCByValueType(dstType, ref modrm, br, sz), GetIVCByValueType(srcType1, ref modrm, br, sz), GetIVCByValueType(srcType2, ref modrm, br, sz));
        }

        //2OP
        public InstructionDetail(InstructionType instruction, OperandType srcType, OperandType dstType, PacketReader br, int sz = 0)
        {
            Instruction = instruction;

            byte? modrm = null;
            Operand = new PairIVC(GetIVCByValueType(srcType, ref modrm, br, sz), GetIVCByValueType(dstType, ref modrm, br, sz));
        }

        //1OP
        public InstructionDetail(InstructionType instruction, OperandType operandTyep, PacketReader br, int sz = 0)
        {
            Instruction = instruction;

            byte? modrm = null;
            Operand = GetIVCByValueType(operandTyep, ref modrm, br, sz);
        }

        //0OP
        public InstructionDetail(InstructionType instruction)
        {
            Instruction = instruction;
            Operand = null;
        }
        
        //全決め
        public InstructionDetail(InstructionType instruction, IIVConstituent operand)
        {
            Instruction = instruction;
            Operand = operand;
        }

        //reg1
        public InstructionDetail(InstructionType instruction, RegType reg, int sz = 0)
        {
            Instruction = instruction;
            switch (sz)
            {
                case 2:
                    Operand = new Register(reg | RegType.R16);
                    break;
                case 4:
                    Operand = new Register(reg);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        //reg2
        public InstructionDetail(InstructionType instruction, RegType srcReg, RegType dstReg, int sz = 0)
        {
            Instruction = instruction;
            switch (sz)
            {
                case 2:
                    Operand = new PairIVC(new Register(srcReg | RegType.R16), new Register(dstReg | RegType.R16));
                    break;
                case 4:
                    Operand = new PairIVC(new Register(srcReg), new Register(dstReg));
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        //REGに対する操作を得る
        static InstructionDetail GetRegOperandDetail(InstructionType instruction, RegType srcReg, OperandType dstPtr, PacketReader br, bool isReverse, int sz = 0)
        {
            InstructionDetail result = new InstructionDetail { Instruction = instruction };
            byte? modrm = null;
            switch (dstPtr)
            {
                case OperandType.moffs8:
                    if (!srcReg.HasFlag(RegType.L)) throw new FormatException("moffs8はレジスタにLフラグが必要です");

                    result.Operand = reversedPair(new Register(srcReg));
                    break;
                case OperandType.moffs32:
                    if (srcReg.HasFlag(RegType.L)) throw new FormatException("moffs32は32or16bitレジスタです");
                    switch (sz)
                    {
                        case 2:
                            result.Operand = reversedPair(new Register(srcReg | RegType.R16));
                            break;
                        case 4:
                            result.Operand = reversedPair(new Register(srcReg));
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                    break;
                default:
                    result.Operand = reversedPair(new Register(srcReg));
                    break;
            }

            //反転考慮
            IIVConstituent reversedPair(IIVConstituent ivc1)
            {
                var ivc2 = GetIVCByValueType(dstPtr, ref modrm, br, sz);
                return new PairIVC(isReverse ? ivc2 : ivc1, isReverse ? ivc1 : ivc2);
            }
            return result;
        }

        //moffs regがsrc
        public static InstructionDetail GetRegOperandDetail(InstructionType instruction, RegType srcReg, OperandType dstPtr, PacketReader br, int sz = 0)
            => GetRegOperandDetail(instruction, srcReg, dstPtr, br, false, sz);

        //moffs ptrがsrc
        public static InstructionDetail GetRegOperandDetail(InstructionType instruction, OperandType srcPtr, RegType dstReg, PacketReader br, int sz = 0)
            => GetRegOperandDetail(instruction, dstReg, srcPtr, br, true, sz);

        //[MOVS, CMPS, ...]についての詳細
        public static InstructionDetail GetxxxSDetail(InstructionType s_inst, int sz = 0, bool singleIVC = false)
        {
            if (s_inst != InstructionType.MOVS && s_inst != InstructionType.CMPS &&
                s_inst != InstructionType.STOS && s_inst != InstructionType.LODS && s_inst != InstructionType.SCAS)
                throw new FormatException("xxxS系の命令のみです");

            InstructionDetail result = new InstructionDetail { Instruction = s_inst };

            var ptrType = getPtrType(sz);
            IIVConstituent srcPtr = new PtrIVC(ptrType, new Register(RegType.EDI));
            IIVConstituent dstPtr = new PtrIVC(ptrType, new Register(RegType.ESI));
            IIVConstituent ptrPair = new PairIVC(srcPtr, dstPtr);

            result.Operand = singleIVC ? srcPtr : ptrPair;
            return result;
        }

        //未知のオペコード
        public static InstructionDetail GetUnknownOpecodeDetail(params byte[] unknownCode)
        {
            switch (unknownCode.Length)
            {
                case 1:
                    return new InstructionDetail(InstructionType.DB, new DB(unknownCode[0]));
                case 2:
                    return new InstructionDetail(InstructionType.DW, new DW(BitConverter.ToUInt16(unknownCode, 0)));
                case 3:
                    var unkCodeList = unknownCode.ToList();
                    unkCodeList.Add(0);
                    return new InstructionDetail(InstructionType.DD, new DD(BitConverter.ToUInt32(unkCodeList.ToArray(), 0)));
                case 4:
                    return new InstructionDetail(InstructionType.DD, new DD(BitConverter.ToUInt32(unknownCode, 0)));
                default:
                    throw new ArgumentException("codeは1byte, 2byte, 4byte");
            }
        }
        public static InstructionDetail GetUnknownOpecodeDetail(uint unknownCode)
            => new InstructionDetail(InstructionType.DD, new DD(unknownCode));

        /// <summary>
        /// typeからIVC取得
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        static IIVConstituent GetIVCByValueType(OperandType type, ref byte? modrm, PacketReader br, int sz)
        {
            switch (type)
            {
                case OperandType.rm32:
                    modrm = modrm ?? br.ReadByte();
                    return GetRm_IVC(modrm.Value, br, sz, 0);
                case OperandType.rm16:
                    modrm = modrm ?? br.ReadByte();
                    return GetRm_IVC(modrm.Value, br, sz^6, 0);
                case OperandType.rm8:
                    modrm = modrm ?? br.ReadByte();
                    return GetRm_IVC(modrm.Value, br, 0, 0);

                case OperandType.m1632:
                    modrm = modrm ?? br.ReadByte();
                    return GetRm_IVC(modrm.Value, br, sz, 2);
                case OperandType.m3232:
                    modrm = modrm ?? br.ReadByte();
                    return GetRm_IVC(modrm.Value, br, sz, 4);

                case OperandType.m16int:
                    modrm = modrm ?? br.ReadByte();
                    return GetRm_IVC(modrm.Value, br, 4, -2);
                case OperandType.m32int:
                    modrm = modrm ?? br.ReadByte();
                    return GetRm_IVC(modrm.Value, br, 4, 0);
                case OperandType.m64fp:
                    modrm = modrm ?? br.ReadByte();
                    return GetRm_IVC(modrm.Value, br, 4, 4);

                case OperandType.r32:
                    modrm = modrm ?? br.ReadByte();
                    return new Register(SelectRegBysz(GetRegister(GetReg(modrm.Value)), sz));
                case OperandType.r16:
                    modrm = modrm ?? br.ReadByte();
                    return new Register(SelectRegBysz(GetRegister(GetReg(modrm.Value)), sz^6));
                case OperandType.r8:
                    modrm = modrm ?? br.ReadByte();
                    return new Register(SelectRegBysz(GetRegister(GetReg(modrm.Value)), 0));
                case OperandType.sreg:
                    modrm = modrm ?? br.ReadByte();
                    return new Register(GetSegmentRegister(GetReg(modrm.Value)));

                case OperandType.rel8:
                    return new Rel8(br.ReadSByte());
                case OperandType.rel32:
                    switch (sz)
                    {
                        case 4:
                            return new Rel32(br.ReadInt32());
                        case 2:
                            return new Rel16(br.ReadInt16());
                        default:
                            throw new ArgumentOutOfRangeException();
                    }

                case OperandType.ptr1616:
                    return new Ptr1616(br.ReadUInt16(), br.ReadUInt16());
                case OperandType.ptr1632:
                    switch (sz)
                    {
                        case 4:
                            return new Ptr1632(br.ReadUInt32(), br.ReadUInt16());
                        case 2:
                            return new Ptr1616(br.ReadUInt16(), br.ReadUInt16());
                        default:
                            throw new ArgumentOutOfRangeException();
                    }

                case OperandType.moffs8:
                case OperandType.moffs32:
                    return new PtrIVC(getPtrType(sz), new Disp32(br.ReadUInt32()));

                case OperandType.imm8:
                    return new Disp8(br.ReadSByte());
                case OperandType.imm16:
                    return new Disp16(br.ReadUInt16());
                case OperandType.imm32:
                    switch (sz)
                    {
                        case 4:
                            return new Disp32(br.ReadUInt32());
                        case 2:
                            return new Disp16(br.ReadUInt16());
                        default:
                            throw new ArgumentOutOfRangeException();
                    }

                case OperandType.one:
                    return new Disp8(1);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        /// modrmからレジスタ操作のポインタやレジスタを取得
        /// </summary>
        /// <param name="modrm"></param>
        /// <param name="br"></param>
        /// <param name="sz"></param>
        /// <returns></returns>
        static IIVConstituent GetRm_IVC(byte modrm, PacketReader br, int sz, int ptrPlus)
        {
            byte rm = Getrm(modrm);
            byte mod = GetMod(modrm);

            //modからivc取得
            IIVConstituent GetIvcByMod(RegType reg, byte myrm)
            {
                switch (mod)
                {
                    case 0b00:
                        if (myrm == 0b101)
                        {
                            return new Disp32(br.ReadUInt32());
                        }
                        else
                        {
                            return new Register(reg);
                        }
                    case 0b01:
                        return new RegisterPlusDisp8(reg, br.ReadSByte());
                    case 0b10:
                        return new RegisterPlusDisp32(reg, br.ReadUInt32());
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            //ptrでwrap
            PtrIVC WrapPtr(IIVConstituent iivc) => new PtrIVC(getPtrType(sz, ptrPlus), iivc);

            //rm分岐
            switch (rm)
            {
                case byte _ when rm == 0b100 && mod != 0b11://sib
                    byte sibop = br.ReadByte();

                    byte Base = (byte)(sibop & 7);
                    byte Index = (byte)((sibop >> 3) & 7);
                    byte scale = (byte)(((sibop >> 6) & 3)*2);

                    RegType baseReg = GetRegister(Base);
                    RegType indexReg = GetRegister(Index);

                    //ptr
                    IIVConstituent inclusionIVC = GetIvcByMod(baseReg, Base);

                    //scale決定
                    if (scale == 0) return WrapPtr(inclusionIVC);
                    else return WrapPtr(new Sib(inclusionIVC, indexReg, scale));

                default://sib以外
                    RegType reg = GetRegister(rm);
                    switch (mod)
                    {
                        case 0b11:
                            return new Register(SelectRegByModrm(reg, sz, rm));
                        default:
                            return WrapPtr(GetIvcByMod(reg, rm));
                    }
            }
        }

        //Baseもしくはr/mからレジスタ取得
        public static RegType GetRegister(byte val)
        {
            switch (val)
            {
                case 0b000:
                    return RegType.EAX;
                case 0b001:
                    return RegType.ECX;
                case 0b010:
                    return RegType.EDX;
                case 0b011:
                    return RegType.EBX;
                case 0b100:
                    return RegType.ESP;
                case 0b101:
                    return RegType.EBP;
                case 0b110:
                    return RegType.ESI;
                case 0b111:
                    return RegType.EDI;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        //セグメントレジスタ取得
        public static RegType GetSegmentRegister(byte val)
        {
            switch (val)
            {
                case 0b000:
                    return RegType.ES;
                case 0b001:
                    return RegType.CS;
                case 0b010:
                    return RegType.SS;
                case 0b011:
                    return RegType.DS;
                case 0b100:
                    return RegType.FS;
                case 0b101:
                    return RegType.GS;
                case 0b110:
                    return RegType.SEG6;
                case 0b111:
                    return RegType.SEG7;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        //szからsized regを選択
        static RegType SelectRegBysz(RegType reg ,int sz)
        {
            switch (sz)
            {
                case 0:
                    return reg | RegType.R16 | RegType.L;
                case 2:
                    return reg | RegType.R16;
                case 4:
                    return reg;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        //szとmodrmからsized regを選択
        static RegType SelectRegByModrm(RegType reg, int sz, byte rm)
        {
            switch (sz)
            {
                case 0:
                    switch (rm)
                    {
                        case byte n when n <= 0b011:
                            return reg | RegType.R16 | RegType.L;
                        case 0b100:
                            return RegType.AH;
                        case 0b101:
                            return RegType.CH;
                        case 0b110:
                            return RegType.DH;
                        case 0b111:
                            return RegType.BH;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                case 2:
                    return reg | RegType.R16;
                case 4:
                    return reg;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        //ptrtype取得
        static PtrIVC.PtrType getPtrType(int sz, int ptrPlus = 0)
        {
            int ptrsz = sz + ptrPlus;
            switch (sz)
            {
                case 0:
                    return PtrIVC.PtrType.BYTE_PTR;
                case 2:
                    return PtrIVC.PtrType.WORD_PTR;
                case 4:
                    return PtrIVC.PtrType.DWORD_PTR;
                case 6:
                    return PtrIVC.PtrType.FWORD_PTR;
                case 8:
                    return PtrIVC.PtrType.QWORD_PTR;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        /// rm取得
        public static byte Getrm(byte modrm) => (byte)(modrm & 7);
        
        /// REG取得
        public static byte GetReg(byte modrm) => (byte)((modrm >> 3) & 7);
        
        /// MOD取得
        public static byte GetMod(byte modrm) => (byte)((modrm >> 6) & 3);

        public override string ToString()
            => $"{Instruction.ToString()} {Operand?.GetString()}";
    }
}
