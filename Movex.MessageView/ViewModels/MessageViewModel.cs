using System.Windows;
using System.Windows.Input;
using System.Threading;

namespace Movex.MessageView
{
    /// <summary>
    /// The settings menu state as a view model
    /// </summary>
    public class MessageViewModel : BaseViewModel
    {
        #region Public Properties
        /// <summary>
        /// The text of the message
        /// </summary>
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
        private Window WindowContainer { get; set; }
        #endregion

        #region Public Command

        /// <summary>
        /// The command to close the window 
        /// </summary>
        public ICommand CloseCommand { get; set; }

        #endregion

        #region Constructor

        /// <summary>
        /// Default Constructor
        /// </summary>
        public MessageViewModel()
        {
            CloseCommand = new RelayCommand(Close);
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Set the message for the Window
        /// </summary>
        /// <param name="text"></param>
        public void SetText(string text) {
            if (text != null) { Text = text; }
        }
        
        /// <summary>
        /// Close the window
        /// </summary>
        public void Close()
        {
            WindowContainer.Close();
            if (CloseWindowEvent != null)
            {
                CloseWindowEvent.Set();
            }
        }

        #endregion

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
        internal void SetWindowContainer(Window wc)
        {
            WindowContainer = wc;
        }

        #endregion

    }
}