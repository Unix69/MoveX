using System;
using System.Windows;
using System.Threading;
using System.Text;
using System.IO;
using System.Collections.Generic;
using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Dialogs;
using Movex.View.Core;

namespace Movex.View
{
    /// <summary>
    /// Interaction logic for BrowsePage.xaml
    /// </summary>
    public partial class BrowsePage : BasePage<BackboneViewModel>
    {
        /// <summary>
        /// Private Members
        /// </summary>
        private List<string> mFilepaths;
        private TransferItemListDesignModel mTransferItemList;

        /// <summary>
        /// Constructor
        /// </summary>
        public BrowsePage()
        {
            InitializeComponent();
            mFilepaths = new List<string>();

            mTransferItemList = ViewModelLocator.TransferItemListDesignModel;
            TransferItemList.ItemsSource = mTransferItemList.Items;
        }

        #region Utility methods
        private void ScanButton_BrowseFiles(object sender, RoutedEventArgs e)
        {

            var defaultFilename = "Select a folder";
            var fileDialog = new OpenFileDialog()
            {
                ValidateNames = false,
                FileName = defaultFilename,
                CheckFileExists = false,
                CheckPathExists = true,
                Multiselect = true
            };

            if (fileDialog.ShowDialog().Value)
            {

                // Check if the user picked a file or a directory, for example:
                if (!fileDialog.FileName.Contains(defaultFilename))
                {
                    // File code
                }
                else // You should probably turn this into an else if instead
                {
                    // Directory code
                }

                

            }

            var builder = new StringBuilder();
            var count = 0;
            var enumerator = fileDialog.FileNames;

            foreach(var item in enumerator)
            {
                builder.AppendLine(item);
                mFilepaths.Add(item);
                mTransferItemList.Items.Add(new TransferItemViewModel(item, new FileInfo(item).Length));
                TransferItemList.Items.Refresh();
                count++;
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
                    }
                }
            }
            catch (Exception)
            {
                return;
            }
        }
        #endregion

        private void SendButton_Click(object sender, RoutedEventArgs e)
        {
            var builder = new StringBuilder();
            var filepaths = mFilepaths.ToArray();

            // Get the ip addresses selected by the user
            var addresses = ((App) Application.Current).GetUserListControl().GetIpAddressesFromUserList();

            // Ask to the FTP Client Service to send data
            if (!(filepaths == null) && !(addresses == null))
            {
                foreach (var item in filepaths) { builder.AppendLine(item); }
                foreach(var item in addresses) { builder.AppendLine(item.ToString()); }
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
    }
}
