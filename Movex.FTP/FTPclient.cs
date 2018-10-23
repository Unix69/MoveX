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





/*
 
TAG MAP :
          0 => send file single client 
          1 => directory tree
          2 => unused
          3 => send multi file single client
*/


namespace Movex.FTP
{
    public class FTPclient {
        //defining constants
        private List<UploadChannel> mUchans = new List<UploadChannel>();
        private List<UploadChannelInfo> mUchaninfos = new List<UploadChannelInfo>();
        private static List<UploadChannelInfo> StaticUchanInfos;
        
        // Synchronization primitives
        private ManualResetEvent mUchanAvailability;
        private ManualResetEvent mWindowAvailability;

        public List<UploadChannelInfo> GetUCInfo()
        {
            return StaticUchanInfos;
        }

        //costructor
        public FTPclient() {}

        //SUPPORT FTPmultiAll_t :

        // 1 - single client single file send used by multithread sending
        private bool FTPsendFile_l(Mutex socketlock, int index, IPAddress ipaddress)
        {
            /*
             Protocol :

           Sended by Client
                              4 byte           N byte             4 byte           K byte
                        byte(filenamelen) | byte(filename) | byte(filesize) | byte(filedata) |

            */
            try
            {

                
                var uchan = GetChannel(ipaddress.ToString());

                if (uchan == null || uchan.IsInterrupted())
                {
                    return (false);
                }

                 Console.WriteLine("\n filesend"+index);
                //send file with lock
                socketlock.WaitOne();
                var clientsocket = uchan.Get_socket();

                if (!SendFile(clientsocket, uchan, index)) {
                    socketlock.ReleaseMutex();
                    return (false);
                }

                socketlock.ReleaseMutex();


                return (true);
            }

            catch (Exception e) { return (false); }


        }

        // 2 - single file multi thread client used by multithread sending
        private bool FTPmultisend_t(Mutex[] socketlock, IPAddress[] ipaddresses, int n) {
            var numclients = ipaddresses.Length;
            var nsendto = new Thread[numclients];
            Console.WriteLine("\nmultisend"+n);
            try {
                for (var i = 0; i < numclients; i++) {
                    {
                        var index = i;
                        var uchan = GetChannel(ipaddresses[i].ToString());
                        var clientsocket = uchan.Get_socket();
                        var filename = uchan.Get_filenames()[n];
                        var filepath = uchan.Get_paths()[n];
                        nsendto[index] = new Thread(new ThreadStart(() => FTPsendFileWithLock(socketlock[index], n, ipaddresses[index])))
                        {
                            Priority = ThreadPriority.Highest
                        };
                        nsendto[index].Start();
                        uchan.Set_upload_thread(nsendto[index], index);
                    }
                }

                for (var i = 0; i < numclients; i++) {
                    nsendto[i].Join();
                }

                return (true);
            }
            catch (Exception e) { return (false); }



        }



        //WRAPPED :

        // 1 - FTPsend wrapped / threadable used by multithread sending
        private void FTPsendFileWithLock(Mutex socketlock, int index, IPAddress ipaddress)
        {
            try
            {
                var result = FTPsendFile_l(socketlock, index, ipaddress);
                return;
            }
            catch (Exception e) { return; }
        }

        // 2 - FTPmultiAll_t wrapped / threadable
        private void FTPmultiAll_thread(IPAddress[] ipaddress) {
            try
            {
                var result = FTPmultiAll_t(ipaddress);
                return;
            }
            catch (Exception e) { return; }
        }

        // 3 - FTPsinglesend wrapped / threadable
        private void FTPsingleFile_serial(IPAddress ipaddress)
        {

            try
            {
                var result = FTPsingleFile_s(ipaddress);
                return;
            }
            catch (Exception e) { return; }

        }

        // 4 - FTPmultisend_s wrapped / threadable
        private void FTPmultiFile_serial(IPAddress ipaddress)
        {
            try
            {
               var result = FTPmultiFile_s(ipaddress);
                return;
            }
            catch (Exception e) { return; }
        }

        // 5 - FTPmultiFile_t wrapped / threadable
        private void FTPmultiFile_thread(IPAddress ipaddress) {
            try
            {
                var result = FTPmultiFile_t(ipaddress);
                return;
            }
            catch (Exception e) { return; }


        }

        // 6 - FTPmultiClient_t wrapped / threadable
        private void FTPmultiClient_thread(IPAddress[] ipaddress)
        {
            try
            {
                var result = FTPmultiClient_t(ipaddress);
                return;
            }
            catch (Exception e) { return; }
        }

        // 7 - FTPmultiClient_s wrapped / threadable
        private void FTPmultiClient_serial(IPAddress[] ipaddress)
        {
            try
            {
                var result = FTPmultiClient_s(ipaddress);
                return;
            }
            catch (Exception e) { return; }


        }

