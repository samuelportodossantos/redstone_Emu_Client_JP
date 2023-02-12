using RedStoneLib.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using static RedStoneLib.Model.Item;

namespace RedStoneLib.Packets
{
    public class PacketReader : BinaryReader
    {
        byte[] _baseData;

        /// <summary>
        /// base stream付きのコンストラクタ
        /// </summary>
        /// <param name="s"></param>
        public PacketReader(Stream s)
            : base(s)
        { }

        /// <summary>
        /// byte配列付きコンストラクタ
        /// </summary>
        /// <param name="bytes"></param>
        public PacketReader(byte[] bytes) 
            : base(new MemoryStream(bytes))
        {
            _baseData = bytes;
        }

        /// <summary>
        /// 元のバイト列を取得
        /// </summary>
        /// <returns></returns>
        public byte[] GetBytes()
            => _baseData;

        /// <summary>
        /// 文字列を読む（文字コード指定可能）
        /// </summary>
        /// <param name="maxCount">読み込む文字列最大値指定</param>
        /// <returns></returns>
        public string ReadSjis(int maxCount, string code)
        {
            var data = ReadBytes(maxCount);
            return Helper.SjisByteToString(data, code);
        }

        /// <summary>
        /// 文字コードがShift-Jisの文字列を読む（文字列最大値指定可）
        /// </summary>
        /// <param name="maxCount">読み込む文字列最大値指定</param>
        /// <returns></returns>
        public string ReadSjis(int maxCount)
        {
            var data = ReadBytes(maxCount);
            return Helper.SjisByteToString(data);
        }

        /// <summary>
        /// 文字コードがShift-Jisの文字列を読む
        /// </summary>
        /// <returns></returns>
        public string ReadSjis()
        {
            List<byte> results = new List<byte>();
            for (long i = BaseStream.Position; i < BaseStream.Length; i++)
            {
                var nowByte = ReadByte();

                //ゼロ発見したら終了
                if (nowByte == 0) break;

                results.Add(nowByte);
            }

            return Helper.SjisByteToString(results.ToArray());
        }

        public long Position()
        {
            return BaseStream.Position;
        }

        /// <summary>
        /// 複数回文字列を読みこむ
        /// </summary>
        /// <param name="count"></param>
        /// <returns></returns>
        public string[] ReadSjises(int count)
            => Enumerable.Range(0, count).Select(_ => ReadSjis()).ToArray();

        /// <summary>
        /// 構造体の読み込み
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T ReadStruct<T>() where T : struct
        {
            Type type = typeof(T);
            int size = type.IsGenericType ? Helper.GenericSizeOf(type) : Marshal.SizeOf(type);
            var structBytes = new byte[size];
            Read(structBytes, 0, structBytes.Length);

            //読み込み
            T result = Helper.BytesToStruct<T>(structBytes);

            return result;
        }

        /// <summary>
        /// アイテム読み込み
        /// </summary>
        /// <returns></returns>
        public Item ReadItem()
            => new Item(ReadStruct<ItemInfo>());

        /// <summary>
        /// 復号化キーのセット
        /// </summary>
        /// <param name="rawKey">生の鍵</param>
        public void SetDataEncodeTable(int rawKey)
        {
            DecodeKey = PacketCrypt.GenerateScenarioDecodeKey(rawKey);
            NeedDecrypt = true;
        }

        /// <summary>
        /// 復号化必要性
        /// </summary>
        public bool NeedDecrypt { get; set; } = false;

        /// <summary>
        /// 復号化キー
        /// </summary>
        private uint DecodeKey;

        /// <summary>
        /// 復号化（内部の暗号化レベル使用）
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <returns></returns>
        public T _Decryption<T>(T obj) where T : new()
        {
            if (NeedDecrypt)
            {
                //復号化
                var encryptedByteArray = Helper.StructToBytes(obj);
                Console.WriteLine("key:" + DecodeKey) ;
                var decryptedByteArray = PacketCrypt.DecodeScenarioBuffer(encryptedByteArray, DecodeKey);
                return Helper.BytesToStruct<T>(decryptedByteArray);
            }
            else
            {
                //復号化不要
                return obj;
            }
        }

