using System.Windows;

namespace Movex.MessageView
{
    /// <summary>
    /// The design-time data for a <see cref="WhereViewModel"/>
    /// </summary>
    public class WhereDesignModel : WhereViewModel
    {
        
        /// <summary>
        /// A single instance of the design model
        /// </summary>
        public static WhereDesignModel Instance => new WhereDesignModel();

        #region Constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        public WhereDesignModel()
        {
            Text = "...";
        }

        #endregion
    }
}
