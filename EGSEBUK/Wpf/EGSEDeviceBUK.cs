//-----------------------------------------------------------------------
// <copyright file="EGSEDeviceBUK.cs" company="IKI RSSI, laboratory №711">
//     Copyright (c) IKI RSSI, laboratory №711. All rights reserved.
// </copyright>
// <author>Семенов Александр, Коробейщиков Иван</author>
//-----------------------------------------------------------------------

namespace EGSE.Devices
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Windows;
    using EGSE;
    using EGSE.Constants;
    using EGSE.Defaults;
    using EGSE.Protocols;
    using EGSE.Telemetry;
    using EGSE.USB;
    using EGSE.Utilites;

    /// <summary>
    /// Конкретный класс устройства КИА.
    /// Содержит команды и протокол управления прибором по USB.
    /// </summary>
    public class DeviceBUK : Device
    {
        private const int SpaceWire2SPTPBuskAddr = 0x08;

        private const int SpaceWire2SPTPBukAddr = 0x09;

        private const int SpaceWire2SPTPBkpAddr = 0x0a;

        private const int SimSpaceWire1SPTPBuskAddr = 0x17;

        private const int SimSpaceWire1SPTPNP1Addr = 0x18;

        private const int SimSpaceWire1SPTPNP2Addr = 0x19;

        /// <summary>
        /// Адресный байт "Сброс адреса записи времени".
        /// </summary>
        private const int TimeResetAddr = 0x01;

        /// <summary>
        /// Адресный байт "Запись данных времени".
        /// </summary>
        private const int TimeDataAddr = 0x02;

        /// <summary>
        /// Адресный байт "Бит установки времени".
        /// </summary>
        private const int TimeSetAddr = 0x03;

        /// <summary>
        /// Адресный байт "Релейные команды [15:8]".
        /// </summary>
        private const int PowerHiAddr = 0x41;

        /// <summary>
        /// Адресный байт "Релейные команды [7:0]".
        /// </summary>
        private const int PowerLoAddr = 0x40;

        /// <summary>
        /// Адресный байт "Бит выдачи релейных команд".
        /// </summary>
        private const int PowerSetAddr = 0x42;

        /// <summary>
        /// Адресный байт "Управление".
        /// </summary>
        private const int SpaceWire2ControlAddr = 0x04;

        /// <summary>
        /// Адресный байт "Управление обменом с приборами по SPTP".
        /// </summary>
        private const int SpaceWire2ControlSPTPAddr = 0x0B;

        /// <summary>
        /// Адресный байт "Управление".
        /// </summary>
        private const int SimSpaceWire1ControlAddr = 0x12;

        /// <summary>
        /// Адресный байт "Управление обменом с приборами по SPTP".
        /// </summary>
        private const int SimSpaceWire1ControlSPTPAddr = 0x21;

        private const int SimSpaceWire1ControlSPTPNP1SendTimeLoAddr = 0x1a;

        private const int SimSpaceWire1ControlSPTPNP1SendTimeHiAddr = 0x1b;

        private const int SimSpaceWire1ControlSPTPNP2SendTimeLoAddr = 0x1c;

        private const int SimSpaceWire1ControlSPTPNP2SendTimeHiAddr = 0x1d;

        private const int SimSpaceWire1ControlSPTPNP1DataSizeLoAddr = 0x1e;

        private const int SimSpaceWire1ControlSPTPNP1DataSizeHiAddr = 0x1f;

        private const int SimSpaceWire1ControlSPTPNP2DataSizeLoAddr = 0x20;

        private const int SimSpaceWire1ControlSPTPNP2DataSizeHiAddr = 0x21;

        private const int SimSpaceWire1RecordFlushAddr = 0x12;

        private const int SimSpaceWire1RecordDataAddr = 0x14;

        private const int SimSpaceWire1RecordSendAddr = 0x15;

        private const int SpaceWire2RecordFlushAddr = 0x05;

        private const int SpaceWire2RecordDataAddr = 0x06;

        private const int SpaceWire2RecordSendAddr = 0x07;

        /// <summary>
        /// Адресный байт "Управление".
        /// </summary>
        private const int SimSpaceWire4ControlAddr = 0x24;

        private const int SimSpaceWire4RecordFlushAddr = 0x25;

        private const int SimSpaceWire4RecordDataAddr = 0x26;

        /// <summary>
        /// Адресный байт "Запись данных(до 1 Кбайт)".
        /// </summary>
        private const int SimSpaceWire4RecordSendAddr = 0x32;

        /// <summary>
        /// Адресный байт "Выбор имитатора Spacewire".
        /// </summary>
        private const int SimSpaceWireControlAddr = 0x11;

        /// <summary>
        /// Обеспечивает доступ к интерфейсу устройства. 
        /// </summary>
        private readonly DevBUK _intfBUK;

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="DeviceBUK" />.
        /// </summary>
        /// <param name="serial">Уникальный идентификатор USB.</param>
        /// <param name="dec">Экземпляр декодера USB для данного устройства.</param>
        /// <param name="intfBUK">Интерфейс управления данным устройством.</param>
        public DeviceBUK(string serial, ProtocolUSBBase dec, DevBUK intfBUK)
            : base(serial, dec, new USBCfg(10))
        {
            _intfBUK = intfBUK;
        }

        /// <summary>
        /// Отправляет команду включить/выключить питание ПК1 БУСК.
        /// </summary>
        /// <param name="value"><c>1</c> если [включить]; иначе, <c>0</c>.</param>
        public void CmdBUSKPower1(uint value)
        {
            byte buf = 0;
            if (_intfBUK.IsBUSKLineA)
            {
                if (0 == value)
                {
                    buf |= 1 << 2;
                }
                else
                {
                    buf |= 1 << 0;
                }
            }

            if (_intfBUK.IsBUSKLineB)
            {
                if (0 == value)
                {
                    buf |= 1 << 3;
                }
                else
                {
                    buf |= 1 << 1;
                }
            }

            SendToUSB(PowerLoAddr, new byte[1] { buf });
            SendToUSB(PowerSetAddr, new byte[1] { 1 });
        }

        /// <summary>
        /// Отправляет команду включить/выключить питание ПК2 БУСК.
        /// </summary>
        /// <param name="value"><c>1</c> если [включить]; иначе, <c>0</c>.</param>
        public void CmdBUSKPower2(uint value)
        {
            byte buf = 0;
            if (_intfBUK.IsBUSKLineA)
            {
                if (0 == value)
                {
                    buf |= 1 << 6;
                }
                else
                {
                    buf |= 1 << 4;
                }
            }

            if (_intfBUK.IsBUSKLineB)
            {
                if (0 == value)
                {
                    buf |= 1 << 7;
                }
                else
                {
                    buf |= 1 << 5;
                }
            }

            SendToUSB(PowerLoAddr, new byte[1] { buf });
            SendToUSB(PowerSetAddr, new byte[1] { 1 });
        }

        /// <summary>
        /// Отправляет команду включить/выключить питание ПК1 БУНД.
        /// </summary>
        /// <param name="value"><c>1</c> если [включить]; иначе, <c>0</c>.</param>
        public void CmdBUNDPower1(uint value)
        {
            byte buf = 0;
            if (_intfBUK.IsBUNDLineA)
            {
                if (0 == value)
                {
                    buf |= 1 << 1;
                }
                else
                {
                    buf |= 1 << 5;
                }
            }

            if (_intfBUK.IsBUNDLineB)
            {
                if (0 == value)
                {
                    buf |= 1 << 3;
                }
                else
                {
                    buf |= 1 << 7;
                }
            }

            SendToUSB(PowerHiAddr, new byte[1] { buf });
            SendToUSB(PowerSetAddr, new byte[1] { 1 });
        }

        /// <summary>
        /// Отправляет команду включить/выключить питание ПК2 БУНД.
        /// </summary>
        /// <param name="value"><c>1</c> если [включить]; иначе, <c>0</c>.</param>
        public void CmdBUNDPower2(uint value)
        {
            byte buf = 0;
            if (_intfBUK.IsBUNDLineA)
            {
                if (0 == value)
                {
                    buf |= 1 << 0;
                }
                else
                {
                    buf |= 1 << 4;
                }
            }

            if (_intfBUK.IsBUNDLineB)
            {
                if (0 == value)
                {
                    buf |= 1 << 2;
                }
                else
                {
                    buf |= 1 << 6;
                }
            }

            SendToUSB(PowerHiAddr, new byte[1] { buf });
            SendToUSB(PowerSetAddr, new byte[1] { 1 });
        }

        /// <summary>
        /// Команда SpaceWire2: Управление.
        /// </summary>
        /// <param name="value">Параметры управления.</param>
        public void CmdSimRouterControl(uint value)
        {            
            SendToUSB(SpaceWire2ControlAddr, new byte[1] { (byte)value });
        }

        /// <summary>
        /// Команда SpaceWire2: Управление SPTP.
        /// </summary>
        /// <param name="value">Параметры управления SPTP.</param>
        public void CmdSimRouterControlSPTP(uint value)
        {
            SendToUSB(SpaceWire2ControlSPTPAddr, new byte[1] { (byte)value }); 
        }

        public void CmdSimSpacewire1Control(uint value)
        {
            if (_intfBUK.IsSpaceWire4IntfOn)
            {
                SendToUSB(SimSpaceWire4ControlAddr, new byte[1] { 0 });
            }
            SendToUSB(SimSpaceWireControlAddr, new byte[1] { 0 });
            SendToUSB(SimSpaceWire1ControlAddr, new byte[1] { (byte)value });            
        }

        public void CmdSimSpacewire1ControlSPTP(uint value)
        {
            SendToUSB(SimSpaceWire1ControlSPTPAddr, new byte[1] { (byte)value });
        }

        public void CmdSimSpacewire1ControlSPTPNP1SendTime(uint value)
        {
            SendToUSB(SimSpaceWire1ControlSPTPNP1SendTimeLoAddr, new byte[1] { (byte)value });
            SendToUSB(SimSpaceWire1ControlSPTPNP1SendTimeHiAddr, new byte[1] { (byte)(value >> 8) });
        }

        public void CmdSimSpacewire1ControlSPTPNP2SendTime(uint value)
        {
            SendToUSB(SimSpaceWire1ControlSPTPNP2SendTimeLoAddr, new byte[1] { (byte)value });
            SendToUSB(SimSpaceWire1ControlSPTPNP2SendTimeHiAddr, new byte[1] { (byte)(value >> 8) });
        }

        public void CmdSimSpacewire1ControlSPTPNP1DataSize(uint value)
        {
            SendToUSB(SimSpaceWire1ControlSPTPNP1DataSizeLoAddr, new byte[1] { (byte)value });
            SendToUSB(SimSpaceWire1ControlSPTPNP1DataSizeHiAddr, new byte[1] { (byte)(value >> 8) });
        }

        public void CmdSimSpacewire1ControlSPTPNP2DataSize(uint value)
        {
            SendToUSB(SimSpaceWire1ControlSPTPNP2DataSizeLoAddr, new byte[1] { (byte)value });
            SendToUSB(SimSpaceWire1ControlSPTPNP2DataSizeHiAddr, new byte[1] { (byte)(value >> 8) });
        }

        public void CmdSimSpacewire1Record(uint value)
        {                       
            if ((null != _intfBUK.Spacewire1Data) && (0 < _intfBUK.Spacewire1Data.Length))
            {
                SendToUSB(SimSpaceWire1RecordFlushAddr, new byte[1] { 1 });
                SendToUSB(SimSpaceWire1RecordDataAddr, _intfBUK.Spacewire1Data);
                SendToUSB(SimSpaceWire1RecordSendAddr, new byte[1] { (byte)value });
            }            
        }

        public void CmdSpacewire2Record(uint value)
        {          
            if ((null != _intfBUK.Spacewire2Data) && (0 < _intfBUK.Spacewire2Data.Length))
            {
                SendToUSB(SpaceWire2RecordFlushAddr, new byte[1] { 1 });
                SendToUSB(SpaceWire2RecordDataAddr, _intfBUK.Spacewire2Data);
                SendToUSB(SpaceWire2RecordSendAddr, new byte[1] { (byte)value });
            }            
        }

        public void CmdSimSpacewire4Control(uint value)
        {
            if (_intfBUK.IsSpaceWire1IntfOn)
            {
                SendToUSB(SimSpaceWire1ControlAddr, new byte[1] { 0 });
            }
            SendToUSB(SimSpaceWireControlAddr, new byte[1] { 1 });
            SendToUSB(SimSpaceWire4ControlAddr, new byte[1] { (byte)value });
        }

        public void CmdSimSpacewire4Record(uint value)
        {
            if ((null != _intfBUK.Spacewire4Data) && (0 < _intfBUK.Spacewire4Data.Length))
            {
                SendToUSB(SimSpaceWire4RecordFlushAddr, new byte[1] { 1 });
                SendToUSB(SimSpaceWire4RecordDataAddr, _intfBUK.Spacewire4Data);
                SendToUSB(SimSpaceWire4RecordSendAddr, new byte[1] { (byte)value });
            }            
        }

        /// <summary>
        /// Команда установки внутреннего времени устройства.
        /// </summary>
        public void CmdSetDeviceTime()
        {
            EgseTime time = new EgseTime();
            time.Encode();
            byte[] buf = new byte[1] { 1 };
            SendToUSB(TimeResetAddr, buf);
            SendToUSB(TimeDataAddr, time.Data);
            SendToUSB(TimeSetAddr, buf);
        }

        public void CmdSpaceWire2LogicAddrBuk(uint value)
        {
            SendToUSB(SpaceWire2SPTPBukAddr, new byte[1] { (byte)value });
        }

        public void CmdSpaceWire2LogicAddrBusk(uint value)
        {
            SendToUSB(SpaceWire2SPTPBuskAddr, new byte[1] { (byte)value });
        }

        public void CmdSimSpaceWire2LogicAddrBuk(uint value)
        {
            SendToUSB(SimSpaceWire1SPTPNP1Addr, new byte[1] { (byte)value });
        }

        public void CmdSimSpaceWire2LogicAddrBusk(uint value)
        {
            SendToUSB(SimSpaceWire1SPTPBuskAddr, new byte[1] { (byte)value });
        }

        public void CmdSetDeviceLogicAddr()
        {
            if (_intfBUK.IsBUK1BM1Channel || _intfBUK.IsBUK1BM2Channel)
            {
                _intfBUK.SpaceWire2LogicAddrBuk = Global.LogicAddrBuk1;
                _intfBUK.SimSpaceWire1AddrNP1 = Global.LogicAddrBuk1;  
            }
            else            
            {
                _intfBUK.SpaceWire2LogicAddrBuk = Global.LogicAddrBuk2;
                _intfBUK.SimSpaceWire1AddrNP1 = Global.LogicAddrBuk2;  
            }
            if (_intfBUK.IsBUK1BM1Channel || _intfBUK.IsBUK2BM1Channel)
            {
                _intfBUK.SpaceWire2AddrBusk = Global.LogicAddrBusk1;
                _intfBUK.SimSpaceWire1AddrBusk = Global.LogicAddrBusk1;  
            }
            else
            {
                _intfBUK.SpaceWire2AddrBusk = Global.LogicAddrBusk2;
                _intfBUK.SimSpaceWire1AddrBusk = Global.LogicAddrBusk2;  
            }
        }
    }

    /// <summary>
    /// Общий экземпляр, позволяющий управлять прибором (принимать данные, выдавать команды).
    /// </summary>
    public class DevBUK : INotifyPropertyChanged, IDataErrorInfo
    {
        /// <summary>
        /// Адресный байт "Статус".
        /// </summary>
        private const int TimeDataAddr = 0x01;

        /// <summary>
        /// Адресный байт "Телеметрия".
        /// </summary>
        private const int TeleDataAddr = 0x15;
        
        /// <summary>
        /// Адресный байт SpaceWire2 "Управление".
        /// </summary>
        private const int SpaceWire2Ctrl = 0x04;

        /// <summary>
        /// Экземпляр декодера протокола USB.
        /// </summary>
        private ProtocolUSB7C6E _decoder;

        /// <summary>
        /// Экземпляр декодера протокола Spacewire.
        /// </summary>
        private ProtocolSpacewire _spacewire2Busk;

        /// <summary>
        /// Экземпляр декодера протокола Spacewire.
        /// </summary>
        private ProtocolSpacewire _spacewire2Buk;

        /// <summary>
        /// Текущее состояние подключения устройства.
        /// </summary>
        private bool _connected;

        /// <summary>
        /// Телеметрия: Запитан ПК1 от БУСК.
        /// </summary>
        private bool _buskPower1;

        /// <summary>
        /// Телеметрия: Запитан ПК2 от БУСК.
        /// </summary>
        private bool _buskPower2;

        /// <summary>
        /// Телеметрия: Запитан ПК1 от БУНД.
        /// </summary>
        private bool _bundPower1;

        /// <summary>
        /// Телеметрия: Запитан ПК2 от БУНД.
        /// </summary>
        private bool _bundPower2;

        /// <summary>
        /// Передавать релейные команды БУСК по линии A.
        /// </summary>
        private bool _isBUSKLineA = true;

        /// <summary>
        /// Передавать релейные команды БУСК по линии B.
        /// </summary>
        private bool _isBUSKLineB = true;

        /// <summary>
        /// Передавать релейные команды БУНД по линии A.
        /// </summary>
        private bool _isBUNDLineA = true;

        /// <summary>
        /// Передавать релейные команды БУНД по линии B.
        /// </summary>
        private bool _isBUNDLineB = true;

        /// <summary>
        /// SPACEWIRE 2: Управление: вкл/выкл интерфейса Spacewire.
        /// </summary>
        private bool _isSpaceWire2IntfOn;

        /// <summary>
        /// SPACEWIRE 2: Управление: Выбор канала.
        /// </summary>
        private SimRouterChannel _simRouterChannel;

        private uint _spaceWire2AddrBusk;
        private uint _spaceWire2AddrBuk;
        private uint _spaceWire2AddrBkp;
        private uint _simSpaceWire1AddrBusk;
        private uint _simSpaceWire1AddrNP1;
        private uint _simSpaceWire1AddrNP2;

        /// <summary>
        /// SPACEWIRE 2: Управление: Установлена связь.
        /// </summary>
        private bool _isSpaceWire2Connected;

        private bool _isSpaceWire2RecordSendRMAP;
        private bool _isSpaceWire2RecordSendBuk;
        private bool _isSpaceWire2RecordSendBkp;

        /// <summary>
        /// SPACEWIRE 2: Управление обменом с приборами по SPTP: включить выдачу секундных меток (1PPS).
        /// </summary>
        private bool _isSpaceWire2TimeMark;

        /// <summary>
        /// SPACEWIRE 2: Управление обменом с приборами по SPTP: включение обмена прибора БС.
        /// </summary>
        private bool _isSpaceWire2BukTrans;

        /// <summary>
        /// SPACEWIRE 2: Управление обменом с приборами по SPTP: включение обмена прибора БКП.
        /// </summary>
        private bool _isSpaceWire2BkpTrans;

        /// <summary>
        /// SPACEWIRE 2: Управление обменом с приборами по SPTP: можно выдавать пакет в БС.
        /// </summary>
        private bool _isSpaceWire2BukTransData;

        /// <summary>
        /// SPACEWIRE 2: Управление обменом с приборами по SPTP: можно выдавать пакет в БКП.
        /// </summary>
        private bool _isSpaceWire2BkpTransData;

        /// <summary>
        /// SPACEWIRE 2: Управление обменом с приборами по SPTP: выдача КБВ прибору БС (только при «1 PPS» == 1).
        /// </summary>
        private bool _isSpaceWire2BukKbv;

        /// <summary>
        /// SPACEWIRE 2: Управление обменом с приборами по SPTP: выдача КБВ прибору БКП (только при «1 PPS» == 1).
        /// </summary>
        private bool _isSpaceWire2BkpKbv;

        /// <summary>
        /// SPACEWIRE 1: Управление: вкл/выкл интерфейса Spacewire.
        /// </summary>
        private bool _isSpaceWire1IntfOn;

        /// <summary>
        /// SPACEWIRE 1: Управление: Установлена связь.
        /// </summary>
        private bool _isSpaceWire1Connected;

        /// <summary>
        /// SPACEWIRE 1: Управление обменом с приборами по SPTP: включение обмена прибора НП1.
        /// </summary>
        private bool _isSpaceWire1NP1Trans;

        /// <summary>
        /// SPACEWIRE 1: Управление обменом с приборами по SPTP: включение обмена прибора НП2.
        /// </summary>
        private bool _isSpaceWire1NP2Trans;

        /// <summary>
        /// SPACEWIRE 1: Управление обменом с приборами по SPTP: можно выдавать пакет в НП1.
        /// </summary>
        private bool _isSpaceWire1NP1TransData;

        /// <summary>
        /// SPACEWIRE 1: Управление обменом с приборами по SPTP: можно выдавать пакет в НП2.
        /// </summary>
        private bool _isSpaceWire1NP2TransData;

        private uint _spaceWire1NP1SendTime;

        private uint _spaceWire1NP2SendTime;

        private uint _spaceWire1NP1DataSize;

        private uint _spaceWire1NP2DataSize;

        private bool _isSpaceWire1RecordBusy;

        private bool _isSpaceWire1RecordSend;

        /// <summary>
        /// SPACEWIRE 4: Управление: вкл/выкл интерфейса Spacewire.
        /// </summary>
        private bool _isSpaceWire4IntfOn;

        /// <summary>
        /// SPACEWIRE 4: Управление: Установлена связь.
        /// </summary>
        private bool _isSpaceWire4Connected;

        private bool _isSpaceWire4TimeMark;

        private bool _isSpaceWire4EEPSend;

        private bool _isSpaceWire4EOPSend;

        private bool _isSpaceWire4AutoSend;

        private bool _isSpaceWire4RecordBusy;

        private bool _isSpaceWire4RecordSend;

        /// <summary>
        /// Записывать данные от прибора в файл.
        /// </summary>
        private bool _isWriteDevDataToFile;

        /// <summary>
        /// Экземпляр класса, представляющий файл для записи данных от прибора.
        /// </summary>
        private FileStream _devDataLogStream;

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="DevBUK" />.
        /// </summary>
        public DevBUK()
        {
            Connected = false;
            ControlValuesList = new List<ControlValue>();
            ControlValuesList.Add(new ControlValue()); // PowerControl = 0
            ControlValuesList.Add(new ControlValue()); // SpaceWire2Control = 1
            ControlValuesList.Add(new ControlValue()); // SpaceWire2ControlSPTP = 2
            ControlValuesList.Add(new ControlValue()); // SpaceWire1Control = 3
            ControlValuesList.Add(new ControlValue()); // SpaceWire4Control = 4
            ControlValuesList.Add(new ControlValue()); // SpaceWire1ControlSPTP = 5
            ControlValuesList.Add(new ControlValue()); // SpaceWire4Record = 6
            ControlValuesList.Add(new ControlValue()); // SpaceWire1Record = 7
            ControlValuesList.Add(new ControlValue()); // SpaceWire1ControlSPTPNP1SendTime = 8
            ControlValuesList.Add(new ControlValue()); // SpaceWire1ControlSPTPNP2SendTime = 9
            ControlValuesList.Add(new ControlValue()); // SpaceWire1ControlSPTPNP1DataSize = 10
            ControlValuesList.Add(new ControlValue()); // SpaceWire1ControlSPTPNP2DataSize = 11
            ControlValuesList.Add(new ControlValue()); // SpaceWire2SPTPLogicBusk = 12
            ControlValuesList.Add(new ControlValue()); // SpaceWire2SPTPLogicBuk = 13
            ControlValuesList.Add(new ControlValue()); // SpaceWire2SPTPLogicBkp = 14
            ControlValuesList.Add(new ControlValue()); // SpaceWire1SPTPSimLogicBusk = 15
            ControlValuesList.Add(new ControlValue()); // SpaceWire1SPTPSimLogicNP1 = 16
            ControlValuesList.Add(new ControlValue()); // SpaceWire1SPTPSimLogicNP2 = 17
            ControlValuesList.Add(new ControlValue()); // SpaceWire2Record = 18
            
            _decoder = new ProtocolUSB7C6E(null, LogsClass.LogUSB, false, true);
            _decoder.GotProtocolMsg += new ProtocolUSBBase.ProtocolMsgEventHandler(OnMessageFunc);
            _decoder.GotProtocolError += new ProtocolUSBBase.ProtocolErrorEventHandler(OnErrorFunc);
            DeviceTime = new EgseTime();

            _spacewire2Busk = new ProtocolSpacewire((uint)Spacewire2Addr.Data, (uint)Spacewire2Addr.End, (uint)Spacewire2Addr.Time1, (uint)Spacewire2Addr.Time2);
            _spacewire2Busk.GotSpacewireMsg += new ProtocolSpacewire.SpacewireMsgEventHandler(OnSpacewire2Msg);
            _decoder.GotProtocolMsg += new ProtocolUSBBase.ProtocolMsgEventHandler(_spacewire2Busk.OnMessageFunc);

            _spacewire2Buk = new ProtocolSpacewire((uint)Spacewire2Addr.BukData, (uint)Spacewire2Addr.BukEnd, (uint)Spacewire2Addr.BukTime1, (uint)Spacewire2Addr.BukTime2);
            _spacewire2Buk.GotSpacewireMsg += new ProtocolSpacewire.SpacewireMsgEventHandler(OnSpacewire2Msg);
            _decoder.GotProtocolMsg += new ProtocolUSBBase.ProtocolMsgEventHandler(_spacewire2Buk.OnMessageFunc);  

            Device = new DeviceBUK(Global.DeviceSerial, _decoder, this);
            Device.ChangeStateEvent = OnChangeConnection;
            _devDataLogStream = null;
            _isWriteDevDataToFile = false;

            Tele = new TelemetryBUK();

            ControlValuesList[Global.PowerControl].AddProperty(Global.PropertyTelePowerBUSK1, 7, 1, Device.CmdBUSKPower1, delegate(uint value) { BUSKPower1 = 1 == value; });
            ControlValuesList[Global.PowerControl].AddProperty(Global.PropertyTelePowerBUSK2, 6, 1, Device.CmdBUSKPower2, delegate(uint value) { BUSKPower2 = 1 == value; });
            ControlValuesList[Global.PowerControl].AddProperty(Global.PropertyTelePowerBUND1, 4, 1, Device.CmdBUNDPower1, delegate(uint value) { BUNDPower1 = 1 == value; });
            ControlValuesList[Global.PowerControl].AddProperty(Global.PropertyBUNDPower2, 5, 1, Device.CmdBUNDPower2, delegate(uint value) { BUNDPower2 = 1 == value; });

            ControlValuesList[Global.SpaceWire2Control].AddProperty(Global.PropertySpaceWire2Channel, 1, 2, Device.CmdSimRouterControl, delegate(uint value) { SelectSimRouterChannel = (SimRouterChannel)value; });
            ControlValuesList[Global.SpaceWire2Control].AddProperty(Global.PropertySpaceWire2IntfOn, 0, 1, Device.CmdSimRouterControl, delegate(uint value) { IsSpaceWire2IntfOn = 1 == value; });
            ControlValuesList[Global.SpaceWire2Control].AddProperty(Global.PropertySpaceWire2Connected, 3, 1, delegate(uint value) { }, delegate(uint value) { IsSpaceWire2Connected = 1 == value; });
            ControlValuesList[Global.SpaceWire2Record].AddProperty(Global.PropertySpaceWire2RecordSendRMAP, 0, 1, Device.CmdSpacewire2Record, delegate(uint value) { IsSpaceWire2RecordSendRMAP = 1 == value; });
            ControlValuesList[Global.SpaceWire2Record].AddProperty(Global.PropertySpaceWire2RecordSendBuk, 1, 1, Device.CmdSpacewire2Record, delegate(uint value) { IsSpaceWire2RecordSendBuk = 1 == value; });
            ControlValuesList[Global.SpaceWire2Record].AddProperty(Global.PropertySpaceWire2RecordSendBkp, 2, 1, Device.CmdSpacewire2Record, delegate(uint value) { IsSpaceWire2RecordSendBkp = 1 == value; });

            ControlValuesList[Global.SpaceWire2SPTPLogicBusk].AddProperty(Global.PropertySpaceWire2LogicBusk, 0, 8, Device.CmdSpaceWire2LogicAddrBusk, delegate(uint value) { SpaceWire2AddrBusk = value; });
            ControlValuesList[Global.SpaceWire2SPTPLogicBuk].AddProperty(Global.PropertySpaceWire2LogicBuk, 0, 8, Device.CmdSpaceWire2LogicAddrBuk, delegate(uint value) { SpaceWire2LogicAddrBuk = value; });
            ControlValuesList[Global.SpaceWire2SPTPLogicBkp].AddProperty(Global.PropertySpaceWire2LogicBkp, 0, 8, delegate(uint value) { }, delegate(uint value) { SpaceWire2AddrBkp = value; });

            ControlValuesList[Global.SpaceWire2ControlSPTP].AddProperty(Global.PropertySpaceWire2TimeMark, 0, 1, Device.CmdSimRouterControlSPTP, delegate(uint value) { IsSpaceWire2TimeMark = 1 == value; });
            ControlValuesList[Global.SpaceWire2ControlSPTP].AddProperty(Global.PropertySpaceWire2BukTrans, 1, 1, Device.CmdSimRouterControlSPTP, delegate(uint value) { IsSpaceWire2BukTrans = 1 == value; });
            ControlValuesList[Global.SpaceWire2ControlSPTP].AddProperty(Global.PropertySpaceWire2BkpTrans, 4, 1, Device.CmdSimRouterControlSPTP, delegate(uint value) { IsSpaceWire2BkpTrans = 1 == value; });
            ControlValuesList[Global.SpaceWire2ControlSPTP].AddProperty(Global.PropertySpaceWire2BukKbv, 2, 1, Device.CmdSimRouterControlSPTP, delegate(uint value) { IsSpaceWire2BukKbv = 1 == value; });
            ControlValuesList[Global.SpaceWire2ControlSPTP].AddProperty(Global.PropertySpaceWire2BkpKbv, 5, 1, Device.CmdSimRouterControlSPTP, delegate(uint value) { IsSpaceWire2BkpKbv = 1 == value; });
            ControlValuesList[Global.SpaceWire2ControlSPTP].AddProperty(Global.PropertySpaceWire2BukTransData, 3, 1, Device.CmdSimRouterControlSPTP, delegate(uint value) { IsSpaceWire2BukTransData = 1 == value; });
            ControlValuesList[Global.SpaceWire2ControlSPTP].AddProperty(Global.PropertySpaceWire2BkpTransData, 6, 1, Device.CmdSimRouterControlSPTP, delegate(uint value) { IsSpaceWire2BkpTransData = 1 == value; });

            ControlValuesList[Global.SpaceWire1Control].AddProperty(Global.PropertySpaceWire1IntfOn, 0, 1, Device.CmdSimSpacewire1Control, delegate(uint value) { IsSpaceWire1IntfOn = 1 == value; });
            ControlValuesList[Global.SpaceWire1Control].AddProperty(Global.PropertySpaceWire1Connected, 3, 1, delegate(uint value) { }, delegate(uint value) { IsSpaceWire1Connected = 1 == value; });

            ControlValuesList[Global.SpaceWire1SPTPSimLogicBusk].AddProperty(Global.PropertySimSpaceWire1LogicBusk, 0, 8, Device.CmdSimSpaceWire2LogicAddrBusk, delegate(uint value) { SimSpaceWire1AddrBusk = value; });
            ControlValuesList[Global.SpaceWire1SPTPSimLogicNP1].AddProperty(Global.PropertySimSpaceWire1LogicNP1, 0, 8, Device.CmdSimSpaceWire2LogicAddrBuk, delegate(uint value) { SimSpaceWire1AddrNP1 = value; });
            ControlValuesList[Global.SpaceWire1SPTPSimLogicNP2].AddProperty(Global.PropertySimSpaceWire1LogicNP2, 0, 8, delegate(uint value) { }, delegate(uint value) { SimSpaceWire1AddrNP2 = value; });
            
            ControlValuesList[Global.SpaceWire1ControlSPTP].AddProperty(Global.PropertySpaceWire1NP1Trans, 0, 1, Device.CmdSimSpacewire1ControlSPTP, delegate(uint value) { IsSpaceWire1NP1Trans = 1 == value; });
            ControlValuesList[Global.SpaceWire1ControlSPTP].AddProperty(Global.PropertySpaceWire1NP2Trans, 2, 1, Device.CmdSimSpacewire1ControlSPTP, delegate(uint value) { IsSpaceWire1NP2Trans = 1 == value; });
            ControlValuesList[Global.SpaceWire1ControlSPTP].AddProperty(Global.PropertySpaceWire1NP1TransData, 1, 1, Device.CmdSimSpacewire1ControlSPTP, delegate(uint value) { IsSpaceWire1NP1TransData = 1 == value; });
            ControlValuesList[Global.SpaceWire1ControlSPTP].AddProperty(Global.PropertySpaceWire1NP2TransData, 3, 1, Device.CmdSimSpacewire1ControlSPTP, delegate(uint value) { IsSpaceWire1NP2TransData = 1 == value; });
            ControlValuesList[Global.SpaceWire1ControlSPTPNP1SendTime].AddProperty(Global.PropertySpaceWire1NP1SendTime, 0, 16, Device.CmdSimSpacewire1ControlSPTPNP1SendTime, delegate(uint value) { SpaceWire1NP1SendTime = value; });
            ControlValuesList[Global.SpaceWire1ControlSPTPNP2SendTime].AddProperty(Global.PropertySpaceWire1NP2SendTime, 0, 16, Device.CmdSimSpacewire1ControlSPTPNP2SendTime, delegate(uint value) { SpaceWire1NP2SendTime = value; });
            ControlValuesList[Global.SpaceWire1ControlSPTPNP1DataSize].AddProperty(Global.PropertySpaceWire1NP1DataSize, 0, 16, Device.CmdSimSpacewire1ControlSPTPNP1DataSize, delegate(uint value) { SpaceWire1NP1DataSize = value; });
            ControlValuesList[Global.SpaceWire1ControlSPTPNP2DataSize].AddProperty(Global.PropertySpaceWire1NP2DataSize, 0, 16, Device.CmdSimSpacewire1ControlSPTPNP2DataSize, delegate(uint value) { SpaceWire1NP2DataSize = value; });
            ControlValuesList[Global.SpaceWire1Record].AddProperty(Global.PropertySpaceWire1RecordBusy, 3, 1, delegate(uint value) { }, delegate(uint value) { IsSpaceWire1RecordBusy = 1 == value; });
            ControlValuesList[Global.SpaceWire1Record].AddProperty(Global.PropertySpaceWire1RecordSend, 0, 1, Device.CmdSimSpacewire1Record, delegate(uint value) { IsSpaceWire1RecordSend = 1 == value; });

            ControlValuesList[Global.SpaceWire4Control].AddProperty(Global.PropertySpaceWire4IntfOn, 0, 1, Device.CmdSimSpacewire4Control, delegate(uint value) { IsSpaceWire4IntfOn = 1 == value; });
            ControlValuesList[Global.SpaceWire4Control].AddProperty(Global.PropertySpaceWire4Connected, 3, 1, delegate(uint value) { }, delegate(uint value) { IsSpaceWire4Connected = 1 == value; });
            ControlValuesList[Global.SpaceWire4Control].AddProperty(Global.PropertySpaceWire4TimeMark, 4, 1, Device.CmdSimSpacewire4Control, delegate(uint value) { IsSpaceWire4TimeMark = 1 == value; });
            ControlValuesList[Global.SpaceWire4Record].AddProperty(Global.PropertySpaceWire4EOPSend, 1, 1, Device.CmdSimSpacewire4Record, delegate(uint value) { IsSpaceWire4EOPSend = 1 == value; });
            ControlValuesList[Global.SpaceWire4Record].AddProperty(Global.PropertySpaceWire4AutoSend, 4, 1, Device.CmdSimSpacewire4Record, delegate(uint value) { IsSpaceWire4AutoSend = 1 == value; });
            ControlValuesList[Global.SpaceWire4Record].AddProperty(Global.PropertySpaceWire4EEPSend, 2, 1, Device.CmdSimSpacewire4Record, delegate(uint value) { IsSpaceWire4EEPSend = 1 == value; });
            ControlValuesList[Global.SpaceWire4Record].AddProperty(Global.PropertySpaceWire4RecordBusy, 3, 1, delegate(uint value) { }, delegate(uint value) { IsSpaceWire4RecordBusy = 1 == value; });
            ControlValuesList[Global.SpaceWire4Record].AddProperty(Global.PropertySpaceWire4RecordSend, 0, 1, Device.CmdSimSpacewire4Record, delegate(uint value) { IsSpaceWire4RecordSend = 1 == value; });
        }

        /// <summary>
        /// Вызывается, когда [меняется значение свойства].
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Вызывается, когда [получено сообщение по spacewire 2].
        /// </summary>
        public event ProtocolSpacewire.SpacewireMsgEventHandler GotSpacewire2Msg;

        /// <summary>
        /// Список возможных каналов имитатора БМ-4.
        /// </summary>
        public enum SimRouterChannel
        {
            /// <summary>
            /// Канал "БУК ПК1 - БМ-4 ПК1".
            /// </summary>
            BUK1BM1 = 0x00,

            /// <summary>
            /// Канал "БУК ПК1 - БМ-4 ПК2".
            /// </summary>
            BUK1BM2 = 0x01,

            /// <summary>
            /// Канал "БУК ПК2 - БМ-4 ПК1".
            /// </summary>
            BUK2BM1 = 0x02,

            /// <summary>
            /// Канал "БУК ПК2 - БМ-4 ПК2".
            /// </summary>
            BUK2BM2 = 0x03
        }

        /// <summary>
        /// Описывает адресные байты Spacewire2.
        /// </summary>
        public enum Spacewire2Addr : uint
        {
            /// <summary>
            /// Адресный байт "SPACEWIRE 2: ВЫХОДНЫЕ ДАННЫЕ (ИМИТАТОР БУСК): Данные Spacewire".
            /// </summary>
            Data = 0x04,

            /// <summary>
            /// Адресный байт "SPACEWIRE 2: ВЫХОДНЫЕ ДАННЫЕ (ИМИТАТОР БУСК): EOP или EEP".
            /// </summary>
            End = 0x05,

            /// <summary>
            /// Адресный байт "SPACEWIRE 2: ВЫХОДНЫЕ ДАННЫЕ (ИМИТАТОР БУСК): TIME TICK".
            /// </summary>
            Time1 = 0x06,

            /// <summary>
            /// Адресный байт "SPACEWIRE 2: ВЫХОДНЫЕ ДАННЫЕ (ИМИТАТОР БУСК): TIME TICK".
            /// </summary>
            Time2 = 0x07,

            /// <summary>
            /// Адресный байт "SPACEWIRE 2: ВХОДНЫЕ ДАННЫЕ (РЕАЛЬНЫЙ БУК): Данные Spacewire".
            /// </summary>
            BukData = 0x08,

            /// <summary>
            /// Адресный байт "SPACEWIRE 2: ВХОДНЫЕ ДАННЫЕ (РЕАЛЬНЫЙ БУК): EOP или EEP".
            /// </summary>
            BukEnd = 0x09,

            /// <summary>
            /// Адресный байт "SPACEWIRE 2: ВХОДНЫЕ ДАННЫЕ (РЕАЛЬНЫЙ БУК): TIME TICK".
            /// </summary>
            BukTime1 = 0x0a,

            /// <summary>
            /// Адресный байт "SPACEWIRE 2: ВХОДНЫЕ ДАННЫЕ (РЕАЛЬНЫЙ БУК): TIME TICK".
            /// </summary>
            BukTime2 = 0x0b
        }

        /// <summary>
        /// Получает или задает доступ к USB прибора.
        /// </summary>
        public DeviceBUK Device { get; set; }

        /// <summary>
        /// Получает значение, показывающее, [подключен] ли прибор.
        /// </summary>
        /// <value>
        ///   <c>true</c> если [подключен]; иначе, <c>false</c>.
        /// </value>
        public bool Connected
        {
            get
            {
                return _connected;
            }

            private set
            {
                _connected = value;
                FirePropertyChangedEvent("Connected");
            }
        }

        /// <summary>
        /// Получает значение, показывающее, есть ли [питание первого полукомплекта БУСК].
        /// </summary>
        /// <value>
        ///   <c>true</c> если [питание первого полукомплекта БУСК]; иначе, <c>false</c>.
        /// </value>
        public bool BUSKPower1
        {
            get
            {
                return _buskPower1;
            }

            private set
            {
                _buskPower1 = value;
                FirePropertyChangedEvent("BUSKPower1");
            }
        }

        /// <summary>
        /// Получает значение, показывающее, есть ли [питание второго полукомплекта БУСК].
        /// </summary>
        /// <value>
        ///   <c>true</c> если [питание второго полукомплекта БУСК]; иначе, <c>false</c>.
        /// </value>
        public bool BUSKPower2
        {
            get
            {
                return _buskPower2;
            }

            private set
            {
                _buskPower2 = value;
                FirePropertyChangedEvent("BUSKPower2");
            }
        }

        /// <summary>
        /// Получает значение, показывающее, есть ли [питание первого полукомплекта БУНД].
        /// </summary>
        /// <value>
        ///   <c>true</c> если [питание первого полукомплекта БУНД]; иначе, <c>false</c>.
        /// </value>
        public bool BUNDPower1
        {
            get
            {
                return _bundPower1;
            }

            private set
            {
                _bundPower1 = value;
                FirePropertyChangedEvent("BUSKPower1");
            }
        }

        /// <summary>
        /// Получает значение, показывающее, есть ли [питание второго полукомплекта БУНД].
        /// </summary>
        /// <value>
        ///   <c>true</c> если [питание второго полукомплекта БУНД]; иначе, <c>false</c>.
        /// </value>
        public bool BUNDPower2
        {
            get
            {
                return _bundPower2;
            }

            private set
            {
                _bundPower2 = value;
                FirePropertyChangedEvent("BUNDPower2");
            }
        }

        /// <summary>
        /// Получает значение, показывающее, нужно ли [передавать релейную команду] БУСК по линии A.
        /// </summary>
        /// <value>
        ///   <c>true</c> если [передавать релейную команду]; иначе, <c>false</c>.
        /// </value>
        public bool IsBUSKLineA
        {
            get
            {
                return _isBUSKLineA;
            }

            private set
            {
                _isBUSKLineA = value;
                FirePropertyChangedEvent("IsBUSKLineA");
            }
        }

        /// <summary>
        /// Получает значение, показывающее, нужно ли [передавать релейную команду] БУСК по линии B.
        /// </summary>
        /// <value>
        ///   <c>true</c> если [передавать релейную команду]; иначе, <c>false</c>.
        /// </value>
        public bool IsBUSKLineB
        {
            get
            {
                return _isBUSKLineB;
            }

            private set
            {
                _isBUSKLineB = value;
                FirePropertyChangedEvent("IsBUSKLineB");
            }
        }

        /// <summary>
        /// Получает значение, показывающее, нужно ли [передавать релейную команду] БУНД по линии A.
        /// </summary>
        /// <value>
        ///   <c>true</c> если [передавать релейную команду]; иначе, <c>false</c>.
        /// </value>
        public bool IsBUNDLineA
        {
            get
            {
                return _isBUNDLineA;
            }

            private set
            {
                _isBUNDLineA = value;
                FirePropertyChangedEvent("IsBUNDLineA");
            }
        }

        /// <summary>
        /// Получает значение, показывающее, нужно ли [передавать релейную команду] БУНД по линии B.
        /// </summary>
        /// <value>
        ///   <c>true</c> если [передавать релейную команду]; иначе, <c>false</c>.
        /// </value>
        public bool IsBUNDLineB
        {
            get
            {
                return _isBUNDLineB;
            }

            private set
            {
                _isBUNDLineB = value;
                FirePropertyChangedEvent("IsBUNDLineB");
            }
        }
        public byte[] Spacewire1Data { get; set; }
        public byte[] Spacewire2Data { get; set; }
        public byte[] Spacewire4Data { get; set; }

        /// <summary>
        /// Получает или задает значение, показывающее, нужно ли [записывать данные от прибора в файл].
        /// </summary>
        /// <value>
        /// <c>true</c> если [записывать данные от прибора в файл]; иначе, <c>false</c>.
        /// </value>
        public bool IsWriteDevDataToFile
        {
            get 
            { 
                return _isWriteDevDataToFile; 
            }

            set
            {
                _isWriteDevDataToFile = value;
                WriteDevData(value);
                FirePropertyChangedEvent("IsWriteDevDataToFile");
            }
        }

        /// <summary>
        /// Получает или задает канал имитатора БМ-4.
        /// </summary>
        /// <value>
        /// Канал имитатора БМ-4.
        /// </value>
        public SimRouterChannel SelectSimRouterChannel
        {
            get 
            {
                return _simRouterChannel;
            }

            set
            {
                if (value == _simRouterChannel)
                {
                    return;
                }        
            
                _simRouterChannel = value;
                ControlValuesList[Global.SpaceWire2Control].SetProperty(Global.PropertySpaceWire2Channel, (int)value); 
                FirePropertyChangedEvent("SelectSimRouterChannel");
                FirePropertyChangedEvent("IsBUK2BM2Channel");
                FirePropertyChangedEvent("IsBUK1BM1Channel");
                FirePropertyChangedEvent("IsBUK1BM2Channel");
                FirePropertyChangedEvent("IsBUK2BM1Channel");
                Device.CmdSetDeviceLogicAddr();
            }
        }

        /// <summary>
        /// Получает или задает значение, показывающее, что [выбран канал БУК ПК1 - БМ-4 ПК1].
        /// </summary>
        /// <value>
        ///   <c>true</c> если [выбран канал БУК ПК1 - БМ-4 ПК1]; иначе, <c>false</c>.
        /// </value>
        public bool IsBUK1BM1Channel
        {
            get
            {
                return SimRouterChannel.BUK1BM1 == SelectSimRouterChannel;
            }

            set
            {
                SelectSimRouterChannel = value ? SimRouterChannel.BUK1BM1 : SelectSimRouterChannel;
            }
        }

        public string ShowSpaceWire2AddrBusk
        { 
            get
            {
                return string.Format(Resource.Get(@"stShowLogicBusk"), SpaceWire2AddrBusk);
            }
        }

        public uint SpaceWire2AddrBusk
        {
            get
            {
                return _spaceWire2AddrBusk;
            }

            set
            {
                _spaceWire2AddrBusk = value;
                ControlValuesList[Global.SpaceWire2SPTPLogicBusk].SetProperty(Global.PropertySpaceWire2LogicBusk, (int)value); 
                FirePropertyChangedEvent("ShowSpaceWire2AddrBusk");
            }
        }


        public string ShowSpaceWire2AddrBuk
        {
            get
            {
                return string.Format(Resource.Get(@"stShowLogicBuk"), SpaceWire2LogicAddrBuk);
            }
        }

        public uint SpaceWire2LogicAddrBuk
        {
            get
            {
                return _spaceWire2AddrBuk;
            }

            set
            {
                _spaceWire2AddrBuk = value;
                ControlValuesList[Global.SpaceWire2SPTPLogicBuk].SetProperty(Global.PropertySpaceWire2LogicBuk, (int)value); 
                FirePropertyChangedEvent("ShowSpaceWire2AddrBuk");
            }
        }

        public string ShowSpaceWire2AddrBkp
        {
            get
            {
                return string.Format(Resource.Get(@"stShowLogicBkp"), SpaceWire2AddrBkp);
            }
        }

        public uint SpaceWire2AddrBkp
        {
            get
            {
                return _spaceWire2AddrBkp;
            }

            set
            {
                _spaceWire2AddrBkp = value;
                ControlValuesList[Global.SpaceWire2SPTPLogicBkp].SetProperty(Global.PropertySpaceWire2LogicBkp, (int)value); 
                FirePropertyChangedEvent("ShowSpaceWire2AddrBkp");
            }
        }

        public string ShowSimSpaceWire1AddrBusk
        {
            get
            {
                return string.Format(Resource.Get(@"stShowSimLogicBusk"), SimSpaceWire1AddrBusk);
            }
        }

        public uint SimSpaceWire1AddrBusk
        {
            get
            {
                return _simSpaceWire1AddrBusk;
            }

            set
            {
                _simSpaceWire1AddrBusk = value;
                ControlValuesList[Global.SpaceWire1SPTPSimLogicBusk].SetProperty(Global.PropertySimSpaceWire1LogicBusk, (int)value); 
                FirePropertyChangedEvent("ShowSimSpaceWire1AddrBusk");
            }
        }

        public string ShowSimSpaceWire1AddrNP1
        {
            get
            {
                return string.Format(Resource.Get(@"stShowSimLogicNP1"), SimSpaceWire1AddrNP1);
            }
        }

        public uint SimSpaceWire1AddrNP1
        {
            get
            {
                return _simSpaceWire1AddrNP1;
            }

            set
            {
                _simSpaceWire1AddrNP1 = value;
                ControlValuesList[Global.SpaceWire1SPTPSimLogicNP1].SetProperty(Global.PropertySimSpaceWire1LogicNP1, (int)value); 
                FirePropertyChangedEvent("ShowSimSpaceWire1AddrNP1");
            }
        }

        public string ShowSimSpaceWire1AddrNP2
        {
            get
            {
                return string.Format(Resource.Get(@"stShowSimLogicNP2"), SimSpaceWire1AddrNP2);
            }
        }

        public uint SimSpaceWire1AddrNP2
        {
            get
            {
                return _simSpaceWire1AddrNP2;
            }

            set
            {
                _simSpaceWire1AddrNP2 = value;
                ControlValuesList[Global.SpaceWire1SPTPSimLogicNP2].SetProperty(Global.PropertySimSpaceWire1LogicNP2, (int)value); 
                FirePropertyChangedEvent("ShowSimSpaceWire1AddrNP2");
            }
        }
        /// <summary>
        /// Получает или задает значение, показывающее, что [выбран канал БУК ПК1 - БМ-4 ПК2].
        /// </summary>
        /// <value>
        ///   <c>true</c> если [выбран канал БУК ПК1 - БМ-4 ПК2]; иначе, <c>false</c>.
        /// </value>
        public bool IsBUK1BM2Channel
        {
            get
            {
                return SimRouterChannel.BUK1BM2 == SelectSimRouterChannel;
            }

            set
            {
                SelectSimRouterChannel = value ? SimRouterChannel.BUK1BM2 : SelectSimRouterChannel;
            }
        }

        /// <summary>
        /// Получает или задает значение, показывающее, что [выбран канал БУК ПК2 - БМ-4 ПК1].
        /// </summary>
        /// <value>
        ///   <c>true</c> если [выбран канал БУК ПК2 - БМ-4 ПК1]; иначе, <c>false</c>.
        /// </value>
        public bool IsBUK2BM1Channel
        {
            get
            {
                return SimRouterChannel.BUK2BM1 == SelectSimRouterChannel;
            }

            set
            {
                SelectSimRouterChannel = value ? SimRouterChannel.BUK2BM1 : SelectSimRouterChannel;
            }
        }

        /// <summary>
        /// Получает или задает значение, показывающее, что [выбран канал БУК ПК2 - БМ-4 ПК2].
        /// </summary>
        /// <value>
        ///   <c>true</c> если [выбран канал БУК ПК2 - БМ-4 ПК2]; иначе, <c>false</c>.
        /// </value>
        public bool IsBUK2BM2Channel
        {
            get
            {
                return SimRouterChannel.BUK2BM2 == SelectSimRouterChannel;
            }

            set
            {
                SelectSimRouterChannel = value ? SimRouterChannel.BUK2BM2 : SelectSimRouterChannel;
            }
        }

        /// <summary>
        /// Получает или задает значение, показывающее, что [интерфейс SpaceWire2 включен].
        /// </summary>
        /// <value>
        /// <c>true</c> если [интерфейс SpaceWire2 включен]; иначе, <c>false</c>.
        /// </value>
        public bool IsSpaceWire2IntfOn
        { 
            get
            {
                return _isSpaceWire2IntfOn;
            }

            set
            {
                _isSpaceWire2IntfOn = value;
                ControlValuesList[Global.SpaceWire2Control].SetProperty(Global.PropertySpaceWire2IntfOn, Convert.ToInt32(_isSpaceWire2IntfOn));
                FirePropertyChangedEvent("IsSpaceWire2IntfOn");
            }
        }

        /// <summary>
        /// Получает или задает значение, показывающее, что [связь по интерфейсу SpaceWire2 установлена].
        /// </summary>
        /// <value>
        /// <c>true</c> если [связь по интерфейсу SpaceWire2 установлена]; иначе, <c>false</c>.
        /// </value>
        public bool IsSpaceWire2Connected 
        {
            get
            {
                return _isSpaceWire2Connected;
            }

            set
            {
                _isSpaceWire2Connected = value;
                FirePropertyChangedEvent("IsSpaceWire2Connected");
            }
        }

        public bool IsSpaceWire2RecordSendRMAP
        {
            get
            {
                return _isSpaceWire2RecordSendRMAP;
            }

            set
            {
                _isSpaceWire2RecordSendRMAP = value;
                ControlValuesList[Global.SpaceWire2Record].SetProperty(Global.PropertySpaceWire2RecordSendRMAP, Convert.ToInt32(_isSpaceWire2RecordSendRMAP));
                FirePropertyChangedEvent("IsSpaceWire2RecordSendRMAP");
            }
        }

        public bool IsSpaceWire2RecordSendBuk
        {
            get
            {
                return _isSpaceWire2RecordSendBuk;
            }

            set
            {
                _isSpaceWire2RecordSendBuk = value;
                ControlValuesList[Global.SpaceWire2Record].SetProperty(Global.PropertySpaceWire2RecordSendBuk, Convert.ToInt32(_isSpaceWire2RecordSendBuk));
                FirePropertyChangedEvent("IsSpaceWire2RecordSendBuk");
            }
        }

        public bool IsSpaceWire2RecordSendBkp
        {
            get
            {
                return _isSpaceWire2RecordSendBkp;
            }

            set
            {
                _isSpaceWire2RecordSendBkp = value;
                ControlValuesList[Global.SpaceWire2Record].SetProperty(Global.PropertySpaceWire2RecordSendBkp, Convert.ToInt32(_isSpaceWire2RecordSendBkp));
                FirePropertyChangedEvent("IsSpaceWire2RecordSendBkp");
            }
        }

        /// <summary>
        /// Получает или задает значение, показывающее, что [выдаются метки времени приборам].
        /// </summary>
        /// <value>
        /// <c>true</c> если [выдаются метки времени приборам]; иначе, <c>false</c>.
        /// </value>
        public bool IsSpaceWire2TimeMark
        {
            get
            {
                return _isSpaceWire2TimeMark;
            }

            set
            {
                _isSpaceWire2TimeMark = value;
                ControlValuesList[Global.SpaceWire2ControlSPTP].SetProperty(Global.PropertySpaceWire2TimeMark, Convert.ToInt32(_isSpaceWire2TimeMark));
                FirePropertyChangedEvent("IsSpaceWire2TimeMark");
            }
        }

        /// <summary>
        /// Получает или задает значение, показывающее, что [включен обмен для прибора БУК].
        /// </summary>
        /// <value>
        /// <c>true</c> если [включен обмен для прибора БУК]; иначе, <c>false</c>.
        /// </value>
        public bool IsSpaceWire2BukTrans
        {
            get
            {
                return _isSpaceWire2BukTrans;
            }

            set
            {
                _isSpaceWire2BukTrans = value;
                ControlValuesList[Global.SpaceWire2ControlSPTP].SetProperty(Global.PropertySpaceWire2BukTrans, Convert.ToInt32(_isSpaceWire2BukTrans));
                FirePropertyChangedEvent("IsSpaceWire2BukTrans");
            }
        }

        /// <summary>
        /// Получает или задает значение, показывающее, что [включен обмен для прибора БКП].
        /// </summary>
        /// <value>
        /// <c>true</c> если [включен обмен для прибора БКП]; иначе, <c>false</c>.
        /// </value>
        public bool IsSpaceWire2BkpTrans
        {
            get
            {
                return _isSpaceWire2BkpTrans;
            }

            set
            {
                _isSpaceWire2BkpTrans = value;
                ControlValuesList[Global.SpaceWire2ControlSPTP].SetProperty(Global.PropertySpaceWire2BkpTrans, Convert.ToInt32(_isSpaceWire2BkpTrans));
                FirePropertyChangedEvent("IsSpaceWire2BkpTrans");
            }
        }

        /// <summary>
        /// Получает или задает значение, показывающее, что [выдается КБВ для прибора БУК].
        /// </summary>
        /// <value>
        /// <c>true</c> если [выдается КБВ для прибора БУК]; иначе, <c>false</c>.
        /// </value>
        public bool IsSpaceWire2BukKbv
        {
            get
            {
                return _isSpaceWire2BukKbv;
            }

            set
            {
                _isSpaceWire2BukKbv = value;
                ControlValuesList[Global.SpaceWire2ControlSPTP].SetProperty(Global.PropertySpaceWire2BukKbv, Convert.ToInt32(_isSpaceWire2BukKbv));
                FirePropertyChangedEvent("IsSpaceWire2BukKbv");
            }
        }

        /// <summary>
        /// Получает или задает значение, показывающее, что [выдается КБВ для прибора БКП].
        /// </summary>
        /// <value>
        /// <c>true</c> если [выдается КБВ для прибора БКП]; иначе, <c>false</c>.
        /// </value>
        public bool IsSpaceWire2BkpKbv
        {
            get
            {
                return _isSpaceWire2BkpKbv;
            }

            set
            {
                _isSpaceWire2BkpKbv = value;
                ControlValuesList[Global.SpaceWire2ControlSPTP].SetProperty(Global.PropertySpaceWire2BkpKbv, Convert.ToInt32(_isSpaceWire2BkpKbv));
                FirePropertyChangedEvent("IsSpaceWire2BkpKbv");
            }
        }

        /// <summary>
        /// Получает или задает значение, показывающее, что [можно выдавать пакеты данных в БУК].
        /// </summary>
        /// <value>
        /// <c>true</c> если [можно выдавать пакеты данных в БУК]; иначе, <c>false</c>.
        /// </value>
        public bool IsSpaceWire2BukTransData
        {
            get
            {
                return _isSpaceWire2BukTransData;
            }

            set
            {
                _isSpaceWire2BukTransData = value;
                ControlValuesList[Global.SpaceWire2ControlSPTP].SetProperty(Global.PropertySpaceWire2BukTransData, Convert.ToInt32(_isSpaceWire2BukTransData));
                FirePropertyChangedEvent("IsSpaceWire2BukTransData");
            }
        }

        /// <summary>
        /// Получает или задает значение, показывающее, что [можно выдавать пакеты данных в БКП].
        /// </summary>
        /// <value>
        /// <c>true</c> если [можно выдавать пакеты данных в БКП]; иначе, <c>false</c>.
        /// </value>
        public bool IsSpaceWire2BkpTransData
        {
            get
            {
                return _isSpaceWire2BkpTransData;
            }

            set
            {
                _isSpaceWire2BkpTransData = value;
                ControlValuesList[Global.SpaceWire2ControlSPTP].SetProperty(Global.PropertySpaceWire2BkpTransData, Convert.ToInt32(_isSpaceWire2BkpTransData));
                FirePropertyChangedEvent("IsSpaceWire2BkpTransData");
            }
        }

        /// <summary>
        /// Получает или задает значение, показывающее, что [интерфейс SpaceWire1 включен].
        /// </summary>
        /// <value>
        /// <c>true</c> если [интерфейс SpaceWire1 включен]; иначе, <c>false</c>.
        /// </value>
        public bool IsSpaceWire1IntfOn
        {
            get
            {
                return _isSpaceWire1IntfOn;
            }

            set
            {
                _isSpaceWire1IntfOn = value;
                ControlValuesList[Global.SpaceWire1Control].SetProperty(Global.PropertySpaceWire1IntfOn, Convert.ToInt32(_isSpaceWire1IntfOn));
                FirePropertyChangedEvent("IsSpaceWire1IntfOn");
            }
        }

        /// <summary>
        /// Получает или задает значение, показывающее, что [связь по интерфейсу SpaceWire1 установлена].
        /// </summary>
        /// <value>
        /// <c>true</c> если [связь по интерфейсу SpaceWire1 установлена]; иначе, <c>false</c>.
        /// </value>
        public bool IsSpaceWire1Connected
        {
            get
            {
                return _isSpaceWire1Connected;
            }

            set
            {
                _isSpaceWire1Connected = value;
                FirePropertyChangedEvent("IsSpaceWire1Connected");
            }
        }

        /// <summary>
        /// Получает или задает значение, показывающее, что [включен обмен для прибора НП1].
        /// </summary>
        /// <value>
        /// <c>true</c> если [включен обмен для прибора НП1]; иначе, <c>false</c>.
        /// </value>
        public bool IsSpaceWire1NP1Trans
        {
            get
            {
                return _isSpaceWire1NP1Trans;
            }

            set
            {
                _isSpaceWire1NP1Trans = value;
                ControlValuesList[Global.SpaceWire1ControlSPTP].SetProperty(Global.PropertySpaceWire1NP1Trans, Convert.ToInt32(_isSpaceWire1NP1Trans));
                FirePropertyChangedEvent("IsSpaceWire1NP1Trans");
            }
        }

        /// <summary>
        /// Получает или задает значение, показывающее, что [включен обмен для прибора НП2].
        /// </summary>
        /// <value>
        /// <c>true</c> если [включен обмен для прибора НП2]; иначе, <c>false</c>.
        /// </value>
        public bool IsSpaceWire1NP2Trans
        {
            get
            {
                return _isSpaceWire1NP2Trans;
            }

            set
            {
                _isSpaceWire1NP2Trans = value;
                ControlValuesList[Global.SpaceWire1ControlSPTP].SetProperty(Global.PropertySpaceWire1NP2Trans, Convert.ToInt32(_isSpaceWire1NP2Trans));
                FirePropertyChangedEvent("IsSpaceWire1NP2Trans");
            }
        }

        /// <summary>
        /// Получает или задает значение, показывающее, что [можно выдавать пакеты данных в НП1].
        /// </summary>
        /// <value>
        /// <c>true</c> если [можно выдавать пакеты данных в НП1]; иначе, <c>false</c>.
        /// </value>
        public bool IsSpaceWire1NP1TransData
        {
            get
            {
                return _isSpaceWire1NP1TransData;
            }

            set
            {
                _isSpaceWire1NP1TransData = value;
                ControlValuesList[Global.SpaceWire1ControlSPTP].SetProperty(Global.PropertySpaceWire1NP1TransData, Convert.ToInt32(_isSpaceWire1NP1TransData));
                FirePropertyChangedEvent("IsSpaceWire1NP1TransData");
            }
        }

        /// <summary>
        /// Получает или задает значение, показывающее, что [можно выдавать пакеты данных в НП2].
        /// </summary>
        /// <value>
        /// <c>true</c> если [можно выдавать пакеты данных в НП2]; иначе, <c>false</c>.
        /// </value>
        public bool IsSpaceWire1NP2TransData
        {
            get
            {
                return _isSpaceWire1NP2TransData;
            }

            set
            {
                _isSpaceWire1NP2TransData = value;
                ControlValuesList[Global.SpaceWire1ControlSPTP].SetProperty(Global.PropertySpaceWire1NP2TransData, Convert.ToInt32(_isSpaceWire1NP2TransData));
                FirePropertyChangedEvent("IsSpaceWire1NP2TransData");
            }
        }

        public uint SpaceWire1NP1SendTime
        {
            get
            {
                return _spaceWire1NP1SendTime;
            }

            set
            {
                _spaceWire1NP1SendTime = value;
                ControlValuesList[Global.SpaceWire1ControlSPTPNP1SendTime].SetProperty(Global.PropertySpaceWire1NP1SendTime, Convert.ToInt32(_spaceWire1NP1SendTime));
                FirePropertyChangedEvent("IsSpaceWire1NP1SendTime");
            }
        }

        public uint SpaceWire1NP2SendTime
        {
            get
            {
                return _spaceWire1NP2SendTime;
            }

            set
            {
                _spaceWire1NP2SendTime = value;
                ControlValuesList[Global.SpaceWire1ControlSPTPNP2SendTime].SetProperty(Global.PropertySpaceWire1NP2SendTime, Convert.ToInt32(_spaceWire1NP2SendTime));
                FirePropertyChangedEvent("IsSpaceWire1NP2SendTime");
            }
        }

        public uint SpaceWire1NP1DataSize
        {
            get
            {
                return _spaceWire1NP1DataSize;
            }

            set
            {
                _spaceWire1NP1DataSize = value;
                ControlValuesList[Global.SpaceWire1ControlSPTPNP1DataSize].SetProperty(Global.PropertySpaceWire1NP1DataSize, Convert.ToInt32(_spaceWire1NP1DataSize));
                FirePropertyChangedEvent("IsSpaceWire1NP1DataSize");
            }
        }

        public uint SpaceWire1NP2DataSize
        {
            get
            {
                return _spaceWire1NP2DataSize;
            }

            set
            {
                _spaceWire1NP2DataSize = value;
                ControlValuesList[Global.SpaceWire1ControlSPTPNP2DataSize].SetProperty(Global.PropertySpaceWire1NP2DataSize, Convert.ToInt32(_spaceWire1NP2DataSize));
                FirePropertyChangedEvent("IsSpaceWire1NP2DataSize");
            }
        }

        public bool IsSpaceWire1RecordBusy
        {
            get
            {
                return _isSpaceWire1RecordBusy || IsSpaceWire1RecordSend;
            }

            set
            {
                _isSpaceWire1RecordBusy = value;
                FirePropertyChangedEvent("IsSpaceWire1RecordBusy");
            }
        }

        public bool IsSpaceWire1RecordSend
        {
            get
            {
                return _isSpaceWire1RecordSend;
            }

            set
            {
                _isSpaceWire1RecordSend = value;
                ControlValuesList[Global.SpaceWire1Record].SetProperty(Global.PropertySpaceWire1RecordSend, Convert.ToInt32(_isSpaceWire1RecordSend));
                FirePropertyChangedEvent("IsSpaceWire1RecordSend");
            }
        }

        /// <summary>
        /// Получает или задает значение, показывающее, что [интерфейс SpaceWire4 включен].
        /// </summary>
        /// <value>
        /// <c>true</c> если [интерфейс SpaceWire4 включен]; иначе, <c>false</c>.
        /// </value>
        public bool IsSpaceWire4IntfOn
        {
            get
            {
                return _isSpaceWire4IntfOn;
            }

            set
            {
                _isSpaceWire4IntfOn = value;
                ControlValuesList[Global.SpaceWire4Control].SetProperty(Global.PropertySpaceWire4IntfOn, Convert.ToInt32(_isSpaceWire4IntfOn));
                FirePropertyChangedEvent("IsSpaceWire4IntfOn");
            }
        }

        /// <summary>
        /// Получает или задает значение, показывающее, что [связь по интерфейсу SpaceWire4 установлена].
        /// </summary>
        /// <value>
        /// <c>true</c> если [связь по интерфейсу SpaceWire4 установлена]; иначе, <c>false</c>.
        /// </value>
        public bool IsSpaceWire4Connected
        {
            get
            {
                return _isSpaceWire4Connected;
            }

            set
            {
                _isSpaceWire4Connected = value;
                FirePropertyChangedEvent("IsSpaceWire4Connected");
            }
        }

        public bool IsSpaceWire4TimeMark
        {
            get
            {
                return _isSpaceWire4TimeMark;
            }

            set
            {
                _isSpaceWire4TimeMark = value;
                ControlValuesList[Global.SpaceWire4Control].SetProperty(Global.PropertySpaceWire4TimeMark, Convert.ToInt32(_isSpaceWire4TimeMark));
                FirePropertyChangedEvent("IsSpaceWire4TimeMark");
            }
        }

        public bool IsSpaceWire4EEPSend
        {
            get
            {
                return _isSpaceWire4EEPSend;
            }

            set
            {
                _isSpaceWire4EEPSend = value;
                ControlValuesList[Global.SpaceWire4Record].SetProperty(Global.PropertySpaceWire4EEPSend, Convert.ToInt32(_isSpaceWire4EEPSend));
                FirePropertyChangedEvent("IsSpaceWire4EEPSend");
            }
        }

        public bool IsSpaceWire4EOPSend
        {
            get
            {
                return _isSpaceWire4EOPSend;
            }

            set
            {
                _isSpaceWire4EOPSend = value;
                ControlValuesList[Global.SpaceWire4Record].SetProperty(Global.PropertySpaceWire4EOPSend, Convert.ToInt32(_isSpaceWire4EOPSend));
                FirePropertyChangedEvent("IsSpaceWire4EOPSend");
            }
        }

        public bool IsSpaceWire4AutoSend
        {
            get
            {
                return _isSpaceWire4AutoSend;
            }

            set
            {
                _isSpaceWire4AutoSend = value;
                ControlValuesList[Global.SpaceWire4Record].SetProperty(Global.PropertySpaceWire4AutoSend, Convert.ToInt32(_isSpaceWire4AutoSend));
                FirePropertyChangedEvent("IsSpaceWire4AutoSend");
            }
        }

        public bool IsSpaceWire4RecordBusy
        {
            get
            {
                return _isSpaceWire4RecordBusy || IsSpaceWire4RecordSend; ;
            }

            set
            {
                _isSpaceWire4RecordBusy = value;
                FirePropertyChangedEvent("IsSpaceWire4RecordBusy");
            }
        }

        public bool IsSpaceWire4RecordSend
        {
            get
            {
                return _isSpaceWire4RecordSend;
            }

            set
            {
                _isSpaceWire4RecordSend = value;
                ControlValuesList[Global.SpaceWire4Record].SetProperty(Global.PropertySpaceWire4RecordSend, Convert.ToInt32(_isSpaceWire4RecordSend));
                FirePropertyChangedEvent("IsSpaceWire4RecordSend");
            }
        }
        
        /// <summary>
        /// Получает размер файла данных от прибора.
        /// </summary>
        /// <value>
        /// Размер файла (в байтах).
        /// </value>
        public long DevDataFileSize
        {
            get 
            {
                if (_devDataLogStream != null)
                {
                    return _devDataLogStream.Length;
                }
                else
                {
                    return 0;
                }
            }
        }

        /// <summary>
        /// Получает имя файла данных от прибора.  
        /// </summary>
        /// <value>
        /// Имя файла.
        /// </value>
        public string DevDataFileName
        {
            get 
            {
                if (_devDataLogStream != null)
                {
                    return _devDataLogStream.Name;
                }
                else
                {
                    return string.Empty;
                }
            }
        }

        /// <summary>
        /// Получает или задает время, пришедшее от прибора.
        /// </summary>       
        public EgseTime DeviceTime { get; set; }

        /// <summary>
        /// Получает или задает экземпляр декодера телеметрии.
        /// </summary>
        public TelemetryBUK Tele { get; set; }

        /// <summary>
        /// Получает или задает список управляющих элементов.
        /// </summary>
        public List<ControlValue> ControlValuesList { get; set; }

        /// <summary>
        /// Для каждого элемента управления тикаем временем.
        /// </summary>
        public void TickAllControlsValues()
        {
            Debug.Assert(ControlValuesList != null, "ControlValuesList не должны быть равны null!");

            foreach (ControlValue cv in ControlValuesList)
            {
                cv.TimerTick();
            }
        }

        /// <summary>
        /// Метод вызывается, когда прибор подсоединяется или отсоединяется.
        /// </summary>
        /// <param name="isConnected">Если установлено <c>true</c> [прибор подключен].</param>
        public void OnChangeConnection(bool isConnected)
        {
            Connected = isConnected;
            if (Connected)
            {
                Device.CmdSetDeviceTime();
                Device.CmdSetDeviceLogicAddr();
                RefreshAllControlsValues();
                LogsClass.LogMain.LogText = Resource.Get(@"stDeviceName") + Resource.Get(@"stConnected");
            }
            else
            {
                LogsClass.LogMain.LogText = Resource.Get(@"stDeviceName") + Resource.Get(@"stDisconnected");
            }
        }

        /// <summary>
        /// Указываем какой файл использовать для записи данных от прибора и по какому каналу.
        /// </summary>
        /// <param name="stream">Поток для записи данных.</param>
        public void SetFileForLogDevData(FileStream stream)
        {
            _devDataLogStream = stream;
        }

        /// <summary>
        /// Записывает данные от прибора в файл.
        /// </summary>
        /// <param name="startWrite">Если установлено <c>true</c> [идет запись].</param>
        public void WriteDevData(bool startWrite)
        {
            if (startWrite)
            {
                string dataLogDir = Directory.GetCurrentDirectory().ToString() + "\\DATA\\";
                Directory.CreateDirectory(dataLogDir);
                string fileName = dataLogDir + Resource.Get(@"stDevLogName") + "_" + DateTime.Now.ToString("yyMMdd_HHmmss") + ".dat";
                _devDataLogStream = new FileStream(fileName, System.IO.FileMode.Create);
            }
            else 
            {
                if (_devDataLogStream != null)
                {
                    _devDataLogStream.Close();
                    _devDataLogStream = null;
                }
            }
        }

        /// <summary>
        /// Called when [spacewire2 MSG].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="SpacewireMsgEventArgs"/> instance containing the event data.</param>
        protected virtual void OnSpacewire2Msg(object sender, SpacewireMsgEventArgs e)
        {
            if (this.GotSpacewire2Msg != null)
            {
                this.GotSpacewire2Msg(sender, e);
            }
        }

        /// <summary>
        /// Вызывается при подключении прибора, чтобы все элементы управления обновили свои значения.
        /// </summary>
        private void RefreshAllControlsValues()
        {
            Debug.Assert(ControlValuesList != null, Resource.Get(@"eNotAssigned"));
            foreach (ControlValue cv in ControlValuesList)
            {
                cv.RefreshGetValue();
            }
        }

        /// <summary>
        /// Метод, обрабатывающий сообщения от декодера USB.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="msg">Сообщение для обработки.</param>
        private void OnMessageFunc(object sender, ProtocolMsgEventArgs msg)
        {
            if (msg != null)
            {
                switch (msg.Addr)
                {
                    case TimeDataAddr:
                        Array.Copy(msg.Data, 0, DeviceTime.Data, 0, 6);
                        ControlValuesList[Global.SpaceWire2Control].UsbValue = msg.Data[7];
                        ControlValuesList[Global.SpaceWire2Record].UsbValue = msg.Data[10]; 
                        ControlValuesList[Global.SpaceWire2SPTPLogicBusk].UsbValue = msg.Data[11];
                        ControlValuesList[Global.SpaceWire2SPTPLogicBuk].UsbValue = msg.Data[12];
                        ControlValuesList[Global.SpaceWire2SPTPLogicBkp].UsbValue = msg.Data[13];
                        ControlValuesList[Global.SpaceWire2ControlSPTP].UsbValue = msg.Data[14];                       
                        ControlValuesList[Global.SpaceWire1Control].UsbValue = msg.Data[17];
                        ControlValuesList[Global.SpaceWire1Record].UsbValue = msg.Data[20];
                        ControlValuesList[Global.SpaceWire1ControlSPTP].UsbValue = msg.Data[21];
                        ControlValuesList[Global.SpaceWire1SPTPSimLogicBusk].UsbValue = msg.Data[22];
                        ControlValuesList[Global.SpaceWire1SPTPSimLogicNP1].UsbValue = msg.Data[23];
                        ControlValuesList[Global.SpaceWire1SPTPSimLogicNP2].UsbValue = msg.Data[24];
                        ControlValuesList[Global.SpaceWire1ControlSPTPNP1SendTime].UsbValue = (msg.Data[26] << 8) | (msg.Data[25]);
                        ControlValuesList[Global.SpaceWire1ControlSPTPNP2SendTime].UsbValue = (msg.Data[28] << 8) | (msg.Data[27]);
                        ControlValuesList[Global.SpaceWire1ControlSPTPNP1DataSize].UsbValue = (msg.Data[30] << 8) | (msg.Data[29]); // XXX
                        ControlValuesList[Global.SpaceWire1ControlSPTPNP2DataSize].UsbValue = (msg.Data[32] << 8) | (msg.Data[31]); // XXX
                        ControlValuesList[Global.SpaceWire4Control].UsbValue = msg.Data[29];
                        ControlValuesList[Global.SpaceWire4Record].UsbValue = msg.Data[32];
                        break;
                    case TeleDataAddr:
                        Tele.Update(msg.Data);
                        ControlValuesList[Global.PowerControl].UsbValue = msg.Data[3];
                        break;
                }
            }
        }    

        /// <summary>
        /// Обработчик ошибок протокола декодера USB.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="msg">Сообщение об ошибке.</param>
        private void OnErrorFunc(object sender, ProtocolErrorEventArgs msg)
        {
            string bufferStr = Converter.ByteArrayToHexStr(msg.Data);
            LogsClass.LogErrors.LogText = string.Format(Resource.Get(@"stOnErrorUSBMsg"), msg.Msg, bufferStr, msg.ErrorPos.ToString());
        }

        /// <summary>
        /// Fires the property changed event.
        /// </summary>
        /// <param name="propertyName">Имя свойства.</param>
        private void FirePropertyChangedEvent(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public string Error
        {
            get
            {
                return null;
            }
        }

        public string this[string name]
        {
            get
            {
                string result = null;

                if (name == "Spacewire1Data")
                {
                    if ((null != Spacewire1Data) && (0 == Spacewire1Data.Length))
                    {
                        result = "Некорректный ввод данных! Повторите ввод.";
                    }
                }

                if (name == "Spacewire4Data")
                {
                    if ((null != Spacewire4Data) && (0 == Spacewire4Data.Length))
                    {
                        result = "Некорректный ввод данных! Повторите ввод.";
                    }
                }

                if (name == "Spacewire2Data")
                {
                    if ((null != Spacewire2Data) && (0 == Spacewire2Data.Length))
                    {
                        result = "Некорректный ввод данных! Повторите ввод.";
                    }
                }

                return result;
            }
        }
    }
}
