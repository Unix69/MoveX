using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Threading;
using System.Collections.Concurrent;

namespace Movex.FTP
{
    public class FTPserver
    {

        #region Private members
        // Member(s) useful for Network Functionality
        private Socket mServersocket;

        // Member(s) useful for History and Restore Functions
        private List<DownloadChannel> mDchans = new List<DownloadChannel>();
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
       
        public bool FTPclose(){
            return(true);
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
                mVisible = new Mutex();
                mDchansDataLock = new Mutex();
                mPrivateMode = PrivateMode;
                mAutomaticReception = AutomaticReception;
                mAutomaticSave = AutomaticSave;
                mDownloadDefaultFolder = DownloadDefaultFolder;
                return;
            }
            catch (Exception e) { return; }
        }
        
        //MAIN FUNCTION THREADABLE

        // 1 - start the main thread of the server
        public void FTPstart() { 
            var ipEnd = new IPEndPoint(IPAddress.Any, FTPsupporter.Port);
            mServersocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            
            try{ mServersocket.Bind(ipEnd); mServersocket.Listen(100); }
            catch(Exception e){return;}
            
            Accept:
    
            Console.WriteLine("Server is on accepting\n");
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
            var message = "You received a request from " + client.LocalEndPoint.ToString() + "\r\nDo you want to accept it?";
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

                var message = "You received a request from " + client.LocalEndPoint.ToString() + "\r\nDo you want to accept it?";
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
                Priority = ThreadPriority.Highest
            };
            recvfrom.Start();
      
