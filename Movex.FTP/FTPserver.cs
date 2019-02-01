using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Threading;
using System.Collections.Concurrent;
using System.Security;

namespace Movex.FTP
{
    public class FTPserver
    {
        #region Private members

        // Member(s) useful for Network Functionality
        private Socket mServersocket;

        // Member(s) useful for syncrhonization with Movex.View.Core
        private WindowRequester mWindowRequester;
        
        // Member(s) useful for History and Restore Functions
        private List<DownloadChannel> mDchans;
        private Mutex mDchansDataLock;
        private ConcurrentDictionary<string, DTransfer> mTransfer;

        // Member(s) useful for Application Settings
        private bool mPrivateMode;
        private bool mAutomaticSave;
        private bool mAutomaticReception;
        private string mDownloadDefaultFolder;
        private Mutex mVisible;

        #endregion

        #region Constructor
        public FTPserver(int port, bool PrivateMode, bool AutomaticReception, bool AutomaticSave, string DownloadDefaultFolder)
        {
            try
            {
                mTransfer = new ConcurrentDictionary<string, DTransfer>();
                mDchans = new List<DownloadChannel>();
                mVisible = new Mutex();
                mDchansDataLock = new Mutex();
                mPrivateMode = PrivateMode;
                mAutomaticReception = AutomaticReception;
                mAutomaticSave = AutomaticSave;
                mDownloadDefaultFolder = DownloadDefaultFolder;
                return;
            }
            catch (Exception e) { throw e; }
        }
        #endregion

        #region Destructor
        public void FTPStop()
            {
                mServersocket.Dispose();
                mTransfer.Clear();
                mDchans.Clear();
                mVisible.Dispose();
                mDchansDataLock.Dispose();
                mWindowRequester.Dispose();

                mServersocket = null;
                mTransfer = null;
                mDchans = null;
                mVisible = null;
                mDchansDataLock = null;
                mWindowRequester = null;
            }
        #endregion

        #region Getter(s) and Setter(s)
        public void SetPrivateMode(bool value) { mPrivateMode = Convert.ToBoolean(value); }
        public void SetAutomaticSave(bool value) { mAutomaticSave = Convert.ToBoolean(value); }
        public void SetAutomaticReception(bool value) { mAutomaticReception = Convert.ToBoolean(value); }
        public void SetDownloadDefaultFolder(string path) { mDownloadDefaultFolder = path; }
        public DTransfer GetTransfer(string ipAddress)
        {
            // Check input syntax
            if (string.IsNullOrEmpty(ipAddress) || string.IsNullOrWhiteSpace(ipAddress))
                return null;

            // Check input semantic (valid for IPv4 only)
            if (!(ipAddress.Split('.').Length == 4))
                return null;

            // Get the DTransfer bound to the ipAddress
            mTransfer.TryGetValue(ipAddress, out var dTransfer);

            // Return the Download Transfer
            if (dTransfer is DTransfer) { return dTransfer; }
            else return null;
        }
        public DownloadChannel GetChannel(string filepath, string ipaddress)
        {
            mDchansDataLock.WaitOne();
            foreach (var dchan in mDchans)
            {
                if (dchan.Get_path().Equals(filepath) && dchan.Get_from().ToString().Equals(ipaddress))
                {
                    mDchansDataLock.ReleaseMutex();
                    return (dchan);
                }

            }
            mDchansDataLock.ReleaseMutex();
            return (null);
        }
        public DownloadChannel GetChannel(string ipaddress)
        {
            mDchansDataLock.WaitOne();
            foreach (var dchan in mDchans)
            {
                if (dchan.Get_from().ToString().Equals(ipaddress))
                {
                    mDchansDataLock.ReleaseMutex();
                    return (dchan);
                }

            }
            mDchansDataLock.ReleaseMutex();
            return (null);
        }
        public void MakeInvisible()
        {
            mVisible.WaitOne();
        }
        public void MakeVisibile()
        {
            mVisible.ReleaseMutex();
        }
        public void SetSynchronization(
            ManualResetEvent requestAvailable,
            ConcurrentQueue<string> requests,
            ConcurrentDictionary<string, int> typeRequests,
            ConcurrentDictionary<string, string> messages,
            ConcurrentDictionary<string, ManualResetEvent[]> sync,
            ConcurrentDictionary<string, ConcurrentBag<string>> responses)
        {
            mWindowRequester = new WindowRequester(requestAvailable, requests, typeRequests, messages, sync, responses);
        }
        #endregion

