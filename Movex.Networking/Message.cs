namespace Movex.Network
{
    class Message
    {
        #region Static members
        public static string MSG_PRESENTATION = "I_AM";
        public static string MSG_ACKNOWLEDGE  = "ACK";
        public static string MSG_DISCOVERY    = "HELLO";
        public static string MSG_UPDATE       = "UPDATE";
        public static string MSG_LEAVE        = "BYE";
        public static string MSG_ERROR        = "ERR";
        #endregion

        #region Public members
        public string mMessageType;
        public RestrictedUser mRestrictedUser;
        #endregion

        #region Constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        public Message()
        {
            mMessageType = null;
            mRestrictedUser = null;
        }
        public Message(string messageType)
        {
            mMessageType = messageType;
            mRestrictedUser = null;
        }
        public Message(string messageType, RestrictedUser restrictedUser) {
            mMessageType = messageType;
            mRestrictedUser = restrictedUser;
        }

        #endregion

        #region Getters
        public string GetMessageType()
        {
            return mMessageType;
        }
        public RestrictedUser GetRestrictedUser()
        {
            return mRestrictedUser;
        }
        #endregion
    }
}