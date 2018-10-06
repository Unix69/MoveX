using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.IO;
using System.Net.Sockets;

/*

     dopo la chiamata a add_new_download
     
    
     Download Channel =>   from  => IP
                           filenames => f1.txt, f2.jpg, f3.exe 
                           received => 0bytes, 0bytes, 0bytes
                           filesizes => 2kbytes, 1Mbytes, 850kbytes
                           multi_single => multi
                           num_trasf => 3
                           index => 3
                           path => usr/home/downloads
                           download_thread => threadptr
     
     
     
     */


namespace Movex.FTP
{
   public class DownloadChannel
    {

        private string mFrom;
        private string [] mFilenames;
        private string[] mFilepaths;
        private Thread [] mDownload_thread;
        private Thread mMain_download_thread;
        private int [] mFilesizes;
        private int [] mReceived;
        private double[] mThroughputs;
        private long[] mStart_time_millisec;
        private long[] mRemaining_time_millisec;
        private int mIndex;
        private int mNum_trasf;
        private int mMulti_single;
        private string mPath;
        private Mutex mMutex;
        private bool mInterrupted;
        private Socket mSocket;
        private List<DownloadChannelInfo> mDchaninfos;
        private int mPs;
        private int mN_infos;
        //costructor

        public DownloadChannel(string from, int multi_single, string path) {
            mFrom = from;
            mMulti_single = multi_single;
            mPath = path;
            mMutex = new Mutex();
            mIndex = 0;
            mInterrupted = false;
            mDchaninfos = new List<DownloadChannelInfo>();
        }

        public void Set_filepaths(string [] filepaths) {
            mFilepaths = filepaths;
        }


        public string [] Get_filepaths()
        {
            return(mFilepaths);
        }


        //creater
        public void Set_ps(int ps)
        {
            var parallel = FTPsupporter.Parallel;
            var serial = FTPsupporter.Serial;

            if (ps == serial)
            {
                mN_infos = ps;
            }
            else if (ps == parallel)
            {
                mN_infos = mNum_trasf;
            }
            mPs = ps;
        }

        public int Get_n_infos()
        {
            return (mN_infos);
        }


        public void Set_main_download_thread(Thread main_download_thread)
        {
            mMain_download_thread = main_download_thread;
            return;
        }

        public void Set_dchaninfo(ref DownloadChannelInfo dchaninfo)
        {
            mDchaninfos.Add(dchaninfo);
        }

        private void CreateReceiveStructure(int num_trasf) {
            
            mFilesizes = new int[num_trasf];
            mFilenames = new string[num_trasf];
            mFilepaths = new string[num_trasf];
            mReceived = new int[num_trasf];
            mThroughputs = new double[num_trasf];
            mStart_time_millisec = new long[num_trasf];
            mRemaining_time_millisec = new long[num_trasf];
            mDownload_thread = new Thread[num_trasf];
            for (var i = 0; i < num_trasf; i++)
            {
                mFilesizes[i] = 0; mReceived[i] = 0;
                mFilenames[i] = "";mFilepaths[i] = mPath;
            }
            mIndex = 0;
            return;
        }


        //Setter

        public void Set_num_trasf(int num_trasf) {
            mNum_trasf = num_trasf;
            CreateReceiveStructure(num_trasf);
            return;
        }

        public void Set_from(string from)
        {
            mFrom = from;
            return;
        }

        public void Set_path(string path)
        {
            mPath = path;
            return;
        }

        public void Set_multi_single(int multi_single)
        {
            mMulti_single = multi_single;
            return;
        }

        public void Set_download_thread(Thread download_thread, int index)
        {
            mDownload_thread[index] = download_thread;
            return;
        }

        public void Set_download_thread(Thread download_thread,string filename)
        {
            mDownload_thread[IndexOf(filename)] = download_thread;
            return;
        }

        public void Set_mutex(Mutex mutex) {
            mMutex = mutex;
        }

        public Socket Get_socket()
        {
            return (mSocket);
        }

        //getter

        public int Get_num_trasf()
        {
            return(mNum_trasf);
        }

        public string Get_from()
        {
            return(mFrom);
        }

        public string Get_path()
        {
            return(mPath);
        }

        public int Get_multi_single()
        {
            return(mMulti_single);
        }

        public Thread Get_download_thread(string filename)
        {
            return (mDownload_thread[IndexOf(filename)]);
        }

