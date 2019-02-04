using System.Collections.Generic;
using Movex.Network;
using System.Threading;
using System.Windows.Forms;
using System.IO;
using System;

namespace Movex.View.Core
{
    /// <summary>
    /// The design-time data for a <see cref="UserListViewModel"/>
    /// </summary>
    public class UserDesignModel : UserViewModel
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
        public UserDesignModel()
        {
           

            IoC.Application.UserDesignModelUpdater = new Thread( () => {

                var currWorkingDirectory = Directory.GetCurrentDirectory();
                var defaultDir = @"ProfilePictures";
                var defaultDirPath = Path.Combine(currWorkingDirectory, defaultDir);
                var defaultProfilePicture = Path.Combine(currWorkingDirectory, @"\Images\Icons\profile.png");

                // Get the technical user
                var u = IoC.User.GetTechnicalUser();
                var ItemsSelected = new List<string>();

                updateModel:

                // Collect the List of User from Network Discovering
                u.GetForFriend();
                var ItemsList = u.GetFriendList();

                IoC.User.FriendsAvailable = ItemsList.Capacity == 0 ? false : true ;
                if (ItemsList.Count == 0)
                {
                    if (Items != null) Items.Clear();
                    if (ItemsSelected != null) ItemsSelected.Clear();
                    IoC.User.FriendsAvailable = false;
                    IoC.TransferItemList.UsersAvailable = false;
                    IoC.TransferItemList.TransferAvailable = false;

                    // Force a DataBinding Refresh creating a new Items Object
                    Items = new List<UserItemViewModel>();
                    
                    goto Sleep;
                }
                else
                {
                    IoC.User.FriendsAvailable = true;

                    // If there are selected users store its IpAddress
                    if (Items != null)
                    {
                        foreach (var item in Items)
                        {
                            if (item.IsSelected)
                            {
                                ItemsSelected.Add(item.IpAddress);
                            }
                        }

                    }

                    Items = new List<UserItemViewModel>();

                    for (var i = 0; i < ItemsList.Count; i++)
                    {

                        // Get the right ProfilePicture (if not OK, put the default one)
                        var ProfilePicturePath = "";
                        if (!File.Exists(Path.Combine(defaultDirPath, ItemsList[i].mProfilePictureFilename)))
                        {
                            ProfilePicturePath = ItemsList[i].mProfilePictureFilename = defaultProfilePicture;
                        }
                        else
                        {
                            ProfilePicturePath = Path.Combine(defaultDirPath, ItemsList[i].mProfilePictureFilename);
                        }

                        // Convert from RestrictedUser to UserItemViewModel
                        var item = new UserItemViewModel
                        {
                            Name = ItemsList[i].mUsername,
                            IpAddress = ItemsList[i].mIpAddress,
                            Initials = ItemsList[i].mUsername[0].ToString(),
                            Message = ItemsList[i].mMessage,
                            ProfilePicture = ProfilePicturePath,
                            NewMessageAvailable = true,
                            IsSelected = false

                        };

                        if (ItemsSelected.Contains(item.IpAddress)) item.IsSelected = true;
                        ItemsSelected.Clear();

                        Items.Add(item);
                    }
                }

                Sleep:

                var time = new Random().Next(2500, 7500);
                Thread.Sleep(time);
                goto updateModel;
                
            });

            // Launch the thread
            // it will be terminated only when the interface is closed
            IoC.Application.UserDesignModelUpdater.Start();
        }

        #endregion
    }
}
