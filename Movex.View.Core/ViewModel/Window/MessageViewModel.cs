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
    public class MessageViewModel : BaseViewModel
    {
        #region Public Properties
        /// <summary>
        /// The text of the message
        /// </summary>
        public string Text { get; set; }
        #endregion

        #region Public Command
        /// <summary>
        /// The command to select No
        /// </summary>
        public ICommand OkCommand { get; set; }
        /// <summary>
        /// The command to close the parent Window container
        /// </summary>
        public ICommand CloseCommand { get; set; }
        #endregion

        #region Constructor
        /// <summary>
        /// Default Constructor
        /// </summary>
        public MessageViewModel()
        {
            OkCommand = new RelayCommand(Ok);
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Send the Yes response to the caller
        /// </summary>
        public void Ok()
        {
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
        #endregion
    }
}