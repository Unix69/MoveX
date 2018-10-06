using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Movex.FTP
{
    class FTPsupporter
    {
        // general send protocol attributes and fields
        
            
        //Type of Sends
        public const int ToSfileonly = 1;
        public const int ToSfileandtree = 2;
        public const int ToStreeonly = 3;
        
        //General Tags
        public const int Filesend = 0; 
        public const int Multifilesend = 1;
        public const int Treesend = 2;
        public const int Multitreesend = 3;

        //File send attributes and fields
        public const int Filenamelensize = 4;
        public const int Filesizesize = 4;
        public const int Tagsize = 4;
        public const int Numfilesize = 4;  
        public const int Headersize = 4 + 4;
        public const int Parallel = 0;
        public const int Serial = 1; 
        
        //Tree send attributes and fields
        public const int Elementsizesize = 4;
        public const int Elementdepthsize = 4;
        public const int Elementlensize = 4;
        public const int Numelementsize = 4;
        public const int Depthsize = 4;
      

        //Added operative attributes
        public const int Numchannelssize = 4;
        public const int Datelensize = 4;
        public const int Pathlensize = 4;
        public const int Iplensize = 4;
        public const int Default_priority_limit = 50;
        public const int Port = 2000;
       
        //unknowns
        public const string UnknownString = "unknown";
        public const int UnknownInt = 0;
        public const double UnknownDouble = 0;
        public const long UnknownLong = 0;
        public const int UnknownTag = 10;


    }
}