        #region Highest-level method
        public void FTPstart()
        {
            var ipEnd = new IPEndPoint(IPAddress.Any, FTPsupporter.ProtocolAttributes.Port);
            mServersocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            try { mServersocket.Bind(ipEnd); mServersocket.Listen(100); }
            catch (Exception e) { throw new IOException("Cannot Bind Server"); }

            Accept:

            Console.WriteLine("[Movex.FTP] [FTPserver.cs] [FTPstart] Accepting new requests.");
            var client = mServersocket.Accept();
            Console.WriteLine("[Movex.FTP] [FTPserver.cs] [FTPstart] New request to serve.");

            if (mPrivateMode == true)
            {
                mVisible.WaitOne();
                mVisible.ReleaseMutex();
                mPrivateMode = false;
            }

            // FIRST STEP: Ask the user if he wants to accept the request.
            string response;
            if (mAutomaticReception == false)
            {
                var message = "You received a request from " + client.RemoteEndPoint.ToString() + "\r\nDo you want to accept it?";
                response = mWindowRequester.AddYesNoWindow(message);
            }
            else
            {
                response = "Yes";
            }
            if (!response.Equals("Yes"))
            {
                client.Close();
                goto Accept;
            }

            // SECOND STEP: Ask the user the download folder.
            string whereToSave;
            if (mAutomaticSave == false)
            {
                var message = "You received a request from " + client.RemoteEndPoint.ToString() + "\r\nDo you want to accept it?";
                whereToSave = mWindowRequester.AddWhereWindow(message);
                whereToSave = NormalizePath(whereToSave);
            }
            else
            {
                whereToSave = NormalizePath(mDownloadDefaultFolder);
            }

            // THIRD STEP: Show the DownloadProgressBar
            var ipAddress = IPAddress.Parse(client.RemoteEndPoint.ToString().Split(':')[0]);
            var downloadTransferAvailability = new ManualResetEvent(false);
            var windowAvailability = new ManualResetEvent(false);
            var recvfrom = new Thread(new ThreadStart(() => {

                try
                {
                    FTPrecv(client, whereToSave, downloadTransferAvailability, windowAvailability);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    throw e;
                }

            }))
            {
                Priority = ThreadPriority.Highest,
                Name = "ServerSessionThread"
            };
            recvfrom.Start();

            Console.WriteLine("[Movex.FTP] [FTPserver.cs] [FTPstart] Passing DownloadTransferAvailability as: " + windowAvailability.Handle.ToInt64().ToString());
            Console.WriteLine("[Movex.FTP] [FTPserver.cs] [FTPstart] Passing WindowAvailability as: " + downloadTransferAvailability.Handle.ToInt64().ToString());
            mWindowRequester.AddDownloadProgressWindow(ipAddress, windowAvailability, downloadTransferAvailability);

            goto Accept;
        }
        #endregion

