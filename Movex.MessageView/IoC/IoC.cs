using Ninject;
using System;

namespace Movex.MessageView
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
        /// A shortcut to access the MessageDesignModel
        /// </summary>
        public static MessageDesignModel Message => IoC.Get<MessageDesignModel>();

        /// <summary>
        /// A shortcut to access the YesNoDesignModel
        /// </summary>
        public static YesNoDesignModel YesNo => IoC.Get<YesNoDesignModel>();

        /// <summary>
        /// A shortcut to access the WhereDesignModel
        /// </summary>
        public static WhereDesignModel Where => IoC.Get<WhereDesignModel>();

        /// <summary>
        /// A shortcut to access the WhereDesignModel
        /// </summary>
        public static ProgressDesignModel Progress => IoC.Get<ProgressDesignModel>();

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
            Kernel.Bind<MessageDesignModel>().ToConstant(new MessageDesignModel());
            Kernel.Bind<YesNoDesignModel>().ToConstant(new YesNoDesignModel());
            Kernel.Bind<WhereDesignModel>().ToConstant(new WhereDesignModel());
            Kernel.Bind<ProgressDesignModel>().ToConstant(new ProgressDesignModel());
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
