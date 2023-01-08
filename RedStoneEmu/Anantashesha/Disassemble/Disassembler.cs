using RedStoneLib.Packets;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using static Anantashesha.Disassemble.InstructionDetail;

namespace Anantashesha.Disassemble
{
    class Disassembler
    {
        /// <summary>
        /// アドレス履歴
        /// </summary>
        List<uint> AddressHistory = new List<uint>();

        /// <summary>
        /// 現在のアドレス
        /// </summary>
        uint CurrentAddress;

        /// <summary>
        /// DataDword開始アドレス
        /// </summary>
        List<uint> DDAddress = new List<uint>();

        /// <summary>
        /// DataByte開始アドレス
        /// </summary>
        List<uint> DBAddress = new List<uint>();

        /// <summary>
        /// RegごとのDB開始アドレス
        /// </summary>
        Dictionary<RegType, uint> DBStartAddress = new Dictionary<RegType, uint>();

        /// <summary>
        /// データアドレス群
        /// </summary>
        List<uint> DataStartAddresses = new List<uint>();

        /// <summary>
        /// 関数スタック
        /// </summary>
        Stack<InstructionDetail> FunctionStack = new Stack<InstructionDetail>();

        /// <summary>
        /// レジスタごとの即値
        /// </summary>
        Dictionary<RegType, uint> ImmOfReg = new Dictionary<RegType, uint>();

        PacketReader br;

        const int _r = 1;//1byte mod
        const int ib = 1;//即値1byte
        const int iw = 2;//即値2byte
        const int id = 4;//即値4byte
        const int disp8 = 1;
        const int moffs8 = 1;
        const int disp32 = 4;
        const int moffs32 = 4;
        const int cb = 1;//8bit offset
        const int cd = 4;//32bit offset
        const int cp = cd;//ptr16:32
        const int SIB = 1;

        //opecode: 0x80~0x83
        static InstructionType[] list8x = new InstructionType[]
        {
            InstructionType.ADD,InstructionType.OR,InstructionType.ADC, InstructionType.SBB,
            InstructionType.AND, InstructionType.SUB, InstructionType.XOR, InstructionType.CMP
        };

        //byte取得してストリームもどす
        byte PseudoGetByte()
        {
            byte res = br.ReadByte();
            br.BaseStream.Position--;
            return res;
        }

        /// <summary>
        /// 上4bit
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        static byte Byte2(byte x)
            => (byte)((x >> 4) & 0x0F);

        /// <summary>
        /// 下4bit
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        static byte Byte1(byte x)
            => (byte)(x & 0x0F);

        /// <summary>
        /// REG取得して戻す
        /// </summary>
        /// <returns></returns>
        byte PseudoReg()
            => GetReg(PseudoGetByte());

        /// <summary>
        /// IIVCを目的型にキャスト
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="src"></param>
        /// <returns></returns>
        public bool CastIIVConstituent<T>(IIVConstituent src, out T dst)
            where T : IIVConstituent
        {
            bool teq<T1>() => typeof(T1) == typeof(T);

            //タプル・ペアの分解
            IIVConstituent[] ivcs = null;
            if (src is PairIVC pi)
            {
                if (teq<PairIVC>()) { dst = (T)(IIVConstituent)pi; return true; }
                ivcs = new IIVConstituent[] { pi.IVC1, pi.IVC2 };
            }
            else if (src is TupleIVC ti)
            {
                if (teq<TupleIVC>()) { dst = (T)(IIVConstituent)ti; return true; }
                ivcs = new IIVConstituent[] { ti.IVC1, ti.IVC2, ti.IVC3 };
            }
            else
            {
                ivcs = new IIVConstituent[] { src };
            }

            for (int i = 0; i < ivcs.Length; i++)
            {
                //分解
                IIVConstituent ivc = ivcs[i];
                if (ivc is PtrIVC ptr)//ptr分解
                {
                    if (teq<PtrIVC>()) { dst = (T)(IIVConstituent)ptr; return true; }
                    ivc = ptr.IVC;
                }
                if (ivc is Sib sib)//sib分解
                {
                    if (teq<Sib>()) { dst = (T)(IIVConstituent)sib; return true; }
                    ivc = sib.Base;
                }

                //チェック
                if (teq<Disp8>())
                {
                    if (ivc is Disp8 disp8) { dst = (T)(IIVConstituent)disp8; return true; }
                    if (ivc is RegisterPlusDisp8 reg8) { dst = (T)(IIVConstituent)reg8.Disp8; return true; }
                }
                if (teq<Disp32>())
                {
                    if (ivc is Disp32 disp32) { dst = (T)(IIVConstituent)disp32; return true; }
                    if (ivc is RegisterPlusDisp32 reg32) { dst = (T)(IIVConstituent)reg32.Disp32; return true; }
                }
                if (teq<Register>())
                {
                    if (ivc is Register reg) { dst = (T)(IIVConstituent)reg; return true; }
                    if (ivc is RegisterPlusDisp8 reg8) { dst = (T)(IIVConstituent)reg8.Reg; return true; }
                    if (ivc is RegisterPlusDisp32 reg32) { dst = (T)(IIVConstituent)reg32.Reg; return true; }
                }
                if (teq<RegisterPlusDisp32>() && ivc is RegisterPlusDisp32 regplus32) { dst = (T)(IIVConstituent)regplus32; return true; }
            }
            dst = default(T);
            return false;
        }
        public T CastIIVConstituent<T>(IIVConstituent src) where T : IIVConstituent
            => CastIIVConstituent<T>(src, out var dst) ? dst : throw new InvalidCastException("未知のキャストエラー");



        public void Disassemble(string fname)
        {
            List<InstructionDetail> result = new List<InstructionDetail>();

            using (FileStream fs = new FileStream(fname, FileMode.Open, FileAccess.Read))
            using (br = new PacketReader(fs))
            {
                //PEヘッダ
                _IMAGE_DOS_HEADER MsHeader = br.ReadStruct<_IMAGE_DOS_HEADER>();
                br.BaseStream.Seek(MsHeader.e_lfanew, SeekOrigin.Begin);

                IMAGE_NT_HEADERS NtHeader = br.ReadStruct<IMAGE_NT_HEADERS>();
                int num = (int)NtHeader.OptionalHeader.NumberOfRvaAndSizes;
                IMAGE_DATA_DIRECTORY[] DataDic = br.Reads<IMAGE_DATA_DIRECTORY>(num);
                IMAGE_SECTION_HEADER[] SectionHeaders = br.Reads<IMAGE_SECTION_HEADER>(num);

                //100に飛ぶ
                br.BaseStream.Seek(NtHeader.OptionalHeader.FileAlignment, SeekOrigin.Begin);

                while (br.BaseStream.Position < NtHeader.OptionalHeader.SizeOfCode)
                {
                    CurrentAddress = (uint)(NtHeader.OptionalHeader.ImageBase + br.BaseStream.Position);
                    AddressHistory.Add(CurrentAddress);

                    Console.Write($"{CurrentAddress:X8}: ");
                    
                    var detail = CheckPrefix();

                    if (detail.Instruction == InstructionType.MOV && ((PairIVC)detail.Operand).IVC1 is Register reg)
                    {
                        var movop = (PairIVC)detail.Operand;
                        if (movop.IVC2 is Disp32 movd32)
                        {
                            ImmOfReg[reg.Reg] = movd32._Disp32;
                        }
                        else if (movop.IVC2 is Disp8 movd8)
                        {
                            ImmOfReg[reg.Reg] = (uint)movd8._Disp8;
                        }
                    }

                    //Dataチェック
                    if (detail.Instruction == InstructionType.NOP)
                    {
                        if (FunctionStack.Count > 0) FunctionStack.Clear();
                    }
                    else if(detail.Instruction!=InstructionType.DD && detail.Instruction!=InstructionType.DB)
                    {
                        IIVConstituent ivc = detail.Operand;

                        //ptrタイプで分類
                        if (CastIIVConstituent<PtrIVC>(ivc, out var ptr) && CastIIVConstituent<Disp32>(ptr, out var disp32ivc))
                        {
                            uint disp32 = disp32ivc._Disp32;
                            var ptrType = ptr._PtrType;

                            if (disp32 >= NtHeader.OptionalHeader.ImageBase + NtHeader.OptionalHeader.SizeOfHeaders && disp32 < NtHeader.OptionalHeader.ImageBase + NtHeader.OptionalHeader.SizeOfCode && !DataStartAddresses.Contains(disp32))
                            {
                                //差分レジスタ
                                var indexReg = RegType.NULL;
                                if (CastIIVConstituent<Sib>(ivc, out var sib))
                                {
                                    indexReg = sib.Index;
                                }
                                else if (CastIIVConstituent<Register>(ptr, out var ptrreg))
                                {
                                    indexReg = ptrreg.Reg;
                                }
                                else
                                {
                                    throw new Exception("invalid Reg");
                                }
                                DataStartAddresses.Add(disp32);

                                switch (ptrType)
                                {
                                    case PtrIVC.PtrType.BYTE_PTR:
                                        FollowUntilGettingImm(DBAddress, indexReg, disp32, 1);
                                        if (CastIIVConstituent<Register>(ivc, out var breg))
                                        {
                                            var dstReg = (breg.Reg & ~ RegType.L) & ~ RegType.R16;
                                            if (DBStartAddress.ContainsKey(dstReg)) throw new Exception("DBのレジスタ重複");
                                            DBStartAddress[dstReg] = disp32;
                                        }
                                        break;
                                    case PtrIVC.PtrType.DWORD_PTR:
                                        if(DBStartAddress.ContainsKey(indexReg))
                                        {
                                            //DBのMAX+1がDDの数
                                            var sp = br.BaseStream.Position;
                                            br.BaseStream.Seek(DBStartAddress[indexReg] - NtHeader.OptionalHeader.ImageBase, SeekOrigin.Begin);
                                            int dbmax = br.ReadBytes(
                                                Enumerable.Range((int)DBStartAddress[indexReg], (int)NtHeader.OptionalHeader.SizeOfCode)
                                                .TakeWhile(t => DBAddress.Contains((uint)t)).Count())
                                                .Max() + 1;
                                            for (uint i = 0; i < dbmax; i++)
                                            {
                                                DDAddress.Add(disp32 + i * 4);
                                            }
                                            DBStartAddress.Remove(indexReg);
                                            br.BaseStream.Seek(sp, SeekOrigin.Begin);
                                        }
                                        else
                                        {
                                            FollowUntilGettingImm(DDAddress, indexReg, disp32, 4);
                                        }
                                        break;
                                    default:
                                        throw new NotImplementedException("PtrType未実装");
                                }
                            }
                        }
                        else
                        {
                            FunctionStack.Push(detail);
                        }
                    }

                    Console.WriteLine(detail.ToString());
                }
            }
        }
        
