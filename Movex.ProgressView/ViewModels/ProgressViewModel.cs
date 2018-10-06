using System.Windows;
using System.Windows.Input;

namespace Movex.ProgressView
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
        /// Stop the process
        /// </summary>
        public void Stop()
        {
            MessageBox.Show("Ciao.");           
        }

        #endregion
    }
}