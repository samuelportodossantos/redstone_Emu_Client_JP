using RedStoneLib.Packets;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using static Anantashesha.Disassemble2.DisasmblerHeader;

namespace Anantashesha.Disassemble2
{
    unsafe class Disassembler
    {
        struct t_addrdec
        {
            public SEG defseg;
            public string descr;
            public t_addrdec(SEG seg, string str)
            { defseg = seg; descr = str; }
        }

        static string[,] regname = new string[3, 9] {
            { "AL", "CL", "DL", "BL", "AH", "CH", "DH", "BH", "R8" },
            { "AX", "CX", "DX", "BX", "SP", "BP", "SI", "DI", "R16" },
            { "EAX","ECX","EDX","EBX","ESP","EBP","ESI","EDI","R32" } };

        static string[] segname = new string[8] {
            "ES","CS","SS","DS","FS","GS","SEG?","SEG?" };

        static string[] sizename = new string[11] {
            "(0-BYTE)", "BYTE", "WORD", "(3-BYTE)",
            "DWORD", "(5-BYTE)", "FWORD", "(7-BYTE)",
            "QWORD", "(9-BYTE)", "TBYTE" };

        static t_addrdec[] addr16 = new t_addrdec[8]
        {
            new t_addrdec(SEG.DS,"BX+SI"),  new t_addrdec( SEG.DS,"BX+DI" ),
            new t_addrdec(SEG.SS,"BP+SI"),  new t_addrdec(SEG.SS,"BP+DI" ),
            new t_addrdec(SEG.DS,"SI" ),    new t_addrdec(SEG.DS,"DI" ),
            new t_addrdec(SEG.SS,"BP" ),    new t_addrdec(SEG.DS,"BX" )
        };

        static t_addrdec[] addr32 = new t_addrdec[8]
        {
            new t_addrdec( SEG.DS,"EAX" ),  new t_addrdec( SEG.DS,"ECX" ),
            new t_addrdec( SEG.DS,"EDX" ),  new t_addrdec( SEG.DS,"EBX" ),
            new t_addrdec( SEG.SS,"" ),     new t_addrdec( SEG.SS,"EBP" ),
            new t_addrdec( SEG.DS,"ESI" ),  new t_addrdec( SEG.DS,"EDI" )
        };

        static bool showmemsize = true;

        /// <summary>
        /// Results of disassembling
        /// </summary>
        public struct DisasmResults
        {
            public int ip;           // インストラクションポインタ
            public string dump;        // コマンドの16進ダンプ
            public string result;      // 逆アセンブルされたコマンド
            public string comment;     // 短いコメント
            public C cmdtype;        // C_xxxの1つ
            public DEC memtype;        // メモリ内のアドレス指定された変数のタイプ
            public int indexed;        // レジスタが含まれたアドレス
            public long jmpconst;     // 定数ジャンプアドレス
            public int jmptable;     // スイッチテーブルの可能なアドレス
            public int adrconst;     // アドレスの定数部分
            public long immconst;     // 即値
            public int zeroconst;      // ゼロ定数を含むかどうか
            public int fixupoffset;    // 32ビットのフィックスアップの可能なオフセット
            public int fixupsize;      // フィックスアップの可能な総サイズまたは0
            public DAE error;          // コマンドを逆アセンブル中にエラーが発生
            public DAW warnings;       // DAW_xxxの組み合わせ
        }

        public enum DisasmMode
        {
            DISASM_SIZE = 0,// コマンドサイズのみ
            DISASM_DATA = 1,// サイズと分析データのみ
            DISASM_FILE = 3,// Disassembly：シンボルなし
            DISASM_CODE = 4,// Disassembly：完全
        }

        /// <summary>
        /// Warnings issued by Disasm():
        /// </summary>
        [Flags]
        public enum DAW
        {
            NOWAR = 0x0000,
            FARADDR = 0x0001,// コマンドは"FAR" JMP、CALLまたはRETN
            SEGMENT = 0x0002,// コマンドはセグメントレジスタをロード
            PRIV = 0x0004,// 特権コマンド
            IO = 0x0008,// I/Oコマンド
            SHIFT = 0x0010,// 範囲外の定数シフト1..31
            PREFIX = 0x0020,// 余分なプレフィックス
            LOCK = 0x0040,// コマンドにLOCK接頭辞が付きます
            STACK = 0x0080,// アラインされていないスタック操作
            DANGER95 = 0x1000,// 実行されるとWin95を混乱させるかもしれない
            DANGEROUS = 0x3000,// 実行されるとOSが混乱することがあります
        }

        // Errors detected during command disassembling.
        public enum DAE
        {
            NOERR = 0,   // エラーなし
            BADCMD = 1,  // 認識できないコマンド
            CROSS = 2,   // コマンドはメモリブロックの終わりを越えます
            BADSEG = 3,  // 未定義セグメントレジスタ
            MEMORY = 4,  // レジスタ where メモリのみが許可されている
            REGISTER = 5,// メモリ where レジスタのみが許可されている
            INTERN = 6,  // 内部エラー
        }

        public enum DEC
        {
            UNKNOWN = 0x00, // 不明なタイプ
            BYTE = 0x01,    // バイトとしてアクセス
            WORD = 0x02,    // 短いものとしてアクセス
            NEXTDATA = 0x03,// コードまたはデータの後続バイト
            DWORD = 0x04,   // 長い間アクセスされました
            FLOAT4 = 0x05,  // 浮動小数点としてアクセス
            FWORD = 0x06,   // ディスクリプタ/ロングポインタとしてアクセス
            FLOAT8 = 0x07,  // ダブルとしてアクセス
            QWORD = 0x08,   // 8バイト整数としてアクセス
            FLOAT10 = 0x09, // long doubleとしてアクセス
            TBYTE = 0x0A,   // 10バイトの整数としてアクセスされる
            STRING = 0x0B,  // ゼロで終了するASCII文字列
            UNICODE = 0x0C, // ゼロ終了UNICODE文字列
            _3DNOW = 0x0D,  // 3Dnowオペランドとしてアクセス
            BYTESW = 0x11,  // 切り替えのためのバイトインデックスとしてアクセスされる
            NEXTCODE = 0x13,// コマンドの次のバイト
            COMMAND = 0x1D, // コマンドの最初のバイト
            JMPDEST = 0x1E, // ジャンプ先
            CALLDEST = 0x1F,// CALL（そしておそらくジャンプする）目的地
            PROCMASK = 0x60,// 手順分析
            PROC = 0x20,    // プロシージャの開始
            PBODY = 0x40,   // プロシージャの本体
            PEND = 0x60,    // プロシージャの終了
            CHECKED = 0x80, // バイトを分析した
        }

        int C_TYPEMASK = 0xF0;// コマンドタイプのマスク

        public enum C : int
        {
            CMD = 0x00,             // 通常の指示
            PSH = 0x10,             // 1wordのPUSH命令
            POP = 0x20,             // 1wordのPOP命令
            MMX = 0x30,             // MMX命令
            FLT = 0x40,             // FPU命令
            JMP = 0x50,             // JUMP命令
            JMC = 0x60,             // 条件付きJUMP命令
            CAL = 0x70,             // CALL命令
            RET = 0x80,             // RET命令
            FLG = 0x90,             // システムフラグを変更する
            RTF = 0xA0,             // C_JMPとC_FLG　同時
            REP = 0xB0,             // REPxxプレフィックス付き命令
            PRI = 0xC0,             // 特権命令
            DAT = 0xD0,             // データ（アドレス）ダブルワード
            NOW = 0xE0,             // 3DNow！ 命令
            BAD = 0xF0,             // 認識できないコマンド
            RARE = 0x08,           // まれなコマンド、まれにプログラムで使用される
            SIZEMASK = 0x07,           // MMXデータサイズまたは特殊フラグ
            EXPL = 0x01,             // （非MMX）明示的なメモリサイズを指定する
        }

        public enum C_DANGER
        {
            DANGER95 = 0x01,           // Win95 / 98ではコマンドが危険です
            DANGER = 0x03,           // コマンドはどこでも危険です
            DANGERLOCK = 0x07,           // 危険なLOCKプレフィックス
        }

        public const byte MAXCMDSIZE = 16;       // Maximal length of 80x86 command
        public const byte MAXCALSIZE = 8;       // Max length of CALL without prefixes
        public const byte NMODELS = 8;       // Number of assembler search models

        public const byte INT3 = 0xCC;    // Code of 1-byte breakpoint
        public const byte NOP = 0x90;    // Code of 1-byte NOP command
        public const uint TRAPFLAG = 0x00000100;    // Trap flag in CPU flag register

