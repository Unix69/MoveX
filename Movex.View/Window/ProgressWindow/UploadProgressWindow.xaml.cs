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
    /// Interaction logic for the UploadProgressWindow
    /// </summary>
    public partial class UploadProgressWindow : Window
    {
        #region Private members
        private ProgressDesignModel mProgress;
        private UTransfer mUploadTransfer;
        private ManualResetEvent mUTransferAvailability;
        private ManualResetEvent mCloseWindow;
        private event EventHandler TransferCompleted;
        private event EventHandler TransferInterrupted;
        private IPAddress mAddress;
        private Thread mInterruptTransferWaiter;
        private bool mIsInterrupted;
        #endregion

        #region Constructor
        public UploadProgressWindow(IPAddress address, ManualResetEvent uTransferAvailability)
        {
            // Initialize Window
            InitializeComponent();
            DataContext = new WindowViewModel(this);

            // Set the CloseWindow Reset Event
            mCloseWindow = new ManualResetEvent(false);

            // Set the internal ProgressControl
            mProgress = (ProgressDesignModel)InternalProgressControl.DataContext;
            mProgress.SetCloseWindowEventHandler(mCloseWindow);
            mProgress.Type = "Upload";

            // Set a handler for the interruption of the transfer
            mInterruptTransferWaiter = new Thread(() => WaitForTransferInterrupted());
            mInterruptTransferWaiter.Start();

            // Assigning member(s)
            mAddress = address;
            mUTransferAvailability = uTransferAvailability;
            mIsInterrupted = false;

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
            try
            {
                mUTransferAvailability.WaitOne();
                if (!mIsInterrupted)
                {
                    mUploadTransfer = IoC.FtpClient.GetTransfer(mAddress);
                    AssignTransferInfoToViewModel(mUploadTransfer);
                }
            }
             catch(Exception Exception)
            {
                var Message = Exception.Message;
                Console.WriteLine("[MOVEX.VIEW] [UploadProgressWindow.xaml.cs] [OnLoad] " + Message + ".");
            }
            
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

            while (!mIsInterrupted && !((progress = ((int)mUploadTransfer.GetTransferPerc()).ToString()).Equals("100")))
            {
                if (int.TryParse(progress, out var x))
                {
                    if (progress.Equals(lastProgress)) { interruptionRisk++; } else { interruptionRisk = 0; }
                    if (interruptionRisk >= 100) { mCloseWindow.Set(); }

                    (sender as BackgroundWorker).ReportProgress(x);
                    lastProgress = progress;
                }
                Thread.Sleep(100);
            }

            if (!(progress == null) && progress.Equals("100"))
            {
                Console.WriteLine("[Movex.View] [UploadProgressWindow.xaml.cs] [Worker_DoWork] 100% COMPLETED.");

                // IF 100% COMPLETED: 1. show the perc
                if (int.TryParse(progress, out var x))
                {
                    (sender as BackgroundWorker).ReportProgress(x);
                }

                // IF 100% COMPLETED: 2. wait a second
                Thread.Sleep(1000);

                // IF 100% COMPLETED: 3. close the window
                TransferCompleted.Invoke(this, EventArgs.Empty);
            }
        }
        private void Worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            try
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
            catch (Exception Exception)
            {
                var Message = Exception.Message;
                Console.WriteLine("[MOVEX.VIEW] [DownloadProgressWindow.xaml.cs] [Worker_ProgressChanged] " + Message + ".");
            }
        }
        private void Window_Close(object sender, EventArgs e)
        {
            Console.WriteLine("[Movex.View] [UploadProgressWindow.xaml.cs] [Window_Close] Closing the UploadProgressWindow.");
            try
            {
                mUTransferAvailability.Set();
                Console.WriteLine("[Movex.View] [UploadProgressWindow.xaml.cs] [Window_Close] Closed the UploadProgressWindow and released UploadProgressWindow Thread.");
                if (mIsInterrupted)
                {
                    ((App)Application.Current).GetWindowRequester().AddMessageWindow("Il trasferimento è stato interrotto.");
                    IoC.FtpClient.Reset();
                }
            }
            catch (Exception Exception)
            {
                var Message = Exception.Message;
                Console.WriteLine("[MOVEX.View] [UploadProgressWindow.xaml.cs] [Window_Close]" + Message + ".");
            }
            Dispatcher.BeginInvoke(new Action(() => {

                try
                {
                    
                    Close();
                    Thread.CurrentThread.Interrupt();
                }
                catch (Exception Exception)
                {
                    var Message = Exception.Message;
                    Console.WriteLine("[MOVEX.View] [UploadProgressWindow.xaml.cs] [Window_Close]" + Message + ".");
                }
            }));
        }
        public void WaitForTransferInterrupted()
        {
            mCloseWindow.WaitOne();
            Interrupt();
        }
        public void Interrupt()
        {
            mIsInterrupted = true;
            TransferInterrupted.Invoke(this, EventArgs.Empty);
            ((App)Application.Current).RemoveThread("SendThread");
        }

        #endregion

        #region Utility method(s)
        private void AssignTransferInfoToViewModel(UTransfer uTransfer)
        {
            try
            {
                Console.WriteLine("[Movex.View] [UploadProgressWindow.xaml.cs] [AssignTransferInfoViewModel] Trying to assign the DownloadTransferInfo to the ViewModel.");

                var ipAddress = mAddress.ToString();
                if (ipAddress != null)
                {
                    mProgress.IpAddress = ipAddress;
                    Console.WriteLine("[Movex.View] [UploadProgressWindow.xaml.cs] [AssignTransferInfoViewModel] Assigned IpAddress: " + mProgress.IpAddress);

                    mProgress.User = IoC.User.GetUsernameByIpAddress(ipAddress);
                    Console.WriteLine("[Movex.View] [UploadProgressWindow.xaml.cs] [AssignTransferInfoViewModel] Assigned User: " + mProgress.User);
                }

                var filename = uTransfer.GetTransferFilename();
                if (filename != null)
                {
                    mProgress.Filename = filename;
                    Console.WriteLine("[Movex.View] [UploadProgressWindow.xaml.cs] [AssignTransferInfoViewModel] Assigned Filename: " + mProgress.Filename);
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
            catch (Exception Exception)
            {
                var Message = Exception.Message;
                Console.WriteLine("[MOVEX.VIEW] [UploadProgressWindow.xaml.cs] [AssignTransferInfoViewModel] " + Message + ".");

                throw Exception;
            }
            
        }
        #endregion
    }
}
