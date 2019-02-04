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
    public partial class DownloadProgressWindow : Window
    {
        #region Private members
        private ProgressDesignModel mProgress;
        private DTransfer mDownloadTransfer;
        private Thread mInterruptTransferWaiter;
        private IPAddress mAddress;
        private ManualResetEvent mDTransferAvailability;
        private ManualResetEvent mCloseWindow;
        private event EventHandler TransferCompleted;
        private event EventHandler TransferInterrupted;
        #endregion

        #region Constructor
        public DownloadProgressWindow(IPAddress address, ManualResetEvent dTransferAvailability)
        {
            // Initialize Window
            InitializeComponent();
            DataContext = new WindowViewModel(this);

            // Set a new CloseWindow Reset Event
            mCloseWindow = new ManualResetEvent(false);

            // Set the internal ProgressControl
            mProgress = (ProgressDesignModel)InternalProgressControl.DataContext;
            mProgress.SetCloseWindowEventHandler(mCloseWindow);
            mProgress.Type = "Download";

            // Set an handler for the Interruption of the transfer
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
            Console.WriteLine("[Movex.View] [DownloadProgressWindow.xaml.cs] [OnLoad] Waiting for the DownloadTransferAvailability.");
            mDTransferAvailability.WaitOne();
            Console.WriteLine("[Movex.View] [DownloadProgressWindow.xaml.cs] [OnLoad] DownloadTransfer is now available.");

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
                    if (interruptionRisk >= 100) { mCloseWindow.Set(); }

                    (sender as BackgroundWorker).ReportProgress(x);
                    lastProgress = progress;
                }
                Thread.Sleep(100);
            }

            // IF 100% COMPLETED: 1. show the perc
            if (!(progress == null) && progress.Equals("100"))
            {
                Console.WriteLine("[Movex.View] [DownloadProgressWindow.xaml.cs] [Worker_DoWork] 100% COMPLETED.");
                if (int.TryParse(progress, out var x))
                {
                    (sender as BackgroundWorker).ReportProgress(x);
                }
            }

            // IF 100% COMPLETED: 2. wait a second
            Thread.Sleep(1000);

            // IF 100% COMPLETED: 3. close the window
            TransferCompleted.Invoke(this, EventArgs.Empty);
        }
        private void Worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            try
            {
                Console.WriteLine("[Movex.View] [DownloadProgressWindow.xaml.cs] [Worker_ProgressChanged] Trying to udapte the DownloadProgressViewModel.");

                var filename = mDownloadTransfer.GetTransferFilename();
                if (filename != null)
                {
                    mProgress.Filename = filename;
                    Console.WriteLine("[Movex.View] [DownloadProgressWindow.xaml.cs] [Worker_ProgressChanged] Assigned Filename: " + mProgress.Filename);
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
            catch(Exception Exception)
            {
                var Message = Exception.Message;
                Console.WriteLine("[MOVEX.VIEW] [DownloadProgressWindow.xaml.cs] [Worker_ProgressChanged] " + Message);
            }
        }
        private void Window_Close(object sender, EventArgs e)
        {
            Console.WriteLine("[Movex.View] [DownloadProgressWindow.xaml.cs] [Window_Close] Closing the window.");
            Dispatcher.BeginInvoke(new Action(() => {

                try
                {
                    // Close the DownloadProgressWindow
                    Close();
                }
                catch (ThreadAbortException Exception)
                {
                    var Message = Exception.Message;
                    Console.WriteLine("[MOVEX.VIEW] [DownloadProgressWindow.xaml.cs] [Window_Close] " + Message + ".");
                }
                catch (Exception Exception)
                {
                    var Message = Exception.Message;
                    Console.WriteLine("[MOVEX.VIEW] [DownloadProgressWindow.xaml.cs] [Window_Close] " + Message + ".");
                }
                finally
                {
                    // Release thread resources
                    Thread.CurrentThread.Interrupt();
                }
            }));
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
            try {
                Console.WriteLine("[Movex.View] [DownloadProgressWindow.xaml.cs] [AssignTransferInfoViewModel] Trying to assign the DownloadTransferInfo to the ViewModel.");

                var ipAddress = mAddress.ToString();
                if (ipAddress != null)
                {                    
                    mProgress.IpAddress = ipAddress;
                    Console.WriteLine("[Movex.View] [DownloadProgressWindow.xaml.cs] [AssignTransferInfoViewModel] Assigned IpAddress: " + mProgress.IpAddress);

                    mProgress.User = IoC.User.GetUsernameByIpAddress(ipAddress);
                    Console.WriteLine("[Movex.View] [DownloadProgressWindow.xaml.cs] [AssignTransferInfoViewModel] Assigned User: " + mProgress.User);
                }
            
                var filename = dTransfer.GetTransferFilename();
                if (filename != null)
                {
                    mProgress.Filename = filename;
                    Console.WriteLine("[Movex.View] [DownloadProgressWindow.xaml.cs] [AssignTransferInfoViewModel] Assigned Filename: " + mProgress.Filename);
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
            catch (Exception Exception)
            {
                var Message = Exception.Message;
                Console.WriteLine("[MOVEX.VIEW] [DownloadProgresWindows.xaml.cs] [AssignTransferInfoViewModel] " + Message + ".");

                throw Exception;
            }
        }
        #endregion
    }
}