        public const byte REG_EAX = 0;         // Indexes of general-purpose registers
        public const byte REG_ECX = 1;         // in t_reg.
        public const byte REG_EDX = 2;
        public const byte REG_EBX = 3;
        public const byte REG_ESP = 4;
        public const byte REG_EBP = 5;
        public const byte REG_ESI = 6;
        public const byte REG_EDI = 7;

        /// <summary>
        /// Indexes of segment/selector registers
        /// </summary>
        enum SEG : byte
        {
            UNDEF = 0,
            ES = 1,
            CS = 2,
            SS = 3,
            DS = 4,
            FS = 5,
            GS = 6,
        }

        CmdData* mycmddata;

        // 逆アセンブラの作業変数
        int datasize = 4;     // データサイズ（1,2,4バイト）
        int addrsize = 4;     // アドレスのサイズ（2または4バイト）
        SEG segprefix;          // セグメントオーバーライドプレフィックスまたはSEG_UNDEF
        bool hasrm;              // コマンドにModR/Mバイトがある
        bool hassib;             // コマンドにSIBバイトがある
        int dispsize;           // 変位のサイズ（存在する場合）
        int immsize;            // 即値のサイズ（存在する場合）
        DAE softerror;          // 重大ではない逆アセンブラエラー
        int addcomment;         // オペランドのコメント値

        // Disasmの入力パラメータのコピー
        byte* cmd;      // バイナリデータへのポインタ
        byte* pfixup;   // 可能なフィックスアップへのポインタまたはNULL
        int size, srcsize;      // 残りのコマンドバッファのサイズ
        DisasmMode Mode;        // 逆アセンブリモード（DISASM_xxx）
        DisasmResults Da;     // 逆アセンブル結果へのポインタ
        int srcip;

        //Argument
        bool Ideal;// 理想的なデコードモードを強制する

        public static void DisasmAll(string fname)
        {
            using (FileStream fs = new FileStream(fname, FileMode.Open, FileAccess.Read))
            using (PacketReader br = new PacketReader(fs))
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

                int ip = (int)(NtHeader.OptionalHeader.ImageBase + br.BaseStream.Position);
                var asm = new Disassembler(br.ReadBytes((int)NtHeader.OptionalHeader.SizeOfCode), ip, DisasmMode.DISASM_CODE);
                System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
                sw.Start();
                while (asm.size>0)
                {
                    var da = asm.Disasm();
                    
                    /*if (da.error != DAE.NOERR)
                    {

                    }*/
                    //Console.WriteLine($"{da.ip:X8}: {da.dump,-25}{da.result,-50}{da.comment}");
                }
                sw.Stop();
                Console.WriteLine($"{sw.Elapsed.TotalMilliseconds}[ms]");
            }
        }

        public Disassembler(byte[] src, int ip, DisasmMode mode, bool ideal = false)
        {
            size = srcsize = src.Length;
            var unmanagedArea = Marshal.AllocHGlobal(size);
            Marshal.Copy(src, 0, unmanagedArea, size);
            cmd = (byte*)unmanagedArea.ToPointer();
            Mode = mode;
            Ideal = ideal;
            srcip = ip;
            mycmddata = GetCmdDataPtr();
        }
        
        void InitWorkingVariable()
        {
            datasize = addrsize = 4;
            segprefix = SEG.UNDEF;
            hasrm = hassib = false;
            dispsize = immsize = addcomment = 0;
            softerror = DAE.NOERR;
        }

