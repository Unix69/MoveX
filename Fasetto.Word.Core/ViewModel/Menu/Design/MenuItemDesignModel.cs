namespace Movex.View.Core
{
    /// <summary>
    /// The design-time data for a <see cref="MenuItemViewModel"/>
    /// </summary>
    public class MenuItemDesignModel : MenuItemViewModel
    {
        #region Singleton

        /// <summary>
        /// A single instance of the design model
        /// </summary>
        public static UserItemDesignModel Instance => new UserItemDesignModel();

        #endregion

        #region Constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        public MenuItemDesignModel()
        {
            Initials = "LM";
            Name = "Luke";
            Message = "This chat app is awesome! I bet it will be fast too";
            ProfilePictureRGB = "3099c5";
        }

        #endregion
    }
}
