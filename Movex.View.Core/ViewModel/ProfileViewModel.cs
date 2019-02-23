using System.Windows.Input;

namespace Movex.View.Core
{
    /// <summary>
    /// The profile menu state as a view model
    /// </summary>
    public class ProfileViewModel : BaseViewModel
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

        /// <summary>
        /// The command to start the search
        /// </summary>
        public ICommand SearchCommand { get; set; }

        #endregion

        #region Constructor

        /// <summary>
        /// Default Constructor
        /// </summary>
        public ProfileViewModel()
        {
            // Create commands
            OpenCommand = new RelayCommand(Open);
            CloseCommand = new RelayCommand(Close);
            SearchCommand = new RelayCommand(Search);
        }

        #endregion

        #region Public Methods
        
        /// <summary>
        /// Open the profile menu
        /// </summary>
        public void Open()
        {
            IoC.Application.ProfileMenuVisible = true;
        }

        /// <summary>
        /// Close the profile menu
        /// </summary>
        public void Close()
        {
            IoC.Application.ProfileMenuVisible = false;
        }

        public void Search()
        {
            IoC.User.SearchForUsers();
        }

        #endregion
    }
}