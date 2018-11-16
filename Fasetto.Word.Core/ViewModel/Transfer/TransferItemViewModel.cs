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
        /// The index of the item for the TransferItem
        /// </summary>
        public int Index { get; set; }

        /// <summary>
        /// The AlternationIndex of the item for the TransferItem
        /// </summary>
        public int AlternationIndex { get; set; }

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
        /// Constructor taking the properties as input
        /// </summary>
        /// <param name="path">The path of the file to be sent.</param>
        /// <param name="size">The size (dimensione) of the file to be sent.</param>
        public TransferItemViewModel(string path, long size)
        {
            Path = path;
            Size = size;
        }

        /// <summary>
        /// Constructor taking the properties as input
        /// </summary>
        /// <param name="index"></param>
        /// <param name="path"></param>
        /// <param name="size"></param>
        public TransferItemViewModel(int index, string path, long size)
        {
            Index = index;
            AlternationIndex = index % 2;
            Path = path;
            Size = size;
        }

        #endregion

    }
}
