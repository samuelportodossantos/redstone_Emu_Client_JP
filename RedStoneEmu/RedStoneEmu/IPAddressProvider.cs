using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace RedStoneEmu
{
    /// <summary>
    /// IPアドレスを提供
    /// </summary>
    public static class IPAddressProvider
    {
        static IPAddressProvider()
        {
            //グローバルIP取得
            WebRequest webreq =　WebRequest.Create("http://checkip.dyndns.org/");

            //サーバーからの応答を受信するためのHttpWebResponseを取得
            WebResponse webres = webreq.GetResponse();
            //応答データを受信するためのStreamを取得
            System.IO.Stream st = webres.GetResponseStream();
            //文字コードを指定して、StreamReaderを作成
            System.IO.StreamReader sr =new System.IO.StreamReader(st, Encoding.UTF8);
            //データをすべて受信
            string htmlSource = sr.ReadToEnd();
            //閉じる
            sr.Close();
            st.Close();
            webres.Close();

            GlobalIP = Regex.Match(htmlSource, @"\d+\.\d+\.\d+\.\d+").Value;
        }

        /// <summary>
        /// ローカルIPアドレス
        /// </summary>
        public static string LocalIP
        {
            get
            {
                // ホスト名を取得する
                string hostname = Dns.GetHostName();

                // ホスト名からIPアドレスを取得する
                IPAddress[] adrList = Dns.GetHostAddresses(hostname);

                string result = "";
                if (adrList.Any(t => t.ToString().Contains("192.168.")))
                {
                    //余分なものを除いて一番上
                    result = adrList.Select(t => t.ToString()).Where(t => t.Contains("192.168.")).First();
                }
                else
                {
                    //存在しない場合は自IP
                    result = "127.0.0.1";
                }

                if (result == "")
                {
                    throw new Exception("ローカルIPが特定できません．");
                }

                return result;
            }
        }

        /// <summary>
        /// グローバルIP
        /// </summary>
        public static string GlobalIP { get; private set; }
    }
}
