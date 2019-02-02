using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Threading;
using System.Security;
using System.Collections.Concurrent;

namespace Movex.FTP
{
    public class FTPclient
    {

        #region Member(s)
        private List<UploadChannel> mUchans;
        private Mutex mUchansDataLock;
        private UTransfer[] mTransfer;

        // Member(s) useful for syncrhonization with Movex.View.Core
        private WindowRequester mWindowRequester;
        #endregion

        public FTPclient()
        {

            mUchans = new List<UploadChannel>();
            mUchansDataLock = new Mutex();
        }

        private void FTPsingleFile_serial(IPAddress ipaddress)
        {

            try
            {
                var result = FTPsingleFile_s(ipaddress);
                return;
            }
            
            catch (SocketException e) { Console.WriteLine(e.Message); throw e; }
            catch (Exception e) { Console.WriteLine(e.Message); throw e; }

        }

        private void FTPmultiFile_serial(IPAddress ipaddress)
        {
            try
            {
                var result = FTPmultiFile_s(ipaddress);
                return;
            }
            
            catch (SocketException e) { Console.WriteLine(e.Message); throw e; }
            catch (Exception e) { Console.WriteLine(e.Message); throw e; }
        }

        private void FTPsingleClientTree_serial(Socket clientsocket, IPAddress ipaddress, string root, UTransfer transfer, ManualResetEvent uTransferAvailability, ManualResetEvent windowAvailability)
        {
            try
            {
                var result = FTPsingleClientTree_s(ref clientsocket, root, ipaddress, transfer, uTransferAvailability, windowAvailability);
                return;
            }
            catch (SocketException e) { Console.WriteLine(e.Message); throw e; }
            catch (Exception e) { Console.WriteLine(e.Message); throw e; }

        }

        private void FTPsingleClientMultiTree_serial(Socket clientsocket, IPAddress ipaddress, string[] roots, UTransfer transfer, ManualResetEvent uTransferAvailability, ManualResetEvent windowAvailability)
        {
            try
            {
                var result = FTPsingleClientMultiTree_s(ref clientsocket, ipaddress, roots, transfer, uTransferAvailability, windowAvailability);
                return;
            }
            catch (SocketException e) { Console.WriteLine(e.Message); throw e; }
            catch (Exception e) { Console.WriteLine(e.Message); throw e; }



        }

        private bool FTPsingleFile_s(IPAddress ipaddress)
        {


            try
            {
                var uchan = GetChannel(ipaddress.ToString());
                uchan.Set_main_upload_thread(Thread.CurrentThread);
                var clientsocket = uchan.Get_socket();
                uchan.StartUpload(0);
                if (!SendFile(ref clientsocket, uchan, 0)) { throw new IOException("Cannot send file"); }
                return (true);
            }

            catch (SocketException e) { Console.WriteLine(e.Message); throw e; }
            catch (Exception e) { Console.WriteLine(e.Message); throw e; }




        }

        private bool FTPmultiFile_s(IPAddress ipaddress)
        {
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

                if (!SendTag(ref clientsocket, FTPsupporter.Tags.Multifilesend)) { throw new IOException("Cannot send tag"); }
                if (!SendNumfile(ref clientsocket, uchan)) { throw new IOException("Cannot send number of files"); }

                for (var i = 0; i < filenames.Length; i++)
                {
                    uchan.StartUpload(i);
                    if (!SendFile(ref clientsocket, uchan, i)) { throw new IOException("Cannot send file"); }
                    Console.WriteLine("thread id=" + Thread.CurrentThread.ManagedThreadId + " fiished to send nfile=" + i);
                }

                return (true);
            }
       
            catch (SocketException e) { Console.WriteLine(e.Message); throw e; }
            catch (Exception e) { Console.WriteLine(e.Message); throw e; }

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
            var sufix = GetBytesSufix(ref b);
            var r = (float)b;
            return (r.ToString() + " " + sufix + "b");
        }

        public void WaitClient(ref Socket clientsocket)
        {
            try
            {
                WaitUntilClose(clientsocket);
            }
            catch (SocketException e) { Console.WriteLine(e.Message); throw e; }
            catch (ObjectDisposedException e) { Console.WriteLine(e.Message); throw e; }
            catch (Exception e) { Console.WriteLine(e.Message); throw e; }
        }

