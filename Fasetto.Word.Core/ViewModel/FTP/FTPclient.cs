using System.Windows.Input;
using System.Net;
using Movex.FTP;
using System.Threading;
using System.Collections.Generic;
using System;

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
            mClient = new Movex.FTP.FTPclient();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Send a file using the FTP Client
        /// </summary>
        public void Send(string[] filepaths, IPAddress[] ipaddress, ManualResetEvent[] WindowsAvailabilities, ManualResetEvent[] TransfersAvailabilities)
        {
            // TODO: valutare la possibilità di sganciare un thread
            mClient.FTPsendAll(filepaths, ipaddress, WindowsAvailabilities, TransfersAvailabilities);
        }



        public UploadChannel GetChannel(IPAddress address) {
            return(mClient.GetChannel(address.ToString()));
        }

        public UTransfer GetTransfer(IPAddress mAddress)
        {
            return (mClient.GetTransfer(mAddress.ToString()));
        }

        #endregion
    }
}