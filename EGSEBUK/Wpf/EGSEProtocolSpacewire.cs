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
    using System.Windows;

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
        public delegate void SpacewireMsgEventHandler(object sender, MsgBase e);

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
                    MsgBase pack = null;                  
                    byte[] arr = _buf.ToArray();
                    try
                    {                     
                        if (10 < arr.Length)
                        {
                            pack = new SpacewireIcdMsgEventArgs(arr, _currentTime1, _currentTime2, msg.Data[0]);
                        }
                        else
                        {
                            pack = new SpacewireSptpMsgEventArgs(arr, _currentTime1, _currentTime2, msg.Data[0]);
                        }                       
                    }
                    catch (ContextMarshalException e)
                    {
                        pack = new SpacewireErrorMsgEventArgs(arr, arr.Length, _currentTime1, _currentTime2, msg.Data[0]);
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
        protected virtual void OnSpacewireMsg(object sender, MsgBase e)
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
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct Icd
        {
            //internal const int MaxLengthOfSpacewirePackage = 0x10004;
            private static readonly BitVector32.Section apidHiSection = BitVector32.CreateSection(0x07);
            private static readonly BitVector32.Section flagSection = BitVector32.CreateSection(0x01, apidHiSection);
            private static readonly BitVector32.Section typeSection = BitVector32.CreateSection(0x01, flagSection);
            private static readonly BitVector32.Section versionSection = BitVector32.CreateSection(0x07, typeSection);
            private static readonly BitVector32.Section apidLoSection = BitVector32.CreateSection(0xFF, versionSection);
            private static readonly BitVector32.Section counterHiSection = BitVector32.CreateSection(0x3F, apidLoSection);
            private static readonly BitVector32.Section segmentSection = BitVector32.CreateSection(0x03, counterHiSection);
            private static readonly BitVector32.Section counterLoSection = BitVector32.CreateSection(0xFF, segmentSection);

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
            private byte[] _nop;

            internal BitVector32 _header;

            private ushort _size;

            //[MarshalAs(UnmanagedType.ByValArray, SizeConst = MaxLengthOfSpacewirePackage)]
            //private byte[] data;

            //internal byte[] Data
            //{
            //    get
            //    {
            //        return data;
            //    }
            //}

            public byte Version
            {
                get
                {
                    return (byte)_header[versionSection];
                }

                set
                {
                    _header[versionSection] = (int)value;
                }
            }

            public IcdType Type
            {
                get
                {
                    return (IcdType)_header[typeSection];
                }

                set
                {
                    _header[typeSection] = (int)value;
                }
            }

            public IcdFlag Flag
            {
                get
                {
                    return (IcdFlag)_header[flagSection];
                }

                set
                {
                    _header[flagSection] = (int)value;
                }
            }

            public short Apid
            {
                get
                {
                    return (short)((_header[apidHiSection] << 8) | _header[apidLoSection]);
                }

                set
                {
                    _header[apidLoSection] = (byte)value;
                    _header[apidHiSection] = (byte)(value >> 8);
                }
            }

            public byte Segment
            {
                get
                {
                    return (byte)_header[segmentSection];
                }

                set
                {
                    _header[segmentSection] = (int)value;
                }
            }

            public short Counter
            {
                get
                {
                    return (short)((_header[counterHiSection] << 8) | _header[counterLoSection]);
                }

                set
                {
                    _header[counterLoSection] = (byte)value;
                    _header[counterHiSection] = (byte)(value >> 8);
                }
            }

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
        }

        private Icd _icdInfo;

        protected override ushort NeededCrc
        {
            get
            {
                return base.NeededCrc;
            }
        }

        public Icd IcdInfo
        {
            get
            {
                return _icdInfo;
            }
        }

        private byte[] _data;

        public enum IcdType
        {
            Tk = 0x01,
            Tm = 0x00
        }

        public enum IcdFlag
        {
            HeaderFill = 0x01,
            HeaderEmpty = 0x00
        }

        public new byte[] Data
        {
            get
            {
                // возвращаем данные без учета заголовка
                return _data;
                //return IcdInfo.Data.Take(base.Data.Length - 6).ToArray();
            }
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
            if ((10 > data.Length) || (10 > base.DataLen))
            {
                throw new ContextMarshalException(Resource.Get(@"eSmallSpacewireIcdData"));
            }

            //if (Icd.MaxLengthOfSpacewirePackage < data.Length)
            //{
            //    throw new ContextMarshalException(Resource.Get(@"eBigSpacewireData"));
            //}

            // преобразуем данные к структуре Sptp.

            //GCHandle pinnedInfo = GCHandle.Alloc(data, GCHandleType.Pinned);
            //try
            //{
            //    _icdInfo = (Icd)Marshal.PtrToStructure(pinnedInfo.AddrOfPinnedObject(), typeof(Icd));
            //}
            //finally
            //{
            //    if (pinnedInfo.IsAllocated)
            //    {
            //        pinnedInfo.Free();
            //    }
            //}

            // преобразуем данные к структуре Sptp.
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
            return base.ToArray();
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
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct Tk
        {
            //internal const int MaxLengthOfSpacewirePackage = 0x10004;
            //private static readonly BitVector32.Section apidHiSection = BitVector32.CreateSection(0x07);
            //private static readonly BitVector32.Section flagSection = BitVector32.CreateSection(0x01, apidHiSection);
            //private static readonly BitVector32.Section typeSection = BitVector32.CreateSection(0x01, flagSection);
            //private static readonly BitVector32.Section versionSection = BitVector32.CreateSection(0x07, typeSection);
            //private static readonly BitVector32.Section apidLoSection = BitVector32.CreateSection(0xFF, versionSection);
            //private static readonly BitVector32.Section counterHiSection = BitVector32.CreateSection(0x3F, apidLoSection);
            //private static readonly BitVector32.Section segmentSection = BitVector32.CreateSection(0x03, counterHiSection);
            //private static readonly BitVector32.Section counterLoSection = BitVector32.CreateSection(0xFF, segmentSection);
         
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 10)]
            private byte[] _nop;

            private BitVector32 _header;

            //[MarshalAs(UnmanagedType.ByValArray, SizeConst = MaxLengthOfSpacewirePackage)]
            //private byte[] data;

            //internal byte[] Data
            //{
            //    get
            //    {
            //        return data;
            //    }
            //}
        }

        private Tk _tkInfo;

        private byte[] _data;

        public Tk TkInfo
        {
            get
            {
                return _tkInfo;
            }
        }

        public new byte[] Data
        {
            get
            {
                // возвращаем данные без учета заголовка
                return _data.Take(_data.Length - 2).ToArray();
                //return TkInfo.Data.Take(IcdInfo.Size).ToArray();
            }
        }

        public ushort Crc
        {
            get
            {
                return (ushort)((_data[_data.Length - 2] << 8) | (_data[_data.Length - 1]));
            }
        }

        public new ushort NeededCrc
        {
            get
            {
                return base.NeededCrc;
                //return Crc16.Get(base.Data, base.Data.Length - 2/*, 0, base.NeededCrc*/);
            }
        }

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="SpacewireTkMsgEventArgs" />.
        /// </summary>
        /// <param name="apid">The apid.</param>
        /// <param name="dict">The dictionary.</param>
        /// <param name="data">The data.</param>
        public SpacewireTkMsgEventArgs(byte[] data, byte time1, byte time2, byte error = 0x00)
            : base(data, time1, time2, error)
        {
            if ((16 > data.Length) || (16 > base.DataLen))
            {
                throw new ContextMarshalException(Resource.Get(@"eSmallSpacewireTkData"));
            }

            //if (Tk.MaxLengthOfSpacewirePackage < data.Length)
            //{
            //    throw new ContextMarshalException(Resource.Get(@"eBigSpacewireData"));
            //}

            // преобразуем данные к структуре Tk.

            //GCHandle pinnedInfo = GCHandle.Alloc(data, GCHandleType.Pinned);
            //try
            //{
            //    _tkInfo = (Tk)Marshal.PtrToStructure(pinnedInfo.AddrOfPinnedObject(), typeof(Tk));
            //}
            //finally
            //{
            //    if (pinnedInfo.IsAllocated)
            //    {
            //        pinnedInfo.Free();
            //    }
            //}

            // преобразуем данные к структуре Tk.
            try
            {
                _tkInfo = Converter.MarshalTo<Tk>(data, out _data);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }

        public static SpacewireTkMsgEventArgs GetNew(byte[] data, byte to, byte from, byte apid, Dictionary<byte, AutoCounter> dict)
        {
            byte[] buf = new byte[data.Length + 16];
            buf[0] = to;
            buf[1] = 0xf2;
            buf[2] = 0x00;
            buf[3] = from;
            Icd icdInfo = new Icd();
            icdInfo.Version = 0;
            icdInfo.Type = IcdType.Tk;
            icdInfo.Flag = IcdFlag.HeaderFill;
            icdInfo.Apid = apid;
            if (!dict.ContainsKey(apid))
                {
                   dict.Add(apid, new AutoCounter());
                }       
            icdInfo.Counter = (short)dict[apid];
            int header = icdInfo._header.Data;
            buf[7] = (byte)(header >> 24);
            buf[6] = (byte)(header >> 16);
            buf[5] = (byte)(header >> 8);
            buf[4] = (byte)(header);
            Array.Copy(data, 0, buf, 14, data.Length);
            ushort crc = Crc16.Get(buf, buf.Length - 2, 4);
            buf[buf.Length - 2] = (byte)(crc >> 8);
            buf[buf.Length - 1] = (byte)crc;
            return new SpacewireTkMsgEventArgs(buf, 0x00, 0x00);
        }

        /// <summary>
        /// Преобразует данные экземпляра к массиву байт.
        /// </summary>
        /// <returns>
        /// Массив байт.
        /// </returns>
        public override byte[] ToArray()
        {
            return base.ToArray();
        }        
    }

    public class SpacewireErrorMsgEventArgs : MsgBase
    {
        public SpacewireErrorMsgEventArgs(byte[] data, int dataLen, byte time1, byte time2, byte error = 0x00)
            : base(data, dataLen)
        {
        }

        public byte[] Data
        {
            get
            {
                return base.Data;
            }
        }

        public int DataLen
        {
            get
            {
                return base.DataLen;
            }
        }
    }

    /// <summary>
    /// Предоставляет аргументы sptp-протокола для события, созданного полученным сообщением по протоколу spacewire.
    /// </summary>
    public class SpacewireSptpMsgEventArgs : MsgBase
    {
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct Sptp
        {
            //internal const int MaxLengthOfSpacewirePackage = 0x10004;
            private static readonly BitVector32.Section toSection = BitVector32.CreateSection(0xFF);
            private static readonly BitVector32.Section protocolIdSection = BitVector32.CreateSection(0xFF, toSection);
            private static readonly BitVector32.Section msgTypeSection = BitVector32.CreateSection(0xFF, protocolIdSection);
            private static readonly BitVector32.Section fromSection = BitVector32.CreateSection(0xFF, msgTypeSection);

            private BitVector32 _header;

            //[MarshalAs(UnmanagedType.ByValArray, SizeConst = MaxLengthOfSpacewirePackage)]
            //private byte[] _data;

            //internal byte[] Data
            //{
            //    get
            //    {
            //        return _data;
            //    }
            //}

            public byte To
            {
                get 
                {
                    return (byte)_header[toSection];
                }

                set
                {
                    _header[toSection] = (int)value;
                }
            }

            public byte ProtocolId
            {
                get
                {
                    return (byte)_header[protocolIdSection];
                }

                set
                {
                    _header[protocolIdSection] = (int)value;
                }
            }

            public SptpType MsgType
            {
                get
                {
                    return (SptpType)_header[msgTypeSection];
                }

                set
                {
                    _header[msgTypeSection] = (int)value;
                }
            }

            public byte From
            {
                get
                {
                    return (byte)_header[fromSection];
                }

                set
                {
                    _header[fromSection] = (int)value;
                }
            }
        }

        private Sptp _sptpInfo;

        public Sptp SptpInfo
        {
            get
            {
                return _sptpInfo;
            }
        }

        private byte[] _data;

        public new byte[] Data
        {
            get
            {
                // возвращаем данные без учета заголовка
                return _data;
                //return SptpInfo.Data.Take(base.DataLen - 4).ToArray();
            }
        }

        protected override ushort NeededCrc
        {
            get
            {
                return Crc16.Get(Data, Data.Length - 2);
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
        public SpacewireSptpMsgEventArgs(byte[] data, byte time1, byte time2, byte error = 0x00)
            : base(data, data.Length)
        {
            if ((4 > data.Length) || (4 > base.DataLen))
            {
                throw new ContextMarshalException(Resource.Get(@"eSmallSpacewireData"));
            }

            //if (Sptp.MaxLengthOfSpacewirePackage < data.Length)
            //{
            //    throw new ContextMarshalException(Resource.Get(@"eBigSpacewireData"));
            //}

            // преобразуем данные к структуре Sptp.

            //GCHandle pinnedInfo = GCHandle.Alloc(data, GCHandleType.Pinned);
            //try
            //{
            //    _sptpInfo = (Sptp)Marshal.PtrToStructure(pinnedInfo.AddrOfPinnedObject(), typeof(Sptp));
            //}
            //catch (Exception)
            //{
            //    _sptpInfo = new Sptp();
            //}
            //finally
            //{
            //    if (pinnedInfo.IsAllocated)
            //    {
            //        pinnedInfo.Free();
            //    }
            //}

            // преобразуем данные к структуре Sptp.
            try
            { 
                _sptpInfo = Converter.MarshalTo<Sptp>(data, out _data);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }

        public static SpacewireSptpMsgEventArgs GetNew(byte[] data, byte to, byte from)
        {
            byte[] buf = new byte[data.Length + 4];
            buf[0] = to;
            buf[1] = 0xf2;
            buf[2] = 0x00;
            buf[3] = from;
            Array.Copy(data, 0, buf, 4, data.Length);
            return new SpacewireSptpMsgEventArgs(buf, 0x00, 0x00);
        }

        /// <summary>
        /// Тип Spacewire SPTP сообщения. 
        /// </summary>
        public enum SptpType
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
        /// <param name="data">The data.</param>
        public SpacewireTimeTickMsgEventArgs(byte data)
        {
            base.Data = new byte[1] { (byte)(data & 0x1F) };
        }

        public byte Tick 
        {
            get
            {
                return base.Data[0];
            }
        }
    }
}
