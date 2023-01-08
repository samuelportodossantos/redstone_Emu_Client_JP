using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using static Anantashesha.Disassemble2.Disassembler;

namespace Anantashesha.Disassemble2
{

    static class DisasmblerHeader
    {
        /// <summary>
        /// MS-DOS ヘッダ 
        /// </summary>
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct _IMAGE_DOS_HEADER
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
        public struct IMAGE_NT_HEADERS
        {
            public uint Signature;
            public IMAGE_FILE_HEADER FileHeader;
            public IMAGE_OPTIONAL_HEADER32 OptionalHeader;
        }

        /// <summary>
        /// ファイルヘッダ
        /// </summary>
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct IMAGE_FILE_HEADER
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
        public struct IMAGE_OPTIONAL_HEADER32
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
        public struct IMAGE_DATA_DIRECTORY
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
        public struct IMAGE_SECTION_HEADER
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

        /////////////////////////////////////////////////////////////
        /////////////////////Assemble////////////////////////////////

        public const short NEGLIMIT = (-16384);      // Limit to display constans as signed
        public const short PSEUDOOP = 128;          // Base for pseudooperands
        public const short TEXTLEN = 256;           // Maximal length of text string

        // Special command features.
        public const byte WW = 0x01;          // Bit W (size of operand)
        public const byte SS = 0x02;          // Bit S (sign extention of immediate)
        public const byte WS = 0x03;          // Bits W and S
        public const byte W3 = 0x08;          // Bit W at position 3
        public const byte CC = 0x10;          // Conditional jump
        public const byte FF = 0x20;          // Forced 16-bit size
        public const byte LL = 0x40;          // Conditional loop
        public const byte PR = 0x80;          // Protected command
        public const byte WP = 0x81;          // I/O command with bit W

        // 80x86のすべての可能なオペランドの型. A bit more than you expected, he?
        public const byte NNN = 0;             // オペランドなし
        public const byte REG = 1;             // Regフィールドの整数レジスタ
        public const byte RCM = 2;             // コマンドバイトの整数レジスタ
        public const byte RG4 = 3;             // Regフィールドの整数4バイト・レジスタ
        public const byte RAC = 4;             // アキュムレータ（AL / AX / EAX、implicit）
        public const byte RAX = 5;             // AX（2バイト、implicit）
        public const byte RDX = 6;             // DX（16ビット implicit ポートアドレス）
        public const byte RCL = 7;             // 暗黙のCLレジスタ（シフト用）
        public const byte RS0 = 8;             // FPUスタックの先頭（暗黙のうちにST（0））
        public const byte RST = 9;             // FPUレジスタ（ST（i））のコマンドバイト
        public const byte RMX = 10;             // MMXレジスタMMx
        public const byte R3D = 11;             // 3DNow！ MMxを登録する
        public const byte MRG = 12;             // ModRMバイトのメモリ / レジスタ
        public const byte MR1 = 13;             // ModRMバイトの1バイトメモリ / レジスタ
        public const byte MR2 = 14;             // ModRMバイトの2バイトメモリ / レジスタ
        public const byte MR4 = 15;             // ModRMバイトの4バイトメモリ / レジスタ
        public const byte RR4 = 16;             // 4バイトのメモリ / レジスタ（レジスタのみ）
        public const byte MR8 = 17;             // ModRMの8バイトメモリ / MMXレジスタ
        public const byte RR8 = 18;             // 8バイトMMXレジスタはModRMのみ
        public const byte MRD = 19;             // 8バイトメモリ / 3DNow！ ModRMに登録する
        public const byte RRD = 20;             // 8バイトメモリ / 3DNow！ （レジスタのみ）
        public const byte MRJ = 21;             // JUMPターゲットとしてのModRMのメモリ / reg
        public const byte MMA = 22;             // LEAのModRMバイトのメモリアドレス
        public const byte MML = 23;             // ModRMバイトのメモリ（LESの場合）
        public const byte MMS = 24;             // ModRMバイトのメモリ（SEG：OFFS）
        public const byte MM6 = 25;             // ModRmのメモリ（6バイトのディスクリプタ）
        public const byte MMB = 26;             // 2つの隣接するメモリ位置（BOUND）
        public const byte MD2 = 27;             // ModRMのメモリ（16ビット整数）
        public const byte MB2 = 28;             // ModRMのメモリ（16ビットバイナリ）
        public const byte MD4 = 29;             // ModRMバイトのメモリ（32ビット整数）
        public const byte MD8 = 30;             // ModRMバイトのメモリ（64ビット整数）
        public const byte MDA = 31;             // ModRMバイトのメモリ（80ビットBCD）
        public const byte MF4 = 32;             // ModRMバイトのメモリ（32ビット・フロート）
        public const byte MF8 = 33;             // ModRMバイトのメモリ（64ビット・フロート）
        public const byte MFA = 34;             // ModRMバイトのメモリ（80ビット浮動小数点）
        public const byte MFE = 35;             // ModRMバイトのメモリ（FPU環境）
        public const byte MFS = 36;             // ModRMバイトのメモリ（FPU状態）
        public const byte MFX = 37;             // ModRMバイトのメモリ（内線FPU状態）
        public const byte MSO = 38;             // 文字列演算子（[ESI]）のソース
        public const byte MDE = 39;             // 文字列演算の宛先（[EDI]）
        public const byte MXL = 40;             // XLATオペランド（[EBX + AL]）
        public const byte IMM = 41;             // 即値データ（8または16 / 32）
        public const byte IMU = 42;             // 即値の符号なしデータ（8または16 / 32）
        public const byte VXD = 43;             // VxDサービス
        public const byte IMX = 44;             // 即値符号拡張可能バイト
        public const byte C01 = 45;             // implicitの定数1（シフトの場合）
        public const byte IMS = 46;             // 即値バイト（シフト用）
        public const byte IM1 = 47;             // 即値バイト
        public const byte IM2 = 48;             // 即値ワード（ENTER / RET）
        public const byte IMA = 49;             // 即値の絶対近傍データアドレス
        public const byte JOB = 50;             // 即値バイトオフセット（ジャンプの場合）
        public const byte JOW = 51;             // 即値完全オフセット（ジャンプの場合）
        public const byte JMF = 52;             // 即値絶対遠隔ジャンプ / 呼び出しアドレス
        public const byte SGM = 53;             // ModRMバイトのセグメントレジスタ
        public const byte SCM = 54;             // コマンドバイト内のセグメントレジスタ
        public const byte CRX = 55;             // 制御レジスタCRx
        public const byte DRX = 56;             // デバッグレジスタDRx

        // 疑似オペランド（暗黙のオペランド、アセンブラコマンドには決して現れない）. Must
        // have index equal to or exceeding PSEUDOOP.
        public const byte PRN = (PSEUDOOP + 0);   // Near return address
        public const byte PRF = (PSEUDOOP + 1);   // Far return address
        public const byte PAC = (PSEUDOOP + 2);   // Accumulator (AL/AX/EAX)
        public const byte PAH = (PSEUDOOP + 3);   // AH (in LAHF/SAHF commands)
        public const byte PFL = (PSEUDOOP + 4);   // Lower byte of flags (in LAHF/SAHF)
        public const byte PS0 = (PSEUDOOP + 5);   // Top of FPU stack (ST(0))
        public const byte PS1 = (PSEUDOOP + 6);   // ST(1)
        public const byte PCX = (PSEUDOOP + 7);   // CX/ECX
        public const byte PDI = (PSEUDOOP + 8);   // EDI (in MMX extentions)

        ////////////////////////////////////////////////////////////////////////////////
        //////////////////////////////// SYMBOLIC NAMES ////////////////////////////////

        // Names of control registers.
        public static string[] crname = new string[] {
          "CR0",      "CR1",      "CR2",      "CR3",
          "CR4",      "CR5",      "CR6",      "CR7" };

        // Names of debug registers.
        public static string[] drname = new string[] {
          "DR0",      "DR1",      "DR2",      "DR3",
          "DR4",      "DR5",      "DR6",      "DR7" };

        ////////////////////////////////////////////////////////////////////////////////
        //////////////////// ASSEMBLER, DISASSEMBLER AND EXPRESSIONS ///////////////////

        /// <summary>
        /// cmddataのポインタ
        /// </summary>
        /// <returns></returns>
        public unsafe static CmdData* GetCmdDataPtr()
        {
            //配列確保
            CmdData* pcmdata = (CmdData*)Marshal.AllocHGlobal(cmddata.Length * Marshal.SizeOf(typeof(CmdData))).ToPointer();

            //コピー
            for (int i = 0; i < cmddata.Length; i++, pcmdata++)
            {
                fixed (CmdData* pcmd = &cmddata[i]) *pcmdata = *pcmd;
            }
            pcmdata -= cmddata.Length;
            return pcmdata;
        }
        
        public unsafe struct CmdData
        {
            public readonly int Mask;                 // コマンドの最初の4バイトのマスク
            public readonly int Code;                 // マスクされたバイトをこれと比較する
            public readonly sbyte Len;                  // メインコマンドコードの長さ
            public readonly byte Bits;                 // コマンド内の特殊ビット
            public readonly byte Arg1, Arg2, Arg3;     // 可能な引数の型
            public readonly C Type;                 // C_xxx + 追加情報
            fixed char _Name[60];                // このコマンドのシンボリック名

            public string Name
            {
                get
                {
                    string result = "";
                    fixed (char* ptr = _Name)
                    {
                        for (char* c = ptr; *c != '\0'; c++) result += *c;
                    }
                    return result;
                }
            }

            public CmdData(int mask, int code, sbyte len, byte bits, byte arg1, byte arg2, byte arg3, C type, string name)
            {
                Mask = mask; Code = code; Len = len; Bits = bits; Arg1 = arg1; Arg2 = arg2; Arg3 = arg3; Type = type;
                if (name.Length > 60) throw new ArgumentException();
                fixed (char* ptr = _Name) for (int i = 0; i < name.Length; i++) ptr[i] = name[i];
            }

            public override string ToString() => Name;
        }

