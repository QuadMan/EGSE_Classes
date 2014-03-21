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
            
    public class ProtocolHsi
    {
        
        
        
        private readonly uint _dataAddr;

        
        
        
        private int _decodeState = 0;

        
        
        
        
        
        private int _errorCount;

        
        
        
        private byte _flag;

        
        
        
        private short _size = -1;

        
        
        
        private byte _hi;

        
        
        
        private byte[] _buf;

        
        
        
        
        public ProtocolHsi(uint dataAddr)
        {
            _dataAddr = dataAddr;
        }

        
        
        
        
        
        public delegate void HsiMsgEventHandler(object sender, HsiMsgEventArgs e);

        
        
        
        public event HsiMsgEventHandler GotHsiMsg;

        
        
        
        
        
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

        
        
        
                        protected virtual void OnHsiMsg(object sender, HsiMsgEventArgs e)
        {
            if (this.GotHsiMsg != null)
            {
                this.GotHsiMsg(sender, e);
            }
        }
    }
    
                public class HsiMsgEventArgs : MsgBase
    {
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct Hsi
        {
            private static readonly BitVector32.Section packStartSection = BitVector32.CreateSection(0xFF);
            private static readonly BitVector32.Section flagSection = BitVector32.CreateSection(0xFF, packStartSection);
            private static readonly BitVector32.Section lineSection = BitVector32.CreateSection(0x1, flagSection);
            private static readonly BitVector32.Section sizeHiSection = BitVector32.CreateSection(0x7F, lineSection);
            private static readonly BitVector32.Section sizeLoSection = BitVector32.CreateSection(0xFF, sizeHiSection);                                                           

            private BitVector32 _header;

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
                
                return _data;
            }
        }

        public new static bool Test(byte[] data)
        {
            return (null != data ? 1 < data.Length : false);
        }

        
        
        
        
        
        public HsiMsgEventArgs(byte[] data, int dataLen)
            : base(data)
        {
            if ((4 > data.Length) || (4 > base.DataLen))
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

        
        
        
        public enum HsiLine
        {
            
            
            
            Main = 0x00,

            
            
            
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
