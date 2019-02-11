using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Movex.FTP
{
    class FTPclientLogger : Logger
    {
        const string FTP_CLIENT_LOG_FILENAME = "FTPclient.log";

        #region Constructor
        public FTPclientLogger() : base(FTP_CLIENT_LOG_FILENAME) {}
        #endregion

        /// <summary>
        /// Log the exception in the specific file
        /// </summary>
        /// <param name="exception"></param>
        public void Log(string Statement)
        {
            Console.WriteLine(Statement);
            base.Log(Statement, FTP_CLIENT_LOG_FILENAME);
        }

    }
}
