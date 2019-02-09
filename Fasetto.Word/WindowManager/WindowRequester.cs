using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Net;
using System.Threading.Tasks;

namespace Movex.View
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
            ConcurrentQueue<string> requests,
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

        #region Destructor
        public void Dispose()
        { 
            mRequestAvailable.Dispose();
            while (!mRequests.IsEmpty) {
                mRequests.TryDequeue(out var content);
                content = null;
            }
            mTypeRequests.Clear();
            mMessages.Clear();
            mSync.Clear();
            mResponses.Clear();
        }
        #endregion

        #region Core Method(s)
        public void AddUploadProgressWindow(
            IPAddress ipAddress,
            ManualResetEvent windowAvailability,
            ManualResetEvent uploadTransferAvailability)
        {
            var Id = new Random().Next(1001,1100).ToString();
            var ip = ipAddress.ToString();
            var syncPrimitives = new ManualResetEvent[2];

            mRequests.Enqueue(Id);
            mTypeRequests.TryAdd(Id, 103);
            mMessages.TryAdd(Id, ip);
            syncPrimitives[0] = uploadTransferAvailability;
            syncPrimitives[1] = windowAvailability;
            mSync.TryAdd(Id, syncPrimitives);
            mRequestAvailable.Set();
        }
        public void RemoveUploadProgressWindow(string ipAddress)
        {
            var Id = new Random().Next(1101, 1200).ToString();
            mRequests.Enqueue(Id);
            mTypeRequests.TryAdd(Id, 108);
            mMessages.TryAdd(Id, ipAddress);
            mRequestAvailable.Set();
        }
        public void RemoveDownloadProgressWindow(string ipAddress)
        {
            var Id = new Random().Next(1201,1300).ToString();
            mRequests.Enqueue(Id);
            mTypeRequests.TryAdd(Id, 109);
            mMessages.TryAdd(Id, ipAddress);
            mRequestAvailable.Set();
        }
        public void AddMessageWindow(string message)
        {
            var Id = new Random().Next(1301,1400).ToString();
            mRequests.Enqueue(Id);
            mTypeRequests.TryAdd(Id, 105);
            mMessages.TryAdd(Id, message);
            mRequestAvailable.Set();
        }
        public void ResetClient()
        {
            var Id = new Random().Next(1401,1500).ToString();
            mRequests.Enqueue(Id);
            mTypeRequests.TryAdd(Id, 106);
            mRequestAvailable.Set();
        }
        #endregion
    }
}
