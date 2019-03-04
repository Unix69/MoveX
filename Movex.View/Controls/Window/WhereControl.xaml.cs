using System.Text;
using System.Windows;
using System.Windows.Controls;
using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using Movex.View.Core;
using System.Threading;
using System.Collections.Concurrent;

namespace Movex.View
{
    /// <summary>
    /// Interaction logic for WhereControl.xaml
    /// </summary>
    public partial class WhereControl : UserControl
    {

        #region Private Member(s)
        private ManualResetEvent mResponseAvailability;
        private WhereDesignModel mWhereDesignModel;
        private bool mOnExit;
        private bool mIsFolderSelected;
        private ManualResetEvent mFolderSelected;
        #endregion

        public WhereControl()
        {
            InitializeComponent();
            DataContext = mWhereDesignModel = new WhereDesignModel();
            mIsFolderSelected = false;
        }

        #region Getter(s) and Setter(s)
        public void SetResponseAvailability(ManualResetEvent e)
        {
            mWhereDesignModel.SetResponseAvailability(e);
        }
        public void SetMessage(string m)
        {
            mWhereDesignModel.SetText(m);
        }
        public void SetResponse(ConcurrentBag<string> r)
        {
            mWhereDesignModel.SetResponse(r);
        }
        public void SetOnExit(bool OnExit)
        {
            mOnExit = OnExit;
        }
        public void SetFolderSelectedEvent(ManualResetEvent FolderSelected)
        {
            mFolderSelected = FolderSelected;
        }
        #endregion

        private void ScanButton_BrowseFolder(object sender, RoutedEventArgs e)
        {

            var builder = new StringBuilder();
            var dialog = new CommonOpenFileDialog()
            {
                Multiselect = false,
                IsFolderPicker = true,
                AllowNonFileSystemItems = true,
                ShowPlacesList = true
            };

            Application.Current.Dispatcher.Invoke(
                (Action)(() => { dialog.ShowDialog(); }));


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
                }
                mIsFolderSelected = true;
            }

            mWhereDesignModel.SetPath(builder.ToString());
            mFolderSelected.Set();
            
        }

        private void ProseguiClick(object sender, RoutedEventArgs e)
        {
            if (!mIsFolderSelected)
            {
                mWhereDesignModel.SetPath(IoC.User.DownloadDefaultFolder);
            }
            mOnExit = true;
            mFolderSelected.Set();
            mWhereDesignModel.Save();
        }
    }
}