        #region High-level method(s)
        private void FTPrecv(Socket clientsocket, string path, ManualResetEvent downloadTransferAvailability, ManualResetEvent windowAvailability)
        {
            // Initializing variable(s)
            var result = false;
            var tag = 0;
            var tos = 0;
            long tot = 0;
            long nfiles = 0;

            try
            {
                Console.WriteLine("[Movex.FTP] [FTPserver.cs] [FTPrecv] Trying to receive data.");

                tos = RecvTag(ref clientsocket);
                if (!CheckToS(tos)) { throw new IOException("Bad ToS"); }

                tot = RecvTotalBytes(ref clientsocket);
                if (!CheckTotalBytes(tot)) { throw new IOException("Bad Total Bytes"); }

                nfiles = RecvTotNFiles(ref clientsocket);
                if (!CheckTotalNFiles(nfiles)) { throw new IOException("Bad Total Number of Files"); }

                Console.WriteLine("Recpt: Receive from Client dim=" + GetConvertedNumber(tot) + " nfiles=" + nfiles);

                if (tot > DiskCapability(path))
                {
                    throw new IOException("No Disk anymore");
                }

                // Put the Download Transfer to the dictionary mTransfer of the active transfers
                // The dictionary<key, value) has:
                // - the ipAddress of the sender, as key
                // - the dTransfers, as value
                var ipAddress = clientsocket.RemoteEndPoint.ToString().Split(':')[0];
                var transfer = new DTransfer(0, 0, tot);
                mTransfer.TryAdd(ipAddress, transfer);

                if (!SendAck(clientsocket, FTPsupporter.ProtocolAttributes.Ack)) { throw new IOException("Cannot send ack"); }

                Gettag:

                tag = RecvTag(ref clientsocket);
                if (!CheckTag(tag)) { throw new IOException("Bad tag"); }

                transfer.StartTransfer();
                switch (tag)
                {
                    case FTPsupporter.Tags.Filesend:
                        {
                            result = FTPsingleFile(ref clientsocket, tag, path, transfer, downloadTransferAvailability, windowAvailability);
                            break;

                        }
                    case FTPsupporter.Tags.Multifilesend:
                        {
                            result = FTPmultiFile_s(ref clientsocket, tag, path, transfer, downloadTransferAvailability, windowAvailability);
                            break;
                        }
                    case FTPsupporter.Tags.Treesend:
                        {
                            result = FTPtree(ref clientsocket, path, transfer, downloadTransferAvailability, windowAvailability);
                            break;

                        }
                    case FTPsupporter.Tags.Multitreesend:
                        {
                            result = FTPmultiTree(ref clientsocket, path, transfer, downloadTransferAvailability, windowAvailability);
                            break;

                        }
                }

                if (tos == FTPsupporter.ToSs.ToSfileandtree)
                {
                    tos = FTPsupporter.Unknown.UnknownInt;
                    goto Gettag;
                }

                if (!SendAck(clientsocket, FTPsupporter.ProtocolAttributes.Ack)) { throw new IOException("Cannot send ack"); }

                // Remove DTrasfer from the DTransfer list and release it
                Console.WriteLine("[Movex.FTP] [FTPserver.cs] [FTPrecv] Releasing DTransfer.");
                mTransfer.TryRemove(ipAddress, out var oldTransfer);
                oldTransfer = null;

                // Release the socket
                Console.WriteLine("[Movex.FTP] [FTPserver.cs] [FTPrecv] Releasing Socket.");
                clientsocket.Shutdown(SocketShutdown.Both);
                clientsocket.Dispose();
                clientsocket.Close();

                var CurrentThreadName = Thread.CurrentThread.Name;
                Console.WriteLine("[Movex.FTP] [FTPserver.cs] [FTPrecv] Releasing Thread: " + CurrentThreadName + ".");
                return;
            }
            catch (OutOfMemoryException e) { Console.WriteLine(e.Message); throw e; }
            catch (SocketException e)
            {

                var message = "Il trasferimento è stato interrotto.";
                var ipAddress = clientsocket.RemoteEndPoint.ToString().Split(':')[0];

                Console.WriteLine(e.Message + " / " + message);
                mWindowRequester.RemoveDownloadProgressWindow(ipAddress);
                mWindowRequester.AddMessageWindow(message);
            }
            catch (ObjectDisposedException e) { Console.WriteLine(e.Message); throw e; }
            catch (ThreadInterruptedException e) { Console.WriteLine(e.Message); throw e; }
            catch (ThreadAbortException e) { Console.WriteLine(e.Message); throw e; }
            catch (Exception e) { Console.WriteLine(e.Message); throw e; }
        }
        #endregion

