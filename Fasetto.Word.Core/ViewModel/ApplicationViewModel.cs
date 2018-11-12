using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Movex.View.Core
{
    /// <summary>
    /// The application state as a view model
    /// </summary>
    public class ApplicationViewModel : BaseViewModel
    {
        /// <summary>
        /// The current page of the application
        /// </summary>
        public ApplicationPage CurrentPage { get; private set; } = ApplicationPage.Landing;

        /// <summary>
        /// True if the side menu should be shown
        /// </summary>
        public bool SideMenuVisible { get; set; } = true;

        /// <summary>
        /// True if the side settings menu should be shown
        /// </summary>
        public bool SettingsMenuVisible { get; set; } = false;

        /// <summary>
        /// True if the side profile menu should be shown
        /// </summary>
        public bool ProfileMenuVisible { get; set; } = false;

        /// <summary>
        /// Thread that updates continuously the list of users at UserDeisignModel
        /// </summary>
        public Thread UserDesignModelUpdater { get; set; } = null;

        /// <summary>
        /// Navigates to the specified page
        /// </summary>
        /// <param name="page">The page to go to</param>
        public void GoToPage(ApplicationPage page)
        {
            // Set the current page
            CurrentPage = page;

            // Show side menu or not?
            // SideMenuVisible = page == ApplicationPage.Chat;

        }

        /// <summary>
        /// The method to terminate the application active threads.
        /// </summary>
        public void Close() {

            // Stop the updater for the User Design Model
            UserDesignModelUpdater.Abort();
            // UserDesignModelUpdater.Join();
            // UserDesignModelUpdater.Interrupt();
            UserDesignModelUpdater = null;

        }

    }
}
