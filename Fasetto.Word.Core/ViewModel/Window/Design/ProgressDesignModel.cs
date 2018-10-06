using System.Windows;

namespace Movex.View.Core
{
    /// <summary>
    /// The design-time data for a <see cref="ProgressDesignModel"/>
    /// </summary>
    public class ProgressDesignModel : ProgressViewModel
    {
        
        /// <summary>
        /// A single instance of the design model
        /// </summary>
        public static ProgressDesignModel Instance => new ProgressDesignModel();

        #region Constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        public ProgressDesignModel()
        {
            User            = "Mario Rossi";
            Filename        = "file.txt";
            Percentage      = "0";
            RemainingTime   = "30";
        }

        #endregion
    }
}