        #region Mid-level method(s)
        private bool FTPmultiFile_s(ref Socket clientsocket, int tag, string path, DTransfer transfer, ManualResetEvent downloadTransferAvailability, ManualResetEvent windowAvailability)
        {
            Console.WriteLine("[Movex.FTP] [FTPserver.cs] [FTPmultiFile_s] Serving the request.");
            var numfile = 0;
            string filename = null;
            try
            {
                var dchan = new DownloadChannel(clientsocket.RemoteEndPoint.ToString().Split(':')[0], tag, path);

                mDchansDataLock.WaitOne();
                mDchans.Add(dchan);
                mDchansDataLock.ReleaseMutex();
                transfer.AttachToInterface(dchan);
                dchan.Set_socket(ref clientsocket);
                dchan.Set_main_download_thread(Thread.CurrentThread);

                downloadTransferAvailability.Set();
                windowAvailability.WaitOne();

                numfile = RecvNumfile(ref clientsocket);
                if (!CheckNumfile(numfile, dchan)) { throw new IOException("Number of files not correct"); }
                for (var i = 0; i < numfile; i++)
                {
                    tag = RecvTag(ref clientsocket);
                    if (!CheckTag(tag)) { throw new IOException("Bad tag"); }
                    filename = RecvFilename(ref clientsocket, dchan, i);
                    if (!CheckFilename(filename)) { throw new IOException("Bad filename"); }
                    RecvFiledata(ref clientsocket, dchan, i, filename);
                }
                transfer.DetachFromInterface(dchan);
                if (!RefreshCannels(dchan)) { throw new IOException("Channels Unrefreshable"); }
                return (true);
            }
            catch (SocketException e) { Console.WriteLine(e.Message); throw e; }
            catch (Exception e) { Console.WriteLine(e.Message); throw e; }

        }
        private bool FTPsingleFile(ref Socket clientsocket, int tag, string path, DTransfer transfer, ManualResetEvent downloadTransferAvailability, ManualResetEvent windowAvailability)
        {
            Console.WriteLine("[Movex.FTP] [FTPserver.cs] [FTPsingleFile] Serving the request.");
            var ip = clientsocket.RemoteEndPoint.ToString().Split(':')[0];
            var dchan = new DownloadChannel(ip, tag, path);
            string filename = null;

            mDchansDataLock.WaitOne();
            mDchans.Add(dchan);
            mDchansDataLock.ReleaseMutex();

            dchan.Set_socket(ref clientsocket);
            dchan.Set_num_trasf(1);
            dchan.Set_main_download_thread(Thread.CurrentThread);

            transfer.AttachToInterface(dchan);
            downloadTransferAvailability.Set();
            windowAvailability.WaitOne();

            try
            {
                filename = RecvFilename(ref clientsocket, dchan, 0);
                if (!CheckFilename(filename)) { throw new IOException("Bad filename"); }
                RecvFiledata(ref clientsocket, dchan, 0, filename);
                transfer.DetachFromInterface(dchan);
                if (!RefreshCannels(dchan)) { throw new IOException("Channels Unrefreshable"); }
                return (true);
            }
            catch (SocketException e) { Console.WriteLine(e.Message); throw e; }
            catch (Exception e) { Console.WriteLine(e.Message); throw e; }


        }
        private bool FTPtree(ref Socket clientsocket, string path, DTransfer transfer, ManualResetEvent downloadTransferAvailability, ManualResetEvent windowAvailability)
        {
            Console.WriteLine("[Movex.FTP] [FTPserver.cs] [FTPtree] Serving the request.");
            var numele = 0;
            var dirpaths = new List<string>();
            var filepaths = new List<string>();
            string[] elements = null;
            int[] depths = null;
            var tag = 0;
            try
            {


                numele = RecvNumele(ref clientsocket);
                if (!CheckNumele(numele)) { { throw new IOException("Bad number of elements"); } }

                elements = new string[numele];
                depths = new int[numele];

                for (var i = 0; i < numele; i++)
                {
                    elements[i] = RecvElement(ref clientsocket);
                    depths[i] = RecvDepth(ref clientsocket);
                }

                CreateDirectoryTree(dirpaths, depths, filepaths, elements, ".");

                tag = RecvTag(ref clientsocket);
                if (!CheckTag(tag)) { throw new IOException("Bad tag"); }


                if (tag != FTPsupporter.Tags.Multifilesend && tag != FTPsupporter.Tags.Filesend)
                {
                    throw new IOException("Bad tag");
                }

                FTPfilesByTree(ref clientsocket, filepaths.ToArray(), path, tag, transfer, downloadTransferAvailability, windowAvailability);
                return (true);
            }
            catch (SocketException e) { Console.WriteLine(e.Message); throw e; }
            catch (Exception e) { Console.WriteLine(e.Message); throw e; }




        }
        private bool FTPmultiTree(ref Socket clientsocket, string path, DTransfer transfer, ManualResetEvent downloadTransferAvailability, ManualResetEvent windowAvailability)
        {
            Console.WriteLine("[Movex.FTP] [FTPserver.cs] [FTPmultiTree] Serving the request.");
            try
            {
                var tag = 0;
                var numtrees = RecvNumele(ref clientsocket);
                if (!CheckNumele(numtrees)) { throw new IOException("Bad number of elements"); }
                for (var i = 0; i < numtrees; i++)
                {
                    tag = RecvTag(ref clientsocket);
                    if (!CheckTag(tag)) { throw new IOException("Bad tag"); }
                    if (!FTPtree(ref clientsocket, path, transfer, downloadTransferAvailability, windowAvailability)) { throw new IOException("Cannot receive tree"); }
                }
                return (true);
            }
            catch (SocketException e) { Console.WriteLine(e.Message); throw e; }
            catch (Exception e) { Console.WriteLine(e.Message); throw e; }


        }
        #endregion

