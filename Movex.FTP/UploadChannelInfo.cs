using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Movex.FTP
{
    public class UploadChannelInfo
    {

        private UploadChannel mUchan;
        private int mCurrent_index;
        private int mEnd_index;

        public void Set_current_index(int current_index) {
            mCurrent_index = current_index;
        }


        public void Set_end_index(int end_index)
        {
            mEnd_index = end_index;
        }

        public UploadChannelInfo(ref UploadChannel uchan) {
            mUchan = uchan;
        }

        public void Switch_upload() {
            if ((mCurrent_index + 1) <= mEnd_index)
                mCurrent_index++;
        }

        public string Get_current_filename() {
            var filename  = "";

            var filenames = mUchan.Get_filenames();
            if (filenames != null)
            {
                try { filename = filenames[mCurrent_index]; }
                catch(Exception e)
                {
                    // Case of a folder and no file(s) inside
                }
            }
            if (mCurrent_index > mEnd_index) {
                filename = FTPsupporter.Unknown.UnknownString;
            }
            return (filename);
        }

        public double Get_current_throughput() {
            if (mCurrent_index >= mEnd_index)
            {
                return (FTPsupporter.Unknown.UnknownDouble);
            }
            return (mUchan.Get_throughputs()[mCurrent_index]);
        }

        public long Get_current_filesize() {
            if (mCurrent_index > mEnd_index)
            {
                return (FTPsupporter.Unknown.UnknownInt);
            }
            return (mUchan.Get_filesizes()[mCurrent_index]);
        }

        public string Get_current_to() {
            if (mCurrent_index > mEnd_index)
            {
                return (FTPsupporter.Unknown.UnknownString);
            }
            return (mUchan.Get_to());
        }

        public string Get_current_remaining_time() {
            if (mCurrent_index > mEnd_index)
            {
                return (FTPsupporter.Unknown.UnknownLong).ToString();
            }
            return (mUchan.Get_remaining_times()[mCurrent_index]).ToString();
        }

        public long Get_current_sended() {
            if (mCurrent_index > mEnd_index)
            {
                return (FTPsupporter.Unknown.UnknownInt);
            }
            return (mUchan.Get_sended()[mCurrent_index]);
        }

        public string Get_current_percentage()
        {
            return (Math.Round(mUchan.GetPerc(), 0, MidpointRounding.AwayFromZero)).ToString();
        }
        

    }
}





