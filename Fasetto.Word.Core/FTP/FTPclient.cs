using System.Windows.Input;
using System.Net;
using Movex.FTP;
using System.Threading;
using System.Collections.Generic;
using System;
using System.Collections.Concurrent;

namespace Movex.View.Core
{
    public class FTPclient
    {

        #region Private Properties
        Movex.FTP.FTPclient mClient;
        // Useful for sync with Movex.FTP
        ManualResetEvent mRequestAvailable;
        ConcurrentQueue<string> mRequests;
        ConcurrentDictionary<string, int> mTypeRequests;
        ConcurrentDictionary<string, string> mMessages;
        ConcurrentDictionary<string, ManualResetEvent[]> mSync;
        ConcurrentDictionary<string, ConcurrentBag<string>> mResponses;
        #endregion

        #region Constructor

        /// <summary>
        /// Default Constructor
        /// </summary>
        public FTPclient()
        {
            try
            {
                mClient = new Movex.FTP.FTPclient();
            } catch(Exception e)
            {
                Reset();
            }

            
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
            mClient.SetSynchronization(requestAvailable, requests, typeRequests, messages, sync, responses);
        }
        /// <summary>
        /// Send a file using the FTP Client
        /// </summary>
        public void Send(string[] filepaths, IPAddress[] ipaddress, ManualResetEvent[] WindowsAvailabilities, ManualResetEvent[] TransfersAvailabilities)
        {
            mClient.FTPsendAll(filepaths, ipaddress, WindowsAvailabilities, TransfersAvailabilities);
        }
        public void Reset()
        {
            mClient = null;
            mClient = new Movex.FTP.FTPclient();
            mClient.SetSynchronization(mRequestAvailable, mRequests, mTypeRequests, mMessages, mSync, mResponses);
        }
        public UploadChannel GetChannel(IPAddress address) {
            return(mClient.GetChannel(address.ToString()));
        }
        public UTransfer GetTransfer(IPAddress mAddress)
        {
            return (mClient.GetTransfer(mAddress.ToString()));
        }
        public void InterruptUpload(IPAddress address)
        {
            mClient.InterruptUpload(address);
        }
        #endregion
    }
}