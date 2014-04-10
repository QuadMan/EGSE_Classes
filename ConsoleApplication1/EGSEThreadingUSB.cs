//-----------------------------------------------------------------------
// <copyright file="EGSEThreadingUSB.cs" company="IKI RSSI, laboratory №711">
//     Copyright (c) IKI RSSI, laboratory №711. All rights reserved.
// </copyright>
// <author>Семенов Александр</author>
//-----------------------------------------------------------------------

// TODO нужно доделать очередь команд в USB
// TODO ввести фоновые потоки, чтобы они закрывались при выходе из основной программы
namespace Egse.Threading
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using Egse.USB;
    using Egse.Utilites;

    /// <summary>
    /// Класс потока чтения данных из USB в большой кольцевой буфер и записи команд в USB
    /// </summary>
    public class FTDIThread
    {
        /// <summary>
        /// Минимальный размер буфера данных в FTDI, при котором считываем данные.
        /// </summary>
        private const uint FTDIMinBufferSize = 1 * 1024;

        /// <summary>
        /// Размер кольцевого буфера входящих данных из USB (см. класс AManager).
        /// </summary>
        private const uint FTDIThreadBufferSize = 100;

        /// <summary>
        /// Время на закрытие потока.
        /// </summary>
        private const int TimeoutToClosingThread = 1000;

        /// <summary>
        /// На сколько мс засыпаем, когда устройство не подключено.
        /// </summary>
        private const int FTDISleepWhenNotConnected = 1000;

        /// <summary>
        /// Максимальный размер очереди команд в USB.
        /// </summary>
        private const int FTDIMaxCMDQueueSize = 15;

        /// <summary>
        /// Очередь команд в USB.
        /// </summary>
        private Queue<byte[]> _cmdQueue;

        /// <summary>
        /// Поток чтения/записи в USB.
        /// </summary>
        private Thread _thread;

        /// <summary>
        /// Доступ к функциям контроллера FTDI.
        /// </summary>
        private USBFTDI _ftdi;

        /// <summary>
        /// Конфигурация устройства.
        /// </summary>
        private USBCfg _cfg;
        
        /// <summary>
        /// Скорость получения данных по USB.
        /// </summary>
        private float _speed;

        /// <summary>
        /// Для рассчета скорости, последнее значение тиков процессора.
        /// </summary>
        private int _lastTickCount;

        /// <summary>
        /// Значение размера трафика по USB.
        /// </summary>
        private long _trafic;

        /// <summary>
        /// Для рассчета скорости, сколько было прочитано данных за секунду.
        /// </summary>
        private long _dataReaded;

        /// <summary>
        /// Флаг остановки процесса получения данных.
        /// </summary>
        private volatile bool _terminateFlag;

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="FTDIThread" />.
        /// Создаем поток, ожидающий подключения устройства с серийным номером
        /// </summary>
        /// <param name="serial">Серийный номер устройства</param>
        /// <param name="cfg">Параметры USB устройства</param>
        /// <param name="bufSize">Размер кольцевого буфера</param>
        public FTDIThread(string serial, USBCfg cfg, uint bufSize = FTDIThreadBufferSize)
        {
            _speed = 0;
            _lastTickCount = 0;
            _dataReaded = 0;
            _terminateFlag = false;
            _cmdQueue = new Queue<byte[]>();
            _cfg = cfg;
            BigBuf = new BigBufferManager(bufSize);
            _ftdi = new USBFTDI(serial, _cfg);
            _thread = new Thread(Execution);
            _thread.IsBackground = true;
        }

        /// <summary>
        /// Делегат, вызываемый при смене состояния подключения устройства.
        /// </summary>
        /// <param name="state">Новое состояние - true - подключен, false - отключен</param>
        public delegate void StateChangedHandleEvent(bool state);

        /// <summary>
        /// Получает или задает кольцевой буфер входящих данных.
        /// </summary>
        public BigBufferManager BigBuf { get; set; }

        /// <summary>
        /// Получает или задает событие на изменение состояния подключения.
        /// </summary>
        public StateChangedHandleEvent StateChangeEvent { get; set; }

        /// <summary>
        /// Получает значение скорости приема данных из USB.
        /// </summary>
        /// <value>
        /// The speed bytes sec.
        /// </value>
        public float SpeedBytesSec
        {
            get
            {
                return _speed;
            }
        }

        /// <summary>
        /// Получает значение трафика по USB.
        /// </summary>
        /// <value>
        /// The trafic.
        /// </value>
        public long Trafic
        {
            get
            {
                return _trafic;
            }
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
            if (_ftdi.IsOpen && (_cmdQueue.Count < FTDIMaxCMDQueueSize))
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
            _thread.Join(TimeoutToClosingThread);
        }

        /// <summary>
        /// Процедура вычисляет скорость чтения данных по USB через замеры секундных интервалов и объема полученных за это время данных.
        /// </summary>
        /// <param name="bytesReaded">Количество принятых данных после последнего вызова процедуры</param>
        private void CalcSpeed(int bytesReaded)
        {
            int tickDelta = Environment.TickCount - _lastTickCount;
            if (tickDelta > 1000)
            {
                _lastTickCount = Environment.TickCount;
                _speed = (float)_dataReaded / tickDelta * 1000;
                _trafic += _dataReaded;
                _dataReaded = 0;
            }
            else
            {
                _dataReaded += bytesReaded;
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
                    
                    if ((_ftdi.Open() == FTD2XXNET.FTDICustom.FT_STATUS.FT_OK) && _ftdi.IsOpen)
                    {
                        // производим настройки устройства
                        // !config _ftdi cfg
                    }
                    else 
                    {
                        System.Threading.Thread.Sleep(FTDISleepWhenNotConnected);    // пока устройство не подключено, поспим
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
                    if ((_ftdi.GetBytesAvailable(ref bytesAvailable) == FTD2XXNET.FTDICustom.FT_STATUS.FT_OK) && (bytesAvailable > FTDIMinBufferSize))
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