        private bool FTPsingleClientTree_s(ref Socket clientsocket, string root, IPAddress ipaddress, UTransfer transfer, ManualResetEvent uTransferAvailability, ManualResetEvent windowAvailability)
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

                if (!SendTag(ref clientsocket, FTPsupporter.Tags.Treesend)) { throw new IOException("Cannot send tag"); }
                if (!SendNumele(ref clientsocket, numelements)) { throw new IOException("Cannot send number of elements"); }

                for (var i = 0; i < numelements; i++)
                {
                    if (element[i].Contains(".")) { filenames[j++] = element[i]; }
                    if (!SendElement(ref clientsocket, element[i])) { throw new IOException("Cannot send element"); }
                    if (!SendDepth(ref clientsocket, depth[i])) { throw new IOException("Cannot send depth"); }
                }

                var result = FTPsendbytree(filepaths.ToArray(), filenames, ipaddress, ref clientsocket, transfer, uTransferAvailability, windowAvailability);

                return (result);
            }
            catch (SocketException e) { Console.WriteLine(e.Message); throw e; }
            catch (Exception e) { Console.WriteLine(e.Message); throw e; }

        }

        private bool FTPsingleClientMultiTree_s(ref Socket clientsocket, IPAddress ipaddress, string[] roots, UTransfer transfer, ManualResetEvent uTransferAvailability, ManualResetEvent windowAvailability)
        {
            try
            {
                var ipend = new IPEndPoint(ipaddress, FTPsupporter.ProtocolAttributes.Port);
                var result = false;

                if (!SendTag(ref clientsocket, FTPsupporter.Tags.Multitreesend))
                {
                    throw new IOException("Cannot send tag");
                }

                if (!SendNumele(ref clientsocket, roots.Length))
                {
                    throw new IOException("Cannot send numer of elements");
                }

                foreach (var root in roots)
                {
                    result = FTPsingleClientTree_s(ref clientsocket, root, ipaddress, transfer, uTransferAvailability, windowAvailability);
                }
                return (result);

            }
            catch (SocketException e) { Console.WriteLine(e.Message); throw e; }
            catch (Exception e) { Console.WriteLine(e.Message); throw e; }



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
            try
            {
                while (clientsocket.Connected) ;
                clientsocket.Shutdown(SocketShutdown.Both);
                clientsocket.Close();
            }
            catch (SocketException e) { Console.WriteLine(e.Message); throw e; }
            catch (ObjectDisposedException e) { Console.WriteLine(e.Message); throw e; }
            catch (Exception e) { Console.WriteLine(e.Message); throw e; }

            return;

        }

        private UploadChannel GetUploadChannel(ref Socket clientsocket, string[] paths, string[] filenames, IPAddress ipaddress)
        {
            try
            {
                var mainUploadThread = Thread.CurrentThread;
                var multifile = (filenames.Length > 1 ? true : false);
                var filesizes = GetFileSizes(paths, filenames);
                var numfile = filenames.Length;
                var tag = (multifile ? FTPsupporter.Tags.Multifilesend : FTPsupporter.Tags.Filesend);

                var uchan = new UploadChannel(null, paths, ipaddress.ToString(), tag);

                uchan.Set_num_trasf(numfile);
                uchan.Set_socket(ref clientsocket);
                uchan.Set_main_upload_thread(mainUploadThread);
                for (var j = 0; j < numfile; j++)
                {
                    uchan.Add_new_upload(filenames[j], filesizes[j]);
                }
                mUchansDataLock.WaitOne();
                mUchans.Add(uchan);
                mUchansDataLock.ReleaseMutex();
                return (uchan);
            }
            catch (IndexOutOfRangeException e) { Console.WriteLine(e.Message); throw e; }
            catch (OutOfMemoryException e) { Console.WriteLine(e.Message); throw e; }
            catch (ApplicationException e) { Console.WriteLine(e.Message); throw e; }
            catch (ObjectDisposedException e) { Console.WriteLine(e.Message); throw e; }
            catch (ArgumentNullException e) { Console.WriteLine(e.Message); throw e; }
            catch (AbandonedMutexException e) { Console.WriteLine(e.Message); throw e; }
            catch (InvalidOperationException e) { Console.WriteLine(e.Message); throw e; }
            catch (Exception e) { Console.WriteLine(e.Message); throw e; }
        }

        private void SetSockets(ref Socket[] clientsockets, IPAddress[] ipaddresses)
        {
            try
            {
                for (var i = 0; i < ipaddresses.Length; i++)
                {
                    clientsockets[i] = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    var ipend = new IPEndPoint(ipaddresses[i], FTPsupporter.ProtocolAttributes.Port);
                    clientsockets[i].Connect(ipend);
                }
                return;
            }
            catch (IndexOutOfRangeException e) { Console.WriteLine(e.Message); throw e; }
            catch (OutOfMemoryException e) { Console.WriteLine(e.Message); throw e; }
            catch (NotSupportedException e) { Console.WriteLine(e.Message); throw e; }
            catch (SocketException e) { Console.WriteLine(e.Message); throw e; }
            catch (ArgumentNullException e) { Console.WriteLine(e.Message); throw e; }
            catch (InvalidOperationException e) { Console.WriteLine(e.Message); throw e; }
            catch (Exception e) { Console.WriteLine(e.Message); throw e; }
        }

        private string[] ExtractFilesAndPaths(ref string[] paths)
        {

            var filenames = new string[paths.Length];
            var i = 0;
            foreach (var path in paths)
            {
                filenames[i] = ExtractCurrentFileFromPath(path);
                paths[i++] = ExtractPathFromFilePath(path);
            }
            return (filenames);
        }

        private void ExtractFilepathsAndDirpaths(string[] paths, ref string[] dirpaths, ref string[] filepaths)
        {
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
            }
            else
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

        private Socket[] GetSocketsForClients(IPAddress[] ipaddresses)
        {
            try
            {
                var numclients = ipaddresses.Length;
                var clientsockets = new Socket[numclients];
                SetSockets(ref clientsockets, ipaddresses);
                return (clientsockets);
            }
            catch (IndexOutOfRangeException e) { Console.WriteLine(e.Message); throw e; }
            catch (OutOfMemoryException e) { Console.WriteLine(e.Message); throw e; }
            catch (NotSupportedException e) { Console.WriteLine(e.Message); throw e; }
            catch (SocketException e) { Console.WriteLine(e.Message); throw e; }
            catch (ArgumentNullException e) { Console.WriteLine(e.Message); throw e; }
            catch (InvalidOperationException e) { Console.WriteLine(e.Message); throw e; }
            catch (Exception e) { Console.WriteLine(e.Message); throw e; }

        }

        private bool FTPFiles(string[] paths, IPAddress ipaddress, ref Socket clientsocket, UTransfer transfer, ManualResetEvent uTransferAvailability, ManualResetEvent windowAvailability)
        {
            try
            {
                var filenames = ExtractFilesAndPaths(ref paths);
                var newUploadChannel = GetUploadChannel(ref clientsocket, paths, filenames, ipaddress);
                transfer.AttachToInterface(newUploadChannel);
                uTransferAvailability.Set();
                windowAvailability.WaitOne();
                Files(paths, ipaddress);
                transfer.DetachFromInterface(newUploadChannel);
                if (!RefreshCannels(newUploadChannel))
                {
                    throw new IOException("Channels Unrefreshable");
                }
            }

            catch (IndexOutOfRangeException e) { Console.WriteLine(e.Message); throw e; }
            catch (OutOfMemoryException e) { Console.WriteLine(e.Message); throw e; }
            catch (ApplicationException e) { Console.WriteLine(e.Message); throw e; }
            catch (ObjectDisposedException e) { Console.WriteLine(e.Message); throw e; }
            catch (ArgumentNullException e) { Console.WriteLine(e.Message); throw e; }
            catch (AbandonedMutexException e) { Console.WriteLine(e.Message); throw e; }
            catch (InvalidOperationException e) { Console.WriteLine(e.Message); throw e; }
            catch (Exception e) { Console.WriteLine(e.Message); throw e; }

            return (true);

        }

        private bool FTPsendbytree(string[] paths, string[] filenames, IPAddress ipaddress, ref Socket connectedsockets, UTransfer transfer, ManualResetEvent uTransferAvailability, ManualResetEvent windowAvailability)
        {
            var clientsockets = connectedsockets;
            try
            {

                var newUploadChannel = GetUploadChannel(ref clientsockets, paths, filenames, ipaddress);
                transfer.AttachToInterface(newUploadChannel);
                uTransferAvailability.Set();
                windowAvailability.WaitOne();
                Files(filenames, ipaddress);
                transfer.DetachFromInterface(newUploadChannel);
                if (!RefreshCannels(newUploadChannel)) { throw new IOException("Channels Unrefreshable"); }
                return (true);
            }
            
            catch (SocketException e) { Console.WriteLine(e.Message); throw e; }
            catch (Exception e) { Console.WriteLine(e.Message); throw e; }

        }

        private void Files(string[] filenames, IPAddress ipaddress)
        {
            try {
            var multifile = false;

            if (filenames.Length > 1) { multifile = true; }

            if (!multifile) { FTPsingleFile_serial(ipaddress); }
            else { FTPmultiFile_serial(ipaddress); }

            return;
            }
            catch (SocketException e) { Console.WriteLine(e.Message); throw e; }
            catch (Exception e) { Console.WriteLine(e.Message); throw e; }
        }

        public UTransfer GetTransfer(string ipAddress)
        {
            foreach (var uTransfer in mTransfer)
            {
                var candidate = uTransfer.GetTo();
                if (candidate != null && candidate.Equals(ipAddress))
                {
                    return uTransfer;
                }
            }
            
            // Case: it couldn't find it
            return(null);
        }


        public UploadChannel GetChannel(string ipaddress)
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

        private string ExtractCurrentDirFromPath(string root)
        {
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

        private int GetAllForDirectoryTree(string root, List<string> filepaths, List<string> elements, List<int> element_depths)
        {
            var nfiles = 0;

            var dir = ExtractCurrentDirFromPath(root);

            if (dir != null)
            {
                elements.Add(dir);
                element_depths.Add(0);
            }
            else
            {
                return (0);
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
            else
            {
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

        private void GetFileListInRoot(string root, List<string> filepaths, List<string> fileFullPaths)
        {
            string[] paths = null;

            if (File.GetAttributes(root).HasFlag(FileAttributes.Directory))
            {

                paths = Directory.GetFileSystemEntries(root);
            }
            else
            {
                return;
            }

            foreach (var path in paths)
            {

                if (File.GetAttributes(path).HasFlag(FileAttributes.Directory))
                {
                    GetFileListInRoot(path, filepaths, fileFullPaths);
                }

                else
                {
                    fileFullPaths.Add(path);
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

        private int Send(ref Socket clientsocket, ref byte[] bufferOut, UploadChannel uchan, int uploadindex)
        {
           
            var sended = 0;
            var length = (bufferOut == null ? 0 : bufferOut.Length);

            try
            {
                while (sended < length)
                {
                    sended += clientsocket.Send(bufferOut, sended , length - sended, 0);
                }
            }
            catch (ArgumentNullException e) { Console.WriteLine(e.Message); throw e; }
            catch (ArgumentException e) { Console.WriteLine(e.Message); throw e; }
            catch (SocketException e) { Console.WriteLine(e.Message); throw e; }
            catch (ObjectDisposedException e) { Console.WriteLine(e.Message); throw e; }
            catch (Exception e) { Console.WriteLine(e.Message); throw e; }


            if (sended == 0 && uchan == null) { throw new IOException("Cannot send any bytes"); }
            if (sended == 0 && uchan != null) { uchan.InterruptUpload(); }
            if (uchan != null){uchan.Incr_sended_p(uploadindex, length);}

            return (sended);

        }

        public bool InterruptUpload(IPAddress ipaddress)
        {
            var uchan = GetChannel(ipaddress.ToString());
            if (uchan == null){ return (false); }

            try{
                uchan.InterruptUpload();
            }
            catch (SecurityException e) { Console.WriteLine(e.Message); throw e; }
            catch (ThreadStateException e) { Console.WriteLine(e.Message); throw e; }

            return (true);
        }

        private bool SendTag(ref Socket clientsocket, int tag)
        {
            var bufferOut = new byte[FTPsupporter.Sizes.Tagsize];
            bufferOut = BitConverter.GetBytes(tag);

            var sended = Send(ref clientsocket, ref bufferOut, null, 0);
            if (sended != FTPsupporter.Sizes.Tagsize)
            {
                throw new IOException("Cannot send tag");
            }
            return (true);
        }

        private bool SendFile(ref Socket clientsocket, UploadChannel uchan, int n)
        {

            byte[] bufferOut;

            var tag = FTPsupporter.Tags.Filesend;
            try
            {
                if (!SendTag(ref clientsocket, tag))
                {
                    throw new IOException("Cannot send tag");
                }
                

                var filename = uchan.Get_filenames()[n];
                var filesize = uchan.Get_filesizes()[n];
                var path = uchan.Get_paths()[n];



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

                var sended = Send(ref clientsocket, ref bufferOut, null, 0);
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
                Console.WriteLine("Send: filename " + filename + " filesize " + GetConvertedNumber(filesize) + " /");
                while (readed < filesize)
                {
                    toread = filesize - readed;
                    var filedata_buff = new byte[(toread > FTPsupporter.Sizes.Filedatasize ? FTPsupporter.Sizes.Filedatasize : toread)];
                    var nread = readFileStream.Read(filedata_buff, 0, filedata_buff.Length);
                    sended = Send(ref clientsocket, ref filedata_buff, uchan, n);
                    Console.WriteLine("Send: readed =" + GetConvertedNumber(readed) + " toread =" + GetConvertedNumber(toread) + " nread =" + GetConvertedNumber(nread) +
                        " remaining time =" + remainingtimes[n] + "s  throughput =" + GetConvertedNumber((long)throughputs[n]) + "/s");
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
                return (true);
            }
            catch (NullReferenceException e) { Console.WriteLine(e.Message); throw e; }
            catch (IndexOutOfRangeException e) { Console.WriteLine(e.Message); throw e; }
            catch (OutOfMemoryException e) { Console.WriteLine(e.Message); throw e; }
            catch (ObjectDisposedException e) { Console.WriteLine(e.Message); throw e; }
            catch (ArgumentNullException e) { Console.WriteLine(e.Message); throw e; }
            catch (ArgumentOutOfRangeException e) { Console.WriteLine(e.Message); throw e; }
            catch (IOException e) { Console.WriteLine(e.Message); throw e; }
            catch (NotSupportedException e) { Console.WriteLine(e.Message); throw e; }
            catch (Exception e) { Console.WriteLine(e.Message); throw e; }
        }

        private bool SendNumfile(ref Socket clientsocket, UploadChannel uchan)
        {
            var numfile = uchan.Get_num_trasf();
            var bufferOut = new byte[FTPsupporter.Sizes.Numfilesize];
            bufferOut = BitConverter.GetBytes(numfile);

            var sended = Send(ref clientsocket, ref bufferOut, null, 0);
            if (sended != FTPsupporter.Sizes.Numfilesize)
            {
                throw new IOException("Cannot send number of files");
            }
            return (true);
        }

        private bool SendElement(ref Socket clientsocket, string element)
        {
            try
            {
                var elementlen_buff = BitConverter.GetBytes(element.Length);
                var element_buff = Encoding.ASCII.GetBytes(element);
                var bufferOut = new byte[FTPsupporter.Sizes.Elementlensize + element.Length];

                elementlen_buff.CopyTo(bufferOut, 0);
                element_buff.CopyTo(bufferOut, FTPsupporter.Sizes.Elementlensize);

                var sended = Send(ref clientsocket, ref bufferOut, null, 0);
                if (sended != (FTPsupporter.Sizes.Elementsizesize + element.Length))
                {
                    throw new IOException("Cannot send element");
                }
                return (true);
            }
            catch (SocketException e) { Console.WriteLine(e.Message); throw e; }

        }

        private bool SendDepth(ref Socket clientsocket, int depth)
        {
            try { 
            var depth_buff = BitConverter.GetBytes(depth);
            var sended = Send(ref clientsocket, ref depth_buff, null, 0);
            if (sended != FTPsupporter.Sizes.Depthsize)
            {
                throw new IOException("Cannot send depth");
            }
            return (true);
            }
            catch (SocketException e) { Console.WriteLine(e.Message); throw e; }
        }

        private bool SendNumele(ref Socket clientsocket, int numele)
        {
            try { 
            var numele_buff = BitConverter.GetBytes(numele);
            var sended = Send(ref clientsocket, ref numele_buff, null, 0);
            if (sended != FTPsupporter.Sizes.Numelementsize)
            {
                throw new IOException("Cannot send number of elements");
            }
            return (true);
            }
            catch (SocketException e) { Console.WriteLine(e.Message); throw e; }
        }

        private bool SendTotNFiles(ref Socket clientsocket, long numfiles)
        {
            try { 
            var numele_buff = BitConverter.GetBytes(numfiles);
            var sended = Send(ref clientsocket, ref numele_buff, null, 0);
            if (sended != FTPsupporter.Sizes.Filesizesize)
            {
                throw new IOException("Cannot send number of elements");
            }
            return (true);
            }
            catch (SocketException e) { Console.WriteLine(e.Message); throw e; }
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

        private bool RefreshCannels(UploadChannel olduchan)
        {

            Console.WriteLine("\n\nUpload channel end details :\n\n");
            if (olduchan == null) { return (false); }
            var filenames = olduchan.Get_filenames();
            var filesizes = olduchan.Get_filesizes();
            var sended = olduchan.Get_sended();
            var throughputs = olduchan.Get_throughputs();
            for (var i = 0; i < olduchan.Get_num_trasf(); i++)
            {
                Console.WriteLine(i + " - filename " + filenames[i] + "  filesize " + GetConvertedNumber(filesizes[i]) + "\n sended " + GetConvertedNumber(sended[i]) + " throughput " + GetConvertedNumber((long)throughputs[i]) + "/s\n\n");
            }
            mUchansDataLock.WaitOne();

            if (!mUchans.Remove(olduchan))
            {
                mUchansDataLock.ReleaseMutex();
                throw new IOException("Channels Unrefreshable");
            }

            mUchansDataLock.ReleaseMutex();
            return (true);

        }

        private void FTPTrees(string[] paths, IPAddress ipaddress, ref Socket clientsocket, UTransfer transfer, ManualResetEvent uTransferAvailability, ManualResetEvent windowAvailability)
        {
            try {
                var multifile = false;
                if (paths.Length > 1) { multifile = true; }

                if (!multifile)
                {
                    FTPsingleClientTree_serial(clientsocket, ipaddress, paths[0], transfer, uTransferAvailability, windowAvailability);
                }
                else
                {
                    FTPsingleClientMultiTree_serial(clientsocket, ipaddress, paths, transfer, uTransferAvailability, windowAvailability);
                }
                return;
            }
            catch (SocketException e) { Console.WriteLine(e.Message); throw e; }
            catch (Exception e) { Console.WriteLine(e.Message); throw e; }
        }

        private int GetToS(string[] dirpaths, string[] filepaths)
        {
            if (dirpaths == null)
            {
                if (filepaths == null)
                {
                    return (FTPsupporter.Unknown.UnknownInt);
                }
                else
                {
                    return (FTPsupporter.ToSs.ToSfileonly);
                }
            }
            else
            {
                if (filepaths == null)
                {
                    return (FTPsupporter.ToSs.ToStreeonly);
                }
                else
                {
                    return (FTPsupporter.ToSs.ToSfileandtree);
                }
            }
        }

        private int Receive(ref Socket client, byte[] bufferIn)
        {
            var received = 0;
            var length = 0;

            try
            {

                length = bufferIn.Length;
                while (received < length)
                {
                    received += client.Receive(bufferIn, received, length - received, 0);
                }

                return (received);

            }
            catch (SocketException e) { Console.WriteLine(e.Message); throw e; }
            catch (Exception e) { throw e; }
        }

        private int RecvAck(ref Socket clientsocket)
        {
            try { 
            var bufferIn = new byte[FTPsupporter.Sizes.Tagsize];
            var received = Receive(ref clientsocket, bufferIn);
            if (received == 0)
            {
                return (FTPsupporter.Unknown.UnknownTag);
            }
            var tag = BitConverter.ToInt32(bufferIn, 0);
            return (tag);
            }
            catch (SocketException e) { Console.WriteLine(e.Message); throw e; }
            catch (Exception e) { Console.WriteLine(e.Message); throw e; }

        }

        private bool CheckAck(int ack)
        {
            if (ack != FTPsupporter.ProtocolAttributes.Ack)
            {
                return (false);
            }

            return (true);
        }

        public bool SendTotalBytes(ref Socket clientsocket, long n)
        {
            try { 
            var bufferOut = new byte[FTPsupporter.Sizes.Filesizesize];
            bufferOut = BitConverter.GetBytes(n);
            var sended = Send(ref clientsocket, ref bufferOut, null, 0);
            if (sended != FTPsupporter.Sizes.Filesizesize)
            {
                throw new IOException("Cannot send total bytes size");
            }
            return (true);
            }
            catch (SocketException e) { Console.WriteLine(e.Message); throw e; }
        }

        public void FTPsendAlltoClient(int tos, string[] directories, string[] files, IPAddress ipaddress, ref Socket clientsocket, ManualResetEvent uTransferAvailability, ManualResetEvent windowAvailability, UTransfer transfer)
        {
            try
            {
                Console.WriteLine("[MOVEX.FTP] [FTPclient.cs] [FTPsendAlltoClient] Trying to send data to client.");

                if (!SendTag(ref clientsocket, tos))
                {
                    return;
                }

                if (!SendTotalBytes(ref clientsocket, transfer.GetToTransfer()))
                {
                    return;
                }


                if (!SendTotNFiles(ref clientsocket, transfer.GetTotTransferToDo()))
                {
                    return;
                }


                //wait the ack
                var ack = RecvAck(ref clientsocket);
                if (!CheckAck(ack))
                {
                    return;
                }

                // Transfer started
                transfer.StartTransfer();
                Console.WriteLine("[MOVEX.FTP] [FTPclient.cs] [FTPsendAlltoClient] Send: Start Transfer with client " + clientsocket.RemoteEndPoint.ToString() + ".");

                if (directories != null)
                {
                    FTPTrees(directories, ipaddress, ref clientsocket, transfer, uTransferAvailability, windowAvailability);
                }

                if (files != null)
                {
                    FTPFiles(files, ipaddress, ref clientsocket, transfer, uTransferAvailability, windowAvailability);
                }

                //wait the ack
                ack = RecvAck(ref clientsocket);
                if (!CheckAck(ack))
                {
                    return;
                }

                // Transfer completed
                Console.WriteLine("[MOVEX.FTP] [FTPclient.cs] [FTPsendAlltoClient] Send: Connection with client " + clientsocket.RemoteEndPoint.ToString() + " ended succesfully.");
                WaitClient(ref clientsocket);
            }
            catch (SocketException Exception)
            {
                var Message = Exception.Message;
                var ipAddress = ipaddress.ToString();
                Console.WriteLine("[MOVEX.FTP] [FTPclient.cs] [FTPsendAlltoClient] " + Message + ".");

                var CriticalMessage = "Il trasferimento è stato interrotto.";
                mWindowRequester.RemoveUploadProgressWindow(ipAddress);
                mWindowRequester.AddMessageWindow(CriticalMessage);
            }
            catch (Exception Exception)
            {
                var Message = Exception.Message;
                var ipAddress = ipaddress.ToString();
                Console.WriteLine("[MOVEX.FTP] [FTPclient.cs] [FTPsendAlltoClient] " + Message + ".");

                var CriticalMessage = "Ci dispiace! Il trasferimento non è andato a buon fine.";
                mWindowRequester.RemoveUploadProgressWindow(ipAddress);
                mWindowRequester.AddMessageWindow(CriticalMessage);
            }
        }

        public void CollectData(int numclients, string[] paths, ref string[] directories, ref string[] files, ref int tos)
        {
            try
            {
                ExtractFilepathsAndDirpaths(paths, ref directories, ref files);
                mTransfer = GetTransferInterfaces(numclients, directories, files);
                tos = GetToS(directories, files);

            }
            catch (ThreadInterruptedException e) { Console.WriteLine(e.Message); throw e; }
            catch (ThreadAbortException e) { Console.WriteLine(e.Message); throw e; }
            catch (Exception Exception) {
                var Message = Exception.Message;
                Console.WriteLine("[MOVEX.VIEW] [FTPclient.cs] [CollectData] " + Message + ".");

                throw Exception;
            }
            return;
        }

        public bool FTPsendAll(string[] paths, IPAddress[] ipaddresses, ManualResetEvent[] WindowsAvailabilities, ManualResetEvent[] TransfersAvailabilities)
        {
           

            try
            {
                var numelements = paths.Length;
                var files = new string[numelements];
                var directories = new string[numelements];
                var numclients = ipaddresses.Length;
                Socket[] clientsockets = null;
                var nsendto = new Thread[numclients];
                var tos = 0;



                var collect = new Thread(new ThreadStart(() => CollectData(numclients, paths, ref directories, ref files, ref tos)))
                {
                    Priority = ThreadPriority.Highest,
                    Name = "CollectDataThread"
                };
                collect.Start();

                clientsockets = GetSocketsForClients(ipaddresses);

                collect.Join();


                for (var i = 0; i < numclients; i++)
                {
                    {
                        var index = i;
                        nsendto[index] = new Thread(new ThreadStart(() => FTPsendAlltoClient(tos, directories, files, ipaddresses[index], ref clientsockets[index], TransfersAvailabilities[index], WindowsAvailabilities[index], mTransfer[index])))
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
            catch (OutOfMemoryException e) { Console.WriteLine(e.Message); throw e; }
            catch (IndexOutOfRangeException e) { Console.WriteLine(e.Message); throw e; }
            catch (SocketException e) { Console.WriteLine(e.Message); throw e; }
            catch (ObjectDisposedException e) { Console.WriteLine(e.Message); throw e; }
            catch (Exception e) { Console.WriteLine(e.Message); throw e; }
        }

        private UTransfer[] GetTransferInterfaces(int numclients, string[] directories, string[] files)
        {
            // Declaring variable(s)
            UTransfer[] ntransfer;
            List<string> filepaths, fileFullPaths;
            long tot = 0;
            
            try
            {
                Console.WriteLine("[MOVEX.FTP] [FTPclient.cs] [GetTrasferInterfaces] Trying to get UploadInterfaces.");

                // VALIDATION: Check input correctness
                if (numclients <= 0)
                {
                    var Message = "Invalid argument passed.";
                    throw new Exception(Message);
                }

                ntransfer = new UTransfer[numclients];
                filepaths = new List<string>();
                fileFullPaths = new List<string>();

                if (directories != null) foreach (var root in directories) GetFileListInRoot(root, filepaths, fileFullPaths);
                if (files != null) fileFullPaths.AddRange(files);

                foreach (var fileFullPath in fileFullPaths) tot += new FileInfo(fileFullPath).Length;

                for (var i = 0; i < numclients; i++)
                {
                    ntransfer[i] = new UTransfer(fileFullPaths.Count, 0, tot);
                }

                return (ntransfer);
            }
            catch (Exception Exception)
            {
                var Message = Exception.Message;
                Console.WriteLine("[MOVEX.FTP] [FTPclient.cs] [GetTrasferInterfaces] " + Message + ".");

                throw Exception;
            }
        }
        public void SetSynchronization(ManualResetEvent requestAvailable, ConcurrentQueue<string> requests, ConcurrentDictionary<string, int> typeRequests, ConcurrentDictionary<string, string> messages, ConcurrentDictionary<string, ManualResetEvent[]> sync, ConcurrentDictionary<string, ConcurrentBag<string>> responses)
        {
            mWindowRequester = new WindowRequester(requestAvailable, requests, typeRequests, messages, sync, responses);
        }
        public void Shutdown()
        {
            Console.WriteLine("[Movex.FTP] [FTPclient.cs] [Shutdown] Shutting down the client.");

            mUchans.Clear();
            mWindowRequester.Dispose();
            mUchansDataLock.Dispose();
            for (var t = 0; t < mTransfer.Length; t++)
            {
                if (mTransfer[t] == null) { continue; }
                else
                {
                    mTransfer[t] = null;
                }
            }

            mUchans = null;
            mWindowRequester = null;
            mUchansDataLock = null;
            mTransfer = null;
        }
    }
}