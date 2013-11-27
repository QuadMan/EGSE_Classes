/*** EDGE_THREADING_FTDI.cs
**
** (с) 2013 ИКИ РАН
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
 *  0.1.1   (27.11.2013) - Добавил рассчет скорости
**
*/

using System;
using EGSE.USB;
using System.Threading;
using EGSE.UTILITES;
using EGSE.Decoders;
using EGSE.Threading;
using System.Collections.Generic;

namespace EGSE.Threading
{
    /// <summary>
    /// Класс потока чтения данных из USB в большой кольцевой буфер и записи команд в USB
    /// </summary>
    class FTDIThread
    {
        const uint FTDI_MIN_BUF_SIZE = 2 * 1024;
        private Queue<byte[]> _cmdQueue;

        // на сколько мс засыпаем, когда устройство не подключено
        private const int FTDI_DEFAULT_SLEEP_WHEN_NOT_CONNECTED = 1000;
        // Размер кольцевого буфера входящих данных из USB (см. класс AManager)
        private const uint FTDI_THREAD_BUF_SIZE_BYTES_DEFAULT = 100;//30*1024*1024;
        // поток чтения/записи в USB
        private Thread _thread;
        // доступ к функциям контроллера FTDI
        private FTDI   _ftdi;
        // конфигурация устройства
        private USBCfg _cfg;
        // кольцевой буфер входящих данных
        public AManager bigBuf;
        //
        private uint _bytesWritten = 0;
        // скорость получения данных по USB
        private float _speed;
        //private int _tickCount;
        private int _tickDelta;
        private int _lastTickCount;
        private UInt64 _dataReaded;
        private volatile bool _terminateFlag;

        // делегат, вызываемый при смене состояния подключения устройства
        public delegate void onStateChangedDelegate(bool state);
        public onStateChangedDelegate onStateChanged;

        
                // есть ли данные для чтения из буфера
                public int dataBytesAvailable
                {
                    get
                    {
                        return bigBuf.getBytesAvailable;
                    }
                }
        /*
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
                */
        // скорость чтения данных из USB
        public float speedBytesSec
        {
            get
            {
                return _speed;
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
            _speed = 0;
            //_tickCount = 0;
            _tickDelta = 0;
            _lastTickCount = 0;
            _dataReaded = 0;
            _terminateFlag = false;

            _cmdQueue = new Queue<byte[]>();

            _cfg    = cfg;
            bigBuf  = new AManager(bufSize);
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
        /// Функция добавляет в очередь сообщений новое
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public bool WriteBuf(byte[] data)
        {
            if (_cmdQueue.Count < 5) {
                _cmdQueue.Enqueue(data);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Функция вычисляет скорость чтения данных по USB через замеры секундных интервалов и объема полученных за это время данных
        /// </summary>
        private void calcSpeed(uint bytesReaded)
        {
            _tickDelta = Environment.TickCount - _lastTickCount;
            if (_tickDelta > 1000)
            {
                _lastTickCount = Environment.TickCount;
                _speed = (float)_dataReaded / _lastTickCount * 1000;
                _dataReaded = 0;
            }
            else
            {
                _dataReaded += bytesReaded;
            }
        }

        // основная функция потока чтения и записи данных в USB FTDI
        private void Execution()
        {
            uint bytesReaded = 0;
            bool _lastOpened = false;       // изначально устройство не подключено
            uint bytesAvailable = 0;

            while (!_terminateFlag)
            {
                // устройство не открыто, пытаемся открыть
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
                // устройство открыто, читаем, сколько можно из буфера USB
                else {                                      
                    // если есть команды для выдачи в USB
                    if (_cmdQueue.Count > 0)
                    {
                        byte[] _buf = _cmdQueue.Dequeue();
                        FTD2XX_NET.FTDICustom.FT_STATUS res = _ftdi.WriteBuf(ref _buf,out _bytesWritten);
                        if ((res == FTD2XX_NET.FTDICustom.FT_STATUS.FT_OK) && (_bytesWritten == _cmdQueue.Peek().Length))
                        {
                            _cmdQueue.Dequeue();
                        }
                    }
                    //
                    if ((_ftdi.GetBytesAvailable(ref bytesAvailable) == FTD2XX_NET.FTDICustom.FT_STATUS.FT_OK) && (bytesAvailable > FTDI_MIN_BUF_SIZE))
                    {
                        _ftdi.ReadBuf(bigBuf.getWriteBuf, bytesAvailable, ref bytesReaded);
                        if (bytesReaded > 0)                   
                        {
                            System.Console.WriteLine("reading " + bytesReaded.ToString());
                            bytesReaded = 0;
                            calcSpeed(bytesReaded);
                        }
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

        public void Finish()
        {
            _terminateFlag = true;
        }
    }
}

