﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Movex.View.Core;
using System.Net;

namespace Movex.View
{
    public class WindowDispatcher : IWindowDispatcher
    {
        #region Const value(s)
        private const int YesNoRequest = 101;
        private const int WhereRequest = 102;
        private const int UploadTransferRequest = 103;
        private const int DownloadTransferRequest = 104;
        #endregion

        #region Private member(s)
        private ConcurrentStack<Window> mWindows;
        private Thread mInternaThread;
        private Thread mLooper;
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
            mWindows = new ConcurrentStack<Window>();
            RequestAvailable = new ManualResetEvent(false);
            Requests = new ConcurrentQueue<string>();
            TypeRequests = new ConcurrentDictionary<string, int>();
            Messages = new ConcurrentDictionary<string, string>();
            Responses = new ConcurrentDictionary<string, ConcurrentBag<string>>();
            Sync = new ConcurrentDictionary<string, ManualResetEvent[]>();
        }
        public void Start() {
            mInternaThread = new Thread(() => Loop());
            mInternaThread.Start();
        }
        public void Stop() {
            mLooper.Abort();
            mInternaThread.Abort();
            mWindows.Clear();
            mLooper = null;
            mInternaThread = null;
        }

        private void Loop()
        {
            mLooper = new Thread(() =>
            {
                waitForRequest: RequestAvailable.WaitOne();
                RequestAvailable.Reset();
                
                // GET the REQUEST-ID
                Requests.TryDequeue(out var Id);
                if (Id == null) goto waitForRequest;

                // GET the REQUEST-TYPE
                TypeRequests.TryGetValue(Id, out var type);

                switch (type)
                {
                    case YesNoRequest:

                        Sync.TryGetValue(Id, out var YesNoResponseAvailability);
                        Messages.TryGetValue(Id, out var message);
                        Responses.TryGetValue(Id, out var response);

                        var YesNoWindowThread = new Thread(() =>
                        {
                            var w = new YesNoWindow(YesNoResponseAvailability[0], message, response);
                            mWindows.Push(w);
                            w.Show();
                            System.Windows.Threading.Dispatcher.Run();
                        });
                        YesNoWindowThread.SetApartmentState(ApartmentState.STA);
                        YesNoWindowThread.Start();
                        break;

                    case WhereRequest:

                        Sync.TryGetValue(Id, out var WhereResponseAvailability);
                        Messages.TryGetValue(Id, out var whereMessage);
                        Responses.TryGetValue(Id, out var whereResponse);

                        var WhereWindowThread = new Thread(() =>
                        {
                            var w = new WhereWindow(WhereResponseAvailability[0], whereMessage, whereResponse);
                            mWindows.Push(w);
                            w.Show();
                            System.Windows.Threading.Dispatcher.Run();
                        });
                        WhereWindowThread.SetApartmentState(ApartmentState.STA);
                        WhereWindowThread.Start();
                        break;

                    case UploadTransferRequest:

                        /*
                         * I am expecting:
                         * syncPrimitives[0] as downloadTransferAvailability ManualResetEvent
                         * syncPrimitives[1] as windowAvailability ManualResetEvent
                         */

                        Sync.TryGetValue(Id, out var syncVariables);
                        Messages.TryGetValue(Id, out var ipAddress);

                        var UploadProgressWindowThread = new Thread(() =>
                        {
                            var w = new ProgressWindow(IPAddress.Parse(ipAddress), syncVariables[0]);
                            mWindows.Push(w);
                            w.Show();
                            syncVariables[1].Set();
                            System.Windows.Threading.Dispatcher.Run();
                        });
                        UploadProgressWindowThread.SetApartmentState(ApartmentState.STA);
                        UploadProgressWindowThread.Start();
                        break;

                    case DownloadTransferRequest:

                        /*
                         * I am expecting:
                         * syncPrimitives[0] as downloadTransferAvailability ManualResetEvent
                         * syncPrimitives[1] as windowAvailability ManualResetEvent
                         */

                        Sync.TryGetValue(Id, out var syncPrimitives);
                        Messages.TryGetValue(Id, out var ip);

                        var DownloadProgressWindowThread = new Thread(() =>
                        {
                            var w = new DownloadProgressWindow(IPAddress.Parse(ip), syncPrimitives[0]);
                            mWindows.Push(w);
                            w.Show();
                            syncPrimitives[1].Set();
                            System.Windows.Threading.Dispatcher.Run();
                        });
                        DownloadProgressWindowThread.SetApartmentState(ApartmentState.STA);
                        DownloadProgressWindowThread.Start();
                        break;

                    default:
                        break;
                }
                
                goto waitForRequest;

            });
            mLooper.Start();
        }
    }
}
