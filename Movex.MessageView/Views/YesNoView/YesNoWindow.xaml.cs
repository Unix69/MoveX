using System;
using System.Windows;
using System.Threading;
using System.ComponentModel;
using System.Collections.Concurrent;

namespace Movex.MessageView
{
    /// <summary>
    /// Interaction logic for YesNoWindow.xaml
    /// </summary>
    public partial class YesNoWindow : Window
    { 
        public YesNoWindow()
        {
            InitializeComponent();
            DataContext = new WindowViewModel(this);
        }

        /// <summary>
        /// Constructor with some text
        /// </summary>
        /// <param name="text"></param>
        public YesNoWindow(string text, ManualResetEvent CloseWindowEvent, ConcurrentStack<string> response)
        {
            // Settings for the Window
            InitializeComponent();
            var WinViewModel = new WindowViewModel(this);
            WinViewModel.SetCloseWindowEvent(CloseWindowEvent);
            DataContext = WinViewModel;

            // Settings for the internal user-controls
            SetText(text);
            SetCloseWindowEventHandler(CloseWindowEvent);
            SetResponse(response);
        }

        /// <summary>
        /// Set the message 
        /// </summary>
        /// <param name="text"></param>
        private void SetText(string text)
        {
            IoC.YesNo.SetText(text);
        }
        private void SetCloseWindowEventHandler(ManualResetEvent CloseWindowEvent)
        {
            IoC.YesNo.SetCloseWindowEvent(CloseWindowEvent);
        }
        private void SetResponse(ConcurrentStack<string> response)
        {
            IoC.YesNo.SetResponse(response);
        }
    }
}
