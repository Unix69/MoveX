using System.Windows.Controls;
using Microsoft.WindowsAPICodePack.Dialogs;
using Movex.View.Core;
using System;
using System.Windows;
using System.Threading;
using System.Windows.Data;
using System.Collections.Concurrent;
using System.Text.RegularExpressions;

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
            var Message = m;

            var IpAddress = ExtractIpAddress(m);
            if (IpAddress != null && IpAddress != "")
            {
                var UserName = IoC.User.GetUsernameByIpAddress(IpAddress);
                if (UserName != null)
                {
                    Message = "Hai ricevuto una richiesta da " + UserName + ".\r\nVuoi accettarla?";
                }
            }
            
            mYesNoDesignModel.SetText(Message);
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

        #region Utility method(s)

        private string ExtractIpAddress(string text)
        {
            var pattern = new Regex(@"\b\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}\b");
            try
            {
                var match = pattern.Matches(text);
                if (match.Count > 0)
                {
                    return match[0].ToString();
                }
                else
                {
                    return null;
                }
            }
            catch(ArgumentNullException Exception)
            {
                var Message = Exception.Message;
                Console.WriteLine("[MOVEX.VIEW] [YesNoControl.xaml.cs] [ExtractIpAddress] " + Message + ".");
                return null;
            }
        }
        public void SetNoAsResponse()
        {
            mYesNoDesignModel.No();
        }
        #endregion
    }
}
