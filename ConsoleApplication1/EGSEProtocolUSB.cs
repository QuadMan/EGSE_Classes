//-----------------------------------------------------------------------
// <copyright file="EGSEProtocolUSB.cs" company="IKI RSSI, laboratory №711">
//     Copyright (c) IKI RSSI, laboratory №711. All rights reserved.
// </copyright>
// <author>Семенов Александр, Коробейщиков Иван</author>
//-----------------------------------------------------------------------

namespace EGSE.Protocols
{
    using EGSE.Utilites;
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Linq;

    /// <summary>
    /// Для преобразования сообщений spacewire к сообщениям верхнего уровня.
    /// </summary>
    public static class MsgWorker
    {
        public static SpacewireKbvMsgEventArgs AsKbv(this Array obj)
        {
            throw new NotImplementedException();
        }

        public static SpacewireKbvMsgEventArgs AsKbv(this SpacewireIcdMsgEventArgs obj)
        {
            SpacewireKbvMsgEventArgs newObj = new SpacewireKbvMsgEventArgs(new byte[] { }, obj.Time1, obj.Time2, obj.Error);
            newObj.FieldPNormal = obj.Data[0];
            newObj.FieldPExtended = obj.Data[1];
            newObj.Kbv = obj.ConvertToInt(obj.Data.Skip(2).ToArray());
            return newObj;
        }

        public static SpacewireKbvMsgEventArgs ToKbv(this Array obj)
        {
            throw new NotImplementedException();
        }

        public static SpacewireTmMsgEventArgs AsTm(this Array obj)
        {
            throw new NotImplementedException();
        }

        public static SpacewireTmMsgEventArgs AsTm(this SpacewireIcdMsgEventArgs obj)
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

        public static SpacewireTkMsgEventArgs AsTk(this SpacewireIcdMsgEventArgs obj)
        {
            throw new NotImplementedException();
        }

        public static SpacewireTkMsgEventArgs ToTk(this Array obj, byte apid, Dictionary<byte, AutoCounter> dict)
        {
            byte[] buf = new byte[obj.Length];
            Array.Copy(obj, buf, buf.Length);
            return new SpacewireTkMsgEventArgs(apid, dict, buf);
        }
    }

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

