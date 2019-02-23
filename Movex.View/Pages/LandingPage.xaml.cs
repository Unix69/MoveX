using System;
using System.Windows;
using System.Threading;
using System.Text;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Dialogs;
using Movex.View.Core;

namespace Movex.View
{
    /// <summary>
    /// Interaction logic for the BrowsePage
    /// </summary>
    public partial class LandingPage : BasePage<BackboneViewModel>
    {
        #region Constructor
        public LandingPage()
        {
            InitializeComponent();
        }
        #endregion
    }
}
