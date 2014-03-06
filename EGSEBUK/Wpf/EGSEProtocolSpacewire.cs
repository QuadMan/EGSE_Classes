//-----------------------------------------------------------------------
// <copyright file="EGSEProtocolSpacewire.cs" company="IKI RSSI, laboratory №711">
//     Copyright (c) IKI RSSI, laboratory №711. All rights reserved.
// </copyright>
// <author>Коробейщиков Иван</author>
//-----------------------------------------------------------------------

namespace EGSE.Protocols
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using EGSE.Utilites;

    /// <summary>
    /// Класс декодера по протоколу Spacewire.
    /// </summary>
    public class ProtocolSpacewire
    {
        /// <summary>
        /// Адресный байт "Данные".
        /// </summary>
        private readonly uint _data;

        /// <summary>
        /// Адресный байт "EOP или EEP".
        /// </summary>
        private readonly uint _eop;

        /// <summary>
        /// Адресный байт "Time tick 1".
        /// </summary>
        private readonly uint _time1;

        /// <summary>
        /// Адресный байт "Time tick 2".
        /// </summary>
        private readonly uint _time2;

        /// <summary>
        /// Текущий буфер, формируемого сообщения протокола spacewire.
        /// </summary>
        private List<byte> _buf;

        /// <summary>
        /// Текущее значение Tick time 1.
        /// </summary>
        private byte _currentTime1;

        /// <summary>
        /// Текущее значение Tick time 2.
        /// </summary>
        private byte _currentTime2;

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="ProtocolSpacewire" />.
        /// </summary>
        /// <param name="data">Адресный байт "Данные".</param>
        /// <param name="eop">Адресный байт "EOP или EEP".</param>
        /// <param name="time1">Адресный байт "Time tick 1".</param>
        /// <param name="time2">Адресный байт "Time tick 2".</param>
        public ProtocolSpacewire(uint data, uint eop, uint time1, uint time2)
        {
            _data = data;
            _eop = eop;
            _time1 = time1;
            _time2 = time2;
            _buf = new List<byte>();
        }

        /// <summary>
        /// Объявление делегата обработки сообщений протокола spacewire.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="SpacewireSptpMsgEventArgs"/> instance containing the event data.</param>
        public delegate void SpacewireMsgEventHandler(object sender, SpacewireSptpMsgEventArgs e);

        public delegate void SpacewireTimeTickMsgEventHandler(object sender, SpacewireTimeTickMsgEventArgs e);

        /// <summary>
        /// Происходит, когда [сформировано сообщение spacewire].
        /// </summary>
        public event SpacewireMsgEventHandler GotSpacewireMsg;

        public event SpacewireTimeTickMsgEventHandler GotSpacewireTimeTick1Msg;

        public event SpacewireTimeTickMsgEventHandler GotSpacewireTimeTick2Msg;

        /// <summary>
        /// Метод, обрабатывающий сообщения от декодера USB.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="msg">Сообщение для обработки.</param>
        public void OnMessageFunc(object sender, ProtocolMsgEventArgs msg)
        {
            if (msg != null)
            {             
                if (_data == msg.Addr)
                { 
                   for (int i = 0; i < msg.DataLen; i++)
                   {
                       _buf.Add(msg.Data[i]); 
                   }
                }
                else if (_eop == msg.Addr)
                {
                    SpacewireSptpMsgEventArgs _msg = new SpacewireIcdMsgEventArgs(_buf.ToArray(), _currentTime1, _currentTime2, msg.Data[0]);
                    OnSpacewireMsg(this, _msg);
                    _buf.Clear();
                }
                else if (_time1 == msg.Addr)
                {
                    _currentTime1 = msg.Data[0];
                    OnTimeTick1Msg(this, new SpacewireTimeTickMsgEventArgs(_currentTime1));
                }
                else if (_time2 == msg.Addr)
                {
                    _currentTime2 = msg.Data[0];
                    OnTimeTick2Msg(this, new SpacewireTimeTickMsgEventArgs(_currentTime2));
                }
            }
        }

        /// <summary>
        /// Обертка события: возникновение сообщения протокола в декодере spacewire.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">Класс описывающий сообщение протокола.</param>
        protected virtual void OnSpacewireMsg(object sender, SpacewireSptpMsgEventArgs e)
        {
            if (this.GotSpacewireMsg != null)
            {
                this.GotSpacewireMsg(sender, e);
            }
        }

        protected virtual void OnTimeTick1Msg(object sender, SpacewireTimeTickMsgEventArgs e)
        {
            if (this.GotSpacewireTimeTick1Msg != null)
            {
                this.GotSpacewireTimeTick1Msg(sender, e);
            }
        }

        protected virtual void OnTimeTick2Msg(object sender, SpacewireTimeTickMsgEventArgs e)
        {
            if (this.GotSpacewireTimeTick2Msg != null)
            {
                this.GotSpacewireTimeTick2Msg(sender, e);
            }
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

        /// <summary>
        /// Получает [версию пакета протокола ICD].
        /// </summary>
        /// <value>
        /// [версия пакета протокола ICD].
        /// </value>
        public BitVector32.Section VersionNumber
        {
            get
            {
                return BitVector32.CreateSection(7, TypeId);
            }
        }

        /// <summary>
        /// Получает [тип протокола ICD].
        /// </summary>
        /// <value>
        /// [тип протокола ICD].
        /// </value>
        public BitVector32.Section TypeId
        {
            get
            {
                return BitVector32.CreateSection(1, Flag);
            }
        }

        /// <summary>
        /// Получает [флаг заголовка данных протокола ICD].
        /// </summary>
        /// <value>
        /// [флаг заголовка данных протокола ICD].
        /// </value>
        public BitVector32.Section Flag
        {
            get
            {
                return BitVector32.CreateSection(1, Apid);
            }
        }

        /// <summary>
        /// Получает [APID протокола ICD].
        /// </summary>
        /// <value>
        /// [APID протокола ICD].
        /// </value>
        public BitVector32.Section Apid
        {
            get
            {
                return BitVector32.CreateSection(0x7FF);
            }
        }

        /// <summary>
        /// Получает [флаги сегментации протокола ICD].
        /// </summary>
        /// <value>
        /// [флаги сегментации протокола ICD].
        /// </value>
        public BitVector32.Section SegmentFlag
        {
            get
            {
                return BitVector32.CreateSection(3, Counter);
            }
        }

        /// <summary>
        /// Получает [счетчик последовательности протокола ICD].
        /// </summary>
        /// <value>
        /// [счетчик последовательности протокола ICD].
        /// </value>
        public BitVector32.Section Counter
        {
            get
            {
                return BitVector32.CreateSection(0x3FFF);
            }
        }

        /// <summary>
        /// Получает или задает [идентификатор пакета протокола ICD].
        /// </summary>
        /// <value>
        /// [идентификатор пакета протокола ICD].
        /// </value>
        public BitVector32 Id { get; set; }

        /// <summary>
        /// Получает или задает [поле контроля последовательности протокола ICD].
        /// </summary>
        /// <value>
        /// [поле контроля последовательности протокола ICD].
        /// </value>
        public BitVector32 Control { get; set; }

        /// <summary>
        /// Получает или задает [длина поля данных протокола ICD].
        /// </summary>
        /// <value>
        /// [длина поля данных протокола ICD].
        /// </value>
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

        /// <summary>
        /// Получает или задает [поле Р нормальное протокола ICD].
        /// </summary>
        /// <value>
        /// [поле Р нормальное протокола ICD].
        /// </value>
        public byte FieldPNormal { get; set; }

        /// <summary>
        /// Получает или задает [поле Р расширенное протокола ICD].
        /// </summary>
        /// <value>
        /// [поле Р расширенное протокола ICD].
        /// </value>
        public byte FieldPExtended { get; set; }

        /// <summary>
        /// Получает или задает [поле Т КБВ протокола ICD].
        /// </summary>
        /// <value>
        /// [поле Т КБВ протокола ICD].
        /// </value>
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

        /// <summary>
        /// Получает или задает [заголовок поля данных протокола ICD].
        /// </summary>
        /// <value>
        /// [заголовок поля данных протокола ICD].
        /// </value>
        public BitVector32 Header { get; set; }

        /// <summary>
        /// Получает [флаг вторичного заголовка CCSDS протокола ICD].
        /// </summary>
        /// <value>
        /// [флаг вторичного заголовка CCSDS протокола ICD].
        /// </value>
        public BitVector32.Section Ccsds
        {
            get
            {
                return BitVector32.CreateSection(1, Version);
            }
        }

        /// <summary>
        /// Получает [номер версии ТМ-пакета PUS протокола ICD].
        /// </summary>
        /// <value>
        /// [номер версии ТМ-пакета PUS протокола ICD].
        /// </value>
        public BitVector32.Section Version
        {
            get
            {
                return BitVector32.CreateSection(7, Acknowledgment);
            }
        }

        /// <summary>
        /// Получает [тип квитирования протокола ICD].
        /// </summary>
        /// <value>
        /// [тип квитирования протокола ICD].
        /// </value>
        public BitVector32.Section Acknowledgment
        {
            get
            {
                return BitVector32.CreateSection(0xF, ServiceType);
            }
        }

        /// <summary>
        /// Получает [тип сервиса протокола ICD].
        /// </summary>
        /// <value>
        /// [тип сервиса протокола ICD].
        /// </value>
        public BitVector32.Section ServiceType
        {
            get
            {
                return BitVector32.CreateSection(0xFF, ServiceSubType);
            }
        }

        /// <summary>
        /// Получает [подтип сервиса протокола ICD].
        /// </summary>
        /// <value>
        /// [подтип сервиса протокола ICD].
        /// </value>
        public BitVector32.Section ServiceSubType
        {
            get
            {
                return BitVector32.CreateSection(0xFF, Reserve);
            }
        }

        /// <summary>
        /// Получает [резерв протокола ICD].
        /// </summary>
        /// <value>
        /// [резерв протокола ICD].
        /// </value>
        public BitVector32.Section Reserve
        {
            get
            {
                return BitVector32.CreateSection(0xFF);
            }
        }

        /// <summary>
        /// Получает [CRC для телекоманды].
        /// </summary>
        /// <value>
        /// [CRC для пакета].
        /// </value>
        public ushort Crc
        {
            get
            {
                return Crc16.Get(PackWithoutCrc, PackWithoutCrc.Length);
            }
        }

        /// <summary>
        /// Получает [пакет телекоманды без CRC].
        /// </summary>
        /// <value>
        /// [пакет телекоманды без CRC].
        /// </value>
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
            ProtocolID = 0xF2;
            Error = 0x00;
            Time1 = 0x00;
            Time2 = 0x00;
        }

        public SpacewireSptpMsgEventArgs(byte[] data, byte to, byte from)
            : this(data)
        {
            MsgType = Type.Data;
            To = to;
            From = from;
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
            byte[] buf = new byte[DataLen + 4];
            buf[0] = (byte)To;
            buf[1] = (byte)ProtocolID;
            buf[2] = (byte)MsgType;
            buf[3] = (byte)From;
            Array.Copy(Data, 0, buf, 4, Data.Length);
            return buf;
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
}
