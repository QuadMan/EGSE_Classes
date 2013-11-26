/*** EDGE_Device.cs
**
** (с) 2013 Семенов Александр, ИКИ РАН
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
using EGSE.Decoders;
using EGSE.USB;

namespace EGSE
{
    /// <summary>
    /// Общий класс устройства КИА
    /// </summary>
    public class Device
    {
        private DecoderThread _dThread;                         // поток декодирования данных из потока USB
        private FTDIThread _fThread;                            // поток чтения данных из USB
        private USBCfg _cfg;                                    // настройки устройства USB и потока чтения данных из USB

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
            //
            if (state == true)
            {
                byte[] tBuf = { 1, 2 };
                _fThread.SendCmd(1,ref tBuf);
            }
        }

        public Device(string Serial, Decoder dec)
        {
            _fThread = new FTDIThread(Serial, _cfg);
            _fThread.onStateChanged = onDevStateChanged;

            _dThread = new DecoderThread(dec, _fThread);
        }

        ~Device()
        {

        }

        public bool SendCmd(uint addr, ref byte[] data)
        {
            _fThread.SendCmd(addr, ref data);
            return true;
        }
    }
}