        public DisasmResults Disasm()
        {
            int sizesens = 2;// サイズに敏感なニーモニックをデコードする方法
            byte* src = cmd;
            InitWorkingVariable();
            Da = new DisasmResults()
            {
                ip = srcip,
                comment = "",
                cmdtype = C.CMD,
                dump = "",
                result = "",
                error = DAE.NOERR,
                warnings = DAW.NOWAR
            };

            int lockprefix = 0;// Non-zero if lock prefix present
            int repprefix = 0; // REPxxx prefix or 0

            bool isprefix = false;

            bool repeated = false;
            byte firstPrefix = 0;

            // 正しい80x86コマンドは、理論上、異なるプレフィックスグループに属するプレフィックスを4つまで含むことができる。
            // これにより、コマンドの可能な最大サイズがMAXCMDSIZE = 16バイトに制限されます。
            // この制限を維持するために、Disasm() が同じグループから2番目のプレフィックスを検出すると、
            // シーケンスの最初のプレフィックスを疑似コマンドとしてフラッシュします。
            while (size > 0)
            {
                // prefixがいくつかあると仮定します
                isprefix = true;
                switch (*cmd)
                {
                    case 0x26:
                        if (segprefix == SEG.UNDEF) segprefix = SEG.ES;
                        else repeated = true; break;
                    case 0x2E:
                        if (segprefix == SEG.UNDEF) segprefix = SEG.CS;
                        else repeated = true; break;
                    case 0x36:
                        if (segprefix == SEG.UNDEF) segprefix = SEG.SS;
                        else repeated = true; break;
                    case 0x3E:
                        if (segprefix == SEG.UNDEF) segprefix = SEG.DS;
                        else repeated = true; break;
                    case 0x64:
                        if (segprefix == SEG.UNDEF) segprefix = SEG.FS;
                        else repeated = true; break;
                    case 0x65:
                        if (segprefix == SEG.UNDEF) segprefix = SEG.GS;
                        else repeated = true; break;
                    case 0x66:
                        if (datasize == 4) datasize = 2;
                        else repeated = true; break;
                    case 0x67:
                        if (addrsize == 4) addrsize = 2;
                        else repeated = true; break;
                    case 0xF0:
                        if (lockprefix == 0) lockprefix = 0xF0;
                        else repeated = true; break;
                    case 0xF2:
                        if (repprefix == 0) repprefix = 0xF2;
                        else repeated = true; break;
                    case 0xF3:
                        if (repprefix == 0) repprefix = 0xF3;
                        else repeated = true; break;
                    default: isprefix = false; break;
                }
                if (!isprefix || repeated)
                    break;// 複数のprefixまたは重複prefix

                if (Mode >= DisasmMode.DISASM_FILE) Da.dump += (*cmd).ToString("X2");

                if (firstPrefix == 0) firstPrefix = *cmd;
                cmd++; srcip++; size--;
            }

            // 重複prefix
            // シーケンスから最初のprefixをフラッシュします。
            if (repeated)
            {
                if (Mode >= DisasmMode.DISASM_FILE)
                {
                    Da.dump = firstPrefix.ToString("X2");// 最初にダンプされたprefixだけを残す
                    string pname;
                    switch (firstPrefix)
                    {
                        case 0x26: pname = SEG.ES.ToString(); break;
                        case 0x2E: pname = SEG.CS.ToString(); break;
                        case 0x36: pname = SEG.SS.ToString(); break;
                        case 0x3E: pname = SEG.DS.ToString(); break;
                        case 0x64: pname = SEG.FS.ToString(); break;
                        case 0x65: pname = SEG.GS.ToString(); break;
                        case 0x66: pname = "DATASIZE"; break;
                        case 0x67: pname = "ADDRSIZE"; break;
                        case 0xF0: pname = "LOCK"; break;
                        case 0xF2: pname = "REPNE"; break;
                        case 0xF3: pname = "REPE"; break;
                        default: pname = "?"; break;
                    }
                    Da.result = $"PREFIX {pname}:";
                    Da.comment = "Superfluous prefix";
                }
                Da.warnings |= DAW.PREFIX;
                if (lockprefix != 0) Da.warnings |= DAW.LOCK;
                Da.cmdtype = C.RARE;

                return Da;
            }

            // ロックプレフィックスが利用可能である場合、残りのコマンドのデコードに影響を与えないので、
            // それを表示して忘れてしまいます。
            if (lockprefix != 0)
            {
                if (Mode >= DisasmMode.DISASM_FILE) Da.result = $"LOCK {Da.result}";
                Da.warnings |= DAW.LOCK;
            }

            // （使用可能な場合）コマンドの最初の3バイトをフェッチし、
            // コマンドテーブルにrepeat prefixとfindコマンドを追加します。
            int code = 0;
            if (size > 0) code |= cmd[0];
            if (size > 1) code |= (cmd[1] << 0x08);
            if (size > 2) code |= (cmd[2] << 0x10);

            if (repprefix != 0)// RER/REPE/REPNE とみなされる
            {
                code = (code << 8) | repprefix;// コマンドの一部
            }

            bool is3dnow = false;
            CmdData* pd = mycmddata;
            for (; pd->Mask != 0; pd++)
            {
                if (((code ^ pd->Code) & pd->Mask) != 0) continue;
                if (Mode >= DisasmMode.DISASM_FILE &&
                (pd->Arg1 == MSO || pd->Arg1 == MDE || pd->Arg2 == MSO || pd->Arg2 == MDE))
                    continue;// 文字列コマンドの短い形式を検索する
                break;
            }

            if ((pd->Type & (C)C_TYPEMASK) == C.NOW)
            {
                // 3DNow！ コマンドは追加の検索が必要です。
                is3dnow = true;
                int j = Get3dnowsuffix();
                if (j < 0)
                {
                    Da.error = DAE.CROSS;
                }
                else
                {
                    for (; pd->Mask != 0; pd++)
                    {
                        if (((code ^ pd->Code) & pd->Mask) != 0) continue;
                        if (((pd->Code >> 0x10) & 0xFF) == j) break;
                    }
                }
            }
            if (pd->Mask == 0)
            {   // コマンドが見つかりません
                Da.cmdtype = C.BAD;
                if (size < 2) Da.error = DAE.CROSS;
                else Da.error = DAE.BADCMD;
            }
            else
            {   // コマンドを認識し、デコードします。
                Da.cmdtype = pd->Type;
                int cxsize = datasize;// カウンタとして使用されるECXのデフォルトサイズ
                if (segprefix == SEG.FS || segprefix == SEG.GS || lockprefix != 0)
                    Da.cmdtype |= C.RARE;             // これらのプレフィックスはまれです
                if (pd->Bits == PR)
                    Da.warnings |= DAW.PRIV;          // 特権コマンド（リング0）
                else if (pd->Bits == WP)
                    Da.warnings |= DAW.IO;

                // I/OコマンドWin32プログラムは通常スタックをDWORDに整列させようとするため、
                // 通常INC ESP（44）とDEC ESP（4C）は実際のコードには表示されません。
                // また、ADD ESP、immおよびSUB ESP、
                // imm（81、C4、imm32; 83、C4、imm8; 81、EC、imm32; 83、EC、imm8）もチェックする。
                if (cmd[0] == 0x44 || cmd[0] == 0x4C || (size >= 3 && (cmd[0] == 0x81 || cmd[0] == 0x83) &&
                  (cmd[1] == 0xC4 || cmd[1] == 0xEC) && (cmd[2] & 0x03) != 0))
                {
                    Da.warnings |= DAW.STACK;
                    Da.cmdtype |= C.RARE;
                }
                // MOV SEG、...（8E ...）でも警告します。 Win32はフラットモードで動作します。
                if (cmd[0] == 0x8E) Da.warnings |= DAW.SEGMENT;
                // オペコードが2バイトの場合は、コマンドを調整します。
                if (pd->Len == 2)
                {
                    if (size == 0) Da.error = DAE.CROSS;
                    else
                    {
                        if (Mode >= DisasmMode.DISASM_FILE) Da.dump += cmd[0].ToString("X2");
                        cmd++; srcip++; size--;
                    }
                }
                if (size == 0) Da.error = DAE.CROSS;

                // いくつかのコマンドは非標準データサイズを特徴とするか、
                // またはデータサイズを選択するビットを有する。
                if ((pd->Bits & WW) != 0 && (cmd[0] & WW) == 0)
                    datasize = 1;                      // コマンドのビットWが0に設定されている
                else if ((pd->Bits & W3) != 0 && (cmd[0] & W3) == 0)
                    datasize = 1;                      // ビットWの別の位置
                else if ((pd->Bits & FF) != 0)
                    datasize = 2;

                // 強制Word（2バイト）サイズデータサイズ（8 / 16ビットまたはCWD / CDQのような32ビット）
                // に依存するニーモニック、またはいくつかの異なるニーモニック（JNZ / JNEなど）
                // を持つコマンドもあります。
                // 最初のケースは '＆'（ニーモニックはオペランドサイズに依存する）または 
                // '$'（アドレスサイズに依存する）のいずれかでマークされます。
                // 後者の場合、特別なマーカーはなく、逆アセンブラはメインニーモニックを選択します。
                if (Mode >= DisasmMode.DISASM_FILE)
                {
                    int mnemosize;
                    char[] name;
                    if (pd->Name[0] == '&') mnemosize = datasize;
                    else if (pd->Name[0] == '$') mnemosize = addrsize;
                    else mnemosize = 0;
                    if (mnemosize != 0)
                    {
                        int i = 0;
                        name = new char[100];
                        foreach (var c in pd->Name)
                        {
                            if (c == ':')
                            {   // 16/32ニーモニック間のセパレータ
                                if (mnemosize == 4) i = 0;
                                else break;
                            }
                            else if (c == '*')
                            {   // 'W','D',またはnoneによって代替
                                if (mnemosize == 4 && sizesens != 2) name[i++] = 'D';
                                else if (mnemosize != 4 && sizesens != 0) name[i++] = 'W';
                            }
                            else name[i++] = c;
                        }
                    }
                    else
                    {
                        name = pd->Name.TakeWhile(t => t != ',').ToArray();
                    }
                    if (repprefix != 0)
                    {
                        var splited = string.Concat(name).Split(' ');
                        string prestr = splited[0], mainstr = splited[1];
                        Da.result += $"{prestr}:{mainstr}";
                    }
                    else Da.result += string.Concat(name);
                }

                // オペランドをデコードする
                //（明示的に - コマンドでエンコードされ、暗黙的に - ムーモニックに存在する、または仮定される 
                // - コマンドによって使用または変更される）。
                // 想定されるオペランドは、すべての明示的および暗黙的オペランドの後にとどまる必要があります。
                // 最大3つのオペランドを使用できます。
                byte arg;
                for (int operand = 0; operand < 3; operand++)
                {
                    if (Da.error != DAE.NOERR) break;  // エラー - 継続する意味がない

                    // commandにsrcとdstの両方が含まれている場合、通常は次のステップで上書きされるため、
                    // dstをコメント化する必要はありません。
                    // グローバルな追加がこれを処理します。
                    // ただし、デコードルーチンはこのフラグを無視することがあります。
                    if (operand == 0 && pd->Arg2 != NNN && pd->Arg2 < PSEUDOOP)
                        addcomment = 0;
                    else
                        addcomment = 1;
                    // Get type of next argument.
                    if (operand == 0) arg = pd->Arg1;
                    else if (operand == 1) arg = pd->Arg2;
                    else arg = pd->Arg3;
                    if (arg == NNN) break;  // オペランドがもうない

                    // arg >= PSEUDOOPの引数はオペランドとみなされ、
                    // 逆アセンブルされた結果では表示されないため、デリミタは必要ありません。
                    if ((Mode >= DisasmMode.DISASM_FILE) && arg < PSEUDOOP)
                    {
                        if (operand == 0) Da.result += ' ';
                        else Da.result += ',';
                    }
                    // コマンドの次のオペランドをデコードし、分析し、コメントに記入する。
                    switch (arg)
                    {
                        case REG:                      // Integer register in Reg field
                            if (size < 2) Da.error = DAE.CROSS;
                            else DecodeRG(cmd[1] >> 3, datasize, REG);
                            hasrm = true; break;
                        case RCM:                      // Integer register in command byte
                            DecodeRG(cmd[0], datasize, RCM); break;
                        case RG4:                      // Integer 4-byte register in Reg field
                            if (size < 2) Da.error = DAE.CROSS;
                            else DecodeRG(cmd[1] >> 3, 4, RG4);
                            hasrm = true; break;
                        case RAC:                      // Accumulator (AL/AX/EAX, implicit)
                            DecodeRG(REG_EAX, datasize, RAC); break;
                        case RAX:                      // AX (2-byte, implicit)
                            DecodeRG(REG_EAX, 2, RAX); break;
                        case RDX:                      // DX (16-bit implicit port address)
                            DecodeRG(REG_EDX, 2, RDX); break;
                        case RCL:                      // Implicit CL register (for shifts)
                            DecodeRG(REG_ECX, 1, RCL); break;
                        case RS0:                      // Top of FPU stack (ST(0))
                            DecodeST(0, 0); break;
                        case RST:                      // FPU register (ST(i)) in command byte
                            DecodeST(cmd[0], 0); break;
                        case RMX:                      // MMX register MMx
                            if (size < 2) Da.error = DAE.CROSS;
                            else DecodeMX(cmd[1] >> 3);
                            hasrm = true; break;
                        case R3D:                      // 3DNow! register MMx
                            if (size < 2) Da.error = DAE.CROSS;
                            else DecodeNR(cmd[1] >> 3);
                            hasrm = true; break;
                        case MRG:                      // Memory/register in ModRM byte
                        case MRJ:                      // Memory/reg in ModRM as JUMP target
                        case MR1:                      // 1-byte memory/register in ModRM byte
                        case MR2:                      // 2-byte memory/register in ModRM byte
                        case MR4:                      // 4-byte memory/register in ModRM byte
                        case MR8:                      // 8-byte memory/MMX register in ModRM
                        case MRD:                      // 8-byte memory/3DNow! register in ModRM
                        case MMA:                      // Memory address in ModRM byte for LEA
                        case MML:                      // Memory in ModRM byte (for LES)
                        case MM6:                      // Memory in ModRm (6-byte descriptor)
                        case MMB:                      // Two adjacent memory locations (BOUND)
                        case MD2:                      // Memory in ModRM byte (16-bit integer)
                        case MB2:                      // Memory in ModRM byte (16-bit binary)
                        case MD4:                      // Memory in ModRM byte (32-bit integer)
                        case MD8:                      // Memory in ModRM byte (64-bit integer)
                        case MDA:                      // Memory in ModRM byte (80-bit BCD)
                        case MF4:                      // Memory in ModRM byte (32-bit float)
                        case MF8:                      // Memory in ModRM byte (64-bit float)
                        case MFA:                      // Memory in ModRM byte (80-bit float)
                        case MFE:                      // Memory in ModRM byte (FPU environment)
                        case MFS:                      // Memory in ModRM byte (FPU state)
                        case MFX:                      // Memory in ModRM byte (ext. FPU state)
                            DecodeMR(arg); break;
                        case MMS:                      // Memory in ModRM byte (as SEG:OFFS)
                            DecodeMR(arg);
                            Da.warnings |= DAW.FARADDR; break;
                        case RR4:                      // 4-byte memory/register (register only)
                        case RR8:                      // 8-byte MMX register only in ModRM
                        case RRD:                      // 8-byte memory/3DNow! (register only)
                            if ((cmd[1] & 0xC0) != 0xC0) softerror = DAE.REGISTER;
                            DecodeMR(arg); break;
                        case MSO:                      // Source in string op's ([ESI])
                            DecodeSO(); break;
                        case MDE:                      // Destination in string op's ([EDI])
                            DecodeDE(); break;
                        case MXL:                      // XLAT operand ([EBX+AL])
                            DecodeXL(); break;
                        case IMM:                      // Immediate data (8 or 16/32)
                        case IMU:                      // Immediate unsigned data (8 or 16/32)
                            if ((pd->Bits & SS) != 0 && (cmd[0] & 0x02) != 0)
                                DecodeIM(1, datasize, arg);
                            else
                                DecodeIM(datasize, 0, arg);
                            break;
                        case VXD:                      // VxD service (32-bit only)
                            DecodeVX(); break;
                        case IMX:                      // Immediate sign-extendable byte
                            DecodeIM(1, datasize, arg); break;
                        case C01:                      // Implicit constant 1 (for shifts)
                            DecodeC1(); break;
                        case IMS:                      // Immediate byte (for shifts)
                        case IM1:                      // Immediate byte
                            DecodeIM(1, 0, arg); break;
                        case IM2:                      // Immediate word (ENTER/RET)
                            DecodeIM(2, 0, arg);
                            if ((Da.immconst & 0x03) != 0) Da.warnings |= DAW.STACK;
                            break;
                        case IMA:                      // Immediate absolute near data address
                            DecodeIA(); break;
                        case JOB:                      // Immediate byte offset (for jumps)
                            DecodeRJ(1, srcip + 2); break;
                        case JOW:                      // Immediate full offset (for jumps)
                            DecodeRJ(datasize, srcip + datasize + 1); break;
                        case JMF:                      // Immediate absolute far jump/call addr
                            DecodeJF();
                            Da.warnings |= DAW.FARADDR; break;
                        case SGM:                      // Segment register in ModRM byte
                            if (size < 2) Da.error = DAE.CROSS;
                            DecodeSG(cmd[1] >> 3); hasrm = true; break;
                        case SCM:                      // Segment register in command byte
                            DecodeSG(cmd[0] >> 3);
                            if ((Da.cmdtype & (C)C_TYPEMASK) == C.POP) Da.warnings |= DAW.SEGMENT;
                            break;
                        case CRX:                      // Control register CRx
                            if ((cmd[1] & 0xC0) != 0xC0) Da.error = DAE.REGISTER;
                            DecodeCR(cmd[1]); break;
                        case DRX:                      // Debug register DRx
                            if ((cmd[1] & 0xC0) != 0xC0) Da.error = DAE.REGISTER;
                            DecodeDR(cmd[1]); break;
                        case PRN:                      // Near return address (pseudooperand)
                            break;
                        case PRF:                      // Far return address (pseudooperand)
                            Da.warnings |= DAW.FARADDR; break;
                        case PAC:                      // Accumulator (AL/AX/EAX, pseudooperand)
                            DecodeRG(REG_EAX, datasize, PAC); break;
                        case PAH:                      // AH (in LAHF/SAHF, pseudooperand)
                        case PFL:                      // Lower byte of flags (pseudooperand)
                            break;
                        case PS0:                      // Top of FPU stack (pseudooperand)
                            DecodeST(0, 1); break;
                        case PS1:                      // ST(1) (pseudooperand)
                            DecodeST(1, 1); break;
                        case PCX:                      // CX/ECX (pseudooperand)
                            DecodeRG(REG_ECX, cxsize, PCX); break;
                        case PDI:                      // EDI (pseudooperand in MMX extentions)
                            DecodeRG(REG_EDI, 4, PDI); break;
                        default:
                            Da.error = DAE.INTERN;        // Unknown argument type
                            break;
                    }
                }
                // コマンドにフィックスアップが含まれている可能性があるかどうかを確認します。
                if (pfixup != null && Da.fixupsize > 0) Da.fixupoffset = (int)(pfixup - src);
                // セグメントプレフィックスとアドレスサイズプレフィックスは、
                // メモリにアクセスしないコマンドには不要です。
                // このような場合は、コマンドを稀に分析に役立ててください。
                if (Da.memtype == DEC.UNKNOWN && (segprefix != SEG.UNDEF || (addrsize != 4 && pd->Name[0] != '$')))
                {
                    Da.warnings |= DAW.PREFIX;
                    Da.cmdtype |= C.RARE;
                }
                // 32ビットプログラムでは、16ビットアドレッシングはほとんどありません。
                // このような場合は、コマンドを分析に役立たないものとしてマークしてください。
                if (addrsize != 4) Da.cmdtype |= C.RARE;
            }
            // 3DNow!のサフィックス コマンドは即値のバイト定数と仮定することによって最もよく説明されます。
            if (is3dnow)
            {
                if (immsize != 0) Da.error = DAE.BADCMD;
                else immsize = 1;
            }
            // 正しいか間違って、デコードされたコマンド。 今すぐダンプしてください。
            if (Da.error != 0)
            {   // コマンドのハードエラーを検出
                if (Mode >= DisasmMode.DISASM_FILE)
                    Da.result = "???";
                if (Da.error == DAE.BADCMD && (cmd[0] == 0x0F || cmd[0] == 0xFF) && size > 0)
                {
                    if (Mode >= DisasmMode.DISASM_FILE) Da.dump += $"{cmd[0]:X2}";
                    cmd++; size--;
                }
                if (size > 0)
                {
                    if (Mode >= DisasmMode.DISASM_FILE) Da.dump += $"{cmd[0]:X2}";
                    cmd++; size--;
                }
            }
            else
            {   // ハードエラーなし、ダンプコマンド
                if (Mode >= DisasmMode.DISASM_FILE)
                {
                    Da.dump += $"{cmd[0]:X2}"; cmd++;
                    if (hasrm) { Da.dump += $"{cmd[0]:X2}"; cmd++; }
                    if (hassib) { Da.dump += $"{cmd[0]:X2}"; cmd++; }
                    if (dispsize != 0)
                    {
                        Da.dump += " ";
                        for (int i = 0; i < dispsize; i++)
                        {
                            Da.dump += $"{cmd[0]:X2}"; cmd++;
                        }
                    }
                    if (immsize != 0)
                    {
                        Da.dump += " ";
                        for (int i = 0; i < immsize; i++)
                        {
                            Da.dump += $"{cmd[0]:X2}"; cmd++;
                        }
                    }
                }
                else cmd += 1 + (hasrm ? 1 : 0) + (hassib ? 1 : 0) + dispsize + immsize;
                size -= 1 + (hasrm ? 1 : 0) + (hassib ? 1 : 0) + dispsize + immsize;
            }
            // コマンドが危険なものでないことを確認
            if (Mode >= DisasmMode.DISASM_DATA)
            {
                foreach (var pdan in dangerous)
                {
                    if (((code ^ pdan.Code) & pdan.Mask) != 0)
                        continue;
                    if (pdan.Type == (C)C_DANGER.DANGERLOCK && lockprefix == 0)
                        break;                         // LOCKプレフィックスなしで無害なコマンド
                    // 危険なコマンド！
                    if (pdan.Type == (C)C_DANGER.DANGER95) Da.warnings |= DAW.DANGER95;
                    else Da.warnings |= DAW.DANGEROUS;
                    break;
                }
            }
            if (Da.error == 0 && softerror != 0) Da.error = softerror; // エラーですが、まだコマンドを表示しています
            if (Mode >= DisasmMode.DISASM_FILE)
            {
                if (Da.error != DAE.NOERR)
                {
                    switch (Da.error)
                    {
                        case DAE.CROSS:
                            Da.comment = "Command crosses end of memory block"; break;
                        case DAE.BADCMD:
                            Da.comment = "Unknown command"; break;
                        case DAE.BADSEG:
                            Da.comment = "Undefined segment register"; break;
                        case DAE.MEMORY:
                            Da.comment = "Illegal use of register"; break;
                        case DAE.REGISTER:
                            Da.comment = "Memory address not allowed"; break;
                        case DAE.INTERN:
                            Da.comment = "Internal OLLYDBG error"; break;
                        default:
                            Da.comment = "Unknown error";
                            break;
                    }
                }
                else if ((Da.warnings & DAW.PRIV) != 0)
                    Da.comment = "Privileged command";
                else if ((Da.warnings & DAW.IO) != 0)
                    Da.comment = "I/O command";
                else if ((Da.warnings & DAW.FARADDR) != 0)
                {
                    if ((Da.cmdtype & (C)C_TYPEMASK) == C.JMP)
                        Da.comment = "Far jump";
                    else if ((Da.cmdtype & (C)C_TYPEMASK) == C.CAL)
                        Da.comment = "Far call";
                    else if ((Da.cmdtype & (C)C_TYPEMASK) == C.RET)
                        Da.comment = "Far return";
                }
                else if ((Da.warnings & DAW.SEGMENT) != 0)
                    Da.comment = "Modification of segment register";
                else if ((Da.warnings & DAW.SHIFT) != 0)
                    Da.comment = "Shift constant out of range 1..31";
                else if ((Da.warnings & DAW.PREFIX) != 0)
                    Da.comment = "Superfluous prefix";
                else if ((Da.warnings & DAW.LOCK) != 0)
                    Da.comment = "LOCK prefix";
                else if ((Da.warnings & DAW.STACK) != 0)
                    Da.comment = "Unaligned stack operation";
            }

            srcip = Da.ip + (srcsize - size);
            srcsize = size;
            return Da;
        }


