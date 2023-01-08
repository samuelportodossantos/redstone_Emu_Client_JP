using RedStoneLib.Packets;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using static Anantashesha.Decompiler.Disassemble.Disassembler;

namespace Anantashesha.Decompiler.Disassemble
{
    class SimpleDisassembler : Disassembler, IDisposable, IEnumerable<t_disasm>
    {
        IMAGE_DOS_HEADER MsHeader;
        IMAGE_NT_HEADERS NtHeader;
        IMAGE_DATA_COLLECTION DataCollection;
        IMAGE_SECTION_HEADER[] SectionHeaders;

        uint CodeBaseIP;
        byte[] AllCodes;
        IntPtr ReservedPtr;

        //状態
        IntPtr CurrentPtr;
        uint _CurrentIP;
        uint CurrentCodeLen;
        t_disasm CurrentCode;

        /// <summary>
        /// スキップを使用
        /// </summary>
        public bool UseSkip { get; set; } = true;

        //ステートセーブ用スタック
        Stack<(IntPtr ptr, uint ip, uint len, bool skip)> SaveStates = new Stack<(IntPtr ptr, uint ip, uint len, bool skip)>();

        //Skipリスト
        Dictionary<uint, uint> SkipList = new Dictionary<uint, uint>();
        //データ領域用Skipリスト
        Dictionary<uint, uint> SkipListForData = new Dictionary<uint, uint>();

        /// <summary>
        /// 初期化
        /// </summary>
        public SimpleDisassembler(string fname)
            : base()
        {
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
                DataCollection = new IMAGE_DATA_COLLECTION(br, num);
                SectionHeaders = br.Reads<IMAGE_SECTION_HEADER>(num);

                //コード開始に飛ぶ
                br.BaseStream.Seek(NtHeader.OptionalHeader.BaseOfCode, SeekOrigin.Begin);
                _CurrentIP = CodeBaseIP = NtHeader.OptionalHeader.ImageBase + NtHeader.OptionalHeader.BaseOfCode;

                //動的確保
                AllCodes = br.ReadBytes((int)NtHeader.OptionalHeader.SizeOfCode);
                ReservedPtr = Marshal.AllocHGlobal(AllCodes.Length);
                Marshal.Copy(AllCodes, 0, ReservedPtr, AllCodes.Length);
                CurrentPtr = ReservedPtr;
            }
        }

        /// <summary>
        /// 解放
        /// </summary>
        public void Dispose()
            => Marshal.FreeHGlobal(ReservedPtr);

        /// <summary>
        /// コード領域に存在するか
        /// </summary>
        /// <param name="ip"></param>
        /// <returns></returns>
        public bool IsInCode(uint ip)
            => ip >= CodeBaseIP && ip <= CodeBaseIP + NtHeader.OptionalHeader.SizeOfCode;

        /// <summary>
        /// メモリ内かレジスタのオペランド
        /// </summary>
        /// <param name="op"></param>
        /// <returns></returns>
        public bool MemoryOrReg(t_operand op)
            => (op.features & OP.MEMORY) != 0 || (op.features & OP.SOMEREG) == OP.REGISTER;

        /// <summary>
        /// switch-case Block疑惑
        /// </summary>
        /// <param name="dataEntryIP"></param>
        /// <returns></returns>
        public bool IsSwitchCaseLikeBlock(out uint dataEntryIP)
        {
            switch (CurrentCode.cmdtypeKind)
            {
                case D.JMP:
                case D.JMC:
                    //INDEXEDフラグ+DDエントリーがコード内
                    if ((CurrentCode.op[0].features & OP.INDEXED) != 0 && IsInCode(CurrentCode.op[0].opconst))
                    {
                        dataEntryIP = CurrentCode.op[0].opconst;
                        return true;
                    }
                    goto default;
                default:
                    dataEntryIP = 0;
                    return false;
            }
        }

        /// <summary>
        /// 現在のIP
        /// </summary>
        public uint CurrentIP
        {
            set
            {
                if (!IsInCode(value))
                    throw new ArgumentOutOfRangeException("ChangeIPの範囲が不正です");
                CurrentPtr = IntPtr.Add(ReservedPtr, (int)value - (int)CodeBaseIP);
                _CurrentIP = value;
                CurrentCodeLen = 0;
            }
            get => _CurrentIP;
        }

        /// <summary>
        /// safe
        /// </summary>
        /// <param name="cmdPtr"></param>
        /// <param name="cmdsize"></param>
        /// <param name="ip"></param>
        /// <param name="da"></param>
        /// <param name="damode"></param>
        /// <returns></returns>
        uint Disasm(IntPtr cmdPtr, uint cmdsize, uint ip, out t_disasm da, DA damode)
        {
            unsafe
            {
                byte* cmd = (byte*)cmdPtr.ToPointer();
                return Disasm(cmd, cmdsize, ip, out da, damode);
            }
        }

