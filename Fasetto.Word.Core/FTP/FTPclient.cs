using System;
using System.Net;
using System.Threading;
using System.Collections.Concurrent;
using Movex.FTP;

namespace Movex.View.Core
{
    public class FTPclient : IDisposable
    {
        #region Members
        FTP.FTPclient mClient;
        ClientExceptionManager mClientExceptionManager;
        ManualResetEvent mRequestAvailable;
        ConcurrentQueue<string> mRequests;
        ConcurrentDictionary<string, int> mTypeRequests;
        ConcurrentDictionary<string, string> mMessages;
        ConcurrentDictionary<string, ManualResetEvent[]> mSync;
        ConcurrentDictionary<string, ConcurrentBag<string>> mResponses;
        #endregion

        #region Constructor
        public FTPclient()
        {
            try
            {
                mClient = new FTP.FTPclient();
                mClientExceptionManager = new ClientExceptionManager();
            }
            catch (Exception Exception)
            {
                mClientExceptionManager.Log(Exception);
            }
        }
        #endregion

        #region Destructor(s)
        public void Dispose()
        {
            mClientExceptionManager.Dispose();
            mRequestAvailable.Dispose();
            while (!mRequests.IsEmpty)
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

        #region Lifecycle method(s)
        public void Reset()
        {
            mClient.Shutdown();
            mClient = new FTP.FTPclient();
            mClient.SetSynchronization(mRequestAvailable, mRequests, mTypeRequests, mMessages, mSync, mResponses);
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Set Synchronization primitives
        /// </summary>
        /// <param name="requestAvailable"></param>
        /// <param name="requests"></param>
        /// <param name="typeRequests"></param>
        /// <param name="messages"></param>
        /// <param name="sync"></param>
        /// <param name="responses"></param>
        public void SetSynchronization(ManualResetEvent requestAvailable, ConcurrentQueue<string> requests, ConcurrentDictionary<string, int> typeRequests,
            ConcurrentDictionary<string, string> messages, ConcurrentDictionary<string, ManualResetEvent[]> sync, ConcurrentDictionary<string, ConcurrentBag<string>> responses)
        {
            // Copy them in the instance of the Movex.View.Core.FTPClient
            mRequestAvailable = requestAvailable;
            mRequests = requests;
            mTypeRequests = typeRequests;
            mMessages = messages;
            mSync = sync;
            mResponses = responses;

            // Send them to the Movex.FTP.FTPClient
            mClient.SetSynchronization(requestAvailable, requests, typeRequests, messages, sync, responses);
        }

        /// <summary>
        /// Send a file using the FTP Client
        /// </summary>
        public void Send(string[] filepaths, IPAddress[] ipaddress, ManualResetEvent[] WindowsAvailabilities, ManualResetEvent[] TransfersAvailabilities)
        {
            try
            {
                mClient.FTPsendAll(filepaths, ipaddress, WindowsAvailabilities, TransfersAvailabilities);
            }
            catch (Exception Exception)
            {
                Console.WriteLine("[MOVEX.VIEW.CORE] [FTPclient.cs] [Send] Error occurred. See log for more datails.");
                mClientExceptionManager.Log(Exception);
            }
        }

        /// <summary>
        /// Access the UploadTransfer associated to the IpAddress
        /// </summary>
        /// <param name="mAddress"></param>
        /// <returns></returns>
        public UTransfer GetTransfer(IPAddress mAddress)
        {
            return (mClient.GetTransfer(mAddress.ToString()));
        }

        /// <summary>
        /// Interrupt the upload
        /// </summary>
        /// <param name="address"></param>
        public void InterruptUpload(IPAddress address)
        {
            Console.WriteLine("[Movex.View.Core] [FTPclient.cs] [InterruptUpload] Interruputing Upload.");
            mClient.InterruptUpload(address);
        }
        #endregion
    }
}