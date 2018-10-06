using System.Windows;
using System.Windows.Input;
using System.Threading;
using System.Collections.Concurrent;

namespace Movex.MessageView
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
        private ManualResetEvent CloseWindowEvent { get; set; }
        private ConcurrentStack<string> Response { get; set; }

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
        /// Set the message for the Window
        /// </summary>
        /// <param name="text"></param>
        public void SetText(string text) {
            if (text != null)
            {
                Text = text;
            }
        }
        
        /// <summary>
        /// Send the Yes response to the caller
        /// </summary>
        public void Yes()
        {
            Response.Push("Yes");
            if (CloseWindowEvent != null)
            {
                CloseWindowEvent.Set();
            }
        }

        /// <summary>
        /// Send the No response to the caller
        /// </summary>
        public void No()
        {
            Response.Push("No");
            if (CloseWindowEvent != null)
            {
                CloseWindowEvent.Set();
            }
        }

        internal void SetCloseWindowEvent(ManualResetEvent closeWindowEvent)
        {
            CloseWindowEvent = closeWindowEvent;
        }

        internal void SetResponse(ConcurrentStack<string> response)
        {
            Response = response;
        }

        #endregion
    }
}