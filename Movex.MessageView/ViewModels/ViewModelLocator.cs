namespace Movex.MessageView
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
        /// The Message Design Model
        /// </summary>
        public static MessageDesignModel MessageDesignModel => IoC.Message;

        /// <summary>
        /// The YesNo Design Model
        /// </summary>
        public static YesNoDesignModel YesNoDesignModel => IoC.YesNo;

        /// <summary>
        /// The Where Design Model
        /// </summary>
        public static WhereDesignModel WhereDesignModel => IoC.Where;

        /// <summary>
        /// The Progress Design Model
        /// </summary>
        public static ProgressDesignModel ProgressDesignModel => IoC.Progress;

        #endregion
    }
}