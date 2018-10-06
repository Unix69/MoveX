using Movex.View.Core;
using System.Windows;
using System.ComponentModel;

namespace Movex.ShortcutView
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private string[] mArgs;
        private int      mIndex;
        
        protected override void OnStartup(StartupEventArgs e)
        {
            // Take the arguments passed and store them
            mArgs = new string[e.Args.Length];
            foreach (var arg in e.Args)
            {
                mArgs[mIndex++] = arg;                
            }

            base.OnStartup(e);

            // Setup IoC
            IoC.Setup();

            // Setup MainWindow
            MainWindow = new MainWindow();
            MainWindow.Closing += MainWindow_Closing;
            MainWindow.Show();
        }

        private void MainWindow_Closing(object sender, CancelEventArgs e)
        {
            e.Cancel = true;
            MainWindow.Visibility = Visibility.Hidden;
            //MainWindow.Close();
            Shutdown();
        }

        public string[] GetArgs()
        {
            if (!(mArgs.Length == 0))
            {
                return mArgs;
            }
            else
            {
                return null;
            }
        }
    }
}
