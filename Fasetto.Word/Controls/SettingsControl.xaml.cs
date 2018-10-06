using System.Windows.Controls;
using Movex.View.Core;
using System.Text;
using Microsoft.WindowsAPICodePack.Dialogs;
using System;

namespace Movex.View
{
    /// <summary>
    /// Interaction logic for SettingsControl.xaml
    /// </summary>
    public partial class SettingsControl : UserControl
    {
        string mPath;

        public SettingsControl()
        {
            InitializeComponent();
            DataContext = IoC.Settings;
        }

        private void Search(object sender, System.Windows.RoutedEventArgs e)
        {

            var builder = new StringBuilder();
            var dialog = new CommonOpenFileDialog()
            {
                Multiselect = false,
                IsFolderPicker = true,
                AllowNonFileSystemItems = true,
                ShowPlacesList = true
            };

            dialog.ShowDialog();
            try
            {
                if (!(dialog.FileName == null))
                {
                    mPath = dialog.FileName;
                    IoC.User.SetDownloadDefaultFolder(mPath);
                }
            }
            catch (InvalidOperationException exc) { }
        }
    }
}