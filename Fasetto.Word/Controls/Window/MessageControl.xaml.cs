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
    /// Interaction logic for MessageControl.xaml
    /// </summary>
    public partial class MessageControl : UserControl
    {
        #region Private Member(s)
        private MessageDesignModel mMessageDesignModel;
        #endregion

        #region Constructor(s)
        public MessageControl()
        {
            InitializeComponent();
            DataContext = mMessageDesignModel = new MessageDesignModel();
        }
        #endregion

        #region Getter(s) and Setter(s)
        public void SetMessage(string m)
        {
            mMessageDesignModel.SetText(m);
        }
        #endregion
        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            mMessageDesignModel.Ok();
        }
    }
}
