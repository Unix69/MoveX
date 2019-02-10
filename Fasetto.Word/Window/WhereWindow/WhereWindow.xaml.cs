using System.Windows;
using System.Threading;
using System.Collections.Concurrent;
using System.ComponentModel;
using System;

namespace Movex.View
{
    /// <summary>
    /// Interaction logic for WhereWindow.xaml
    /// </summary>
    public partial class WhereWindow : Window
    {
        ManualResetEvent mFolderSelected;
        bool mOnExit;

        public WhereWindow(ManualResetEvent WhereResponseAvailability, string whereMessage, ConcurrentBag<string> whereResponse)
        {
            InitializeComponent();
            var WinViewModel = new WindowViewModel(this);            
            DataContext = WinViewModel;
            mFolderSelected = new ManualResetEvent(false);
            mOnExit = false;

            // Passing a synchronization primitive to the WhereControl
            var control = WhereWindow_WhereControl;
            control.SetResponseAvailability(WhereResponseAvailability);
            control.SetMessage(whereMessage);
            control.SetResponse(whereResponse);
            control.SetOnExit(mOnExit);
            control.SetFolderSelectedEvent(mFolderSelected);

            // Set a sync primitive to wait for folder selection
            var worker = new BackgroundWorker();
            worker.DoWork += WaitForFolderSelection;
            worker.RunWorkerAsync();
        }

        public void WaitForFolderSelection(object sender, DoWorkEventArgs e)
        {
            Wait: mFolderSelected.WaitOne();
            if (!mOnExit)
            {
                Console.WriteLine("Activating the window.");
                Dispatcher.Invoke(() => { Activate(); });
                mFolderSelected.Reset();
                goto Wait;
            }   
        }
    }
}
