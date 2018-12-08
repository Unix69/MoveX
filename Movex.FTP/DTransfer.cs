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
            mTotNTransfer = totntransfer;
            mNTransfer = 0;
            mToTransfer = totransfer;
            mTransfered = transfered;
        }

        public void StartTransfer()
        {
            mStartTimeMillisec = (long)(DateTimeOffset.Now.Ticks / TimeSpan.TicksPerMillisecond);
        }
       

        public void AttachToInterface(DownloadChannel dchan) { mDchan = dchan; }

        public void SetTransfered(long transfered) { mTransfered = transfered; }
        public void SetToTransfer(long totransfer) { mToTransfer = totransfer; }

        public void DetachFromInterface(DownloadChannel dchan)
        {
            if (mDchan != null)
            {
                mTransfered += dchan.GetTotReceived();
            }
            mDchan = null;
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
                var var_milliseconds = (long)(DateTimeOffset.Now.Ticks / TimeSpan.TicksPerMillisecond);
                return ((mToTransfer - GetTransfered()) / (GetTransfered() / (var_milliseconds - mStartTimeMillisec)));
            }
            catch (DivideByZeroException e) { return (0); }
            catch (Exception e) { throw e; }
        }
        public string GetTransferFilename()
        {
            if (mDchan != null)
            {
                return (mDchan.Get_filenames()[mDchan.Get_index_current_transfer()]);
            }
            return (null);
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
            return ((GetTransfered() / mToTransfer) * 100);
        }
        public string GetFrom()
        {            
            if (mDchan == null) { return null; }
            else return mDchan.Get_from();
        }
    }
}
