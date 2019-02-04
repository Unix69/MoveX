using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Windows.Forms;
using System.Threading;
using Newtonsoft.Json;
using System.IO;
using Newtonsoft.Json.Linq;

namespace Movex.Network
{
    class UdpManager
    {
        private static bool DEBUG = true;

        public class UdpState
        {
            public IPEndPoint mEP;
            public UdpClient mU;

            public UdpState(IPEndPoint a, UdpClient b)
            {
                 mEP = a;
                 mU = b;
            }
        }

        public int mListenPort = 1500;
        public IPAddress mIP;
        public IPAddress mSUBNET_MASK;
        public IPAddress mBROADCAST;
        private const string NetworkLogPath = @".\Log\Network.log";
        private Mutex mNetworkLogPathMutex;

        #region Private members
        private UdpClient mUdpClient;
        private NetworkInformation mNetworkInfo;
        private List<RestrictedUser> mRestrictedUsers;
        private RestrictedUser mRestrictedUser;
        private Mutex mMut;
        private const int TCPport = 1513;
        private TcpManager mTcpManager;
        private Thread mReceivePictureThread;
        private bool mPrivateMode;
        #endregion

        /// <summary>
        /// Constructor for the UdpManager class
        /// </summary>
        public UdpManager(RestrictedUser ru, bool privateMode)
        {
            //Create the Network Log
            var sw = File.Create(NetworkLogPath);
            sw.Close();

            // Prepare the data structure for the Network Log
            mNetworkLogPathMutex = new Mutex();

            mUdpClient = new UdpClient(new IPEndPoint(IPAddress.Any, mListenPort));
            mRestrictedUser = ru;
            mRestrictedUsers = new List<RestrictedUser>();
            mMut = new Mutex();
            mNetworkInfo = new NetworkInformation();
            CollectLocalNetworkInformations(mNetworkInfo);
            mTcpManager = new TcpManager();
            mTcpManager.Init(1512, ref mNetworkLogPathMutex, NetworkLogPath);
            mPrivateMode = privateMode;

            using (var streamWriter = File.AppendText(NetworkLogPath))
            {
                var currentTime = DateTime.Now.ToString("h: mm:ss tt");
                streamWriter.WriteLine("[" + currentTime + "]" + " " + "Connection Manager created.");
                streamWriter.Close();
            }
            
        }
        public UdpManager(UdpClient u, RestrictedUser ru)
        {
            mUdpClient = u;
            mRestrictedUser = ru;
            mRestrictedUsers = new List<RestrictedUser>();
            mMut = new Mutex();
            mNetworkInfo = new NetworkInformation();
            CollectLocalNetworkInformations(mNetworkInfo);
            mTcpManager = new TcpManager();
        }

        

        /// <summary>
        /// Set the restricted user 
        /// </summary>
        /// <param name="ru"></param>
        public void SetRestrictedUser(RestrictedUser ru)
        {
            mRestrictedUser = ru;
        }

