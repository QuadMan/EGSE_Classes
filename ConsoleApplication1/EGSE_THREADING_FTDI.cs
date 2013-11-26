/*** EDGE_THREADING_FTDI.cs
**
** (с) 2013 Семенов Александр, ИКИ РАН
 *
 * Модуль потока чтения данных из USB и запись команд в USB
**
** Author: Семенов Александр
** Project: КИА
** Module: EDGE USB FTDI
** Requires: 
** Comments:
 * - нужно доделать очередь команд в USB
**
** History:
**  0.1.0	(26.11.2013) -	Начальная версия
**
*/

using System;
using EGSE.USB;
using System.Threading;
using EGSE.UTILITES;
using EGSE.Decoders;
using EGSE.Threading;

namespace EGSE.Threading
{
    /// <summary>
    /// Класс потока чтения данных из USB в большой кольцевой буфер и записи команд в USB
    /// </summary>
    class FTDIThread
    {
        // на сколько мс засыпаем, когда устройство не подключено
        private const int FTDI_DEFAULT_SLEEP_WHEN_NOT_CONNECTED = 1000;
        // Размер кольцевого буфера входящих данных из USB (30 МБ)
        private const uint FTDI_THREAD_BUF_SIZE_BYTES_DEFAULT = 30*1024*1024;
        // поток чтения/записи в USB
        private Thread _thread;
        // доступ к функциям контроллера FTDI
        private FTDI   _ftdi;
        // конфигурация устройства
        private USBCfg _cfg;
        // кольцевой буфер входящих данных
        public CBuf bigBuf;

        // делегат, вызываемый при смене состояния подключения устройства
        public delegate void onStateChangedDelegate(bool state);
        public onStateChangedDelegate onStateChanged;

        // есть ли данные для чтения из буфера
        public uint dataBytesAvailable
        {
            get
            {
                return (curWritePos - curReadPos);
            }
        }

        // текущая позиция указателя чтения в кольцевом буфере
        public uint curReadPos
        {
            get
            {
                return bigBuf.curReadPos;
            }
        }

        // текущая позиция указателя записи в кольцевом буфере
        public uint curWritePos
        {
            get
            {
                return bigBuf.curWritePos;
            }
        }

        // поток получения и записи данных в USB FTDI
        /// <summary>
        /// Создаем поток, ожидающий подключения устройства с серийным номером
        /// </summary>
        /// <param name="Serial">Серийный номер устройства</param>
        /// <param name="cfg">Параметры USB устройства</param>
        /// <param name="bufSize">Размер кольцевого буфера</param>
        public FTDIThread(string Serial, USBCfg cfg, uint bufSize = FTDI_THREAD_BUF_SIZE_BYTES_DEFAULT)
        {
            _cfg    = cfg;
            bigBuf  = new CBuf(bufSize);
            _ftdi   = new FTDI(Serial,_cfg);

            _thread = new Thread(Execution);
            _thread.Start();
        }

        // деструктор потока USB FTDI
        ~FTDIThread()
        {
            _ftdi.Close();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="addr"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public bool SendCmd(uint addr, ref byte[] data)
        {
            uint bytesWritten = 0;

            //_ftdi.WriteBuf(ref data,bytesWritten);
            return true;
        }

        // основная функция потока чтения и записи данных в USB FTDI
        private void Execution()
        {
            uint bytesReaded = 0;
            bool _lastOpened = false;       // изначально устройство не подключено

            while (true)
            {
                if (!_ftdi.isOpen)
                {
                    if ((_ftdi.Open() == FTD2XX_NET.FTDICustom.FT_STATUS.FT_OK) && (_ftdi.isOpen))
                    {
                        // производим настройки устройства
                        //!config _ftdi cfg
                        System.Console.WriteLine("yahoo...");
                    }
                    else 
                    {
                        System.Console.WriteLine("connecting...");
                        System.Threading.Thread.Sleep(FTDI_DEFAULT_SLEEP_WHEN_NOT_CONNECTED);    // пока устройство не подключено, поспим
                    }
                }

                // читаем, сколько можно из буфера USB
                if (_ftdi.isOpen)
                {
                    _ftdi.ReadAll(ref bytesReaded);
                    if (bytesReaded > 0)                    // пишем полученные данные в кольцевой буфер
                    {
                        System.Console.WriteLine("reading " + bytesReaded.ToString());
                        bytesReaded = 0;
                        //!_ftdi._inBuf.CopyTo(_bigBuf, _bigBuf.curWritePos);
                    }
                }

                // если состояние устройства изменилось
                if (_lastOpened != _ftdi.isOpen)
                {
                    _lastOpened = _ftdi.isOpen;
                    if (onStateChanged != null)
                    {
                        onStateChanged(_ftdi.isOpen);           // вызываем делегата
                    }
                }

                System.Threading.Thread.Sleep(_cfg.sleep);
            }
        }
    }
}

