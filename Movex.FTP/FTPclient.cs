using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Threading;
using System.Text.RegularExpressions;
using Movex;


namespace Movex.FTP
{
    public class FTPclient {
        
        private List<UploadChannel> mUchans;
        private Mutex mUchansDataLock;
        private List<UploadChannelInfo> mUchaninfos;
        private static List<UploadChannelInfo> StaticUchanInfos;       
        private ManualResetEvent mUchanAvailability;
        private ManualResetEvent mWindowAvailability;




        public List<UploadChannelInfo> GetUCInfo()
        {
            return StaticUchanInfos;
        }

       
        public FTPclient() {
           mUchans = new List<UploadChannel>();
           mUchansDataLock = new Mutex();
           mUchaninfos = new List<UploadChannelInfo>();
           StaticUchanInfos = new List<UploadChannelInfo>();
        }

      
       
        private void FTPsingleFile_serial(IPAddress ipaddress)
        {

            try
            {
                var result = FTPsingleFile_s(ipaddress);
                return;
            }
            catch (Exception e) { throw new IOException("Cannot send file"); }

        }

      
        private void FTPmultiFile_serial(IPAddress ipaddress)
        {
            try
            {
               var result = FTPmultiFile_s(ipaddress);
                return;
            }
            catch (Exception e) { throw new IOException("Cannot send multiple files"); }
        }

      
      
     

        private void FTPsingleClientTree_serial(Socket clientsocket, IPAddress ipaddress, string root)
        {
            try
            {
                var result = FTPsingleClientTree_s(clientsocket, root, ipaddress);
                return;
            }
            catch (Exception e) { throw new IOException("Cannot send tree"); }

        }



        private void FTPsingleClientMultiTree_serial(Socket clientsocket, IPAddress ipaddress, string[] roots)
        {
            try
            {
                var result = FTPsingleClientMultiTree_s(clientsocket, ipaddress, roots);
                return;
            }
            catch (Exception e)
            {
                throw new IOException("Cannot send multiple trees");
            }


        }


      

       

        
        private bool FTPsingleFile_s(IPAddress ipaddress) {


            try
            {
                var uchan = GetChannel(ipaddress.ToString());
                uchan.Set_main_upload_thread(Thread.CurrentThread);
                var clientsocket = uchan.Get_socket();
                var filename = uchan.Get_filenames()[0];
                var filepath = uchan.Get_paths()[0];
                uchan.StartUpload(0);
                if (!SendFile(clientsocket, uchan, 0)) { throw new IOException("Cannot send file"); }
                return (true);
            }

            catch (Exception e) { throw new IOException("Cannot send file"); }


        }

       
        private bool FTPmultiFile_s(IPAddress ipaddress) {
            try
            {
                var uchan = GetChannel(ipaddress.ToString());

                if (uchan == null) { throw new IOException("Upload Channel Unaviable"); }

                var ipend = new IPEndPoint(ipaddress, FTPsupporter.ProtocolAttributes.Port);
                var numfiles = uchan.Get_num_trasf();
                var filenames = uchan.Get_filenames();
                var paths = uchan.Get_paths();
                var clientsocket = uchan.Get_socket();



                uchan.Set_main_upload_thread(Thread.CurrentThread);

                if (!SendTag(clientsocket, FTPsupporter.Tags.Multifilesend)) { throw new IOException("Cannot send tag"); }
                if (!SendNumfile(clientsocket, uchan)) { throw new IOException("Cannot send number of files"); }

                for (var i = 0; i < filenames.Length; i++)
                {
                    uchan.StartUpload(i);
                    if (!SendFile(clientsocket, uchan, i)) { throw new IOException("Cannot send file"); }
                    Console.WriteLine("thread id=" + Thread.CurrentThread.ManagedThreadId + " fiished to send nfile=" + i);
                }

                return (true);
            }
            catch (Exception e) { throw new IOException("Cannot send multiple files"); }

        }


        public string getBytesSufix(ref double bytes) {
            string[] sufixes = {"",  "K", "M", "G", "T", "P" };
            var s = 0;
            while (bytes > 1024) {
                bytes /= 1024;
                s++;
            }
            return (sufixes[s]);
        }

        public string getConvertedNumber(long bytes) {
            double b = bytes;
            string sufix = getBytesSufix(ref b);
            float r = (float)b;
            return (r.ToString() + " " + sufix + "b");
        }

      
       
