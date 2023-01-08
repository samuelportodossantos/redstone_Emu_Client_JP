using RedStoneLib.Model;
using RedStoneLib.Packets;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace RedStoneLib
{
    public static class PublicHelper
    {
        /// <summary>
        /// シーズン変数
        /// </summary>
        public static uint SeasonVariable = 2;

        /// <summary>
        /// 国 0:韓国 1:日本
        /// </summary>
        public static uint Country = 1;

        public delegate void WriteWarningDelegate(string text, params object[] args);
        public delegate void WriteInternalDelegate(string text, params object[] args);

        /// <summary>
        /// 危険を示すWriteLine
        /// </summary>
        /// <param name="text"></param>
        /// <param name="args"></param>
        public static WriteWarningDelegate WriteWarning = null;

        /// <summary>
        /// 内部的なWriteLine
        /// </summary>
        /// <param name="text"></param>
        /// <param name="args"></param>
        public static WriteInternalDelegate WriteInternal = null;
    }

    /// <summary>
    /// その他の補助
    /// </summary>
    static class Helper
    {
        /// <summary>
        /// 静的ランダム
        /// </summary>
        public static Random StaticRandom = new Random();

        /// <summary>
        /// 実数型[%]で抽選する
        /// </summary>
        /// <param name="percentage"></param>
        /// <returns></returns>
        public static bool Lottery(double percentage)
            => percentage > StaticRandom.NextDouble() * 100.0;

        /// <summary>
        /// 正規乱数を得る
        /// </summary>
        /// <param name="rand"></param>
        /// <param name="mean"></param>
        /// <param name="stdDev"></param>
        /// <returns></returns>
        public static double NextGaussian(this Random rand, double mean = 0, double stdDev = 1)
        {
            //uniform(0,1] random doubles
            double u1 = 1.0 - rand.NextDouble();
            double u2 = 1.0 - rand.NextDouble();
            //random normal(0,1)
            double randStdNormal = Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Sin(2.0 * Math.PI * u2);

            //random normal(mean,stdDev^2)
            return mean + stdDev * randStdNormal;
        }

        /// <summary>
        /// 構造体から文字列を生成
        /// </summary>
        /// <typeparam name="T">struct</typeparam>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static string StructToString<T>(T obj) where T : struct
        {
            return string.Join(",", StructToBytes(obj));
        }

        /// <summary>
        /// 文字列から構造体を生成
        /// </summary>
        /// <typeparam name="T">struct</typeparam>
        /// <param name="str"></param>
        /// <returns></returns>
        public static T StringToStruct<T>(string str) where T : struct
        {
            return BytesToStruct<T>(str.Split(',').Select(t => Convert.ToByte(t)).ToArray());
        }

        /// <summary>
        /// ジェネリック型の強制コピー
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data"></param>
        /// <param name="value"></param>
        public static unsafe T BytesToGenericStruct<T>(byte[] data) where T : new()
        {
            T value = new T();
            var tr = __makeref(value);
            fixed (byte* ptr = &data[0])
            {
                *(IntPtr*)&tr = (IntPtr)ptr;
                value = __refvalue(tr, T);
            }
            return value;
        }

        /// <summary>
        /// Byte配列から構造体を生成
        /// </summary>
        /// <param name="bytes"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static object BytesToStruct(byte[] bytes, Type type)
        {
            GCHandle gch = GCHandle.Alloc(bytes, GCHandleType.Pinned);
            object result = Marshal.PtrToStructure(gch.AddrOfPinnedObject(), type);
            gch.Free();

            return result;
        }

        /// <summary>
        /// Byte配列から構造体を生成
        /// </summary>
        /// <typeparam name="T">struct</typeparam>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public static T BytesToStruct<T>(byte[] bytes) where T : new()
            => typeof(T).IsGenericType ? BytesToGenericStruct<T>(bytes) : (T)BytesToStruct(bytes, typeof(T));

        /// <summary>
        /// 構造体からByte配列を生成
        /// </summary>
        /// <typeparam name="T">struct</typeparam>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static byte[] StructToBytes<T>(T obj)
        {
            int size = Marshal.SizeOf(typeof(T));
            IntPtr ptr = Marshal.AllocHGlobal(size);
            Marshal.StructureToPtr(obj, ptr, false);

            byte[] bytes = new byte[size];
            Marshal.Copy(ptr, bytes, 0, size);
            Marshal.FreeHGlobal(ptr);
            return bytes;
        }

        /// <summary>
        /// ジェネリック型のサイズ
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static int GenericSizeOf(Type type)
        {
            IEnumerable<int> SizesOfFieldType()
            {
                foreach(var field in type.GetFields())
                {
                    Type fieldType = field.FieldType;
                    if (fieldType.IsGenericType)
                    {
                        yield return GenericSizeOf(fieldType);
                    }
                    else
                    {
                        yield return Marshal.SizeOf(fieldType);
                    }
                }
            }
            return SizesOfFieldType().Sum();
        }

        /// <summary>
        /// BytesToStructFromFieldConstructer版Sizeof
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private static int ParamatersSizeOf(Type type)
        {
            var flags = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance;

            int counter = 0;

            //フィールド列挙
            foreach (var field in type.GetFields(flags))
            {
                //配列の元のタイプ考慮したfield型取得
                var elementType = field.FieldType.IsArray ? field.FieldType.GetElementType() : field.FieldType;

                //配列サイズ　配列じゃない場合は1
                int len = field.FieldType.IsArray || field.FieldType == typeof(string) ? field.GetCustomAttribute<MarshalAsAttribute>().SizeConst : 1;

                //コンストラクタのリスト
                var constructers = elementType.GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance);

                var helper = (BytesToStructHelper)elementType.GetCustomAttribute(typeof(BytesToStructHelper), false);
                if (helper != null)
                {
                    counter += helper.Size * len;
                    continue;
                }

                //string
                if (field.FieldType == typeof(string))
                {
                    counter += len;
                    continue;
                }

                var fieldHelper = (BytesToStructHelper)field.GetCustomAttribute(typeof(BytesToStructHelper), false);
                for (int i = 0; i < len; i++)
                {
                    if (constructers.Length > 0)
                    {
                        //コンストラクタが存在
                        ConstructorInfo targetConstructer;

                        try
                        {
                            //選択されたprivateコンストラクタ
                            targetConstructer = constructers.Where(t => t.IsPrivate).Skip(fieldHelper?.ConstructerIndex ?? 0).First();
                        }
                        catch (InvalidOperationException ex)
                        {
                            throw new InvalidOperationException($"専用コンストラクタが存在しない，もしくは多いです：{elementType} {field.Name}", ex);
                        }

                        //コンストラクタのパラメータの型サイズ全て加算
                        counter += targetConstructer.GetParameters().Sum(t => Marshal.SizeOf(t.ParameterType));
                    }
                    else
                    {
                        //単純な変数
                        if (elementType.IsEnum)
                        {
                            //共用体
                            counter += Marshal.SizeOf(Enum.GetUnderlyingType(elementType));
                        }
                        else if (elementType.IsGenericType)
                        {
                            //generic
                            counter += elementType.GetFields(flags).Sum(t => Marshal.SizeOf(t.FieldType));
                        }
                        else
                        {
                            counter += Marshal.SizeOf(elementType);
                        }
                    }
                }
            }
            return counter;
        }

        //PacketReader.Readのメタ
        static MethodInfo MetaReads = typeof(PacketReader).GetMethod(nameof(PacketReader.ReadSingle));

        //一度作ったGenericMethodの辞書
        static Dictionary<Type, MethodInfo> genericMethodDictionary = new Dictionary<Type, MethodInfo>();

        //値のサイズのチェック
        static List<Type> TypeSizeCheck = new List<Type>();


        /// <summary>
        /// byte配列から構造体の生成
        /// 型のフィールド変数コンストラクタからフィールド変数を構築する
        /// </summary>
        /// <typeparam name="T">output type</typeparam>
        /// <param name="data">raw data</param>
        /// <returns></returns>
        public static T BytesToStructFromFieldConstructer<T>(byte[] data) where T : new()
         => (T)BytesToStructFromFieldConstructer(typeof(T), data);

        private static object BytesToStructFromFieldConstructer(Type type, byte[] data)
        {
            //サイズチェック（once）
            lock (TypeSizeCheck)
            {
                if (!TypeSizeCheck.Contains(type))
                {
                    int size = ParamatersSizeOf(type);
                    if (size != data.Length) throw new ArgumentOutOfRangeException($"型と配列のサイズが違います．{type.Name} Size: {size}, Data Size: {data.Length}");
                    else TypeSizeCheck.Add(type);
                }
            }

            object result = Activator.CreateInstance(type);

            var flags = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance;

            //復号化して読み込み
            using (PacketReader skillReader = new PacketReader(data))
            {
                //辞書にMethodInfo保存しつつ値読み込み
                object Read(Type readType)
                {
                    lock (genericMethodDictionary)
                    {
                        if (!genericMethodDictionary.ContainsKey(readType))
                        {
                            genericMethodDictionary[readType] = MetaReads.MakeGenericMethod(readType);
                        }
                        return genericMethodDictionary[readType].Invoke(skillReader, null);
                    }
                }

                //フィールド列挙
                foreach (var field in type.GetFields(flags))
                {
                    //配列の元のタイプ考慮したfield型取得
                    var elementType = field.FieldType.IsArray ? field.FieldType.GetElementType() : field.FieldType;

                    //配列サイズ　配列じゃない場合は1
                    int len = field.FieldType.IsArray || field.FieldType == typeof(string) ? field.GetCustomAttribute<MarshalAsAttribute>().SizeConst : 1;

                    //コンストラクタのリスト
                    var constructers = elementType.GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance);

                    //string
                    if (field.FieldType == typeof(string))
                    {
                        type.GetField(field.Name).SetValueDirect(__makeref(result), skillReader.ReadSjis(len));
                        continue;
                    }

                    //結果
                    var value = Array.CreateInstance(elementType, len);
                    var helper = (BytesToStructHelper)elementType.GetCustomAttribute(typeof(BytesToStructHelper), false);
                    var fieldHelper = (BytesToStructHelper)field.GetCustomAttribute(typeof(BytesToStructHelper), false);
                    for (int i = 0; i < len; i++)
                    {
                        if (helper?.BytesToStructFromFieldConstructer ?? false)
                        {
                            //再帰で取得
                            value.SetValue(BytesToStructFromFieldConstructer(elementType, skillReader.ReadBytes(helper.Size)), i);
                        }
                        else if (constructers.Length > 0)
                        {
                            //コンストラクタが存在
                            ConstructorInfo targetConstructer;

                            try
                            {
                                //選択されたprivateコンストラクタ
                                targetConstructer = constructers.Where(t => t.IsPrivate).Skip(fieldHelper?.ConstructerIndex ?? 0).First();
                            }
                            catch (InvalidOperationException ex)
                            {
                                throw new InvalidOperationException($"専用コンストラクタが存在しない，もしくは多いです：{elementType} {field.Name}", ex);
                            }

                            //コンストラクタのパラメータ取得
                            var paramaters = targetConstructer.GetParameters().Select(t => Read(t.ParameterType)).ToArray();

                            //コンストラクタ
                            value.SetValue(targetConstructer.Invoke(paramaters), i);
                        }
                        else
                        {
                            //単純な変数
                            value.SetValue(Read(elementType), i);
                        }
                    }

                    //代入
                    type.GetField(field.Name, flags).SetValueDirect(__makeref(result), field.FieldType.IsArray ? value : value.GetValue(0));
                }
            }
            return result;
        }

        /// <summary>
        /// stringからsjisのバイト列を生成
        /// 最大値指定可能
        /// </summary>
        /// <param name="str"></param>
        /// <param name="maxCount"></param>
        /// <returns></returns>
        public static byte[] StringToSjisByte(string str, int maxCount)
        {
            var bs = StringToSjisByte(str).ToList();

            //差
            int diff = maxCount - bs.Count;

            if (diff > 0)
            {
                //正なら0増やす
                bs.AddRange(Enumerable.Repeat<byte>(0, diff));
            }
            else if (diff < 0)
            {
                //負なら減らす
                bs.RemoveRange(bs.Count + diff, -diff);
            }
            return bs.ToArray();
        }

        /// <summary>
        /// stringからsjisのバイト列を生成
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static byte[] StringToSjisByte(string str)
            => Encoding.GetEncoding("Shift_JIS").GetBytes($"{str}\0");

        /// <summary>
        /// sjisのバイト列からstringを生成
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static string SjisByteToString(byte[] data)
        {
            return SjisByteToString(data, "Shift_JIS");
        }

        /// <summary>
        /// バイト列から文字コードcodeでデコード
        /// </summary>
        /// <param name="data"></param>
        /// <param name="code"></param>
        /// <returns></returns>
        public static string SjisByteToString(byte[] data, string code)
        {
            if (data.Length == 0) return null;
            Encoding sjisEnc = Encoding.GetEncoding(code);

            int num = 0;
            bool isBreak = false;
            for (int i = 0; i < data.Length; i++)
            {
                if (data[i] == 0)
                {
                    num = i;
                    isBreak = true;
                    break;
                }
            }
            if (!isBreak) num = data.Length;//0が最後までない場合
            data = data.Take(num).ToArray();
            return sjisEnc.GetString(data);
        }

        /// <summary>
        /// ユークリッド
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static double Hypot(double a, double b) =>
            Math.Sqrt(a * a + b * b);

        /// <summary>
        /// ユークリッド
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static double Hypot(double x1, double y1, double x2, double y2) =>
            Math.Sqrt(Math.Pow(x1 - x2, 2) + Math.Pow(y1 - y2, 2));

        /// <summary>
        /// 共用体フィールド作成
        /// </summary>
        /// <typeparam name="T">enumのベース構造体</typeparam>
        /// <param name="enumName"></param>
        /// <param name="src"></param>
        public static void EnumFieldMaker<T>(string enumName, Dictionary<T, string> src)
            where T : struct
        {
            //翻訳ファイルネーム
            string translate_fname = "please_translate.txt";

            //フィールドネーム
            string enumfield_fname = "enum_field.txt";

            //削除
            if (File.Exists(translate_fname)) File.Delete(translate_fname);

            //作成
            using (StreamWriter sw = new StreamWriter(new FileStream(translate_fname, FileMode.CreateNew, FileAccess.Write)))
            {
                src.Values.ToList().ForEach(t => sw.WriteLine(t));
            }

            //起動＆終了するまで待機
            System.Diagnostics.Process.Start("terapad.exe", translate_fname).WaitForExit();

            //整形
            Dictionary<string, int> distinctFieldName = new Dictionary<string, int>();
            string ShapingString(string str)
            {
                var r = Regex.Replace(str, @"^\w|\s\w", t => t.ToString().ToUpper());
                r = Regex.Replace(r, @"\s", "");
                r = Regex.Replace(r, @"\(([^\)]+)\)", t => "_" + t.Groups[1].Value.ToUpper());
                r = Regex.Replace(r, @"(\d+)%", "$1");
                r = Regex.Replace(r, @"%", "Percent");
                r = Regex.Replace(r, "'|/", "");
                r = Regex.Replace(r, "・|,", "_");
                if (distinctFieldName.ContainsKey(r))
                {
                    r += $"_{(++distinctFieldName[r]).ToString()}";
                }
                else
                {
                    distinctFieldName[r] = 0;
                }
                return r;
            }

            //翻訳済みからリスト作成
            Dictionary<T, (string comment, string fieldname)> withTranslated;

            using (StreamReader sr = new StreamReader(new FileStream(translate_fname, FileMode.Open, FileAccess.Read)))
            {
                withTranslated = src.Keys.ToDictionary(t => t, t => (src[t], ShapingString(sr.ReadLine())));
            }

            //翻訳削除
            if (File.Exists(translate_fname)) File.Delete(translate_fname);
            //前のフィールド削除
            if (File.Exists(enumfield_fname)) File.Delete(enumfield_fname);

            //フィールド作成
            using (StreamWriter sw = new StreamWriter(new FileStream(enumfield_fname, FileMode.CreateNew, FileAccess.Write)))
            {
                sw.WriteLine($"enum {enumName} : {typeof(T).Name}");
                sw.WriteLine("{");
                foreach (var key in withTranslated.Keys)
                {
                    sw.WriteLine($"[Comment(\"{withTranslated[key].comment}\")]");
                    sw.WriteLine($"{withTranslated[key].fieldname}={key.ToString()},");
                }
                sw.WriteLine("}");
            }
            System.Diagnostics.Process.Start("terapad.exe", enumfield_fname);
        }

        /// <summary>
        /// Effect.dat, OPEffect.datからenumフィールド作る 残しておく
        /// </summary>
        public static void EffectFieldMaker()
        {
            //ファイル名
            string fname = @"effect.dat";
            Dictionary<uint, (string comment, string valueName)> EffectDic = new Dictionary<uint, (string comment, string valueName)>();
            using (PacketReader br = new PacketReader(File.OpenRead(fname)))
            {
                for (int i = 0; i < (fname.Contains("op") ? 175 : 0x165); i++)
                {
                    uint index = br.ReadUInt32();
                    string str = br.ReadSjis(0x100);
                    str = Regex.Replace(str, @"<(c:\w*|n)>", "");
                    str = Regex.Replace(str, @"\r|\n", @"");

                    //コメント
                    string comment = str;

                    //変数名（未翻訳）
                    string valueName = Regex.Replace(str, @"\[(.?)0\]", @"$1 any");
                    valueName = Regex.Replace(valueName, @"\[(.?)1\]", @"$1 any");
                    valueName = Regex.Replace(valueName, @"\[(0*1)\]", @"any times any");
                    valueName = Regex.Replace(valueName, @"\+", @" plus ");
                    valueName = Regex.Replace(valueName, @"％", @" percent");
                    valueName = Regex.Replace(valueName, @"-", @" and");
                    valueName = Regex.Replace(valueName, @"  ", @" ");
                    valueName = Regex.Replace(valueName, @"^\s", @"");

                    EffectDic[index] = (comment, valueName);
                }
            }

            //未翻訳書き出し
            using(StreamWriter sw = new StreamWriter(new FileStream("toTranslate.txt", FileMode.CreateNew, FileAccess.Write)))
            {
                foreach(var key in EffectDic.Keys)
                {
                    sw.WriteLine(EffectDic[key].valueName);
                }
            }

            //読み込み
            List<string> translatedList = new List<string>();
            using (StreamReader sr = new StreamReader(new FileStream("translated_effect.txt", FileMode.Open, FileAccess.Read)))
            {
                uint counter = 0;
                while(sr.Peek() > -1)
                {
                    string translated = sr.ReadLine();
                    translated = Regex.Replace(translated, @"\s([a-z])", m => m.ToString().ToUpper());
                    translated = Regex.Replace(translated, @"\-([a-z])", m => m.ToString().ToUpper());
                    translated = Regex.Replace(translated, @"(\s|\.|\?|\/|\-|')", "");
                    translated = Regex.Replace(translated, @"~", "To");
                    translated = Regex.Replace(translated, @"\*", "Times");
                    translated = Regex.Replace(translated, @"(\(|\)|:|\[|\])", "_");
                    if (translatedList.Contains(translated))
                    {
                        int number = 2;
                        while (translatedList.Contains(translated + number.ToString())) number++;
                        translated += number.ToString();
                    }
                    translatedList.Add(translated);
                    EffectDic[counter] = (EffectDic[counter].comment, translated);
                    counter++;
                }
            }

            //フィールド化
            using (StreamWriter sw = new StreamWriter(new FileStream("field.txt", FileMode.Create, FileAccess.Write)))
            {
                foreach (var key in EffectDic.Keys)
                {
                    sw.WriteLine("/// <summary>");
                    sw.WriteLine("/// {0}", EffectDic[key].comment);
                    sw.WriteLine("/// </summary>");
                    sw.WriteLine("{0} = {1},", EffectDic[key].valueName, key);
                    sw.WriteLine();
                }
            }
        }
    }

    /// <summary>
    /// BytesToStructFromFieldConstructerの補助
    /// </summary>
    public class BytesToStructHelper : Attribute
    {
        public bool BytesToStructFromFieldConstructer;
        public int ConstructerIndex;
        public int Size;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="size">構造体の真のサイズ</param>
        /// <param name="bytesToStructFromFieldConstructer">コンストラクタから処理される</param>
        public BytesToStructHelper(int size = 0, bool bytesToStructFromFieldConstructer = false, int constructerIndex = 0)
        {
            Size = size;
            ConstructerIndex = constructerIndex;
            BytesToStructFromFieldConstructer = bytesToStructFromFieldConstructer;
        }
    }
    
    /// <summary>
    /// コメント属性
    /// </summary>
    public class Comment : Attribute
    {
        public string Str;

        public Comment(string str)
        {
            Str = str;
        }
    }

    /// <summary>
    /// 内部にリストを持ち，常に最大値を返すInt32
    /// </summary>
    public struct MaxInt32 : IConvertible, IFormattable
    {
        private List<int> _values;

        public static MaxInt32 operator +(MaxInt32 a, int b)
        {
            if (a._values == null)
            {
                a._values = new List<int>();
            }
            a._values.Add(b);
            return a;
        }

        public static MaxInt32 operator -(MaxInt32 a, int b)
        {
            a._values.Remove(b);
            if (a._values.Count == 0)
            {
                a._values = null;
            }
            return a;
        }

        /// <summary>
        /// 最大値
        /// </summary>
        private int value
            => _values?.Max() ?? int.MaxValue;

        public TypeCode GetTypeCode()
        {
            return value.GetTypeCode();
        }

        public bool ToBoolean(IFormatProvider provider)
        {
            return ((IConvertible)value).ToBoolean(provider);
        }

        public char ToChar(IFormatProvider provider)
        {
            return ((IConvertible)value).ToChar(provider);
        }

        public sbyte ToSByte(IFormatProvider provider)
        {
            return ((IConvertible)value).ToSByte(provider);
        }

        public byte ToByte(IFormatProvider provider)
        {
            return ((IConvertible)value).ToByte(provider);
        }

        public short ToInt16(IFormatProvider provider)
        {
            return ((IConvertible)value).ToInt16(provider);
        }

        public ushort ToUInt16(IFormatProvider provider)
        {
            return ((IConvertible)value).ToUInt16(provider);
        }

        public int ToInt32(IFormatProvider provider)
            => value;

        public uint ToUInt32(IFormatProvider provider)
        {
            return ((IConvertible)value).ToUInt32(provider);
        }

        public long ToInt64(IFormatProvider provider)
        {
            return ((IConvertible)value).ToInt64(provider);
        }

        public ulong ToUInt64(IFormatProvider provider)
        {
            return ((IConvertible)value).ToUInt64(provider);
        }

        public float ToSingle(IFormatProvider provider)
        {
            return ((IConvertible)value).ToSingle(provider);
        }

        public double ToDouble(IFormatProvider provider)
        {
            return ((IConvertible)value).ToDouble(provider);
        }

        public decimal ToDecimal(IFormatProvider provider)
        {
            return ((IConvertible)value).ToDecimal(provider);
        }

        public DateTime ToDateTime(IFormatProvider provider)
        {
            return ((IConvertible)value).ToDateTime(provider);
        }

        public string ToString(IFormatProvider provider)
        {
            return value.ToString(provider);
        }

        public object ToType(Type conversionType, IFormatProvider provider)
        {
            return ((IConvertible)value).ToType(conversionType, provider);
        }

        public override string ToString()
            => value.ToString();

        public string ToString(string format, IFormatProvider formatProvider)
                => ToString();
    }
}
