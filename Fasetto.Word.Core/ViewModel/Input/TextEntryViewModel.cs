using System.Windows.Forms;
using System.Windows.Input;

namespace Movex.View.Core
{
    /// <summary>
    /// A view model for text entry to edit a string value
    /// </summary>
    public class TextEntryViewModel : BaseViewModel
    {
        #region Public Properties

        /// <summary>
        /// The identifying label
        /// </summary>
        public string Label { get; set; }

        /// <summary>
        /// The current stored value
        /// </summary>
        public string OriginalText { get; set; }

        /// <summary>
        /// The edited value
        /// </summary>
        public string EditedText { get; set; }

        /// <summary>
        /// Indicated if the current text is going to be edited
        /// </summary>
        public bool Editing { get; set; }

        #endregion

        #region Public Commands

        /// <summary>
        /// Put the control into the editing mode
        /// </summary>
        public ICommand EditCommand { get; set; }

        /// <summary>
        /// Put out the control from the editing mode
        /// </summary>
        public ICommand CancelCommand { get; set; }

        /// <summary>
        /// Store the values
        /// </summary>
        public ICommand SaveCommand { get; set; }

        #endregion

        #region Constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        public TextEntryViewModel()
        {
            EditCommand = new RelayCommand(Edit);
            CancelCommand = new RelayCommand(Cancel);
            SaveCommand = new RelayCommand(Save);            
        }

        #endregion

        #region Command methods

        /// <summary>
        /// Puts the control into the editing mode
        /// </summary>
        public void Edit()
        { 
            EditedText = OriginalText;
            Editing = true;
        }

        /// <summary>
        /// Puts out the control from the editing mode
        /// </summary>
        public void Cancel()
        {
            Editing = false;
        }

        /// <summary>
        /// Store the value
        /// </summary>
        public void Save()
        {
            OriginalText = EditedText;
            if (Label == "Nome")
            {
                IoC.User.SetName(OriginalText);
            }
            else if (Label == "Messaggio")
            {
                IoC.User.SetMessage(OriginalText);
            }
            Editing = false;
        }

        #endregion
    }
}
