using System.Collections.Generic;
using Movex.Network;
using System.Threading;
using System.Windows.Forms;
using System.IO;

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
                var defaultProfilePicture = Path.Combine(currWorkingDirectory, @"Images/Icons/profile.png");

                // Get the technical user
                var u = IoC.User.GetTechnicalUser();
                List<UserItemViewModel> ItemsStore = null;

                updateModel:

                // Collect the List of User from Network Discovering
                u.GetForFriend();
                var ItemsList = u.GetFriendList();
                if (Items != null) { ItemsStore = Items; }
                Items = new List<UserItemViewModel>();
                
                if (ItemsStore != null)
                foreach(var item in ItemsStore)
                {
                     Items.Add(item);
                }

                // Inser the ModelData to the Items object
                for (var i = 0; i < ItemsList.Count; i++)
                {
                    // Convert from RestrictedUser to UserItemViewModel
                    if (!File.Exists(Path.Combine(defaultDirPath, ItemsList[i].mProfilePictureFilename)))
                        { ItemsList[i].mProfilePictureFilename = defaultProfilePicture; }
                    var candidate = new UserItemViewModel
                    {
                        Name = ItemsList[i].mUsername,
                        IpAddress = ItemsList[i].mIpAddress,
                        Initials = ItemsList[i].mUsername[0].ToString(),
                        Message = ItemsList[i].mMessage,
                        ProfilePicture = Path.Combine(defaultDirPath, ItemsList[i].mProfilePictureFilename),
                        NewMessageAvailable = true,
                        IsSelected = false
                        
                    };

                    // If it is not present in the list add
                    if (!Items.Exists(x => x.IpAddress == candidate.IpAddress)) {
                        Items.Add(candidate);
                        //SetItems(Items);
                    }
                }
                

                // Wait and then try to update again
                Thread.Sleep(2500);
                goto updateModel;
                
            });

            // Launch the thread
            // it will be terminated only when the interface is closed
            IoC.Application.UserDesignModelUpdater.Start();
        }

        #endregion

        private void SetItems(List<UserItemViewModel> items)
        {
            Items = items;
        }
    }
}
