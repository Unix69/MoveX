using System.Collections.Generic;

namespace Movex.View.Core
{
    public class TransferItemListViewModel : BaseViewModel
    {

        /// <summary>
        /// The trasferItemList items
        /// </summary>
        public List<TransferItemViewModel> Items { get; set; }

        /// <summary>
        /// Indicate available item(s) in the list of transfers
        /// </summary>
        public bool TransferAvailable { get; set; }

        /// <summary>
        /// Indicate available user(s) in the list of transfers
        /// </summary>
        public bool UsersAvailable { get; set; }


    }
}
