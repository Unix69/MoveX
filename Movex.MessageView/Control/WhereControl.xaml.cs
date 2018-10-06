using System.Text;
using System.Windows;
using System.Windows.Controls;
using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.Windows.Threading;

namespace Movex.MessageView
{
    /// <summary>
    /// Interaction logic for WhereControl.xaml
    /// </summary>
    public partial class WhereControl : UserControl
    {
        public WhereControl()
        {
            InitializeComponent();
        }

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

            IoC.Where.SetPath(builder.ToString());
        }
    }
}
