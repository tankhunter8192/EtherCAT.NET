using EtherCAT.NET;
using EtherCAT.NET.Infrastructure;
using System.Net.NetworkInformation;

namespace WebMaster.Store
{
    public static class TempStore
    {
        /*
         * Here are all network interfaces stores. Gets filled on startup and/or on manual scan request for adding nics
         */
        public static List<NetworkInterface> NetworkInterfacesAll { get; set; } = new();
        /*
         * List for not used network interfaces: 
         * if a network interface gets an EcMaster -> remove
         * if a network interface gets a connection to user -> remove (optinal, maybe is combi mode possible)
         */
        public static List<NetworkInterface> NetworkInterfacesAvil { get; set; } = new();
        /*
         *EcMasters 
         */
        public static List<EcMaster> ecMasters { get; set; } = new();
        /*
         * RootSlaves for the ecMasters
         */
        public static List<SlaveInfo> RootSlaves { get; set; } = new();
        public static List<List<SlaveInfo>> SlaveLists { get; set; } = new();
        /*
         * 
         */
        public static string ESIpath { get; set; } = "";
    }
}
