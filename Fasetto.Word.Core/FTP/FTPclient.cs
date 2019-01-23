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