        public virtual byte[] ToArray()
        {
            throw new NotImplementedException();
        }
    }

    public class SpacewireTimeTickMsgEventArgs : MsgBase
    {
        public SpacewireTimeTickMsgEventArgs()
        { 

        }

        public SpacewireTimeTickMsgEventArgs(byte data)
        {
            Data = new byte[1] { (byte)(data & 0x1F) };
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
        public SpacewireSptpMsgEventArgs()
        { 
        }

        public SpacewireSptpMsgEventArgs(byte[] data)
        {
            Data = new byte[data.Length];
            Array.Copy(data, Data, data.Length);
            DataLen = Data.Length;
            MsgType = Type.Data;
            To = 0x00;
            From = 0x00;
            ProtocolID = 0x00;
            Error = 0x00;
            Time1 = 0x00;
            Time2 = 0x00;
        }

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="SpacewireSptpMsgEventArgs" />.
        /// </summary>
        /// <param name="data">The data.</param>
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

        /// <summary>
        /// Тип Spacewire SPTP сообщения. 
        /// </summary>
        public enum Type
        {
            /// <summary>
            /// Сообщение не декодируемо.
            /// </summary>
            Error = 0xFF,

            /// <summary>
            /// Сообщение "Запрос кредита".
            /// </summary>
            Request = 0x80,

            /// <summary>
            /// Сообщение "Предоставления кредита".
            /// </summary>
            Reply = 0xC0,

            /// <summary>
            /// Сообщение "Данные".
            /// </summary>
            Data = 0x00
        }

        /// <summary>
        /// Получает или задает поле spacewire "Устройство передачи".
        /// </summary>
        /// <value>
        /// Идентификатор устройства передачи сообщения.
        /// </value>
        public byte From { get; set; }

        /// <summary>
        /// Получает или задает поле spacewire "Устройство получения".
        /// </summary>
        /// <value>
        /// Идентификатор устройства приема сообщения.
        /// </value>
        public byte To { get; set; }

        /// <summary>
        /// Получает или задает идентификатор протокола.
        /// </summary>
        /// <value>
        /// Идентификатор протокола.
        /// </value>
        public byte ProtocolID { get; set; }

        /// <summary>
        /// Получает или задает тип сообщения spacewire SPTP.
        /// </summary>
        /// <value>
        /// Тип сообщения spacewire SPTP.
        /// </value>
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

        public override byte[] ToArray()
        {
            throw new NotImplementedException();
        }
    }

    public class SpacewireIcdMsgEventArgs : SpacewireSptpMsgEventArgs
    {
        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="SpacewireIcdMsgEventArgs" />.
        /// </summary>
        public SpacewireIcdMsgEventArgs()
        { 
        }

        public SpacewireIcdMsgEventArgs(byte[] data)
            : base(data)
        {
        }

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="SpacewireIcdMsgEventArgs" />.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <param name="time1">Time tick 1</param>
        /// <param name="time2">Time tick 2</param>
        /// <param name="error">Ошибка в сообщении.</param>
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

        public BitVector32.Section VersionNumber
        {
            get
            {
                return BitVector32.CreateSection(7, TypeId);                    
            }
            set
            { }
        }

        public BitVector32.Section TypeId
        {
            get
            {
                return BitVector32.CreateSection(1, Flag);
            }
        }

        public BitVector32.Section Flag
        {
            get
            {
                return BitVector32.CreateSection(1, Apid);
            }
        }

        public BitVector32.Section Apid
        {
            get
            {
                return BitVector32.CreateSection(0x7FF);
            }
        }

        public BitVector32.Section SegmentFlag
        {
            get
            {
                return BitVector32.CreateSection(3, Counter);
            }
        }

        public BitVector32.Section Counter
        {
            get
            {
                return BitVector32.CreateSection(0x3FFF);
            }
        }

        public BitVector32 Id { get; set; }

        public BitVector32 Control { get; set; }

        public BitVector32 Size { get; set; }

        public int ConvertToInt(byte[] array)
        {
            int pos = 0;
            int result = 0;
            foreach (byte by in array.Reverse<byte>().ToArray())
            {
                result |= (int)(by << pos);
                pos += 8;
            }

            return result;
        }

        public override byte[] ToArray()
        {
            throw new NotImplementedException();
        }
    }

    public class SpacewireKbvMsgEventArgs : SpacewireIcdMsgEventArgs
    {
        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="SpacewireKbvMsgEventArgs" />.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <param name="time1">Time tick 1</param>
        /// <param name="time2">Time tick 2</param>
        /// <param name="error">Ошибка в сообщении.</param>
        public SpacewireKbvMsgEventArgs(byte[] data, byte time1, byte time2, byte error = 0x00)
            : base(data, time1, time2, error)
        {
            if (6 <= DataLen)
            {
                FieldPNormal = Data[0];
                FieldPExtended = Data[1];
                Kbv = ConvertToInt(Data.Skip(2).ToArray());
            }
        }

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="SpacewireKbvMsgEventArgs" />.
        /// </summary>
        public SpacewireKbvMsgEventArgs()          
        {
        }

        public byte FieldPNormal { get; set; }

        public byte FieldPExtended { get; set; }

        public int Kbv { get; set; }

        public override byte[] ToArray()
        {
            throw new NotImplementedException();
        }
    }

    public class SpacewireTmMsgEventArgs : SpacewireIcdMsgEventArgs
    {
        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="SpacewireTmMsgEventArgs" />.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <param name="time1">Time tick 1</param>
        /// <param name="time2">Time tick 2</param>
        /// <param name="error">Ошибка в сообщении.</param>
        public SpacewireTmMsgEventArgs(byte[] data, byte time1, byte time2, byte error = 0x00)
            : base(data, time1, time2, error)
        {
        }

        public override byte[] ToArray()
        {
            throw new NotImplementedException();
        }
    }

    public class SpacewireTkMsgEventArgs : SpacewireIcdMsgEventArgs
    {
        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="SpacewireTkMsgEventArgs" />.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <param name="time1">Time tick 1</param>
        /// <param name="time2">Time tick 2</param>
        /// <param name="error">Ошибка в сообщении.</param>
        public SpacewireTkMsgEventArgs(byte[] data, byte time1, byte time2, byte error = 0x00)
            : base(data, time1, time2, error)
        {
        
        }

        public SpacewireTkMsgEventArgs(byte apid, Dictionary<byte, AutoCounter> dict, byte[] data)
            : base(data)
        {
            BitVector32 id = new BitVector32();
            BitVector32 control = new BitVector32();
            BitVector32 header = new BitVector32();
            id[VersionNumber] = 0;
            id[TypeId] = 1;
            id[Flag] = 1;
            id[Apid] = apid;                        
            control[SegmentFlag] = 3;
            if (!dict.ContainsKey(apid))
            {
                dict.Add(apid, new AutoCounter());
            }
            control[Counter] = (int)dict[apid];                        
            header[Ccsds] = 0;
            header[Version] = 1;
            header[Acknowledgment] = 8;
            header[ServiceType] = 0;
            header[ServiceSubType] = 0;
            header[Reserve] = 0;
            Id = new BitVector32(id);
            Control = new BitVector32(control);
            Size = new BitVector32((DataLen - 1));
            Header = new BitVector32(header);
        }

        public BitVector32 Header { get; set; }

        public BitVector32.Section Ccsds
        {
            get
            {
                return BitVector32.CreateSection(1, Version);
            }
        }

        public BitVector32.Section Version
        {
            get
            {
                return BitVector32.CreateSection(7, Acknowledgment);
            }
        }

        public BitVector32.Section Acknowledgment
        {
            get
            {
                return BitVector32.CreateSection(0xF, ServiceType);
            }
        }

        public BitVector32.Section ServiceType
        {
            get
            {
                return BitVector32.CreateSection(0xFF, ServiceSubType);
            }
        }

        public BitVector32.Section ServiceSubType
        {
            get
            {
                return BitVector32.CreateSection(0xFF, Reserve);
            }
        }

        public BitVector32.Section Reserve
        {
            get
            {
                return BitVector32.CreateSection(0xFF);
            }
        }

        public ushort Crc 
        {
            get
            {
                return Crc16.Get(PackWithoutCrc, PackWithoutCrc.Length);
            } 
        }

        public byte[] PackWithoutCrc
        {
            get
            {
                byte[] buf = new byte[DataLen + 10];
                buf[0] = (byte)(Id.Data >> 8);
                buf[1] = (byte)Id.Data;
                buf[2] = (byte)(Control.Data >> 8);
                buf[3] = (byte)Control.Data;
                buf[4] = (byte)(Size.Data >> 8);
                buf[5] = (byte)Size.Data;
                buf[6] = (byte)(Header.Data >> 24);
                buf[7] = (byte)(Header.Data >> 16);
                buf[8] = (byte)(Header.Data >> 8);
                buf[9] = (byte)Header.Data;
                Array.Copy(Data, 0, buf, 10, Data.Length);
                return buf;
            }
        }

        public override byte[] ToArray()
        {
            byte[] packWithoutCRC = PackWithoutCrc;
            byte[] buf = new byte[packWithoutCRC.Length + 2];
            Array.Copy(packWithoutCRC, buf, packWithoutCRC.Length);
            buf[buf.Length - 1] = (byte)(Crc >> 8);
            buf[buf.Length - 2] = (byte)Crc;
            return buf;
        }

        public static class Crc16
        {
            // старший и младший байты должны быть поменяны местами
            static readonly ushort[] crc16Table = new ushort[]
            {
                0x0000, 0xC1C0, 0x81C1, 0x4001, 0x01C3, 0xC003, 0x8002, 0x41C2,
                0x01C6, 0xC006, 0x8007, 0x41C7, 0x0005, 0xC1C5, 0x81C4, 0x4004,
                0x01CC, 0xC00C, 0x800D, 0x41CD, 0x000F, 0xC1CF, 0x81CE, 0x400E,
                0x000A, 0xC1CA, 0x81CB, 0x400B, 0x01C9, 0xC009, 0x8008, 0x41C8,
                0x01D8, 0xC018, 0x8019, 0x41D9, 0x001B, 0xC1DB, 0x81DA, 0x401A,
                0x001E, 0xC1DE, 0x81DF, 0x401F, 0x01DD, 0xC01D, 0x801C, 0x41DC,
                0x0014, 0xC1D4, 0x81D5, 0x4015, 0x01D7, 0xC017, 0x8016, 0x41D6,
                0x01D2, 0xC012, 0x8013, 0x41D3, 0x0011, 0xC1D1, 0x81D0, 0x4010,
                0x01F0, 0xC030, 0x8031, 0x41F1, 0x0033, 0xC1F3, 0x81F2, 0x4032,
                0x0036, 0xC1F6, 0x81F7, 0x4037, 0x01F5, 0xC035, 0x8034, 0x41F4,
                0x003C, 0xC1FC, 0x81FD, 0x403D, 0x01FF, 0xC03F, 0x803E, 0x41FE,
                0x01FA, 0xC03A, 0x803B, 0x41FB, 0x0039, 0xC1F9, 0x81F8, 0x4038,
                0x0028, 0xC1E8, 0x81E9, 0x4029, 0x01EB, 0xC02B, 0x802A, 0x41EA,
                0x01EE, 0xC02E, 0x802F, 0x41EF, 0x002D, 0xC1ED, 0x81EC, 0x402C,
                0x01E4, 0xC024, 0x8025, 0x41E5, 0x0027, 0xC1E7, 0x81E6, 0x4026,
                0x0022, 0xC1E2, 0x81E3, 0x4023, 0x01E1, 0xC021, 0x8020, 0x41E0,
                0x01A0, 0xC060, 0x8061, 0x41A1, 0x0063, 0xC1A3, 0x81A2, 0x4062,
                0x0066, 0xC1A6, 0x81A7, 0x4067, 0x01A5, 0xC065, 0x8064, 0x41A4,
                0x006C, 0xC1AC, 0x81AD, 0x406D, 0x01AF, 0xC06F, 0x806E, 0x41AE,
                0x01AA, 0xC06A, 0x806B, 0x41AB, 0x0069, 0xC1A9, 0x81A8, 0x4068,
                0x0078, 0xC1B8, 0x81B9, 0x4079, 0x01BB, 0xC07B, 0x807A, 0x41BA,
                0x01BE, 0xC07E, 0x807F, 0x41BF, 0x007D, 0xC1BD, 0x81BC, 0x407C,
                0x01B4, 0xC074, 0x8075, 0x41B5, 0x0077, 0xC1B7, 0x81B6, 0x4076,
                0x0072, 0xC1B2, 0x81B3, 0x4073, 0x01B1, 0xC071, 0x8070, 0x41B0,
                0x0050, 0xC190, 0x8191, 0x4051, 0x0193, 0xC053, 0x8052, 0x4192,
                0x0196, 0xC056, 0x8057, 0x4197, 0x0055, 0xC195, 0x8194, 0x4054,
                0x019C, 0xC05C, 0x805D, 0x419D, 0x005F, 0xC19F, 0x819E, 0x405E,
                0x005A, 0xC19A, 0x819B, 0x405B, 0x0199, 0xC059, 0x8058, 0x4198,
                0x0188, 0xC048, 0x8049, 0x4189, 0x004B, 0xC18B, 0x818A, 0x404A,
                0x004E, 0xC18E, 0x818F, 0x404F, 0x018D, 0xC04D, 0x804C, 0x418C,
                0x0044, 0xC184, 0x8185, 0x4045, 0x0187, 0xC047, 0x8046, 0x4186,
                0x0182, 0xC042, 0x8043, 0x4183, 0x0041, 0xC181, 0x8180, 0x4040 
            };

            public static ushort Get(byte[] bytes, int len)
            {
                ushort crc = 0xFFFF;
                for (var i = 0; i < len; i++)
                    crc = (ushort)((crc << 8) ^ crc16Table[(crc >> 8) ^ bytes[i]]);
                return crc;
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
