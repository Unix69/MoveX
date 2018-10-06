using System.Collections.Generic;
using System.Windows;
using System.Windows.Navigation;

namespace Movex.ProgressView
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private MainWindow[] mViews;

        /// <summary>
        /// Occurs when the Run method of the Application object is called.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Launch the Progress Views
            for (var i=0; i<mViews.Length; i++)
            {
                mViews[i].Show();
            }
        }

        /// <summary>
        /// Occurs when content that was navigated to by a navigator in the application has been loaded, parsed, and has begun rendering.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnLoadCompleted(NavigationEventArgs e)
        {
            base.OnLoadCompleted(e);
        }

        /// <summary>
        /// Create ProgressViews and fill their corrispective view models using the ChannelInfo
        /// </summary>
        /// <param name="channelInfo"></param>
        /// <param name="sent"></param>
        public void SetChannelInfo(ref string[][] channelInfo)
        {
            var n = channelInfo.Length;

            // Setup the Inversion of Control Static Class
            IoC.Setup(n);

            // Set the views
            mViews = new MainWindow[n];
            for (var i=0; i<n; i++)
            {
                mViews[i] = new MainWindow();
                mViews[i].AssignViewModel(i);
                mViews[i].FiilViewModel(ref channelInfo[i]);

            }

        }

    }
}
