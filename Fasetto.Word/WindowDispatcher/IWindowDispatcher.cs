using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Movex.View.WindowDispatcher
{
    interface IWindowDispatcher
    {
        void Init();
        void Start();
        void Stop();
    }
}
