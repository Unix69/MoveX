using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Movex.FTP
{
    public class WindowRequester : IDisposable
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

        #region Destructor
        public void Dispose()
        { /*
            mRequestAvailable.Dispose();
            while (!mRequests.IsEmpty) {
                mRequests.TryDequeue(out var content);
                content = null;
            }
            mTypeRequests.Clear();
            mMessages.Clear();
            mSync.Clear();
            mResponses.Clear();
            */
        }
        #endregion

        #region Core Method(s)
        public string AddYesNoWindow(string message)
        {
            var Id = new Random().Next(1000).ToString();
            var responseBag = new ConcurrentBag<string>();
            var responsesAvailable = new ManualResetEvent[1];
            responsesAvailable[0] = new ManualResetEvent(false);
            
            mRequests.Enqueue(Id);
            mTypeRequests.TryAdd(Id, 101);
            mMessages.TryAdd(Id, message);
            mSync.TryAdd(Id, responsesAvailable);
            mResponses.TryAdd(Id, responseBag);
            mRequestAvailable.Set();
            responsesAvailable[0].WaitOne();
            mResponses.TryGetValue(Id, out var responseContainer);
            responseContainer.TryTake(out var response);

            return response;
        }
        public string AddWhereWindow(string message)
        {
            var Id = new Random().Next(1000).ToString();
            var whereResponseBag = new ConcurrentBag<string>();
            var whereResponseAvailable = new ManualResetEvent[1];
            whereResponseAvailable[0] = new ManualResetEvent(false);

            mRequests.Enqueue(Id);
            mTypeRequests.TryAdd(Id, 102);
            mMessages.TryAdd(Id, message);
            mSync.TryAdd(Id, whereResponseAvailable);
            mResponses.TryAdd(Id, whereResponseBag);
            mRequestAvailable.Set();
            whereResponseAvailable[0].WaitOne();
            mResponses.TryGetValue(Id, out var whereResponseContainer);
            whereResponseContainer.TryTake(out var whereToSave);

            return whereToSave;
        }
        public void AddDownloadProgressWindow(IPAddress ipAddress, ManualResetEvent windowAvailability, ManualResetEvent downloadTransferAvailability)
        {
            var Id = new Random().Next(1000).ToString();
            var ip = ipAddress.ToString();
            var syncPrimitives = new ManualResetEvent[2];

            mRequests.Enqueue(Id);
            mTypeRequests.TryAdd(Id, 104);
            mMessages.TryAdd(Id, ip);
            syncPrimitives[0] = downloadTransferAvailability;
            syncPrimitives[1] = windowAvailability;
            mSync.TryAdd(Id, syncPrimitives);
            mRequestAvailable.Set();
        }
        public void RemoveUploadProgressWindow(string ipAddress)
        {
            var Id = new Random().Next(1000).ToString();
            mRequests.Enqueue(Id);
            mTypeRequests.TryAdd(Id, 108);
            mMessages.TryAdd(Id, ipAddress);
            mRequestAvailable.Set();
        }
        public void RemoveDownloadProgressWindow(string ipAddress)
        {
            var Id = new Random().Next(1000).ToString();
            mRequests.Enqueue(Id);
            mTypeRequests.TryAdd(Id, 109);
            mMessages.TryAdd(Id, ipAddress);
            mRequestAvailable.Set();
        }
        public void AddMessageWindow(string message)
        {
            var Id = new Random().Next(1000).ToString();
            mRequests.Enqueue(Id);
            mTypeRequests.TryAdd(Id, 105);
            mMessages.TryAdd(Id, message);
            mRequestAvailable.Set();
        }
        public void ResetServer()
        {
            var Id = new Random().Next(1000).ToString();
            mRequests.Enqueue(Id);
            mTypeRequests.TryAdd(Id, 107);
            mRequestAvailable.Set();
        }
        #endregion

    }
}
