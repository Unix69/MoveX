using System.Collections.Generic;

namespace Movex.View.Core
{
    /// <summary>
    /// A view model for the overview chat list
    /// </summary>
    public class ToggleListViewModel : BaseViewModel
    {
        /// <summary>
        /// The chat list items for the list
        /// </summary>
        public List<ToggleViewModel> Items { get; set; }
    }
}