        // デコード、パラメータのタイプ、およびその他の有用な情報を含む使用可能なプロセッサコマンドのリスト
        // 最後の要素はフィールドマスク = 0
        // ニーモニックがアンパサンド（'&'）で始まる場合、ニーモニックはオペランドサイズ（16ビットまたは32ビット）によって異なる方法でデコードされます。 
        // ニーモニックがドル（'$'）で始まる場合、このニーモニックはアドレスのサイズに依存します。
        // セミコロン（';'）は16ビット形式を32ビットから分離し、アスタリスク（'*'）はW（16）、D（32）、またはnone（16 / 32）文字のいずれかに置き換えられます。
        // commandの型がC_MMXまたはC_NOWの場合、または型にC_EXPL（ = 0x01）が含まれている場合、Disassemblerはメモリオペランドの明示的なサイズを指定する必要があります。
        public static CmdData[] cmddata = new CmdData[]
            {
                new CmdData( 0x0000FF, 0x000090, 1,00,  NNN,NNN,NNN, (C)((int)C.CMD + 0),        "NOP" ),
                new CmdData( 0x0000FE, 0x00008A, 1,WW,  REG,MRG,NNN, (C)((int)C.CMD + 0),        "MOV" ),
                new CmdData( 0x0000F8, 0x000050, 1,00,  RCM,NNN,NNN, (C)((int)C.PSH + 0),        "PUSH" ),
                new CmdData( 0x0000FE, 0x000088, 1,WW,  MRG,REG,NNN, (C)((int)C.CMD + 0),        "MOV" ),
                new CmdData( 0x0000FF, 0x0000E8, 1,00,  JOW,NNN,NNN, (C)((int)C.CAL + 0),        "CALL" ),
                new CmdData( 0x0000FD, 0x000068, 1,SS,  IMM,NNN,NNN, (C)((int)C.PSH + 0),        "PUSH" ),
                new CmdData( 0x0000FF, 0x00008D, 1,00,  REG,MMA,NNN, (C)((int)C.CMD + 0),        "LEA" ),
                new CmdData( 0x0000FF, 0x000074, 1,CC,  JOB,NNN,NNN, (C)((int)C.JMC + 0),        "JE,JZ" ),
                new CmdData( 0x0000F8, 0x000058, 1,00,  RCM,NNN,NNN, (C)((int)C.POP + 0),        "POP" ),
                new CmdData( 0x0038FC, 0x000080, 1,WS,  MRG,IMM,NNN, (C)((int)C.CMD + 1),        "ADD" ),
                new CmdData( 0x0000FF, 0x000075, 1,CC,  JOB,NNN,NNN, (C)((int)C.JMC + 0),        "JNZ,JNE" ),
                new CmdData( 0x0000FF, 0x0000EB, 1,00,  JOB,NNN,NNN, (C)((int)C.JMP + 0),        "JMP" ),
                new CmdData( 0x0000FF, 0x0000E9, 1,00,  JOW,NNN,NNN, (C)((int)C.JMP + 0),        "JMP" ),
                new CmdData( 0x0000FE, 0x000084, 1,WW,  MRG,REG,NNN, (C)((int)C.CMD + 0),        "TEST" ),
                new CmdData( 0x0038FE, 0x0000C6, 1,WW,  MRG,IMM,NNN, (C)((int)C.CMD + 1),        "MOV" ),
                new CmdData( 0x0000FE, 0x000032, 1,WW,  REG,MRG,NNN, (C)((int)C.CMD + 0),        "XOR" ),
                new CmdData( 0x0000FE, 0x00003A, 1,WW,  REG,MRG,NNN, (C)((int)C.CMD + 0),        "CMP" ),
                new CmdData( 0x0038FC, 0x003880, 1,WS,  MRG,IMM,NNN, (C)((int)C.CMD + 1),        "CMP" ),
                new CmdData( 0x0038FF, 0x0010FF, 1,00,  MRJ,NNN,NNN, (C)((int)C.CAL + 0),        "CALL" ),
                new CmdData( 0x0000FF, 0x0000C3, 1,00,  PRN,NNN,NNN, (C)((int)C.RET + 0),        "RETN,RET" ),
                new CmdData( 0x0000F0, 0x0000B0, 1,W3,  RCM,IMM,NNN, (C)((int)C.CMD + 0),        "MOV" ),
                new CmdData( 0x0000FE, 0x0000A0, 1,WW,  RAC,IMA,NNN, (C)((int)C.CMD + 0),        "MOV" ),
                new CmdData( 0x00FFFF, 0x00840F, 2,CC,  JOW,NNN,NNN, (C)((int)C.JMC + 0),        "JE,JZ" ),
                new CmdData( 0x0000F8, 0x000040, 1,00,  RCM,NNN,NNN, (C)((int)C.CMD + 0),        "INC" ),
                new CmdData( 0x0038FE, 0x0000F6, 1,WW,  MRG,IMU,NNN, (C)((int)C.CMD + 1),        "TEST" ),
                new CmdData( 0x0000FE, 0x0000A2, 1,WW,  IMA,RAC,NNN, (C)((int)C.CMD + 0),        "MOV" ),
                new CmdData( 0x0000FE, 0x00002A, 1,WW,  REG,MRG,NNN, (C)((int)C.CMD + 0),        "SUB" ),
                new CmdData( 0x0000FF, 0x00007E, 1,CC,  JOB,NNN,NNN, (C)((int)C.JMC + 0),        "JLE,JNG" ),
                new CmdData( 0x00FFFF, 0x00850F, 2,CC,  JOW,NNN,NNN, (C)((int)C.JMC + 0),        "JNZ,JNE" ),
                new CmdData( 0x0000FF, 0x0000C2, 1,00,  IM2,PRN,NNN, (C)((int)C.RET + 0),        "RETN" ),
                new CmdData( 0x0038FF, 0x0030FF, 1,00,  MRG,NNN,NNN, (C)((int)C.PSH + 1),        "PUSH" ),
                new CmdData( 0x0038FC, 0x000880, 1,WS,  MRG,IMU,NNN, (C)((int)C.CMD + 1),        "OR" ),
                new CmdData( 0x0038FC, 0x002880, 1,WS,  MRG,IMM,NNN, (C)((int)C.CMD + 1),        "SUB" ),
                new CmdData( 0x0000F8, 0x000048, 1,00,  RCM,NNN,NNN, (C)((int)C.CMD + 0),        "DEC" ),
                new CmdData( 0x00FFFF, 0x00BF0F, 2,00,  REG,MR2,NNN, (C)((int)C.CMD + 1),        "MOVSX" ),
                new CmdData( 0x0000FF, 0x00007C, 1,CC,  JOB,NNN,NNN, (C)((int)C.JMC + 0),        "JL,JNGE" ),
                new CmdData( 0x0000FE, 0x000002, 1,WW,  REG,MRG,NNN, (C)((int)C.CMD + 0),        "ADD" ),
                new CmdData( 0x0038FC, 0x002080, 1,WS,  MRG,IMU,NNN, (C)((int)C.CMD + 1),        "AND" ),
                new CmdData( 0x0000FE, 0x00003C, 1,WW,  RAC,IMM,NNN, (C)((int)C.CMD + 0),        "CMP" ),
                new CmdData( 0x0038FF, 0x0020FF, 1,00,  MRJ,NNN,NNN, (C)((int)C.JMP + 0),        "JMP" ),
                new CmdData( 0x0038FE, 0x0010F6, 1,WW,  MRG,NNN,NNN, (C)((int)C.CMD + 1),        "NOT" ),
                new CmdData( 0x0038FE, 0x0028C0, 1,WW,  MRG,IMS,NNN, (C)((int)C.CMD + 1),        "SHR" ),
                new CmdData( 0x0000FE, 0x000038, 1,WW,  MRG,REG,NNN, (C)((int)C.CMD + 0),        "CMP" ),
                new CmdData( 0x0000FF, 0x00007D, 1,CC,  JOB,NNN,NNN, (C)((int)C.JMC + 0),        "JGE,JNL" ),
                new CmdData( 0x0000FF, 0x00007F, 1,CC,  JOB,NNN,NNN, (C)((int)C.JMC + 0),        "JG,JNLE" ),
                new CmdData( 0x0038FE, 0x0020C0, 1,WW,  MRG,IMS,NNN, (C)((int)C.CMD + 1),        "SHL" ),
                new CmdData( 0x0000FE, 0x00001A, 1,WW,  REG,MRG,NNN, (C)((int)C.CMD + 0),        "SBB" ),
                new CmdData( 0x0038FE, 0x0018F6, 1,WW,  MRG,NNN,NNN, (C)((int)C.CMD + 1),        "NEG" ),
                new CmdData( 0x0000FF, 0x0000C9, 1,00,  NNN,NNN,NNN, (C)((int)C.CMD + 0),        "LEAVE" ),
                new CmdData( 0x0000FF, 0x000060, 1,00,  NNN,NNN,NNN, (C)((int)C.CMD + (int)C.RARE + 0), "&PUSHA*" ),
                new CmdData( 0x0038FF, 0x00008F, 1,00,  MRG,NNN,NNN, (C)((int)C.POP + 1),        "POP" ),
                new CmdData( 0x0000FF, 0x000061, 1,00,  NNN,NNN,NNN, (C)((int)C.CMD + (int)C.RARE + 0), "&POPA*" ),
                new CmdData( 0x0000F8, 0x000090, 1,00,  RAC,RCM,NNN, (C)((int)C.CMD + 0),        "XCHG" ),
                new CmdData( 0x0000FE, 0x000086, 1,WW,  MRG,REG,NNN, (C)((int)C.CMD + 0),        "XCHG" ),
                new CmdData( 0x0000FE, 0x000000, 1,WW,  MRG,REG,NNN, (C)((int)C.CMD + 0),        "ADD" ),
                new CmdData( 0x0000FE, 0x000010, 1,WW,  MRG,REG,NNN, (C)((int)C.CMD + (int)C.RARE + 0), "ADC" ),
                new CmdData( 0x0000FE, 0x000012, 1,WW,  REG,MRG,NNN, (C)((int)C.CMD + (int)C.RARE + 0), "ADC" ),
                new CmdData( 0x0000FE, 0x000020, 1,WW,  MRG,REG,NNN, (C)((int)C.CMD + 0),        "AND" ),
                new CmdData( 0x0000FE, 0x000022, 1,WW,  REG,MRG,NNN, (C)((int)C.CMD + 0),        "AND" ),
                new CmdData( 0x0000FE, 0x000008, 1,WW,  MRG,REG,NNN, (C)((int)C.CMD + 0),        "OR" ),
                new CmdData( 0x0000FE, 0x00000A, 1,WW,  REG,MRG,NNN, (C)((int)C.CMD + 0),        "OR" ),
                new CmdData( 0x0000FE, 0x000028, 1,WW,  MRG,REG,NNN, (C)((int)C.CMD + 0),        "SUB" ),
                new CmdData( 0x0000FE, 0x000018, 1,WW,  MRG,REG,NNN, (C)((int)C.CMD + (int)C.RARE + 0), "SBB" ),
                new CmdData( 0x0000FE, 0x000030, 1,WW,  MRG,REG,NNN, (C)((int)C.CMD + 0),        "XOR" ),
                new CmdData( 0x0038FC, 0x001080, 1,WS,  MRG,IMM,NNN, (C)((int)C.CMD + (int)C.RARE + 1), "ADC" ),
                new CmdData( 0x0038FC, 0x001880, 1,WS,  MRG,IMM,NNN, (C)((int)C.CMD + (int)C.RARE + 1), "SBB" ),
                new CmdData( 0x0038FC, 0x003080, 1,WS,  MRG,IMU,NNN, (C)((int)C.CMD + 1),        "XOR" ),
                new CmdData( 0x0000FE, 0x000004, 1,WW,  RAC,IMM,NNN, (C)((int)C.CMD + 0),        "ADD" ),
                new CmdData( 0x0000FE, 0x000014, 1,WW,  RAC,IMM,NNN, (C)((int)C.CMD + (int)C.RARE + 0), "ADC" ),
                new CmdData( 0x0000FE, 0x000024, 1,WW,  RAC,IMU,NNN, (C)((int)C.CMD + 0),        "AND" ),
                new CmdData( 0x0000FE, 0x00000C, 1,WW,  RAC,IMU,NNN, (C)((int)C.CMD + 0),        "OR" ),
                new CmdData( 0x0000FE, 0x00002C, 1,WW,  RAC,IMM,NNN, (C)((int)C.CMD + 0),        "SUB" ),
                new CmdData( 0x0000FE, 0x00001C, 1,WW,  RAC,IMM,NNN, (C)((int)C.CMD + (int)C.RARE + 0), "SBB" ),
                new CmdData( 0x0000FE, 0x000034, 1,WW,  RAC,IMU,NNN, (C)((int)C.CMD + 0),        "XOR" ),
                new CmdData( 0x0038FE, 0x0000FE, 1,WW,  MRG,NNN,NNN, (C)((int)C.CMD + 1),        "INC" ),
                new CmdData( 0x0038FE, 0x0008FE, 1,WW,  MRG,NNN,NNN, (C)((int)C.CMD + 1),        "DEC" ),
                new CmdData( 0x0000FE, 0x0000A8, 1,WW,  RAC,IMU,NNN, (C)((int)C.CMD + 0),        "TEST" ),
                new CmdData( 0x0038FE, 0x0020F6, 1,WW,  MRG,NNN,NNN, (C)((int)C.CMD + 1),        "MUL" ),
                new CmdData( 0x0038FE, 0x0028F6, 1,WW,  MRG,NNN,NNN, (C)((int)C.CMD + 1),        "IMUL" ),
                new CmdData( 0x00FFFF, 0x00AF0F, 2,00,  REG,MRG,NNN, (C)((int)C.CMD + 0),        "IMUL" ),
                new CmdData( 0x0000FF, 0x00006B, 1,00,  REG,MRG,IMX, (C)((int)C.CMD + (int)C.RARE + 0), "IMUL" ),
                new CmdData( 0x0000FF, 0x000069, 1,00,  REG,MRG,IMM, (C)((int)C.CMD + (int)C.RARE + 0), "IMUL" ),
                new CmdData( 0x0038FE, 0x0030F6, 1,WW,  MRG,NNN,NNN, (C)((int)C.CMD + 1),        "DIV" ),
                new CmdData( 0x0038FE, 0x0038F6, 1,WW,  MRG,NNN,NNN, (C)((int)C.CMD + 1),        "IDIV" ),
                new CmdData( 0x0000FF, 0x000098, 1,00,  NNN,NNN,NNN, (C)((int)C.CMD + 0),        "&CBW:CWDE" ),
                new CmdData( 0x0000FF, 0x000099, 1,00,  NNN,NNN,NNN, (C)((int)C.CMD + 0),        "&CWD:CDQ" ),
                new CmdData( 0x0038FE, 0x0000D0, 1,WW,  MRG,C01,NNN, (C)((int)C.CMD + 1),        "ROL" ),
                new CmdData( 0x0038FE, 0x0008D0, 1,WW,  MRG,C01,NNN, (C)((int)C.CMD + 1),        "ROR" ),
                new CmdData( 0x0038FE, 0x0010D0, 1,WW,  MRG,C01,NNN, (C)((int)C.CMD + 1),        "RCL" ),
                new CmdData( 0x0038FE, 0x0018D0, 1,WW,  MRG,C01,NNN, (C)((int)C.CMD + 1),        "RCR" ),
                new CmdData( 0x0038FE, 0x0020D0, 1,WW,  MRG,C01,NNN, (C)((int)C.CMD + 1),        "SHL" ),
                new CmdData( 0x0038FE, 0x0028D0, 1,WW,  MRG,C01,NNN, (C)((int)C.CMD + 1),        "SHR" ),
                new CmdData( 0x0038FE, 0x0038D0, 1,WW,  MRG,C01,NNN, (C)((int)C.CMD + 1),        "SAR" ),
                new CmdData( 0x0038FE, 0x0000D2, 1,WW,  MRG,RCL,NNN, (C)((int)C.CMD + 1),        "ROL" ),
                new CmdData( 0x0038FE, 0x0008D2, 1,WW,  MRG,RCL,NNN, (C)((int)C.CMD + 1),        "ROR" ),
                new CmdData( 0x0038FE, 0x0010D2, 1,WW,  MRG,RCL,NNN, (C)((int)C.CMD + 1),        "RCL" ),
                new CmdData( 0x0038FE, 0x0018D2, 1,WW,  MRG,RCL,NNN, (C)((int)C.CMD + 1),        "RCR" ),
                new CmdData( 0x0038FE, 0x0020D2, 1,WW,  MRG,RCL,NNN, (C)((int)C.CMD + 1),        "SHL" ),
                new CmdData( 0x0038FE, 0x0028D2, 1,WW,  MRG,RCL,NNN, (C)((int)C.CMD + 1),        "SHR" ),
                new CmdData( 0x0038FE, 0x0038D2, 1,WW,  MRG,RCL,NNN, (C)((int)C.CMD + 1),        "SAR" ),
                new CmdData( 0x0038FE, 0x0000C0, 1,WW,  MRG,IMS,NNN, (C)((int)C.CMD + 1),        "ROL" ),
                new CmdData( 0x0038FE, 0x0008C0, 1,WW,  MRG,IMS,NNN, (C)((int)C.CMD + 1),        "ROR" ),
                new CmdData( 0x0038FE, 0x0010C0, 1,WW,  MRG,IMS,NNN, (C)((int)C.CMD + 1),        "RCL" ),
                new CmdData( 0x0038FE, 0x0018C0, 1,WW,  MRG,IMS,NNN, (C)((int)C.CMD + 1),        "RCR" ),
                new CmdData( 0x0038FE, 0x0038C0, 1,WW,  MRG,IMS,NNN, (C)((int)C.CMD + 1),        "SAR" ),
                new CmdData( 0x0000FF, 0x000070, 1,CC,  JOB,NNN,NNN, (C)((int)C.JMC + 0),        "JO" ),
                new CmdData( 0x0000FF, 0x000071, 1,CC,  JOB,NNN,NNN, (C)((int)C.JMC + 0),        "JNO" ),
                new CmdData( 0x0000FF, 0x000072, 1,CC,  JOB,NNN,NNN, (C)((int)C.JMC + 0),        "JB,JC" ),
                new CmdData( 0x0000FF, 0x000073, 1,CC,  JOB,NNN,NNN, (C)((int)C.JMC + 0),        "JNB,JNC" ),
                new CmdData( 0x0000FF, 0x000076, 1,CC,  JOB,NNN,NNN, (C)((int)C.JMC + 0),        "JBE,JNA" ),
                new CmdData( 0x0000FF, 0x000077, 1,CC,  JOB,NNN,NNN, (C)((int)C.JMC + 0),        "JA,JNBE" ),
                new CmdData( 0x0000FF, 0x000078, 1,CC,  JOB,NNN,NNN, (C)((int)C.JMC + 0),        "JS" ),
                new CmdData( 0x0000FF, 0x000079, 1,CC,  JOB,NNN,NNN, (C)((int)C.JMC + 0),        "JNS" ),
                new CmdData( 0x0000FF, 0x00007A, 1,CC,  JOB,NNN,NNN, (C)((int)C.JMC + (int)C.RARE + 0), "JPE,JP" ),
                new CmdData( 0x0000FF, 0x00007B, 1,CC,  JOB,NNN,NNN, (C)((int)C.JMC + (int)C.RARE + 0), "JPO,JNP" ),
                new CmdData( 0x0000FF, 0x0000E3, 1,00,  JOB,NNN,NNN, (C)((int)C.JMC + (int)C.RARE + 0), "$JCXZ:JECXZ" ),
                new CmdData( 0x00FFFF, 0x00800F, 2,CC,  JOW,NNN,NNN, (C)((int)C.JMC + 0),        "JO" ),
                new CmdData( 0x00FFFF, 0x00810F, 2,CC,  JOW,NNN,NNN, (C)((int)C.JMC + 0),        "JNO" ),
                new CmdData( 0x00FFFF, 0x00820F, 2,CC,  JOW,NNN,NNN, (C)((int)C.JMC + 0),        "JB,JC" ),
                new CmdData( 0x00FFFF, 0x00830F, 2,CC,  JOW,NNN,NNN, (C)((int)C.JMC + 0),        "JNB,JNC" ),
                new CmdData( 0x00FFFF, 0x00860F, 2,CC,  JOW,NNN,NNN, (C)((int)C.JMC + 0),        "JBE,JNA" ),
                new CmdData( 0x00FFFF, 0x00870F, 2,CC,  JOW,NNN,NNN, (C)((int)C.JMC + 0),        "JA,JNBE" ),
                new CmdData( 0x00FFFF, 0x00880F, 2,CC,  JOW,NNN,NNN, (C)((int)C.JMC + 0),        "JS" ),
                new CmdData( 0x00FFFF, 0x00890F, 2,CC,  JOW,NNN,NNN, (C)((int)C.JMC + 0),        "JNS" ),
                new CmdData( 0x00FFFF, 0x008A0F, 2,CC,  JOW,NNN,NNN, (C)((int)C.JMC + (int)C.RARE + 0), "JPE,JP" ),
                new CmdData( 0x00FFFF, 0x008B0F, 2,CC,  JOW,NNN,NNN, (C)((int)C.JMC + (int)C.RARE + 0), "JPO,JNP" ),
                new CmdData( 0x00FFFF, 0x008C0F, 2,CC,  JOW,NNN,NNN, (C)((int)C.JMC + 0),        "JL,JNGE" ),
                new CmdData( 0x00FFFF, 0x008D0F, 2,CC,  JOW,NNN,NNN, (C)((int)C.JMC + 0),        "JGE,JNL" ),
                new CmdData( 0x00FFFF, 0x008E0F, 2,CC,  JOW,NNN,NNN, (C)((int)C.JMC + 0),        "JLE,JNG" ),
                new CmdData( 0x00FFFF, 0x008F0F, 2,CC,  JOW,NNN,NNN, (C)((int)C.JMC + 0),        "JG,JNLE" ),
                new CmdData( 0x0000FF, 0x0000F8, 1,00,  NNN,NNN,NNN, (C)((int)C.CMD + 0),        "CLC" ),
                new CmdData( 0x0000FF, 0x0000F9, 1,00,  NNN,NNN,NNN, (C)((int)C.CMD + 0),        "STC" ),
                new CmdData( 0x0000FF, 0x0000F5, 1,00,  NNN,NNN,NNN, (C)((int)C.CMD + (int)C.RARE + 0), "CMC" ),
                new CmdData( 0x0000FF, 0x0000FC, 1,00,  NNN,NNN,NNN, (C)((int)C.CMD + 0),        "CLD" ),
                new CmdData( 0x0000FF, 0x0000FD, 1,00,  NNN,NNN,NNN, (C)((int)C.CMD + 0),        "STD" ),
                new CmdData( 0x0000FF, 0x0000FA, 1,00,  NNN,NNN,NNN, (C)((int)C.CMD + (int)C.RARE + 0), "CLI" ),
                new CmdData( 0x0000FF, 0x0000FB, 1,00,  NNN,NNN,NNN, (C)((int)C.CMD + (int)C.RARE + 0), "STI" ),
                new CmdData( 0x0000FF, 0x00008C, 1,FF,  MRG,SGM,NNN, (C)((int)C.CMD + (int)C.RARE + 0), "MOV" ),
                new CmdData( 0x0000FF, 0x00008E, 1,FF,  SGM,MRG,NNN, (C)((int)C.CMD + (int)C.RARE + 0), "MOV" ),
                new CmdData( 0x0000FE, 0x0000A6, 1,WW,  MSO,MDE,NNN, (C)((int)C.CMD + 1),        "CMPS" ),
                new CmdData( 0x0000FE, 0x0000AC, 1,WW,  MSO,NNN,NNN, (C)((int)C.CMD + 1),        "LODS" ),
                new CmdData( 0x0000FE, 0x0000A4, 1,WW,  MDE,MSO,NNN, (C)((int)C.CMD + 1),        "MOVS" ),
                new CmdData( 0x0000FE, 0x0000AE, 1,WW,  MDE,PAC,NNN, (C)((int)C.CMD + 1),        "SCAS" ),
                new CmdData( 0x0000FE, 0x0000AA, 1,WW,  MDE,PAC,NNN, (C)((int)C.CMD + 1),        "STOS" ),
                new CmdData( 0x00FEFF, 0x00A4F3, 1,WW,  MDE,MSO,PCX, (C)((int)C.REP + 1),        "REP MOVS" ),
                new CmdData( 0x00FEFF, 0x00ACF3, 1,WW,  MSO,PAC,PCX, (C)((int)C.REP + (int)C.RARE + 1), "REP LODS" ),
                new CmdData( 0x00FEFF, 0x00AAF3, 1,WW,  MDE,PAC,PCX, (C)((int)C.REP + 1),        "REP STOS" ),
                new CmdData( 0x00FEFF, 0x00A6F3, 1,WW,  MDE,MSO,PCX, (C)((int)C.REP + 1),        "REPE CMPS" ),
                new CmdData( 0x00FEFF, 0x00AEF3, 1,WW,  MDE,PAC,PCX, (C)((int)C.REP + 1),        "REPE SCAS" ),
                new CmdData( 0x00FEFF, 0x00A6F2, 1,WW,  MDE,MSO,PCX, (C)((int)C.REP + 1),        "REPNE CMPS" ),
                new CmdData( 0x00FEFF, 0x00AEF2, 1,WW,  MDE,PAC,PCX, (C)((int)C.REP + 1),        "REPNE SCAS" ),
                new CmdData( 0x0000FF, 0x0000EA, 1,00,  JMF,NNN,NNN, (C)((int)C.JMP + (int)C.RARE + 0), "JMP" ),
                new CmdData( 0x0038FF, 0x0028FF, 1,00,  MMS,NNN,NNN, (C)((int)C.JMP + (int)C.RARE + 1), "JMP" ),
                new CmdData( 0x0000FF, 0x00009A, 1,00,  JMF,NNN,NNN, (C)((int)C.CAL + (int)C.RARE + 0), "CALL" ),
                new CmdData( 0x0038FF, 0x0018FF, 1,00,  MMS,NNN,NNN, (C)((int)C.CAL + (int)C.RARE + 1), "CALL" ),
                new CmdData( 0x0000FF, 0x0000CB, 1,00,  PRF,NNN,NNN, (C)((int)C.RET + (int)C.RARE + 0), "RETF" ),
                new CmdData( 0x0000FF, 0x0000CA, 1,00,  IM2,PRF,NNN, (C)((int)C.RET + (int)C.RARE + 0), "RETF" ),
                new CmdData( 0x00FFFF, 0x00A40F, 2,00,  MRG,REG,IMS, (C)((int)C.CMD + 0),        "SHLD" ),
                new CmdData( 0x00FFFF, 0x00AC0F, 2,00,  MRG,REG,IMS, (C)((int)C.CMD + 0),        "SHRD" ),
                new CmdData( 0x00FFFF, 0x00A50F, 2,00,  MRG,REG,RCL, (C)((int)C.CMD + 0),        "SHLD" ),
                new CmdData( 0x00FFFF, 0x00AD0F, 2,00,  MRG,REG,RCL, (C)((int)C.CMD + 0),        "SHRD" ),
                new CmdData( 0x00F8FF, 0x00C80F, 2,00,  RCM,NNN,NNN, (C)((int)C.CMD + (int)C.RARE + 0), "BSWAP" ),
                new CmdData( 0x00FEFF, 0x00C00F, 2,WW,  MRG,REG,NNN, (C)((int)C.CMD + (int)C.RARE + 0), "XADD" ),
                new CmdData( 0x0000FF, 0x0000E2, 1,LL,  JOB,PCX,NNN, (C)((int)C.JMC + 0),        "$LOOP*" ),
                new CmdData( 0x0000FF, 0x0000E1, 1,LL,  JOB,PCX,NNN, (C)((int)C.JMC + 0),        "$LOOP*E" ),
                new CmdData( 0x0000FF, 0x0000E0, 1,LL,  JOB,PCX,NNN, (C)((int)C.JMC + 0),        "$LOOP*NE" ),
                new CmdData( 0x0000FF, 0x0000C8, 1,00,  IM2,IM1,NNN, (C)((int)C.CMD + 0),        "ENTER" ),
                new CmdData( 0x0000FE, 0x0000E4, 1,WP,  RAC,IM1,NNN, (C)((int)C.CMD + (int)C.RARE + 0), "IN" ),
                new CmdData( 0x0000FE, 0x0000EC, 1,WP,  RAC,RDX,NNN, (C)((int)C.CMD + (int)C.RARE + 0), "IN" ),
                new CmdData( 0x0000FE, 0x0000E6, 1,WP,  IM1,RAC,NNN, (C)((int)C.CMD + (int)C.RARE + 0), "OUT" ),
                new CmdData( 0x0000FE, 0x0000EE, 1,WP,  RDX,RAC,NNN, (C)((int)C.CMD + (int)C.RARE + 0), "OUT" ),
                new CmdData( 0x0000FE, 0x00006C, 1,WP,  MDE,RDX,NNN, (C)((int)C.CMD + (int)C.RARE + 1), "INS" ),
                new CmdData( 0x0000FE, 0x00006E, 1,WP,  RDX,MDE,NNN, (C)((int)C.CMD + (int)C.RARE + 1), "OUTS" ),
                new CmdData( 0x00FEFF, 0x006CF3, 1,WP,  MDE,RDX,PCX, (C)((int)C.REP + (int)C.RARE + 1), "REP INS" ),
                new CmdData( 0x00FEFF, 0x006EF3, 1,WP,  RDX,MDE,PCX, (C)((int)C.REP + (int)C.RARE + 1), "REP OUTS" ),
                new CmdData( 0x0000FF, 0x000037, 1,00,  NNN,NNN,NNN, (C)((int)C.CMD + (int)C.RARE + 0), "AAA" ),
                new CmdData( 0x0000FF, 0x00003F, 1,00,  NNN,NNN,NNN, (C)((int)C.CMD + (int)C.RARE + 0), "AAS" ),
                new CmdData( 0x00FFFF, 0x000AD4, 2,00,  NNN,NNN,NNN, (C)((int)C.CMD + (int)C.RARE + 0), "AAM" ),
                new CmdData( 0x0000FF, 0x0000D4, 1,00,  IM1,NNN,NNN, (C)((int)C.CMD + (int)C.RARE + 0), "AAM" ),
                new CmdData( 0x00FFFF, 0x000AD5, 2,00,  NNN,NNN,NNN, (C)((int)C.CMD + (int)C.RARE + 0), "AAD" ),
                new CmdData( 0x0000FF, 0x0000D5, 1,00,  IM1,NNN,NNN, (C)((int)C.CMD + (int)C.RARE + 0), "AAD" ),
                new CmdData( 0x0000FF, 0x000027, 1,00,  NNN,NNN,NNN, (C)((int)C.CMD + (int)C.RARE + 0), "DAA" ),
                new CmdData( 0x0000FF, 0x00002F, 1,00,  NNN,NNN,NNN, (C)((int)C.CMD + (int)C.RARE + 0), "DAS" ),
                new CmdData( 0x0000FF, 0x0000F4, 1,PR,  NNN,NNN,NNN, (C)((int)C.PRI + (int)C.RARE + 0), "HLT" ),
                new CmdData( 0x0000FF, 0x00000E, 1,00,  SCM,NNN,NNN, (C)((int)C.PSH + (int)C.RARE + 0), "PUSH" ),
                new CmdData( 0x0000FF, 0x000016, 1,00,  SCM,NNN,NNN, (C)((int)C.PSH + (int)C.RARE + 0), "PUSH" ),
                new CmdData( 0x0000FF, 0x00001E, 1,00,  SCM,NNN,NNN, (C)((int)C.PSH + (int)C.RARE + 0), "PUSH" ),
                new CmdData( 0x0000FF, 0x000006, 1,00,  SCM,NNN,NNN, (C)((int)C.PSH + (int)C.RARE + 0), "PUSH" ),
                new CmdData( 0x00FFFF, 0x00A00F, 2,00,  SCM,NNN,NNN, (C)((int)C.PSH + (int)C.RARE + 0), "PUSH" ),
                new CmdData( 0x00FFFF, 0x00A80F, 2,00,  SCM,NNN,NNN, (C)((int)C.PSH + (int)C.RARE + 0), "PUSH" ),
                new CmdData( 0x0000FF, 0x00001F, 1,00,  SCM,NNN,NNN, (C)((int)C.POP + (int)C.RARE + 0), "POP" ),
                new CmdData( 0x0000FF, 0x000007, 1,00,  SCM,NNN,NNN, (C)((int)C.POP + (int)C.RARE + 0), "POP" ),
                new CmdData( 0x0000FF, 0x000017, 1,00,  SCM,NNN,NNN, (C)((int)C.POP + (int)C.RARE + 0), "POP" ),
                new CmdData( 0x00FFFF, 0x00A10F, 2,00,  SCM,NNN,NNN, (C)((int)C.POP + (int)C.RARE + 0), "POP" ),
                new CmdData( 0x00FFFF, 0x00A90F, 2,00,  SCM,NNN,NNN, (C)((int)C.POP + (int)C.RARE + 0), "POP" ),
                new CmdData( 0x0000FF, 0x0000D7, 1,00,  MXL,NNN,NNN, (C)((int)C.CMD + (int)C.RARE + 1), "XLAT" ),
                new CmdData( 0x00FFFF, 0x00BE0F, 2,00,  REG,MR1,NNN, (C)((int)C.CMD + 1),        "MOVSX" ),
                new CmdData( 0x00FFFF, 0x00B60F, 2,00,  REG,MR1,NNN, (C)((int)C.CMD + 1),        "MOVZX" ),
                new CmdData( 0x00FFFF, 0x00B70F, 2,00,  REG,MR2,NNN, (C)((int)C.CMD + 1),        "MOVZX" ),
                new CmdData( 0x0000FF, 0x00009B, 1,00,  NNN,NNN,NNN, (C)((int)C.CMD + 0),        "WAIT" ),
                new CmdData( 0x0000FF, 0x00009F, 1,00,  PAH,PFL,NNN, (C)((int)C.CMD + (int)C.RARE + 0), "LAHF" ),
                new CmdData( 0x0000FF, 0x00009E, 1,00,  PFL,PAH,NNN, (C)((int)C.CMD + (int)C.RARE + 0), "SAHF" ),
                new CmdData( 0x0000FF, 0x00009C, 1,00,  NNN,NNN,NNN, (C)((int)C.PSH + 0),        "&PUSHF*" ),
                new CmdData( 0x0000FF, 0x00009D, 1,00,  NNN,NNN,NNN, (C)((int)C.FLG + 0),        "&POPF*" ),
                new CmdData( 0x0000FF, 0x0000CD, 1,00,  IM1,NNN,NNN, (C)((int)C.CAL + (int)C.RARE + 0), "INT" ),
                new CmdData( 0x0000FF, 0x0000CC, 1,00,  NNN,NNN,NNN, (C)((int)C.CAL + (int)C.RARE + 0), "INT3" ),
                new CmdData( 0x0000FF, 0x0000CE, 1,00,  NNN,NNN,NNN, (C)((int)C.CAL + (int)C.RARE + 0), "INTO" ),
                new CmdData( 0x0000FF, 0x0000CF, 1,00,  NNN,NNN,NNN, (C)((int)C.RTF + (int)C.RARE + 0), "&IRET*" ),
                new CmdData( 0x00FFFF, 0x00900F, 2,CC,  MR1,NNN,NNN, (C)((int)C.CMD + 0),        "SETO" ),
                new CmdData( 0x00FFFF, 0x00910F, 2,CC,  MR1,NNN,NNN, (C)((int)C.CMD + 0),        "SETNO" ),
                new CmdData( 0x00FFFF, 0x00920F, 2,CC,  MR1,NNN,NNN, (C)((int)C.CMD + 0),        "SETB,SETC" ),
                new CmdData( 0x00FFFF, 0x00930F, 2,CC,  MR1,NNN,NNN, (C)((int)C.CMD + 0),        "SETNB,SETNC" ),
                new CmdData( 0x00FFFF, 0x00940F, 2,CC,  MR1,NNN,NNN, (C)((int)C.CMD + 0),        "SETE,SETZ" ),
                new CmdData( 0x00FFFF, 0x00950F, 2,CC,  MR1,NNN,NNN, (C)((int)C.CMD + 0),        "SETNE,SETNZ" ),
                new CmdData( 0x00FFFF, 0x00960F, 2,CC,  MR1,NNN,NNN, (C)((int)C.CMD + 0),        "SETBE,SETNA" ),
                new CmdData( 0x00FFFF, 0x00970F, 2,CC,  MR1,NNN,NNN, (C)((int)C.CMD + 0),        "SETA,SETNBE" ),
                new CmdData( 0x00FFFF, 0x00980F, 2,CC,  MR1,NNN,NNN, (C)((int)C.CMD + 0),        "SETS" ),
                new CmdData( 0x00FFFF, 0x00990F, 2,CC,  MR1,NNN,NNN, (C)((int)C.CMD + 0),        "SETNS" ),
                new CmdData( 0x00FFFF, 0x009A0F, 2,CC,  MR1,NNN,NNN, (C)((int)C.CMD + (int)C.RARE + 0), "SETPE,SETP" ),
                new CmdData( 0x00FFFF, 0x009B0F, 2,CC,  MR1,NNN,NNN, (C)((int)C.CMD + (int)C.RARE + 0), "SETPO,SETNP" ),
                new CmdData( 0x00FFFF, 0x009C0F, 2,CC,  MR1,NNN,NNN, (C)((int)C.CMD + 0),        "SETL,SETNGE" ),
                new CmdData( 0x00FFFF, 0x009D0F, 2,CC,  MR1,NNN,NNN, (C)((int)C.CMD + 0),        "SETGE,SETNL" ),
                new CmdData( 0x00FFFF, 0x009E0F, 2,CC,  MR1,NNN,NNN, (C)((int)C.CMD + 0),        "SETLE,SETNG" ),
                new CmdData( 0x00FFFF, 0x009F0F, 2,CC,  MR1,NNN,NNN, (C)((int)C.CMD + 0),        "SETG,SETNLE" ),
                new CmdData( 0x38FFFF, 0x20BA0F, 2,00,  MRG,IM1,NNN, (C)((int)C.CMD + (int)C.RARE + 1), "BT" ),
                new CmdData( 0x38FFFF, 0x28BA0F, 2,00,  MRG,IM1,NNN, (C)((int)C.CMD + (int)C.RARE + 1), "BTS" ),
                new CmdData( 0x38FFFF, 0x30BA0F, 2,00,  MRG,IM1,NNN, (C)((int)C.CMD + (int)C.RARE + 1), "BTR" ),
                new CmdData( 0x38FFFF, 0x38BA0F, 2,00,  MRG,IM1,NNN, (C)((int)C.CMD + (int)C.RARE + 1), "BTC" ),
                new CmdData( 0x00FFFF, 0x00A30F, 2,00,  MRG,REG,NNN, (C)((int)C.CMD + (int)C.RARE + 1), "BT" ),
                new CmdData( 0x00FFFF, 0x00AB0F, 2,00,  MRG,REG,NNN, (C)((int)C.CMD + (int)C.RARE + 1), "BTS" ),
                new CmdData( 0x00FFFF, 0x00B30F, 2,00,  MRG,REG,NNN, (C)((int)C.CMD + (int)C.RARE + 1), "BTR" ),
                new CmdData( 0x00FFFF, 0x00BB0F, 2,00,  MRG,REG,NNN, (C)((int)C.CMD + (int)C.RARE + 1), "BTC" ),
                new CmdData( 0x0000FF, 0x0000C5, 1,00,  REG,MML,NNN, (C)((int)C.CMD + (int)C.RARE + 0), "LDS" ),
                new CmdData( 0x0000FF, 0x0000C4, 1,00,  REG,MML,NNN, (C)((int)C.CMD + (int)C.RARE + 0), "LES" ),
                new CmdData( 0x00FFFF, 0x00B40F, 2,00,  REG,MML,NNN, (C)((int)C.CMD + (int)C.RARE + 0), "LFS" ),
                new CmdData( 0x00FFFF, 0x00B50F, 2,00,  REG,MML,NNN, (C)((int)C.CMD + (int)C.RARE + 0), "LGS" ),
                new CmdData( 0x00FFFF, 0x00B20F, 2,00,  REG,MML,NNN, (C)((int)C.CMD + (int)C.RARE + 0), "LSS" ),
                new CmdData( 0x0000FF, 0x000063, 1,00,  MRG,REG,NNN, (C)((int)C.CMD + (int)C.RARE + 0), "ARPL" ),
                new CmdData( 0x0000FF, 0x000062, 1,00,  REG,MMB,NNN, (C)((int)C.CMD + (int)C.RARE + 0), "BOUND" ),
                new CmdData( 0x00FFFF, 0x00BC0F, 2,00,  REG,MRG,NNN, (C)((int)C.CMD + (int)C.RARE + 0), "BSF" ),
                new CmdData( 0x00FFFF, 0x00BD0F, 2,00,  REG,MRG,NNN, (C)((int)C.CMD + (int)C.RARE + 0), "BSR" ),
                new CmdData( 0x00FFFF, 0x00060F, 2,PR,  NNN,NNN,NNN, (C)((int)C.CMD + (int)C.RARE + 0), "CLTS" ),
                new CmdData( 0x00FFFF, 0x00400F, 2,CC,  REG,MRG,NNN, (C)((int)C.CMD + 0),        "CMOVO" ),
                new CmdData( 0x00FFFF, 0x00410F, 2,CC,  REG,MRG,NNN, (C)((int)C.CMD + 0),        "CMOVNO" ),
                new CmdData( 0x00FFFF, 0x00420F, 2,CC,  REG,MRG,NNN, (C)((int)C.CMD + 0),        "CMOVB,CMOVC" ),
                new CmdData( 0x00FFFF, 0x00430F, 2,CC,  REG,MRG,NNN, (C)((int)C.CMD + 0),        "CMOVNB,CMOVNC" ),
                new CmdData( 0x00FFFF, 0x00440F, 2,CC,  REG,MRG,NNN, (C)((int)C.CMD + 0),        "CMOVE,CMOVZ" ),
                new CmdData( 0x00FFFF, 0x00450F, 2,CC,  REG,MRG,NNN, (C)((int)C.CMD + 0),        "CMOVNE,CMOVNZ" ),
                new CmdData( 0x00FFFF, 0x00460F, 2,CC,  REG,MRG,NNN, (C)((int)C.CMD + 0),        "CMOVBE,CMOVNA" ),
                new CmdData( 0x00FFFF, 0x00470F, 2,CC,  REG,MRG,NNN, (C)((int)C.CMD + 0),        "CMOVA,CMOVNBE" ),
                new CmdData( 0x00FFFF, 0x00480F, 2,CC,  REG,MRG,NNN, (C)((int)C.CMD + 0),        "CMOVS" ),
                new CmdData( 0x00FFFF, 0x00490F, 2,CC,  REG,MRG,NNN, (C)((int)C.CMD + 0),        "CMOVNS" ),
                new CmdData( 0x00FFFF, 0x004A0F, 2,CC,  REG,MRG,NNN, (C)((int)C.CMD + 0),        "CMOVPE,CMOVP" ),
                new CmdData( 0x00FFFF, 0x004B0F, 2,CC,  REG,MRG,NNN, (C)((int)C.CMD + 0),        "CMOVPO,CMOVNP" ),
                new CmdData( 0x00FFFF, 0x004C0F, 2,CC,  REG,MRG,NNN, (C)((int)C.CMD + 0),        "CMOVL,CMOVNGE" ),
                new CmdData( 0x00FFFF, 0x004D0F, 2,CC,  REG,MRG,NNN, (C)((int)C.CMD + 0),        "CMOVGE,CMOVNL" ),
                new CmdData( 0x00FFFF, 0x004E0F, 2,CC,  REG,MRG,NNN, (C)((int)C.CMD + 0),        "CMOVLE,CMOVNG" ),
                new CmdData( 0x00FFFF, 0x004F0F, 2,CC,  REG,MRG,NNN, (C)((int)C.CMD + 0),        "CMOVG,CMOVNLE" ),
                new CmdData( 0x00FEFF, 0x00B00F, 2,WW,  MRG,REG,NNN, (C)((int)C.CMD + (int)C.RARE + 0), "CMPXCHG" ),
                new CmdData( 0x38FFFF, 0x08C70F, 2,00,  MD8,NNN,NNN, (C)((int)C.CMD + (int)C.RARE + 1), "CMPXCHG8B" ),
                new CmdData( 0x00FFFF, 0x00A20F, 2,00,  NNN,NNN,NNN, (C)((int)C.CMD + 0),        "CPUID" ),
                new CmdData( 0x00FFFF, 0x00080F, 2,PR,  NNN,NNN,NNN, (C)((int)C.CMD + (int)C.RARE + 0), "INVD" ),
                new CmdData( 0x00FFFF, 0x00020F, 2,00,  REG,MRG,NNN, (C)((int)C.CMD + (int)C.RARE + 0), "LAR" ),
                new CmdData( 0x00FFFF, 0x00030F, 2,00,  REG,MRG,NNN, (C)((int)C.CMD + (int)C.RARE + 0), "LSL" ),
                new CmdData( 0x38FFFF, 0x38010F, 2,PR,  MR1,NNN,NNN, (C)((int)C.CMD + (int)C.RARE + 0), "INVLPG" ),
                new CmdData( 0x00FFFF, 0x00090F, 2,PR,  NNN,NNN,NNN, (C)((int)C.CMD + (int)C.RARE + 0), "WBINVD" ),
                new CmdData( 0x38FFFF, 0x10010F, 2,PR,  MM6,NNN,NNN, (C)((int)C.CMD + (int)C.RARE + 0), "LGDT" ),
                new CmdData( 0x38FFFF, 0x00010F, 2,00,  MM6,NNN,NNN, (C)((int)C.CMD + (int)C.RARE + 0), "SGDT" ),
                new CmdData( 0x38FFFF, 0x18010F, 2,PR,  MM6,NNN,NNN, (C)((int)C.CMD + (int)C.RARE + 0), "LIDT" ),
                new CmdData( 0x38FFFF, 0x08010F, 2,00,  MM6,NNN,NNN, (C)((int)C.CMD + (int)C.RARE + 0), "SIDT" ),
                new CmdData( 0x38FFFF, 0x10000F, 2,PR,  MR2,NNN,NNN, (C)((int)C.CMD + (int)C.RARE + 0), "LLDT" ),
                new CmdData( 0x38FFFF, 0x00000F, 2,00,  MR2,NNN,NNN, (C)((int)C.CMD + (int)C.RARE + 0), "SLDT" ),
                new CmdData( 0x38FFFF, 0x18000F, 2,PR,  MR2,NNN,NNN, (C)((int)C.CMD + (int)C.RARE + 0), "LTR" ),
                new CmdData( 0x38FFFF, 0x08000F, 2,00,  MR2,NNN,NNN, (C)((int)C.CMD + (int)C.RARE + 0), "STR" ),
                new CmdData( 0x38FFFF, 0x30010F, 2,PR,  MR2,NNN,NNN, (C)((int)C.CMD + (int)C.RARE + 0), "LMSW" ),
                new CmdData( 0x38FFFF, 0x20010F, 2,00,  MR2,NNN,NNN, (C)((int)C.CMD + (int)C.RARE + 0), "SMSW" ),
                new CmdData( 0x38FFFF, 0x20000F, 2,00,  MR2,NNN,NNN, (C)((int)C.CMD + (int)C.RARE + 0), "VERR" ),
                new CmdData( 0x38FFFF, 0x28000F, 2,00,  MR2,NNN,NNN, (C)((int)C.CMD + (int)C.RARE + 0), "VERW" ),
                new CmdData( 0xC0FFFF, 0xC0220F, 2,PR,  CRX,RR4,NNN, (C)((int)C.CMD + (int)C.RARE + 0), "MOV" ),
                new CmdData( 0xC0FFFF, 0xC0200F, 2,00,  RR4,CRX,NNN, (C)((int)C.CMD + (int)C.RARE + 0), "MOV" ),
                new CmdData( 0xC0FFFF, 0xC0230F, 2,PR,  DRX,RR4,NNN, (C)((int)C.CMD + (int)C.RARE + 0), "MOV" ),
                new CmdData( 0xC0FFFF, 0xC0210F, 2,PR,  RR4,DRX,NNN, (C)((int)C.CMD + (int)C.RARE + 0), "MOV" ),
                new CmdData( 0x00FFFF, 0x00310F, 2,00,  NNN,NNN,NNN, (C)((int)C.CMD + (int)C.RARE + 0), "RDTSC" ),
                new CmdData( 0x00FFFF, 0x00320F, 2,PR,  NNN,NNN,NNN, (C)((int)C.CMD + (int)C.RARE + 0), "RDMSR" ),
                new CmdData( 0x00FFFF, 0x00300F, 2,PR,  NNN,NNN,NNN, (C)((int)C.CMD + (int)C.RARE + 0), "WRMSR" ),
                new CmdData( 0x00FFFF, 0x00330F, 2,PR,  NNN,NNN,NNN, (C)((int)C.CMD + (int)C.RARE + 0), "RDPMC" ),
                new CmdData( 0x00FFFF, 0x00AA0F, 2,PR,  NNN,NNN,NNN, (C)((int)C.RTF + (int)C.RARE + 0), "RSM" ),
                new CmdData( 0x00FFFF, 0x000B0F, 2,00,  NNN,NNN,NNN, (C)((int)C.CMD + (int)C.RARE + 0), "UD2" ),
                new CmdData( 0x00FFFF, 0x00340F, 2,00,  NNN,NNN,NNN, (C)((int)C.CMD + (int)C.RARE + 0), "SYSENTER" ),
                new CmdData( 0x00FFFF, 0x00350F, 2,PR,  NNN,NNN,NNN, (C)((int)C.CMD + (int)C.RARE + 0), "SYSEXIT" ),
                new CmdData( 0x0000FF, 0x0000D6, 1,00,  NNN,NNN,NNN, (C)((int)C.CMD + (int)C.RARE + 0), "SALC" ),
	            // FPU instructions. Never change the order of instructions!
	            new CmdData( 0x00FFFF, 0x00F0D9, 2,00,  PS0,NNN,NNN, (C)((int)C.FLT + 0),        "F2XM1" ),
                new CmdData( 0x00FFFF, 0x00E0D9, 2,00,  PS0,NNN,NNN, (C)((int)C.FLT + 0),        "FCHS" ),
                new CmdData( 0x00FFFF, 0x00E1D9, 2,00,  PS0,NNN,NNN, (C)((int)C.FLT + 0),        "FABS" ),
                new CmdData( 0x00FFFF, 0x00E2DB, 2,00,  NNN,NNN,NNN, (C)((int)C.FLT + 0),        "FCLEX" ),
                new CmdData( 0x00FFFF, 0x00E3DB, 2,00,  NNN,NNN,NNN, (C)((int)C.FLT + 0),        "FINIT" ),
                new CmdData( 0x00FFFF, 0x00F6D9, 2,00,  NNN,NNN,NNN, (C)((int)C.FLT + 0),        "FDECSTP" ),
                new CmdData( 0x00FFFF, 0x00F7D9, 2,00,  NNN,NNN,NNN, (C)((int)C.FLT + 0),        "FINCSTP" ),
                new CmdData( 0x00FFFF, 0x00E4D9, 2,00,  PS0,NNN,NNN, (C)((int)C.FLT + 0),        "FTST" ),
                new CmdData( 0x00FFFF, 0x00FAD9, 2,00,  PS0,NNN,NNN, (C)((int)C.FLT + 0),        "FSQRT" ),
                new CmdData( 0x00FFFF, 0x00FED9, 2,00,  PS0,NNN,NNN, (C)((int)C.FLT + 0),        "FSIN" ),
                new CmdData( 0x00FFFF, 0x00FFD9, 2,00,  PS0,NNN,NNN, (C)((int)C.FLT + 0),        "FCOS" ),
                new CmdData( 0x00FFFF, 0x00FBD9, 2,00,  PS0,NNN,NNN, (C)((int)C.FLT + 0),        "FSINCOS" ),
                new CmdData( 0x00FFFF, 0x00F2D9, 2,00,  PS0,NNN,NNN, (C)((int)C.FLT + 0),        "FPTAN" ),
                new CmdData( 0x00FFFF, 0x00F3D9, 2,00,  PS0,PS1,NNN, (C)((int)C.FLT + 0),        "FPATAN" ),
                new CmdData( 0x00FFFF, 0x00F8D9, 2,00,  PS1,PS0,NNN, (C)((int)C.FLT + 0),        "FPREM" ),
                new CmdData( 0x00FFFF, 0x00F5D9, 2,00,  PS1,PS0,NNN, (C)((int)C.FLT + 0),        "FPREM1" ),
                new CmdData( 0x00FFFF, 0x00F1D9, 2,00,  PS0,PS1,NNN, (C)((int)C.FLT + 0),        "FYL2X" ),
                new CmdData( 0x00FFFF, 0x00F9D9, 2,00,  PS0,PS1,NNN, (C)((int)C.FLT + 0),        "FYL2XP1" ),
                new CmdData( 0x00FFFF, 0x00FCD9, 2,00,  PS0,NNN,NNN, (C)((int)C.FLT + 0),        "FRNDINT" ),
                new CmdData( 0x00FFFF, 0x00E8D9, 2,00,  NNN,NNN,NNN, (C)((int)C.FLT + 0),        "FLD1" ),
                new CmdData( 0x00FFFF, 0x00E9D9, 2,00,  NNN,NNN,NNN, (C)((int)C.FLT + 0),        "FLDL2T" ),
                new CmdData( 0x00FFFF, 0x00EAD9, 2,00,  NNN,NNN,NNN, (C)((int)C.FLT + 0),        "FLDL2E" ),
                new CmdData( 0x00FFFF, 0x00EBD9, 2,00,  NNN,NNN,NNN, (C)((int)C.FLT + 0),        "FLDPI" ),
                new CmdData( 0x00FFFF, 0x00ECD9, 2,00,  NNN,NNN,NNN, (C)((int)C.FLT + 0),        "FLDLG2" ),
                new CmdData( 0x00FFFF, 0x00EDD9, 2,00,  NNN,NNN,NNN, (C)((int)C.FLT + 0),        "FLDLN2" ),
                new CmdData( 0x00FFFF, 0x00EED9, 2,00,  NNN,NNN,NNN, (C)((int)C.FLT + 0),        "FLDZ" ),
                new CmdData( 0x00FFFF, 0x00FDD9, 2,00,  PS0,PS1,NNN, (C)((int)C.FLT + 0),        "FSCALE" ),
                new CmdData( 0x00FFFF, 0x00D0D9, 2,00,  NNN,NNN,NNN, (C)((int)C.FLT + 0),        "FNOP" ),
                new CmdData( 0x00FFFF, 0x00E0DF, 2,FF,  RAX,NNN,NNN, (C)((int)C.FLT + 0),        "FSTSW" ),
                new CmdData( 0x00FFFF, 0x00E5D9, 2,00,  PS0,NNN,NNN, (C)((int)C.FLT + 0),        "FXAM" ),
                new CmdData( 0x00FFFF, 0x00F4D9, 2,00,  PS0,NNN,NNN, (C)((int)C.FLT + 0),        "FXTRACT" ),
                new CmdData( 0x00FFFF, 0x00D9DE, 2,00,  PS0,PS1,NNN, (C)((int)C.FLT + 0),        "FCOMPP" ),
                new CmdData( 0x00FFFF, 0x00E9DA, 2,00,  PS0,PS1,NNN, (C)((int)C.FLT + 0),        "FUCOMPP" ),
                new CmdData( 0x00F8FF, 0x00C0DD, 2,00,  RST,NNN,NNN, (C)((int)C.FLT + 0),        "FFREE" ),
                new CmdData( 0x00F8FF, 0x00C0DA, 2,00,  RS0,RST,NNN, (C)((int)C.FLT + 0),        "FCMOVB" ),
                new CmdData( 0x00F8FF, 0x00C8DA, 2,00,  RS0,RST,NNN, (C)((int)C.FLT + 0),        "FCMOVE" ),
                new CmdData( 0x00F8FF, 0x00D0DA, 2,00,  RS0,RST,NNN, (C)((int)C.FLT + 0),        "FCMOVBE" ),
                new CmdData( 0x00F8FF, 0x00D8DA, 2,00,  RS0,RST,NNN, (C)((int)C.FLT + 0),        "FCMOVU" ),
                new CmdData( 0x00F8FF, 0x00C0DB, 2,00,  RS0,RST,NNN, (C)((int)C.FLT + 0),        "FCMOVNB" ),
                new CmdData( 0x00F8FF, 0x00C8DB, 2,00,  RS0,RST,NNN, (C)((int)C.FLT + 0),        "FCMOVNE" ),
                new CmdData( 0x00F8FF, 0x00D0DB, 2,00,  RS0,RST,NNN, (C)((int)C.FLT + 0),        "FCMOVNBE" ),
                new CmdData( 0x00F8FF, 0x00D8DB, 2,00,  RS0,RST,NNN, (C)((int)C.FLT + 0),        "FCMOVNU" ),
                new CmdData( 0x00F8FF, 0x00F0DB, 2,00,  RS0,RST,NNN, (C)((int)C.FLT + 0),        "FCOMI" ),
                new CmdData( 0x00F8FF, 0x00F0DF, 2,00,  RS0,RST,NNN, (C)((int)C.FLT + 0),        "FCOMIP" ),
                new CmdData( 0x00F8FF, 0x00E8DB, 2,00,  RS0,RST,NNN, (C)((int)C.FLT + 0),        "FUCOMI" ),
                new CmdData( 0x00F8FF, 0x00E8DF, 2,00,  RS0,RST,NNN, (C)((int)C.FLT + 0),        "FUCOMIP" ),
                new CmdData( 0x00F8FF, 0x00C0D8, 2,00,  RS0,RST,NNN, (C)((int)C.FLT + 0),        "FADD" ),
                new CmdData( 0x00F8FF, 0x00C0DC, 2,00,  RST,RS0,NNN, (C)((int)C.FLT + 0),        "FADD" ),
                new CmdData( 0x00F8FF, 0x00C0DE, 2,00,  RST,RS0,NNN, (C)((int)C.FLT + 0),        "FADDP" ),
                new CmdData( 0x00F8FF, 0x00E0D8, 2,00,  RS0,RST,NNN, (C)((int)C.FLT + 0),        "FSUB" ),
                new CmdData( 0x00F8FF, 0x00E8DC, 2,00,  RST,RS0,NNN, (C)((int)C.FLT + 0),        "FSUB" ),
                new CmdData( 0x00F8FF, 0x00E8DE, 2,00,  RST,RS0,NNN, (C)((int)C.FLT + 0),        "FSUBP" ),
                new CmdData( 0x00F8FF, 0x00E8D8, 2,00,  RS0,RST,NNN, (C)((int)C.FLT + 0),        "FSUBR" ),
                new CmdData( 0x00F8FF, 0x00E0DC, 2,00,  RST,RS0,NNN, (C)((int)C.FLT + 0),        "FSUBR" ),
                new CmdData( 0x00F8FF, 0x00E0DE, 2,00,  RST,RS0,NNN, (C)((int)C.FLT + 0),        "FSUBRP" ),
                new CmdData( 0x00F8FF, 0x00C8D8, 2,00,  RS0,RST,NNN, (C)((int)C.FLT + 0),        "FMUL" ),
                new CmdData( 0x00F8FF, 0x00C8DC, 2,00,  RST,RS0,NNN, (C)((int)C.FLT + 0),        "FMUL" ),
                new CmdData( 0x00F8FF, 0x00C8DE, 2,00,  RST,RS0,NNN, (C)((int)C.FLT + 0),        "FMULP" ),
                new CmdData( 0x00F8FF, 0x00D0D8, 2,00,  RST,PS0,NNN, (C)((int)C.FLT + 0),        "FCOM" ),
                new CmdData( 0x00F8FF, 0x00D8D8, 2,00,  RST,PS0,NNN, (C)((int)C.FLT + 0),        "FCOMP" ),
                new CmdData( 0x00F8FF, 0x00E0DD, 2,00,  RST,PS0,NNN, (C)((int)C.FLT + 0),        "FUCOM" ),
                new CmdData( 0x00F8FF, 0x00E8DD, 2,00,  RST,PS0,NNN, (C)((int)C.FLT + 0),        "FUCOMP" ),
                new CmdData( 0x00F8FF, 0x00F0D8, 2,00,  RS0,RST,NNN, (C)((int)C.FLT + 0),        "FDIV" ),
                new CmdData( 0x00F8FF, 0x00F8DC, 2,00,  RST,RS0,NNN, (C)((int)C.FLT + 0),        "FDIV" ),
                new CmdData( 0x00F8FF, 0x00F8DE, 2,00,  RST,RS0,NNN, (C)((int)C.FLT + 0),        "FDIVP" ),
                new CmdData( 0x00F8FF, 0x00F8D8, 2,00,  RS0,RST,NNN, (C)((int)C.FLT + 0),        "FDIVR" ),
                new CmdData( 0x00F8FF, 0x00F0DC, 2,00,  RST,RS0,NNN, (C)((int)C.FLT + 0),        "FDIVR" ),
                new CmdData( 0x00F8FF, 0x00F0DE, 2,00,  RST,RS0,NNN, (C)((int)C.FLT + 0),        "FDIVRP" ),
                new CmdData( 0x00F8FF, 0x00C0D9, 2,00,  RST,NNN,NNN, (C)((int)C.FLT + 0),        "FLD" ),
                new CmdData( 0x00F8FF, 0x00D0DD, 2,00,  RST,PS0,NNN, (C)((int)C.FLT + 0),        "FST" ),
                new CmdData( 0x00F8FF, 0x00D8DD, 2,00,  RST,PS0,NNN, (C)((int)C.FLT + 0),        "FSTP" ),
                new CmdData( 0x00F8FF, 0x00C8D9, 2,00,  RST,PS0,NNN, (C)((int)C.FLT + 0),        "FXCH" ),
                new CmdData( 0x0038FF, 0x0000D8, 1,00,  MF4,PS0,NNN, (C)((int)C.FLT + 1),        "FADD" ),
                new CmdData( 0x0038FF, 0x0000DC, 1,00,  MF8,PS0,NNN, (C)((int)C.FLT + 1),        "FADD" ),
                new CmdData( 0x0038FF, 0x0000DA, 1,00,  MD4,PS0,NNN, (C)((int)C.FLT + 1),        "FIADD" ),
                new CmdData( 0x0038FF, 0x0000DE, 1,00,  MD2,PS0,NNN, (C)((int)C.FLT + 1),        "FIADD" ),
                new CmdData( 0x0038FF, 0x0020D8, 1,00,  MF4,PS0,NNN, (C)((int)C.FLT + 1),        "FSUB" ),
                new CmdData( 0x0038FF, 0x0020DC, 1,00,  MF8,PS0,NNN, (C)((int)C.FLT + 1),        "FSUB" ),
                new CmdData( 0x0038FF, 0x0020DA, 1,00,  MD4,PS0,NNN, (C)((int)C.FLT + 1),        "FISUB" ),
                new CmdData( 0x0038FF, 0x0020DE, 1,00,  MD2,PS0,NNN, (C)((int)C.FLT + 1),        "FISUB" ),
                new CmdData( 0x0038FF, 0x0028D8, 1,00,  MF4,PS0,NNN, (C)((int)C.FLT + 1),        "FSUBR" ),
                new CmdData( 0x0038FF, 0x0028DC, 1,00,  MF8,PS0,NNN, (C)((int)C.FLT + 1),        "FSUBR" ),
                new CmdData( 0x0038FF, 0x0028DA, 1,00,  MD4,PS0,NNN, (C)((int)C.FLT + 1),        "FISUBR" ),
                new CmdData( 0x0038FF, 0x0028DE, 1,00,  MD2,PS0,NNN, (C)((int)C.FLT + 1),        "FISUBR" ),
                new CmdData( 0x0038FF, 0x0008D8, 1,00,  MF4,PS0,NNN, (C)((int)C.FLT + 1),        "FMUL" ),
                new CmdData( 0x0038FF, 0x0008DC, 1,00,  MF8,PS0,NNN, (C)((int)C.FLT + 1),        "FMUL" ),
                new CmdData( 0x0038FF, 0x0008DA, 1,00,  MD4,PS0,NNN, (C)((int)C.FLT + 1),        "FIMUL" ),
                new CmdData( 0x0038FF, 0x0008DE, 1,00,  MD2,PS0,NNN, (C)((int)C.FLT + 1),        "FIMUL" ),
                new CmdData( 0x0038FF, 0x0010D8, 1,00,  MF4,PS0,NNN, (C)((int)C.FLT + 1),        "FCOM" ),
                new CmdData( 0x0038FF, 0x0010DC, 1,00,  MF8,PS0,NNN, (C)((int)C.FLT + 1),        "FCOM" ),
                new CmdData( 0x0038FF, 0x0018D8, 1,00,  MF4,PS0,NNN, (C)((int)C.FLT + 1),        "FCOMP" ),
                new CmdData( 0x0038FF, 0x0018DC, 1,00,  MF8,PS0,NNN, (C)((int)C.FLT + 1),        "FCOMP" ),
                new CmdData( 0x0038FF, 0x0030D8, 1,00,  MF4,PS0,NNN, (C)((int)C.FLT + 1),        "FDIV" ),
                new CmdData( 0x0038FF, 0x0030DC, 1,00,  MF8,PS0,NNN, (C)((int)C.FLT + 1),        "FDIV" ),
                new CmdData( 0x0038FF, 0x0030DA, 1,00,  MD4,PS0,NNN, (C)((int)C.FLT + 1),        "FIDIV" ),
                new CmdData( 0x0038FF, 0x0030DE, 1,00,  MD2,PS0,NNN, (C)((int)C.FLT + 1),        "FIDIV" ),
                new CmdData( 0x0038FF, 0x0038D8, 1,00,  MF4,PS0,NNN, (C)((int)C.FLT + 1),        "FDIVR" ),
                new CmdData( 0x0038FF, 0x0038DC, 1,00,  MF8,PS0,NNN, (C)((int)C.FLT + 1),        "FDIVR" ),
                new CmdData( 0x0038FF, 0x0038DA, 1,00,  MD4,PS0,NNN, (C)((int)C.FLT + 1),        "FIDIVR" ),
                new CmdData( 0x0038FF, 0x0038DE, 1,00,  MD2,PS0,NNN, (C)((int)C.FLT + 1),        "FIDIVR" ),
                new CmdData( 0x0038FF, 0x0020DF, 1,00,  MDA,NNN,NNN, (C)((int)C.FLT + (int)C.RARE + 1), "FBLD" ),
                new CmdData( 0x0038FF, 0x0030DF, 1,00,  MDA,PS0,NNN, (C)((int)C.FLT + (int)C.RARE + 1), "FBSTP" ),
                new CmdData( 0x0038FF, 0x0010DE, 1,00,  MD2,PS0,NNN, (C)((int)C.FLT + 1),        "FICOM" ),
                new CmdData( 0x0038FF, 0x0010DA, 1,00,  MD4,PS0,NNN, (C)((int)C.FLT + 1),        "FICOM" ),
                new CmdData( 0x0038FF, 0x0018DE, 1,00,  MD2,PS0,NNN, (C)((int)C.FLT + 1),        "FICOMP" ),
                new CmdData( 0x0038FF, 0x0018DA, 1,00,  MD4,PS0,NNN, (C)((int)C.FLT + 1),        "FICOMP" ),
                new CmdData( 0x0038FF, 0x0000DF, 1,00,  MD2,NNN,NNN, (C)((int)C.FLT + 1),        "FILD" ),
                new CmdData( 0x0038FF, 0x0000DB, 1,00,  MD4,NNN,NNN, (C)((int)C.FLT + 1),        "FILD" ),
                new CmdData( 0x0038FF, 0x0028DF, 1,00,  MD8,NNN,NNN, (C)((int)C.FLT + 1),        "FILD" ),
                new CmdData( 0x0038FF, 0x0010DF, 1,00,  MD2,PS0,NNN, (C)((int)C.FLT + 1),        "FIST" ),
                new CmdData( 0x0038FF, 0x0010DB, 1,00,  MD4,PS0,NNN, (C)((int)C.FLT + 1),        "FIST" ),
                new CmdData( 0x0038FF, 0x0018DF, 1,00,  MD2,PS0,NNN, (C)((int)C.FLT + 1),        "FISTP" ),
                new CmdData( 0x0038FF, 0x0018DB, 1,00,  MD4,PS0,NNN, (C)((int)C.FLT + 1),        "FISTP" ),
                new CmdData( 0x0038FF, 0x0038DF, 1,00,  MD8,PS0,NNN, (C)((int)C.FLT + 1),        "FISTP" ),
                new CmdData( 0x0038FF, 0x0000D9, 1,00,  MF4,NNN,NNN, (C)((int)C.FLT + 1),        "FLD" ),
                new CmdData( 0x0038FF, 0x0000DD, 1,00,  MF8,NNN,NNN, (C)((int)C.FLT + 1),        "FLD" ),
                new CmdData( 0x0038FF, 0x0028DB, 1,00,  MFA,NNN,NNN, (C)((int)C.FLT + 1),        "FLD" ),
                new CmdData( 0x0038FF, 0x0010D9, 1,00,  MF4,PS0,NNN, (C)((int)C.FLT + 1),        "FST" ),
                new CmdData( 0x0038FF, 0x0010DD, 1,00,  MF8,PS0,NNN, (C)((int)C.FLT + 1),        "FST" ),
                new CmdData( 0x0038FF, 0x0018D9, 1,00,  MF4,PS0,NNN, (C)((int)C.FLT + 1),        "FSTP" ),
                new CmdData( 0x0038FF, 0x0018DD, 1,00,  MF8,PS0,NNN, (C)((int)C.FLT + 1),        "FSTP" ),
                new CmdData( 0x0038FF, 0x0038DB, 1,00,  MFA,PS0,NNN, (C)((int)C.FLT + 1),        "FSTP" ),
                new CmdData( 0x0038FF, 0x0028D9, 1,00,  MB2,NNN,NNN, (C)((int)C.FLT + 0),        "FLDCW" ),
                new CmdData( 0x0038FF, 0x0038D9, 1,00,  MB2,NNN,NNN, (C)((int)C.FLT + 0),        "FSTCW" ),
                new CmdData( 0x0038FF, 0x0020D9, 1,00,  MFE,NNN,NNN, (C)((int)C.FLT + 0),        "FLDENV" ),
                new CmdData( 0x0038FF, 0x0030D9, 1,00,  MFE,NNN,NNN, (C)((int)C.FLT + 0),        "FSTENV" ),
                new CmdData( 0x0038FF, 0x0020DD, 1,00,  MFS,NNN,NNN, (C)((int)C.FLT + 0),        "FRSTOR" ),
                new CmdData( 0x0038FF, 0x0030DD, 1,00,  MFS,NNN,NNN, (C)((int)C.FLT + 0),        "FSAVE" ),
                new CmdData( 0x0038FF, 0x0038DD, 1,00,  MB2,NNN,NNN, (C)((int)C.FLT + 0),        "FSTSW" ),
                new CmdData( 0x38FFFF, 0x08AE0F, 2,00,  MFX,NNN,NNN, (C)((int)C.FLT + 0),        "FXRSTOR" ),
                new CmdData( 0x38FFFF, 0x00AE0F, 2,00,  MFX,NNN,NNN, (C)((int)C.FLT + 0),        "FXSAVE" ),
                new CmdData( 0x00FFFF, 0x00E0DB, 2,00,  NNN,NNN,NNN, (C)((int)C.FLT + 0),        "FENI" ),
                new CmdData( 0x00FFFF, 0x00E1DB, 2,00,  NNN,NNN,NNN, (C)((int)C.FLT + 0),        "FDISI" ),
	            // MMX instructions. Length of MMX operand fields (in bytes) is added to the
	            // type, length of 0 means 8-byte MMX operand.
	            new CmdData( 0x00FFFF, 0x00770F, 2,00,  NNN,NNN,NNN, (C)((int)C.MMX + 0),        "EMMS" ),
                new CmdData( 0x00FFFF, 0x006E0F, 2,00,  RMX,MR4,NNN, (C)((int)C.MMX + 0),        "MOVD" ),
                new CmdData( 0x00FFFF, 0x007E0F, 2,00,  MR4,RMX,NNN, (C)((int)C.MMX + 0),        "MOVD" ),
                new CmdData( 0x00FFFF, 0x006F0F, 2,00,  RMX,MR8,NNN, (C)((int)C.MMX + 0),        "MOVQ" ),
                new CmdData( 0x00FFFF, 0x007F0F, 2,00,  MR8,RMX,NNN, (C)((int)C.MMX + 0),        "MOVQ" ),
                new CmdData( 0x00FFFF, 0x00630F, 2,00,  RMX,MR8,NNN, (C)((int)C.MMX + 2),        "PACKSSWB" ),
                new CmdData( 0x00FFFF, 0x006B0F, 2,00,  RMX,MR8,NNN, (C)((int)C.MMX + 4),        "PACKSSDW" ),
                new CmdData( 0x00FFFF, 0x00670F, 2,00,  RMX,MR8,NNN, (C)((int)C.MMX + 2),        "PACKUSWB" ),
                new CmdData( 0x00FFFF, 0x00FC0F, 2,00,  RMX,MR8,NNN, (C)((int)C.MMX + 1),        "PADDB" ),
                new CmdData( 0x00FFFF, 0x00FD0F, 2,00,  RMX,MR8,NNN, (C)((int)C.MMX + 2),        "PADDW" ),
                new CmdData( 0x00FFFF, 0x00FE0F, 2,00,  RMX,MR8,NNN, (C)((int)C.MMX + 4),        "PADDD" ),
                new CmdData( 0x00FFFF, 0x00F80F, 2,00,  RMX,MR8,NNN, (C)((int)C.MMX + 1),        "PSUBB" ),
                new CmdData( 0x00FFFF, 0x00F90F, 2,00,  RMX,MR8,NNN, (C)((int)C.MMX + 2),        "PSUBW" ),
                new CmdData( 0x00FFFF, 0x00FA0F, 2,00,  RMX,MR8,NNN, (C)((int)C.MMX + 4),        "PSUBD" ),
                new CmdData( 0x00FFFF, 0x00EC0F, 2,00,  RMX,MR8,NNN, (C)((int)C.MMX + 1),        "PADDSB" ),
                new CmdData( 0x00FFFF, 0x00ED0F, 2,00,  RMX,MR8,NNN, (C)((int)C.MMX + 2),        "PADDSW" ),
                new CmdData( 0x00FFFF, 0x00E80F, 2,00,  RMX,MR8,NNN, (C)((int)C.MMX + 1),        "PSUBSB" ),
                new CmdData( 0x00FFFF, 0x00E90F, 2,00,  RMX,MR8,NNN, (C)((int)C.MMX + 2),        "PSUBSW" ),
                new CmdData( 0x00FFFF, 0x00DC0F, 2,00,  RMX,MR8,NNN, (C)((int)C.MMX + 1),        "PADDUSB" ),
                new CmdData( 0x00FFFF, 0x00DD0F, 2,00,  RMX,MR8,NNN, (C)((int)C.MMX + 2),        "PADDUSW" ),
                new CmdData( 0x00FFFF, 0x00D80F, 2,00,  RMX,MR8,NNN, (C)((int)C.MMX + 1),        "PSUBUSB" ),
                new CmdData( 0x00FFFF, 0x00D90F, 2,00,  RMX,MR8,NNN, (C)((int)C.MMX + 2),        "PSUBUSW" ),
                new CmdData( 0x00FFFF, 0x00DB0F, 2,00,  RMX,MR8,NNN, (C)((int)C.MMX + 0),        "PAND" ),
                new CmdData( 0x00FFFF, 0x00DF0F, 2,00,  RMX,MR8,NNN, (C)((int)C.MMX + 0),        "PANDN" ),
                new CmdData( 0x00FFFF, 0x00740F, 2,00,  RMX,MR8,NNN, (C)((int)C.MMX + 1),        "PCMPEQB" ),
                new CmdData( 0x00FFFF, 0x00750F, 2,00,  RMX,MR8,NNN, (C)((int)C.MMX + 2),        "PCMPEQW" ),
                new CmdData( 0x00FFFF, 0x00760F, 2,00,  RMX,MR8,NNN, (C)((int)C.MMX + 4),        "PCMPEQD" ),
                new CmdData( 0x00FFFF, 0x00640F, 2,00,  RMX,MR8,NNN, (C)((int)C.MMX + 1),        "PCMPGTB" ),
                new CmdData( 0x00FFFF, 0x00650F, 2,00,  RMX,MR8,NNN, (C)((int)C.MMX + 2),        "PCMPGTW" ),
                new CmdData( 0x00FFFF, 0x00660F, 2,00,  RMX,MR8,NNN, (C)((int)C.MMX + 4),        "PCMPGTD" ),
                new CmdData( 0x00FFFF, 0x00F50F, 2,00,  RMX,MR8,NNN, (C)((int)C.MMX + 2),        "PMADDWD" ),
                new CmdData( 0x00FFFF, 0x00E50F, 2,00,  RMX,MR8,NNN, (C)((int)C.MMX + 2),        "PMULHW" ),
                new CmdData( 0x00FFFF, 0x00D50F, 2,00,  RMX,MR8,NNN, (C)((int)C.MMX + 2),        "PMULLW" ),
                new CmdData( 0x00FFFF, 0x00EB0F, 2,00,  RMX,MR8,NNN, (C)((int)C.MMX + 0),        "POR" ),
                new CmdData( 0x00FFFF, 0x00F10F, 2,00,  RMX,MR8,NNN, (C)((int)C.MMX + 2),        "PSLLW" ),
                new CmdData( 0x38FFFF, 0x30710F, 2,00,  MR8,IM1,NNN, (C)((int)C.MMX + 2),        "PSLLW" ),
                new CmdData( 0x00FFFF, 0x00F20F, 2,00,  RMX,MR8,NNN, (C)((int)C.MMX + 4),        "PSLLD" ),
                new CmdData( 0x38FFFF, 0x30720F, 2,00,  MR8,IM1,NNN, (C)((int)C.MMX + 4),        "PSLLD" ),
                new CmdData( 0x00FFFF, 0x00F30F, 2,00,  RMX,MR8,NNN, (C)((int)C.MMX + 0),        "PSLLQ" ),
                new CmdData( 0x38FFFF, 0x30730F, 2,00,  MR8,IM1,NNN, (C)((int)C.MMX + 0),        "PSLLQ" ),
                new CmdData( 0x00FFFF, 0x00E10F, 2,00,  RMX,MR8,NNN, (C)((int)C.MMX + 2),        "PSRAW" ),
                new CmdData( 0x38FFFF, 0x20710F, 2,00,  MR8,IM1,NNN, (C)((int)C.MMX + 2),        "PSRAW" ),
                new CmdData( 0x00FFFF, 0x00E20F, 2,00,  RMX,MR8,NNN, (C)((int)C.MMX + 4),        "PSRAD" ),
                new CmdData( 0x38FFFF, 0x20720F, 2,00,  MR8,IM1,NNN, (C)((int)C.MMX + 4),        "PSRAD" ),
                new CmdData( 0x00FFFF, 0x00D10F, 2,00,  RMX,MR8,NNN, (C)((int)C.MMX + 2),        "PSRLW" ),
                new CmdData( 0x38FFFF, 0x10710F, 2,00,  MR8,IM1,NNN, (C)((int)C.MMX + 2),        "PSRLW" ),
                new CmdData( 0x00FFFF, 0x00D20F, 2,00,  RMX,MR8,NNN, (C)((int)C.MMX + 4),        "PSRLD" ),
                new CmdData( 0x38FFFF, 0x10720F, 2,00,  MR8,IM1,NNN, (C)((int)C.MMX + 4),        "PSRLD" ),
                new CmdData( 0x00FFFF, 0x00D30F, 2,00,  RMX,MR8,NNN, (C)((int)C.MMX + 0),        "PSRLQ" ),
                new CmdData( 0x38FFFF, 0x10730F, 2,00,  MR8,IM1,NNN, (C)((int)C.MMX + 0),        "PSRLQ" ),
                new CmdData( 0x00FFFF, 0x00680F, 2,00,  RMX,MR8,NNN, (C)((int)C.MMX + 1),        "PUNPCKHBW" ),
                new CmdData( 0x00FFFF, 0x00690F, 2,00,  RMX,MR8,NNN, (C)((int)C.MMX + 2),        "PUNPCKHWD" ),
                new CmdData( 0x00FFFF, 0x006A0F, 2,00,  RMX,MR8,NNN, (C)((int)C.MMX + 4),        "PUNPCKHDQ" ),
                new CmdData( 0x00FFFF, 0x00600F, 2,00,  RMX,MR8,NNN, (C)((int)C.MMX + 1),        "PUNPCKLBW" ),
                new CmdData( 0x00FFFF, 0x00610F, 2,00,  RMX,MR8,NNN, (C)((int)C.MMX + 2),        "PUNPCKLWD" ),
                new CmdData( 0x00FFFF, 0x00620F, 2,00,  RMX,MR8,NNN, (C)((int)C.MMX + 4),        "PUNPCKLDQ" ),
                new CmdData( 0x00FFFF, 0x00EF0F, 2,00,  RMX,MR8,NNN, (C)((int)C.MMX + 0),        "PXOR" ),
	            // AMD extentions to MMX command set (including Athlon/PIII extentions).
	            new CmdData( 0x00FFFF, 0x000E0F, 2,00,  NNN,NNN,NNN, (C)((int)C.MMX + 0),        "FEMMS" ),
                new CmdData( 0x38FFFF, 0x000D0F, 2,00,  MD8,NNN,NNN, (C)((int)C.MMX + 0),        "PREFETCH" ),
                new CmdData( 0x38FFFF, 0x080D0F, 2,00,  MD8,NNN,NNN, (C)((int)C.MMX + 0),        "PREFETCHW" ),
                new CmdData( 0x00FFFF, 0x00F70F, 2,00,  RMX,RR8,PDI, (C)((int)C.MMX + 1),        "MASKMOVQ" ),
                new CmdData( 0x00FFFF, 0x00E70F, 2,00,  MD8,RMX,NNN, (C)((int)C.MMX + 0),        "MOVNTQ" ),
                new CmdData( 0x00FFFF, 0x00E00F, 2,00,  RMX,MR8,NNN, (C)((int)C.MMX + 1),        "PAVGB" ),
                new CmdData( 0x00FFFF, 0x00E30F, 2,00,  RMX,MR8,NNN, (C)((int)C.MMX + 2),        "PAVGW" ),
                new CmdData( 0x00FFFF, 0x00C50F, 2,00,  RR4,RMX,IM1, (C)((int)C.MMX + 2),        "PEXTRW" ),
                new CmdData( 0x00FFFF, 0x00C40F, 2,00,  RMX,MR2,IM1, (C)((int)C.MMX + 2),        "PINSRW" ),
                new CmdData( 0x00FFFF, 0x00EE0F, 2,00,  RMX,MR8,NNN, (C)((int)C.MMX + 2),        "PMAXSW" ),
                new CmdData( 0x00FFFF, 0x00DE0F, 2,00,  RMX,MR8,NNN, (C)((int)C.MMX + 1),        "PMAXUB" ),
                new CmdData( 0x00FFFF, 0x00EA0F, 2,00,  RMX,MR8,NNN, (C)((int)C.MMX + 2),        "PMINSW" ),
                new CmdData( 0x00FFFF, 0x00DA0F, 2,00,  RMX,MR8,NNN, (C)((int)C.MMX + 1),        "PMINUB" ),
                new CmdData( 0x00FFFF, 0x00D70F, 2,00,  RG4,RR8,NNN, (C)((int)C.MMX + 1),        "PMOVMSKB" ),
                new CmdData( 0x00FFFF, 0x00E40F, 2,00,  RMX,MR8,NNN, (C)((int)C.MMX + 2),        "PMULHUW" ),
                new CmdData( 0x38FFFF, 0x00180F, 2,00,  MD8,NNN,NNN, (C)((int)C.MMX + 0),        "PREFETCHNTA" ),
                new CmdData( 0x38FFFF, 0x08180F, 2,00,  MD8,NNN,NNN, (C)((int)C.MMX + 0),        "PREFETCHT0" ),
                new CmdData( 0x38FFFF, 0x10180F, 2,00,  MD8,NNN,NNN, (C)((int)C.MMX + 0),        "PREFETCHT1" ),
                new CmdData( 0x38FFFF, 0x18180F, 2,00,  MD8,NNN,NNN, (C)((int)C.MMX + 0),        "PREFETCHT2" ),
                new CmdData( 0x00FFFF, 0x00F60F, 2,00,  RMX,MR8,NNN, (C)((int)C.MMX + 1),        "PSADBW" ),
                new CmdData( 0x00FFFF, 0x00700F, 2,00,  RMX,MR8,IM1, (C)((int)C.MMX + 2),        "PSHUFW" ),
                new CmdData( 0xFFFFFF, 0xF8AE0F, 2,00,  NNN,NNN,NNN, (C)((int)C.MMX + 0),        "SFENCE" ),
	            // AMD 3DNow! instructions (including Athlon extentions).
	            new CmdData( 0x00FFFF, 0xBF0F0F, 2,00,  RMX,MR8,NNN, (C)((int)C.NOW + 1),        "PAVGUSB" ),
                new CmdData( 0x00FFFF, 0x9E0F0F, 2,00,  R3D,MRD,NNN, (C)((int)C.NOW + 4),        "PFADD" ),
                new CmdData( 0x00FFFF, 0x9A0F0F, 2,00,  R3D,MRD,NNN, (C)((int)C.NOW + 4),        "PFSUB" ),
                new CmdData( 0x00FFFF, 0xAA0F0F, 2,00,  R3D,MRD,NNN, (C)((int)C.NOW + 4),        "PFSUBR" ),
                new CmdData( 0x00FFFF, 0xAE0F0F, 2,00,  R3D,MRD,NNN, (C)((int)C.NOW + 4),        "PFACC" ),
                new CmdData( 0x00FFFF, 0x900F0F, 2,00,  RMX,MRD,NNN, (C)((int)C.NOW + 4),        "PFCMPGE" ),
                new CmdData( 0x00FFFF, 0xA00F0F, 2,00,  RMX,MRD,NNN, (C)((int)C.NOW + 4),        "PFCMPGT" ),
                new CmdData( 0x00FFFF, 0xB00F0F, 2,00,  RMX,MRD,NNN, (C)((int)C.NOW + 4),        "PFCMPEQ" ),
                new CmdData( 0x00FFFF, 0x940F0F, 2,00,  R3D,MRD,NNN, (C)((int)C.NOW + 4),        "PFMIN" ),
                new CmdData( 0x00FFFF, 0xA40F0F, 2,00,  R3D,MRD,NNN, (C)((int)C.NOW + 4),        "PFMAX" ),
                new CmdData( 0x00FFFF, 0x0D0F0F, 2,00,  R3D,MR8,NNN, (C)((int)C.NOW + 4),        "PI2FD" ),
                new CmdData( 0x00FFFF, 0x1D0F0F, 2,00,  RMX,MRD,NNN, (C)((int)C.NOW + 4),        "PF2ID" ),
                new CmdData( 0x00FFFF, 0x960F0F, 2,00,  R3D,MRD,NNN, (C)((int)C.NOW + 4),        "PFRCP" ),
                new CmdData( 0x00FFFF, 0x970F0F, 2,00,  R3D,MRD,NNN, (C)((int)C.NOW + 4),        "PFRSQRT" ),
                new CmdData( 0x00FFFF, 0xB40F0F, 2,00,  R3D,MRD,NNN, (C)((int)C.NOW + 4),        "PFMUL" ),
                new CmdData( 0x00FFFF, 0xA60F0F, 2,00,  R3D,MRD,NNN, (C)((int)C.NOW + 4),        "PFRCPIT1" ),
                new CmdData( 0x00FFFF, 0xA70F0F, 2,00,  R3D,MRD,NNN, (C)((int)C.NOW + 4),        "PFRSQIT1" ),
                new CmdData( 0x00FFFF, 0xB60F0F, 2,00,  R3D,MRD,NNN, (C)((int)C.NOW + 4),        "PFRCPIT2" ),
                new CmdData( 0x00FFFF, 0xB70F0F, 2,00,  RMX,MR8,NNN, (C)((int)C.NOW + 2),        "PMULHRW" ),
                new CmdData( 0x00FFFF, 0x1C0F0F, 2,00,  RMX,MRD,NNN, (C)((int)C.NOW + 4),        "PF2IW" ),
                new CmdData( 0x00FFFF, 0x8A0F0F, 2,00,  R3D,MRD,NNN, (C)((int)C.NOW + 4),        "PFNACC" ),
                new CmdData( 0x00FFFF, 0x8E0F0F, 2,00,  R3D,MRD,NNN, (C)((int)C.NOW + 4),        "PFPNACC" ),
                new CmdData( 0x00FFFF, 0x0C0F0F, 2,00,  R3D,MR8,NNN, (C)((int)C.NOW + 4),        "PI2FW" ),
                new CmdData( 0x00FFFF, 0xBB0F0F, 2,00,  R3D,MRD,NNN, (C)((int)C.NOW + 4),        "PSWAPD" ),
	            // Some alternative mnemonics for Assembler, not used by Disassembler (so
	            // implicit pseudooperands are not marked).
	            new CmdData( 0x0000FF, 0x0000A6, 1,00,  NNN,NNN,NNN, (C)((int)C.CMD + 0),        "CMPSB" ),
                new CmdData( 0x00FFFF, 0x00A766, 2,00,  NNN,NNN,NNN, (C)((int)C.CMD + 0),        "CMPSW" ),
                new CmdData( 0x0000FF, 0x0000A7, 1,00,  NNN,NNN,NNN, (C)((int)C.CMD + 0),        "CMPSD" ),
                new CmdData( 0x0000FF, 0x0000AC, 1,00,  NNN,NNN,NNN, (C)((int)C.CMD + 0),        "LODSB" ),
                new CmdData( 0x00FFFF, 0x00AD66, 2,00,  NNN,NNN,NNN, (C)((int)C.CMD + 0),        "LODSW" ),
                new CmdData( 0x0000FF, 0x0000AD, 1,00,  NNN,NNN,NNN, (C)((int)C.CMD + 0),        "LODSD" ),
                new CmdData( 0x0000FF, 0x0000A4, 1,00,  NNN,NNN,NNN, (C)((int)C.CMD + 0),        "MOVSB" ),
                new CmdData( 0x00FFFF, 0x00A566, 2,00,  NNN,NNN,NNN, (C)((int)C.CMD + 0),        "MOVSW" ),
                new CmdData( 0x0000FF, 0x0000A5, 1,00,  NNN,NNN,NNN, (C)((int)C.CMD + 0),        "MOVSD" ),
                new CmdData( 0x0000FF, 0x0000AE, 1,00,  NNN,NNN,NNN, (C)((int)C.CMD + 0),        "SCASB" ),
                new CmdData( 0x00FFFF, 0x00AF66, 1,00,  NNN,NNN,NNN, (C)((int)C.CMD + 0),        "SCASW" ),
                new CmdData( 0x0000FF, 0x0000AF, 1,00,  NNN,NNN,NNN, (C)((int)C.CMD + 0),        "SCASD" ),
                new CmdData( 0x0000FF, 0x0000AA, 1,00,  NNN,NNN,NNN, (C)((int)C.CMD + 0),        "STOSB" ),
                new CmdData( 0x00FFFF, 0x00AB66, 2,00,  NNN,NNN,NNN, (C)((int)C.CMD + 0),        "STOSW" ),
                new CmdData( 0x0000FF, 0x0000AB, 1,00,  NNN,NNN,NNN, (C)((int)C.CMD + 0),        "STOSD" ),
                new CmdData( 0x00FFFF, 0x00A4F3, 1,00,  NNN,NNN,NNN, (C)((int)C.REP + 0),        "REP MOVSB" ),
                new CmdData( 0xFFFFFF, 0xA5F366, 2,00,  NNN,NNN,NNN, (C)((int)C.REP + 0),        "REP MOVSW" ),
                new CmdData( 0x00FFFF, 0x00A5F3, 1,00,  NNN,NNN,NNN, (C)((int)C.REP + 0),        "REP MOVSD" ),
                new CmdData( 0x00FFFF, 0x00ACF3, 1,00,  NNN,NNN,NNN, (C)((int)C.REP + 0),        "REP LODSB" ),
                new CmdData( 0xFFFFFF, 0xADF366, 2,00,  NNN,NNN,NNN, (C)((int)C.REP + 0),        "REP LODSW" ),
                new CmdData( 0x00FFFF, 0x00ADF3, 1,00,  NNN,NNN,NNN, (C)((int)C.REP + 0),        "REP LODSD" ),
                new CmdData( 0x00FFFF, 0x00AAF3, 1,00,  NNN,NNN,NNN, (C)((int)C.REP + 0),        "REP STOSB" ),
                new CmdData( 0xFFFFFF, 0xABF366, 2,00,  NNN,NNN,NNN, (C)((int)C.REP + 0),        "REP STOSW" ),
                new CmdData( 0x00FFFF, 0x00ABF3, 1,00,  NNN,NNN,NNN, (C)((int)C.REP + 0),        "REP STOSD" ),
                new CmdData( 0x00FFFF, 0x00A6F3, 1,00,  NNN,NNN,NNN, (C)((int)C.REP + 0),        "REPE CMPSB" ),
                new CmdData( 0xFFFFFF, 0xA7F366, 2,00,  NNN,NNN,NNN, (C)((int)C.REP + 0),        "REPE CMPSW" ),
                new CmdData( 0x00FFFF, 0x00A7F3, 1,00,  NNN,NNN,NNN, (C)((int)C.REP + 0),        "REPE CMPSD" ),
                new CmdData( 0x00FFFF, 0x00AEF3, 1,00,  NNN,NNN,NNN, (C)((int)C.REP + 0),        "REPE SCASB" ),
                new CmdData( 0xFFFFFF, 0xAFF366, 2,00,  NNN,NNN,NNN, (C)((int)C.REP + 0),        "REPE SCASW" ),
                new CmdData( 0x00FFFF, 0x00AFF3, 1,00,  NNN,NNN,NNN, (C)((int)C.REP + 0),        "REPE SCASD" ),
                new CmdData( 0x00FFFF, 0x00A6F2, 1,00,  NNN,NNN,NNN, (C)((int)C.REP + 0),        "REPNE CMPSB" ),
                new CmdData( 0xFFFFFF, 0xA7F266, 2,00,  NNN,NNN,NNN, (C)((int)C.REP + 0),        "REPNE CMPSW" ),
                new CmdData( 0x00FFFF, 0x00A7F2, 1,00,  NNN,NNN,NNN, (C)((int)C.REP + 0),        "REPNE CMPSD" ),
                new CmdData( 0x00FFFF, 0x00AEF2, 1,00,  NNN,NNN,NNN, (C)((int)C.REP + 0),        "REPNE SCASB" ),
                new CmdData( 0xFFFFFF, 0xAFF266, 2,00,  NNN,NNN,NNN, (C)((int)C.REP + 0),        "REPNE SCASW" ),
                new CmdData( 0x00FFFF, 0x00AFF2, 1,00,  NNN,NNN,NNN, (C)((int)C.REP + 0),        "REPNE SCASD" ),
                new CmdData( 0x0000FF, 0x00006C, 1,00,  NNN,NNN,NNN, (C)((int)C.CMD + (int)C.RARE + 0), "INSB" ),
                new CmdData( 0x00FFFF, 0x006D66, 2,00,  NNN,NNN,NNN, (C)((int)C.CMD + (int)C.RARE + 0), "INSW" ),
                new CmdData( 0x0000FF, 0x00006D, 1,00,  NNN,NNN,NNN, (C)((int)C.CMD + (int)C.RARE + 0), "INSD" ),
                new CmdData( 0x0000FF, 0x00006E, 1,00,  NNN,NNN,NNN, (C)((int)C.CMD + (int)C.RARE + 0), "OUTSB" ),
                new CmdData( 0x00FFFF, 0x006F66, 2,00,  NNN,NNN,NNN, (C)((int)C.CMD + (int)C.RARE + 0), "OUTSW" ),
                new CmdData( 0x0000FF, 0x00006F, 1,00,  NNN,NNN,NNN, (C)((int)C.CMD + (int)C.RARE + 0), "OUTSD" ),
                new CmdData( 0x00FFFF, 0x006CF3, 1,00,  NNN,NNN,NNN, (C)((int)C.REP + 0),        "REP INSB" ),
                new CmdData( 0xFFFFFF, 0x6DF366, 2,00,  NNN,NNN,NNN, (C)((int)C.REP + 0),        "REP INSW" ),
                new CmdData( 0x00FFFF, 0x006DF3, 1,00,  NNN,NNN,NNN, (C)((int)C.REP + 0),        "REP INSD" ),
                new CmdData( 0x00FFFF, 0x006EF3, 1,00,  NNN,NNN,NNN, (C)((int)C.REP + 0),        "REP OUTSB" ),
                new CmdData( 0xFFFFFF, 0x6FF366, 2,00,  NNN,NNN,NNN, (C)((int)C.REP + 0),        "REP OUTSW" ),
                new CmdData( 0x00FFFF, 0x006FF3, 1,00,  NNN,NNN,NNN, (C)((int)C.REP + 0),        "REP OUTSD" ),
                new CmdData( 0x0000FF, 0x0000E1, 1,00,  JOB,NNN,NNN, (C)((int)C.JMC + 0),        "$LOOP*Z" ),
                new CmdData( 0x0000FF, 0x0000E0, 1,00,  JOB,NNN,NNN, (C)((int)C.JMC + 0),        "$LOOP*NZ" ),
                new CmdData( 0x0000FF, 0x00009B, 1,00,  NNN,NNN,NNN, (C)((int)C.CMD + 0),        "FWAIT" ),
                new CmdData( 0x0000FF, 0x0000D7, 1,00,  NNN,NNN,NNN, (C)((int)C.CMD + 0),        "XLATB" ),
                new CmdData( 0x00FFFF, 0x00C40F, 2,00,  RMX,RR4,IM1, (C)((int)C.MMX + 2),        "PINSRW" ),
                new CmdData( 0x00FFFF, 0x0020CD, 2,00,  VXD,NNN,NNN, (C)((int)C.CAL + (int)C.RARE + 0), "VxDCall" ),
	            // Pseudocommands used by Assembler for masked search only.
	            new CmdData( 0x0000F0, 0x000070, 1,CC,  JOB,NNN,NNN, (C)((int)C.JMC + 0),        "JCC" ),
                new CmdData( 0x00F0FF, 0x00800F, 2,CC,  JOW,NNN,NNN, (C)((int)C.JMC + 0),        "JCC" ),
                new CmdData( 0x00F0FF, 0x00900F, 2,CC,  MR1,NNN,NNN, (C)((int)C.CMD + 1),        "SETCC" ),
                new CmdData( 0x00F0FF, 0x00400F, 2,CC,  REG,MRG,NNN, (C)((int)C.CMD + 0),        "CMOVCC" ),
	            // End of command table.
	            new CmdData( 0x000000, 0x000000, 0,00,  NNN,NNN,NNN, (C)((int)C.CMD + 0),        "" )
            };

        public static CmdData vxdcmd =               // Decoding of VxD calls (Win95/98)
  new CmdData(0x00FFFF, 0x0020CD, 2, 00, VXD, NNN, NNN, (C)((int)C.CAL + (int)C.RARE + 0), "VxDCall");

        // Bit combinations that can be potentially dangerous when executed:
        public static CmdData[] dangerous = new CmdData[]{
  new CmdData( 0x00FFFF, 0x00DCF7, 0,0,0,0,0,(C)C_DANGER.DANGER95,"Win95/98 may crash when NEG ESP is executed" ),
  new CmdData( 0x00FFFF, 0x00D4F7, 0,0,0,0,0,(C)C_DANGER.DANGER95,"Win95/98 may crash when NOT ESP is executed" ),
  new CmdData( 0x00FFFF, 0x0020CD, 0,0,0,0,0,(C)C_DANGER.DANGER95,"Win95/98 may crash when VxD call is executed in user mode" ),
  new CmdData( 0xF8FFFF, 0xC8C70F, 0,0,0,0,1,(C)C_DANGER.DANGERLOCK,"LOCK CMPXCHG8B may crash some processors when executed" )
};
    }
}
