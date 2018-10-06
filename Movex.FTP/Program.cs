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
        static void Main(string [] args) { 

            var server = new FTPserver(2000, false, false, false, @".\Downloads\");
            var client = new FTPclient();
            var paths = new string[1];
            var ip = new IPAddress[1];

            var serverThread = new Thread(new ThreadStart(() => server.FTPstart()));
            serverThread.Start();
            ip[0] = IPAddress.Parse("127.0.0.1");
            var ipepoint = new IPEndPoint(ip[0], 2000);
            paths[0] = Path.Combine(Directory.GetCurrentDirectory(), "Intro.pdf");

            // Show Info Window
            var strB = new StringBuilder();
            strB.AppendLine("I am going to send:");
            strB.AppendLine("FILE: " + paths[0]);
            strB.AppendLine("DESTINATION: " + ip[0].ToString());



            // client.FTPsendAll(paths, ip, 100);
            // Console.WriteLine(path);

            

            serverThread.Join();
            serverThread.Interrupt();
            serverThread = null;

            return;
        }

    }

}
