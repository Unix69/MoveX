using System.Windows;
using System.Threading;
using System.Collections.Concurrent;

namespace Movex.View
{
    /// <summary>
    /// Interaction logic for WhereWindow.xaml
    /// </summary>
    public partial class WhereWindow : Window
    { 
        public WhereWindow(ManualResetEvent WhereResponseAvailability, string whereMessage, ConcurrentBag<string> whereResponse)
        {
            InitializeComponent();
            var WinViewModel = new WindowViewModel(this);            
            DataContext = WinViewModel;

            // Passing a synchronization primitive to the WhereControl
            var control = WhereWindow_WhereControl;
            control.SetResponseAvailability(WhereResponseAvailability);
            control.SetMessage(whereMessage);
            control.SetResponse(whereResponse);
        }
    }
}
