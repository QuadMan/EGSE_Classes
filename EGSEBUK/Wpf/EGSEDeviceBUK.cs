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
    public class EgseBuk : Device
    {
        /// <summary>
        /// Адресный байт "Адрес ИМИТАТОРА БУСКа".
        /// </summary>
        private const int Spacewire2LogicBuskAddr = 0x08;

        /// <summary>
        /// Адресный байт "Адрес БC".
        /// </summary>
        private const int Spacewire2LogicBukAddr = 0x09;

        /// <summary>
        /// Адресный байт "Адрес БКП".
        /// </summary>
        private const int Spacewire2LogicBkpAddr = 0x0a;

        /// <summary>
        /// Адресный байт "Адрес БУСК".
        /// </summary>
        private const int Spacewire1LogicBuskAddr = 0x17;

        /// <summary>
        /// Адресный байт "Адрес НП1".
        /// </summary>
        private const int Spacewire1LogicSD1Addr = 0x18;

        /// <summary>
        /// Адресный байт "Адрес НП2".
        /// </summary>
        private const int Spacewire1LogicSD2Addr = 0x19;

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
        private const int Spacewire2ControlAddr = 0x04;

        /// <summary>
        /// Адресный байт "Управление обменом с приборами по SPTP".
        /// </summary>
        private const int SpaceWire2SPTPControlAddr = 0x0B;

        /// <summary>
        /// Адресный байт "Управление".
        /// </summary>
        private const int Spacewire1ControlAddr = 0x12;

        /// <summary>
        /// Адресный байт "Управление обменом с приборами по SPTP".
        /// </summary>
        private const int Spacewire1SPTPControlAddr = 0x21;

        /// <summary>
        /// Адресный байт "Счетчик миллисекунд для НП1 (через сколько готовы данные)".
        /// </summary>
        private const int Spacewire1SPTPControlSD1SendTimeLoAddr = 0x1a;

        /// <summary>
        /// Адресный байт "Счетчик миллисекунд для НП1 (через сколько готовы данные)".
        /// </summary>
        private const int Spacewire1SPTPControlSD1SendTimeHiAddr = 0x1b;

        /// <summary>
        /// Адресный байт "Счетчик миллисекунд для НП2 (через сколько готовы данные)".
        /// </summary>
        private const int Spacewire1SPTPControlSD2SendTimeLoAddr = 0x1c;

        /// <summary>
        /// Адресный байт "Счетчик миллисекунд для НП2 (через сколько готовы данные)".
        /// </summary>
        private const int Spacewire1SPTPControlSD2SendTimeHiAddr = 0x1d;

        /// <summary>
        /// Адресный байт "Кол-во байт в пакете НП1".
        /// </summary>
        private const int Spacewire1SPTPControlSD1DataSizeLoAddr = 0x1e;

        /// <summary>
        /// Адресный байт "Кол-во байт в пакете НП1".
        /// </summary>
        private const int Spacewire1SPTPControlSD1DataSizeHiAddr = 0x1f;

        /// <summary>
        /// Адресный байт "Кол-во байт в пакете НП2".
        /// </summary>
        private const int Spacewire1SPTPControlSD2DataSizeLoAddr = 0x20;

        /// <summary>
        /// Адресный байт "Кол-во байт в пакете НП2".
        /// </summary>
        private const int Spacewire1SPTPControlSD2DataSizeHiAddr = 0x21;

        /// <summary>
        /// Адресный байт "Сброс адреса записи данных".
        /// </summary>
        private const int Spacewire1RecordFlushAddr = 0x12;

        /// <summary>
        /// Адресный байт "Запись данных".
        /// </summary>
        private const int Spacewire1RecordDataAddr = 0x14;

        /// <summary>
        /// Адресный байт "Запись данных(до 1 Кбайт)".
        /// </summary>
        private const int Spacewire1RecordSendAddr = 0x15;

        /// <summary>
        /// Адресный байт "Сброс адреса записи данных".
        /// </summary>
        private const int Spacewire2RecordFlushAddr = 0x05;

        /// <summary>
        /// Адресный байт "Запись данных".
        /// </summary>
        private const int Spacewire2RecordDataAddr = 0x06;

        /// <summary>
        /// Адресный байт "Запись данных(до 1 Кбайт)".
        /// </summary>
        private const int Spacewire2RecordSendAddr = 0x07;

        /// <summary>
        /// Адресный байт "Управление".
        /// </summary>
        private const int Spacewire4ControlAddr = 0x24;

        /// <summary>
        /// Адресный байт "Сброс адреса записи данных".
        /// </summary>
        private const int Spacewire4RecordFlushAddr = 0x25;

        /// <summary>
        /// Адресный байт "Запись данных".
        /// </summary>
        private const int Spacewire4RecordDataAddr = 0x26;

        /// <summary>
        /// Адресный байт "Запись данных(до 1 Кбайт)".
        /// </summary>
        private const int Spacewire4RecordSendAddr = 0x32;

        /// <summary>
        /// Адресный байт "Выбор имитатора Spacewire".
        /// </summary>
        private const int SelectSpacewireControlAddr = 0x11;

        /// <summary>
        /// Обеспечивает доступ к интерфейсу устройства. 
        /// </summary>
        private readonly EgseBukNotify _intfBUK;

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="EgseBuk" />.
        /// </summary>
        /// <param name="serial">Уникальный идентификатор USB.</param>
        /// <param name="dec">Экземпляр декодера USB для данного устройства.</param>
        /// <param name="intfBUK">Интерфейс управления данным устройством.</param>
        public EgseBuk(string serial, ProtocolUSBBase dec, EgseBukNotify intfBUK)
            : base(serial, dec, new USBCfg(10))
        {
            _intfBUK = intfBUK;
        }

        /// <summary>
        /// Отправляет команду включить/выключить питание ПК1 БУСК.
        /// </summary>
        /// <param name="value"><c>1</c> если [включить]; иначе, <c>0</c>.</param>
        public void CmdPowerBusk1(int value)
        {
            byte buf = 0;
            if (_intfBUK.TelemetryNotify.IsBuskLineA)
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

            if (_intfBUK.TelemetryNotify.IsBuskLineB)
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
        public void CmdPowerBusk2(int value)
        {
            byte buf = 0;
            if (_intfBUK.TelemetryNotify.IsBuskLineA)
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

            if (_intfBUK.TelemetryNotify.IsBuskLineB)
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
        public void CmdPowerBund1(int value)
        {
            byte buf = 0;
            if (_intfBUK.TelemetryNotify.IsBundLineA)
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

            if (_intfBUK.TelemetryNotify.IsBundLineB)
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
        public void CmdPowerBund2(int value)
        {
            byte buf = 0;
            if (_intfBUK.TelemetryNotify.IsBundLineA)
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

            if (_intfBUK.TelemetryNotify.IsBundLineB)
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
        public void CmdSpacewire2Control(int value)
        {            
            SendToUSB(Spacewire2ControlAddr, new byte[1] { (byte)value });
        }

        /// <summary>
        /// Команда SpaceWire2: Управление SPTP.
        /// </summary>
        /// <param name="value">Параметры управления SPTP.</param>
        public void CmdSpacewire2SPTPControl(int value)
        {
            SendToUSB(SpaceWire2SPTPControlAddr, new byte[1] { (byte)value }); 
        }

        /// <summary>
        /// Команда SpaceWire1: Управление.
        /// </summary>
        /// <param name="value">Параметры управления.</param>
        public void CmdSpacewire1Control(int value)
        {
            if (_intfBUK.Spacewire4Notify.IsIntfOn)
            {
                SendToUSB(Spacewire4ControlAddr, new byte[1] { 0 });
            }

            SendToUSB(SelectSpacewireControlAddr, new byte[1] { 0 });
            SendToUSB(Spacewire1ControlAddr, new byte[1] { (byte)value });            
        }

        /// <summary>
        /// Команда SpaceWire1: Управление SPTP.
        /// </summary>
        /// <param name="value">Параметры управления SPTP.</param>
        public void CmdSpacewire1ControlSPTP(int value)
        {
            SendToUSB(Spacewire1SPTPControlAddr, new byte[1] { (byte)value });
        }

        /// <summary>
        /// Команда SpaceWire1: Управление SPTP: Счетчик миллисекунд для НП1 (через сколько готовы данные).
        /// </summary>
        /// <param name="value">Счетчик миллисекунд для НП1 (через сколько готовы данные).</param>
        public void CmdSpacewire1SPTPControlSD1SendTime(int value)
        {
            SendToUSB(Spacewire1SPTPControlSD1SendTimeLoAddr, new byte[1] { (byte)value });
            SendToUSB(Spacewire1SPTPControlSD1SendTimeHiAddr, new byte[1] { (byte)(value >> 8) });
        }

        /// <summary>
        /// Команда SpaceWire1: Управление SPTP: Счетчик миллисекунд для НП2 (через сколько готовы данные).
        /// </summary>
        /// <param name="value">Счетчик миллисекунд для НП2 (через сколько готовы данные).</param>
        public void CmdSpacewire1SPTPControlSD2SendTime(int value)
        {
            SendToUSB(Spacewire1SPTPControlSD2SendTimeLoAddr, new byte[1] { (byte)value });
            SendToUSB(Spacewire1SPTPControlSD2SendTimeHiAddr, new byte[1] { (byte)(value >> 8) });
        }

        /// <summary>
        /// Команда SpaceWire1: Управление SPTP: Кол-во байт в пакете НП1.
        /// </summary>
        /// <param name="value">Кол-во байт в пакете НП1.</param>
        public void CmdSpacewire1SPTPControlSD1DataSize(int value)
        {
            SendToUSB(Spacewire1SPTPControlSD1DataSizeLoAddr, new byte[1] { (byte)value });
            SendToUSB(Spacewire1SPTPControlSD1DataSizeHiAddr, new byte[1] { (byte)(value >> 8) });
        }

        /// <summary>
        /// Команда SpaceWire1: Управление SPTP: Кол-во байт в пакете НП2.
        /// </summary>
        /// <param name="value">Кол-во байт в пакете НП2.</param>
        public void CmdSpacewire1SPTPControlSD2DataSize(int value)
        {
            SendToUSB(Spacewire1SPTPControlSD2DataSizeLoAddr, new byte[1] { (byte)value });
            SendToUSB(Spacewire1SPTPControlSD2DataSizeHiAddr, new byte[1] { (byte)(value >> 8) });
        }

        /// <summary>
        /// Команда SpaceWire1: Запись данных(до 1 Кбайт).
        /// </summary>
        /// <param name="value">Данные для записи.</param>
        public void CmdSpacewire1Record(int value)
        {                       
            if ((null != _intfBUK.Spacewire1Notify.Data) && (0 < _intfBUK.Spacewire1Notify.Data.Length))
            {
                SendToUSB(Spacewire1RecordFlushAddr, new byte[1] { 1 });
                SendToUSB(Spacewire1RecordDataAddr, _intfBUK.Spacewire1Notify.Data);
                SendToUSB(Spacewire1RecordSendAddr, new byte[1] { (byte)value });
            }            
        }

        /// <summary>
        /// Команда SpaceWire2: Запись данных(до 1 Кбайт).
        /// </summary>
        /// <param name="value">Данные для записи.</param>
        public void CmdSpacewire2Record(int value)
        {          
            if ((null != _intfBUK.Spacewire2Notify.Data) && (0 < _intfBUK.Spacewire2Notify.Data.Length))
            {
                SendToUSB(Spacewire2RecordFlushAddr, new byte[1] { 1 });
                SendToUSB(Spacewire2RecordDataAddr, _intfBUK.Spacewire2Notify.Data);
                SendToUSB(Spacewire2RecordSendAddr, new byte[1] { (byte)value });
            }            
        }

        /// <summary>
        /// Команда SpaceWire4: Управление.
        /// </summary>
        /// <param name="value">Параметры управления.</param>
        public void CmdSpacewire4Control(int value)
        {
            if (_intfBUK.Spacewire1Notify.IsIntfOn)
            {
                SendToUSB(Spacewire1ControlAddr, new byte[1] { 0 });
            }

            SendToUSB(SelectSpacewireControlAddr, new byte[1] { 1 });
            SendToUSB(Spacewire4ControlAddr, new byte[1] { (byte)value });
        }

        /// <summary>
        /// Команда SpaceWire4: Запись данных(до 1 Кбайт).
        /// </summary>
        /// <param name="value">Данные для записи.</param>
        public void CmdSpacewire4Record(int value)
        {
            if ((null != _intfBUK.Spacewire4Notify.Data) && (0 < _intfBUK.Spacewire4Notify.Data.Length))
            {
                SendToUSB(Spacewire4RecordFlushAddr, new byte[1] { 1 });
                SendToUSB(Spacewire4RecordDataAddr, _intfBUK.Spacewire4Notify.Data);
                SendToUSB(Spacewire4RecordSendAddr, new byte[1] { (byte)value });
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

        /// <summary>
        /// Команда SpaceWire2: Адрес БС.
        /// </summary>
        /// <param name="value">Логический адрес БС.</param>
        public void CmdSpacewire2LogicBuk(int value)
        {
            SendToUSB(Spacewire2LogicBukAddr, new byte[1] { (byte)value });
        }

        /// <summary>
        /// Команда SpaceWire2: Адрес ИМИТАТОРА БУСКа.
        /// </summary>
        /// <param name="value">Логический адрес БУСК.</param>
        public void CmdSpacewire2LogicBusk(int value)
        {
            SendToUSB(Spacewire2LogicBuskAddr, new byte[1] { (byte)value });
        }

        /// <summary>
        /// Команда SpaceWire1: Адрес НП1.
        /// </summary>
        /// <param name="value">Логический адрес НП1.</param>
        public void CmdSpacewire1LogicSD1(int value)
        {
            SendToUSB(Spacewire1LogicSD1Addr, new byte[1] { (byte)value });
        }

        /// <summary>
        /// Команда SpaceWire1: Адрес БУСК.
        /// </summary>
        /// <param name="value">Логический адрес БУСК.</param>
        public void CmdSpacewire1LogicBusk(int value)
        {
            SendToUSB(Spacewire1LogicBuskAddr, new byte[1] { (byte)value });
        }

        /// <summary>
        /// Команда установки логических адресов.
        /// </summary>
        public void CmdSetDeviceLogicAddr()
        {
            if (_intfBUK.Spacewire2Notify.IsBUK1BM1Channel || _intfBUK.Spacewire2Notify.IsBUK1BM2Channel)
            {
                _intfBUK.Spacewire2Notify.LogicBuk = Global.LogicAddrBuk1;
                _intfBUK.Spacewire1Notify.LogicSD1 = Global.LogicAddrBuk1;  
            }
            else            
            {
                _intfBUK.Spacewire2Notify.LogicBuk = Global.LogicAddrBuk2;
                _intfBUK.Spacewire1Notify.LogicSD1 = Global.LogicAddrBuk2;  
            }

            if (_intfBUK.Spacewire2Notify.IsBUK1BM1Channel || _intfBUK.Spacewire2Notify.IsBUK2BM1Channel)
            {
                _intfBUK.Spacewire2Notify.LogicBusk = Global.LogicAddrBusk1;
                _intfBUK.Spacewire1Notify.LogicBusk = Global.LogicAddrBusk1;  
            }
            else
            {
                _intfBUK.Spacewire2Notify.LogicBusk = Global.LogicAddrBusk2;
                _intfBUK.Spacewire1Notify.LogicBusk = Global.LogicAddrBusk2;  
            }
        }
    }

    /// <summary>
    /// Общий экземпляр, позволяющий управлять прибором (принимать данные, выдавать команды).
    /// </summary>
    public class EgseBukNotify : INotifyPropertyChanged, IDataErrorInfo
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
        /// Экземпляр декодера протокола USB.
        /// </summary>
        private ProtocolUSB7C6E _decoderUSB;

        /// <summary>
        /// Экземпляр декодера протокола Spacewire.
        /// </summary>
        private ProtocolSpacewire _decoderSpacewireBusk;

        /// <summary>
        /// Экземпляр декодера протокола Spacewire.
        /// </summary>
        private ProtocolSpacewire _decoderSpacewireBuk;

        /// <summary>
        /// Текущее состояние подключения устройства.
        /// </summary>
        private bool _isConnected;

        /// <summary>
        /// Записывать данные от прибора в файл.
        /// </summary>
        private bool _isWriteDevDataToFile;

        /// <summary>
        /// Экземпляр класса, представляющий файл для записи данных от прибора.
        /// </summary>
        private FileStream _devDataLogStream;

        /// <summary>
        /// Отображать ли окно имитатора spacewire.
        /// </summary>
        private bool _isShowSimSpacewire;

        /// <summary>
        /// Отображать ли окно имитатора БМ-4.
        /// </summary>
        private bool _isShowSimRouter;

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="EgseBukNotify" />.
        /// </summary>
        public EgseBukNotify()
        {
            IsConnected = false;

            _decoderUSB = new ProtocolUSB7C6E(null, LogsClass.LogUSB, false, true);
            _decoderUSB.GotProtocolMsg += new ProtocolUSBBase.ProtocolMsgEventHandler(OnMessageFunc);
            _decoderUSB.GotProtocolError += new ProtocolUSBBase.ProtocolErrorEventHandler(OnErrorFunc);
            DeviceTime = new EgseTime();

            Device = new EgseBuk(Global.DeviceSerial, _decoderUSB, this);
            Device.ChangeStateEvent = OnChangeConnection;

            ControlValuesList = new Dictionary<string, ControlValue>();

            ControlValuesList.Add(Global.Power, new ControlValue());
            TelemetryNotify = new Telemetry(this);
            Spacewire1Notify = new Spacewire1(this);
            Spacewire2Notify = new Spacewire2(this);
            Spacewire3Notify = new Spacewire3(this);
            Spacewire4Notify = new Spacewire4(this);
            
            _decoderSpacewireBusk = new ProtocolSpacewire((uint)Spacewire2Addr.Data, (uint)Spacewire2Addr.End, (uint)Spacewire2Addr.Time1, (uint)Spacewire2Addr.Time2);
            _decoderSpacewireBusk.GotSpacewireMsg += new ProtocolSpacewire.SpacewireMsgEventHandler(OnSpacewire2Msg);
            _decoderUSB.GotProtocolMsg += new ProtocolUSBBase.ProtocolMsgEventHandler(_decoderSpacewireBusk.OnMessageFunc);

            _decoderSpacewireBuk = new ProtocolSpacewire((uint)Spacewire2Addr.BukData, (uint)Spacewire2Addr.BukEnd, (uint)Spacewire2Addr.BukTime1, (uint)Spacewire2Addr.BukTime2);
            _decoderSpacewireBuk.GotSpacewireMsg += new ProtocolSpacewire.SpacewireMsgEventHandler(OnSpacewire2Msg);
            _decoderUSB.GotProtocolMsg += new ProtocolUSBBase.ProtocolMsgEventHandler(_decoderSpacewireBuk.OnMessageFunc);  

            _devDataLogStream = null;
            _isWriteDevDataToFile = false;
            
            Tele = new TelemetryBUK();
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
        /// Получает или задает нотификатор телеметрии.
        /// </summary>
        /// <value>
        /// Экземпляр нотификатора.
        /// </value>
        public Telemetry TelemetryNotify { get; set; }

        /// <summary>
        /// Получает или задает нотификатор spacewire1.
        /// </summary>
        /// <value>
        /// Экземпляр нотификатора.
        /// </value>
        public Spacewire1 Spacewire1Notify { get; set; }

        /// <summary>
        /// Получает или задает нотификатор spacewire2.
        /// </summary>
        /// <value>
        /// Экземпляр нотификатора.
        /// </value>
        public Spacewire2 Spacewire2Notify { get; set; }

        /// <summary>
        /// Получает или задает нотификатор spacewire3.
        /// </summary>
        /// <value>
        /// Экземпляр нотификатора.
        /// </value>
        public Spacewire3 Spacewire3Notify { get; set; }

        /// <summary>
        /// Получает или задает нотификатор spacewire4.
        /// </summary>
        /// <value>
        /// Экземпляр нотификатора.
        /// </value>
        public Spacewire4 Spacewire4Notify { get; set; }

        /// <summary>
        /// Получает или задает доступ к USB прибора.
        /// </summary>
        public EgseBuk Device { get; set; }

        /// <summary>
        /// Получает значение, показывающее, [подключен] ли прибор.
        /// </summary>
        /// <value>
        ///   <c>true</c> если [подключен]; иначе, <c>false</c>.
        /// </value>
        public bool IsConnected
        {
            get
            {
                return _isConnected;
            }

            private set
            {
                _isConnected = value;
                FirePropertyChangedEvent("IsConnected");
            }
        }                       

        /// <summary>
        /// Получает значение, показывающее, видно ли [окно имитатора spacewire].
        /// </summary>
        /// <value>
        ///   <c>true</c> если [окно имитатора spacewire] видно; иначе, <c>false</c>.
        /// </value>
        public bool IsShowSimSpacewire
        {
            get
            {
                return _isShowSimSpacewire;
            }

            private set
            {
                _isShowSimSpacewire = value;
                FirePropertyChangedEvent("IsShowSimSpacewire");
            }
        }

        /// <summary>
        /// Получает значение, показывающее, видно ли [окно имитатора БМ-4].
        /// </summary>
        /// <value>
        ///   <c>true</c> если [окно имитатора БМ-4] видно; иначе, <c>false</c>.
        /// </value>
        public bool IsShowSimRouter
        {
            get
            {
                return _isShowSimRouter;
            }

            private set
            {
                _isShowSimRouter = value;
                FirePropertyChangedEvent("IsShowSimRouter");
            }
        }

        /// <summary>
        /// Получает заголовок окна имитатора БМ-4.
        /// </summary>
        /// <value>
        /// Заголовок окна.
        /// </value>
        public string Caption
        {
            get
            {
                return Spacewire2Notify.ShowLogicBusk;
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
        public Dictionary<string, ControlValue> ControlValuesList { get; set; }

        /// <summary>
        /// Получает сообщение об ошибке в объекте.
        /// </summary>
        /// <returns>An error message indicating what is wrong with this object. The default is an empty string ("").</returns>
        public string Error
        {
            get
            {
                return null;
            }
        }

        /// <summary>
        /// Gets the <see cref="System.String"/> with the specified name.
        /// </summary>
        /// <value>
        /// The <see cref="System.String"/>.
        /// </value>
        /// <param name="name">The name.</param>
        /// <returns>Сообщение об ошибке.</returns>
        public string this[string name]
        {
            get
            {
                string result = null;

                return result;
            }
        }

        /// <summary>
        /// Для каждого элемента управления тикаем временем.
        /// </summary>
        public void TickAllControlsValues()
        {
            Debug.Assert(ControlValuesList != null, "ControlValuesList не должны быть равны null!");
            foreach (var cv in ControlValuesList)
            {
                (cv.Value as ControlValue).TimerTick(); 
            }
        }

        /// <summary>
        /// Метод вызывается, когда прибор подсоединяется или отсоединяется.
        /// </summary>
        /// <param name="isConnected">Если установлено <c>true</c> [прибор подключен].</param>
        public void OnChangeConnection(bool isConnected)
        {
            IsConnected = isConnected;
            if (IsConnected)
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
            foreach (var cv in ControlValuesList)
            {
               (cv.Value as ControlValue).RefreshGetValue();
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
                        ControlValuesList[Global.Spacewire2.Control].UsbValue = msg.Data[7];
                        ControlValuesList[Global.Spacewire2.Record].UsbValue = msg.Data[10]; 
                        ControlValuesList[Global.Spacewire2.SPTPLogicBusk].UsbValue = msg.Data[11];
                        ControlValuesList[Global.Spacewire2.SPTPLogicBuk].UsbValue = msg.Data[12];
                        ControlValuesList[Global.Spacewire2.SPTPLogicBkp].UsbValue = msg.Data[13];
                        ControlValuesList[Global.Spacewire2.SPTPControl].UsbValue = msg.Data[14];                       
                        ControlValuesList[Global.Spacewire1.Control].UsbValue = msg.Data[17];
                        ControlValuesList[Global.Spacewire1.Record].UsbValue = msg.Data[20];
                        ControlValuesList[Global.Spacewire1.SPTPControl].UsbValue = msg.Data[21];
                        ControlValuesList[Global.Spacewire1.SPTPLogicBusk].UsbValue = msg.Data[22];
                        ControlValuesList[Global.Spacewire1.SPTPLogicSD1].UsbValue = msg.Data[23];
                        ControlValuesList[Global.Spacewire1.SPTPLogicSD2].UsbValue = msg.Data[24];
                        ControlValuesList[Global.Spacewire1.SD1SendTime].UsbValue = (msg.Data[26] << 8) | msg.Data[25];
                        ControlValuesList[Global.Spacewire1.SD2SendTime].UsbValue = (msg.Data[28] << 8) | msg.Data[27];
                        ControlValuesList[Global.Spacewire1.SD1DataSize].UsbValue = (msg.Data[30] << 8) | msg.Data[29]; // XXX
                        ControlValuesList[Global.Spacewire1.SD2DataSize].UsbValue = (msg.Data[32] << 8) | msg.Data[31]; // XXX
                        ControlValuesList[Global.Spacewire4.Control].UsbValue = msg.Data[29];
                        ControlValuesList[Global.Spacewire4.Record].UsbValue = msg.Data[32];
                        break;
                    case TeleDataAddr:
                        Tele.Update(msg.Data);
                        ControlValuesList[Global.Power].UsbValue = msg.Data[3];
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

        /// <summary>
        /// Прототип подкласса нотификатора.
        /// </summary>
        public class SubNotify : INotifyPropertyChanged
        {
            /// <summary>
            /// Инициализирует новый экземпляр класса <see cref="SubNotify" />.
            /// </summary>
            /// <param name="owner">The owner.</param>
            public SubNotify(EgseBukNotify owner)
            {
                Owner = owner;
                ControlValuesList = Owner.ControlValuesList;
                Device = Owner.Device;
                InitControlValue();
                InitProperties();
            }

            /// <summary>
            /// Occurs when a property value changes.
            /// </summary>
            public event PropertyChangedEventHandler PropertyChanged;

            /// <summary>
            /// Получает доступ к интерфейсу устройства. 
            /// </summary>
            protected EgseBukNotify Owner { get; private set; }

            /// <summary>
            /// Получает доступ к USB прибора.
            /// </summary>
            protected EgseBuk Device { get; private set; }

            /// <summary>
            /// Получает список управляющих элементов.
            /// </summary>
            protected Dictionary<string, ControlValue> ControlValuesList { get; private set; }

            /// <summary>
            /// Fires the property changed event.
            /// </summary>
            /// <param name="propertyName">Name of the property.</param>
            protected void FirePropertyChangedEvent(string propertyName)
            {
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
                }
            }

            /// <summary>
            /// Initializes the control value.
            /// </summary>
            protected virtual void InitControlValue()
            {
            }

            /// <summary>
            /// Initializes the properties.
            /// </summary>
            protected virtual void InitProperties()
            {
            }
        }

        /// <summary>
        /// Нотификатор телеметрии.
        /// </summary>
        public class Telemetry : SubNotify, IDataErrorInfo
        {
            /// <summary>
            /// Передавать релейные команды БУСК по линии A.
            /// </summary>
            private bool _isBuskLineA = true;

            /// <summary>
            /// Передавать релейные команды БУСК по линии B.
            /// </summary>
            private bool _isBuskLineB = true;

            /// <summary>
            /// Передавать релейные команды БУНД по линии A.
            /// </summary>
            private bool _isBundLineA = true;

            /// <summary>
            /// Передавать релейные команды БУНД по линии B.
            /// </summary>
            private bool _isBundLineB = true;

            /// <summary>
            /// Телеметрия: Запитан ПК1 от БУСК.
            /// </summary>
            private bool _powerBusk1;

            /// <summary>
            /// Телеметрия: Запитан ПК2 от БУСК.
            /// </summary>
            private bool _powerBusk2;

            /// <summary>
            /// Телеметрия: Запитан ПК1 от БУНД.
            /// </summary>
            private bool _powerBund1;

            /// <summary>
            /// Телеметрия: Запитан ПК2 от БУНД.
            /// </summary>
            private bool _powerBund2;

            /// <summary>
            /// Инициализирует новый экземпляр класса <see cref="Telemetry" />.
            /// </summary>
            /// <param name="owner">The owner.</param>
            public Telemetry(EgseBukNotify owner)
                : base(owner)
            {
            }

            /// <summary>
            /// Получает значение, показывающее, нужно ли [передавать релейную команду] БУСК по линии A.
            /// </summary>
            /// <value>
            ///   <c>true</c> если [передавать релейную команду]; иначе, <c>false</c>.
            /// </value>
            public bool IsBuskLineA
            {
                get
                {
                    return _isBuskLineA;
                }

                private set
                {
                    _isBuskLineA = value;
                    FirePropertyChangedEvent("IsBuskLineA");
                }
            }

            /// <summary>
            /// Получает значение, показывающее, нужно ли [передавать релейную команду] БУСК по линии B.
            /// </summary>
            /// <value>
            ///   <c>true</c> если [передавать релейную команду]; иначе, <c>false</c>.
            /// </value>
            public bool IsBuskLineB
            {
                get
                {
                    return _isBuskLineB;
                }

                private set
                {
                    _isBuskLineB = value;
                    FirePropertyChangedEvent("IsBuskLineB");
                }
            }

            /// <summary>
            /// Получает значение, показывающее, нужно ли [передавать релейную команду] БУНД по линии A.
            /// </summary>
            /// <value>
            ///   <c>true</c> если [передавать релейную команду]; иначе, <c>false</c>.
            /// </value>
            public bool IsBundLineA
            {
                get
                {
                    return _isBundLineA;
                }

                private set
                {
                    _isBundLineA = value;
                    FirePropertyChangedEvent("IsBundLineA");
                }
            }

            /// <summary>
            /// Получает значение, показывающее, нужно ли [передавать релейную команду] БУНД по линии B.
            /// </summary>
            /// <value>
            ///   <c>true</c> если [передавать релейную команду]; иначе, <c>false</c>.
            /// </value>
            public bool IsBundLineB
            {
                get
                {
                    return _isBundLineB;
                }

                private set
                {
                    _isBundLineB = value;
                    FirePropertyChangedEvent("IsBundLineB");
                }
            }

            /// <summary>
            /// Получает значение, показывающее, есть ли [питание первого полукомплекта БУСК].
            /// </summary>
            /// <value>
            ///   <c>true</c> если [питание первого полукомплекта БУСК]; иначе, <c>false</c>.
            /// </value>
            public bool PowerBusk1
            {
                get
                {
                    return _powerBusk1;
                }

                private set
                {
                    _powerBusk1 = value;
                    FirePropertyChangedEvent("PowerBusk1");
                }
            }

            /// <summary>
            /// Получает значение, показывающее, есть ли [питание второго полукомплекта БУСК].
            /// </summary>
            /// <value>
            ///   <c>true</c> если [питание второго полукомплекта БУСК]; иначе, <c>false</c>.
            /// </value>
            public bool PowerBusk2
            {
                get
                {
                    return _powerBusk2;
                }

                private set
                {
                    _powerBusk2 = value;
                    FirePropertyChangedEvent("PowerBusk2");
                }
            }

            /// <summary>
            /// Получает значение, показывающее, есть ли [питание первого полукомплекта БУНД].
            /// </summary>
            /// <value>
            ///   <c>true</c> если [питание первого полукомплекта БУНД]; иначе, <c>false</c>.
            /// </value>
            public bool PowerBund1
            {
                get
                {
                    return _powerBund1;
                }

                private set
                {
                    _powerBund1 = value;
                    FirePropertyChangedEvent("PowerBund1");
                }
            }

            /// <summary>
            /// Получает значение, показывающее, есть ли [питание второго полукомплекта БУНД].
            /// </summary>
            /// <value>
            ///   <c>true</c> если [питание второго полукомплекта БУНД]; иначе, <c>false</c>.
            /// </value>
            public bool PowerBund2
            {
                get
                {
                    return _powerBund2;
                }

                private set
                {
                    _powerBund2 = value;
                    FirePropertyChangedEvent("PowerBund2");
                }
            }

            /// <summary>
            /// Получает сообщение об ошибке в объекте.
            /// </summary>
            /// <returns>An error message indicating what is wrong with this object. The default is an empty string ("").</returns>
            public string Error
            {
                get
                {
                    return null;
                }
            }

            /// <summary>
            /// Gets the <see cref="System.String"/> with the specified name.
            /// </summary>
            /// <value>
            /// The <see cref="System.String"/>.
            /// </value>
            /// <param name="name">The name.</param>
            /// <returns>Сообщение об ошибке.</returns>
            public string this[string name]
            {
                get
                {
                    string result = null;

                    return result;
                }
            }

            /// <summary>
            /// Initializes the control value.
            /// </summary>
            protected override void InitControlValue()
            {
                ControlValuesList.Add(Global.Telemetry, new ControlValue());
            }

            /// <summary>
            /// Initializes the properties.
            /// </summary>
            protected override void InitProperties()
            {
                ControlValuesList[Global.Telemetry].AddProperty(Global.Telemetry.PowerBusk1, 7, 1, Device.CmdPowerBusk1, value => PowerBusk1 = 1 == value);
                ControlValuesList[Global.Telemetry].AddProperty(Global.Telemetry.PowerBusk2, 6, 1, Device.CmdPowerBusk2, value => PowerBusk2 = 1 == value);
                ControlValuesList[Global.Telemetry].AddProperty(Global.Telemetry.PowerBund1, 4, 1, Device.CmdPowerBund1, value => PowerBund1 = 1 == value);
                ControlValuesList[Global.Telemetry].AddProperty(Global.Telemetry.PowerBund2, 5, 1, Device.CmdPowerBund2, value => PowerBund2 = 1 == value);
            }
        }

        /// <summary>
        /// Нотификатор spacewire1.
        /// </summary>
        public class Spacewire1 : SubNotify, IDataErrorInfo
        {
            /// <summary>
            /// Управление: вкл/выкл интерфейса Spacewire.
            /// </summary>
            private bool _isIntfOn;

            /// <summary>
            /// Управление: Установлена связь.
            /// </summary>
            private bool _isConnected;

            /// <summary>
            /// SPTP: включение обмена прибора НП1.
            /// </summary>
            private bool _isNP1Trans;

            /// <summary>
            /// SPTP: включение обмена прибора НП2.
            /// </summary>
            private bool _isNP2Trans;

            /// <summary>
            /// SPTP: можно выдавать пакет в НП1.
            /// </summary>
            private bool _isNP1TransData;

            /// <summary>
            /// SPTP: можно выдавать пакет в НП2.
            /// </summary>
            private bool _isSD2TransData;

            /// <summary>
            /// SPTP: Адрес БУСК.
            /// </summary>
            private int _logicBusk;

            /// <summary>
            /// SPTP: Адрес НП1.
            /// </summary>
            private int _logicSD1;

            /// <summary>
            /// SPTP: Адрес НП2.
            /// </summary>
            private int _logicSD2;

            /// <summary>
            /// SPTP: Счетчик миллисекунд для НП1 (через сколько готовы данные).
            /// </summary>
            private int _sd1SendTime = 0;

            /// <summary>
            /// SPTP: Счетчик миллисекунд для НП2 (через сколько готовы данные).
            /// </summary>
            private int _sd2SendTime = 0;

            /// <summary>
            /// SPTP: Кол-во байт в пакете НП1.
            /// </summary>
            private int _sd1DataSize = 0;

            /// <summary>
            /// SPTP: Кол-во байт в пакете НП2.
            /// </summary>
            private int _sd2DataSize = 0;

            /// <summary>
            /// Запись данных(до 1 Кбайт): Бит занятости.
            /// </summary>
            private bool _isRecordBusy;

            /// <summary>
            /// Запись данных(до 1 Кбайт): Бит выдачи посылки.
            /// </summary>
            private bool _isRecordSend;

            /// <summary>
            /// Инициализирует новый экземпляр класса <see cref="Spacewire1" />.
            /// </summary>
            /// <param name="owner">The owner.</param>
            public Spacewire1(EgseBukNotify owner)
                : base(owner)
            {
            }

            /// <summary>
            /// Получает или задает буфер данных для передачи в USB.
            /// </summary>
            /// <value>
            /// Буфер данных.
            /// </value>
            public byte[] Data { get; set; }

            /// <summary>
            /// Получает строку представления [логический адрес БУСК].
            /// </summary>
            /// <value>
            /// Строка [логический адрес БУСК].
            /// </value>
            public string ShowLogicBusk
            {
                get
                {
                    return string.Format(Resource.Get(@"stShowSimLogicBusk"), LogicBusk);
                }
            }

            /// <summary>
            /// Получает или задает [логический адрес БУСК].
            /// </summary>
            /// <value>
            /// Значение [логический адрес БУСК].
            /// </value>
            public int LogicBusk
            {
                get
                {
                    return _logicBusk;
                }

                set
                {
                    _logicBusk = value;
                    ControlValuesList[Global.Spacewire1.SPTPLogicBusk].SetProperty(Global.Spacewire1.SPTPLogicBusk, value);
                    FirePropertyChangedEvent("ShowLogicBusk");
                    FirePropertyChangedEvent("LogicBusk");
                }
            }

            /// <summary>
            /// Получает строку представления [логический адрес НП1].
            /// </summary>
            /// <value>
            /// Строка [логический адрес НП1].
            /// </value>
            public string ShowLogicSD1
            {
                get
                {
                    return string.Format(Resource.Get(@"stShowSimLogicSD1"), LogicSD1);
                }
            }

            /// <summary>
            /// Получает или задает [логический адрес НП1].
            /// </summary>
            /// <value>
            /// Значение [логический адрес НП1].
            /// </value>
            public int LogicSD1
            {
                get
                {
                    return _logicSD1;
                }

                set
                {
                    _logicSD1 = value;
                    ControlValuesList[Global.Spacewire1.SPTPLogicSD1].SetProperty(Global.Spacewire1.SPTPLogicSD1, value);
                    FirePropertyChangedEvent("ShowLogicSD1");
                    FirePropertyChangedEvent("LogicSD1");
                }
            }

            /// <summary>
            /// Получает строку представления [логический адрес НП2].
            /// </summary>
            /// <value>
            /// Строка [логический адрес НП2].
            /// </value>
            public string ShowLogicSD2
            {
                get
                {
                    return string.Format(Resource.Get(@"stShowSimLogicSD2"), LogicSD2);
                }
            }

            /// <summary>
            /// Получает или задает [логический адрес НП2].
            /// </summary>
            /// <value>
            /// Значение [логический адрес НП2].
            /// </value>
            public int LogicSD2
            {
                get
                {
                    return _logicSD2;
                }

                set
                {
                    _logicSD2 = value;
                    ControlValuesList[Global.Spacewire1.SPTPLogicSD2].SetProperty(Global.Spacewire1.SPTPLogicSD2, value);
                    FirePropertyChangedEvent("ShowLogicSD2");
                    FirePropertyChangedEvent("LogicSD2");
                }
            }

            /// <summary>
            /// Получает или задает значение, показывающее, что [интерфейс SpaceWire1 включен].
            /// </summary>
            /// <value>
            /// <c>true</c> если [интерфейс SpaceWire1 включен]; иначе, <c>false</c>.
            /// </value>
            public bool IsIntfOn
            {
                get
                {
                    return _isIntfOn;
                }

                set
                {
                    _isIntfOn = value;
                    ControlValuesList[Global.Spacewire1.Control].SetProperty(Global.Spacewire1.Control.IntfOn, Convert.ToInt32(value));
                    FirePropertyChangedEvent("IsIntfOn");
                }
            }

            /// <summary>
            /// Получает или задает значение, показывающее, что [связь по интерфейсу SpaceWire1 установлена].
            /// </summary>
            /// <value>
            /// <c>true</c> если [связь по интерфейсу SpaceWire1 установлена]; иначе, <c>false</c>.
            /// </value>
            public bool IsConnected
            {
                get
                {
                    return _isConnected;
                }

                set
                {
                    _isConnected = value;
                    FirePropertyChangedEvent("IsConnected");
                }
            }

            /// <summary>
            /// Получает или задает значение, показывающее, что [включен обмен для прибора НП1].
            /// </summary>
            /// <value>
            /// <c>true</c> если [включен обмен для прибора НП1]; иначе, <c>false</c>.
            /// </value>
            public bool IsNP1Trans
            {
                get
                {
                    return _isNP1Trans;
                }

                set
                {
                    _isNP1Trans = value;
                    ControlValuesList[Global.Spacewire1.SPTPControl].SetProperty(Global.Spacewire1.SPTPControl.NP1Trans, Convert.ToInt32(value));
                    FirePropertyChangedEvent("IsNP1Trans");
                }
            }

            /// <summary>
            /// Получает или задает значение, показывающее, что [включен обмен для прибора НП2].
            /// </summary>
            /// <value>
            /// <c>true</c> если [включен обмен для прибора НП2]; иначе, <c>false</c>.
            /// </value>
            public bool IsNP2Trans
            {
                get
                {
                    return _isNP2Trans;
                }

                set
                {
                    _isNP2Trans = value;
                    ControlValuesList[Global.Spacewire1.SPTPControl].SetProperty(Global.Spacewire1.SPTPControl.NP2Trans, Convert.ToInt32(value));
                    FirePropertyChangedEvent("IsNP2Trans");
                }
            }

            /// <summary>
            /// Получает или задает значение, показывающее, что [можно выдавать пакеты данных в НП1].
            /// </summary>
            /// <value>
            /// <c>true</c> если [можно выдавать пакеты данных в НП1]; иначе, <c>false</c>.
            /// </value>
            public bool IsNP1TransData
            {
                get
                {
                    return _isNP1TransData;
                }

                set
                {
                    _isNP1TransData = value;
                    ControlValuesList[Global.Spacewire1.SPTPControl].SetProperty(Global.Spacewire1.SPTPControl.NP1TransData, Convert.ToInt32(value));
                    FirePropertyChangedEvent("IsNP1TransData");
                }
            }

            /// <summary>
            /// Получает или задает значение, показывающее, что [можно выдавать пакеты данных в НП2].
            /// </summary>
            /// <value>
            /// <c>true</c> если [можно выдавать пакеты данных в НП2]; иначе, <c>false</c>.
            /// </value>
            public bool IsSD2TransData
            {
                get
                {
                    return _isSD2TransData;
                }

                set
                {
                    _isSD2TransData = value;
                    ControlValuesList[Global.Spacewire1.SPTPControl].SetProperty(Global.Spacewire1.SPTPControl.SD2TransData, Convert.ToInt32(value));
                    FirePropertyChangedEvent("IsSD2TransData");
                }
            }

            /// <summary>
            /// Получает или задает значение [Счетчик миллисекунд для НП1 (через сколько готовы данные)].
            /// </summary>
            /// <value>
            /// Счетчик миллисекунд для НП1 (через сколько готовы данные).
            /// </value>
            public int SD1SendTime
            {
                get
                {
                    return _sd1SendTime;
                }

                set
                {
                    _sd1SendTime = value;
                    ControlValuesList[Global.Spacewire1.SD1SendTime].SetProperty(Global.Spacewire1.SD1SendTime, Convert.ToInt32(value));
                    FirePropertyChangedEvent("SD1SendTime");
                }
            }

            /// <summary>
            /// Получает или задает значение [Счетчик миллисекунд для НП2 (через сколько готовы данные)].
            /// </summary>
            /// <value>
            /// Счетчик миллисекунд для НП2 (через сколько готовы данные).
            /// </value>
            public int SD2SendTime
            {
                get
                {
                    return _sd2SendTime;
                }

                set
                {
                    _sd2SendTime = value;
                    ControlValuesList[Global.Spacewire1.SD2SendTime].SetProperty(Global.Spacewire1.SD2SendTime, Convert.ToInt32(value));
                    FirePropertyChangedEvent("SD2SendTime");
                }
            }

            /// <summary>
            /// Получает или задает значение [Кол-во байт в пакете НП1].
            /// </summary>
            /// <value>
            /// Кол-во байт в пакете НП1.
            /// </value>
            public int SD1DataSize
            {
                get
                {
                    return _sd1DataSize;
                }

                set
                {
                    _sd1DataSize = value;
                    ControlValuesList[Global.Spacewire1.SD1DataSize].SetProperty(Global.Spacewire1.SD1DataSize, Convert.ToInt32(value));
                    FirePropertyChangedEvent("SD1DataSize");
                }
            }

            /// <summary>
            /// Получает или задает значение [Кол-во байт в пакете НП2].
            /// </summary>
            /// <value>
            /// Кол-во байт в пакете НП2.
            /// </value>
            public int SD2DataSize
            {
                get
                {
                    return _sd2DataSize;
                }

                set
                {
                    _sd2DataSize = value;
                    ControlValuesList[Global.Spacewire1.SD2DataSize].SetProperty(Global.Spacewire1.SD2DataSize, Convert.ToInt32(value));
                    FirePropertyChangedEvent("SD2DataSize");
                }
            }

            /// <summary>
            /// Получает или задает значение, показывающее, что [Бит занятости записи - 1].
            /// </summary>
            /// <value>
            ///   <c>true</c> если [Бит занятости записи - 1]; иначе, <c>false</c>.
            /// </value>
            public bool IsRecordBusy
            {
                get
                {
                    return _isRecordBusy || IsRecordSend;
                }

                set
                {
                    _isRecordBusy = value;
                    FirePropertyChangedEvent("IsRecordBusy");
                }
            }

            /// <summary>
            /// Получает или задает значение, показывающее, что [Бит выдачи посылки - 1].
            /// </summary>
            /// <value>
            ///   <c>true</c> если [Бит выдачи посылки - 1]; иначе, <c>false</c>.
            /// </value>
            public bool IsRecordSend
            {
                get
                {
                    return _isRecordSend;
                }

                set
                {
                    _isRecordSend = value;
                    ControlValuesList[Global.Spacewire1.Record].SetProperty(Global.Spacewire1.Record.Send, Convert.ToInt32(value));
                    FirePropertyChangedEvent("IsRecordSend");
                }
            }

            /// <summary>
            /// Получает сообщение об ошибке в объекте.
            /// </summary>
            /// <returns>An error message indicating what is wrong with this object. The default is an empty string ("").</returns>
            public string Error
            {
                get
                {
                    return null;
                }
            }

            /// <summary>
            /// Gets the <see cref="System.String"/> with the specified name.
            /// </summary>
            /// <value>
            /// The <see cref="System.String"/>.
            /// </value>
            /// <param name="name">The name.</param>
            /// <returns>Сообщение об ошибке.</returns>
            public string this[string name]
            {
                get
                {
                    string result = null;

                    if (name == "Data")
                    {
                        if ((null != Data) && (0 == Data.Length))
                        {
                            result = "Некорректный ввод данных! Повторите ввод.";
                        }
                    }

                    return result;
                }
            }

            /// <summary>
            /// Initializes the control value.
            /// </summary>
            protected override void InitControlValue()
            {
                ControlValuesList.Add(Global.Spacewire1.Control, new ControlValue());
                ControlValuesList.Add(Global.Spacewire1.Record, new ControlValue());
                ControlValuesList.Add(Global.Spacewire1.SPTPControl, new ControlValue());
                ControlValuesList.Add(Global.Spacewire1.SPTPLogicBusk, new ControlValue());
                ControlValuesList.Add(Global.Spacewire1.SPTPLogicSD1, new ControlValue());
                ControlValuesList.Add(Global.Spacewire1.SPTPLogicSD2, new ControlValue());
                ControlValuesList.Add(Global.Spacewire1.SD1DataSize, new ControlValue());
                ControlValuesList.Add(Global.Spacewire1.SD2DataSize, new ControlValue());
                ControlValuesList.Add(Global.Spacewire1.SD1SendTime, new ControlValue());
                ControlValuesList.Add(Global.Spacewire1.SD2SendTime, new ControlValue());
            }

            /// <summary>
            /// Initializes the properties.
            /// </summary>
            protected override void InitProperties()
            {
                ControlValuesList[Global.Spacewire1.Control].AddProperty(Global.Spacewire1.Control.IntfOn, 0, 1, Device.CmdSpacewire1Control, value => IsIntfOn = 1 == value);
                ControlValuesList[Global.Spacewire1.Control].AddProperty(Global.Spacewire1.Control.Connected, 3, 1, delegate { }, value => IsConnected = 1 == value);
                ControlValuesList[Global.Spacewire1.SPTPLogicBusk].AddProperty(Global.Spacewire1.SPTPLogicBusk, 0, 8, Device.CmdSpacewire1LogicBusk, value => LogicBusk = value);
                ControlValuesList[Global.Spacewire1.SPTPLogicSD1].AddProperty(Global.Spacewire1.SPTPLogicSD1, 0, 8, Device.CmdSpacewire1LogicSD1, value => LogicSD1 = value);
                ControlValuesList[Global.Spacewire1.SPTPLogicSD2].AddProperty(Global.Spacewire1.SPTPLogicSD2, 0, 8, delegate { }, value => LogicSD2 = value);
                ControlValuesList[Global.Spacewire1.SPTPControl].AddProperty(Global.Spacewire1.SPTPControl.NP1Trans, 0, 1, Device.CmdSpacewire1ControlSPTP, value => IsNP1Trans = 1 == value);
                ControlValuesList[Global.Spacewire1.SPTPControl].AddProperty(Global.Spacewire1.SPTPControl.NP2Trans, 2, 1, Device.CmdSpacewire1ControlSPTP, value => IsNP2Trans = 1 == value);
                ControlValuesList[Global.Spacewire1.SPTPControl].AddProperty(Global.Spacewire1.SPTPControl.NP1TransData, 1, 1, Device.CmdSpacewire1ControlSPTP, value => IsNP1TransData = 1 == value);
                ControlValuesList[Global.Spacewire1.SPTPControl].AddProperty(Global.Spacewire1.SPTPControl.SD2TransData, 3, 1, Device.CmdSpacewire1ControlSPTP, value => IsSD2TransData = 1 == value);
                ControlValuesList[Global.Spacewire1.SD1SendTime].AddProperty(Global.Spacewire1.SD1SendTime, 0, 16, Device.CmdSpacewire1SPTPControlSD1SendTime, value => SD1SendTime = value);
                ControlValuesList[Global.Spacewire1.SD2SendTime].AddProperty(Global.Spacewire1.SD2SendTime, 0, 16, Device.CmdSpacewire1SPTPControlSD2SendTime, value => SD2SendTime = value);
                ControlValuesList[Global.Spacewire1.SD1DataSize].AddProperty(Global.Spacewire1.SD1DataSize, 0, 16, Device.CmdSpacewire1SPTPControlSD1DataSize, value => SD1DataSize = value);
                ControlValuesList[Global.Spacewire1.SD2DataSize].AddProperty(Global.Spacewire1.SD2DataSize, 0, 16, Device.CmdSpacewire1SPTPControlSD2DataSize, value => SD2DataSize = value);
                ControlValuesList[Global.Spacewire1.Record].AddProperty(Global.Spacewire1.Record.Busy, 3, 1, delegate { }, value => IsRecordBusy = 1 == value);
                ControlValuesList[Global.Spacewire1.Record].AddProperty(Global.Spacewire1.Record.Send, 0, 1, Device.CmdSpacewire1Record, value => IsRecordSend = 1 == value);
            }
        }

        /// <summary>
        /// Нотификатор spacewire2.
        /// </summary>
        public class Spacewire2 : SubNotify, IDataErrorInfo
        {
            /// <summary>
            /// SPTP: Адрес ИМИТАТОРА БУСКа.
            /// </summary>
            private int _logicBusk;

            /// <summary>
            /// SPTP: Адрес БС.
            /// </summary>
            private int _logicBuk;

            /// <summary>
            /// SPTP: Адрес БКП.
            /// </summary>
            private int _logicBkp;

            /// <summary>
            /// Управление: вкл/выкл интерфейса Spacewire.
            /// </summary>
            private bool _isIntfOn;

            /// <summary>
            /// Управление: Установлена связь.
            /// </summary>
            private bool _isConnected;

            /// <summary>
            /// Запись данных(до 1 Кбайт): Выдача посылки RMAP (самосбр.).
            /// </summary>
            private bool _isSendRMAP;

            /// <summary>
            /// Запись данных(до 1 Кбайт): 1 – выдача посылки в прибор БС (самосбр.).
            /// </summary>
            private bool _isSendBuk;

            /// <summary>
            /// Запись данных(до 1 Кбайт): 1 – выдача посылки в прибор БКП (самосбр.).
            /// </summary>
            private bool _isSendBkp;

            /// <summary>
            /// Управление обменом с приборами по SPTP: включить выдачу секундных меток (1PPS).
            /// </summary>
            private bool _isTimeMark;

            /// <summary>
            /// Управление обменом с приборами по SPTP: включение обмена прибора БС.
            /// </summary>
            private bool _isBukTrans;

            /// <summary>
            /// Управление обменом с приборами по SPTP: включение обмена прибора БКП.
            /// </summary>
            private bool _isBkpTrans;

            /// <summary>
            /// Управление обменом с приборами по SPTP: можно выдавать пакет в БС.
            /// </summary>
            private bool _isBukTransData;

            /// <summary>
            /// Управление обменом с приборами по SPTP: можно выдавать пакет в БКП.
            /// </summary>
            private bool _isBkpTransData;

            /// <summary>
            /// Управление обменом с приборами по SPTP: выдача КБВ прибору БС (только при «1 PPS» == 1).
            /// </summary>
            private bool _isBukKbv;

            /// <summary>
            /// Управление обменом с приборами по SPTP: выдача КБВ прибору БКП (только при «1 PPS» == 1).
            /// </summary>
            private bool _isBkpKbv;

            /// <summary>
            /// Управление: Выбор канала.
            /// </summary>
            private SimRouterChannel _simRouterChannel;

            /// <summary>
            /// Инициализирует новый экземпляр класса <see cref="Spacewire2" />.
            /// </summary>
            /// <param name="owner">The owner.</param>
            public Spacewire2(EgseBukNotify owner)
                : base(owner)
            {
            }

            /// <summary>
            /// Получает или задает буфер данных для передачи в USB.
            /// </summary>
            /// <value>
            /// Буфер данных.
            /// </value>
            public byte[] Data { get; set; }

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
                    ControlValuesList[Global.Spacewire2.Control].SetProperty(Global.Spacewire2.Control.Channel, (int)value);
                    FirePropertyChangedEvent("SelectSimRouterChannel");
                    FirePropertyChangedEvent("IsBUK2BM2Channel");
                    FirePropertyChangedEvent("IsBUK1BM1Channel");
                    FirePropertyChangedEvent("IsBUK1BM2Channel");
                    FirePropertyChangedEvent("IsBUK2BM1Channel");
                    Owner.FirePropertyChangedEvent("Caption");
                    Device.CmdSetDeviceLogicAddr();
                }
            }

            /// <summary>
            /// Получает строку представления [логический адрес БУСК].
            /// </summary>
            /// <value>
            /// Строка [логический адрес БУСК].
            /// </value>
            public string ShowLogicBusk
            {
                get
                {
                    return string.Format(Resource.Get(@"stShowLogicBusk"), LogicBusk);
                }
            }

            /// <summary>
            /// Получает или задает [логический адрес БУСК].
            /// </summary>
            /// <value>
            /// Значение [логический адрес БУСК].
            /// </value>
            public int LogicBusk
            {
                get
                {
                    return _logicBusk;
                }

                set
                {
                    _logicBusk = value;
                    ControlValuesList[Global.Spacewire2.SPTPLogicBusk].SetProperty(Global.Spacewire2.SPTPLogicBusk, value);
                    FirePropertyChangedEvent("ShowLogicBusk");
                    FirePropertyChangedEvent("LogicBusk");
                }
            }

            /// <summary>
            /// Получает строку представления [логический адрес БУК].
            /// </summary>
            /// <value>
            /// Строка [логический адрес БУК].
            /// </value>
            public string ShowLogicBuk
            {
                get
                {
                    return string.Format(Resource.Get(@"stShowLogicBuk"), LogicBuk);
                }
            }

            /// <summary>
            /// Получает или задает [логический адрес БУК].
            /// </summary>
            /// <value>
            /// Значение [логический адрес БУК].
            /// </value>
            public int LogicBuk
            {
                get
                {
                    return _logicBuk;
                }

                set
                {
                    _logicBuk = value;
                    ControlValuesList[Global.Spacewire2.SPTPLogicBuk].SetProperty(Global.Spacewire2.SPTPLogicBuk, value);
                    FirePropertyChangedEvent("ShowLogicBuk");
                    FirePropertyChangedEvent("LogicBuk");
                }
            }

            /// <summary>
            /// Получает строку представления [логический адрес БКП].
            /// </summary>
            /// <value>
            /// Строка [логический адрес БКП].
            /// </value>
            public string ShowLogicBkp
            {
                get
                {
                    return string.Format(Resource.Get(@"stShowLogicBkp"), LogicBkp);
                }
            }

            /// <summary>
            /// Получает или задает [логический адрес БКП].
            /// </summary>
            /// <value>
            /// Значение [логический адрес БКП].
            /// </value>
            public int LogicBkp
            {
                get
                {
                    return _logicBkp;
                }

                set
                {
                    _logicBkp = value;
                    ControlValuesList[Global.Spacewire2.SPTPLogicBkp].SetProperty(Global.Spacewire2.SPTPLogicBkp, value);
                    FirePropertyChangedEvent("ShowLogicBkp");
                    FirePropertyChangedEvent("LogicBkp");
                }
            }

            /// <summary>
            /// Получает или задает значение, показывающее, что [интерфейс SpaceWire2 включен].
            /// </summary>
            /// <value>
            /// <c>true</c> если [интерфейс SpaceWire2 включен]; иначе, <c>false</c>.
            /// </value>
            public bool IsIntfOn
            {
                get
                {
                    return _isIntfOn;
                }

                set
                {
                    _isIntfOn = value;
                    ControlValuesList[Global.Spacewire2.Control].SetProperty(Global.Spacewire2.Control.IntfOn, Convert.ToInt32(value));
                    FirePropertyChangedEvent("IsIntfOn");
                }
            }

            /// <summary>
            /// Получает или задает значение, показывающее, что [связь по интерфейсу SpaceWire2 установлена].
            /// </summary>
            /// <value>
            /// <c>true</c> если [связь по интерфейсу SpaceWire2 установлена]; иначе, <c>false</c>.
            /// </value>
            public bool IsConnected
            {
                get
                {
                    return _isConnected;
                }

                set
                {
                    _isConnected = value;
                    FirePropertyChangedEvent("IsConnected");
                }
            }

            /// <summary>
            /// Получает или задает значение, показывающее, что [Бит отправки RMAP посылки - 1].
            /// </summary>
            /// <value>
            ///   <c>true</c> если [Бит отправки RMAP посылки - 1]; иначе, <c>false</c>.
            /// </value>
            public bool IsSendRMAP
            {
                get
                {
                    return _isSendRMAP;
                }

                set
                {
                    _isSendRMAP = value;
                    ControlValuesList[Global.Spacewire2.Record].SetProperty(Global.Spacewire2.Record.SendRMAP, Convert.ToInt32(value));
                    FirePropertyChangedEvent("IsSendRMAP");
                }
            }

            /// <summary>
            /// Получает или задает значение, показывающее, что [Бит отправки посылки в БУК - 1].
            /// </summary>
            /// <value>
            ///   <c>true</c> если [Бит отправки посылки в БУК - 1]; иначе, <c>false</c>.
            /// </value>
            public bool IsSendBuk
            {
                get
                {
                    return _isSendBuk;
                }

                set
                {
                    _isSendBuk = value;
                    ControlValuesList[Global.Spacewire2.Record].SetProperty(Global.Spacewire2.Record.SendBuk, Convert.ToInt32(value));
                    FirePropertyChangedEvent("IsSendBuk");
                }
            }

            /// <summary>
            /// Получает или задает значение, показывающее, что [Бит отправки посылки в БКП - 1].
            /// </summary>
            /// <value>
            ///   <c>true</c> если [Бит отправки посылки в БКП - 1]; иначе, <c>false</c>.
            /// </value>
            public bool IsSendBkp
            {
                get
                {
                    return _isSendBkp;
                }

                set
                {
                    _isSendBkp = value;
                    ControlValuesList[Global.Spacewire2.Record].SetProperty(Global.Spacewire2.Record.SendBkp, Convert.ToInt32(value));
                    FirePropertyChangedEvent("IsSendBkp");
                }
            }

            /// <summary>
            /// Получает или задает значение, показывающее, что [выдаются метки времени приборам].
            /// </summary>
            /// <value>
            /// <c>true</c> если [выдаются метки времени приборам]; иначе, <c>false</c>.
            /// </value>
            public bool IsTimeMark
            {
                get
                {
                    return _isTimeMark;
                }

                set
                {
                    _isTimeMark = value;
                    ControlValuesList[Global.Spacewire2.SPTPControl].SetProperty(Global.Spacewire2.SPTPControl.TimeMark, Convert.ToInt32(value));
                    FirePropertyChangedEvent("IsTimeMark");
                }
            }

            /// <summary>
            /// Получает или задает значение, показывающее, что [включен обмен для прибора БУК].
            /// </summary>
            /// <value>
            /// <c>true</c> если [включен обмен для прибора БУК]; иначе, <c>false</c>.
            /// </value>
            public bool IsBukTrans
            {
                get
                {
                    return _isBukTrans;
                }

                set
                {
                    _isBukTrans = value;
                    ControlValuesList[Global.Spacewire2.SPTPControl].SetProperty(Global.Spacewire2.SPTPControl.BukTrans, Convert.ToInt32(value));
                    FirePropertyChangedEvent("IsBukTrans");
                }
            }

            /// <summary>
            /// Получает или задает значение, показывающее, что [включен обмен для прибора БКП].
            /// </summary>
            /// <value>
            /// <c>true</c> если [включен обмен для прибора БКП]; иначе, <c>false</c>.
            /// </value>
            public bool IsBkpTrans
            {
                get
                {
                    return _isBkpTrans;
                }

                set
                {
                    _isBkpTrans = value;
                    ControlValuesList[Global.Spacewire2.SPTPControl].SetProperty(Global.Spacewire2.SPTPControl.BkpTrans, Convert.ToInt32(value));
                    FirePropertyChangedEvent("IsBkpTrans");
                }
            }

            /// <summary>
            /// Получает или задает значение, показывающее, что [выдается КБВ для прибора БУК].
            /// </summary>
            /// <value>
            /// <c>true</c> если [выдается КБВ для прибора БУК]; иначе, <c>false</c>.
            /// </value>
            public bool IsBukKbv
            {
                get
                {
                    return _isBukKbv;
                }

                set
                {
                    _isBukKbv = value;
                    ControlValuesList[Global.Spacewire2.SPTPControl].SetProperty(Global.Spacewire2.SPTPControl.BukKbv, Convert.ToInt32(value));
                    FirePropertyChangedEvent("IsBukKbv");
                }
            }

            /// <summary>
            /// Получает или задает значение, показывающее, что [выдается КБВ для прибора БКП].
            /// </summary>
            /// <value>
            /// <c>true</c> если [выдается КБВ для прибора БКП]; иначе, <c>false</c>.
            /// </value>
            public bool IsBkpKbv
            {
                get
                {
                    return _isBkpKbv;
                }

                set
                {
                    _isBkpKbv = value;
                    ControlValuesList[Global.Spacewire2.SPTPControl].SetProperty(Global.Spacewire2.SPTPControl.BkpKbv, Convert.ToInt32(value));
                    FirePropertyChangedEvent("IsBkpKbv");
                }
            }

            /// <summary>
            /// Получает или задает значение, показывающее, что [можно выдавать пакеты данных в БУК].
            /// </summary>
            /// <value>
            /// <c>true</c> если [можно выдавать пакеты данных в БУК]; иначе, <c>false</c>.
            /// </value>
            public bool IsBukTransData
            {
                get
                {
                    return _isBukTransData;
                }

                set
                {
                    _isBukTransData = value;
                    ControlValuesList[Global.Spacewire2.SPTPControl].SetProperty(Global.Spacewire2.SPTPControl.BukTransData, Convert.ToInt32(value));
                    FirePropertyChangedEvent("IsBukTransData");
                }
            }

            /// <summary>
            /// Получает или задает значение, показывающее, что [можно выдавать пакеты данных в БКП].
            /// </summary>
            /// <value>
            /// <c>true</c> если [можно выдавать пакеты данных в БКП]; иначе, <c>false</c>.
            /// </value>
            public bool IsBkpTransData
            {
                get
                {
                    return _isBkpTransData;
                }

                set
                {
                    _isBkpTransData = value;
                    ControlValuesList[Global.Spacewire2.SPTPControl].SetProperty(Global.Spacewire2.SPTPControl.BkpTransData, Convert.ToInt32(value));
                    FirePropertyChangedEvent("IsBkpTransData");
                }
            }

            /// <summary>
            /// Получает сообщение об ошибке в объекте.
            /// </summary>
            /// <returns>An error message indicating what is wrong with this object. The default is an empty string ("").</returns>
            public string Error
            {
                get
                {
                    return null;
                }
            }

            /// <summary>
            /// Gets the <see cref="System.String"/> with the specified name.
            /// </summary>
            /// <value>
            /// The <see cref="System.String"/>.
            /// </value>
            /// <param name="name">The name.</param>
            /// <returns>Сообщение об ошибке.</returns>
            public string this[string name]
            {
                get
                {
                    string result = null;

                    if (name == "Data")
                    {
                        if ((null != Data) && (0 == Data.Length))
                        {
                            result = "Некорректный ввод данных! Повторите ввод.";
                        }
                    }

                    return result;
                }
            }

            /// <summary>
            /// Initializes the control value.
            /// </summary>
            protected override void InitControlValue()
            {
                ControlValuesList.Add(Global.Spacewire2.Control, new ControlValue());
                ControlValuesList.Add(Global.Spacewire2.Record, new ControlValue());
                ControlValuesList.Add(Global.Spacewire2.SPTPControl, new ControlValue());
                ControlValuesList.Add(Global.Spacewire2.SPTPLogicBusk, new ControlValue());
                ControlValuesList.Add(Global.Spacewire2.SPTPLogicBuk, new ControlValue());
                ControlValuesList.Add(Global.Spacewire2.SPTPLogicBkp, new ControlValue());
            }

            /// <summary>
            /// Initializes the properties.
            /// </summary>
            protected override void InitProperties()
            {
                ControlValuesList[Global.Spacewire2.Control].AddProperty(Global.Spacewire2.Control.Channel, 1, 2, Device.CmdSpacewire2Control, value => SelectSimRouterChannel = (SimRouterChannel)value);
                ControlValuesList[Global.Spacewire2.Control].AddProperty(Global.Spacewire2.Control.IntfOn, 0, 1, Device.CmdSpacewire2Control, value => IsIntfOn = 1 == value);
                ControlValuesList[Global.Spacewire2.Control].AddProperty(Global.Spacewire2.Control.Connected, 3, 1, delegate { }, value => IsConnected = 1 == value);
                ControlValuesList[Global.Spacewire2.Record].AddProperty(Global.Spacewire2.Record.SendRMAP, 0, 1, Device.CmdSpacewire2Record, value => IsSendRMAP = 1 == value);
                ControlValuesList[Global.Spacewire2.Record].AddProperty(Global.Spacewire2.Record.SendBuk, 1, 1, Device.CmdSpacewire2Record, value => IsSendBuk = 1 == value);
                ControlValuesList[Global.Spacewire2.Record].AddProperty(Global.Spacewire2.Record.SendBkp, 2, 1, Device.CmdSpacewire2Record, value => IsSendBkp = 1 == value);
                ControlValuesList[Global.Spacewire2.SPTPLogicBusk].AddProperty(Global.Spacewire2.SPTPLogicBusk, 0, 8, Device.CmdSpacewire2LogicBusk, value => LogicBusk = value);
                ControlValuesList[Global.Spacewire2.SPTPLogicBuk].AddProperty(Global.Spacewire2.SPTPLogicBuk, 0, 8, Device.CmdSpacewire2LogicBuk, value => LogicBuk = value);
                ControlValuesList[Global.Spacewire2.SPTPLogicBkp].AddProperty(Global.Spacewire2.SPTPLogicBkp, 0, 8, delegate { }, value => LogicBkp = value);
                ControlValuesList[Global.Spacewire2.SPTPControl].AddProperty(Global.Spacewire2.SPTPControl.TimeMark, 0, 1, Device.CmdSpacewire2SPTPControl, value => IsTimeMark = 1 == value);
                ControlValuesList[Global.Spacewire2.SPTPControl].AddProperty(Global.Spacewire2.SPTPControl.BukTrans, 1, 1, Device.CmdSpacewire2SPTPControl, value => IsBukTrans = 1 == value);
                ControlValuesList[Global.Spacewire2.SPTPControl].AddProperty(Global.Spacewire2.SPTPControl.BkpTrans, 4, 1, Device.CmdSpacewire2SPTPControl, value => IsBkpTrans = 1 == value);
                ControlValuesList[Global.Spacewire2.SPTPControl].AddProperty(Global.Spacewire2.SPTPControl.BukKbv, 2, 1, Device.CmdSpacewire2SPTPControl, value => IsBukKbv = 1 == value);
                ControlValuesList[Global.Spacewire2.SPTPControl].AddProperty(Global.Spacewire2.SPTPControl.BkpKbv, 5, 1, Device.CmdSpacewire2SPTPControl, value => IsBkpKbv = 1 == value);
                ControlValuesList[Global.Spacewire2.SPTPControl].AddProperty(Global.Spacewire2.SPTPControl.BukTransData, 3, 1, Device.CmdSpacewire2SPTPControl, value => IsBukTransData = 1 == value);
                ControlValuesList[Global.Spacewire2.SPTPControl].AddProperty(Global.Spacewire2.SPTPControl.BkpTransData, 6, 1, Device.CmdSpacewire2SPTPControl, value => IsBkpTransData = 1 == value);
            }
        }

        /// <summary>
        /// Нотификатор spacewire3.
        /// </summary>
        public class Spacewire3 : SubNotify, IDataErrorInfo
        {
            /// <summary>
            /// Инициализирует новый экземпляр класса <see cref="Spacewire3" />.
            /// </summary>
            /// <param name="owner">The owner.</param>
            public Spacewire3(EgseBukNotify owner)
                : base(owner)
            {
            }

            /// <summary>
            /// Получает сообщение об ошибке в объекте.
            /// </summary>
            /// <returns>An error message indicating what is wrong with this object. The default is an empty string ("").</returns>
            public string Error
            {
                get
                {
                    return null;
                }
            }

            /// <summary>
            /// Gets the <see cref="System.String"/> with the specified name.
            /// </summary>
            /// <value>
            /// The <see cref="System.String"/>.
            /// </value>
            /// <param name="name">The name.</param>
            /// <returns>Сообщение об ошибке.</returns>
            public string this[string name]
            {
                get
                {
                    string result = null;

                    return result;
                }
            }

            /// <summary>
            /// Initializes the control value.
            /// </summary>
            protected override void InitControlValue()
            {
            }

            /// <summary>
            /// Initializes the properties.
            /// </summary>
            protected override void InitProperties()
            {
            }
        }

        /// <summary>
        /// Управление spacewire4.
        /// </summary>
        public class Spacewire4 : SubNotify, IDataErrorInfo
        {
            /// <summary>
            /// Управление: вкл/выкл интерфейса Spacewire.
            /// </summary>
            private bool _isIntfOn;

            /// <summary>
            /// Управление: Установлена связь.
            /// </summary>
            private bool _isConnected;

            /// <summary>
            /// Управление: Включение метки времени (1 Гц).
            /// </summary>
            private bool _isTimeMark;

            /// <summary>
            /// Запись данных(до 1 Кбайт): EEP или EOP.
            /// </summary>
            private bool _isEEPSend;

            /// <summary>
            /// Запись данных(до 1 Кбайт): Выдача в конце посылки EOP или EEP.
            /// </summary>
            private bool _isEOPSend;

            /// <summary>
            /// Запись данных(до 1 Кбайт): Автоматическая выдача.
            /// </summary>
            private bool _isAutoSend;

            /// <summary>
            /// Запись данных(до 1 Кбайт): Бит занятости.
            /// </summary>
            private bool _isRecordBusy;

            /// <summary>
            /// Запись данных(до 1 Кбайт): Бит занятости.
            /// </summary>
            private bool _isRecordSend;

            /// <summary>
            /// Инициализирует новый экземпляр класса <see cref="Spacewire4" />.
            /// </summary>
            /// <param name="owner">The owner.</param>
            public Spacewire4(EgseBukNotify owner)
                : base(owner)
            {
            }

            /// <summary>
            /// Получает или задает буфер данных для передачи в USB.
            /// </summary>
            /// <value>
            /// Буфер данных.
            /// </value>
            public byte[] Data { get; set; }

            /// <summary>
            /// Получает или задает значение, показывающее, что [интерфейс Spacewire включен].
            /// </summary>
            /// <value>
            /// <c>true</c> если [интерфейс SpaceWire4 включен]; иначе, <c>false</c>.
            /// </value>
            public bool IsIntfOn
            {
                get
                {
                    return _isIntfOn;
                }

                set
                {
                    _isIntfOn = value;
                    ControlValuesList[Global.Spacewire4.Control].SetProperty(Global.Spacewire4.Control.IntfOn, Convert.ToInt32(_isIntfOn));
                    FirePropertyChangedEvent("IsIntfOn");
                }
            }

            /// <summary>
            /// Получает или задает значение, показывающее, что [связь по интерфейсу Spacewire установлена].
            /// </summary>
            /// <value>
            /// <c>true</c> если [связь по интерфейсу SpaceWire4 установлена]; иначе, <c>false</c>.
            /// </value>
            public bool IsConnected
            {
                get
                {
                    return _isConnected;
                }

                set
                {
                    _isConnected = value;
                    FirePropertyChangedEvent("IsConnected");
                }
            }

            /// <summary>
            /// Получает или задает значение, показывающее, что [выдается метка времени].
            /// </summary>
            /// <value>
            /// <c>true</c> если [выдается метка времени]; иначе, <c>false</c>.
            /// </value>
            public bool IsTimeMark
            {
                get
                {
                    return _isTimeMark;
                }

                set
                {
                    _isTimeMark = value;
                    ControlValuesList[Global.Spacewire4.Control].SetProperty(Global.Spacewire4.Control.TimeMark, Convert.ToInt32(_isTimeMark));
                    FirePropertyChangedEvent("IsTimeMark");
                }
            }

            /// <summary>
            /// Получает или задает значение, показывающее, что [выдается EEP].
            /// </summary>
            /// <value>
            /// <c>true</c> если [выдается EEP]; иначе, <c>false</c>.
            /// </value>
            public bool IsEEPSend
            {
                get
                {
                    return _isEEPSend;
                }

                set
                {
                    _isEEPSend = value;
                    ControlValuesList[Global.Spacewire4.Record].SetProperty(Global.Spacewire4.Record.EEPSend, Convert.ToInt32(value));
                    FirePropertyChangedEvent("IsEEPSend");
                }
            }

            /// <summary>
            /// Получает или задает значение, показывающее, что [выдается EOP].
            /// </summary>
            /// <value>
            /// <c>true</c> если [выдается EOP]; иначе, <c>false</c>.
            /// </value>
            public bool IsEOPSend
            {
                get
                {
                    return _isEOPSend;
                }

                set
                {
                    _isEOPSend = value;
                    ControlValuesList[Global.Spacewire4.Record].SetProperty(Global.Spacewire4.Record.EOPSend, Convert.ToInt32(value));
                    FirePropertyChangedEvent("IsEOPSend");
                }
            }

            /// <summary>
            /// Получает или задает значение, показывающее, что [включена автоматическая выдача посылки].
            /// </summary>
            /// <value>
            /// <c>true</c> если [включена автоматическая выдача посылки]; иначе, <c>false</c>.
            /// </value>
            public bool IsAutoSend
            {
                get
                {
                    return _isAutoSend;
                }

                set
                {
                    _isAutoSend = value;
                    ControlValuesList[Global.Spacewire4.Record].SetProperty(Global.Spacewire4.Record.AutoSend, Convert.ToInt32(value));
                    FirePropertyChangedEvent("IsAutoSend");
                }
            }

            /// <summary>
            /// Получает или задает значение, показывающее, что [Бит занятости записи - 1].
            /// </summary>
            /// <value>
            ///   <c>true</c> если [Бит занятости записи - 1]; иначе, <c>false</c>.
            /// </value>
            public bool IsRecordBusy
            {
                get
                {
                    return _isRecordBusy || IsRecordSend;
                }

                set
                {
                    _isRecordBusy = value;
                    FirePropertyChangedEvent("IsRecordBusy");
                }
            }

            /// <summary>
            /// Получает или задает значение, показывающее, что [Бит выдачи посылки - 1].
            /// </summary>
            /// <value>
            ///   <c>true</c> если [Бит выдачи посылки - 1]; иначе, <c>false</c>.
            /// </value>
            public bool IsRecordSend
            {
                get
                {
                    return _isRecordSend;
                }

                set
                {
                    _isRecordSend = value;
                    ControlValuesList[Global.Spacewire4.Record].SetProperty(Global.Spacewire4.Record.RecordSend, Convert.ToInt32(value));
                    FirePropertyChangedEvent("IsRecordSend");
                }
            }

            /// <summary>
            /// Получает сообщение об ошибке в объекте.
            /// </summary>
            /// <returns>An error message indicating what is wrong with this object. The default is an empty string ("").</returns>
            public string Error
            {
                get
                {
                    return null;
                }
            }

            /// <summary>
            /// Gets the <see cref="System.String"/> with the specified name.
            /// </summary>
            /// <value>
            /// The <see cref="System.String"/>.
            /// </value>
            /// <param name="name">The name.</param>
            /// <returns>Сообщение об ошибке.</returns>
            public string this[string name]
            {
                get
                {
                    string result = null;

                    if (name == "Data")
                    {
                        if ((null != Data) && (0 == Data.Length))
                        {
                            result = "Некорректный ввод данных! Повторите ввод.";
                        }
                    }

                    return result;
                }
            }

            /// <summary>
            /// Initializes the control value.
            /// </summary>
            protected override void InitControlValue()
            {
                ControlValuesList.Add(Global.Spacewire4.Control, new ControlValue());
                ControlValuesList.Add(Global.Spacewire4.Record, new ControlValue());
            }

            /// <summary>
            /// Initializes the properties.
            /// </summary>
            protected override void InitProperties()
            {
                ControlValuesList[Global.Spacewire4.Control].AddProperty(Global.Spacewire4.Control.IntfOn, 0, 1, Device.CmdSpacewire4Control, value => IsIntfOn = 1 == value);
                ControlValuesList[Global.Spacewire4.Control].AddProperty(Global.Spacewire4.Control.Connected, 3, 1, delegate { }, value => IsConnected = 1 == value);
                ControlValuesList[Global.Spacewire4.Control].AddProperty(Global.Spacewire4.Control.TimeMark, 4, 1, Device.CmdSpacewire4Control, value => IsTimeMark = 1 == value);
                ControlValuesList[Global.Spacewire4.Record].AddProperty(Global.Spacewire4.Record.EOPSend, 1, 1, Device.CmdSpacewire4Record, value => IsEOPSend = 1 == value);
                ControlValuesList[Global.Spacewire4.Record].AddProperty(Global.Spacewire4.Record.AutoSend, 4, 1, Device.CmdSpacewire4Record, value => IsAutoSend = 1 == value);
                ControlValuesList[Global.Spacewire4.Record].AddProperty(Global.Spacewire4.Record.EEPSend, 2, 1, Device.CmdSpacewire4Record, value => IsEEPSend = 1 == value);
                ControlValuesList[Global.Spacewire4.Record].AddProperty(Global.Spacewire4.Record.RecordBusy, 3, 1, delegate { }, value => IsRecordBusy = 1 == value);
                ControlValuesList[Global.Spacewire4.Record].AddProperty(Global.Spacewire4.Record.RecordSend, 0, 1, Device.CmdSpacewire4Record, value => IsRecordSend = 1 == value);
            }
        }
    }
}
