﻿using Movex.View.Core;
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
        private System.Windows.Forms.NotifyIcon mNotifyIcon;
        private bool mIsExit;
        private WindowDispatcher mWindowDispatcher;

        #region Instance Members for project-inner-access

        private BrowsePage mBrowsePage;
        private UserListControl mUserListControl;

        #endregion

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

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
        }

        private void MainWindow_Closing(object sender, CancelEventArgs e)
        {
            if (!mIsExit)
            {
                e.Cancel = true;
                MainWindow.Hide(); // A hidden window can be shown again, a closed one not
            }
        }

        /// <summary>
        /// Getter(s) and setter(s)
        /// </summary>
        /// <param name="control"></param>
        public void SetUserListControl(UserListControl control) { mUserListControl = control; }
        public UserListControl GetUserListControl() { return mUserListControl; }
        public void SetBrowsePage(BrowsePage browsePage) { mBrowsePage = browsePage; }
        public BrowsePage GetBrowsePage() { return mBrowsePage; }
    }
}
