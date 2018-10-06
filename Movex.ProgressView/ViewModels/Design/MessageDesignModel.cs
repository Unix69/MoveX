using System.Windows;

namespace Movex.ProgressView
{
    /// <summary>
    /// The design-time data for a <see cref="MessageViewModel"/>
    /// </summary>
    public class MessageDesignModel : MessageViewModel
    {
        
        /// <summary>
        /// A single instance of the design model
        /// </summary>
        public static MessageDesignModel Instance => new MessageDesignModel();

        #region Constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        public MessageDesignModel()
        {
            Text = "...";
        }

        #endregion
    }
}
