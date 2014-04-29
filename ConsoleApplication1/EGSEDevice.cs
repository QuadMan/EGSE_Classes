//-----------------------------------------------------------------------
// <copyright file="EGSEDevice.cs" company="IKI RSSI, laboratory №711">
//     Copyright (c) IKI RSSI, laboratory №711. All rights reserved.
// </copyright>
// <author>Семенов Александр</author>
//-----------------------------------------------------------------------

// TODO продумать, как передавать между потоками сообщение, что декодеру нужно сделать сброс при перепоключении устройства
namespace Egse
{
    using Egse.Protocols;
    using Egse.Threading;
    using Egse.USB;
    using Egse.Utilites;
        
    /// <summary>
    /// Общий класс устройства КИА.
    /// Примичание:
    /// При наследовании, необходимо в функции OnDevStateChanged вызывать base.OnDevStateChanged(state).
    /// </summary>
    public class Device
    {
        /// <summary>
        /// Поток декодирования данных из потока USB.
        /// </summary>
        private ProtocolThread _decodeThread;

        /// <summary>
        /// Поток чтения данных из USB.
        /// </summary>
        private FTDIThread readThread;

        /// <summary>
        /// Настройки устройства USB и потока чтения данных из USB.
        /// </summary>
        private USBCfg _cfg;

        /// <summary>
        /// Протокол, исполтьзуемый в USB.
        /// </summary>
        private ProtocolUSBBase decoder;

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="Device" />.
        /// Создает процессы по чтению данных из USB и декодированию этих данных.
        /// </summary>
        /// <param name="serial">Серийный номер USB устройства, с которого нужно получать данные</param>
        /// <param name="dec">Класс декодера, который нужно использовать в приборе</param>
        /// <param name="cfg">Конфигурация драйвера USB (настройка параметров потока, буферов чтения и тд)</param>
        public Device(string serial, ProtocolUSBBase dec, USBCfg cfg)
        {
            this.decoder = dec;
            _cfg = cfg;
            this.readThread = new FTDIThread(serial, _cfg);            
            this.readThread.StateChangeEvent = OnDevStateChanged;
            _decodeThread = new ProtocolThread(this.decoder, this.readThread);
        }

        /// <summary>
        /// Определение делегата обработки сообщения.
        /// </summary>
        /// <param name="connected">Подключен блок или нет</param>
        public delegate void ChangeStateEventHandler(bool connected);

        /// <summary>
        /// Получает или задает делегат, вызываемый при распознавании очередного сообщения декодером.
        /// </summary>
        public ChangeStateEventHandler ChangeStateEvent { get; set; }

        /// <summary>
        /// Получает скорость приема данных.
        /// </summary>
        public float Speed
        {
            get
            {
                return this.readThread.SpeedBytesSec;
            }
        }

        /// <summary>
        /// Получает значение трафика данных.
        /// </summary>
        public long Trafic
        {
            get
            {
                return this.readThread.Trafic;
            }
        }

        /// <summary>
        /// Получает максимальный размер большого (глобального) буфера.
        /// </summary>
        public uint GlobalBufferSize
        {
            get
            {
                uint res = _decodeThread.MaxBufferSize;
                _decodeThread.MaxBufferSize = 0;
                return res;
            }
        }

        /// <summary>
        /// Получает количество байт доступных для приема из USB.
        /// </summary>
        /// <value>
        /// Количество байт доступных для приема из USB.
        /// </value>
        public int BytesAvailable
        {
            get
            {
                return this.readThread.BigBuf.BytesAvailable;
            }
        }

        /// <summary>
        /// Запуск потока декодера данных.
        /// </summary>
        public void Start()
        {
            this.readThread.Start();
        }

        /// <summary>
        /// Завершаем все потоки.
        /// </summary>
        public void FinishAll()
        {
            this.readThread.Finish();
            _decodeThread.Finish();
        }

        /// <summary>
        /// Функция вызывается когда меняется состояние подключения USB устройства.
        /// Примечание:
        /// Должна быть переопределена в потомке.
        /// </summary>
        /// <param name="connected">Состояние USB устройства - открыто (true) или закрыто (false)</param>
        public virtual void OnDevStateChanged(bool connected)
        {
            if ((_decodeThread != null) && connected)
            {
                // при подключении к устройству, делаем сброс декодера в исходное состояние
                _decodeThread.ResetDecoder();
            }

            if (ChangeStateEvent != null)
            {
                ChangeStateEvent(connected);
            }
        }

        /// <summary>
        /// Функция выдает команду в USB.
        /// </summary>
        /// <param name="addr">Адрес, по которому нужно передать данные</param>
        /// <param name="data">Данные для передачи</param>
        /// <returns>Возвращает результат записи в очередь команд USB.</returns>
        public bool SendToUSB(uint addr, byte[] data)
        {
            new { data }.CheckNotNull();
            
            if (0 != data.Length)
            {
                byte[] dataOut;
                this.decoder.Encode(addr, data, out dataOut);
                try
                {
                    if (this.readThread.TryWrite(dataOut))
                    {
                        return true;
                    }
                    else
                    {
                        System.Windows.MessageBox.Show(Resource.Get(@"eNotAddToQueue"));
                    }
                }
                catch (Egse.Threading.FTDIThread.DeviceNotOpenedException e)
                {
                    System.Windows.MessageBox.Show(e.Message);
                }
            }        

            return false;
        }
    }
}