        // 1,2または4バイトの汎用整数レジスタの名前を逆アセンブルし、要求され、
        // 使用可能であれば、その内容をダンプします。
        // パラメータタイプによって、一部のオペランドタイプの内容のデコードが変更されます。
        void DecodeRG(int index, int datasize, int type)
        {
            int sizeindex;
            if (Mode < DisasmMode.DISASM_DATA) return; // デコードする必要はありません
            index &= 0x07;
            if (datasize == 1)
                sizeindex = 0;
            else if (datasize == 2)
                sizeindex = 1;
            else if (datasize == 4)
                sizeindex = 2;
            else { Da.error = DAE.INTERN; return; }
            if (Mode >= DisasmMode.DISASM_FILE && type < PSEUDOOP)// pseudooperandではない
                Da.result += regname[sizeindex, index];
        }

        // 80ビット浮動小数点レジスタの名前を逆アセンブルし、可能であれば、その内容をダンプします。
        void DecodeST(int index, int pseudoop)
        {
            if (Mode < DisasmMode.DISASM_FILE) return;// デコードする必要はありません
            index &= 0x07;
            if (pseudoop == 0) Da.result += $"ST({index})";
        }

        // 64ビットMMXレジスタの名前を逆アセンブルします。
        void DecodeMX(int index)
        {
            if (Mode < DisasmMode.DISASM_FILE) return;// デコードする必要はありません
            index &= 0x07;
            Da.result += $"MM{index}";
        }

