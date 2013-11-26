using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using EGSE;
using EGSE.Decoders;

namespace ConsoleApplication1
{

    class Program
    {

        void onNewMessage(ref ProtocolMsg msg)
        {

        }

        static void Main(string[] args)
        {
            Device    _dev;
            Dec5D4ECRC _dec;

            _dec = new Dec5D4ECRC();
            //!_dec.onMessage = onNewMessage;
            _dev = new Device("KBUNILG", _dec);
          
            System.Console.ReadLine();
        }
    }
}
