using System;
using System.Windows;
using System.Threading;
using System.ComponentModel;

namespace Movex.ProgressView
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
    }
}