        // 64ビット "3DNow！"の名前を逆アセンブルします。 利用可能であれば、その内容をダンプします。
        void DecodeNR(int index) => DecodeMX(index);

        // サービス機能は、MASMまたはIdeal形式の有効なメモリアドレスを逆アセンブルした文字列に追加します。
        // パラメータ： defseg - 指定されたレジスタの組み合わせに対するデフォルトのセグメント、
        //              descr  - 完全にデコードされたレジスタの一部
        //              offset - アドレスの定数部分
        //              dsize  - バイト単位のデータサイズ
        // グローバルフラグ 'symbolic'が設定されている場合、
        // 関数はオフセットをいくつかのラベルの名前としてデコードしようとします。                                                            
        void Memadr(SEG defseg, string descr, int offset, int dsize)
        {
            int i;
            SEG seg;
            string s = "";
            if (Mode < DisasmMode.DISASM_FILE || descr == null)
                return;// デコードの必要性または可能性なし
            if (segprefix != SEG.UNDEF) seg = segprefix; else seg = defseg;
            if (Ideal) Da.result += "[";

            // ディスアセンブラでメモリオペランドのサイズが省略されることがあります。
            // すなわち、フラグshowmemsizeは0でなければならず、ビットCをタイプする。
            // EXPLは0でなければならない（このビットは明示的なオペランド・サイズが必要であることを意味する）、
            // コマンドのタイプはCであってはならない
            // MMXまたはC.
            // NOW（ビットC.EXPLはこれらの場合異なる意味を持つため）
            // それ以外の場合は、正確なサイズを指定する必要があります。                                  
            if (showmemsize || (Da.cmdtype & (C)C_TYPEMASK) == C.MMX ||
              (Da.cmdtype & (C)C_TYPEMASK) == C.NOW || (Da.cmdtype & C.EXPL) != 0)
            {
                if (dsize < sizename.Length)
                    Da.result += $"{sizename[dsize]} {(!Ideal ? "PTR " : "")}";
                else
                    Da.result += $"({dsize}-BYTE) {(!Ideal ? "PTR " : "")}";
            }
            if (seg != SEG.UNDEF)
                Da.result += $"{seg}:";
            if (!Ideal) Da.result += "[";
            Da.result += descr;
            if (offset == 0)
            {
                if (descr == "") Da.result += "0";
            }
            else
            {
                if (Mode >= DisasmMode.DISASM_CODE)
                    i = Decodeaddress(offset, s, null);
                else i = 0;
                if (i > 0)
                {   // シンボリック形式でデコードされたオフセット
                    if (descr != "") Da.result += "+";
                    Da.result += s;
                }
                else if (offset < 0 && offset > -16384 && descr != "")
                    Da.result += $"-{-offset:X}";
                else
                {
                    if (descr != "") Da.result += "+";
                    Da.result += offset.ToString("X");
                }
            }
            Da.result += "]";
        }

