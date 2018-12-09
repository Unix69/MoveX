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
    public partial class ProgressWindow : System.Windows.Window
    {
        #region Private members
        private Movex.View.Core.FTPclient mFtpClient;
        private string[] mPaths;
        private int mIndexCurrentTransfer;
        private IPAddress mAddress;
        private Thread mFtpClientThread;
        private UTransfer mUploadTransfer;
        private ManualResetEvent mUTransferAvailability;
        private event EventHandler TransferCompleted;
        #endregion

        // Constructor(s)
        public ProgressWindow()
        {
            InitializeComponent();
            DataContext = new WindowViewModel(this);
        }

        public ProgressWindow(IPAddress address, ManualResetEvent uTransferAvailability)
        {
            // Initialize Window
            InitializeComponent();
            DataContext = new WindowViewModel(this);

            // Assigning member(s)
            mAddress = address;
            mUTransferAvailability = uTransferAvailability;
            mIndexCurrentTransfer = 0;

            // Manage event(s)
            Loaded += OnLoad;
            ContentRendered += Window_ContentRendered;
            TransferCompleted += Window_Close;
        }

        #region Event Handler(s)
        private void OnLoad(object sender, RoutedEventArgs e)
        {
            // Wait for data from FTPclient to load them in the Window
            mUTransferAvailability.WaitOne();
            mUploadTransfer = IoC.FtpClient.GetTransfer(mAddress);
            AssignTransferInfoToViewModel(mUploadTransfer);
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

            while (! ((progress = mUploadTransfer.GetTransferPerc().ToString()).Equals("100")) )
            {
                if (int.TryParse(progress, out int x))
                {
                    (sender as BackgroundWorker).ReportProgress(x);
                }
                Thread.Sleep(100);
            }

            // IF 100% COMPLETED: 1. show the perc
            if (!(progress == null) && progress.Equals("100"))
                if (int.TryParse(progress, out int x))
                    (sender as BackgroundWorker).ReportProgress(x);

            // IF 100% COMPLETED: 2. wait a second
            Thread.Sleep(1000);

            // IF 100% COMPLETED: 3. close the window
            TransferCompleted.Invoke(this, EventArgs.Empty);

        }
        private void Worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {

            var filename = mUploadTransfer.GetTransferFilename();
            if (filename != null)
            {
                IoC.Progress.Filename = filename;
            }

            IoC.Progress.Percentage = mUploadTransfer.GetTransferPerc().ToString();
            IoC.Progress.RemainingTime = HumanReadableTime.MillisecToHumanReadable(mUploadTransfer.GetRemainingTime());

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

        private void AssignTransferInfoToViewModel(UTransfer uTransfer)
        {

            var ipAddress = uTransfer.GetTo();
            if (ipAddress != null)
            {
                IoC.Progress.IpAddress = ipAddress;
                IoC.Progress.User = ipAddress;
            }
            
            var filename = uTransfer.GetTransferFilename();
            if (filename != null)
            {
                IoC.Progress.Filename = filename;
            }

            IoC.Progress.Percentage = uTransfer.GetTransferPerc().ToString();
            IoC.Progress.RemainingTime = HumanReadableTime.MillisecToHumanReadable(uTransfer.GetRemainingTime());
        }
    }
}
