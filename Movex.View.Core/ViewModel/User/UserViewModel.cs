using System.Collections.Generic;

namespace Movex.View.Core
{
    /// <summary>
    /// A view model for users list
    /// </summary>
    public class UserViewModel : BaseViewModel
    {
        /// <summary>
        /// The items of the users list
        /// </summary>
        public List<UserItemViewModel> Items { get; set; }

        /// <summary>
        /// The number of selected users
        /// </summary>
        public int ItemsSelected { get; set; }
    }
}
