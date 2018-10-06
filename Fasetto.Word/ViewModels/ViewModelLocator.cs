using Movex.View.Core;

namespace Movex.View
{
    /// <summary>
    /// Locates view models from the IoC for use in binding in Xaml files
    /// </summary>
    public class ViewModelLocator
    {
        #region Public Properties

        /// <summary>
        /// Singleton instance of the locator
        /// </summary>
        public static ViewModelLocator Instance { get; private set; } = new ViewModelLocator();

        /// <summary>
        /// The application view model
        /// </summary>
        public static ApplicationViewModel ApplicationViewModel => IoC.Get<ApplicationViewModel>();

        /// <summary>
        /// The settings view model
        /// </summary>
        public static SettingsViewModel SettingsViewModel => IoC.Get<SettingsViewModel>();

        /// <summary>
        /// The profile view model
        /// </summary>
        public static ProfileViewModel ProfileViewModel => IoC.Get<ProfileViewModel>();

        /// <summary>
        /// The User view model
        /// </summary>
        public static UserItemViewModel UserItemViewModel => IoC.Get<LocalUserItemViewModel>();

        /// <summary>
        /// The Progress Design Model
        /// </summary>
        public static ProgressDesignModel ProgressDesignModel => IoC.Get<ProgressDesignModel>();

        #endregion
    }
}