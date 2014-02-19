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
        private const int SpaceWire2Control = 0x04;

        /// <summary>
        /// Адресный байт "Управление обменом с приборами по SPTP".
        /// </summary>
        private const int SpaceWire2ControlSPTP = 0x0B;

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
            SendToUSB(SpaceWire2Control, new byte[1] { (byte)value }); 
        }

        /// <summary>
        /// Команда SpaceWire2: Управление SPTP.
        /// </summary>
        /// <param name="value">Параметры управления SPTP.</param>
        public void CmdSimRouterControlSPTP(uint value)
        {
            SendToUSB(SpaceWire2ControlSPTP, new byte[1] { (byte)value }); 
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
    }

    /// <summary>
    /// Общий экземпляр, позволяющий управлять прибором (принимать данные, выдавать команды).
    /// </summary>
    public class DevBUK : INotifyPropertyChanged
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

        /// <summary>
        /// SPACEWIRE 2: Управление: Установлена связь.
        /// </summary>
        private bool _isSpaceWire2Connected;

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

            ControlValuesList[Global.SpaceWire2ControlSPTP].AddProperty(Global.PropertySpaceWire2TimeMark, 0, 1, Device.CmdSimRouterControlSPTP, delegate(uint value) { IsSpaceWire2TimeMark = 1 == value; });
            ControlValuesList[Global.SpaceWire2ControlSPTP].AddProperty(Global.PropertySpaceWire2BukTrans, 1, 1, Device.CmdSimRouterControlSPTP, delegate(uint value) { IsSpaceWire2BukTrans = 1 == value; });
            ControlValuesList[Global.SpaceWire2ControlSPTP].AddProperty(Global.PropertySpaceWire2BkpTrans, 4, 1, Device.CmdSimRouterControlSPTP, delegate(uint value) { IsSpaceWire2BkpTrans = 1 == value; });
            ControlValuesList[Global.SpaceWire2ControlSPTP].AddProperty(Global.PropertySpaceWire2BukKbv, 2, 1, Device.CmdSimRouterControlSPTP, delegate(uint value) { IsSpaceWire2BukKbv = 1 == value; });
            ControlValuesList[Global.SpaceWire2ControlSPTP].AddProperty(Global.PropertySpaceWire2BkpKbv, 5, 1, Device.CmdSimRouterControlSPTP, delegate(uint value) { IsSpaceWire2BkpKbv = 1 == value; });
            ControlValuesList[Global.SpaceWire2ControlSPTP].AddProperty(Global.PropertySpaceWire2BukTransData, 3, 1, Device.CmdSimRouterControlSPTP, delegate(uint value) { IsSpaceWire2BukTransData = 1 == value; });
            ControlValuesList[Global.SpaceWire2ControlSPTP].AddProperty(Global.PropertySpaceWire2BkpTransData, 6, 1, Device.CmdSimRouterControlSPTP, delegate(uint value) { IsSpaceWire2BkpTransData = 1 == value; });
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
                ControlValuesList[Global.SpaceWire1Control].SetProperty(Global.PropertySpaceWire1NP1Trans, Convert.ToInt32(_isSpaceWire1NP1Trans));
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
                ControlValuesList[Global.SpaceWire1Control].SetProperty(Global.PropertySpaceWire1NP2Trans, Convert.ToInt32(_isSpaceWire1NP2Trans));
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
                ControlValuesList[Global.SpaceWire1Control].SetProperty(Global.PropertySpaceWire1NP1TransData, Convert.ToInt32(_isSpaceWire1NP1TransData));
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
                ControlValuesList[Global.SpaceWire1Control].SetProperty(Global.PropertySpaceWire1NP2TransData, Convert.ToInt32(_isSpaceWire1NP2TransData));
                FirePropertyChangedEvent("IsSpaceWire1NP2TransData");
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
                        ControlValuesList[Global.SpaceWire2ControlSPTP].UsbValue = msg.Data[14];
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
    }
}
