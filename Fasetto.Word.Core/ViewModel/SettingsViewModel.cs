using System.Windows.Input;

namespace Movex.View.Core
{
    /// <summary>
    /// The settings menu state as a view model
    /// </summary>
    public class SettingsViewModel : BaseViewModel
    {
        #region Public Properties

        /// <summary>
        /// The current user name
        /// </summary>
        public TextEntryViewModel Name { get; set; }

        /// <summary>
        /// The current user surname
        /// </summary>
        public TextEntryViewModel Surname { get; set; }

        /// <summary>
        /// The current user message
        /// </summary>
        public TextEntryViewModel Message { get; set; }

        #endregion

        #region Public Command

        /// <summary>
        /// The command to close the settings menu
        /// </summary>
        public ICommand CloseCommand { get; set; }

        /// <summary>
        /// The command to open the settings menu
        /// </summary>
        public ICommand OpenCommand { get; set; }

        #endregion

        #region Constructor

        /// <summary>
        /// Default Constructor
        /// </summary>
        public SettingsViewModel()
        {
            // Create commands
            OpenCommand = new RelayCommand(Open);
            CloseCommand = new RelayCommand(Close);
            
        }

        #endregion

        #region Public Methods
        
        /// <summary>
        /// Open the settings menu
        /// </summary>
        public void Open()
        {
            IoC.Get<ApplicationViewModel>().SettingsMenuVisible = true;
        }

        /// <summary>
        /// Close the settings menu
        /// </summary>
        public void Close()
        {
            IoC.Get<ApplicationViewModel>().SettingsMenuVisible = false;
        }

        #endregion
    }
}