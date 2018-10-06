using System;
using System.Windows;
using System.Threading;
using System.ComponentModel;

namespace Movex.ProgressView
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MessageWindow : Window
    { 
        public MessageWindow()
        {
            InitializeComponent();
            DataContext = new WindowViewModel(this);
        }   
    }
}
