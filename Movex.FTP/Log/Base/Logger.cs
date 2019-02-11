using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Movex.FTP
{
    class Logger
    {
        const string LOG_FOLDERNAME = "Log";

        #region Constuctor(s)
        public Logger(string LOG_FILENAME)
        {
            var LogFilePath = Path.Combine(LOG_FOLDERNAME, LOG_FILENAME);
            if (File.Exists(LogFilePath))
            {
                // Empty the content of the file (if exists)
                File.WriteAllText(LogFilePath, string.Empty);
            }
            else
            {
                // Create the file (if not exists)
                File.Create(LogFilePath);
            }
        }
        #endregion

        #region Destructor(s)
        public void Dispose() {}
        #endregion

        #region Core Method(s)
        /// <summary>
        /// Append Exception Information to the Crash Log File
        /// </summary>
        /// <param name="exception"></param>
        public void Log(string Statement, string LOG_FILENAME)
        {
            try
            {
                var CrashLogFolder = CreateFolder();
                var CrashLogFolderPath = CrashLogFolder.FullName;
                var LogFilePath = Path.Combine(CrashLogFolderPath, LOG_FILENAME);
                var CurrentTime = DateTime.Now.ToString("h:mm:ss tt");
                var NewLine = "\r\n";

                File.AppendAllText(LogFilePath, "[" + CurrentTime + "] " + Statement + NewLine);
            }
            catch (Exception ie)
            {
                Console.WriteLine("[MOVEX.FTP] [Logger.cs] [Log] " + ie.Message);
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
                var DirectoryInfo = Directory.CreateDirectory(LOG_FOLDERNAME);

                return DirectoryInfo;
            }
            catch (Exception exception)
            {
                Console.WriteLine("[MOVEX.FTP] [Logger.cs] [CreateFolder] " + exception.Message);
                return null;
            }
        }
        #endregion
    }
}
