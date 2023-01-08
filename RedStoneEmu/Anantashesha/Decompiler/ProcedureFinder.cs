//#define PARALLEL 

using Anantashesha.Decompiler.Disassemble;
using Anantashesha.Decompiler.ProcedureAnalyzers;
using RedStoneLib.Packets;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Anantashesha.Decompiler
{
    class ProcedureFinder:Disassembler
    {
        /// <summary>
        /// MS-DOS ヘッダ 
        /// </summary>
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct IMAGE_DOS_HEADER
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
            public uint NumberOfSymbols;
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
        Dictionary<uint, string> GetSymbols(PacketReader br)
        {
            //デバッグ情報
            Dictionary<uint, string> result = null;
            if (DataDic[6].Size >= Marshal.SizeOf(typeof(IMAGE_DEBUG_DIRECTORY)) && DataDic[6].VirtualAddress < br.BaseStream.Length)
            {
                br.BaseStream.Seek(DataDic[6].VirtualAddress, SeekOrigin.Begin);
                IMAGE_DEBUG_DIRECTORY[] DebugDirectory = br.Reads<IMAGE_DEBUG_DIRECTORY>((int)(DataDic[6].Size / Marshal.SizeOf(typeof(IMAGE_DEBUG_DIRECTORY))));

                //overflow check
                if (DebugDirectory[0].PointerToRawData + Marshal.SizeOf(typeof(IMAGE_COFF_SYMBOLS_HEADER)) > br.BaseStream.Length) return null;

                //coff収集
                br.BaseStream.Seek(DebugDirectory[0].PointerToRawData, SeekOrigin.Begin);
                IMAGE_COFF_SYMBOLS_HEADER CoffHeader = br.ReadStruct<IMAGE_COFF_SYMBOLS_HEADER>();

                //overflow check
                if (DebugDirectory[0].PointerToRawData + CoffHeader.NumberOfSymbols * Marshal.SizeOf(typeof(SYMENT)) > br.BaseStream.Length) return null;
                
                SYMENT[] Syments = br.Reads<SYMENT>((int)CoffHeader.NumberOfSymbols);

                var savePoint = br.BaseStream.Position;

                result = new Dictionary<uint, string>();

                //名前取得
                foreach (var symbol in Syments.Where(t => t.SectionNumber > 0))
                {
                    uint addr = symbol.Addr + NtHeader.OptionalHeader.ImageBase;
                    if (symbol.NameZero == 0)
                    {
                        br.BaseStream.Seek(symbol.NameOffset, SeekOrigin.Current);
                        result[addr] = br.ReadSjis();
                        br.BaseStream.Seek(savePoint, SeekOrigin.Begin);
                    }
                    else
                    {
                        List<byte> strbyte = new List<byte>(BitConverter.GetBytes(symbol.NameZero));
                        strbyte.AddRange(BitConverter.GetBytes(symbol.NameOffset));
                        if (!result.ContainsKey(addr) || result[addr][0] == '.')
                        {
                            result[addr] = Encoding.ASCII.GetString(strbyte.TakeWhile(t => t != 0).ToArray());
                        }
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// サブルーチン
        /// </summary>
        class Subroutine
        {
            public int index;
            public uint StartAddr;
            public string Symbol;
            public t_disasm[] Instruments;

            //呼び出し元
            public List<uint> CallFrom = new List<uint>();

            //呼び出し先
            public List<uint> CallTo;

            //使用静的変数
            public int[] StaticVariables;

            //呼び出し規則
            public CallingConvention CallingConvention;

            public Subroutine(uint startAddr, string symbol, t_disasm[] instruments, List<uint> callTo)
            {
                StartAddr = startAddr;
                Symbol = symbol;
                Instruments = instruments;
                CallTo = callTo;
            }
            
            /// <summary>
            /// 呼び出し元のセット
            /// </summary>
            /// <param name="src"></param>
            public static void SetCallFrom(ref Dictionary<uint, Subroutine> src, Func<t_operand, bool> CheckValidAddress)
            {
                foreach (var startAddr in src.Keys.ToList())
                {
                    foreach (var callto in src[startAddr].CallTo)
                    {
                        if (!src[callto].CallFrom.Contains(startAddr))
                            src[callto].CallFrom.Add(startAddr);
                    }
                }
            }

            public override string ToString()
                => Symbol ?? base.ToString();
        }

        /// <summary>
        /// 静的変数
        /// </summary>
        public class Variable
        {
            public int index;
            public uint Addr;
            public string Symbol;

            //呼び出し元
            public List<int> CallFrom = new List<int>();

            public Variable(uint addr, string symbol)
            {
                Addr = addr;
                Symbol = symbol;
            }

            public Variable() { }

            public override string ToString()
                => Symbol ?? base.ToString();
        }

        IMAGE_DOS_HEADER MsHeader;
        IMAGE_NT_HEADERS NtHeader;
        IMAGE_DATA_DIRECTORY[] DataDic;
        IMAGE_SECTION_HEADER[] SectionHeaders;
        string SourceFileName;
        readonly byte[] codeSrc;
        readonly uint codeBaseIp;

        public ProcedureFinder(string fname) : base()
        {
            SourceFileName = fname;
            using (FileStream fs = new FileStream(fname, FileMode.Open, FileAccess.Read))
            using (PacketReader br = new PacketReader(fs))
            {
                //PEヘッダ取得
                MsHeader = br.ReadStruct<IMAGE_DOS_HEADER>();
                //NTヘッダへ
                br.BaseStream.Seek(MsHeader.e_lfanew, SeekOrigin.Begin);
                //NTヘッダ取得
                NtHeader = br.ReadStruct<IMAGE_NT_HEADERS>();

                int num = (int)NtHeader.OptionalHeader.NumberOfRvaAndSizes;
                //IMG_DATA,SECTION_DATAの取得
                DataDic = br.Reads<IMAGE_DATA_DIRECTORY>(num);
                SectionHeaders = br.Reads<IMAGE_SECTION_HEADER>(num);

                //コード開始に飛ぶ
                br.BaseStream.Seek(NtHeader.OptionalHeader.BaseOfCode, SeekOrigin.Begin);
                codeBaseIp = NtHeader.OptionalHeader.ImageBase + NtHeader.OptionalHeader.BaseOfCode;
                codeSrc = br.ReadBytes((int)NtHeader.OptionalHeader.SizeOfCode);
            }
        }

        //関数取得用
        object getValueLocker = new object();
        T[] getValues<T>(uint targetIp, int num) where T : new()
        {
            T[] res = null;
            lock (getValueLocker)
            {
                using (PacketReader br = new PacketReader(codeSrc))
                {
                    try
                    {
                        br.BaseStream.Seek(targetIp - NtHeader.OptionalHeader.ImageBase - NtHeader.OptionalHeader.BaseOfCode, SeekOrigin.Begin);
                        res = br.Reads<T>(num);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"getValues error:{ex.Message} (0x{targetIp:X8}, {num})");
                    }
                }
            }
            return res;
        }

        /// <summary>
        /// 木の作成
        /// </summary>
        public (BayesianNetwork[] funcs, BayesianNetwork[] vars) MakeTree(int answerSize = 0)
        {
            Dictionary<uint, Subroutine> funcs;
            Dictionary<int, Variable> varDic;
            uint mainAddr, fsize;
            string funcfname, varfname;
            using (FileStream fs = new FileStream(SourceFileName, FileMode.Open, FileAccess.Read))
            using (PacketReader br = new PacketReader(fs))
            {
                fsize = (uint)br.BaseStream.Length;

                //ファイル検索
                funcfname = $"{(answerSize > 0 ? "train" : "ans")}_{fsize}.bayes";
                varfname = $"{(answerSize > 0 ? "train_var" : "ans_var")}_{fsize}.bayes";
                if (File.Exists(funcfname) && File.Exists(varfname))
                {
                    BayesianNetwork[] funcres, varres;
                    Console.Write($"\"{funcfname}\" exist. loading...");
                    using (StreamReader sr = new StreamReader(funcfname, new UTF8Encoding(false)))
                    {
                        funcres = (BayesianNetwork[])new XmlSerializer(typeof(BayesianNetwork[])).Deserialize(sr);
                        Console.WriteLine($" {funcres.Length} functions loaded.");
                    }
                    Console.Write($"\"{varfname}\" exist. loading...");
                    using (StreamReader sr = new StreamReader(varfname, new UTF8Encoding(false)))
                    {
                        varres = (BayesianNetwork[])new XmlSerializer(typeof(BayesianNetwork[])).Deserialize(sr);
                        Console.WriteLine($" {varres.Length} variables loaded.");
                    }
                    return (funcres, varres);
                }

                //Symbol取得
                var symbols = GetSymbols(br);

                //関数取得
                funcs = CollectFuncs(codeSrc, symbols, out var vars, out mainAddr).ToDictionary(t => t.StartAddr, t => t);
                varDic = vars.ToDictionary(t => t.index, t => t);

                //呼び出しセット
                Subroutine.SetCallFrom(ref funcs, CheckImmutable);
            }

            //マップ作成
            Console.Write("Make map in progress... ");
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            sw.Start();

            Dictionary<uint, BayesianNetwork> network = new Dictionary<uint, BayesianNetwork>();

            //新規func
            Dictionary<int, Subroutine> smartFunc = new Dictionary<int, Subroutine>();
            Dictionary<int, int> mappedIndex = new Dictionary<int, int>();//旧→新Index対応表

            //使用する静的変数
            List<int> useStaticVariableIndexes = new List<int>();

            //幅優先探索でレイヤ決める
            Queue<List<uint>> loadQueue = new Queue<List<uint>>();
            List<uint> visitedAddr = new List<uint> { mainAddr };
            Dictionary<uint, int> tmpLayer = new Dictionary<uint, int>();
            loadQueue.Enqueue(new List<uint> { mainAddr });
            int index = 0;
            while (loadQueue.Count > 0)
            {
                var currentLoad = loadQueue.Dequeue();//軌跡
                var currentAddr = currentLoad.Last();//現在アドレス
                var currentFunc = funcs[currentAddr];//現在の関数

                //生成
                if (!network.ContainsKey(currentAddr))
                {
                    network[currentAddr] = new BayesianNetwork(index, currentAddr, currentFunc.Symbol, currentFunc.Instruments.Length);
                    network[currentAddr].Variables = currentFunc.StaticVariables;
                    smartFunc[index] = currentFunc;
                    smartFunc[index].index = index;
                    if (currentFunc.StaticVariables != null) useStaticVariableIndexes.AddRange(currentFunc.StaticVariables);
                    mappedIndex[currentFunc.index] = index;
                    index++;
                }

                //レイヤ更新
                network[currentAddr].Layer = currentLoad.Count() - 1;

                List<int> children = new List<int>();
                foreach (var nextFunc in currentFunc.CallTo.Where(t => !currentLoad.Contains(t)).Select(t => funcs[t]))
                {
                    IEnumerable<uint> lazyMap()
                    {
                        foreach (var node in currentLoad) yield return node;
                        yield return nextFunc.StartAddr;
                    }

                    //子ノード追加
                    children.Add((int)nextFunc.StartAddr);

                    if (!visitedAddr.Contains(nextFunc.StartAddr))
                    {
                        visitedAddr.Add(nextFunc.StartAddr);

                        //未訪問はEnqueue
                        loadQueue.Enqueue(new List<uint>(lazyMap()));

                        //仮レイヤ
                        tmpLayer[nextFunc.StartAddr] = currentLoad.Count();
                    }
                    else if ((network.TryGetValue(nextFunc.StartAddr, out var net) ? net.Layer : tmpLayer[nextFunc.StartAddr]) > currentLoad.Count() - 1)
                    {
                        //訪問済みはLayerの更新目的でEnqueue
                        loadQueue.Enqueue(new List<uint>(lazyMap()));
                    }
                }

                //子ノード
                if (network[currentFunc.StartAddr].Children == null) network[currentFunc.StartAddr].Children = children.ToArray();
            }

            //子ノード更新
            foreach (var addr in network.Keys)
            {
                network[addr].Children = network[addr].Children.Select(adr => smartFunc.Values.First(u => u.StartAddr == (uint)adr).index).ToArray();
            }

            //親ノード更新
            Dictionary<uint, List<int>> parents = new Dictionary<uint, List<int>>();
            foreach (var parentAddr in network.Keys)
            {
                foreach (var childAddr in network[parentAddr].Children.Select(t => smartFunc[t].StartAddr))
                {
                    if (!parents.ContainsKey(childAddr)) parents[childAddr] = new List<int>();
                    parents[childAddr].Add(network[parentAddr].Index);
                }
            }
            foreach(var addr in network.Keys)
            {
                if (addr == mainAddr) network[addr].Parents = new int[0];
                else network[addr].Parents = parents[addr].ToArray();
            }
            sw.Stop();
            Console.WriteLine($"done. {sw.Elapsed.TotalMilliseconds}[ms]");

            //prob決める
            foreach (var addr in network.Keys)
            {
                if (answerSize > 0)
                {
                    network[addr].Probs = new double[answerSize];
                }
                else
                {
                    foreach (var calltoFunc in network[addr].Children.Select(t => smartFunc[t]))
                    {
                        if (network[calltoFunc.StartAddr].Probs == null) network[calltoFunc.StartAddr].Probs = new double[network.Count()];
                        network[calltoFunc.StartAddr].Probs[calltoFunc.index] = 1.0;
                    }
                }
            }
            if (answerSize > 0)
            {
                //既知の関数
                network[mainAddr].Probs[0] = 1.0;
                int[] indexOf11;
                using (StreamReader reader = new StreamReader("main11.b"))
                {
                    indexOf11 = reader.ReadLine().Split(',').Select(t => Convert.ToInt32(t)).ToArray();
                }
                foreach (var main11addr in main11CallDic[mainAddr].Select((v,i)=>new { v,i}))
                {
                    network[main11addr.v].Probs[indexOf11[main11addr.i]] = 1.0;
                }
            }

            //静的変数更新
            List<BayesianNetwork> variableNodes = new List<BayesianNetwork>();
            foreach(var variableIndex in useStaticVariableIndexes.Distinct())
            {
                var variable = varDic[variableIndex];
                variableNodes.Add(new BayesianNetwork(variable.index, variable.Addr, variable.Symbol,
                    variable.CallFrom.Where(t=> mappedIndex.ContainsKey(t)).Select(t=> mappedIndex[t]).ToArray()));//新Indexに更新
            }

            //保存
            Console.Write($"Save to \"{funcfname}\"... ");
            using (StreamWriter writer = new StreamWriter(funcfname, false, new UTF8Encoding(false)))
            {
                new XmlSerializer(typeof(BayesianNetwork[])).Serialize(writer, network.Values.ToArray());
            }
            Console.Write($"Save to \"{varfname}\"... ");
            using (StreamWriter writer = new StreamWriter(varfname, false, new UTF8Encoding(false)))
            {
                new XmlSerializer(typeof(BayesianNetwork[])).Serialize(writer, variableNodes.ToArray());
            }

            if (answerSize == 0)
            {
                //メイン直下１３の関数インデックス
                using (StreamWriter writer = new StreamWriter("main11.b"))
                {
                    writer.WriteLine(string.Join(",", main11CallDic[mainAddr].Select(t => network[t].Index)));
                }
            }
            Console.WriteLine("done.");

            return (network.Values.ToArray(), variableNodes.ToArray());
        }

        /// <summary>
        /// 全て逆コンパイル
        /// </summary>
        /// <param name="fname"></param>
        /// <returns></returns>
        unsafe Dictionary<uint, t_disasm> DisasmAll(Dictionary<uint, int> dataSegmentSize = null)
        {
            List<t_disasm> asms = new List<t_disasm>();

            IntPtr unmanagedArea = Marshal.AllocHGlobal(codeSrc.Length);
            Marshal.Copy(codeSrc, 0, unmanagedArea, codeSrc.Length);
            byte* cmd = (byte*)unmanagedArea.ToPointer();
            uint cmdsize = (uint)codeSrc.Length;

            Console.Write($"{cmdsize} bytes disassembling in progress... ");
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            sw.Start();
            int zeroLength = 0;//コードのゼロの長さ
            for (uint l = 0, ip = codeBaseIp; 0 < cmdsize; cmdsize -= l, ip += l, cmd += l)
            {
                if (dataSegmentSize != null && dataSegmentSize.TryGetValue(ip, out int size))
                {
                    l = (uint)size;
                    continue;
                }
                l = Disasm(cmd, cmdsize, ip, out var da, /*DA.TEXT | DA.DUMP*/ 0);
                for(int i = 0; i < l; i++)
                {
                    if (cmd[i] == 0) zeroLength++;
                    else
                    {
                        zeroLength = 0;
                        break;
                    }
                }
                if (zeroLength > 0x0B)//ゼロ多いとコード終了
                {
                    asms = asms.Where(t => t.ip < ip + l - zeroLength).ToList();
                    break;
                }
                asms.Add(da);
            }
            Dictionary<uint, t_disasm> result = asms.ToDictionary(t => t.ip, t => t);
            sw.Stop();
            Console.WriteLine($"done. {sw.Elapsed.TotalMilliseconds}[ms]");
            Marshal.FreeHGlobal(unmanagedArea);
            return result;
        }

        /// <summary>
        /// アドレス有効チェック
        /// </summary>
        /// <param name="op"></param>
        /// <returns></returns>
        bool CheckImmutable(t_operand op)
        {
            if ((op.features & OP.MEMORY) != 0 || (op.features & OP.SOMEREG) == OP.REGISTER) return false;// memory or reg call
            else if (!IsInCodeArea(op.opconst)) return false;// overflow size
            else return true;
        }
        
        /// <summary>
        /// コード領域内判定
        /// </summary>
        /// <param name="addr"></param>
        /// <returns></returns>
        bool IsInCodeArea(uint addr)
            => addr >= NtHeader.OptionalHeader.ImageBase + NtHeader.OptionalHeader.BaseOfCode &&
            addr < NtHeader.OptionalHeader.ImageBase + NtHeader.OptionalHeader.BaseOfCode + NtHeader.OptionalHeader.SizeOfCode;

        /// <summary>
        /// サブルーチンの収集
        /// </summary>
        /// <param name="codes"></param>
        /// <returns></returns>
        Subroutine[] CollectFuncs(byte[] src, Dictionary<uint, string> symbols, out Variable[] variables, out uint mainAddr)
        {
            var codes = DisasmAll();
            List<Subroutine> funcs = new List<Subroutine>();
            Dictionary<uint, List<bool>> thiscallList = new Dictionary<uint, List<bool>>();//thiscall疑惑の関数アドレス

            //最終ip
            var finalIP = codes.Keys.Max();

            //変数
            Dictionary<uint, Variable> varDic = new Dictionary<uint, Variable>();
            Dictionary<uint, List<uint>> varCallFromFuncAddrs = new Dictionary<uint, List<uint>>();//静的変数呼び出し元
            Dictionary<uint, List<int>> funcCallToVarAddrs = new Dictionary<uint, List<int>>();//関数が呼び出す静的変数

            //DD領域の記録
            Dictionary<uint, int> dataSegmentSize = new Dictionary<uint, int>();

            int progress = 0;

            IntPtr reserved = Marshal.AllocHGlobal(src.Length);
            Marshal.Copy(src, 0, reserved, src.Length);

            object locker = new object();

            //update候補
            Dictionary<uint, int> foundMainDic = new Dictionary<uint, int>();

            //disasm結果から関数収集
            Console.Write("Collect functions... ");
            int curPos = Console.CursorLeft;

            //呼び出しリスト
            uint[] getCallIPs() => codes.Values
                .Where(t => t.cmdtypeKind == D.CALL &&
                CheckImmutable(t.op[0]) &&//only const call
                t.op[0].opconst <= finalIP)//code overflow
                .GroupBy(t=>t.op[0].opconst)//distinct
                .Select(t => t.First().ip).OrderBy(t => t).ToArray();
            var callIPs = getCallIPs();

            //サブルーチンlist作成
            var subroutineStarts = callIPs.Select(t => codes[t].op[0].opconst).Distinct().ToList();

            //収集実態
            void collectProcess(uint startAddr, uint codeip)
            {
                try
                {
                    var func = Finder(reserved, codes, startAddr, finalIP, subroutineStarts, dataSegmentSize, ref thiscallList, out var callTo, out var staticVars, out var foundMain);
                    if (func.Count() > 0)
                    {
                        lock (locker)
                        {
                            //関数
                            string symbol = null;
                            if (symbols != null) symbol = symbols.TryGetValue(startAddr, out string outsymbol) ? outsymbol : null;
                            funcs.Add(new Subroutine(startAddr, symbol, func, callTo));

                            if (foundMain >= 0) foundMainDic[startAddr] = foundMain;

                            //変数
                            foreach (var variable in staticVars)
                            {
                                if (varDic.ContainsKey(variable))
                                {
                                    varCallFromFuncAddrs[variable].Add(startAddr);
                                }
                                else
                                {
                                    symbol = null;
                                    if (symbols != null) symbol = symbols.TryGetValue(variable, out string outsymbol) ? outsymbol : null;
                                    varDic[variable] = new Variable(variable, symbol);
                                    varCallFromFuncAddrs[variable] = new List<uint> { startAddr };
                                }
                            }

                            progress++;
                            if (progress % 100 == 0)
                            {
                                Console.Write($"{(double)progress / codes.Count * 100.0:P4}");
                                Console.SetCursorPosition(curPos, Console.CursorTop);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"{codeip:X8}: {ex.Message}\r\n{ex.StackTrace}");
                }
            }

            //codebaseを直列で関数収集
#if PARALLEL
            Parallel.ForEach(callIPs, (startAddr) =>
#else
            foreach (var callIp in callIPs)
#endif
            {
                var code = codes[callIp];
                var startAddr = code.op[0].opconst;

                collectProcess(startAddr, code.ip);//収集
            }
#if PARALLEL
            );
#endif
            Console.SetCursorPosition(curPos, Console.CursorTop);
            Console.WriteLine($"{1.0:P4} {funcs.Count()} functions, {varDic.Count()} variables found.");

            //関数内部から関数収集
            var allFuncInCall = funcs.SelectMany(t => t.CallTo).Distinct().ToArray();
            var prevDataSegmentSizeCount = 0;
            while (true)
            {
                if (prevDataSegmentSizeCount!=dataSegmentSize.Count)
                {
                    //DD領域除去して再逆ディスアセンブル
                    Dictionary<uint, uint> oldCodeCallAddr = callIPs.ToDictionary(t => t, t => codes[t].op[0].opconst);
                    codes = DisasmAll(dataSegmentSize);
                    var newCallIPs = getCallIPs();
                    var toBeRemovedProcedures = callIPs.Where(t => !newCallIPs.Contains(t)).ToArray();

                    if (toBeRemovedProcedures.Length > 0)
                    {
                        var removeAddrs = toBeRemovedProcedures.Select(t => oldCodeCallAddr[t]).ToArray();
                        funcs.RemoveAll(t => removeAddrs.Contains(t.StartAddr));
                        Console.WriteLine($"{toBeRemovedProcedures.Length}個の取得ミス削除 {funcs.Count()} functions found.");
                    }
                    callIPs = newCallIPs;
                    prevDataSegmentSizeCount = dataSegmentSize.Count;
                }

                var starts = funcs.Select(t => t.StartAddr).ToArray();
                var omissions = allFuncInCall.Where(t => !starts.Contains(t)).ToArray();
                if (omissions.Length <= 0) break;
                subroutineStarts.AddRange(omissions);
                Console.Write($"{omissions.Length}個の取得漏れ発見... ");
                curPos = Console.CursorLeft;
#if PARALLEL
                Parallel.ForEach(omissions, (omission) =>
#else
                foreach (var omission in omissions)
#endif
                { collectProcess(omission, omission); }
#if PARALLEL
                );
#endif
                Console.SetCursorPosition(curPos, Console.CursorTop);
                Console.WriteLine($"{1.0:P4} {funcs.Count()} functions, {varDic.Count()} variables found.");
                allFuncInCall = omissions.Select(t=>funcs.First(u=>u.StartAddr==t)).SelectMany(t => t.CallTo).Distinct().ToArray();
            }

            //解放
            Marshal.FreeHGlobal(reserved);

            //メイン
            if (foundMainDic.Count == 0)
            {
                Console.WriteLine("Main Function Not Found");
                mainAddr = 0;
            }
            else
            {
                mainAddr = foundMainDic.OrderBy(t => t.Value).First().Key;
                Console.WriteLine($"Main Function Address: 0x{mainAddr:X8}");
            }

            //関数重複削除
            /*Subroutine[] result;
            var distinct = funcs.GroupBy(t => t.StartAddr).ToArray();
            if (distinct.Count(t => t.Count() > 1) >= 1)
            {
                int distFuncs = distinct.Count(t => t.Count() > 1);
                Console.WriteLine($"{distFuncs}個の重複削除 {funcs.Count() - distFuncs} functions found.");
                result = distinct.Select(t => t.First()).OrderBy(t => t.StartAddr).ToArray();
            }
            else
            {
                result = funcs.OrderBy(t => t.StartAddr).ToArray();
            }*/
            var result = funcs.OrderBy(t => t.StartAddr).ToArray();

            //関数インデックスセット
            for (int i = 0; i < result.Length; i++)
            {
                result[i].index = i;
            }
            var funcIndexDic = result.ToDictionary(t => t.StartAddr, t => t.index);

            //静的変数呼び出し元セット&インデックスセット
            foreach (var varAddr in varDic.Keys.OrderBy(t => t).Select((v, i) => new { v, i }))
            {
                varDic[varAddr.v].index = varAddr.i;
                foreach (var callfrom in varCallFromFuncAddrs[varAddr.v].Distinct())
                {
                    varDic[varAddr.v].CallFrom.Add(funcIndexDic[callfrom]);
                    if (funcCallToVarAddrs.ContainsKey(callfrom)) funcCallToVarAddrs[callfrom].Add(varAddr.i);
                    else funcCallToVarAddrs[callfrom] = new List<int> { varAddr.i };
                }
            }
            //関数の静的変数呼び出しセット
            foreach(var funcAddr in funcCallToVarAddrs.Keys)
            {
                result[funcIndexDic[funcAddr]].StaticVariables = funcCallToVarAddrs[funcAddr].ToArray();
            }

            variables = varDic.Values.OrderBy(t => t.Addr).ToArray();
            return result;
        }

        /// <summary>
        /// safeなdisasm
        /// </summary>
        /// <param name="cmdPtr"></param>
        /// <param name="cmdsize"></param>
        /// <param name="ip"></param>
        /// <param name="da"></param>
        /// <param name="damode"></param>
        /// <returns></returns>
        protected uint Disasm(IntPtr cmdPtr, uint cmdsize, uint ip, out t_disasm da, DA damode)
        {
            unsafe
            {
                byte* cmd = (byte*)cmdPtr.ToPointer();
                return Disasm(cmd, cmdsize, ip, out da, damode);
            }
        }

        //メイン関数で呼ばれてる11個の関数
        Dictionary<uint, uint[]> main11CallDic = new Dictionary<uint, uint[]>();
        object main11Locker = new object();
        object thiscallLocker = new object();

        /// <summary>
        /// 関数の追跡
        /// </summary>
        /// <param name="srcPtr"></param>
        /// <param name="codes"></param>
        /// <param name="start"></param>
        /// <param name="finalIP"></param>
        /// <param name="callTo"></param>
        /// <param name="staticVariables"></param>
        /// <param name="foundMainNest"></param>
        /// <param name="getValues"></param>
        /// <returns></returns>
        t_disasm[] Finder(IntPtr srcPtr, Dictionary<uint, t_disasm> codes, uint start, uint finalIP, List<uint> subroutineStarts, Dictionary<uint, int> dataSegmentSize,
            ref Dictionary<uint, List<bool>> thiscallList, out List<uint> callTo, out uint[] staticVariables, out int foundMainNest)
        {
            //jmpアドレスリスト
            List<uint> visitedJmpAddr = new List<uint>();

            //呼び出し関数リスト
            List<uint> funcCallTo = new List<uint>();

            //使用する静的変数リスト
            List<uint> funcStaticVariables = new List<uint>();

            //thiscallの呼び出し関数リスト
            Dictionary<uint, List<bool>> funcThiscallList= new Dictionary<uint, List<bool>>();

            //アドレススキップサイズ辞書
            Dictionary<uint, uint> skipSizeDic = new Dictionary<uint, uint>();
            Dictionary<uint, (bool isMain, uint?[] regOfCmp)> afterSkipStateDic = new Dictionary<uint, (bool isMain, uint?[] regOfCmp)>();//skip後の状態

            //メイン探索用
            int innerFoundMainNest = -1;
            uint[] main11callAddr = null;

            //実態関連
            IntPtr startPtr = IntPtr.Add(srcPtr, (int)(start - NtHeader.OptionalHeader.ImageBase - NtHeader.OptionalHeader.BaseOfCode));
            uint maxSize = NtHeader.OptionalHeader.ImageBase + NtHeader.OptionalHeader.BaseOfCode + NtHeader.OptionalHeader.SizeOfCode - start;
            
            //実態実行
            var result = ChaseMain(start, startPtr, maxSize);

            //out, ref引数に戻す
            callTo = funcCallTo;
            foundMainNest = innerFoundMainNest;
            staticVariables = funcStaticVariables.Distinct().ToArray();

            //Main11探索
            if (innerFoundMainNest >= 0)
            {
                lock (main11Locker)
                {
                    main11CallDic[start] = main11callAddr.Select(t => result.First(i => i.ip == t).op[0].opconst).ToArray();
                }
            }

            return result;
            
            //遅延逆アセンブル
            IEnumerable<t_disasm> LazyDisasm(uint inner_start, IntPtr cmdPtr, uint cmdsize, bool needReDisasm, bool unoptimize = true)
            {
                for (uint l = 0, ip = inner_start; 0 < (int)cmdsize; cmdsize -= l, ip += l, cmdPtr = IntPtr.Add(cmdPtr, (int)l))
                {
                    t_disasm code;
                    if (skipSizeDic.ContainsKey(ip))
                    {   //skip
                        l = skipSizeDic[ip];
                        if (!needReDisasm && !codes.ContainsKey(ip + l))
                            needReDisasm = true;
                        continue;
                    }
                    else if (needReDisasm)
                    {   //再逆アセンブル
                        l = Disasm(cmdPtr, cmdsize, ip, out code, 0);
                    }
                    else
                    {   //引用
                        code = codes[ip];
                        l = code.size;
                    }
                    if (unoptimize && code.cmdtypeKind == D.JMP && 
                        (code.op[0].opconst < start || subroutineStarts.Any(t => t <= code.op[0].opconst && t > start)))
                    {
                        //call→jmpの最適化解除
                        code.cmdtype = (code.cmdtype & ~D.JMP) | D.CALL;
                        code.exttype |= DX.CALLRET;
                    }
                    yield return code;
                }
            }

            //追跡実態
            t_disasm[] ChaseMain(uint inner_start, IntPtr cmdPtr, uint cmdsize, int nest = 0)
            {
                List<t_disasm> inner_result = new List<t_disasm>();

                //data in code関連
                uint?[] regOfCmp = new uint?[NREG];
                (byte scale, int[] datas)[] regOfDatas = new(byte scale, int[] datas)[NREG];

                bool needReDisasm = false;

                if (!codes.ContainsKey(inner_start))
                {
                    needReDisasm = true;//codesが壊れているので再逆アセンブル
                }

                int pushNum = 0;
                uint lastIp = inner_start;
                bool checkMain = false, checkThiscall = false;
                foreach (var code in LazyDisasm(inner_start, cmdPtr, cmdsize, needReDisasm))
                {
                    var cmdtype = code.cmdtypeKind;
                    var codeop = code.op;
                    if (afterSkipStateDic.ContainsKey(code.ip))//復旧
                    {
                        checkMain = afterSkipStateDic[code.ip].isMain;
                        regOfCmp = afterSkipStateDic[code.ip].regOfCmp;
                    }

                    lastIp = code.ip + (cmdtype == D.RET || cmdtype == D.INT ? 0 : code.size);
                    inner_result.Add(code);

                    switch (cmdtype)
                    {
                        case D.JMC:
                        case D.JMP:
                            if (cmdtype == D.JMP && codeop[0].opconst < code.ip)
                            {

                            }

                            if (visitedJmpAddr.Contains(code.ip)) break;

                            //ジャンプ実行
                            t_disasm[] jump(uint to)
                            {
                                if (to > finalIP) return null;

                                //skip size登録
                                var jumpSkipSize = code.ip - inner_start + code.size;
                                if (jumpSkipSize != 0)
                                {
                                    skipSizeDic[inner_start] = jumpSkipSize;
                                    afterSkipStateDic[inner_start] = (checkMain, regOfCmp);
                                }

                                //相対jmp先
                                uint jumpTo = 0;
                                if (code.ip > to) jumpTo = code.ip - to;
                                else jumpTo = finalIP - to;

                                return ChaseMain(to, IntPtr.Add(cmdPtr, (int)to - (int)inner_start), jumpTo, nest + 1);
                            }

                            int jmpIndex = Array.FindIndex(codeop[0].scale, t => t == 4);
                            if (jmpIndex < 0 &&CheckImmutable(codeop[0]))
                            {
                                visitedJmpAddr.Add(code.ip);
                                var jumpto = jump(codeop[0].opconst);
                                var cnt = jumpto.Count();
                                if (jumpto != null) inner_result.AddRange(jumpto);
                            }
                            else if (jmpIndex >= 0)
                            {
                                //switch-case statement
                                uint[] addrs;
                                if (regOfDatas[jmpIndex].datas != null)
                                {
                                    //indexがすでにある
                                    addrs = regOfDatas[jmpIndex].datas.Distinct().Select(offset =>
                                    {
                                        switch (regOfDatas[jmpIndex].scale)
                                        {
                                            case 4: return getValues<uint>((uint)(codeop[0].opconst + 4 * offset), 1)[0];
                                            case 2: return getValues<uint>((uint)(codeop[0].opconst + 4 * offset), 1)[0];
                                            case 1: return getValues<uint>((uint)(codeop[0].opconst + 4 * offset), 1)[0];
                                            default: throw new InvalidOperationException("謎のScale");
                                        }
                                    }).ToArray();
                                }
                                else if (regOfCmp[jmpIndex] != null && regOfCmp[jmpIndex] <= 0x50)
                                {
                                    //indexがレジスタにある
                                    int size = (int)regOfCmp[jmpIndex].Value + 1;
                                    addrs = getValues<uint>(codeop[0].opconst, size);
                                    dataSegmentSize[codeop[0].opconst] = 4 * size;
                                }
                                else goto noSwitchCase;//switch-case statementでない（アンセブルミス？）

                                if (addrs.All(t => IsInCodeArea(t)))
                                {
                                    if (checkMain) checkMain = addrs.Length == 0x0C;

                                    skipSizeDic[codeop[0].opconst] = (uint)(addrs.Length * 4);

                                    visitedJmpAddr.Add(code.ip);
                                    foreach (var addr in addrs)
                                    {
                                        var jumpto = jump(addr);
                                        if (jumpto == null) continue;
                                        if (checkMain) checkMain = jumpto.First(t => t.ip == addr).cmdtypeKind == D.CALL;
                                        inner_result.AddRange(jumpto);
                                    }
                                    if (checkMain)
                                    {
                                        main11callAddr = addrs;
                                        innerFoundMainNest = nest;
                                        checkMain = false;
                                    }
                                }
                                noSwitchCase:;
                            }

                            break;
                        case D.CALL:
                            var callToAddr = codeop[0].opconst;
                            if (!CheckImmutable(codeop[0]) || funcCallTo.Contains(callToAddr)) break;

                            //呼出規則thiscallチェック
                            if (funcThiscallList.ContainsKey(callToAddr)) funcThiscallList[callToAddr].Add(checkThiscall);
                            else funcThiscallList[callToAddr] = new List<bool> { checkThiscall };
                            checkThiscall = false;

                            //呼び出しアドレス追加
                            funcCallTo.Add(callToAddr);
                            if (innerFoundMainNest < 0) checkMain = pushNum == 2;
                            pushNum = 0;
                            if ((code.exttype & DX.CALLRET) != 0) goto case D.RET;
                            break;
                        case D.PUSH:
                            if (checkThiscall && codeop[0].reg == REG.ECX && codeop[0].granularity == 4)
                            {
                                //PUSH ECXは取り消し
                                checkThiscall = false;
                            }
                            pushNum++;
                            break;
                        case D.INT:
                        case D.RET:
                            goto retn;
                        case D.TEST:
                            switch (code.cmdname)
                            {
                                case "CMP":
                                    //レジスタと定数の比較
                                    if (codeop[0].opsize >= 4 && codeop[0].features == OP.REGISTER && codeop[1].features == OP.CONST)
                                    {
                                        regOfCmp[(int)codeop[0].reg] = codeop[1].opconst;
                                    }
                                    break;
                            }

                            //関数呼出し前のTEST防止
                            if (checkThiscall) checkThiscall = codeop[0].reg != REG.ECX || codeop[0].granularity != 4;

                            //グローバル変数
                            CollectGlobalVariable(code);
                            break;
                        case D.CMD:
                            //関数呼出し前のCMD防止
                            if (checkThiscall) checkThiscall = codeop[0].reg != REG.ECX || codeop[0].granularity != 4;

                            //グローバル変数
                            CollectGlobalVariable(code);
                            break;
                        case D.MOV:
                        case D _ when (code.exttype & DX.TYPEMASK) == DX.LEA:

                            //data in code statement
                            int targetReg = (int)codeop[0].reg;
                            int movIndex = Array.FindIndex(codeop[1].scale, t => t >= 1);
                            if (movIndex >= 0 && regOfCmp[movIndex] != null && IsInCodeArea(codeop[1].opconst))
                            {
                                int datanum = (int)regOfCmp[movIndex].Value + 1;
                                uint datastartaddr = codeop[1].opconst;
                                switch (codeop[1].scale[movIndex])
                                {
                                    case 4: regOfDatas[targetReg] = (4, getValues<int>(datastartaddr, datanum)); break;
                                    case 2: regOfDatas[targetReg] = (2, getValues<short>(datastartaddr, datanum).Select(t => (int)t).ToArray()); break;
                                    case 1: regOfDatas[targetReg] = (1, getValues<byte>(datastartaddr, datanum).Select(t => (int)t).ToArray()); break;
                                    default: throw new InvalidOperationException("謎のScale");
                                }

                                dataSegmentSize[datastartaddr] = codeop[1].scale[movIndex] * datanum;
                                regOfCmp[targetReg] = null;
                                skipSizeDic[codeop[1].opconst] = (uint)(datanum * codeop[1].scale[movIndex]);
                            }
                            else if ((codeop[0].features & OP.REGISTER) != 0 && regOfDatas[targetReg].datas != null) regOfDatas[targetReg].datas = null;
 
                            //呼び出し規約thiscallチェック
                                 checkThiscall = codeop[0].reg == REG.ECX && codeop[0].granularity == 4;

                            //グローバル変数
                            CollectGlobalVariable(code);
                            break;
                    }
                    //codeからグローバル変数収集
                    void CollectGlobalVariable(t_disasm targetCode)
                    {
                        var inGlobalOP = targetCode.op.Where(t => (t.features & OP.MEMORY) != 0 && (t.features & OP.INDEXED) == 0 && t.seg == SEG.DS);
                        if (inGlobalOP.Count() > 0)
                        {
                            var gvar = inGlobalOP.First().opconst;
                            if (gvar == 0) throw new InvalidOperationException($"global variable is zero. (IP:0x{targetCode.ip:X8})");
                            if (gvar >= NtHeader.OptionalHeader.ImageBase + NtHeader.OptionalHeader.BaseOfData &&
                                gvar < NtHeader.OptionalHeader.ImageBase + NtHeader.OptionalHeader.SizeOfImage)
                            {
                                funcStaticVariables.Add(gvar);
                            }
                        }
                    }
                }
                retn:;

                //set skip size
                var skipSize = lastIp - inner_start;
                if (skipSize != 0)
                {
                    skipSizeDic[inner_start] = skipSize;
                    afterSkipStateDic[inner_start] = (checkMain, regOfCmp);
                }

                //重複除去&ソート
                uint pastip = 0;
                return inner_result.OrderBy(t => t.ip).Where(code =>
                {
                    if (code.ip != pastip)
                    {
                        pastip = code.ip;
                        return true;
                    }
                    else return false;
                }).ToArray();
            }
        }
    }
}
