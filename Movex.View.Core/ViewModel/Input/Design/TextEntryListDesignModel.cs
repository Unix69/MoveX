using System.Collections.Generic;

namespace Movex.View.Core
{
    /// <summary>
    /// The design-time data for a <see cref="TextEntryListViewModel"/>
    /// </summary>
    public class TextEntryListDesignModel : TextEntryListViewModel
    {
        #region Singleton

        /// <summary>
        /// A single instance of the design model
        /// </summary>
        public static TextEntryListDesignModel Instance => new TextEntryListDesignModel();

        #endregion

        #region Constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        public TextEntryListDesignModel()
        {
            Items = new List<TextEntryViewModel>
            {

                new TextEntryViewModel
                {
                    Label = "Nome",
                    OriginalText = IoC.User.Name,
                    EditedText = "Editing. . ."
                },

                new TextEntryViewModel
                {
                   Label = "Messaggio",
                   OriginalText = IoC.User.Message,
                   EditedText = "Editing. . ."
                }

            };
        }

        #endregion
    }
}