        // 8 - FTPautonomus_s wrapped / threadable
        public void FTPautonomousSend(int priority, string[] paths, IPAddress[] ipaddress, ref Socket [] clientsockets)
        {
            try
            {
                var result = FTPsend(priority, paths, ipaddress, ref clientsockets);
                return;
            }
            catch (Exception e) { return; }


        }



        // 9 - FTPsingleclientTree wrapped / threadable
        private void FTPsingleClientTree_thread(Socket clientsocket, IPAddress ipaddress, string root) {
            try
            {
                var result = FTPsingleClientTree_t(clientsocket, ipaddress, root);
                return;
            }
            catch (Exception e) { return; }

        }

        // 10 - FTPsingleclientTree wrapped / threadable
        private void FTPsingleClientTree_serial(Socket clientsocket, IPAddress ipaddress, string root)
        {
            try
            {
                var result = FTPsingleClientTree_s(clientsocket, root, ipaddress);
                return;
            }
            catch (Exception e) { return; }

        }

        // 11 - FTPsendAllTree_t wrapped / threadable
        private void FTPmultiClientTree_thread(Socket[] clientsockets, IPAddress[] ipaddresses, string root) {
            try
            {
                var result = FTPmultiClientTree_t(clientsockets, ipaddresses, root);
                return;
            }
            catch (Exception e) { return; }


        }


        private void FTPmultiClientTree_serial(Socket[] clientsockets, IPAddress[] ipaddresses, string root)
        {
            try
            {
                var result = FTPmultiClientTree_s(clientsockets, ipaddresses, root);
                return;
            }
            catch (Exception e) { return; }


        }


        private void FTPmultiClientMultiTree_thread(Socket[] clientsockets, IPAddress[] ipaddresses, string[] roots)
        {
            try
            {
                var result = FTPmultiClientMultiTree_t(clientsockets, ipaddresses, roots);
                return;
            }
            catch (Exception e) { return; }


        }


        private void FTPmultiClientMultiTree_serial(Socket[] clientsockets, IPAddress[] ipaddresses, string[] roots)
        {
            try
            {
                var result = FTPmultiClientMultiTree_s(clientsockets, ipaddresses, roots);
                return;
            }
            catch (Exception e) { return; }


        }


        private void FTPsingleClientMultiTree_serial(Socket clientsocket, IPAddress ipaddress, string[] roots)
        {
            try
            {
                var result = FTPsingleClientMultiTree_s(clientsocket, ipaddress, roots);
                return;
            }
            catch (Exception e) { return; }


        }


        //MAIN FUNCTIONS : 

        // 1 - multi thread client multi thread file
        private bool FTPmultiAll_t(IPAddress[] ipaddresses)
        {
            try
            {
                var uchan0 = GetChannel(ipaddresses[0].ToString());
                var numfiles = uchan0.Get_num_trasf();
                var numclients = ipaddresses.Length;
                var socketlocks = new Mutex[numclients];
                var nsendto = new Thread[numfiles];

                for (var i = 0; i < numclients; i++)
                {
                    var uchan = GetChannel(ipaddresses[i].ToString());
                    var clientsocket = uchan.Get_socket();
                    if (!SendTag(clientsocket, FTPsupporter.Multifilesend)) { return (false); }
                    if (!SendNumfile(clientsocket, uchan)) { return (false); }
                    socketlocks[i] = new Mutex();
                }


                for (var i = 0; i < numfiles; i++)
                {
                    //send file
                    {
                        var index = i;
                        nsendto[index] = new Thread(new ThreadStart(() => FTPmultisend_t(socketlocks, ipaddresses, index)))
                        {
                            Priority = ThreadPriority.Highest
                        };
                        nsendto[index].Start();
                    }
                }

                for (var i = 0; i < numfiles; i++)
                {
                    nsendto[i].Join();
                }


                return (true);
            }
            catch (Exception e) { return (false); }

        }

        // 2 - single file single client
        private bool FTPsingleFile_s(IPAddress ipaddress) {


            try
            {
                var uchan = GetChannel(ipaddress.ToString());
                var clientsocket = uchan.Get_socket();
                var filename = uchan.Get_filenames()[0];
                var filepath = uchan.Get_paths()[0];

                if (!SendFile(clientsocket, uchan, 0)) { return (false); }
                return (true);
            }

            catch (Exception e) { return (false); }


        }

        // 3 - multi serial file single client
        private bool FTPmultiFile_s(IPAddress ipaddress) {
            try
            {
                var uchan = GetChannel(ipaddress.ToString());

                if (uchan == null) { return (false); }

                var ipend = new IPEndPoint(ipaddress, FTPsupporter.Port);
                var numfiles = uchan.Get_num_trasf();
                var filenames = uchan.Get_filenames();
                var paths = uchan.Get_paths();
                var clientsocket = uchan.Get_socket();



                uchan.Set_upload_thread(Thread.CurrentThread, 0);

                if (!SendTag(clientsocket, FTPsupporter.Multifilesend)) { return (false); }
                if (!SendNumfile(clientsocket, uchan)) { return (false); }


                for (var i = 0; i < filenames.Length; i++)
                {
                    if (!SendFile(clientsocket, uchan, i)) { return (false); }
                }

             

                return (true);
            }
            catch (Exception e) { return (false); }

        }

