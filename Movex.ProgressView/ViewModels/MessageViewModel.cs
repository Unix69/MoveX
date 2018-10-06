using System.Windows;
using System.Windows.Input;

namespace Movex.ProgressView
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
        /// Close the window
        /// </summary>
        public void Close()
        {
            
        }

        #endregion
    }
}