        // ModRM / SIBバイトからメモリ/レジスタを逆アセンブルし、
        // 可能であればメモリのアドレスと内容をダンプします。
        void DecodeMR(int type)
        {
            int j, memonly = 0;
            SEG seg = SEG.UNDEF;
            bool inmemory;
            int sib;
            int dsize, regsize, addr;
            string s;
            if (size < 2)// メモリブロック外のModRMバイト
            {
                Da.error = DAE.CROSS; return;
            }
            hasrm = true;
            dsize = regsize = datasize; // アドレス指定されたレジスタ/メモリのデフォルトサイズModMフィールドのレジスタが許可されます。

            // ModMのアドレス指定されたメモリまたはレジスタのサイズと種類はコマンドサイズに影響を与えません。
            // registerを使用すると、optypeが不正確になり、後で修正する必要があります。
            // ModフィールドとMフィールドだけを残す
            int c = cmd[1] & 0xC7;
            if (Mode >= DisasmMode.DISASM_DATA)
            {
                if ((c & 0xC0) == 0xC0)
                    inmemory = false;// Register operand
                else
                    inmemory = true;// Memory operand
                switch (type)
                {
                    case MRG:                        // Memory/register in ModRM byte
                        if (inmemory)
                        {
                            if (datasize == 1) Da.memtype = DEC.BYTE;
                            else if (datasize == 2) Da.memtype = DEC.WORD;
                            else Da.memtype = DEC.DWORD;
                        }
                        break;
                    case MRJ:                        // Memory/reg in ModRM as JUMP target
                        if (datasize != 2 && inmemory)
                            Da.memtype = DEC.DWORD;
                        if (Mode >= DisasmMode.DISASM_FILE)
                            Da.result += "NEAR ";
                        break;
                    case MR1:                        // 1-byte memory/register in ModRM byte
                        dsize = regsize = 1;
                        if (inmemory) Da.memtype = DEC.BYTE; break;
                    case MR2:                        // 2-byte memory/register in ModRM byte
                        dsize = regsize = 2;
                        if (inmemory) Da.memtype = DEC.WORD; break;
                    case MR4:                        // 4-byte memory/register in ModRM byte
                    case RR4:                        // 4-byte memory/register (register only)
                        dsize = regsize = 4;
                        if (inmemory) Da.memtype = DEC.DWORD; break;
                    case MR8:                        // 8-byte memory/MMX register in ModRM
                    case RR8:                        // 8-byte MMX register only in ModRM
                        dsize = 8;
                        if (inmemory) Da.memtype = DEC.QWORD; break;
                    case MRD:                        // 8-byte memory/3DNow! register in ModRM
                    case RRD:                        // 8-byte memory/3DNow! (register only)
                        dsize = 8;
                        if (inmemory) Da.memtype = DEC._3DNOW; break;
                    case MMA:                        // Memory address in ModRM byte for LEA
                        memonly = 1; break;
                    case MML:                        // Memory in ModRM byte (for LES)
                        dsize = datasize + 2; memonly = 1;
                        if (datasize == 4 && inmemory)
                            Da.memtype = DEC.FWORD;
                        Da.warnings |= DAW.SEGMENT;
                        break;
                    case MMS:                        // Memory in ModRM byte (as SEG:OFFS)
                        dsize = datasize + 2; memonly = 1;
                        if (datasize == 4 && inmemory)
                            Da.memtype = DEC.FWORD;
                        if (Mode >= DisasmMode.DISASM_FILE)
                            Da.result += "FAR ";
                        break;
                    case MM6:                        // Memory in ModRM (6-byte descriptor)
                        dsize = 6; memonly = 1;
                        if (inmemory) Da.memtype = DEC.FWORD; break;
                    case MMB:                        // Two adjacent memory locations (BOUND)
                        dsize = (Ideal ? datasize : datasize * 2); memonly = 1; break;
                    case MD2:                        // Memory in ModRM byte (16-bit integer)
                    case MB2:                        // Memory in ModRM byte (16-bit binary)
                        dsize = 2; memonly = 1;
                        if (inmemory) Da.memtype = DEC.WORD; break;
                    case MD4:                        // Memory in ModRM byte (32-bit integer)
                        dsize = 4; memonly = 1;
                        if (inmemory) Da.memtype = DEC.DWORD; break;
                    case MD8:                        // Memory in ModRM byte (64-bit integer)
                        dsize = 8; memonly = 1;
                        if (inmemory) Da.memtype = DEC.QWORD; break;
                    case MDA:                        // Memory in ModRM byte (80-bit BCD)
                        dsize = 10; memonly = 1;
                        if (inmemory) Da.memtype = DEC.TBYTE; break;
                    case MF4:                        // Memory in ModRM byte (32-bit float)
                        dsize = 4; memonly = 1;
                        if (inmemory) Da.memtype = DEC.FLOAT4; break;
                    case MF8:                        // Memory in ModRM byte (64-bit float)
                        dsize = 8; memonly = 1;
                        if (inmemory) Da.memtype = DEC.FLOAT8; break;
                    case MFA:                        // Memory in ModRM byte (80-bit float)
                        dsize = 10; memonly = 1;
                        if (inmemory) Da.memtype = DEC.FLOAT10; break;
                    case MFE:                        // Memory in ModRM byte (FPU environment)
                        dsize = 28; memonly = 1; break;
                    case MFS:                        // Memory in ModRM byte (FPU state)
                        dsize = 108; memonly = 1; break;
                    case MFX:                        // Memory in ModRM byte (ext. FPU state)
                        dsize = 512; memonly = 1; break;
                    default:                         // Operand is not in ModM!
                        Da.error = DAE.INTERN;
                        break;
                }
            }
            addr = 0;
            // "ModM/SIB"アドレスを解読するには多くの可能性があります。
            // 最初の可能性は、ModM - 汎用、MMXまたは3DNow！に登録することです。
            if ((c & 0xC0) == 0xC0)
            {    // デコードレジスタオペランド
                if (type == MR8 || type == RR8)
                    DecodeMX(c);                     // MMXレジスタ
                else if (type == MRD || type == RRD)
                    DecodeNR(c);                     // 3DNow!レジスタ
                else
                    DecodeRG(c, regsize, type); // 汎用レジスタ
                if (memonly != 0)
                    softerror = DAE.MEMORY;          // メモリのみが許可されているレジスタ
                return;
            }
            // 次の可能性：16ビット・アドレッシング・モード、
            // まれに32ビット・フラット・モデルであるが、依然としてプロセッサーによってサポートされている。
            // ここではSIBバイトは使用されません。
            if (addrsize == 2)
            {
                if (c == 0x06)
                {   // 即値アドレスの特別なケース
                    dispsize = 2;
                    if (size < 4) Da.error = DAE.CROSS;// メモリブロックの外側のDisp16
                    else if (Mode >= DisasmMode.DISASM_DATA)
                    {
                        Da.adrconst = addr = *(ushort*)(cmd + 2);
                        if (addr == 0) Da.zeroconst = 1;
                        seg = SEG.DS;
                        Memadr(seg, "", addr, dsize);
                    }
                }
                else
                {
                    Da.indexed = 1;
                    if ((c & 0xC0) == 0x40)
                    {   // 8ビット符号付きdisp
                        if (size < 3) Da.error = DAE.CROSS;
                        else addr = cmd[2] & 0xFFFF;
                        dispsize = 1;
                    }
                    else if ((c & 0xC0) == 0x80)
                    {   // 16ビット符号なしdisp
                        if (size < 4) Da.error = DAE.CROSS;
                        else addr = *(ushort*)(cmd + 2);
                        dispsize = 2;
                    }
                    if (Mode >= DisasmMode.DISASM_DATA && Da.error == DAE.NOERR)
                    {
                        Da.adrconst = addr;
                        if (addr == 0) Da.zeroconst = 1;
                        seg = addr16[c & 0x07].defseg;
                        Memadr(seg, addr16[c & 0x07].descr, addr, dsize);
                    }
                }
            }
            // 次の可能性：即値の32ビットアドレス
            else if (c == 0x05)
            {   // 即値アドレスの特別なケース
                dispsize = 4;
                if (size < 6)
                    Da.error = DAE.CROSS;             // Disp32 outside the memory block
                else if (Mode >= DisasmMode.DISASM_DATA)
                {
                    Da.adrconst = addr = (int)*(uint*)(cmd + 2);
                    if (pfixup == null) pfixup = cmd + 2;
                    Da.fixupsize += 4;
                    if (addr == 0) Da.zeroconst = 1;
                    seg = SEG.DS;
                    Memadr(seg, "", addr, dsize);
                }
            }
            // 次の可能性：SIBバイト付き32ビットアドレス
            else if ((c & 0x07) == 0x04)
            {   // SIBアドレス
                sib = cmd[2]; hassib = true;
                s = "";
                if (c == 0x04 && (sib & 0x07) == 0x05)
                {
                    dispsize = 4;// ベースなしの即値アドレス
                    if (size < 7)
                        Da.error = DAE.CROSS;// メモリブロックの外側のDisp32
                    else
                    {
                        Da.adrconst = addr = (int)*(uint*)(cmd + 3);
                        if (pfixup == null) pfixup = cmd + 3;
                        Da.fixupsize += 4;
                        if (addr == 0) Da.zeroconst = 1;
                        if ((sib & 0x38) != 0x20)
                        {   // インデックスレジスタが存在
                            Da.indexed = 1;
                            if (type == MRJ) Da.jmptable = addr;
                        }
                        seg = SEG.DS;
                    }
                }
                else
                {   // ベースと、最終的にはdisp
                    if ((c & 0xC0) == 0x40)
                    {   // 8-bit disp
                        dispsize = 1;
                        if (size < 4) Da.error = DAE.CROSS;
                        else
                        {
                            Da.adrconst = addr = cmd[3];
                            if (addr == 0) Da.zeroconst = 1;
                        }
                    }
                    else if ((c & 0xC0) == 0x80)
                    {     // 32-bit disp
                        dispsize = 4;
                        if (size < 7)
                            Da.error = DAE.CROSS;         // Disp32 outside the memory block
                        else
                        {
                            Da.adrconst = addr = (int)*(uint*)(cmd + 3);
                            if (pfixup == null) pfixup = cmd + 3;
                            Da.fixupsize += 4;
                            if (addr == 0) Da.zeroconst = 1;

                            // ほとんどのコンパイラはジャンプテーブル（スイッチ）に対処するために
                            // [index * 4 + displacement]タイプのアドレスを使用します。 
                            // しかし、完全性のために、スケール1または4、ベースまたは両方を含むインデックスを
                            // 含むすべてのケースを許可します。
                            if (type == MRJ) Da.jmptable = addr;
                        }
                    }
                    Da.indexed = 1;
                    j = sib & 0x07;
                    if (Mode >= DisasmMode.DISASM_FILE)
                    {

                        s = regname[2, j];
                        seg = addr32[j].defseg;
                    }
                }
                if ((sib & 0x38) != 0x20)
                {   // Scaled index が存在
                    if ((sib & 0xC0) == 0x40) Da.indexed = 2;
                    else if ((sib & 0xC0) == 0x80) Da.indexed = 4;
                    else if ((sib & 0xC0) == 0xC0) Da.indexed = 8;
                    else Da.indexed = 1;
                }
                if (Mode >= DisasmMode.DISASM_FILE && Da.error == DAE.NOERR)
                {
                    if ((sib & 0x38) != 0x20)
                    {   // Scaled index が存在
                        if (s != "") s += "+";
                        s += addr32[(sib >> 3) & 0x07].descr;
                        if ((sib & 0xC0) == 0x40)
                        {
                            Da.jmptable = 0;// ほとんどがswitch-case
                            s += "*2";
                        }
                        else if ((sib & 0xC0) == 0x80)
                            s += "*4";
                        else if ((sib & 0xC0) == 0xC0)
                        {
                            Da.jmptable = 0;// ほとんどがswitch-case
                            s += "*8";
                        }
                    }
                    Memadr(seg, s, addr, dsize);
                }
            }
            // 最後の可能性：SIBバイトのない32ビットアドレス
            else
            {   // No SIB
                if ((c & 0xC0) == 0x40)
                {
                    dispsize = 1;
                    if (size < 3) Da.error = DAE.CROSS; // メモリブロックの外側のDisp8
                    else
                    {
                        Da.adrconst = addr = cmd[2];
                        if (addr == 0) Da.zeroconst = 1;
                    }
                }
                else if ((c & 0xC0) == 0x80)
                {
                    dispsize = 4;
                    if (size < 6)
                        Da.error = DAE.CROSS;// メモリブロックの外側のDisp32
                    else
                    {
                        Da.adrconst = addr = (int)*(uint*)(cmd + 2);
                        if (pfixup == null) pfixup = cmd + 2;
                        Da.fixupsize += 4;
                        if (addr == 0) Da.zeroconst = 1;
                        if (type == MRJ) Da.jmptable = addr;
                    }
                }
                Da.indexed = 1;
                if (Mode >= DisasmMode.DISASM_FILE && Da.error == DAE.NOERR)
                {
                    seg = addr32[c & 0x07].defseg;
                    Memadr(seg, addr32[c & 0x07].descr, addr, dsize);
                }
            }
        }

