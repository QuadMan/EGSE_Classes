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
** TODO: ввести фоновые потоки, чтобы они закрывались при выходе из основной программы
** History:
**  0.1.0	(26.11.2013) -	Начальная версия
 *  0.1.1   (27.11.2013) - Добавил рассчет скорости
**
*/

using System;
using System.Threading;
using System.Collections.Generic;

using EGSE.USB;
using EGSE.Utilites;
using EGSE.Protocols;
using EGSE.Threading;

namespace EGSE.Threading
{
    /// <summary>
    /// Класс потока чтения данных из USB в большой кольцевой буфер и записи команд в USB
    /// </summary>
    class FTDIThread
    {
        const uint FTDI_MIN_BUF_SIZE = 1 * 1024;
        private Queue<byte[]> _cmdQueue;
        // время на закрытие потока
        private const int TIMEOUT_TO_CLOSING_THREAD = 1000;
        // на сколько мс засыпаем, когда устройство не подключено
        private const int FTDI_DEFAULT_SLEEP_WHEN_NOT_CONNECTED = 1000;
        // Размер кольцевого буфера входящих данных из USB (см. класс AManager)
        private const uint FTDI_THREAD_BUF_SIZE_BYTES_DEFAULT = 100;
        // поток чтения/записи в USB
        private Thread _thread;
        // доступ к функциям контроллера FTDI
        private USBFTDI   _ftdi;
        // конфигурация устройства
        private USBCfg _cfg;
        // кольцевой буфер входящих данных
        public BigBufferManager bigBuf;
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
            bigBuf  = new BigBufferManager(bufSize);
            _ftdi   = new USBFTDI(Serial,_cfg);

            _thread = new Thread(Execution);
            _thread.IsBackground = true;
            //_thread.Start();
        }

        public void Start()
        {
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
        private void calcSpeed(int bytesReaded)
        {
            _tickDelta = Environment.TickCount - _lastTickCount;
            if (_tickDelta > 1000)
            {
                _lastTickCount = Environment.TickCount;
                _speed = (float)_dataReaded / _tickDelta * 1000;
                _dataReaded = 0;
            }
            else
            {
                _dataReaded += (UInt64)bytesReaded;
            }
        }

        // основная функция потока чтения и записи данных в USB FTDI
        private void Execution()
        {
            int bytesReaded = 0;
            bool _lastOpened = false;       // изначально устройство не подключено
            int bytesAvailable = 0;

            while (!_terminateFlag)
            {
                // устройство не открыто, пытаемся открыть
                if (!_ftdi.isOpen)
                {
                    if ((_ftdi.Open() == FTD2XXNET.FTDICustom.FT_STATUS.FT_OK) && (_ftdi.isOpen))
                    {
                        // производим настройки устройства
                        //!config _ftdi cfg
                    }
                    else 
                    {
                        System.Threading.Thread.Sleep(FTDI_DEFAULT_SLEEP_WHEN_NOT_CONNECTED);    // пока устройство не подключено, поспим
                    }
                }
                // устройство открыто, читаем, сколько можно из буфера USB
                else {                                      
                    // если есть команды для выдачи в USB
                    if (_cmdQueue.Count > 0)
                    {
                        byte[] _buf = _cmdQueue.Peek();
                        FTD2XXNET.FTDICustom.FT_STATUS res = _ftdi.WriteBuf(ref _buf,out _bytesWritten);
                        if ((res == FTD2XXNET.FTDICustom.FT_STATUS.FT_OK) && (_bytesWritten == _buf.Length))
                        {
                            _cmdQueue.Dequeue();
                        }
                    }
                    //
                    if ((_ftdi.GetBytesAvailable(ref bytesAvailable) == FTD2XXNET.FTDICustom.FT_STATUS.FT_OK) && (bytesAvailable > FTDI_MIN_BUF_SIZE))
                    {
                            _ftdi.ReadBuf(bigBuf.writeBuf, bytesAvailable, ref bytesReaded);
                            if (bytesReaded > 0)
                            {
                                bigBuf.moveNextWrite(bytesReaded);

                                calcSpeed(bytesReaded);
#if DEBUG_TEXT
                                System.Console.WriteLine("speed = {0}", speedBytesSec);
#endif
                                bytesReaded = 0;
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
            _thread.Join(TIMEOUT_TO_CLOSING_THREAD);
        }
    }
}