        private void ReceiveCallback(IAsyncResult ar)
        {
            var e = (IPEndPoint)((UdpState)(ar.AsyncState)).mEP;
            var u = (UdpClient)((UdpState)(ar.AsyncState)).mU;
            var state = new UdpState(e, u);

            try {
                var receivedBytes = u.EndReceive(ar, ref e);
                u.BeginReceive(new AsyncCallback(ReceiveCallback), (object)state);

                var receivedString = Encoding.Unicode.GetString(receivedBytes);
                var messageReceived = Unjsonify(receivedString);
                var ipSender = e.Address;

                // Ignore the messages that I receive by myself
                if (!(ipSender.Equals(mIP)))
                {
                    if (messageReceived.GetMessageType().Equals(Message.MSG_ERROR))
                    {
                        // Record the operation to the Network Log
                        mNetworkLogPathMutex.WaitOne();
                        using (var streamWriter = File.AppendText(NetworkLogPath))
                        {
                            var currentTime = DateTime.Now.ToString("h: mm:ss tt");
                            streamWriter.WriteLine("[" + currentTime + "]" + " " + "Receveid from: " + ipSender.ToString() + " " + "the message: " + Message.MSG_ERROR + ".");
                            streamWriter.Close();
                        }
                        mNetworkLogPathMutex.ReleaseMutex();

                        // Do nothing
                    }
                    else if (messageReceived.GetMessageType().Equals(Message.MSG_DISCOVERY))
                    {
                        // Record the operation to the Network Log
                        mNetworkLogPathMutex.WaitOne();
                        using (var streamWriter = File.AppendText(NetworkLogPath))
                        {
                            var currentTime = DateTime.Now.ToString("h: mm:ss tt");
                            streamWriter.WriteLine("[" + currentTime + "]" + " " + "Receveid from: " + ipSender.ToString() + " " + "the message: " + Message.MSG_DISCOVERY + ".");
                            streamWriter.Close();
                        }
                        mNetworkLogPathMutex.ReleaseMutex();
                        if (mPrivateMode) return;

                        // Reply with a message of presentation
                        var message = new Message(Message.MSG_PRESENTATION, mRestrictedUser);
                        SendMessage(Jsonify(message), ipSender);
                    }
                    else if (messageReceived.GetMessageType().Equals(Message.MSG_PRESENTATION))
                    {
                        // Record the operation to the Network Log
                        mNetworkLogPathMutex.WaitOne();
                        using (var streamWriter = File.AppendText(NetworkLogPath))
                        {
                            var currentTime = DateTime.Now.ToString("h: mm:ss tt");
                            streamWriter.WriteLine("[" + currentTime + "]" + " " + "Receveid from: " + ipSender.ToString() + " " + "the message: " + Message.MSG_PRESENTATION + ".");
                            streamWriter.Close();
                        }
                        mNetworkLogPathMutex.ReleaseMutex();

                        var candidate = messageReceived.GetRestrictedUser();
                        var i = mRestrictedUsers.FindIndex(x => x.mIpAddress == candidate.mIpAddress);
                        if (i != -1)
                        {
                            mRestrictedUsers.RemoveAt(i);
                        }
                        mRestrictedUsers.Add(candidate);

                        if (mPrivateMode) return;

                        // Reply with an acknoledge message and store the new user                    
                        var message = new Message(Message.MSG_ACKNOWLEDGE, mRestrictedUser);
                        SendMessage(Jsonify(message), ipSender);
                        SendProfilePicture(ipSender, mRestrictedUser.mProfilePictureFilename);

                    }
                    else if (messageReceived.GetMessageType().Equals(Message.MSG_ACKNOWLEDGE))
                    {
                        // Record the operation to the Network Log
                        mNetworkLogPathMutex.WaitOne();
                        using (var streamWriter = File.AppendText(NetworkLogPath))
                        {
                            var currentTime = DateTime.Now.ToString("h: mm:ss tt");
                            streamWriter.WriteLine("[" + currentTime + "]" + " " + "Receveid from: " + ipSender.ToString() + " " + "the message: " + Message.MSG_ACKNOWLEDGE + ".");
                            streamWriter.Close();
                        }
                        mNetworkLogPathMutex.ReleaseMutex();

                        // Reply with no message, and only add the user
                        var candidate = messageReceived.GetRestrictedUser();
                        var i = mRestrictedUsers.FindIndex(x => x.mIpAddress == candidate.mIpAddress);
                        if (i != -1)
                        {
                            mRestrictedUsers.RemoveAt(i);
                        }
                        mRestrictedUsers.Add(candidate);
                    }
                    else if (messageReceived.GetMessageType().Equals(Message.MSG_LEAVE))
                    {
                        mNetworkLogPathMutex.WaitOne();
                        using (var streamWriter = File.AppendText(NetworkLogPath))
                        {
                            var currentTime = DateTime.Now.ToString("h:mm:ss tt");
                            streamWriter.WriteLine("[" + currentTime + "]" + " " + "Receveid from: " + ipSender.ToString() + " " + "the message: " + Message.MSG_LEAVE + ".");

                            // Remove the user from the list
                            var index = mRestrictedUsers.FindIndex(user => user.mIpAddress.Equals(ipSender.ToString()));
                            streamWriter.WriteLine("[" + currentTime + "] Removing user at Index: " + index.ToString());
                            mRestrictedUsers.RemoveAt(index);

                            streamWriter.Close();
                        }
                        mNetworkLogPathMutex.ReleaseMutex();
                    }
                    else if (messageReceived.GetMessageType().Equals(Message.MSG_UPDATE))
                    {
                        // Record the operation to the Network Log
                        mNetworkLogPathMutex.WaitOne();
                        using (var streamWriter = File.AppendText(NetworkLogPath))
                        {
                            var currentTime = DateTime.Now.ToString("h:mm:ss tt");
                            streamWriter.WriteLine("[" + currentTime + "]" + " " + "Receveid from: " + ipSender.ToString() + " " + "the message: " + Message.MSG_UPDATE + ".");
                            streamWriter.Close();
                        }
                        mNetworkLogPathMutex.ReleaseMutex();

                        // Wait for profile picture
                        WaitForProfilePictures();

                        // Remove the user from the list
                        var index = mRestrictedUsers.FindIndex(user => user.mIpAddress.Equals(ipSender.ToString()));
                        mRestrictedUsers.RemoveAt(index);
                        mRestrictedUsers.Add(messageReceived.GetRestrictedUser());
                    }
                }

            } catch (ObjectDisposedException obe) { return; }
        }

