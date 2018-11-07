using System;
using System.Windows;
using System.Threading;
using System.Text;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Dialogs;
using Movex.View.Core;

namespace Movex.View
{
    /// <summary>
    /// Interaction logic for the BrowsePage
    /// </summary>
    public partial class BrowsePage : BasePage<BackboneViewModel>
    {
        #region Private members
        private List<string> mFilepaths;
        private TransferItemListDesignModel mTransferItemList;
        #endregion

        #region Constructor
        public BrowsePage()
        {
            InitializeComponent();
            mFilepaths = new List<string>();

            mTransferItemList                   = ViewModelLocator.TransferItemListDesignModel;
            TransferItemList.ItemsSource        = mTransferItemList.Items;
            mTransferItemList.TransferAvailable = mTransferItemList.Items.Capacity == 0 ? false : true;
        }
        #endregion

        #region Utility methods
        private void ScanButton_BrowseFiles(object sender, RoutedEventArgs e)
        {
            var defaultFilename = "Select a file";
            var fileDialog = new OpenFileDialog()
            {
                ValidateNames = false,
                CheckFileExists = false,
                CheckPathExists = true,
                Multiselect = true
            };

            if (fileDialog.ShowDialog().Value)
            {
                if (!fileDialog.FileName.Contains(defaultFilename))
                {
                    var builder = new StringBuilder();
                    var enumerator = fileDialog.FileNames;

                    foreach (var item in enumerator)
                    {
                        builder.AppendLine(item);
                        mFilepaths.Add(item);
                        mTransferItemList.Items.Add(new TransferItemViewModel(item, new FileInfo(item).Length));
                        TransferItemList.Items.Refresh();
                        mTransferItemList.TransferAvailable = true;
                    }
                }
                else {}
            }
        }
        private void ScanButton_BrowseFolders(object sender, RoutedEventArgs e)
        {

            var builder = new StringBuilder();
            var dialog = new CommonOpenFileDialog()
            {
                Multiselect = true,
                IsFolderPicker = true,
                AllowNonFileSystemItems = true,
                ShowPlacesList = true
            };

            dialog.ShowDialog();

            try
            {
                var count = 0;
                using (var enumerator = dialog.FileNames.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                        count++;
                }

                if (count > 0)
                {
                    foreach (var item in dialog.FileNames)
                    {
                        builder.AppendLine(item);
                        mFilepaths.Add(item);
                        var folderSize = Directory.GetFiles(item, "*", SearchOption.AllDirectories).Sum(t => (new FileInfo(t).Length));
                        mTransferItemList.Items.Add(new TransferItemViewModel(item, folderSize));
                        TransferItemList.Items.Refresh();
                        mTransferItemList.TransferAvailable = true;
                    }
                }
            }
            catch (Exception)
            {
                return;
            }
        }
        private void SendButton_Click(object sender, RoutedEventArgs e)
        {
            var builder = new StringBuilder();
            var filepaths = mFilepaths.ToArray();

            // Get the ip addresses selected by the user
            var addresses = ((App)Application.Current).GetUserListControl().GetIpAddressesFromUserList();

            // Ask to the FTP Client Service to send data
            if (!(filepaths == null) && !(addresses == null))
            {
                foreach (var item in filepaths) { builder.AppendLine(item); }
                foreach (var item in addresses) { builder.AppendLine(item.ToString()); }
                System.Windows.MessageBox.Show(builder.ToString());

                var WindowThread = new Thread(() =>
                {
                    var windowAvailability = new ManualResetEvent(false);
                    new ProgressWindow(IoC.FtpClient, filepaths, addresses, windowAvailability).Show();
                    windowAvailability.Set();
                    System.Windows.Threading.Dispatcher.Run();
                });
                WindowThread.SetApartmentState(ApartmentState.STA);
                WindowThread.Start();

            }
        }
        #endregion
    }
}
