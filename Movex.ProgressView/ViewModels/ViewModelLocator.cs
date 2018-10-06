namespace Movex.ProgressView
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
        /// The progress view model
        /// </summary>
        public static ProgressDesignModel ProgressDesignModel => IoC.Get<ProgressDesignModel>();

        /// <summary>
        /// The progress view model
        /// </summary>
        public static MessageDesignModel MessageDesignModel => IoC.Get<MessageDesignModel>();

        #endregion
    }
}