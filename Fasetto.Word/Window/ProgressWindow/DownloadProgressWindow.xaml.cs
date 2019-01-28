using System;
using System.Windows;
using System.Threading;
using System.ComponentModel;
using Movex.View.Core;
using System.Net;
using Movex.FTP;
using System.Collections.Generic;

namespace Movex.View
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class DownloadProgressWindow : System.Windows.Window
    {
        #region Private members
        private string[] mPaths;
        private int mIndexCurrentTransfer;
        private IPAddress mAddress;
        private Thread mInterruptTransferWaiter;
        private DTransfer mDownloadTransfer;
        private ManualResetEvent mDTransferAvailability;
        private ManualResetEvent mCloseWindow;
        private event EventHandler TransferCompleted;
        private event EventHandler TransferInterrupted;
        private ProgressDesignModel mProgress;
        #endregion

        #region Constructor
        public DownloadProgressWindow(IPAddress address, ManualResetEvent dTransferAvailability)
        {
            // Initialize Window
            InitializeComponent();
            DataContext = new WindowViewModel(this);

            // Set the internal ProgressControl
            mCloseWindow = new ManualResetEvent(false);
            mProgress = (ProgressDesignModel)InternalProgressControl.DataContext;
            mProgress.SetCloseWindowEventHandler(mCloseWindow);
            mProgress.Type = "Download";
            mInterruptTransferWaiter = new Thread(() => OnTransferInterrupted());
            mInterruptTransferWaiter.Start();

            // Assigning member(s)
            mAddress = address;
            mDTransferAvailability = dTransferAvailability;

            // Manage event(s)
            Loaded += OnLoad;
            ContentRendered += Window_ContentRendered;
            TransferInterrupted += Window_Close;
            TransferCompleted += Window_Close;
        }
        #endregion

        #region Event Handler(s)
        private void OnLoad(object sender, RoutedEventArgs e)
        {
            // Wait for data from FTPclient to load them in the Window
            mDTransferAvailability.WaitOne();
            mDownloadTransfer = IoC.FtpServer.GetTransfer(mAddress.ToString());
            AssignTransferInfoToViewModel(mDownloadTransfer);
        }
        private void Window_ContentRendered(object sender, EventArgs e)
        {
            var worker = new BackgroundWorker()
            {
                WorkerReportsProgress = true
            };

            worker.DoWork += Worker_DoWork;
            worker.ProgressChanged += Worker_ProgressChanged;
            worker.RunWorkerAsync();
        }
        private void Worker_DoWork(object sender, DoWorkEventArgs e)
        {
            var progress = "0";
            var lastProgress = "0";
            var interruptionRisk = 0;

            while (!((progress = ((int)mDownloadTransfer.GetTransferPerc()).ToString()).Equals("100")))
            {
                if (int.TryParse(progress, out var x))
                {
                    if (progress.Equals(lastProgress)) { interruptionRisk++; } else { interruptionRisk = 0; }
                    if (interruptionRisk >= 35) {  /* mCloseWindow.Set(); */ }

                    (sender as BackgroundWorker).ReportProgress(x);
                    lastProgress = progress;
                }
                Thread.Sleep(100);
            }

            // IF 100% COMPLETED: 1. show the perc
            if (!(progress == null) && progress.Equals("100"))
                if (int.TryParse(progress, out var x))
                    (sender as BackgroundWorker).ReportProgress(x);

            // IF 100% COMPLETED: 2. wait a second
            Thread.Sleep(1000);

            // IF 100% COMPLETED: 3. close the window
            TransferCompleted.Invoke(this, EventArgs.Empty);

        }
        private void Worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            var filename = mDownloadTransfer.GetTransferFilename();
            if (filename != null)
            {
                mProgress.Filename = filename;
            }

            mProgress.Percentage = ((int)mDownloadTransfer.GetTransferPerc()).ToString();
            var RemainingTime = HumanReadableTime.MillisecToHumanReadable(mDownloadTransfer.GetRemainingTime());
            if (RemainingTime == null)
            {
                mProgress.RemainingTime = "Evaluating...";
            }
            else
            {
                mProgress.RemainingTime = RemainingTime;
            }
        }
        private void Window_Close(object sender, EventArgs e)
        {
            Dispatcher.BeginInvoke(new Action(() => { Close(); }));
        }
        private void OnTransferInterrupted()
        {
            mCloseWindow.WaitOne();
            TransferInterrupted.Invoke(this, EventArgs.Empty);

            // Launch a message window
            var MessageWindowThread = new Thread(() =>
            {
                var w = new MessageWindow("Il trasferimento è stato interrotto.");
                w.Show();
                System.Windows.Threading.Dispatcher.Run();
            });
            MessageWindowThread.SetApartmentState(ApartmentState.STA);
            MessageWindowThread.Start();
        }
        #endregion

        #region Utility method(s)
        private void AssignTransferInfoToViewModel(DTransfer dTransfer)
        {
            var ipAddress = dTransfer.GetFrom();
            if (ipAddress.Contains(":"))
            {
                ipAddress = ipAddress.Split(':')[0];
            }
            if (ipAddress != null)
            {
                mProgress.IpAddress = ipAddress;
                mProgress.User = IoC.User.GetUsernameByIpAddress(ipAddress);
            }
            
            var filename = dTransfer.GetTransferFilename();
            if (filename != null)
            {
                mProgress.Filename = filename;
            }

            mProgress.Percentage = ((int)dTransfer.GetTransferPerc()).ToString();
            var RemainingTime = HumanReadableTime.MillisecToHumanReadable(dTransfer.GetRemainingTime());
            if (RemainingTime == null)
            {
                mProgress.RemainingTime = "Evaluating...";
            }
            else
            {
                mProgress.RemainingTime = RemainingTime;
            }
        }
        #endregion

        #region Getter(s) and Setter(s)
        public IPAddress GetIpAddress() { return mAddress; }
        #endregion

    }
}
