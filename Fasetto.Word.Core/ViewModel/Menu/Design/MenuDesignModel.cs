using System.Collections.Generic;

namespace Movex.View.Core
{
    /// <summary>
    /// The design-time data for a <see cref="MenuListViewModel"/>
    /// </summary>
    public class MenuDesignModel : UserViewModel
    {
        #region Singleton

        /// <summary>
        /// A single instance of the design model
        /// </summary>
        public static UserDesignModel Instance => new UserDesignModel();

        #endregion

        #region Constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        public MenuDesignModel()
        {
            Items = new List<UserItemViewModel>
            {
                new UserItemViewModel
                {
                    Name = "Luke",
                    Initials = "LM",
                    Message = "This chat app is awesome! I bet it will be fast too",
                    ProfilePicture = "3099c5",
                    NewMessageAvailable = true
                },
                new UserItemViewModel
                {
                    Name = "Jesse",
                    Initials = "JA",
                    Message = "Hey dude, here are the new icons",
                    ProfilePicture = "fe4503"
                },
                new UserItemViewModel
                {
                    Name = "Parnell",
                    Initials = "PL",
                    Message = "The new server is up, got 192.168.1.1",
                    ProfilePicture = "00d405",
                    IsSelected = true
                },
                new UserItemViewModel
                {
                    Name = "Luke",
                    Initials = "LM",
                    Message = "This chat app is awesome! I bet it will be fast too",
                    ProfilePicture = "3099c5"
                },
                new UserItemViewModel
                {
                    Name = "Jesse",
                    Initials = "JA",
                    Message = "Hey dude, here are the new icons",
                    ProfilePicture = "fe4503"
                }
            };
        }

        #endregion
    }
}