        public void WaitClient(Socket clientsocket) {
            WaitUntilClose(clientsocket);
        }
        



        private bool FTPsingleClientTree_s(Socket clientsocket, string root, IPAddress ipaddress)
        {
            try
            {

                var elements = new List<string>();
                var filepaths = new List<string>();
                var depths = new List<int>();
                var numelements = 0;
                int[] depth = null;
                string[] element = null;
                string[] filenames = null;
                var j = 0;

                var nfiles = GetAllForDirectoryTree(root, filepaths, elements, depths);

                depth = depths.ToArray();
                element = elements.ToArray();
                numelements = element.Length;
                filenames = new string[nfiles];

                if (!SendTag(clientsocket, FTPsupporter.Tags.Treesend)) { throw new IOException("Cannot send tag"); }
                if (!SendNumele(clientsocket, numelements)) { throw new IOException("Cannot send number of elements"); }

                for (var i = 0; i < numelements; i++)
                {
                    if (element[i].Contains(".")) { filenames[j++] = element[i]; }
                    if (!SendElement(clientsocket, element[i])) { throw new IOException("Cannot send element"); }
                    if (!SendDepth(clientsocket, depth[i])) { throw new IOException("Cannot send depth"); }
                }

                var result = FTPsendbytree(filepaths.ToArray(), filenames, ipaddress, ref clientsocket);

                return (result);
            }
            catch (Exception e) { throw new IOException("Cannot send tree"); }

        }

        private bool FTPsingleClientMultiTree_s(Socket clientsocket, IPAddress ipaddress, string[] roots)
        {
            try
            {
                var ipend = new IPEndPoint(ipaddress, FTPsupporter.ProtocolAttributes.Port);
                var result = false;

                if (!SendTag(clientsocket, FTPsupporter.Tags.Multitreesend))
                {
                    throw new IOException("Cannot send tag");
                }

                if (!SendNumele(clientsocket, roots.Length))
                {
                    throw new IOException("Cannot send numer of elements");
                }

                foreach (var root in roots)
                {
                    result = FTPsingleClientTree_s(clientsocket, root, ipaddress);
                }
                return (result);

            }
            catch (Exception e) { throw new IOException("Cannot send multiple trees"); }

        }









