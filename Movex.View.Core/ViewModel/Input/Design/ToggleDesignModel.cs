namespace Movex.View.Core
{
    /// <summary>
    /// The design-time data for a <see cref="ToogleViewModel"/>
    /// </summary>
    public class ToggleDesignModel : ToggleViewModel
    {
        #region Singleton

        /// <summary>
        /// A single instance of the design model
        /// </summary>
        public static ToggleDesignModel Instance => new ToggleDesignModel();

        #endregion

        #region Constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        public ToggleDesignModel()
        {
            Name = "Modalità privata";
            Explanation = "Impedisci agli altri di trovarti ma puoi comunque inviare files.";
            Active = false;
        }

        #endregion
    }
}
