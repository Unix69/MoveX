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
        public string Size { get; set; }

        #endregion

        #region Constructor
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
            Size = GetConvertedNumber(size);
        }

        #endregion

        #region Utility methods
        public string GetBytesSufix(ref double bytes)
        {
            string[] sufixes = { "", "K", "M", "G", "T", "P" };
            var s = 0;
            while (bytes > 1024)
            {
                bytes /= 1024;
                s++;
            }
            return (sufixes[s]);
        }
        public string GetConvertedNumber(long bytes)
        {
            double b = bytes;
            var sufix = GetBytesSufix(ref b);
            var r = Math.Round(b,2);
            return (r.ToString() + " " + sufix + "b");
        }
        #endregion

    }
}
