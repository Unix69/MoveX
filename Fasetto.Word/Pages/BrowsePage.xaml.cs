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
using System.Windows.Controls;

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
        private TransferItemViewModel mSelectedItem;
        #endregion

        #region Constructor
        public BrowsePage()
        {
            InitializeComponent();
            mFilepaths = new List<string>();

            mTransferItemList                   = ViewModelLocator.TransferItemListDesignModel;
            TransferItemList.ItemsSource        = mTransferItemList.Items;
            mTransferItemList.TransferAvailable = mTransferItemList.Items.Count == 0 ? false : true;

            ((App)(Application.Current)).SetBrowsePage(this); 
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
                    var TransferItemListCount = mTransferItemList.Items.Count;

                    foreach (var item in enumerator)
                    {
                        builder.AppendLine(item);
                        mFilepaths.Add(item);
                        mTransferItemList.Items.Add(new TransferItemViewModel(++TransferItemListCount, item, new FileInfo(item).Length));
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
                    var TransferItemListCount = mTransferItemList.Items.Count;
                    foreach (var item in dialog.FileNames)
                    {
                        builder.AppendLine(item);
                        mFilepaths.Add(item);
                        var folderSize = Directory.GetFiles(item, "*", SearchOption.AllDirectories).Sum(t => (new FileInfo(t).Length));
                        mTransferItemList.Items.Add(new TransferItemViewModel(++TransferItemListCount, item, folderSize));
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
                Console.WriteLine("The users selected for the upload are: " + addresses.Length.ToString());
                var TransferAvailabilities = new ManualResetEvent[addresses.Length];
                var WindowsAvailabilities = new ManualResetEvent[addresses.Length];
                for (var k = 0; k < WindowsAvailabilities.Length; k++)
                {
                    WindowsAvailabilities[k] = new ManualResetEvent(false);
                    TransferAvailabilities[k] = new ManualResetEvent(false);
                }
                for (var i=0; i<addresses.Length; i++)
                {
                     // Scope valid for each thread only
                    {       
                        var index = i;
                        var WindowThread = new Thread(() =>
                        {
                            var address = addresses[index];
                            var windowAvailability = WindowsAvailabilities[index];
                            var uTransferAvailability = TransferAvailabilities[index];

                            new ProgressWindow(address, uTransferAvailability).Show();
                            windowAvailability.Set();
                            System.Windows.Threading.Dispatcher.Run();   
                        });
                        WindowThread.SetApartmentState(ApartmentState.STA);
                        WindowThread.Start();
                    }
                }

                IoC.FtpClient.Send(filepaths, addresses, WindowsAvailabilities, TransferAvailabilities);

            }
        }
        public void ClearTransferItemsList()
        {
            // CLEAR items from the current TransferItemList
            mTransferItemList.Items.Clear();
            mTransferItemList.TransferAvailable = false;
            mTransferItemList = null;
            TransferItemList.Items.Refresh();

            // RELEASE the member(s)
            mFilepaths.Clear();
            mFilepaths = null;

            // Clear the selected users too
            ((App)(Application.Current)).GetUserListControl().ClearSelectedUsers();
        }
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            ClearTransferItemsList();
        }
        private void TransferItemList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                mSelectedItem = (TransferItemViewModel)e.AddedItems[0];
                Console.WriteLine("Selected item: " + mSelectedItem.Path);
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message);
            }
        }
        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Get two values
                var TransferItemIndexToRemove = mSelectedItem.Index - 1;
                var TransferItemListCount = mTransferItemList.Items.Count;

                // Remove the item from the list
                mTransferItemList.Items.Remove(mSelectedItem);

                // Update all the subsequent item reducing the Index (for the alternationIndex purpose)
                for (var i = TransferItemIndexToRemove; i < TransferItemListCount - 1; i++)
                {
                    mTransferItemList.Items[i].Index -= 1;
                    mTransferItemList.Items[i].AlternationIndex = mTransferItemList.Items[i].Index % 2;
                }

                // Refresh the ViewModel
                TransferItemList.Items.Refresh();
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message);
            }

        }
        #endregion

    }
}
