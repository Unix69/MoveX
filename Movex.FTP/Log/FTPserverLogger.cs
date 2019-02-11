using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Movex.FTP
{
    class FTPserverLogger : Logger
    {
        const string FTP_SERVER_LOG_FILENAME = "FTPserver.log";

        #region Constructor
        public FTPserverLogger() : base(FTP_SERVER_LOG_FILENAME) { }
        #endregion

        /// <summary>
        /// Log the exception in the specific file
        /// </summary>
        /// <param name="exception"></param>
        public void Log(string Statement)
        {
            Console.WriteLine(Statement);
            base.Log(Statement, FTP_SERVER_LOG_FILENAME);
        }

    }
}
