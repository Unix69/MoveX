using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Movex.FTP
{
    public class WindowRequester
    {
        #region Member(s)
        // Member(s) useful for syncrhonization with Movex.View.Core
        private ManualResetEvent mRequestAvailable;
        private ConcurrentQueue<string> mRequests;
        private ConcurrentDictionary<string, int> mTypeRequests;
        private ConcurrentDictionary<string, string> mMessages;
        private ConcurrentDictionary<string, ManualResetEvent[]> mSync;
        private ConcurrentDictionary<string, ConcurrentBag<string>> mResponses;
        #endregion

        #region Constructor
        public WindowRequester(
            ManualResetEvent requestAvailable,
            ConcurrentQueue<string>  requests,
            ConcurrentDictionary<string, int> typeRequests,
            ConcurrentDictionary<string, string> messages,
            ConcurrentDictionary<string, ManualResetEvent[]> sync,
            ConcurrentDictionary<string, ConcurrentBag<string>> responses)
        {
            mRequestAvailable = requestAvailable;
            mRequests = requests;
            mTypeRequests = typeRequests;
            mMessages = messages;
            mSync = sync;
            mResponses = responses;
        }
        #endregion

        #region Core Method(s)
        public void RemoveUploadProgressWindow(string ipAddress)
        {
            var Id = "RemoveUploadProgressWindowRequest";
            mRequests.Enqueue(Id);
            mTypeRequests.TryAdd(Id, 108);
            mMessages.TryAdd(Id, ipAddress);
            mRequestAvailable.Set();
        }
        public void RemoveDownloadProgressWindow(string ipAddress)
        {
            var Id = "RemoveDownloadProgressWindowRequest";
            mRequests.Enqueue(Id);
            mTypeRequests.TryAdd(Id, 109);
            mMessages.TryAdd(Id, ipAddress);
            mRequestAvailable.Set();
        }
        public void AddMessageWindow(string message)
        {
            var Id = "MessageWindowRequest";
            mRequests.Enqueue(Id);
            mTypeRequests.TryAdd(Id, 105);
            mMessages.TryAdd(Id, message);
            mRequestAvailable.Set();
        }
        #endregion

    }
}
