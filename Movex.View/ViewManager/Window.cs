using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Movex.View.Windows
{
    /// <summary>
    /// Support class to manage the Windows
    /// </summary>
    public class Window<T>
    {
        /// <summary>
        /// Public enumerators
        /// </summary>
        public enum WindowType { MessageWindow = 0, YesNoWindow = 1, WhereWindow = 2, ProgressWindow = 3};

        /// <summary>
        /// Private members
        /// </summary>
        private WindowType mType;
        private T mPack;
        private ManualResetEvent mCloseWindowEvent;
        private ManualResetEvent mOpenWindowEvent;
        private ManualResetEvent mResponseAvailability;
        private ConcurrentStack<string> mResponse;

        /// <summary>
        /// Default Constructor
        /// </summary>
        /// <param name="type"></param>
        /// <param name="text"></param>
        public Window(WindowType type, T pack, ManualResetEvent responseAvailability, ConcurrentStack<string> response)
        {
            mType = type;
            mPack = pack;
            mCloseWindowEvent = new ManualResetEvent(false);
            mOpenWindowEvent = new ManualResetEvent(false);
            mResponseAvailability = responseAvailability;
            mResponse = response;
        }

        /// <summary>
        /// Public getters
        /// </summary>
        /// <returns></returns>
        new public WindowType GetType()
        {
            return mType;
        }
        public T GetPack()
        {
            return mPack;
        }
        public ManualResetEvent GetCloseWindowEvent()
        {
            return mCloseWindowEvent;
        }
        public ManualResetEvent GetOpenWindowEvent()
        {
            return mOpenWindowEvent;
        }
        public ConcurrentStack<string> GetResponse()
        {
            return mResponse;
        }
        public ManualResetEvent GetResponseAvailablity()
        {
            return mResponseAvailability;
        }

    }
}
