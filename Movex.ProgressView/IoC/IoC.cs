﻿using Ninject;
using System;

namespace Movex.ProgressView
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
        public static ProgressDesignModel[] Progress => IoC.Get<ProgressDesignModel[]>();

        #endregion

        #region Construction

        /// <summary>
        /// Sets up the IoC container, binds all information required and is ready for use
        /// NOTE: Must be called as soon as your application starts up to ensure all 
        ///       services can be found
        /// </summary>
        public static void Setup(int n)
        {
            // Bind all required view models
            BindViewModels(n);
        }

        /// <summary>
        /// Binds all singleton view models
        /// </summary>
        private static void BindViewModels(int n)
        {
            // Bind to a single instance of the Progress Design Model
            Kernel.Bind<ProgressDesignModel[]>().ToConstant(new ProgressDesignModel[n]);
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
