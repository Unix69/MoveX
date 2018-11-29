using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Threading;
using System.Collections.Concurrent;
using System.Runtime.InteropServices;
using System.Security;
namespace Movex.FTP
{
    public class FTPserver
    {

        #region Private members
        // Member(s) useful for Network Functionality
        private Socket mServersocket;

        // Member(s) useful for History and Restore Functions
        private List<DownloadChannel> mDchans;
        private Mutex mDchansDataLock;

        // Member(s) useful for Application Settings
        private bool mPrivateMode;
        private bool mAutomaticSave;
        private bool mAutomaticReception;
        private string mDownloadDefaultFolder;
        private Mutex mVisible;

        // Member(s) useful for syncrhonization with Movex.View.Core
        private ManualResetEvent mRequestAvailable;
        private ConcurrentQueue<string> mRequests;
        private ConcurrentDictionary<string, int> mTypeRequests;
        private ConcurrentDictionary<string, string> mMessages;
        private ConcurrentDictionary<string, ManualResetEvent> mSync;
        private ConcurrentDictionary<string, ConcurrentBag<string>> mResponses;
        #endregion


        public void SetSynchronization2(ManualResetEvent requestAvailable, ConcurrentQueue<string> requests, ConcurrentDictionary<string, int> typeRequests, ConcurrentDictionary<string, string> messages, ConcurrentDictionary<string, ManualResetEvent> sync, ConcurrentDictionary<string, ConcurrentBag<string>> responses)
        {
            mRequestAvailable = requestAvailable;
            mRequests = requests;
            mTypeRequests = typeRequests;
            mMessages = messages;
            mSync = sync;
            mResponses = responses;
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


        public void MakeInvisible() {
            mVisible.WaitOne();
        }
        public void MakeVisibile() {
            mVisible.ReleaseMutex();
        }

        public FTPserver(int port, bool PrivateMode, bool AutomaticReception, bool AutomaticSave, string DownloadDefaultFolder)
        {
            try
            {

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

        public void FTPstart() {
            var ipEnd = new IPEndPoint(IPAddress.Any, FTPsupporter.ProtocolAttributes.Port);
            mServersocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            try { mServersocket.Bind(ipEnd); mServersocket.Listen(100); }
            catch (Exception e) { throw new IOException("Cannot Bind Server"); }

            Accept:

            var client = mServersocket.Accept();


            if (mPrivateMode == true) {
                mVisible.WaitOne();
                mVisible.ReleaseMutex();
                mPrivateMode = false;
            }

            /*
            string response;
            if (mAutomaticReception == false) { 
            // Set the message in a concurrent stack
            var message = "You received a request from " + client.RemoteEndPoint.ToString() + "\r\nDo you want to accept it?";
            var Id = "MyId";

            mRequests.Enqueue(Id);
            mTypeRequests.TryAdd(Id, 101);
            mMessages.TryAdd(Id, message);
            var responseAvailable = new ManualResetEvent(false);
            mSync.TryAdd(Id, responseAvailable);
            var responseBag = new ConcurrentBag<string>();
            mResponses.TryAdd(Id, responseBag);
            mRequestAvailable.Set();
            responseAvailable.WaitOne();
            mResponses.TryGetValue(Id, out var responseContainer);
            responseContainer.TryTake(out response);
            } else { response = "Yes"; }

            if (!response.Equals("Yes"))
            {
                client.Close();
                goto Accept;
            }

            string whereToSave;
            if (mAutomaticSave == false)
            {

                var message = "You received a request from " + client.RemoteEndPoint.ToString() + "\r\nDo you want to accept it?";
                var Id = "MyId2";
                mRequests.Enqueue(Id);
                mTypeRequests.TryAdd(Id, 102);
                mMessages.TryAdd(Id, message);
                var whereResponseAvailable = new ManualResetEvent(false);
                mSync.TryAdd(Id, whereResponseAvailable);
                var whereResponseBag = new ConcurrentBag<string>();
                mResponses.TryAdd(Id, whereResponseBag);
                mRequestAvailable.Set();
                whereResponseAvailable.WaitOne();
                mResponses.TryGetValue(Id, out var whereResponseContainer);
                whereResponseContainer.TryTake(out whereToSave);
            }
            else { whereToSave = mDownloadDefaultFolder; }


            var path = @".\Downloads\";
            if (whereToSave != null) path = Path.GetFullPath(whereToSave);
            path = path + @"\";
            */
            var path = @".\";
            var recvfrom = new Thread(new ThreadStart(() => FTPrecv(client, path)))
            {
                Priority = ThreadPriority.Highest,
                Name = "ServerSessionThread"
            };
            recvfrom.Start();

            goto Accept;
        }



        private int Receive(ref Socket client, ref byte[] bufferIn, DownloadChannel dchan, int downloadindex) {

                var received = 0;
                var length = (bufferIn == null ? 0 : bufferIn.Length);

            try
            {
                while (received < length)
                {
                    received += client.Receive(bufferIn, received, length - received, 0);
                }
            }
            catch (ArgumentNullException e) { Console.WriteLine(e.Message); throw e; }
            catch (ArgumentOutOfRangeException e) { Console.WriteLine(e.Message); throw e; }
            catch (SocketException e) { Console.WriteLine(e.Message); throw e; }
            catch (ObjectDisposedException e) { Console.WriteLine(e.Message); throw e; }
            catch (SecurityException e) { Console.WriteLine(e.Message); throw e; }
            catch (Exception e) { Console.WriteLine(e.Message); throw e; }




                if (received == 0 && dchan == null) { throw new IOException("Cannot receive any bytes"); }
                if (received == 0 && dchan != null) { dchan.InterruptDownload(); }
                if (dchan != null) { dchan.Incr_received_p(downloadindex, length); }

            return (received);
        }






        private bool FTPmultiFile_s(ref Socket clientsocket, int tag, string path, DTransfer transfer)
        {
            var numfile = 0;
            string filename = null;
            try
            {
                var dchan = new DownloadChannel(clientsocket.LocalEndPoint.ToString(), tag, path);

                mDchansDataLock.WaitOne();
                mDchans.Add(dchan);
                mDchansDataLock.ReleaseMutex();
                transfer.AttachToInterface(dchan);
                dchan.Set_socket(ref clientsocket);
                dchan.Set_main_download_thread(Thread.CurrentThread);

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
            catch (Exception e) { throw e; }
        }




        private bool FTPsingleFile(ref Socket clientsocket, int tag, string path, DTransfer transfer)
        {
            var dchan = new DownloadChannel(clientsocket.LocalEndPoint.ToString(), tag, path);

            mDchansDataLock.WaitOne();
            mDchans.Add(dchan);
            mDchansDataLock.ReleaseMutex();

            transfer.AttachToInterface(dchan);
            dchan.Set_socket(ref clientsocket);

            string filename = null;
            dchan.Set_num_trasf(1);
            dchan.Set_main_download_thread(Thread.CurrentThread);


            try
            {
                filename = RecvFilename(ref clientsocket, dchan, 0);
                if (!CheckFilename(filename)) { throw new IOException("Bad filename"); }
                RecvFiledata(ref clientsocket, dchan, 0, filename);
                transfer.DetachFromInterface(dchan);
                if (!RefreshCannels(dchan)) { throw new IOException("Channels Unrefreshable"); }
                return (true);
            }
            catch (Exception e) { throw e; }


        }


        public string getBytesSufix(ref double bytes)
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

        public string getConvertedNumber(long bytes)
        {
            double b = bytes;
            string sufix = getBytesSufix(ref b);
            float r = (float)b;
            return (r.ToString() + " " + sufix + "b");
        }


        private bool FTPtree(ref Socket clientsocket, string path, DTransfer transfer)
        {
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


                if (tag != FTPsupporter.Tags.Multifilesend && tag != FTPsupporter.Tags.Filesend) {
                    throw new IOException("Bad tag");
                }

                FTPfilesByTree(ref clientsocket, filepaths.ToArray(), path, tag, transfer);
                return (true);
            }
            catch (Exception e) { throw e; }



        }


        private bool FTPmultiTree(ref Socket clientsocket, string path, DTransfer transfer)
        {
            try
            {
                var tag = 0;
                var numtrees = RecvNumele(ref clientsocket);
                if (!CheckNumele(numtrees)) { throw new IOException("Bad number of elements"); }
                for (var i = 0; i < numtrees; i++) {
                    tag = RecvTag(ref clientsocket);
                    if (!CheckTag(tag)) { throw new IOException("Bad tag"); }
                    if (!FTPtree(ref clientsocket, path, transfer)) { throw new IOException("Cannot receive tree"); }
                }
                return (true);
            }
            catch (Exception e) { throw e; }

        }


        private bool FTPfilesByTree(ref Socket clientsocket, string[] paths, string path, int t, DTransfer transfer) {
            var numfile = 0;
            string filename = null;
            var tag = 0;
            try
            {

                var dchan = new DownloadChannel(clientsocket.LocalEndPoint.ToString(), t, path);

                mDchansDataLock.WaitOne();
                mDchans.Add(dchan);
                mDchansDataLock.ReleaseMutex();
                transfer.AttachToInterface(dchan);
                dchan.Set_socket(ref clientsocket);

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
            catch (Exception e) { throw e; }
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



        private DownloadChannel GetChannel(string filepath, string ipaddress)
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

        public string GetPathOfFather(int index, int[] depths, string[] elements, List<string> dirpaths, List<string> filepaths) {
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
            while (Directory.Exists(rootname)) {
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

        private long DiskCapability(string path) {
            if (path != null) {
                string exactPath = Path.GetFullPath(path);
                if (exactPath != null)
                {
                    DriveInfo dDrive = new DriveInfo(exactPath);
                    if (dDrive.IsReady)
                    {
                        return (dDrive.AvailableFreeSpace);
                    }
                }
            }
            return(0);
        }

        public bool CheckToS(int tos) {
            return (tos == FTPsupporter.ToSs.ToSfileandtree || tos == FTPsupporter.ToSs.ToStreeonly || tos == FTPsupporter.ToSs.ToSfileonly);
        }


         private void FTPrecv(Socket clientsocket, string path) {
            var result = false;
            var tag = 0;   
            var tos = 0;
            long tot = 0;
            long nfiles = 0;
          
            try
            {

                tos = RecvTag(ref clientsocket);
                if (!CheckToS(tos)) { throw new IOException("Bad ToS"); }

                tot = RecvTotalBytes(ref clientsocket);
                if (!CheckTotalBytes(tot)) { throw new IOException("Bad Total Bytes"); }

                nfiles = RecvTotNFiles(ref clientsocket);
                if (!CheckTotalNFiles(nfiles)) { throw new IOException("Bad Total Number of Files"); }


                Console.WriteLine("Recpt: Receive from Client dim=" + getConvertedNumber(tot) + " nfiles="+nfiles);


                if (tot > DiskCapability(path)) {
                    throw new IOException("No Disk anymore");
                }



                DTransfer transfer = new DTransfer(0, 0, tot);
                //todo: set of manualresetevent



                if (!SendAck(clientsocket, FTPsupporter.ProtocolAttributes.Ack))
                {
                    throw new IOException("Cannot send ack");
                }

                Gettag:

                tag = RecvTag(ref clientsocket);
                if (!CheckTag(tag)) { throw new IOException("Bad tag"); }


                switch (tag)
                {
                    case FTPsupporter.Tags.Filesend:
                        {

                            result = FTPsingleFile(ref clientsocket, tag, path, transfer);
                            break;

                        }
                    case FTPsupporter.Tags.Multifilesend:
                        {
                            result = FTPmultiFile_s(ref clientsocket, tag, path, transfer);
                            break;
                        }
                    case FTPsupporter.Tags.Treesend:
                        {

                            result = FTPtree(ref clientsocket, path, transfer);
                            break;

                        }
                    case FTPsupporter.Tags.Multitreesend:
                        {

                            result = FTPmultiTree(ref clientsocket, path, transfer);
                            break;

                        }
                }

                if (tos == FTPsupporter.ToSs.ToSfileandtree)
                {
                    tos = FTPsupporter.Unknown.UnknownInt;
                    goto Gettag;
                }


                if (!SendAck(clientsocket, FTPsupporter.ProtocolAttributes.Ack))
                {
                    throw new IOException("Cannot send ack");
                }

                clientsocket.Shutdown(SocketShutdown.Both);
                clientsocket.Dispose();
                clientsocket.Close();

                return;
            }
            catch (OutOfMemoryException e) { Console.WriteLine(e.Message); throw e; }
            catch (SocketException e) { Console.WriteLine(e.Message); throw e; }
            catch (ObjectDisposedException e) { Console.WriteLine(e.Message); throw e; }
            catch (ThreadInterruptedException e) { Console.WriteLine(e.Message); throw e; }
            catch (ThreadAbortException e) { Console.WriteLine(e.Message); throw e; }
            catch (Exception e) { Console.WriteLine(e.Message); throw e; }
        }

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



        private bool SendAck(Socket clientsocket, int ack) {
            var bufferOut = new byte[FTPsupporter.Sizes.Tagsize];
            bufferOut = BitConverter.GetBytes(ack);

            var sended = Send(clientsocket, ref bufferOut);
            if (sended != FTPsupporter.Sizes.Tagsize)
            {
                throw new IOException("Cannot send ack");
            }
            return (true);
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
                Console.WriteLine(i + " - filename " + filenames[i] + "  filesize " + getConvertedNumber(filesizes[i]) + "\n sended " + getConvertedNumber(received[i]) + " throughput " + getConvertedNumber((long)throughputs[i]) + "/s\n\n");
            }
            mDchansDataLock.WaitOne();
            var result = mDchans.Remove(olddchan);
            mDchansDataLock.ReleaseMutex();
            return (result);
        }

        private DownloadChannel GetChannel(string ipaddress)
        {
            foreach (var dchan in  mDchans)
            {
                if (dchan.Get_from().ToString().Equals(ipaddress))
                {
                    return (dchan);
                }

            }

            return (null);
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
            }
            catch (SecurityException e) { Console.WriteLine(e.Message); throw e; }
            catch (ThreadStateException e) { Console.WriteLine(e.Message); throw e; }
            return (true);
        }

        private bool CheckTotalBytes(long totalbytes)
        {
            return (totalbytes > 0 && totalbytes <= System.Int64.MaxValue);
        }

        private bool CheckTotalNFiles(long nfiles)
        {
            return (nfiles > 0 && nfiles <= System.Int64.MaxValue);
        }

        private long RecvTotalBytes(ref Socket clientsocket) {
            var bufferIn = new byte[FTPsupporter.Sizes.Filesizesize];
            var received = (long)Receive(ref clientsocket, ref bufferIn, null, 0);
            if (received != FTPsupporter.Sizes.Filesizesize) {
                throw new IOException("Bad total bytes");
            }
            var totalbytes = BitConverter.ToInt64(bufferIn, 0);
            return (totalbytes);
        }


        private int RecvTag(ref Socket clientsocket) {
            var bufferIn = new byte[FTPsupporter.Sizes.Tagsize];
            var received = Receive(ref clientsocket, ref bufferIn, null, 0);
            if (received == 0)
            {
                throw new IOException("Bad tag");
            }
            var tag = BitConverter.ToInt32(bufferIn, 0);
            return (tag);
        }

        private bool CheckTag(int tag) {
            return (
                tag == FTPsupporter.Tags.Filesend || 
                tag == FTPsupporter.Tags.Multifilesend || 
                tag == FTPsupporter.Tags.Treesend || 
                tag == FTPsupporter.Tags.Multitreesend
                );
        }


        private long RecvTotNFiles(ref Socket clientsocket) {
            var bufferIn = new byte[FTPsupporter.Sizes.Filesizesize];
            var received = (long)Receive(ref clientsocket, ref bufferIn, null, 0);
            if (received != FTPsupporter.Sizes.Filesizesize) { throw new IOException("Cannot Receive total number of files") ; }
            var nfiles = BitConverter.ToInt64(bufferIn, 0);
            return (nfiles);

        }

        private string RecvFilename(ref Socket clientsocket, DownloadChannel dchan, int n)
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

        private bool CheckFilename(string filename){
                return(!String.IsNullOrWhiteSpace(filename) && filename.Length > 0 && filename.Length <= Int32.MaxValue);
       }

       
 
        private void RecvFiledata(ref Socket clientsocket, DownloadChannel dchan, int n, string filename) {
            long toreceive = 0;
            int nrecv = 0;
            var bufferIn = new byte[FTPsupporter.Sizes.Filesizesize];
            var received = (long) Receive(ref clientsocket,ref bufferIn, null, 0);
            if(received != FTPsupporter.Sizes.Filesizesize) { return; }
            var filesize = BitConverter.ToInt64(bufferIn, 0);


            dchan.Add_new_download(filename, filesize);
            received = 0;

            var path = dchan.Get_filepaths()[n];
            var binarybuffer = new BinaryWriter(File.Open(path + filename, FileMode.Append));
            Console.WriteLine("Recpt: write file=" + filename + " size=" + getConvertedNumber(filesize));

            var throughputs = dchan.Get_throughputs();
            var remainingtimes = dchan.Get_remaining_times();

            dchan.StartDownload(n);


            while (received < filesize)
            {
                toreceive = filesize - ((long)received);
                var filedata_buff = new byte[(toreceive > FTPsupporter.Sizes.Filedatasize ? FTPsupporter.Sizes.Filedatasize : toreceive)];
                nrecv = Receive(ref clientsocket, ref filedata_buff, dchan, n);
                binarybuffer.Write(filedata_buff, 0 , nrecv);
                Console.WriteLine("Recpt: received " + getConvertedNumber(received) + " toreceive " + getConvertedNumber(toreceive) 
                    + " nrecv " + getConvertedNumber(nrecv) +
                        " remaining time =" + remainingtimes[n] + "s  throughput =" + getConvertedNumber((long)throughputs[n])+"/s");
                received += (long) nrecv;
            }

            CloseBufferW(binarybuffer);
          

            return;
        }

        private int RecvNumfile(ref Socket clientsocket)
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

        private bool CheckNumfile(int numfile, DownloadChannel dchan)
        {
            dchan.Set_num_trasf(numfile);
            return (numfile > 0 && numfile <= System.Int32.MaxValue);
        }
        
        private string RecvElement(ref Socket clientsocket)
        {
            var bufferIn = new byte[FTPsupporter.Sizes.Elementlensize];
            var received = Receive(ref clientsocket,ref bufferIn, null, 0);
            if (received == 0)
            {
                throw new IOException("Bad element length");
            }
            var elementlen = BitConverter.ToInt32(bufferIn, 0);
            bufferIn = new byte[elementlen];
            received = Receive(ref clientsocket,ref bufferIn, null, 0);
            if (received == 0)
            {
                throw new IOException("Bad element");
            }
            return (Encoding.ASCII.GetString(bufferIn, 0, elementlen));
          
        }

        private int RecvDepth(ref Socket clientsocket)
        {
            var bufferIn = new byte[FTPsupporter.Sizes.Elementdepthsize];
            var received = Receive(ref clientsocket,ref bufferIn, null, 0);
            if (received == 0)
            {
                throw new IOException("Bad depth");
            }
            return (BitConverter.ToInt32(bufferIn, 0));
        }

        private int RecvNumele(ref Socket clientsocket)
        {
            var bufferIn = new byte[FTPsupporter.Sizes.Numelementsize];
            var received = Receive(ref clientsocket,ref bufferIn, null, 0);
            if (received != FTPsupporter.Sizes.Numelementsize)
            {
                throw new IOException("Bad number of elements");
            }
            var numele = BitConverter.ToInt32(bufferIn, 0);
            return (numele);
        }


        private bool CheckNumele(int numele)
        {
            return (numele > 0 && numele <= System.Int32.MaxValue);
        }

        // GETTER(s) AND SETTER(s)
        public void SetPrivateMode(bool value) { mPrivateMode = Convert.ToBoolean(value); }
        public void SetAutomaticSave(bool value) { mAutomaticSave = Convert.ToBoolean(value); }
        public void SetAutomaticReception(bool value) { mAutomaticReception = Convert.ToBoolean(value); }
        public void SetDownloadDefaultFolder(string path) { mDownloadDefaultFolder = path; }

    }
}
