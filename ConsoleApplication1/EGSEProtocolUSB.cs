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
        /// Получает или задает данные сообщения.
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
        /// Получает или задает длину сообщения.
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
    /// Класс обмена сообщениями по протоколу Spacewire.
    /// </summary>
    public class SpacewireMsgEventArgs : MsgBase
    {
        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="SpacewireMsgEventArgs" />.
        /// </summary>
        /// <param name="msgDataLen">Длина текущего сообщения spacewire.</param>
        /// <param name="time1">Time tick 1</param>
        /// <param name="time2">Time tick 2</param>
        /// <param name="error">Ошибка в сообщении.</param>
        public SpacewireMsgEventArgs(uint msgDataLen, byte time1, byte time2, byte error = 0x00)
        {
            Data = new byte[msgDataLen];
            DataLen = 0;
            Error = error;
            Time1 = time1;
            Time2 = time2;
        }
        
        /// <summary>
        /// Получает или задает ошибку в приеме сообщения.
        /// </summary>
        /// <value>
        /// Метка ошибки.
        /// </value>
        public byte Error { get; set; }

        /// <summary>
        /// Получает или задает Time tick 1.
        /// </summary>
        /// <value>
        /// Значение Time tick 1.
        /// </value>
        public byte Time1 { get; set; }

        /// <summary>
        /// Получает или задает Time tick 2.
        /// </summary>
        /// <value>
        /// Значение Time tick 2.
        /// </value>
        public byte Time2 { get; set; }
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
        /// Инициализирует новый экземпляр класса <see cref="ProtocolMsgEventArgs" />.
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
        /// Инициализирует новый экземпляр класса <see cref="ProtocolErrorEventArgs" />.
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
        /// Сброс конечного автомата состояния протокола в исходное состояние .
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
