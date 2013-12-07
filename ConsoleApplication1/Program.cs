using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.IO;
using System.Collections.Specialized;

using EGSE.Utilites;
using EGSE.Protocols;
using EGSE.Threading;
using EGSE;

namespace ConsoleApplication1
{
    class Program
    {
        public static string ByteArrayToString(byte[] ba, int len)
        {
            string hex = BitConverter.ToString(ba, 0, len);
            return hex.Replace("-", " ");
        }
        //static Device _dev;

        static void onDecMsg(ProtocolMsgEventArgs msg)
        {
                //System.Console.WriteLine("addr={0}", msg1.addr);
                System.Console.WriteLine("addr={0} data={1}", msg.Addr, ByteArrayToString(msg.Data, msg.DataLen));
                //AManager.Run(msg as USBProtocolMsg);  
        }

        static void onErrMsg(ProtocolErrorEventArgs msg)
        {
            System.Console.WriteLine("msg={0} pos={2} data={1}", msg.Msg, ByteArrayToString(msg.Data, msg.DataLen), msg.ErrorPos);
        }

        static void fastDecode()
        {

        }

        static void TMDecoder(ProtocolMsgEventArgs msg)
        {
            //Array.Copy(msg.data, raw, msg.data.Length);
            //fastDecode();
        }

        static void Main(string[] args)
        {
            /* Для проверки  декодера из файла или буфера */
            ProtocolUSB5E4D dec = new ProtocolUSB5E4D();
            dec.GotProtocolMsg += new ProtocolUSBBase.ProtocolMsgEventHandler(onDecMsg);
            dec.GotProtocolError += new ProtocolUSBBase.ProtocolErrorEventHandler(onErrMsg);
            //*/
            FileStream fStream = new FileStream(@"D:\Projects\USBLOG\Debug\Win32\logs\201310\21_161433.dat", FileMode.Open);
            ProtocolThread dT = new ProtocolThread(dec, fStream);
            /* проверка работы модуля приема и декодирования данных */
           // FileStream fStream = new FileStream(@"D:\Projects\USBLOG\Release\Win32\logs\201310\usb_log.dat", FileMode.Create);
          //  TextWriter fTxtWriter = new StreamWriter(@"D:\Projects\USBLOG\Release\Win32\logs\201310\usb_log.txt");
          //  ProtocolUSB5E4D _dec = new ProtocolUSB5E4D(fStream, fTxtWriter, false, false);
          //  _dec.onMessage = onDecMsg;
           // _dec.onProtocolError = onErrMsg;
          //  _dev = new Device("FTVAFGPQ", _dec, new EGSE.USB.USBCfg(10));
          
            ///* выдаем команду в устройство */
            byte[] tmp = { 1 };
            byte[] tmpbuf;
            dec.Encode(0x03, tmp, out tmpbuf);
            /* ждем пока не нажмем кнопку в консоли, тогда все завершаем */
            System.Console.ReadLine();
            /* текстовый лог загрываем */
          //  _dev.finishAll();
          //  fTxtWriter.Flush();
          //  fTxtWriter.Close();
          //  fStream.Close();
        }
    }
}
