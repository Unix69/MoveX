using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace Movex.MessageView
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            Setup();
            base.OnStartup(e);
        }

        /// <summary>
        /// Setup configuration to handle dynamic ViewModel(s)
        /// </summary>
        public void Setup() {
            IoC.Setup();
        }
    }
}
