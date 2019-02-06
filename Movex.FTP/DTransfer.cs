using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Movex.FTP
{
  public class DTransfer
    {
         
        private DownloadChannel mDchan;
        private long mStartTimeMillisec;
        private long mToTransfer;
        private long mTransfered;
        private long mNTransfer;
        private long mTotNTransfer;


        public DTransfer()
        {
            mToTransfer = 0;
            mTransfered = 0;
            mNTransfer = 0;
            mTotNTransfer = 0;

        }

        public DTransfer(long totntransfer, long transfered, long totransfer)
        {
            Console.WriteLine("[Movex.FTP] [DTransfer.cs] [DTransfer] Creating a new DTransfer.");
            mTotNTransfer = totntransfer;
            mNTransfer = 0;
            mToTransfer = totransfer;
            mTransfered = transfered;
        }

        public void StartTransfer()
        {
            mStartTimeMillisec = (long)(DateTimeOffset.Now.Ticks / TimeSpan.TicksPerMillisecond);
        }
       
        public void AttachToInterface(DownloadChannel dchan)
        {
            Console.WriteLine("[Movex.FTP] [DTransfer.cs] [AttachToInterface] Trying to attach dchan to DTransfer.");
            if (dchan != null)
            {
                mDchan = dchan;
                Console.WriteLine("[Movex.FTP] [DTransfer.cs] [AttachToInterface] Attached dchan to DTransfer interface.");
                Console.WriteLine("[Movex.FTP] [DTransfer.cs] [AttachToInterface] Dchan from: " + dchan.Get_from());
            }
        }

        public void SetTransfered(long transfered) { mTransfered = transfered; }
        public void SetToTransfer(long totransfer) { mToTransfer = totransfer; }

        public void DetachFromInterface(DownloadChannel dchan)
        {
            if (mDchan != null)
            {
                mTransfered += dchan.GetTotReceived();
                mNTransfer += dchan.GetCompleteTransfer();
            }
            mDchan = null;
            Console.WriteLine("[Movex.FTP] [DTransfer.cs] [DetachFromInterface] Detached dchan to DTransfer interface.");
        }

        public float GetTransferThroughput()
        {
            if (mDchan != null)
            {
                return ((float)mDchan.Get_throughputs()[mDchan.Get_index_current_transfer()]);
            }
            return (0);
        }
        public long GetRemainingTime()
        {
            try
            {
                var timeNow = (long)(DateTimeOffset.Now.Ticks / TimeSpan.TicksPerMillisecond);
                var timeTaken = timeNow - mStartTimeMillisec;
                var processed = GetTransfered();
                var left = (mToTransfer - GetTransfered());
                var remainingTime = timeTaken * ((float)left / processed);

                return ((long)remainingTime);
            }
            catch (DivideByZeroException e) { return (0); }
            catch (Exception e) { throw e; }
        }
        public string GetTransferFilename()
        {
            try
            {
                if (mDchan != null)
                {
                    return (mDchan.Get_filenames()[mDchan.Get_index_current_transfer()]);
                }
            }
            catch (Exception Exception)
            {
                var Message = Exception.Message;
                Console.WriteLine("[MOVEX.FTP] [DTransfer.cs] [GetTransferFilename] " + Message);
            }

            Console.WriteLine("[Movex.FTP] [DTransfer.cs] [GetTransferFilename] Dchan null.");
            return null;
        }
        public long GetTransferFilesize()
        {
            if (mDchan != null)
            {
                return (mDchan.Get_filesizes()[mDchan.Get_index_current_transfer()]);
            }
            return (0);
        }
        public long GetTransfered()
        {
            if (mDchan == null)
            {
                return (mTransfered);
            }
            else
            {
                return (mTransfered + mDchan.GetTotReceived());
            }
        }
        public long GetToTransfer() { return (mToTransfer); }
        public long GetTransferComplete()
        {
            if (mDchan != null)
            {
                return (mNTransfer + mDchan.GetCompleteTransfer());
            }
            return (mNTransfer);
        }
        public long GetTotTransferToDo()
        {
            return (mTotNTransfer);
        }
        public float GetTransferPerc()
        {
            try
            {
                var ratio = ((float)GetTransfered() / mToTransfer);
                var perc =  ratio * 100;

                return perc;
            }
            catch (DivideByZeroException Exception)
            {
                var Message = Exception.Message;
                Console.WriteLine("[MOVEX.FTP] [Dtransfer.cs] [GetTransferPerc]" + Message + ".");

                return 0;
            }
            catch (Exception Exception) {
                var Message = Exception.Message;
                Console.WriteLine("[MOVEX.FTP] [Dtransfer.cs] [GetTransferPerc]" + Message + ".");

                throw Exception;
            }
        }
        public string GetFrom()
        {            
            if (mDchan == null) { return null; }
            else return mDchan.Get_from();
        }
    }
}
