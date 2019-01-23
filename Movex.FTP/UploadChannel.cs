using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Net.Sockets;
using System.Security;

namespace Movex.FTP
{
    public class UploadChannel
    {
        private string mTo;
        private string[] mFilenames;
        private Thread mMain_upload_thread;
        private Thread[] mUpload_threads;
        private string[] mPaths;
       
        private long[] mFilesizes;
        private long[] mSended;
        private double[] mThroughputs;
        private long[] mStart_time_millisec;
        private long[] mCurrent_time_millisec;
        private long[] mRemaining_time_millisec;
        private int mIndex;
        private int mIndexCurrentTransfer;
        private int mNum_trasf;
        private int mMulti_single;
        private Socket mSocket;
        private bool mInterrupted;
        //costructor

        public UploadChannel() { }

        public UploadChannel(Socket socket, string[] paths, string to, int multi_single)
        {
            mPaths = paths;
            mTo = to;
            mMulti_single = multi_single;
            mIndex = 0;
            mIndexCurrentTransfer = 0;
            mSocket = socket;
            mInterrupted = false;
        }


  
        public void Set_paths(string [] paths) {
            mPaths = paths;
        }

        private void CreateSendStructure(int num_trasf)
        {

            mFilesizes = new long[num_trasf];
            mFilenames = new string[num_trasf];
            mSended = new long[num_trasf];
            mThroughputs = new double[num_trasf];
            mStart_time_millisec = new long[num_trasf];
            mCurrent_time_millisec = new long[num_trasf];
            mRemaining_time_millisec = new long[num_trasf];
            mUpload_threads = new Thread[num_trasf];
            for (var i = 0; i < num_trasf; i++)
            {
                mFilesizes[i] = mSended[i] = 0;
                mThroughputs[i] = 0;
                mStart_time_millisec[i] = 0;
                mCurrent_time_millisec[i] = 0;
                mFilenames[i] = "";
            }
            mIndex = 0;
            return;
        }


        //Setter

        public void Set_upload_thread(Thread upload_thread, int index)
        {
            mUpload_threads[index] = upload_thread;
            return;
        }

        public void Set_upload_thread(Thread upload_thread, string filename)
        {
            mUpload_threads[IndexOf(filename)] = upload_thread;
            return;
        }

        public void Set_num_trasf(int num_trasf)
        {
            mNum_trasf = num_trasf;
            CreateSendStructure(num_trasf);
            return;
        }

        public void Set_to(string to)
        {
            mTo = to;
            return;
        }

        public void Set_multi_single(int multi_single)
        {
            mMulti_single = multi_single;
            return;
        }

        public void Set_filenames(string []filenames) {
            mFilenames = filenames;
            return;
        }

        public void Set_filesizes(long[] filesizes)
        {
            mFilesizes = filesizes;
            return;
        }

        public void Set_path(string path, int n)
        {
            mPaths[n] = path;
            return;
        }

      
        public void Set_socket(ref Socket socket)
        {
            mSocket = socket;
            return;
        }

        public void Set_main_upload_thread(Thread main_upload_thread) {
            mMain_upload_thread = main_upload_thread;
            return;
        }

        public int Get_index_current_transfer()
        {
            return (mIndexCurrentTransfer);
        }

        public Socket Get_socket()
        {
            return (mSocket);
        }

        public string Get_path(string filename)
        {
            return (mPaths[IndexOf(filename)]);
        }

        public string [] Get_paths()
        {
            return (mPaths);
        }

        public int Get_num_trasf()
        {
            return (mNum_trasf);
        }

        public string Get_to()
        {
            return (mTo);
        }

        public int Get_multi_single()
        {
            return (mMulti_single);
        }

        public Thread Get_upload_thread(string filename)
        {
            return (mUpload_threads[IndexOf(filename)]);
        }

        public Thread Get_upload_thread(int index)
        {
            return (mUpload_threads[index]);
        }

        public long[] Get_sended()
        {
            return (mSended);
        }

        public long[] Get_filesizes()
        {
            return (mFilesizes);
        }

        public string[] Get_filenames()
        {
            return (mFilenames);
        }

        public double[] Get_throughputs()
        {
            return (mThroughputs);
        }

        public long[] Get_remaining_times()
        {
            return (mRemaining_time_millisec);
        }

        public Thread Get_main_upload_thread() {
            return (mMain_upload_thread);
        }


