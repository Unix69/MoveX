using Movex.View.Core;
using System.Windows;
using System.ComponentModel;
using System.Threading;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Collections.Concurrent;

namespace Movex.View
{
    public partial class App : Application
    {
        public static IntPtr WindowHandle { get; private set; }

        [DllImport("user32", CharSet = CharSet.Unicode)]
        static extern IntPtr FindWindow(string cls, string win);
        [DllImport("user32")]
        static extern IntPtr SetForegroundWindow(IntPtr hWnd);
        [DllImport("user32")]
        static extern bool IsIconic(IntPtr hWnd);
        [DllImport("user32")]
        static extern bool OpenIcon(IntPtr hWnd);

        public enum Mode { Traditional, Contextual };
        private System.Windows.Forms.NotifyIcon mNotifyIcon;
        private bool mIsExit;
        private WindowDispatcher mWindowDispatcher;
        private string[] mArgs;
        private int mIndex;
        private Mode mModeOn = Mode.Traditional;
        private WindowRequester mWindowRequester;
        private List<Thread> mThreadsContainer;
        private static Mutex NamedMutex;

        #region Members for project-inner-access
        private BrowsePage mBrowsePage;
        private UserListControl mUserListControl;
        #endregion

        #region Lifecycle method(s)
        /// <summary>
        /// At Application Startup
        /// </summary>
        /// <param name="e"></param>
        protected override void OnStartup(StartupEventArgs e)
        {
            // Understand whether first instance or not
            const string appName = "MoveX";
            NamedMutex = new Mutex(true, appName, out var createdNew);
            var proc = Process.GetCurrentProcess();
            var processName = proc.ProcessName.Replace(".vshost", "");
            var runningProcess = Process.GetProcesses()
                .FirstOrDefault(x => (x.ProcessName == processName || x.ProcessName == proc.ProcessName || x.ProcessName == proc.ProcessName + ".vshost") && x.Id != proc.Id);

            if (e.Args.Length > 0)
            {
                SetArguments(e.Args);
            }

            base.OnStartup(e);

            if (!createdNew)
            {
                ActivateOtherWindow();
                if(mArgs != null && mArgs.Length > 0)
                {
                    UnsafeNative.SendMessage(runningProcess.MainWindowHandle, string.Join("|", mArgs));
                }
                Application.Current.Shutdown();
            }
            else if (createdNew)
            {
                // Store the arguments passed (if any)
                if (e.Args.Length > 0)
                {
                    mModeOn = Mode.Contextual;
                }

                // Initialize variables
                mBrowsePage = null;
                mUserListControl = null;
                mThreadsContainer = new List<Thread>();

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
        /// <summary>
        /// At application shutting down
        /// </summary>
        private void ExitApplication()
        {
            var response = mWindowRequester.AddYesNoWindow("Sei sicuro di voler chiudere l'applicazione?");
            if (response.Equals("Yes"))
            {
                MainWindow.Hide();

                mWindowDispatcher.Stop();
                mWindowRequester.Dispose();
                mNotifyIcon.Visible = false;
                mNotifyIcon.Dispose();
                mNotifyIcon = null;

                ReleaseThreads();
                IoC.Dispose();
                Environment.Exit(Environment.ExitCode);
            }
        }
        #endregion

        #region Event Handler(s)
        private void MainWindow_Closing(object sender, CancelEventArgs e)
        {
            ExitApplication();
        }
        #endregion

        #region Utility method(s)
        private void CreateContextMenu()
        {
            mNotifyIcon.ContextMenuStrip = new System.Windows.Forms.ContextMenuStrip();
            mNotifyIcon.ContextMenuStrip.Items.Add("MoveX").Click += (s, e) => ShowMainWindow();
            mNotifyIcon.ContextMenuStrip.Items.Add("Esci").Click += (s, e) => ExitApplication();
        }
        public void ReleaseThreads() { foreach (var t in mThreadsContainer) t.Abort(); }
        #endregion

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
        public void AddThread(Thread t) { mThreadsContainer.Add(t); }
        public void RemoveThread(string Name)
        {
            Thread stone = null;
            try
            {
                Console.WriteLine("[Movex.View] [App.xaml.cs] [RemoveThread] Trying to remove thread: " + Name + ".");

                foreach (var t in mThreadsContainer)
                {
                    if (t.Name.Equals(Name))
                    {
                        t.Join();
                        stone = t;
                        break;
                    }
                }
                if (stone != null) { mThreadsContainer.Remove(stone); }

                var ThreadsCount = mThreadsContainer.Count;
                Console.WriteLine("[Movex.View] [App.xaml.cs] [RemoveThread] Current active threads now: " + ThreadsCount + ".");
            }
            catch (Exception Exception)
            {
                var Message = Exception.Message;
                Console.WriteLine("[MOVEX.VIEW] [App.xaml.cs] [RemoveThread] " + Message);
            }
        }
        public void SetArguments(string[] Arguments)
        {
            mArgs = new string[Arguments.Length];
            foreach (var arg in Arguments)
            {
                mArgs[mIndex++] = arg;
            }
        }
        #endregion

        private void ActivateOtherWindow()
        {
            var other = FindWindow(null, "moveX - Local Sharing Point");
            if (other != IntPtr.Zero)
            {
                SetForegroundWindow(other);
                if (IsIconic(other))
                    OpenIcon(other);
            }
        }
    }
}
