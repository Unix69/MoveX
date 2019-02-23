using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Movex.View.Core;
using System.Net;
using System.Windows;

namespace Movex.View
{
    public class WindowDispatcher : IWindowDispatcher
    {
        #region Const value(s)
        private const int YesNoRequest = 101;
        private const int WhereRequest = 102;
        private const int UploadTransferRequest = 103;
        private const int DownloadTransferRequest = 104;
        private const int MessageRequest = 105;
        private const int ResetFtpClient = 106;
        private const int ResetFtpServer = 107;
        private const int RemoveUploadTransferWindow = 108;
        private const int RemoveDownloadTransferWindow = 109;
        #endregion

        #region Private member(s)
        private Thread mInternaThread;
        #endregion

        #region Static member(s)
        public static ManualResetEvent RequestAvailable;
        public static ConcurrentQueue<string> Requests;
        public static ConcurrentDictionary<string, int> TypeRequests;
        public static ConcurrentDictionary<string, string> Messages;
        public static ConcurrentDictionary<string, ConcurrentBag<string>> Responses;
        public static ConcurrentDictionary<string, ManualResetEvent[]> Sync;
        #endregion

        public void Init()
        {
            RequestAvailable = new ManualResetEvent(false);
            Requests = new ConcurrentQueue<string>();
            TypeRequests = new ConcurrentDictionary<string, int>();
            Messages = new ConcurrentDictionary<string, string>();
            Responses = new ConcurrentDictionary<string, ConcurrentBag<string>>();
            Sync = new ConcurrentDictionary<string, ManualResetEvent[]>();
        }
        public void Start()
        {
            mInternaThread = new Thread(() => Loop());
            mInternaThread.Start();
        }
        public void Stop()
        {
            mInternaThread.Abort();
            mInternaThread = null;
        }