        #region Low-level method(s)
        private long RecvTotalBytes(ref Socket clientsocket)
        {
            try
            {
                var bufferIn = new byte[FTPsupporter.Sizes.Filesizesize];
                var received = (long)Receive(ref clientsocket, ref bufferIn, null, 0);
                if (received != FTPsupporter.Sizes.Filesizesize)
                {
                    throw new IOException("Bad total bytes");
                }
                var totalbytes = BitConverter.ToInt64(bufferIn, 0);
                return (totalbytes);
            }
            catch (SocketException e) { Console.WriteLine(e.Message); throw e; }
            catch (Exception e) { Console.WriteLine(e.Message); throw e; }

        }
        private int RecvTag(ref Socket clientsocket)
        {
            try
            {
                var bufferIn = new byte[FTPsupporter.Sizes.Tagsize];
                var received = Receive(ref clientsocket, ref bufferIn, null, 0);
                if (received == 0)
                {
                    throw new IOException("Bad tag");
                }
                var tag = BitConverter.ToInt32(bufferIn, 0);
                return (tag);
            }
            catch (SocketException e) { Console.WriteLine(e.Message); throw e; }
            catch (Exception e) { Console.WriteLine(e.Message); throw e; }
        }
        private long RecvTotNFiles(ref Socket clientsocket)
        {
            try
            {
                var bufferIn = new byte[FTPsupporter.Sizes.Filesizesize];
                var received = (long)Receive(ref clientsocket, ref bufferIn, null, 0);
                if (received != FTPsupporter.Sizes.Filesizesize) { throw new IOException("Cannot Receive total number of files"); }
                var nfiles = BitConverter.ToInt64(bufferIn, 0);
                return (nfiles);
            }
            catch (SocketException e) { Console.WriteLine(e.Message); throw e; }
            catch (Exception e) { Console.WriteLine(e.Message); throw e; }

        }
        private string RecvFilename(ref Socket clientsocket, DownloadChannel dchan, int n)
        {
            try
            {
                var bufferIn = new byte[FTPsupporter.Sizes.Filenamelensize];
                var received = Receive(ref clientsocket, ref bufferIn, null, n);
                var filenamelen = BitConverter.ToInt32(bufferIn, 0);
                bufferIn = new byte[filenamelen];
                received += Receive(ref clientsocket, ref bufferIn, null, n);
                if (received == 0)
                {
                    throw new IOException("Bad Filename");
                }
                var filename = Encoding.ASCII.GetString(bufferIn, 0, filenamelen);
                filename = AdjustFilename(filename, dchan.Get_path());
                return (filename);
            }
            catch (SocketException e) { Console.WriteLine(e.Message); throw e; }
            catch (Exception e) { Console.WriteLine(e.Message); throw e; }
        }
        private void RecvFiledata(ref Socket clientsocket, DownloadChannel dchan, int n, string filename)
        {


            long toreceive = 0;
            int nrecv = 0;
            var bufferIn = new byte[FTPsupporter.Sizes.Filesizesize];
            var received = (long)Receive(ref clientsocket, ref bufferIn, null, 0);
            if (received != FTPsupporter.Sizes.Filesizesize) { return; }
            var filesize = BitConverter.ToInt64(bufferIn, 0);


            dchan.Add_new_download(filename, filesize);
            received = 0;

            var path = dchan.Get_filepaths()[n];
            var binarybuffer = new BinaryWriter(File.Open(path + filename, FileMode.Append));
            Console.WriteLine("Recpt: write file=" + filename + " size=" + GetConvertedNumber(filesize));

            var throughputs = dchan.Get_throughputs();
            var remainingtimes = dchan.Get_remaining_times();

            dchan.StartDownload(n);


            while (received < filesize)
            {
                try
                {
                    toreceive = filesize - ((long)received);
                    var filedata_buff = new byte[(toreceive > FTPsupporter.Sizes.Filedatasize ? FTPsupporter.Sizes.Filedatasize : toreceive)];
                    nrecv = Receive(ref clientsocket, ref filedata_buff, dchan, n);
                    binarybuffer.Write(filedata_buff, 0, nrecv);
                    /*
                    Console.WriteLine("Recpt: received " + GetConvertedNumber(received) + " toreceive " + GetConvertedNumber(toreceive)
                        + " nrecv " + GetConvertedNumber(nrecv) +
                        " remaining time =" + remainingtimes[n] + "s  throughput =" + GetConvertedNumber((long)throughputs[n]) + "/s");
                    */
                    received += (long)nrecv;
                }
                catch (SocketException e) { Console.WriteLine(e.Message); throw e; }
                catch (Exception e) { Console.WriteLine(e.Message); throw e; }
            }

            CloseBufferW(binarybuffer);
            return;
        }
        private int RecvNumfile(ref Socket clientsocket)
        {
            try
            {
                var bufferIn = new byte[FTPsupporter.Sizes.Numfilesize];
                var received = Receive(ref clientsocket, ref bufferIn, null, 0);
                if (received != FTPsupporter.Sizes.Numfilesize)
                {
                    throw new IOException("Bad numfile");
                }
                var numfile = BitConverter.ToInt32(bufferIn, 0);
                return (numfile);
            }
            catch (SocketException e) { Console.WriteLine(e.Message); throw e; }
            catch (Exception e) { Console.WriteLine(e.Message); throw e; }
        }
        private string RecvElement(ref Socket clientsocket)
        {
            try
            {
                var bufferIn = new byte[FTPsupporter.Sizes.Elementlensize];
                var received = Receive(ref clientsocket, ref bufferIn, null, 0);
                if (received == 0)
                {
                    throw new IOException("Bad element length");
                }
                var elementlen = BitConverter.ToInt32(bufferIn, 0);
                bufferIn = new byte[elementlen];
                received = Receive(ref clientsocket, ref bufferIn, null, 0);
                if (received == 0)
                {
                    throw new IOException("Bad element");
                }
                return (Encoding.ASCII.GetString(bufferIn, 0, elementlen));
            }
            catch (SocketException e) { Console.WriteLine(e.Message); throw e; }
            catch (Exception e) { Console.WriteLine(e.Message); throw e; }
        }
        private int RecvDepth(ref Socket clientsocket)
        {
            try
            {
                var bufferIn = new byte[FTPsupporter.Sizes.Elementdepthsize];
                var received = Receive(ref clientsocket, ref bufferIn, null, 0);
                if (received == 0)
                {
                    throw new IOException("Bad depth");
                }
                return (BitConverter.ToInt32(bufferIn, 0));
            }
            catch (SocketException e) { Console.WriteLine(e.Message); throw e; }
            catch (Exception e) { Console.WriteLine(e.Message); throw e; }
        }
        private int RecvNumele(ref Socket clientsocket)
        {
            try
            {
                var bufferIn = new byte[FTPsupporter.Sizes.Numelementsize];
                var received = Receive(ref clientsocket, ref bufferIn, null, 0);
                if (received != FTPsupporter.Sizes.Numelementsize)
                {
                    throw new IOException("Bad number of elements");
                }
                var numele = BitConverter.ToInt32(bufferIn, 0);
                return (numele);
            }
            catch (SocketException e) { Console.WriteLine(e.Message); throw e; }
            catch (Exception e) { Console.WriteLine(e.Message); throw e; }
        }
        private bool SendAck(Socket clientsocket, int ack)
        {
            try
            {
                var bufferOut = new byte[FTPsupporter.Sizes.Tagsize];
                bufferOut = BitConverter.GetBytes(ack);

                var sended = Send(clientsocket, ref bufferOut);
                if (sended != FTPsupporter.Sizes.Tagsize)
                {
                    throw new IOException("Cannot send ack");
                }
                return (true);
            }
            catch (SocketException e) { Console.WriteLine(e.Message); throw e; }
            catch (Exception e) { Console.WriteLine(e.Message); throw e; }

        }
        #endregion

