using RedStoneLib.Packets;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Anantashesha.Decompiler.Disassemble
{
    unsafe partial class Disassembler
    {
        ////////////////////////////////////////////////////////////////////////////////
        //////////////////////////////// SYMBOLIC NAMES ////////////////////////////////

        // 8-bit register names, sorted by 'natural' index (as understood by CPU, not
        // in the alphabetical order as some 'programmers' prefer).
        static string[] regname8 = new string[NREG] {
          "AL",       "CL",       "DL",       "BL",
          "AH",       "CH",       "DH",       "BH"  };

        // 16-bit register names.
        static string[] regname16 = new string[NREG] {
          "AX",       "CX",       "DX",       "BX",
          "SP",       "BP",       "SI",       "DI"  };

        // 32-bit register names.
        static string[] regname32 = new string[NREG] {
          "EAX",      "ECX",      "EDX",      "EBX",
          "ESP",      "EBP",      "ESI",      "EDI" };

        // Names of segment registers.
        static string[] segname = new string[NREG] {
          "ES",       "CS",       "SS",       "DS",
          "FS",       "GS",       "SEG6:",    "SEG7:" };

        // Names of FPU registers, classical form.
        static string[] fpulong = new string[NREG] {
          "ST(0)",    "ST(1)",    "ST(2)",    "ST(3)",
          "ST(4)",    "ST(5)",    "ST(6)",    "ST(7)" };

        // Names of FPU registers, short form.
        static string[] fpushort = new string[NREG] {
          "ST0",      "ST1",      "ST2",      "ST3",
          "ST4",      "ST5",      "ST6",      "ST7" };

        // Names of MMX/3DNow! registers.
        static string[] mmxname = new string[NREG] {
          "MM0",      "MM1",      "MM2",      "MM3",
          "MM4",      "MM5",      "MM6",      "MM7" };

        // Names of 128-bit SSE registers.
        static string[] sse128 = new string[NREG] {
          "XMM0",     "XMM1",     "XMM2",     "XMM3",
          "XMM4",     "XMM5",     "XMM6",     "XMM7" };

        // Names of 256-bit SSE registers.
        static string[] sse256 = new string[NREG] {
          "YMM0",     "YMM1",     "YMM2",     "YMM3",
          "YMM4",     "YMM5",     "YMM6",     "YMM7" };

        // Names of control registers.
        static string[] crname = new string[NREG] {
          "CR0",      "CR1",      "CR2",      "CR3",
          "CR4",      "CR5",      "CR6",      "CR7" };

        // Names of debug registers.
        static string[] drname = new string[NREG] {
          "DR0",      "DR1",      "DR2",      "DR3",
          "DR4",      "DR5",      "DR6",      "DR7" };

        // データ型の宣言
        // sseサイズモードによっては、16バイトデータ型（DQWORD）の名前をXMMWORDに、
        // 32ビット型（QQWORD）の名前をYMMWORDに変更することができます。
        static string[] sizename = new string[33] {
          null,          "BYTE",     "WORD",     null,
          "DWORD",    null,          "FWORD",    null,
          "QWORD",    null,          "TBYTE",    null,
          null,          null,          null,          null,
          "DQWORD",   null,          null,          null,
          null,          null,          null,          null,
          null,          null,          null,          null,
          null,          null,          null,          null,
          "QQWORD"};

        // Keywords for immediate data. HLA uses sizename[] instead of sizekey[].
        static string[] sizekey = new string[33] {
          null,          "DB",       "DW",       null,
          "DD",       null,          "DF",       null,
          "DQ",       null,          "DT",       null,
          null,          null,          null,          null,
          "DDQ",      null,          null,          null,
          null,          null,          null,          null,
          null,          null,          null,          null,
          null,          null,          null,          null,
          "DQQ" };

        // Keywords for immediate data in AT&T format.
        static string[] sizeatt = new string[33] {
          null,          ".BYTE",    ".WORD",    null,
          ".LONG",    null,          ".FWORD",   null,
          ".QUAD",    null,          ".TBYTE",   null,
          null,          null,          null,          null,
          ".DQUAD",   null,          null,          null,
          null,          null,          null,          null,
          null,          null,          null,          null,
          null,          null,          null,          null,
          ".QQUAD" };

        // Comparison predicates in SSE [0..7] and VEX commands [0..31].
        static string[] ssepredicate = new string[32] {
          "EQ",       "LT",       "LE",       "UNORD",
          "NEQ",      "NLT",      "NLE",      "ORD",
          "EQ_UQ",    "NGE",      "NGT",      "FALSE",
          "NEQ_OQ",   "GE",       "GT",       "TRUE",
          "EQ_OS",    "LT_OQ",    "LE_OQ",    "UNORD_S",
          "NEQ_US",   "NLT_UQ",   "NLE_UQ",   "ORD_S",
          "EQ_US",    "NGE_UQ",   "NGT_UQ",   "FALSE_OS",
          "NEQ_OS",   "GE_OQ",    "GT_OQ",    "TRUE_US" };


        ////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////// DISASSEMBLER /////////////////////////////////

        // Intermediate disassembler data
        struct t_imdata
        {
            public t_disasm da;              // disassemblyの結果
            public DA damode;             // DA_xxxの逆アセンブル・モード
            public t_config config;         // 逆アセンブラ構成
            public PF prefixlist;         // コマンドのプレフィックスリストPF_xxx
            public uint ssesize;              // SSEオペランドのサイズ（16/32バイト）
            public uint immsize1;           // 最初の即値定数のサイズ
            public uint immsize2;           // 2番目の即値定数のサイズ
            public uint mainsize;           // プレフィックス付きコマンドのサイズ
            public uint modsize;            // ModRegRM / SIBバイトのサイズ
            public uint dispsize;           // アドレスオフセットのサイズ
            public int usesdatasize;         // データサイズ接頭辞を持つことができる
            public int usesaddrsize;         // アドレスサイズ接頭辞を持つことができる
            public int usessegment;          // セグメントオーバーライドプレフィックス
        }

        // Default disassembler configuration
        static t_config defconfig = new t_config {
            disasmmode = DAMODE.MASM,           // メインスタイル、DAMODE_xxxの1つ
            memmode = NUM.STD | NUM.DECIMAL,   // アドレスの定数部分、NUM_xxx
            jmpmode = NUM.STD | NUM.LONG,      // ジャンプ先/通話先、NUM_xxx
            binconstmode = NUM.STD | NUM.LONG,      // 2進定数、NUM_xxx
            constmode = NUM.STD | NUM.DECIMAL,   // 数値定数、NUM_xxx
            lowercase = false,                     // 小文字の表示を強制する
            tabarguments = false,                     // ニーモニックと引数のタブ
            extraspace = false,                     // 引数間の余分なスペース
            useretform = false,                     // RETNの代わりにRETを使用する
            shortstringcmds = true,                     // 短い形式の文字列コマンドを使用する
            putdefseg = true,                     // リスティングのデフォルトセグメントを表示する
            showmemsize = true,                     // 常にメモリサイズを表示する
            shownear = false,                     // NEAR修飾子を表示する
            ssesizemode = true,                     // SSEオペランドのサイズをデコードする方法
            jumphintmode = false,                     // ジャンプヒントをデコードする方法
            sizesens = 0,                     // サイズに敏感なニーモニックをデコードする方法
            simplifiedst = false,                     // FPUスタックのトップをデコードする方法
            hiliteoperands = false                      // オペランドを強調表示する
        };

        // AT&T disassembler configuration
        static t_config attconfig = new t_config {
            disasmmode = DAMODE.ATT,         // メインスタイル、DAMODE_xxxの1つ
            memmode = NUM.X | NUM.DECIMAL,  // アドレスの定数部分、NUM_xxx
            jmpmode = NUM.X | NUM.LONG,     // ジャンプ先/通話先、NUM_xxx
            binconstmode = NUM.X | NUM.LONG,     // 2進定数、NUM_xxx
            constmode = NUM.X | NUM.DECIMAL,  // 数値定数、NUM_xxx
            lowercase = true,                  // 小文字の表示を強制する
            tabarguments = true,                  // ニーモニックと引数のタブ
            extraspace = true,                  // 引数間の余分なスペース
            useretform = false,                  // RETNの代わりにRETを使用する
            shortstringcmds = true,                  // 短い形式の文字列コマンドを使用する
            putdefseg = false,                  // リスティングのデフォルトセグメントを表示する
            showmemsize = false,                  // 常にメモリサイズを表示する
            shownear = false,                  // NEAR修飾子を表示する
            ssesizemode = true,                  // SSEオペランドのサイズをデコードする方法
            jumphintmode = false,                  // ジャンプヒントをデコードする方法
            sizesens = 0,                  // サイズに敏感なニーモニックをデコードする方法
            simplifiedst = false,                  // FPUスタックのトップをデコードする方法
            hiliteoperands = false                   // オペランドを強調表示する
        };


        ////////////////////////////////////////////////////////////////////////////////
        ////////////////////////////// SERVICE FUNCTIONS ///////////////////////////////

        static char[] hexcharu = new char[16]{          // Nibble-to-hexdigit table, uppercase
          '0', '1', '2', '3', '4', '5', '6', '7',
          '8', '9', 'A', 'B', 'C', 'D', 'E', 'F' };

        static char[] hexcharl = new char[16]{          // Nibble-to-hexdigit table, lowercase
          '0', '1', '2', '3', '4', '5', '6', '7',
          '8', '9', 'a', 'b', 'c', 'd', 'e', 'f' };

        //static string cvtlower = "";

        // Copies at most n-1 wide characters from src to dest and assures that dest is
        // null-terminated. Slow but reliable. Returns number of copied characters, not
        // including the terminal null. Attention, does not check that input parameters
        // are correct!
        static int Tstrcopy(char* dest, int n, string src)
        {
            int i = 0;
            if (n <= 0)
                return 0;
            foreach(var c in src)
            {
                i++;
                *dest++ = c;
            };
            *dest = '\0';
            return i;
        }
        
        // Copies at most n-1 wide characters from src to dest and assures that dest is
        // null-terminated. If lowercase is 1, simultaneously converts it to lower
        // case. Slow but reliable. Returns number of copied characters, not including
        // the terminal null. Attention, does not check that input parameters are
        // correct!
        static string Tcopycase(char* src, bool lowercase)
        {
            string result = string.Concat(Enumerable.Range(0, TEXTLEN - 1).Select(t => src[t]).TakeWhile(t => t != '\0'));
            return lowercase ? result.ToLower() : result;
        }
        /*
        // Dumps ncode bytes of code to the string s. Returns length of resulting text,
        // characters, not including terminal zero. Attention, does not check that
        // input parameters are correct or that s has sufficient length!
        static int Thexdump(char* s, char* code, int ncode, bool lowercase)
        {
            int d, n;
            char* hexchar;
            hexchar = (lowercase ? hexcharl : hexcharu);
            n = 0;
            while (ncode > 0)
            {
                d = *code++;
                s[n++] = hexchar[(d >> 4) & 0x0F];
                s[n++] = hexchar[d & 0x0F];
                ncode--;
            }
            s[n] = '\0';
            return n;
        }
*/
        // Converts unsigned 1-, 2- or 4-byte number to hexadecimal text, according to
        // the specified mode and type of argument. String s must be at least SHORTNAME
        // characters long. Returns length of resulting text in characters, not
        // including the terminal zero.
        static string Hexprint(int size, int u, t_imdata im, B arg)
        {
            string buf = "";
            if (size == 1)
                u &= 0x000000FF;                     // 8-bit number
            else if (size == 2)
                u &= 0x0000FFFF;                     // 16-bit number
            else
                size = 4;                            // Correct possible errors
            B mod = arg & B.MODMASK;
            NUM nummode;
            if (mod == B.ADDR)
                nummode = im.config.memmode;
            else if (mod == B.JMPCALL || mod == B.JMPCALLFAR)
                nummode = im.config.jmpmode;
            else if (mod == B.BINARY)
                nummode = im.config.binconstmode;
            else
                nummode = im.config.constmode;
            if ((nummode & NUM.DECIMAL) != 0 && (mod == B.SIGNED || mod == B.UNSIGNED ||
              (u < DECLIMIT && mod != B.BINARY && mod != B.JMPCALL && mod != B.JMPCALLFAR))
            )
            {
                // Period marks decimals in OllyDbg
                buf = $"{u:X}{((nummode & NUM.STYLE) == NUM.OLLY && u >= 10 ? "." : "")}";
            }
            else
            {
                buf = $"{((nummode & NUM.STYLE) == NUM.X ? "0x" : "")}{u:X}{((nummode & NUM.STYLE) == NUM.STD ? "h" : "")}";
            }
            return buf;
        }

        void tstrcpy(char* src, string dst)
        {
            for (int i = 0; i < dst.Length; i++)
            {
                src[i] = dst[i];
            }
        }

        ////////////////////////////////////////////////////////////////////////////////
        ///////////////////////// INTERNAL DISASSEMBLER TABLES /////////////////////////

        static Dictionary<int, List<t_bincmd>> cmdchain = null;  // 最初のCMDMASKビットでソートされたコマンド
        static t_modrm[] modrm16;   // 16ビットModRMデコード
        static t_modrm[] modrm32;   // SIBなしの32ビットModRMデコード
        static t_modrm[] sib0;      // Mod = 00のModRM-SIBデコード
        static t_modrm[] sib1;      // Mod = 01のModRM-SIBデコード
        static t_modrm[] sib2;      // Mod = 10のModRM-SIBデコード

        static bool decodeaddress(out string label, int addr)
        {
            label = "";
            return false;
        }

        // 逆アセンブラテーブルを初期化します。
        // 起動時にこの関数を1回呼び出します。
        // 成功した場合は0を返し、初期化が失敗した場合は-1を返します。
        // 最後のケースでは、継続は不可能であり、プログラムは終了する必要があります。
        protected Disassembler()
        {
            int c, scale;

            // 最初のCMDMASKビットでコマンド・ディスクリプタをコマンド・チェーンにソートします。
            cmdchain = new Dictionary<int, List<t_bincmd>>();

            foreach (var pcmd in bincmd)
            {
                if ((pcmd.cmdtype & D.CMDTYPE) == D.PSEUDO)
                    continue;                           // Pseudocommand, 検索モデルのみ
                uint code = pcmd.code;
                uint mask = pcmd.mask & CMDMASK;
                for (int u = 0; u < CMDMASK + 1; u++)
                {
                    if (((u ^ code) & mask) != 0)
                        continue;                      // コマンドの最初のバイトが異なる
                    if (!cmdchain.ContainsKey(u)) cmdchain[u] = new List<t_bincmd>();
                    cmdchain[u].Add(pcmd);
                }
            }
            // Prepare for SIB
            modrm16 = new t_modrm[256];
            modrm32 = new t_modrm[256];
            sib0 = new t_modrm[256];
            sib1 = new t_modrm[256];
            sib2 = new t_modrm[256];

            // Prepare 16-bit ModRM decodings.
            for (c = 0x00; c <= 0xFF; c++)
            {
                modrm16[c] = new t_modrm();
                REG reg = (REG)(c & 0x07);
                if ((c & 0xC0) == 0xC0)
                {
                    // Register in ModRM.
                    modrm16[c].size = 1;
                    modrm16[c].features = 0;                // Register, its type as yet unknown
                    modrm16[c].reg = reg;
                    modrm16[c].defseg = SEG.UNDEF;
                    modrm16[c].basereg = REG.UNDEF;
                }
                else if ((c & 0xC7) == 0x06)
                {
                    // Special case of immediate address.
                    modrm16[c].size = 3;
                    modrm16[c].dispsize = 2;
                    modrm16[c].features = OP.MEMORY | OP.OPCONST | OP.ADDR16;
                    modrm16[c].reg = REG.UNDEF;
                    modrm16[c].defseg = SEG.DS;
                    modrm16[c].basereg = REG.UNDEF;
                }
                else
                {
                    modrm16[c].features = OP.MEMORY | OP.INDEXED | OP.ADDR16;
                    if ((c & 0xC0) == 0x40)
                    {
                        modrm16[c].dispsize = 1; modrm16[c].features |= OP.OPCONST;
                    }
                    else if ((c & 0xC0) == 0x80)
                    {
                        modrm16[c].dispsize = 2; modrm16[c].features |= OP.OPCONST;
                    }
                    modrm16[c].size = modrm16[c].dispsize + 1;
                    modrm16[c].reg = REG.UNDEF;
                    switch ((int)reg)
                    {
                        case 0:
                            modrm16[c].scale[(int)REG.EBX] = 1; modrm16[c].scale[(int)REG.ESI] = 1;
                            modrm16[c].defseg = SEG.DS;
                            modrm16[c].ardec = "BX+SI";
                            modrm16[c].aratt = "%BX,%SI";
                            modrm16[c].aregs = (1 << (int)REG.EBX) | (1 << (int)REG.ESI);
                            modrm16[c].basereg = REG.ESI; break;
                        case 1:
                            modrm16[c].scale[(int)REG.EBX] = 1; modrm16[c].scale[(int)REG.EDI] = 1;
                            modrm16[c].defseg = SEG.DS;
                            modrm16[c].ardec = "BX+DI";
                            modrm16[c].aratt = "%BX,%DI";
                            modrm16[c].aregs = (1 << (int)REG.EBX) | (1 << (int)REG.EDI);
                            modrm16[c].basereg = REG.EDI; break;
                        case 2:
                            modrm16[c].scale[(int)REG.EBP] = 1; modrm16[c].scale[(int)REG.ESI] = 1;
                            modrm16[c].defseg = SEG.SS;
                            modrm16[c].ardec = "BP+SI";
                            modrm16[c].aratt = "%BP,%SI";
                            modrm16[c].aregs = (1 << (int)REG.EBP) | (1 << (int)REG.ESI);
                            modrm16[c].basereg = REG.ESI; break;
                        case 3:
                            modrm16[c].scale[(int)REG.EBP] = 1; modrm16[c].scale[(int)REG.EDI] = 1;
                            modrm16[c].defseg = SEG.SS;
                            modrm16[c].ardec = "BP+DI";
                            modrm16[c].aratt = "%BP,%DI";
                            modrm16[c].aregs = (1 << (int)REG.EBP) | (1 << (int)REG.EDI);
                            modrm16[c].basereg = REG.EDI; break;
                        case 4:
                            modrm16[c].scale[(int)REG.ESI] = 1;
                            modrm16[c].defseg = SEG.DS;
                            modrm16[c].ardec = "SI";
                            modrm16[c].aratt = "%SI";
                            modrm16[c].aregs = (1 << (int)REG.ESI);
                            modrm16[c].basereg = REG.ESI; break;
                        case 5:
                            modrm16[c].scale[(int)REG.EDI] = 1;
                            modrm16[c].defseg = SEG.DS;
                            modrm16[c].ardec = "DI";
                            modrm16[c].aratt = "%DI";
                            modrm16[c].aregs = (1 << (int)REG.EDI);
                            modrm16[c].basereg = REG.EDI; break;
                        case 6:
                            modrm16[c].scale[(int)REG.EBP] = 1;
                            modrm16[c].defseg = SEG.SS;
                            modrm16[c].ardec = "BP";
                            modrm16[c].aratt = "%BP";
                            modrm16[c].aregs = (1 << (int)REG.EBP);
                            modrm16[c].basereg = REG.EBP; break;
                        case 7:
                            modrm16[c].scale[(int)REG.EBX] = 1;
                            modrm16[c].defseg = SEG.DS;
                            modrm16[c].ardec = "BX";
                            modrm16[c].aratt = "%BX";
                            modrm16[c].aregs = (1 << (int)REG.EBX);
                            modrm16[c].basereg = REG.EBX;
                            break;
                    }
                }
            }
            // Prepare 32-bit ModRM decodings without SIB.
            for (c = 0x00; c <= 0xFF; c++)
            {
                modrm32[c] = new t_modrm();
                REG reg = (REG)(c & 0x07);
                if ((c & 0xC0) == 0xC0)
                {
                    // Register in ModRM.
                    modrm32[c].size = 1;
                    modrm32[c].features = 0;                // Register, its type as yet unknown
                    modrm32[c].reg = reg;
                    modrm32[c].defseg = SEG.UNDEF;
                    modrm32[c].basereg = REG.UNDEF;
                }
                else if ((c & 0xC7) == 0x05)
                {
                    // Special case of 32-bit immediate address.
                    modrm32[c].size = 5;
                    modrm32[c].dispsize = 4;
                    modrm32[c].features = OP.MEMORY | OP.OPCONST;
                    modrm32[c].reg = REG.UNDEF;
                    modrm32[c].defseg = SEG.DS;
                    modrm32[c].basereg = REG.UNDEF;
                }
                else
                {
                    // Regular memory address.
                    modrm32[c].features = OP.MEMORY;
                    modrm32[c].reg = REG.UNDEF;
                    if ((c & 0xC0) == 0x40)
                    {
                        modrm32[c].dispsize = 1;              // 8-bit sign-extended displacement
                        modrm32[c].features |= OP.OPCONST;
                    }
                    else if ((c & 0xC0) == 0x80)
                    {
                        modrm32[c].dispsize = 4;              // 32-bit displacement
                        modrm32[c].features |= OP.OPCONST;
                    }
                    if (reg == REG.ESP)
                    {
                        // SIB byte follows, decode with sib32.
                        if ((c & 0xC0) == 0x00) modrm32[c].psib = sib0;
                        else if ((c & 0xC0) == 0x40) modrm32[c].psib = sib1;
                        else modrm32[c].psib = sib2;
                        modrm32[c].basereg = REG.UNDEF;
                    }
                    else
                    {
                        modrm32[c].size = 1 + modrm32[c].dispsize;
                        modrm32[c].features |= OP.INDEXED;
                        modrm32[c].defseg = (reg == REG.EBP ? SEG.SS : SEG.DS);
                        modrm32[c].scale[(int)reg] = 1;
                        modrm32[c].ardec = regname32[(int)reg];
                        modrm32[c].aratt = $"%{regname32[(int)reg]}";
                        modrm32[c].aregs = (uint)(1 << (int)reg);
                        modrm32[c].basereg = reg;
                    }
                }
            }
            // Prepare 32-bit ModRM decodings with SIB, case Mod=00: usually no disp.
            for (c = 0x00; c <= 0xFF; c++)
            {
                sib0[c] = new t_modrm();
                sib0[c].features = OP.MEMORY;
                sib0[c].reg = REG.UNDEF;
                int reg = c & 0x07, sreg = (c >> 3) & 0x07;
                if ((c & 0xC0) == 0) scale = 1;
                else if ((c & 0xC0) == 0x40) scale = 2;
                else if ((c & 0xC0) == 0x80) scale = 4;
                else scale = 8;
                if ((REG)sreg != REG.ESP)
                {
                    sib0[c].scale[sreg] = (byte)scale;
                    sib0[c].ardec = regname32[sreg];
                    sib0[c].aregs = (uint)(1 << sreg);
                    sib0[c].features |= OP.INDEXED;
                    if (scale > 1)
                    {
                        sib0[c].ardec += $"*{scale}";
                    }
                }
                if ((REG)reg == REG.EBP)
                {
                    sib0[c].size = 6;
                    sib0[c].dispsize = 4;
                    sib0[c].features |= OP.OPCONST;
                    sib0[c].defseg = SEG.DS;
                    sib0[c].basereg = REG.UNDEF;
                }
                else
                {
                    sib0[c].size = 2;
                    sib0[c].defseg = (((REG)reg == REG.ESP || (REG)reg == REG.EBP) ? SEG.SS : SEG.DS);
                    sib0[c].scale[reg]++;
                    sib0[c].features |= OP.INDEXED;
                    if (sib0[c].ardec != "") sib0[c].ardec += "+";
                    sib0[c].ardec += regname32[reg];
                    sib0[c].aregs |= (uint)(1 << reg);
                    sib0[c].basereg = (REG)reg;
                }
                if ((REG)reg != REG.EBP)
                {
                    sib0[c].aratt = $"%{regname32[reg]}"; 
                }
                if ((REG)sreg != REG.ESP)
                {
                    sib0[c].aratt += $",%{regname32[sreg]}";
                    if (scale > 1)
                    {
                        sib0[c].aratt += $",{scale}";
                    }
                }
            }
            // Prepare 32-bit ModRM decodings with SIB, case Mod=01: 8-bit displacement.
            for (c = 0x00; c <= 0xFF; c++)
            {
                sib1[c] = new t_modrm();
                sib1[c].features = OP.MEMORY | OP.INDEXED | OP.OPCONST;
                sib1[c].reg = REG.UNDEF;
                int reg = c & 0x07, sreg = (c >> 3) & 0x07;
                if ((c & 0xC0) == 0) scale = 1;
                else if ((c & 0xC0) == 0x40) scale = 2;
                else if ((c & 0xC0) == 0x80) scale = 4;
                else scale = 8;
                sib1[c].size = 3;
                sib1[c].dispsize = 1;
                sib1[c].defseg = (((REG)reg == REG.ESP || (REG)reg == REG.EBP) ? SEG.SS : SEG.DS);
                sib1[c].scale[reg] = 1;
                sib1[c].basereg = (REG)reg;
                sib1[c].aregs = (uint)(1 << reg);
                if ((REG)sreg != REG.ESP)
                {
                    sib1[c].scale[sreg] += (byte)scale;
                    sib1[c].ardec += regname32[sreg];
                    sib1[c].aregs |= (uint)(1 << sreg);
                    if (scale > 1)
                    {
                        sib1[c].ardec += $"*{scale}";
                    }
                }
                if (sib1[c].ardec != "") sib1[c].ardec += '+';
                sib1[c].ardec+= regname32[reg];
                sib1[c].aratt = $"%{regname32[reg]}";
                if ((REG)sreg != REG.ESP)
                {
                    sib1[c].aratt += $",%{regname32[sreg]}";
                    if (scale > 1)
                    {
                        sib1[c].aratt += $",{scale}";
                    }
                }
            }
            // Prepare 32-bit ModRM decodings with SIB, case Mod=10: 32-bit displacement.
            for (c = 0x00; c <= 0xFF; c++)
            {
                sib2[c] = new t_modrm();
                sib2[c].features = OP.MEMORY | OP.INDEXED | OP.OPCONST;
                sib2[c].reg = REG.UNDEF;
                int reg = c & 0x07, sreg = (c >> 3) & 0x07;
                if ((c & 0xC0) == 0) scale = 1;
                else if ((c & 0xC0) == 0x40) scale = 2;
                else if ((c & 0xC0) == 0x80) scale = 4;
                else scale = 8;
                sib2[c].size = 6;
                sib2[c].dispsize = 4;
                sib2[c].defseg = (((REG)reg == REG.ESP || (REG)reg == REG.EBP) ? SEG.SS : SEG.DS);
                sib2[c].scale[reg] = (byte)1;
                sib2[c].basereg = (REG)reg;
                sib2[c].aregs = (uint)(1 << reg);
                if ((REG)sreg != REG.ESP)
                {
                    sib2[c].scale[sreg] += (byte)scale;
                    sib2[c].ardec+= regname32[sreg];
                    sib2[c].aregs |= (uint)(1 << sreg);
                    if (scale > 1)
                    {
                        sib2[c].ardec += $"*{scale}";
                    }
                }
                if (sib2[c].ardec != "") sib2[c].ardec += '+';
                sib2[c].ardec += regname32[reg];
                sib2[c].aratt = $"%{regname32[reg]}";
                if ((REG)sreg != REG.ESP)
                {
                    sib2[c].aratt += $",%{regname32[sreg]}";
                    if (scale > 1)
                    {
                        sib2[c].aratt += $",{scale}";
                    }
                }
            }
            // Fill lowercase conversion table. This table replaces tolower(). When
            // compiled with Borland C++ Builder, spares significant time.
            //for (c = 0; c < 256; c++)  cvtlower[c] = (tchar)ttolower(c);
            // Report success.
        }


        ////////////////////////////////////////////////////////////////////////////////
        ////////////////////////////// AUXILIARY ROUTINES //////////////////////////////

        // Given index of byte register, returns index of 32-bit container.
        static REG Byteregtodwordreg(int bytereg)
        {
            if (bytereg < 0 || bytereg >= NREG)
                return REG.UNDEF;
            if (bytereg >= 4)
                return (REG)(bytereg - 4);
            return (REG)bytereg;
        }

        // Checks prefix override flags and generates warnings if prefix is superfluous.
        // Returns index of segment register. Note that Disasm() assures that two
        // segment override bits in im->prefixlist can't be set simultaneously.
        static SEG Getsegment(t_imdata im, B arg, SEG defseg)
        {
            if ((im.prefixlist & PF.SEGMASK) == 0)
                return defseg;                     // Optimization for most frequent case
            switch (im.prefixlist & PF.SEGMASK)
            {
                case PF.ES:
                    if (defseg == SEG.ES) im.da.warnings |= DAW.DEFSEG;
                    if ((arg & B.NOSEG) != 0) im.da.warnings |= DAW.SEGPREFIX;
                    return SEG.ES;
                case PF.CS:
                    if (defseg == SEG.CS) im.da.warnings |= DAW.DEFSEG;
                    if ((arg & B.NOSEG) != 0) im.da.warnings |= DAW.SEGPREFIX;
                    return SEG.CS;
                case PF.SS:
                    if (defseg == SEG.SS) im.da.warnings |= DAW.DEFSEG;
                    if ((arg & B.NOSEG) != 0) im.da.warnings |= DAW.SEGPREFIX;
                    return SEG.SS;
                case PF.DS:
                    if (defseg == SEG.DS) im.da.warnings |= DAW.DEFSEG;
                    if ((arg & B.NOSEG) != 0) im.da.warnings |= DAW.SEGPREFIX;
                    return SEG.DS;
                case PF.FS:
                    if (defseg == SEG.FS) im.da.warnings |= DAW.DEFSEG;
                    if ((arg & B.NOSEG) != 0) im.da.warnings |= DAW.SEGPREFIX;
                    return SEG.FS;
                case PF.GS:
                    if (defseg == SEG.GS) im.da.warnings |= DAW.DEFSEG;
                    if ((arg & B.NOSEG) != 0) im.da.warnings |= DAW.SEGPREFIX;
                    return SEG.GS;
                default: return defseg;            // Most frequent case of default segment
            }
        }

        // Decodes generalized memory address to text.
        static string Memaddrtotext(t_imdata im, B arg, uint datasize, SEG seg, string regpart, int constpart)
        {
            string s = "";
            if (im.config.disasmmode == DAMODE.ATT)
            {
                // AT&T memory address syntax is so different from Intel that I process it
                // separately from the rest.
                if ((arg & B.MODMASK) == B.JMPCALL)
                    s += '*';
                // On request, I show only explicit segments.
                if ((im.config.putdefseg && (arg & B.NOSEG) == 0) ||
                  (im.prefixlist & PF.SEGMASK) != 0
                )
                {
                    s += $"%{(im.config.lowercase ? segname[(int)seg].ToLower() : segname[(int)seg])}:";
                }
                // Add constant part (offset).
                if (constpart < 0 && constpart > NEGLIMIT)
                {
                    s += '-';
                    s += Hexprint(((im.prefixlist & PF.ASIZE) != 0 ? 2 : 4), -constpart, im, B.ADDR);
                }
                else if (constpart != 0)
                {
                    if (seg != SEG.FS && seg != SEG.GS && decodeaddress(out var label, constpart))
                        s += label;
                    else
                        s += Hexprint(((im.prefixlist & PF.ASIZE) != 0 ? 2 : 4), constpart, im, B.ADDR);
                    ;
                }
                // Add register part of address, may be absent.
                if (regpart != "")
                {
                    s += $"({regpart})";
                }
            }
            else
            {
                // Mark far and near jump/call addresses.
                if ((arg & B.MODMASK) == B.JMPCALLFAR)
                    s += "FAR ";
                else if (im.config.shownear && (arg & B.MODMASK) == B.JMPCALL)
                    s += "NEAR ";
                if (im.config.disasmmode != DAMODE.MASM)
                {;
                    s += "[";
                    if ((im.prefixlist & PF.ASIZE) != 0 && regpart == "")
                        s += "SMALL ";
                }
                // If operand is longer than 32 bytes or of type B.ANYMEM (memory contents
                // unimportant), its size is not displayed. Otherwise, bit B.SHOWSIZE
                // indicates that explicit operand's size can't be omitted.
                if (datasize <= 32 && (arg & B.ARGMASK) != B.ANYMEM &&
                  (im.config.showmemsize || (arg & B.SHOWSIZE) != 0)
                )
                {
                    if (im.config.disasmmode == DAMODE.HLA)
                        s += "TYPE ";
                    if ((arg & B.ARGMASK) == B.INTPAIR && im.config.disasmmode == DAMODE.IDEAL)
                    {
                        // If operand is a pair of integers (BOUND), Borland in IDEAL mode
                        // expects size of single integer, whereas MASM requires size of the
                        // whole pair.
                        s += $"{sizename[datasize / 2]} ";
                    }
                    else if (datasize == 16 && im.config.ssesizemode)
                        s += "XMMWORD ";
                    else if (datasize == 32 && im.config.ssesizemode)
                        s += "YMMWORD ";
                    else
                    {
                        s += $"{sizename[datasize]} ";
                        if (im.config.disasmmode == DAMODE.MASM)
                            s += "PTR ";
                    }
                }
                // On request, I show only explicit segments.
                if ((im.config.putdefseg && (arg & B.NOSEG) == 0) ||
                    (im.prefixlist & PF.SEGMASK) != 0) s += $"{segname[(int)seg]}:";
                if (im.config.disasmmode == DAMODE.MASM)
                {
                    s += "[";
                    if ((im.prefixlist & PF.ASIZE) != 0 && regpart == "")
                        s += "SMALL ";
                }
                // Add register part of address, may be absent.
                if (regpart != "")
                    s += regpart;
                if (regpart != "" && constpart < 0 && constpart > NEGLIMIT)
                {
                    s += $"-{Hexprint(((im.prefixlist & PF.ASIZE) != 0 ? 2 : 4), -constpart, im, B.ADDR)}";
                }
                else if (constpart != 0 || regpart == "")
                {
                    if (regpart != "") s += '+';
                    if (seg != SEG.FS && seg != SEG.GS && decodeaddress(out var label, constpart))
                        s += label;
                    else
                        s += Hexprint(((im.prefixlist & PF.ASIZE) != 0 ? 2 : 4), constpart, im, B.ADDR);
                }
                s += "]";
            }
            return s;
        }

        // Service function, returns granularity of MMX, 3DNow! and SSE operands.
        static uint Getgranularity(B arg)
        {
            uint granularity;
            switch (arg & B.ARGMASK)
            {
                case B.MREG8x8:                    // MMX register as 8 8-bit integers
                case B.MMX8x8:                     // MMX reg/memory as 8 8-bit integers
                case B.MMX8x8DI:                   // MMX 8 8-bit integers at [DS:(E)DI]
                case B.XMM0I8x16:                  // XMM0 as 16 8-bit integers
                case B.SREGI8x16:                  // SSE register as 16 8-bit sigints
                case B.SVEXI8x16:                  // SSE reg in VEX as 16 8-bit sigints
                case B.SIMMI8x16:                  // SSE reg in immediate 8-bit constant
                case B.SSEI8x16:                   // SSE reg/memory as 16 8-bit sigints
                case B.SSEI8x16DI:                 // SSE 16 8-bit sigints at [DS:(E)DI]
                case B.SSEI8x8L:                   // Low 8 8-bit ints in SSE reg/memory
                case B.SSEI8x4L:                   // Low 4 8-bit ints in SSE reg/memory
                case B.SSEI8x2L:                   // Low 2 8-bit ints in SSE reg/memory
                    granularity = 1; break;
                case B.MREG16x4:                   // MMX register as 4 16-bit integers
                case B.MMX16x4:                    // MMX reg/memory as 4 16-bit integers
                case B.SREGI16x8:                  // SSE register as 8 16-bit sigints
                case B.SVEXI16x8:                  // SSE reg in VEX as 8 16-bit sigints
                case B.SSEI16x8:                   // SSE reg/memory as 8 16-bit sigints
                case B.SSEI16x4L:                  // Low 4 16-bit ints in SSE reg/memory
                case B.SSEI16x2L:                  // Low 2 16-bit ints in SSE reg/memory
                    granularity = 2; break;
                case B.MREG32x2:                   // MMX register as 2 32-bit integers
                case B.MMX32x2:                    // MMX reg/memory as 2 32-bit integers
                case B._3DREG:                     // 3DNow! register as 2 32-bit floats
                case B._3DNOW:                     // 3DNow! reg/memory as 2 32-bit floats
                case B.SREGF32x4:                  // SSE register as 4 32-bit floats
                case B.SVEXF32x4:                  // SSE reg in VEX as 4 32-bit floats
                case B.SREGF32L:                   // Low 32-bit float in SSE register
                case B.SVEXF32L:                   // Low 32-bit float in SSE in VEX
                case B.SREGF32x2L:                 // Low 2 32-bit floats in SSE register
                case B.SSEF32x4:                   // SSE reg/memory as 4 32-bit floats
                case B.SSEF32L:                    // Low 32-bit float in SSE reg/memory
                case B.SSEF32x2L:                  // Low 2 32-bit floats in SSE reg/memory
                    granularity = 4; break;
                case B.XMM0I32x4:                  // XMM0 as 4 32-bit integers
                case B.SREGI32x4:                  // SSE register as 4 32-bit sigints
                case B.SVEXI32x4:                  // SSE reg in VEX as 4 32-bit sigints
                case B.SREGI32L:                   // Low 32-bit sigint in SSE register
                case B.SREGI32x2L:                 // Low 2 32-bit sigints in SSE register
                case B.SSEI32x4:                   // SSE reg/memory as 4 32-bit sigints
                case B.SSEI32x2L:                  // Low 2 32-bit sigints in SSE reg/memory
                    granularity = 4; break;
                case B.MREG64:                     // MMX register as 1 64-bit integer
                case B.MMX64:                      // MMX reg/memory as 1 64-bit integer
                case B.XMM0I64x2:                  // XMM0 as 2 64-bit integers
                case B.SREGF64x2:                  // SSE register as 2 64-bit floats
                case B.SVEXF64x2:                  // SSE reg in VEX as 2 64-bit floats
                case B.SREGF64L:                   // Low 64-bit float in SSE register
                case B.SVEXF64L:                   // Low 64-bit float in SSE in VEX
                case B.SSEF64x2:                   // SSE reg/memory as 2 64-bit floats
                case B.SSEF64L:                    // Low 64-bit float in SSE reg/memory
                    granularity = 8; break;
                case B.SREGI64x2:                  // SSE register as 2 64-bit sigints
                case B.SVEXI64x2:                  // SSE reg in VEX as 2 64-bit sigints
                case B.SSEI64x2:                   // SSE reg/memory as 2 64-bit sigints
                case B.SREGI64L:                   // Low 64-bit sigint in SSE register
                    granularity = 8; break;
                default:
                    granularity = 1;                   // Treat unknown ops as string of bytes
                    break;
            }
            return granularity;
        }


        ////////////////////////////////////////////////////////////////////////////////
        ////////////////////////// OPERAND DECODING ROUTINES ///////////////////////////

        // Decodes 8/16/32-bit integer register operand. ATTENTION, calling routine
        // must set usesdatasize and usesaddrsize by itself!
        static void Operandintreg(ref t_imdata im, uint datasize, REG index, ref t_operand op)
        {
            REG reg32;
            op.features = OP.REGISTER;
            op.opsize = op.granularity = datasize;
            op.reg = index;
            op.seg = SEG.UNDEF;
            // Add container register to lists of used and modified registers.
            if (datasize == 1)
                reg32 = Byteregtodwordreg((int)index);
            else
                reg32 = index;
            if ((op.arg & B.CHG) == 0)
            {
                op.uses = (uint)(1 << (int)reg32);
                im.da.uses |= (uint)(1 << (int)reg32);
            }
            if ((op.arg & (B.CHG | B.UPD)) != 0)
            {
                op.modifies = (uint)(1 << (int)reg32);
                im.da.modifies |= (uint)(1 << (int)reg32);
            }
            // Warn if ESP is misused.
            if ((op.arg & B.NOESP) != 0 && reg32 == REG.ESP)
                im.da.warnings |= DAW.NOESP;
            // Decode name of integer register.
            if ((im.damode & DA.TEXT)!=0)
            {
                if (im.config.disasmmode == DAMODE.ATT)
                {
                    if ((op.arg & B.MODMASK) == B.JMPCALL) op.text += '*';
                    op.text += '%';
                }
                if (datasize == 4)// Most frequent case first
                    op.text += regname32[(int)index];
                else if (datasize == 1)
                    op.text += regname8[(int)index];
                else
                    op.text += regname16[(int)index];// 16-bit registers are seldom
            }
        }
        static void Operandintreg(ref t_imdata im, uint datasize, int index, ref t_operand op)
            => Operandintreg(ref im, datasize, (REG)index, ref op);

        // Decodes 16/32-bit memory address in ModRM/SIB bytes. Returns full length of
        // address (ModRM+SIB+displacement) in bytes, 0 if ModRM indicates register
        // operand and -1 on error. ATTENTION, calling routine must set usesdatasize,
        // granularity (preset to datasize) and reg together with OP_MODREG by itself!
        static int Operandmodrm(ref t_imdata im, uint datasize, byte* cmd, uint cmdsize, ref t_operand op)
        {
            t_modrm pmrm;
            if (cmdsize == 0)
            {
                im.da.errors |= DAE.CROSS;         // Command crosses end of memory block
                return -1;
            }
            // Decode ModRM/SIB. Most of the work is already done in Preparedisasm(), we
            // only need to find corresponding t_modrm.
            if ((im.prefixlist & PF.ASIZE) != 0)
            {
                pmrm = modrm16[cmd[0]];               // 16-bit address
                im.modsize = 1;
            }
            else
            {
                pmrm = modrm32[cmd[0]];
                if (pmrm.psib == null)
                    im.modsize = 1;                   // No SIB byte
                else
                {
                    if (cmdsize < 2)
                    {
                        im.da.errors |= DAE.CROSS;     // Command crosses end of memory block
                        return -1;
                    }
                    pmrm = pmrm.psib[cmd[1]];
                    im.modsize = 2;                   // Both ModRM and SIB
                }
            }
            // Check whether ModRM indicates register operand and immediately return if
            // true. As a side effect, modsize is already set.
            if ((cmd[0] & 0xC0) == 0xC0)
                return 0;
            // Operand in memory.
            op.opsize = datasize;
            op.granularity = datasize;            // Default, may be overriden later
            op.reg = REG.UNDEF;
            im.usesaddrsize = 1;                  // Address size prefix is meaningful
            im.usessegment = 1;                   // Segment override prefix is meaningful

            // Fetch precalculated t_modrm fields.
            op.features = pmrm.features;
            for(int i = 0; i < 8; i++)
            {
                op.scale[i] = pmrm.scale[i];
            }
            op.aregs = pmrm.aregs;
            im.da.uses |= pmrm.aregs;           // Mark registers used to form address
                                                 // Get displacement, if any.
            im.dispsize = pmrm.dispsize;
            if (pmrm.dispsize != 0)
            {
                if (cmdsize < pmrm.size)
                {
                    im.da.errors |= DAE.CROSS;       // Command crosses end of memory block
                    return -1;
                }
                if (pmrm.dispsize == 1)             // 8-bit displacement is sign-extended
                    op.opconst = im.da.memconst = cmd[im.modsize];
                else if (pmrm.dispsize == 4)
                {   // 32-bit full displacement
                    im.da.memfixup = (int)(im.mainsize + im.modsize);      // Possible 32-bit fixup
                    op.opconst = im.da.memconst = *(uint*)(cmd + im.modsize);
                }
                else                               // 16-bit displacement, very rare
                    op.opconst = im.da.memconst = *(ushort*)(cmd + im.modsize);
            }
            // Get segment.
            op.seg = Getsegment(im, op.arg, pmrm.defseg);
            // Warn if memory contents is 16-bit jump/call destination.
            if (datasize == 2 && (op.arg & B.MODMASK) == B.JMPCALL)
                im.da.warnings |= DAW.JMP16;
            // Decode memory operand to text, if requested.
            if ((im.damode & DA.TEXT) != 0)
            {
                string ardec = (im.config.disasmmode == DAMODE.ATT ? pmrm.aratt : pmrm.ardec);
                op.text += Memaddrtotext(im, op.arg, datasize, op.seg, ardec, (int)op.opconst);
            }
            return (int)pmrm.size;
        }

        // Decodes 16/32-bit immediate address (used only for 8/16/32-bit memory-
        // accumulator moves). ATTENTION, calling routine must set usesdatasize by
        // itself!
        static void Operandimmaddr(ref t_imdata im, uint datasize, byte* cmd, uint cmdsize, ref t_operand op)
        {
            if ((im.prefixlist & PF.ASIZE) != 0)
                im.dispsize = 2;
            else
                im.dispsize = 4;
            if (cmdsize < im.dispsize)
            {
                im.da.errors |= DAE.CROSS;         // Command crosses end of memory block
                return;
            }
            op.features = OP.MEMORY | OP.OPCONST;
            op.opsize = op.granularity = datasize;
            op.reg = REG.UNDEF;
            im.usesaddrsize = 1;                  // Address size prefix is meaningful
            im.usessegment = 1;                   // Segment override prefix is meaningful
            if (im.dispsize == 4)
            {               // 32-bit immediate address
                            // 32-bit address means possible fixup, calculate offset.
                im.da.memfixup = (int)im.mainsize;
                op.opconst = im.da.memconst = *(uint*)cmd;
            }
            else
            {                               // 16-bit immediate address, very rare
                op.opconst = im.da.memconst = *(ushort*)cmd;
                op.features |= OP.ADDR16;
            }
            // Get segment.
            op.seg = Getsegment(im, op.arg, SEG.DS);
            // Decode memory operand to text, if requested.
            if ((im.damode & DA.TEXT) != 0)
                op.text += Memaddrtotext(im, op.arg, datasize, op.seg, "", (int)op.opconst);
        }

        // Decodes simple register address ([reg16] or [reg32]). Flag changesreg must
        // be 0 if register remains unchanged and 1 if it changes. If fixseg is set to
        // SEG_UNDEF, assumes overridable DS:, otherwise assumes fixsegment that cannot
        // be overriden with segment prefix. If fixaddrsize is 2 or 4, assumes 16- or
        // 32-bit addressing only, otherwise uses default. ATTENTION, calling routine
        // must set usesdatasize by itself!
        static void Operandindirect(ref t_imdata im, int index, bool changesreg, SEG fixseg,
          int fixaddrsize, uint datasize, ref t_operand op)
        {
            op.features = OP.MEMORY | OP.INDEXED;
            if (changesreg)
            {
                op.features |= OP.MODREG;
                op.reg = (REG)index;
                im.da.modifies |= (1u << index);
            }
            else
                op.reg = REG.UNDEF;
            if (fixaddrsize == 2)
                op.features |= OP.ADDR16;
            else if (fixaddrsize == 0)
            {
                im.usesaddrsize = 1;                // Address size prefix is meaningful
                if ((im.prefixlist & PF.ASIZE) != 0)
                {
                    op.features |= OP.ADDR16;
                    fixaddrsize = 2;
                }
            }
            // Get segment.
            if (fixseg == SEG.UNDEF)
            {
                op.seg = Getsegment(im, op.arg, SEG.DS);
                im.usessegment = 1;
            }               // Segment override prefix is meaningful
            else
                op.seg = fixseg;
            op.opsize = datasize;
            op.granularity = datasize;            // Default, may be overriden later
            op.scale[index] = 1;
            op.aregs = (1u << index);
            im.da.uses |= (1u << index);
            // Warn if memory contents is 16-bit jump/call destination.
            if (datasize == 2 && (op.arg & B.MODMASK) == B.JMPCALL)
                im.da.warnings |= DAW.JMP16;
            // Decode source operand to text, if requested.
            if ((im.damode & DA.TEXT) != 0)
            {
                string ardec = "";
                if (im.config.disasmmode == DAMODE.ATT) ardec += '%';

                if (fixaddrsize == 2)
                    ardec += regname16[index];
                else
                    ardec += regname32[index];
                if (fixseg == SEG.UNDEF)
                    op.text += Memaddrtotext(im, op.arg, datasize, op.seg, ardec, 0);
                else
                {
                    PF originallist = im.prefixlist;
                    im.prefixlist &= ~PF.SEGMASK;
                    op.text += Memaddrtotext(im, op.arg, datasize, op.seg, ardec, 0);
                    im.prefixlist = originallist;
                }
            }
        }

        static void Operandindirect(ref t_imdata im, REG index, bool changesreg, SEG fixseg, int fixaddrsize, uint datasize, ref t_operand op)
            => Operandindirect(ref im, (int)index, changesreg, fixseg, fixaddrsize, datasize, ref op);

        // Decodes XLAT source address ([(E)BX+AL]). Note that I set scale of EAX to 1,
        // which is not exactly true. ATTENTION, calling routine must set usesdatasize
        // by itself!
        static void Operandxlat(ref t_imdata im, ref t_operand op)
        {
            op.features = OP.MEMORY | OP.INDEXED;
            if ((im.prefixlist & PF.ASIZE) != 0)
                op.features |= OP.ADDR16;
            im.usesaddrsize = 1;                  // Address size prefix is meaningful
            im.usessegment = 1;                   // Segment override prefix is meaningful
            op.opsize = 1;
            op.granularity = 1;
            op.reg = REG.UNDEF;
            // Get segment.
            op.seg = Getsegment(im, op.arg, SEG.DS);
            op.scale[(int)REG.EAX] = 1;                // This is not correct!
            op.scale[(int)REG.EBX] = 1;
            op.aregs = (1 << (int)REG.EAX) | (1 << (int)REG.EBX);
            im.da.uses |= op.aregs;
            // Decode address to text, if requested.
            if ((im.damode & DA.TEXT) != 0)
            {
                string ardec;
                if (im.config.disasmmode == DAMODE.ATT)
                    ardec = ((im.prefixlist & PF.ASIZE) != 0 ? "%BX,%AL" : "%EBX,%AL");
                else
                    ardec = ((im.prefixlist & PF.ASIZE) != 0 ? "BX+AL" : "EBX+AL");
                op.text = Memaddrtotext(im, op.arg, 1, op.seg, ardec, 0);
            }
        }

        // Decodes stack pushes of any size, including implicit return address in
        // CALLs. ATTENTION, calling routine must set usesdatasize by itself!
        static void Operandpush(ref t_imdata im, uint datasize, ref t_operand op)
        {
            int addrsize;
            op.features = OP.MEMORY | OP.INDEXED | OP.MODREG;
            op.reg = REG.ESP;
            op.aregs = (1 << (int)REG.ESP);
            im.da.modifies |= op.aregs;
            im.usesaddrsize = 1;                  // Address size prefix is meaningful
            if ((im.prefixlist & PF.ASIZE) != 0)
            {
                op.features |= OP.ADDR16;
                addrsize = 2;
            }
            else
                addrsize = 4;                        // Flat model!
            op.seg = SEG.SS;
            if ((op.arg & B.ARGMASK) == B.PUSHA)
            {
                im.da.uses = 0xFF;                 // Uses all general registers
                op.opsize = datasize * 8;
            }
            else if ((op.arg & B.ARGMASK) == B.PUSHRETF)
            {
                im.da.uses |= op.aregs;
                op.opsize = datasize * 2;
            }
            else
            {
                im.da.uses |= op.aregs;
                // Warn if memory contents is 16-bit jump/call destination.
                if (datasize == 2 && (op.arg & B.MODMASK) == B.JMPCALL)
                    im.da.warnings |= DAW.JMP16;
                op.opsize = datasize;
            }
            op.opconst = (uint)(-(int)op.opsize);       // ESP is predecremented
            op.granularity = datasize;            // Default, may be overriden later
            op.scale[(int)REG.ESP] = 1;
            // Decode source operand to text, if requested.
            if ((im.damode & DA.TEXT) != 0)
            {
                string ardec = "";
                if (im.config.disasmmode == DAMODE.ATT)
                {
                    ardec = "%";
                }
                if (addrsize == 2)
                    ardec += regname16[(int)REG.ESP];
                else
                    ardec += regname32[(int)REG.ESP];
                PF originallist = im.prefixlist;
                im.prefixlist &= ~PF.SEGMASK;
                op.text = Memaddrtotext(im, op.arg, datasize, op.seg, ardec, 0);
                im.prefixlist = originallist;
            }
        }

        // Decodes segment register.
        static void Operandsegreg(ref t_imdata im, int index, ref t_operand op)
        {
            op.features = OP.SEGREG;
            if (index >= NSEG)
            {
                op.features |= OP.INVALID;          // Invalid segment register
                im.da.errors |= DAE.BADSEG;
            }
            op.opsize = op.granularity = 2;
            op.reg = (REG)index;
            op.seg = SEG.UNDEF;                   // Because this is not a memory address
            if ((op.arg & (B.CHG | B.UPD))!=0)
                im.da.warnings |= DAW.SEGMOD;      // Modifies segment register
                                                     // Decode name of segment register.
            if ((im.damode & DA.TEXT)!=0)
            {
                if (im.config.disasmmode == DAMODE.ATT) op.text += '%';
                op.text += segname[index];
            }
        }

        static void Operandsegreg(ref t_imdata im, SEG index, ref t_operand op)
            => Operandsegreg(ref im, (int)index, ref op);

        // Decodes FPU register operand.
        static void Operandfpureg(ref t_imdata im, int index, ref t_operand op)
        {
            op.features = OP.FPUREG;
            op.opsize = op.granularity = 10;
            op.reg = (REG)index;
            op.seg = SEG.UNDEF;                   // Because this is not a memory address
                                                  // Decode name of FPU register.
            if ((im.damode & DA.TEXT) != 0)
            {
                if (im.config.disasmmode == DAMODE.ATT)
                {
                    if (im.config.simplifiedst && index == 0)
                        op.text += "%ST";
                    else
                    {
                        op.text = $"%{fpushort[index]}";
                    }
                }
                else if (im.config.simplifiedst && index == 0)
                    op.text += "ST";
                else if (im.config.disasmmode != DAMODE.HLA)
                    op.text += fpulong[index];
                else
                    op.text += fpushort[index];
            }
        }

        // Decodes MMX register operands. ATTENTION, does not set correct granularity!
        static void Operandmmxreg(ref t_imdata im, int index, ref t_operand op)
        {
            op.features = OP.MMXREG;
            op.opsize = 8;
            op.granularity = 4;                   // Default, correct it later!
            op.reg = (REG)index;
            op.seg = SEG.UNDEF;
            // Decode name of MMX/3DNow! register.
            if ((im.damode & DA.TEXT)!=0)
            {
                if (im.config.disasmmode == DAMODE.ATT) op.text += '%';
                op.text += mmxname[index];
            }
        }

        // Decodes 3DNow! register operands. ATTENTION, does not set correct
        // granularity!
        static void Operandnowreg(ref t_imdata im, int index, ref t_operand op)
        {
            op.features = OP._3DNOWREG;
            op.opsize = 8;
            op.granularity = 4;                   // Default, correct it later!
            op.reg = (REG)index;
            op.seg = SEG.UNDEF;
            // Decode name of MMX/3DNow! register.
            if ((im.damode & DA.TEXT)!=0)
            {
                if (im.config.disasmmode == DAMODE.ATT) op.text += '%';
                op.text += mmxname[index];
            }
        }

        // Decodes SSE register operands. ATTENTION, does not set correct granularity!
        static void Operandssereg(ref t_imdata im, int index, ref t_operand op)
        {
            op.features = OP.SSEREG;
            if ((op.arg & B.NOVEXSIZE) != 0)
                op.opsize = 16;
            else
                op.opsize = im.ssesize;
            op.granularity = 4;                   // Default, correct it later!
            op.reg = (REG)index;
            op.seg = SEG.UNDEF;
            // Note that some rare SSE commands may use Reg without ModRM.
            if (im.modsize == 0)
                im.modsize = 1;
            // Decode name of SSE register.
            if ((im.damode & DA.TEXT) != 0)
            {
                if (im.config.disasmmode == DAMODE.ATT) op.text += '%';
                if (op.opsize == 32) op.text += sse256[index];
                else op.text += sse128[index];
            }
        }

        // Decodes flag register EFL.
        static void Operandefl(ref t_imdata im, uint datasize, ref t_operand op)
        {
            op.features = OP.OTHERREG;
            op.opsize = op.granularity = datasize;
            op.reg = REG.UNDEF;
            op.seg = SEG.UNDEF;
            // Decode name of register.
            if ((im.damode & DA.TEXT)!=0)
            {
                if (im.config.disasmmode == DAMODE.ATT)
                    op.text += "%EFL";
                else
                    op.text += "EFL";
            }
        }

        // Decodes 8/16/32-bit immediate jump/call offset relative to EIP of next
        // command.
        static void Operandoffset(ref t_imdata im, uint offsetsize, uint datasize,
          byte* cmd, uint cmdsize, uint offsaddr, ref t_operand op)
        {
            if (cmdsize < offsetsize)
            {
                im.da.errors |= DAE.CROSS;         // Command crosses end of memory block
                return;
            }
            op.features = OP.CONST;
            op.opsize = op.granularity = datasize; // NOT offsetsize!
            im.immsize1 = offsetsize;
            op.reg = REG.UNDEF;
            op.seg = SEG.UNDEF;
            offsaddr += offsetsize;
            if (offsetsize == 1)                   // Sign-extandable constant
                op.opconst = (uint)*(sbyte*)cmd + offsaddr;
  else if (offsetsize == 2)              // 16-bit immediate offset, rare
                op.opconst = *(ushort*)cmd + offsaddr;
            else                                 // 32-bit immediate offset
                op.opconst = *(uint*)cmd + offsaddr;
            if (datasize == 2)
            {
                op.opconst &= 0x0000FFFF;
                im.da.warnings |= DAW.JMP16;
            }    // Practically unused in Win32 code
            im.usesdatasize = 1;
            // Decode address of destination to text, if requested.
            if ((im.damode & DA.TEXT)!=0)
            {
                if (offsetsize == 1 && im.config.disasmmode != DAMODE.HLA &&
                  im.config.disasmmode != DAMODE.ATT)
                    op.text += "SHORT ";

                if (datasize == 4)
                {
                    if (decodeaddress(out var label, (int)op.opconst))
                        op.text += label;
                    else
                    {
                        if (im.config.disasmmode == DAMODE.ATT)
                            op.text += '$';
                        op.text += Hexprint(4, (int)op.opconst, im, op.arg);
                    }
                }
                else
                {
                    if (im.config.disasmmode == DAMODE.ATT)
                        op.text += '$';
                    op.text += Hexprint(2, (int)op.opconst, im, op.arg);
                }
            }
        }

        // Decodes 16:16/16:32-bit immediate absolute far jump/call address.
        static void Operandimmfaraddr(ref t_imdata im, uint datasize, byte* cmd,
          uint cmdsize, ref t_operand op)
        {
            if (cmdsize < datasize + 2)
            {
                im.da.errors |= DAE.CROSS;         // Command crosses end of memory block
                return;
            }
            op.features = OP.CONST | OP.SELECTOR;
            op.opsize = datasize + 2;
            op.granularity = datasize;            // Attention, non-standard case!
            op.reg = REG.UNDEF;
            op.seg = SEG.UNDEF;
            im.immsize1 = datasize;
            im.immsize2 = 2;
            if (datasize == 2)
            {
                op.opconst = *(ushort*)cmd;
                im.da.warnings |= DAW.JMP16;
            }     // Practically unused in Win32 code
            else
            {
                op.opconst = *(uint*)cmd;
                im.da.immfixup = (int)im.mainsize;
            }
            op.selector = *(ushort*)(cmd + datasize);
            im.usesdatasize = 1;
            // Decode address of destination to text, if requested.
            if ((im.damode & DA.TEXT)!=0)
            {
                if (im.config.disasmmode == DAMODE.ATT)
                {
                    op.text = "$";
                }
                else
                    op.text += "FAR ";
                op.text += Hexprint(2, (int)op.selector, im, op.arg);
                op.text += ':';
                if (im.config.disasmmode == DAMODE.ATT)
                    op.text += '$';
                op.text += Hexprint(4, (int)op.opconst, im, op.arg);
            }
        }

        // Decodes immediate constant 1 used in shift operations.
        static void Operandone(ref t_imdata im, ref t_operand op)
        {
            op.features = OP.CONST;
            op.opsize = op.granularity = 1;        // Just to make it defined
            op.reg = REG.UNDEF;
            op.seg = SEG.UNDEF;
            op.opconst = 1;
            if ((im.damode & DA.TEXT)!=0)
            {
                if (im.config.disasmmode == DAMODE.ATT)
                    op.text += "$1";
                else
                    op.text += "1";
            }
        }

        // Decodes 8/16/32-bit immediate constant (possibly placed after ModRegRM-SIB-
        // Disp combination). Constant is nbytes long in the command and extends to
        // constsize bytes. If constant is a count, it deals with data of size datasize.
        // ATTENTION, calling routine must set usesdatasize by itself!
        static void Operandimmconst(ref t_imdata im, uint nbytes, uint constsize,
          uint datasize, byte* cmd, uint cmdsize, bool issecond, ref t_operand op)
        {
            if (cmdsize < im.modsize + im.dispsize + nbytes + (issecond ? im.immsize1 : 0))
            {
                im.da.errors |= DAE.CROSS;         // Command crosses end of memory block
                return;
            }
            op.features = OP.CONST;
            op.opsize = op.granularity = constsize;
            cmd += im.modsize + im.dispsize;
            if (!issecond)
                im.immsize1 = nbytes;               // First constant
            else
            {
                im.immsize2 = nbytes;               // Second constant (ENTER only)
                cmd += im.immsize1;
            }
            op.reg = REG.UNDEF;
            op.seg = SEG.UNDEF;
            if (nbytes == 4)
            {   // 32-bit immediate constant
                op.opconst = *(uint*)cmd;
                im.da.immfixup = (int)(im.mainsize + im.modsize + im.dispsize + (issecond ? im.immsize1 : 0));
            }
            else if (nbytes == 1)                  // 8-byte constant, maybe sign-extendable
                op.opconst = (uint)*(sbyte*)cmd;
            else                                 // 16-bite immediate constant, rare
                op.opconst = *(ushort*)cmd;
            if (constsize == 1)
                op.opconst &= 0x000000FF;
            else if (constsize == 2)
                op.opconst &= 0x0000FFFF;
            switch (op.arg & B.MODMASK)
            {
                case B.BITCNT:                     // Constant is a bit count
                    if ((datasize == 4 && op.opconst > 31) ||
                      (datasize == 1 && op.opconst > 7) ||
                      (datasize == 2 && op.opconst > 15)) im.da.warnings |= DAW.SHIFT;
                    break;
                case B.SHIFTCNT:                   // Constant is a shift count
                    if (op.opconst == 0 ||
                      (datasize == 4 && op.opconst > 31) || (datasize == 1 && op.opconst > 7) ||
                      (datasize == 2 && op.opconst > 15)) im.da.warnings |= DAW.SHIFT;
                    break;
                case B.STACKINC:                   // Stack increment must be DWORD-aligned
                    if ((op.opconst & 0x3) != 0)
                        im.da.warnings |= DAW.STACK;
                    im.da.stackinc = op.opconst;
                    break;
                default: break;
            }
            if ((im.damode & DA.TEXT) != 0)
            {
                B mod = op.arg & B.MODMASK;
                if (constsize == 1)
                {                // 8-bit constant
                    if (im.config.disasmmode == DAMODE.ATT)
                        op.text += '$';
                    op.text+=Hexprint(1, (int)op.opconst, im, op.arg);
                }
                else if (constsize == 4)
                {           // 32-bit constant
                    if ((mod == B.NONSPEC || mod == B.JMPCALL || mod == B.JMPCALLFAR) && decodeaddress(out var label, (int)op.opconst))
                        op.text += label;
                    else
                    {
                        int u;
                        if (im.config.disasmmode == DAMODE.ATT)
                            op.text += '$';
                        if (mod != B.UNSIGNED && mod != B.BINARY && mod != B.PORT &&
                          (int)op.opconst < 0 &&
                          (mod == B.SIGNED || (int)op.opconst > NEGLIMIT)
                        )
                        {
                            op.text +='-'; u = -(int)op.opconst;
                        }
                        else
                            u = (int)op.opconst;
                        op.text += Hexprint(4, u, im, op.arg);
                    }
                }
                else
                {                             // 16-bit constant
                    if (im.config.disasmmode == DAMODE.ATT)
                        op.text += '$';
                    else if ((op.arg & B.SHOWSIZE) != 0)
                    {
                        op.text += $"{sizename[constsize]} ";
                    }
                    op.text += Hexprint(2, (int)op.opconst, im, op.arg);
                }
            }
            return;
        }

        // Decodes contrtol register operands.
        static void Operandcreg(ref t_imdata im, int index, ref t_operand op)
        {
            op.features = OP.CREG;
            op.opsize = op.granularity = 4;
            op.reg = (REG)index;
            op.seg = SEG.UNDEF;
            // Decode name of control register.
            if ((im.damode & DA.TEXT)!=0)
            {
                if (im.config.disasmmode == DAMODE.ATT) op.text += '%';
                op.text += crname[index];
            }
            // Some control registers are physically absent_
            if (index != 0 && index != 2 && index != 3 && index != 4)
                im.da.errors |= DAE.BADCR;
        }

        // Decodes debug register operands.
        static void Operanddreg(ref t_imdata im, int index, ref t_operand op)
        {
            op.features = OP.DREG;
            op.opsize = op.granularity = 4;
            op.reg = (REG)index;
            op.seg = SEG.UNDEF;
            // Decode name of debug register.
            if ((im.damode & DA.TEXT)!=0)
            {
                if (im.config.disasmmode == DAMODE.ATT) op.text += '%';
                op.text += drname[index];
            }
        }

        // Decodes FPU status register FSt_
        static void Operandfst(ref t_imdata im, ref t_operand op)
        {
            op.features = OP.OTHERREG;
            op.opsize = op.granularity = 2;
            op.reg = REG.UNDEF;
            op.seg = SEG.UNDEF;
            // Decode name of register.
            if ((im.damode & DA.TEXT)!=0)
            {
                if (im.config.disasmmode == DAMODE.ATT)
                    op.text += "%FST";
                else
                    op.text += "FST";
            }
        }

        // Decodes FPU control register FCW.
        static void Operandfcw(ref t_imdata im, ref t_operand op)
        {
            op.features = OP.OTHERREG;
            op.opsize = op.granularity = 2;
            op.reg = REG.UNDEF;
            op.seg = SEG.UNDEF;
            // Decode name of register.
            if ((im.damode & DA.TEXT)!=0)
            {
                if (im.config.disasmmode == DAMODE.ATT)
                    op.text += "%FCW";
                else
                    op.text += "FCW";
            }
        }

        // Decodes SSE control register MXCSR.
        static void Operandmxcsr(ref t_imdata im, ref t_operand op)
        {
            op.features = OP.OTHERREG;
            op.opsize = op.granularity = 4;
            op.reg = REG.UNDEF;
            op.seg = SEG.UNDEF;
            // Decode name of register.
            if ((im.damode & DA.TEXT)!=0)
            {
                if (im.config.disasmmode == DAMODE.ATT)
                    op.text += "%MXCSR";
                else
                    op.text += "MXCSR";
            }
        }

        ////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////// DISASSEMBLER /////////////////////////////////

        // Disassembles first command in the binary code of given length at given
        // address. Assumes that address and data size attributes of all participating
        // segments are 32 bit (flat model). Returns length of the command or 0 in case
        // of severe error.
        protected uint Disasm(byte* cmd, uint cmdsize, uint ip, out t_disasm da, DA damode)
        {
            int q;
            uint n, datasize = 0;
            int noperand = 0;
            t_imdata im;
            t_modrm pmrm;
            // Verify input parameters.
            if (cmd == null || cmdsize == 0 || cmdchain == null)
            {
                da = default(t_disasm);
                return 0;// Error in parameters or uninitialized
            }

            // Initialize t_disasm structure that receives results of disassembly. This
            // structure is very large, memset() or several memset()'s would take much,
            // much longer.
            da = t_disasm.New(ip);

            // Prepare intermediate data. This data allows to keep Disasm() reentrant
            // (thread-safe).
            damode |= DA.PSEUDO;
            im.da = da;
            im.damode = damode;
            t_config config;
            im.config = config = defconfig;       // Use default configuration
            im.prefixlist = 0;
            im.ssesize = 16;                       // Default
            im.immsize1 = im.immsize2 = 0;
            im.mainsize = 0;
            PF prefix, prefixmask = PF.SEGMASK;
            // Correct 80x86 command may contain up to 4 prefixes belonging to different
            // prefix groups. If Disasm() detects second prefix from the same group, it
            // flushes first prefix in the sequence as a pseudocommand. (This is not
            // quite true; all CPUs that I have tested accept repeating prefixes. Still,
            // who will place superfluous and possibly nonportable prefixes into the
            // code?)
            for (n = 0; ; n++)
            {
                if (n >= cmdsize)
                {                  // Command crosses end of memory block
                    n = 0; im.prefixlist = 0;            // Decode as standalone prefix
                    break;
                }
                // Note that some CPUs treat REPx and LOCK as belonging to the same group.
                switch (cmd[n])
                {
                    case 0x26: prefix = PF.ES; prefixmask = PF.SEGMASK; break;
                    case 0x2E: prefix = PF.CS; prefixmask = PF.SEGMASK; break;
                    case 0x36: prefix = PF.SS; prefixmask = PF.SEGMASK; break;
                    case 0x3E: prefix = PF.DS; prefixmask = PF.SEGMASK; break;
                    case 0x64: prefix = PF.FS; prefixmask = PF.SEGMASK; break;
                    case 0x65: prefix = PF.GS; prefixmask = PF.SEGMASK; break;
                    case 0x66: prefix = prefixmask = PF.DSIZE; break;
                    case 0x67: prefix = prefixmask = PF.ASIZE; break;
                    case 0xF0: prefix = prefixmask = PF.LOCK; break;
                    case 0xF2: prefix = PF.REPNE; prefixmask = PF.REPMASK; break;
                    case 0xF3: prefix = PF.REP; prefixmask = PF.REPMASK; break;
                    default: prefix = 0; break;
                }
                if (prefix == 0)
                    break;
                if ((im.prefixlist & prefixmask) != 0)
                {
                    da.errors |= DAE.SAMEPREF;        // Two prefixes from the same group
                    break;
                }
                im.prefixlist |= prefix;
            }
            // There may be VEX prefix preceding command body. Yes, VEX is supported in
            // the 32-bit mode! And even in the 16-bit, but who cares?
            int vex = 0, vexreg = 0;
            DX vexlead = 0;
            if (cmdsize >= n + 3 && (*(ushort*)(cmd + n) & 0xC0FE) == 0xC0C4)
            {
                // VEX is not compatible with LOCK, 66, F2 and F3 prefixes. VEX is not
                // compatible with REX, too, but REX prefixes are missing in 32-bit mode.
                if ((im.prefixlist & (PF.LOCK | PF._66 | PF.F2 | PF.F3)) != 0)
                    da.errors |= DAE.SAMEPREF;        // Incompatible prefixes
                else
                {
                    if (cmd[n] == 0xC5)
                    {
                        // 2-byte VEX prefix.
                        im.prefixlist |= PF.VEX2;
                        vex = cmd[n + 1];
                        vexlead = DX.VEX | DX.LEAD0F; n += 2;
                    }
                    else
                    {
                        // 3-byte VEX prefix.
                        im.prefixlist |= PF.VEX3;
                        vex = cmd[n + 2] + (cmd[n + 1] << 8);    // Note the order of the bytes!
                        switch (vex & 0x1F00)
                        {
                            case 0x0100: vexlead = DX.VEX | DX.LEAD0F; n += 3; break;
                            case 0x0200: vexlead = DX.VEX | DX.LEAD38; n += 3; break;
                            case 0x0300: vexlead = DX.VEX | DX.LEAD3A; n += 3; break;
                            default: vex = 0; break;       // Unsupported VEX, decode as LES
                        }
                    }
                    if (vex != 0)
                    {
                        // Get size of operands.
                        if ((vex & 0x0004) != 0)
                            im.ssesize = 32;               // 256-bit SSE operands
                                                           // Get register encoded in VEX prefix.
                        vexreg = (~vex >> 3) & 0x07;
                        // Check for SIMD prefix.
                        switch (vex & 0x3)
                        {
                            case 0x0001: im.prefixlist |= PF._66; break;
                            case 0x0002: im.prefixlist |= PF.F3; break;
                            case 0x0003: im.prefixlist |= PF.F2; break;
                        }
                    }
                }
                if (n >= cmdsize)
                {                  // Command crosses end of memory block
                    n = 0; vex = 0; im.prefixlist = 0;     // Decode as LES
                }
            }
            // We have gathered all prefixes, including those that are integral part of
            // the SSE command.
            if (n > 4 || (da.errors & DAE.SAMEPREF) != 0)
            {
                if (n > 4) da.errors |= DAE.MANYPREF;
                n = 0; im.prefixlist = 0;
            }           // Decode as standalone prefix
            da.prefixes = im.prefixlist;
            da.nprefix = n;
            // Fetch first 4 bytes of the command and find start of command chain in the
            // command table.
            int code;
            if (cmdsize >= n + sizeof(int))
                code = *(int*)(cmd + n);            // Optimization for most frequent case
            else
            {
                code = cmd[n];

                if (cmdsize > n + 0) code |= cmd[n + 1];
                if (cmdsize > n + 1) code |= (cmd[n + 2] << 0x08);
                if (cmdsize > n + 2) code |= (cmd[n + 3] << 0x10);
            }
            // ウォークチェーンと一致するコマンドを検索します。コマンドは次の場合にマッチします：
            // (1) マスクで許容されるコードビットがコマンドとディスクリプタで一致する。
            // (2) コマンドタイプがD.MEMORYを含むとき、ModRegRMバイトはメモリを示さなければならず、
            //     タイプがD.REGISTERを含むとき、Modはレジスタを指示しなければならない。
            // (3) ビットD.DATAxxまたはD.ADDRxxがセットされているとき、
            //     データおよび / またはコードのサイズはこれらのビットと一致しなければならない。
            // (4) フィールドD.MUSTMASKは、収集されたプレフィックスに一致しなければならない。
            // (5) VEX接頭辞の有無は、DX.VEXと一致する必要があります。 
            //     VEXが存在する場合、暗黙の先頭バイトはvexleadと一致しなければならず、ビットLはDX.VLMASKと一致しなければならない。
            // (6) 短い形式の文字列コマンドが要求された場合、ビットD.LONGFORMをクリアする必要があります。
            //     または、DS：以外のセグメントオーバーライド接頭辞、
            //     またはアドレスサイズ接頭辞を指定する必要があります。
            // (7) D.POSTBYTEビットがセットされている場合、ModRegRM / SIB / offsetの後のバイトは、ポストバイと一致しなければならない。
            //     postbytedコマンドはすべて、ModRegRM形式のメモリアドレスを含み、即時定数は含まれていないことに注意してください。
            // (8）条件付きコマンドの代替形式が要求された場合、
            //     コマンドは条件付きで、DX.ZEROMASKまたはDX.CARRYMASKとしてマークされている場合、
            //     これらのビットがdamodeと一致するかどうかを確認します。 
            //     （フラグZ！= 0の条件分岐は、JZまたはJEのいずれかとして分解することができます。
            //      最初の形式はSUBまたはDECよりも優先されます; 2番目の形式はCMP後により自然です）。
            // (9）コマンドがニーモニックRETNを持っていて、代替形式RETが必要な場合はスキップしてください。 -RETが続きます。
            bool success = false;
            D cmdtype = 0;
            t_bincmd pcmd = default(t_bincmd);
            foreach (var mycmd in cmdchain[(code & CMDMASK)])
            {
                if (((code ^ mycmd.code) & mycmd.mask) != 0)
                    continue;// (1)異なるコードビット
                pcmd = mycmd;
                if (pcmd.name == null) break;
                cmdtype = pcmd.cmdtype;
                if ((damode & DA.TEXT) != 0)
                {
                    if ((pcmd.exttype & DX.RETN) != 0 && config.useretform)
                        continue;                      // (9) 予想に近いリターンのRETフォーム
                    if ((cmdtype & D.COND) != 0 &&
                      (pcmd.exttype & (DX.ZEROMASK | DX.CARRYMASK)) != 0
                    )
                    {
                        if ((damode & DA.JZ) != 0 && (pcmd.exttype & DX.ZEROMASK) == DX.JE)
                            continue;                    // (8) DX.JZを待つ
                        if ((damode & DA.JC) != 0 && (pcmd.exttype & DX.CARRYMASK) == DX.JB)
                            continue;                    // (8) DX.JCを待つ
                    }
                }
                if ((pcmd.exttype & (DX.VEX | DX.LEADMASK)) != vexlead)
                    continue;// (5) マッチしないVEXプレフィックス
                if ((pcmd.exttype & DX.VEX) != 0)
                {
                    if (((pcmd.exttype & DX.VLMASK) == DX.LSHORT && (vex & 0x04) != 0) ||
                      ((pcmd.exttype & DX.VLMASK) == DX.LLONG && (vex & 0x04) == 0))
                        continue;// (5) マッチしないVEX.L
                }
                if ((cmdtype & (D.MEMORY | D.REGISTER | D.LONGFORM | D.SIZEMASK | D.MUSTMASK | D.POSTBYTE)) == 0)
                {
                    success = true;// 最も頻繁な場合の最適化
                    break;
                }
                switch (cmdtype & D.MUSTMASK)
                {
                    case D.MUST66:                   // (4) (SSE) Requires 66, no F2 or F3
                        if ((im.prefixlist & (PF._66 | PF.F2 | PF.F3)) != PF._66) continue;
                        break;
                    case D.MUSTF2:                   // (4) (SSE) Requires F2, no 66 or F3
                        if ((im.prefixlist & (PF._66 | PF.F2 | PF.F3)) != PF.F2) continue;
                        break;
                    case D.MUSTF3:                   // (4) (SSE) Requires F3, no 66 or F2
                        if ((im.prefixlist & (PF._66 | PF.F2 | PF.F3)) != PF.F3) continue;
                        break;
                    case D.MUSTNONE:                 // (4) (MMX,SSE) Requires no 66, F2, F3
                        if ((im.prefixlist & (PF._66 | PF.F2 | PF.F3)) != 0) continue;
                        break;
                    case D.NEEDF2:                   // (4) (SSE) Requires F2, no F3
                        if ((im.prefixlist & (PF.F2 | PF.F3)) != PF.F2) continue;
                        break;
                    case D.NEEDF3:                   // (4) (SSE) Requires F3, no F2
                        if ((im.prefixlist & (PF.F2 | PF.F3)) != PF.F3) continue;
                        break;
                    case D.NOREP:                    // (4) Must not include F2 or F3
                        if ((im.prefixlist & (PF.REP | PF.REPNE)) != 0) continue;
                        break;
                    case D.MUSTREP:                  // (4) Must include F3 (REP)
                    case D.MUSTREPE:                 // (4) Must include F3 (REPE)
                        if ((im.prefixlist & PF.REP) == 0) continue;
                        break;
                    case D.MUSTREPNE:                // (4) Must include F2 (REPNE)
                        if ((im.prefixlist & PF.REPNE) == 0) continue;
                        break;
                    default: break;
                }
                if ((cmdtype & D.DATA16) != 0 && (im.prefixlist & PF.DSIZE) == 0)
                    continue;                        // (3) 16-bit data expected
                if ((cmdtype & D.DATA32) != 0 && (im.prefixlist & PF.DSIZE) != 0)
                    continue;                        // (3) 32-bit data expected
                if ((cmdtype & D.ADDR16) != 0 && (im.prefixlist & PF.ASIZE) == 0)
                    continue;                        // (3) 16-bit address expected
                if ((cmdtype & D.ADDR32) != 0 && (im.prefixlist & PF.ASIZE) != 0)
                    continue;                        // (3) 32-bit address expected
                if ((cmdtype & D.LONGFORM) != 0 && config.shortstringcmds &&
                  (im.prefixlist & (PF.ES | PF.CS | PF.SS | PF.FS | PF.GS | PF.ASIZE)) == 0)
                    continue;                        // (6) Short form of string cmd expected
                if ((cmdtype & D.MEMORY) != 0)
                {
                    // (2) Command expects operand in memory (Mod in ModRegRM is not 11b).
                    if (n + pcmd.length >= cmdsize)
                        break;                         // Command longer than available code
                    if ((cmd[n + pcmd.length] & 0xC0) == 0xC0) continue;
                }
                else if ((cmdtype & D.REGISTER) != 0)
                {
                    // (2) Command expects operand in register (Mod in ModRegRM is 11b).
                    if (n + pcmd.length >= cmdsize)
                        break;                         // Command longer than available code
                    if ((cmd[n + pcmd.length] & 0xC0) != 0xC0) continue;
                }

                if ((cmdtype & D.POSTBYTE) != 0)
                {
                    // コマンドは、コードの一部としてModRegRM/SIB/offsetの後にポストバイトを要求します。
                    // コマンドが利用可能なコードよりも長い場合、すぐにレポートの一致エラーが他の場所で報告されます。

                    uint m = n + pcmd.length;          // ModRegRMバイトへのオフセット
                    if (m >= cmdsize)
                        break;                          // 利用可能なコードより長いコマンド
                    if ((im.prefixlist & PF.ASIZE) != 0)
                        m += modrm16[cmd[m]].size;      //16ビットアドレス
                    else
                    {
                        pmrm = modrm32[cmd[m]];
                        if (pmrm.psib == null)         // SIBなしの32ビットアドレス
                            m += pmrm.size;
                        else if (m + 1 >= cmdsize)      // 利用可能なコードより長いコマンド
                            break;
                        else                            // SIBつきの32ビットアドレス
                            m += pmrm.psib[cmd[m + 1]].size;
                    }
                    if (m >= cmdsize)
                        break;                          // 利用可能なコードより長いコマンド

                    // SSEおよびAVXコマンドのアスタリスクは、比較述語を意味します。
                    // 事前定義された範囲を確認してください。
                    if (cmd[m] == (byte)pcmd.postbyte ||
                      ((cmdtype & D.WILDCARD) != 0 && cmd[m] < ((pcmd.exttype & DX.VEX) != 0 ? 32 : 8)))
                        im.immsize1 = 1;                // (7) ポストバイトをimm constとして解釈する
                    else
                        continue;                       // (7)
                }
                success = true;
                break;                             // Perfect match, command found
            }

            // コマンドが悪いが接頭辞の前にある場合は、最初の接頭辞をスタンドアロンとしてデコードします。
            // この場合、コマンドの接頭辞のリストは空です。
            if (!success)
            {
                if (im.prefixlist != 0)
                {
                    n = 0; da.nprefix = 0; da.prefixes = im.prefixlist = 0;
                    code = cmd[n] & CMDMASK;
                    foreach (var mycmd in cmdchain[code])
                    {
                        if ((mycmd.cmdtype & D.CMDTYPE) != D.PREFIX)
                            continue;
                        if (((code ^ mycmd.code) & mycmd.mask) == 0)
                        {
                            pcmd = mycmd;
                            cmdtype = pcmd.cmdtype;
                            da.errors |= DAE.BADCMD;
                            break;
                        }
                    }
                }
                // 一致するコマンドが見つからない場合は、エラーを報告し、1バイトをコマンド長として返します。
                if (pcmd.name == null)
                {
                    if ((damode & DA.DUMP) != 0)
                        da.dump = cmd[0].ToString("X2");
                    if ((damode & DA.TEXT) != 0)
                    {
                        if (config.disasmmode == DAMODE.HLA)
                            da.result += sizename[1];
                        else if (config.disasmmode == DAMODE.ATT)
                            da.result += sizeatt[1];
                        else
                            da.result += sizekey[1];
                        da.result += $" {*cmd:X2}";
                    }
                    da.size = 1;
                    da.nprefix = 0;
                    da.prefixes = 0;
                    da.cmdtype = D.BAD;
                    da.exttype = 0;
                    da.errors |= DAE.BADCMD;          // Unrecognized command
                    if ((damode & DA.HILITE) != 0)
                    {
                        da.masksize = da.result.Length;
                        da.mask = string.Concat(Enumerable.Range(0, da.masksize).Select(_ => (char)DRAW.SUSPECT));
                    }
                    return 1;
                }
            }
            // 接頭辞のリストからコマンドの不可欠な部分である接頭辞を除外します。
            // 最初の比較は、義務的なプレフィックスがない場合が最も頻繁に最適化されます。
            if ((cmdtype & (D.SIZEMASK | D.MUSTMASK)) != 0)
            {
                switch (cmdtype & D.MUSTMASK)
                {
                    case D.MUST66:                  // (SSE) Requires 66, no F2 or F3
                    case D.MUSTF2:                  // (SSE) Requires F2, no 66 or F3
                    case D.MUSTF3:                  // (SSE) Requires F3, no 66 or F2
                        im.prefixlist &= ~(PF._66 | PF.F2 | PF.F3); break;
                    case D.NEEDF2:                  // (SSE) Requires F2, no F3
                    case D.NEEDF3:                  // (SSE) Requires F3, no F2
                        im.prefixlist &= ~(PF.F2 | PF.F3); break;
                    default: break;
                }
                if ((cmdtype & D.DATA16) != 0)      // データサイズ接頭辞を含める必要があります
                    im.prefixlist &= ~PF.DSIZE;
                if ((cmdtype & D.ADDR16) != 0)      // アドレスサイズ接頭辞を含める必要があります
                    im.prefixlist &= ~PF.ASIZE;
            }
            // Prepare for disassembling.
            im.modsize = 0;                         // ModRegRM/SIB バイトのサイズ
            im.dispsize = 0;                        // アドレスオフセットのサイズ
            im.usesdatasize = 0;
            im.usesaddrsize = 0;
            im.usessegment = 0;
            da.cmdtype = cmdtype;
            da.exttype = pcmd.exttype;
            n += pcmd.length;                      // ModRegRMまたはimm定数のオフセット
            if (n > cmdsize)
            {
                da.errors |= DAE.CROSS;             // メモリブロックのコマンドが終了
                goto error;
            }
            im.mainsize = n;                        // プレフィックス付きコマンドのサイズ

            // デフォルトのデータサイズを設定（多くのコマンドとオペランドがそれを上書きします）
            if ((cmdtype & D.SIZE01) != 0 && (cmd[n - 1] & 0x01) == 0)
            {
                if ((im.prefixlist & PF.DSIZE) != 0)
                    da.warnings |= DAW.DATASIZE;    // 余分なデータサイズ接頭辞
                datasize = 1;
            }
            else if ((im.prefixlist & PF.DSIZE) != 0)
                datasize = 2;
            else
                datasize = 4;

            // オペランド処理
            int k;
            t_operand op;
            for (int i = 0; i < NOPERAND; i++)
            {
                B arg = pcmd.arg[i];
                if ((arg & B.ARGMASK) == B.NONE)
                    break;                          // 最も頻繁な場合の最適化

                // pseudooperands がスキップされる場合、私はそれを処理します。
                // このようなオペランドには重要な情報が含まれることがあります。
                if ((arg & B.PSEUDO) != 0 && (damode & DA.PSEUDO) == 0)
                {
                    continue;// pseudooperandsをスキップ要求
                }
                else
                    op = da.op[noperand];
                op.arg = arg;
                switch (arg & B.ARGMASK)
                {
                    case B.AL:                      // Register AL
                        Operandintreg(ref im, 1, REG.AL, ref op);
                        break;
                    case B.AH:                      // Register AH
                        Operandintreg(ref im, 1, REG.AH, ref op);
                        break;
                    case B.AX:                       // Register AX
                        Operandintreg(ref im, 2, REG.EAX, ref op);
                        break;
                    case B.CL:                       // Register CL
                        Operandintreg(ref im, 1, REG.CL, ref op);
                        break;
                    case B.CX:                       // Register CX
                        Operandintreg(ref im, 2, REG.ECX, ref op);
                        break;
                    case B.DX:                       // Register DX
                        Operandintreg(ref im, 2, REG.EDX, ref op);
                        break;
                    case B.DXPORT:                   // Register DX as I/O port address
                        Operandintreg(ref im, 2, REG.EDX, ref op);
                        op.features |= OP.PORT;
                        break;
                    case B.EAX:                      // Register EAX
                        Operandintreg(ref im, 4, REG.EAX, ref op);
                        break;
                    case B.EBX:                      // Register EBX
                        Operandintreg(ref im, 4, REG.EBX, ref op);
                        break;
                    case B.ECX:                      // Register ECX
                        Operandintreg(ref im, 4, REG.ECX, ref op);
                        break;
                    case B.EDX:                      // Register EDX
                        Operandintreg(ref im, 4, REG.EDX, ref op);
                        break;
                    case B.ACC:                      // Accumulator (AL/AX/EAX)
                        Operandintreg(ref im, datasize, REG.EAX, ref op);
                        im.usesdatasize = 1;
                        break;
                    case B.STRCNT:                   // Register CX or ECX as REPxx counter
                        Operandintreg(ref im, ((im.prefixlist & PF.ASIZE) != 0 ? 2u : 4u), REG.ECX, ref op);
                        im.usesaddrsize = 1;
                        break;
                    case B.DXEDX:                    // Register DX or EDX in DIV/MUL
                        Operandintreg(ref im, datasize, REG.EDX, ref op);
                        im.usesdatasize = 1;
                        break;
                    case B.BPEBP:                    // Register BP or EBP in ENTER/LEAVE
                        Operandintreg(ref im, datasize, REG.EBP, ref op);
                        im.usesdatasize = 1;
                        break;
                    case B.REG:                      // 8/16/32-bit register in Reg
                        // B.REGを使用するすべてのコマンドには、ModRMを必要とする別のオペランドもあるので
                        // ここではmodsizeを設定する必要はありません。
                        if (n >= cmdsize)
                            da.errors |= DAE.CROSS;       // Command crosses end of memory block
                        else
                        {
                            Operandintreg(ref im, datasize, (cmd[n] >> 3) & 0x07, ref op);
                            im.usesdatasize = 1;
                        }
                        break;
                    case B.REG16:                    // 16-bit register in Reg
                        if (n >= cmdsize)
                            da.errors |= DAE.CROSS;       // Command crosses end of memory block
                        else
                            Operandintreg(ref im, 2, (cmd[n] >> 3) & 0x07, ref op);
                        break;
                    case B.REG32:                    // 32-bit register in Reg
                        if (n >= cmdsize)
                            da.errors |= DAE.CROSS;       // Command crosses end of memory block
                        else
                            Operandintreg(ref im, 4, (cmd[n] >> 3) & 0x07, ref op);
                        break;
                    case B.REGCMD:                   // 16/32-bit register in last cmd byte
                        Operandintreg(ref im, datasize, cmd[n - 1] & 0x07, ref op);
                        im.usesdatasize = 1;
                        break;
                    case B.REGCMD8:                  // 8-bit register in last cmd byte
                        Operandintreg(ref im, 1, cmd[n - 1] & 0x07, ref op);
                        break;
                    case B.ANYREG:                   // Reg field is unused, any allowed
                        break;
                    case B.INT:                      // 8/16/32-bit register/memory in ModRM
                    case B.INT1632:                  // 16/32-bit register/memory in ModRM
                        k = Operandmodrm(ref im, datasize, cmd + n, cmdsize - n, ref op);
                        if (k < 0) break;                // Error in address
                        if (k == 0) Operandintreg(ref im, datasize, cmd[n] & 0x07, ref op);
                        im.usesdatasize = 1;
                        break;
                    case B.INT8:                     // 8-bit register/memory in ModRM
                        k = Operandmodrm(ref im, 1, cmd + n, cmdsize - n, ref op);
                        if (k < 0) break;                // Error in address
                        if (k == 0) Operandintreg(ref im, 1, cmd[n] & 0x07, ref op);
                        break;
                    case B.INT16:                    // 16-bit register/memory in ModRM
                        k = Operandmodrm(ref im, 2, cmd + n, cmdsize - n, ref op);
                        if (k < 0) break;                // Error in address
                        if (k == 0) Operandintreg(ref im, 2, cmd[n] & 0x07, ref op);
                        break;
                    case B.INT32:                    // 32-bit register/memory in ModRM
                        k = Operandmodrm(ref im, 4, cmd + n, cmdsize - n, ref op);
                        if (k < 0) break;                // Error in address
                        if (k == 0) Operandintreg(ref im, 4, cmd[n] & 0x07, ref op);
                        break;
                    case B.INT64:                    // 64-bit integer in ModRM, memory only
                        k = Operandmodrm(ref im, 8, cmd + n, cmdsize - n, ref op);
                        if (k < 0) break;                // Error in address
                        if (k == 0)
                        {
                            // Register is not allowed, decode as 32-bit register and set error.
                            Operandintreg(ref im, 4, cmd[n] & 0x07, ref op);
                            op.features |= OP.INVALID;
                            da.errors |= DAE.MEMORY; break;
                        }
                        break;
                    case B.INT128:                   // 128-bit integer in ModRM, memory only
                        k = Operandmodrm(ref im, 16, cmd + n, cmdsize - n, ref op);
                        if (k < 0) break;                // Error in address
                        if (k == 0)
                        {
                            // Register is not allowed, decode as 32-bit register and set error.
                            Operandintreg(ref im, 4, cmd[n] & 0x07, ref op);
                            op.features |= OP.INVALID;
                            da.errors |= DAE.MEMORY; break;
                        }
                        break;
                    case B.IMMINT:                   // 8/16/32-bit int at immediate addr
                        Operandimmaddr(ref im, datasize, cmd + n, cmdsize - n, ref op);
                        im.usesdatasize = 1;
                        break;
                    case B.INTPAIR:                  // Two signed 16/32 in ModRM, memory only
                        k = Operandmodrm(ref im, 2 * datasize, cmd + n, cmdsize - n, ref op);
                        if (k < 0) break;                // Error in address
                        op.granularity = datasize;
                        if (k == 0)
                        {
                            // Register is not allowed, decode as register and set error.
                            Operandintreg(ref im, datasize, cmd[n] & 0x07, ref op);
                            op.features |= OP.INVALID;
                            da.errors |= DAE.MEMORY; break;
                        }
                        im.usesdatasize = 1;
                        break;
                    case B.SEGOFFS:                  // 16:16/16:32 absolute address in memory
                        k = Operandmodrm(ref im, datasize + 2, cmd + n, cmdsize - n, ref op);
                        if (k < 0) break;                // Error in address
                        if (k == 0)
                        {
                            // Register is not allowed, decode and set error.
                            Operandintreg(ref im, datasize, cmd[n] & 0x07, ref op);
                            op.features |= OP.INVALID;
                            da.errors |= DAE.MEMORY; break;
                        }
                        im.usesdatasize = 1;
                        break;
                    case B.STRDEST:                  // 8/16/32-bit string dest, [ES:(E)DI]
                        Operandindirect(ref im, REG.EDI, true, SEG.ES, 0, datasize, ref op);
                        im.usesdatasize = 1;
                        break;
                    case B.STRDEST8:                 // 8-bit string destination, [ES:(E)DI]
                        Operandindirect(ref im, REG.EDI, true, SEG.ES, 0, 1, ref op);
                        break;
                    case B.STRSRC:                   // 8/16/32-bit string source, [(E)SI]
                        Operandindirect(ref im, REG.ESI, true, SEG.UNDEF, 0, datasize, ref op);
                        im.usesdatasize = 1;
                        break;
                    case B.STRSRC8:                  // 8-bit string source, [(E)SI]
                        Operandindirect(ref im, REG.ESI, true, SEG.UNDEF, 0, 1, ref op);
                        break;
                    case B.XLATMEM:                  // 8-bit memory in XLAT, [(E)BX+AL]
                        Operandxlat(ref im, ref op);
                        break;
                    case B.EAXMEM:                   // Reference to memory addressed by [EAX]
                        Operandindirect(ref im, REG.EAX, false, SEG.UNDEF, 4, 1, ref op);
                        break;
                    case B.LONGDATA:                 // Long data in ModRM, mem only
                        k = Operandmodrm(ref im, 256, cmd + n, cmdsize - n, ref op);
                        if (k < 0) break;                // Error in address
                        op.granularity = 1;             // Just a trick
                        if (k == 0)
                        {
                            // Register is not allowed, decode and set error.
                            Operandintreg(ref im, 4, cmd[n] & 0x07, ref op);
                            op.features |= OP.INVALID;
                            da.errors |= DAE.MEMORY; break;
                        }
                        im.usesdatasize = 1;             // Caveat user
                        break;
                    case B.ANYMEM:                   // Reference to memory, data unimportant
                        k = Operandmodrm(ref im, 1, cmd + n, cmdsize - n, ref op);
                        if (k < 0) break;                // Error in address
                        if (k == 0)
                        {
                            // Register is not allowed, decode and set error.
                            Operandintreg(ref im, 4, cmd[n] & 0x07, ref op);
                            op.features |= OP.INVALID;
                            da.errors |= DAE.MEMORY;
                        }
                        break;
                    case B.STKTOP:                   // 16/32-bit int top of stack
                        Operandindirect(ref im, REG.ESP, true, SEG.SS, 0, datasize, ref op);
                        im.usesdatasize = 1;
                        break;
                    case B.STKTOPFAR:                // Top of stack (16:16/16:32 far addr)
                        Operandindirect(ref im, REG.ESP, true, SEG.SS, 0, datasize * 2, ref op);
                        op.granularity = datasize;
                        im.usesdatasize = 1;
                        break;
                    case B.STKTOPEFL:                // 16/32-bit flags on top of stack
                        Operandindirect(ref im, REG.ESP, true, SEG.SS, 0, datasize, ref op);
                        im.usesdatasize = 1;
                        break;
                    case B.STKTOPA:                  // 16/32-bit top of stack all registers
                        Operandindirect(ref im, REG.ESP, true, SEG.SS, 0, datasize * 8, ref op);
                        op.granularity = datasize;
                        op.modifies = da.modifies = 0xFF;
                        im.usesdatasize = 1;
                        break;
                    case B.PUSH:                     // 16/32-bit int push to stack
                    case B.PUSHRET:                  // 16/32-bit push of return address
                    case B.PUSHRETF:                 // 16:16/16:32-bit push of far retaddr
                    case B.PUSHA:                    // 16/32-bit push all registers
                        Operandpush(ref im, datasize, ref op);
                        im.usesdatasize = 1;
                        break;
                    case B.EBPMEM:                   // 16/32-bit int at [EBP]
                        Operandindirect(ref im, REG.EBP, true, SEG.SS, 0, datasize, ref op);
                        im.usesdatasize = 1;
                        break;
                    case B.SEG:                      // Segment register in Reg
                        if (n >= cmdsize)
                            da.errors |= DAE.CROSS;       // Command crosses end of memory block
                        else
                            Operandsegreg(ref im, (cmd[n] >> 3) & 0x07, ref op);
                        break;
                    case B.SEGNOCS:                  // Segment register in Reg, but not CS
                        if (n >= cmdsize)
                            da.errors |= DAE.CROSS;       // Command crosses end of memory block
                        else
                        {
                            k = (cmd[n] >> 3) & 0x07;
                            Operandsegreg(ref im, k, ref op);
                            if ((SEG)k == SEG.SS)
                                da.exttype |= DX.WONKYTRAP;
                            if ((SEG)k == SEG.CS)
                            {
                                op.features |= OP.INVALID;
                                da.errors |= DAE.BADSEG;
                            }
                        }
                        break;
                    case B.SEGCS:                    // Segment register CS
                        Operandsegreg(ref im, SEG.CS, ref op);
                        break;
                    case B.SEGDS:                    // Segment register DS
                        Operandsegreg(ref im, SEG.DS, ref op);
                        break;
                    case B.SEGES:                    // Segment register ES
                        Operandsegreg(ref im, SEG.ES, ref op);
                        break;
                    case B.SEGFS:                    // Segment register FS
                        Operandsegreg(ref im, SEG.FS, ref op);
                        break;
                    case B.SEGGS:                    // Segment register GS
                        Operandsegreg(ref im, SEG.GS, ref op);
                        break;
                    case B.SEGSS:                    // Segment register SS
                        Operandsegreg(ref im, SEG.SS, ref op);
                        break;
                    case B.ST:                       // 80-bit FPU register in last cmd byte
                        Operandfpureg(ref im, cmd[n - 1] & 0x07, ref op);
                        break;
                    case B.ST0:                      // 80-bit FPU register ST0
                        Operandfpureg(ref im, 0, ref op);
                        break;
                    case B.ST1:                      // 80-bit FPU register ST1
                        Operandfpureg(ref im, 1, ref op);
                        break;
                    case B.FLOAT32:                  // 32-bit float in ModRM, memory only
                        k = Operandmodrm(ref im, 4, cmd + n, cmdsize - n, ref op);
                        if (k < 0) break;                // Error in address
                        if (k == 0)
                        {
                            // Register is not allowed, decode as FPU register and set error.
                            Operandfpureg(ref im, cmd[n] & 0x07, ref op);
                            op.features |= OP.INVALID;
                            da.errors |= DAE.MEMORY;
                        }
                        break;
                    case B.FLOAT64:                  // 64-bit float in ModRM, memory only
                        k = Operandmodrm(ref im, 8, cmd + n, cmdsize - n, ref op);
                        if (k < 0) break;                // Error in address
                        if (k == 0)
                        {
                            // Register is not allowed, decode as FPU register and set error.
                            Operandfpureg(ref im, cmd[n] & 0x07, ref op);
                            op.features |= OP.INVALID;
                            da.errors |= DAE.MEMORY;
                        }
                        break;
                    case B.FLOAT80:                  // 80-bit float in ModRM, memory only
                        k = Operandmodrm(ref im, 10, cmd + n, cmdsize - n, ref op);
                        if (k < 0) break;                // Error in address
                        if (k == 0)
                        {
                            // Register is not allowed, decode as FPU register and set error.
                            Operandfpureg(ref im, cmd[n] & 0x07, ref op);
                            op.features |= OP.INVALID;
                            da.errors |= DAE.MEMORY;
                        }
                        break;
                    case B.BCD:                      // 80-bit BCD in ModRM, memory only
                        k = Operandmodrm(ref im, 10, cmd + n, cmdsize - n, ref op);
                        if (k < 0) break;                // Error in address
                        if (k == 0)
                        {
                            // Register is not allowed, decode as FPU register and set error.
                            Operandfpureg(ref im, cmd[n] & 0x07, ref op);
                            op.features |= OP.INVALID;
                            da.errors |= DAE.MEMORY;
                        }
                        break;
                    case B.MREG8x8:                  // MMX register as 8 8-bit integers
                    case B.MREG16x4:                 // MMX register as 4 16-bit integers
                    case B.MREG32x2:                 // MMX register as 2 32-bit integers
                    case B.MREG64:                   // MMX register as 1 64-bit integer
                        if (n >= cmdsize)
                            da.errors |= DAE.CROSS;       // Command crosses end of memory block
                        else
                        {
                            Operandmmxreg(ref im, (cmd[n] >> 3) & 0x07, ref op);
                            op.granularity = Getgranularity(arg);
                        }
                        break;
                    case B.MMX8x8:                   // MMX reg/memory as 8 8-bit integers
                    case B.MMX16x4:                  // MMX reg/memory as 4 16-bit integers
                    case B.MMX32x2:                  // MMX reg/memory as 2 32-bit integers
                    case B.MMX64:                    // MMX reg/memory as 1 64-bit integer
                        k = Operandmodrm(ref im, 8, cmd + n, cmdsize - n, ref op);
                        if (k < 0) break;                // Error in address
                        if (k == 0) Operandmmxreg(ref im, cmd[n] & 0x07, ref op);
                        op.granularity = Getgranularity(arg);
                        break;
                    case B.MMX8x8DI:                 // MMX 8 8-bit integers at [DS:(E)DI]
                        Operandindirect(ref im, REG.EDI, false, SEG.UNDEF, 0, 8, ref op);
                        op.granularity = Getgranularity(arg);
                        break;
                    case B._3DREG:                    // 3DNow! register as 2 32-bit floats
                        if (n >= cmdsize)
                            da.errors |= DAE.CROSS;       // Command crosses end of memory block
                        else
                        {
                            Operandnowreg(ref im, (cmd[n] >> 3) & 0x07, ref op);
                            op.granularity = 4;
                        }
                        break;
                    case B._3DNOW:                    // 3DNow! reg/memory as 2 32-bit floats
                        k = Operandmodrm(ref im, 8, cmd + n, cmdsize - n, ref op);
                        if (k < 0) break;                // Error in address
                        if (k == 0) Operandnowreg(ref im, cmd[n] & 0x07, ref op);
                        op.granularity = 4;
                        break;
                    case B.SREGF32x4:                // SSE register as 4 32-bit floats
                    case B.SREGF32L:                 // Low 32-bit float in SSE register
                    case B.SREGF32x2L:               // Low 2 32-bit floats in SSE register
                    case B.SREGF64x2:                // SSE register as 2 64-bit floats
                    case B.SREGF64L:                 // Low 64-bit float in SSE register
                        if (n >= cmdsize)
                            da.errors |= DAE.CROSS;       // Command crosses end of memory block
                        else
                        {
                            Operandssereg(ref im, (cmd[n] >> 3) & 0x07, ref op);
                            op.granularity = Getgranularity(arg);
                        }
                        break;
                    case B.SVEXF32x4:                // SSE reg in VEX as 4 32-bit floats
                    case B.SVEXF32L:                 // Low 32-bit float in SSE in VEX
                    case B.SVEXF64x2:                // SSE reg in VEX as 2 64-bit floats
                    case B.SVEXF64L:                 // Low 64-bit float in SSE in VEX
                        Operandssereg(ref im, vexreg, ref op);
                        op.granularity = Getgranularity(arg);
                        break;
                    case B.SSEF32x4:                 // SSE reg/memory as 4 32-bit floats
                    case B.SSEF64x2:                 // SSE reg/memory as 2 64-bit floats
                        k = Operandmodrm(ref im,
                          ((arg & B.NOVEXSIZE) != 0 ? 16 : im.ssesize), cmd + n, cmdsize - n, ref op);
                        if (k < 0) break;                // Error in address
                        if (k == 0) Operandssereg(ref im, cmd[n] & 0x07, ref op);
                        op.granularity = Getgranularity(arg);
                        break;
                    case B.SSEF32L:                  // Low 32-bit float in SSE reg/memory
                        k = Operandmodrm(ref im, 4, cmd + n, cmdsize - n, ref op);
                        if (k < 0) break;                // Error in address
                        if (k == 0)                      // Operand in SSE register
                            Operandssereg(ref im, cmd[n] & 0x07, ref op);
                        op.granularity = 4;
                        break;
                    case B.SSEF32x2L:                // Low 2 32-bit floats in SSE reg/memory
                        k = Operandmodrm(ref im,
                          ((arg & B.NOVEXSIZE) != 0 ? 16 : im.ssesize) / 2, cmd + n, cmdsize - n, ref op);
                        if (k < 0) break;                // Error in address
                        if (k == 0)                      // Operand in SSE register
                            Operandssereg(ref im, cmd[n] & 0x07, ref op);
                        op.granularity = 4;
                        break;
                    case B.SSEF64L:                  // Low 64-bit float in SSE reg/memory
                        k = Operandmodrm(ref im, 8, cmd + n, cmdsize - n, ref op);
                        if (k < 0) break;                // Error in address
                        if (k == 0)                      // Operand in SSE register
                            Operandssereg(ref im, cmd[n] & 0x07, ref op);
                        op.granularity = 8;
                        break;
                    case B.XMM0I32x4:                // XMM0 as 4 32-bit integers
                    case B.XMM0I64x2:                // XMM0 as 2 64-bit integers
                    case B.XMM0I8x16:                // XMM0 as 16 8-bit integers
                        Operandssereg(ref im, 0, ref op);
                        op.granularity = Getgranularity(arg);
                        break;
                    case B.SREGI8x16:                // SSE register as 16 8-bit sigints
                    case B.SREGI16x8:                // SSE register as 8 16-bit sigints
                    case B.SREGI32x4:                // SSE register as 4 32-bit sigints
                    case B.SREGI64x2:                // SSE register as 2 64-bit sigints
                    case B.SREGI32L:                 // Low 32-bit sigint in SSE register
                    case B.SREGI32x2L:               // Low 2 32-bit sigints in SSE register
                    case B.SREGI64L:                 // Low 64-bit sigint in SSE register
                        if (n >= cmdsize)
                            da.errors |= DAE.CROSS;       // Command crosses end of memory block
                        else
                        {
                            Operandssereg(ref im, (cmd[n] >> 3) & 0x07, ref op);
                            op.granularity = Getgranularity(arg);
                        }
                        break;
                    case B.SVEXI8x16:                // SSE reg in VEX as 16 8-bit sigints
                    case B.SVEXI16x8:                // SSE reg in VEX as 8 16-bit sigints
                    case B.SVEXI32x4:                // SSE reg in VEX as 4 32-bit sigints
                    case B.SVEXI64x2:                // SSE reg in VEX as 2 64-bit sigints
                        Operandssereg(ref im, vexreg, ref op);
                        op.granularity = Getgranularity(arg);
                        break;
                    case B.SSEI8x16:                 // SSE reg/memory as 16 8-bit sigints
                    case B.SSEI16x8:                 // SSE reg/memory as 8 16-bit sigints
                    case B.SSEI32x4:                 // SSE reg/memory as 4 32-bit sigints
                    case B.SSEI64x2:                 // SSE reg/memory as 2 64-bit sigints
                        k = Operandmodrm(ref im,
                          ((arg & B.NOVEXSIZE) != 0 ? 16 : im.ssesize), cmd + n, cmdsize - n, ref op);
                        if (k < 0) break;                // Error in address
                        if (k == 0)
                            Operandssereg(ref im, cmd[n] & 0x07, ref op);
                        op.granularity = Getgranularity(arg);
                        break;
                    case B.SSEI8x8L:                 // Low 8 8-bit ints in SSE reg/memory
                    case B.SSEI16x4L:                // Low 4 16-bit ints in SSE reg/memory
                    case B.SSEI32x2L:                // Low 2 32-bit sigints in SSE reg/memory
                        k = Operandmodrm(ref im,
                          ((arg & B.NOVEXSIZE) != 0 ? 16 : im.ssesize) / 2, cmd + n, cmdsize - n, ref op);
                        if (k < 0) break;                // Error in address
                        if (k == 0)
                            Operandssereg(ref im, cmd[n] & 0x07, ref op);
                        op.granularity = Getgranularity(arg);
                        break;
                    case B.SSEI8x4L:                 // Low 4 8-bit ints in SSE reg/memory
                    case B.SSEI16x2L:                // Low 2 16-bit ints in SSE reg/memory
                        k = Operandmodrm(ref im, 4, cmd + n, cmdsize - n, ref op);
                        if (k < 0) break;                // Error in address
                        if (k == 0)
                            Operandssereg(ref im, cmd[n] & 0x07, ref op);
                        op.granularity = Getgranularity(arg);
                        break;
                    case B.SSEI8x2L:                 // Low 2 8-bit ints in SSE reg/memory
                        k = Operandmodrm(ref im, 2, cmd + n, cmdsize - n, ref op);
                        if (k < 0) break;                // Error in address
                        if (k == 0)
                            Operandssereg(ref im, cmd[n] & 0x07, ref op);
                        op.granularity = Getgranularity(arg);
                        break;
                    case B.SSEI8x16DI:               // SSE 16 8-bit sigints at [DS:(E)DI]
                        Operandindirect(ref im, REG.EDI, false, SEG.UNDEF, 0,
                          ((arg & B.NOVEXSIZE) != 0 ? 16 : im.ssesize), ref op);
                        op.granularity = 1;
                        break;
                    case B.EFL:                      // Flags register EFL
                        Operandefl(ref im, 4, ref op);
                        break;
                    case B.FLAGS8:                   // Flags (low byte)
                        Operandefl(ref im, 1, ref op);
                        break;
                    case B.OFFSET:                   // 16/32 const offset from next command
                        Operandoffset(ref im, datasize, datasize, cmd + n, cmdsize - n, da.ip + n, ref op);
                        break;
                    case B.BYTEOFFS:                 // 8-bit sxt const offset from next cmd
                        Operandoffset(ref im, 1, datasize, cmd + n, cmdsize - n, da.ip + n, ref op);
                        break;
                    case B.FARCONST:                 // 16:16/16:32 absolute address constant
                        Operandimmfaraddr(ref im, datasize, cmd + n, cmdsize - n, ref op);
                        break;
                    case B.DESCR:                    // 16:32 descriptor in ModRM
                        k = Operandmodrm(ref im, 6, cmd + n, cmdsize - n, ref op);
                        if (k < 0) break;                // Error in address
                        if (k == 0)
                        {
                            // Register is not allowed, decode as 32-bit register and set error.
                            Operandintreg(ref im, 4, cmd[n] & 0x07, ref op);
                            op.features |= OP.INVALID;
                            da.errors |= DAE.MEMORY;
                        }
                        break;
                    case B._1:                        // Immediate constant 1
                        Operandone(ref im, ref op);
                        break;
                    case B.CONST8:                   // Immediate 8-bit constant
                        Operandimmconst(ref im, 1, 1, datasize, cmd + n, cmdsize - n, false, ref op);
                        if ((arg & B.PORT) != 0) op.features |= OP.PORT;
                        break;
                    case B.SIMMI8x16:                // SSE reg in immediate 8-bit constant
                        if (cmdsize - n < im.modsize + im.dispsize + 1)
                        {
                            da.errors |= DAE.CROSS;       // Command crosses end of memory block
                            break;
                        }
                        im.immsize1 = 1;
                        Operandssereg(ref im, (cmd[n + im.modsize + im.dispsize] >> 4) & 0x07, ref op);
                        op.granularity = Getgranularity(arg);
                        break;
                    case B.CONST8_2:                 // Immediate 8-bit const, second in cmd
                        Operandimmconst(ref im, 1, 1, datasize, cmd + n, cmdsize - n, true, ref op);
                        break;
                    case B.CONST16:                  // Immediate 16-bit constant
                        Operandimmconst(ref im, 2, 2, datasize, cmd + n, cmdsize - n, false, ref op);
                        break;
                    case B.CONST:                    // Immediate 8/16/32-bit constant
                    case B.CONSTL:                   // Immediate 16/32-bit constant
                        Operandimmconst(ref im, datasize, datasize, datasize, cmd + n, cmdsize - n, false, ref op);
                        im.usesdatasize = 1;
                        break;
                    case B.SXTCONST:                 // Immediate 8-bit sign-extended to size
                        Operandimmconst(ref im, 1, datasize, datasize, cmd + n, cmdsize - n, false, ref op);
                        im.usesdatasize = 1;
                        break;
                    case B.CR:                       // Control register in Reg
                        Operandcreg(ref im, (cmd[n] >> 3) & 0x07, ref op);
                        break;
                    case B.CR0:                      // Control register CR0
                        Operandcreg(ref im, 0, ref op);
                        break;
                    case B.DR:                       // Debug register in Reg
                        Operanddreg(ref im, (cmd[n] >> 3) & 0x07, ref op);
                        break;
                    case B.FST:                      // FPU status register
                        Operandfst(ref im, ref op);
                        break;
                    case B.FCW:                      // FPU control register
                        Operandfcw(ref im, ref op);
                        break;
                    case B.MXCSR:                    // SSE media control and status register
                        Operandmxcsr(ref im, ref op);
                        break;
                    default:                         // Internal error
                        da.errors |= DAE.INTERN;
                        break;
                }
                if ((arg & B._32BITONLY) != 0 && op.opsize != 4)
                    da.warnings |= DAW.NONCLASS;
                if ((arg & B.MODMASK) == B.JMPCALLFAR)
                    da.warnings |= DAW.FARADDR;
                if ((arg & B.PSEUDO) != 0) op.features |= OP.PSEUDO;
                if ((arg & (B.CHG | B.UPD)) != 0) op.features |= OP.MOD;
                op.opconstsize = im.dispsize;
                da.op[noperand++] = op;
            }
            if (im.prefixlist != 0)
            {   // 最も頻繁な場合の最適化
                // If LOCK prefix is present, report error if prefix is not allowed by
                // command and warning otherwise. Application code usually doesn't need
                // atomic bus access.
                if ((im.prefixlist & PF.LOCK) != 0)
                {
                    if ((cmdtype & D.LOCKABLE) == 0) da.errors |= DAE.LOCK;
                    else da.warnings |= DAW.LOCK;
                }
                // Warn if data size prefix is present but not used by command.
                if ((im.prefixlist & PF.DSIZE) != 0 && im.usesdatasize == 0 &&
                  (pcmd.exttype & DX.TYPEMASK) != DX.NOP)
                    da.warnings |= DAW.DATASIZE;
                // Warn if address size prefix is present but not used by command.
                if ((im.prefixlist & PF.ASIZE) != 0 && im.usesaddrsize == 0)
                    da.warnings |= DAW.ADDRSIZE;
                // Warn if segment override prefix is present but command doesn't access
                // memory. Prefixes CS: and DS: are also used as branch hints in
                // conditional branches.
                if ((im.prefixlist & PF.SEGMASK) != 0 && im.usessegment == 0)
                {
                    if ((cmdtype & D.BHINT) == 0 || (im.prefixlist & PF.HINT) == 0)
                        da.warnings |= DAW.SEGPREFIX;
                }
                // Warn if REPxx prefix is present but not used by command. Attention,
                // Intel frequently uses these prefixes for different means!
                if ((im.prefixlist & PF.REPMASK) != 0)
                {
                    if (((im.prefixlist & PF.REP) != 0 && (cmdtype & D.MUSTMASK) != D.MUSTREP &&
                    (cmdtype & D.MUSTMASK) != D.MUSTREPE) ||
                    ((im.prefixlist & PF.REPNE) != 0 && (cmdtype & D.MUSTMASK) != D.MUSTREPNE))
                        da.warnings |= DAW.REPPREFIX;
                }
            }
            // Warn on unaligned stack, I/O and privileged commands.
            switch (cmdtype & D.CMDTYPE)
            {
                case D.PUSH:
                    if (datasize == 2) da.warnings |= DAW.STACK; break;
                case D.INT:
                    da.warnings |= DAW.INTERRUPT; break;
                case D.IO:
                    da.warnings |= DAW.IO; break;
                case D.PRIVILEGED:
                    da.warnings |= DAW.PRIV;
                    break;
            }
            // Warn on system, privileged  and undocumented commands.
            if ((cmdtype & D.USEMASK) != 0)
            {
                if ((cmdtype & D.USEMASK) == D.RARE || (cmdtype & D.USEMASK) == D.SUSPICIOUS)
                    da.warnings |= DAW.RARE;
                if ((cmdtype & D.USEMASK) == D.UNDOC) da.warnings |= DAW.NONCLASS;
            }
            // If command implicitly changes ESP, it uses and modifies this register.
            if ((cmdtype & D.CHGESP) != 0)
            {
                da.uses |= (1 << (int)REG.ESP);
                da.modifies |= (1 << (int)REG.ESP);
            }
            error:
            // Prepare hex dump, if requested. As maximal size of command is limited to
            // MAXCMDSIZE=16 bytes, string can't overflow.
            if ((damode & DA.DUMP) != 0)
            {
                if ((da.errors & DAE.CROSS) != 0)        // Incomplete command
                    da.dump += string.Concat(Enumerable.Range(0, (int)cmdsize).Select(t => cmd[t].ToString("X2")));
                else
                {
                    int u;
                    // Dump prefixes. REPxx is treated as prefix and separated from command
                    // with semicolon; prefixes 66, F2 and F3 that are part of SSE command
                    // are glued with command's body - well, at least if there are no other
                    // prefixes inbetween.
                    for (u = 0; u < da.nprefix; u++)
                    {
                        da.dump += cmd[u].ToString("X2");
                        if (cmd[u] == 0x66 && (cmdtype & D.MUSTMASK) == D.MUST66) continue;
                        if (cmd[u] == 0xF2 && ((cmdtype & D.MUSTMASK) == D.MUSTF2 ||
                          (cmdtype & D.MUSTMASK) == D.NEEDF2)) continue;
                        if (cmd[u] == 0xF3 && ((cmdtype & D.MUSTMASK) == D.MUSTF3 ||
                          (cmdtype & D.MUSTMASK) == D.NEEDF3)) continue;
                        if ((im.prefixlist & (PF.VEX2 | PF.VEX3)) != 0 && u == da.nprefix - 2)
                            continue;
                        if ((im.prefixlist & PF.VEX3) != 0 && u == da.nprefix - 3)
                            continue;
                        da.dump += ':';
                    }
                    // Dump body of the command, including ModRegRM and SIB bytes.
                    da.dump += string.Concat(Enumerable.Range(0, (int)(im.mainsize + im.modsize - u))
                        .Select(t => cmd[u + t].ToString("X2")));
                    // Dump displacement, if any, separated with space from command's body.
                    if (im.dispsize > 0)
                    {
                        da.dump += ' ';
                        da.dump += string.Concat(Enumerable.Range(0, (int)im.dispsize)
                            .Select(t => cmd[im.mainsize + im.modsize + t].ToString("X2")));
                    }
                    // Dump immediate constants, if any.
                    if (im.immsize1 > 0)
                    {
                        da.dump += ' ';
                        da.dump += string.Concat(Enumerable.Range(0, (int)im.immsize1)
                            .Select(t => cmd[im.mainsize + im.modsize + im.dispsize + t].ToString("X2")));
                    }
                    if (im.immsize2 > 0)
                    {
                        da.dump += ' ';
                        da.dump += string.Concat(Enumerable.Range(0, (int)im.immsize2)
                            .Select(t => cmd[im.mainsize + im.modsize + im.dispsize + im.immsize1 + t].ToString("X2")));
                    }
                }
            }
            if ((da.errors & DAE.CROSS) != 0) da.cmdname = "???";
            else da.cmdname = pcmd.name;

            // Prepare disassembled command. There are many options that control look
            // and feel of disassembly, so the procedure is a bit, errr, boring.
            if ((damode & DA.TEXT) != 0)
            {
                if ((da.errors & DAE.CROSS) != 0)
                {      // Incomplete command
                    da.result += "???";
                    if ((damode & DA.HILITE) != 0)
                    {
                        da.mask = string.Concat(Enumerable.Range(0, da.result.Length).Select(_ => (char)DRAW.SUSPECT));
                        da.masksize = da.result.Length;
                    }
                }
                else
                {
                    // If LOCK and/or REPxx prefix is present, prepend it to the command.
                    // Such cases are rare, first comparison makes small optimization.
                    if ((im.prefixlist & (PF.LOCK | PF.REPMASK)) != 0)
                    {
                        if ((im.prefixlist & PF.LOCK) != 0)
                            da.result += "LOCK ";
                        if ((im.prefixlist & PF.REPNE) != 0)
                            da.result += "REPNE ";
                        else if ((im.prefixlist & PF.REP) != 0)
                        {
                            if ((cmdtype & D.MUSTMASK) == D.MUSTREPE)
                                da.result += "REPE ";
                            else
                                da.result += "REP ";
                        }
                    }
                    // If there is a branch hint, prefix jump mnemonics with '+' (taken) or
                    // '-' (not taken), or use pseudoprefixes BHT/BHNT. I don't know how MASM
                    // indicates hints.
                    if ((cmdtype & D.BHINT) != 0)
                    {
                        if (!config.jumphintmode)
                        {
                            if ((im.prefixlist & PF.TAKEN) != 0)
                                da.result += '+';
                            else if ((im.prefixlist & PF.NOTTAKEN) != 0)
                                da.result += '-';
                        }
                        else
                        {
                            if ((im.prefixlist & PF.TAKEN) != 0)
                                da.result += "BHT ";
                            else if ((im.prefixlist & PF.NOTTAKEN) != 0)
                                da.result += "BHNT ";
                        }
                    }
                    // Get command mnemonics. If mnemonics contains asterisk, it must be
                    // replaced by W, D or none according to sizesens. Asterisk in SSE and
                    // AVX commands means comparison predicate.
                    if ((cmdtype & D.WILDCARD) != 0)
                    {
                        foreach (var c in pcmd.name)
                        {
                            if (c != '*')
                                da.result += c;
                            else if ((cmdtype & D.POSTBYTE) != 0)
                                da.result += ssepredicate[cmd[im.mainsize + im.modsize + im.dispsize]];
                            else if (datasize == 4 && (config.sizesens == 0 || config.sizesens == 1))
                                da.result += 'D';
                            else if (datasize == 2 && (config.sizesens == 1 || config.sizesens == 2))
                                da.result += 'W';
                        }
                    }
                    else
                    {
                        da.result += pcmd.name;
                        if (config.disasmmode == DAMODE.ATT && im.usesdatasize != 0)
                        {
                            // AT＆Tのニーモニックにはオペランドのサフィックスが付いています。
                            if ((cmdtype & D.CMDTYPE) != D.CMD &&
                              (cmdtype & D.CMDTYPE) != D.MOV &&
                              (cmdtype & D.CMDTYPE) != D.MOVC &&
                              (cmdtype & D.CMDTYPE) != D.TEST &&
                              (cmdtype & D.CMDTYPE) != D.STRING &&
                              (cmdtype & D.CMDTYPE) != D.PUSH &&
                              (cmdtype & D.CMDTYPE) != D.POP) goto ifend;
                            else if (datasize == 1) da.result += "B";
                            else if (datasize == 2) da.result += "W";
                            else if (datasize == 4) da.result += "L";
                            else if (datasize == 8) da.result += "Q";
                            ifend:;
                        }
                    }
                    DRAW cfill = 0;
                    if ((damode & DA.HILITE) != 0)
                    {
                        D type = cmdtype & D.CMDTYPE;
                        if (da.errors != 0)
                            cfill = DRAW.SUSPECT;
                        else switch (cmdtype & D.CMDTYPE)
                            {
                                case D.JMP:                  // Unconditional near jump
                                case D.JMPFAR:               // Unconditional far jump
                                    cfill = DRAW.JUMP; break;
                                case D.JMC:                  // Conditional jump on flags
                                case D.JMCX:                 // Conditional jump on (E)CX (and flags)
                                    cfill = DRAW.CJMP; break;
                                case D.PUSH:                 // PUSH exactly 1 (d)word of data
                                case D.POP:                  // POP exactly 1 (d)word of data
                                    cfill = DRAW.PUSHPOP; break;
                                case D.CALL:                 // Plain near call
                                case D.CALLFAR:              // Far call
                                case D.INT:                  // Interrupt
                                    cfill = DRAW.CALL; break;
                                case D.RET:                  // Plain near return from call
                                case D.RETFAR:               // Far return or IRET
                                    cfill = DRAW.RET; break;
                                case D.FPU:                  // FPU command
                                case D.MMX:                  // MMX instruction, incl. SSE extensions
                                case D._3DNOW:                // 3DNow! instruction
                                case D.SSE:                  // SSE instruction
                                case D.AVX:                  // AVX instruction
                                    cfill = DRAW.FPU; break;
                                case D.IO:                   // Accesses I/O ports
                                case D.SYS:                  // Legal but useful in system code only
                                case D.PRIVILEGED:           // Privileged (non-Ring3) command
                                    cfill = DRAW.SUSPECT; break;
                                default:
                                    cfill = DRAW.PLAIN;
                                    break;
                            }
                        da.mask = string.Concat(Enumerable.Range(0, da.result.Length).Select(_ => (char)cfill));
                        da.masksize = da.result.Length;
                    }
                    // Add decoded operands. In HLA mode, order of operands is inverted
                    // except for comparison commands (marked with bit D.HLADIR) and
                    // arguments are enclosed in parenthesis (except for immediate jumps).
                    // In AT&T mode, order of operands is always inverted. Operands of type
                    // B.PSEUDO are implicit and don't appear in text.
                    bool enclose;
                    if (config.disasmmode == DAMODE.HLA &&
                      (pcmd.arg[0] & B.ARGMASK) != B.OFFSET &&
                      (pcmd.arg[0] & B.ARGMASK) != B.BYTEOFFS &&
                      (pcmd.arg[0] & B.ARGMASK) != B.FARCONST)
                        enclose = true;                     // Enclose operand list in parenthesis
                    else
                        enclose = false;
                    if ((damode & DA.HILITE) != 0 && config.hiliteoperands)
                        cfill = DRAW.PLAIN;
                    int nout = 0;
                    for (int i = 0; i < noperand; i++)
                    {
                        if ((config.disasmmode == DAMODE.HLA && (cmdtype & D.HLADIR) == 0) ||
                          config.disasmmode == DAMODE.ATT)
                            k = noperand - 1 - i;              // Inverted (HLA/AT&T) order of operands
                        else
                            k = i;                         // Direct (Intel's) order of operands
                        B arg = da.op[k].arg;
                        if ((arg & B.ARGMASK) == B.NONE || (arg & B.PSEUDO) != 0)
                            continue;                    // Empty or implicit operand
                        q = da.result.Length;
                        if (nout == 0)
                        {
                            // Spaces between mnemonic and first operand.
                            da.result += ' ';
                            if (config.tabarguments)
                            {
                                for (; da.result.Length < 8;) da.result += ' ';
                            }
                            if (enclose)
                            {
                                da.result += '(';
                                if (config.extraspace) da.result += ' ';
                            }
                        }
                        else
                        {
                            // Comma and optional space between operands.
                            da.result += ',';
                            if (config.extraspace) da.result += ' ';
                        }
                        if ((damode & DA.HILITE) != 0)
                        {
                            da.mask += string.Concat(Enumerable.Range(0, da.result.Length - q).Select(_ => (char)cfill));
                            da.masksize = da.result.Length;
                        }
                        // Operand itself.
                        q = da.result.Length;
                        op = da.op[k];
                        da.result += op.text;
                        if ((damode & DA.HILITE) != 0)
                        {
                            DRAW ofill;
                            if (!config.hiliteoperands)
                                ofill = cfill;
                            else if ((op.features & OP.REGISTER) != 0)
                                ofill = DRAW.IREG;
                            else if ((op.features & (OP.FPUREG | OP.MMXREG | OP._3DNOWREG | OP.SSEREG)) != 0)
                                ofill = DRAW.FREG;
                            else if ((op.features & (OP.SEGREG | OP.CREG | OP.DREG)) != 0)
                                ofill = DRAW.SYSREG;
                            else if ((op.features & OP.MEMORY) != 0)
                            {
                                if (op.scale[(int)REG.ESP] != 0 || op.scale[(int)REG.EBP] != 0)
                                    ofill = DRAW.STKMEM;
                                else
                                    ofill = DRAW.MEM;
                                ;
                            }
                            else if ((op.features & OP.CONST) != 0)
                                ofill = DRAW.CONST;
                            else
                                ofill = cfill;
                            da.mask += string.Concat(Enumerable.Range(0, da.result.Length - q).Select(_ => (char)ofill));
                            da.masksize = da.result.Length;
                        }
                        nout++;
                    }
                    // All arguments added, close list.
                    if (enclose && nout != 0)
                    {
                        q = da.result.Length;
                        if (config.extraspace) da.result += ' ';
                        da.result += ')';
                        if ((damode & DA.HILITE) != 0)
                        {
                            da.mask += string.Concat(Enumerable.Range(0, da.result.Length - q).Select(_ => (char)cfill));
                            da.masksize = da.result.Length;
                        }
                    }
                }
            }
            // Calculate total size of command.
            if ((da.errors & DAE.CROSS) != 0)          // Incomplete command
                n = cmdsize;
            else
                n += im.modsize + im.dispsize + im.immsize1 + im.immsize2;
            da.size = n;

            da.dumpDatas = Enumerable.Range(0, (int)n).Select(t => cmd[t]).ToArray();
            return n;
        }

        // Given error and warning lists, returns pointer to the string describing
        // relatively most severe error or warning, or NULL if there are no errors or
        // warnings.
        string Geterrwarnmessage(DAE errors, DAW warnings)
        {
            string ps;
            if (errors == 0 && warnings == 0)
                ps = null;
            else if ((errors & DAE.BADCMD) != 0)
                ps = "Unknown command";
            else if ((errors & DAE.CROSS) != 0)
                ps = "Command crosses end of memory block";
            else if ((errors & DAE.MEMORY) != 0)
                ps = "Illegal use of register";
            else if ((errors & DAE.REGISTER) != 0)
                ps = "Memory address is not allowed";
            else if ((errors & DAE.LOCK) != 0)
                ps = "LOCK prefix is not allowed";
            else if ((errors & DAE.BADSEG) != 0)
                ps = "Invalid segment register";
            else if ((errors & DAE.SAMEPREF) != 0)
                ps = "Two prefixes from the same group";
            else if ((errors & DAE.MANYPREF) != 0)
                ps = "More than 4 prefixes";
            else if ((errors & DAE.BADCR) != 0)
                ps = "Invalid CR register";
            else if ((errors & DAE.INTERN) != 0)
                ps = "Internal OllyDbg error";
            else if ((warnings & DAW.DATASIZE) != 0)
                ps = "Superfluous operand size prefix";
            else if ((warnings & DAW.ADDRSIZE) != 0)
                ps = "Superfluous address size prefix";
            else if ((warnings & DAW.SEGPREFIX) != 0)
                ps = "Superfluous segment override prefix";
            else if ((warnings & DAW.REPPREFIX) != 0)
                ps = "Superfluous REPxx prefix";
            else if ((warnings & DAW.DEFSEG) != 0)
                ps = "Explicit default segment register";
            else if ((warnings & DAW.JMP16) != 0)
                ps = "16-bit jump, call or return";
            else if ((warnings & DAW.FARADDR) != 0)
                ps = "Far jump or call";
            else if ((warnings & DAW.SEGMOD) != 0)
                ps = "Modification of segment register";
            else if ((warnings & DAW.PRIV) != 0)
                ps = "Privileged instruction";
            else if ((warnings & DAW.IO) != 0)
                ps = "I/O command";
            else if ((warnings & DAW.SHIFT) != 0)
                ps = "Shift out of range";
            else if ((warnings & DAW.LOCK) != 0)
                ps = "Command uses (valid)!=0) LOCK prefix";
            else if ((warnings & DAW.STACK) != 0)
                ps = "Unaligned stack operation";
            else if ((warnings & DAW.NOESP) != 0)
                ps = "Suspicious use of stack pointer";
            else if ((warnings & DAW.NONCLASS) != 0)
                ps = "Undocumented instruction or encoding";
            else
                ps = null;
            return ps;
        }
    }
}
