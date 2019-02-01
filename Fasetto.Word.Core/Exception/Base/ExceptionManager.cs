using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Movex.View.Core
{
    class ExceptionManager : IDisposable
    {
        const string CRASH_LOG_FOLDERNAME = "Log";

        #region Constuctor(s)
        public ExceptionManager() {}
        #endregion

        #region Destructor(s)
        public void Dispose() {}
        #endregion

        #region Core Method(s)
        /// <summary>
        /// Append Exception Information to the Crash Log File
        /// </summary>
        /// <param name="exception"></param>
        public void Log(Exception exception, string CRASH_LOG_FILENAME)
        {
            try
            {
                var CrashLogFolder = CreateFolder();
                var CrashLogFolderPath = CrashLogFolder.FullName;
                var CrashLogFilePath = Path.Combine(CrashLogFolderPath, CRASH_LOG_FILENAME);
                var CurrentTime = DateTime.Now.ToString("h:mm:ss tt");
                var Message = exception.Message;
                var Source = exception.Source;
                var StackTrace = exception.StackTrace;
                var NewLine = "\r\n";

                File.AppendAllText(CrashLogFilePath, "[" + CurrentTime + "] " + "Message: " + Message + "." + NewLine);
                File.AppendAllText(CrashLogFilePath, "[" + CurrentTime + "] " + "Source: " + Source + "." + NewLine);
                File.AppendAllText(CrashLogFilePath, StackTrace + "." + NewLine);
                File.AppendAllText(CrashLogFilePath, NewLine);
            }
            catch (Exception ie)
            {
                Console.WriteLine("[ExceptionManager.cs] [Log] " + exception.Message);
            }
        }
        #endregion

        #region Utility method(s)
        /// <summary>
        /// Create the Crash Log Folder unless arealdy exists
        /// </summary>
        /// <returns></returns>
        private DirectoryInfo CreateFolder()
        {
            try
            {
                var CurrentDirectory = Directory.GetCurrentDirectory();
                var DirectoryInfo = Directory.CreateDirectory(CRASH_LOG_FOLDERNAME);

                return DirectoryInfo;
            }
            catch (Exception exception)
            {
                Console.WriteLine("[ExceptionManager.cs] [CreateFolder] " + exception.Message);
                return null;
            }
        }
        #endregion
    }
}
