using System.Windows.Controls;
using Movex.View.Core;

namespace Movex.View
{
    /// <summary>
    /// Interaction logic for ToggleControl.xaml
    /// </summary>
    public partial class ProgressControl : UserControl
    {
        public ProgressControl()
        {
            InitializeComponent();
            DataContext = new ProgressDesignModel();
        }
    }
}
