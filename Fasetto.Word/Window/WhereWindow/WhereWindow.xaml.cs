using System;
using System.Windows;
using System.Threading;
using System.ComponentModel;
using System.Collections.Concurrent;
using Movex.View.Core;

namespace Movex.View
{
    /// <summary>
    /// Interaction logic for WhereWindow.xaml
    /// </summary>
    public partial class WhereWindow : Window
    { 
        public WhereWindow()
        {
            InitializeComponent();
            DataContext = new WindowViewModel(this);
        }

        public WhereWindow(ManualResetEvent WhereResponseAvailability, string whereMessage, ConcurrentBag<string> whereResponse)
        {
            InitializeComponent();
            var WinViewModel = new WindowViewModel(this);            
            DataContext = WinViewModel;

            // Passing a synchronization primitive to the YesNoControl
            var control = WhereWindow_WhereControl;
            control.SetResponseAvailability(WhereResponseAvailability);
            control.SetMessage(whereMessage);
            control.SetResponse(whereResponse);
        }
    }
}
