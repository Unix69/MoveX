using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Movex.FTP
{
    class Program
    {
        static void Main(string[] args)
        {
            var server = new FTPserver(2000, false, false, false, @".\Downloads\");
            var client = new FTPclient();
            var paths = new string[3];
            var ip = new IPAddress[1];
            var serverThread = new Thread(new ThreadStart(() => server.FTPstart()));
            serverThread.Start();
            ip[0] = IPAddress.Parse("127.0.0.2");
            paths[0] = @".\A";
            paths[1] = @".\B";
            paths[2] = @".\ciao.txt";
            client.FTPsendAll(paths, ip, 100, null, null);
            serverThread.Join();
            serverThread.Interrupt();
            serverThread = null;
            return;

        }
    }
}
