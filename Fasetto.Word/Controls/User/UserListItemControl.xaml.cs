using System.Windows;
using System.Windows.Controls;

namespace Movex.View
{
    /// <summary>
    /// Interaction logic for UserListItemControl.xaml
    /// </summary>
    public partial class UserListItemControl : UserControl
    {
        public UserListItemControl()
        {
            InitializeComponent();
        }

        private void CheckBox_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            int n;

            var userListControl = ((App)(Application.Current)).GetUserListControl();
            if (userListControl != null)
            {
                n = userListControl.RefreshAndGetUsersSelectedNumber();
                var browsePage = ((App)(Application.Current)).GetBrowsePage();
                if (browsePage != null)
                {
                    browsePage.UpdateUsersAvailable(n);
                }
            }
        }
    }
}
