using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Movex.Network
{
    public class RestrictedUser
    {
        #region Public members
        public string mUsername;
        public string mIpAddress;
        public string mMessage;
        public string mProfilePictureFilename;
        #endregion

        #region Routine Methods
        /// <summary>
        /// Comparator
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static bool RestrictedUserAreEqual(RestrictedUser a, RestrictedUser b)
        {
            if (a.mUsername != b.mUsername)
                return false;

            if (a.mIpAddress != b.mIpAddress)
                return false;

            if (a.mMessage != b.mMessage)
                return false;

            if (a.mProfilePictureFilename != b.mProfilePictureFilename)
                return false;

            return true;
        }
        #endregion
    }
}
