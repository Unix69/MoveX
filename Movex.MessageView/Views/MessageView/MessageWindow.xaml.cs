using System;
using System.Windows;
using System.Threading;
using System.ComponentModel;

namespace Movex.MessageView
{
    /// <summary>
    /// Interaction logic for MessageWindow.xaml
    /// </summary>
    public partial class MessageWindow : Window
    {
        
        /// <summary>
        /// Default constructor
        /// </summary>
        public MessageWindow()
        {
            InitializeComponent();
            DataContext = new WindowViewModel(this);
        }

        /// <summary>
        /// Constructor with some text
        /// </summary>
        /// <param name="text"></param>
        public MessageWindow(string text, ManualResetEvent CloseWindowEvent)
        {

            // Settings for the Window
            InitializeComponent();
            var WinViewModel = new WindowViewModel(this);
            WinViewModel.SetCloseWindowEvent(CloseWindowEvent);
            DataContext = WinViewModel;

            // Settings for the interal user-controls
            SetText(text);
            SetCloseWindowEventHandler(CloseWindowEvent);
        }

        private void SetCloseWindowEventHandler(ManualResetEvent CloseWindowEvent)
        {
            IoC.Message.SetCloseWindowEventHandler(CloseWindowEvent);
        }

        /// <summary>
        /// Set the message to the MessageViewModel
        /// </summary>
        /// <param name="text"></param>
        public void SetText(string text)
        {
            IoC.Message.SetText(text);
        }
    }
}
