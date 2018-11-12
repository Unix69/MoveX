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
            ((App)(Application.Current)).GetUserListControl().RefreshAndGetUsersSelectedNumber();
            // In this case I do not need to catch the value returned.
        }
    }
}
