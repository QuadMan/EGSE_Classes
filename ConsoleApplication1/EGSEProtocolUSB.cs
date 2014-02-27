//-----------------------------------------------------------------------
// <copyright file="EGSEProtocolUSB.cs" company="IKI RSSI, laboratory №711">
//     Copyright (c) IKI RSSI, laboratory №711. All rights reserved.
// </copyright>
// <author>Семенов Александр, Коробейщиков Иван</author>
//-----------------------------------------------------------------------

namespace EGSE.Protocols
{
    using System;
    using System.Linq;
    using System.Collections.Specialized;

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
        protected virtual byte[] ToArray()
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Класс обмена сообщениями по протоколу Spacewire.
    /// </summary>
    public class SpacewireSptpMsgEventArgs : MsgBase
    {
        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="SpacewireSptpMsgEventArgs" />.
        /// </summary>
        /// <param name="msgDataLen">Длина текущего сообщения spacewire.</param>
        /// <param name="time1">Time tick 1</param>
        /// <param name="time2">Time tick 2</param>
        /// <param name="error">Ошибка в сообщении.</param>
        public SpacewireSptpMsgEventArgs(byte[] data, byte time1, byte time2, byte error = 0x00)
        {
            if (4 <= data.Length)
            {
                Data = new byte[data.Length - 4];
                Array.Copy(data.Skip<byte>(4).ToArray(), Data, data.Length - 4);
                DataLen = data.Length - 4;
                MsgType = (Type)data[2];
                To = data[0];
                From = data[3];
                ProtocolID = data[1];
                Error = error;
                Time1 = time1;
                Time2 = time2;
            }
            else
            {
                Data = new byte[data.Length];
                Array.Copy(data, Data, data.Length);
                DataLen = data.Length;
                MsgType = Type.Error;
                To = 0;
                From = 0;
                ProtocolID = 0;
                Error = 0xFF;
                Time1 = 0;
                Time2 = 0;
            }
        }

        public byte From { get; set; }

        public byte To { get; set; }

        public byte ProtocolID { get; set; }

        public enum Type
        {
            Error = 0xFF,
            Request = 0x80,
            Reply = 0xC0,
            Data = 0x00
        }

        public Type MsgType { get; set; } 

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

        protected override byte[] ToArray()
        {
            throw new NotImplementedException();
        }
    }

    public class SpacewireIcdMsgEventArgs : SpacewireSptpMsgEventArgs
    {
        public SpacewireIcdMsgEventArgs(byte[] data, byte time1, byte time2, byte error = 0x00)
            : base(data, time1, time2, error)
        {
            if (6 <= DataLen)
            {
                byte[] buf = new byte[DataLen - 6];
                Array.Copy(Data.Skip<byte>(6).ToArray(), buf, DataLen - 6);
                Id = new BitVector32(ConvertToInt(Data.Take<byte>(2).ToArray()));
                Control = new BitVector32(ConvertToInt(Data.Take<byte>(4).ToArray().Skip<byte>(2).ToArray()));
                Size = new BitVector32(ConvertToInt(Data.Take<byte>(6).ToArray().Skip<byte>(4).ToArray()));
                Data = buf;
                DataLen = buf.Length;
            }
        }

        private int ConvertToInt(byte[] array)
        {
            int pos = 0;
            int result = 0;
            foreach (byte by in array)
            {
                result |= (int)(by << pos);
                pos += 8;
            }
            return result;
        }

        public BitVector32 Id { get; set; }
        public BitVector32 Control { get; set; }
        public BitVector32 Size { get; set; }
        protected override byte[] ToArray()
        {
            throw new NotImplementedException();
        }
    }

    public class SpacewireKbvMsgEventArgs : SpacewireIcdMsgEventArgs
    {
        public SpacewireKbvMsgEventArgs(byte[] data, byte time1, byte time2, byte error = 0x00)
            : base(data, time1, time2, error)
        {}
        protected override byte[] ToArray()
        {
            throw new NotImplementedException();
        }
    }

    public class SpacewireTmMsgEventArgs : SpacewireIcdMsgEventArgs
    {
        public SpacewireTmMsgEventArgs(byte[] data, byte time1, byte time2, byte error = 0x00)
            : base(data, time1, time2, error)
        {}
        protected override byte[] ToArray()
        {
            throw new NotImplementedException();
        }
    }

    public class SpacewireTkMsgEventArgs : SpacewireIcdMsgEventArgs
    {
        public SpacewireTkMsgEventArgs(byte[] data, byte time1, byte time2, byte error = 0x00)
            : base(data, time1, time2, error)
        {}
        protected override byte[] ToArray()
        {
            throw new NotImplementedException();
        }
    }

    public static class MsgWorker
    {
        public static SpacewireKbvMsgEventArgs AsKbv(this Array obj)
        {
            throw new NotImplementedException();
        }
        public static SpacewireKbvMsgEventArgs ToKbv(this Array obj)
        {
            throw new NotImplementedException();
        }

        public static SpacewireTmMsgEventArgs AsTm(this Array obj)
        {
            throw new NotImplementedException();
        }
        public static SpacewireTmMsgEventArgs ToTm(this Array obj)
        {
            throw new NotImplementedException();
        }

        public static SpacewireTkMsgEventArgs AsTk(this Array obj)
        {
            throw new NotImplementedException();
        }
        public static SpacewireTkMsgEventArgs ToTk(this Array obj)
        {
            throw new NotImplementedException();
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
