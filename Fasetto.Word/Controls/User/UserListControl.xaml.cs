using System.Windows;
using System.Threading;
using System.Windows.Controls;
using Movex.View.Core;
using System.Net;

namespace Movex.View
{
    /// <summary>
    /// Interaction logic for MenuControl.xaml
    /// </summary>
    public partial class UserListControl : UserControl
    {
        public UserListControl()
        {
            InitializeComponent();
            DataContext = new UserDesignModel();
            ((App)(Application.Current)).SetUserListControl(this);
        }

        /// Helpers Methods
        public IPAddress[] GetIpAddressesFromUserList()
        {
            IPAddress[] IPs;
            var UserListDesignModel = (UserDesignModel) UserList.DataContext;
            if (!(UserListDesignModel == null))
            {
                // Get the user list
                var userList = UserListDesignModel.Items;

                // Get the number of selected items and allocate accordingly
                var i = 0;
                foreach (var user in userList)
                {
                    if (user.IsSelected)
                    {
                        i += 1;
                    }
                }
                IPs = new IPAddress[i];

                // Iterate over the user list to catch users with the flag IsSelected
                i = 0;
                foreach (var user in userList)
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
                System.Windows.MessageBox.Show("No user selected");
                return null;
            }
        }

    }
}
