using System;
using System.Collections.Generic;

namespace Movex.View.Core
{
    /// <summary>
    /// The design-time data for a <see cref="ToggleListViewModel"/>
    /// </summary>
    public class ToggleListDesignModel : ToggleListViewModel
    {
        #region Singleton

        /// <summary>
        /// A single instance of the design model
        /// </summary>
        public static ToggleListDesignModel Instance => new ToggleListDesignModel();

        #endregion

        #region Constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        public ToggleListDesignModel()
        {
            Items = new List<ToggleViewModel>
            {
                new ToggleViewModel
                {
                   Name = "Modalità privata",
                   Explanation = "Impedisci agli altri di trovarti ma puoi comunque inviare files.",
                   Active = Convert.ToBoolean(IoC.User.PrivateMode)
                },

                new ToggleViewModel
                {
                   Name = "Ricezione automatica",
                   Explanation = "Accetti automaticamente tutti le richieste di trasferimento.",
                   Active = Convert.ToBoolean(IoC.User.AutomaticReception)
                },

                new ToggleViewModel
                {
                   Name = "Salvataggio automatico",
                   Explanation = "I trasferimenti vengono salvati in una cartella di default.",
                   Active = Convert.ToBoolean(IoC.User.AutomaticSave)
                }
                /*
                new ToggleViewModel
                {
                   Name = "Ottieni massime prestazioni",
                   Explanation = "Aumenti l'utilizzo della CPU e richiedi più energia.",
                   Active = false
                }
                */

            };
        }

        #endregion
    }
}
