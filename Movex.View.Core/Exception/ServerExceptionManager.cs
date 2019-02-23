using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Movex.View.Core
{
    class ServerExceptionManager : ExceptionManager
    {
        const string SERVER_CRASH_LOG_FILENAME = "ServerCrash.log";

        #region Constructor(s)
        public ServerExceptionManager() {}
        #endregion

        /// <summary>
        /// Log the exception in the specific file
        /// </summary>
        /// <param name="exception"></param>
        public void Log(Exception Exception)
        {
            var Message = Exception.Message;
            Console.WriteLine("[ServerExceptionManager] [Log] " + Message + " - See log for more details.");

            base.Log(Exception, SERVER_CRASH_LOG_FILENAME);
        }
        
    }
}
