using System.Windows;
using System.Windows.Input;
using System.Threading;
using System.Collections.Concurrent;
using System;

namespace Movex.View.Core
{
    /// <summary>
    /// The settings menu state as a view model
    /// </summary>
    public class YesNoViewModel : BaseViewModel
    {
        #region Public Properties

        /// <summary>
        /// The text of the message
        /// </summary>
        public string Text { get; set; }
        private ManualResetEvent ResponseAvailability { get; set; }
        private ManualResetEvent CloseWindowEvent { get; set; }
        private ConcurrentBag<string> Response { get; set; }

        #endregion

        #region Public Command

        /// <summary>
        /// The command to select Yes
        /// </summary>
        public ICommand YesCommand { get; set; }

        /// <summary>
        /// The command to select No
        /// </summary>
        public ICommand NoCommand { get; set; }

        /// <summary>
        /// The command to close the parent Window container
        /// </summary>
        public ICommand CloseCommand { get; set; }

        #endregion

        #region Constructor

        /// <summary>
        /// Default Constructor
        /// </summary>
        public YesNoViewModel()
        {
            YesCommand = new RelayCommand(Yes);
            NoCommand = new RelayCommand(No);
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Send the Yes response to the caller
        /// </summary>
        public void Yes()
        {
            if (ResponseAvailability == null) { return; }

            Response.Add("Yes");
            ResponseAvailability.Set();
        }
        /// <summary>
        /// Send the No response to the caller
        /// </summary>
        public void No()
        {
            if (ResponseAvailability == null) { return; }

            Response.Add("No");
            ResponseAvailability.Set();
        }
        /// <summary>
        /// Set the message for the Window
        /// </summary>
        /// <param name="text"></param>
        public void SetText(string text)
        {
            if (text != null)
            {
                Text = text;
            }
        }
        internal void SetCloseWindowEvent(ManualResetEvent closeWindowEvent)
        {
            CloseWindowEvent = closeWindowEvent;
        }
        public void SetResponse(ConcurrentBag<string> response)
        {
            Response = response;
        }
        public void SetResponseAvailability(ManualResetEvent responseAvailability)
        {
            ResponseAvailability = responseAvailability;
        }

        #endregion
    }
}