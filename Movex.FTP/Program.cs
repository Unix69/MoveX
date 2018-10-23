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
            var serverThread = new Thread(new ThreadStart(() => server.FTPstart()));
            serverThread.Start();
            serverThread.Join();
            serverThread.Interrupt();
            serverThread = null;
            return;

        }
    }
}
