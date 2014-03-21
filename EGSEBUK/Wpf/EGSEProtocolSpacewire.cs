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
            
    public class ProtocolSpacewire
    {
        
        
        
        private readonly uint _data;

        
        
        
        private readonly uint _eop;

        
        
        
        private readonly uint _time1;

        
        
        
        private readonly uint _time2;

        
        
        
        private List<byte> _buf;

        
        
        
        private byte _currentTime1;

        
        
        
        private byte _currentTime2;

        
        
        
        
        
        
        
        public ProtocolSpacewire(uint data, uint eop, uint time1, uint time2)
        {
            _data = data;
            _eop = eop;
            _time1 = time1;
            _time2 = time2;
            _buf = new List<byte>();
        }

        
        
        
        
        
        public delegate void SpacewireMsgEventHandler(object sender, MsgBase e);

        
        
        
        
        
        public delegate void SpacewireTimeTickMsgEventHandler(object sender, SpacewireTimeTickMsgEventArgs e);

        
        
        
        public event SpacewireMsgEventHandler GotSpacewireMsg;

        
        
        
        public event SpacewireTimeTickMsgEventHandler GotSpacewireTimeTick1Msg;

        
        
        
        public event SpacewireTimeTickMsgEventHandler GotSpacewireTimeTick2Msg;

        
        
        
        
        
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
                        if (SpacewireTkMsgEventArgs.Test(arr))
                        {
                            pack = new SpacewireTkMsgEventArgs(arr, _currentTime1, _currentTime2, msg.Data[0]);
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

        
        
        
        
        
        protected virtual void OnSpacewireMsg(object sender, MsgBase e)
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
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct Icd
        {
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

            public override string ToString()
            {
                return string.Format(Resource.Get(@"stIcdStringExt"), Version, Type, Flag, Apid, Segment, Counter, Size);
            }

            public string ToString(bool extended)
            {
                return extended ? this.ToString() : string.Format(Resource.Get(@"stIcdString"), Version, Type, Flag, Apid, Segment, Counter, Size);
            }
        }

        private Icd _icdInfo;

        public new static bool Test(byte[] data)
        {
            return (null != data ? 9 < data.Length : false);
        }

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
                
                return _data;
            }
        }

        
        
        
        
        
        
        
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

        
        
        
        
        
        public override byte[] ToArray()
        {
            return base.ToArray();
        }
    }

    
    
    
    public class SpacewireObtMsgEventArgs : SpacewireIcdMsgEventArgs
    {
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct Obt
        {          
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 10)]
            private byte[] _nop;

            private byte _normal;

            private byte _extended;

            private uint _obt;

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
        }

        private Obt _obtInfo;

        public Obt ObtInfo
        {
            get
            {
                return _obtInfo;
            }
        }

        
        
        
        
        
        
        
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

        public new static bool Test(byte[] data)
        {
            if (null != data ? 16 == data.Length : false)
            {
                byte[] raw = data.Skip(4).Take(4).ToArray();
                BitVector32 head = new BitVector32(ConvertToInt(raw));
                Icd icdInfo = new Icd();
                icdInfo._header = head;
                return icdInfo.Version == 0
                       && icdInfo.Type == SpacewireIcdMsgEventArgs.IcdType.Tk
                       && icdInfo.Flag == SpacewireIcdMsgEventArgs.IcdFlag.HeaderEmpty;
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
    }

    
    
    
    public class SpacewireTmMsgEventArgs : SpacewireIcdMsgEventArgs
    {
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct Tm
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 10)]
            private byte[] _nop;

            private BitVector32 _header;
           
        }

        private Tm _tmInfo;

        private byte[] _data;

        public Tm TmInfo
        {
            get
            {
                return _tmInfo;
            }
        }

        public new byte[] Data
        {
            get
            {
                return _data.Take(_data.Length - 2).ToArray();
            }
        }
                
     
        public SpacewireTmMsgEventArgs(byte[] data, byte time1, byte time2, byte error = 0x00)
            : base(data, time1, time2, error)
        {
            if (16 > data.Length)
            {
                throw new ContextMarshalException(Resource.Get(@"eSmallSpacewireTmData"));
            }


            try
            {
                _tmInfo = Converter.MarshalTo<Tm>(data, out _data);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }

        public new static bool Test(byte[] data)
        {
            if (null != data ? 9 < data.Length : false)
            {
                byte[] raw = data.Skip(4).Take(4).ToArray();
                BitVector32 head = new BitVector32(ConvertToInt(raw));
                Icd icdInfo = new Icd();
                icdInfo._header = head;
                return icdInfo.Version == 0
                       && icdInfo.Type == SpacewireIcdMsgEventArgs.IcdType.Tm
                       && icdInfo.Flag == SpacewireIcdMsgEventArgs.IcdFlag.HeaderFill;
            }
            else
            {
                return false;
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
            }
        }
        
        
        
        
        
        
        public override byte[] ToArray()
        {
            return base.ToArray();
        }

        public bool CrcCheck()
        {
            return false;
        }
    }

    
    
    
    public class SpacewireTkMsgEventArgs : SpacewireIcdMsgEventArgs
    {
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct Tk
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 10)]
            private byte[] _nop;
            
            private BitVector32 _header;
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
                
                return _data.Take(_data.Length - 2).ToArray();
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
            }
        }

        
        
        
        
        
        
        public SpacewireTkMsgEventArgs(byte[] data, byte time1, byte time2, byte error = 0x00)
            : base(data, time1, time2, error)
        {
            if (16 > data.Length)
            {
                throw new ContextMarshalException(Resource.Get(@"eSmallSpacewireTkData"));
            }

            
            try
            {
                _tkInfo = Converter.MarshalTo<Tk>(data, out _data);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }

        public new static bool Test(byte[] data)
        {
            if (null != data ? 9 < data.Length : false)
            {
                byte[] raw = data.Skip(4).Take(4).ToArray();
                BitVector32 head = new BitVector32(ConvertToInt(raw));
                Icd icdInfo = new Icd();
                icdInfo._header = head;
                return icdInfo.Version == 0
                       && icdInfo.Type == SpacewireIcdMsgEventArgs.IcdType.Tk
                       && icdInfo.Flag == SpacewireIcdMsgEventArgs.IcdFlag.HeaderFill;
            }
            else
            {
                return false;
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

        
        
        
        
        
        
        public override byte[] ToArray()
        {
            return base.ToArray();
        }        
    }

    public class SpacewireErrorMsgEventArgs : MsgBase
    {
        private  string _errorMsg;
        public SpacewireErrorMsgEventArgs(byte[] data, byte time1, byte time2, byte error = 0x00, string msg = null)
            : base(data)
        {
             _errorMsg = (null == msg ? string.Empty : msg);   
        }

        public new byte[] Data
        {
            get
            {
                return base.Data;
            }
        }

        public string ErrorMessage()
        {
            return _errorMsg;
        }

        public new int DataLen
        {
            get
            {
                return base.DataLen;
            }
        }
    }

    
    
    
    public class SpacewireSptpMsgEventArgs : MsgBase
    {
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct Sptp
        {
            private static readonly BitVector32.Section toSection = BitVector32.CreateSection(0xFF);
            private static readonly BitVector32.Section protocolIdSection = BitVector32.CreateSection(0xFF, toSection);
            private static readonly BitVector32.Section msgTypeSection = BitVector32.CreateSection(0xFF, protocolIdSection);
            private static readonly BitVector32.Section fromSection = BitVector32.CreateSection(0xFF, msgTypeSection);

            private BitVector32 _header;

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

            public override string ToString()
            {
                return string.Format(Resource.Get(@"stSptpStringExt"), To, ProtocolId, MsgType, From);
            }

            public string ToString(bool extended)
            {
                return extended ? this.ToString() : string.Format(Resource.Get(@"stSptpString"), To, ProtocolId, MsgType, From);
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
                
                return _data;
            }
        }

        protected override ushort NeededCrc
        {
            get
            {
                return Crc16.Get(Data, Data.Length - 2);
            }
        }

        public new static bool Test(byte[] data)
        {
            return (null != data ? 3 < data.Length : false);
        }

        public SpacewireSptpMsgEventArgs(byte[] data, byte time1, byte time2, byte error = 0x00)
            : base(data)
        {
            if (4 > data.Length)
            {
                throw new ContextMarshalException(Resource.Get(@"eSmallSpacewireData"));
            }

            
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

        
        
        
        public enum SptpType
        {
            
            
            
            Request = 0x80,

            
            
            
            Reply = 0xC0,

            
            
            
            Data = 0x00
        }

        
        
        
        
        
        
        public byte Error { get; set; }

        
        
        
        
        
        
        public byte Time1 { get; set; }

        
        
        
        
        
        
        public byte Time2 { get; set; }

        
        
        
        
        public override byte[] ToArray()
        {
            return base.Data;
        }
    }

    
    
    
    public class SpacewireTimeTickMsgEventArgs : MsgBase
    {
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct TimeTick
        {
            private byte _tick;

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

        private TimeTick _timeTickInfo;

        public TimeTick TimeTickInfo
        {
            get
            {
                return _timeTickInfo;
            }
        }

        
        
        
        
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

        public override byte[] ToArray()
        {
            return base.Data;
        }
    }
}