        // 暗黙のストリング操作のソースを逆アセンブルし、使用可能であればアドレスと内容をダンプします。
        void DecodeSO()
        {
            if (Mode < DisasmMode.DISASM_FILE) return;        // No need to decode
            if (datasize == 1) Da.memtype = DEC.BYTE;
            else if (datasize == 2) Da.memtype = DEC.WORD;
            else if (datasize == 4) Da.memtype = DEC.DWORD;
            Da.indexed = 1;
            Memadr(SEG.DS, regname[addrsize == 2 ? 1 : 2, REG_ESI], 0, datasize);
        }

        // 暗黙のストリング操作の宛先を逆アセンブルし、使用可能であればアドレスと内容をダンプします。
        // 宛先は常にセグメントESを使用し、この設定は無効にすることはできません。
        void DecodeDE()
        {
            SEG seg;
            if (Mode < DisasmMode.DISASM_FILE) return;        // No need to decode
            if (datasize == 1) Da.memtype = DEC.BYTE;
            else if (datasize == 2) Da.memtype = DEC.WORD;
            else if (datasize == 4) Da.memtype = DEC.DWORD;
            Da.indexed = 1;
            seg = segprefix; segprefix = SEG.ES;     // Fake Memadr by changing segment prefix
            Memadr(SEG.DS, regname[addrsize == 2 ? 1 : 2, REG_EDI], 0, datasize);
            segprefix = seg;                       // Restore segment prefix
        }

        // XLATオペランドをデコードし、使用可能であればアドレスと内容をダンプします。
        void DecodeXL()
        {
            if (Mode < DisasmMode.DISASM_FILE) return;        // No need to decode
            Da.memtype = DEC.BYTE;
            Da.indexed = 1;
            Memadr(SEG.DS, (addrsize == 2 ? "BX+AL" : "EBX+AL"), 0, 1);
        }

        // "size constsize"の即値オペランドをデコードします。
        // sxtがゼロでない場合、バイトオペランドはsxtバイトに符号拡張する必要があります。
        // イミディエート定数の型がこのことを前提とする場合、
        // 小さな負のオペランドが符号付きの負の数として表示されることがあります。
        // ほとんどの場合、即値オペランドはコメントウィンドウに表示されないことに注意してください。
        void DecodeIM(int constsize, int sxt, int type)
        {
            int i;
            string name = "", comment = "";
            immsize += constsize;                    // Allows several immediate operands
            if (Mode < DisasmMode.DISASM_DATA) return;
            int l = 1 + (hasrm ? 1 : 0) + (hassib ? 1 : 0) + dispsize + (immsize - constsize);
            int data = 0;
            if ((int)size < l + constsize)
                Da.error = DAE.CROSS;
            else if (constsize == 1)
            {
                if (sxt == 0) data = cmd[l];
                else data = (sbyte)cmd[l];
                if (type == IMS && ((data & 0xE0) != 0 || data == 0))
                {
                    Da.warnings |= DAW.SHIFT;
                    Da.cmdtype |= C.RARE;
                }
            }
            else if (constsize == 2)
            {
                if (sxt == 0) data = *(ushort*)(cmd + l);
                else data = *(short*)(cmd + l);
            }
            else
            {
                data = *(int*)(cmd + l);
                if (pfixup == null) pfixup = cmd + l;
                Da.fixupsize += 4;
            }
            if (sxt == 2) data &= 0x0000FFFF;
            if (data == 0 && Da.error == 0) Da.zeroconst = 1;
            // コマンドのENTERは、Intelのルールの例外として、2つの即時定数を持っています。
            // 2番目の定数はめったに使われないので、最初の定数がゼロでない場合（通常はそうです）、
            // 検索から除外します。
            if (Da.immconst == 0) Da.immconst = data;
            if (Mode >= DisasmMode.DISASM_FILE && Da.error == DAE.NOERR)
            {
                if (Mode >= DisasmMode.DISASM_CODE && type != IMU)
                    i = Decodeaddress(data, name, comment);
                else
                {
                    i = 0; comment = "";
                }
                if (i != 0)
                {
                    Da.result += name;
                }
                else if (type == IMU || type == IMS || type == IM2 || data >= 0 || data < NEGLIMIT)
                    Da.result += $"{data:X}";
                else
                    Da.result += $"-{-data:X}";
                if (addcomment != 0 && comment != "") Da.comment = comment;
            }
        }

        // VxDサービス名をデコードします（常に4バイト）
        void DecodeVX()
        {
            immsize += 4;// 複数の即時オペランドを許可する
            if (Mode < DisasmMode.DISASM_DATA) return;
            int l = 1 + (hasrm ? 1 : 0) + (hassib ? 1 : 0) + dispsize + (immsize - 4);
            if (size < l + 4)
            {
                Da.error = DAE.CROSS;
                return;
            }
            int data = *(int*)(cmd + l);
            if (data == 0 && Da.error == 0) Da.zeroconst = 1;
            if (Da.immconst == 0)
                Da.immconst = data;
            if (Mode >= DisasmMode.DISASM_FILE && Da.error == DAE.NOERR)
            {
                if ((data & 0x00008000) != 0 && "VxDCall".ToLower() == Da.result.ToLower())
                    Da.result = "VxDJump";
                Da.result += data.ToString("X");
            }
        }

        //暗黙の定数1をデコードします（シフトコマンドで使用されます）。 
        // このオペランドはそれほど重要ではないため、コメントウィンドウには表示されません。
        void DecodeC1()
        {
            if (Mode < DisasmMode.DISASM_DATA) return;
            Da.immconst = 1;
            if (Mode >= DisasmMode.DISASM_FILE) Da.result += "1";
        }

