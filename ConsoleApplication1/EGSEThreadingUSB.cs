//-----------------------------------------------------------------------
// <copyright file="EGSEThreadingUSB.cs" company="IKI RSSI, laboratory №711">
//     Copyright (c) IKI RSSI, laboratory №711. All rights reserved.
// </copyright>
// <author>Семенов Александр</author>
//-----------------------------------------------------------------------

// TODO нужно доделать очередь команд в USB
// TODO ввести фоновые потоки, чтобы они закрывались при выходе из основной программы
namespace EGSE.Threading
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using EGSE.USB;
    using EGSE.Utilites;

    /// <summary>
    /// Класс потока чтения данных из USB в большой кольцевой буфер и записи команд в USB
    /// </summary>
    public class FTDIThread
    {
        /// <summary>
        /// минимальный размер буфера данных в FTDI, при котором считываем данные
        /// </summary>
        private const uint FTDI_MIN_BUF_SIZE = 1 * 1024;

        /// <summary>
        /// Размер кольцевого буфера входящих данных из USB (см. класс AManager) 
        /// </summary>
        private const uint FTDI_THREAD_BUF_SIZE_BYTES_DEFAULT = 100;

        /// <summary>
        /// время на закрытие потока
        /// </summary>
        private const int TIMEOUT_TO_CLOSING_THREAD = 1000;

        /// <summary>
        /// на сколько мс засыпаем, когда устройство не подключено 
        /// </summary>
        private const int FTDI_DEFAULT_SLEEP_WHEN_NOT_CONNECTED = 1000;

        /// <summary>
        /// Максимальный размер очереди команд в USB
        /// </summary>
        private const int FTDI_MAX_CMD_QUEUE_SIZE = 5;

        /// <summary>
        /// Очередь команд в USB
        /// </summary>
        private Queue<byte[]> _cmdQueue;

        /// <summary>
        /// поток чтения/записи в USB 
        /// </summary>
        private Thread _thread;

        /// <summary>
        /// доступ к функциям контроллера FTDI 
        /// </summary>
        private USBFTDI _ftdi;

        /// <summary>
        /// конфигурация устройства 
        /// </summary>
        private USBCfg _cfg;

        /// <summary>
        /// кольцевой буфер входящих данных 
        /// </summary>
        public BigBufferManager BigBuf;
        
        /// <summary>
        /// скорость получения данных по USB
        /// </summary>
        private float _speed;

        /// <summary>
        /// Для рассчета скорости, последнее значение тиков процессора
        /// </summary>
        private int _lastTickCount;

        /// <summary>
        /// Для рассчета скорости, сколько было прочитано данных за секунду
        /// </summary>
        private ulong _dataReaded;

        /// <summary>
        /// Флаг остановки процесса получения данных
        /// </summary>
        private volatile bool _terminateFlag;

        /// <summary>
        /// делегат, вызываемый при смене состояния подключения устройства 
        /// </summary>
        /// <param name="state">Новое состояние - true - подключен, false - отключен</param>
        public delegate void StateChangedHandleEvent(bool state);

        /// <summary>
        /// событие на изменение состояния подключения
        /// </summary>
        public StateChangedHandleEvent StateChangeEvent;
        
        /// <summary>
        /// Значение скорости приема данных из USB
        /// </summary>
        public float SpeedBytesSec
        {
            get
            {
                return _speed;
            }
        }

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="FTDIThread" />.
        /// Создаем поток, ожидающий подключения устройства с серийным номером
        /// </summary>
        /// <param name="serial">Серийный номер устройства</param>
        /// <param name="cfg">Параметры USB устройства</param>
        /// <param name="bufSize">Размер кольцевого буфера</param>
        public FTDIThread(string serial, USBCfg cfg, uint bufSize = FTDI_THREAD_BUF_SIZE_BYTES_DEFAULT)
        {
            _speed = 0;
            _lastTickCount = 0;
            _dataReaded = 0;
            _terminateFlag = false;

            _cmdQueue = new Queue<byte[]>();

            _cfg    = cfg;
            BigBuf  = new BigBufferManager(bufSize);
            _ftdi   = new USBFTDI(serial, _cfg);

            _thread = new Thread(Execution);
            _thread.IsBackground = true;
        }

        /// <summary>
        /// Запускаем поток.
        /// Был сделан отдельный запуск, так как программа (UI) не успевала проинициализироваться, 
        /// а устройство уже подключалось и передавало данные
        /// </summary>
        public void Start()
        {
            _thread.Start();
        }

        /// <summary>
        /// Функция добавляет в очередь сообщений новое
        /// Если прибор не подсоединен, то ничего и не добавляем
        /// </summary>
        /// <param name="data">Буфер для отправки в USB</param>
        /// <returns>если в очереди есть место, возвращает TRUE</returns>
        public bool WriteBuf(byte[] data)
        {
            if ((_ftdi.IsOpen) && (_cmdQueue.Count < FTDI_MAX_CMD_QUEUE_SIZE))
            {
                _cmdQueue.Enqueue(data);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Завершаем поток
        /// </summary>
        public void Finish()
        {
            _terminateFlag = true;
            _thread.Join(TIMEOUT_TO_CLOSING_THREAD);
        }

        /// <summary>
        /// Функция вычисляет скорость чтения данных по USB через замеры секундных интервалов и объема полученных за это время данных
        /// </summary>
        private void CalcSpeed(int bytesReaded)
        {
            int tickDelta = Environment.TickCount - _lastTickCount;
            if (tickDelta > 1000)
            {
                _lastTickCount = Environment.TickCount;
                _speed = (float)_dataReaded / tickDelta * 1000;
                _dataReaded = 0;
            }
            else
            {
                _dataReaded += (ulong)bytesReaded;
            }
        }

        /// <summary>
        /// основная функция потока чтения и записи данных в USB FTDI 
        /// </summary>
        private void Execution()
        {
            int bytesReaded = 0;
            bool _lastOpened = false;       // изначально устройство не подключено
            int bytesAvailable = 0;
            uint bytesWritten = 0;

            while (!_terminateFlag)
            {
                // устройство не открыто, пытаемся открыть
                if (!_ftdi.IsOpen)
                {
                    // очищаем очередь сообщений на отправку, если мы отсоединились
                    if (_cmdQueue.Count > 0)
                    {
                        _cmdQueue.Clear();
                    }
                    
                    if ((_ftdi.Open() == FTD2XXNET.FTDICustom.FT_STATUS.FT_OK) && (_ftdi.IsOpen))
                    {
                        // производим настройки устройства
                        // !config _ftdi cfg
                    }
                    else 
                    {
                        System.Threading.Thread.Sleep(FTDI_DEFAULT_SLEEP_WHEN_NOT_CONNECTED);    // пока устройство не подключено, поспим
                    }
                }
                else 
                {
                    // устройство открыто, читаем, сколько можно из буфера USB
                    // если есть команды для выдачи в USB
                    if (_cmdQueue.Count > 0)
                    {
                        byte[] _buf = _cmdQueue.Peek();
                        FTD2XXNET.FTDICustom.FT_STATUS res = _ftdi.WriteBuf(ref _buf, out bytesWritten);
                        if ((res == FTD2XXNET.FTDICustom.FT_STATUS.FT_OK) && (bytesWritten == _buf.Length))
                        {
                            _cmdQueue.Dequeue();
                        }
                    }

                    // данные в буфере USB накопилось достаточно, читаем их в буфер
                    if ((_ftdi.GetBytesAvailable(ref bytesAvailable) == FTD2XXNET.FTDICustom.FT_STATUS.FT_OK) && (bytesAvailable > FTDI_MIN_BUF_SIZE))
                    {
                        _ftdi.ReadBuf(BigBuf.WriteBuf, bytesAvailable, ref bytesReaded);
                        if (bytesReaded > 0)
                        {
                            BigBuf.MoveNextWrite(bytesReaded);
                            CalcSpeed(bytesReaded);
                            bytesReaded = 0;
                        }
                    }
                }

                // если состояние устройства изменилось
                if (_lastOpened != _ftdi.IsOpen)
                {
                    _lastOpened = _ftdi.IsOpen;
                    if (StateChangeEvent != null)
                    {
                        StateChangeEvent(_ftdi.IsOpen);           
                    }
                }

                System.Threading.Thread.Sleep(_cfg.Sleep);
            }
        }
    }
}