            goto Accept;
        }


        //WRAPPED :

        // 1 - FTPfilewrite_ wrapped / threadable support FTPmultirecv_t
        private void FTPfilewrite(byte[] bufferIn, DownloadChannel dchan, int id)
        {

            try
            {
                var result = FTPfilewrite_(bufferIn, dchan, id);

                return;
            }
            catch (Exception e) { return; }
            
        }

        // 2 - FTPfilewritepath_ wrapped / threadable support FTPrecvfilesbytree_t
        private void FTPfilewritepath(string path, byte[] bufferIn, DownloadChannel dchan, int id)
        {

            try
            {
                var result = FTPfilewritepath_(path, bufferIn, dchan, id);

                return;
            }
            catch (Exception e) { return; }

        }


        //SUPPORT FTPmultirecv_t :

        // 1 - file recv support FTPmultirecv_t
        private bool FTPfilewrite_(byte[] bufferIn, DownloadChannel dchan, int index)
        {
            BinaryWriter binarybuffer = null;
            try
            {
                var filename = dchan.Get_filenames()[index];
                var path = dchan.Get_filepaths()[index];
                var filesize = dchan.Get_filesizes()[index];
                binarybuffer = new BinaryWriter(File.Open(path + filename, FileMode.Append));
                binarybuffer.Write(bufferIn, 0, filesize);
                CloseBufferW(binarybuffer);
                dchan.GetDownloadChannelInfo(filename).Switch_download();
                return (true);
            }
            catch (Exception e) {
                 CloseBufferW(binarybuffer);
                return (false);
            }

        }

        //SUPPORT FTPrecvfilesbyTree_t

        // 2 - recv the single file and save it in the rigth tree path
        private bool FTPfilewritepath_(string path, byte[] bufferIn, DownloadChannel dchan, int index)
        {
            BinaryWriter binarybuffer = null;
            try
            {
                var filename = dchan.Get_filenames()[index];
                var filesize = dchan.Get_filesizes()[index];
                binarybuffer = new BinaryWriter(File.Open(path + filename, FileMode.Append));
                binarybuffer.Write(bufferIn, 0, filesize);
                CloseBufferW(binarybuffer);
                dchan.GetDownloadChannelInfo(filename).Switch_download();
                return(true);
            }
            catch (Exception e) {
                CloseBufferW(binarybuffer);
                return (false);
            }



        }

        private int Receive(Socket client, byte [] bufferIn, DownloadChannel dchan, int downloadindex) {
            try
            {
                var received = 0;
                var length = 0;
                if (dchan == null)
                {
                    length = bufferIn.Length;
                    received = client.Receive(bufferIn, 0, length, 0);
                    return (received);
                }

                if (!client.Connected || dchan.IsInterrupted())
                {
                    dchan.InterruptDownload();
                    return (0);
                }

                length = bufferIn.Length;
                received = client.Receive(bufferIn, 0, length, 0);
                if (received == 0)
                {
                    dchan.InterruptDownload();
                }
                else
                {
                    dchan.Incr_received_p(downloadindex, length);
                }
                return (received);
            }
            catch (Exception e) { return (0); }
            }



        //MAIN RECV METHODS :

        // 1 - multi thread file recv
        private bool FTPmultiFile_t(DownloadChannel dchan)
        {

            var numfile = 0;
            string filename = null;
            byte[][] bufferFile;
            Thread[] nrecvfrom;
            var tag = 0;

            try
            {
                var clientsocket = dchan.Get_socket();
                numfile = RecvNumfile(clientsocket);
                if (!CheckNumfile(numfile, dchan)) { return (false); }
                dchan.Set_main_download_thread(Thread.CurrentThread);
                dchan.Set_ps(FTPsupporter.Parallel);
                AttachDownloadInfosToChannel(ref dchan);


                bufferFile = new byte[numfile][];
                nrecvfrom = new Thread[numfile];

                for (var i = 0; i < numfile; i++)
                {

                    tag = RecvTag(clientsocket);
                    if (!CheckTag(tag)) {return (false);}
                    filename = RecvFilename(clientsocket, dchan, i);
                    if (!CheckFilename(filename)) { return (false); }
                    bufferFile[i] = RecvFiledata(clientsocket, dchan, i, filename);
                    if (!CheckFileData(bufferFile[i], dchan, i)) {return (false);}
                     


                    {
                        var index = i;
                        nrecvfrom[i] = new Thread(new ThreadStart(() => FTPfilewrite(bufferFile[index], dchan, index)))
                        {
                            Priority = ThreadPriority.Highest
                        };
                        nrecvfrom[i].Start();
                        dchan.Set_download_thread(nrecvfrom[i], i);
                    }
                }

                var numthreads = nrecvfrom.Length;
               
                for (var i = 0; i < numthreads; i++)
                {
                    nrecvfrom[i].Join();
                }

                return (true);
            }
            catch (Exception e) { return (false); }



        }

        // 2 - serial multi file recieve
        private bool FTPmultiFile_s(DownloadChannel dchan)
        {
            var numfile = 0;
            byte[] bufferFile = null;
            string filename = null;
            var tag = 0;

            var clientsocket = dchan.Get_socket();
            dchan.Set_ps(FTPsupporter.Serial);
            dchan.Set_main_download_thread(Thread.CurrentThread);


            try
            {

                numfile = RecvNumfile(clientsocket);
                if (!CheckNumfile(numfile, dchan)){return (false);}
                AttachDownloadInfosToChannel(ref dchan);
                for (var i = 0; i < numfile; i++)
                {
                    tag = RecvTag(clientsocket);
                    if (!CheckTag(tag)){return (false);}
                    filename = RecvFilename(clientsocket, dchan, i);
                    if (!CheckFilename(filename)) { return (false); }
                    bufferFile = RecvFiledata(clientsocket, dchan, i, filename);
                    if (!CheckFileData(bufferFile, dchan, i)) { return (false);}
                    FTPfilewrite(bufferFile, dchan, i);
                }
                return (true);
            }
            catch (Exception e) { return (false); }
        }


        public void AttachDownloadInfosToChannel(ref DownloadChannel dchan) {
            var n = dchan.Get_n_infos();
            var numfile = dchan.Get_num_trasf();
            var ps = dchan.Get_ps();
            for (var i = 0; i < n; i++)
            {
                var dchaninfo = new DownloadChannelInfo(ref dchan);
                dchaninfo.Set_current_index(i);
                if (ps == FTPsupporter.Serial)
                {
                    dchaninfo.Set_end_index(numfile);
                }
                else
                {
                    dchaninfo.Set_end_index(i + 1);
                }

                dchan.Set_dchaninfo(ref dchaninfo);
            }
        }

        // 3 - single file recv
        private bool FTPsingleFile(DownloadChannel dchan)
        {
            string filename = null;
            byte[] bufferFile = null;
            var clientsocket = dchan.Get_socket();
            dchan.Set_num_trasf(1);
            dchan.Set_main_download_thread(Thread.CurrentThread);
            dchan.Set_ps(FTPsupporter.Serial);
            AttachDownloadInfosToChannel(ref dchan);
            var path = dchan.Get_filepaths()[0];


            try
            {
                filename = RecvFilename(clientsocket, dchan, 0);
                if (!CheckFilename(filename)) { return (false); }
                bufferFile = RecvFiledata(clientsocket, dchan, 0, filename);
                if (!CheckFileData(bufferFile, dchan, 0)) { return (false); }
                dchan.GetDownloadChannelInfo(filename).Switch_download();
                var filesize = dchan.Get_filesizes()[0];
                var binarybuffer = new BinaryWriter(File.Open(path + filename, FileMode.Append));
                binarybuffer.Write(bufferFile, 0, filesize);
                CloseBufferW(binarybuffer);
                return (true);
            }
            catch (Exception e) { return (false); }


        }

        // 4 - tree with file recv 
        private bool FTPtree(Socket clientsocket, string path)
        {
            var numele = 0;
            var dirpaths = new List<string>();
            var filepaths = new List<string>();
            string[] elements = null;
            int[] depths = null;
            var tag = 0;
            try
            {
               

                numele = RecvNumele(clientsocket);

                elements = new string[numele];
                depths = new int[numele];

                for (var i = 0; i < numele; i++)
                {
                    elements[i] = RecvElement(clientsocket);
                    depths[i] = RecvDepth(clientsocket);
                }

                CreateDirectoryTree(dirpaths, depths, filepaths, elements, ".");

                tag = RecvTag(clientsocket);
                if (!CheckTag(tag)) { return(false); }

               
                if (tag != FTPsupporter.Multifilesend && tag != FTPsupporter.Filesend) {
                    return (false);
                }

                FTPfilesByTree(clientsocket, filepaths.ToArray(), path, tag);
                        return (true);
            }
            catch (Exception e) { return (false); }



        }

        // 4 - tree with file recv 
        private bool FTPmultiTree(Socket clientsocket, string path)
        {
            try
            {
                var tag = 0;
                var numtrees = RecvNumele(clientsocket);
                for(var i = 0; i < numtrees; i++){
                    tag = RecvTag(clientsocket);
                    if (!CheckTag(tag)) { return (false); }
                    if (!FTPtree(clientsocket, path)){return (false);}
                }
                return(true);
            }
            catch (Exception e) { return (false); }

        }

        // 5 - recv all file and save it in the rigth position into directory tree using threading
        private bool FTPfilesByTree(Socket clientsocket, string [] paths, string path, int t) {
        var numfile = 0;
        string filename = null;
        byte [][] bufferFile = null;
        Thread [] nrecvfrom = null;
        var tag = 0;
            try
            {

            var dchan = new DownloadChannel(clientsocket.LocalEndPoint.ToString(), t, path);
            mDchans.Add(dchan);
            /*if (IsWorkingPath(dchan, path)) {
                var ch = GetChannel(path, clientsocket.LocalEndPoint.ToString());
                    mDchans.Add(dchan);
                    dchan.Set_mutex(ch.Get_Mutex());
            }
            */
           

            dchan.Set_socket(ref clientsocket);
            //dchan.Get_Mutex().WaitOne();
            dchan.Set_ps(FTPsupporter.Serial);
            //mDchansDataLock.WaitOne();
            //mDchans.Add(dchan);
            //mDchansDataLock.ReleaseMutex();
             


                if (t == FTPsupporter.Filesend)
                {
                    bufferFile = new byte[1][];
                    dchan.Set_num_trasf(1);
                    dchan.Set_filepaths(paths);
                    AttachDownloadInfosToChannel(ref dchan);
                    filename = RecvFilename(clientsocket, dchan, 0);
                    if (!CheckFilename(filename)) { return (false); }
                    bufferFile[0] = RecvFiledata(clientsocket, dchan, 0, filename);
                    if (!CheckFileData(bufferFile[0], dchan, 0)) { return (false); }
                    if (!FTPfilewrite_(bufferFile[0], dchan, 0)) { return (false); }
                }
                else
                {

                    numfile = RecvNumfile(clientsocket);
                    if (!CheckNumfile(numfile, dchan)) { return (false); }
                    dchan.Set_filepaths(paths);
                    AttachDownloadInfosToChannel(ref dchan);
                    bufferFile = new byte[numfile][];
                    nrecvfrom = new Thread[numfile];

                    for (var i = 0; i < numfile; i++)
                    {
                        tag = RecvTag(clientsocket);
                        if (!CheckTag(tag)) { return (false); }
                        filename = RecvFilename(clientsocket, dchan, 0);
                        if (!CheckFilename(filename)) { return (false); }
                        bufferFile[i] = RecvFiledata(clientsocket, dchan, 0, filename);
                        if (!CheckFileData(bufferFile[i], dchan, i)) { return (false); }

                        //send file
                        {
                            var index = i;
                            nrecvfrom[i] = new Thread(new ThreadStart(() => FTPfilewritepath(paths[index], bufferFile[index], dchan, index)))
                            {
                                Priority = ThreadPriority.Highest
                            };
                            nrecvfrom[i].Start();
                            dchan.Set_download_thread(nrecvfrom[i], i);
                        }
                    }

                    for (var i = 0; i < nrecvfrom.Length; i++)
                    {
                        nrecvfrom[i].Join();
                    }
                }
                if (!RefreshCannels(dchan)) { return(false); }
                return (true);
            }
            catch (Exception e) { return (false); }
}

       
//SUPPORTING METHODS

        // 1 - rename correctly the file
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
            return (filenametemp+estension);

        }

        // 2 - test if this path is used by another thread
        private bool IsWorkingPath(DownloadChannel newdchan, string path) {
            mDchansDataLock.WaitOne();
            foreach (var dchan in mDchans)
            {
                if (dchan.Get_path().Equals(path)) {
                         mDchansDataLock.ReleaseMutex();
                    return (true);
                }

            }
            mDchans.Add(newdchan);
            mDchansDataLock.ReleaseMutex();
                return (false);
        }

        // 3 - get a download channel with specified path
        private DownloadChannel GetChannel(string filepath, string ipaddress)
        {
            mDchansDataLock.WaitOne();
            foreach (var dchan in  mDchans)
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

        // 4 - says if the elementpath is a dir or no
        private bool IsDir(string path)
        {
            if (path.IndexOf(@".") == -1)
            {
                return (true);
            }
            return (false);
        }

        // 5 - create a dirpath or a filepath using isDir function
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

        public string GetPathOfFather(int index, int [] depths, string [] elements, List<string> dirpaths, List<string> filepaths) {
            string father = null;
            for (var i = index - 1; i >= 0; i--)
            {
                //todo cartella e non file
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
                   
                    return(path);
                    
                }

            }
            return (null);
        }

        // 6 - create the directory tree and return dirpaths and filepath
        public void CreateDirectoryTree(List<string> dirpaths, int[] depths, List<string> filepaths, string[] elements, string curroot)
        {
            var numelements = elements.Length;
            var rootname = curroot + @"\" + elements[0];
            var n = 0;
            var newelement = elements[0];
            while(Directory.Exists(rootname)){
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

        // SINGLE CLIENT THREAD VOID :

        // 1 - handle single client
        private void FTPrecv(Socket clientsocket, string path) {
            var result = false;
            var tag = 0;   
            var tos = 0;



            tos = RecvTag(clientsocket);
            if (!CheckTag(tag)) { return; }
         
           
      

           Gettag:

            tag = RecvTag(clientsocket);
            if (!CheckTag(tag)) { return; }
           
            var dchan = new DownloadChannel(clientsocket.LocalEndPoint.ToString(), tag, path);
            mDchans.Add(dchan);
            /*
                        if (IsWorkingPath(dchan, path)) {
                            var ch = GetChannel(path, clientsocket.LocalEndPoint.ToString());

                            dchan.Set_mutex(ch.Get_Mutex());
                        }


                        Console.WriteLine("get mutex " + Thread.CurrentThread.ManagedThreadId);
                        dchan.Get_Mutex().WaitOne();
              */
            dchan.Set_socket(ref clientsocket);



            switch (tag)
            {
                case FTPsupporter.Filesend:
                    {

                        result = FTPsingleFile(dchan);
                        break;

                    }
                case FTPsupporter.Multifilesend:
                    {

                        if (FTPcpu_priority() >= 50)
                        {
                            result = FTPmultiFile_t(dchan);
                        }
                        else
                        {
                            result = FTPmultiFile_s(dchan);
                        }
                        break;

                    }
                case FTPsupporter.Treesend:
                    {

                        result = FTPtree(clientsocket, path);
                        break;

                    }
                case FTPsupporter.Multitreesend:
                    {

                        result = FTPmultiTree(clientsocket, path);
                        break;

                    }
            }

            if(tos == FTPsupporter.ToSfileandtree){
                tos = FTPsupporter.UnknownInt;
                if (!RefreshCannels(dchan)) { return; }
                goto Gettag;
            }
      //      Console.WriteLine("release mutex " + Thread.CurrentThread.ManagedThreadId);
    //        dchan.Get_Mutex().ReleaseMutex();
            if (!RefreshCannels(dchan)) { return; }
            FTPclose();
            clientsocket.Close();
            return;
        }

        private bool RefreshCannels(DownloadChannel olddchan)
        {
           //mDchansDataLock.WaitOne();   
           var result = mDchans.Remove(olddchan);
           //mDchansDataLock.ReleaseMutex();
           return(result);
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
            var dchan = GetChannel(ipaddress.ToString());
            if (dchan == null)
            {
                return (false);
            }
            dchan.InterruptDownload();
            return (true);
        }

        private int RecvTag(Socket clientsocket) {
            var bufferIn = new byte[FTPsupporter.Tagsize];
            var received = Receive(clientsocket, bufferIn, null, 0);
            if (received == 0)
            {
                return (FTPsupporter.UnknownTag);
            }
            var tag = BitConverter.ToInt32(bufferIn, 0);
            return (tag);
        }

        private bool CheckTag(int tag) {
            return (tag != FTPsupporter.UnknownTag);
        }

        private bool CheckFilename(string filename) {
            return (filename != FTPsupporter.UnknownString);
        }

        private string RecvFilename(Socket clientsocket, DownloadChannel dchan, int n)
        {
            var bufferIn = new byte[FTPsupporter.Filenamelensize];
            var received = Receive(clientsocket, bufferIn, null, n);
            var filenamelen = BitConverter.ToInt32(bufferIn, 0);
            bufferIn = new byte[filenamelen];
            received += Receive(clientsocket, bufferIn, null, n);
            if (received == 0)
            {
                return (FTPsupporter.UnknownString);
            }
            var filename = Encoding.ASCII.GetString(bufferIn, 0, filenamelen);
            filename = AdjustFilename(filename, dchan.Get_path());
            return (filename);
        }

        private bool CheckFileData(byte [] bufferFile, DownloadChannel dchan, int n){
                var filesize = dchan.Get_filesizes()[n];
                if(bufferFile == null)
                  { return(false); }
                if(bufferFile.Length != filesize)
                  { return(false);}
                return(true);
       }

        private byte [] RecvFiledata(Socket clientsocket, DownloadChannel dchan, int n, string filename) {
            var bufferIn = new byte[FTPsupporter.Filesizesize];
            var received = Receive(clientsocket, bufferIn, null, 0);
            if(received != FTPsupporter.Filesizesize) { return(null); }
            var filesize = BitConverter.ToInt32(bufferIn, 0);
            dchan.Add_new_download(filename, filesize);
            bufferIn = new byte[filesize];
            received += Receive(clientsocket, bufferIn, dchan, n);
            return(bufferIn);
        }

        private int RecvNumfile(Socket clientsocket)
        {
            var bufferIn = new byte[FTPsupporter.Numfilesize];
            var received = Receive(clientsocket, bufferIn, null, 0);
            if (received != FTPsupporter.Numfilesize)
            {
                return (FTPsupporter.UnknownInt);
            }
            var numfile = BitConverter.ToInt32(bufferIn, 0);
            return (numfile);
        }

        private bool CheckNumfile(int numfile, DownloadChannel dchan)
        {
            dchan.Set_num_trasf(numfile);
            return (numfile != FTPsupporter.UnknownInt);
        }

        private int FTPcpu_priority() {
            return ( mDchans.ToArray().Length * 10);
        }
        
        private string RecvElement(Socket clientsocket)
        {
            var bufferIn = new byte[FTPsupporter.Elementlensize];
            var received = Receive(clientsocket, bufferIn, null, 0);
            if (received == 0)
            {
                return (FTPsupporter.UnknownString);
            }
            var elementlen = BitConverter.ToInt32(bufferIn, 0);
            bufferIn = new byte[elementlen];
            received = Receive(clientsocket, bufferIn, null, 0);
            if (received == 0)
            {
                return (FTPsupporter.UnknownString);
            }
            return (Encoding.ASCII.GetString(bufferIn, 0, elementlen));
          
        }

        private int RecvDepth(Socket clientsocket)
        {
            var bufferIn = new byte[FTPsupporter.Elementdepthsize];
            var received = Receive(clientsocket, bufferIn, null, 0);
            if (received == 0)
            {
                return (FTPsupporter.UnknownInt);
            }
            return (BitConverter.ToInt32(bufferIn, 0));
        }

        private int RecvNumele(Socket clientsocket)
        {
            var bufferIn = new byte[FTPsupporter.Numelementsize];
            var received = Receive(clientsocket, bufferIn, null, 0);
            if (received != FTPsupporter.Numelementsize)
            {
                return (FTPsupporter.UnknownInt);
            }
            var numele = BitConverter.ToInt32(bufferIn, 0);
            return (numele);
        }


        private bool CheckNumele(int numele)
        {
            return (numele != FTPsupporter.UnknownInt);
        }

        // GETTER(s) AND SETTER(s)
        public void SetPrivateMode(bool value) { mPrivateMode = Convert.ToBoolean(value); }
        public void SetAutomaticSave(bool value) { mAutomaticSave = Convert.ToBoolean(value); }
        public void SetAutomaticReception(bool value) { mAutomaticReception = Convert.ToBoolean(value); }
        public void SetDownloadDefaultFolder(string path) { mDownloadDefaultFolder = path; }

    }
}