        /// <summary>
        /// レジスタの即値が得られるまで辿る
        /// </summary>
        /// <param name="list"></param>
        /// <param name="register"></param>
        /// <param name="disp32"></param>
        /// <param name="size"></param>
        void FollowUntilGettingImm(List<uint> list, RegType register, uint disp32, int size)
        {
            if (register == RegType.NULL) throw new InvalidOperationException("DataByteをたどる命令はレジスターが伴います");
            while (FunctionStack.Count > 0)
            {
                var histroy = FunctionStack.Pop();
                if (histroy.Instruction == InstructionType.CMP && (!CastIIVConstituent<Register>(histroy.Operand, out var cmpreg) || cmpreg.Reg == register) ||
                    (histroy.Instruction == InstructionType.MOV || histroy.Instruction == InstructionType.AND) && CastIIVConstituent<Register>(histroy.Operand, out var movreg) && movreg.Reg == register)
                {
                    int cmpCount = 0;
                    if (CastIIVConstituent<Disp8>(histroy.Operand, out var disp8cnt))
                    {
                        cmpCount = disp8cnt._Disp8 + 1;
                    }
                    else if (CastIIVConstituent<Disp32>(histroy.Operand, out var disp32cnt))
                    {
                        cmpCount = (int)disp32cnt._Disp32 + 1;
                    }
                    else
                    {
                        //レジスタしかない
                        register = ((Register)(((PairIVC)histroy.Operand).IVC2)).Reg;
                        cmpCount = (int)ImmOfReg[register];
                        if (cmpCount > 0x10)
                        {
                            //Register特定不可能
                            break;
                        }
                    }
                    for (uint i = 0; i < cmpCount; i++)
                    {
                        list.Add((uint)(disp32 + i * size));
                    }
                    break;
                }
            }
        }

        /// <summary>
        /// 接頭辞チェック
        /// </summary>
        /// <param name="br"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        InstructionDetail CheckPrefix(int count = 0, int opsz = 4)
        {
            if (count > 4 || DBAddress.Contains(CurrentAddress) || DDAddress.Contains(CurrentAddress))
            {
                return GetInsturctionDetile(opsz);
            }

            byte prefix = br.ReadByte();
            switch (prefix)
            {
                case 0xF0:
                case 0xF2://group1
                case 0xF3://REP
                case 0x2E:
                case 0x36:
                case 0x3E:
                case 0x26:
                case 0x64:
                case 0x65://group2
                case 0x67://group4
                    return CheckPrefix(count + 1);
                case 0x66://group3
                    return CheckPrefix(count + 1, opsz ^ 6);
                default:
                    br.BaseStream.Position--;
                    return GetInsturctionDetile(opsz);

            }
        }

