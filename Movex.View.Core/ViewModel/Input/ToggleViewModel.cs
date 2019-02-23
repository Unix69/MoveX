using System.Windows.Input;

namespace Movex.View.Core
{
    /// <summary>
    /// A view model for a toggle button
    /// </summary>
    public class ToggleViewModel : BaseViewModel
    {
        #region Public Properties

        /// <summary>
        /// The identifying label
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The current stored value
        /// </summary>
        public string Explanation { get; set; }

        /// <summary>
        /// Indicated if the current toggle button is active
        /// </summary>
        public bool Active { get; set; }

        #endregion

        #region Public Commands

        /// <summary>
        /// Put the control into the editing mode
        /// </summary>
        public ICommand SaveActiveStatusCommand { get; set; }

        #endregion

        #region Constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        public ToggleViewModel()
        {
            SaveActiveStatusCommand = new RelayCommand(SaveActiveStatus);
        }

        #endregion

        #region Command methods

        /// <summary>
        /// Puts the control into the editing mode
        /// </summary>
        public void SaveActiveStatus()
        {
            // Save the status of the Toogle Button to the LocalDB
            if (Name == "Modalità privata")
            {
                IoC.User.SetPrivateMode(Active.ToString());
            }
            else if (Name == "Ricezione automatica")
            {
                Database.UpdateLocalDB("AutomaticReception", Active.ToString());
                IoC.FtpServer.SetAutomaticReception(Active);
            }
            else if (Name == "Salvataggio automatico")
            {
                Database.UpdateLocalDB("AutomaticSave", Active.ToString());
                IoC.FtpServer.SetAutomaticSave(Active);
            }
        }

        #endregion
    }
}
