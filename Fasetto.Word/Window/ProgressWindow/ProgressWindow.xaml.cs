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
        private UploadChannel mChannel;
        private ManualResetEvent mUchanAvailability;
        private event EventHandler TransferCompleted;
        #endregion

        // Constructor(s)
        public ProgressWindow()
        {
            InitializeComponent();
            DataContext = new WindowViewModel(this);
        }

        public ProgressWindow(Movex.View.Core.FTPclient client, string[] paths, IPAddress address, ManualResetEvent uchanAvailability)
        {
            // Initialize Window
            InitializeComponent();
            DataContext = new WindowViewModel(this);

            // Assigning member(s)
            mFtpClient = client;
            mPaths = paths;
            mAddress = address;
            mUchanAvailability = uchanAvailability;
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
            mUchanAvailability.WaitOne();
            mChannel = mFtpClient.GetChannel(mAddress);
            AssignUploadChannelInfoToViewModel(mChannel);
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

            while (! ((progress = mChannel.GetPerc().ToString()).Equals("100")) )
            {
                if (int.TryParse(progress, out int x))
                {
                    (sender as BackgroundWorker).ReportProgress(x);
                }
                Thread.Sleep(500);
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
            mIndexCurrentTransfer = mChannel.Get_index_current_transfer();
            IoC.Progress.Percentage = (e.ProgressPercentage).ToString();
            IoC.Progress.Filename = mChannel.Get_filenames()[mIndexCurrentTransfer];
            IoC.Progress.RemainingTime = mChannel.GetRemainingTime().ToString();
            
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

        private void AssignUploadChannelInfoToViewModel(UploadChannel uchan)
        {

            IoC.Progress.IpAddress = uchan.Get_to();
            IoC.Progress.User = uchan.Get_to();
            IoC.Progress.Filename = uchan.Get_filenames()[0];
            IoC.Progress.Percentage = uchan.GetPerc().ToString();
            IoC.Progress.RemainingTime = uchan.GetRemainingTime().ToString();
        }




    }
}
