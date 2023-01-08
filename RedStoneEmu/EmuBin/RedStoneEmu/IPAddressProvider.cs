using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using UPNPLib;

namespace RedStoneEmu
{
    /// <summary>
    /// IPアドレスを提供
    /// </summary>
    public static class IPAddressProvider
    {
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

                //余分なものを除いて一番上
                var result = adrList.Select(t => t.ToString()).Where(t => t.Contains("192.168.")).FirstOrDefault();

                return result;
            }
        }

        /// <summary>
        /// グローバルIPアドレス
        /// </summary>
        public static string GlobalIP
        {
            get
            {
                Logger.WriteInternal("グローバルIPアドレス取得中...");

                UPnPControlPoint p = new UPnPControlPoint();
                var result = p.GetExternalIPAddress();
                Logger.WriteInternal("完了：{0}", result);

                return result;
            }
        }

        /// <summary>
        /// ルータからグローバルIPアドレスを取得するクラス
        /// </summary>
        private class UPnPControlPoint
        {
            private UPnPService Service { get; set; }
            private UPnPDevice GetDevice(IUPnPDeviceFinder finder, string typeUri)
            {
                foreach (UPnPDevice item in finder.FindByType(typeUri, 0))
                {
                    return item;
                }
                return null;
            }
            private UPnPDevice GetDevice(IUPnPDeviceFinder finder, bool nextChance)
            {
                UPnPDevice device = null;
                if (!nextChance)
                {
                    device = this.GetDevice(finder, "urn:schemas-upnp-org:service:WANPPPConnection:1");
                }
                if (device == null || nextChance)
                {
                    device = this.GetDevice(finder, "urn:schemas-upnp-org:service:WANIPConnection:1");
                }
                return device;
            }
            private UPnPService GetService(UPnPDevice device, string serviceId)
            {
                try
                {
                    return device.Services[serviceId];
                }
                catch (ArgumentException)
                {
                    return null;
                }
            }
            private UPnPService GetService(UPnPDevice device)
            {
                UPnPService service = this.GetService(device, "urn:upnp-org:serviceId:WANPPPConn1");
                if (service == null)
                {
                    service = this.GetService(device, "urn:upnp-org:serviceId:WANIPConn1");
                }
                return service;
            }
            private UPnPService GetService(bool nextChance = false)
            {
                UPnPDevice device = this.GetDevice(new UPnPDeviceFinder(), nextChance);
                if (device == null)
                {
                    return null;
                }
                return this.GetService(device);
            }
            public UPnPControlPoint()
            {
                this.Service = GetService(true);
                if (this.Service == null)
                {
                    this.Service = GetService();
                }
            }
            private object InvokeAction(string bstrActionName, object vInActionArgs)
            {
                if (Service == null)
                {
                    return null;
                }
                try
                {
                    object result = new object();
                    Service.InvokeAction(bstrActionName, vInActionArgs, ref result);
                    return result;
                }
                catch (COMException)
                {
                    return null;
                }
            }
            public string GetExternalIPAddress()
            {
                object result = InvokeAction("GetExternalIPAddress", new object[] { });
                if (result == null)
                {
                    return null;
                }
                return (string)((object[])result)[0];
            }
            public void AddPortMapping(string remoteHost, ushort externalPort, string protocol, ushort internalPort, string internalClient, string description)
            {
                var arguments = new object[] { remoteHost, externalPort, protocol, internalPort, internalClient, true, description, 0 };
                InvokeAction("AddPortMapping", arguments);
            }
            public void DeletePortMapping(string remoteHost, ushort externalPort, string protocol)
            {
                var arguments = new object[] { remoteHost, externalPort, protocol };
                InvokeAction("DeletePortMapping", arguments);
            }
        }
    }
}
