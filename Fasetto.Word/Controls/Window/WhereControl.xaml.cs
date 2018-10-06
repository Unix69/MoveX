using System.Text;
using System.Windows;
using System.Windows.Controls;
using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.Windows.Threading;
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
        #endregion

        public WhereControl()
        {
            InitializeComponent();
            DataContext = mWhereDesignModel = new WhereDesignModel();
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
            }

            mWhereDesignModel.SetPath(builder.ToString());
        }

        private void ProseguiClick(object sender, RoutedEventArgs e)
        {
            mWhereDesignModel.Save();
        }
    }
}
