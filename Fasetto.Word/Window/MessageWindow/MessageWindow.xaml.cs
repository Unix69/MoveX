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
        public MessageWindow(string message)
        {
            InitializeComponent();
            DataContext = new WindowViewModel(this);

            // Passing a synchronization primitive to the MessageControl
            var control = MessageWindow_MessageControl;
            control.SetMessage(message);

            Console.WriteLine("[Movex.View] [MessageWindow.xaml.cs] [MessageWindow] New MessageWindow crated.");
        }
    }
}
