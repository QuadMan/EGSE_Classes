using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using EGSE.UTILITES;
using EGSE.Decoders.USB;
using EGSE.Threading;
using EGSE;
using System.IO;
using System.Collections.Specialized;
//using EGSE;
//using EGSE.Decoders;


namespace ConsoleApplication1
{
    class Program
    {
        public static string ByteArrayToString(byte[] ba, int len)
        {
            string hex = BitConverter.ToString(ba, 0, len);
            return hex.Replace("-", " ");
        }
        static Device _dev;

        static void onDecMsg(MsgBase msg)
        {
            USBProtocolMsg msg1 = msg as USBProtocolMsg;
            if (msg1 != null)
            {
                //System.Console.WriteLine("addr={0}", msg1.addr);
                //System.Console.WriteLine("addr={0} data={1}", msg1.addr, ByteArrayToString(msg1.data, msg1.dataLen));
                //AManager.Run(msg as USBProtocolMsg);
            }
            //System.Console.WriteLine("errCnt={0}", _dec.errorsCount);
        }

        static void onErrMsg(MsgBase msg)
        {
            USBProtocolErrorMsg msg1 = msg as USBProtocolErrorMsg;
            if (msg1 != null) {
                System.Console.WriteLine("msg={0} pos={2} data={1}", msg1.Msg, ByteArrayToString(msg1.data, msg1.dataLen), msg1.bufPos);
            }
        }

        static void fastDecode()
        {

        }

        static void TMDecoder(USBProtocolMsg msg)
        {
            //Array.Copy(msg.data, raw, msg.data.Length);
            //fastDecode();
        }

        static void Main(string[] args)
        {
            /* Для проверки  декодера из файла или буфера */
          //  Decoder5E4D dec = new Decoder5E4D();
          //  dec.onMessage = onDecMsg;
           // dec.onProtocolError = onErrMsg;
            //*/
            //FileStream fStream = new FileStream(@"D:\Projects\USBLOG\Release\Win32\logs\201310\tmpBin.dat", FileMode.Open);
            //DecoderThread dT = new DecoderThread(dec, fStream);

            /* проверка работы модуля приема и декодирования данных */
            FileStream fStream = new FileStream(@"D:\Projects\USBLOG\Release\Win32\logs\201310\usb_log.dat", FileMode.Create);
            TextWriter fTxtWriter = new StreamWriter(@"D:\Projects\USBLOG\Release\Win32\logs\201310\usb_log.txt");
            Decoder5E4D _dec = new Decoder5E4D(fStream, fTxtWriter, false, false);
            _dec.onMessage = onDecMsg;
            _dec.onProtocolError = onErrMsg;
            _dev = new Device("FTVAFGPQ", _dec, new EGSE.USB.USBCfg(10));
          
            ///* выдаем команду в устройство */
            byte[] tmp = { 1 };
            _dev.SendCmd(0x03, tmp);
            /* ждем пока не нажмем кнопку в консоли, тогда все завершаем */
            System.Console.ReadLine();
            /* текстовый лог загрываем */
            _dev.finishAll();
            fTxtWriter.Flush();
            fTxtWriter.Close();
            fStream.Close();
        }
    }
}
