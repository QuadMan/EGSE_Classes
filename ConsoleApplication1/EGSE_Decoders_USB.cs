/*** EDGE_Decoders.cs
**
** (с) 2013 ИКИ РАН
 *
 * Модуль декодеров протоколов USB
**
** Author: Семенов Александр, Коробейщиков Иван
** Project: КИА
** Module: EDGE Decoders
** Requires: 
** Comments:
 * - пришлось уйти от интерфейса в сторону абстрактного класса, так как необходимо привязаться к функциям абстрактного класса, а не интерфейса 
 * 
 * - нужно написать юнит-тест для декодера
**
** History:
**  0.1.0	(26.11.2013) -	Начальная версия
 *  0.1.1   (27.11.2013) - Поменял заголовки функций, будем пользоваться передачей массивов
**
*/

using System;

namespace EGSE.Decoders.USB
{
    /// <summary>
    /// Класс сообщения, которыми обменивается по USB 
    /// </summary>
    public class USBProtocolMsg
    {
        // адрес, которому принадлежат данные
        public uint addr;
        // данные
        public byte[] buf;
    }

    /// <summary>
    /// Абстрактный класс декодера
    /// </summary>
    public abstract class USBDecoder
    {
        // сброс конечного автомата состояния протокола в исходное состояние
        abstract public void reset();

        // функция декодирования буфера
        /// <summary>
        /// Функция декодирования буфера
        /// </summary>
        /// <param name="buf">буфер с данными для декодирования</param>
        abstract public void decode(byte[] buf);

        /// <summary>
        /// Функция кодирования данных
        /// </summary>
        /// <param name="addr">адрес, по которому данные должны быть переданы</param>
        /// <param name="buf">данные для передачи</param>
        /// <param name="bufOut">выходной буфер</param>
        abstract public void encode(uint addr, byte[] buf, out byte[] bufOut);

        /// <summary>
        /// Делегат, вызываемый при возникновении ошибки в декодере
        /// </summary>
        /// <param name="errBuf">буфер, содержащий ошибку</param>
        /// <param name="bufPos">указатель в буфере, где произошла ошибка</param>
        public delegate void onProtocolErrorDelegate(byte[] errBuf, uint bufPos);
        public onProtocolErrorDelegate onProtocolError;

        /// <summary>
        /// Делегат, вызываемый при распознавании очередного сообщения декодером
        /// </summary>
        /// <param name="msg"></param>
        public delegate void onMessageDelegate(out USBProtocolMsg msg);
        public onMessageDelegate onMessage;
    }

    /// <summary>
    /// Класс декодера протокола типа 5D 4E ADDR LEN_HI LEN_LO DATA...DATA CRC8
    /// </summary>
    public class USB_5D4ECRC : USBDecoder
    {
        public USB_5D4ECRC()
        {

        }

        override public void reset()
        {

        }

        override public void decode(byte[] buf)
        {

        }

        override public void encode(uint addr, byte[] buf, out byte[] bufOut)
        {
            bufOut = new byte[10];
        }

    }
}