        // 4 - multi thread file single client
        private bool FTPmultiFile_t(IPAddress ipaddress) {
            var ipadresses = new IPAddress[1];
            ipadresses[0] = ipaddress;
            return (FTPmultiAll_t(ipadresses));
        }

        // 5 - single file multi thread client
        private bool FTPmultiClient_t(IPAddress[] ipaddress) {
            return (FTPmultiAll_t(ipaddress));
        }

        // 6 - single file multi serial client
        private bool FTPmultiClient_s(IPAddress[] ipaddresses)
        {
            try
            {
                var numclients = ipaddresses.Length;

                for (var i = 0; i < numclients; i++)
                {
                    var uchan = GetChannel(ipaddresses[i].ToString());
                    if (uchan == null) { return (false); }
                    uchan.Set_main_upload_thread(Thread.CurrentThread);
                }


                for (var i = 0; i < numclients; i++)
                {
                    var uchan = GetChannel(ipaddresses[i].ToString());
                    var clientsocket = uchan.Get_socket();
                    if (!SendFile(clientsocket, uchan, i)) { return (false); }

                }

           
                return (true);
            }
            catch (Exception e) { return (false); }


        }

        // 7 - wait some clients sending ends
        private void WaitClients(Socket [] clientsockets) {
            foreach (var clientsocket in clientsockets) {
                WaitClient(clientsocket);
            }
        }
        
        // 8 - wait one client sending end
        public void WaitClient(Socket clientsocket) {
            WaitUntilClose(clientsocket);
        }

        // 9 - all tree single client serial file
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

                if (!SendTag(clientsocket, FTPsupporter.Treesend)) { return (false); }
                if (!SendNumele(clientsocket, numelements)) { return (false); }

                for (var i = 0; i < numelements; i++)
                {

                    if (element[i].Contains(".")) { filenames[j++] = element[i]; }
                    if (!SendElement(clientsocket, element[i])) { return (false); }
                    if (!SendDepth(clientsocket, depth[i])) { return (false); }
                }

                var clientsockets = new Socket[1];
                clientsockets[0] = clientsocket;
                var ipaddresses = new IPAddress[1];
                ipaddresses[0] = ipaddress;

