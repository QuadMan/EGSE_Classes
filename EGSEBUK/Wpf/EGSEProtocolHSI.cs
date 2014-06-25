//-----------------------------------------------------------------------
// <copyright file="EGSEProtocolHsi.cs" company="IKI RSSI, laboratory №711">
//     Copyright (c) IKI RSSI, laboratory №711. All rights reserved.
// </copyright>
// <author>Коробейщиков Иван</author>
//-----------------------------------------------------------------------

namespace Egse.Protocols
{
    using System;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.Runtime.InteropServices;
    using System.Windows;
    using Egse.Utilites;
    using Egse.Defaults;

    /// <summary>
    /// Класс декодера по протоколу ВСИ.
    /// </summary>
    public class ProtocolHsi
    {  
        /// <summary>
        /// Адресный байт, по которому ожидается ВСИ сообщение.
        /// </summary>
        private readonly uint _dataAddr;

        /// <summary>
        /// Состояние декодера.
        /// </summary>
        private int _decodeState = 0;

        /// <summary>
        /// Количество ошибок декодера.
        /// </summary>
        private int _errorCount;

        /// <summary>
        /// Значение поля "ФЛАГ", формируемого ВСИ сообщения.
        /// </summary>
        private byte _flag;

        /// <summary>
        /// Размер(в байтах), формируемого ВСИ сообщения. 
        /// </summary>
        private short _size = -1;

        /// <summary>
        /// Значение поля "HI", формируемого ВСИ сообщения.
        /// </summary>
        private byte _hi;

        /// <summary>
        /// Формируемое ВСИ сообщение.
        /// </summary>
        private byte[] _buf;

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="ProtocolHsi" />.
        /// </summary>
        /// <param name="dataAddr">Адресный байт: сообщения ВСИ.</param>
        public ProtocolHsi(uint dataAddr)
        {
            _dataAddr = dataAddr;
        }

        /// <summary>
        /// Обработка сообщений ВСИ протокола.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="HsiMsgEventArgs"/> instance containing the event data.</param>
        public delegate void HsiMsgEventHandler(object sender, HsiMsgEventArgs e);

        /// <summary>
        /// Вызывается когда [получено сообщений ВСИ].
        /// </summary>
        public event HsiMsgEventHandler GotHsiMsg;

        /// <summary>
        /// Вызывается когда [получено сообщение по USB].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="msg">The <see cref="ProtocolMsgEventArgs"/> instance containing the event data.</param>
        public void OnMessageFunc(object sender, ProtocolMsgEventArgs msg)
        {
            new { msg }.CheckNotNull(); 
            if (_dataAddr != msg.Addr)
            {
                return;
            }

            for (int i = 0; i < msg.DataLen; i++)
            {
                switch (_decodeState)
                {
                    case 0:
                        if (0xA4 != msg.Data[i])
                        {
                            _errorCount++;
                            _decodeState = -1;
                        }

                        break;
                    case 1:
                        {
                            _flag = msg.Data[i];
                        }

                        break;
                    case 2:
                        {
                            _hi = msg.Data[i];
                        }

                        break;
                    case 3:
                        {
                            _size = (short)((short)(_hi << 8) & 0x7FFF | msg.Data[i]);
                            _buf = new byte[_size + 4];
                            _buf[0] = 0xa4;
                            _buf[1] = _flag;
                            _buf[2] = _hi;
                            _buf[3] = msg.Data[i];                            
                        }

                        break;
                    default:
                        {
                            _buf[_buf.Length - _size] = msg.Data[i];
                            _size--;
                        }

                        break;
                }

                _decodeState++;
                if (0 == _size)
                {
                    HsiMsgEventArgs _msg = new HsiMsgEventArgs(_buf, _buf.Length);
                    OnHsiMsg(this, _msg);
                    _decodeState = 0;
                    _size = -1;
                }
            }                  
        }

        /// <summary>
        /// Вызывается когда [сформировано сообщение ВСИ].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="HsiMsgEventArgs"/> instance containing the event data.</param>
        protected virtual void OnHsiMsg(object sender, HsiMsgEventArgs e)
        {
            if (this.GotHsiMsg != null)
            {
                this.GotHsiMsg(sender, e);
            }
        }
    }

    /// <summary>
    /// Обмен сообщениями по протоколу ВСИ.
    /// </summary>
    public class HsiMsgEventArgs : BaseMsgEventArgs
    {
        /// <summary>
        /// Агрегат доступа к заголовку ВСИ сообщения.
        /// </summary>
        private Hsi _info;

        /// <summary>
        /// Данные ВСИ сообщения.
        /// </summary>
        private byte[] _data;

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="HsiMsgEventArgs" />.
        /// </summary>
        /// <param name="data">"сырые" данные ВСИ сообщения.</param>
        /// <param name="dataLen">Длина ВСИ сообщения.</param>
        /// <exception cref="System.ContextMarshalException">Если длины сообщения не достаточно для декодирования.</exception>
        public HsiMsgEventArgs(byte[] data, int dataLen)
            : base(data)
        {
            if ((4 > data.Length) || (4 > DataLen))
            {
                throw new ContextMarshalException(Resource.Get(@"eSmallHsiData"));
            }
            
            try
            {
                _info = Converter.MarshalTo<Hsi>(data, out _data);
            }
            catch (AccessViolationException e)
            {
                MessageBox.Show(e.Message);
            }
        }