        // 直近の絶対データアドレスをデコードします。
        // このオペランドは、8080互換のコマンドで使用され、
        // メモリからアキュムレータにデータを戻すことができます。
        // ModR/MとSIBのバイトは、IAオペランドのコマンドには現れません。
        void DecodeIA()
        {
            int addr;
            if (size < 1 + addrsize)
            {
                Da.error = DAE.CROSS; return;
            }
            dispsize = (int)addrsize;
            if (Mode < DisasmMode.DISASM_DATA) return;
            if (datasize == 1) Da.memtype = DEC.BYTE;
            else if (datasize == 2) Da.memtype = DEC.WORD;
            else if (datasize == 4) Da.memtype = DEC.DWORD;
            if (addrsize == 2)
                addr = *(ushort*)(cmd + 1);
            else
            {
                addr = (int)*(uint*)(cmd + 1);
                if (pfixup == null) pfixup = cmd + 1;
                Da.fixupsize += 4;
            }
            Da.adrconst = addr;
            if (addr == 0) Da.zeroconst = 1;
            if (Mode >= DisasmMode.DISASM_FILE)
            {
                Memadr(SEG.DS, "", addr, datasize);
            }
        }

        // サイズのオフセットのnextipに相対的にジャンプします。
        void DecodeRJ(int offsize, int nextip)
        {
            int i, addr;
            string s = "";
            if (size < offsize + 1)
            {
                Da.error = DAE.CROSS; return;
            }
            dispsize = offsize;// dispとしてのオフセットを解釈する
            if (Mode < DisasmMode.DISASM_DATA) return;
            if (offsize == 1)
                addr = cmd[1] + nextip;
            else if (offsize == 2)
                addr = *(short*)(cmd + 1) + nextip;
            else
                addr = (int)*(uint*)(cmd + 1) + nextip;
            if (datasize == 2)
                addr &= 0xFFFF;
            Da.jmpconst = addr;
            if (addr == 0) Da.zeroconst = 1;
            if (Mode >= DisasmMode.DISASM_FILE)
            {
                if (offsize == 1) Da.result += "SHORT ";
                if (Mode >= DisasmMode.DISASM_CODE)
                    i = Decodeaddress(addr, s, Da.comment);
                else
                    i = 0;
                if (i == 0)
                    Da.result += $"{addr:X8}";
                else
                    Da.result += s;
                ;
            }
        }

        // 即値 absolute far jump addressをデコードします。
        // フラットモデルでは、そのようなアドレスは使用されません
        // （主にセレクタがコマンドで直接指定されているため）。
        // したがって、シンボルとしてデコードしたりコメントにコメントしたりすることはありません。
        // 値でセレクタを検索できるように、私はそれを直接の定数として解釈します。
        void DecodeJF()
        {
            int addr, seg;
            if (size < 1 + addrsize + 2)
            {
                Da.error = DAE.CROSS; return;
            }
            dispsize = (int)addrsize; immsize = 2;// 些細ではないが解釈が許される
            if (Mode < DisasmMode.DISASM_DATA) return;
            if (addrsize == 2)
            {
                addr = *(ushort*)(cmd + 1);
                seg = *(ushort*)(cmd + 3);
            }
            else
            {
                addr = (int)*(uint*)(cmd + 1);
                seg = *(ushort*)(cmd + 5);
            }
            Da.jmpconst = addr;
            Da.immconst = seg;
            if (addr == 0 || seg == 0) Da.zeroconst = 1;
            if (Mode >= DisasmMode.DISASM_FILE)
            {
                Da.result += $"FAR {seg:X4}:{addr:X8}";
            }
        }

        // デコードセグメントレジスタ
        // フラットモデルでは、このタイプのオペランドはめったにありません。
        void DecodeSG(int index)
        {
            if (Mode < DisasmMode.DISASM_DATA) return;
            index &= 0x07;
            if (index >= 6) softerror = DAE.BADSEG;  // 未定義セグメントレジスタ
            if (Mode >= DisasmMode.DISASM_FILE)
            {
                Da.result += ((SEG)index).ToString();
            }
        }

        // ModR/MバイトのR部分でアドレス指定されたデコード制御レジスタ。
        // このタイプのオペランドは非常にまれです。
        // 制御レジスタの内容は特権レベル0からしかアクセスできないので、
        // ここでそれらをダンプすることはできません。
        void DecodeCR(int index)
        {
            hasrm = true;
            if (Mode >= DisasmMode.DISASM_FILE)
            {
                index = (index >> 3) & 0x07;
                Da.result += crname[index];
            }
        }

        // ModR/MバイトのR部分でアドレス指定されたデバッグレジスタをデコードします。
        // このタイプのオペランドは非常にまれです。
        // CONTEXT構造で利用可能なデバッグレジスタだけをダンプすることができます。
        void DecodeDR(int index)
        {
            hasrm = true;
            if (Mode >= DisasmMode.DISASM_FILE)
            {
                index = (index >> 3) & 0x07;
                Da.result += drname[index];
            }
        }

        /// <summary>
        /// 3DNow！オペランドをスキップし，コマンド接尾辞を抽出します。
        /// このサブルーチンは、cmdがまだ3DNow！の開始点を指していることを前提としています。 
        /// コマンド（すなわち、2バイト0F、0Fのシーケンス）に変換する。
        /// </summary>
        /// <returns>接尾辞を返すか、接尾辞がメモリブロックの外側にある場合は-1を返します。</returns>
        int Get3dnowsuffix()
        {
            int c, sib, offset;
            if (size < 3) return -1;               // Suffix outside the memory block
            offset = 3;
            c = cmd[2] & 0xC7;                     // Leave only Mod and M fields
            // ModMの レジスタ - 汎用、MMX、または 3DNow!
            if ((c & 0xC0) == 0xC0) goto ifend;
            // 16ビットアドレッシングモードでは、ここではSIBバイトは使用されません。
            else if (addrsize == 2)
            {
                if (c == 0x06)                       // Special case of immediate address
                    offset += 2;
                else if ((c & 0xC0) == 0x40)         // 8-bit signed displacement
                    offset++;
                else if ((c & 0xC0) == 0x80)         // 16-bit unsigned displacement
                    offset += 2;
            }
            // Immediate 32-bit address.
            else if (c == 0x05)                    // Special case of immediate address
                offset += 4;
            // 32-bit address with SIB byte.
            else if ((c & 0x07) == 0x04)
            {         // SIB addresation
                if (size < 4) return -1;             // Suffix outside the memory block
                sib = cmd[3]; offset++;
                if (c == 0x04 && (sib & 0x07) == 0x05)
                    offset += 4;                       // Immediate address without base
                else if ((c & 0xC0) == 0x40)         // 8-bit displacement
                    offset += 1;
                else if ((c & 0xC0) == 0x80)         // 32-bit dislacement
                    offset += 4;
            }
            // 32-bit address without SIB byte
            else if ((c & 0xC0) == 0x40)
                offset += 1;
            else if ((c & 0xC0) == 0x80)
                offset += 4; ifend:
            if (offset >= size) return -1; // メモリブロックの外側のサフィックス
            return cmd[offset];
        }

        // 関数は、80x86フラグがコマンドに設定された条件を満たすかどうかをチェックします。
        // 条件が満たされた場合は1を返し、そうでない場合は0を返し、エラーの場合は-1を返します（これは不可能です）。
        int Checkcondition(int code, int flags)
        {
            int cond, temp;
            switch (code & 0x0E)
            {
                case 0:                            // If overflow
                    cond = flags & 0x0800; break;
                case 2:                            // If below
                    cond = flags & 0x0001; break;
                case 4:                            // If equal
                    cond = flags & 0x0040; break;
                case 6:                            // If below or equal
                    cond = flags & 0x0041; break;
                case 8:                            // If sign
                    cond = flags & 0x0080; break;
                case 10:                           // If parity
                    cond = flags & 0x0004; break;
                case 12:                           // If less
                    temp = flags & 0x0880;
                    cond = (temp == 0x0800 || temp == 0x0080) ? 1 : 0; break;
                case 14:                           // If less or equal
                    temp = flags & 0x0880;
                    cond = (temp == 0x0800 || temp == 0x0080 || (flags & 0x0040) != 0) ? 1 : 0; break;
                default: return -1;                // Internal error, not possible!
            }
            if ((code & 0x01) == 0) return cond;
            else return (cond == 0) ? 1 : 0;// Invert condition
        }

        // アドレスをsymb（nsymbバイトの長さ、終端のゼロ文字を含む）にデコードし、
        // その可能な意味をコメントします。
        // 終了ゼロを含まない、symbのバイト数を返します。
        int Decodeaddress(int addr, string symb, string comment)
        {


            // 環境特有のルーチン！ 自分でやって！


            return 0;
        }
    }
}