        private long[] GetFileSizes(string[] paths, string[] filenames)
        {
            var filesizes = new long[filenames.Length];
            for (var i = 0; i < filenames.Length; i++)
            {
                if (!File.Exists(paths[i] + @"\" + filenames[i]))
                {
                    return (null);
                }
                filesizes[i] = new System.IO.FileInfo(paths[i] + @"\" + filenames[i]).Length;
            }
            return (filesizes);
        }

        private long GetTotFileSize(string[] paths, string[] filenames)
        {
            long totbytes = 0;
            for (var i = 0; i < filenames.Length; i++)
            {
                if (!File.Exists(paths[i] + filenames[i]))
                {
                    return (0);
                }
                totbytes += new System.IO.FileInfo(paths[i] + filenames[i]).Length;
            }
            return (totbytes);
        }


        private void WaitUntilClose(Socket clientsocket)
        {
            while (clientsocket.Connected);
            clientsocket.Dispose();
            clientsocket.Close();
            return;

        }



        private UploadChannel GetUploadChannels(ref Socket clientsocket, string[] paths, string[] filenames, IPAddress ipaddress, ref Thread mainthread, int ps)
        {
            var mainUploadThread = mainthread;
            var multifile = (filenames.Length > 1 ? true : false);
            var filesizes = GetFileSizes(paths, filenames);
            var numfile = filenames.Length;
            var tag = (multifile ? FTPsupporter.Tags.Multifilesend : FTPsupporter.Tags.Filesend);

                var uchan = new UploadChannel(null, paths, ipaddress.ToString(), tag);
                uchan.Set_num_trasf(numfile);
                uchan.Set_socket(ref clientsocket);
                uchan.Set_ps(ps);
                for (var j = 0; j < numfile; j++)
                {
                    uchan.Add_new_upload(filenames[j], filesizes[j]);
                }
                uchan.Set_main_upload_thread(mainUploadThread);
                mUchansDataLock.WaitOne();
                mUchans.Add(uchan);
                mUchansDataLock.ReleaseMutex();
            return(uchan);
        }


        


        private List<UploadChannelInfo> GetUploadChannelInfos(UploadChannel newUploadChannel)
        {
            var end_index = 0;
            var newUploadChannelInfos = new List<UploadChannelInfo>();
                var n = newUploadChannel.Get_n_infos();
                for (var i = 0; i < n; i++)
                {
                    var uchaninfo = new UploadChannelInfo(ref newUploadChannel);
                    uchaninfo.Set_current_index(i);


                    if (newUploadChannel.Get_ps() == FTPsupporter.ProtocolAttributes.Parallel)
                    {
                        end_index = i + 1;
                    }
                    else if (newUploadChannel.Get_ps() == FTPsupporter.ProtocolAttributes.Serial)
                    {
                        end_index = newUploadChannel.Get_num_trasf() - 1;
                    }

                    uchaninfo.Set_end_index(end_index);
                    newUploadChannel.Set_uchaninfo(ref uchaninfo);
                    newUploadChannelInfos.Add(uchaninfo);
                    mUchaninfos.Add(uchaninfo);
            }
            return (newUploadChannelInfos);
        }




       
     
        private void SetSockets(ref Socket[] clientsockets, IPAddress[] ipaddresses) {
            for (var i = 0; i < ipaddresses.Length; i++) {
                clientsockets[i] = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                var ipend = new IPEndPoint(ipaddresses[i], FTPsupporter.ProtocolAttributes.Port);
                clientsockets[i].Connect(ipend);
            }
            return;
        }

      
        private string [] ExtractFilesAndPaths(ref string[] paths) {
            var filenames = new string[paths.Length];
            var i = 0;
            foreach (var path in paths) {
                filenames[i] = ExtractCurrentFileFromPath(path);
                paths[i++] = ExtractPathFromFilePath(path);
            }
            return (filenames);
        }

      
        private void ExtractFilepathsAndDirpaths(string [] paths, ref string [] dirpaths, ref string [] filepaths) {
            var ndirs = 0;
            var nfiles = 0;
            foreach (var path in paths)
            {
                var element = ExtractCurrentFileFromPath(path);
                if (element == null)
                {
                    dirpaths[ndirs++] = path;
                }
                else
                {
                    filepaths[nfiles++] = path;
                }
            }

            
            if (ndirs == 0)
            {
                dirpaths = null;
            } else
            {
                Array.Resize(ref dirpaths, ndirs);
            }

          
            if (nfiles == 0)
            {
                filepaths = null;
            }
            else
            {
                Array.Resize(ref filepaths, nfiles);
            }

            return;
        }

        private Socket[] GetSocketsForClients(IPAddress[] ipaddresses) {
            var numclients = ipaddresses.Length;
            var clientsockets = new Socket[numclients];
            SetSockets(ref clientsockets, ipaddresses);
            return (clientsockets);
        }



        private bool FTPsend(string[] paths, IPAddress ipaddress, ref Socket clientsocket)
        {
            Thread ftp_thread = null;
            var newUploadChannel = new UploadChannel();
            var newUploadChannelInfos = new List<UploadChannelInfo>();
            var ps = -1;
            var filenames = ExtractFilesAndPaths(ref paths);

            ps = FTPset_thread(ref ftp_thread, filenames, ipaddress);
            newUploadChannel = GetUploadChannels(ref clientsocket, paths, filenames, ipaddress, ref ftp_thread, ps);
            newUploadChannelInfos = GetUploadChannelInfos(newUploadChannel);
            StaticUchanInfos = newUploadChannelInfos;

            // This synchronization primitive say: Get the UchanInfo. It is now available
            /*  mUchanAvailability.Set();

              // This synchornization primitive wait until Window(s) available
              mWindowAvailability.WaitOne();
              */

            ftp_thread.Name = "ClientFileSendAllThread";
            FTPstartAndWaitThread(ref ftp_thread);

            if (!RefreshCannels(newUploadChannel, newUploadChannelInfos)) {
                throw new IOException("Channels Unrefreshable");
            }

            return (true);

        }


       
       
        private void FTPstartAndWaitThread(ref Thread ftp_thread) {
            ftp_thread.Priority = ThreadPriority.Highest;
            ftp_thread.Start();
            ftp_thread.Join();
            return;
        }

      

        private bool FTPsendbytree(string[] paths, string[] filenames, IPAddress ipaddress, ref Socket connectedsockets)
        {

            Thread ftp_thread = null;
            var newUploadChannel = new UploadChannel();
            var newUploadChannelInfos = new List<UploadChannelInfo>();
            var clientsockets = connectedsockets;
            var ps = -1;

            try
            {
                ps = FTPset_thread(ref ftp_thread, filenames, ipaddress);

                newUploadChannel = GetUploadChannels(ref clientsockets, paths, filenames, ipaddress, ref ftp_thread, ps);
                newUploadChannelInfos = GetUploadChannelInfos(newUploadChannel);

                StaticUchanInfos = newUploadChannelInfos;
                ftp_thread.Priority = ThreadPriority.Highest;
                ftp_thread.Start();

                // This synchronization primitive say: Get the UchanInfo. It is now available
                //   mUchanAvailability.Set();

                // This synchornization primitive wait until Window(s) available
                // mWindowAvailability.WaitOne();

                ftp_thread.Join();

                if (!RefreshCannels(newUploadChannel, newUploadChannelInfos)) { throw new IOException("Channels Unrefreshable"); }
                return (true);
            }
            catch(Exception e){ throw new IOException("Cannot send files releated to tree"); }

        }






        private int FTPset_thread(ref Thread ftp_thread, string[] filenames, IPAddress ipaddress)
        {
            var multifile = false;

            var ps = FTPsupporter.ProtocolAttributes.Serial;

            if (filenames.Length > 1) { multifile = true; }

            if (!multifile)
            {
                ftp_thread = new Thread(() => FTPsingleFile_serial(ipaddress));
            }
            else
            {
               ftp_thread = new Thread(() => FTPmultiFile_serial(ipaddress));
            }
        
            return (ps);
        }

        private UploadChannel GetChannel(string ipaddress)
            {
            mUchansDataLock.WaitOne();
            foreach (var uchan in mUchans)
                {
                    if (uchan.Get_to().ToString().Equals(ipaddress))
                    {
                    mUchansDataLock.ReleaseMutex();
                    return (uchan);
                    }

                }
            mUchansDataLock.ReleaseMutex();
            return (null);
            }

        private string ExtractCurrentFromPath(string path)
        {
                var decomp = path.Split(@"\".ToCharArray());
                var last = decomp.Length - 1;
                return (decomp[last]);
        }

        private string ExtractCurrentFileFromPath(string filepath)
        {
            if (!File.GetAttributes(filepath).HasFlag(FileAttributes.Directory))
            {
                var decomp = filepath.Split(@"\".ToCharArray());
                var last = decomp.Length - 1;
                return (decomp[last]);
            }
            return (null);
        }

        private string ExtractCurrentDirFromPath(string root) {
            if (File.GetAttributes(root).HasFlag(FileAttributes.Directory))
            {
                var decomp = root.Split(@"\".ToCharArray());
                var last = decomp.Length - 1;
                return (decomp[last]);
            }
            return (null);
        }

        private string ExtractPathFromFilePath(string filepath)
        {
            if (!File.GetAttributes(filepath).HasFlag(FileAttributes.Directory))
            {
                var decomp = filepath.Split(@"\".ToCharArray());
                var file = decomp[decomp.Length - 1];
                var length = filepath.Length - file.Length;
                filepath = filepath.Substring(0, length);

            }
            return (filepath);
        }

       
        private int GetAllForDirectoryTree(string root, List<string> filepaths, List<string> elements, List<int> element_depths) {
                var nfiles = 0;

                var dir = ExtractCurrentDirFromPath(root);
             
                if (dir != null)
                {
                   elements.Add(dir);
                   element_depths.Add(0);
                }
                else
                {
                   return(0);
                }

            CollectFileListAsTree(root, filepaths, 1, elements, element_depths);
            nfiles = filepaths.ToArray().Length;

                
                return (nfiles);

            }

      
        private void CollectFileListAsTree(string root, List<string> filepaths, int depth, List<string> elements, List<int> depths)
            {
            string[] paths = null;

            if (File.GetAttributes(root).HasFlag(FileAttributes.Directory))
            {

                paths = Directory.GetFileSystemEntries(root);
            }
            else {
                return;
            }

            foreach (var path in paths)
                {
          
                    if (File.GetAttributes(path).HasFlag(FileAttributes.Directory))
                    {
                        var element = ExtractCurrentDirFromPath(path);
                        elements.Add(element);
                        depths.Add(depth);         
                        CollectFileListAsTree(path, filepaths, depth + 1, elements, depths);
                    }

                    else
                    {
                        var element = ExtractCurrentFileFromPath(path);
                        elements.Add(element);
                        depths.Add(depth);
                        filepaths.Add(ExtractPathFromFilePath(path));
                    }
                }

                return;
            }

        private void GetPathOfAllFile(string[] paths, List<string> filepaths)
            {
               
                foreach (var path in paths)
                {
                  
                    if (File.GetAttributes(path).HasFlag(FileAttributes.Directory))
                    {
                     
                        var pathsArray = Directory.GetFileSystemEntries(path);

                      
                        var list = new List<string>(pathsArray);
                        var subFolderPaths = list.ToArray();

                        
                        GetPathOfAllFile(subFolderPaths, filepaths);
                    }

                    else
                    {
                        var filepath = Path.GetDirectoryName(path);
                        filepaths.Add(filepath);
                    }
                }

                return;
            }

       
        private int Send(Socket clientsocket, byte[] bufferOut, UploadChannel uchan, int uploadindex) {
            var length = 0;
            var sended = 0;
            if (uchan != null && (!clientsocket.Connected || uchan.IsInterrupted()))
            {
                uchan.InterruptUpload();
                return (0);
            }

           

            length = bufferOut.Length;
            sended = clientsocket.Send(bufferOut, 0, length, 0);

            if (uchan == null)
            {
                return (sended);
            }


            if (sended == 0)
            {
                uchan.InterruptUpload();
            }
            else {
                uchan.Incr_sended_p(uploadindex, length);
            }
            return (sended);
        }

      
        private bool InterruptUpload(IPAddress ipaddress)
        {
            var uchan = GetChannel(ipaddress.ToString());
            if (uchan == null)
            {
                return (false);
            }
            uchan.InterruptUpload();
            return (true);
        }

        private bool SendTag(Socket clientsocket, int tag)
        {
            var bufferOut = new byte[FTPsupporter.Sizes.Tagsize];
            bufferOut = BitConverter.GetBytes(tag);

            var sended = Send(clientsocket, bufferOut, null, 0);
            if (sended != FTPsupporter.Sizes.Tagsize)
            {
                throw new IOException("Cannot send tag");
            }
            return (true);
        }

        private bool SendFile(Socket clientsocket, UploadChannel uchan, int n)
        {
            
            byte[] bufferOut;

            var tag = FTPsupporter.Tags.Filesend;
            try
            {
                if (!SendTag(clientsocket, tag))
                {
                    throw new IOException("Cannot send tag");
                }


                var filename = uchan.Get_filenames()[n];
                var filesize = uchan.Get_filesizes()[n];
                var path = uchan.Get_paths()[n];

                var uchaninfo = uchan.GetUploadChannelInfo(filename);


                var filenamelen_buff = new byte[FTPsupporter.Sizes.Filenamelensize];
                var filename_buff = new byte[filename.Length];
                var filesize_buff = new byte[FTPsupporter.Sizes.Filesizesize];

                filenamelen_buff = BitConverter.GetBytes(filename.Length);
                filename_buff = Encoding.ASCII.GetBytes(filename);
                filesize_buff = BitConverter.GetBytes(filesize);

                bufferOut = new byte[filenamelen_buff.Length + filename_buff.Length + filesize_buff.Length];
                filenamelen_buff.CopyTo(bufferOut, 0);
                filename_buff.CopyTo(bufferOut, filenamelen_buff.Length);
                filesize_buff.CopyTo(bufferOut, filenamelen_buff.Length + filename_buff.Length);

                var sended = Send(clientsocket, bufferOut, null, 0);
                if (sended != filenamelen_buff.Length + filename_buff.Length + filesize_buff.Length)
                {
                    throw new IOException("Cannot send fileheader");
                }


                var readFileStream = File.OpenRead(path + filename);
                long toread = 0;
                long readed = 0;
                var x = 1;
                var remainingtimes = uchan.Get_remaining_times();
                var throughputs = uchan.Get_throughputs();
                Console.WriteLine("Send: filename " + filename + " filesize " + filesize + "/");
                while (readed < filesize)
                {
                    toread = filesize - readed;
                    var filedata_buff = new byte[(toread > FTPsupporter.Sizes.Filedatasize ? FTPsupporter.Sizes.Filedatasize : toread)];
                    var nread = readFileStream.Read(filedata_buff, 0, filedata_buff.Length);
                    sended = Send(clientsocket, filedata_buff, uchan, n);
                    Console.WriteLine("Send: readed =" +getConvertedNumber(readed) + " toread =" + getConvertedNumber(toread) + " nread =" +getConvertedNumber(nread) + 
                        " remaining time =" + remainingtimes[n] + "s  throughput =" + getConvertedNumber((long) throughputs[n])+"/s");
                    if (sended != nread)
                    {
                        readFileStream.Dispose();
                        readFileStream.Close();
                        throw new IOException("Cannot send file");
                    }
                    readed += nread;
                }

                readFileStream.Dispose();
                readFileStream.Close();
                uchaninfo.Switch_upload();
                return (true);
            }
            catch (Exception e) { throw new IOException("Cannot send file"); }
        }        

        private bool SendNumfile(Socket clientsocket, UploadChannel uchan)
        {
            var numfile = uchan.Get_num_trasf();
            var bufferOut = new byte[FTPsupporter.Sizes.Numfilesize];
            bufferOut = BitConverter.GetBytes(numfile);

            var sended = Send(clientsocket, bufferOut, null, 0);
            if (sended != FTPsupporter.Sizes.Numfilesize)
            {
                throw new IOException("Cannot send number of files");
            }
            return (true);
        }

        private bool SendElement(Socket clientsocket, string element)
        {
         
            var elementlen_buff = BitConverter.GetBytes(element.Length);
            var element_buff = Encoding.ASCII.GetBytes(element);
            var bufferOut = new byte[FTPsupporter.Sizes.Elementlensize + element.Length];

            elementlen_buff.CopyTo(bufferOut, 0);
            element_buff.CopyTo(bufferOut, FTPsupporter.Sizes.Elementlensize);

            var sended = Send(clientsocket, bufferOut, null, 0);
            if (sended != (FTPsupporter.Sizes.Elementsizesize + element.Length))
            {
                throw new IOException("Cannot send element");
            }
            return (true);
            
        }

        private bool SendDepth(Socket clientsocket, int depth)
        {
            var depth_buff = BitConverter.GetBytes(depth);
            var sended = Send(clientsocket, depth_buff, null, 0);
            if (sended != FTPsupporter.Sizes.Depthsize){
                throw new IOException("Cannot send depth");
            }
            return (true);
        }

        private bool SendNumele(Socket clientsocket, int numele)
        {
            var numele_buff = BitConverter.GetBytes(numele);
            var sended = Send(clientsocket, numele_buff, null, 0);
            if (sended != FTPsupporter.Sizes.Numelementsize){
                throw new IOException("Cannot send number of elements");
            }
            return (true);
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


        private bool RefreshCannels(UploadChannel olduchan, List<UploadChannelInfo> olduchaninfos)
        {

            Console.WriteLine("\n\nUpload channel end details :\n\n");
            if (olduchan == null || olduchaninfos == null) { return (false); }
            var filenames = olduchan.Get_filenames();
            var filesizes = olduchan.Get_filesizes();
            var sended = olduchan.Get_sended();
            var throughputs = olduchan.Get_throughputs();
            for (var i = 0; i < olduchan.Get_num_trasf(); i++) {
                Console.WriteLine(i + " - filename " + filenames[i] + "  filesize "+getConvertedNumber(filesizes[i])+"\n sended "+getConvertedNumber(sended[i])+" throughput "+getConvertedNumber((long) throughputs[i])+"/s\n\n");
            }
            mUchansDataLock.WaitOne();
            if (!mUchans.Remove(olduchan)) {
                mUchansDataLock.ReleaseMutex();
                throw new IOException("Channels Unrefreshable");
            }
            
            foreach (var olduchaninfo in olduchaninfos)
            {
                if (!mUchaninfos.Remove(olduchaninfo)) {
                    mUchansDataLock.ReleaseMutex();
                    throw new IOException("ChannelInfos Unrefreshable"); }
            }

            mUchansDataLock.ReleaseMutex();
            return (true);

        }


        private Thread FTPget_thread(string[] paths, IPAddress ipaddress, Socket clientsocket)
        {
            var multifile = false;
            Thread ftp_thread = null;
           
            if (paths.Length > 1) { multifile = true; }

            if (!multifile)
            {
                ftp_thread = new Thread(() => FTPsingleClientTree_serial(clientsocket, ipaddress, paths[0]));
            }
            else
            {
                ftp_thread = new Thread(() => FTPsingleClientMultiTree_serial(clientsocket, ipaddress, paths));

            }

            ftp_thread.Name = "ClientSendAllTreeThread";

            return (ftp_thread);

        }




        private bool FTPTreesend(string[] directories, ref Socket clientsocket, IPAddress ipaddress)
        {
            try
            {
                var ftp_thread = FTPget_thread(directories, ipaddress, clientsocket);
                FTPstartAndWaitThread(ref ftp_thread);
                return (true);
            }
            catch (Exception e) { throw new IOException("Cannot send any tree"); }
        }


      
        private int GetToS(string [] dirpaths, string [] filepaths){
            if(dirpaths == null){
               if(filepaths == null){
                  return(FTPsupporter.Unknown.UnknownInt);
               }
               else{
                  return(FTPsupporter.ToSs.ToSfileonly);
               } 
            }
            else{
               if(filepaths == null){
                 return(FTPsupporter.ToSs.ToStreeonly);
               }
               else{
                 return(FTPsupporter.ToSs.ToSfileandtree);
               }
            }
        }

      


        private int Receive(Socket client, byte[] bufferIn)
        {
            var received = 0;
            var length = 0; 

            try
            {
               
                    length = bufferIn.Length;
                    while (received < length) {
                         received += client.Receive(bufferIn, received, length - received, 0);
                    }
                   
                    return (received);

            }
            catch (Exception e) { throw new IOException("Cannot receive ack"); }
        }



        private int RecvAck(Socket clientsocket)
        {
            var bufferIn = new byte[FTPsupporter.Sizes.Tagsize];
            var received = Receive(clientsocket, bufferIn);
            if (received == 0)
            {
                return (FTPsupporter.Unknown.UnknownTag);
            }
            var tag = BitConverter.ToInt32(bufferIn, 0);
            return (tag);
        }

        private bool CheckAck(int ack)
        {
            if (ack != FTPsupporter.ProtocolAttributes.Ack)
            {
                return (false);
            }
           
            return (true);
        }

        public void FTPsendAlltoClient(int tos, string [] directories, string[] files, IPAddress ipaddress, ref Socket clientsocket) {
            try
            {
                if (!SendTag(clientsocket, tos))
                {
                    return;
                }

                //wait the ack
                var ack = RecvAck(clientsocket);
                if (!CheckAck(ack))
                {
                    return;
                }

                Console.WriteLine("Send: Start Transfer with client " + clientsocket.RemoteEndPoint.ToString());

                if (directories != null)
                {
                    FTPTreesend(directories, ref clientsocket, ipaddress);
                }

                if (files != null)
                {
                    FTPsend(files, ipaddress, ref clientsocket);
                }

                //wait the ack
                ack = RecvAck(clientsocket);
                if (!CheckAck(ack))
                {
                    return;
                }
                Console.WriteLine("Send: Connection with client "+ clientsocket.RemoteEndPoint.ToString() + " ended succesfully");
                WaitClient(clientsocket);
            }
            catch (Exception e) { throw new IOException("Cannot execute sending to client"); }
        }


        public bool FTPsendAll(string[] paths, IPAddress[] ipaddresses, ManualResetEvent uchanAvailability, ManualResetEvent windowAvailability)
        {
            var numelements = paths.Length;
            var files = new string[numelements];
            var directories = new string[numelements];
            var numclients = ipaddresses.Length;
            Socket[] clientsockets = null;
            Thread[] nsendto = new Thread[numclients];
            var tos = 0;
            mUchanAvailability = uchanAvailability;
            mWindowAvailability = windowAvailability;

            try
            {
                ExtractFilepathsAndDirpaths(paths, ref directories, ref files);
                clientsockets = GetSocketsForClients(ipaddresses);
                tos = GetToS(directories, files);
                for (var i = 0; i < numclients; i++)
                {
                    {
                        var index = i;
                        nsendto[index] = new Thread(new ThreadStart(() => FTPsendAlltoClient(tos, directories, files, ipaddresses[index], ref clientsockets[index])))
                        {
                            Priority = ThreadPriority.Highest,
                            Name = "ClientThread" + index
                        };
                        nsendto[index].Start();
                    }
                }

                for (var i = 0; i < numclients; i++)
                {
                    nsendto[i].Join();
                }

                return (true);

            }

            catch (Exception e) { throw new IOException("Cannot send to clients"); }
        }




























































    }
}














































