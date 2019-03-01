using System.Windows.Input;
using System.IO;
using System;
using System.Text;
using System.Windows.Forms;
using Movex.Network;
using System.Threading;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

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
        public bool FriendsAvailable { get; set; }
        #endregion

        #region Private Members
        private User mUser;
        private ManualResetEvent mUpdateEvent;
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
            var errorOccurrences = 0;

            // Load the local values from the database
            GetValues:
            var dictionary = Database.GetValues();
            if (dictionary == null)
            {
                Console.WriteLine("Errore #DB001: errore nel caricamento dei dati utente.");
            }
            else
            {
                // Set the values got from the local database
                try
                {
                    ProfilePicture = dictionary["ProfilePicture"];
                    Name = dictionary["Name"];
                    Message = dictionary["Message"];
                    IpAddress = dictionary["IpAddress"];
                    DownloadDefaultFolder = dictionary["DownloadDefaultFolder"];
                    AutomaticSave = dictionary["AutomaticSave"];
                    AutomaticReception = dictionary["AutomaticReception"];
                    PrivateMode = dictionary["PrivateMode"];

                    if (!File.Exists(ProfilePicture))
                    {
                        var currWorkingDirectory = Directory.GetCurrentDirectory();
                        ProfilePicture = SetDefaultProfilePicture(@"Images\Icons\profile.png");
                    }

                }
                catch (KeyNotFoundException exception)
                {
                    Console.WriteLine(exception.Message);
                    errorOccurrences += 1;

                    if (errorOccurrences >= 2)
                    {
                        Console.WriteLine("Errore #DB003: errore nel caricamento dei dati utente.");
                    }
                    else
                    {
                        Console.WriteLine("Errore #DB002: errore nel caricamento dei dati utente.");
                        Database.Drop();
                        goto GetValues;
                    }
                }
            }

            mUpdateEvent = new ManualResetEvent(false);
            mUser = new User(Name, Message, ProfilePicture, Convert.ToBoolean(PrivateMode), mUpdateEvent);

            // Initialize the research for friends by network
            mUser.ListenForFriends();
            mUser.SearchForFriends();
        }

        #endregion

        #region Public Methods
        /// <summary>
        /// Set the default path of the Folder for the Downloads
        /// </summary>
        public void SetDownloadDefaultFolder(string path)
        {
            DownloadDefaultFolder = path;
            Database.UpdateLocalDB(nameof(DownloadDefaultFolder), path);
            IoC.FtpServer.SetDownloadDefaultFolder(path);
        }

        public void SetAutomaticSave(string value)
        {
            AutomaticSave = value;
            Database.UpdateLocalDB(nameof(AutomaticSave), value);
        }

        public void SetPrivateMode(string value)
        {
            PrivateMode = value;
            Database.UpdateLocalDB(nameof(PrivateMode), value);
            IoC.FtpServer.SetPrivateMode(Convert.ToBoolean(value));
            mUser.SetPrivateMode(Convert.ToBoolean(value));
            if (value.Equals("True")) mUser.SayByeToFriends();
            else if (value.Equals("False")) mUser.SearchForFriends();
        }

        /// <summary>
        /// Set the name of the user
        /// </summary>
        public void SetName(string name)
        {
            Name = name;
            Database.UpdateLocalDB(nameof(Name), name);
            var ThreadDelegate = new ThreadStart(() => mUser.SetUsername(name));
            var Thread = new Thread(ThreadDelegate);
            Thread.Start();
        }

        /// <summary>
        /// Set the message of the user
        /// </summary>
        public void SetMessage(string message)
        {
            Message = message;
            Database.UpdateLocalDB(nameof(Message), message);
            var ThreadDelegate = new ThreadStart(() => mUser.SetMessage(message));
            var Thread = new Thread(ThreadDelegate);
            Thread.Start();
        }

        /// <summary>
        /// Set the profile picture path
        /// </summary>
        public void SetProfilePicture(string path)
        {
            ProfilePicture = path;
            Database.UpdateLocalDB(nameof(ProfilePicture), path);
            var ThreadDelegate = new ThreadStart(() => mUser.SetProfilePicturePath(path));
            var Thread = new Thread(ThreadDelegate);
            Thread.Start();
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

        /// <summary>
        /// Release the resources associated to the User
        /// </summary>
        public void Release()
        {
            mUser.Release();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ipAddress"></param>
        /// <returns></returns>
        public string GetUsernameByIpAddress(string ipAddress)
        {
            return mUser.GetUsernameByIpAddress(ipAddress);
        }

        public ManualResetEvent GetUpdateEvent()
        {
            return mUpdateEvent;
        }

        #endregion

        private string SetDefaultProfilePicture(string sourceFilepath)
        {
            // Create the default directory for profile pictures if it does not exists
            var currWorkingDirectory = Directory.GetCurrentDirectory();
            var defaultDir = @"ProfilePictures";
            var defaultDirPath = Path.Combine(currWorkingDirectory, defaultDir);
            var sourceFileFullPath = Path.Combine(currWorkingDirectory, sourceFilepath);
            if (!Directory.Exists(defaultDirPath))
            {
                try
                {
                    Directory.CreateDirectory(defaultDir);
                }
                catch (Exception excep)
                {
                    MessageBox.Show(excep.Message);
                }
            }

            // Set the tmp filename
            var extension = Path.GetExtension(sourceFileFullPath);
            var finalFilename = RandomString(12) + extension;
            var finalFilePath = Path.Combine(defaultDirPath, finalFilename);

            // Copy the file
            File.Copy(sourceFileFullPath, finalFilePath, true);

            return finalFilePath;

        }

        string RandomString(int length)
        {
            var random = new Random();
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }

    }

}
