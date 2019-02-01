using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Movex.View.Core
{
    class ClientExceptionManager : ExceptionManager
    {
        const string CLIENT_CRASH_LOG_FILENAME = "ClientCrash.log";

        /// <summary>
        /// Log the exception in the specific file
        /// </summary>
        /// <param name="exception"></param>
        public void Log(Exception Exception)
        {
            var Message = Exception.Message;
            Console.WriteLine("[ClientExceptionManager] [Log] " + Message + " - See log for more details.");

            base.Log(Exception, CLIENT_CRASH_LOG_FILENAME);
        }

    }
}