        #region Lowest-level method(s)
        private int Send(Socket clientsocket, ref byte[] bufferOut)
        {
            var length = 0;
            var sended = 0;
            try
            {
                length = FTPsupporter.Sizes.Tagsize;
                while (sended < length)
                {
                    sended += clientsocket.Send(bufferOut, sended, length - sended, 0);
                }
                return (sended);
            }
            catch (ArgumentNullException e) { Console.WriteLine(e.Message); throw e; }
            catch (ArgumentException e) { Console.WriteLine(e.Message); throw e; }
            catch (SocketException e) { Console.WriteLine(e.Message); throw e; }
            catch (ObjectDisposedException e) { Console.WriteLine(e.Message); throw e; }
            catch (Exception e) { Console.WriteLine(e.Message); throw e; }

        }
        private int Receive(ref Socket client, ref byte[] bufferIn, DownloadChannel dchan, int downloadindex)
        {

            var received = 0;
            var length = (bufferIn == null ? 0 : bufferIn.Length);

            CheckSocketConnection(ref client);
            while (received < length)
            {
                try
                {
                    received += client.Receive(bufferIn, received, length - received, 0);
                }
                catch (ArgumentNullException e) { Console.WriteLine(e.Message); throw e; }
                catch (ArgumentOutOfRangeException e) { Console.WriteLine(e.Message); throw e; }
                catch (SocketException e) { Console.WriteLine(e.Message); throw e; }
                catch (ObjectDisposedException e) { Console.WriteLine(e.Message); throw e; }
                catch (SecurityException e) { Console.WriteLine(e.Message); throw e; }
                catch (Exception e) { Console.WriteLine(e.Message); throw e; }
            }

            if (received == 0 && dchan == null) { throw new IOException("Cannot receive any bytes"); }
            if (received == 0 && dchan != null)
            {
                dchan.InterruptDownload();
            }
            if (dchan != null) { dchan.Incr_received_p(downloadindex, length); }

            return (received);
        }
        #endregion

