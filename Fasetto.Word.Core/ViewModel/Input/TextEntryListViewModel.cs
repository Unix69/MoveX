using System.Collections.Generic;

namespace Movex.View.Core
{
    /// <summary>
    /// A view model for the overview chat list
    /// </summary>
    public class TextEntryListViewModel : BaseViewModel
    {
        /// <summary>
        /// The TextEntry list of the items
        /// </summary>
        public List<TextEntryViewModel> Items { get; set; }
    }
}
