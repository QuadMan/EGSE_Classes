//-----------------------------------------------------------------------
// <copyright file="EGSEProtocolSpacewire.cs" company="IKI RSSI, laboratory №711">
//     Copyright (c) IKI RSSI, laboratory №711. All rights reserved.
// </copyright>
// <author>Коробейщиков Иван</author>
//-----------------------------------------------------------------------

namespace Egse.Protocols
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Linq;
    using System.Runtime.InteropServices;
    using System.Windows;
    using Egse.Utilites;

    /// <summary>
    /// Обмен сообщениями по протоколу Spacewire.
    /// </summary>
    public class ProtocolSpacewire
    {
        /// <summary>
        /// Адресный байт: данные spacewire.
        /// </summary>
        private readonly uint _data;

        /// <summary>
        /// Адресный байт: маркер eop или eep.
        /// </summary>
        private readonly uint _eop;

        /// <summary>
        /// Адресный байт: time tick1.
        /// </summary>               
        private readonly uint _time1;

        /// <summary>
        /// Адресный байт: time tick2.
        /// </summary>
        private readonly uint _time2;

        /// <summary>
        /// Формируемое сообщение spacewire.
        /// </summary>
        private List<byte> _buf;

        /// <summary>
        /// Значение поля: TIME TICK1.
        /// </summary>                
        private byte _currentTime1;

        /// <summary>
        /// Значение поля: TIME TICK2.
        /// </summary>
        private byte _currentTime2;

        /// <summary>
        /// <c>true</c> если не нужно декодировать протокол.
        /// </summary>
        private bool _isEmptyProto = false;

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="ProtocolSpacewire" />.
        /// </summary>
        /// <param name="data">Адресный байт: данные spacewire.</param>
        /// <param name="eop">Адресный байт: метка EOP.</param>
        /// <param name="time1">Адресный байт: Time tick 1.</param>
        /// <param name="time2">Адресный байт: Time tick 2.</param>
        /// <param name="isEmptyProto">Если установлено <c>true</c> [декодирование протокола отсутствует].</param>
        public ProtocolSpacewire(uint data, uint eop, uint time1, uint time2, bool isEmptyProto = false)
        {
            _data = data;
            _eop = eop;
            _time1 = time1;
            _time2 = time2;
            _isEmptyProto = isEmptyProto;
            _buf = new List<byte>();
        }

        /// <summary>
        /// Обработка spacewire-сообщений.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="BaseMsgEventArgs"/> instance containing the event data.</param>
        public delegate void SpacewireMsgEventHandler(object sender, BaseMsgEventArgs e);

        /// <summary>
        /// Обработка time-tick-сообщений.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="SpacewireTimeTickMsgEventArgs"/> instance containing the event data.</param>
        public delegate void SpacewireTimeTickMsgEventHandler(object sender, SpacewireTimeTickMsgEventArgs e);

        /// <summary>
        /// Вызывается когда [получено spacewire-сообщение].
        /// </summary>
        public event SpacewireMsgEventHandler GotSpacewireMsg;

        /// <summary>
        /// Вызывается когда [получено timetick-сообщение по 1 адресу].
        /// </summary>
        public event SpacewireTimeTickMsgEventHandler GotSpacewireTimeTick1Msg;

        /// <summary>
        /// Вызывается когда [получено timetick-сообщение по 2 адресу].
        /// </summary>     
        public event SpacewireTimeTickMsgEventHandler GotSpacewireTimeTick2Msg;

        /// <summary>
        /// Вызывается когда [получено сообщение по USB].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="msg">The <see cref="ProtocolMsgEventArgs"/> instance containing the event data.</param>
        /// <exception cref="System.ContextMarshalException">Не удалось "опознать" spacewire сообщение.</exception>
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
                    BaseMsgEventArgs pack = null;                  
                    byte[] arr = _buf.ToArray();
                    try
                    {
                        if (_isEmptyProto && SpacewireEmptyProtoMsgEventArgs.Test(arr))
                        {
                            pack = new SpacewireEmptyProtoMsgEventArgs(arr, _currentTime1, _currentTime2, msg.Data[0]);
                        }
                        else if (SpacewireTkMsgEventArgs.Test(arr))
                        {
                            pack = new SpacewireTkMsgEventArgs(arr, _currentTime1, _currentTime2, msg.Data[0]);
                        }
                        else if (SpacewireTm604MsgEventArgs.Test(arr))
                        {
                            pack = new SpacewireTm604MsgEventArgs(arr, _currentTime1, _currentTime2, msg.Data[0]);
                        }
                        else if (SpacewireTmMsgEventArgs.Test(arr))
                        {
                            pack = new SpacewireTmMsgEventArgs(arr, _currentTime1, _currentTime2, msg.Data[0]);
                        }
                        else if (SpacewireObtMsgEventArgs.Test(arr))
                        {
                            pack = new SpacewireObtMsgEventArgs(arr, _currentTime1, _currentTime2, msg.Data[0]);
                        }
                        else if (SpacewireIcdMsgEventArgs.Test(arr))
                        {
                            pack = new SpacewireIcdMsgEventArgs(arr, _currentTime1, _currentTime2, msg.Data[0]);
                        }
                        else if (SpacewireSptpMsgEventArgs.Test(arr))
                        {
                            pack = new SpacewireSptpMsgEventArgs(arr, _currentTime1, _currentTime2, msg.Data[0]);
                        }
                        else
                        {
                            throw new ContextMarshalException(Resource.Get(@"eUnknowSpacewireData"));
                        }
                    }
                    catch (ContextMarshalException e)
                    {
                        pack = new SpacewireErrorMsgEventArgs(arr, _currentTime1, _currentTime2, msg.Data[0], e.Message);
                    }
                    
                    if (null != pack)
                    {
                        OnSpacewireMsg(this, pack);
                    }

                    _buf.Clear();
                }
                else if (_time1 == msg.Addr)
                {
                    _currentTime1 = msg.Data[0];
                    OnTimeTick1Msg(this, new SpacewireTimeTickMsgEventArgs(new byte[1] { msg.Data[0] }, _currentTime1, _currentTime2));
                }
                else if (_time2 == msg.Addr)
                {
                    _currentTime2 = msg.Data[0];
                    OnTimeTick2Msg(this, new SpacewireTimeTickMsgEventArgs(new byte[1] { msg.Data[0] }, _currentTime1, _currentTime2));
                }
            }
        }

        /// <summary>
        /// Вызывается когда [сформировано spacewire-сообщение].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="BaseMsgEventArgs"/> instance containing the event data.</param>
        protected virtual void OnSpacewireMsg(object sender, BaseMsgEventArgs e)
        {
            if (this.GotSpacewireMsg != null)
            {
                this.GotSpacewireMsg(sender, e);
            }
        }

        /// <summary>
        /// Вызывается когда [сформировано timetick-сообщение по первому адресу.].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="SpacewireTimeTickMsgEventArgs"/> instance containing the event data.</param>
        protected virtual void OnTimeTick1Msg(object sender, SpacewireTimeTickMsgEventArgs e)
        {
            if (this.GotSpacewireTimeTick1Msg != null)
            {
                this.GotSpacewireTimeTick1Msg(sender, e);
            }
        }

        /// <summary>
        /// Вызывается когда [сформировано timetick-сообщение по второму адресу.].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="SpacewireTimeTickMsgEventArgs"/> instance containing the event data.</param>                          
        protected virtual void OnTimeTick2Msg(object sender, SpacewireTimeTickMsgEventArgs e)
        {
            if (this.GotSpacewireTimeTick2Msg != null)
            {
                this.GotSpacewireTimeTick2Msg(sender, e);
            }
        }
    }

    /// <summary>
    /// Обмен сообщениями по протоколу Icd.
    /// </summary>
    public class SpacewireIcdMsgEventArgs : SpacewireSptpMsgEventArgs
    {
        /// <summary>
        /// Агрегат доступа к заголовку icd-сообщения.
        /// </summary>
        private Icd _icdInfo;

        /// <summary>
        /// Данные icd-сообщения.
        /// </summary>
        private byte[] _data;

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="SpacewireIcdMsgEventArgs" />.
        /// </summary>
        /// <param name="data">"сырые" данные сообщения.</param>
        /// <param name="time1">Значение TimeTick1.</param>
        /// <param name="time2">Значение TimeTick2.</param>
        /// <param name="error">Значение Error.</param>
        /// <exception cref="System.ContextMarshalException">Если длина сообщения не достаточна для декодирования.</exception>
        public SpacewireIcdMsgEventArgs(byte[] data, byte time1, byte time2, byte error = 0x00)
            : base(data, time1, time2, error)
        {
            if (10 > data.Length)
            {
                throw new ContextMarshalException(Resource.Get(@"eSmallSpacewireIcdData"));
            }
            
            try
            {
                _icdInfo = Converter.MarshalTo<Icd>(data, out _data);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }

        /// <summary>
        /// Тип сообщения Icd.
        /// </summary>
        public enum IcdType
        {
            /// <summary>
            /// Сообщение icd "телекоманда".
            /// </summary>
            Tk = 0x01,

            /// <summary>
            /// Сообщение icd "телеметрия".
            /// </summary>
            Tm = 0x00
        }

        /// <summary>
        /// Флаг сообщения icd.
        /// </summary>
        public enum IcdFlag
        {
            /// <summary>
            /// Сообщение содержит заголовок в данных.
            /// </summary>
            HeaderFill = 0x01,

            /// <summary>
            /// Сообщение не содержит заголовок в данных.
            /// </summary>
            HeaderEmpty = 0x00
        }

        public override Sptp SptpInfo
        {
            get
            {
                return _icdInfo.SptpInfo;
            }
        }



        /// <summary>
        /// Получает данные icd сообщения. 
        /// </summary>
        /// <value>
        /// Данные icd сообщения.
        /// </value>
        public new byte[] Data
        {
            get
            {
                return _data;
            }
        }

        /// <summary>
        /// Получает агрегат доступа к заголовку icd сообщения.
        /// </summary>
        /// <value>
        /// Агрегат доступа к заголовку icd сообщения.
        /// </value>
        public virtual Icd IcdInfo
        {
            get
            {
                return _icdInfo;
            }
        }

        /// <summary>
        /// Получает истинное значение CRC для текущего сообщения.
        /// </summary>
        /// <value>
        /// Истинное значение CRC для текущего сообщения.
        /// </value>
        protected override ushort NeededCrc
        {
            get
            {
                return base.NeededCrc;
            }
        }

        /// <summary>
        /// "Сырая" проверка на принадлежность к Icd-сообщению.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <returns><c>true</c> если проверка пройдена успешно, иначе <c>false</c>.</returns>
        public static new bool Test(byte[] data)
        {
            return null != data ? 9 < data.Length : false;
        }

        /// <summary>
        /// Преобразует массив байт к целому.
        /// TODO что-то не понял зачем сделал len!!!
        /// </summary>
        /// <param name="array">Массив байт.</param>
        /// <param name="len">Количество учитываемых байт от начала массива.</param>
        /// <returns>Полученное целое, знаковое.</returns>
        public static int ConvertToInt(byte[] array, int len = 0)
        {
            if (0 == len)
            {
                len = array.Length;
            }

            int pos = 0;
            int result = 0;
            foreach (byte by in array.Take(len).ToArray())
            {
                result |= (int)(by << pos);
                pos += 8;
            }

            return result;
        }

        /// <summary>
        /// Получить "сырое" представление посылки.
        /// </summary>
        /// <returns>Массив байт.</returns>
        public override byte[] ToArray()
        {
            return base.ToArray();
        }

        /// <summary>
        /// Агрегат доступа к заголовку сообщения Icd.
        /// </summary>
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct Icd
        {
            /// <summary>
            /// Маска поля заголовка: идентификатор пакета: APID.
            /// </summary>
            private static readonly BitVector32.Section ApidHiSection = BitVector32.CreateSection(0x07);

            /// <summary>
            /// Маска поля заголовка: идентификатор пакета: Флаг заголовка данных.
            /// </summary>
            private static readonly BitVector32.Section FlagSection = BitVector32.CreateSection(0x01, ApidHiSection);

            /// <summary>
            /// Маска поля заголовка: идентификатор пакета: Тип.
            /// </summary>
            private static readonly BitVector32.Section TypeSection = BitVector32.CreateSection(0x01, FlagSection);

            /// <summary>
            /// Маска поля заголовка: идентификатор пакета: Номер версии.
            /// </summary>
            private static readonly BitVector32.Section VersionSection = BitVector32.CreateSection(0x07, TypeSection);

            /// <summary>
            /// Маска поля заголовка: идентификатор пакета: APID.
            /// </summary>
            private static readonly BitVector32.Section ApidLoSection = BitVector32.CreateSection(0xFF, VersionSection);

            /// <summary>
            /// Маска поля заголовка: идентификатор пакета: Счетчик последовательности.
            /// </summary>
            private static readonly BitVector32.Section CounterHiSection = BitVector32.CreateSection(0x3F, ApidLoSection);

            /// <summary>
            /// Маска поля заголовка: идентификатор пакета: Флаг сегментации.
            /// </summary>
            private static readonly BitVector32.Section SegmentSection = BitVector32.CreateSection(0x03, CounterHiSection);

            /// <summary>
            /// Маска поля заголовка: идентификатор пакета: Счетчик последовательности.
            /// </summary>
            private static readonly BitVector32.Section CounterLoSection = BitVector32.CreateSection(0xFF, SegmentSection);

            /// <summary>
            /// Агрегат доступа к заголовку sptp-сообщения.
            /// </summary>
            private SpacewireSptpMsgEventArgs.Sptp sptpHeader;

            /// <summary>
            /// Заголовок icd.
            /// </summary>
            private BitVector32 _header;

            /// <summary>
            /// Размер поля данных icd.
            /// </summary>
            private ushort _size;



            /// <summary>
            /// Получает или задает icd-поле "номер версии".
            /// </summary>
            /// <value>
            /// Значение icd-поля "номер версии".
            /// </value>
            public byte Version
            {
                get
                {
                    return (byte)_header[VersionSection];
                }

                set
                {
                    _header[VersionSection] = (int)value;
                }
            }

            /// <summary>
            /// Получает или задает icd-поле "тип".
            /// </summary>
            /// <value>
            /// Значение icd-поля "тип".
            /// </value>
            public IcdType Type
            {
                get
                {
                    return (IcdType)_header[TypeSection];
                }

                set
                {
                    _header[TypeSection] = (int)value;
                }
            }

            /// <summary>
            /// Получает или задает icd-поле "флаг заголовка данных".
            /// </summary>
            /// <value>
            /// Значение icd-поля "флаг заголовка данных".
            /// </value>
            public IcdFlag Flag
            {
                get
                {
                    return (IcdFlag)_header[FlagSection];
                }

                set
                {
                    _header[FlagSection] = (int)value;
                }
            }

            /// <summary>
            /// Получает или задает icd-поле "APID".
            /// </summary>
            /// <value>
            /// Значение icd-поля "APID".
            /// </value>
            public short Apid
            {
                get
                {
                    return (short)((_header[ApidHiSection] << 8) | _header[ApidLoSection]);
                }

                set
                {
                    _header[ApidLoSection] = (byte)value;
                    _header[ApidHiSection] = (byte)(value >> 8);
                }
            }

            /// <summary>
            /// Получает или задает icd-поле "флаг сегментации".
            /// </summary>
            /// <value>
            /// Значение icd-поля "флаг сегментации".
            /// </value>
            public byte Segment
            {
                get
                {
                    return (byte)_header[SegmentSection];
                }

                set
                {
                    _header[SegmentSection] = (int)value;
                }
            }

            /// <summary>
            /// Получает или задает icd-поле "счетчик последовательности".
            /// </summary>
            /// <value>
            /// Значение icd-поля "счетчик последовательности".
            /// </value>
            public short Counter
            {
                get
                {
                    return (short)((_header[CounterHiSection] << 8) | _header[CounterLoSection]);
                }

                set
                {
                    _header[CounterLoSection] = (byte)value;
                    _header[CounterHiSection] = (byte)(value >> 8);
                }
            }

            /// <summary>
            /// Получает или задает icd-поле "длина поля данных".
            /// </summary>
            /// <value>
            /// Значение icd-поля "длина поля данных".
            /// </value>
            public ushort Size
            {
                get
                {
                    return (ushort)((_size >> 8) | (_size << 8));
                }

                set
                {
                    _size = (ushort)((value >> 8) | (value << 8));
                }
            }

            /// <summary>
            /// Получает или задает заголовок Icd сообщения.
            /// </summary>
            /// <value>
            /// Заголовок Icd сообщения.
            /// </value>
            public BitVector32 Header
            {
                get
                {
                    return _header;
                }

                set
                {
                    _header = value;
                }
            }

            /// <summary>
            /// Получает или задает агрегат доступа к заголовку sptp сообщения.
            /// </summary>
            /// <value>
            /// Агрегат доступа к заголовку sptp сообщения.
            /// </value>
            internal Sptp SptpInfo
            {
                get
                {
                    return this.sptpHeader;
                }

                set
                {
                    this.sptpHeader = value;
                }
            }

            /// <summary>
            /// Returns a <see cref="System.String" /> that represents this instance.
            /// </summary>
            /// <returns>
            /// A <see cref="System.String" /> that represents this instance.
            /// </returns>
            public override string ToString()
            {
                return string.Format(Resource.Get(@"stIcdStringExt"), Version, (byte)Type, (byte)Flag, Apid, Segment, Counter, Size, SptpInfo);
            }

            /// <summary>
            /// Returns a <see cref="System.String" /> that represents this instance.
            /// </summary>
            /// <param name="extended">if set to <c>true</c> [extended].</param>
            /// <returns>
            /// A <see cref="System.String" /> that represents this instance.
            /// </returns>
            public string ToString(bool extended)
            {
                return extended ? this.ToString() : string.Format(Resource.Get(@"stIcdString"), Version, (byte)Type, (byte)Flag, Apid, Segment, Counter, Size, SptpInfo.ToString(extended));
            }
        }
    }

    /// <summary>
    /// Обмен сообщениями КБВ по протоколу Spacewire.
    /// </summary>
    public class SpacewireObtMsgEventArgs : SpacewireIcdMsgEventArgs
    {
        /// <summary>
        /// Агрегат доступа к заголовку кбв сообщения.
        /// </summary>
        private Obt _obtInfo;

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="SpacewireObtMsgEventArgs" />.
        /// </summary>
        /// <param name="data">"сырые" данные сообщения.</param>
        /// <param name="time1">Значение TimeTick1.</param>
        /// <param name="time2">Значение TimeTick2.</param>
        /// <param name="error">Значение Error.</param>
        /// <exception cref="System.ContextMarshalException">Если длина сообщения не достаточна для декодирования.</exception>
        public SpacewireObtMsgEventArgs(byte[] data, byte time1, byte time2, byte error = 0x00)
            : base(data, time1, time2, error)
        {
            if (16 != data.Length)
            {
                throw new ContextMarshalException(Resource.Get(@"eSpacewireObtData"));
            }
            
            try
            {
                byte[] nop;
                _obtInfo = Converter.MarshalTo<Obt>(data, out nop);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }

        /// <summary>
        /// Получает агрегат доступа к заголовку obt сообщения.
        /// </summary>
        /// <value>
        /// Агрегат доступа к заголовку obt сообщения.
        /// </value>
        public Obt ObtInfo
        {
            get
            {
                return _obtInfo;
            }
        }

        public override Sptp SptpInfo
        {
            get
            {
                return this.IcdInfo.SptpInfo;
            }
        }

        public override Icd IcdInfo
        {
            get
            {
                return _obtInfo.IcdInfo;
            }
        }

        /// <summary>
        /// Проверка на принадлежность к КБВ-сообщению.
        /// </summary>
        /// <param name="data">"сырые" данные посылки.</param>
        /// <returns><c>true</c> если проверка пройдена успешно, иначе <c>false</c>.</returns>
        public static new bool Test(byte[] data)
        {
            if (null != data ? 16 == data.Length : false)
            {
                byte[] raw = data.Skip(4).Take(4).ToArray();
                BitVector32 head = new BitVector32(ConvertToInt(raw));
                Icd icdInfo = new Icd();
                icdInfo.Header = head;
                return icdInfo.Version == 0
                       && icdInfo.Type == SpacewireIcdMsgEventArgs.IcdType.Tk
                       && icdInfo.Flag == SpacewireIcdMsgEventArgs.IcdFlag.HeaderEmpty;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Получить "сырое" представление посылки.
        /// </summary>
        /// <returns>
        /// Массив байт.
        /// </returns>
        public override byte[] ToArray()
        {
            return base.ToArray();
        }

        /// <summary>
        /// Агрегат доступа к заголовку сообщения Obt(код бортового времени).
        /// </summary>
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct Obt
        {
            /// <summary>
            /// Агрегат доступа к заголовку icd.
            /// </summary>
            private Icd _icdHeader;

            /// <summary>
            /// Значение поля Р: нормальное.
            /// </summary>
            private byte _normal;

            /// <summary>
            /// Значение поля Р: расширенное.
            /// </summary>
            private byte _extended;

            /// <summary>
            /// Значение поля Т: КБВ.
            /// </summary>
            private uint _obt;



            /// <summary>
            /// Получает или задает obt-поле "Поле P: нормальное".
            /// </summary>
            /// <value>
            /// Значение obt-поля "Поле Р: нормальное"
            /// </value>
            public byte Normal
            {
                get
                {
                    return _normal;
                }

                set
                {
                    _normal = value;
                }
            }

            /// <summary>
            /// Получает или задает obt-поле "Поле P: расширенное".
            /// </summary>
            /// <value>
            /// Значение obt-поля "Поле Р: расширенное"
            /// </value>
            public byte Extended
            {
                get
                {
                    return _extended;
                }

                set
                {
                    _extended = value;
                }
            }

            /// <summary>
            /// Получает или задает obt-поле "Поле Т: КБВ".
            /// </summary>
            /// <value>
            /// Значение obt-поля "Поле Т: КБВ"
            /// </value>
            public uint Value
            {
                get
                {
                    return _obt.ReverseBytes();
                }

                set
                {
                    _obt = value.ReverseBytes();
                }
            }

            /// <summary>
            /// Получает или задает агрегат доступа к заголовку icd сообщения.
            /// </summary>
            /// <value>
            /// Агрегат доступа к заголовку icd сообщения.
            /// </value>
            internal Icd IcdInfo
            {
                get
                {
                    return _icdHeader;
                }

                set
                {
                    _icdHeader = value;
                }
            }
        }
    }

    /// <summary>
    /// Обмен сообщениями телеметрии по протоколу Spacewire.
    /// </summary>
    public class SpacewireTmMsgEventArgs : SpacewireIcdMsgEventArgs
    {
        /// <summary>
        /// Агрегат доступа к заголовку сообщения телеметрии.
        /// </summary>
        private Tm msgInfo;

        /// <summary>
        /// Данные сообщения телеметрии.
        /// </summary>
        private byte[] _data;

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="SpacewireTmMsgEventArgs" />.
        /// </summary>
        /// <param name="data">"сырые" данные сообщения.</param>
        /// <param name="time1">Значение TimeTick1.</param>
        /// <param name="time2">Значение TimeTick2.</param>
        /// <param name="error">Значение Error.</param>
        /// <exception cref="System.ContextMarshalException">Если длина сообщения не достаточна для декодирования.</exception>
        public SpacewireTmMsgEventArgs(byte[] data, byte time1, byte time2, byte error = 0x00)
            : base(data, time1, time2, error)
        {
            if (22 > data.Length)
            {
                throw new ContextMarshalException(Resource.Get(@"eSmallSpacewireTmData"));
            }

            try
            {
                this.msgInfo = Converter.MarshalTo<Tm>(data, out _data);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }

        /// <summary>
        /// Получает агрегат доступа к заголовку tm сообщения.
        /// </summary>
        /// <value>
        /// Агрегат доступа к заголовку tm сообщения.
        /// </value>
        public virtual Tm TmInfo
        {
            get
            {
                return this.msgInfo;
            }
        }

        public override Sptp SptpInfo
        {
            get
            {
                return this.IcdInfo.SptpInfo;
            }
        }

        public override Icd IcdInfo
        {
            get
            {
                return this.msgInfo.IcdInfo;
            }
        }

        /// <summary>
        /// Получает данные сообщения телеметрии.
        /// </summary>
        /// <value>
        /// Данные сообщения телеметрии.
        /// </value>
        public new byte[] Data
        {
            get
            {
                return _data.Take(_data.Length - 2).ToArray();
            }
        }

        /// <summary>
        /// Получает значение CRC из сообщения.
        /// </summary>
        /// <value>
        /// Значение CRC из сообщения.
        /// </value>
        public ushort Crc
        {
            get
            {
                return (ushort)((_data[_data.Length - 2] << 8) | _data[_data.Length - 1]);
            }
        }

        /// <summary>
        /// Получает истинное значение CRC для текущего сообщения.
        /// </summary>
        /// <value>
        /// Истинное значение CRC для текущего сообщения.
        /// </value>
        public new ushort NeededCrc
        {
            get
            {
                return base.NeededCrc;
            }
        }


        /// <summary>
        /// Проверка на принадлежность к сообщению телеметрии.
        /// </summary>
        /// <param name="data">"сырые" данные посылки.</param>
        /// <returns><c>true</c> если проверка пройдена успешно, иначе <c>false</c>.</returns>
        public static new bool Test(byte[] data)
        {
            if (null != data ? 9 < data.Length : false)
            {
                byte[] raw = data.Skip(4).Take(4).ToArray();
                BitVector32 head = new BitVector32(ConvertToInt(raw));
                Icd icdInfo = new Icd();
                icdInfo.Header = head;
                return icdInfo.Version == 0
                       && icdInfo.Type == SpacewireIcdMsgEventArgs.IcdType.Tm
                       && icdInfo.Flag == SpacewireIcdMsgEventArgs.IcdFlag.HeaderFill;
            }
            else
            {
                return false;
            }
        }

        public override string ToString()
        {
            return string.Format(Resource.Get(@"stTmMsgToString"), this.DataLen, this.TmInfo.ToString(false), Converter.ByteArrayToHexStr(this.Data, isSmart: true), this.Crc == this.NeededCrc ? " " : "CRC error, [" + this.NeededCrc.ToString("X4") + "]");
        }

        /// <summary>
        /// Получить "сырое" представление посылки.
        /// </summary>
        /// <returns>
        /// Массив байт.
        /// </returns>
        public override byte[] ToArray()
        {
            return base.ToArray();
        }

        /// <summary>
        /// Агрегат доступа к заголовку сообщения Tm(телеметрии).
        /// </summary>
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct Tm
        {
            /// <summary>
            /// Маска поля заголовка поля данных: Резерв.
            /// </summary>
            private static readonly BitVector32.Section SubBitReserveSection = BitVector32.CreateSection(0x0F);

            /// <summary>
            /// Маска поля заголовка поля данных: Номер версии ТМ-пакета PUS.
            /// </summary>
            private static readonly BitVector32.Section VersionSection = BitVector32.CreateSection(0x07, SubBitReserveSection);

            /// <summary>
            /// Маска поля заголовка поля данных: Резерв.
            /// </summary>
            private static readonly BitVector32.Section BitReserveSection = BitVector32.CreateSection(0x01, VersionSection);

            /// <summary>
            /// Маска поля заголовка поля данных: Тип сервиса.
            /// </summary>
            private static readonly BitVector32.Section ServiceSection = BitVector32.CreateSection(0xFF, BitReserveSection);

            /// <summary>
            /// Маска поля заголовка поля данных: Подтип сервиса.
            /// </summary>
            private static readonly BitVector32.Section SubServiceSection = BitVector32.CreateSection(0xFF, ServiceSection);

            /// <summary>
            /// Маска поля заголовка поля данных: Резерв (п/о).
            /// </summary>
            private static readonly BitVector32.Section ReserveSection = BitVector32.CreateSection(0xFF, SubServiceSection);

            /// <summary>
            /// Агрегат доступа к заголовку icd.
            /// </summary>
            private SpacewireIcdMsgEventArgs.Icd _icdHeader;

            /// <summary>
            /// Заголовок сообщения телеметрии.
            /// </summary>
            private BitVector32 _header;

            /// <summary>
            /// Не используется.
            /// </summary>
            private ushort nullTerminated;

            /// <summary>
            /// Поле заголовка данных: Время.
            /// </summary>
            private int icdTime;



            /// <summary>
            /// Получает или задает значение резервного байта.
            /// </summary>
            /// <value>
            /// Значение резервного байта.
            /// </value>
            public byte Reserve
            {
                get
                {
                    return (byte)_header[ReserveSection];
                }

                set
                {
                    _header[ReserveSection] = (int)value;
                }
            }

            /// <summary>
            /// Получает или задает тип сервиса.
            /// </summary>
            /// <value>
            /// Тип сервиса.
            /// </value>
            public byte Service
            {
                get
                {
                    return (byte)_header[ServiceSection];
                }

                set
                {
                    _header[ServiceSection] = (int)value;
                }
            }

            /// <summary>
            /// Получает или задает подтип сервиса.
            /// </summary>
            /// <value>
            /// Подтип сервиса.
            /// </value>
            public byte SubService
            {
                get
                {
                    return (byte)_header[SubServiceSection];
                }

                set
                {
                    _header[SubServiceSection] = (int)value;
                }
            }

            /// <summary>
            /// Получает или задает значение резервного бита.
            /// </summary>
            /// <value>
            /// Хначение резервного бита.
            /// </value>
            public byte BitReserve
            {
                get
                {
                    return (byte)_header[BitReserveSection];
                }

                set
                {
                    _header[BitReserveSection] = (int)value;
                }
            }

            /// <summary>
            /// Получает или задает номер версии ТМ-пакета PUS.
            /// </summary>
            /// <value>
            /// Номер версии ТМ-пакета PUS.
            /// </value>
            public byte Version
            {
                get
                {
                    return (byte)_header[VersionSection];
                }

                set
                {
                    _header[VersionSection] = (int)value;
                }
            }

            /// <summary>
            /// Получает или задает значение резервного бита.
            /// </summary>
            /// <value>
            /// Значение резервного бита.
            /// </value>
            public byte SubBitReserve
            {
                get
                {
                    return (byte)_header[SubBitReserveSection];
                }

                set
                {
                    _header[SubBitReserveSection] = (int)value;
                }
            }

            /// <summary>
            /// Получает или задает опорное бортовое время пакета.
            /// </summary>
            /// <value>
            /// Опорное бортовое время пакета.
            /// </value>
            public int Time
            {
                get
                {
                    byte[] temp = BitConverter.GetBytes(this.icdTime);
                    Array.Reverse(temp);
                    return BitConverter.ToInt32(temp, 0);                   
                }

                set
                {
                    byte[] temp = BitConverter.GetBytes(value);
                    Array.Reverse(temp);
                    this.icdTime = BitConverter.ToInt32(temp, 0);
                }
            }

            /// <summary>
            /// Получает или задает агрегат доступа к заголовку icd сообщения.
            /// </summary>
            /// <value>
            /// Агрегат доступа к заголовку icd сообщения.
            /// </value>
            internal Icd IcdInfo
            {
                get
                {
                    return _icdHeader;
                }

                set
                {
                    _icdHeader = value;
                }
            }

            /// <summary>
            /// Returns a <see cref="System.String" /> that represents this instance.
            /// </summary>
            /// <returns>
            /// A <see cref="System.String" /> that represents this instance.
            /// </returns>
            public override string ToString()
            {
                return string.Format(Resource.Get(@"stTmStringExt"), BitReserve, Version, SubBitReserve, Service, SubService, Reserve, Time, IcdInfo);
            }

            /// <summary>
            /// Returns a <see cref="System.String" /> that represents this instance.
            /// </summary>
            /// <param name="extended">if set to <c>true</c> [extended].</param>
            /// <returns>
            /// A <see cref="System.String" /> that represents this instance.
            /// </returns>
            public string ToString(bool extended)
            {
                return extended ? this.ToString() : string.Format(Resource.Get(@"stTmString"), BitReserve, Version, SubBitReserve, Service, SubService, Reserve, Time, IcdInfo.ToString(extended));
            }
        }
    }

    public class SpacewireTm604MsgEventArgs : SpacewireTmMsgEventArgs
    {
        private Tm604 msgInfo;

        public SpacewireTm604MsgEventArgs(byte[] data, byte time1, byte time2, byte error = 0x00)
            : base(data, time1, time2, error)
        {
            if (790 > data.Length)
            {
                throw new ContextMarshalException(Resource.Get(@"eSmallSpacewireTm604Data"));
            }

            try
            {
                this.msgInfo = Converter.MarshalTo<Tm604>(data);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }


        public Tm604 Tm604Info
        {
            get
            {
                return this.msgInfo;
            }
        }


        public override Tm TmInfo
        {
            get
            {
                return this.Tm604Info.TmInfo;
            }
        }

        public override Sptp SptpInfo
        {
            get
            {
                return this.IcdInfo.SptpInfo;
            }
        }

        public override Icd IcdInfo
        {
            get
            {
                return this.TmInfo.IcdInfo;
            }
        }

        public static new bool Test(byte[] data)
        {
            if (null != data ? 9 < data.Length : false)
            {
                byte[] raw = data.Skip(4).Take(4).ToArray();
                BitVector32 head = new BitVector32(ConvertToInt(raw));
                Icd icdInfo = new Icd();
                icdInfo.Header = head;
                return icdInfo.Version == 0
                       && icdInfo.Type == SpacewireIcdMsgEventArgs.IcdType.Tm
                       && icdInfo.Flag == SpacewireIcdMsgEventArgs.IcdFlag.HeaderFill
                       && icdInfo.Apid == 0x604;
            }
            else
            {
                return false;
            }
        }


        public override byte[] ToArray()
        {
            return base.ToArray();
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct TmBuk
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 256)]
            private byte[] buf;

            public byte[] Buffer
            {
                get
                {
                    if (null == this.buf)
                    {
                        return new byte[256];
                    }

                    return this.buf;
                }
            }
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct TmKvv
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 512)]
            private byte[] buf;

            public byte[] Buffer
            {
                get
                {
                    if (null == this.buf)
                    {
                        return new byte[512];
                    }

                    return this.buf;
                }
            }
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct Tm604
        {
            private Tm telemHeader;

            private TmKvv tmkvvHeader;

            private TmBuk tmbukHeader;

            public TmBuk TmBukInfo
            {
                get
                {
                    return this.tmbukHeader;
                }
            }

            public TmKvv TmKvvInfo
            {
                get
                {
                    return this.tmkvvHeader;
                }
            }

            internal Tm TmInfo
            {
                get
                {
                    return this.telemHeader;
                }

                set
                {
                    this.telemHeader = value;
                }
            }

            public override string ToString()
            {
                return string.Empty;
            }

            public string ToString(bool extended)
            {
                return extended ? this.ToString() : string.Empty;
            }
        }






    }

    /// <summary>
    /// Обмен сообщениями телекоманд по протоколу Spacewire.
    /// </summary>
    public class SpacewireTkMsgEventArgs : SpacewireIcdMsgEventArgs
    {
        private const int HeaderSize = 3;
        private const int CrcSize = sizeof(ushort);

        /// <summary>
        /// Для счетчика последовательности телекоманд.
        /// </summary>
        private static Dictionary<short, AutoCounter> _dict;

        /// <summary>
        /// Агрегат доступа к заголовку телекоманды.
        /// </summary>
        private Tk msgInfo;

        /// <summary>
        /// Данные телекоманды.
        /// </summary>
        private byte[] data;



        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="SpacewireTkMsgEventArgs" />.
        /// </summary>
        /// <param name="data">"сырые" данные сообщения.</param>
        /// <param name="time1">Значение TimeTick1.</param>
        /// <param name="time2">Значение TimeTick2.</param>
        /// <param name="error">Значение Error.</param>
        /// <exception cref="System.ContextMarshalException">Если длина сообщения не достаточна для декодирования.</exception>
        public SpacewireTkMsgEventArgs(byte[] data, byte time1, byte time2, byte error = 0x00)
            : base(data, time1, time2, error)
        {
            if (16 > data.Length)
            {
                throw new ContextMarshalException(Resource.Get(@"eSmallSpacewireTkData"));
            }
            
            try
            {
                this.msgInfo = Converter.MarshalTo<Tk>(data, out this.data);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }

        /// <summary>
        /// Получает агрегат доступа к заголовку сообщения телекоманды.
        /// </summary>
        /// <value>
        /// Агрегат доступа к заголовку сообщения телекоманды.
        /// </value>
        public Tk TkInfo
        {
            get
            {
                return this.msgInfo;
            }
        }

        public override Sptp SptpInfo
        {
            get
            {
                return this.IcdInfo.SptpInfo;
            }
        }

        public override Icd IcdInfo
        {
            get
            {
                return this.msgInfo.IcdInfo;
            }
        }

        /// <summary>
        /// Получает данные сообщения телекоманды.
        /// </summary>
        /// <value>
        /// Данные сообщения телекоманды.
        /// </value>
        public new byte[] Data
        {
            get
            {
                return this.data.Take(this.data.Length - 2).ToArray();
            }
        }

        /// <summary>
        /// Получает значение CRC из сообщения.
        /// </summary>
        /// <value>
        /// Значение CRC из сообщения.
        /// </value>
        public ushort Crc
        {
            get
            {
                return (ushort)((this.data[this.data.Length - 2] << 8) | this.data[this.data.Length - 1]);
            }
        }

        /// <summary>
        /// Получает истинное значение CRC для текущего сообщения.
        /// </summary>
        /// <value>
        /// Истинное значение CRC для текущего сообщения.
        /// </value>
        public new ushort NeededCrc
        {
            get
            {
                return base.NeededCrc;
            }
        }

        /// <summary>
        /// Формирует телекоманду в виде посылки.
        /// </summary>
        /// <param name="data">Данные телекоманды.</param>
        /// <param name="to">Адрес прибора назначения.</param>
        /// <param name="from">Адрес прибора инициатора.</param>
        /// <param name="apid">Apid прибора назначения.</param>
        /// <param name="isReceipt">если установлено <c>true</c> [включить подтверждение получения].</param>
        /// <param name="isExec">если установлено <c>true</c> [включить подтверждение исполнения].</param>
        /// <returns>
        /// Сформированное spacewire-сообщение телекоманды.
        /// </returns>
        public static SpacewireTkMsgEventArgs GetNew(byte[] data, byte to, byte from, short apid, bool isReceipt, bool isExec)
        {
            if (null == _dict)
            {
                _dict = new Dictionary<short, AutoCounter>();
            }

            Sptp sptpInfo = new Sptp();
            sptpInfo.From = from;
            sptpInfo.MsgType = SptpType.Data;
            sptpInfo.ProtocolId = SptpProtocol.Standard;
            sptpInfo.To = to;

            Icd icdInfo = new Icd();
            icdInfo.Version = 0;
            icdInfo.Type = IcdType.Tk;
            icdInfo.Flag = IcdFlag.HeaderFill;
            icdInfo.Apid = apid;
            icdInfo.Segment = 3;
            icdInfo.SptpInfo = sptpInfo;
            icdInfo.Size = (ushort)(data.Length + CrcSize + HeaderSize);
            if (!_dict.ContainsKey(apid))
            {
                _dict.Add(apid, new AutoCounter());
            }

            icdInfo.Counter = (short)_dict[apid];

            Tk telecmdInfo = new Tk();
            telecmdInfo.IcdInfo = icdInfo;
            telecmdInfo.Version = 1;
            telecmdInfo.Acknowledgment = (byte)((Convert.ToInt32(isReceipt) << 3) | Convert.ToInt32(isExec));

            byte[] rawData = Converter.MarshalFrom<Tk>(telecmdInfo, ref data);

            ushort crc = Crc16.Get(rawData, rawData.Length, 4);
            Array.Resize(ref rawData, rawData.Length + CrcSize);
            rawData[rawData.GetUpperBound(0) - 1] = (byte)(crc >> 8);
            rawData[rawData.GetUpperBound(0)] = (byte)crc;
            return new SpacewireTkMsgEventArgs(rawData, 0x00, 0x00);
        }

        /// <summary>
        /// Проверка на принадлежность к сообщению телекоманды.
        /// </summary>
        /// <param name="data">"сырые" данные посылки.</param>
        /// <returns><c>true</c> если проверка пройдена успешно, иначе <c>false</c>.</returns>
        public static new bool Test(byte[] data)
        {
            if (null != data ? 9 < data.Length : false)
            {
                byte[] raw = data.Skip(4).Take(4).ToArray();
                BitVector32 head = new BitVector32(ConvertToInt(raw));
                Icd icdInfo = new Icd();
                icdInfo.Header = head;
                return icdInfo.Version == 0
                       && icdInfo.Type == SpacewireIcdMsgEventArgs.IcdType.Tk
                       && icdInfo.Flag == SpacewireIcdMsgEventArgs.IcdFlag.HeaderFill;
            }
            else
            {
                return false;
            }
        }

        public override string ToString()
        {
            return string.Format(Resource.Get(@"stTkMsgToString"), this.DataLen, this.TkInfo.ToString(false), Converter.ByteArrayToHexStr(this.Data, isSmart: true), this.Crc == this.NeededCrc ? " " : "CRC error, [" + this.NeededCrc.ToString("X4") + "]");
        }



        /// <summary>
        /// Получить "сырое" представление посылки.
        /// </summary>
        /// <returns>
        /// Массив байт.
        /// </returns>
        public override byte[] ToArray()
        {
            return base.ToArray();
        }  

        /// <summary>
        /// Агрегат доступа к заголовку сообщения Tk(телекоманды).
        /// </summary>
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct Tk
        {
            /// <summary>
            /// Агрегат доступа к icd заголовку.
            /// </summary>
            private Icd icdHeader;

            private byte flagVersionAcknowledgment;

            private byte service;

            private byte subService;



            /// <summary>
            /// Получает или задает тип сервиса.
            /// </summary>
            /// <value>
            /// Тип сервиса.
            /// </value>
            public byte Service
            {
                get
                {
                    return this.service;
                }

                set
                {
                    this.service = value;
                }
            }

            /// <summary>
            /// Получает или задает подтип сервиса.
            /// </summary>
            /// <value>
            /// Подтип сервиса.
            /// </value>
            public byte SubService
            {
                get
                {
                    return this.subService;
                }

                set
                {
                    this.subService = value;
                }
            }

            /// <summary>
            /// Получает или задает тип квитирования.
            /// </summary>
            /// <value>
            /// Тип квитирования.
            /// </value>
            public byte Acknowledgment
            {
                get
                {
                    return (byte)(this.flagVersionAcknowledgment & 0xF);
                }

                set
                {
                    this.flagVersionAcknowledgment |= (byte)(value & 0xF);
                }
            }

            /// <summary>
            /// Получает или задает новер версии ТК-пакета PUS.
            /// </summary>
            /// <value>
            /// Номер версии ТК-пакета PUS.
            /// </value>
            public byte Version
            {
                get
                {
                    return (byte)((this.flagVersionAcknowledgment >> 4) & 0x7);
                }

                set
                {
                    this.flagVersionAcknowledgment |= (byte)((value & 0x7) << 4);
                }
            }

            /// <summary>
            /// Получает или задает флаг вторичного заголовка CCSDS.
            /// </summary>
            /// <value>
            /// Флаг вторичного заголовка CCSDS.
            /// </value>
            public byte Flag
            {
                get
                {
                    return (byte)((this.flagVersionAcknowledgment >> 7) & 0x1);
                }

                set
                {
                    this.flagVersionAcknowledgment |= (byte)((value & 0x1) << 7);
                }
            }

            /// <summary>
            /// Получает или задает агрегат доступа к заголовку icd.
            /// </summary>
            /// <value>
            /// Агрегат доступа к заголовку icd.
            /// </value>
            internal Icd IcdInfo
            {
                get
                {
                    return this.icdHeader;
                }

                set
                {
                    this.icdHeader = value;
                }
            }

            /// <summary>
            /// Returns a <see cref="System.String" /> that represents this instance.
            /// </summary>
            /// <returns>
            /// A <see cref="System.String" /> that represents this instance.
            /// </returns>
            public override string ToString()
            {
                return string.Format(Resource.Get(@"stTkStringExt"), Flag, Version, Acknowledgment, Service, SubService, IcdInfo);
            }

            /// <summary>
            /// Returns a <see cref="System.String" /> that represents this instance.
            /// </summary>
            /// <param name="extended">if set to <c>true</c> [extended].</param>
            /// <returns>
            /// A <see cref="System.String" /> that represents this instance.
            /// </returns>
            public string ToString(bool extended)
            {
                return extended ? this.ToString() : string.Format(Resource.Get(@"stTkString"), Flag, Version, Acknowledgment, Service, SubService, IcdInfo.ToString(extended));
            }
        }

    }

    /// <summary>
    /// Обмен сообщениями об ошибках по протоколу Spacewire.
    /// </summary>
    public class SpacewireErrorMsgEventArgs : BaseMsgEventArgs
    {
        /// <summary>
        /// Сообщение об ошибке.
        /// </summary>
        private string errorMsg;

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="SpacewireErrorMsgEventArgs" />.
        /// </summary>
        /// <param name="data">"сырые" данные сообщения.</param>
        /// <param name="time1">Значение TimeTick1.</param>
        /// <param name="time2">Значение TimeTick2.</param>
        /// <param name="error">Значение Error.</param>
        /// <param name="msg">Сообщение об ошибке.</param>
        public SpacewireErrorMsgEventArgs(byte[] data, byte time1, byte time2, byte error = 0x00, string msg = null)
            : base(data)
        {
             this.errorMsg = null == msg ? string.Empty : msg;   
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
        /// Получает данные сообщения.
        /// </summary>
        public new byte[] Data
        {
            get
            {
                return base.Data;
            }
        }

        public override string ToString()
        {
            return string.Format(Resource.Get(@"stErrorMsgToString"), this.DataLen, Converter.ByteArrayToHexStr(this.Data, isSmart: true), this.ErrorMessage());
        }

        /// <summary>
        /// Сообщение об ошибке.
        /// </summary>
        /// <returns>Строка сообщения об ошибке.</returns>
        public string ErrorMessage()
        {
            return this.errorMsg;
        }
    }

    /// <summary>
    /// Обмен сообщениями по протоколу Spacewire (не используя декодер протоколов).
    /// </summary>
    public class SpacewireEmptyProtoMsgEventArgs : BaseMsgEventArgs
    {
        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="SpacewireEmptyProtoMsgEventArgs" />.
        /// </summary>
        /// <param name="data">"сырые" данные сообщения.</param>
        /// <param name="time1">Значение TimeTick1.</param>
        /// <param name="time2">Значение TimeTick2.</param>
        /// <param name="error">Значение Error.</param>
        /// <exception cref="System.ContextMarshalException">Если длина сообщения не достаточна для декодирования.</exception>
        public SpacewireEmptyProtoMsgEventArgs(byte[] data, byte time1, byte time2, byte error = 0x00)
            : base(data)
        {
            if (0 >= data.Length)
            {
                throw new ContextMarshalException(Resource.Get(@"eSmallSpacewireData"));
            }
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
        /// "Сырая" проверка на принадлежность к empty-сообщению.
        /// </summary>
        /// <param name="data">"сырые" данные посылки.</param>
        /// <returns><c>true</c> если проверка пройдена успешно, иначе <c>false</c>.</returns>
        public static new bool Test(byte[] data)
        {
            return null != data ? 0 <= data.Length : false;
        }

        /// <summary>
        /// Возвращает данные экземпляра как массив байт.
        /// </summary>
        /// <returns>
        /// Массив байт.
        /// </returns>
        public override byte[] ToArray()
        {
            return base.Data;
        }
    }

    /// <summary>
    /// Обмен сообщениями по протоколу Sptp.
    /// </summary>
    public class SpacewireSptpMsgEventArgs : BaseMsgEventArgs
    {
        /// <summary>
        /// Агрегат доступа к заголовку sptp-сообщения.
        /// </summary>
        private Sptp msgInfo;

        /// <summary>
        /// Данные сообщения.
        /// </summary>
        private byte[] _data;

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="SpacewireSptpMsgEventArgs" />.
        /// </summary>
        /// <param name="data">"сырые" данные сообщения.</param>
        /// <param name="time1">Значение TimeTick1.</param>
        /// <param name="time2">Значение TimeTick2.</param>
        /// <param name="error">Значение Error.</param>
        /// <exception cref="System.ContextMarshalException">Если длина сообщения не достаточна для декодирования.</exception>
        public SpacewireSptpMsgEventArgs(byte[] data, byte time1, byte time2, byte error = 0x00)
            : base(data)
        {
            if (4 > data.Length)
            {
                throw new ContextMarshalException(Resource.Get(@"eSmallSpacewireData"));
            }
            
            try
            { 
                this.msgInfo = Converter.MarshalTo<Sptp>(data, out _data);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }

        /// <summary>
        /// Протокол сообщения sptp.
        /// </summary>
        public enum SptpProtocol : byte
        {
            /// <summary>
            /// Стандартный протокол SPTP: 0xF2.
            /// </summary>
            Standard = 0xf2
        }

        /// <summary>
        /// Тип сообщения sptp.
        /// </summary>
        public enum SptpType
        {
            /// <summary>
            /// Сообщение "запрос квоты".
            /// </summary>
            Request = 0x80,

            /// <summary>
            /// Сообщение "предоставление квоты".
            /// </summary>
            Reply = 0xC0,

            /// <summary>
            /// Сообщение "передача данных".
            /// </summary>                        
            Data = 0x00
        }

        /// <summary>
        /// Получает агрегат доступа к заголовку Sptp.
        /// </summary>
        /// <value>
        /// Агрегат доступа к заголовку Sptp.
        /// </value>
        public virtual Sptp SptpInfo
        {
            get
            {
                return this.msgInfo;
            }
        }

        /// <summary>
        /// Получает данные сообщения.
        /// </summary>
        public new byte[] Data
        {
            get
            {
                return _data;
            }
        }

        /// <summary>
        /// Получает или задает значение поля: ERROR.
        /// </summary>
        /// <value>
        /// Значение поля: ERROR.
        /// </value>
        public byte Error { get; set; }

        /// <summary>
        /// Получает или задает значение поля: TIMETICK1.
        /// </summary>
        /// <value>
        /// Значение поля: TIMETICK1.
        /// </value>
        public byte Time1 { get; set; }

        /// <summary>
        /// Получает или задает значение поля: TIMETICK2.
        /// </summary>
        /// <value>
        /// Значение поля: TIMETICK2.
        /// </value>
        public byte Time2 { get; set; }

        /// <summary>
        /// Получает значение CRC, сгенерированного для данного сообщения.
        /// </summary>
        /// <value>
        /// Значение CRC, сгенерированного для данного сообщения.
        /// </value>
        protected override ushort NeededCrc
        {
            get
            {
                return Crc16.Get(Data, Data.Length - 2);
            }
        }

        /// <summary>
        /// Формирует sptp-сообщение.
        /// </summary>
        /// <param name="data">Данные sptp-сообщения.</param>
        /// <param name="to">Адрес прибора назначения.</param>
        /// <param name="from">Адрес прибора инициатора.</param>
        /// <returns>Сформированное spacewire-сообщение.</returns>
        public static SpacewireSptpMsgEventArgs GetNew(byte[] data, byte to, byte from)
        {
            Sptp sptpInfo = new Sptp();
            sptpInfo.From = from;
            sptpInfo.MsgType = SptpType.Data;
            sptpInfo.ProtocolId = SptpProtocol.Standard;
            sptpInfo.To = to;

            byte[] rawData = Converter.MarshalFrom<Sptp>(sptpInfo, ref data);

            return new SpacewireSptpMsgEventArgs(rawData, 0x00, 0x00);
        }

        /// <summary>
        /// "Сырая" проверка на принадлежность к sptp-сообщению.
        /// </summary>
        /// <param name="data">"сырые" данные посылки.</param>
        /// <returns><c>true</c> если проверка пройдена успешно, иначе <c>false</c>.</returns>
        public static new bool Test(byte[] data)
        {
            return null != data ? 3 < data.Length : false;
        }

        public override string ToString()
        {
            return string.Format(Resource.Get(@"stSptpMsgToString"), this.DataLen, this.SptpInfo.ToString(false), Converter.ByteArrayToHexStr(this.Data, isSmart: true));
        }




        /// <summary>
        /// Преобразует данные экземпляра к массиву байт.
        /// </summary>
        /// <returns>
        /// Массив байт.
        /// </returns>
        public override byte[] ToArray()
        {
            return base.Data;
        }

        /// <summary>
        /// Агрегат доступа к заголовку сообщения Sptp.
        /// </summary>
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct Sptp
        {
            /// <summary>
            /// Маска поля заголовка: адрес прибора назначения.
            /// </summary>
            private static readonly BitVector32.Section ToSection = BitVector32.CreateSection(0xFF);

            /// <summary>
            /// Маска поля заголовка: идентификатор протокола.
            /// </summary>
            private static readonly BitVector32.Section ProtocolIdSection = BitVector32.CreateSection(0xFF, ToSection);

            /// <summary>
            /// Маска поля заголовка: тип сообщения.
            /// </summary>
            private static readonly BitVector32.Section MsgTypeSection = BitVector32.CreateSection(0xFF, ProtocolIdSection);

            /// <summary>
            /// Маска поля заголовка: адрес прибора инициатора.
            /// </summary>
            private static readonly BitVector32.Section FromSection = BitVector32.CreateSection(0xFF, MsgTypeSection);

            /// <summary>
            /// Заголовок sptp-сообщения.
            /// </summary>
            private BitVector32 header;

            /// <summary>
            /// Получает или задает значение поля: Адрес прибора назначения.
            /// </summary>
            /// <value>
            /// Значение поля: Адрес прибора назначения.
            /// </value>
            public byte To
            {
                get
                {
                    return (byte)this.header[ToSection];
                }

                set
                {
                    this.header[ToSection] = (int)value;
                }
            }

            /// <summary>
            /// Получает или задает значение поля: протокол передачи.
            /// </summary>
            /// <value>
            /// Значение поля: протокол передачи.
            /// </value>
            public SptpProtocol ProtocolId
            {
                get
                {
                    return (SptpProtocol)this.header[ProtocolIdSection];
                }

                set
                {
                    this.header[ProtocolIdSection] = (int)value;
                }
            }

            /// <summary>
            /// Получает или задает значение поля: тип сообщения.
            /// </summary>
            /// <value>
            /// Значение поля: тип сообщения.
            /// </value>
            public SptpType MsgType
            {
                get
                {
                    return (SptpType)this.header[MsgTypeSection];
                }

                set
                {
                    this.header[MsgTypeSection] = (int)value;
                }
            }

            /// <summary>
            /// Получает или задает значение поля: Адрес прибора инициатора.
            /// </summary>
            /// <value>
            /// Значение поля: Адрес прибора инициатора.
            /// </value>
            public byte From
            {
                get
                {
                    return (byte)this.header[FromSection];
                }

                set
                {
                    this.header[FromSection] = (int)value;
                }
            }

            /// <summary>
            /// Returns a <see cref="System.String" /> that represents this instance.
            /// </summary>
            /// <returns>
            /// A <see cref="System.String" /> that represents this instance.
            /// </returns>
            public override string ToString()
            {
                return string.Format(Resource.Get(@"stSptpStringExt"), To, (byte)ProtocolId, (byte)MsgType, From);
            }

            /// <summary>
            /// Returns a <see cref="System.String" /> that represents this instance.
            /// </summary>
            /// <param name="extended">if set to <c>true</c> [extended].</param>
            /// <returns>
            /// A <see cref="System.String" /> that represents this instance.
            /// </returns>
            public string ToString(bool extended)
            {
                return extended ? this.ToString() : string.Format(Resource.Get(@"stSptpString"), To, (byte)ProtocolId, (byte)MsgType, From);
            }
        }
    }

    /// <summary>
    /// Обмен сообщениями TimeTick по протоколу Spacewire.
    /// </summary>
    public class SpacewireTimeTickMsgEventArgs : BaseMsgEventArgs
    {
        /// <summary>
        /// Агрегат доступа к заголовку timetick-сообщения.
        /// </summary>
        private TimeTick _timeTickInfo;

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="SpacewireTimeTickMsgEventArgs" />.
        /// </summary>
        /// <param name="data">"сырые" данные сообщения.</param>
        /// <param name="time1">Значение TimeTick1.</param>
        /// <param name="time2">Значение TimeTick2.</param>
        /// <param name="error">Значение Error.</param>
        /// <exception cref="System.ContextMarshalException">Если длина сообщения не достаточна для декодирования.</exception>
        public SpacewireTimeTickMsgEventArgs(byte[] data, byte time1, byte time2, byte error = 0x00)
            : base(data)
        {
            if (1 != data.Length)
            {
                throw new ContextMarshalException(Resource.Get(@"eTickTimeSpacewireData"));
            }
            
            try
            {
                byte[] nop;
                _timeTickInfo = Converter.MarshalTo<TimeTick>(data, out nop);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }

        /// <summary>
        /// Получает агрегат доступа к заголовку timetick-сообщения.
        /// </summary>
        /// <value>
        /// Агрегат доступа к заголовку timetick-сообщения.
        /// </value>
        public TimeTick TimeTickInfo
        {
            get
            {
                return _timeTickInfo;
            }
        }

        /// <summary>
        /// Преобразует данные экземпляра к массиву байт.
        /// </summary>
        /// <returns>
        /// Массив байт.
        /// </returns>
        public override byte[] ToArray()
        {
            return Data;
        }

        /// <summary>
        /// Агрегат доступа к заголовку сообщения TimeTick.
        /// </summary>
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct TimeTick
        {
            /// <summary>
            /// Значение поля: Tick time.
            /// </summary>
            private byte _tick;

            /// <summary>
            /// Получает или задает значение Time tick.
            /// </summary>
            /// <value>
            /// Значение Time tick.
            /// </value>
            public byte Value
            {
                get
                {
                    return _tick;
                }

                set
                {
                    _tick = value;
                }
            }
        }       
    }
}
