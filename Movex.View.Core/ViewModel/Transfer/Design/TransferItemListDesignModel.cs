using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Movex.View.Core
{
    public class TransferItemListDesignModel : TransferItemListViewModel
    {
        #region Singleton
        /// <summary>
        /// A single instance of the design model
        /// </summary>
        public static TransferItemListDesignModel Instance => new TransferItemListDesignModel();
        #endregion

        #region Constructor
        public TransferItemListDesignModel()
        {
            Items = new List<TransferItemViewModel>{};
        }
        #endregion
    }
}
