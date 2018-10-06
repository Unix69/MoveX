using System.Windows.Input;
using System.IO;
using System;
using System.Text;
using System.Windows.Forms;
using Movex.Network;
using System.Threading;
using System.Collections.Generic;

namespace Movex.View.Core
{
    /// <summary>
    /// A view model for the user
    /// </summary>
    public class LocalUserItemViewModel : UserItemViewModel
    {
        #region Public Properties

        // All the public properties are inherited from UserItemViewModel

        public string PrivateMode { get; set; }

        public string AutomaticReception { get; set; }

        public string AutomaticSave { get; set; }

        #endregion

        #region Private Members

        private User mUser;
        private Database mDatabase;

        #endregion

        #region Public Command

        /// <summary>
        /// The command to set the Profile Picture
        /// </summary>
        public ICommand SetProfilePictureCommand { get; set; }

        #endregion

        #region Constructor

        /// <summary>
        /// Default Constructor
        /// </summary>
        public LocalUserItemViewModel()
        {
            mDatabase = new Database();
            var errorOccurrences = 0;

            // Load the local values from the database
            GetValues:
            var dictionary = mDatabase.GetValues();
            if (dictionary == null)
            {
                MessageBox.Show("Errore #DB001: errore nel caricamento dei dati utente.\r\n"
                                    + "Contatta il supporto.");
            }

            // Set the values got from the local database
            try
            { 
                ProfilePicture          = dictionary["ProfilePicture"];
                Name                    = dictionary["Name"];
                Message                 = dictionary["Message"];
                IpAddress               = dictionary["IpAddress"];
                DownloadDefaultFolder   = dictionary["DownloadDefaultFolder"];
                AutomaticSave           = dictionary["AutomaticSave"];
                AutomaticReception      = dictionary["AutomaticReception"];
                PrivateMode             = dictionary["PrivateMode"];
            }
            catch (KeyNotFoundException exc)
            {
                errorOccurrences += 1;

                if (errorOccurrences >= 2)
                {
                    MessageBox.Show("Errore #DB003: errore nel caricamento dei dati utente.\r\n"
                                        +"Contatta il supporto.");
                    // In this case:
                    // The view will not show any value (the fields will be empty)
                }
                else
                {
                    MessageBox.Show("Errore #DB002: errore nel caricamento dei dati utente.\r\n"
                                        +"Contatta il supporto.");
                    mDatabase.Drop();
                    goto GetValues;
                }
            }

            mUser = new User(Name, Message, ProfilePicture, Convert.ToBoolean(PrivateMode));

            // Initialize the research for friends by network
            mUser.ListenForFriends();
            mUser.SearchForFriends();
            Thread.Sleep(100);

        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Set the profile picture path
        /// </summary>
        public void SetProfilePicture(string path)
        {
            ProfilePicture = path;
            mDatabase.UpdateLocalDB(nameof(ProfilePicture), path);
            mUser.SetProfilePicturePath(path);
        }

        /// <summary>
        /// Set the default path of the Folder for the Downloads
        /// </summary>
        public void SetDownloadDefaultFolder(string path)
        {
            DownloadDefaultFolder = path;
            mDatabase.UpdateLocalDB(nameof(DownloadDefaultFolder), path);
            IoC.FtpServer.SetDownloadDefaultFolder(path);
        }

        public void SetAutomaticSave(string value)
        {
            AutomaticSave = value;
            mDatabase.UpdateLocalDB(nameof(AutomaticSave), value);
        }

        public void SetPrivateMode(string value)
        {
            PrivateMode = value;
            mDatabase.UpdateLocalDB(nameof(PrivateMode), value);
            IoC.FtpServer.SetPrivateMode(Convert.ToBoolean(value));
            mUser.SetPrivateMode(Convert.ToBoolean(value));
        }

        /// <summary>
        /// Set the name of the user
        /// </summary>
        public void SetName(string name)
        {
            Name = name;
           mDatabase.UpdateLocalDB(nameof(Name), name);
        }

        /// <summary>
        /// Set the message of the user
        /// </summary>
        public void SetMessage(string message)
        {
            Message = message;
            mDatabase.UpdateLocalDB(nameof(Message), message);
        }

        /// <summary>
        /// Get the User Object of the Movex.Network Project
        /// </summary>
        public User GetTechnicalUser() {
            return mUser;
        }

        /// <summary>
        /// Launch a call towards the other users in the network
        /// </summary>
        public void SearchForUsers()
        {
            mUser.SearchForFriends();
        }
        #endregion
    }


}
