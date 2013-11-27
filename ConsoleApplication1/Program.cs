using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using EGSE.UTILITES;
//using EGSE;
//using EGSE.Decoders;

namespace ConsoleApplication1
{

    class Program
    {
        const int gMax = 100000000;
        static Queue<byte[]> tQueue = new Queue<byte[]>(2);

        static void addData(ref byte[] data)
        {
            tQueue.Enqueue(data);
        }

        static void addData(ref byte[] data, int offset, int dataSz) {
            tQueue.Enqueue(data);
        }

//        static void addData(byte *dataPtr, int dataSz)
//        {
        
        //}

        /*
        internal unsafe struct FixedBytes
        {
            unsafe fixed byte bigBuf2[gMax];


            unsafe public void testUnsafe()
            {
                // Pin the buffer to a fixed location in memory.
                fixed (byte* bPtr = bigBuf2)
                {
                    uint i = 0;
                    while (i++ < gMax)
                    {
                        *(bPtr+i) = (byte)i;
                    }
                }
            }
        }
        */
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

            /*
            byte[] tmpBuf = {1,2,3};

            addData(ref tmpBuf);
            tmpBuf = new byte[3]{4,5,6};
            addData(ref tmpBuf);
            tmpBuf = new byte[3] { 7, 8, 9 };
            addData(ref tmpBuf);
            System.Console.WriteLine(tQueue.Dequeue()[0]);
            System.Console.WriteLine(tQueue.Dequeue()[0]);
            System.Console.WriteLine(tQueue.Dequeue()[0]);
             */
            /*
            PerfCounter pTimer = new PerfCounter();
            byte[] bigBuf = new byte[gMax];
            pTimer.Start();
            for (uint i = 0; i < gMax; i++)
            {
                bigBuf[i] = 255;
            }
            System.Console.WriteLine("1:{0}", pTimer.Finish());
            System.Console.ReadLine();
             */
            //
            /*
            FixedBytes bigStruct = new FixedBytes();
            pTimer.Start();
            bigStruct.testUnsafe();
            System.Console.WriteLine("2:{0:### ### ##0.0000}", pTimer.Finish());
            System.Console.ReadLine();
             */
        }
    }
}
