//-----------------------------------------------------------------------
// <copyright file="EGSEProtocolUSB.cs" company="IKI RSSI, laboratory №711">
//     Copyright (c) IKI RSSI, laboratory №711. All rights reserved.
// </copyright>
// <author>Семенов Александр, Коробейщиков Иван</author>
//-----------------------------------------------------------------------

namespace Egse.Protocols
{
    using System;
    using Egse.Utilites;

    /// <summary>
    /// Базовый класс сообщения.
    /// </summary>
    public class BaseMsgEventArgs : EventArgs
    {
        /// <summary>
        /// Данные сообщения.
        /// </summary>
        private byte[] data;

        /// <summary>
        /// Длина сообщения.
        /// </summary>
        private int dataLen;

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="BaseMsgEventArgs" />.
        /// </summary>
        /// <param name="data">"сырые" данные сообщения.</param>
        public BaseMsgEventArgs(byte[] data)
        {
            new { data }.CheckNotNull();

            this.data = data;
            this.dataLen = data.Length;
        }
      
        /// <summary>
        /// Получает или задает данные сообщения.
        /// </summary>
        protected byte[] Data
        {
            get
            {
                return this.data;
            }

            set
            {
                this.data = value;
            }
        }

        /// <summary>
        /// Получает или задает длину сообщения.
        /// </summary>
        protected int DataLen
        {
            get
            {
                return this.dataLen;
            }

            set
            {
                this.dataLen = value;
            }
        }

        /// <summary>
        /// Получает значение CRC, сгенерированного для данного сообщения.
        /// </summary>
        /// <value>
        /// Значение CRC, сгенерированного для данного сообщения.
        /// </value>
        protected virtual ushort NeededCrc
        {
            get
            {
                return Crc16.Get(this.data, this.data.Length);
            }
        }

        /// <summary>
        /// Преобразует данные экземпляра к массиву байт.
        /// </summary>
        /// <returns>Массив байт.</returns>
        public virtual byte[] ToArray()
        {
            return this.data;
        }

        /// <summary>
        /// Все посылки удовлетворяют условию.
        /// </summary>
        /// <param name="data">"сырые" данные посылки.</param>
        /// <returns><c>true</c> всегда.</returns>
        /// <exception cref="System.NotImplementedException">Нет реализации.</exception>
        protected internal static bool Test(byte[] data)
        {
            return true;
        }
    }

    /// <summary>
    /// Класс обмена сообщениями по протоколам USB.
    /// </summary>
    public class ProtocolMsgEventArgs : BaseMsgEventArgs
    {
        /// <summary>
        /// Адрес, по которому пришло сообщение.
        /// </summary>
        private uint _addr;

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="ProtocolMsgEventArgs" />.
        /// Конструктор события: декодером обнаружено сообщение.
        /// </summary>
        /// <param name="maxDataLen">Размер буфера</param>
        public ProtocolMsgEventArgs(uint maxDataLen)
            : base(new byte[maxDataLen])
        {
            base.DataLen = 0;
            Addr = 0;
        }

        /// <summary>
        /// Получает данные сообщения.
        /// </summary>
        public new byte[] Data 
        {
            get
            {
                return base.Data;
            }
        }

        /// <summary>
        /// Получает или задает длину сообщения.
        /// </summary>
        public new int DataLen
        {
            get
            {
                return base.DataLen;
            }

            set
            {
                base.DataLen = value;
            }
        }

        /// <summary>
        /// Получает или задает адрес, по которому пришло сообщение.
        /// </summary>
        public uint Addr
        {
            get
            {
                return _addr;
            }

            set
            {
                _addr = value;
            }
        }
    }

    /// <summary>
    /// Класс ошибки кодера
    /// </summary>
    public class ProtocolErrorEventArgs : BaseMsgEventArgs
    {
        /// <summary>
        /// Позиция ошибки в буфере
        /// </summary>
        private uint _errorPos;

