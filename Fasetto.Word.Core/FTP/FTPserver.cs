using System;
using System.Net;
using System.IO;
using System.Threading;
using System.Collections.Concurrent;
using Movex.FTP;



namespace Movex.View.Core
{
    public class FTPserver : IDisposable
    {
        private const string CrashLogFilename = "crashLog.txt";

        #region private Properties
        private FTP.FTPserver mServer;
        private Thread mServerThread;
        private ServerExceptionManager mServerExceptionManager;
        ManualResetEvent mRequestAvailable;
        ConcurrentQueue<string> mRequests;
        ConcurrentDictionary<string, int> mTypeRequests;
        ConcurrentDictionary<string, string> mMessages;
        ConcurrentDictionary<string, ManualResetEvent[]> mSync;
        ConcurrentDictionary<string, ConcurrentBag<string>> mResponses;
        #endregion

        #region Constructor(s)
        public FTPserver()
        {
            try
            {
                Init();
            }
            catch(Exception Exception)
            {
                mServerExceptionManager.Log(Exception);
            }
            
        }
        #endregion

        #region Destructor(s)
        public void Dispose()
        {
            mServerExceptionManager.Dispose();
            mRequestAvailable.Dispose();
            while(!mRequests.IsEmpty)
            {
                mRequests.TryDequeue(out var content);
                content = null;
            }
            mTypeRequests.Clear();
            mMessages.Clear();
            mSync.Clear();
            mResponses.Clear();
        }
        #endregion

        #region LifeCycle Method(s)
        private void Init()
        {
            var PrivateMode = Convert.ToBoolean(IoC.User.PrivateMode);
            var AutomaticReception = Convert.ToBoolean(IoC.User.AutomaticReception);
            var AutomaticSave = Convert.ToBoolean(IoC.User.AutomaticSave);
            var DonwloadDefaultFolder = IoC.User.DownloadDefaultFolder;

            mServer = new FTP.FTPserver(2000, PrivateMode, AutomaticReception, AutomaticSave, DonwloadDefaultFolder);
            mServerExceptionManager = new ServerExceptionManager();
            mServerThread = new Thread(new ThreadStart(() => {

                try
                {
                    mServer.FTPstart();
                }
                catch (Exception Exception)
                {
                    mServerExceptionManager.Log(Exception);
                }

            }));
            mServerThread.Start();
        }
        public void Shutdown()
        {
            mServer.FTPStop();
            mServerThread.Abort();
            mServerThread = null;
        }
        public void Reset()
        {
            Shutdown();
            Init();
            SetSynchronization(mRequestAvailable, mRequests, mTypeRequests, mMessages, mSync, mResponses);
        }
        #endregion

        #region Public Methods
        public void SetSynchronization(
            ManualResetEvent requestAvailable,
            ConcurrentQueue<string> requests,
            ConcurrentDictionary<string, int> typeRequests,
            ConcurrentDictionary<string, string> messages,
            ConcurrentDictionary<string, ManualResetEvent[]> sync,
            ConcurrentDictionary<string, ConcurrentBag<string>> responses)
        {
            // Copy them in the instance of the Movex.View.Core.FTPClient
            mRequestAvailable = requestAvailable;
            mRequests = requests;
            mTypeRequests = typeRequests;
            mMessages = messages;
            mSync = sync;
            mResponses = responses;

            // Send them to the Movex.FTP.FTPClient
            mServer.SetSynchronization(requestAvailable, requests, typeRequests, messages, sync, responses);
        }
        public void SetPrivateMode(bool value) { mServer.SetPrivateMode(value); }
        public void SetAutomaticReception(bool value) { mServer.SetAutomaticReception(value); }
        public void SetAutomaticSave(bool value) { mServer.SetAutomaticSave(value); }
        public void SetDownloadDefaultFolder(string path) { mServer.SetDownloadDefaultFolder(path); }
        public DTransfer GetTransfer(string ipAddress)
        {
            return mServer.GetTransfer(ipAddress);
        }
        public DownloadChannel GetChannel(IPAddress address)
        {
            return mServer.GetChannel(address.ToString());
        }
        public void InterruptDownload(IPAddress ipAdress)
        {
            mServer.InterruptDownload(ipAdress);
        }
        #endregion
    }
}