        #region Service method(s)
        private bool FTPfilesByTree(ref Socket clientsocket, string[] paths, string path, int t, DTransfer transfer, ManualResetEvent downloadTransferAvailability, ManualResetEvent windowAvailability)
        {
            var numfile = 0;
            string filename = null;
            var tag = 0;
            try
            {

                var dchan = new DownloadChannel(clientsocket.RemoteEndPoint.ToString().Split(':')[0], t, path);

                mDchansDataLock.WaitOne();
                mDchans.Add(dchan);
                mDchansDataLock.ReleaseMutex();
                transfer.AttachToInterface(dchan);
                dchan.Set_socket(ref clientsocket);

                downloadTransferAvailability.Set();
                windowAvailability.WaitOne();

                if (t == FTPsupporter.Tags.Filesend)
                {
                    dchan.Set_num_trasf(1);
                    dchan.Set_filepaths(paths);
                    filename = RecvFilename(ref clientsocket, dchan, 0);
                    if (!CheckFilename(filename)) { throw new IOException("Bad filename"); }
                    RecvFiledata(ref clientsocket, dchan, 0, filename);
                }
                else
                {

                    numfile = RecvNumfile(ref clientsocket);
                    if (!CheckNumfile(numfile, dchan)) { throw new IOException("Bad number of files"); }
                    dchan.Set_filepaths(paths);
                    for (var i = 0; i < numfile; i++)
                    {
                        tag = RecvTag(ref clientsocket);
                        if (!CheckTag(tag)) { throw new IOException("Bad tag"); }
                        filename = RecvFilename(ref clientsocket, dchan, 0);
                        if (!CheckFilename(filename)) { throw new IOException("Bad filename"); }
                        RecvFiledata(ref clientsocket, dchan, 0, filename);
                    }
                }
                transfer.DetachFromInterface(dchan);
                if (!RefreshCannels(dchan)) { throw new IOException("Channels Unrefeshable"); }
                return (true);
            }
            catch (SocketException e) { Console.WriteLine(e.Message); throw e; }
            catch (Exception e) { Console.WriteLine(e.Message); throw e; }

        }
        private bool RefreshCannels(DownloadChannel olddchan)
        {
            Console.WriteLine("\n\nDownload channel end details :\n\n");
            if (olddchan == null) { return (false); }
            var filenames = olddchan.Get_filenames();
            var filesizes = olddchan.Get_filesizes();
            var received = olddchan.Get_received();
            var throughputs = olddchan.Get_throughputs();
            for (var i = 0; i < olddchan.Get_num_trasf(); i++)
            {
                Console.WriteLine(i + " - filename " + filenames[i] + "  filesize " + GetConvertedNumber(filesizes[i]) + "\n sended " + GetConvertedNumber(received[i]) + " throughput " + GetConvertedNumber((long)throughputs[i]) + "/s\n\n");
            }
            mDchansDataLock.WaitOne();
            var result = mDchans.Remove(olddchan);
            Console.WriteLine("[Movex.FTP] [FTPserver.cs] [RefreshCannel] DChan list has now: " + mDchans.Count + "channels.");
            mDchansDataLock.ReleaseMutex();
            return (result);
        }
        public bool InterruptDownload(IPAddress ipaddress)
        {
            try
            {
                var dchan = GetChannel(ipaddress.ToString());
                if (dchan == null)
                {
                    return (false);
                }
                dchan.InterruptDownload();
                return (true);
            }
            catch (SocketException e) { Console.WriteLine(e.Message); throw e; }
            catch (Exception e) { Console.WriteLine(e.Message); throw e; }
        }
        #endregion

        #region Validation method(s)
        public bool CheckToS(int tos)
        {
            return (tos == FTPsupporter.ToSs.ToSfileandtree || tos == FTPsupporter.ToSs.ToStreeonly || tos == FTPsupporter.ToSs.ToSfileonly);
        }
        private bool CheckFilename(string filename)
        {
            return (!string.IsNullOrWhiteSpace(filename) && filename.Length > 0 && filename.Length <= int.MaxValue);
        }
        private bool CheckTotalBytes(long totalbytes)
        {
            return (totalbytes > 0 && totalbytes <= int.MaxValue);
        }
        private bool CheckTotalNFiles(long nfiles)
        {
            return (nfiles > 0 && nfiles <= System.Int64.MaxValue);
        }
        private bool CheckTag(int tag)
        {
            return (
                tag == FTPsupporter.Tags.Filesend ||
                tag == FTPsupporter.Tags.Multifilesend ||
                tag == FTPsupporter.Tags.Treesend ||
                tag == FTPsupporter.Tags.Multitreesend
                );
        }
        private bool CheckNumfile(int numfile, DownloadChannel dchan)
        {
            dchan.Set_num_trasf(numfile);
            return (numfile > 0 && numfile <= int.MaxValue);
        }
        private bool CheckNumele(int numele)
        {
            return (numele > 0 && numele <= int.MaxValue);
        }
        #endregion