        public void InterruptUpload() {
            try
            {

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


                if (mMain_upload_thread != null)
                {
                    mMain_upload_thread.Interrupt();
                    mInterrupted = true;
                    mMain_upload_thread.Abort();
                }
            }
            catch (SecurityException e) { Console.WriteLine(e.Message); throw e; }
            catch (ThreadStateException e) { Console.WriteLine(e.Message); throw e; }

        }

        public bool IsInterrupted() {
            return (mInterrupted);
        }

        
        //additional function

        public void StartUpload(int index) {
            
            try
            {
                mIndexCurrentTransfer = index;
                if (mStart_time_millisec != null)
                {
                    mStart_time_millisec[index] = DateTimeOffset.Now.Ticks / TimeSpan.TicksPerMillisecond;
                    mCurrent_time_millisec[index] = mStart_time_millisec[index];
                }
            }
            catch (DivideByZeroException e) { return; }
            catch (Exception e) { throw e; }

        }


        public void StartUpload(string filename)
        {
            try
            {
                if (mStart_time_millisec != null)
                {
                    var index = IndexOf(filename);
                    mIndexCurrentTransfer = index;
                    if (index > 0 && index < mNum_trasf)
                    {
                        mStart_time_millisec[index] = DateTimeOffset.Now.Ticks / TimeSpan.TicksPerMillisecond;
                        mCurrent_time_millisec[index] = mStart_time_millisec[index];
                    }
                }
                return;
            }
            catch (DivideByZeroException e) { return; }
            catch (Exception e) { throw e; }
        }




        public void Add_new_upload(string filename, long filesize)
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
            catch (DivideByZeroException e) { return; }
            catch (Exception e) { throw e; }
        }





        public void Incr_sended_p(int index, int incr)
        {
            try
            {
                mIndexCurrentTransfer = index;
                var var_milliseconds = (long)(DateTimeOffset.Now.Ticks / TimeSpan.TicksPerMillisecond);
                if (mSended != null && mRemaining_time_millisec != null && mThroughputs != null)
                {
                    mSended[index] += incr;
                    mThroughputs[index] = (incr / (var_milliseconds - mCurrent_time_millisec[index])) * 1000;
                    mCurrent_time_millisec[index] = var_milliseconds;
                    mRemaining_time_millisec[index] = (long)((mFilesizes[index] - mSended[index]) / ((mSended[index] / (var_milliseconds - mStart_time_millisec[index])) * 1000));
                }
            }
            catch (DivideByZeroException e) { return; }
            catch (Exception e) { throw e; }
        }

        public int IndexOf(string filename)
        {
            if (filename != null && mFilenames != null) { 
                for (var i = 0; i < mNum_trasf; i++)
                {
                    if (mFilenames[i].Equals(filename))
                    {
                        return (i);
                    }
                }
                return (-1);
        }
            return (-1);
        }

        public void IncrSendedOf(string filename, int incr)
        {
            Incr_sended_p(IndexOf(filename), incr);
        }

        public long GetTotSended()
        {
            long TotSended = 0;
            for (var i = 0; i < mNum_trasf; i++)
            {
                TotSended += mSended[i];
            }
            return (TotSended);
        }

        public long GetTotFilesize()
        {
            long TotFilesize = 0;
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
                if (mFilesizes[i] == mSended[i])
                {
                    completetransfer++;
                }
            }
            return (completetransfer);
        }

        
        public int GetPrevIndex()
        {
            return (mIndex - 1);
        }

        public int GetNextIndex()
        {
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
                float tosend = mFilesizes[index];
                var sended = (float)mSended[index];
                return ((sended * 100) / tosend);
            }
            catch (DivideByZeroException e) { return(0); }
           
        }

        public long GetRemainingTime() {
            try
            {
                var totBytes = GetTotSended();
                var totFileBytes = GetTotFilesize();
                var var_milliseconds = (long)(DateTimeOffset.Now.Ticks / TimeSpan.TicksPerMillisecond);
                return ((long)((totFileBytes - totBytes) / ((totBytes / (var_milliseconds - mStart_time_millisec[0])) * 1000)));
            }
            catch (DivideByZeroException e) { return (0); }
            catch (Exception e) { throw e; }
        }

        public float GetPerc()
        {
            try
            {
                float sended = 0;
                float tosend = 0;
                for (var i = 0; i < mNum_trasf; i++)
                {
                    tosend += (float)mFilesizes[i];
                    sended += (float)mSended[i];
                }
                return((float) Math.Round(((sended * 100) / tosend), 0, MidpointRounding.AwayFromZero));
            }
            catch (DivideByZeroException e) { return(0); }
            catch (Exception e) { throw e; }
        }








    }
}
