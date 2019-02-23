using System.Net;
using System.Threading;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System;

namespace Movex.Network
{
    public class User
    {

        #region Private members

        private string mUsername;
        private string mProfilePicturePath;
        private string mMessage;
        private IPAddress mIpAddress;
        private List<RestrictedUser> mFriendList;
        private UdpManager mConnManager;
        private Thread mListenThread;
        private bool mPrivateMode;

        #endregion

        #region Constructor(s)

        /// <summary>
        /// Constructor for the User class
        /// </summary>
        public User(string username, string message, string profilePicturePath, bool mPrivateMode)
        {
            mUsername = username;
            mMessage = message;
            mProfilePicturePath = profilePicturePath;
            mIpAddress = new NetworkInformation().GetLocalNetworkInformation()[0];

            mFriendList = new List<RestrictedUser>();
            mConnManager = new UdpManager(RestrictMe(), mPrivateMode);
        }

        #endregion

        #region Getter(s) and Setter(s)

        /// <summary>
        /// Set the username of the User
        /// </summary>
        /// <param name="name"></param>
        public void SetUsername(string name)
        {
            mUsername = name;
            // Update the informations change
            mConnManager.SetRestrictedUser(RestrictMe());
            mConnManager.SendUpdateMessage();
        }

        /// <summary>
        /// Get the username
        /// </summary>
        public string GetUsername()
        {
            return mUsername;
        }

        /// <summary>
        /// Set the Profile Picture filename of the User
        /// </summary>
        /// <param profilePicture="profilePicture"></param>
        public void SetProfilePicturePath(string profilePicturePath)
        {
            mProfilePicturePath = profilePicturePath;
            // Update the informations change
            mConnManager.SetRestrictedUser(RestrictMe());
            mConnManager.SendUpdateMessage();
        }

        /// <summary>
        /// Set the Message from the User
        /// </summary>
        /// <param message="message"></param>
        public void SetMessage(string message)
        {
            mMessage = message;
            // Update the informations change
            mConnManager.SetRestrictedUser(RestrictMe());
            mConnManager.SendUpdateMessage();
        }

        /// <summary>
        /// Set the IpAddress value from a IPAddress object
        /// </summary>
        /// <param name="addr"></param>
        public void SetIpAddress(IPAddress addr) {
            mIpAddress = addr;
        }

        /// <summary>
        /// Set private mode for the User (make it hidden)
        /// </summary>
        /// <param name="v"></param>
        public void SetPrivateMode(bool v)
        {
            mPrivateMode = v;
            mConnManager.SetPrivateMode(v);
        }

        /// <summary>
        /// Return the list of friends
        /// </summary>
        /// <returns></returns>
        public List<RestrictedUser> GetFriendList()
        {
            return mFriendList;
        }

        /// <summary>
        /// Extract the username from the friend whose ipaddress matches 
        /// </summary>
        /// <param name="ipAddress"></param>
        /// <returns></returns>
        public string GetUsernameByIpAddress(string IpAddress)
        {
            // Search the user in the Friend List
            Console.WriteLine("[User.cs] [GetUsernameByIpAddress] Searching for a friend with IpAddress: " + IpAddress);
            foreach (var friend in mFriendList)
            {
                Console.WriteLine("[User.cs] [GetUsernameByIpAddress] Now evaluating friend: " + friend.mUsername);
                if (friend.mIpAddress.Equals(IpAddress))
                {
                    Console.WriteLine("[User.cs] [GetUsernameByIpAddress] User found. Returning: " + friend.mUsername);
                    return friend.mUsername;
                }
            }

            // If not found return null
            Console.WriteLine("[User.cs] [GetUsernameByIpAddress] No User found. Returning: null");
            return null;
        }

        #endregion

        #region Routine Method(s)

        /// <summary>
        /// Launch a thread that listen for messages and reply accordingly
        /// </summary>
        public void ListenForFriends()
        {
            mListenThread = new Thread(() => mConnManager.StartReceiveMessages())
            {
                Priority = ThreadPriority.Highest
            };
            mListenThread.Start();
        }

        /// <summary>
        /// Update the list of Friends of the user contacting the Connection Manager
        /// </summary>
        public void GetForFriend()
        {
            mFriendList = mConnManager.GetRestrictedUsers();
        }

        /// <summary>
        /// Send a message over the network for discovering friends
        /// </summary>
        public void SearchForFriends()
        {
            mConnManager.WaitForProfilePictures();
            mConnManager.SendDiscoveryMessage();
        }

        /// <summary>
        /// Send a message that contains the information of the user
        /// </summary>
        private void SendMyInfoTo(IPAddress receiverAddr)
        {
            mConnManager.SendMessage(ToJson(), receiverAddr);
        }

        /// <summary>
        /// Add a new friend to the list of friends
        /// </summary>
        /// <returns></returns>
        public void AddFriend(string FriendAsJsonObject)
        {
            var friend = new RestrictedUser();
            friend = JsonConvert.DeserializeObject<RestrictedUser>(FriendAsJsonObject);


            if (!(mFriendList.Contains(friend)) && !(RestrictedUser.RestrictedUserAreEqual(friend, RestrictMe())))
                mFriendList.Add(friend);
        }

        /// <summary>
        /// Send a message to friends for being gone
        /// </summary>
        public void SayByeToFriends()
        {
            mConnManager.SendByeMessage();
        }

        public void UpdateFriendsAboutMe()
        {
            mConnManager.SendUpdateMessage();
        }

        #endregion

        /// <summary>
        /// Collect the broadcast address specific information
        /// </summary>
        /// <returns></returns>
        public string GetTechnicalInformation_BroadcastAddress()
        {
            return mConnManager.mBROADCAST.ToString();
        }

        /// <summary>
        /// Jsonify the object to send its information over the network
        /// </summary>
        /// <returns></returns>
        public string ToJson()
        {
            var restrictedUser = new RestrictedUser()
            {
                mUsername = mUsername.ToString(),
                mIpAddress = mIpAddress.ToString(),
            };

            return JsonConvert.SerializeObject(restrictedUser);
        }

        /// <summary>
        /// Build the restricted version of me
        /// </summary>
        /// <returns></returns>
        private RestrictedUser RestrictMe()
        {
            return new RestrictedUser()
            {
                mUsername = mUsername,
                mIpAddress = mIpAddress.ToString(),
                mMessage = mMessage,
                mProfilePictureFilename = Path.GetFileName(mProfilePicturePath)
            };
        }

        /// <summary>
        /// Release all the resources of the User Object
        /// </summary>
        public void Release()
        {
            // Initialize the release
            SayByeToFriends();

            // Release all the resources
            mConnManager.Release();
            mListenThread.Join();
            mListenThread.Interrupt();
            mListenThread = null;
        }

    }
}