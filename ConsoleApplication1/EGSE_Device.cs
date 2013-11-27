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
**
*/

using System;
using EGSE.Threading;
using EGSE.Decoders.USB;
using EGSE.USB;

namespace EGSE
{
    /// <summary>
    /// Общий класс устройства КИА
    /// При наследовании, необходимо в функции onDevStateChanged вызывать base.onDevStateChanged(state)
    /// </summary>
    public class Device
    {
        private DecoderThread _dThread;                         // поток декодирования данных из потока USB
        private FTDIThread _fThread;                            // поток чтения данных из USB
        private USBCfg _cfg;                                    // настройки устройства USB и потока чтения данных из USB
        private USBDecoder _dec;

         /// <summary>
        /// Создает процессы по чтению данных из USB и декодированию этих данных
        /// Все, что нужно, для обеспечения связи по USB
        /// </summary>
        /// <param name="Serial">Серийный номер USB устройства, с которого нужно получать данные</param>
        /// <param name="dec">Класс декодера, который нужно использовать в приборе</param>
        /// <param name="cfg">Конфигурация драйвера USB (настройка параметров потока, буферов чтения и тд)</param>
        public Device(string Serial, USBDecoder dec, USBCfg cfg)
        {
            _dec = dec;
            _cfg = cfg;
            _fThread = new FTDIThread(Serial, _cfg);
            _fThread.onStateChanged = onDevStateChanged;

            _dThread = new DecoderThread(_dec, _fThread);
        }

        ~Device()
        {
            _fThread.Finish();
            _dThread.Finish();
            // TODO: нужно ли ждать Join от потоков?
        }

        /// <summary>
        /// Функция вызывается когда меняется состояние подключения USB устройства
        /// Должна быть переопределена в потомке
        /// </summary>
        /// <param name="state">Состояние USB устройства - открыто (true) или закрыто (false)</param>
        virtual public void onDevStateChanged(bool state)
        {
            if (_dThread != null)
            {
                _dThread.resetDecoder();
            }
        }

        /// <summary>
        /// Функция выдает команду в USB
        /// </summary>
        /// <param name="addr">адрес, по которому нужно передать данные</param>
        /// <param name="data">сами данные</param>
        /// <returns></returns>
        public bool SendCmd(uint addr, byte[] data)
        {
            byte[] dataOut;

            _dec.encode(addr, data, out dataOut);
            bool res = _fThread.WriteBuf(dataOut);
            return res;
        }
    }
}