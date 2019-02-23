using System;
using System.Windows;
using System.Windows.Controls;
using Movex.View.Core;
using System.Net;
using System.Collections.Generic;

namespace Movex.View
{
    /// <summary>
    /// Interaction logic for MenuControl.xaml
    /// </summary>
    public partial class UserListControl : UserControl
    {

        #region Private Members
        int UsersSelectedNumber { get; set; }
        UserDesignModel mUserDesignModel;
        #endregion

        #region Constructor
        public UserListControl()
        {
            InitializeComponent();
            DataContext = new UserDesignModel();
            ((App)(Application.Current)).SetUserListControl(this);
            UsersSelectedNumber = ((UserDesignModel)DataContext).ItemsSelected;
        }
        #endregion

        #region Helper Method(s)
        public IPAddress[] GetIpAddressesFromUserList()
        {
            IPAddress[] IPs;
            List<UserItemViewModel> usersList;

            var n = RefreshAndGetUsersSelectedNumber();
            var UserListDesignModel = (UserDesignModel)UserList.DataContext;
            if (!(UserListDesignModel == null) && !(n == -1))
            {
                IPs = new IPAddress[n];
                usersList = UserListDesignModel.Items;

                // Iterate over the user list to catch users with the flag IsSelected
                var i = 0;
                foreach (var user in usersList)
                {
                    if (user.IsSelected)
                    {
                        IPs[i++] = IPAddress.Parse(user.IpAddress);
                    }
                }

                // Return the list of IP
                return IPs;
            }
            else
            {
                return null;
            }
        }
        public int RefreshAndGetUsersSelectedNumber()
        {
            var UserListDesignModel = (UserDesignModel)UserList.DataContext;
            if (!(UserListDesignModel == null))
            {
                // Get the user list
                var userList = UserListDesignModel.Items;

                // Get the number of selected items and allocate accordingly
                var n = 0;
                foreach (var user in userList)
                {
                    if (user.IsSelected)
                    {
                        n += 1;
                    }
                }

                // Udpate the UI updating the value
                UserListDesignModel.ItemsSelected = n;

                // Routing interface
                if (n == 0) {
                    /* TODO: evaluate to remove this piece of code
                     * 
                    if (((App)(Application.Current)).GetModeOn() == App.Mode.Traditional)
                    {
                        ((App)(Application.Current)).GetBrowsePage().ClearTransferItemsList();
                        ((App)(Application.Current)).SetBrowsePage(null);
                        IoC.Application.GoToPage(ApplicationPage.Landing);
                    }
                    */
                }
                else if (n > 0) {
                    IoC.Application.GoToPage(ApplicationPage.Browse);
                    ((App)(Application.Current)).GetBrowsePage().UpdateUsersAvailable(n);
                }
                
                // Return the vale
                return n;
            }
            else
            {
                Console.WriteLine("Cannot read the number of current selected users in UserListControl.");
                return -1;
            }
        }
        public void ClearSelectedUsers()
        {
            var UserListDesignModel = (UserDesignModel) DataContext;
            if (!(UserListDesignModel == null))
            {
                // Get the user list
                var userList = UserListDesignModel.Items;

                // Deselect the selected users
                foreach (var user in userList)
                {
                    if (user.IsSelected)
                    {
                        user.IsSelected = false;
                    }
                }

                // Udpate the UI updating the value
                UserListDesignModel.ItemsSelected = 0;

                // Go to the Landing Page
                IoC.Application.GoToPage(ApplicationPage.Landing);
            }
            else
            {
                Console.WriteLine("Cannot clear the current selection in UserListControl(ler).");
            }
        }
        public int GetUsersSelectedNumber()
        {
            return UsersSelectedNumber;
        }
        #endregion

    }
}
