using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;
using System.Windows.Forms;
using System.Threading;
using System.IO;

namespace Movex.Network
{
    class TcpManager
    {
        private static bool DEBUG = true;

        #region Private members
        private IPAddress mIp;
        private TcpListener mListener;
        private Socket mClient;
        private static ManualResetEvent TcpClientConnected;
        private static ManualResetEvent SendDone;
        private static ManualResetEvent ConnectDone;
        private string mNetworkLogPath;
        private Mutex mNetworkLogPathMutex;
        private bool mIsExit;
        private static string DefaultPath = @"ProfilePictures";
        private static int DefaulPort;
        private const int DataSizeLimit = 1024 * 1024;              // Avoid to have buffer larger than 1 MB.
        private const int NumMaxConnections = 30;                   // Avoid Denial Of Service attacks or something.
        #endregion

        #region Constructor
        /// <summary>
        /// Default constructor
        /// </summary>
        public TcpManager() {

            // Set Default values
            var ni = new NetworkInformation();
            CollectLocalNetworkInformations(ni);

            // Set the initial value for this synchonization handles
            TcpClientConnected = new ManualResetEvent(false);
            ConnectDone = new ManualResetEvent(false);
            SendDone = new ManualResetEvent(false);

            try {

                // Set the Listener
                mListener = new TcpListener(mIp, 1512);

                // Set the Client
                ActivateClient();

            }
            catch (Exception e)
            {
                MessageBox.Show("Error launching the TCP Connection Manager at Movex.Network Module: " + e.ToString());
            }
            
        }
        #endregion

        #region Getters and Setters
        private void SetDefaultPath(string path)
        {
            DefaultPath = path;
        }
        private void SetPort(int port)
        {
            DefaulPort = port;
        }
        private void SetNetworkLogPathMutex(ref Mutex m)
        {
            mNetworkLogPathMutex = m;
        }
        private void SetNetworkLogPath(string s)
        {
            mNetworkLogPath = s;
        }
        private void SetIsExit(bool b)
        {
            mIsExit = b;
        }
        #endregion

        #region Routine methods

        /// <summary>
        /// Initialization method
        /// </summary>
        /// <param name="path"></param>
        /// <param name="port"></param>
        public void Init(int port, ref Mutex mNetworkLogPathMutex, string mNetworkLogPath)
        {
            SetPort(port);
            SetNetworkLogPathMutex(ref mNetworkLogPathMutex);
            SetNetworkLogPath(mNetworkLogPath);
        }

        /// <summary>
        /// Make the listener able to receive incoming connection requests.
        /// </summary>
        public void StartListener()
        {
            try
            {
                mListener.Start(NumMaxConnections);
            }
            catch(Exception e)
            {
                // Show ErrorView with error message.
                MessageBox.Show(e.ToString());
            }
        }

        /// <summary>
        /// Make the listener not able anymore to receive incoming connection requests.
        /// </summary>
        public void StopListener()
        {
            try
            {
                SetIsExit(true);
                TcpClientConnected.Set();
                mListener.Stop();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }
        }

        /// <summary>
        /// Close the client in a way such that all resourses are relased back.
        /// </summary>
        public void StopClient()
        {
            mClient.Dispose();
        }

        #endregion