        /// <summary>
        /// 暗号化されたTをRead
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="stream"></param>
        /// <returns></returns>
        public T EncryptionRead<T>() where T : new()
        {
            if (ReadFuncs.ContainsKey(typeof(T)))
            {
                return _Decryption((T)ReadFuncs[typeof(T)](this));
            }
            //辞書に追加
            ReadFuncs[typeof(T)] = s => Helper.BytesToStruct<T>(s.ReadBytes(Marshal.SizeOf(typeof(T))));
            return EncryptionRead<T>();
            throw new NotImplementedException();
        }

        /// <summary>
        /// 暗号化されたTをRead（複数）
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="count"></param>
        /// <param name="sequentially">逐次復号化</param>
        /// <returns></returns>
        public T[] EncryptionReads<T>(int count, bool sequentially=false) where T: new()
        {
            if (ReadFuncs.ContainsKey(typeof(T)))
            {
                if (sequentially)
                {
                    //逐次復号化
                    T[] result = new T[count];
                    for (int i = 0; i < count; i++)
                    {
                        byte[] byteArray = ReadBytes(Marshal.SizeOf(typeof(T)));
                        byteArray = PacketCrypt.DecodeScenarioBuffer(byteArray, DecodeKey);
                        result[i] = Helper.BytesToStruct<T>(byteArray);
                    }
                    return result;
                }
                else
                {
                    //一括復号化
                    byte[] byteArray = ReadBytes(count * Marshal.SizeOf(typeof(T)));
                    if (NeedDecrypt)
                    {
                        //復号化
                        byteArray = PacketCrypt.DecodeScenarioBuffer(byteArray, DecodeKey);
                    }

                    if (typeof(T) == typeof(byte))
                    {
                        //Tがbyteの場合はそのまま返す
                        return (T[])(object)byteArray;
                    }
                    else
                    {
                        //byte以外作成
                        T[] result = new T[count];
                        using (PacketReader br = new PacketReader(byteArray))
                        {
                            for (int i = 0; i < count; i++)
                            {
                                result[i] = (T)ReadFuncs[typeof(T)](br);
                            }
                        }
                        return result;
                    }
                }
            }

            //辞書に追加
            ReadFuncs[typeof(T)] = s => Helper.BytesToStruct<T>(s.ReadBytes(Marshal.SizeOf(typeof(T))));
            return EncryptionReads<T>(count, sequentially);
            throw new NotImplementedException();
        }

        /// <summary>
        /// Tを複数読み込む
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="count"></param>
        /// <returns></returns>
        public T[] Reads<T>(int count) where T : new()
        {
            T[] result = new T[count];

            if (!ReadFuncs.ContainsKey(typeof(T)))
            {
                //辞書に追加
                if (typeof(T).IsEnum)
                {
                    //列挙型考慮
                    ReadFuncs[typeof(T)] =
                        s => (T)Helper.BytesToStruct(s.ReadBytes(Marshal.SizeOf(Enum.GetUnderlyingType(typeof(T)))),
                        Enum.GetUnderlyingType(typeof(T)));
                }
                else
                {
                    ReadFuncs[typeof(T)] = s => Helper.BytesToStruct<T>(s.ReadBytes(typeof(T).IsGenericType ? Helper.GenericSizeOf(typeof(T)) : Marshal.SizeOf(typeof(T))));
                }

            }
            for (int i = 0; i < count; i++)
            {
                result[i] = (T)ReadFuncs[typeof(T)](this);
            }
            return result;
        }

        /// <summary>
        /// Tを一つ読み込む
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T ReadSingle<T>() where T : new()
            => Reads<T>(1)[0];

        /// <summary>
        /// 型とPacketReaderの写像辞書
        /// </summary>
        static Dictionary<Type, Func<PacketReader, object>> ReadFuncs = new Dictionary<Type, Func<PacketReader, object>>()
            {
                {typeof(bool), s => s.ReadBoolean()},
                {typeof(byte), s => s.ReadByte()},
                {typeof(short), s => s.ReadInt16()},
                {typeof(int), s => s.ReadInt32()},
                {typeof(long), s => s.ReadInt64()},
                {typeof(ushort), s => s.ReadUInt16()},
                {typeof(uint), s => s.ReadUInt32()},
                {typeof(ulong), s => s.ReadUInt64()},
                {typeof(string), s => s.ReadSjis()},
            };
        
    }
}
