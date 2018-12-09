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
        private IPAddress mAddress;
        private Thread mFtpClientThread;
        private DTransfer mDownloadTransfer;
        private ManualResetEvent mDTransferAvailability;
        private event EventHandler TransferCompleted;
        #endregion

        #region Constructor
        public DownloadProgressWindow(IPAddress address, ManualResetEvent dTransferAvailability)
        {
            // Initialize Window
            InitializeComponent();
            DataContext = new WindowViewModel(this);

            // Assigning member(s)
            mAddress = address;
            mDTransferAvailability = dTransferAvailability;

            // Manage event(s)
            Loaded += OnLoad;
            ContentRendered += Window_ContentRendered;
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
        private bool ChangeInterface() {

            return (true);
        }
        private void Worker_DoWork(object sender, DoWorkEventArgs e)
        {
            string progress;

            while (! ((progress = mDownloadTransfer.GetTransferPerc().ToString()).Equals("100")) )
            {
                if (int.TryParse(progress, out var x))
                {
                    (sender as BackgroundWorker).ReportProgress(x);
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
                IoC.Progress.Filename = filename;
            }

            IoC.Progress.Percentage = mDownloadTransfer.GetTransferPerc().ToString();
            var RemainingTime = HumanReadableTime.MillisecToHumanReadable(mDownloadTransfer.GetRemainingTime());
            if (RemainingTime == null)
            {
                IoC.Progress.RemainingTime = "Evaluating...";
            }
            else
            {
                IoC.Progress.RemainingTime = RemainingTime;
            }
        }
        private void Window_Close(object sender, EventArgs e)
        {
            if (mFtpClientThread != null)
            { 
                mFtpClientThread.Interrupt();
                mFtpClientThread = null;
            }
            Dispatcher.BeginInvoke(new Action(() => { Close(); }));
        }
        #endregion

        #region Utility method(s)
        private void AssignTransferInfoToViewModel(DTransfer dTransfer)
        {
            var ipAddress = dTransfer.GetFrom();
            if (ipAddress != null)
            {
                IoC.Progress.IpAddress = ipAddress;
                IoC.Progress.User = IoC.User.GetUsernameByIpAddress(ipAddress);
            }
            
            var filename = dTransfer.GetTransferFilename();
            if (filename != null)
            {
                IoC.Progress.Filename = filename;
            }

            IoC.Progress.Percentage = dTransfer.GetTransferPerc().ToString();
            var RemainingTime = HumanReadableTime.MillisecToHumanReadable(dTransfer.GetRemainingTime());
            if (RemainingTime == null)
            {
                IoC.Progress.RemainingTime = "Evaluating...";
            }
            else
            {
                IoC.Progress.RemainingTime = RemainingTime;
            }
        }
        #endregion
    }
}
