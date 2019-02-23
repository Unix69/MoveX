namespace Movex.View.Core
{
    /// <summary>
    /// The design-time data for a <see cref="UserItemViewModel"/>
    /// </summary>
    public class UserItemDesignModel : UserItemViewModel
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
        public UserItemDesignModel()
        {
            Initials = "LM";
            Name = "Luke";
            Message = "This chat app is awesome! I bet it will be fast too";
            ProfilePicture = "3099c5";
        }

        #endregion
    }
}