        /// <summary>
        /// UInt32取得
        /// </summary>
        /// <param name="ip"></param>
        /// <returns></returns>
        public uint GetUInt32(uint ip)
        {
            if (!IsInCode(ip))
                return 0;
            return BitConverter.ToUInt32(AllCodes, (int)(ip - CodeBaseIP));
        }

        /// <summary>
        /// UInt16取得
        /// </summary>
        /// <param name="ip"></param>
        /// <returns></returns>
        public ushort GetUInt16(uint ip)
        {
            if (!IsInCode(ip))
                return 0;
            return BitConverter.ToUInt16(AllCodes, (int)(ip - CodeBaseIP));
        }

        /// <summary>
        /// UInt8取得
        /// </summary>
        /// <param name="ip"></param>
        /// <returns></returns>
        public byte GetUInt8(uint ip)
        {
            if (!IsInCode(ip))
                return 0;
            return AllCodes[(int)(ip - CodeBaseIP)];
        }

        /// <summary>
        /// Byte列取得
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public byte[] GetBytes(uint ip, int count)
            => AllCodes.Skip((int)(ip - CodeBaseIP)).Take(count).ToArray();

        /// <summary>
        /// 次のコード
        /// </summary>
        /// <returns></returns>
        public IEnumerator<t_disasm> GetEnumerator()
        {
            int zeroLength = 0;//コードのゼロの長さ
            List<t_disasm> suspiciousCodes = new List<t_disasm>();
            for (CurrentCodeLen = 0; _CurrentIP < CodeBaseIP + NtHeader.OptionalHeader.SizeOfCode;
                _CurrentIP += CurrentCodeLen, CurrentPtr = IntPtr.Add(CurrentPtr, (int)CurrentCodeLen))
            {
                //スキップ
                if (UseSkip && SkipList.TryGetValue(_CurrentIP, out var nextip))
                {
                    CurrentIP = nextip;
                    continue;
                }
                //データはスキップなし
                if (SkipListForData.TryGetValue(_CurrentIP, out var nextipfordata))
                {
                    CurrentIP = nextipfordata;
                    continue;
                }

                //取得
                CurrentCodeLen = Disasm(CurrentPtr, CodeBaseIP + NtHeader.OptionalHeader.SizeOfCode - _CurrentIP, _CurrentIP, out CurrentCode, 0);

                //skipボーナス
                if (UseSkip && SkipListForData.ContainsKey(_CurrentIP + CurrentCodeLen))
                {
                    continue;
                }

                //ゼロチェック
                for (int i = 0; i < CurrentCodeLen; i++)
                {
                    if (AllCodes[(int)(_CurrentIP - CodeBaseIP) + i] == 0) zeroLength++;
                    else
                    {
                        //疑い解消
                        if (zeroLength > 0)
                        {
                            zeroLength = 0;
                            if (suspiciousCodes.Count > 0)
                            {
                                foreach (var code in suspiciousCodes) yield return code;
                                suspiciousCodes.Clear();
                            }
                        }
                        break;
                    }
                }

                if (zeroLength > 0x0B)  //ゼロ多いとコード終了
                    break;
                else if (zeroLength > 0)//コード終了疑惑のため保留
                    suspiciousCodes.Add(CurrentCode);
                else                    //正常
                    yield return CurrentCode;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
            => GetEnumerator();

        /// <summary>
        /// データ領域用スキップリスト取り込み
        /// </summary>
        /// <param name="startIP"></param>
        /// <param name="endIP"></param>
        public void TakeInSkipListForData(uint startIP, uint endIP)
        {
            //追加する開始アドレスと一致する終了アドレスを持つスキップ
            uint startIsEndIP = SkipListForData.SingleOrDefault(t => t.Value == startIP).Key;
            if (startIsEndIP != 0)
            {
                SkipListForData[startIsEndIP] = endIP;
                return;
            }

            //追加する終了アドレスと一致する開始アドレスを持つスキップ
            uint endIsStartIP = SkipListForData.Keys.SingleOrDefault(t => t == endIP);
            if (endIsStartIP != 0)
            {
                var orgEndIP = SkipListForData[endIsStartIP];
                SkipListForData.Remove(endIsStartIP);
                SkipListForData[startIP] = orgEndIP;
                return;
            }
            
            SkipListForData[startIP] = endIP;
        }

        /// <summary>
        /// IPとコードの辞書からSkipList作成して取り込む
        /// </summary>
        /// <param name="blockCodes"></param>
        public void TakeInSkipList(Dictionary<uint, t_disasm[]> blockCodes)
        {
            //各ブロックのIP+合計サイズ計算
            var codeSizes = blockCodes.ToDictionary(t => t.Key, t => (uint)(t.Key + t.Value.Sum(u => u.size)));

            //入力削る
            codeSizes = ComputeSkipList(codeSizes);

            //削る前にスキップ
            if (codeSizes.ContainsKey(_CurrentIP))
            {
                CurrentIP = codeSizes[_CurrentIP];
            }

            //メンバに入れる前に削る
            SkipList = ComputeSkipList(codeSizes, SkipList);

            foreach(var newSkip in codeSizes.Keys)
            {
                uint skipLast = codeSizes[newSkip];
                if (TryGetProtrudeLength(newSkip, skipLast, SkipList, out var protrudeLen))
                    skipLast += protrudeLen;
                SkipList[newSkip] = skipLast;
            }

            //長さが重複するものを削る
            Dictionary<uint, uint> ComputeSkipList(Dictionary<uint, uint> dst, Dictionary<uint, uint> src = null)
            {
                if (src == null) src = dst;
                else if (src.Count == 0) return new Dictionary<uint, uint>();
                bool changed;
                do
                {
                    changed = false;
                    foreach (var blockIP in src.Keys)
                    {
                        if (TryGetProtrudeLength(blockIP, src[blockIP], dst, out var protrudeLen))
                        {
                            src[blockIP] += protrudeLen;
                            changed = true;
                            break;
                        }
                    }
                } while (changed);
                return src;
            }

            //重複する長さ取得試行
            bool TryGetProtrudeLength(uint startIP, uint endIP, Dictionary<uint, uint> dst, out uint protrudeLen)
            {
                //スタートIPが今のブロックにめり込んでるブロック
                var includeStartIPs = dst.Keys.Where(t => t > startIP && t <= endIP).ToArray();
                if (includeStartIPs.Length == 0)
                {
                    protrudeLen = 0;
                    return false;
                }
                //はみ出した長さが最大のブロック
                var mostProtrudeBlock = includeStartIPs.Select(t => new { ip = t, protrude = dst[t] - endIP }).OrderByDescending(t => t.protrude).First();
                //足して削る
                if (mostProtrudeBlock.protrude > 0)
                    protrudeLen = mostProtrudeBlock.protrude;
                else protrudeLen = 0;
                if(protrudeLen+endIP== 0x0042ae7b)
                {

                }

                foreach (var deleteIP in includeStartIPs) dst.Remove(deleteIP);
                return true;
            }
        }

        /// <summary>
        /// ステートセーブ
        /// </summary>
        public void StateSave()
            => SaveStates.Push((CurrentPtr, CurrentIP, CurrentCodeLen, UseSkip));

        /// <summary>
        /// ステートロード
        /// </summary>
        /// <returns></returns>
        public void StateLoad()
        {
            if (SaveStates.Count == 0) throw new InvalidOperationException("セーブされていません");
            (CurrentPtr, CurrentIP, CurrentCodeLen, UseSkip) = SaveStates.Pop();
        }

        /// <summary>
        /// 一時停止
        /// </summary>
        public void Pause()
            => CurrentCodeLen = 0;

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
        /// イメージデータコレクション
        /// </summary>
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct IMAGE_DATA_COLLECTION
        {
            public IMAGE_DATA_DIRECTORY EXPORT => BaseData[0];
            public IMAGE_DATA_DIRECTORY IMPORT => BaseData[1];
            public IMAGE_DATA_DIRECTORY RESOURCE => BaseData[2];
            public IMAGE_DATA_DIRECTORY EXCEPTION => BaseData[3];
            public IMAGE_DATA_DIRECTORY SECURITY => BaseData[4];
            public IMAGE_DATA_DIRECTORY BASERELOC => BaseData[5];
            public IMAGE_DATA_DIRECTORY DEBUG => BaseData[6];
            public IMAGE_DATA_DIRECTORY COPYRIGHT => BaseData[7];
            public IMAGE_DATA_DIRECTORY GLOBALPTR => BaseData[8];
            public IMAGE_DATA_DIRECTORY TLS => BaseData[9];
            public IMAGE_DATA_DIRECTORY LOAD_CONFIG => BaseData[10];
            public IMAGE_DATA_DIRECTORY BOUND_IMPORT => BaseData[11];
            public IMAGE_DATA_DIRECTORY IAT => BaseData[12];
            public IMAGE_DATA_DIRECTORY DELAY_IMPORT => BaseData[13];
            public IMAGE_DATA_DIRECTORY COM_DESCRIPTOR => BaseData[14];
            public IMAGE_DATA_DIRECTORY _RESERVED => BaseData[14];

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
            private readonly IMAGE_DATA_DIRECTORY[] BaseData;

            public IMAGE_DATA_COLLECTION(PacketReader pr, int num)
            {
                BaseData = new IMAGE_DATA_DIRECTORY[16];
                for(int i = 0; i < 16; i++)
                {
                    if (i < num) BaseData[i] = pr.ReadStruct<IMAGE_DATA_DIRECTORY>();
                    else BaseData[i] = default(IMAGE_DATA_DIRECTORY);
                }
            }
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
    }
}
