using System.Windows.Input;
using System.IO;
using System;
using System.Text;
using System.Windows.Forms;
using Movex.Network;
using System.Threading;
using System.Collections.Generic;

namespace Movex.View.Core
{
    /// <summary>
    /// A view model for the user
    /// </summary>
    public class UserItemViewModel : BaseViewModel
    {
        #region Public Properties

        /// <summary>
        /// The display name of this chat list
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// A message from the User
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// The current IP Address of the User
        /// </summary>
        public string IpAddress { get; set; }

        /// <summary>
        /// The initials to show for the profile picture background
        /// </summary>
        public string Initials { get; set; }

        /// <summary>
        /// The Profile Picture path for the User
        /// </summary>
        public string ProfilePicture { get; set; } 

        /// <summary>
        /// True if there are unread messages in this chat 
        /// </summary>
        public bool NewMessageAvailable { get; set; }

        /// <summary>
        /// True if this item is currently selected
        /// </summary>
        public bool IsSelected { get; set; }

        /// <summary>
        /// The default folder for the Downloads
        /// </summary>
        public string DownloadDefaultFolder { get; set; }

        #endregion

        #region Constructor

        /// <summary>
        /// Default Constructor
        /// </summary>
        public UserItemViewModel() { }

        #endregion

    }


}
