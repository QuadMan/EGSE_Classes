/*** EDGE_Decoders_USB.cs
**
** (с) 2013 ИКИ РАН
 *
 * Базовый класс модуля протоколов USB и сообщений
**
** Author: Семенов Александр, Коробейщиков Иван
** Project: КИА
** Module: EDGE Decoders USB
** Requires: 
** Comments:
**
** History:
**  0.1.0	(26.11.2013) - Начальная версия
 *  0.1.1   (27.11.2013) - Поменял заголовки функций, будем пользоваться передачей массивов
 *  0.1.2   (29.11.2013) - Выделил отдельный базовый класс протокола и сообщения протокола
**
*/

using System;

namespace EGSE.Decoders.USB
{
    /// <summary>
    /// Базовый класс сообщения
    /// TODO: возможно, его вывести выше в иерархии, и использовать как базу для всего
    /// </summary>
    public class MsgBase
    {
        /// <summary>
        /// Данные сообщения
        /// </summary>
        public byte[] data;
        /// <summary>
        /// Длина сообщения
        /// </summary>
        public int dataLen;
    }

    /// <summary>
    /// Класс обмена сообщениями по протоколам в USB
    /// </summary>
    public class USBProtocolMsg : MsgBase
    {
        /// <summary>
        /// Адрес, по которому пришло сообщение
        /// </summary>
        public uint addr;

        /// <summary>
        /// Создаем сообщение с заданными размером буфера (максимальным, который поддерживается
        /// текущим протоколом)
        /// </summary>
        /// <param name="maxDataLen">максимальный размер буфера данных</param>
        public USBProtocolMsg(uint maxDataLen)
        {
            data = new byte[maxDataLen];
            dataLen = 0;
            addr = 0;
        }

        /// <summary>
        /// Очищаем класс сообщения
        /// TODO: может быть это и не нужно
        /// </summary>
        public void clear()
        {
            dataLen = 0;
            addr = 0;
        }
    }

    /// <summary>
    /// Класс ошибки декодера, который выдается в делегат при обнаружении ошибки протокола
    /// </summary>
    public class USBProtocolErrorMsg : MsgBase
    {
        /// <summary>
        /// Позиция ошибки в буфере
        /// </summary>
        public uint bufPos;

        /// <summary>
        /// Создаем сообщение с ошибкой
        /// </summary>
        public USBProtocolErrorMsg(uint maxDataLen)
        {
            data = new byte[maxDataLen];
            dataLen = 0;
            bufPos = 0;
        }
    }

    /// <summary>
    /// Абстрактный класс протокола USB
    /// </summary>
    public abstract class USBProtocolBase
    {
        /// <summary>
        /// сброс конечного автомата состояния протокола в исходное состояние 
        /// </summary>
        abstract public void reset();

        /// <summary>
        /// Функция декодирования буфера
        /// </summary>
        /// <param name="buf">буфер с данными для декодирования</param>
        abstract public void decode(byte[] buf, int bufSize);

        /// <summary>
        /// Функция кодирования данных
        /// Если функция выполняется с ошибкой, bufOut = null
        /// </summary>
        /// <param name="addr">адрес, по которому данные должны быть переданы</param>
        /// <param name="buf">данные для передачи</param>
        /// <param name="bufOut">выходной буфер</param>
        abstract public bool encode(uint addr, byte[] buf, out byte[] bufOut);

        /// <summary>
        ///  Определение делегата обработки ошибок протокола
        /// </summary>
        /// <param name="errMsg">класс сообщения, порожденный от MsgBase, описывающиё ошибку</param>
        public delegate void onProtocolErrorDelegate(MsgBase errMsg);

        /// <summary>
        /// Делегат, вызываемый при возникновении ошибки в декодере
        /// </summary>
        public onProtocolErrorDelegate onProtocolError;

        /// <summary>
        /// Определение делегата обработки сообщения
        /// </summary>
        /// <param name="msg"></param>
        public delegate void onMessageDelegate(MsgBase msg);
        /// <summary>
        /// Делегат, вызываемый при распознавании очередного сообщения декодером
        /// </summary>
        public onMessageDelegate onMessage;
    }
}