        /// <summary>
        /// Линия приема/передачи.
        /// </summary>
        public enum HsiLine
        {
            /// <summary>
            /// Основня линия приема/передачи.
            /// </summary>
            [Description("ОСН")]
            Main = 0x00,

            /// <summary>
            /// Резервная линия приема/передачи.
            /// </summary>
            [Description("РЕЗ")]
            Resv = 0x01
        }
       
        /// <summary>
        /// Тип сообщения ВСИ.
        /// </summary>
        public enum Type
        {
            /// <summary>
            /// Сообщение "КБВ".
            /// </summary>
            Obt = 0x01,

            /// <summary>
            /// Сообщение "УКС".
            /// </summary>
            Cmd = 0x02,

            /// <summary>
            /// Сообщение "запрос статуса".
            /// </summary>
            RequestState = 0x03,

            /// <summary>
            /// Сообщение "запрос данных".
            /// </summary>
            RequestData = 0x04,

            /// <summary>
            /// Сообщение "метка времени".
            /// </summary>
            Time = 0x05
        }

        /// <summary>
        /// Получает агрегат доступа к заголовоку ВСИ сообщения.
        /// </summary>
        /// <value>
        /// Агрегат доступа к заголовку ВСИ сообщения.
        /// </value>
        public Hsi Info
        {
            get
            {
                return _info;
            }
        }

        /// <summary>
        /// Получает данные ВСИ сообщения.
        /// </summary>
        public new byte[] Data
        {
            get
            {
                return _data;
            }
        }

        /// <summary>
        /// "Сырая" проверка на принадлежность к ВСИ сообщениям.
        /// </summary>
        /// <param name="data">"сырые" данные.</param>
        /// <returns><c>true</c> если проверка пройдена успешно, иначе <c>false</c>.</returns>
        public static new bool Test(byte[] data)
        {
            return null != data ? 1 < data.Length : false;
        }

        public override string ToString()
        {
            return string.Format(Resource.Get(@"stHsiMsgToString"), this.Info.Line.Description(), this.Data.Length, Converter.ByteArrayToHexStr(this.Data, isSmart: true));
        }

        /// <summary>
        /// Агрегат доступа к заголовку сообщения ВСИ.
        /// </summary>
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct Hsi
        {
            /// <summary>
            /// Маска поля заголовка: начало сообщения (0xA4).
            /// </summary>
            private static readonly BitVector32.Section PackStartSection = BitVector32.CreateSection(0xFF);

            /// <summary>
            /// Маска поля заголовка: FLAG.
            /// </summary>
            private static readonly BitVector32.Section FlagSection = BitVector32.CreateSection(0xFF, PackStartSection);

            /// <summary>
            /// Маска поля заголовка: HI.
            /// </summary>
            private static readonly BitVector32.Section SizeHiSection = BitVector32.CreateSection(0x7F, FlagSection);

            /// <summary>
            /// Маска поля заголовка: линия приема сообщения (1 - резервная, 0 - основная).
            /// </summary>
            private static readonly BitVector32.Section LineSection = BitVector32.CreateSection(0x1, SizeHiSection);

            /// <summary>
            /// Маска поля заголовка: LO.
            /// </summary>
            private static readonly BitVector32.Section SizeLoSection = BitVector32.CreateSection(0xFF, LineSection);
                                                                        
            /// <summary>
            /// Заголовок ВСИ сообщения.
            /// </summary>
            private BitVector32 _header;

            /// <summary>
            /// Получает или задает флаг ВСИ сообщения.
            /// </summary>
            /// <value>
            /// Значение флага ВСИ сообщения.
            /// </value>
            public Type Flag
            {
                get
                {
                    return (Type)_header[FlagSection];
                }

                set
                {
                    _header[FlagSection] = (int)value;
                }
            }

            /// <summary>
            /// Получает или задает линию приема/передачи ВСИ сообщения.
            /// </summary>
            /// <value>
            /// Линия приема/передачи ВСИ сообщения.
            /// </value>
            public HsiLine Line
            {
                get
                {
                    return (HsiLine)(byte)_header[LineSection];
                }

                set
                {
                    _header[LineSection] = (int)value;
                }
            }

            /// <summary>
            /// Получает или задает размер(в байтах) ВСИ сообщения.
            /// </summary>
            /// <value>
            /// Размер(в байтах) ВСИ сообщения.
            /// </value>
            public int Size
            {
                get
                {
                    return (_header[SizeHiSection] << 8) | _header[SizeLoSection];
                }

                set
                {
                    _header[SizeHiSection] = (value >> 8) & 0x7F;
                    _header[SizeLoSection] = (byte)value;
                }
            }
        }
    }
}
