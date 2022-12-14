using System;
using System.Windows;
using System.Threading;
using System.ComponentModel;
using System.Collections.Concurrent;
using Movex.View.Core;

namespace Movex.View
{
    /// <summary>
    /// Interaction logic for YesNoWindow.xaml
    /// </summary>
    public partial class YesNoWindow : Window
    {
        

        public YesNoWindow(ManualResetEvent YesNoWindowAvailability, string message, ConcurrentBag<string> response)
        {
            InitializeComponent();
            DataContext = new WindowViewModel(this);

            // Passing a synchronization primitive to the YesNoControl
            var control = YesNoWindow_ProgressControl;
            control.SetResponseAvailability(YesNoWindowAvailability);
            control.SetMessage(message);
            control.SetResponse(response);
        }

        private void Close_Event(object sender, RoutedEventArgs e)
        {
            YesNoWindow_ProgressControl.SetNoAsResponse();
        }
    }
}
