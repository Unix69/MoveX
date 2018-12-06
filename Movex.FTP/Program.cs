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
            var paths = new string[2];
            var ip = new IPAddress[1];
            var serverThread = new Thread(new ThreadStart(() => server.FTPstart()));
            serverThread.Start();
            ip[0] = IPAddress.Parse("127.0.0.1");
            paths[0] = @".\ciao.txt";
            paths[1] = @".\ciao.txt";
            /*
            ManualResetEvent[] transferAvailability = new ManualResetEvent[1];
            transferAvailability[0] = new ManualResetEvent(false);
            client.FTPsendAll(paths, ip, transferAvailability);
            serverThread.Join();
            serverThread.Interrupt();
            serverThread = null;
            */
            return;

        }
    }
}
