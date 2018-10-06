using System.Windows;

namespace Movex.ProgressView
{
    /// <summary>
    /// The design-time data for a <see cref="ToogleViewModel"/>
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
            User            = "unknown";
            Filename        = "unknown";
            Percentage      = "0";
            RemainingTime   = "60";
        }

        #endregion
    }
}
