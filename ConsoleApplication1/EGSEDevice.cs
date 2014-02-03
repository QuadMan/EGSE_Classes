/*** EDGE_Device.cs
**
** (с) 2013 ИКИ РАН
 *
 * Модуль устройства USB
**
** Author: Семенов Александр
** Project: КИА
** Module: EDGE Device
** Requires: 
** Comments:
 * - продумать, как передавать между потоками сообщение, что декодеру нужно сделать сброс при перепоключении устройства
**
** History:
**  0.1.0	(26.11.2013) -	Начальная версия
 *  0.1.1   (04.12.2013) - Изменил комментарии для проверки git
**
*/
namespace EGSE
{
    using System;
    using EGSE.Protocols;
    using EGSE.Threading;
    using EGSE.USB;
        
    /// <summary>
    /// Общий класс устройства КИА
    /// При наследовании, необходимо в функции onDevStateChanged вызывать base.onDevStateChanged(state)
    /// </summary>
    public class Device
    {
        /// <summary>
        /// Скорость приема данных
        /// </summary>
        public float Speed
        {
            get
            {
                return _fThread.SpeedBytesSec;
            }
        }

        /// <summary>
        /// Максимальный размер большого (глобального) буфера
        /// </summary>
        public uint GlobalBufferSize
        {
            get
            {
                uint res = _dThread.MaxBufferSize;
                _dThread.MaxBufferSize = 0;
                return res;
            }
        }

        /// <summary>
        /// поток декодирования данных из потока USB
        /// </summary>
        private ProtocolThread _dThread;

        /// <summary>
        /// поток чтения данных из USB
        /// </summary>
        private FTDIThread _fThread;

        /// <summary>
        /// настройки устройства USB и потока чтения данных из USB
        /// </summary>
        private USBCfg _cfg; 

        /// <summary>
        /// протокол, исполтьзуемый в USB
        /// </summary>
        private ProtocolUSBBase _dec;

        /// <summary>
        /// Создает процессы по чтению данных из USB и декодированию этих данных
        /// Все, что нужно, для обеспечения связи по USB
        /// </summary>
        /// <param name="serial">Серийный номер USB устройства, с которого нужно получать данные</param>
        /// <param name="dec">Класс декодера, который нужно использовать в приборе</param>
        /// <param name="cfg">Конфигурация драйвера USB (настройка параметров потока, буферов чтения и тд)</param>
        public Device(string serial, ProtocolUSBBase dec, USBCfg cfg)
        {
            _dec = dec;
            _cfg = cfg;
            _fThread = new FTDIThread(serial, _cfg);
            _fThread.StateChangeEvent = OnDevStateChanged;

            _dThread = new ProtocolThread(_dec, _fThread);
        }

        /// <summary>
        /// Запуск потока декодера данных
        /// </summary>
        public void Start()
        {
            _fThread.Start();
        }

        /// <summary>
        /// Завершаем все потоки
        /// </summary>
        public void FinishAll()
        {
            _fThread.Finish();
            _dThread.Finish();
        }

        /// <summary>
        /// Определение делегата обработки сообщения
        /// </summary>
        /// <param name="connected">Подключен блок или нет</param>
        public delegate void ChangeStateEventHandler(bool connected);

        /// <summary>
        /// Делегат, вызываемый при распознавании очередного сообщения декодером
        /// </summary>
        public ChangeStateEventHandler ChangeStateEvent;

        /// <summary>
        /// Функция вызывается когда меняется состояние подключения USB устройства
        /// Должна быть переопределена в потомке
        /// </summary>
        /// <param name="connected">Состояние USB устройства - открыто (true) или закрыто (false)</param>
        virtual public void OnDevStateChanged(bool connected)
        {
            if ((_dThread != null) && connected)
            {
                // при подключении к устройству, делаем сброс декодера в исходное состояние
                _dThread.ResetDecoder();
            }

            if (ChangeStateEvent != null)
            {
                ChangeStateEvent(connected);
            }
        }

        /// <summary>
        /// Функция выдает команду в USB
        /// </summary>
        /// <param name="addr">адрес, по которому нужно передать данные</param>
        /// <param name="data">сами данные</param>
        /// <returns>Возвращает результат записи в очередь команд USB</returns>
        public bool SendCmd(uint addr, byte[] data)
        {
            byte[] dataOut;

            _dec.Encode(addr, data, out dataOut);
            bool res = _fThread.WriteBuf(dataOut);
            return res;
        }
    }
}