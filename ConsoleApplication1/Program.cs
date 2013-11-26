using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
//using EGSE;
//using EGSE.Decoders;

namespace ConsoleApplication1
{

    class Program
    {

        static Queue<byte[]> tQueue = new Queue<byte[]>(2);

        static void addData(ref byte[] data)
        {
            tQueue.Enqueue(data);
        }


        static void Main(string[] args)
        {
            /*
            Device    _dev;
            Dec5D4ECRC _dec;

            _dec = new Dec5D4ECRC();
            //!_dec.onMessage = onNewMessage;
            _dev = new Device("KBUNILG", _dec);
          
            System.Console.ReadLine();
             */

            byte[] tmpBuf = {1,2,3};

            addData(ref tmpBuf);
            tmpBuf = new byte[3]{4,5,6};
            addData(ref tmpBuf);
            tmpBuf = new byte[3] { 7, 8, 9 };
            addData(ref tmpBuf);
            System.Console.WriteLine(tQueue.Dequeue()[0]);
            System.Console.WriteLine(tQueue.Dequeue()[0]);
            System.Console.WriteLine(tQueue.Dequeue()[0]);
        }
    }
}
