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
    using System.Windows.Input;
    using EGSE;
    using EGSE.Constants;
    using EGSE.Defaults;
    using EGSE.Protocols;
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
        /// Адресный байт "Датчики затворов [7:0]".
        /// </summary>
        private const int LockLoAddr = 0x44;

        /// <summary>
        /// Адресный байт "Датчики затворов [11:8]".
        /// </summary>
        private const int LockHiAddr = 0x45;

        /// <summary>
        /// Адресный байт "Бит выдачи релейных команд для установки затворов".
        /// </summary>
        private const int LockSetAddr = 0x46;

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
        private const int Spacewire1SPTPControlAddr = 0x16; //0x21;

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
        /// Адресный байт "Управление".
        /// </summary>
        private const int Spacewire3ControlAddr = 0x0c;

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
        private const int Spacewire4RecordSendAddr = 0x27;

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
        /// Отправляет команду включить/выключить затвор УФЕС ОСН.
        /// </summary>
        /// <param name="value"><c>1</c> если [включить]; иначе, <c>0</c>.</param>
        public void CmdUfesLock1(int value)
        {
            byte buf = 0;
            if (0 == value)
            {
                buf |= 1 << 3;
            }
            else
            {
                buf |= 1 << 2;
            }

            SendToUSB(LockHiAddr, new byte[1] { buf });
            SendToUSB(LockSetAddr, new byte[1] { 1 });
        }

        /// <summary>
        /// Отправляет команду включить/выключить затвор УФЕС РЕЗ.
        /// </summary>
        /// <param name="value"><c>1</c> если [включить]; иначе, <c>0</c>.</param>
        public void CmdUfesLock2(int value)
        {
            byte buf = 0;
            if (0 == value)
            {
                buf |= 1 << 1;
            }
            else
            {
                buf |= 1 << 0;
            }

            SendToUSB(LockHiAddr, new byte[1] { buf });
            SendToUSB(LockSetAddr, new byte[1] { 1 });
        }

        /// <summary>
        /// Отправляет команду включить/выключить затвор ВУФЕС ОСН.
        /// </summary>
        /// <param name="value"><c>1</c> если [включить]; иначе, <c>0</c>.</param>
        public void CmdVufesLock1(int value)
        {
            byte buf = 0;
            if (0 == value)
            {
                buf |= 1 << 7;
            }
            else
            {
                buf |= 1 << 6;
            }

            SendToUSB(LockLoAddr, new byte[1] { buf });
            SendToUSB(LockSetAddr, new byte[1] { 1 });
        }

        /// <summary>
        /// Отправляет команду включить/выключить затвор ВУФЕС РЕЗ.
        /// </summary>
        /// <param name="value"><c>1</c> если [включить]; иначе, <c>0</c>.</param>
        public void CmdVufesLock2(int value)
        {
            byte buf = 0;
            if (0 == value)
            {
                buf |= 1 << 5;
            }
            else
            {
                buf |= 1 << 4;
            }

            SendToUSB(LockLoAddr, new byte[1] { buf });
            SendToUSB(LockSetAddr, new byte[1] { 1 });
        }

        /// <summary>
        /// Отправляет команду включить/выключить затвор СДЩ ОСН.
        /// </summary>
        /// <param name="value"><c>1</c> если [включить]; иначе, <c>0</c>.</param>
        public void CmdSdchshLock1(int value)
        {
            byte buf = 0;
            if (0 == value)
            {
                buf |= 1 << 3;
            }
            else
            {
                buf |= 1 << 2;
            }

            SendToUSB(LockLoAddr, new byte[1] { buf });
            SendToUSB(LockSetAddr, new byte[1] { 1 });
        }

        /// <summary>
        /// Отправляет команду включить/выключить затвор СДЩ РЕЗ.
        /// </summary>
        /// <param name="value"><c>1</c> если [включить]; иначе, <c>0</c>.</param>
        public void CmdSdchshLock2(int value)
        {
            byte buf = 0;
            if (0 == value)
            {
                buf |= 1 << 1;
            }
            else
            {
                buf |= 1 << 0;
            }

            SendToUSB(LockLoAddr, new byte[1] { buf });
            SendToUSB(LockSetAddr, new byte[1] { 1 });
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
            if (_intfBUK.Spacewire4Notify.IsEnable)
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
        /// Команда SpaceWire3: Управление.
        /// </summary>
        /// <param name="value">Параметры управления.</param>
        public void CmdSpacewire3Control(int value)
        {
            SendToUSB(Spacewire3ControlAddr, new byte[1] { (byte)value });
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
            }
            SendToUSB(Spacewire4RecordSendAddr, new byte[1] { (byte)value });
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
                _intfBUK.Spacewire1Notify.LogicBusk = Global.LogicAddrBusk1;
                _intfBUK.Spacewire2Notify.LogicBusk = Global.LogicAddrBusk1;
            }
            else
            {
                _intfBUK.Spacewire1Notify.LogicBusk = Global.LogicAddrBusk2;
                _intfBUK.Spacewire2Notify.LogicBusk = Global.LogicAddrBusk2;
            }                        
        }

        internal void CmdHSILine1(int value)
        {
            throw new NotImplementedException();
        }

        internal void CmdHSILine2(int value)
        {
            throw new NotImplementedException();
        }

        internal void CmdSimHSIControl(int value)
        {
            throw new NotImplementedException();
        }

        internal void CmdSimHSIRecord(int value)
        {
            throw new NotImplementedException();
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
        /// Адресный байт "Данные интерфейса ВСИ".
        /// </summary>
        private const int HSIDataAddr = 0x14;

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
        /// Экземпляр декодера протокола Spacewire.
        /// </summary>
        private ProtocolSpacewire _decoderSpacewireSDIn;

        /// <summary>
        /// Экземпляр декодера протокола Spacewire.
        /// </summary>
        private ProtocolSpacewire _decoderSpacewireSDOut;

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
        /// Отображать ли [окно "имитатор ВСИ"].
        /// </summary>
        private bool _isShowHSI;

        /// <summary>
        /// Отображать ли [окно "имитатор БУСК"].
        /// </summary>
        private bool _isShowSpacewire;

        /// <summary>
        /// Отображать ли [окно "имитатор НП"].
        /// </summary>
        private bool _isShowSD;

        /// <summary>
        /// Отображать ли [окно "имитатор БУК (для ВСИ)"].
        /// </summary>
        private bool _isShowSimHSI;

        /// <summary>
        /// Отображать ли [окно "имитатор БУК (для БУСК)"].
        /// </summary>
        private bool _isShowSimSpacewire;

        /// <summary>
        /// Отображать ли [окно "имитатор БУК (для НП)"].
        /// </summary>
        private bool _isShowSimSD;

        /// <summary>
        /// Время, пришедшее от прибора.
        /// </summary>
        private EgseTime _deviceTime;

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

            HSINotify = new HSI(this);
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

            _decoderSpacewireSDIn = new ProtocolSpacewire((uint)Spacewire3Addr.InData, (uint)Spacewire3Addr.InEnd, (uint)Spacewire3Addr.InTime1, (uint)Spacewire3Addr.InTime2);
            _decoderSpacewireSDIn.GotSpacewireMsg += new ProtocolSpacewire.SpacewireMsgEventHandler(OnSpacewire3Msg);
            _decoderUSB.GotProtocolMsg += new ProtocolUSBBase.ProtocolMsgEventHandler(_decoderSpacewireSDIn.OnMessageFunc);

            _decoderSpacewireSDOut = new ProtocolSpacewire((uint)Spacewire3Addr.OutData, (uint)Spacewire3Addr.OutEnd, (uint)Spacewire3Addr.OutTime1, (uint)Spacewire3Addr.OutTime2);
            _decoderSpacewireSDOut.GotSpacewireMsg += new ProtocolSpacewire.SpacewireMsgEventHandler(OnSpacewire3Msg);
            _decoderUSB.GotProtocolMsg += new ProtocolUSBBase.ProtocolMsgEventHandler(_decoderSpacewireSDOut.OnMessageFunc);  

            _devDataLogStream = null;
            _isWriteDevDataToFile = false;
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
        /// Вызывается, когда [получено сообщение по spacewire 3].
        /// </summary>
        public event ProtocolSpacewire.SpacewireMsgEventHandler GotSpacewire3Msg;
        private int _waitForTimer = 100;

        /// <summary>
        /// Возможные рабочие приборы.
        /// </summary>
        public enum WorkDevice
        {
            /// <summary>
            /// Рабочий прибор "УФЕС".
            /// </summary>
            Ufes = 0x00,

            /// <summary>
            /// Рабочий прибор "ВУФЕС".
            /// </summary>
            Vufes = 0x01,

            /// <summary>
            /// Рабочий прибор "СДЩ".
            /// </summary>
            Sdchsh = 0x02
        }

        public enum Line
        {
            Main = 0x02,

            Resv = 0x01,

            MainReserv = 0x00
        }

        /// <summary>
        /// Полукомплекты рабочего прибора.
        /// </summary>
        public enum HalfSet
        {
            /// <summary>
            /// Первый полукомплект.
            /// </summary>
            First = 0x00,

            /// <summary>
            /// Второй полукомплект.
            /// </summary>
            Second = 0x01
        }

        /// <summary>
        /// Список возможных каналов имитатора БМ-4.
        /// </summary>
        public enum SpacewireChannel
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
        /// Описывает адресные байты Spacewire2.
        /// </summary>
        public enum Spacewire3Addr : uint
        {
            /// <summary>
            /// Адресный байт "SPACEWIRE 3: ВЫХОДНЫЕ ДАННЫЕ: Данные Spacewire".
            /// </summary>
            OutData = 0x0c,

            /// <summary>
            /// Адресный байт "SPACEWIRE 3: ВЫХОДНЫЕ ДАННЫЕ: EOP или EEP".
            /// </summary>
            OutEnd = 0x0d,

            /// <summary>
            /// Адресный байт "SPACEWIRE 3: ВЫХОДНЫЕ ДАННЫЕ: TIME TICK".
            /// </summary>
            OutTime1 = 0x0e,

            /// <summary>
            /// Адресный байт "SPACEWIRE 3: ВЫХОДНЫЕ ДАННЫЕ: TIME TICK".
            /// </summary>
            OutTime2 = 0x0f,

            /// <summary>
            /// Адресный байт "SPACEWIRE 3: ВХОДНЫЕ ДАННЫЕ: Данные Spacewire".
            /// </summary>
            InData = 0x10,

            /// <summary>
            /// Адресный байт "SPACEWIRE 3: ВХОДНЫЕ ДАННЫЕ: EOP или EEP".
            /// </summary>
            InEnd = 0x11,

            /// <summary>
            /// Адресный байт "SPACEWIRE 2: ВХОДНЫЕ ДАННЫЕ: TIME TICK".
            /// </summary>
            InTime1 = 0x12,

            /// <summary>
            /// Адресный байт "SPACEWIRE 3: ВХОДНЫЕ ДАННЫЕ: TIME TICK".
            /// </summary>
            InTime2 = 0x13
        }

        /// <summary>
        /// Получает или задает нотификатор телеметрии.
        /// </summary>
        /// <value>
        /// Экземпляр нотификатора.
        /// </value>
        public Telemetry TelemetryNotify { get; set; }

        /// <summary>
        /// Получает или задает нотификатор ВСИ интерфейса.
        /// </summary>
        /// <value>
        /// Экземпляр нотификатора.
        /// </value>
        public HSI HSINotify { get; set; }

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
        /// Получает значение, показывающее, открыто ли [окно "имитатор ВСИ"].
        /// </summary>
        /// <value>
        ///   <c>true</c> если [окно "имитатор ВСИ"] открыто; иначе, <c>false</c>.
        /// </value>
        public bool IsShowHSI
        {
            get
            {
                return _isShowHSI;
            }

            private set
            {
                _isShowHSI = value;
                FirePropertyChangedEvent("IsShowHSI");
            }
        }

        /// <summary>
        /// Получает значение, показывающее, открыто ли [окно "имитатор БУСК"].
        /// </summary>
        /// <value>
        ///   <c>true</c> если [окно "имитатор БУСК"] открыто; иначе, <c>false</c>.
        /// </value>
        public bool IsShowSpacewire
        {
            get
            {
                return _isShowSpacewire;
            }

            private set
            {
                _isShowSpacewire = value;
                FirePropertyChangedEvent("IsShowSpacewire");
            }
        }

        /// <summary>
        /// Получает значение, показывающее, открыто ли [окно "имитатор НП"].
        /// </summary>
        /// <value>
        ///   <c>true</c> если [окно "имитатор НП"] открыто; иначе, <c>false</c>.
        /// </value>
        public bool IsShowSD
        {
            get
            {
                return _isShowSD;
            }

            private set
            {
                _isShowSD = value;
                FirePropertyChangedEvent("IsShowSD");
            }
        }

        /// <summary>
        /// Получает значение, показывающее, открыто ли [окно "имитатор БУК (для ВСИ)"].
        /// </summary>
        /// <value>
        ///   <c>true</c> если [окно "имитатор БУК (для ВСИ)"] открыто; иначе, <c>false</c>.
        /// </value>
        public bool IsShowSimHSI
        {
            get
            {
                return _isShowSimHSI;
            }

            private set
            {
                _isShowSimHSI = value;
                FirePropertyChangedEvent("IsShowSimHSI");
            }
        }

        /// <summary>
        /// Получает значение, показывающее, открыто ли [окно "имитатор БУК (для БУСК)"].
        /// </summary>
        /// <value>
        ///   <c>true</c> если [окно "имитатор БУК (для БУСК)"] открыто; иначе, <c>false</c>.
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
        /// Получает значение, показывающее, открыто ли [окно "имитатор БУК (для НП)"].
        /// </summary>
        /// <value>
        ///   <c>true</c> если [окно "имитатор БУК (для НП)"] открыто; иначе, <c>false</c>.
        /// </value>
        public bool IsShowSimSD
        {
            get
            {
                return _isShowSimSD;
            }

            private set
            {
                _isShowSimSD = value;
                FirePropertyChangedEvent("IsShowSimSD");
            }
        }

        /// <summary>
        /// Получает значение текущей скорости по USB.
        /// </summary>
        /// <value>
        /// Скорость передачи данных по USB.
        /// </value>
        public float DeviceSpeed
        {
            get
            {
                return Device.Speed;
            }
        }

        /// <summary>
        /// Получает значение трафика по USB.
        /// </summary>
        /// <value>
        /// Трафик данных по USB.
        /// </value>
        public long DeviceTrafic
        {
            get
            {
                return Device.Trafic;
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
        public EgseTime DeviceTime
        {
            get
            {                
                return _deviceTime;                
            }

            set
            {
                _deviceTime = value;                
            }
        }

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
            if(++_waitForTimer == 4)
            {
                Device.CmdSetDeviceLogicAddr();   
            }
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
                _waitForTimer = 0;                                  
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
        /// <param name="e">The <see cref="SpacewireSptpMsgEventArgs"/> instance containing the event data.</param>
        protected virtual void OnSpacewire2Msg(object sender, SpacewireSptpMsgEventArgs e)
        {
            if (this.GotSpacewire2Msg != null)
            {
                if (IsRequestSpacewireMsg(e))
                {
                    if (Spacewire2Notify.LogicBusk == e.From)
                    {
                        Spacewire2Notify.Spacewire2RequestQueueFromBusk++;
                    }
                    else if (Spacewire2Notify.LogicBuk == e.From)
                    {
                        Spacewire2Notify.Spacewire2RequestQueueFromBuk++;
                    }
                    else
                    {
                        this.GotSpacewire2Msg(sender, e);
                    }
                }
                else if (IsReplySpacewireMsg(e))
                {
                    if (Spacewire2Notify.LogicBusk == e.From)
                    {
                        Spacewire2Notify.Spacewire2ReplyQueueFromBusk++;
                    }
                    else if (Spacewire2Notify.LogicBuk == e.From)
                    {
                        Spacewire2Notify.Spacewire2ReplyQueueFromBuk++;
                    }
                    else
                    {
                        this.GotSpacewire2Msg(sender, e);
                    }
                }
                else if (IsKbvSpacewireMsg(e))
                {
                    Spacewire2Notify.Spacewire2Kbv = (e as SpacewireIcdMsgEventArgs).AsKbv().Kbv;
                }
                else
                {
                    this.GotSpacewire2Msg(sender, e);
                }
            }
        }

        /// <summary>
        /// Called when [spacewire3 MSG].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="SpacewireSptpMsgEventArgs"/> instance containing the event data.</param>
        protected virtual void OnSpacewire3Msg(object sender, SpacewireSptpMsgEventArgs e)
        {
            ControlValuesList[Global.Spacewire3.Control].SetProperty(Global.Spacewire3.Control.Transmission, Convert.ToInt32(Spacewire3Notify.IsTransmission));
            if (this.GotSpacewire3Msg != null)
            {
                {
                    this.GotSpacewire3Msg(sender, e);
                }
            }
        }

        /// <summary>
        /// Определяет когда [сообщение по spacewire] [является запросом квоты].
        /// </summary>
        /// <param name="msg">The <see cref="SpacewireSptpMsgEventArgs"/> instance containing the event data.</param>
        /// <returns>true если сообщение "запрос квоты"</returns>
        private bool IsRequestSpacewireMsg(SpacewireSptpMsgEventArgs msg)
        {
            return (SpacewireSptpMsgEventArgs.Type.Request == msg.MsgType);
        }

        private bool IsKbvSpacewireMsg(SpacewireSptpMsgEventArgs msg)
        {
            return 6 == msg.DataLen;
        }
        private bool IsReplySpacewireMsg(SpacewireSptpMsgEventArgs msg)
        {

            return (SpacewireSptpMsgEventArgs.Type.Reply == msg.MsgType);
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
                        Array.Copy(msg.Data, 1, DeviceTime.Data, 0, 6);
                        FirePropertyChangedEvent("DeviceTime");
                        FirePropertyChangedEvent("DeviceSpeed");
                        FirePropertyChangedEvent("DeviceTrafic");
                        ControlValuesList[Global.Spacewire2.Control].UsbValue = msg.Data[7];
                        ControlValuesList[Global.Spacewire2.Record].UsbValue = msg.Data[10]; 
                        ControlValuesList[Global.Spacewire2.SPTPLogicBusk].UsbValue = msg.Data[11];
                        ControlValuesList[Global.Spacewire2.SPTPLogicBuk].UsbValue = msg.Data[12];
                        ControlValuesList[Global.Spacewire2.SPTPLogicBkp].UsbValue = msg.Data[13];
                        ControlValuesList[Global.Spacewire2.SPTPControl].UsbValue = msg.Data[14];
                        ControlValuesList[Global.Spacewire3.Control].UsbValue = msg.Data[15];  
                        ControlValuesList[Global.Spacewire1.Control].UsbValue = msg.Data[17];
                        ControlValuesList[Global.Spacewire1.Record].UsbValue = msg.Data[20];
                        ControlValuesList[Global.Spacewire1.SPTPControl].UsbValue = msg.Data[21];
                        ControlValuesList[Global.Spacewire1.SPTPLogicBusk].UsbValue = msg.Data[22];
                        ControlValuesList[Global.Spacewire1.SPTPLogicSD1].UsbValue = msg.Data[23];
                        ControlValuesList[Global.Spacewire1.SPTPLogicSD2].UsbValue = msg.Data[24];
                        ControlValuesList[Global.Spacewire1.SD1SendTime].UsbValue = (msg.Data[26] << 8) | msg.Data[25];
                        ControlValuesList[Global.Spacewire1.SD2SendTime].UsbValue = (msg.Data[28] << 8) | msg.Data[27];
                        ////ControlValuesList[Global.Spacewire1.SD1DataSize].UsbValue = (msg.Data[30] << 8) | msg.Data[29]; // XXX
                        ////ControlValuesList[Global.Spacewire1.SD2DataSize].UsbValue = (msg.Data[32] << 8) | msg.Data[31]; // XXX
                        ControlValuesList[Global.Spacewire4.Control].UsbValue = msg.Data[29];
                        ControlValuesList[Global.Spacewire4.Record].UsbValue = msg.Data[32];
                        break;
                    case TeleDataAddr:
                        ControlValuesList[Global.Telemetry].UsbValue = (msg.Data[2] << 16) | (msg.Data[3] << 8) | msg.Data[4];
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
                    System.Windows.Input.CommandManager.InvalidateRequerySuggested();
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
        /// Нотификатор ВСИ интерфейса.
        /// </summary>
        public class HSI : SubNotify, IDataErrorInfo
        {
            private bool _isLockResv1;
            private bool _isLockMain1;
            private bool _isOnOff1;
            private bool _isLockResv2;
            private bool _isLockMain2;
            private bool _isOnOff2;
            private int _stateCounter1;
            private int _frameCounter1;
            private int _stateCounter2;
            private int _frameCounter2;
            private bool _isRecordSend;
            private bool _isRequest;
            private int _lineOut;
            private int _lineIn;
            private bool _isLineMain1;
            private bool _isLineMainResv1;
            private bool _isLineResv1;
            private bool _isLineMain2;
            private bool _isLineResv2;
            private bool _isLineMainResv2;

            /// <summary>
            /// Инициализирует новый экземпляр класса <see cref="HSI" />.
            /// </summary>
            /// <param name="owner">The owner.</param>
            public HSI(EgseBukNotify owner)
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
                ControlValuesList.Add(Global.HSI.Line1, new ControlValue());
                ControlValuesList.Add(Global.HSI.Line1StateCounter, new ControlValue());
                ControlValuesList.Add(Global.HSI.Line1FrameCounter, new ControlValue());
                ControlValuesList.Add(Global.HSI.Line2, new ControlValue());
                ControlValuesList.Add(Global.HSI.Line2StateCounter, new ControlValue());
                ControlValuesList.Add(Global.HSI.Line2FrameCounter, new ControlValue());
                ControlValuesList.Add(Global.SimHSI.Control, new ControlValue());
                ControlValuesList.Add(Global.SimHSI.Record, new ControlValue());                
            }

            /// <summary>
            /// Initializes the properties.
            /// </summary>
            protected override void InitProperties()
            {
                ControlValuesList[Global.HSI.Line1].AddProperty(Global.HSI.Line1.LockMain, 1, 1, Device.CmdHSILine1, value => IsLockMain1 = 1 == value);
                ControlValuesList[Global.HSI.Line1].AddProperty(Global.HSI.Line1.LockResv, 2, 1, Device.CmdHSILine1, value => IsLockResv1 = 1 == value);
                ControlValuesList[Global.HSI.Line1].AddProperty(Global.HSI.Line1.OnOff, 0, 1, Device.CmdHSILine1, value => IsOnOff1 = 1 == value);
                ControlValuesList[Global.HSI.Line2].AddProperty(Global.HSI.Line2.LockMain, 1, 1, Device.CmdHSILine2, value => IsLockMain2 = 1 == value);
                ControlValuesList[Global.HSI.Line2].AddProperty(Global.HSI.Line2.LockResv, 2, 1, Device.CmdHSILine2, value => IsLockResv2 = 1 == value);
                ControlValuesList[Global.HSI.Line2].AddProperty(Global.HSI.Line2.OnOff, 0, 1, Device.CmdHSILine2, value => IsOnOff2 = 1 == value);
                ControlValuesList[Global.HSI.Line1StateCounter].AddProperty(Global.HSI.Line1StateCounter, 0, 32, delegate { }, value => StateCounter1 = value);
                ControlValuesList[Global.HSI.Line1FrameCounter].AddProperty(Global.HSI.Line1FrameCounter, 0, 32, delegate { }, value => FrameCounter1 = value);
                ControlValuesList[Global.HSI.Line2StateCounter].AddProperty(Global.HSI.Line2StateCounter, 0, 32, delegate { }, value => StateCounter2 = value);
                ControlValuesList[Global.HSI.Line2FrameCounter].AddProperty(Global.HSI.Line2FrameCounter, 0, 32, delegate { }, value => FrameCounter2 = value);
                ControlValuesList[Global.SimHSI.Control].AddProperty(Global.SimHSI.Control.LineIn, 2, 1, Device.CmdSimHSIControl, value => LineIn = value);
                ControlValuesList[Global.SimHSI.Control].AddProperty(Global.SimHSI.Control.LineOut, 1, 1, Device.CmdSimHSIControl, value => LineOut = value);
                ControlValuesList[Global.SimHSI.Control].AddProperty(Global.SimHSI.Control.Request, 0, 1, Device.CmdSimHSIControl, value => IsRequest = 1 == value);
                ControlValuesList[Global.SimHSI.Record].AddProperty(Global.SimHSI.Record.Send, 0, 1, Device.CmdSimHSIRecord, value => IsRecordSend = 1 == value);
            }
           
            /// <summary>
            /// Получает или задает значение, показывающее, что [Запрет передачи по резервной линии КВВ ПК1].
            /// </summary>
            /// <value>
            /// <c>true</c> если [Запрет передачи по резервной линии КВВ ПК1]; иначе, <c>false</c>.
            /// </value>
            public bool IsLockResv1 
            {
                get
                {
                    return _isLockResv1;
                }

                set
                {
                    _isLockResv1 = value;
                    ControlValuesList[Global.HSI.Line1].SetProperty(Global.HSI.Line1.LockResv, Convert.ToInt32(value));
                    FirePropertyChangedEvent("IsLockResv1");
                }
            }

            /// <summary>
            /// Получает или задает значение, показывающее, что [Запрет передачи по основной линии КВВ ПК1].
            /// </summary>
            /// <value>
            /// <c>true</c> если [Запрет передачи по основной линии КВВ ПК1]; иначе, <c>false</c>.
            /// </value>
            public bool IsLockMain1
            {
                get
                {
                    return _isLockMain1;
                }

                set
                {
                    _isLockMain1 = value;
                    ControlValuesList[Global.HSI.Line1].SetProperty(Global.HSI.Line1.LockMain, Convert.ToInt32(value));
                    FirePropertyChangedEvent("IsLockMain1");
                }
            }

            public bool IsLineMain1
            {
                get
                {
                    return _isLineMain1;
                }

                set
                {
                    IsLockResv1 = value;
                    IsLockMain1 = !value;
                    _isLineMain1 = value;
                    FirePropertyChangedEvent("IsLineMain1");
                }
            }

            public bool IsLineResv1
            {
                get
                {
                    return _isLineResv1;
                }

                set
                {
                    IsLockResv1 = !value;
                    IsLockMain1 = value;
                    _isLineResv1 = value;
                    FirePropertyChangedEvent("IsLineResv1");
                }
            }

            public bool IsLineMainResv1
            {
                get
                {
                    return _isLineMainResv1;
                }

                set
                {
                    IsLockResv1 = !value;
                    IsLockMain1 = !value;
                    _isLineMainResv1 = value;
                    FirePropertyChangedEvent("IsLineMainResv1");
                }
            }

            public bool IsLineMain2
            {
                get
                {
                    return _isLineMain2;
                }

                set
                {
                    IsLockResv2 = value;
                    IsLockMain2 = !value;
                    _isLineMain2 = value;
                    FirePropertyChangedEvent("IsLineMain2");
                }
            }

            public bool IsLineResv2
            {
                get
                {
                    return _isLineResv2;
                }

                set
                {
                    IsLockResv2 = !value;
                    IsLockMain2 = value;
                    _isLineResv2 = value;
                    FirePropertyChangedEvent("IsLineResv2");
                }
            }

            public bool IsLineMainResv2
            {
                get
                {
                    return _isLineMainResv2;
                }

                set
                {
                    IsLockResv2 = !value;
                    IsLockMain2 = !value;
                    _isLineMainResv2 = value;
                    FirePropertyChangedEvent("IsLineMainResv2");
                }
            }

            /// <summary>
            /// Получает или задает значение, показывающее, что [Вкл/выкл КВВ ПК1].
            /// </summary>
            /// <value>
            /// <c>true</c> если [Вкл/выкл КВВ ПК1]; иначе, <c>false</c>.
            /// </value>
            public bool IsOnOff1
            {
                get
                {
                    return _isOnOff1;
                }

                set
                {
                    _isOnOff1 = value;
                    ControlValuesList[Global.HSI.Line1].SetProperty(Global.HSI.Line1.OnOff, Convert.ToInt32(value));
                    FirePropertyChangedEvent("IsOnOff1");
                }
            }

            /// <summary>
            /// Получает или задает значение, показывающее, что [Запрет передачи по резервной линии КВВ ПК2].
            /// </summary>
            /// <value>
            /// <c>true</c> если [Запрет передачи по резервной линии КВВ ПК2]; иначе, <c>false</c>.
            /// </value>
            public bool IsLockResv2
            {
                get
                {
                    return _isLockResv2;
                }

                set
                {
                    _isLockResv2 = value;
                    ControlValuesList[Global.HSI.Line2].SetProperty(Global.HSI.Line2.LockResv, Convert.ToInt32(value));
                    FirePropertyChangedEvent("IsLockResv2");
                }
            }

            /// <summary>
            /// Получает или задает значение, показывающее, что [Запрет передачи по основной линии КВВ ПК2].
            /// </summary>
            /// <value>
            /// <c>true</c> если [Запрет передачи по основной линии КВВ ПК2]; иначе, <c>false</c>.
            /// </value>
            public bool IsLockMain2
            {
                get
                {
                    return _isLockMain2;
                }

                set
                {
                    _isLockMain2 = value;
                    ControlValuesList[Global.HSI.Line2].SetProperty(Global.HSI.Line2.LockMain, Convert.ToInt32(value));
                    FirePropertyChangedEvent("IsLockMain2");
                }
            }

            /// <summary>
            /// Получает или задает значение, показывающее, что [Вкл/выкл КВВ ПК2].
            /// </summary>
            /// <value>
            /// <c>true</c> если [Вкл/выкл КВВ ПК2]; иначе, <c>false</c>.
            /// </value>
            public bool IsOnOff2
            {
                get
                {
                    return _isOnOff2;
                }

                set
                {
                    _isOnOff2 = value;
                    ControlValuesList[Global.HSI.Line2].SetProperty(Global.HSI.Line2.OnOff, Convert.ToInt32(value));
                    FirePropertyChangedEvent("IsOnOff2");
                }
            }

            public int StateCounter1
            {
                get
                {
                    return _stateCounter1;
                }

                set
                {
                    _stateCounter1 = value;
                    ControlValuesList[Global.HSI.Line1StateCounter].SetProperty(Global.HSI.Line1StateCounter, Convert.ToInt32(value));
                    FirePropertyChangedEvent("StateCounter1");
                }
            }
            public int FrameCounter1
            {
                get
                {
                    return _frameCounter1;
                }

                set
                {
                    _frameCounter1 = value;
                    ControlValuesList[Global.HSI.Line1FrameCounter].SetProperty(Global.HSI.Line1FrameCounter, Convert.ToInt32(value));
                    FirePropertyChangedEvent("FrameCounter1");
                }
            }

            public int StateCounter2
            {
                get
                {
                    return _stateCounter2;
                }

                set
                {
                    _stateCounter2 = value;
                    ControlValuesList[Global.HSI.Line2StateCounter].SetProperty(Global.HSI.Line2StateCounter, Convert.ToInt32(value));
                    FirePropertyChangedEvent("StateCounter2");
                }
            }

            public int FrameCounter2
            {
                get
                {
                    return _frameCounter2;
                }

                set
                {
                    _frameCounter2 = value;
                    ControlValuesList[Global.HSI.Line2FrameCounter].SetProperty(Global.HSI.Line2FrameCounter, Convert.ToInt32(value));
                    FirePropertyChangedEvent("FrameCounter2");
                }
            }

            public int LineIn
            {
                get
                {
                    return _lineIn;
                }

                set
                {
                    _lineIn = value;
                    ControlValuesList[Global.SimHSI.Control].SetProperty(Global.SimHSI.Control.LineIn, Convert.ToInt32(value));
                    FirePropertyChangedEvent("LineIn");
                }
            }

            public int LineOut
            {
                get
                {
                    return _lineOut;
                }

                set
                {
                    _lineOut = value;
                    ControlValuesList[Global.SimHSI.Control].SetProperty(Global.SimHSI.Control.LineOut, Convert.ToInt32(value));
                    FirePropertyChangedEvent("LineOut");
                }
            }

            public bool IsRequest
            {
                get
                {
                    return _isRequest;
                }

                set
                {
                    _isRequest = value;
                    ControlValuesList[Global.SimHSI.Control].SetProperty(Global.SimHSI.Control.Request, Convert.ToInt32(value));
                    FirePropertyChangedEvent("IsRequest");
                }
            }

            public bool IsRecordSend
            {
                get
                {
                    return _isRecordSend;
                }

                set
                {
                    _isRecordSend = value;
                    ControlValuesList[Global.SimHSI.Record].SetProperty(Global.SimHSI.Record.Send, Convert.ToInt32(value));
                    FirePropertyChangedEvent("IsRecordSend");
                }
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
            /// Телеметрия: Питание УФЕС ОСН.
            /// </summary>
            private bool _ufesPower1;

            /// <summary>
            /// Телеметрия: Питание УФЕС РЕЗ.
            /// </summary>
            private bool _ufesPower2;

            /// <summary>
            /// Телеметрия: Питание ВУФЕС ОСН.
            /// </summary>
            private bool _vufesPower1;

            /// <summary>
            /// Телеметрия: Питание ВУФЕС РЕЗ.
            /// </summary>
            private bool _vufesPower2;

            /// <summary>
            /// Телеметрия: Питание СДЩ ОСН.
            /// </summary>
            private bool _sdchshPower1;

            /// <summary>
            /// Телеметрия: Питание СДЩ РЕЗ.
            /// </summary>
            private bool _sdchshPower2;

            /// <summary>
            /// Телеметрия: Подсветка УФЕС ОСН.
            /// </summary>
            private bool _ufesLight1;

            /// <summary>
            /// Телеметрия: Подсветка УФЕС РЕЗ.
            /// </summary>
            private bool _ufesLight2;

            /// <summary>
            /// Телеметрия: Подсветка ВУФЕС ОСН.
            /// </summary>
            private bool _vufesLight1;

            /// <summary>
            /// Телеметрия: Подсветка ВУФЕС РЕЗ.
            /// </summary>
            private bool _vufesLight2;

            /// <summary>
            /// Телеметрия: Подсветка СДЩ ОСН.
            /// </summary>
            private bool _sdchshLight1;

            /// <summary>
            /// Телеметрия: Подсветка СДЩ РЕЗ.
            /// </summary>
            private bool _sdchshLight2;

            /// <summary>
            /// Телеметрия: Затвор УФЕС ОСН.
            /// </summary>
            private bool _ufesLock1;

            /// <summary>
            /// Телеметрия: Затвор УФЕС РЕЗ.
            /// </summary>
            private bool _ufesLock2;

            /// <summary>
            /// Телеметрия: Затвор ВУФЕС ОСН.
            /// </summary>
            private bool _vufesLock1;

            /// <summary>
            /// Телеметрия: Затвор ВУФЕС РЕЗ.
            /// </summary>
            private bool _vufesLock2;

            /// <summary>
            /// Телеметрия: Затвор СДЩ ОСН.
            /// </summary>
            private bool _sdchshLock1;

            /// <summary>
            /// Телеметрия: Затвор СДЩ РЕЗ.
            /// </summary>
            private bool _sdchshLock2;

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
            /// Получает или задает значение, показывающее, есть ли [питание первого полукомплекта БУСК].
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

                set
                {
                    _powerBusk1 = value;                   
                    FirePropertyChangedEvent("PowerBusk1");
                }
            }

            /// <summary>
            /// Получает или задает значение, показывающее, есть ли [питание второго полукомплекта БУСК].
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

                set
                {
                    _powerBusk2 = value;
                    FirePropertyChangedEvent("PowerBusk2");
                }
            }

            /// <summary>
            /// Получает или задает значение, показывающее, есть ли [питание первого полукомплекта БУНД].
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

                set
                {
                    _powerBund1 = value;
                    FirePropertyChangedEvent("PowerBund1");
                }
            }

            /// <summary>
            /// Получает или задает значение, показывающее, есть ли [питание второго полукомплекта БУНД].
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

                set
                {
                    _powerBund2 = value;
                    FirePropertyChangedEvent("PowerBund2");
                }
            }

            /// <summary>
            /// Получает или задает значение, показывающее, включен ли [затвор прибора УФЕС ОСН].
            /// </summary>
            /// <value>
            ///   <c>true</c> если [затвор прибора УФЕС ОСН] включен; иначе, <c>false</c>.
            /// </value>
            public bool UfesLock1 
            { 
                get
                {
                    return _ufesLock1;
                }

                set
                {
                    _ufesLock1 = value;
                    ControlValuesList[Global.Telemetry].SetProperty(Global.Telemetry.UfesLock1, Convert.ToInt32(value));
                    FirePropertyChangedEvent("UfesLock1");
                }
            }

            /// <summary>
            /// Получает или задает значение, показывающее, включен ли [затвор прибора УФЕС РЕЗ].
            /// </summary>
            /// <value>
            ///   <c>true</c> если [затвор прибора УФЕС РЕЗ] включен; иначе, <c>false</c>.
            /// </value>
            public bool UfesLock2
            {
                get
                {
                    return _ufesLock2;
                }

                set
                {
                    _ufesLock2 = value;
                    ControlValuesList[Global.Telemetry].SetProperty(Global.Telemetry.UfesLock2, Convert.ToInt32(value));
                    FirePropertyChangedEvent("UfesLock2");
                }
            }

            /// <summary>
            /// Получает или задает значение, показывающее, включен ли [затвор прибора ВУФЕС ОСН].
            /// </summary>
            /// <value>
            ///   <c>true</c> если [затвор прибора ВУФЕС ОСН] включен; иначе, <c>false</c>.
            /// </value>
            public bool VufesLock1
            {
                get
                {
                    return _vufesLock1;
                }

                set
                {
                    _vufesLock1 = value;
                    ControlValuesList[Global.Telemetry].SetProperty(Global.Telemetry.VufesLock1, Convert.ToInt32(value));
                    FirePropertyChangedEvent("VufesLock1");
                }
            }

            /// <summary>
            /// Получает или задает значение, показывающее, включен ли [затвор прибора ВУФЕС РЕЗ].
            /// </summary>
            /// <value>
            ///   <c>true</c> если [затвор прибора ВУФЕС РЕЗ] включен; иначе, <c>false</c>.
            /// </value>
            public bool VufesLock2
            {
                get
                {
                    return _vufesLock2;
                }

                set
                {
                    _vufesLock2 = value;
                    ControlValuesList[Global.Telemetry].SetProperty(Global.Telemetry.VufesLock2, Convert.ToInt32(value));
                    FirePropertyChangedEvent("VufesLock2");
                }
            }

            /// <summary>
            /// Получает или задает значение, показывающее, включен ли [затвор прибора СДЩ ОСН].
            /// </summary>
            /// <value>
            ///   <c>true</c> если [затвор прибора СДЩ ОСН] включен; иначе, <c>false</c>.
            /// </value>
            public bool SdchshLock1
            {
                get
                {
                    return _sdchshLock1;
                }

                set
                {
                    _sdchshLock1 = value;
                    ControlValuesList[Global.Telemetry].SetProperty(Global.Telemetry.SdchshLock1, Convert.ToInt32(value));
                    FirePropertyChangedEvent("SdchshLock1");
                }
            }

            /// <summary>
            /// Получает или задает значение, показывающее, включен ли [затвор прибора СДЩ РЕЗ].
            /// </summary>
            /// <value>
            ///   <c>true</c> если [затвор прибора СДЩ РЕЗ] включен; иначе, <c>false</c>.
            /// </value>
            public bool SdchshLock2
            {
                get
                {
                    return _sdchshLock2;
                }

                set
                {
                    _sdchshLock2 = value;
                    ControlValuesList[Global.Telemetry].SetProperty(Global.Telemetry.SdchshLock2, Convert.ToInt32(value));
                    FirePropertyChangedEvent("SdchshLock2");
                }
            }

            /// <summary>
            /// Получает значение, показывающее, есть ли [питание УФЕС ОСН].
            /// </summary>
            /// <value>
            ///   <c>true</c> если [питание УФЕС ОСН]; иначе, <c>false</c>.
            /// </value>
            public bool UfesPower1
            {
                get
                {
                    return _ufesPower1;
                }

                private set
                {
                    _ufesPower1 = value;
                    FirePropertyChangedEvent("UfesPower1");
                }
            }

            /// <summary>
            /// Получает значение, показывающее, есть ли [питание УФЕС РЕЗ].
            /// </summary>
            /// <value>
            ///   <c>true</c> если [питание УФЕС РЕЗ]; иначе, <c>false</c>.
            /// </value>
            public bool UfesPower2
            {
                get
                {
                    return _ufesPower2;
                }

                private set
                {
                    _ufesPower2 = value;
                    FirePropertyChangedEvent("UfesPower2");
                }
            }

            /// <summary>
            /// Получает значение, показывающее, есть ли [питание ВУФЕС ОСН].
            /// </summary>
            /// <value>
            ///   <c>true</c> если [питание ВУФЕС ОСН]; иначе, <c>false</c>.
            /// </value>
            public bool VufesPower1
            {
                get
                {
                    return _vufesPower1;
                }

                private set
                {
                    _vufesPower1 = value;
                    FirePropertyChangedEvent("VufesPower1");
                }
            }

            /// <summary>
            /// Получает значение, показывающее, есть ли [питание ВУФЕС РЕЗ].
            /// </summary>
            /// <value>
            ///   <c>true</c> если [питание ВУФЕС РЕЗ]; иначе, <c>false</c>.
            /// </value>
            public bool VufesPower2
            {
                get
                {
                    return _vufesPower2;
                }

                private set
                {
                    _vufesPower2 = value;
                    FirePropertyChangedEvent("VufesPower2");
                }
            }

            /// <summary>
            /// Получает значение, показывающее, есть ли [питание СДЩ ОСН].
            /// </summary>
            /// <value>
            ///   <c>true</c> если [питание СДЩ ОСН]; иначе, <c>false</c>.
            /// </value>
            public bool SdchshPower1
            {
                get
                {
                    return _sdchshPower1;
                }

                private set
                {
                    _sdchshPower1 = value;
                    FirePropertyChangedEvent("SdchshPower1");
                }
            }

            /// <summary>
            /// Получает значение, показывающее, есть ли [питание СДЩ РЕЗ].
            /// </summary>
            /// <value>
            ///   <c>true</c> если [питание СДЩ РЕЗ]; иначе, <c>false</c>.
            /// </value>
            public bool SdchshPower2
            {
                get
                {
                    return _sdchshPower2;
                }

                private set
                {
                    _sdchshPower2 = value;
                    FirePropertyChangedEvent("SdchshPower2");
                }
            }

            /// <summary>
            /// Получает значение, показывающее, есть ли [подсветка УФЕС ОСН].
            /// </summary>
            /// <value>
            ///   <c>true</c> если [подсветка УФЕС ОСН]; иначе, <c>false</c>.
            /// </value>
            public bool UfesLight1
            {
                get
                {
                    return _ufesLight1;
                }

                private set
                {
                    _ufesLight1 = value;
                    FirePropertyChangedEvent("UfesLight1");
                }
            }

            /// <summary>
            /// Получает значение, показывающее, есть ли [подсветка УФЕС РЕЗ].
            /// </summary>
            /// <value>
            ///   <c>true</c> если [подсветка УФЕС РЕЗ]; иначе, <c>false</c>.
            /// </value>
            public bool UfesLight2
            {
                get
                {
                    return _ufesLight2;
                }

                private set
                {
                    _ufesLight2 = value;
                    FirePropertyChangedEvent("UfesLight2");
                }
            }

            /// <summary>
            /// Получает значение, показывающее, есть ли [подсветка ВУФЕС ОСН].
            /// </summary>
            /// <value>
            ///   <c>true</c> если [подсветка ВУФЕС ОСН]; иначе, <c>false</c>.
            /// </value>
            public bool VufesLight1
            {
                get
                {
                    return _vufesLight1;
                }

                private set
                {
                    _vufesLight1 = value;
                    FirePropertyChangedEvent("VufesLight1");
                }
            }
            
            /// <summary>
            /// Получает значение, показывающее, есть ли [подсветка ВУФЕС РЕЗ].
            /// </summary>
            /// <value>
            ///   <c>true</c> если [подсветка ВУФЕС РЕЗ]; иначе, <c>false</c>.
            /// </value>
            public bool VufesLight2
            {
                get
                {
                    return _vufesLight2;
                }

                private set
                {
                    _vufesLight2 = value;
                    FirePropertyChangedEvent("VufesLight2");
                }
            }

            /// <summary>
            /// Получает значение, показывающее, есть ли [подсветка СДЩ ОСН].
            /// </summary>
            /// <value>
            ///   <c>true</c> если [подсветка СДЩ ОСН]; иначе, <c>false</c>.
            /// </value>
            public bool SdchshLight1
            {
                get
                {
                    return _sdchshLight1;
                }

                private set
                {
                    _sdchshLight1 = value;
                    FirePropertyChangedEvent("SdchshLight1");
                }
            }

            /// <summary>
            /// Получает значение, показывающее, есть ли [подсветка СДЩ РЕЗ].
            /// </summary>
            /// <value>
            ///   <c>true</c> если [подсветка СДЩ РЕЗ]; иначе, <c>false</c>.
            /// </value>
            public bool SdchshLight2
            {
                get
                {
                    return _sdchshLight2;
                }

                private set
                {
                    _sdchshLight2 = value;
                    FirePropertyChangedEvent("SdchshLight2");
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
                ControlValuesList[Global.Telemetry].AddProperty(Global.Telemetry.PowerBusk1, 15, 1, Device.CmdPowerBusk1, value => PowerBusk1 = 1 == value);
                ControlValuesList[Global.Telemetry].AddProperty(Global.Telemetry.PowerBusk2, 14, 1, Device.CmdPowerBusk2, value => PowerBusk2 = 1 == value);
                ControlValuesList[Global.Telemetry].AddProperty(Global.Telemetry.PowerBund1, 12, 1, Device.CmdPowerBund1, value => PowerBund1 = 1 == value);
                ControlValuesList[Global.Telemetry].AddProperty(Global.Telemetry.PowerBund2, 13, 1, Device.CmdPowerBund2, value => PowerBund2 = 1 == value);
                ControlValuesList[Global.Telemetry].AddProperty(Global.Telemetry.UfesLight1, 6, 1, delegate { }, value => UfesLight1 = 1 == value);
                ControlValuesList[Global.Telemetry].AddProperty(Global.Telemetry.UfesLight2, 7, 1, delegate { }, value => UfesLight2 = 1 == value);
                ControlValuesList[Global.Telemetry].AddProperty(Global.Telemetry.VufesLight1, 8, 1, delegate { }, value => VufesLight1 = 1 == value);
                ControlValuesList[Global.Telemetry].AddProperty(Global.Telemetry.VufesLight2, 9, 1, delegate { }, value => VufesLight2 = 1 == value);
                ControlValuesList[Global.Telemetry].AddProperty(Global.Telemetry.SdchshLight1, 10, 1, delegate { }, value => SdchshLight1 = 1 == value);
                ControlValuesList[Global.Telemetry].AddProperty(Global.Telemetry.SdchshLight2, 11, 1, delegate { }, value => SdchshLight2 = 1 == value);
                ControlValuesList[Global.Telemetry].AddProperty(Global.Telemetry.UfesPower1, 20, 1, delegate { }, value => UfesPower1 = 1 == value);
                ControlValuesList[Global.Telemetry].AddProperty(Global.Telemetry.UfesPower2, 21, 1, delegate { }, value => UfesPower2 = 1 == value);
                ControlValuesList[Global.Telemetry].AddProperty(Global.Telemetry.VufesPower1, 19, 1, delegate { }, value => VufesPower1 = 1 == value);
                ControlValuesList[Global.Telemetry].AddProperty(Global.Telemetry.VufesPower2, 18, 1, delegate { }, value => VufesPower2 = 1 == value);
                ControlValuesList[Global.Telemetry].AddProperty(Global.Telemetry.SdchshPower1, 17, 1, delegate { }, value => SdchshPower1 = 1 == value);
                ControlValuesList[Global.Telemetry].AddProperty(Global.Telemetry.SdchshPower2, 16, 1, delegate { }, value => SdchshPower2 = 1 == value);
                ControlValuesList[Global.Telemetry].AddProperty(Global.Telemetry.UfesLock1, 0, 1, Device.CmdUfesLock1, value => UfesLock1 = 1 == value);
                ControlValuesList[Global.Telemetry].AddProperty(Global.Telemetry.UfesLock2, 1, 1, Device.CmdUfesLock2, value => UfesLock2 = 1 == value);
                ControlValuesList[Global.Telemetry].AddProperty(Global.Telemetry.VufesLock1, 2, 1, Device.CmdVufesLock1, value => VufesLock1 = 1 == value);
                ControlValuesList[Global.Telemetry].AddProperty(Global.Telemetry.VufesLock2, 3, 1, Device.CmdVufesLock2, value => VufesLock2 = 1 == value);
                ControlValuesList[Global.Telemetry].AddProperty(Global.Telemetry.SdchshLock1, 4, 1, Device.CmdSdchshLock1, value => SdchshLock1 = 1 == value);
                ControlValuesList[Global.Telemetry].AddProperty(Global.Telemetry.SdchshLock2, 5, 1, Device.CmdSdchshLock2, value => SdchshLock2 = 1 == value);
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
            /// Количество запросов квот по spacewire.
            /// </summary>
            private long _spacewire2RequestQueueFromBusk;

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
            private bool _isEnable;

            /// <summary>
            /// Управление: Установлена связь.
            /// </summary>
            private bool _isConnect;

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
            private SpacewireChannel _simRouterChannel;
            private long _spacewire2ReplyQueueFromBusk;
            private long _spacewire2ReplyQueueFromBuk;
            private long _spacewire2RequestQueueFromBuk;
            private long _spacewire2Kbv;
            private int _curApid;

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

            public long Spacewire2RequestQueueFromBusk
            {
                get
                {
                    return _spacewire2RequestQueueFromBusk;
                }

                set
                {
                    _spacewire2RequestQueueFromBusk = value;
                    FirePropertyChangedEvent("Spacewire2RequestQueueFromBusk");
                }
            }

            public long Spacewire2ReplyQueueFromBusk
            {
                get
                {
                    return _spacewire2ReplyQueueFromBusk;
                }

                set
                {
                    _spacewire2ReplyQueueFromBusk = value;
                    FirePropertyChangedEvent("Spacewire2ReplyQueueFromBusk");
                }
            }

            public long Spacewire2RequestQueueFromBuk
            {
                get
                {
                    return _spacewire2RequestQueueFromBuk;
                }

                set
                {
                    _spacewire2RequestQueueFromBuk = value;
                    FirePropertyChangedEvent("Spacewire2RequestQueueFromBuk");
                }
            }

            public long Spacewire2ReplyQueueFromBuk
            {
                get
                {
                    return _spacewire2ReplyQueueFromBuk;
                }

                set
                {
                    _spacewire2ReplyQueueFromBuk = value;
                    FirePropertyChangedEvent("Spacewire2ReplyQueueFromBuk");
                }
            }

            public long Spacewire2Kbv
            {
                get
                {
                    return _spacewire2Kbv;
                }

                set
                {
                    _spacewire2Kbv = value;
                    FirePropertyChangedEvent("Spacewire2Kbv");
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
                    return SpacewireChannel.BUK1BM1 == SelectSimRouterChannel;
                }

                set
                {
                    SelectSimRouterChannel = value ? SpacewireChannel.BUK1BM1 : SelectSimRouterChannel;
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
                    return SpacewireChannel.BUK1BM2 == SelectSimRouterChannel;
                }

                set
                {
                    SelectSimRouterChannel = value ? SpacewireChannel.BUK1BM2 : SelectSimRouterChannel;
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
                    return SpacewireChannel.BUK2BM1 == SelectSimRouterChannel;
                }

                set
                {
                    SelectSimRouterChannel = value ? SpacewireChannel.BUK2BM1 : SelectSimRouterChannel;
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
                    return SpacewireChannel.BUK2BM2 == SelectSimRouterChannel;
                }

                set
                {
                    SelectSimRouterChannel = value ? SpacewireChannel.BUK2BM2 : SelectSimRouterChannel;
                }
            }

            /// <summary>
            /// Получает или задает канал имитатора БМ-4.
            /// </summary>
            /// <value>
            /// Канал имитатора БМ-4.
            /// </value>
            public SpacewireChannel SelectSimRouterChannel
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
            /// Получает или задает значение, показывающее, что [интерфейс Spacewire включен].
            /// </summary>
            /// <value>
            /// <c>true</c> если [интерфейс Spacewire включен]; иначе, <c>false</c>.
            /// </value>
            public bool IsEnable
            {
                get
                {
                    return _isEnable;
                }

                set
                {
                    _isEnable = value;
                    ControlValuesList[Global.Spacewire2.Control].SetProperty(Global.Spacewire2.Control.Enable, Convert.ToInt32(value));
                    FirePropertyChangedEvent("IsEnable");
                }
            }

            /// <summary>
            /// Получает или задает значение, показывающее, что [связь по интерфейсу Spacewire установлена].
            /// </summary>
            /// <value>
            /// <c>true</c> если [связь по интерфейсу Spacewire установлена]; иначе, <c>false</c>.
            /// </value>
            public bool IsConnect
            {
                get
                {
                    return _isConnect;
                }

                set
                {
                    _isConnect = value;
                    ControlValuesList[Global.Spacewire2.Control].SetProperty(Global.Spacewire2.Control.Connect, Convert.ToInt32(value));
                    FirePropertyChangedEvent("IsConnect");
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

            public int CurApid
            {
                get
                {
                    return _curApid;
                }

                set
                {
                    _curApid = value;
                    FirePropertyChangedEvent("CurApid");
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
                ControlValuesList[Global.Spacewire2.Control].AddProperty(Global.Spacewire2.Control.Channel, 1, 2, Device.CmdSpacewire2Control, value => SelectSimRouterChannel = (SpacewireChannel)value);
                ControlValuesList[Global.Spacewire2.Control].AddProperty(Global.Spacewire2.Control.Enable, 0, 1, Device.CmdSpacewire2Control, value => IsEnable = 1 == value);
                ControlValuesList[Global.Spacewire2.Control].AddProperty(Global.Spacewire2.Control.Connect, 3, 1, delegate { }, value => IsConnect = 1 == value);
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
            /// Рабочий прибор.
            /// </summary>
            private WorkDevice _workDevice;

            /// <summary>
            /// Управление: вкл/выкл интерфейса Spacewire.
            /// </summary>
            private bool _isEnable;

            /// <summary>
            /// Управление: Установлена связь.
            /// </summary>
            private bool _isConnect;

            /// <summary>
            /// Управление: Сигнал передачи кадров.
            /// </summary>
            private bool _isTransmission;

            /// <summary>
            /// Полукомплект рабочего прибора.
            /// </summary>
            private HalfSet _workDeviceHalfSet;
            private ICommand _enableCommand;

            /// <summary>
            /// Инициализирует новый экземпляр класса <see cref="Spacewire3" />.
            /// </summary>
            /// <param name="owner">The owner.</param>
            public Spacewire3(EgseBukNotify owner)
                : base(owner)
            {
            }

            /// <summary>
            /// Получает или задает значение, показывающее, что [интерфейс Spacewire включен].
            /// </summary>
            /// <value>
            /// <c>true</c> если [интерфейс SpaceWire включен]; иначе, <c>false</c>.
            /// </value>
            public bool IsEnable
            {
                get
                {
                    return _isEnable;
                }

                set
                {
                    _isEnable = value;
                    ControlValuesList[Global.Spacewire3.Control].SetProperty(Global.Spacewire3.Control.Enable, Convert.ToInt32(value));
                    ControlValuesList[Global.Spacewire4.Control].SetProperty(Global.Spacewire4.Control.Connect, Convert.ToInt32(0));
                    FirePropertyChangedEvent("IsEnable");
                }
            }

            public ICommand EnableCommand
            {
                get
                {
                    if (_enableCommand == null)
                    {
                        _enableCommand = new RelayCommand(
                            param => this.Enable(),
                            param => this.CanEnable()
                        );
                    }
                    return _enableCommand;
                }
            }

            private bool CanEnable()
            {
                return true;
            }

            private void Enable()
            {
                IsEnable = !IsEnable;
            }

            /// <summary>
            /// Получает или задает значение, показывающее, что [связь по интерфейсу Spacewire установлена].
            /// </summary>
            /// <value>
            /// <c>true</c> если [связь по интерфейсу SpaceWire установлена]; иначе, <c>false</c>.
            /// </value>
            public bool IsConnect
            {
                get
                {
                    return _isConnect;
                }

                set
                {
                    _isConnect = value;
                    ControlValuesList[Global.Spacewire3.Control].SetProperty(Global.Spacewire3.Control.Connect, Convert.ToInt32(value));
                    FirePropertyChangedEvent("IsConnect");
                }
            }

            /// <summary>
            /// Получает или задает значение, показывающее, что [выбрано рабочее устройство УФЕС].
            /// </summary>
            /// <value>
            ///   <c>true</c> если [выбрано рабочее устройство УФЕС]; иначе, <c>false</c>.
            /// </value>
            public bool IsUfesDevice
            {
                get
                {
                    return WorkDevice.Ufes == SelectWorkDevice;
                }

                set
                {
                    SelectWorkDevice = value ? WorkDevice.Ufes : SelectWorkDevice;
                }
            }

            /// <summary>
            /// Получает или задает значение, показывающее, что [выбрано рабочее устройство ВУФЕС].
            /// </summary>
            /// <value>
            ///   <c>true</c> если [выбрано рабочее устройство ВУФЕС]; иначе, <c>false</c>.
            /// </value>
            public bool IsVufesDevice
            {
                get
                {
                    return WorkDevice.Vufes == SelectWorkDevice;
                }

                set
                {
                    SelectWorkDevice = value ? WorkDevice.Vufes : SelectWorkDevice;
                }
            }

            /// <summary>
            /// Получает или задает значение, показывающее, что [выбрано рабочее устройство СДЩ].
            /// </summary>
            /// <value>
            ///   <c>true</c> если [выбрано рабочее устройство СДЩ]; иначе, <c>false</c>.
            /// </value>
            public bool IsSdchshDevice
            {
                get
                {
                    return WorkDevice.Sdchsh == SelectWorkDevice;
                }

                set
                {
                    SelectWorkDevice = value ? WorkDevice.Sdchsh : SelectWorkDevice;
                }
            }

            /// <summary>
            /// Получает или задает значение, показывающее, что [передаются кадры данных].
            /// </summary>
            /// <value>
            ///   <c>true</c> если [передаются кадры данных]; иначе, <c>false</c>.
            /// </value>
            public bool IsTransmission
            {
                get
                {                  
                    return _isTransmission;
                }

                set
                {
                    _isTransmission = value;
                    FirePropertyChangedEvent("IsTransmission");
                }
            }

            /// <summary>
            /// Получает или задает значение, показывающее, что [выбран первый полукомплект].
            /// </summary>
            /// <value>
            ///   <c>true</c> если [выбран первый полукомплект]; иначе, <c>false</c>.
            /// </value>
            public bool IsFirstHalfSet
            {
                get
                {
                    return HalfSet.First == SelectHalfSet;
                }

                set
                {
                    SelectHalfSet = value ? HalfSet.First : SelectHalfSet;
                }
            }

            /// <summary>
            /// Получает или задает значение, показывающее, что [выбран второй полукомплект].
            /// </summary>
            /// <value>
            ///   <c>true</c> если [выбран второй полукомплект]; иначе, <c>false</c>.
            /// </value>
            public bool IsSecondHalfSet
            {
                get
                {
                    return HalfSet.Second == SelectHalfSet;
                }

                set
                {
                    SelectHalfSet = value ? HalfSet.Second : SelectHalfSet;
                }
            }

            /// <summary>
            /// Получает или задает полукомплект прибора.
            /// </summary>
            /// <value>
            /// Полукомплект прибора.
            /// </value>
            public HalfSet SelectHalfSet
            {
                get
                {
                    return _workDeviceHalfSet;
                }

                set
                {
                    _workDeviceHalfSet = value;
                    ControlValuesList[Global.Spacewire3.Control].SetProperty(Global.Spacewire3.Control.HalfSet, (int)value);
                    FirePropertyChangedEvent("SelectHalfSet");
                    FirePropertyChangedEvent("IsFirstHalfSet");
                    FirePropertyChangedEvent("IsSecondHalfSet");
                }
            }

            /// <summary>
            /// Получает или задает рабочий прибор.
            /// </summary>
            /// <value>
            /// Рабочий прибор.
            /// </value>
            public WorkDevice SelectWorkDevice
            {
                get
                {
                    return _workDevice;
                }

                set
                {
                    _workDevice = value;
                    ControlValuesList[Global.Spacewire3.Control].SetProperty(Global.Spacewire3.Control.WorkDevice, (int)value);
                    FirePropertyChangedEvent("SelectWorkDevice");
                    FirePropertyChangedEvent("IsUfesDevice");
                    FirePropertyChangedEvent("IsVufesDevice");
                    FirePropertyChangedEvent("IsSdchshDevice");
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
                ControlValuesList.Add(Global.Spacewire3.Control, new ControlValue());
            }

            /// <summary>
            /// Initializes the properties.
            /// </summary>
            protected override void InitProperties()
            {                
                ControlValuesList[Global.Spacewire3.Control].AddProperty(Global.Spacewire3.Control.Enable, 0, 1, Device.CmdSpacewire3Control, value => IsEnable = 1 == value);
                ControlValuesList[Global.Spacewire3.Control].AddProperty(Global.Spacewire3.Control.Connect, 3, 1, delegate { }, value => IsConnect = 1 == value);
                ControlValuesList[Global.Spacewire3.Control].AddProperty(Global.Spacewire3.Control.Transmission, 5, 1, delegate { }, value => IsTransmission = 1 == value);
                ControlValuesList[Global.Spacewire3.Control].AddProperty(Global.Spacewire3.Control.HalfSet, 4, 1, Device.CmdSpacewire3Control, value => SelectHalfSet = (HalfSet)value);
                ControlValuesList[Global.Spacewire3.Control].AddProperty(Global.Spacewire3.Control.WorkDevice, 1, 2, Device.CmdSpacewire3Control, value => SelectWorkDevice = (WorkDevice)value);
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
            private bool _isEnable;

            /// <summary>
            /// Управление: Установлена связь.
            /// </summary>
            private bool _isConnect;

            /// <summary>
            /// Управление: Включение метки времени (1 Гц).
            /// </summary>
            private bool _isIssueTimeMark;

            /// <summary>
            /// Запись данных(до 1 Кбайт): EEP или EOP.
            /// </summary>
            private bool _isIssueEEP;

            /// <summary>
            /// Запись данных(до 1 Кбайт): Выдача в конце посылки EOP или EEP.
            /// </summary>
            private bool _isIssueEOP;

            /// <summary>
            /// Запись данных(до 1 Кбайт): Автоматическая выдача.
            /// </summary>
            private bool _isIssueAuto;

            /// <summary>
            /// Запись данных(до 1 Кбайт): Бит занятости.
            /// </summary>
            private bool _isRecordBusy;

            /// <summary>
            /// Запись данных(до 1 Кбайт): Бит занятости.
            /// </summary>
            private bool _isIssuePackage;

            private ICommand _enableCommand;
            private ICommand _issueTimeMarkCommand;
            private ICommand _issueEEPCommand;
            private ICommand _issueEOPCommand;
            private ICommand _issuePackageCommand;
            private ICommand _issueAutoCommand;

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
            /// <c>true</c> если [интерфейс SpaceWire включен]; иначе, <c>false</c>.
            /// </value>
            public bool IsEnable
            {
                get
                {
                    return _isEnable;
                }

                set
                {
                    _isEnable = value;
                    ControlValuesList[Global.Spacewire4.Control].SetProperty(Global.Spacewire4.Control.Enable, Convert.ToInt32(value));
                    ControlValuesList[Global.Spacewire3.Control].SetProperty(Global.Spacewire3.Control.Connect, Convert.ToInt32(0));
                    FirePropertyChangedEvent("IsEnable");
                }
            }

            public ICommand EnableCommand
            {
                get
                {
                    if (null == _enableCommand)
                    {
                        _enableCommand = new RelayCommand(obj => { IsEnable = !IsEnable; }, obj => { return true; });
                    }
                    return _enableCommand;
                }
            }

            /// <summary>
            /// Получает или задает значение, показывающее, что [связь по интерфейсу Spacewire установлена].
            /// </summary>
            /// <value>
            /// <c>true</c> если [связь по интерфейсу SpaceWire установлена]; иначе, <c>false</c>.
            /// </value>
            public bool IsConnect
            {
                get
                {
                    return _isConnect;
                }

                set
                {
                    _isConnect = value;
                    ControlValuesList[Global.Spacewire4.Control].SetProperty(Global.Spacewire4.Control.Connect, Convert.ToInt32(value));
                    FirePropertyChangedEvent("IsConnect");
                }
            }

            /// <summary>
            /// Получает или задает значение, показывающее, что [выдается метка времени].
            /// </summary>
            /// <value>
            /// <c>true</c> если [выдается метка времени]; иначе, <c>false</c>.
            /// </value>
            public bool IsIssueTimeMark
            {
                get
                {
                    return _isIssueTimeMark;
                }

                set
                {
                    _isIssueTimeMark = value;
                    ControlValuesList[Global.Spacewire4.Control].SetProperty(Global.Spacewire4.Control.TimeMark, Convert.ToInt32(_isIssueTimeMark));
                    FirePropertyChangedEvent("IsIssueTimeMark");
                }
            }

            public ICommand IssueTimeMarkCommand
            {
                get
                {
                    if (_issueTimeMarkCommand == null)
                    {
                        _issueTimeMarkCommand = new RelayCommand(obj => { IsIssueTimeMark = !IsIssueTimeMark; }, obj => { return true; });
                    }
                    return _issueTimeMarkCommand;
                }
            }
            
            /// <summary>
            /// Получает или задает значение, показывающее, что [выдается EEP].
            /// </summary>
            /// <value>
            /// <c>true</c> если [выдается EEP]; иначе, <c>false</c>.
            /// </value>
            public bool IsIssueEEP
            {
                get
                {
                    return _isIssueEEP;
                }

                set
                {
                    _isIssueEEP = value;
                    ControlValuesList[Global.Spacewire4.Record].SetProperty(Global.Spacewire4.Record.IssueEEP, Convert.ToInt32(value));
                    FirePropertyChangedEvent("IsIssueEEP");
                }
            }

            public ICommand IssueEEPCommand
            {
                get
                {
                    if (_issueEEPCommand == null)
                    {
                        _issueEEPCommand = new RelayCommand(obj => { IsIssueEEP = !IsIssueEEP; }, obj => { return true; });
                    }
                    return _issueEEPCommand;
                }
            }

            /// <summary>
            /// Получает или задает значение, показывающее, что [выдается EOP].
            /// </summary>
            /// <value>
            /// <c>true</c> если [выдается EOP]; иначе, <c>false</c>.
            /// </value>
            public bool IsIssueEOP
            {
                get
                {
                    return _isIssueEOP;
                }

                set
                {
                    _isIssueEOP = value;
                    ControlValuesList[Global.Spacewire4.Record].SetProperty(Global.Spacewire4.Record.EOPSend, Convert.ToInt32(value));
                    FirePropertyChangedEvent("IsIssueEOP");
                }
            }

            public ICommand IssueEOPCommand
            {
                get
                {
                    if (_issueEOPCommand == null)
                    {
                        _issueEOPCommand = new RelayCommand(obj => { IsIssueEOP = !IsIssueEOP; }, obj => { return true; });
                    }
                    return _issueEOPCommand;
                }
            }

            /// <summary>
            /// Получает или задает значение, показывающее, что [включена автоматическая выдача посылки].
            /// </summary>
            /// <value>
            /// <c>true</c> если [включена автоматическая выдача посылки]; иначе, <c>false</c>.
            /// </value>
            public bool IsIssueAuto
            {
                get
                {
                    return _isIssueAuto;
                }

                set
                {
                    _isIssueAuto = value;
                    ControlValuesList[Global.Spacewire4.Record].SetProperty(Global.Spacewire4.Record.IssueAuto, Convert.ToInt32(value));
                    FirePropertyChangedEvent("IsIssueAuto");
                }
            }

            public ICommand IssueAutoCommand
            {
                get
                {
                    if (_issueAutoCommand == null)
                    {
                        _issueAutoCommand = new RelayCommand(obj => { IsIssueAuto = !IsIssueAuto; }, obj => { return true; });
                    }
                    return _issueAutoCommand;
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
                    return _isRecordBusy || IsIssuePackage;
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
            public bool IsIssuePackage
            {
                get
                {
                    return _isIssuePackage;
                }

                set
                {
                    _isIssuePackage = value;
                    ControlValuesList[Global.Spacewire4.Record].SetProperty(Global.Spacewire4.Record.IssuePackage, Convert.ToInt32(value));
                    FirePropertyChangedEvent("IsIssuePackage");                   
                }
            }
            public ICommand IssuePackageCommand
            {
                get
                {
                    if (_issuePackageCommand == null)
                    {
                        _issuePackageCommand = new RelayCommand(obj => { IsIssuePackage = true; }, obj => { return !IsRecordBusy; });
                    }
                    return _issuePackageCommand;
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
                ControlValuesList[Global.Spacewire4.Control].AddProperty(Global.Spacewire4.Control.Enable, 0, 1, Device.CmdSpacewire4Control, value => IsEnable = 1 == value);
                ControlValuesList[Global.Spacewire4.Control].AddProperty(Global.Spacewire4.Control.Connect, 3, 1, delegate { }, value => IsConnect = 1 == value);
                ControlValuesList[Global.Spacewire4.Control].AddProperty(Global.Spacewire4.Control.TimeMark, 4, 1, Device.CmdSpacewire4Control, value => IsIssueTimeMark = 1 == value);
                ControlValuesList[Global.Spacewire4.Record].AddProperty(Global.Spacewire4.Record.EOPSend, 1, 1, Device.CmdSpacewire4Record, value => IsIssueEOP = 1 == value);
                ControlValuesList[Global.Spacewire4.Record].AddProperty(Global.Spacewire4.Record.IssueAuto, 4, 1, Device.CmdSpacewire4Record, value => IsIssueAuto = 1 == value);
                ControlValuesList[Global.Spacewire4.Record].AddProperty(Global.Spacewire4.Record.IssueEEP, 2, 1, Device.CmdSpacewire4Record, value => IsIssueEEP = 1 == value);
                ControlValuesList[Global.Spacewire4.Record].AddProperty(Global.Spacewire4.Record.RecordBusy, 3, 1, delegate { }, value => IsRecordBusy = 1 == value);
                ControlValuesList[Global.Spacewire4.Record].AddProperty(Global.Spacewire4.Record.IssuePackage, 0, 1, Device.CmdSpacewire4Record, value => IsIssuePackage = 1 == value);
            }
        }
    }
}