        private void Loop()
        {
            var threads = new List<Thread>();
            var windows = new ConcurrentDictionary<string, Window>();

            waitForRequest: RequestAvailable.WaitOne();
            RequestAvailable.Reset();

            // GET the REQUEST-ID
            Requests.TryDequeue(out var Id);
            if (Id == null) goto waitForRequest;

            // GET the REQUEST-TYPE
            TypeRequests.TryRemove(Id, out var type);

            Console.WriteLine("[Movex.FTP] [WindowDispatcher.xaml.cs] [Loop] Received a new type:" + type + " request with ID:" + Id + ".");

            switch (type)
            {
                case YesNoRequest:

                    /*
                     * I am expecting:
                     * syncPrimitives[0] as YesNoResponseAvailability ManualResetEvent
                     */

                    Sync.TryRemove(Id, out var YesNoResponseAvailability);
                    Messages.TryRemove(Id, out var message);
                    Responses.TryGetValue(Id, out var response);

                    var YesNoWindowThread = new Thread(() =>
                    {
                        var w = new YesNoWindow(YesNoResponseAvailability[0], message, response);
                        w.Show();
                        System.Windows.Threading.Dispatcher.Run();
                    });
                    YesNoWindowThread.SetApartmentState(ApartmentState.STA);
                    YesNoWindowThread.Start();
                    break;

                case WhereRequest:

                    /*
                     * I am expecting:
                     * syncPrimitives[0] as WhereResponseAvailability ManualResetEvent
                     */

                    Sync.TryRemove(Id, out var WhereResponseAvailability);
                    Messages.TryRemove(Id, out var whereMessage);
                    Responses.TryGetValue(Id, out var whereResponse);

                    var WhereWindowThread = new Thread(() =>
                    {
                        var w = new WhereWindow(WhereResponseAvailability[0], whereMessage, whereResponse);
                        w.Show();
                        System.Windows.Threading.Dispatcher.Run();
                    });
                    WhereWindowThread.SetApartmentState(ApartmentState.STA);
                    WhereWindowThread.Start();
                    break;

                case UploadTransferRequest:

                    /*
                     * I am expecting:
                     * syncPrimitives[0] as uploadTransferAvailability ManualResetEvent
                     * syncPrimitives[1] as windowAvailability ManualResetEvent
                     */

                    Sync.TryRemove(Id, out var syncVariables);
                    Messages.TryRemove(Id, out var ipAddress);

                    Console.WriteLine("[Movex.View] [WindowDispatcher.cs] [UploadTransferRequest] New WindowRequest for User with IpAddress: " + ipAddress);

                    var UploadProgressWindowThread = new Thread(() =>
                    {
                        var w = new UploadProgressWindow(IPAddress.Parse(ipAddress), syncVariables[0]);
                        windows.TryAdd(ipAddress, w);
                        w.Show();
                        syncVariables[1].Set();
                        Console.WriteLine("[Movex.View] [WindowDispatcher.cs] [UploadTransferRequest] Set the UploadProgressWindow as available. The transfer can take action.");
                        System.Windows.Threading.Dispatcher.Run();
                    });
                    UploadProgressWindowThread.SetApartmentState(ApartmentState.STA);
                    UploadProgressWindowThread.Name = ipAddress;
                    threads.Add(UploadProgressWindowThread);
                    UploadProgressWindowThread.Start();
                    break;

                case DownloadTransferRequest:

                    /*
                     * I am expecting:
                     * syncPrimitives[0] as downloadTransferAvailability ManualResetEvent
                     * syncPrimitives[1] as windowAvailability ManualResetEvent
                     */

                    Sync.TryRemove(Id, out var syncPrimitives);
                    Messages.TryRemove(Id, out var ip);

                    Console.WriteLine("[Movex.View] [WindowDispatcher.cs] [DownloadTransferRequest] New WindowRequest from User with IpAddress: " + ip);

                    var DownloadProgressWindowThread = new Thread(() =>
                    {
                        var w = new DownloadProgressWindow(IPAddress.Parse(ip), syncPrimitives[0]);
                        windows.TryAdd(ip, w);
                        w.Show();
                        syncPrimitives[1].Set();
                        Console.WriteLine("[Movex.View] [WindowDispatcher.cs] [DownloadTransferRequest] Set the DonwloadProgressWindow as available. The transfer can take action.");
                        System.Windows.Threading.Dispatcher.Run();
                    });
                    DownloadProgressWindowThread.SetApartmentState(ApartmentState.STA);
                    DownloadProgressWindowThread.Name = ip;
                    threads.Add(DownloadProgressWindowThread);
                    DownloadProgressWindowThread.Start();
                    break;

                case MessageRequest:

                    Messages.TryRemove(Id, out var msg);
                    Console.WriteLine("[Movex.View] [WindowDispatcher.cs] [MessageRequest] New MessageWindow requested.");

                    var MessageWindowThread = new Thread(() =>
                    {
                        var w = new MessageWindow(msg);
                        w.Show();
                        Console.WriteLine("[Movex.View] [WindowDispatcher.cs] [MessageRequest] Showing the MessageWindow requested.");
                        System.Windows.Threading.Dispatcher.Run();
                    });
                    MessageWindowThread.SetApartmentState(ApartmentState.STA);
                    MessageWindowThread.Start();
                    break;

                case ResetFtpClient:

                    IoC.FtpClient.Reset();
                    break;

                case ResetFtpServer:

                    IoC.FtpServer.Reset();
                    break;

                case RemoveUploadTransferWindow:

                    Messages.TryGetValue(Id, out var ipAddressReceiver);

                    windows.TryRemove(ipAddressReceiver, out var uploadProgressWindow);
                    if (uploadProgressWindow != null)
                    {
                        ((UploadProgressWindow)uploadProgressWindow).Interrupt();
                    }
                    
                    /*
                    foreach (var t in threads.ToList())
                    {
                        if (ipAddressReceiver.Equals(t.Name))
                        {
                            t.Interrupt();
                            threads.Remove(t);
                        }
                    }
                    */
                    break;

                case RemoveDownloadTransferWindow:

                    Messages.TryGetValue(Id, out var ipAddressSender);

                    windows.TryRemove(ipAddressSender, out var downloadProgressWindow);
                    if (downloadProgressWindow != null)
                    {
                        ((DownloadProgressWindow)downloadProgressWindow).Interrupt();
                    }
                    break;

                default:
                    break;
            }

            goto waitForRequest;
        }
    }
}
