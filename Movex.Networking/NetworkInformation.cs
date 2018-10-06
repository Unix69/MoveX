using System;
using System.Collections.Generic;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace Movex.Network
{
    public class NetworkInformation
    {
        private IPAddress mLocalIP;
        private IPAddress mLocalSubnetMask;
        private IPAddress mLocalBroadcastAddress;

        public NetworkInformation()
        {
             mLocalIP = GetLocalIPAddress();
             mLocalSubnetMask = GetSubnetMask(mLocalIP);
             mLocalBroadcastAddress = GetBroadcastAddress(mLocalIP, mLocalSubnetMask);
        }

        // Return the local IP address
        public static IPAddress GetLocalIPAddress()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                    return ip;

            throw new Exception("Local IP Address not found.");
        }

        // Return the Subnet Mask of a given localIP
        public static IPAddress GetSubnetMask(IPAddress localIP)
        {
            foreach (var adapter in NetworkInterface.GetAllNetworkInterfaces()) {
                foreach (var unicastIPAddressInformation in adapter.GetIPProperties().UnicastAddresses) {
                    if (unicastIPAddressInformation.Address.AddressFamily == AddressFamily.InterNetwork) {

                        if (localIP.Equals(unicastIPAddressInformation.Address))
                            return unicastIPAddressInformation.IPv4Mask;

                    }
                }
            }

            throw new Exception("Local Subnet Mask Address not found.");
        }

        // Return the local Subnet Mask (the local IP is auto-computed)
        public static IPAddress GetLocalSubnetMask()
        {
            var localIP = GetLocalIPAddress();
            foreach (var adapter in NetworkInterface.GetAllNetworkInterfaces()) {
                foreach (var unicastIPAddressInformation in adapter.GetIPProperties().UnicastAddresses) {
                    if (unicastIPAddressInformation.Address.AddressFamily == AddressFamily.InterNetwork) {

                        if (localIP.Equals(unicastIPAddressInformation.Address))
                            return unicastIPAddressInformation.IPv4Mask;

                    }
                }
            }

            throw new Exception("Local Subnet Mask Address not found.");
        }

        // Return the Broadcast Address of a given localIP and Subnet Mask
        public static IPAddress GetBroadcastAddress(IPAddress localIP, IPAddress localSubnetMask)
        {
            //Get the SubnetMask bytes before finding the complement
            var complimentIP = new IPAddress(localSubnetMask.Address);
            var addressBytes = complimentIP.GetAddressBytes();
            var complimentBytes = new byte[4];

            //Get the compliment of the SubnetMask
            for (var i = 0; i < addressBytes.Length; i++)
            {
                complimentBytes[i] = (byte)~addressBytes[i];
            }

            //bitwise OR the compliment of the Subnet Mask and the host IP
            var localBytes = localIP.GetAddressBytes();
            var broadcastBytes = new byte[4];
            for (var i = 0; i < localBytes.Length; i++)
            {
                broadcastBytes[i] = (byte)(complimentBytes[i] | localBytes[i]);
            }

            return new IPAddress(broadcastBytes);
        }

        // Return the local Broadcast Address (localIP and Submnet Mask are auto-computed)
        public IPAddress GetLocalBroadcastAddress()
        {
            var localIP = GetLocalIPAddress();
            var localSubnetMask = GetSubnetMask(localIP);

            //Get the SubnetMask bytes before finding the complement
            var complimentIP = new IPAddress(localSubnetMask.Address);
            var addressBytes = complimentIP.GetAddressBytes();
            var complimentBytes = new byte[4];

            //Get the compliment of the SubnetMask
            for (var i = 0; i < addressBytes.Length; i++)
            {
                complimentBytes[i] = (byte)~addressBytes[i];
            }

            //bitwise OR the compliment of the Subnet Mask and the host IP
            var localBytes = localIP.GetAddressBytes();
            var broadcastBytes = new byte[4];
            for (var i = 0; i < localBytes.Length; i++)
            {
                broadcastBytes[i] = (byte)(complimentBytes[i] | localBytes[i]);
            }

            return new IPAddress(broadcastBytes);
        }

        /// <summary>
        /// Return localIP, localSubnetMask, localBroadcastAddress
        /// </summary>
        /// <returns>List of IP Addresses specified</returns>
        public List<IPAddress> GetLocalNetworkInformation()
        {
            var localNetworkInformation = new List<IPAddress>
            {
                mLocalIP,
                mLocalSubnetMask,
                mLocalBroadcastAddress
            };
            return localNetworkInformation;
        }
    }
}
        
