using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Movex.FTP
{
   public class UTransfer
    {
        private UploadChannel mUchan;
        private long mStartTimeMillisec;
        private long mToTransfer;
        private long mTransfered;
        private long mNTransfer;
        private long mTotNTransfer;

        public UTransfer() {
            mToTransfer = 0;
            mTransfered = 0;
            mNTransfer = 0;
            mTotNTransfer = 0;
        }
        public UTransfer(long totntransfer, long transfered, long totransfer)
        {
            mTotNTransfer = totntransfer;
            mNTransfer = 0;
            mToTransfer = totransfer;
            mTransfered = transfered;
        }

        public void StartTransfer() {
            mStartTimeMillisec = (long)(DateTimeOffset.Now.Ticks / TimeSpan.TicksPerMillisecond);
        }
        public void AttachToInterface(UploadChannel uchan) { mUchan = uchan; }
        public void SetTransfered(long transfered) { mTransfered = transfered; }
        public void SetToTransfer(long totransfer) { mToTransfer = totransfer; }
        public void DetachFromInterface(UploadChannel uchan) {
            if (mUchan != null)
            {
                mTransfered += uchan.GetTotSended();
                mNTransfer += uchan.GetCompleteTransfer();
            }
            mUchan = null;
        }
        public float GetTransferThroughput() {
            if (mUchan != null)
            {
                return ((float) mUchan.Get_throughputs()[mUchan.Get_index_current_transfer()]);
            }
            return (0);
        }
        public long GetRemainingTime() {
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
            try
            {
                Console.WriteLine("[Movex.FTP] [UTransfer.cs] [GetTransferFilename] Trying to get TransferFilename");
                if (mUchan != null)
                {
                    return (mUchan.Get_filenames()[mUchan.Get_index_current_transfer()]);
                }
            }
            catch(Exception Exception)
            {
                var Message = Exception.Message;
                Console.WriteLine("[MOVEX.FTP] [UTransfer.cs] [GetTransferFilename] " + Message);
            }

            Console.WriteLine("[Movex.FTP] [UTransfer.cs] [GetTransferFilename] Dchan null.");
            return null;
        }
        public long GetTransferFilesize()
        {
            if (mUchan != null)
            {
                return (mUchan.Get_filesizes()[mUchan.Get_index_current_transfer()]);
            }
            return (0);
        }
        public long GetTransfered() {
            if (mUchan == null)
            {
                return (mTransfered);
            }
            else {
                return (mTransfered + mUchan.GetTotSended());
            }
        }
        public long GetToTransfer() { return (mToTransfer); }
        public long GetTransferComplete()
        {
            if (mUchan != null)
            {
                return (mNTransfer + mUchan.GetCompleteTransfer());
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

                Console.WriteLine("[Movex.FTP] [UTransfer.cs] [GetTransferPerc] Trying to get the transfer percentage.");

                var ratio = ((float)GetTransfered() / mToTransfer);
                var perc = ratio * 100;

                return perc;
            }
            catch (DivideByZeroException Exception) {
                var Message = Exception.Message;
                Console.WriteLine("[MOVEX.FTP] [Dtransfer.cs] [GetTransferPerc]" + Message + ".");

                return 0;
            }
            catch (Exception Exception)
            {
                var Message = Exception.Message;
                Console.WriteLine("[MOVEX.FTP] [Dtransfer.cs] [GetTransferPerc]" + Message + ".");

                throw Exception;
            }
        }
       
       
        public string GetTo()
        {
            if (mUchan == null) { return null; }
            else return mUchan.Get_to();
        }
    }
}
