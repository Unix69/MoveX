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
        private IPAddress[] mAddresses;
        private Thread mFtpClientThread;
        private List<UploadChannelInfo> mChannelsInfo;
        private ManualResetEvent mWindowAvailability;
        private event EventHandler TransferCompleted;
        #endregion

        // Constructor(s)
        public ProgressWindow()
        {
            InitializeComponent();
            DataContext = new WindowViewModel(this);
        }
        public ProgressWindow(Movex.View.Core.FTPclient client, string[] paths, IPAddress[] addresses, ManualResetEvent windowAvailability)
        {
            // Initialize Window
            InitializeComponent();
            DataContext = new WindowViewModel(this);

            // Assigning member(s)
            mFtpClient = client;
            mPaths = paths;
            mAddresses = addresses;
            mWindowAvailability = windowAvailability;

            // Manage event(s)
            Loaded += OnLoad;
            ContentRendered += Window_ContentRendered;
            TransferCompleted += Window_Close;
        }

        #region Event Handler(s)
        private void OnLoad(object sender, RoutedEventArgs e)
        {
            var uploadChannelInfoAvailability = new ManualResetEvent(false);

            // Send the request to transfer file(s)
            mFtpClientThread = new Thread(() => { mFtpClient.Send(mPaths, mAddresses, uploadChannelInfoAvailability, mWindowAvailability); });
            mFtpClientThread.Start();
            
            // Wait for data from FTPclient to load them in the Window
            uploadChannelInfoAvailability.WaitOne();
            mChannelsInfo = mFtpClient.GetUChanInfo();
            AssignUploadChannelInfoToViewModel(mChannelsInfo[0]);
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
            string progress;

            while (! ((progress = mChannelsInfo[0].Get_current_percentage()).Equals("100")) )
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
            IoC.Progress.Percentage = (e.ProgressPercentage).ToString();
            IoC.Progress.User = mChannelsInfo[0].Get_current_to();
            IoC.Progress.Filename = mChannelsInfo[0].Get_current_filename();
            IoC.Progress.RemainingTime = mChannelsInfo[0].Get_current_remaining_time().ToString();
            
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

        private void AssignUploadChannelInfoToViewModel(UploadChannelInfo uchanInfo)
        {
            IoC.Progress.User = uchanInfo.Get_current_to();
            IoC.Progress.Filename = uchanInfo.Get_current_filename();
            IoC.Progress.Percentage = uchanInfo.Get_current_percentage();
            IoC.Progress.RemainingTime = uchanInfo.Get_current_remaining_time().ToString();
        }

    }
}
