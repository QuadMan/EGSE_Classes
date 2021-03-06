﻿//-----------------------------------------------------------------------
// <copyright file="EGSEProtocolUSB.cs" company="IKI RSSI, laboratory №711">
//     Copyright (c) IKI RSSI, laboratory №711. All rights reserved.
// </copyright>
// <author>Семенов Александр, Коробейщиков Иван</author>
//-----------------------------------------------------------------------

namespace EGSE.Protocols
{
    using System;

    /// <summary>
    /// Базовый класс сообщения.
    /// TODO: возможно, его вывести выше в иерархии, и использовать как базу для всего.
    /// </summary>
    public class MsgBase : EventArgs
    {
        /// <summary>
        /// Данные сообщения.
        /// </summary>
        private byte[] _data;

        /// <summary>
        /// Длина сообщения.
        /// </summary>
        private int _dataLen;

        /// <summary>
        /// Gets or sets data message.
        /// Считать или записать данные сообщения.
        /// </summary>
        public byte[] Data
        {
            get
            {
                return _data;
            }

            set
            {
                _data = value;
            }
        }

        /// <summary>
        /// Gets or sets длину сообщения.
        /// </summary>
        public int DataLen
        {
            get
            {
                return _dataLen;
            }

            set
            {
                _dataLen = value;
            }
        }
    }

    /// <summary>
    /// Класс обмена сообщениями по протоколам USB.
    /// </summary>
    public class ProtocolMsgEventArgs : MsgBase
    {
        /// <summary>
        /// Адрес, по которому пришло сообщение.
        /// </summary>
        private uint _addr;

        /// <summary>
        /// Initializes a new instance of the <see cref="ProtocolMsgEventArgs" /> class.
        /// Конструктор события: декодером обнаружено сообщение.
        /// </summary>
        /// <param name="maxDataLen">Размер буфера</param>
        public ProtocolMsgEventArgs(uint maxDataLen)
        {
            Data = new byte[maxDataLen];
            DataLen = 0;
            Addr = 0;
        }

        /// <summary>
        /// Gets or sets адрес, по которому пришло сообщение.
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
    public class ProtocolErrorEventArgs : MsgBase
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
        /// Initializes a new instance of the <see cref="ProtocolErrorEventArgs" /> class.
        /// Конструктор события: ошибка в кодере
        /// </summary>
        /// <param name="maxDataLen">Размер буфера</param>
        public ProtocolErrorEventArgs(uint maxDataLen)
        {
            Data = new byte[maxDataLen];
            DataLen = 0;
            ErrorPos = 0;
        }

        /// <summary>
        /// Gets or sets позиция ошибки в буфере
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
        /// Gets or sets признак ошибки
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
        /// <param name="e">Класс описывающий ошибку протокола</param>
        public delegate void ProtocolErrorEventHandler(ProtocolErrorEventArgs e);

        /// <summary>
        /// Объявление делегата обработки сообщений протокола
        /// </summary>
        /// <param name="e">Класс описывающий сообщение протокола</param>
        public delegate void ProtocolMsgEventHandler(ProtocolMsgEventArgs e);

        /// <summary>
        /// Объявление события: возникновение ошибки протокола в декодере
        /// </summary>
        public event ProtocolErrorEventHandler GotProtocolError;

        /// <summary>
        /// Объявление события: возникновение сообщения протокола в декодере
        /// </summary>
        public event ProtocolMsgEventHandler GotProtocolMsg;

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
        /// Обертка события: возникновение ошибки протокола в декодере
        /// </summary>
        /// <param name="e">Класс описывающий ошибку протокола</param>
        protected virtual void OnProtocolError(ProtocolErrorEventArgs e)
        {
            if (this.GotProtocolError != null)
            {
                this.GotProtocolError(e);
            }
        }

        /// <summary>
        /// Обертка события: возникновение сообщения протокола в декодере
        /// </summary>
        /// <param name="e">Класс описывающий сообщение протокола</param>
        protected virtual void OnProtocolMsg(ProtocolMsgEventArgs e)
        {
            if (this.GotProtocolMsg != null)
            {
                this.GotProtocolMsg(e);
            }
        }
    }
}
