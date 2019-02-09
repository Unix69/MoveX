using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.IO;
using System.Net.Sockets;
using System.Security;

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
        private Thread mMain_download_thread;
        private long [] mFilesizes;
        private long [] mReceived;
        private double[] mThroughputs;
        private long[] mStart_time_millisec;
        private long[] mCurrent_time_millisec;
        private long[] mRemaining_time_millisec;
        private int mIndex;
        private int mNum_trasf;
        private int mMulti_single;
        private string mPath;
        private int mIndexCurrentTransfer;
        private bool mInterrupted;
        private Socket mSocket;
        //costructor

        public DownloadChannel(string from, int multi_single, string path) {
            mFrom = from;
            mMulti_single = multi_single;
            mPath = path;
            mIndex = 0;
            mIndexCurrentTransfer = 0;
            mInterrupted = false;
        }

        public void Set_filepaths(string [] filepaths) {
            mFilepaths = filepaths;
        }


        public string [] Get_filepaths()
        {
            return(mFilepaths);
        }

        public void Set_main_download_thread(Thread main_download_thread)
        {
            mMain_download_thread = main_download_thread;
            return;
        }

       
        private void CreateReceiveStructure(int num_trasf) {
            try
            {
                mFilesizes = new long[num_trasf];
                mFilenames = new string[num_trasf];
                mFilepaths = new string[num_trasf];
                mReceived = new long[num_trasf];
                mThroughputs = new double[num_trasf];
                mStart_time_millisec = new long[num_trasf];
                mCurrent_time_millisec = new long[num_trasf];
                mRemaining_time_millisec = new long[num_trasf];
                for (var i = 0; i < num_trasf; i++)
                {
                    mFilesizes[i] = 0; mReceived[i] = 0;
                    mStart_time_millisec[i] = 0;
                    mCurrent_time_millisec[i] = 0;
                    mThroughputs[i] = 0;
                    mFilenames[i] = ""; mFilepaths[i] = mPath;
                }
                mIndex = 0;
                return;
            }
            catch (NullReferenceException e) { Console.WriteLine(e.Message); throw e; }
            catch (OutOfMemoryException e) { Console.WriteLine(e.Message); throw e; }
            catch (IndexOutOfRangeException e) { Console.WriteLine(e.Message); throw e; }
            catch (Exception e) { Console.WriteLine(e.Message); throw e; }
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

        
        public long [] Get_received() {
            return (mReceived);
        }

        public long [] Get_filesizes() {
            return (mFilesizes);
        }

        public string [] Get_filenames() {
            return (mFilenames);
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
            try
            {
                Console.WriteLine("[Movex.FTP] [DownloadChannel.cs] [InterruptDownload] Trying to interrupt download.");
                try
                {
                    if (mSocket != null)
                    {
                        mSocket.Shutdown(SocketShutdown.Both);
                        mSocket.Dispose();
                        mSocket.Close();
                    }
                }

                catch (SocketException e) { Console.WriteLine(e.Message); return; }
                catch (ObjectDisposedException e) { Console.WriteLine(e.Message); return; }
                catch (Exception e) { Console.WriteLine(e.Message); return; }

                Console.WriteLine("[Movex.FTP] [DownloadChannel.cs] [InterruptDownload] Socket interrupted.");

                if (mMain_download_thread != null)
                {
                    mMain_download_thread.Interrupt();
                    mInterrupted = true;
                }
            }
            catch (SecurityException e) { Console.WriteLine(e.Message); throw e; }
            catch (ThreadStateException e) { Console.WriteLine(e.Message); throw e; }

            Console.WriteLine("[Movex.FTP] [DownloadChannel.cs] [InterruptDownload] Socket and thread interrupted.");
        }

        public bool IsInterrupted()
        {
            return (mInterrupted);
        }

        public void StartDownload(int index)
        {
            try
            {
                mIndexCurrentTransfer = index;
                if (mStart_time_millisec != null && mCurrent_time_millisec != null)
                {
                    mStart_time_millisec[index] = DateTimeOffset.Now.Ticks / TimeSpan.TicksPerMillisecond;
                    mCurrent_time_millisec[index] = mStart_time_millisec[index];
                }
            }
            catch (NullReferenceException e) { Console.WriteLine(e.Message); throw e; }
            catch (OverflowException e) { Console.WriteLine(e.Message); throw e; }
            catch (IndexOutOfRangeException e) { Console.WriteLine(e.Message); throw e; }
            catch (DivideByZeroException e) { return; }
            catch (Exception e) { throw e; }
        }


        public void StartDownload(string filename)
        {
            try
            {
                if (mStart_time_millisec != null && mCurrent_time_millisec != null)
                {
                    var index = IndexOf(filename);
                    mIndexCurrentTransfer = index;
                    if (index > 0 && index < mNum_trasf)
                    {
                        mStart_time_millisec[IndexOf(filename)] = DateTimeOffset.Now.Ticks / TimeSpan.TicksPerMillisecond;
                        mCurrent_time_millisec[index] = mStart_time_millisec[index];
                    }
                }
            }
            catch (NullReferenceException e) { Console.WriteLine(e.Message); throw e; }
            catch (OverflowException e) { Console.WriteLine(e.Message); throw e; }
            catch (IndexOutOfRangeException e) { Console.WriteLine(e.Message); throw e; }
            catch (DivideByZeroException e) { return; }
            catch (Exception e) { throw e; }
        }




        public void Add_new_download(string filename, long filesize)
        {
            try
            {
                if (mStart_time_millisec != null && mFilenames != null && mFilesizes != null)
                {
                    mFilenames[mIndex] = filename;
                    mStart_time_millisec[mIndex] = DateTimeOffset.Now.Ticks / TimeSpan.TicksPerMillisecond;
                    mCurrent_time_millisec[mIndex] = mStart_time_millisec[mIndex];
                    mFilesizes[mIndex++] = filesize;
                }
                return;
            }
            catch (NullReferenceException e) { Console.WriteLine(e.Message); throw e; }
            catch (OverflowException e) { Console.WriteLine(e.Message); throw e; }
            catch (IndexOutOfRangeException e) { Console.WriteLine(e.Message); throw e; }
            catch (DivideByZeroException e) { return; }
            catch (Exception e) { throw e; }
        }

        public void Incr_received_p(int index, int incr)
        {
            try
            {
                mIndexCurrentTransfer = index;
                var var_milliseconds = (long)(DateTimeOffset.Now.Ticks / TimeSpan.TicksPerMillisecond);
                if (mReceived != null && mRemaining_time_millisec != null && mThroughputs != null)
                {
                    mReceived[index] += incr;
                    mThroughputs[index] = (incr / (var_milliseconds - mCurrent_time_millisec[index])) * 1000;
                    mCurrent_time_millisec[index] = var_milliseconds;
                    mRemaining_time_millisec[index] = (long)((mFilesizes[index] - mReceived[index]) / ((mReceived[index] / (var_milliseconds - mStart_time_millisec[index])) * 1000));

                }
            }
            catch (NullReferenceException e) { Console.WriteLine(e.Message); throw e; }
            catch (OverflowException e) { Console.WriteLine(e.Message); throw e; }
            catch (IndexOutOfRangeException e) { Console.WriteLine(e.Message); throw e; }
            catch (DivideByZeroException e) { return; }
            catch (Exception e) { throw e; }
            
        }

        public int IndexOf(string filename) {
            try
            {
                for (var i = 0; i < mNum_trasf; i++)
                {
                    if (mFilenames[i].Equals(filename))
                    {
                        return (i);
                    }
                }
                return (-1);
            }
            catch (NullReferenceException e) { Console.WriteLine(e.Message); throw e; }
            catch (IndexOutOfRangeException e) { Console.WriteLine(e.Message); throw e; }
            catch (Exception e) { Console.WriteLine(e.Message); throw e; }
        }

        public void IncrReceivedOf(string filename, int incr) {
            Incr_received_p(IndexOf(filename), incr);
        }

        public long GetTotReceived()
        {
            try
            {
                long TotReceived = 0;
                for (var i = 0; i < mNum_trasf; i++)
                {
                    TotReceived += mReceived[i];
                }
                return (TotReceived);
            }
            catch (NullReferenceException e) { Console.WriteLine(e.Message); throw e; }
            catch (OverflowException e) { Console.WriteLine(e.Message); throw e; }
            catch (IndexOutOfRangeException e) { Console.WriteLine(e.Message); throw e; }
            catch (Exception e) { Console.WriteLine(e.Message); throw e; }
        }

        public long GetTotFilesize()
        {
            try
            {
                long TotFilesize = 0;
                for (var i = 0; i < mNum_trasf; i++)
                {
                    TotFilesize += mFilesizes[i];
                }
                return (TotFilesize);
            }
            catch (NullReferenceException e) { Console.WriteLine(e.Message); throw e; }
            catch (OverflowException e) { Console.WriteLine(e.Message); throw e; }
            catch (IndexOutOfRangeException e) { Console.WriteLine(e.Message); throw e; }
            catch (Exception e) { Console.WriteLine(e.Message); throw e; }
        }

        public int GetCompleteTransfer()
        {
            try
            {
                var completetransfer = 0;
                for (var i = 0; i < mNum_trasf; i++)
                {
                    if (mFilesizes[i] == mReceived[i])
                    {
                        completetransfer++;
                    }
                }
                return (completetransfer);
            }
            catch (NullReferenceException e) { Console.WriteLine(e.Message); throw e; }
            catch (OverflowException e) { Console.WriteLine(e.Message); throw e; }
            catch (IndexOutOfRangeException e) { Console.WriteLine(e.Message); throw e; }
            catch (Exception e) { Console.WriteLine(e.Message); throw e; }
        }

        public int GetPrevIndex() {
            return (mIndex - 1);
        }

        public int GetNextIndex() {
            return (mIndex);
        }

        public float GetPercOfTransfer(string filename)
        {
            try
            {
                var index = IndexOf(filename);
                if (index == -1)
                {
                    return (-1);
                }
                var toreceive = (float)mFilesizes[index];
                var received = (float)mReceived[index];
                return ((received * 100) / toreceive);
            }
            catch (NullReferenceException e) { Console.WriteLine(e.Message); throw e; }
            catch (DivideByZeroException e) { Console.WriteLine(e.Message); throw e; }
            catch (IndexOutOfRangeException e) { Console.WriteLine(e.Message); throw e; }
            catch (Exception e) { Console.WriteLine(e.Message); throw e; }
        }

        public float GetPerc() {
            try
            {
                float receive = 0;
                float toreceive = 0;
                for (var i = 0; i < mNum_trasf; i++)
                {
                    toreceive += (float)mFilesizes[i];
                    receive += (float)mReceived[i];
                }
                return ((receive * 100) / toreceive);
            }
            catch (NullReferenceException e) { Console.WriteLine(e.Message); throw e; }
            catch (DivideByZeroException e) { Console.WriteLine(e.Message); throw e; }
            catch (IndexOutOfRangeException e) { Console.WriteLine(e.Message); throw e; }
            catch (Exception e) { Console.WriteLine(e.Message); throw e; }
        }

        public double[] Get_throughputs()
        {
            return (mThroughputs);
        }

        public long[] Get_remaining_times()
        {
            return (mRemaining_time_millisec);
        }

      
        public void Set_filenames(string []filenames) {
            mFilenames = filenames;
            return;
        }

        public int Get_index_current_transfer()
        {
            return (mIndexCurrentTransfer);
        }

        public DownloadChannel(Socket socket, string[] paths, string from, int multi_single)
        {
            mFilepaths = paths;
            mFrom = from;
            mMulti_single = multi_single;
            mIndex = 0;
            mSocket = socket;
            mInterrupted = false;
        }


      
        }
    }

