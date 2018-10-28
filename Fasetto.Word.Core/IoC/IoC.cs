using Ninject;
using System;
using Movex.Network;

namespace Movex.View.Core
{
    /// <summary>
    /// The IoC container for our application
    /// </summary>
    public static class IoC
    {
        #region Public Properties

        /// <summary>
        /// The kernel for our IoC container
        /// </summary>
        public static IKernel Kernel { get; private set; } = new StandardKernel();

        /// <summary>
        /// A shortcut to access the ApplicationViewModel
        /// </summary>
        public static ApplicationViewModel Application => IoC.Get<ApplicationViewModel>();

        /// <summary>
        /// A shortcut to access the SettingsViewModel
        /// </summary>
        public static SettingsViewModel Settings => IoC.Get<SettingsViewModel>();

        /// <summary>
        /// A shortcut to access the ProfileViewModel
        /// </summary>
        public static ProfileViewModel Profile => IoC.Get<ProfileViewModel>();

        /// <summary>
        /// A shortcut to access the LocalUserItemViewModel
        /// </summary>
        public static LocalUserItemViewModel User => IoC.Get<LocalUserItemViewModel>();

        /// <summary>
        /// A shortcut to access the ProgressDesignModel
        /// </summary>
        public static ProgressDesignModel Progress => IoC.Get<ProgressDesignModel>();

        /// <summary>
        /// A shortcut to access the TransferItemListDesignModel
        /// </summary>
        public static TransferItemListDesignModel TransferItemList => IoC.Get<TransferItemListDesignModel>();

        /// <summary>
        /// Static reference for FTP Client Utility
        /// </summary>
        public static FTPclient FtpClient => IoC.Get<FTPclient>();

        /// <summary>
        /// Static reference for FTP Server Utility
        /// </summary>
        public static FTPserver FtpServer => IoC.Get<FTPserver>();


        #endregion

        #region Construction

        /// <summary>
        /// Sets up the IoC container, binds all information required and is ready for use
        /// NOTE: Must be called as soon as your application starts up to ensure all 
        ///       services can be found
        /// </summary>
        public static void Setup()
        {
            // Bind all required view models
            BindViewModels();
        }

        /// <summary>
        /// Binds all singleton view models
        /// </summary>
        private static void BindViewModels()
        {
            // Bind to a single instance of Application view model
            Kernel.Bind<ApplicationViewModel>().ToConstant(new ApplicationViewModel());
            Kernel.Bind<SettingsViewModel>().ToConstant(new SettingsViewModel());
            Kernel.Bind<ProfileViewModel>().ToConstant(new ProfileViewModel());
            Kernel.Bind<ProgressDesignModel>().ToConstant(new ProgressDesignModel());
            Kernel.Bind<LocalUserItemViewModel>().ToConstant(new LocalUserItemViewModel());
            Kernel.Bind<TransferItemListDesignModel>().ToConstant(new TransferItemListDesignModel());
            Kernel.Bind<FTPclient>().ToConstant(new FTPclient());
            Kernel.Bind<FTPserver>().ToConstant(new FTPserver());
        }

        /// <summary>
        /// Release the resources
        /// </summary>
        public static void Dispose()
        {
            User.Release();
            Application.Close();
            FtpServer.Shutdown();
        }

        #endregion

        /// <summary>
        /// Get's a service from the IoC, of the specified type
        /// </summary>
        /// <typeparam name="T">The type to get</typeparam>
        /// <returns></returns>
        public static T Get<T>()
        {
            return Kernel.Get<T>();
        }
    }
}
