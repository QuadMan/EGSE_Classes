using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FTD2XX_NET;

namespace ConsoleApplication1
{
    class Program
    {
        
        static void Main(string[] args)
        {
            FTDI ftdi = new FTDI();
            ftdi.OpenBySerialNumber("test");
            System.Console.WriteLine("ohohoh");
            ftdi.Close();
            System.Console.ReadLine();
        }
    }
}