        internal void SetPrivateMode(bool v)
        {
            mPrivateMode = v;
        }

        public void StartReceiveMessages()
        {
            var endPoint = new IPEndPoint(IPAddress.Any, mListenPort);
            var s = new UdpState(endPoint, mUdpClient);
            mUdpClient.BeginReceive(new AsyncCallback(ReceiveCallback), s);

            // Record operation to Network Log
            mNetworkLogPathMutex.WaitOne();
            using (var streamWriter = File.AppendText(NetworkLogPath))
            {
                var currentTime = DateTime.Now.ToString("h: mm:ss tt");
                streamWriter.WriteLine("[" + currentTime + "]" + " " + "I am now started listening for UDP Datagrams, and ready to manage messages.");
                streamWriter.Close();
            }
            mNetworkLogPathMutex.ReleaseMutex();

        }

        public void StopReceiveMessages()
        {
            // Check the presence of the variable
            if (mUdpClient == null) {
                return;
            }

            mUdpClient.Close();
            mUdpClient.Dispose();
        }

        /// <summary>
        /// Send a specified message to a specified IP
        /// </summary>
        /// <param name="message"></param>
        /// <param name="ipReceiver"></param>
        public void SendMessage(string message, IPAddress ipReceiver)
        {
            var msg = Encoding.Unicode.GetBytes(message);
            var endPoint = new IPEndPoint(ipReceiver, mListenPort);

            try
            { 
                mUdpClient.Send(msg, msg.Length, endPoint);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }

            // Record operation to Network Log
            mNetworkLogPathMutex.WaitOne();
            using (var streamWriter = File.AppendText(NetworkLogPath))
            {
                var currentTime = DateTime.Now.ToString("h: mm:ss tt");
                var messageFormatted = JValue.Parse(message).ToString(Formatting.Indented);
                streamWriter.WriteLine("[" + currentTime + "]" + " " + "Sent to " + ipReceiver.ToString() + " the message: \r\n\r\n" + messageFormatted + "\r\n");
                streamWriter.Close();
            }
            mNetworkLogPathMutex.ReleaseMutex();
        }

        public void SendProfilePicture(IPAddress ipReceiver, string profilePictureFilename)
        {
            // Get the absolute path of the filename
            // (dynamic value: it changes over the local installations)
            var currWorkingDirectory = Directory.GetCurrentDirectory();
            var ProfilePicturesFolderPath = @"ProfilePictures";
            var path = Path.Combine(currWorkingDirectory, ProfilePicturesFolderPath, profilePictureFilename);

            // Verify the path existence
            if (!File.Exists(path))
            {
                // TODO: Instead of the System MessageBox show the Custom ErrorView
                if (DEBUG) Console.WriteLine("[ERROR] File path not valid when sending my profile picture: " + path);
                return;
            };

            // Send the file
            mTcpManager.SendFile(ipReceiver, path);

            // Stop the TCP Client and release resources
            mTcpManager.StopClient();

            // Record the operation to Network Log
            mNetworkLogPathMutex.WaitOne();
            using (var streamWriter = File.AppendText(NetworkLogPath))
            {
                var currentTime = DateTime.Now.ToString("h: mm:ss tt");
                streamWriter.WriteLine("[" + currentTime + "]" + " " + "Sent profile picture.");
                streamWriter.Close();
            }
            mNetworkLogPathMutex.ReleaseMutex();
        }

