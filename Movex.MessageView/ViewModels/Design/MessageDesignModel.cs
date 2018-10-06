using System;
using System.Threading;
using System.Windows;

namespace Movex.MessageView
{
    /// <summary>
    /// The design-time data for a <see cref="MessageDesignModel"/>
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
