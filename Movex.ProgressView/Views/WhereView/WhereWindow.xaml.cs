using System;
using System.Windows;
using System.Threading;
using System.ComponentModel;

namespace Movex.ProgressView
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
    }
}
