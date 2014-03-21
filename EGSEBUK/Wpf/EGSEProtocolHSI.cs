//-----------------------------------------------------------------------
// <copyright file="EGSEProtocolHsi.cs" company="IKI RSSI, laboratory №711">
//     Copyright (c) IKI RSSI, laboratory №711. All rights reserved.
// </copyright>
// <author>Коробейщиков Иван</author>
//-----------------------------------------------------------------------

namespace EGSE.Protocols
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using EGSE.Utilites;
    using System.Runtime.InteropServices;
    using System.Collections.Specialized;
    using System.Windows;

    /// <summary>
    /// Декодер протокола ВСИ.
    /// </summary>
    public class ProtocolHsi
    {
        /// <summary>
        /// Адресный байт "ВСИ".
        /// </summary>
        private readonly uint _dataAddr;

        /// <summary>
        /// Текущий шаг декодера.
        /// </summary>
        private int _decodeState = 0;

        /// <summary>
        /// Количество ошибок декодера.
        /// Примечание: 
        /// Считает количество не распознанных байт.
        /// </summary>
        private int _errorCount;

        /// <summary>
        /// Текущий флаг кадра.
        /// </summary>
        private byte _flag;

        /// <summary>
        /// Текущий размер кадра.
        /// </summary>
        private short _size = -1;

        /// <summary>
        /// Текущий байт HI.
        /// </summary>
        private byte _hi;

        /// <summary>
        /// Текущий кадр.
        /// </summary>
        private byte[] _buf;

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="ProtocolHsi" />.
        /// </summary>
        /// <param name="dataAddr">The data addr.</param>
        public ProtocolHsi(uint dataAddr)
        {
            _dataAddr = dataAddr;
        }

        /// <summary>
        /// Обработка сообщений протокола ВСИ.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="HsiMsgEventArgs"/> instance containing the event data.</param>
        public delegate void HsiMsgEventHandler(object sender, HsiMsgEventArgs e);

        /// <summary>
        /// Происходит, когда [сформировано сообщение ВСИ].
        /// </summary>
        public event HsiMsgEventHandler GotHsiMsg;

        /// <summary>
        /// Метод, обрабатывающий сообщения от декодера USB.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="msg">Сообщение для обработки.</param>
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
        /// Обертка события: возникновение сообщения протокола в декодере ВСИ.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">Класс описывающий сообщение протокола.</param>
        protected virtual void OnHsiMsg(object sender, HsiMsgEventArgs e)
        {
            if (this.GotHsiMsg != null)
            {
                this.GotHsiMsg(sender, e);
            }
        }
    }
    
    /// <summary>
    /// Предоставляет аргументы для события, созданного полученным сообщением по протоколу ВСИ.
    /// </summary>
    public class HsiMsgEventArgs : MsgBase
    {
        [StructLayout(LayoutKind.Sequential, Pack = 1/*, Size = 0x10003*/)]
        public struct Hsi
        {
            //internal const int MaxLengthOfHsiPackage = 0xFFFF;
            private static readonly BitVector32.Section packStartSection = BitVector32.CreateSection(0xFF);
            private static readonly BitVector32.Section flagSection = BitVector32.CreateSection(0xFF, packStartSection);
            private static readonly BitVector32.Section lineSection = BitVector32.CreateSection(0x1, flagSection);
            private static readonly BitVector32.Section sizeHiSection = BitVector32.CreateSection(0x7F, lineSection);
            private static readonly BitVector32.Section sizeLoSection = BitVector32.CreateSection(0xFF, sizeHiSection);                                                           

            private BitVector32 _header;

            //[MarshalAs(UnmanagedType.ByValArray, SizeConst = MaxLengthOfHsiPackage)]
            //private byte[] _data;

            //internal byte[] Data
            //{
            //    get
            //    {
            //        return _data;
            //    }
            //}

            public Type Flag
            {
                get
                {
                    return (Type)_header[flagSection];
                }

                set
                {
                    _header[flagSection] = (int)value;
                }
            }

            public HsiLine Line
            {
                get
                {
                    return (HsiLine)_header[lineSection];
                }

                set
                {
                    _header[lineSection] = (int)value;
                }
            }

            public int Size
            {
                get
                {
                    return (_header[sizeHiSection] << 8) | (_header[sizeLoSection]);
                }

                set
                {
                    _header[sizeHiSection] = (value >> 8) & 0x7F;
                    _header[sizeLoSection] = (byte)value;
                }
            }
        }

        private Hsi _info;

        public Hsi Info 
        { 
            get 
            { 
                return _info; 
            } 
        }

        private byte[] _data;

        public new byte[] Data
        {
            get
            {
                // возвращаем данные без учета заголовка
                return _data;
                //return Info.Data.Take(base.DataLen - 4).ToArray();
            }
        }

        public new static bool Test(byte[] data)
        {
            return (null != data ? 1 < data.Length : false);
        }

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="HsiMsgEventArgs" />.
        /// </summary>
        /// <param name="data">Данные кадра.</param>
        /// <param name="dataLen">Длина кадра.</param>
        public HsiMsgEventArgs(byte[] data, int dataLen)
            : base(data)
        {
            if ((4 > data.Length) || (4 > base.DataLen))
            {
                throw new ContextMarshalException(Resource.Get(@"eSmallHsiData"));
            }

            //if (Hsi.MaxLengthOfHsiPackage < data.Length)
            //{
            //    throw new ContextMarshalException(Resource.Get(@"eBigHsiData"));
            //}

            // преобразуем данные к структуре Hsi.

            //GCHandle pinnedInfo = GCHandle.Alloc(data, GCHandleType.Pinned);
            //try
            //{
            //    var structure = Marshal.PtrToStructure(pinnedInfo.AddrOfPinnedObject(), typeof(Hsi));
            //    _info = (Hsi)structure;
            //    IntPtr ptr = new IntPtr(pinnedInfo.AddrOfPinnedObject().ToInt32() + Marshal.SizeOf(typeof(Hsi)));
            //    _data = new byte[dataLen - Marshal.SizeOf(typeof(Hsi))];
            //    Marshal.Copy(ptr, _data, 0, _data.Length);                
            //}
            //catch
            //{
            //    _info = new Hsi();
            //}
            //finally
            //{
            //    if (pinnedInfo.IsAllocated)
            //    {
            //        pinnedInfo.Free();
            //    }
            //}

            // преобразуем данные к структуре Hsi.
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
        /// Линия передачи ВСИ.
        /// </summary>
        public enum HsiLine
        {
            /// <summary>
            /// Основная линия ВСИ.
            /// </summary>
            Main = 0x00,

            /// <summary>
            /// Резервная линия ВСИ.
            /// </summary>
            Resv = 0x01
        }

        public enum Type
        {
            Obt = 0x01,
            Cmd = 0x02,
            RequestState = 0x03,
            RequestData = 0x04,
            Time = 0x05
        }
    }
}
