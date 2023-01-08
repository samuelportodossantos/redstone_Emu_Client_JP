using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Anantashesha.Decompiler.Disassemble
{
    unsafe partial class Disassembler
    {

        ////////////////////////////////////////////////////////////////////////////////
        const int TEXTLEN = 256;             // テキスト文字列の最大長
        const int SHORTNAME = 32;              // 短縮名またはモジュール名の最大長


        const int NOPERAND = 4;      // オペランドの最大許容数
        public const int NREG = 8;      // レジスタの数（任意のタイプの）
        const int NSEG = 6;      // 有効なセグメント・レジスタの数
        const int MAXCMDSIZE = 16;      // 有効な80x86コマンドの最大長
        const int NEGLIMIT = (-16384);      // 負の値としてオフセットをデコードすることに制限する
        const int DECLIMIT = 16384;   // 小数点としての定数のデコード制限

        // CMDMASK can be used to balance between the necessary memory size and the
        // disassembly time.
        const int CMDMASK = 0x3FFF;          // Search mask for Disassembler, 2**n-1
        const int NCHAIN = 44300;           // Max allowed number of chain links

        // Registers.
        public enum REG:int
        {
            UNDEF = (-1),

            // Codes of general purpose registers
            EAX = 0,
            ECX = 1,
            EDX = 2,
            EBX = 3,
            ESP = 4,
            EBP = 5,
            ESI = 6,
            EDI = 7,

            //Symbolic indices of 8-bit registers
            AL = 0,
            CL = 1,
            DL = 2,
            BL = 3,
            AH = 0,
            CH = 1,
            DH = 2,
            BH = 3,
        }

        public static string[] REGNAME = new string[]
        {
            "EAX","ECX","EDX","EBX","ESP","EBP","ESI","EDI"
        };

        // Codes of segment/selector registers
        public enum SEG
        {
            UNDEF = (-1),           
            ES = 0,
            CS = 1,
            SS = 2,
            DS = 3,
            FS = 4,
            GS = 5,
        }

        // Command highlighting.
        public enum DRAW {
            PLAIN = '.',//0x0000000C      // 平凡な命令
            JUMP = '>',//0x0000000D      // 無条件ジャンプコマンド
            CJMP = '?',//0x0000000E      // 条件ジャンプコマンド
            PUSHPOP = '=',//0x0000000F      // PUSH / POPコマンド
            CALL = '@',//0x00000010      // CALLコマンド
            RET = '<',//0x00000011      // RETコマンド
            FPU = '1',//0x00000012      // FPU、MMX、3DNow！ SSEコマンド
            SUSPECT = '!',//0x00000013      // 不正なシステム特権コマンド
                          // Operand highlighting.
            IREG = 'R',//0x00000018      // 汎用レジスタ
            FREG = 'F',//0x00000019      // FPU、MMXおよびSSEレジスタ
            SYSREG = 'S',//0x0000001A      // セグメントおよびシステムレジスタ
            STKMEM = 'K',//0x0000001B      // ESPまたはEBPでアクセスされるメモリ
            MEM = 'M',//0x0000001C      // その他のメモリ
            CONST = 'C',//0x0000001E      // 定数
        }

        public enum D : uint
        {
            NONE = 0x00000000,      // 特別な機能はありません

            // コマンドの一般的なタイプは、1つだけが許可されます。
            CMDTYPE = 0x0000001F,   // コマンドの種類を抽出するためのマスク
            CMD = 0x00000000,       // 普通（以下に記載されていないもの）
            MOV = 0x00000001,       // 整数レジスタへの移動または整数レジスタからの移動
            MOVC = 0x00000002,      // 条件レジスタへの条件付き移動
            SETC = 0x00000003,      // 条件付きセット整数レジスタ
            TEST = 0x00000004,      // データのテストに使用します（CMP、TEST、AND...）
            STRING = 0x00000005,    // REPxxxプレフィックスを持つ文字列コマンド
            JMP = 0x00000006,       // 無条件近点ジャンプ
            JMPFAR = 0x00000007,    // 無条件遠距離ジャンプ
            JMC = 0x00000008,       // フラグの条件ジャンプ
            JMCX = 0x00000009,      // 条件付きジャンプ（E）CX（およびフラグ）
            PUSH = 0x0000000A,      // 正確に1（d）ワードのデータをプッシュする
            POP = 0x0000000B,       // POP正確に1（d）ワードのデータ
            CALL = 0x0000000C,      // プレーン・ニア・コール
            CALLFAR = 0x0000000D,   // 遠くの呼び出し
            INT = 0x0000000E,       // 割り込み
            RET = 0x0000000F,       // コールからのリターン近くの平野
            RETFAR = 0x00000010,    // 遠方またはIRET
            FPU = 0x00000011,       // FPUコマンド
            MMX = 0x00000012,       // MMX命令、 SSE拡張
            _3DNOW = 0x00000013,    // 3DNow！命令
            SSE = 0x00000014,       // SSE命令
            IO = 0x00000015,        // I / Oポートにアクセスする
            SYS = 0x00000016,       // 法的だがシステムコードでのみ有効
            PRIVILEGED = 0x00000017,// 特権付き（Ring3以外の）コマンド
            AVX = 0x00000018,       // AVX命令
            XOP = 0x00000019,       // XOPプレフィックス付きAMD命令
            DATA = 0x0000001C,      // アナライザによって認識されるデータ
            PSEUDO = 0x0000001D,    // 疑似命令、検索モデルのみ
            PREFIX = 0x0000001E,    // スタンドアロンプ​​レフィックス
            BAD = 0x0000001F,       // 不正なコマンドまたは認識できないコマンド

            // コマンドの追加部分。
            SIZE01 = 0x00000020,    // 最後のcmdのBit = 0x01はデータサイズ
            POSTBYTE = 0x00000040,  // コマンドはポストバイトで続行されます

            // 文字列コマンドの場合は、長い形式と短い形式のいずれかを選択できます。
            LONGFORM = 0x00000080,// ロングフォームの文字列コマンド

            // いくつかのコマンドのデコードは、データまたはアドレスのサイズに依存します。
            SIZEMASK = 0x00000F00,// データ/アドレスサイズ依存のマスク
            DATA16 = 0x00000100,// 16ビットのデータサイズが必要
            DATA32 = 0x00000200,// 32ビットのデータサイズが必要
            ADDR16 = 0x00000400,// 16ビットアドレスサイズが必要
            ADDR32 = 0x00000800,// 32ビットアドレスサイズが必要

            // コマンドが所有していても、所有していなくてもよいプレフィックス。
            MUSTMASK = 0x0000F000,// プレフィックスの固定セットのマスク
            NOMUST = 0x00000000,// 義務的なプレフィックスはありません（デフォルト）
            MUST66 = 0x00001000,// （SSE、AVX）66、F2またはF3なし
            MUSTF2 = 0x00002000,// （SSE、AVX）F2、66またはF3が必要
            MUSTF3 = 0x00003000,// （SSE、AVX）F3、66またはF2が必要
            MUSTNONE = 0x00004000,// （MMX、SSE、AVX）必要なし66、F2、F3
            NEEDF2 = 0x00005000,// （SSE、AVX）F2、F3なし
            NEEDF3 = 0x00006000,// （SSE、AVX）F3、F2なし
            NOREP = 0x00007000,// F2またはF3を含まない
            MUSTREP = 0x00008000,// F3（REP）
            MUSTREPE = 0x00009000,// F3（REPE）
            MUSTREPNE = 0x0000A000,// F2（REPNE）
            LOCKABLE = 0x00010000,// F0（LOCK、メモリのみ）
            BHINT = 0x00020000,// 分岐ヒント（2E、3E）

            // ModRM-SIBでのいくつかのコマンドのデコードは、レジスタかメモリかに依存します。
            MEMORY = 0x00040000,// Modフィールドはメモリを示す必要があります
            REGISTER = 0x00080000,// Modフィールドはレジスタを示す必要があります

            // コマンドによる副作用。
            FLAGMASK = 0x00700000,// 変更されたフラグを抽出するマスク
            NOFLAGS = 0x00000000,// フラグS、Z、P、O、Cは変更されません。
            ALLFLAGS = 0x00100000,// フラグS、Z、P、O、Cを変更します。
            FLAGZ = 0x00200000,// フラグZのみを変更する
            FLAGC = 0x00300000,// フラグCのみを変更する
            FLAGSCO = 0x00400000,// フラグCとOのみを変更する
            FLAGD = 0x00500000,// フラグDのみを変更する
            FLAGSZPC = 0x00600000,// フラグZ、P、Cのみを変更する（FPU）
            NOCFLAG = 0x00700000,// S、Z、P、O変更、C影響なし
            FPUMASK = 0x01800000,// FPUスタックへの影響をマスクする
            FPUSAME = 0x00000000,// FPUスタックを回転しません（デフォルト）
            FPUPOP = 0x00800000,// ポップFPUスタック
            FPUPOP2 = 0x01000000,// FPUスタックを2回ポップする
            FPUPUSH = 0x01800000,// FPUスタックをプッシュする
            CHGESP = 0x02000000,// コマンドがESPを間接的に変更する

            // コマンド機能。
            HLADIR = 0x04000000,// HLAにおけるオペランドの非標準的な順序
            WILDCARD = 0x08000000,// ニーモニックにはW / Dワイルドカード（ '*'）が含まれています
            COND = 0x10000000,// 条件付き（アクションはフラグに依存する）
            USESCARRY = 0x20000000,// キャリーフラグを使用する
            USEMASK = 0xC0000000,// 異常なコマンドを検出するためのマスク
            RARE = 0x40000000,// Win32アプリケーションではまれな、または古くなった
            SUSPICIOUS = 0x80000000,// 疑わしいコマンド
            UNDOC = 0xC0000000,// 記載されていないコマンド
        }

        // Extension of D_xxx.
        public enum DX : uint
        {
            ZEROMASK = 0x00000003,// FLAGS.Zフラグのデコード方法
            JE = 0x00000001,// JE、JNZの代わりにJE、JNE
            JZ = 0x00000002,// JZ、JNZの代わりにJE、JNE
            CARRYMASK = 0x0000000C,// FLAGS.Cフラグをデコードする方法
            JB = 0x00000004,// JE、JNCの代わりにJAE、JB
            JC = 0x00000008,// JC、JAEの代わりにJNC、JB
            RETN = 0x00000010,// ニーモニックはRETNです
            VEX = 0x00000100,// VEXプレフィックスが必要です
            VLMASK = 0x00000600,// VEXオペランドの長さを抽出するマスク
            LSHORT = 0x00000000,// 128ビットのみ
            LBOTH = 0x00000200,// 128ビット版と256ビット版の両方
            LLONG = 0x00000400,// 256ビットのみ
            IGNOREL = 0x00000600,// VEX.Lを無視する
            NOVREG = 0x00000800,// VEX.vvvvはすべて1に設定する必要があります
            VWMASK = 0x00003000,// VEX.Wを抽出するマスク
            W0 = 0x00001000,// VEX.Wは0でなければならない
            W1 = 0x00002000,// VEX.Wは1でなければならない
            LEADMASK = 0x00070000,// 先頭のopcodeバイトを抽出するためのマスク
            LEAD0F = 0x00000000,// 暗黙の0F先頭バイト（デフォルト）
            LEAD38 = 0x00010000,// 暗黙の0F 38のopcodeバイト
            LEAD3A = 0x00020000,// 暗黙の0F 3Aの先頭のopcodeバイト
            WONKYTRAP = 0x00800000,// このコマンドをシングルステップ実行しないでください。
            TYPEMASK = 0xFF000000,// 精密コマンドタイプマスク
            ADD = 0x01000000,// コマンドは整数追加です
            SUB = 0x02000000,// コマンドは整数SUBです。
            LEA = 0x03000000,// コマンドはLEAです
            NOP = 0x04000000,// コマンドはNOPです。
            CALLRET = 0x08000000,//CALL後のretn

            LVEX = (VEX | LBOTH),
            GVEX = (VEX | LLONG)
        }
        
        // Type of operand, only one is allowed. Size of SSE operands is given for the
        // case of 128-bit operations and usually doubles for 256-bit AVX commands. If
        // B_NOVEXSIZE is set, memory may double but XMM registers are not promoted to
        // YMM.
        public enum B : uint
        {
            ARGMASK = 0x000000FF, // 引数の型を抽出するマスク
            NONE = 0x00000000, // オペランドの不在
            AL = 0x00000001, // AL登録
            AH = 0x00000002, // AHを登録する
            AX = 0x00000003, // レジスタAX
            CL = 0x00000004, // CL登録
            CX = 0x00000005, // レジスタCX
            DX = 0x00000006, // DXを登録する
            DXPORT = 0x00000007, // I / OポートアドレスとしてDXを登録する
            EAX = 0x00000008, // EAXを登録する
            EBX = 0x00000009, // EBXの登録
            ECX = 0x0000000A, // ECX登録
            EDX = 0x0000000B, // 登録EDX
            ACC = 0x0000000C, // アキュムレータ（AL / AX / EAX）
            STRCNT = 0x0000000D, // CXまたはECXをREPxxカウンタとして登録する
            DXEDX = 0x0000000E, // DIV / MULにDXまたはEDXを登録する
            BPEBP = 0x0000000F, // ENTER / LEAVEにBPまたはEBPを登録する
            REG = 0x00000010, // レジスタ内の8/16/32ビットレジスタ
            REG16 = 0x00000011, // レジスタ内の16ビットレジスタ
            REG32 = 0x00000012, // Regの32ビットレジスタ
            REGCMD = 0x00000013, // 最後のcmdバイトの16/32ビットレジスタ
            REGCMD8 = 0x00000014, // 最後の8バイトの8ビットレジスタ
            ANYREG = 0x00000015, // Regフィールドは使用されていません。
            INT = 0x00000016, // ModRMの8/16/32ビットレジスタ/メモリ
            INT8 = 0x00000017, // ModRMの8ビットレジスタ/メモリ
            INT16 = 0x00000018, // ModRMの16ビットレジスタ/メモリ
            INT32 = 0x00000019, // ModRMの32ビットレジスタ/メモリ
            INT1632 = 0x0000001A, // ModRMの16/32ビットレジスタ/メモリ
            INT64 = 0x0000001B, // ModRMの64ビット整数、メモリのみ
            INT128 = 0x0000001C, // ModRMの128ビット整数、メモリのみ
            IMMINT = 0x0000001D, // 直前のアドレスで8/16/32ビットint
            INTPAIR = 0x0000001E, // ModRMで2つの16/32サイン付き、メモリのみ
            SEGOFFS = 0x0000001F, // 16：16/16：32メモリ内の絶対アドレス
            STRDEST = 0x00000020, // 8/16/32ビット文字列dest、[ES：（E）DI]
            STRDEST8 = 0x00000021, // 8ビットのストリングデスティネーション、[ES：（E）DI]
            STRSRC = 0x00000022, // 8/16/32ビット文字列ソース、[（E）SI]
            STRSRC8 = 0x00000023, // 8ビットの文字列ソース、[（E）SI]
            XLATMEM = 0x00000024, // XLATの8ビット・メモリ、[（E）BX + AL]
            EAXMEM = 0x00000025, // [EAX]が扱うメモリへの参照
            LONGDATA = 0x00000026, // ModRMの長いデータ、memのみ
            ANYMEM = 0x00000027, // メモリへの参照、重要でないデータ
            STKTOP = 0x00000028, // 16/32ビットintスタックトップ
            STKTOPFAR = 0x00000029, // スタックトップ（16：16/16：32 far addr）
            STKTOPEFL = 0x0000002A, // スタックの最上位にある16/32ビットフラグ
            STKTOPA = 0x0000002B, // 16/32ビットトップ・トゥ・オール・レジスタ
            PUSH = 0x0000002C, // 16/32ビットint push to stack
            PUSHRET = 0x0000002D, // リターンアドレスの16/32ビットプッシュ
            PUSHRETF = 0x0000002E, // 16：16/16：far retaddrの32ビットプッシュ
            PUSHA = 0x0000002F, // 16/32ビットプッシュオールレジスタ
            EBPMEM = 0x00000030, // [EBP] の16/32ビット整数
            SEG = 0x00000031, // Regのセグメントレジスタ
            SEGNOCS = 0x00000032, // Regではセグメント・レジスタが、CSではそうではない
            SEGCS = 0x00000033, // セグメントレジスタCS
            SEGDS = 0x00000034, // セグメントレジスタDS
            SEGES = 0x00000035, // セグメントレジスタES
            SEGFS = 0x00000036, // セグメントレジスタFS
            SEGGS = 0x00000037, // セグメントレジスタGS
            SEGSS = 0x00000038, // セグメントレジスタSS
            ST = 0x00000039, // 最後のcmdバイトの80ビットFPUレジスタ
            ST0 = 0x0000003A, // 80ビットFPUレジスタST0
            ST1 = 0x0000003B, // 80ビットFPUレジスタST1
            FLOAT32 = 0x0000003C, // ModRMの32ビット浮動小数点、メモリのみ
            FLOAT64 = 0x0000003D, // ModRMの64ビット浮動小数点、メモリのみ
            FLOAT80 = 0x0000003E, // ModRMの80ビット浮動小数点、メモリのみ
            BCD = 0x0000003F, // ModRMの80ビットBCD、メモリのみ
            MREG8x8 = 0x00000040, // 8つの8ビット整数としてのMMXレジスタ
            MMX8x8 = 0x00000041, // 8つの8ビット整数としてのMMX reg / memory
            MMX8x8DI = 0x00000042, // [DS：（E）DI] にMMX 8個の8ビット整数
            MREG16x4 = 0x00000043, // 4つの16ビット整数としてのMMXレジスタ
            MMX16x4 = 0x00000044, // 4つの16ビット整数としてのMMX reg / memory
            MREG32x2 = 0x00000045, // 2つの32ビット整数としてのMMXレジスタ
            MMX32x2 = 0x00000046, // 2つの32ビット整数としてのMMX reg / memory
            MREG64 = 0x00000047, // 1 64ビット整数としてのMMXレジスタ
            MMX64 = 0x00000048, // 1 64ビット整数としてのMMX reg / memory
            _3DREG = 0x00000049, // 3DNow！ 2 32ビット浮動小数点として登録
            _3DNOW = 0x0000004A, // 3DNow！ 2つの32ビット浮動小数点としてreg / memory
            XMM0I32x4 = 0x0000004B, // 4つの32ビット整数としてのXMM0
            XMM0I64x2 = 0x0000004C, // 2 64ビット整数としてのXMM0
            XMM0I8x16 = 0x0000004D, // 16個の8ビット整数としてのXMM0
            SREGF32x4 = 0x0000004E, // 4つの32ビット浮動小数点数としてのSSEレジスタ
            SREGF32L = 0x0000004F, // SSEレジスタの低32ビット浮動小数点
            SREGF32x2L = 0x00000050, // SSEレジスタの2つの32ビット浮動小数点数
            SSEF32x4 = 0x00000051, // 4つの32ビット浮動小数点としてのSSEレジスタ/メモリ
            SSEF32L = 0x00000052, // SSEレジスタ/メモリの低32ビット浮動小数点
            SSEF32x2L = 0x00000053, // SSEレジスタ/メモリの2つの32ビット浮動小数点数
            SREGF64x2 = 0x00000054, // 2 64ビット浮動小数点としてのSSEレジスタ
            SREGF64L = 0x00000055, // SSEレジスタの低64ビット浮動小数点
            SSEF64x2 = 0x00000056, // 2 64ビット浮動小数点としてのSSE reg / memory
            SSEF64L = 0x00000057, // SSEレジスタ/メモリの低64ビット浮動小数点
            SREGI8x16 = 0x00000058, // SSEは16ビットの8ビットシングントとして登録します
            SSEI8x16 = 0x00000059, // SSE reg / memoryを16ビットの8ビットシングントとして
            SSEI8x16DI = 0x0000005A, // [DS：（E）DI] にSSE 16 8ビットシグネチャ
            SSEI8x8L = 0x0000005B, // SSEレジスタ/メモリ内の8個の8ビット整数の低値
            SSEI8x4L = 0x0000005C, // SSEレジスタ/メモリの4つの8ビット整数
            SSEI8x2L = 0x0000005D, // SSEレジスタ/メモリ内の2つの8ビット整数が低い
            SREGI16x8 = 0x0000005E, // SSEは8つの16ビットのシグニチャとして登録されます
            SSEI16x8 = 0x0000005F, // SSE reg / memoryを8つの16ビットのシグニチャとして
            SSEI16x4L = 0x00000060, // SSEレジスタ/メモリ内の4つの16ビット整数が少なく
            SSEI16x2L = 0x00000061, // SSEレジスタ/メモリ内の2つの16ビット整数が低い
            SREGI32x4 = 0x00000062, // SSEは32ビットの4つのシグニチャとして登録されます
            SREGI32L = 0x00000063, // SSEレジスタの32ビットの低位シグネチャ
            SREGI32x2L = 0x00000064, // SSEレジスタの2つの32ビット・セービング
            SSEI32x4 = 0x00000065, // 4つの32ビットシガントとしてのSSE reg / memory
            SSEI32x2L = 0x00000066, // SSEレジスタ/メモリ内の2つの32ビット・セービング
            SREGI64x2 = 0x00000067, // SSEは64ビットの2つのシグニチャとして登録します
            SSEI64x2 = 0x00000068, // SSE reg / memoryを2 64ビットのシグニチャとして
            SREGI64L = 0x00000069, // SSEレジスタの低64ビット・シグニチャ
            EFL = 0x0000006A, // フラグEFLを登録する
            FLAGS8 = 0x0000006B, // フラグ（下位バイト）
            OFFSET = 0x0000006C, // 次のコマンドからの16/32 constオフセット
            BYTEOFFS = 0x0000006D, // 次のcmdからの8ビットsxt constオフセット
            FARCONST = 0x0000006E, // 16：16/16：32絶対アドレス定数
            DESCR = 0x0000006F, // ModRMの16:32記述子
            _1 = 0x00000070, // 即値定数1
            CONST8 = 0x00000071, // 即値8ビット定数
            CONST8_2 = 0x00000072, // 直ちに8ビットのconst、cmdで2番目
            CONST16 = 0x00000073, // 即値16ビット定数
            CONST = 0x00000074, // 即値8/16/32ビット定数
            CONSTL = 0x00000075, // 即値16/32ビット定数
            SXTCONST = 0x00000076, // 即値の8ビット符号拡張 - サイズに変換
            CR = 0x00000077, // Regの制御レジスタ
            CR0 = 0x00000078, // 制御レジスタCR0
            DR = 0x00000079, // Regのデバッグレジスタ
            FST = 0x0000007A, // FPUステータスレジスタ
            FCW = 0x0000007B, // FPU制御レジスタ
            MXCSR = 0x0000007C, // SSEメディアコントロールとステータスレジスタ
            SVEXF32x4 = 0x0000007D, // VEXのSSE regを4 32ビット浮動小数点数として
            SVEXF32L = 0x0000007E, // VEXでのSSEの低32ビット浮動小数点
            SVEXF64x2 = 0x0000007F, // 2つの64ビット浮動小数点としてのVEXにおけるSSE reg
            SVEXF64L = 0x00000080, // VEXでSSEの低64ビット浮動小数点
            SVEXI8x16 = 0x00000081, // VEXのSSE regを16ビットの8ビットシングントとして
            SVEXI16x8 = 0x00000082, // 8つの16ビットシガントとしてのVEXにおけるSSE reg
            SVEXI32x4 = 0x00000083, // SSEは4つの32ビットsigintsとしてVEXをregします
            SVEXI64x2 = 0x00000084, // 2つの64ビットsigintsとしてのVEXのSSE reg
            SIMMI8x16 = 0x00000085, // 即値8ビット定数のSSE reg

            ARG = 0x00000086,
            LVAR = 0x00000087,

            // Type modifiers, used for interpretation of contents, only one is allowed.
            MODMASK = 0x000F0000,     // 型修飾子を抽出するマスク
            NONSPEC = 0x00000000,     // 非特定オペランド
            UNSIGNED = 0x00010000,     // 符号なし小数点としてのデコード
            SIGNED = 0x00020000,     // 符号付き小数点としてのデコード
            BINARY = 0x00030000,     // バイナリ（全16進）データとしてデコードする
            BITCNT = 0x00040000,     // ビット数
            SHIFTCNT = 0x00050000,     // シフトカウント
            COUNT = 0x00060000,     // 汎用カウント
            NOADDR = 0x00070000,     // アドレスではありません
            JMPCALL = 0x00080000,     // ジャンプ/コール/リターン先付近
            JMPCALLFAR = 0x00090000,     // ファージャンプ/コール/リターン先
            STACKINC = 0x000A0000,     // 符号なしスタックインクリメント/デクリメント
            PORT = 0x000B0000,     // I / Oポート
            ADDR = 0x000F0000,     // 内部使用

            // Validity markers.
            MEMORY = 0x00100000,   // メモリのみ、regバージョンは異なる
            REGISTER = 0x00200000,   // 登録のみ、memバージョンは異なる
            MEMONLY = 0x00400000,   // レジスタにオペランドがある場合に警告する
            REGONLY = 0x00800000,   // メモリ内のオペランドの場合は警告
            _32BITONLY = 0x01000000,   // 16ビットオペランドの場合に警告する
            NOESP = 0x02000000,   // ESPは許可されていません

            // Miscellaneous options.  
            NOVEXSIZE = 0x04000000,   // 256ビットAVXの常に128ビットSSE
            SHOWSIZE = 0x08000000,   // 常に議論の大きさを紛らわしいものにする
            CHG = 0x10000000,   // 変更されました。古いコンテンツは使用されていません
            UPD = 0x20000000,   // 古いコンテンツを使用して変更されました
            PSEUDO = 0x40000000,   // Pseoudooperand、アセンブラcmdではありません
            NOSEG = 0x80000000,   // セレクタのオフセットを追加しない
        }

        public enum OP : uint
        {
            // Location of operand, only one bit is allowed.
            SOMEREG = 0x000000FF,     // あらゆる種類のレジスタのマスク
            REGISTER = 0x00000001,     // オペランドは汎用レジスタです
            SEGREG = 0x00000002,     // オペランドはセグメントレジスタです
            FPUREG = 0x00000004,     // オペランドはFPUレジスタ
            MMXREG = 0x00000008,     // オペランドはMMXレジスタです
            _3DNOWREG = 0x00000010,     // オペランドは3DNowです！ 登録
            SSEREG = 0x00000020,     // オペランドはSSEレジスタです
            CREG = 0x00000040,     // オペランドはコントロールレジスタ
            DREG = 0x00000080,     // オペランドはデバッグレジスタです
            MEMORY = 0x00000100,     // オペランドはメモリ内にあります
            CONST = 0x00000200,     // オペランドは即値です

            // Additional operand properties.
            PORT = 0x00000400,     // I / Oポートへのアクセスに使用
            OTHERREG = 0x00000800,     // EFLやMXCSRのような特別登録
            INVALID = 0x00001000,     // 無効なオペランドです（mem-onlyのregと同様）。
            PSEUDO = 0x00002000,     // 擬似perand（擬音語ではない）
            MOD = 0x00004000,     // コマンドがオペランドを変更/更新することがあります
            MODREG = 0x00008000,     // メモリ、ただし、reg（POP、MOVSD）を変更
            IMPORT = 0x00020000,     // 別のモジュールからインポートされた値
            SELECTOR = 0x00040000,     // 即値セレクタを含む

            // Additional properties of memory address.
            INDEXED = 0x00080000,     // メモリアドレスにはレジスタが含まれます
            OPCONST = 0x00100000,     // メモリアドレスに定数が含まれています
            ADDR16 = 0x00200000,     // 16ビットメモリアドレス
            ADDR32 = 0x00400000,     // 明示的な32ビットメモリアドレス
        }

        public enum DAMODE {
            MASM = 0,              // MASM組み立て/分解スタイル
            IDEAL = 1,              // 理想的なアセンブル/ディスアセンブルスタイル
            HLA = 2,              // HLAアセンブル/ディスアセンブルスタイル
            ATT = 3,              // AT＆Tディスアセンブルスタイル
        }

        public enum NUM {
            STYLE = 0x0003,          // ヘキサスタイルを抽出するマスク
            STD = 0x0000,          // 123、12345678h、0ABCD1234h
            X = 0x0001,          // 123、0x12345678、0xABCD1234
            OLLY = 0x0002,          // 123.、12345678、0ABCD1234
            LONG = 0x0010,          // 1234hの代わりに00001234h
            DECIMAL = 0x0020,          // DECLIMITの場合は7Bhの代わりに123
        }

        // Disassembling options.
        public enum DA
        {
            TEXT = 0x00000001, // コマンドをテキストとコメントにデコードする
            HILITE = 0x00000002,     // 構文ハイライトの使用
            JZ = 0x00000004,     // JZ、JNZの代わりにJE、JNE
            JC = 0x00000008,     // JC、JAEの代わりにJNC、JB
            DUMP = 0x00000020,     // コマンドを16進テキストにダンプする
            PSEUDO = 0x00000400,     // pseudooperandリスト
        }

        // Disassembling errors.
        public enum DAE
        {
            NOERR = 0x00000000,     // エラーなし
            BADCMD = 0x00000001,     // 認識できないコマンド
            CROSS = 0x00000002,     // コマンドはメモリブロックの最後を交差する
            MEMORY = 0x00000004,     // メモリのみが許可されている場所に登録する
            REGISTER = 0x00000008,     // レジスタのみが許可されているメモリ
            LOCK = 0x00000010,     // LOCKプレフィックスは使用できません
            BADSEG = 0x00000020,     // 無効なセグメントレジスタ
            SAMEPREF = 0x00000040,     // 同じグループの2つのプレフィックス
            MANYPREF = 0x00000080,     // 4つ以上のプレフィックス
            BADCR = 0x00000100,     // 無効なCRレジスタ
            INTERN = 0x00000200,     // 内部エラー
        }

        // Disassembling warnings.
        public enum DAW
        {
            NOWARN = 0x00000000,     // 警告なし
            DATASIZE = 0x00000001,     // 余分なデータサイズプレフィックス
            ADDRSIZE = 0x00000002,     // 不必要なアドレスサイズプレフィックス
            SEGPREFIX = 0x00000004,     // 余分なセグメントオーバーライドプレフィックス
            REPPREFIX = 0x00000008,     // 不必要なREPxxプレフィックス
            DEFSEG = 0x00000010,     // セグメントプレフィックスはデフォルトと一致します
            JMP16 = 0x00000020,     // 16ビットジャンプ、コールまたはリターン
            FARADDR = 0x00000040,     // 遠くへのジャンプまたは呼び出し
            SEGMOD = 0x00000080,     // セグメントレジスタを変更する
            PRIV = 0x00000100,     // 特権コマンド
            IO = 0x00000200,     // I / Oコマンド
            SHIFT = 0x00000400,     // 範囲外のシフト1..31
            LOCK = 0x00000800,     // 有効なLOCKプレフィックスを持つコマンド
            STACK = 0x00001000,     // アラインされていないスタック操作
            NOESP = 0x00002000,     // スタックポインタの疑わしい使用
            RARE = 0x00004000,     // まれな、まれに使用されるコマンド
            NONCLASS = 0x00008000,     // 非標準コードまたは文書化されていないコード
            INTERRUPT = 0x00010000,     // 割り込みコマンド
        }


        // List of prefixes.
        public enum PF : uint
        {
            SEGMASK = 0x0000003F,  // セグメントオーバーライドプレフィックスのマスク
            ES = 0x00000001,  // 0x26、ESセグメントオーバーライド
            CS = 0x00000002,  // 0x2E、CSセグメントの上書き
            SS = 0x00000004,  // 0x36、SSセグメントの上書き
            DS = 0x00000008,  // 0x3E、DSセグメントの上書き
            FS = 0x00000010,  // 0x64、FSセグメントの上書き
            GS = 0x00000020,  // 0x65、GSセグメントの上書き
            DSIZE = 0x00000040,  // 0x66、データサイズオーバーライド
            ASIZE = 0x00000080,  // 0x67、アドレスサイズオーバーライド
            LOCK = 0x00000100,  // 0xF0、バスロック
            REPMASK = 0x00000600,  // 繰り返しプレフィックスのマスク
            REPNE = 0x00000200,  // 0xF2、REPNEプレフィックス
            REP = 0x00000400,  // 0xF3、REP / REPEプレフィックス
            BYTE = 0x00000800,  // cmdexecで使用されるコマンドのサイズビット
            MUSTMASK = D.MUSTMASK,  // 必要なプレフィックス、t_asmmodで使用される
            VEX2 = 0x00010000,  // 2バイトのVEXプレフィックス
            VEX3 = 0x00020000,  // 3バイトのVEXプレフィックス
                                // Useful shortcuts.
            _66 = DSIZE,// SSEプレフィックスの代替名
            F2 = REPNE,
            F3 = REP,
            HINT = (CS | DS),// 分岐ヒントの代替名
            NOTTAKEN = CS,
            TAKEN = DS,
            VEX = (VEX2 | VEX3)
        }

        /// <summary>
        /// ModRMバイトデコード
        /// </summary>
        class t_modrm
        {
            public uint size;                 // SIBとdisp、バイトの合計サイズ
            public t_modrm[] psib = null;     // SIBテーブルへのポインタまたはNULL
            public uint dispsize;             // 変位のサイズまたは存在しない場合は0
            public OP features;                // オペランド機能、OP_xxxのセット
            public REG reg;                    // レジスタインデックスまたはREG_UNDEF
            public SEG defseg;                 // デフォルトのセレクタ（SEG_xxx）
            public byte[] scale = new byte[NREG];// メモリアドレス内のレジスタのスケール
            public uint aregs;                // アドレスで使用されるレジスタのリスト
            public REG basereg;                // ベースまたはREG_UNDEFとして使用されるレジスタ
            public string ardec="";// アドレスの一部のレジスタ、INTEL fmt
            public string aratt="";// アドレスの一部のレジスタ、AT&T fmt
        }

        /// <summary>
        /// 80x86コマンドの説明
        /// </summary>
        struct t_bincmd
        {
            public string name;                 // このコマンドのシンボリック名 
            public D cmdtype;              // コマンドの機能、D_xxxのセット
            public DX exttype;              // より多くの機能、DX_xxxのセット
            public uint length;               // メインコードの長さ（ModRM / SIBの前）
            public uint mask;                 // コマンドの最初の4バイトのマスク
            public uint code;                 // マスクされたバイトをこれと比較する
            public uint postbyte;             // ポストバイト
            public B[] arg;                     // 引数の型、B_xxxの集合

            public t_bincmd(string name, D cmdtype, DX exttype, uint length, uint mask, uint code, uint postbyte, params B[] args)
            {
                this.cmdtype = cmdtype;this.exttype = exttype;this.length = length;this.mask = mask;this.code = code;this.postbyte = postbyte;
                this.name = name;arg = args;
            }
        }

        /// <summary>
        /// 逆アセンブラ構成
        /// </summary>
        protected struct t_config
        {
            public DAMODE disasmmode;           // メインスタイル、DAMODE_xxxの1つ
            public NUM memmode;              // アドレスの定数部分、NUM_xxx
            public NUM jmpmode;              // ジャンプ先/通話先、NUM_xxx
            public NUM binconstmode;         // 2進定数、NUM_xxx
            public NUM constmode;            // 数値定数、NUM_xxx
            public bool lowercase;            // 小文字の表示を強制する
            public bool tabarguments;         // ニーモニックと引数のタブ
            public bool extraspace;           // 引数間の余分なスペース
            public bool useretform;           // RETNの代わりにRETを使用する
            public bool shortstringcmds;      // 短い形式の文字列コマンドを使用する
            public bool putdefseg;            // リスティングのデフォルトセグメントを表示する
            public bool showmemsize;          // 常にメモリサイズを表示する
            public bool shownear;             // NEAR修飾子を表示する
            public bool ssesizemode;          // SSEオペランドのサイズをデコードする方法
            public bool jumphintmode;         // ジャンプヒントをデコードする方法
            public int sizesens;             // サイズに敏感なニーモニックをデコードする方法
            public bool simplifiedst;         // FPUスタックのトップをデコードする方法
            public bool hiliteoperands;       // オペランドを強調表示する
        }

        /// <summary>
        /// 逆アセンブルされたオペランドの説明
        /// </summary>
        public struct t_operand
        {
            public OP features;                  // オペランド機能、OP_xxxのセット
            public B arg;                       // オペランドタイプ、B_xxxのセット
            public int optype;                 // DEC_INT、DEC_FLOATまたはDEC_UNKNOWN
            public uint opsize;                 // データの合計サイズ、バイト
            public uint granularity;            // 要素のサイズ（opsize exc。MMX / SSE）
            public REG reg;                    // REG_xxx（POPでもESP）またはREG_UNDEF
            public uint uses;                 // 使用された規制のリスト（アドレスにはない！）
            public uint modifies;             // 変更されたregsのリスト（addrにない！）
            // メモリアドレスの説明。
            public SEG seg;                    // セレクタ（SEG_xxx）
            public byte[] scale;     // メモリアドレス内のレジスタのスケール
            public uint aregs;                // アドレスで使用されるレジスタのリスト
            public uint opconst;              // アドレスの定数または定数部分
            public uint opconstsize;            //定数のサイズ
            public uint selector;             // 遠方ジャンプ/コールの即値セレクタ
            public int varnum;//正：引数，負：ローカル変数　の番号
            // テキストのデコード。
            public string text;                // テキストにデコードされたオペランド

            public static t_operand New()
            {
                t_operand op = new t_operand();
                op.features = 0;
                op.arg = 0;
                op.optype = 0;
                op.opsize = op.granularity = 0;
                op.reg = REG.UNDEF;
                op.uses = 0;
                op.modifies = 0;
                op.seg = SEG.UNDEF;
                op.scale = new byte[NREG];
                op.aregs = 0;
                op.opconst = 0;
                op.opconstsize = 0;
                op.selector = 0;
                op.text = "";
                return op;
            }

            /// <summary>
            /// 符号付き定数
            /// </summary>
            public int SignedOpconst
            {
                get
                {
                    switch (opconstsize > 0 ? opconstsize : granularity)
                    {
                        case 4:
                            return (int)opconst;
                        case 2:
                            return (short)opconst;
                        case 1:
                            return (sbyte)opconst;
                        default:
                            throw new ArgumentOutOfRangeException("opconstsizeが不正");
                    }
                }
            }

            /// <summary>
            /// メモリ内に指定レジスタが存在
            /// REG=UNDEFの場合はANY
            /// </summary>
            /// <param name="reg"></param>
            /// <returns></returns>
            public bool IsRegIncludedMemory(REG reg = REG.UNDEF)
            {
                if ((features & OP.MEMORY) == 0) return false;
                if (reg == REG.UNDEF)
                    return (features & OP.INDEXED) != 0 || this.reg != REG.UNDEF;
                else
                    return scale[(int)reg] > 0 || this.reg == reg;
            }

            /// <summary>
            /// メモリ外に指定レジスタが存在
            /// REG=UNDEFの場合はANY
            /// </summary>
            /// <param name="reg"></param>
            /// <returns></returns>
            public bool IsRegNotIncludedMemory(REG reg = REG.UNDEF)
            {
                if ((features & OP.MEMORY) != 0) return false;
                if (reg == REG.UNDEF) return this.reg != REG.UNDEF;
                else return this.reg == reg;
            }

            /// <summary>
            /// レジスタのみ
            /// </summary>
            public bool IsRegister
                => (features & OP.SOMEREG) == OP.REGISTER;

            /// <summary>
            /// レジスタのみ
            /// </summary>
            public bool HasRegister
                => (features & OP.REGISTER) != 0;
        }

        public interface IDisasm
        {
            uint ip { get; set; }
        }

        /// <summary>
        /// ディスアセンブルされたコマンド
        /// 
        /// 使用されるレジスタは、結果を生成するために内容が必要なレジスタです。
        /// 変更されたレジスタは、値が変更されたレジスタです。
        /// たとえば、コマンドMOV EAX、[EBX + ECX] はEBXとECXを使用し、EAXを変更します。
        /// コマンドADD ESI、EDIはESIとEDIを使用し、ESIを変更します。
        /// </summary>
        public struct t_disasm:IDisasm
        {
            public uint ip { get; set; }         // 最初のコマンドバイトのアドレス
            public uint size;            // コマンドの全長、バイト
            public D cmdtype;              // コマンドのタイプ、D_xxx
            public DX exttype;              // より多くの機能、DX_xxxのセット
            public PF prefixes;             // プレフィックスのリスト、PF_xxxのセット
            public uint nprefix;              // プレフィックスの数（SSE2を含む）
            public int memfixup;             // 最初の4バイトフィックスアップのオフセットまたは-1
            public int immfixup;             // 2番目の4バイトフィックスアップのオフセット、または-1
            public DAE errors;                 // DAE_xxxのセット
            public DAW warnings;               // DAW_xxxのセット
            public uint uses;                 // 使用されるレジスタのリスト
            public uint  modifies;             // 変更されたレジスタのリスト
            public uint memconst;             // メモリアドレスまたは0の定数
            public uint stackinc;             // ENTER / RETN / RETFのデータサイズ
            public t_operand[] op;             // オペランド
            public string dump;                // コマンドの16進数のダンプ
            public string result;              // 完全に解読されたコマンドをテキストとして
            public string mask;                // 結果を強調表示するマスク
            public string cmdname;
            public int masksize;               // 結果に対応するマスクの長さ

            public byte[] dumpDatas;

            public D cmdtypeKind => cmdtype & D.CMDTYPE;

            public static t_disasm New(uint ip) 
            {
                t_disasm da = new t_disasm();
                da.ip = ip;
                da.memfixup = da.immfixup = -1;
                da.errors = DAE.NOERR;
                da.warnings = DAW.NOWARN;
                da.uses = 0;
                da.modifies = 0;
                da.memconst = 0;
                da.stackinc = 0;
                if (da.op == null)
                    da.op = new t_operand[NOPERAND];
                for (int i = 0; i < NOPERAND; i++)
                {
                    da.op[i] = t_operand.New();
                }
                da.mask = "";
                da.masksize = 0;
                da.result = da.dump = "";
                da.dumpDatas = null;
                return da;
            }
            
            /// <summary>
            /// パラメータ変数の取得試行
            /// </summary>
            /// <param name="reg"></param>
            /// <param name="ops"></param>
            /// <param name="opconst"></param>
            /// <returns></returns>
            public bool TryGetParamaterOPIndex(REG reg, out int opindex)
            {
                int indexReg = (int)reg;
                int index = 0;
                foreach (var targetop in op.TakeWhile(t => t.arg != B.NONE))
                {
                    if ((targetop.features & OP.OPCONST) != 0 && targetop.scale[indexReg] == 1)
                    {
                        opindex = index;
                        return true;
                    }
                    index++;
                }
                opindex = default(int);
                return false;
            }

            /// <summary>
            /// CMD REG, CONSTを試行し，CONSTのOPを取得
            /// REG=UNDEFの場合はANY
            /// </summary>
            /// <param name="reg"></param>
            /// <param name="opconst"></param>
            /// <returns></returns>
            public bool TryGetREG_CONST(REG reg, out t_operand outop)
            {
                if ((reg != REG.UNDEF && op[0].reg == reg || reg == REG.UNDEF && (op[0].features & OP.SOMEREG) == OP.REGISTER)
                    && (op[0].features & OP.MEMORY) == 0 && (op[1].features & OP.CONST) != 0 && (op[1].features & OP.MEMORY) == 0)
                {
                    outop = op[1];
                    return true;
                }
                outop = default(t_operand);
                return false;
            }

            //ToString用
            private static Disassembler ToStringDisasm;
            static t_disasm()=>ToStringDisasm = new Disassembler();

            public override string ToString()
            {
                if (result == "" && dumpDatas != null)
                {
                    IntPtr unmanagedArea = IntPtr.Zero;
                    try
                    {
                        uint maxsize = Math.Max(size, 0x10);
                        unmanagedArea = Marshal.AllocHGlobal((int)maxsize);
                        Marshal.Copy(dumpDatas, 0, unmanagedArea, (int)size);
                        byte* cmd = (byte*)unmanagedArea.ToPointer();
                        ToStringDisasm.Disasm(cmd, maxsize, ip, out var textda, DA.TEXT);
                        result = textda.result;
                    }
                    finally
                    {
                        if (unmanagedArea != IntPtr.Zero)
                            Marshal.FreeHGlobal(unmanagedArea);
                    }
                }
                return $"{ip:X8}: {result}";
            }
        }
    }
}
