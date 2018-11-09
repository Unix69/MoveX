﻿using System;
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
            catch (Exception e) { throw new IOException("Server not started"); }
        }
     
        public void FTPstart() { 
            var ipEnd = new IPEndPoint(IPAddress.Any, FTPsupporter.ProtocolAttributes.Port);
            mServersocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            
            try{ mServersocket.Bind(ipEnd); mServersocket.Listen(100); }
            catch(Exception e) { throw new IOException("Cannot Bind Server"); }
            
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



        private int Receive(Socket client, ref byte [] bufferIn, DownloadChannel dchan, int downloadindex) {
            try
            {

                if (dchan != null && (!client.Connected || dchan.IsInterrupted()))
                {
                    dchan.InterruptDownload();
                    return (0);
                }

                var received = 0;
                var length = 0;

              
                length = bufferIn.Length;

                while (received < length) {
                    received += client.Receive(bufferIn, received, length - received, 0);
                }

                if (dchan == null)
                {
                    return (received);
                }

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
            catch (Exception e) { throw new IOException("Cannot receive"); }
        }



      
       
   
        private bool FTPmultiFile_s(ref Socket clientsocket, int tag, string path)
        {
            var numfile = 0;
            byte[] bufferFile = null;
            string filename = null;

            var dchan = new DownloadChannel(clientsocket.LocalEndPoint.ToString(), tag, path);
            mDchansDataLock.WaitOne();
            mDchans.Add(dchan);
            mDchansDataLock.ReleaseMutex();
            dchan.Set_socket(ref clientsocket);
            dchan.Set_ps(FTPsupporter.ProtocolAttributes.Serial);
            dchan.Set_main_download_thread(Thread.CurrentThread);


            try
            {

                numfile = RecvNumfile(clientsocket);
                if (!CheckNumfile(numfile, dchan)) { throw new IOException("Number of files not correct"); }
                AttachDownloadInfosToChannel(ref dchan);
                for (var i = 0; i < numfile; i++)
                {
                    tag = RecvTag(clientsocket);
                    if (!CheckTag(tag)) { throw new IOException("Bad tag"); }
                    filename = RecvFilename(clientsocket, dchan, i);
                    if (!CheckFilename(filename)) { throw new IOException("Bad filename"); }
                    RecvFiledata(clientsocket, dchan, i, filename);
                }

                if (!RefreshCannels(dchan)) { throw new IOException("Channels Unrefreshable"); }
                return (true);
            }
            catch (Exception e) { throw new IOException("Cannot receive files"); }
        }


        public void AttachDownloadInfosToChannel(ref DownloadChannel dchan) {
            var n = dchan.Get_n_infos();
            var numfile = dchan.Get_num_trasf();
            var ps = dchan.Get_ps();
            for (var i = 0; i < n; i++)
            {
                var dchaninfo = new DownloadChannelInfo(ref dchan);
                dchaninfo.Set_current_index(i);
                if (ps == FTPsupporter.ProtocolAttributes.Serial)
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

        private bool FTPsingleFile(ref Socket clientsocket, int tag, string path)
        {
            var dchan = new DownloadChannel(clientsocket.LocalEndPoint.ToString(), tag, path);

            mDchansDataLock.WaitOne();
            mDchans.Add(dchan);
            mDchansDataLock.ReleaseMutex();

            dchan.Set_socket(ref clientsocket);

            string filename = null;
            byte[] bufferFile = null;
            dchan.Set_num_trasf(1);
            dchan.Set_main_download_thread(Thread.CurrentThread);
            dchan.Set_ps(FTPsupporter.ProtocolAttributes.Serial);
            AttachDownloadInfosToChannel(ref dchan);


            try
            {
                filename = RecvFilename(clientsocket, dchan, 0);
                if (!CheckFilename(filename)) { throw new IOException("Bad filename"); }
                RecvFiledata(clientsocket, dchan, 0, filename);
                if (!RefreshCannels(dchan)) { throw new IOException("Channels Unrefreshable"); }
                return (true);
            }
            catch (Exception e) { return (false); }


        }

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
                if (!CheckNumele(numele)) { { throw new IOException("Bad number of elements"); } }

                elements = new string[numele];
                depths = new int[numele];

                for (var i = 0; i < numele; i++)
                {
                    elements[i] = RecvElement(clientsocket);            
                    depths[i] = RecvDepth(clientsocket);
                }

                CreateDirectoryTree(dirpaths, depths, filepaths, elements, ".");

                tag = RecvTag(clientsocket);
                if (!CheckTag(tag)) { throw new IOException("Bad tag"); }

               
                if (tag != FTPsupporter.Tags.Multifilesend && tag != FTPsupporter.Tags.Filesend) {
                    throw new IOException("Bad tag");
                }

                FTPfilesByTree(clientsocket, filepaths.ToArray(), path, tag);
                        return (true);
            }
            catch (Exception e) { throw new IOException("Cannot receive tree"); }



        }

         
        private bool FTPmultiTree(Socket clientsocket, string path)
        {
            try
            {
                var tag = 0;
                var numtrees = RecvNumele(clientsocket);
                if (!CheckNumele(numtrees)) { throw new IOException("Bad number of elements"); }
                for (var i = 0; i < numtrees; i++){
                    tag = RecvTag(clientsocket);
                    if (!CheckTag(tag)) { throw new IOException("Bad tag"); }
                    if (!FTPtree(clientsocket, path)){ throw new IOException("Cannot receive tree"); }
                }
                return(true);
            }
            catch (Exception e) { throw new IOException("Cannot receive multiple trees"); }

        }

     
        private bool FTPfilesByTree(Socket clientsocket, string [] paths, string path, int t) {
        var numfile = 0;
        string filename = null;
        byte [] bufferFile = null;
        Thread [] nrecvfrom = null;
        var tag = 0;
            try
            {

            var dchan = new DownloadChannel(clientsocket.LocalEndPoint.ToString(), t, path);

                mDchansDataLock.WaitOne();
                mDchans.Add(dchan);
                mDchansDataLock.ReleaseMutex();

                dchan.Set_socket(ref clientsocket);
                dchan.Set_ps(FTPsupporter.ProtocolAttributes.Serial);

                if (t == FTPsupporter.Tags.Filesend)
                {
                    dchan.Set_num_trasf(1);
                    dchan.Set_filepaths(paths);
                    AttachDownloadInfosToChannel(ref dchan);
                    filename = RecvFilename(clientsocket, dchan, 0);
                    if (!CheckFilename(filename)) { throw new IOException("Bad filename"); }
                    RecvFiledata(clientsocket, dchan, 0, filename);
                }
                else
                {

                    numfile = RecvNumfile(clientsocket);
                    if (!CheckNumfile(numfile, dchan)) { throw new IOException("Bad number of files"); }
                    dchan.Set_filepaths(paths);
                    AttachDownloadInfosToChannel(ref dchan);
                    for (var i = 0; i < numfile; i++)
                    {
                        tag = RecvTag(clientsocket);
                        if (!CheckTag(tag)) { throw new IOException("Bad tag"); }
                        filename = RecvFilename(clientsocket, dchan, 0);
                        if (!CheckFilename(filename)) { throw new IOException("Bad filename"); }
                        RecvFiledata(clientsocket, dchan, 0, filename);
                    }
                }
                if (!RefreshCannels(dchan)) { throw new IOException("Channels Unrefeshable"); }
                return (true);
            }
            catch (Exception e) { return (false); }
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
            return (filenametemp+estension);

        }

     

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

        public string GetPathOfFather(int index, int [] depths, string [] elements, List<string> dirpaths, List<string> filepaths) {
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
                   
                    return(path);
                    
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









        //adding send for server sending ack first and at end of session




        public bool CheckToS(int tos) {
            return (tos == FTPsupporter.ToSs.ToSfileandtree || tos == FTPsupporter.ToSs.ToStreeonly || tos == FTPsupporter.ToSs.ToSfileonly);
        }


         private void FTPrecv(Socket clientsocket, string path) {
            var result = false;
            var tag = 0;   
            var tos = 0;



            tos = RecvTag(clientsocket);
            if (!CheckToS(tos)) { throw new IOException("Bad ToS"); }

            if (!SendAck(clientsocket, FTPsupporter.ProtocolAttributes.Ack))
            {
                throw new IOException("Cannot send ack");
            }

            Gettag:

            tag = RecvTag(clientsocket);
            if (!CheckTag(tag)) { throw new IOException("Bad tag"); }
           
           
            switch (tag)
            {
                case FTPsupporter.Tags.Filesend:
                    {

                        result = FTPsingleFile(ref clientsocket, tag, path);
                        break;

                    }
                case FTPsupporter.Tags.Multifilesend:
                    {
                        result = FTPmultiFile_s(ref clientsocket, tag, path);
                        break;
                    }
                case FTPsupporter.Tags.Treesend:
                    {

                        result = FTPtree(clientsocket, path);
                        break;

                    }
                case FTPsupporter.Tags.Multitreesend:
                    {

                        result = FTPmultiTree(clientsocket, path);
                        break;

                    }
            }

            if(tos == FTPsupporter.ToSs.ToSfileandtree){
                tos = FTPsupporter.Unknown.UnknownInt;
                goto Gettag;
            }


            if (!SendAck(clientsocket, FTPsupporter.ProtocolAttributes.Ack)) {
                throw new IOException("Cannot send ack");
            }

            clientsocket.Close();

            return;
        }

        private int Send(Socket clientsocket, byte[] bufferOut)
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
            catch (Exception e) { throw new IOException("Cannot send ack"); }
        }



        private bool SendAck(Socket clientsocket, int ack) {
            var bufferOut = new byte[FTPsupporter.Sizes.Tagsize];
            bufferOut = BitConverter.GetBytes(ack);

            var sended = Send(clientsocket, bufferOut);
            if (sended != FTPsupporter.Sizes.Tagsize)
            {
                throw new IOException("Cannot send ack");
            }
            return (true);
        }


        private bool RefreshCannels(DownloadChannel olddchan)
        {
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
            var dchan = GetChannel(ipaddress.ToString());
            if (dchan == null)
            {
                return (false);
            }
            dchan.InterruptDownload();
            return (true);
        }

        private int RecvTag(Socket clientsocket) {
            var bufferIn = new byte[FTPsupporter.Sizes.Tagsize];
            var received = Receive(clientsocket, ref bufferIn, null, 0);
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

        private string RecvFilename(Socket clientsocket, DownloadChannel dchan, int n)
        {
            var bufferIn = new byte[FTPsupporter.Sizes.Filenamelensize];
            var received = Receive(clientsocket, ref bufferIn, null, n);
            var filenamelen = BitConverter.ToInt32(bufferIn, 0);
            bufferIn = new byte[filenamelen];
            received += Receive(clientsocket, ref bufferIn, null, n);
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
 
        private void RecvFiledata(Socket clientsocket, DownloadChannel dchan, int n, string filename) {
            long toreceive = 0;
            int nrecv = 0;
            var bufferIn = new byte[FTPsupporter.Sizes.Filesizesize];
            var received = (long) Receive(clientsocket,ref bufferIn, null, 0);
            if(received != FTPsupporter.Sizes.Filesizesize) { return; }
            var filesize = BitConverter.ToInt64(bufferIn, 0);


            dchan.Add_new_download(filename, filesize);
            received = 0;

            var path = dchan.Get_filepaths()[n];
            var binarybuffer = new BinaryWriter(File.Open(path + filename, FileMode.Append));
            Console.WriteLine("Recpt: write file=" + filename + " size=" + filesize + " buffersize=" + bufferIn.Length);

         
            Console.WriteLine("Recpt: filename " + filename + " filesize " + filesize + "/");
            while (received < filesize)
            {
                toreceive = filesize - ((long)received);
                var filedata_buff = new byte[(toreceive > FTPsupporter.Sizes.Filedatasize ? FTPsupporter.Sizes.Filedatasize : toreceive)];
                nrecv = Receive(clientsocket, ref filedata_buff, dchan, n);
                binarybuffer.Write(filedata_buff, 0 , nrecv);
                Console.WriteLine("Recpt: received " + received + " toreceive " + toreceive + " buffersize " + filedata_buff.Length + " nrecv " + nrecv);
                received += (long) nrecv;
            }

            CloseBufferW(binarybuffer);
            dchan.GetDownloadChannelInfo(filename).Switch_download();

            return;
        }

        private int RecvNumfile(Socket clientsocket)
        {
            var bufferIn = new byte[FTPsupporter.Sizes.Numfilesize];
            var received = Receive(clientsocket, ref bufferIn, null, 0);
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
        
        private string RecvElement(Socket clientsocket)
        {
            var bufferIn = new byte[FTPsupporter.Sizes.Elementlensize];
            var received = Receive(clientsocket,ref bufferIn, null, 0);
            if (received == 0)
            {
                throw new IOException("Bad element length");
            }
            var elementlen = BitConverter.ToInt32(bufferIn, 0);
            bufferIn = new byte[elementlen];
            received = Receive(clientsocket,ref bufferIn, null, 0);
            if (received == 0)
            {
                throw new IOException("Bad element");
            }
            return (Encoding.ASCII.GetString(bufferIn, 0, elementlen));
          
        }

        private int RecvDepth(Socket clientsocket)
        {
            var bufferIn = new byte[FTPsupporter.Sizes.Elementdepthsize];
            var received = Receive(clientsocket,ref bufferIn, null, 0);
            if (received == 0)
            {
                throw new IOException("Bad depth");
            }
            return (BitConverter.ToInt32(bufferIn, 0));
        }

        private int RecvNumele(Socket clientsocket)
        {
            var bufferIn = new byte[FTPsupporter.Sizes.Numelementsize];
            var received = Receive(clientsocket,ref bufferIn, null, 0);
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
