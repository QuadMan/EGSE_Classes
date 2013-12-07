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

namespace EGSE.Protocols
{
    /// <summary>
    /// Базовый класс сообщения
    /// TODO: возможно, его вывести выше в иерархии, и использовать как базу для всего
    /// </summary>
    public class MsgBase : EventArgs
    {
        /// <summary>
        /// Данные сообщения
        /// </summary>
        public byte[] Data;
        /// <summary>
        /// Длина сообщения
        /// </summary>
        public int DataLen;
    }
    /// <summary>
    /// Класс обмена сообщениями по протоколам USB
    /// </summary>
    public class ProtocolMsgEventArgs : MsgBase
    {
        /// <summary>
        /// Адрес, по которому пришло сообщение
        /// </summary>
        public uint Addr;
        /// <summary>
        /// Конструктор события: декодером обнаружено сообщение 
        /// </summary>
        /// <param name="maxDataLen">Размер буфера</param>
        public ProtocolMsgEventArgs(uint maxDataLen)
        {
            Data = new byte[maxDataLen];
            DataLen = 0;
            Addr = 0;
        }
    }
    /// <summary>
    /// Класс ошибки кодера
    /// </summary>
    public class ProtocolErrorEventArgs : MsgBase
    {
        /// <summary>
        /// Позиция ошибки в буфере
        /// </summary>
        public uint ErrorPos;
        /// <summary>
        /// Признак ошибки
        /// </summary>
        public string Msg;
        /// <summary>
        /// Конструктор события: ошибка в кодере
        /// </summary>
        /// <param name="maxDataLen">Размер буфера</param>
        public ProtocolErrorEventArgs(uint maxDataLen)
        {
            Data = new byte[maxDataLen];
            DataLen = 0;
            ErrorPos = 0;
        }
    }
    /// <summary>
    /// Абстрактный класс протокола USB
    /// </summary>
    public abstract class ProtocolUSBBase
    {
        /// <summary>
        /// Сброс конечного автомата состояния протокола в исходное состояние 
        /// </summary>
        public abstract void Reset();
        /// <summary>
        /// Метод декодирования данных
        /// </summary>
        /// <param name="buf">Буфер с данными для декодирования</param>
        /// <param name="bufLen">Размер буфера с данными</param>
        public abstract void Decode(byte[] buf, int bufLen);
        /// <summary>
        /// Метод кодирования данных
        /// Если функция выполняется с ошибкой, bufOut = null
        /// </summary>
        /// <param name="addr">Адрес, по которому данные должны быть переданы</param>
        /// <param name="buf">Буфер для передачи</param>
        /// <param name="bufOut">Выходной буфер</param>
        /// <returns>false если функция выполнена с ошибкой</returns>
        public virtual bool Encode(uint addr, byte[] buf, out byte[] bufOut)
        {
            bufOut = null;
            return false;
        }
        /// <summary>
        /// Объявление делегата обработки ошибок протокола
        /// </summary>
        /// <param name="e">Класс описывающий ошибку протокола</param>
        public delegate void ProtocolErrorEventHandler(ProtocolErrorEventArgs e);
        /// <summary>
        /// Объявление события: возникновение ошибки протокола в декодере
        /// </summary>
        public event ProtocolErrorEventHandler GotProtocolError;
        /// <summary>
        /// Обертка события: возникновение ошибки протокола в декодере
        /// </summary>
        /// <param name="e">Класс описывающий ошибку протокола</param>
        protected virtual void OnProtocolError(ProtocolErrorEventArgs e)
        {
            if (GotProtocolError != null)
            {
                GotProtocolError(e);
            }

        }
        /// <summary>
        /// Объявление делегата обработки сообщений протокола
        /// </summary>
        /// <param name="e">Класс описывающий сообщение протокола</param>
        public delegate void ProtocolMsgEventHandler(ProtocolMsgEventArgs e);
        /// <summary>
        /// Объявление события: возникновение сообщения протокола в декодере
        /// </summary>
        public event ProtocolMsgEventHandler GotProtocolMsg;
        /// <summary>
        /// Обертка события: возникновение сообщения протокола в декодере
        /// </summary>
        /// <param name="e">Класс описывающий сообщение протокола</param>
        protected virtual void OnProtocolMsg(ProtocolMsgEventArgs e)
        {
            if (GotProtocolMsg != null)
            {
                GotProtocolMsg(e);
            }

        }
    }
}
