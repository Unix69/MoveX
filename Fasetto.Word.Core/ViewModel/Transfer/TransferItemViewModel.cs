using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Movex.View.Core
{
    public class TransferItemViewModel : BaseViewModel
    {

        #region Public Properties

        /// <summary>
        /// The path of the item to be sent
        /// </summary>
        public string Path { get; set; }

        /// <summary>
        /// The Size (dimension) of the item to be sent
        /// </summary>
        public long Size { get; set; }

        #endregion

        #region Constructor

        /// <summary>
        /// Default construcor
        /// </summary>
        public TransferItemViewModel() { }

        /// <summary>
        ///  Constructor taking the properties as input
        /// </summary>
        /// <param name="path">The path of the file to be sent.</param>
        /// <param name="size">The size (dimensione) of the file to be sent.</param>
        public TransferItemViewModel(string path, long size)
        {
            Path = path;
            Size = size;
        }

        #endregion

    }
}
