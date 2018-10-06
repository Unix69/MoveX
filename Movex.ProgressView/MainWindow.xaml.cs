using System;
using System.Windows;
using System.Threading;
using System.ComponentModel;

namespace Movex.ProgressView
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private int mViewModelIndex;

        /* CHANNEL INFO STRUCTURE
            [0] => filename
            [1] => actual percentage
            [2] => remaining time
            [3] => user
        */

        // Constructor
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new WindowViewModel(this);
        }

        // Setter
        public void AssignViewModel(int index)
        {
            // The assignment of the viewModel
            // occurr only with the index
            mViewModelIndex = index;
        }
        public void FiilViewModel(ref string[] channelInfo)
        {
            IoC.Progress[mViewModelIndex].User = channelInfo[3];
            IoC.Progress[mViewModelIndex].Filename = channelInfo[0];
            IoC.Progress[mViewModelIndex].Percentage = channelInfo[1];
            IoC.Progress[mViewModelIndex].RemainingTime = channelInfo[2];
        }

        // Methods used to render the reactivity of the window
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
        void Worker_DoWork(object sender, DoWorkEventArgs e)
        {
            var progress = IoC.Progress[mViewModelIndex].Percentage;
            while (!(progress.Equals("100")))
            {
                if (int.TryParse(progress, out int x))
                {
                    (sender as BackgroundWorker).ReportProgress(x);
                }
                Thread.Sleep(100);
            }
            
        }
        void Worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            IoC.Progress[mViewModelIndex].Percentage = (e.ProgressPercentage).ToString();
        }
    }
}
