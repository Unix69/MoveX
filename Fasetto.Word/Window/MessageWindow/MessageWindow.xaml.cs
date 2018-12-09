using System;
using System.Windows;
using System.Threading;
using System.ComponentModel;
using System.Collections.Concurrent;
using Movex.View.Core;

namespace Movex.View
{
    /// <summary>
    /// Interaction logic for MessageWindow.xaml
    /// </summary>
    public partial class MessageWindow : Window
    {
        public MessageWindow()
        {
            InitializeComponent();
            DataContext = new WindowViewModel(this);
        }

        public MessageWindow(ManualResetEvent MessageWindowAvailability)
        {
            InitializeComponent();
            DataContext = new WindowViewModel(this);

            // Passing a synchronization primitive to the YesNoControl
            var control = MessageWindow_ProgressControl;
            control.SetResponseAvailability(MessageWindowAvailability);
        }

        public MessageWindow(ManualResetEvent MessageWindowAvailability, string message, ConcurrentBag<string> response)
        {
            InitializeComponent();
            DataContext = new WindowViewModel(this);

            // Passing a synchronization primitive to the YesNoControl
            var control = MessageWindow_ProgressControl;
            control.SetResponseAvailability(MessageWindowAvailability);
            control.SetMessage(message);
            control.SetResponse(response);
        }



    }
}
