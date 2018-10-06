using System.Windows.Controls;
using Microsoft.WindowsAPICodePack.Dialogs;
using Movex.View.Core;
using System;
using System.Windows;
using System.Threading;
using System.Windows.Data;
using System.Collections.Concurrent;

namespace Movex.View
{
    /// <summary>
    /// Interaction logic for YesNoControl.xaml
    /// </summary>
    public partial class YesNoControl : UserControl
    {
        #region Private Member(s)
        private ManualResetEvent mResponseAvailability;
        private YesNoDesignModel mYesNoDesignModel;
        #endregion

        #region Constructor(s)
        public YesNoControl()
        {
            InitializeComponent();
            DataContext = mYesNoDesignModel = new YesNoDesignModel();
        }
        #endregion

        #region Getter(s) and Setter(s)
        public void SetResponseAvailability(ManualResetEvent e)
        {
            mYesNoDesignModel.SetResponseAvailability(e);
        }
        public void SetMessage(string m)
        {
            mYesNoDesignModel.SetText(m);
        }
        public void SetResponse(ConcurrentBag<string> r)
        {
            mYesNoDesignModel.SetResponse(r);
        }
        #endregion

        private void NoButton_Click(object sender, RoutedEventArgs e)
        {
            mYesNoDesignModel.No();
        }

        private void YesButton_Click(object sender, RoutedEventArgs e)
        {
            mYesNoDesignModel.Yes();
        }
    }
}
