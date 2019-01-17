using System;
using System.Windows;
using System.Threading;
using System.ComponentModel;
using Movex.View.Core;
using System.Net;
using Movex.FTP;

namespace Movex.View
{
    /// <summary>
    /// Interaction logic for ProgressWindow.xaml
    /// </summary>
    public partial class UploadProgressWindow : Window
    {
        #region Private members
        private string[] mPaths;
        private int mIndexCurrentTransfer;
        private IPAddress mAddress;
        private Thread mInterruptTransferWaiter;
        private UTransfer mUploadTransfer;
        private ManualResetEvent mUTransferAvailability;
        private ManualResetEvent mCloseWindow;
        private event EventHandler TransferCompleted;
        private event EventHandler TransferInterrupted;
        private ProgressDesignModel mProgress;
        #endregion

        #region Constructor(s)
        public UploadProgressWindow(IPAddress address, ManualResetEvent uTransferAvailability)
        {
            // Initialize Window
            InitializeComponent();
            DataContext = new WindowViewModel(this);

            // Set the internal ProgressControl
            mCloseWindow = new ManualResetEvent(false);
            mProgress = (ProgressDesignModel)InternalProgressControl.DataContext;
            mProgress.SetCloseWindowEventHandler(mCloseWindow);
            mProgress.Type = "Upload";
            mInterruptTransferWaiter = new Thread(() => OnTransferInterrupted());
            mInterruptTransferWaiter.Start();

            // Assigning member(s)
            mAddress = address;
            mUTransferAvailability = uTransferAvailability;
            mIndexCurrentTransfer = 0;

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
        private void Worker_DoWork(object sender, DoWorkEventArgs e)
        {
            var progress = "0";
            var lastProgress = "0";
            var interruptionRisk = 0;

            while (!((progress = ((int)mUploadTransfer.GetTransferPerc()).ToString()).Equals("100")))
            {
                if (int.TryParse(progress, out var x))
                {
                    if (progress.Equals(lastProgress)) { interruptionRisk++; } else { interruptionRisk = 0; }
                    if (interruptionRisk >= 35) { mCloseWindow.Set(); }

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

            var filename = mUploadTransfer.GetTransferFilename();
            if (filename != null)
            {
                mProgress.Filename = filename;
            }

            mProgress.Percentage = ((int)mUploadTransfer.GetTransferPerc()).ToString();

            var RemainingTime = HumanReadableTime.MillisecToHumanReadable(mUploadTransfer.GetRemainingTime());
            if (RemainingTime != null)
            {
                mProgress.RemainingTime = RemainingTime;
            }
        }
        private void Window_Close(object sender, EventArgs e)
        {
            Dispatcher.BeginInvoke(new Action(() => { Close(); }));
            IoC.FtpClient.Reset();
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
        private void AssignTransferInfoToViewModel(UTransfer uTransfer)
        {
            var ipAddress = uTransfer.GetTo();
            if (ipAddress != null)
            {
                mProgress.IpAddress = ipAddress;
                mProgress.User = IoC.User.GetUsernameByIpAddress(ipAddress);
            }
            
            var filename = uTransfer.GetTransferFilename();
            if (filename != null)
            {
                mProgress.Filename = filename;
            }

            mProgress.Percentage = ((int)uTransfer.GetTransferPerc()).ToString();

            var RemainingTime = HumanReadableTime.MillisecToHumanReadable(uTransfer.GetRemainingTime());
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
    }
}