        /// <summary>
        /// Признак ошибки
        /// </summary>
        private string _msg;

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="ProtocolErrorEventArgs" />.
        /// Конструктор события: ошибка в кодере
        /// </summary>
        /// <param name="maxDataLen">Размер буфера</param>
        public ProtocolErrorEventArgs(uint maxDataLen)
            : base(new byte[maxDataLen])
        {
            base.DataLen = 0;
            ErrorPos = 0;
        }

        /// <summary>
        /// Получает данные сообщения.
        /// </summary>
        public new byte[] Data
        {
            get
            {
                return base.Data;
            }
        }

        /// <summary>
        /// Получает длину сообщения.
        /// </summary>
        public new int DataLen
        {
            get
            {
                return base.DataLen;
            }
        }

        /// <summary>
        /// Получает или задает позицию ошибки в буфере.
        /// </summary>
        public uint ErrorPos
        {
            get
            {
                return _errorPos;
            }

            set
            {
                _errorPos = value;
            }
        }

        /// <summary>
        /// Получает или задает признак ошибки.
        /// </summary>
        public string Msg
        {
            get
            {
                return _msg;
            }

            set
            {
                _msg = value;
            }
        }
    }

    /// <summary>
    /// Абстрактный класс протокола USB
    /// </summary>
    public abstract class ProtocolUSBBase
    {
        /// <summary>
        /// Объявление делегата обработки ошибок протокола
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">Класс описывающий ошибку протокола</param>
        public delegate void ProtocolErrorEventHandler(object sender, ProtocolErrorEventArgs e);

        /// <summary>
        /// Объявление делегата обработки сообщений протокола.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">Класс описывающий сообщение протокола</param>
        public delegate void ProtocolMsgEventHandler(object sender, ProtocolMsgEventArgs e);

        /// <summary>
        /// Объявление события: возникновение ошибки протокола в декодере.
        /// </summary>
        public event ProtocolErrorEventHandler GotProtocolError;

        /// <summary>
        /// Объявление события: возникновение сообщения протокола в декодере.
        /// </summary>
        public event ProtocolMsgEventHandler GotProtocolMsg;

        /// <summary>
        /// Сброс конечного автомата состояния протокола в исходное состояние.
        /// </summary>
        public abstract void Reset();

        /// <summary>
        /// Метод декодирования данных.
        /// </summary>
        /// <param name="buf">Буфер с данными для декодирования.</param>
        /// <param name="bufLen">Размер буфера с данными.</param>
        public abstract void Decode(byte[] buf, int bufLen);

        /// <summary>
        /// Метод кодирования данных.
        /// Если функция выполняется с ошибкой, bufOut = null.
        /// </summary>
        /// <param name="addr">Адрес, по которому данные должны быть переданы.</param>
        /// <param name="buf">Буфер для передачи.</param>
        /// <param name="bufOut">Выходной буфер.</param>
        /// <returns>false если функция выполнена с ошибкой.</returns>
        public virtual bool Encode(uint addr, byte[] buf, out byte[] bufOut)
        {
            bufOut = null;
            return false;
        }

        public class MaxBufferFtdiException : ApplicationException
        {
            public MaxBufferFtdiException(string message)
                : base(message)
            {
            }
        }

        /// <summary>
        /// Обертка события: возникновение ошибки протокола в декодере.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">Класс описывающий ошибку протокола.</param>
        protected virtual void OnProtocolError(object sender, ProtocolErrorEventArgs e)
        {
            if (this.GotProtocolError != null)
            {
                this.GotProtocolError(sender, e);
            }
        }

        /// <summary>
        /// Обертка события: возникновение сообщения протокола в декодере.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">Класс описывающий сообщение протокола.</param>
        protected virtual void OnProtocolMsg(object sender, ProtocolMsgEventArgs e)
        {
            if (this.GotProtocolMsg != null)
            {
                this.GotProtocolMsg(sender, e);
            }
        }
    }
}