        InstructionDetail GetInsturctionDetile(int opsz)
        {
            byte opecode = br.ReadByte();

            //0x~3xまでのスタイル
            InstructionDetail GetDetailBy0x1x2c3xStyle(InstructionType type)
            {
                switch (Byte1(opecode))
                {
                    case 0x00:
                    case 0x08:
                        return new InstructionDetail(type, OperandType.rm32, OperandType.r32, br);
                    case 0x01:
                    case 0x09:
                        return new InstructionDetail(type, OperandType.rm32, OperandType.r32, br, opsz);
                    case 0x02:
                    case 0x0A:
                        return new InstructionDetail(type, OperandType.r32, OperandType.rm32, br);
                    case 0x03:
                    case 0x0B:
                        return new InstructionDetail(type, OperandType.r32, OperandType.rm32, br, opsz);
                    case 0x04:
                    case 0x0C:
                        return GetRegOperandDetail(type, RegType.AL, OperandType.imm8, br, opsz);
                    case 0x05:
                    case 0x0D:
                        return new InstructionDetail(type, OperandType.imm32, br, opsz);
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            //0x~3xまでのスタイル（push pop）
            InstructionDetail GetPPDetailBy0x1x2c3xStyle(RegType reg)
            {
                switch (Byte1(opecode))
                {
                    case 0x06:
                    case 0x0E:
                        return new InstructionDetail(InstructionType.PUSH, reg, opsz);
                    case 0x07:
                    case 0x0F:
                        return new InstructionDetail(InstructionType.POP, reg, opsz);
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            //動的オペコード・静的オペランドの命令がRegで分岐する時のInstructionDetail
            InstructionDetail GetDetailByReg_Dic(Dictionary<int, InstructionType> instDic, params OperandType[] operandTypes)
            {
                try
                {
                    switch (operandTypes.Length)
                    {
                        case 0:
                            return new InstructionDetail(instDic[PseudoReg()]);
                        case 1:
                            return new InstructionDetail(instDic[PseudoReg()], operandTypes[0], br, opsz);
                        case 2:
                            return new InstructionDetail(instDic[PseudoReg()], operandTypes[0], operandTypes[1], br, opsz);
                        case 3:
                            return new InstructionDetail(instDic[PseudoReg()], operandTypes[0], operandTypes[1], operandTypes[2], br, opsz);
                        default:
                            throw new ArgumentOutOfRangeException("InstructionDetailは0~3までの長さのOperandTypeです");
                    }
                }
                catch (KeyNotFoundException)
                {
                    return GetUnknownOpecodeDetail(opecode, br.ReadByte());
                }
            }
            //上記の配列版
            InstructionDetail GetDetailByReg(InstructionType[] insts, params OperandType[] operandTypes)
                => GetDetailByReg_Dic(insts.Select((v, i) => new { item = v, index = i }).ToDictionary(t => t.index, t => t.item), operandTypes);
            
            //データ領域判定
            if (DDAddress.Contains(CurrentAddress))
            {
                DDAddress.Remove(CurrentAddress);
                br.BaseStream.Position--;
                return GetUnknownOpecodeDetail(br.ReadUInt32());
            }
            if (DBAddress.Contains(CurrentAddress))
            {
                if (DBStartAddress.Count > 0) DBStartAddress.Clear();
                DBAddress.Remove(CurrentAddress);
                return GetUnknownOpecodeDetail(opecode);
            }

            switch (opecode)
            {
                case byte n when n <= 0x05:
                    return GetDetailBy0x1x2c3xStyle(InstructionType.ADD);
                case byte n when n <= 0x07:
                    return GetPPDetailBy0x1x2c3xStyle(RegType.ES);
                case byte n when n <= 0x0D:
                    return GetDetailBy0x1x2c3xStyle(InstructionType.OR);
                case 0x0E:
                    return new InstructionDetail(InstructionType.PUSH, RegType.CS, opsz);
                case 0x0F:
                    return GetExpansionInsturctionSize(opsz);
                case byte n when n <= 0x15:
                    return GetDetailBy0x1x2c3xStyle(InstructionType.ADC);
                case byte n when n <= 0x17:
                    return GetPPDetailBy0x1x2c3xStyle(RegType.SS);
                case byte n when n <= 0x1D:
                    return GetDetailBy0x1x2c3xStyle(InstructionType.SBB);
                case byte n when n <= 0x1F:
                    return GetPPDetailBy0x1x2c3xStyle(RegType.DS);

                case byte n when n <= 0x25:
                    return GetDetailBy0x1x2c3xStyle(InstructionType.AND);
                case 0x26:
                    throw new NotImplementedException("セグメントオーバーライドプリフィックス ES");
                case 0x27:
                    return new InstructionDetail(InstructionType.DAA);
                case byte n when n <= 0x2D:
                    return GetDetailBy0x1x2c3xStyle(InstructionType.SUB);
                case 0x2E:
                    throw new NotImplementedException("セグメントオーバーライドプリフィックス CS");
                case 0x2F:
                    return new InstructionDetail(InstructionType.DAS);
                case byte n when n <= 0x35:
                    return GetDetailBy0x1x2c3xStyle(InstructionType.XOR);
                case 0x36:
                    throw new NotImplementedException("セグメントオーバーライドプリフィックス SS");
                case 0x37:
                    return new InstructionDetail(InstructionType.AAA);
                case byte n when n <= 0x3D:
                    return GetDetailBy0x1x2c3xStyle(InstructionType.CMP);
                case 0x3E:
                    throw new NotImplementedException("セグメントオーバーライドプリフィックス DS");
                case 0x3F:
                    return new InstructionDetail(InstructionType.AAS);

                case byte n when n <= 0x47:
                    return new InstructionDetail(InstructionType.INC, GetRegister(Byte1(opecode)), opsz);
                case byte n when n <= 0x4F:
                    return new InstructionDetail(InstructionType.DEC, GetRegister((byte)(Byte1(opecode) - 8)), opsz);
                case byte n when n <= 0x57:
                    return new InstructionDetail(InstructionType.PUSH, GetRegister(Byte1(opecode)), opsz);
                case byte n when n <= 0x5F:
                    return new InstructionDetail(InstructionType.POP, GetRegister((byte)(Byte1(opecode) - 8)), opsz);

                case 0x60:
                    return new InstructionDetail(InstructionType.PUSH, RegType.AD, opsz);
                case 0x61:
                    return new InstructionDetail(InstructionType.POP, RegType.AD, opsz);
                case 0x62:
                    return new InstructionDetail(InstructionType.BOUND, OperandType.r32, OperandType.m3232, br, opsz);
                case 0x63:
                    return new InstructionDetail(InstructionType.ARPL, OperandType.rm16, OperandType.r16, br, opsz);
                case 0x64:
                    throw new NotImplementedException("セグメントオーバーライドプリフィックス FS");
                case 0x65:
                    throw new NotImplementedException("セグメントオーバーライドプリフィックス GS");
                case 0x66:
                    throw new NotImplementedException("オペランドサイズプリフィックス");
                case 0x67:
                    throw new NotImplementedException("アドレスサイズプリフィックス");
                case 0x68:
                    return new InstructionDetail(InstructionType.PUSH, OperandType.imm32, br, opsz);
                case 0x6A:
                    return new InstructionDetail(InstructionType.PUSH, OperandType.imm8, br);
                case 0x69:
                    return new InstructionDetail(InstructionType.IMUL, OperandType.r32, OperandType.rm32, OperandType.imm32, br, opsz);
                case 0x6B:
                    return new InstructionDetail(InstructionType.IMUL, OperandType.r32, OperandType.rm32, OperandType.imm8, br, opsz);
                case 0x6C:
                case 0x6D:
                    return new InstructionDetail(InstructionType.INS);
                case 0x6E:
                case 0x6F:
                    return new InstructionDetail(InstructionType.OUTS);

                case 0x70:
                    return new InstructionDetail(InstructionType.JO, OperandType.rel8, br);
                case 0x71:
                    return new InstructionDetail(InstructionType.JNO, OperandType.rel8, br);
                case 0x72:
                    return new InstructionDetail(InstructionType.JB, OperandType.rel8, br);
                case 0x73:
                    return new InstructionDetail(InstructionType.JAE, OperandType.rel8, br);
                case 0x74:
                    return new InstructionDetail(InstructionType.JZ, OperandType.rel8, br);
                case 0x75:
                    return new InstructionDetail(InstructionType.JNZ, OperandType.rel8, br);
                case 0x76:
                    return new InstructionDetail(InstructionType.JBE, OperandType.rel8, br);
                case 0x77:
                    return new InstructionDetail(InstructionType.JA, OperandType.rel8, br);
                case 0x78:
                    return new InstructionDetail(InstructionType.JS, OperandType.rel8, br);
                case 0x79:
                    return new InstructionDetail(InstructionType.JNS, OperandType.rel8, br);
                case 0x7A:
                    return new InstructionDetail(InstructionType.JP, OperandType.rel8, br);
                case 0x7B:
                    return new InstructionDetail(InstructionType.JNP, OperandType.rel8, br);
                case 0x7C:
                    return new InstructionDetail(InstructionType.JL, OperandType.rel8, br);
                case 0x7D:
                    return new InstructionDetail(InstructionType.JGE, OperandType.rel8, br);
                case 0x7E:
                    return new InstructionDetail(InstructionType.JLE, OperandType.rel8, br);
                case 0x7F:
                    return new InstructionDetail(InstructionType.JG, OperandType.rel8, br);

                case 0x80:
                case 0x82:
                    return GetDetailByReg(list8x, OperandType.rm8, OperandType.imm8);
                case 0x81:
                    return GetDetailByReg(list8x, OperandType.rm32, OperandType.imm32);
                case 0x83:
                    return GetDetailByReg(list8x, OperandType.rm32, OperandType.imm8);
                case 0x84:
                    return new InstructionDetail(InstructionType.TEST, OperandType.rm8, OperandType.r8, br);
                case 0x85:
                    return new InstructionDetail(InstructionType.TEST, OperandType.rm32, OperandType.r32, br, opsz);
                case 0x86:
                    return new InstructionDetail(InstructionType.XCHG, OperandType.r8, OperandType.rm8, br);
                case 0x87:
                    return new InstructionDetail(InstructionType.XCHG, OperandType.r32, OperandType.rm32, br, opsz);
                case 0x88:
                    return new InstructionDetail(InstructionType.MOV, OperandType.rm8, OperandType.r8, br);
                case 0x89:
                    return new InstructionDetail(InstructionType.MOV, OperandType.rm32, OperandType.r32, br, opsz);
                case 0x8A:
                    return new InstructionDetail(InstructionType.MOV, OperandType.r8, OperandType.rm8, br);
                case 0x8B:
                    return new InstructionDetail(InstructionType.MOV, OperandType.r32, OperandType.rm32, br, opsz);
                case 0x8C:
                    return new InstructionDetail(InstructionType.MOV, OperandType.rm16, OperandType.sreg, br, opsz);
                case 0x8D:
                    return new InstructionDetail(InstructionType.LEA, OperandType.r32, OperandType.rm32, br, opsz);
                case 0x8E:
                    return new InstructionDetail(InstructionType.MOV, OperandType.sreg, OperandType.rm16, br, opsz);
                case 0x8F:// XOPプリフィックス
                    return GetDetailByReg(new InstructionType[] { InstructionType.POP }, OperandType.rm32);

                case 0x90://nop
                    return new InstructionDetail(InstructionType.NOP);
                case byte n when n <= 0x97:
                    return new InstructionDetail(InstructionType.XCHG, RegType.EAX, GetRegister(Byte1(n)), opsz);
                case 0x98:
                    return new InstructionDetail(opsz == 4 ? InstructionType.CWDE : InstructionType.CBW);
                case 0x99:
                    return new InstructionDetail(opsz == 4 ? InstructionType.CDQ : InstructionType.CWD);
                case 0x9A://far call
                    return new InstructionDetail(InstructionType.CALL, OperandType.ptr1632, br, opsz);
                case 0x9B://wait
                    return new InstructionDetail(InstructionType.WAIT);
                case 0x9C:
                    return new InstructionDetail(InstructionType.PUSH, RegType.FD, opsz);
                case 0x9D:
                    return new InstructionDetail(InstructionType.POP, RegType.FD, opsz);
                case 0x9E:
                    return new InstructionDetail(InstructionType.SAHF);
                case 0x9F:
                    return new InstructionDetail(InstructionType.LAHF);

                case 0xA0:
                    return GetRegOperandDetail(InstructionType.MOV, RegType.AL, OperandType.moffs8, br);
                case 0xA1:
                    return GetRegOperandDetail(InstructionType.MOV, RegType.EAX, OperandType.moffs32, br, opsz);
                case 0xA2:
                    return GetRegOperandDetail(InstructionType.MOV, OperandType.moffs8, RegType.AL, br);
                case 0xA3:
                    return GetRegOperandDetail(InstructionType.MOV, OperandType.moffs32, RegType.EAX, br, opsz);
                case 0xA4:
                    return GetxxxSDetail(InstructionType.MOVS);
                case 0xA5:
                    return GetxxxSDetail(InstructionType.MOVS, opsz);
                case 0xA6:
                    return GetxxxSDetail(InstructionType.CMPS);
                case 0xA7:
                    return GetxxxSDetail(InstructionType.CMPS, opsz);
                case 0xA8:
                    return GetRegOperandDetail(InstructionType.TEST, RegType.AL, OperandType.imm8, br);
                case 0xA9:
                    return GetRegOperandDetail(InstructionType.TEST, RegType.EAX, OperandType.imm32, br, opsz);
                case 0xAA:
                    return GetxxxSDetail(InstructionType.STOS, singleIVC: true);
                case 0xAB:
                    return GetxxxSDetail(InstructionType.STOS, opsz, singleIVC: true);
                case 0xAC:
                    return GetxxxSDetail(InstructionType.LODS, singleIVC: true);
                case 0xAD:
                    return GetxxxSDetail(InstructionType.LODS, opsz, singleIVC: true);
                case 0xAE:
                    return GetxxxSDetail(InstructionType.SCAS, singleIVC: true);
                case 0xAF:
                    return GetxxxSDetail(InstructionType.SCAS, opsz, singleIVC: true);

                case 0xB0:
                    return GetRegOperandDetail(InstructionType.MOV, RegType.AL, OperandType.imm8, br);
                case 0xB1:
                    return GetRegOperandDetail(InstructionType.MOV, RegType.CL, OperandType.imm8, br);
                case 0xB2:
                    return GetRegOperandDetail(InstructionType.MOV, RegType.DL, OperandType.imm8, br);
                case 0xB3:
                    return GetRegOperandDetail(InstructionType.MOV, RegType.BL, OperandType.imm8, br);
                case 0xB4:
                    return GetRegOperandDetail(InstructionType.MOV, RegType.AH, OperandType.imm8, br);
                case 0xB5:
                    return GetRegOperandDetail(InstructionType.MOV, RegType.CH, OperandType.imm8, br);
                case 0xB6:
                    return GetRegOperandDetail(InstructionType.MOV, RegType.DH, OperandType.imm8, br);
                case 0xB7:
                    return GetRegOperandDetail(InstructionType.MOV, RegType.BH, OperandType.imm8, br);
                case 0xB8:
                    return GetRegOperandDetail(InstructionType.MOV, RegType.EAX, OperandType.imm32, br, opsz);
                case 0xB9:
                    return GetRegOperandDetail(InstructionType.MOV, RegType.ECX, OperandType.imm32, br, opsz);
                case 0xBA:
                    return GetRegOperandDetail(InstructionType.MOV, RegType.EDX, OperandType.imm32, br, opsz);
                case 0xBB:
                    return GetRegOperandDetail(InstructionType.MOV, RegType.EBX, OperandType.imm32, br, opsz);
                case 0xBC:
                    return GetRegOperandDetail(InstructionType.MOV, RegType.ESP, OperandType.imm32, br, opsz);
                case 0xBD:
                    return GetRegOperandDetail(InstructionType.MOV, RegType.EBP, OperandType.imm32, br, opsz);
                case 0xBE:
                    return GetRegOperandDetail(InstructionType.MOV, RegType.ESI, OperandType.imm32, br, opsz);
                case 0xBF:
                    return GetRegOperandDetail(InstructionType.MOV, RegType.EDI, OperandType.imm32, br, opsz);

                case 0xC0:
                    switch (PseudoReg())
                    {
                        case 0:
                            return new InstructionDetail(InstructionType.ROL, OperandType.rm8, OperandType.imm8, br);
                        case 1:
                            return new InstructionDetail(InstructionType.ROR, OperandType.rm8, OperandType.imm8, br);
                        case 2:
                            return new InstructionDetail(InstructionType.RCL, OperandType.rm8, OperandType.imm8, br);
                        case 3:
                            return new InstructionDetail(InstructionType.RCR, OperandType.rm8, OperandType.imm8, br);
                        case 4:
                            return new InstructionDetail(InstructionType.SHL, OperandType.rm8, OperandType.imm8, br);
                        case 5:
                            return new InstructionDetail(InstructionType.SHR, OperandType.rm8, OperandType.imm8, br);
                        case 6:
                            return new InstructionDetail(InstructionType.SAL, OperandType.rm8, OperandType.imm8, br);
                        case 7:
                            return new InstructionDetail(InstructionType.SAR, OperandType.rm8, OperandType.imm8, br);
                        default:
                            throw new IndexOutOfRangeException();
                    }
                case 0xC1:
                    switch (PseudoReg())
                    {
                        case 0:
                            return new InstructionDetail(InstructionType.ROL, OperandType.rm32, OperandType.imm8, br, opsz);
                        case 1:
                            return new InstructionDetail(InstructionType.ROR, OperandType.rm32, OperandType.imm8, br, opsz);
                        case 2:
                            return new InstructionDetail(InstructionType.RCL, OperandType.rm32, OperandType.imm8, br, opsz);
                        case 3:
                            return new InstructionDetail(InstructionType.RCR, OperandType.rm32, OperandType.imm8, br, opsz);
                        case 4:
                            return new InstructionDetail(InstructionType.SHL, OperandType.rm32, OperandType.imm8, br, opsz);
                        case 5:
                            return new InstructionDetail(InstructionType.SHR, OperandType.rm32, OperandType.imm8, br, opsz);
                        case 6:
                            return new InstructionDetail(InstructionType.SAL, OperandType.rm32, OperandType.imm8, br, opsz);
                        case 7:
                            return new InstructionDetail(InstructionType.SAR, OperandType.rm32, OperandType.imm8, br, opsz);
                        default:
                            throw new IndexOutOfRangeException();
                    }
                case 0xC2:
                    return new InstructionDetail(InstructionType.RETN, OperandType.imm16, br);
                case 0xC3:
                    return new InstructionDetail(InstructionType.RETN);
                case 0xC4:
                    return new InstructionDetail(InstructionType.LES, OperandType.r32, OperandType.m1632, br, opsz);
                case 0xC5:
                    return new InstructionDetail(InstructionType.LDS, OperandType.r32, OperandType.m1632, br, opsz);
                case 0xC6:
                    switch (PseudoReg())
                    {
                        case 0:
                            return new InstructionDetail(InstructionType.MOV, OperandType.rm8, OperandType.imm8, br);
                        default:
                            return GetUnknownOpecodeDetail(opecode, br.ReadByte());
                    }
                case 0xC7:
                    switch (PseudoReg())
                    {
                        case 0:
                            return new InstructionDetail(InstructionType.MOV, OperandType.rm32, OperandType.imm32, br, opsz);
                        default:
                            return GetUnknownOpecodeDetail(opecode, br.ReadByte());
                    }
                case 0xC8:
                    return new InstructionDetail(InstructionType.ENTER, OperandType.imm16, OperandType.imm8, br);
                case 0xC9:
                    return new InstructionDetail(InstructionType.LEAVE);
                case 0xCA://RET
                    return new InstructionDetail(InstructionType.RETF, OperandType.imm32, br, opsz);
                case 0xCB:
                    return new InstructionDetail(InstructionType.RETF);
                case 0xCC:
                    return new InstructionDetail(InstructionType.INT3);
                case 0xCD:
                    return new InstructionDetail(InstructionType.INT, OperandType.imm8, br);
                case 0xCE:
                    return new InstructionDetail(InstructionType.INTO);
                case 0xCF:
                    return new InstructionDetail(opsz == 4 ? InstructionType.IRETD : InstructionType.IRETW);

                case 0xD0:
                    switch (PseudoReg())
                    {
                        case 0:
                            return new InstructionDetail(InstructionType.ROL, OperandType.rm8, OperandType.one, br);
                        case 1:
                            return new InstructionDetail(InstructionType.ROR, OperandType.rm8, OperandType.one, br);
                        case 2:
                            return new InstructionDetail(InstructionType.RCL, OperandType.rm8, OperandType.one, br);
                        case 3:
                            return new InstructionDetail(InstructionType.RCR, OperandType.rm8, OperandType.one, br);
                        case 4:
                            return new InstructionDetail(InstructionType.SHL, OperandType.rm8, OperandType.one, br);
                        case 5:
                            return new InstructionDetail(InstructionType.SHR, OperandType.rm8, OperandType.one, br);
                        case 6:
                            return new InstructionDetail(InstructionType.SAL, OperandType.rm8, OperandType.one, br);
                        case 7:
                            return new InstructionDetail(InstructionType.SAR, OperandType.rm8, OperandType.one, br);
                        default:
                            return GetUnknownOpecodeDetail(opecode, br.ReadByte());
                    }
                case 0xD1:
                    switch (PseudoReg())
                    {
                        case 0:
                            return new InstructionDetail(InstructionType.ROL, OperandType.rm32, OperandType.one, br, opsz);
                        case 1:
                            return new InstructionDetail(InstructionType.ROR, OperandType.rm32, OperandType.one, br, opsz);
                        case 2:
                            return new InstructionDetail(InstructionType.RCL, OperandType.rm32, OperandType.one, br, opsz);
                        case 3:
                            return new InstructionDetail(InstructionType.RCR, OperandType.rm32, OperandType.one, br, opsz);
                        case 4:
                            return new InstructionDetail(InstructionType.SHL, OperandType.rm32, OperandType.one, br, opsz);
                        case 5:
                            return new InstructionDetail(InstructionType.SHR, OperandType.rm32, OperandType.one, br, opsz);
                        case 6:
                            return new InstructionDetail(InstructionType.SAL, OperandType.rm32, OperandType.one, br, opsz);
                        case 7:
                            return new InstructionDetail(InstructionType.SAR, OperandType.rm32, OperandType.one, br, opsz);
                        default:
                            return GetUnknownOpecodeDetail(opecode, br.ReadByte());
                    }
                case 0xD2:
                    switch (PseudoReg())
                    {
                        case 0:
                            return GetRegOperandDetail(InstructionType.ROL, OperandType.rm8, RegType.CL, br);
                        case 1:
                            return GetRegOperandDetail(InstructionType.ROR, OperandType.rm8, RegType.CL, br);
                        case 2:
                            return GetRegOperandDetail(InstructionType.RCL, OperandType.rm8, RegType.CL, br);
                        case 3:
                            return GetRegOperandDetail(InstructionType.RCR, OperandType.rm8, RegType.CL, br);
                        case 4:
                            return GetRegOperandDetail(InstructionType.SHL, OperandType.rm8, RegType.CL, br);
                        case 5:
                            return GetRegOperandDetail(InstructionType.SHR, OperandType.rm8, RegType.CL, br);
                        case 6:
                            return GetRegOperandDetail(InstructionType.SAL, OperandType.rm8, RegType.CL, br);
                        case 7:
                            return GetRegOperandDetail(InstructionType.SAR, OperandType.rm8, RegType.CL, br);
                        default:
                            return GetUnknownOpecodeDetail(opecode, br.ReadByte());
                    }
                case 0xD3:
                    switch (PseudoReg())
                    {
                        case 0:
                            return GetRegOperandDetail(InstructionType.ROL, OperandType.rm32, RegType.CL, br, opsz);
                        case 1:
                            return GetRegOperandDetail(InstructionType.ROR, OperandType.rm32, RegType.CL, br, opsz);
                        case 2:
                            return GetRegOperandDetail(InstructionType.RCL, OperandType.rm32, RegType.CL, br, opsz);
                        case 3:
                            return GetRegOperandDetail(InstructionType.RCR, OperandType.rm32, RegType.CL, br, opsz);
                        case 4:
                            return GetRegOperandDetail(InstructionType.SHL, OperandType.rm32, RegType.CL, br, opsz);
                        case 5:
                            return GetRegOperandDetail(InstructionType.SHR, OperandType.rm32, RegType.CL, br, opsz);
                        case 6:
                            return GetRegOperandDetail(InstructionType.SAL, OperandType.rm32, RegType.CL, br, opsz);
                        case 7:
                            return GetRegOperandDetail(InstructionType.SAR, OperandType.rm32, RegType.CL, br, opsz);
                        default:
                            return GetUnknownOpecodeDetail(opecode, br.ReadByte());
                    }
                case 0xD4:
                    return new InstructionDetail(InstructionType.AAM, OperandType.imm8, br, opsz);
                case 0xD5:
                    return new InstructionDetail(InstructionType.AAD, OperandType.imm8, br, opsz);
                case 0xD6:
                    return new InstructionDetail(InstructionType.SETALC);
                case 0xD7:
                    return new InstructionDetail(InstructionType.XLAT, new PtrIVC(PtrIVC.PtrType.BYTE_PTR, new Sib(new Register(RegType.EBX), RegType.AL, 0)));
                case 0xD8:
                    switch (br.ReadByte())
                    {
                        case byte n when n >= 0xC0 && n < 0xC8:
                            return new InstructionDetail(InstructionType.FADD, new STi(n - 0xC0));
                        case byte n when n >= 0xC8 && n < 0xD0:
                            return new InstructionDetail(InstructionType.FMUL, new STi(n - 0xC8));
                        case byte n when n >= 0xD0 && n < 0xD8:
                            return new InstructionDetail(InstructionType.FCOM, new STi(n - 0xD0));
                        case byte n when n >= 0xD8 && n < 0xE0:
                            return new InstructionDetail(InstructionType.FCOMP, new STi(n - 0xD8));
                        case byte n when n >= 0xE0 && n < 0xE8:
                            return new InstructionDetail(InstructionType.FSUB, new STi(n - 0xE0));
                        case byte n when n >= 0xE8 && n < 0xF0:
                            return new InstructionDetail(InstructionType.FSUBR, new STi(n - 0xE8));
                        case byte n when n >= 0xF0 && n < 0xF8:
                            return new InstructionDetail(InstructionType.FDIV, new STi(n - 0xF0));
                        case byte n when n >= 0xF8 && n <= 0xFF:
                            return new InstructionDetail(InstructionType.FDIVR, new STi(n - 0xF8));
                        case byte n:
                            br.BaseStream.Position--;
                            switch (GetReg(n))
                            {
                                case 0:
                                    return new InstructionDetail(InstructionType.FADD, OperandType.m32int, br, opsz);
                                case 1:
                                    return new InstructionDetail(InstructionType.FMUL, OperandType.m32int, br, opsz);
                                case 2:
                                    return new InstructionDetail(InstructionType.FCOM, OperandType.m32int, br, opsz);
                                case 3:
                                    return new InstructionDetail(InstructionType.FCOMP, OperandType.m32int, br, opsz);
                                case 4:
                                    return new InstructionDetail(InstructionType.FSUB, OperandType.m32int, br, opsz);
                                case 5:
                                    return new InstructionDetail(InstructionType.FSUBR, OperandType.m32int, br, opsz);
                                case 6:
                                    return new InstructionDetail(InstructionType.FDIV, OperandType.m32int, br, opsz);
                                case 7:
                                    return new InstructionDetail(InstructionType.FDIVR, OperandType.m32int, br, opsz);
                            }
                            return GetUnknownOpecodeDetail(opecode, br.ReadByte());
                    }
                case 0xD9:
                    switch (br.ReadByte())
                    {

                        case byte n when n >= 0xC0 && n < 0xC8:
                            return new InstructionDetail(InstructionType.FLD, new STi(n - 0xC0));
                        case byte n when n >= 0xC8 && n < 0xD0:
                            return new InstructionDetail(InstructionType.FXCH, new STi(n - 0xC8));
                        case byte n:
                            br.BaseStream.Position--;
                            switch (GetReg(n))
                            {
                                case 0:
                                    return new InstructionDetail(InstructionType.FLD, OperandType.rm32, br, opsz);
                                case 2:
                                    return new InstructionDetail(InstructionType.FST, OperandType.rm32, br, opsz);
                                case 3:
                                    return new InstructionDetail(InstructionType.FSTP, OperandType.rm32, br, opsz);
                                case 4:
                                    return new InstructionDetail(InstructionType.FLDENV, OperandType.rm32, br, opsz);
                                case 5:
                                    return new InstructionDetail(InstructionType.FLDCW, OperandType.rm32, br, opsz);
                                case 6:
                                    return new InstructionDetail(InstructionType.FNSTENV, OperandType.rm32, br, opsz);
                                case 7:
                                    return new InstructionDetail(InstructionType.FNSTCW, OperandType.rm32, br, opsz);
                                default:
                                    throw new NotImplementedException($"OP:0x{opecode:X2} {PseudoReg()}");
                            }
                    }
                case 0xDA:
                    switch (br.ReadByte())
                    {
                        case byte n when n >= 0xC0 && n < 0xC7:
                            return new InstructionDetail(InstructionType.FCMOVB, new STi(n - 0xC0));
                        case byte n when n >= 0xC7 && n < 0xCF:
                            return new InstructionDetail(InstructionType.FCMOVE, new STi(n - 0xC8));
                        case byte n when n >= 0xCF && n < 0xD7:
                            return new InstructionDetail(InstructionType.FCMOVBE, new STi(n - 0xD0));
                        case byte n when n >= 0xD7 && n < 0xDF:
                            return new InstructionDetail(InstructionType.FCMOVBU, new STi(n - 0xD8));
                        case 0xE9:
                            return new InstructionDetail(InstructionType.FUCOMPP);
                        case byte n:
                            br.BaseStream.Position--;
                            switch (GetReg(n))
                            {
                                case 0:
                                    return new InstructionDetail(InstructionType.FIADD, OperandType.m32int, br, opsz);
                                case 1:
                                    return new InstructionDetail(InstructionType.FIMUL, OperandType.m32int, br, opsz);
                                case 2:
                                    return new InstructionDetail(InstructionType.FICOM, OperandType.m32int, br, opsz);
                                case 3:
                                    return new InstructionDetail(InstructionType.FICOMP, OperandType.m32int, br, opsz);
                                case 4:
                                    return new InstructionDetail(InstructionType.FISUB, OperandType.m32int, br, opsz);
                                case 5:
                                    return new InstructionDetail(InstructionType.FISUBR, OperandType.m32int, br, opsz);
                                case 6:
                                    return new InstructionDetail(InstructionType.FIDIV, OperandType.m32int, br, opsz);
                                case 7:
                                    return new InstructionDetail(InstructionType.FIDIVR, OperandType.m32int, br, opsz);
                            }
                            return GetUnknownOpecodeDetail(opecode, br.ReadByte());
                    }
                case 0xDB:
                    switch (br.ReadByte())
                    {
                        case 0xE2:
                            return new InstructionDetail(InstructionType.FNCLEX);
                        case 0xE3:
                            return new InstructionDetail(InstructionType.FNINIT);
                        case byte n when n >= 0xE8 && n < 0xF0:
                            return new InstructionDetail(InstructionType.FUCOMI, new STi(n - 0xE8));
                        case byte n when n >= 0xF0 && n < 0xF7:
                            return new InstructionDetail(InstructionType.FCOMI, new STi(n - 0xF0));
                        case byte n:
                            br.BaseStream.Position--;
                            switch (GetReg(n))
                            {
                                case 0:
                                    return new InstructionDetail(InstructionType.FILD, OperandType.m32int, br, opsz);
                                case 2:
                                    return new InstructionDetail(InstructionType.FIST, OperandType.m32int, br, opsz);
                                case 3:
                                    return new InstructionDetail(InstructionType.FISTP, OperandType.m32int, br, opsz);
                                case 5:
                                    return new InstructionDetail(InstructionType.FLD, OperandType.m32int, br, opsz);
                                case 7:
                                    return new InstructionDetail(InstructionType.FSTP, OperandType.m32int, br, opsz);
                            }
                            return GetUnknownOpecodeDetail(opecode, br.ReadByte());
                    }
                case 0xDC:
                    switch (br.ReadByte())
                    {
                        case byte n when n >= 0xC0 && n < 0xC8:
                            return new InstructionDetail(InstructionType.FADD, new STi(n - 0xC0));
                        case byte n when n >= 0xC8 && n < 0xD0:
                            return new InstructionDetail(InstructionType.FMUL, new STi(n - 0xC8));
                        case byte n when n >= 0xE0 && n < 0xE8:
                            return new InstructionDetail(InstructionType.FSUBR, new STi(n - 0xE0));
                        case byte n when n >= 0xE8 && n < 0xF0:
                            return new InstructionDetail(InstructionType.FSUB, new STi(n - 0xE8));
                        case byte n when n >= 0xF0 && n < 0xF8:
                            return new InstructionDetail(InstructionType.FDIVR, new STi(n - 0xF0));
                        case byte n when n >= 0xF8 && n <= 0xFF:
                            return new InstructionDetail(InstructionType.FDIV, new STi(n - 0xF8));
                        case byte n:
                            br.BaseStream.Position--;
                            switch (GetReg(n))
                            {
                                case 0:
                                    return new InstructionDetail(InstructionType.FADD, OperandType.m64fp, br, opsz);
                                case 1:
                                    return new InstructionDetail(InstructionType.FMUL, OperandType.m64fp, br, opsz);
                                case 2:
                                    return new InstructionDetail(InstructionType.FCOM, OperandType.m64fp, br, opsz);
                                case 3:
                                    return new InstructionDetail(InstructionType.FCOMP, OperandType.m64fp, br, opsz);
                                case 4:
                                    return new InstructionDetail(InstructionType.FSUB, OperandType.m64fp, br, opsz);
                                case 5:
                                    return new InstructionDetail(InstructionType.FSUBR, OperandType.m64fp, br, opsz);
                                case 6:
                                    return new InstructionDetail(InstructionType.FDIV, OperandType.m64fp, br, opsz);
                                case 7:
                                    return new InstructionDetail(InstructionType.FDIVR, OperandType.m64fp, br, opsz);
                            }
                            return GetUnknownOpecodeDetail(opecode, br.ReadByte());
                    }
                case 0xDD:
                    switch (br.ReadByte())
                    {
                        case byte n when n >= 0xC0 && n < 0xC8:
                            return new InstructionDetail(InstructionType.FFREE, new STi(n - 0xC0));
                        case byte n when n >= 0xD0 && n < 0xD8:
                            return new InstructionDetail(InstructionType.FST, new STi(n - 0xD0));
                        case byte n when n >= 0xD8 && n < 0xE0:
                            return new InstructionDetail(InstructionType.FSTP, new STi(n - 0xD8));
                        case byte n when n >= 0xE0 && n < 0xE8:
                            return new InstructionDetail(InstructionType.FUCOM, new STi(n - 0xE0));
                        case byte n when n >= 0xE8 && n < 0xF0:
                            return new InstructionDetail(InstructionType.FUCOMP, new STi(n - 0xE8));
                        case byte n:
                            br.BaseStream.Position--;
                            switch (GetReg(n))
                            {
                                case 0:
                                    return new InstructionDetail(InstructionType.FLD, OperandType.m64fp, br, opsz);
                                case 2:
                                    return new InstructionDetail(InstructionType.FST, OperandType.m64fp, br, opsz);
                                case 3:
                                    return new InstructionDetail(InstructionType.FSTP, OperandType.m64fp, br, opsz);
                                case 4:
                                    return new InstructionDetail(InstructionType.FSTOR, OperandType.m64fp, br, opsz);
                                case 6:
                                    return new InstructionDetail(InstructionType.FNSAVE, OperandType.m64fp, br, opsz);
                                case 7:
                                    return new InstructionDetail(InstructionType.FNSTSW, OperandType.m64fp, br, opsz);
                            }
                            return GetUnknownOpecodeDetail(opecode, br.ReadByte());
                    }
                case 0xDE:
                    switch (br.ReadByte())
                    {
                        case byte n when n >= 0xC0 && n < 0xC8:
                            return new InstructionDetail(InstructionType.FADDP, new STi(n - 0xC0));
                        case byte n when n >= 0xC8 && n < 0xD0:
                            return new InstructionDetail(InstructionType.FMULP, new STi(n - 0xC8));
                        case 0xD9:
                            return new InstructionDetail(InstructionType.FCOMPP);
                        case byte n when n >= 0xE0 && n < 0xE8:
                            return new InstructionDetail(InstructionType.FSUBRP, new STi(n - 0xE0));
                        case byte n when n >= 0xE8 && n < 0xF0:
                            return new InstructionDetail(InstructionType.FSUBP, new STi(n - 0xE8));
                        case byte n when n >= 0xF0 && n < 0xF8:
                            return new InstructionDetail(InstructionType.FDIVRP, new STi(n - 0xF0));
                        case byte n when n >= 0xF8 && n <= 0xFF:
                            return new InstructionDetail(InstructionType.FDIVP, new STi(n - 0xF8));
                        case byte n:
                            br.BaseStream.Position--;
                            switch (GetReg(n))
                            {
                                case 0:
                                    return new InstructionDetail(InstructionType.FIADD, OperandType.m16int, br, opsz);
                                case 1:
                                    return new InstructionDetail(InstructionType.FIMUL, OperandType.m16int, br, opsz);
                                case 2:
                                    return new InstructionDetail(InstructionType.FICOM, OperandType.m16int, br, opsz);
                                case 3:
                                    return new InstructionDetail(InstructionType.FICOMP, OperandType.m16int, br, opsz);
                                case 4:
                                    return new InstructionDetail(InstructionType.FISUB, OperandType.m16int, br, opsz);
                                case 5:
                                    return new InstructionDetail(InstructionType.FISUBR, OperandType.m16int, br, opsz);
                                case 6:
                                    return new InstructionDetail(InstructionType.FIDIV, OperandType.m16int, br, opsz);
                                case 7:
                                    return new InstructionDetail(InstructionType.FIDIVR, OperandType.m16int, br, opsz);
                            }
                            return GetUnknownOpecodeDetail(opecode, br.ReadByte());
                    }
                case 0xDF:
                    switch (br.ReadByte())
                    {
                        case 0xE0:
                            return new InstructionDetail(InstructionType.FNSTSW);
                        case byte n when n >= 0xE8 && n < 0xF0:
                            return new InstructionDetail(InstructionType.FUCOMIP, new STi(n - 0xE8));
                        case byte n when n >= 0xF0 && n < 0xF7:
                            return new InstructionDetail(InstructionType.FCOMIP, new STi(n - 0xF0));
                        case byte n:
                            br.BaseStream.Position--;
                            switch (GetReg(n))
                            {
                                case 0:
                                    return new InstructionDetail(InstructionType.FILD, OperandType.m16int, br, opsz);
                                case 2:
                                    return new InstructionDetail(InstructionType.FIST, OperandType.m16int, br, opsz);
                                case 3:
                                    return new InstructionDetail(InstructionType.FISTP, OperandType.m16int, br, opsz);
                                case 4:
                                    return new InstructionDetail(InstructionType.FBLD, OperandType.m16int, br, opsz);
                                case 5:
                                    return new InstructionDetail(InstructionType.FILD, OperandType.m64fp, br, opsz);
                                case 6:
                                    return new InstructionDetail(InstructionType.FISTP, OperandType.m16int, br, opsz);
                                case 7:
                                    return new InstructionDetail(InstructionType.FNSTSW, OperandType.m64fp, br, opsz);
                            }
                            return GetUnknownOpecodeDetail(opecode, br.ReadByte());
                    }

                case 0xE0:
                    return new InstructionDetail(InstructionType.LOOPNZ, OperandType.rel8, br);
                case 0xE1:
                    return new InstructionDetail(InstructionType.LOOPZ, OperandType.rel8, br);
                case 0xE2:
                    return new InstructionDetail(InstructionType.LOOP, OperandType.rel8, br);
                case 0xE3:
                    return new InstructionDetail(InstructionType.JECXZ, OperandType.rel8, br);
                case 0xE4:
                    return GetRegOperandDetail(InstructionType.IN, RegType.AL, OperandType.imm8, br);
                case 0xE5:
                    return GetRegOperandDetail(InstructionType.IN, RegType.EAX, OperandType.imm8, br);
                case 0xE6:
                    return GetRegOperandDetail(InstructionType.OUT, OperandType.imm8, RegType.AL, br);
                case 0xE7:
                    return GetRegOperandDetail(InstructionType.OUT, OperandType.imm8, RegType.EAX, br);
                case 0xE8://CALL
                    return new InstructionDetail(InstructionType.CALL, OperandType.rel32, br, opsz);
                case 0xE9:
                    return new InstructionDetail(InstructionType.JMP, OperandType.rel32, br, opsz);
                case 0xEA:
                    return new InstructionDetail(InstructionType.JMP, OperandType.ptr1632, br, opsz);
                case 0xEB:
                    return new InstructionDetail(InstructionType.JMP, OperandType.rel8, br);
                case 0xEC:
                    return new InstructionDetail(InstructionType.IN, RegType.AL, RegType.DX, opsz);
                case 0xED:
                    return new InstructionDetail(InstructionType.IN, RegType.EAX, RegType.DX, opsz);
                case 0xEE:
                    return new InstructionDetail(InstructionType.OUT, RegType.DX, RegType.AL, opsz);
                case 0xEF:
                    return new InstructionDetail(InstructionType.OUT, RegType.DX, RegType.EAX, opsz);

                case 0xF0:
                    throw new NotImplementedException("LOCK");
                case 0xF1:
                    return new InstructionDetail(InstructionType.INT1);
                case 0xF2:
                case 0xF3://REP
                    throw new NotImplementedException($"OP:0x{opecode:X2}");
                case 0xF4:
                    return new InstructionDetail(InstructionType.HLT);
                case 0xF5:
                    return new InstructionDetail(InstructionType.CMC);
                case 0xF6:
                    switch (PseudoReg())
                    {
                        case 0:
                        case 1:
                            return new InstructionDetail(InstructionType.TEST, OperandType.rm8, OperandType.imm8, br);
                        case 2:
                            return new InstructionDetail(InstructionType.NOT, OperandType.rm8, br);
                        case 3:
                            return new InstructionDetail(InstructionType.NEG, OperandType.rm8, br);
                        case 4:
                            return new InstructionDetail(InstructionType.MUL, OperandType.rm8, br);
                        case 5:
                            return new InstructionDetail(InstructionType.IMUL, OperandType.rm8, br);
                        case 6:
                            return new InstructionDetail(InstructionType.DIV, OperandType.rm8, br);
                        case 7:
                            return new InstructionDetail(InstructionType.IDIV, OperandType.rm8, br);
                        default:
                            return GetUnknownOpecodeDetail(opecode, br.ReadByte());
                    }
                case 0xF7:
                    switch (PseudoReg())
                    {
                        case 0:
                        case 1:
                            return new InstructionDetail(InstructionType.TEST, OperandType.rm32, OperandType.imm32, br, opsz);
                        case 2:
                            return new InstructionDetail(InstructionType.NOT, OperandType.rm32, br, opsz);
                        case 3:
                            return new InstructionDetail(InstructionType.NEG, OperandType.rm32, br, opsz);
                        case 4:
                            return new InstructionDetail(InstructionType.MUL, OperandType.rm32, br, opsz);
                        case 5:
                            return new InstructionDetail(InstructionType.IMUL, OperandType.rm32, br, opsz);
                        case 6:
                            return new InstructionDetail(InstructionType.DIV, OperandType.rm32, br, opsz);
                        case 7:
                            return new InstructionDetail(InstructionType.IDIV, OperandType.rm32, br, opsz);
                        default:
                            return GetUnknownOpecodeDetail(opecode, br.ReadByte());
                    }
                case 0xF8:
                    return new InstructionDetail(InstructionType.CLC);
                case 0xF9:
                    return new InstructionDetail(InstructionType.STC);
                case 0xFA:
                    return new InstructionDetail(InstructionType.CLI);
                case 0xFB:
                    return new InstructionDetail(InstructionType.STI);
                case 0xFC:
                    return new InstructionDetail(InstructionType.CLD);
                case 0xFD:
                    return new InstructionDetail(InstructionType.STD);
                case 0xFE:
                    switch (PseudoReg())
                    {
                        case 0:
                            return new InstructionDetail(InstructionType.INC, OperandType.rm8, br);
                        case 1:
                            return new InstructionDetail(InstructionType.DEC, OperandType.rm8, br);
                        default:
                            return GetUnknownOpecodeDetail(opecode, br.ReadByte());
                    }
                case 0xFF:
                    switch (PseudoReg())
                    {
                        case 0:
                            return new InstructionDetail(InstructionType.INC, OperandType.rm32, br, opsz);
                        case 1:
                            return new InstructionDetail(InstructionType.DEC, OperandType.rm32, br, opsz);
                        case 2://win32api呼び出し
                            return new InstructionDetail(InstructionType.CALL, OperandType.rm32, br, opsz);
                        case 3://far call
                            return new InstructionDetail(InstructionType.CALL, OperandType.m1632, br, opsz);
                        case 4:
                            return new InstructionDetail(InstructionType.JMP, OperandType.rm32, br, opsz);
                        case 5:
                            return new InstructionDetail(InstructionType.JMP, OperandType.m1632, br, opsz);
                        case 6:
                            return new InstructionDetail(InstructionType.PUSH, OperandType.rm32, br, opsz);
                        default:
                            return GetUnknownOpecodeDetail(opecode, br.ReadByte());
                    }

                default:
                    throw new NotImplementedException();
            }
        }

        /// <summary>
        /// 拡張用
        /// </summary>
        /// <param name="br"></param>
        /// <param name="opsz"></param>
        /// <returns></returns>
        InstructionDetail GetExpansionInsturctionSize(int opsz)
        {
            byte opecode = br.ReadByte();

            switch (opecode)
            {
                case 0x00:
                    switch (PseudoReg())
                    {
                        case 0:
                            return new InstructionDetail(InstructionType.SLDT, OperandType.r32, br, opsz);
                        case 1:
                            return new InstructionDetail(InstructionType.STR, OperandType.rm16, br, opsz);
                        case 2:
                            return new InstructionDetail(InstructionType.LLDT, OperandType.rm16, br, opsz);
                        case 3:
                            return new InstructionDetail(InstructionType.LTR, OperandType.rm16, br, opsz);
                        case 4:
                            return new InstructionDetail(InstructionType.VERR, OperandType.rm16, br, opsz);
                        case 5:
                            return new InstructionDetail(InstructionType.VERW, OperandType.rm16, br, opsz);
                        default:
                            return GetUnknownOpecodeDetail(0x0F, opecode, br.ReadByte());
                    }
                case 0x01:
                case 0x02:
                case 0x03:
                case 0x04:
                case 0x0A:
                case 0x0C:
                case 0x05:
                case 0x06:
                case 0x07:
                case 0x08:
                case 0x09:
                case 0x0B:
                case 0x0E:
                case 0x0D:
                case 0x0F://3DNow!
                    throw new NotImplementedException($"OP:0x0F 0x{opecode:X2}");

                case 0x20://Debug系
                    return GetRegOperandDetail(InstructionType.MOV, OperandType.r32, RegType.CR1, br, opsz);
                case 0x21:
                    return GetRegOperandDetail(InstructionType.MOV, OperandType.r32, RegType.DR1, br, opsz);
                case 0x22:
                    return GetRegOperandDetail(InstructionType.MOV, RegType.CR1, OperandType.r32, br, opsz);
                case 0x23:
                    return GetRegOperandDetail(InstructionType.MOV, RegType.DR1, OperandType.r32, br, opsz);
                case 0x24:
                    return GetUnknownOpecodeDetail(0x0F, opecode);
                case 0x25:
                    return GetUnknownOpecodeDetail(0x0F, opecode);
                case 0x26:
                    return GetUnknownOpecodeDetail(0x0F, opecode);
                case 0x27:
                    return GetUnknownOpecodeDetail(0x0F, opecode);
                case 0x28:
                case 0x29://0x29~0x2F:不完全なオペランド
                    return new InstructionDetail(opsz == 4 ? InstructionType.MOVAPS : InstructionType.MOVAPD, OperandType.imm8, br, opsz);
                case 0x2A:
                    return new InstructionDetail(InstructionType.CVTPI2PS, OperandType.imm8, br, opsz);
                case 0x2B:
                    return GetUnknownOpecodeDetail(0x0F, opecode);
                case 0x2C:
                    return new InstructionDetail(InstructionType.CVTTPS2PI, OperandType.imm8, br, opsz);
                case 0x2D:
                    return new InstructionDetail(InstructionType.CVTPS2PI, OperandType.imm8, br, opsz);
                case 0x2E:
                    return new InstructionDetail(InstructionType.UCOMISS, OperandType.imm8, br, opsz);
                case 0x2F:
                    return new InstructionDetail(InstructionType.COMISS, OperandType.imm8, br, opsz);

                case 0x30:
                case 0x31:
                case 0x32:
                case 0x33:
                case 0x34:
                case 0x35:
                case 0x37:
                case 0x36:
                case 0x39:
                case 0x3B:
                case 0x3C:
                case 0x3D:
                case 0x3E:
                case 0x3F:
                case 0x3A:
                case 0x38:
                    throw new NotImplementedException($"OP:0x0F 0x{opecode:X2}");
                case 0x54:

                case 0x73:
                    switch (PseudoReg())
                    {
                        case 2:
                            return new InstructionDetail(InstructionType.PSRLQ, OperandType.rm8, OperandType.imm8, br, opsz);
                        case 6:
                            return new InstructionDetail(InstructionType.PSLLQ, OperandType.rm8, OperandType.imm8, br, opsz);
                        default:
                            return GetUnknownOpecodeDetail(0x0F, opecode);
                    }
                case 0x7E:
                    return new InstructionDetail(InstructionType.MOVD, OperandType.rm32, OperandType.r32, br, opsz);
                case 0x7F:
                    return new InstructionDetail(InstructionType.MOVQ, OperandType.m3232, OperandType.r32, br, opsz);

                case 0x80:
                    return new InstructionDetail(InstructionType.JO, OperandType.rel32, br, opsz);
                case 0x81:
                    return new InstructionDetail(InstructionType.JNO, OperandType.rel32, br, opsz);
                case 0x82:
                    return new InstructionDetail(InstructionType.JB, OperandType.rel32, br, opsz);
                case 0x83:
                    return new InstructionDetail(InstructionType.JAE, OperandType.rel32, br, opsz);
                case 0x84:
                    return new InstructionDetail(InstructionType.JZ, OperandType.rel32, br, opsz);
                case 0x85:
                    return new InstructionDetail(InstructionType.JNZ, OperandType.rel32, br, opsz);
                case 0x86:
                    return new InstructionDetail(InstructionType.JBE, OperandType.rel32, br, opsz);
                case 0x87:
                    return new InstructionDetail(InstructionType.JA, OperandType.rel32, br, opsz);
                case 0x88:
                    return new InstructionDetail(InstructionType.JS, OperandType.rel32, br, opsz);
                case 0x89:
                    return new InstructionDetail(InstructionType.JNS, OperandType.rel32, br, opsz);
                case 0x8A:
                    return new InstructionDetail(InstructionType.JP, OperandType.rel32, br, opsz);
                case 0x8B:
                    return new InstructionDetail(InstructionType.JNP, OperandType.rel32, br, opsz);
                case 0x8C:
                    return new InstructionDetail(InstructionType.JL, OperandType.rel32, br, opsz);
                case 0x8D:
                    return new InstructionDetail(InstructionType.JGE, OperandType.rel32, br, opsz);
                case 0x8E:
                    return new InstructionDetail(InstructionType.JLE, OperandType.rel32, br, opsz);
                case 0x8F:
                    return new InstructionDetail(InstructionType.JG, OperandType.rel32, br, opsz);

                case 0x90:
                    return new InstructionDetail(InstructionType.SETO, OperandType.rm8, br);
                case 0x91:
                    return new InstructionDetail(InstructionType.SETNO, OperandType.rm8, br);
                case 0x92:
                    return new InstructionDetail(InstructionType.SETB, OperandType.rm8, br);
                case 0x93:
                    return new InstructionDetail(InstructionType.SETAE, OperandType.rm8, br);
                case 0x94:
                    return new InstructionDetail(InstructionType.SETZ, OperandType.rm8, br);
                case 0x95:
                    return new InstructionDetail(InstructionType.SETNZ, OperandType.rm8, br);
                case 0x96:
                    return new InstructionDetail(InstructionType.SETBE, OperandType.rm8, br);
                case 0x97:
                    return new InstructionDetail(InstructionType.SETA, OperandType.rm8, br);
                case 0x98:
                    return new InstructionDetail(InstructionType.SETS, OperandType.rm8, br);
                case 0x99:
                    return new InstructionDetail(InstructionType.SETNS, OperandType.rm8, br);
                case 0x9A:
                    return new InstructionDetail(InstructionType.SETP, OperandType.rm8, br);
                case 0x9B:
                    return new InstructionDetail(InstructionType.SETNP, OperandType.rm8, br);
                case 0x9C:
                    return new InstructionDetail(InstructionType.SETL, OperandType.rm8, br);
                case 0x9D:
                    return new InstructionDetail(InstructionType.SETGE, OperandType.rm8, br);
                case 0x9E:
                    return new InstructionDetail(InstructionType.SETLE, OperandType.rm8, br);
                case 0x9F:
                    return new InstructionDetail(InstructionType.SETG, OperandType.rm8, br);

                case 0xA0:
                case 0xA1:
                case 0xA2:
                case 0xA3:
                case 0xA4:
                    return new InstructionDetail(InstructionType.SHRD, OperandType.rm32, OperandType.r32, OperandType.imm8, br, opsz);
                case 0xA5:
                    var preIDA5 = new InstructionDetail(InstructionType.SHRD, OperandType.rm32, OperandType.r32, br, opsz);
                    preIDA5.Operand = new TupleIVC(((PairIVC)preIDA5.Operand).IVC1, ((PairIVC)preIDA5.Operand).IVC2, new Register(RegType.CL));
                    return preIDA5;
                case 0xA6:
                case 0xA7:
                case 0xA8:
                case 0xA9:
                case 0xAA:
                case 0xAB:
                case 0xAC:
                    throw new NotImplementedException($"OP:0x0F 0x{opecode:X2}");
                case 0xAD:
                    return new InstructionDetail(InstructionType.SHRD, OperandType.rm32, OperandType.r32, br, opsz);
                case 0xAE://オペランド未完成
                    switch (br.ReadByte())
                    {
                        case 0:
                            return new InstructionDetail(InstructionType.FXRSTOR);
                        case 1:
                            return new InstructionDetail(InstructionType.LDMXCSR);
                        case 2:
                            return new InstructionDetail(InstructionType.STMXCSR);
                        case 3:
                            return new InstructionDetail(InstructionType.XSAVE);
                        case 4:
                            return new InstructionDetail(InstructionType.XSTOR);
                        case byte n:
                            return GetUnknownOpecodeDetail(0x0F, opecode, n);
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                case 0xAF:
                    return new InstructionDetail(InstructionType.IMUL, OperandType.r32, OperandType.rm32, br, opsz);
                case 0xB0:
                    return new InstructionDetail(InstructionType.CMPXCHG, OperandType.rm8, OperandType.r8, br);
                case 0xB1:
                    return new InstructionDetail(InstructionType.CMPXCHG, OperandType.rm32, OperandType.r32, br, opsz);
                case 0xB2:
                    return new InstructionDetail(InstructionType.LSS, OperandType.r32, OperandType.m1632, br, opsz);
                case 0xB3:
                    return new InstructionDetail(InstructionType.BTR, OperandType.rm32, OperandType.r32, br, opsz);
                case 0xB4:
                    return new InstructionDetail(InstructionType.LFS, OperandType.r32, OperandType.m1632, br, opsz);
                case 0xB5:
                    return new InstructionDetail(InstructionType.LGS, OperandType.r32, OperandType.m1632, br, opsz);
                case 0xB6:
                    return new InstructionDetail(InstructionType.MOVZX, OperandType.r32, OperandType.rm8, br, opsz);
                case 0xB7:
                    return new InstructionDetail(InstructionType.MOVZX, OperandType.r32, OperandType.rm16, br, opsz);
                case 0xB8:
                    return GetUnknownOpecodeDetail(0x0F, opecode);
                case 0xB9:
                    return new InstructionDetail(InstructionType.UD1);
                case 0xBA:
                    switch (PseudoReg())
                    {
                        case 4:
                            return new InstructionDetail(InstructionType.BT, OperandType.rm32, OperandType.imm8, br, opsz);
                        case 5:
                            return new InstructionDetail(InstructionType.BTS, OperandType.rm32, OperandType.imm8, br, opsz);
                        case 6:
                            return new InstructionDetail(InstructionType.BTR, OperandType.rm32, OperandType.imm8, br, opsz);
                        case 7:
                            return new InstructionDetail(InstructionType.BTC, OperandType.rm32, OperandType.imm8, br, opsz);
                        default:
                            return GetUnknownOpecodeDetail(0x0F, opecode, br.ReadByte());
                    }
                case 0xBB:
                    return new InstructionDetail(InstructionType.BTC, OperandType.rm32, OperandType.r32, br, opsz);
                case 0xBC:
                    return new InstructionDetail(InstructionType.BSF, OperandType.r32, OperandType.rm32, br, opsz);
                case 0xBD:
                    return new InstructionDetail(InstructionType.BSR, OperandType.r32, OperandType.rm32, br, opsz);
                case 0xBE:
                    return new InstructionDetail(InstructionType.MOVSX, OperandType.r32, OperandType.rm8, br, opsz);
                case 0xBF:
                    return new InstructionDetail(InstructionType.MOVSX, OperandType.r32, OperandType.rm32, br, opsz);
                case 0xC0:
                    return new InstructionDetail(InstructionType.XADD, OperandType.rm8, OperandType.r8, br, opsz);
                case 0xC1:
                    return new InstructionDetail(InstructionType.XADD, OperandType.rm32, OperandType.r32, br, opsz);

                case 0xF0:
                    return GetUnknownOpecodeDetail(0x0F, opecode);
                case 0xF1:

                default:
                    throw new NotImplementedException($"OP:0x0F 0x{opecode:X2}");
            }
        }

        /// <summary>
        /// MS-DOS ヘッダ 
        /// </summary>
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct _IMAGE_DOS_HEADER
        {
            public ushort e_magic;                     // Magic number
            public ushort e_cblp;                      // Bytes on last page of file
            public ushort e_cp;                        // Pages in file
            public ushort e_crlc;                      // Relocations
            public ushort e_cparhdr;                   // Size of header in paragraphs
            public ushort e_minalloc;                  // Minimum extra paragraphs needed
            public ushort e_maxalloc;                  // Maximum extra paragraphs needed
            public ushort e_ss;                        // Initial (relative) SS value
            public ushort e_sp;                        // Initial SP value
            public ushort e_csum;                      // Checksum
            public ushort e_ip;                        // Initial IP value
            public ushort e_cs;                        // Initial (relative) CS value
            public ushort e_lfarlc;                    // File address of relocation table
            public ushort e_ovno;                      // Overlay number
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
            public ushort[] e_res;                    // Reserved words
            public ushort e_oemid;                     // OEM identifier (for e_oeminfo)
            public ushort e_oeminfo;                   // OEM information; e_oemid specific
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 10)]
            public ushort[] e_res2;                  // Reserved words
            public uint e_lfanew;                    // File address of new exe header
        }

        /// <summary>
        /// NTヘッダ
        /// </summary>
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct IMAGE_NT_HEADERS
        {
            public uint Signature;
            public IMAGE_FILE_HEADER FileHeader;
            public IMAGE_OPTIONAL_HEADER32 OptionalHeader;
        }

        /// <summary>
        /// ファイルヘッダ
        /// </summary>
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct IMAGE_FILE_HEADER
        {
            public ushort Machine;
            public ushort NumberOfSections;
            public uint TimeDateStamp;
            public uint PointerToSymbolTable;
            public uint NumberOfSymbols;
            public ushort SizeOfOptionalHeader;
            public ushort Characteristics;
        }

        /// <summary>
        /// オプショナルヘッダ
        /// </summary>
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct IMAGE_OPTIONAL_HEADER32
        {
            // Standard fields.
            public ushort Magic;
            public byte MajorLinkerVersion;
            public byte MinorLinkerVersion;
            public uint SizeOfCode;
            public uint SizeOfInitializedData;
            public uint SizeOfUninitializedData;
            public uint AddressOfEntryPoint;
            public uint BaseOfCode;
            public uint BaseOfData;

            // NT additional fields.
            public uint ImageBase;
            public uint SectionAlignment;
            public uint FileAlignment;
            public ushort MajorOperatingSystemVersion;
            public ushort MinorOperatingSystemVersion;
            public ushort MajorImageVersion;
            public ushort MinorImageVersion;
            public ushort MajorSubsystemVersion;
            public ushort MinorSubsystemVersion;
            public uint Win32VersionValue;
            public uint SizeOfImage;
            public uint SizeOfHeaders;
            public uint CheckSum;
            public ushort Subsystem;
            public ushort DllCharacteristics;
            public uint SizeOfStackReserve;
            public uint SizeOfStackCommit;
            public uint SizeOfHeapReserve;
            public uint SizeOfHeapCommit;
            public uint LoaderFlags;
            public uint NumberOfRvaAndSizes;
        }

        /// <summary>
        /// イメージデータ
        /// </summary>
        [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
        struct IMAGE_DATA_DIRECTORY
        {
            public uint VirtualAddress;
            public uint Size;

            public override string ToString()
                => Size == 0 ? "×" : "○";
        }

        /// <summary>
        /// セクションヘッダ
        /// </summary>
        [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
        struct IMAGE_SECTION_HEADER
        {
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 8)]
            public string Name;

            public uint VirtualSize;
            public uint VirtualAddress;
            public uint SizeOfRawData;
            public uint PointerToRawData;
            public uint PointerToRelocations;
            public uint PointerToLinenumbers;
            public ushort NumberOfRelocations;
            public ushort NumberOfLinenumbers;
            public uint Characteristics;

            public override string ToString() => Name;
        }

        /// <summary>
        /// デバッグ情報
        /// </summary>
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct IMAGE_DEBUG_DIRECTORY
        {
            public uint Characteristics;
            public uint TimeDateStamp;
            public ushort MajorVersion;
            public ushort MinorVersion;
            public uint Type;
            public uint SizeOfData;
            public uint AddressOfRawData;
            public uint PointerToRawData;
        }

        /// <summary>
        /// デバッグCOFF情報
        /// </summary>
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct IMAGE_COFF_SYMBOLS_HEADER
        {
            public int NumberOfSymbols;
            public uint LvaToFirstSymbol;
            public uint NumberOfLinenumbers;
            public uint LvaToFirstLinenumber;
            public uint RvaToFirstByteOfCode;
            public uint RvaToLastByteOfCode;
            public uint RvaToFirstByteOfData;
            public uint RvaToLastByteOfData;
        }

        enum SymbolType : ushort
        {
            NULL = 0b0000,     // 記号なし
            VOID = 0b0001,     //void関数の引数（使用されていない）
            CHAR = 0b0010,     //キャラクター
            SHORT = 0b0011,    //短い整数
            INT = 0b0100,      //整数
            LONG = 0b0101,     //長整数
            FLOAT = 0b0110,    //浮動小数点
            DOUBLE = 0b0111,   //倍精度浮動小数点
            STRUCT = 0b1000,   //構造
            UNION = 0b1001,    //連合
            ENUM = 0b1010,     //列挙
            MOE = 0b1011,      //列挙のメンバー
            UCHAR = 0b1100,    //署名のない文字
            USHORT = 0b1101,   //署名のない短い
            UINT = 0b1110,     //符号なし整数
            ULONG = 0b1111,    //符号なしlong
            LNGDBL = 0b01_0000,//ロング・ダブル（特殊なビット・パターン）
            DT_PTR = 0b01_0000,  //Tへのポインタ
            DT_FCN = 0b10_0000,  //Tを返す関数
            DT_ARY = 0b11_0000,  //Tの配列
        }

        enum StrageClassType : byte
        {
            NULL_T = 0,		//立入り禁止
            AUTO = 1,		//自動変数
            EXT = 2,		//外部（パブリック）シンボル - これはグローバルとエクスターナルをカバーします
            STATIC = 3,		//静的（プライベート）シンボル
            REG = 4,		//レジスタ変数
            EXTDEF = 5,	    //外部定義
            LABEL = 6,	    //ラベル
            ULABEL = 7,	    //未定義ラベル
            MOS = 8,		//構造のメンバー
            ARG = 9,		//関数の引数
            STRTAG = 10,	//構造タグ
            MOU = 11,		//組合員
            UNION = 12,	    //ユニオンタグ
            TPDEF = 13,	    //型定義
            USTATIC = 14,	//定義されていない静的
            ENUM = 15,	    //列挙タグ
            MoENUM = 16,	//列挙のメンバー
            REGPARM = 17,	//レジスタパラメータ
            FIELD = 18,	    //ビットフィールド
            AUTOARG = 19,	//自動議論
            LASTENT = 20,	//ダミーエントリ（ブロックの終了）
            BLOCK = 100,	//".bb"または ".eb" - ブロックの先頭または末尾
            FCN = 101,	//".bf"または ".ef" - 関数の開始または終了
            EOS = 102,	//構造の終わり
            FILE = 103,	//ファイル名
            LINE = 104,	//行番号、シンボルとして再フォーマット
            ALIAS = 105,	//重複タグ
            HIDDEN = 106,	//dmert public libのextシンボル
            EFCN = 255,	//関数の物理的終わり
        }

        enum SectionType : short
        {
            bss = 3,
            data = 2,
            text = 1,
            UNDEFINED = 0,
            ABSOLUTE = -1,
            DEBUG = -2,
        }

        /// <summary>
        /// シンボル
        /// </summary>
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct SYMENT
        {
            public uint NameZero;

            public uint NameOffset;

            public uint Addr;

            public SectionType SectionNumber;

            //基本型と派生型で構成
            public SymbolType Type;

            //シンボルがどこで何を表しているかを示す
            public StrageClassType StrageClass;

            //補助エントリの数
            public byte NumberOfAuxSymbols;

            public override string ToString()
                => $"{SectionNumber} : {StrageClass} {Type}";
        }

        /// <summary>
        /// 関数名を得る
        /// </summary>
        /// <param name="DataDic"></param>
        void GetNamedFunctions(IMAGE_DATA_DIRECTORY[] DataDic)
        {
            //デバッグ情報
            br.BaseStream.Seek(DataDic[6].VirtualAddress, SeekOrigin.Begin);
            IMAGE_DEBUG_DIRECTORY[] DebugDirectory = br.Reads<IMAGE_DEBUG_DIRECTORY>((int)(DataDic[6].Size / Marshal.SizeOf(typeof(IMAGE_DEBUG_DIRECTORY))));

            //coff収集
            br.BaseStream.Seek(DebugDirectory[0].PointerToRawData, SeekOrigin.Begin);
            IMAGE_COFF_SYMBOLS_HEADER CoffHeader = br.ReadStruct<IMAGE_COFF_SYMBOLS_HEADER>();
            SYMENT[] Symbols = br.Reads<SYMENT>(CoffHeader.NumberOfSymbols);

            var savePoint = br.BaseStream.Position;

            //名前取得
            Dictionary<string, SYMENT> namedSymbols = new Dictionary<string, SYMENT>();
            foreach (var symbol in Symbols.Where(t => t.SectionNumber > 0))
            {
                if (symbol.NameZero == 0)
                {
                    br.BaseStream.Seek(symbol.NameOffset, SeekOrigin.Current);
                    namedSymbols[br.ReadSjis()] = symbol;
                    br.BaseStream.Seek(savePoint, SeekOrigin.Begin);
                }
                else
                {
                    List<byte> strbyte = new List<byte>(BitConverter.GetBytes(symbol.NameZero));
                    strbyte.AddRange(BitConverter.GetBytes(symbol.NameOffset));
                    namedSymbols[Encoding.ASCII.GetString(strbyte.TakeWhile(t => t != 0).ToArray())] = symbol;
                }
            }
        }
    }
}