        public Thread Get_download_thread(int index)
        {
            return (mDownload_thread[index]);
        }

        public int [] Get_received() {
            return (mReceived);
        }

        public int [] Get_filesizes() {
            return (mFilesizes);
        }

        public string [] Get_filenames() {
            return (mFilenames);
        }

        public Mutex Get_Mutex()
        {
            return (mMutex);
        }

        public int Get_ps()
        {
            return (mPs);
        }

        public Thread Get_main_download_thread()
        {
            return (mMain_download_thread);
        }


        public void Set_socket(ref Socket socket)
        {
            mSocket = socket;
            return;
        }

        //additional function

        public void InterruptDownload()
        {
            mInterrupted = true;
        }

        public bool IsInterrupted()
        {
            return (mInterrupted);
        }


        public void Add_new_download(string filename, int filesize)
        {

            mFilenames[mIndex] = filename;
            mStart_time_millisec[mIndex] = DateTimeOffset.Now.Ticks / TimeSpan.TicksPerMillisecond;
            mFilesizes[mIndex++] = filesize;

            return;
        }

        public void Incr_received_p(int index, int incr)
        {
            var var_milliseconds = (long)(DateTimeOffset.Now.Ticks / TimeSpan.TicksPerMillisecond);
            mReceived[index] += incr;
            mThroughputs[index] = (double) mReceived[index] / (var_milliseconds - mStart_time_millisec[index]);
            mRemaining_time_millisec[index] = (long)((mFilesizes[index] - mReceived[index]) / mThroughputs[index]);
        }

        public int IndexOf(string filename) {
            for (var i = 0; i < mNum_trasf; i++)
            {
                if (mFilenames[i].Equals(filename)){
                    return (i);
                }
            }
            return (-1);
        }

        public void IncrReceivedOf(string filename, int incr) {
            Incr_received_p(IndexOf(filename), incr);
        }

        public int GetTotReceived()
        {
            var TotReceived = 0;
            for (var i = 0; i < mNum_trasf; i++)
            {
                TotReceived += mReceived[i];
            }
            return (TotReceived);
        }

        public int GetTotFilesize()
        {
            var TotFilesize = 0;
            for (var i = 0; i < mNum_trasf; i++)
            {
                TotFilesize += mFilesizes[i];
            }
            return (TotFilesize);
        }

        public int GetCompleteTransfer()
        {
            var completetransfer = 0;
            for (var i = 0; i < mNum_trasf; i++)
            {
                if (mFilesizes[i] == mReceived[i]) {
                    completetransfer ++;
                }
            }
            return (completetransfer);
        }

        public int GetPrevIndex() {
            return (mIndex - 1);
        }

        public int GetNextIndex() {
            return (mIndex);
        }

        public float GetPercOfTransfer(string filename)
        {
            var index = IndexOf(filename);
            if (index == -1) {
                return (-1);
            }
            var toreceive = (float) mFilesizes[index];
            var received = (float) mReceived[index];
            return ((received * 100) / toreceive);

        }

        public float GetPerc() {
            float receive = 0;
            float toreceive = 0;
            for (var i = 0; i < mNum_trasf; i++)
            {
                    toreceive += (float) mFilesizes[i];
                    receive += (float) mReceived[i];
            }
            return ((receive*100)/toreceive);
        }

        public double[] Get_throughputs()
        {
            return (mThroughputs);
        }

        public long[] Get_remaining_times()
        {
            return (mRemaining_time_millisec);
        }

        public DownloadChannelInfo GetDownloadChannelInfo(string filename)
        {
            foreach (var dchaninfo in mDchaninfos)
            {
                if (dchaninfo.Get_current_filename().Equals(filename))
                {
                    return (dchaninfo);
                }
            }
            return (null);
        }

        public List<DownloadChannelInfo> GetDownloadChannelInfos()
        {
            return (mDchaninfos);
        }
        public void Set_filenames(string []filenames) {
            mFilenames = filenames;
            return;
        }

       public DownloadChannel(Socket socket, string[] paths, string from, int multi_single)
        {
            mFilepaths = paths;
            mFrom = from;
            mN_infos = 0;
            mMulti_single = multi_single;
            mIndex = 0;
            mSocket = socket;
            mInterrupted = false;
        }


      
        }
    }