                var result = FTPsendbytree(100, filepaths.ToArray(), filenames, ipaddresses, ref clientsockets);
                result = true;
                return (result);
            }
            catch (Exception e) { return (false); }

        }

        // 10 - all tree single client multi thread file
        private bool FTPsingleClientTree_t(Socket clientsocket, IPAddress ipaddress, string root) {
            try
            {
                var ipend = new IPEndPoint(ipaddress, FTPsupporter.Port);
                var elements = new List<string>();
                var filepaths = new List<string>();
                var depths = new List<int>();
                var numelements = 0;
                int[] depth = null;
                string[] element = null;
                string[] files = null;
                var j = 0;
                var nfiles = GetAllForDirectoryTree(root, filepaths, elements, depths);

                var uchan = GetChannel(ipaddress.ToString());
                if (uchan == null) { return (false); }


                depth = depths.ToArray();
                element = elements.ToArray();
                numelements = element.Length;
                files = new string[nfiles];

                if (!SendTag(clientsocket, FTPsupporter.Treesend)) { return (false); }
                if (!SendNumele(clientsocket, numelements)) { return (false); }

                for (var i = 0; i < numelements; i++)
                {

                    if (!element[i].Contains(@"\")) { files[j++] = element[i]; }
                    if (!SendElement(clientsocket, element[i])) { return (false); }
                    if (!SendDepth(clientsocket, depth[i])) { return (false); }
                }

                var ipaddresses = new IPAddress[1];
                ipaddresses[0] = ipaddress;
                var result = true;
                //bool result = FTPsend(100 , files, ipaddresses);

                return (result);
            }
            catch (Exception e) { return (false); }

        }

        // 11 - all tree multi client multi thread file
        private bool FTPmultiClientTree_t(Socket[] clientsockets, IPAddress[] ipaddresses, string root)
        {
            try
            {

                var numclients = ipaddresses.Length;
                var nsendto = new Thread[numclients];


                for (var i = 0; i < numclients; i++)
                {
                    //send file
                    {
                        var index = i;
                        nsendto[index] = new Thread(new ThreadStart(() => FTPsingleClientTree_thread(clientsockets[index], ipaddresses[index], root)))
                        {
                            Priority = ThreadPriority.Highest
                        };
                        nsendto[index].Start();
                    }

                }


                for (var i = 0; i < numclients; i++)
                {
                    nsendto[i].Join();
                }
                
                for (var i = 0; i < numclients; i++)
                {
                    WaitUntilClose(clientsockets[i]);
                }


                return (true);
            }
            catch (Exception e) { return (false); }

        }


        // 12 - all tree multi client serial 
        private bool FTPmultiClientTree_s(Socket[] clientsockets, IPAddress[] ipaddresses, string root)
        {
            try
            {

                var numclients = ipaddresses.Length;
                var nsendto = new Thread[numclients];



                for (var i = 0; i < numclients; i++)
                {
                    var result = FTPsingleClientTree_s(clientsockets[i], root, ipaddresses[i]);

                }
                return (true);
            }
            catch (Exception e) { return (false); }

        }

        // 13 - all trees single client serial file 
        private bool FTPsingleClientMultiTree_s(Socket clientsocket, IPAddress ipaddress, string[] roots)
        {
            try
            {
                var ipend = new IPEndPoint(ipaddress, FTPsupporter.Port);
                var result = false;
                if (!SendTag(clientsocket, FTPsupporter.Multitreesend)) {
                    return (false);
                }
                if (!SendNumele(clientsocket, roots.Length)) {
                    return (false);
                }
                foreach (var root in roots)
                {
                    result = FTPsingleClientTree_s(clientsocket, root, ipaddress);
                }
                return (result);

            }
            catch (Exception e) { return (false); }

        }

        // 14 - multi thread client multi thread file
        private bool FTPmultiClientMultiTree_t(Socket[] clientsockets, IPAddress[] ipaddresses, string[] roots)
        {
            try
            {
                var numclients = ipaddresses.Length;
                var nsendto = new Thread[numclients];



                for (var i = 0; i < numclients; i++)
                {
                    //send file
                    {
                        var index = i;
                        nsendto[index] = new Thread(new ThreadStart(() => FTPsingleClientMultiTree_serial(clientsockets[index], ipaddresses[index], roots)))
                        {
                            Priority = ThreadPriority.Highest
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
            catch (Exception e) { return (false); }

        }

        // 15 - all trees multi client serial file
        private bool FTPmultiClientMultiTree_s(Socket[] clientsockets, IPAddress[] ipaddresses, string[] roots)
        {
            try
            {
                var result = false;
                var numclients = ipaddresses.Length;

                for (var i = 0; i < numclients; i++)
                {
                    result = FTPsingleClientMultiTree_s(clientsockets[i], ipaddresses[i], roots);
                }
                return (result);
            }
            catch (Exception e) { return (false); }

        }


        //SUPPORTING METHODS

        // 1 - get all filesizes of this file array (filepaths + filenames)
        private int[] GetFileSizes(string[] paths, string[] filenames)
        {
            var filesizes = new int[filenames.Length];
            for (var i = 0; i < filenames.Length; i++)
            {
                if (!File.Exists(paths[i] + @"\" + filenames[i]))
                {
                    return (null);
                }
                filesizes[i] = File.ReadAllBytes(paths[i] + @"\" + filenames[i]).Length;
            }
            return (filesizes);
        }

        // 2 - get sum of all filesizes of this file array (filepaths + filenames)
        private int GetTotFileSize(string[] paths, string[] filenames)
        {
            var totbytes = 0;
            for (var i = 0; i < filenames.Length; i++)
            {
                if (!File.Exists(paths[i] + filenames[i]))
                {
                    return (0);
                }
                totbytes += File.ReadAllBytes(paths[i] + filenames[i]).Length;
            }
            return (totbytes);
        }

        // 3 - wait a closing of socket
        private void WaitUntilClose(Socket clientsocket)
        {
            while (clientsocket.Connected) ;
            clientsocket.Close();
            return;

        }

        // 4 - return a CPU priority number 
        private int FTPcpu_priority(int priority, string[] paths, string[] filenames, IPAddress[] ipaddress) {

            if (priority > 100 || priority < 0)
            {
                return (0);
            }

            var urg = 0;
            var numclients = ipaddress.Length;
            var numfiles = filenames.Length;
            var filebytes = GetFileSizes(paths, filenames);
            var totbytes = GetTotFileSize(paths, filenames);
            const int normalizer_numfiles = 10;
            const int normalizer_filebytes = 1000000;
            var normalizer_medbytes = 100;

            urg += numfiles / normalizer_numfiles;

            for (var i = 0; i < numfiles; i++)
            {
                if (filebytes[i] > normalizer_filebytes)
                {
                    urg++;
                }
            }

            var bytes_med = totbytes / numfiles;

            if (bytes_med > normalizer_medbytes)
            {
                urg += numfiles;
            }


            if (numclients > 2)
            {
                urg += numclients * 10;
            }

            if (totbytes > numfiles * normalizer_medbytes)
            {
                urg += numfiles;
            }

            var max = (numfiles / normalizer_numfiles) + (numfiles * 3);
            var perc = (urg / max) * 100;

            var result = (perc + priority) / 2;

            return (result);

        }

        // 5 - setup of upload channels
        private List<UploadChannel> GetUploadChannels(ref Socket[] clientsockets, string[] paths, string[] filenames, IPAddress[] ipaddress, ref Thread mainthread, int ps) {
            var newUploadChannels = new List<UploadChannel>();
            var mainUploadThread = mainthread;
            var multiclient = (ipaddress.Length > 1 ? true : false);
            var multifile = (filenames.Length > 1 ? true : false);
            var filesizes = GetFileSizes(paths, filenames);
            var numclients = ipaddress.Length;
            var numfile = filenames.Length;
            var tag = (multifile ? FTPsupporter.Multifilesend : FTPsupporter.Filesend);

            for (var i = 0; i < numclients; i++) {
                var uchan = new UploadChannel(null, paths, ipaddress[i].ToString(), tag);
                uchan.Set_num_trasf(numfile);
                uchan.Set_socket(ref clientsockets[i]);
                uchan.Set_ps(ps);
                for (var j = 0; j < numfile; j++)
                {
                    uchan.Add_new_upload(filenames[j], filesizes[j]);
                }
                uchan.Set_main_upload_thread(mainUploadThread);
                newUploadChannels.Add(uchan);
                
                //lock require
                mUchans.Add(uchan);
                //lock release
            }
            return (newUploadChannels);
        }

        // 6 - get channel info from new uploadchannels for an interface
        private List<UploadChannelInfo> GetUploadChannelInfos(List<UploadChannel> newUploadChannels)
        {
            var end_index = 0;
            var newUploadChannelInfos = new List<UploadChannelInfo>();
            foreach (var uchan in newUploadChannels) {
                var n = uchan.Get_n_infos();
                for (var i = 0; i < n; i++)
                {
                    var curruchan = uchan;
                    var uchaninfo = new UploadChannelInfo(ref curruchan);
                    uchaninfo.Set_current_index(i);
                    

                    if (uchan.Get_ps() == FTPsupporter.Parallel)
                    {
                        end_index = i + 1;
                    }
                    else if (uchan.Get_ps() == FTPsupporter.Serial)
                    {
                        end_index = curruchan.Get_num_trasf() - 1;
                    }

                    uchaninfo.Set_end_index(end_index);
                    curruchan.Set_uchaninfo(ref uchaninfo);
                    newUploadChannelInfos.Add(uchaninfo);
                   
                   //lock require
                   mUchaninfos.Add(uchaninfo);
                   //lock release
                }
            }
            return (newUploadChannelInfos);
        }

        // 7 - setup socket with references and ipadresses
        private void SetSockets(ref Socket[] clientsockets, IPAddress[] ipaddresses) {
            for (var i = 0; i < ipaddresses.Length; i++) {
                clientsockets[i] = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                var ipend = new IPEndPoint(ipaddresses[i], FTPsupporter.Port);
                clientsockets[i].Connect(ipend);
            }
            return;
        }

        // 8 - extract files from filepaths
        private string [] ExtractFilesAndPaths(ref string[] paths) {
            var filenames = new string[paths.Length];
            var i = 0;
            foreach (var path in paths) {
                filenames[i] = ExtractCurrentFileFromPath(path);
                paths[i++] = ExtractPathFromFilePath(path);
            }
            return (filenames);
        }

        // 9 - extract filepaths and dirpaths
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

            // Check on the number of directories (if 0, dealloc, otherwise resize accordiglgy)
            if (ndirs == 0)
            {
                dirpaths = null;
            } else
            {
                Array.Resize(ref dirpaths, ndirs);
            }

            // Check on the number of paths (if 0, dealloc, otherwise resize accordiglgy)
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

        // 10 - socket getter (use setsocket)
        private Socket[] GetSocketsForClients(IPAddress[] ipaddresses) {
            var numclients = ipaddresses.Length;
            var clientsockets = new Socket[numclients];
            SetSockets(ref clientsockets, ipaddresses);
            return (clientsockets);
        }

        
        // 11 - call the best thread for the users request FTP transfer
        private bool FTPsend(int priority, string [] paths, IPAddress[] ipaddresses, ref Socket [] clientsockets) {
                Thread ftp_thread = null;
                var newUploadChannels = new List<UploadChannel>();
                var newUploadChannelInfos = new List<UploadChannelInfo>();
                var ps = -1;
                var filenames = ExtractFilesAndPaths(ref paths);

            ps = FTPset_thread(priority, ref ftp_thread, paths, filenames, ipaddresses);
            newUploadChannels = GetUploadChannels(ref clientsockets, paths, filenames, ipaddresses, ref ftp_thread, ps);
            newUploadChannelInfos = GetUploadChannelInfos(newUploadChannels);
            StaticUchanInfos = newUploadChannelInfos;

            // This synchronization primitive say: Get the UchanInfo. It is now available
            /*  mUchanAvailability.Set();

              // This synchornization primitive wait until Window(s) available
              mWindowAvailability.WaitOne();
              */
            FTPstartAndWaitThread(ref ftp_thread);

            if (!RefreshCannels(newUploadChannels, newUploadChannelInfos)) {return (false);}

            return (true);

            }

        // 12 - thread managment function
        private void FTPstartAndWaitThread(ref Thread ftp_thread) {
            ftp_thread.Priority = ThreadPriority.Highest;
            ftp_thread.Start();

            for (var i=0; i<10; i++)
            {
                Thread.Sleep(1);
            }

            ftp_thread.Join();
            return;
        }

        // 13 - call the best thread for the users request FTP transfer
        private bool FTPsendbytree(int priority, string[] paths, string[] filenames, IPAddress[] ipaddresses,ref Socket [] connectedsockets)
        {
            Thread ftp_thread = null;
            var newUploadChannels = new List<UploadChannel>();
            var newUploadChannelInfos = new List<UploadChannelInfo>();
            var clientsockets = connectedsockets;
            var ps = -1;
            ps = FTPset_thread(priority, ref ftp_thread, paths, filenames, ipaddresses);
            newUploadChannels = GetUploadChannels(ref clientsockets, paths, filenames, ipaddresses, ref ftp_thread, ps);
            newUploadChannelInfos = GetUploadChannelInfos(newUploadChannels);
            StaticUchanInfos = newUploadChannelInfos;

            ftp_thread.Priority = ThreadPriority.Highest;
            ftp_thread.Start();

            // This synchronization primitive say: Get the UchanInfo. It is now available
         //   mUchanAvailability.Set();

            // This synchornization primitive wait until Window(s) available
           // mWindowAvailability.WaitOne();

            ftp_thread.Join();

            if (!RefreshCannels(newUploadChannels, newUploadChannelInfos)) { return (false); }
            return (true);

        }

     
     
        

        //9
        private int FTPset_thread(int priority,ref Thread ftp_thread, string [] path, string[] filenames, IPAddress[] ipaddresses) {
            var multiclient = false;
            var multifile = false;
            var ps = 0;


            var ftp_priority = 50;
            if (ipaddresses.Length > 1){multiclient = true;}
            if (filenames.Length > 1){multifile = true;}

             
            if ((!multiclient) && (!multifile))
            {
                ftp_thread = new Thread(() => FTPsingleFile_serial(ipaddresses[0]));
                ps = FTPsupporter.Serial;
            }
            else
            {
                if ((!multiclient) && (multifile))
                {
                    ftp_thread = (ftp_priority < FTPsupporter.Default_priority_limit ? new Thread(() => FTPmultiFile_serial(ipaddresses[0])) : new Thread(() => FTPmultiFile_thread(ipaddresses[0])));
                    ps = (ftp_priority < FTPsupporter.Default_priority_limit ? FTPsupporter.Serial : FTPsupporter.Parallel);
                }
                else if ((multiclient) && (!multifile))
                {
                    ftp_thread = (ftp_priority < FTPsupporter.Default_priority_limit ? new Thread(() => FTPmultiClient_serial(ipaddresses)) : new Thread(() => FTPmultiClient_thread(ipaddresses)));
                    ps = (ftp_priority < FTPsupporter.Default_priority_limit ? FTPsupporter.Serial : FTPsupporter.Parallel);
                }
                else
                {
                    ftp_thread = new Thread(() => FTPmultiAll_thread(ipaddresses));
                    ps = FTPsupporter.Parallel;
                }
            }
            return (ps);
        }

        // 10 - get an upload channel with specified path
        private UploadChannel GetChannel(string ipaddress)
            {
                foreach (var uchan in mUchans)
                {
                    if (uchan.Get_to().ToString().Equals(ipaddress))
                    {
                        return (uchan);
                    }

                }

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

        // 12 - get all parameter of directory tree
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

        // 13 - get recursivly all directories and files 
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

            // Go through each path
            foreach (var path in paths)
                {
                    // Check if the filePath is a directory
                    if (File.GetAttributes(path).HasFlag(FileAttributes.Directory))
                    {
                        var element = ExtractCurrentDirFromPath(path);
                        elements.Add(element);
                        depths.Add(depth);

                        // Follow the path recursively
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

        // 15 - get all paths of files inside a directory and subdirectory
        private void GetPathOfAllFile(string[] paths, List<string> filepaths)
            {
                // Go through each path
                foreach (var path in paths)
                {
                    // Check if the filePath is a directory
                    if (File.GetAttributes(path).HasFlag(FileAttributes.Directory))
                    {
                        // Get the an array of files and subfolders
                        var pathsArray = Directory.GetFileSystemEntries(path);

                        // Convert to list by default constructors and fill an Iterator
                        var list = new List<string>(pathsArray);
                        var subFolderPaths = list.ToArray();

                        // Follow the path recursively
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

        // 16 - send a data buffer while set uchans parameters on the specific file (uploadindex)
        private int Send(Socket clientsocket, byte[] bufferOut, UploadChannel uchan, int uploadindex) {
            var length = 0;
            var sended = 0;
            if (uchan == null)
            {
                length = bufferOut.Length;
                sended = clientsocket.Send(bufferOut, 0, length, 0);
                return (sended);
            }

            if (!clientsocket.Connected || uchan.IsInterrupted())
            {
                uchan.InterruptUpload();
                return (0);
            }
            length = bufferOut.Length;
            sended = clientsocket.Send(bufferOut, 0, length, 0);
            if (sended == 0)
            {
                uchan.InterruptUpload();
            }
            else {
                uchan.Incr_sended_p(uploadindex, length);
            }
            return (sended);
        }

        // 17 - set UploadChannel in interrupted mode
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
            var bufferOut = new byte[FTPsupporter.Tagsize];
            bufferOut = BitConverter.GetBytes(tag);

            var sended = Send(clientsocket, bufferOut, null, 0);
            if (sended != FTPsupporter.Tagsize)
            {
                return (false);
            }
            return (true);
        }

        private bool SendFile(Socket clientsocket, UploadChannel uchan, int n)
        {
            byte[] bufferOut;

            var tag = FTPsupporter.Filesend;
            if (!SendTag(clientsocket, tag))
            {
                return (false);
            }

          
            var filename = uchan.Get_filenames()[n];
            var filesize = uchan.Get_filesizes()[n];
            var path = uchan.Get_paths()[n];

            var uchaninfo = uchan.GetUploadChannelInfo(filename);


            var filenamelen_buff = new byte[filename.Length];
            var filename_buff = new byte[filename.Length];
            var filesize_buff = new byte[filename.Length];
            var filedata_buff = new byte[filename.Length];

            filenamelen_buff = BitConverter.GetBytes(filename.Length);
            filename_buff = Encoding.ASCII.GetBytes(filename);
            filedata_buff = File.ReadAllBytes(path + filename);
            filesize_buff = BitConverter.GetBytes(filedata_buff.Length);

            bufferOut = new byte[FTPsupporter.Filenamelensize + filename.Length + FTPsupporter.Filesizesize];
            filenamelen_buff.CopyTo(bufferOut, 0);
            filename_buff.CopyTo(bufferOut, FTPsupporter.Filenamelensize);
            filesize_buff.CopyTo(bufferOut, FTPsupporter.Filenamelensize + filename_buff.Length);

            var sended = Send(clientsocket, bufferOut, uchan, 0);
            if (sended != FTPsupporter.Filenamelensize + filename.Length + FTPsupporter.Filesizesize)
            {
                return (false);
            }

            bufferOut = new byte[filedata_buff.Length];

            filedata_buff.CopyTo(bufferOut, 0);

            sended = Send(clientsocket, bufferOut, uchan, n);
            if (sended != filedata_buff.Length)
            {
                return (false);
            }

            uchaninfo.Switch_upload();

            return (true);
        }        

        private bool SendNumfile(Socket clientsocket, UploadChannel uchan)
        {
            var numfile = uchan.Get_num_trasf();
            var bufferOut = new byte[FTPsupporter.Numfilesize];
            bufferOut = BitConverter.GetBytes(numfile);

            var sended = Send(clientsocket, bufferOut, null, 0);
            if (sended != FTPsupporter.Numfilesize)
            {
                return (false);
            }
            return (true);
        }

        private bool SendElement(Socket clientsocket, string element)
        {
         
            var elementlen_buff = BitConverter.GetBytes(element.Length);
            var element_buff = Encoding.ASCII.GetBytes(element);
            var bufferOut = new byte[FTPsupporter.Elementlensize + element.Length];

            elementlen_buff.CopyTo(bufferOut, 0);
            element_buff.CopyTo(bufferOut, FTPsupporter.Elementlensize);

            var sended = Send(clientsocket, bufferOut, null, 0);
            if (sended != (FTPsupporter.Elementsizesize + element.Length))
            {
                return (false);
            }
            return (true);
            
        }

        private bool SendDepth(Socket clientsocket, int depth)
        {
            var depth_buff = BitConverter.GetBytes(depth);
            var sended = Send(clientsocket, depth_buff, null, 0);
            if (sended != FTPsupporter.Depthsize){
                return (false);
            }
            return (true);
        }

        private bool SendNumele(Socket clientsocket, int numele)
        {
            var numele_buff = BitConverter.GetBytes(numele);
            var sended = Send(clientsocket, numele_buff, null, 0);
            if (sended != FTPsupporter.Numelementsize){ //interrupt sending
                return (false);
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

    
         public bool FTPclose(){
       
            return(true);
        }

       

       

        private bool RefreshCannels(List<UploadChannel> olduchans, List<UploadChannelInfo> olduchaninfos)
        {
            //lock require
            foreach (var olduchan in olduchans)
            {
                if (!mUchans.Remove(olduchan)) { return (false); }
            }

            foreach (var olduchaninfo in olduchaninfos)
            {
                if (!mUchaninfos.Remove(olduchaninfo)) { return (false); }
            }
            //lock release
            return (true);

        }

        private void FTPautonomousTreeSend(int priority, string[] paths, IPAddress[] ipaddresses, ref Socket[] clientsockets)
        {
            
                try
                {
                    var result = FTPTreesend(priority, paths, ref clientsockets, ipaddresses);
                    return;
                }
                catch (Exception e) { return; }
            
        }

        private int FTPcpu_priority(int priority, string[] paths, IPAddress[] ipaddresses)
        {

            if (priority > 100 || priority < 0)
            {
                return (0);
            }

            var numpaths = paths.Length;
            var numclients = ipaddresses.Length;
            var urg = 0;


            if (numclients > 2)
            {
                urg += numclients * 10;
            }

            if (numpaths > 2)
            {
                urg += numpaths * 5;
            }

            var max = (numclients * 10) + (numpaths * 5);
            var perc = (urg / max) * 100;

            var result = (perc + priority) / 2;

            return (result);

        }

        private Thread FTPget_thread(int priority, string[] paths, IPAddress[] ipaddresses, Socket [] clientsockets)
        {
            var multiclient = false;
            var multifile = false;
            var ftp_priority = 0;
            Thread ftp_thread = null;
            if (ipaddresses.Length > 1) { multiclient = true; }
            if (paths.Length > 1) { multifile = true; }

            ftp_priority = FTPcpu_priority(priority, paths, ipaddresses);

            if ((!multiclient) && (!multifile))
            {
                ftp_thread = new Thread(() => FTPsingleClientTree_serial(clientsockets[0], ipaddresses[0], paths[0]));
            }
            else
            {
                if ((!multiclient) && (multifile))
                {
                    ftp_thread = new Thread(() => FTPsingleClientMultiTree_serial(clientsockets[0], ipaddresses[0], paths));
                }
                else if ((multiclient) && (!multifile))
                {
                    ftp_thread = new Thread(() => FTPmultiClientTree_thread(clientsockets, ipaddresses, paths[0]));
                    ftp_thread = (ftp_priority > FTPsupporter.Default_priority_limit ? new Thread(() => FTPmultiClientTree_serial(clientsockets, ipaddresses, paths[0])) : new Thread(() => FTPmultiClientTree_thread(clientsockets, ipaddresses, paths[0])));

                }
                else
                {
                    ftp_thread = (ftp_priority > FTPsupporter.Default_priority_limit ? new Thread(() => FTPmultiClientMultiTree_serial(clientsockets, ipaddresses, paths)) : new Thread(() => FTPmultiClientMultiTree_thread(clientsockets, ipaddresses, paths)));
                }
            }
            return (ftp_thread);
    }

        private bool FTPTreesend(int priority, string [] directories, ref Socket [] clientsockets, IPAddress [] ipaddresses) {
            try {
                var ftp_thread = FTPget_thread(priority, directories, ipaddresses, clientsockets);
                FTPstartAndWaitThread(ref ftp_thread);
                return (true);
            }
            catch (Exception e) { return (false); }
        }

        private int GetToS(string [] dirpaths, string [] filepaths){
            if(dirpaths == null){
               if(filepaths == null){
                  return(FTPsupporter.UnknownInt);
               }
               else{
                  return(FTPsupporter.ToSfileonly);
               } 
            }
            else{
               if(filepaths == null){
                 return(FTPsupporter.ToStreeonly);
               }
               else{
                 return(FTPsupporter.ToSfileandtree);
               }
            }
        }

        public bool FTPsendAll(string[] paths, IPAddress[] ipaddresses, int priority, ManualResetEvent uchanAvailability, ManualResetEvent windowAvailability) {
            var numelements = paths.Length;
            var files = new string[numelements];
            var directories = new string[numelements];
            Socket [] clientsockets = null;
            var tos = 0;
            mUchanAvailability = uchanAvailability;
            mWindowAvailability = windowAvailability;

            try
            {
                ExtractFilepathsAndDirpaths(paths, ref directories, ref files);
                clientsockets = GetSocketsForClients(ipaddresses);
                tos = GetToS(directories, files);
                
                if(!SendToS(clientsockets, tos)){
                    return(false);
                }
                
                if (directories != null) {
                    FTPTreesend(priority, directories, ref clientsockets, ipaddresses);
                }
                
                if (files != null) { 
                    FTPsend(priority, files, ipaddresses, ref clientsockets);
                }
                FTPclose();
                WaitClients(clientsockets);
              
            }
            
            catch (Exception e) { return (false); }
            return (true);
        }

        private bool SendToS(Socket [] clientsockets, int tag){
               try{
                   foreach(var s in clientsockets)
                   {
                     SendTag(s, tag);
                  }
                   return(true);
               }
               catch(Exception e){return(false);}

        }





























































    }
}














































