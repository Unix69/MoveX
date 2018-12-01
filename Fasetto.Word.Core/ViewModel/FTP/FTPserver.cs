using System.Windows.Input;
using System.Net;
using Movex.FTP;
using System.Threading;
using System;
using System.Collections.Concurrent;

namespace Movex.View.Core
{
    public class FTPserver
    {

        #region private Properties
        private Movex.FTP.FTPserver mServer;
        private Thread mServerThread;
        #endregion

        #region Constructor
        /// <summary>
        /// Default Constructor
        /// </summary>
        public FTPserver()
        {
            var PrivateMode = Convert.ToBoolean(IoC.User.PrivateMode);
            var AutomaticReception = Convert.ToBoolean(IoC.User.AutomaticReception);
            var AutomaticSave = Convert.ToBoolean(IoC.User.AutomaticSave);
            var DonwloadDefaultFolder = IoC.User.DownloadDefaultFolder;

            mServer = new Movex.FTP.FTPserver(2000, PrivateMode, AutomaticReception, AutomaticSave, DonwloadDefaultFolder);
            mServerThread = new Thread(new ThreadStart(() => mServer.FTPstart()));
            mServerThread.Start();
        }
        #endregion

        #region Destructor

        public void Shutdown()
        {

            // mServerThread.Abort();
            // mServerThread.Join();
            // mServerThread.Interrupt();
            mServerThread = null;

        }

        #endregion

        #region Public Methods
        public void SetSynchronization(
            ManualResetEvent requestAvailable,
            ConcurrentQueue<string> requests,
            ConcurrentDictionary<string, int> typeRequests,
            ConcurrentDictionary<string, string> messages,
            ConcurrentDictionary<string, ManualResetEvent> sync,
            ConcurrentDictionary<string, ConcurrentBag<string>> responses)
        {
            mServer.SetSynchronization(requestAvailable, requests, typeRequests, messages, sync, responses);
        }
        public void SetPrivateMode(bool value) { mServer.SetPrivateMode(value); }
        public void SetAutomaticReception(bool value) { mServer.SetAutomaticReception(value); }
        public void SetAutomaticSave(bool value) { mServer.SetAutomaticSave(value); }
        public void SetDownloadDefaultFolder(string path) { mServer.SetDownloadDefaultFolder(path); }

        #endregion
    }
}