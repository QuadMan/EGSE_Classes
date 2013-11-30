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
//using EGSE;
//using EGSE.Decoders;

namespace ConsoleApplication1
{

    class Program
    {
        static Device _dev;
        static USB_7C6EDecoder _dec;

        static void onDecMsg(MsgBase msg)
        {
            //USBProtocolMsg msg1 = msg as USBProtocolMsg;
            //if (msg1 != null) {
            //    System.Console.WriteLine("addr={0}", msg1.addr);
            //}
            //System.Console.WriteLine("errCnt={0}", _dec.errorsCount);
        }

        static void onErrMsg(MsgBase msg)
        {
            USBProtocolErrorMsg msg1 = msg as USBProtocolErrorMsg;
            if (msg1 != null) {
                //System.Console.WriteLine("addr={0}", msg1.addr);
            }
        }


        static void Main(string[] args)
        {
            /* Для проверки  декодера из файла или буфера */
            //USB_7C6EDecoder dec = new USB_7C6EDecoder();
            ////dec.onMessage = onDecMsg;
            ///*
            //byte[] tmpBuf = new byte[10];
            //tmpBuf[0] = 0x7C;
            //tmpBuf[1] = 0x6E;
            //tmpBuf[2] = 1;
            //tmpBuf[3] = 2;
            //tmpBuf[4] = 0xAA;
            //tmpBuf[5] = 0xBB;
            //dec.decode(tmpBuf);
            //*/
            //FileStream fStream = new FileStream(@"d:\PROJECTS\USB_FTDI_LOG_BUNI_LG.dat", FileMode.Open);
            //DecoderThread dT = new DecoderThread(dec, fStream);

            /* проверка работы модуля приема и декодирования данных */
            //FileStream fStream = new FileStream(@"d:\PROJECTS\usb_log_KIA_BUNI_LG.dat", FileMode.Create);
            TextWriter fTxtWriter = new StreamWriter(@"d:\PROJECTS\usb_log_KIA_BUNI_LG.txt");//, FileMode.Create);
            _dec = new USB_7C6EDecoder(null,fTxtWriter,false,true);//fStream, true);
            _dec.onMessage = onDecMsg;
            _dec.onProtocolError = onErrMsg;
            _dev = new Device("KBUNILG", _dec, new EGSE.USB.USBCfg(10));
          
            /* выдаем команду в устройство */
            byte[] tmp = {6};
            _dev.SendCmd(0xE,tmp);
            /* ждем пока не нажмем кнопку в консоли, тогда все завершаем */
            System.Console.ReadLine();
            /* текстовый лог загрываем */
            fTxtWriter.Flush();
            fTxtWriter.Close();
            //fStream.Close();
        }
    }
}