        #region Utility function(s)
        private string NormalizePath(string path)
        {
            var normalized = path;

            normalized = normalized.Replace("\r", "");
            normalized = normalized.Replace("\n", "");
            normalized = normalized + @"\";

            return normalized;
        }
        private string AdjustFilename(string filename, string path)
        {
            var n = 0;
            var filenametemp = Path.GetFileNameWithoutExtension(filename);
            var estension = Path.GetExtension(filename);
            while (File.Exists(path + filenametemp + estension))
            {
                filenametemp = Path.GetFileNameWithoutExtension(path + filename);
                filenametemp += "(" + n.ToString() + ")";
                n++;
            }
            return (filenametemp + estension);

        }
        private void CheckSocketConnection(ref Socket clientsocket)
        {
            try
            {
                var cond1 = clientsocket.Poll(1000, SelectMode.SelectRead);
                var cond2 = (clientsocket.Available == 0);
                if (cond1 && cond2)
                {
                    throw new SocketException();
                }
            }
            catch (NotSupportedException e) { Console.WriteLine(e.Message); throw e; }
            catch (SocketException e) { Console.WriteLine(e.Message); throw e; }
            catch (ObjectDisposedException e) { Console.WriteLine(e.Message); throw e; }
        }
        private bool IsDir(string path)
        {
            if (path.IndexOf(@".") == -1)
            {
                return (true);
            }
            return (false);
        }
        private void CreatePath(string element, string root, List<string> filepaths, List<string> dirpaths)
        {
            if (IsDir(element))
            {
                Directory.CreateDirectory(root + @"\" + element);
                dirpaths.Add(root + @"\" + element);

            }
            else
            {
                filepaths.Add(root + @"\");
            }
            return;

        }
        public string GetBytesSufix(ref double bytes)
        {
            string[] sufixes = { "", "K", "M", "G", "T", "P" };
            var s = 0;
            while (bytes > 1024)
            {
                bytes /= 1024;
                s++;
            }
            return (sufixes[s]);
        }
        public string GetConvertedNumber(long bytes)
        {
            double b = bytes;
            string sufix = GetBytesSufix(ref b);
            float r = (float)b;
            return (r.ToString() + " " + sufix + "b");
        }
        private void CloseBufferW(BinaryWriter binarybuffer)
        {
            binarybuffer.Flush();
            binarybuffer.Close();
            binarybuffer.Dispose();
            return;
        }
        private void CloseBufferR(BinaryReader binarybuffer)
        {
            binarybuffer.Close();
            binarybuffer.Dispose();
            return;
        }
        private long DiskCapability(string path)
        {
            if (path != null)
            {
                var exactPath = Path.GetFullPath(path);
                if (exactPath != null)
                {
                    var dDrive = new DriveInfo(exactPath);
                    if (dDrive.IsReady)
                    {
                        return (dDrive.AvailableFreeSpace);
                    }
                }
            }
            return (0);
        }
        public string GetPathOfFather(int index, int[] depths, string[] elements, List<string> dirpaths, List<string> filepaths)
        {
            string father = null;
            for (var i = index - 1; i >= 0; i--)
            {

                if (depths[i] < depths[index] && IsDir(elements[i]))
                {
                    father = elements[i];
                    father = father.Replace(@"\", "");
                    break;
                }
            }

            foreach (var path in dirpaths)
            {
                var tmp = path.Split(@"\".ToCharArray());

                var last = tmp[tmp.Length - 1];
                if (last == father)
                {

                    return (path);

                }

            }
            return (null);
        }
        public void CreateDirectoryTree(List<string> dirpaths, int[] depths, List<string> filepaths, string[] elements, string curroot)
        {
            var numelements = elements.Length;
            var rootname = curroot + @"\" + elements[0];
            var n = 0;
            var newelement = elements[0];
            while (Directory.Exists(rootname))
            {
                newelement = elements[0] + "(" + n + ")";
                rootname = curroot + @"\" + elements[0] + "(" + n + ")";
                n++;
            }
            elements[0] = newelement;
            CreatePath(elements[0], curroot, filepaths, dirpaths);
            curroot = curroot + @"\" + elements[0];

            for (var index = 1; index < numelements; index++)
            {
                if (depths[index] == depths[index - 1])
                {
                    var fatherpath = GetPathOfFather(index, depths, elements, dirpaths, filepaths);
                    CreatePath(elements[index], fatherpath, filepaths, dirpaths);
                    if (IsDir(elements[index]))
                    {
                        curroot = fatherpath + @"\" + elements[index];
                    }
                }

                else if (depths[index] > depths[index - 1])
                {
                    CreatePath(elements[index], curroot, filepaths, dirpaths);
                    if (IsDir(elements[index]))
                    {
                        curroot = curroot + @"\" + elements[index];
                    }
                }
                else
                {
                    var fatherpath = GetPathOfFather(index, depths, elements, dirpaths, filepaths);
                    CreatePath(elements[index], fatherpath, filepaths, dirpaths);
                    if (IsDir(elements[index]))
                    {
                        curroot = fatherpath + @"\" + elements[index];
                    }
                }
            }
        }
        #endregion
    }
}
