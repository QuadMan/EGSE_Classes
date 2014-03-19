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
    using System.Runtime.InteropServices;

    /// <summary>
    /// Декодер протокола Spacewire.
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
        /// Обслуживает [сообщения протокола spacewire].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="SpacewireSptpMsgEventArgs"/> instance containing the event data.</param>
        public delegate void SpacewireMsgEventHandler(object sender, SpacewireSptpMsgEventArgs e);

        /// <summary>
        /// Обслуживает [сообщения timetick протокола spacewire].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="SpacewireTimeTickMsgEventArgs"/> instance containing the event data.</param>
        public delegate void SpacewireTimeTickMsgEventHandler(object sender, SpacewireTimeTickMsgEventArgs e);

        /// <summary>
        /// Происходит, когда [сформировано сообщение spacewire].
        /// </summary>
        public event SpacewireMsgEventHandler GotSpacewireMsg;

        /// <summary>
        /// Происходит, когда [сформировано сообщение timetick 1 spacewire].
        /// </summary>
        public event SpacewireTimeTickMsgEventHandler GotSpacewireTimeTick1Msg;

        /// <summary>
        /// Происходит, когда [сформировано сообщение timetick 2 spacewire].
        /// </summary>
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

        /// <summary>
        /// Обертка события: возникновение TimeTick1-сообщения в декодере spacewire.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">Класс описывающий сообщение протокола.</param>
        protected virtual void OnTimeTick1Msg(object sender, SpacewireTimeTickMsgEventArgs e)
        {
            if (this.GotSpacewireTimeTick1Msg != null)
            {
                this.GotSpacewireTimeTick1Msg(sender, e);
            }
        }

        /// <summary>
        /// Обертка события: возникновение TimeTick2-сообщения в декодере spacewire.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">Класс описывающий сообщение протокола.</param>
        protected virtual void OnTimeTick2Msg(object sender, SpacewireTimeTickMsgEventArgs e)
        {
            if (this.GotSpacewireTimeTick2Msg != null)
            {
                this.GotSpacewireTimeTick2Msg(sender, e);
            }
        }
    }

    /// <summary>
    /// Предоставляет аргументы icd-протокола для события, созданного полученным сообщением по протоколу spacewire.
    /// </summary>
    public class SpacewireIcdMsgEventArgs : SpacewireSptpMsgEventArgs
    {
        ///// <summary>
        ///// Инициализирует новый экземпляр класса <see cref="SpacewireIcdMsgEventArgs" />.
        ///// </summary>
        //public SpacewireIcdMsgEventArgs()
        //{
        //}

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="SpacewireIcdMsgEventArgs" />.
        /// </summary>
        /// <param name="data">The data.</param>
        public SpacewireIcdMsgEventArgs(byte[] data)
            : base(data, data.Length, 0x00, 0x00)
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
            //if (6 <= DataLen)
            //{
            //    byte[] buf = new byte[DataLen - 6];
            //    Array.Copy(Data.Skip<byte>(6).ToArray(), buf, DataLen - 6);
            //    Id = new BitVector32(ConvertToInt(Data.Take<byte>(2).ToArray()));
            //    Control = new BitVector32(ConvertToInt(Data.Take<byte>(4).ToArray().Skip<byte>(2).ToArray()));
            //    Size = new BitVector32(ConvertToInt(Data.Take<byte>(6).ToArray().Skip<byte>(4).ToArray()));
            //    Data = buf;
            //    DataLen = buf.Length;
            //}
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

        /// <summary>
        /// Преобразует массив байт (количеством от 1 до 4) к целому.
        /// </summary>
        /// <param name="array">Массив байт.</param>
        /// <returns>Знаковое целое.</returns>
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

        /// <summary>
        /// Преобразует данные экземпляра к массиву байт.
        /// </summary>
        /// <returns>Массив байт.</returns>
        /// <exception cref="System.NotImplementedException">Нет реализации.</exception>
        public override byte[] ToArray()
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Предоставляет аргументы КБВ-кадра для события, созданного полученным сообщением по протоколу spacewire.
    /// </summary>
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

        ///// <summary>
        ///// Инициализирует новый экземпляр класса <see cref="SpacewireKbvMsgEventArgs" />.
        ///// </summary>
        //public SpacewireKbvMsgEventArgs()
        //{
        //}

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

        /// <summary>
        /// Преобразует данные экземпляра к массиву байт.
        /// </summary>
        /// <returns>
        /// Массив байт.
        /// </returns>
        /// <exception cref="System.NotImplementedException">Нет реализации.</exception>
        public override byte[] ToArray()
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Предоставляет аргументы ТМ-кадра для события, созданного полученным сообщением по протоколу spacewire.
    /// </summary>
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

        /// <summary>
        /// Преобразует данные экземпляра к массиву байт.
        /// </summary>
        /// <returns>
        /// Массив байт.
        /// </returns>
        /// <exception cref="System.NotImplementedException">Нет реализации.</exception>
        public override byte[] ToArray()
        {
            throw new NotImplementedException();
        }

        public bool CrcCheck()
        {
            return false;
        }
    }

    /// <summary>
    /// Предоставляет аргументы ТК-кадра для события, созданного полученным сообщением по протоколу spacewire.
    /// </summary>
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

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="SpacewireTkMsgEventArgs" />.
        /// </summary>
        /// <param name="apid">The apid.</param>
        /// <param name="dict">The dictionary.</param>
        /// <param name="data">The data.</param>
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
            Size = new BitVector32(DataLen - 1);
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
        /// Получает [сформированный CRC для телекоманды].
        /// </summary>
        /// <value>
        /// [сформированный CRC для телекоманды].
        /// </value>
        public ushort GetCrc(byte[] pack)
        {
            return Crc16.Get(pack, pack.Length);
        }

        public ushort Crc
        {
            get
            {
                return (ushort)((Data[Data.Length - 1] << 8) | (Data[Data.Length - 2]));
            }
        }

        /// <summary>
        /// Получает [пакет телекоманды без CRC].
        /// </summary>
        /// <value>
        /// [пакет телекоманды без CRC].
        /// </value>
        public byte[] GetPackWithoutCrc()
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

        /// <summary>
        /// Преобразует данные экземпляра к массиву байт.
        /// </summary>
        /// <returns>
        /// Массив байт.
        /// </returns>
        public override byte[] ToArray()
        {
            byte[] packWithoutCRC = GetPackWithoutCrc();
            byte[] buf = new byte[packWithoutCRC.Length + 2];
            Array.Copy(packWithoutCRC, buf, packWithoutCRC.Length);
            ushort newCrc = GetCrc(packWithoutCRC);
            buf[buf.Length - 1] = (byte)(newCrc >> 8);
            buf[buf.Length - 2] = (byte)newCrc;
            return buf;
        }
        
    }

    /// <summary>
    /// Предоставляет аргументы sptp-протокола для события, созданного полученным сообщением по протоколу spacewire.
    /// </summary>
    public class SpacewireSptpMsgEventArgs : MsgBase
    {
        [StructLayout(LayoutKind.Sequential)]
        public struct Sptp
        {
            private const int MaxLengthOfSpacewirePackage = 0x10004;
            private static readonly BitVector32.Section toSection = BitVector32.CreateSection(0xFF);
            private static readonly BitVector32.Section protocolIdSection = BitVector32.CreateSection(0xFF, toSection);
            private static readonly BitVector32.Section msgTypeSection = BitVector32.CreateSection(0xFF, protocolIdSection);
            private static readonly BitVector32.Section fromSection = BitVector32.CreateSection(0xFF, msgTypeSection);

            private BitVector32 header;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = MaxLengthOfSpacewirePackage)]
            private byte[] data;

            internal byte[] Data
            {
                get
                {
                    return data;
                }
            }

            public byte To
            {
                get 
                {
                    return (byte)header[toSection];
                }

                set
                {
                    header[toSection] = (int)value;
                }
            }

            public byte ProtocolId
            {
                get
                {
                    return (byte)header[protocolIdSection];
                }

                set
                {
                    header[protocolIdSection] = (int)value;
                }
            }

            public Type MsgType
            {
                get
                {
                    return (Type)header[msgTypeSection];
                }

                set
                {
                    header[msgTypeSection] = (int)value;
                }
            }

            public byte From
            {
                get
                {
                    return (byte)header[fromSection];
                }

                set
                {
                    header[fromSection] = (int)value;
                }
            }
        }

        ///// <summary>
        ///// Инициализирует новый экземпляр класса <see cref="SpacewireSptpMsgEventArgs" />.
        ///// </summary>
        ///// <param name="data">The data.</param>
        ///// <param name="to">Адрес устройства получатель.</param>
        ///// <param name="from">Адрес устройства отправитель.</param>
        //[Obsolete("Используй конструктор с 5 аргументами")]
        //public SpacewireSptpMsgEventArgs(byte[] data, byte to, byte from)
        //    : this(data, data.Length, 0x00, 0x00)
        //{
        //    MsgType = Type.Data;
        //    To = to;
        //    From = from;
        //}

        public Sptp Info;

        public new byte[] Data
        {
            get
            {
                // возвращаем данные без учета заголовка
                return Info.Data.Take(base.DataLen - 4).ToArray();
            }
        }

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="SpacewireSptpMsgEventArgs" />.
        /// </summary>
        /// <param name="data">Данные посылки spacewire.</param>
        /// <param name="dataLen">Длина посылки spacewire.</param>
        /// <param name="time1">Time tick 1</param>
        /// <param name="time2">Time tick 2</param>
        /// <param name="error">Ошибка в сообщении.</param>
        /// <exception cref="System.ContextMarshalException">Размер кадра spacewire меньше 4 байт!</exception>
        public SpacewireSptpMsgEventArgs(byte[] data, int dataLen, byte time1, byte time2, byte error = 0x00)
            : base(data, dataLen)
        {
            new { data }.CheckNotNull();

            if ((4 > data.Length) || (4 > base.DataLen))
            {
                throw new ContextMarshalException(Resource.Get(@"eSmallSpacewireData"));
            }

            // преобразуем данные к структуре Sptp.
            GCHandle pinnedInfo = GCHandle.Alloc(data, GCHandleType.Pinned);
            Info = (Sptp)Marshal.PtrToStructure(pinnedInfo.AddrOfPinnedObject(), typeof(Sptp));
            pinnedInfo.Free();
        }

        /// <summary>
        /// Тип Spacewire SPTP сообщения. 
        /// </summary>
        public enum Type
        {
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

        ///// <summary>
        ///// Получает или задает поле spacewire "Устройство передачи".
        ///// </summary>
        ///// <value>
        ///// Идентификатор устройства передачи сообщения.
        ///// </value>
        //public byte From { get; set; }

        ///// <summary>
        ///// Получает или задает поле spacewire "Устройство получения".
        ///// </summary>
        ///// <value>
        ///// Идентификатор устройства приема сообщения.
        ///// </value>
        //public byte To { get; set; }

        ///// <summary>
        ///// Получает или задает идентификатор протокола.
        ///// </summary>
        ///// <value>
        ///// Идентификатор протокола.
        ///// </value>
        //public byte ProtocolID { get; set; }

        ///// <summary>
        ///// Получает или задает тип сообщения spacewire SPTP.
        ///// </summary>
        ///// <value>
        ///// Тип сообщения spacewire SPTP.
        ///// </value>
        //public Type MsgType { get; set; }

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

        /// <summary>
        /// Преобразует данные экземпляра к массиву байт.
        /// </summary>
        /// <returns>Массив байт.</returns>
        public override byte[] ToArray()
        {
            //byte[] buf = new byte[DataLen + 4];
            //buf[0] = (byte)To;
            //buf[1] = (byte)ProtocolID;
            //buf[2] = (byte)MsgType;
            //buf[3] = (byte)From;
            //Array.Copy(Data, 0, buf, 4, Data.Length);
            return base.Data;
        }
    }

    /// <summary>
    /// Предоставляет аргументы TimeTick-кадра для события, созданного полученным сообщением по протоколу spacewire.
    /// </summary>
    public class SpacewireTimeTickMsgEventArgs : MsgBase
    {
        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="SpacewireTimeTickMsgEventArgs" />.
        /// </summary>
        public SpacewireTimeTickMsgEventArgs()
        {
        }

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="SpacewireTimeTickMsgEventArgs" />.
        /// </summary>
        /// <param name="data">The data.</param>
        public SpacewireTimeTickMsgEventArgs(byte data)
        {
            Data = new byte[1] { (byte)(data & 0x1F) };
        }
    }
}
