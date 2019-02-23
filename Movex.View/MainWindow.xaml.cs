using Movex.View.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Movex.View
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static IntPtr WindowHandle { get; private set; }

        public MainWindow()
        {
            InitializeComponent();
            DataContext = new WindowViewModel(this);

            Loaded += (s, e) =>
            {
                MainWindow.WindowHandle = new WindowInteropHelper(Application.Current.MainWindow).Handle;
                HwndSource.FromHwnd(MainWindow.WindowHandle)?.AddHook(new HwndSourceHook(HandleMessages));
            };
        }

        internal static void HandleParameter(string[] args) {
            ((App)Application.Current).SetModeOn(App.Mode.Contextual);
            ((App)Application.Current).SetArguments(args);
            IoC.Application.GoToPage(ApplicationPage
                .Browse);
        }

        private static IntPtr HandleMessages(IntPtr handle, int message, IntPtr wParameter, IntPtr lParameter, ref Boolean handled)
        {
            if (handle != MainWindow.WindowHandle)
                return IntPtr.Zero;

            var data = UnsafeNative.GetMessage(message, lParameter);

            if (data != null)
            {
                if (Application.Current.MainWindow == null)
                    return IntPtr.Zero;

                if (Application.Current.MainWindow.WindowState == WindowState.Minimized)
                    Application.Current.MainWindow.WindowState = WindowState.Normal;

                UnsafeNative.SetForegroundWindow(new WindowInteropHelper(Application.Current.MainWindow).Handle);

                var args = data.Split('|');
                HandleParameter(args);
                handled = true;
            }

            return IntPtr.Zero;
        }
    }
}
