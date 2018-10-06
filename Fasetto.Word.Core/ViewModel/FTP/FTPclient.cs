using System.Windows.Input;
using System.Net;
using Movex.FTP;
using System.Threading;
using System.Collections.Generic;

namespace Movex.View.Core
{
    public class FTPclient
    {

        #region private Properties

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
        public void Send(string[] filepaths, IPAddress[] ipaddress, ManualResetEvent uchanAvailability, ManualResetEvent windowAvailability)
        {
            mClient.FTPsendAll(filepaths, ipaddress, 100, uchanAvailability, windowAvailability);
        }

        public List<UploadChannelInfo> GetUChanInfo()
        {
            return mClient.GetUCInfo();
        }

        #endregion
    }
}