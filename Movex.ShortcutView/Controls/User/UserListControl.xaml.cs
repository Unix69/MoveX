using System.Windows;
using System.Windows.Controls;
using System.Net;
using System.IO;
using System.Threading;
using Movex.View.Core;

namespace Movex.ShortcutView
{
    public partial class UserListControl : UserControl
    {
        public UserListControl()
        {
            InitializeComponent();
        }

        /// Helpers Methods
        private IPAddress[] GetIpAddressesFromUserList()
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
                MessageBox.Show("No user selected");
                return null;
            }
        }
        private void GetFoldersPathsAndFilesName(string[] absolutePaths, ref string[] folders, ref string[] files)
        {
            var size = absolutePaths.Length;
            folders = new string[size];
            files = new string[size];

            var i = 0;
            foreach (var path in absolutePaths)
            {
                folders[i] = Path.GetDirectoryName(path);
                files[i] = Path.GetFileName(path);
                i += 1;
            }

        }

        /// <summary>
        /// Send the files at button click
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            // Get the IP addresses 
            var IPs = GetIpAddressesFromUserList();

            // Get the folders and filenames
            var absolutePaths = ((App) Application.Current).GetArgs();
            string[] folders = null;
            string[] files = null;
            GetFoldersPathsAndFilesName(absolutePaths, ref folders, ref files);
            MessageBox.Show("I am going to send (using TCP): " + folders[0] + ", " + files[0] + ".");

            // Send the content
            new Thread(() => IoC.FtpClient.Send(100, folders, files, IPs, 2000)).Start();
        }
    }
}
