using System.Collections.Generic;

namespace Movex.View.Core
{
    /// <summary>
    /// A view model for the users list
    /// </summary>
    public class MenuViewModel : BaseViewModel
    {
        /// <summary>
        /// The usrs list items
        /// </summary>
        public List<MenuItemViewModel> Items { get; set; }
    }
}
