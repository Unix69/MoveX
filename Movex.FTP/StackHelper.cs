using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Movex.FTP
{
    public class StackHelper<T>
    {
        private ConcurrentStack<T> mStack;
        public StackHelper()
        {
            mStack = new ConcurrentStack<T>();
        }
        public ConcurrentStack<T> GetStack() { return mStack; }
    }
}