        /// <summary>
        /// Open TCP Manager and user it receive asynchronously profile pictures 
        /// </summary>
        public void WaitForProfilePictures()
        {
            mTcpManager.StartListener();
            mReceivePictureThread = new Thread(() => mTcpManager.ReceiveFile())
            {
                Priority = ThreadPriority.Highest
            };
            mReceivePictureThread.Start();

            // Record the operation to Network Log
            mNetworkLogPathMutex.WaitOne();
            using (var streamWriter = File.AppendText(NetworkLogPath))
            {
                var currentTime = DateTime.Now.ToString("h: mm:ss tt");
                streamWriter.WriteLine("[" + currentTime + "]" + " " + "I am now started listening for TCP Connections, and ready to manage profile pictures.");
                streamWriter.Close();
            }
            mNetworkLogPathMutex.ReleaseMutex();
        }

        /// <summary>
        /// Send a discovery message in broadcast manner to every user
        /// </summary>
        public void SendDiscoveryMessage()
        {
            var message = new Message(Message.MSG_DISCOVERY);
            SendMessage(Jsonify(message), mBROADCAST);
        }

        /// <summary>
        /// Send a bye message in broadcast manner to every user
        /// </summary>
        public void SendByeMessage()
        {
            var message = new Message(Message.MSG_LEAVE);
            SendMessage(Jsonify(message), mBROADCAST);
        }

        /// <summary>
        /// Send an update message to the specified user
        /// </summary>
        /// <param name="ReceiverIp"></param>
        public void SendUpdateMessage()
        {
            var message = new Message(Message.MSG_UPDATE, mRestrictedUser);
            var messageJ = Jsonify(message);

            SendMessage(messageJ, mBROADCAST);

            mNetworkLogPathMutex.WaitOne();
            using (var streamWriter = File.AppendText(NetworkLogPath))
            {
                var currentTime = DateTime.Now.ToString("h: mm:ss tt");
                streamWriter.WriteLine("[" + currentTime + "]" + " Sent Update Message to Broadcast.");
                streamWriter.Close();
            }
            mNetworkLogPathMutex.ReleaseMutex();

            foreach (var user in mRestrictedUsers)
            {
                var IP = IPAddress.Parse(user.mIpAddress);
                SendProfilePicture(IP, mRestrictedUser.mProfilePictureFilename);
            }

        }

        /// <summary>
        /// Return the updated list of restricted users
        /// </summary>
        /// <returns></returns>
        public List<RestrictedUser> GetRestrictedUsers()
        {
            return mRestrictedUsers;   
        }

        /// <summary>
        /// Get the local network informations
        /// </summary>
        /// <param name="ni"></param>
        private void CollectLocalNetworkInformations(NetworkInformation ni)
        {
            var list = ni.GetLocalNetworkInformation();
            mIP           = list[0];
            mSUBNET_MASK  = list[1];
            mBROADCAST    = list[2];
        }

        /// <summary>
        /// Convert a passed message to the related json object
        /// </summary>
        /// <param name="m"></param>
        private string Jsonify(Message m)
        {
            return JsonConvert.SerializeObject(m);
        }

        /// <summary>
        /// Get the Message from the jsonObject
        /// </summary>
        /// <param name="jsonObject"></param>
        /// <returns></returns>
        private Message Unjsonify(string jsonObject)
        {
            Message message;

            try
            {
                message = JsonConvert.DeserializeObject<Message>(jsonObject);
                
            }
            catch (Exception e)
            {
                MessageBox.Show("Error #JSON-001: The message received is not a Json Object.");
                message = new Message(Message.MSG_ERROR);
            }

            return message;

        }

        public void Release()
        {
            StopReceiveMessages();

            // Stop the TCP Manager
            mTcpManager.StopListener();
            mTcpManager.StopClient();
        }

    }
}
