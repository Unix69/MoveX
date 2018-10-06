using System.Windows;

namespace Movex.MessageView
{
    /// <summary>
    /// The design-time data for a <see cref="YesNoDesignModel"/>
    /// </summary>
    public class YesNoDesignModel : YesNoViewModel
    {
        
        /// <summary>
        /// A single instance of the design model
        /// </summary>
        public static YesNoDesignModel Instance => new YesNoDesignModel();

        #region Constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        public YesNoDesignModel()
        {
            Text = "...";
        }

        #endregion
    }
}