        #region Core methods
        public static void BeginAcceptTcpClientCallback(IAsyncResult ar)
        {
            // Get the listener that handles the client request.
            var listener = (TcpListener)ar.AsyncState;

            try {

                // End the operation and get the client request.
                var client = listener.EndAcceptTcpClient(ar);

                // Get the stream
                var stream = client.GetStream();

                /*      THE FORMAT OF THE CONTENT IS:
                 * 
                 *  filenameSize    |   filename    |   dataSize   |    data
                 *     4 BYTE            N BYTE          8 BYTE        K BYTE
                 */
                
               // Read the filenameSize.
               var buffer = new byte[4];
               stream.Read(buffer, 0, 4);
               var filenameSize = BitConverter.ToInt32(buffer, 0);

               // Read the filename.
               buffer = new byte[filenameSize];
               stream.Read(buffer, 0, filenameSize);
               var filename = Encoding.UTF8.GetString(buffer);

               // Read the dataSize.
               buffer = new byte[8];
               stream.Read(buffer, 0, 8);
               var dataSize = BitConverter.ToInt32(buffer, 0);

               // Read the data
               if (dataSize < DataSizeLimit)
               {
                   buffer = new byte[dataSize];
                   stream.Read(buffer, 0, dataSize);

                   // Create the file
                   var filepath = Path.Combine(DefaultPath, filename);
                   var fileStream = File.Open(filepath, FileMode.Create);

                   // Write the file.
                   var binarybuffer = new BinaryWriter(fileStream);
                   binarybuffer.Write(buffer, 0, dataSize);
                   binarybuffer.Flush();
                   binarybuffer.Close();
               }
               else
               {
                   // Create the file.
                   var filepath = Path.Combine(DefaultPath, filename);
                   File.Create(filepath);

                   var toRead = dataSize;
                   var read = 0;

                   while (read < dataSize)
                   {
                       if (toRead > DataSizeLimit)
                           toRead = DataSizeLimit;

                       buffer = new byte[toRead];
                       read += stream.Read(buffer, 0, toRead);
                       toRead -= read;

                       // Write the piece of file
                       var fileStream = File.Open(filepath, FileMode.Append);
                       var binarybuffer = new BinaryWriter(fileStream);
                       binarybuffer.Write(buffer, 0, toRead);
                       binarybuffer.Flush();
                       binarybuffer.Close();
                   }
               }
            }
            catch (ObjectDisposedException obe)
            {
                return;
            }
            catch (Exception e)
            {
                // Show ErrorView with error message
                Console.WriteLine(e.Message);
            }

            // Signal the calling thread to continue.
            TcpClientConnected.Set();

        }
        public void ReceiveFile()
        {
            TcpClientConnected.Reset();
            mListener.BeginAcceptTcpClient(new AsyncCallback(BeginAcceptTcpClientCallback), mListener);
            TcpClientConnected.WaitOne();

            // Valid at application-shutdown
            if (mIsExit == true) { return; }

            // Record the operation to Network Log
            mNetworkLogPathMutex.WaitOne();
            using (var streamWriter = File.AppendText(mNetworkLogPath))
            {
                var currentTime = DateTime.Now.ToString("h: mm:ss tt");
                streamWriter.WriteLine("[" + currentTime + "]" + " " + "Received profile picture.");
                streamWriter.Close();
            }
            mNetworkLogPathMutex.ReleaseMutex();


        }
        private static void BeginConnectCallback(IAsyncResult ar)
        {
            ConnectDone.Set();
        }
        private static void BeginSendFileCallback(IAsyncResult ar)
        {
            var client = (Socket)ar.AsyncState;
            client.EndSendFile(ar);
            SendDone.Set();
        }
        public void SendFile(IPAddress to, string path)
        {
            try {

                // 1) Establish a connection to the remote host asynchronously.
                ConnectDone.Reset();
                try { mClient.BeginConnect(to, DefaulPort, BeginConnectCallback, mClient); }
                catch (Exception e){
                    try
                    {
                        ActivateClient();
                        mClient.BeginConnect(to, DefaulPort, BeginConnectCallback, mClient);
                    }
                    catch (Exception exc) { return; } }
                ConnectDone.WaitOne();

                // 2) Send the data file asyncronously.

                        /*      THE FORMAT OF THE CONTENT IS:
                        * 
                        *  filenameSize    |   filename    |   dataSize   |    data
                        *     4 BYTE            N BYTE          8 BYTE        K BYTE
                        */

                        // Get the filename, filenameSize, dataSize
                        var filename = Path.GetFileName(path);
                        var filenameSize = filename.Length;
                        var dataSize = new System.IO.FileInfo(path).Length;

                        // Set the filename bytes, filenameSize bytes, dataSize bytes
                        var filenameBytes = new byte[filenameSize];
                        filenameBytes = Encoding.UTF8.GetBytes(filename);
                        var filenameSizeBytes = new byte[4];
                        filenameSizeBytes = BitConverter.GetBytes(filenameSize);
                        var dataSizeBytes = new byte[4];
                        dataSizeBytes = BitConverter.GetBytes(dataSize);

                        // Record the step to the NetworkLog
                        mNetworkLogPathMutex.WaitOne();
                        using (var streamWriter = File.AppendText(mNetworkLogPath))
                        {
                            var currentTime = DateTime.Now.ToString("h: mm:ss tt");

                            streamWriter.WriteLine("[" + currentTime + "]" + " " + "ProfilePicture to send is:\r\n"
                            + "filenameSize: " + filenameSize + "(" + filenameSizeBytes.Length + ")\r\n"
                            + "filename: " + filename + "(" + filenameBytes.Length + ")\r\n"
                            + "dataSize: " + dataSize + "(" + dataSizeBytes.Length + ").");

                            streamWriter.Close();
                        }
                        mNetworkLogPathMutex.ReleaseMutex();

                        // Send the stream of bytes
                        var prebuf = new byte[4 + filenameBytes.Length + 8];
                        System.Buffer.BlockCopy(filenameSizeBytes, 0, prebuf, 0, 4);
                        System.Buffer.BlockCopy(filenameBytes, 0, prebuf, 4, filenameBytes.Length);
                        System.Buffer.BlockCopy(dataSizeBytes, 0, prebuf, 4+filenameBytes.Length, 8);
                        mClient.SendFile(path, prebuf, null, TransmitFileOptions.UseDefaultWorkerThread);

                // 3) Close the connection.
                mClient.Close();

            }
            catch (Exception exc)
            {
                   //TODO: remove "error sending the picture" and put the error message in a error message window
                if (DEBUG) MessageBox.Show("Error sending the picture: " + exc.Message.ToString());
            }
        }
        #endregion

        #region Utility methods
        /// <summary>
        /// Collect the informations of the network using an instance of the NetworkInformation object
        /// </summary>
        /// <param name="ni"></param>
        private void CollectLocalNetworkInformations(NetworkInformation ni)
        {
            var list = ni.GetLocalNetworkInformation();
            mIp = list[0];
        }
        private void ActivateClient() {
            mClient = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        }
        #endregion

    }
}
