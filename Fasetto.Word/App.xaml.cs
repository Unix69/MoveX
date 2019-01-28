using Movex.View.Core;
using System.Windows;
using System.ComponentModel;
using System.Threading;
using System;

namespace Movex.View
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public enum Mode { Traditional, Contextual };
        private System.Windows.Forms.NotifyIcon mNotifyIcon;
        private bool mIsExit;
        private WindowDispatcher mWindowDispatcher;
        private string[] mArgs;
        private int mIndex;
        private Mode mModeOn = Mode.Traditional;
        private WindowRequester mWindowRequester;

        #region Members for project-inner-access
        private BrowsePage mBrowsePage;
        private UserListControl mUserListControl;
        #endregion

        protected override void OnStartup(StartupEventArgs e)
        {
            // Store the arguments passed (if any)
            if (e.Args.Length > 0) {
                mModeOn = Mode.Contextual;

                mArgs = new string[e.Args.Length];
                foreach (var arg in e.Args)
                {
                    mArgs[mIndex++] = arg;
                }
            }

            base.OnStartup(e);

            // Initialize variables
            mBrowsePage = null;
            mUserListControl = null;

            // Launch the WindowDispatcher (it runs in background)
            mWindowDispatcher = new WindowDispatcher();
            mWindowDispatcher.Init();
            mWindowDispatcher.Start();

            // Setup IoC
            IoC.Setup();
            IoC.FtpServer.SetSynchronization(
                WindowDispatcher.RequestAvailable,
                WindowDispatcher.Requests,
                WindowDispatcher.TypeRequests,
                WindowDispatcher.Messages,
                WindowDispatcher.Sync,
                WindowDispatcher.Responses
                );
            IoC.FtpClient.SetSynchronization(
                WindowDispatcher.RequestAvailable,
                WindowDispatcher.Requests,
                WindowDispatcher.TypeRequests,
                WindowDispatcher.Messages,
                WindowDispatcher.Sync,
                WindowDispatcher.Responses
                );
            mWindowRequester = new WindowRequester(
                WindowDispatcher.RequestAvailable,
                WindowDispatcher.Requests,
                WindowDispatcher.TypeRequests,
                WindowDispatcher.Messages,
                WindowDispatcher.Sync,
                WindowDispatcher.Responses
                );
           
            // Setup MainWindow
            MainWindow = new MainWindow();
            MainWindow.Closing += MainWindow_Closing;

            // Setup the System Tray
            mNotifyIcon = new System.Windows.Forms.NotifyIcon();
            mNotifyIcon.DoubleClick += (s, args) => ShowMainWindow();
            mNotifyIcon.Icon = Movex.View.Properties.Resources.MyIcon;
            mNotifyIcon.Visible = true;
            CreateContextMenu();

            // Launch the MainWindow
            ShowMainWindow();
        }

        private void CreateContextMenu()
        {
            mNotifyIcon.ContextMenuStrip = new System.Windows.Forms.ContextMenuStrip();
            mNotifyIcon.ContextMenuStrip.Items.Add("MoveX").Click += (s, e) => ShowMainWindow();
            mNotifyIcon.ContextMenuStrip.Items.Add("Esci").Click += (s, e) => ExitApplication();
        }

        private void ExitApplication()
        {
            mIsExit = true;
            MainWindow.Close();
            
            mWindowDispatcher.Stop();
            mNotifyIcon.Visible = false;
            mNotifyIcon.Dispose();
            mNotifyIcon = null;

            IoC.Dispose();

            Environment.Exit(Environment.ExitCode);
        }

        /// <summary>
        /// Handle application events to show-up the Main Window Application
        /// </summary>
        public void ShowMainWindow()
        {
            if (MainWindow.IsVisible)
            {
                if (MainWindow.WindowState == WindowState.Minimized)
                {
                    MainWindow.WindowState = WindowState.Normal;
                }
                MainWindow.Activate();
            }
            else
            {
                MainWindow.Show();
            }

            if (mModeOn == Mode.Contextual)
            {
                IoC.Application.GoToPage(ApplicationPage.Browse);
            }
        }

        private void MainWindow_Closing(object sender, CancelEventArgs e)
        {
            if (!mIsExit)
            {
                e.Cancel = true;
                MainWindow.Hide(); // A hidden window can be shown again, a closed one not
            }
        }

        #region Getter(s) and Setter(s)
        public void SetUserListControl(UserListControl control) { mUserListControl = control; }
        public UserListControl GetUserListControl() { return mUserListControl; }
        public void SetBrowsePage(BrowsePage browsePage) { mBrowsePage = browsePage; }
        public BrowsePage GetBrowsePage() { return mBrowsePage; }
        public string[] GetArgs()
        {
            if (!(mArgs.Length == 0))
            {
                return mArgs;
            }
            else
            {
                return null;
            }
        }
        public Mode GetModeOn() { return mModeOn; }
        public void SetModeOn(Mode modeOn) { mModeOn = modeOn; }
        public WindowRequester GetWindowRequester() { return mWindowRequester; }
        #endregion
    }
}
