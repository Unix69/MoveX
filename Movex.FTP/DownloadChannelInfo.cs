using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Movex.FTP
{
    public class DownloadChannelInfo
    {
        private DownloadChannel mDchan;
        private int mCurrent_index;
        private int mEnd_index;

        public void Set_current_index(int current_index)
        {
            mCurrent_index = current_index;
        }


        public void Set_end_index(int end_index)
        {
            mEnd_index = end_index;
        }

        public DownloadChannelInfo(ref DownloadChannel dchan)
        {
            mDchan = dchan;
        }

        public void Switch_download()
        {
            mCurrent_index++;
        }

        public string Get_current_filename()
        {
            var filename = mDchan.Get_filenames()[mCurrent_index];
            if (mCurrent_index >= mEnd_index)
            {
                return (FTPsupporter.UnknownString);
            }
            return (filename);
        }

        public double Get_current_throughput()
        {
            if (mCurrent_index >= mEnd_index)
            {
                return (FTPsupporter.UnknownDouble);
            }
            return (mDchan.Get_throughputs()[mCurrent_index]);
        }

        public int Get_current_filesize()
        {
            if (mCurrent_index >= mEnd_index)
            {
                return (FTPsupporter.UnknownInt);
            }
            return (mDchan.Get_filesizes()[mCurrent_index]);
        }

        public string Get_current_to()
        {
            if (mCurrent_index >= mEnd_index)
            {
                return (FTPsupporter.UnknownString);
            }
            return (mDchan.Get_from());
        }

        public long Get_current_remaining_time()
        {
            if (mCurrent_index >= mEnd_index)
            {
                return (FTPsupporter.UnknownLong);
            }
            return (mDchan.Get_remaining_times()[mCurrent_index]);
        }

        public int Get_current_sended()
        {
            if (mCurrent_index >= mEnd_index)
            {
                return (FTPsupporter.UnknownInt);
            }
            return (mDchan.Get_received()[mCurrent_index]);
        }
    }
}
