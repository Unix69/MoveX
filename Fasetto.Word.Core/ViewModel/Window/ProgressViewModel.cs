﻿using System.Threading;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;

namespace Movex.View.Core
{
    /// <summary>
    /// The settings menu state as a view model
    /// </summary>
    public class ProgressViewModel : BaseViewModel
    {
        #region Public Properties

        /// <summary>
        /// The current user
        /// </summary>
        public string User { get; set; }

        /// <summary>
        /// The current file name
        /// </summary>
        public string Filename { get; set; }

        /// <summary>
        /// The current percentage
        /// </summary>
        public string Percentage { get; set; }

        /// <summary>
        /// The current remaining time
        /// </summary>
        public string RemainingTime { get; set; }

        public string Text { get; set; }

        #endregion

        #region Private Properties

        /// <summary>
        /// The handler to signal the caller thread that the window has been closed
        /// </summary>
        private ManualResetEvent CloseWindowEvent { get; set; } = null;

        /// <summary>
        /// The Window containing this ViewModel
        /// </summary>
        //private Window WindowContainer { get; set; }
        #endregion

        #region Public Command

        /// <summary>
        /// The command to close the settings menu
        /// </summary>
        public ICommand StopCommand { get; set; }

        #endregion

        #region Constructor

        /// <summary>
        /// Default Constructor
        /// </summary>
        public ProgressViewModel()
        {
            StopCommand = new RelayCommand(Stop);
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Set the message for the Window
        /// </summary>
        /// <param name="text"></param>
        public void SetText(string text)
        {
            if (text != null) { Text = text; }
        }

        /// <summary>
        /// Stop the process
        /// </summary>
        public void Stop()
        {
            MessageBox.Show("Ciao.");           
        }

        #region Internal Methods

        /// <summary>
        /// Set the handler for the CloseWindowEvent
        /// </summary>
        /// <param name="closeWindowEvent"></param>
        internal void SetCloseWindowEventHandler(ManualResetEvent closeWindowEvent)
        {
            if (closeWindowEvent != null)
                CloseWindowEvent = closeWindowEvent;
        }

        /// <summary>
        /// Set the WindowContainer
        /// </summary>
        /// <param name="wc"></param>
        /*
        internal void SetWindowContainer(Window wc)
        {
            WindowContainer = wc;
        }
        */

        #endregion

        #endregion
    }
}