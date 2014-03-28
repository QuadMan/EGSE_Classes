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
    using System.Runtime.CompilerServices;
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
    using Microsoft.Win32;

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
        private const int Spacewire2SPTPControlAddr = 0x0B;

        /// <summary>
        /// Адресный байт "Управление".
        /// </summary>
        private const int Spacewire1ControlAddr = 0x12;

        /// <summary>
        /// Адресный байт "Управление обменом с приборами по SPTP".
        /// </summary>
        private const int Spacewire1SPTPControlAddr = 0x16;

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
        private const int Spacewire1RecordFlushAddr = 0x13;

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
        /// Адресный байт "КВВ ПК1".
        /// </summary>
        private const int HSI1ControlAddr = 0x30;

        /// <summary>
        /// Адресный байт "КВВ ПК2".
        /// </summary>
        private const int HSI2ControlAddr = 0x31;

        /// <summary>
        /// Адресный байт "Дополнительные байты".
        /// </summary>
        private const int HSIStateAddr = 0x48;

        /// <summary>
        /// Адресный байт "Управление".
        /// </summary>
        private const int SimHSIControlAddr = 0x36;

        /// <summary>
        /// Адресный байт "Датчики затворов".
        /// </summary>
        private const int ShutterLoAddr = 0x44;

        /// <summary>
        /// Адресный байт "Датчики затворов".
        /// </summary>
        private const int ShutterHiAddr = 0x45;

        /// <summary>
        /// Адресный байт "Бит выдачи релейных команд для установки затворов".
        /// </summary>
        private const int ShutterSendAddr = 0x46;

        /// <summary>
        /// Адресный байт "Бит выбора режима".
        /// </summary>
        private const int ShutterAutoAddr = 0x47;

        /// <summary>
        /// Адресный байт "Сброс адреса данных УКС".
        /// </summary>
        private const int SimHSIRecordFlushAddr = 0x39;

        /// <summary>
        /// Адресный байт "Данные УКС".
        /// </summary>
        private const int SimHSIRecordDataAddr = 0x38;

        /// <summary>
        /// Адресный байт "Выдача УКС".
        /// </summary>
        private const int SimHSIRecordSendAddr = 0x37;

        /// <summary>
        /// Адресный байт "TX_FLAG".
        /// </summary>
        private const int SimHSIRecordTXFlagAddr = 0x3a;

        /// <summary>
        /// Адресный байт "TX_BYTE_NUMBER".
        /// </summary>
        private const int SimHSIRecordByteNumberAddr = 0x3b;

        /// <summary>
        /// Обеспечивает доступ к интерфейсу устройства. 
        /// </summary>
        private readonly EgseBukNotify _intfBUK;

        /// <summary>
        /// Для временного хранения массивов.
        /// Примечание:
        /// При отправлении активного УКС, тут временно сохраняется последний отправленный УКС.
        /// </summary>
        private byte[] _tempData;
               
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
        internal void CmdPowerBusk1(int value)
        {
            byte buf = 0;
            if (_intfBUK.TelemetryNotify.IsBuskLineA)
            {
                if (!IsBitSet(value, 15))
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
                if (!IsBitSet(value, 15))
                {
                    buf |= 1 << 3;
                }
                else
                {
                    buf |= 1 << 1;
                }
            }

            SendToUSB(PowerLoAddr, new byte[1] { buf });
            SendToUSB(PowerHiAddr, new byte[1] { 0 });
            SendToUSB(PowerSetAddr, new byte[1] { 1 });
        }

        /// <summary>
        /// Отправляет команду включить/выключить питание ПК2 БУСК.
        /// </summary>
        /// <param name="value"><c>1</c> если [включить]; иначе, <c>0</c>.</param>
        internal void CmdPowerBusk2(int value)
        {
            byte buf = 0;
            if (_intfBUK.TelemetryNotify.IsBuskLineA)
            {
                if (!IsBitSet(value, 14))
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
                if (!IsBitSet(value, 14))
                {
                    buf |= 1 << 7;
                }
                else
                {
                    buf |= 1 << 5;
                }
            }

            SendToUSB(PowerLoAddr, new byte[1] { buf });
            SendToUSB(PowerHiAddr, new byte[1] { 0 });
            SendToUSB(PowerSetAddr, new byte[1] { 1 });
        }

        /// <summary>
        /// Отправляет команду включить/выключить питание ПК1 БУНД.
        /// </summary>
        /// <param name="value"><c>1</c> если [включить]; иначе, <c>0</c>.</param>
        internal void CmdPowerBund1(int value)
        {
            byte buf = 0;
            if (_intfBUK.TelemetryNotify.IsBundLineA)
            {
                if (!IsBitSet(value, 12))
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
                if (!IsBitSet(value, 12))
                {
                    buf |= 1 << 3;
                }
                else
                {
                    buf |= 1 << 7;
                }
            }

            SendToUSB(PowerLoAddr, new byte[1] { 0 });
            SendToUSB(PowerHiAddr, new byte[1] { buf });
            SendToUSB(PowerSetAddr, new byte[1] { 1 });
        }

        /// <summary>
        /// Отправляет команду включить/выключить питание ПК2 БУНД.
        /// </summary>
        /// <param name="value"><c>1</c> если [включить]; иначе, <c>0</c>.</param>
        internal void CmdPowerBund2(int value)
        {
            byte buf = 0;
            if (_intfBUK.TelemetryNotify.IsBundLineA)
            {
                if (!IsBitSet(value, 13))
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
                if (!IsBitSet(value, 13))
                {
                    buf |= 1 << 2;
                }
                else
                {
                    buf |= 1 << 6;
                }
            }

            SendToUSB(PowerLoAddr, new byte[1] { 0 });
            SendToUSB(PowerHiAddr, new byte[1] { buf });
            SendToUSB(PowerSetAddr, new byte[1] { 1 });
        }

        /// <summary>
        /// Отправляет команду включить/выключить затвор УФЕС ОСН.
        /// </summary>
        /// <param name="value"><c>1</c> если [включить]; иначе, <c>0</c>.</param>
        internal void CmdUfesLock1(int value)
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
        internal void CmdUfesLock2(int value)
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
        internal void CmdVufesLock1(int value)
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
        internal void CmdVufesLock2(int value)
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
        internal void CmdSdchshLock1(int value)
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
        internal void CmdSdchshLock2(int value)
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
        internal void CmdSpacewire2Control(int value)
        {            
            SendToUSB(Spacewire2ControlAddr, new byte[1] { (byte)value });
        }

        /// <summary>
        /// Команда SpaceWire2: Управление SPTP.
        /// </summary>
        /// <param name="value">Параметры управления SPTP.</param>
        internal void CmdSpacewire2SPTPControl(int value)
        {
            SendToUSB(Spacewire2SPTPControlAddr, new byte[1] { (byte)value }); 
        }

        /// <summary>
        /// Команда SpaceWire1: Управление.
        /// </summary>
        /// <param name="value">Параметры управления.</param>
        internal void CmdSpacewire1Control(int value)
        {
            if (_intfBUK.Spacewire4Notify.IsIssueEnable)
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
        internal void CmdSpacewire1ControlSPTP(int value)
        {
            SendToUSB(Spacewire1SPTPControlAddr, new byte[1] { (byte)value });
        }

        /// <summary>
        /// Команда SpaceWire1: Управление SPTP: Счетчик миллисекунд для НП1 (через сколько готовы данные).
        /// </summary>
        /// <param name="value">Счетчик миллисекунд для НП1 (через сколько готовы данные).</param>
        internal void CmdSpacewire1SPTPControlSD1SendTime(int value)
        {
            SendToUSB(Spacewire1SPTPControlSD1SendTimeLoAddr, new byte[1] { (byte)value });
            SendToUSB(Spacewire1SPTPControlSD1SendTimeHiAddr, new byte[1] { (byte)(value >> 8) });
        }

        /// <summary>
        /// Команда SpaceWire1: Управление SPTP: Счетчик миллисекунд для НП2 (через сколько готовы данные).
        /// </summary>
        /// <param name="value">Счетчик миллисекунд для НП2 (через сколько готовы данные).</param>
        internal void CmdSpacewire1SPTPControlSD2SendTime(int value)
        {
            SendToUSB(Spacewire1SPTPControlSD2SendTimeLoAddr, new byte[1] { (byte)value });
            SendToUSB(Spacewire1SPTPControlSD2SendTimeHiAddr, new byte[1] { (byte)(value >> 8) });
        }

        /// <summary>
        /// Команда SpaceWire1: Управление SPTP: Кол-во байт в пакете НП1.
        /// </summary>
        /// <param name="value">Кол-во байт в пакете НП1.</param>
        internal void CmdSpacewire1SPTPControlSD1DataSize(int value)
        {
            SendToUSB(Spacewire1SPTPControlSD1DataSizeLoAddr, new byte[1] { (byte)value });
            SendToUSB(Spacewire1SPTPControlSD1DataSizeHiAddr, new byte[1] { (byte)(value >> 8) });
        }

        /// <summary>
        /// Команда SpaceWire1: Управление SPTP: Кол-во байт в пакете НП2.
        /// </summary>
        /// <param name="value">Кол-во байт в пакете НП2.</param>
        internal void CmdSpacewire1SPTPControlSD2DataSize(int value)
        {
            SendToUSB(Spacewire1SPTPControlSD2DataSizeLoAddr, new byte[1] { (byte)value });
            SendToUSB(Spacewire1SPTPControlSD2DataSizeHiAddr, new byte[1] { (byte)(value >> 8) });
        }

        /// <summary>
        /// Команда SpaceWire1: Запись данных(до 1 Кбайт).
        /// </summary>
        /// <param name="value">Данные для записи.</param>
        internal void CmdSpacewire1Record(int value)
        {
            SendToUSB(Spacewire1RecordFlushAddr, new byte[1] { 1 });        
            if ((null != _intfBUK.Spacewire1Notify.Data) && (0 < _intfBUK.Spacewire1Notify.Data.Length))
            {               
                SendToUSB(Spacewire1RecordDataAddr, _intfBUK.Spacewire1Notify.Data);                
            }

            SendToUSB(Spacewire1RecordSendAddr, new byte[1] { (byte)value });
        }

        /// <summary>
        /// Команда SpaceWire2: Запись данных(до 1 Кбайт).
        /// </summary>
        /// <param name="value">Данные для записи.</param>
        internal void CmdSpacewire2Record(int value)
        {
            SendToUSB(Spacewire2RecordFlushAddr, new byte[1] { 1 });
            if ((null != _intfBUK.Spacewire2Notify.Data) && (0 < _intfBUK.Spacewire2Notify.Data.Length))
            {
                if (_intfBUK.Spacewire2Notify.IsMakeTK)
                {
                    SendToUSB(Spacewire2RecordDataAddr, _intfBUK.Spacewire2Notify.Data.ToTk((byte)_intfBUK.Spacewire2Notify.LogicBuk, (byte)_intfBUK.Spacewire2Notify.LogicBusk, _intfBUK.Spacewire2Notify.CurApid/*, _intfBUK.Spacewire2Notify.CounterIcd*/).ToArray());
                }
                else
                {
                    SendToUSB(Spacewire2RecordDataAddr, _intfBUK.Spacewire2Notify.Data.ToSptp((byte)_intfBUK.Spacewire2Notify.LogicBuk, (byte)_intfBUK.Spacewire2Notify.LogicBusk).ToArray());
                }
            }

            SendToUSB(Spacewire2RecordSendAddr, new byte[1] { (byte)value });
        }

        /// <summary>
        /// Команда SpaceWire4: Управление.
        /// </summary>
        /// <param name="value">Параметры управления.</param>
        internal void CmdSpacewire4Control(int value)
        {
            if (_intfBUK.Spacewire1Notify.IsIssueEnable)
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
        internal void CmdSpacewire3Control(int value)
        {
            SendToUSB(Spacewire3ControlAddr, new byte[1] { (byte)value });
        }

        /// <summary>
        /// Команда SpaceWire4: Запись данных(до 1 Кбайт).
        /// </summary>
        /// <param name="value">Данные для записи.</param>
        internal void CmdSpacewire4Record(int value)
        {
            SendToUSB(Spacewire4RecordFlushAddr, new byte[1] { 1 });
            if ((null != _intfBUK.Spacewire4Notify.Data) && (0 < _intfBUK.Spacewire4Notify.Data.Length))
            {                
                SendToUSB(Spacewire4RecordDataAddr, _intfBUK.Spacewire4Notify.Data);                
            }

            SendToUSB(Spacewire4RecordSendAddr, new byte[1] { (byte)value });
        }

        /// <summary>
        /// Команда установки внутреннего времени устройства.
        /// </summary>
        internal void CmdSetDeviceTime()
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
        internal void CmdSpacewire2LogicBuk(int value)
        {
            SendToUSB(Spacewire2LogicBukAddr, new byte[1] { (byte)value });
        }

        /// <summary>
        /// Команда SpaceWire2: Адрес ИМИТАТОРА БУСКа.
        /// </summary>
        /// <param name="value">Логический адрес БУСК.</param>
        internal void CmdSpacewire2LogicBusk(int value)
        {
            SendToUSB(Spacewire2LogicBuskAddr, new byte[1] { (byte)value });
        }

        /// <summary>
        /// Команда SpaceWire1: Адрес НП1.
        /// </summary>
        /// <param name="value">Логический адрес НП1.</param>
        internal void CmdSpacewire1LogicSD1(int value)
        {
            SendToUSB(Spacewire1LogicSD1Addr, new byte[1] { (byte)value });
        }

        /// <summary>
        /// Команда SpaceWire1: Адрес БУСК.
        /// </summary>
        /// <param name="value">Логический адрес БУСК.</param>
        internal void CmdSpacewire1LogicBusk(int value)
        {
            SendToUSB(Spacewire1LogicBuskAddr, new byte[1] { (byte)value });
        }

        /// <summary>
        /// Команда установки логических адресов.
        /// </summary>
        internal void CmdSetDeviceLogicAddr()
        {
            if (_intfBUK.Spacewire2Notify.IsChannelBuk1) 
            {
                _intfBUK.Spacewire2Notify.LogicBuk = Global.LogicAddrBuk1;
                _intfBUK.Spacewire1Notify.LogicSD1 = Global.LogicAddrBuk1;
            }
            else
            {
                _intfBUK.Spacewire2Notify.LogicBuk = Global.LogicAddrBuk2;
                _intfBUK.Spacewire1Notify.LogicSD1 = Global.LogicAddrBuk2;
            }

            if (_intfBUK.Spacewire2Notify.IsChannelBusk1)
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

        /// <summary>
        /// Команда [управления ВСИ ПК1].
        /// </summary>
        /// <param name="value">Байт управления.</param>
        internal void CmdHSILine1(int value)
        {
            SendToUSB(HSI1ControlAddr, new byte[1] { (byte)value });
        }

        /// <summary>
        /// Команда [управления ВСИ ПК2].
        /// </summary>
        /// <param name="value">Байт управления.</param>
        internal void CmdHSILine2(int value)
        {
            SendToUSB(HSI2ControlAddr, new byte[1] { (byte)value });
        }

        /// <summary>
        /// Команда [управления ВСИ статусом].
        /// </summary>
        /// <param name="value">Байт управления.</param>
        internal void CmdHSIState(int value)
        {
            SendToUSB(HSIStateAddr, new byte[1] { (byte)value });
        }
        
        /// <summary>
        /// Команда [управления имитатором ВСИ].
        /// </summary>
        /// <param name="value">Байт управления.</param>
        internal void CmdSimHSIControl(int value)
        {
            SendToUSB(SimHSIControlAddr, new byte[1] { (byte)value });
        }

        /// <summary>
        /// Команда [управления датчиками затворов].
        /// </summary>
        /// <param name="value">Слово управления.</param>
        internal void CmdShutters(int value)
        {
            SendToUSB(ShutterLoAddr, new byte[1] { (byte)value });
            SendToUSB(ShutterHiAddr, new byte[1] { (byte)(value >> 8) });
            SendToUSB(ShutterSendAddr, new byte[1] { 1 });    
        }

        /// <summary>
        /// Команда [включения автоматического управления датчиками затворов].
        /// </summary>
        /// <param name="value">Бит управления.</param>
        internal void CmdAutoShutters(int value)
        {
            byte x;
            if (0 != value)
            {
                x = 1;
            }
            else
            {
                x = 0;
            }

            SendToUSB(ShutterAutoAddr, new byte[1] { x });
        }

        /// <summary>
        /// Команда [выдачи УКС ВСИ].
        /// </summary>
        /// <param name="value">Байт управления выдачей УКС.</param>
        internal void CmdSimHSIRecord(int value)
        {
            SendToUSB(SimHSIRecordTXFlagAddr, new byte[1] { 2 });            
            if ((null != _intfBUK.HSINotify.Data) && (0 < _intfBUK.HSINotify.Data.Length))
            {
                SendToUSB(SimHSIRecordByteNumberAddr, new byte[1] { Convert.ToByte(_intfBUK.HSINotify.Data.Length) });
                SendToUSB(SimHSIRecordFlushAddr, new byte[1] { 1 });
                SendToUSB(SimHSIRecordDataAddr, _intfBUK.HSINotify.Data);
            }

            SendToUSB(SimHSIRecordSendAddr, new byte[1] { (byte)value });            
        }

        /// <summary>
        /// Временно сохраняет массив байт.
        /// </summary>
        internal void TempData()
        {
            if ((null != _intfBUK.HSINotify.Data) && (0 != _intfBUK.HSINotify.Data.Length))
            {
                _tempData = new byte[_intfBUK.HSINotify.Data.Length];
                Array.Copy(_intfBUK.HSINotify.Data, _tempData, _intfBUK.HSINotify.Data.Length);
            }
        }

        /// <summary>
        /// Восстанавливает сохраненный массив байт.
        /// </summary>
        internal void RevertData()
        {
            if ((null != _tempData) && (0 != _tempData.Length))
            {
                _intfBUK.HSINotify.Data = new byte[_tempData.Length];
                Array.Copy(_tempData, _intfBUK.HSINotify.Data, _tempData.Length);
            }
        }

        /// <summary>
        /// Команда [выдачи активного УКС активировать/деактивировать ПК1 ВСИ].
        /// Примечание:
        /// Временно подменяет данные УКС в ВСИ нотификаторе для формирования УКС активации.
        /// </summary>
        /// <param name="value">1 - для активации, 0 - деактивация.</param>
        internal void CmdSimHSI1(int value)
        { 
            TempData();
            if (1 == value)
            {                                
                _intfBUK.HSINotify.Data = new byte[1] { 0xA1 };
            }
            else
            {
                _intfBUK.HSINotify.Data = new byte[1] { 0xFF };
            }

            CmdSimHSIRecord(1);
            RevertData();
        }

        /// <summary>
        /// Команда [выдачи активного УКС активировать/деактивировать ПК2 ВСИ].
        /// Примечание:
        /// Временно подменяет данные УКС в ВСИ нотификаторе для формирования УКС активации.
        /// </summary>
        /// <param name="value">1 - для активации, 0 - деактивация.</param>
        internal void CmdSimHSI2(int value)
        {
            TempData();
            if (1 == value)
            {
                _intfBUK.HSINotify.Data = new byte[1] { 0xA2 };
            }
            else
            {
                _intfBUK.HSINotify.Data = new byte[1] { 0xFF };
            }

            CmdSimHSIRecord(1);
            RevertData();
        }

        /// <summary>
        /// Определяет, является ли [бит установленным] [в указанном целом].
        /// </summary>
        /// <param name="b">Проверяемое целое.</param>
        /// <param name="pos">Позиция бита.</param>
        /// <returns><c>true</c> если бит установлен.</returns>
        private bool IsBitSet(int b, int pos)
        {
            return (b & (1 << pos)) != 0;
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
        /// Экземпляр декодера протокола ВСИ.
        /// </summary>
        private ProtocolHsi _decoderHsi;

        /// <summary>
        /// Текущее состояние подключения устройства.
        /// </summary>
        private bool _isConnected;

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
        /// Отображать ли [окно "монитор USB"].
        /// </summary>
        private bool _isShowUsbSendsMonitor;

        /// <summary>
        /// Датчики затворов: бит выбора режима.
        /// </summary>
        private bool _isIssueManualShutter = true;

        /// <summary>
        /// СДЩ: Датчики затворов: закрытия.
        /// </summary>
        private DevEnabled _issueSdchshClose = DevEnabled.Off;

        /// <summary>
        /// СДЩ: Датчики затворов: открытия.
        /// </summary>
        private DevEnabled _issueSdchshOpen = DevEnabled.Off;

        /// <summary>
        /// ВУФЕС: Датчики затворов: закрытия.
        /// </summary>
        private DevEnabled _issueVufesClose = DevEnabled.Off;

        /// <summary>
        /// ВУФЕС: Датчики затворов: открытия.
        /// </summary>
        private DevEnabled _issueVufesOpen = DevEnabled.Off;

        /// <summary>
        /// УФЕС: Датчики затворов: закрытия.
        /// </summary>
        private DevEnabled _issueUfesClose = DevEnabled.Off;

        /// <summary>
        /// УФЕС: Датчики затворов: открытия.
        /// </summary>
        private DevEnabled _issueUfesOpen = DevEnabled.Off;
        
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

            UITestNotify = new UITest(this);
            UITestNotify.UsbLogFile = LogsClass.LogUSB.FileName;

            _decoderSpacewireBusk = new ProtocolSpacewire((uint)Spacewire2.Addr.Data, (uint)Spacewire2.Addr.End, (uint)Spacewire2.Addr.Time1, (uint)Spacewire2.Addr.Time2);
            _decoderSpacewireBusk.GotSpacewireMsg += new ProtocolSpacewire.SpacewireMsgEventHandler(OnSpacewire2Msg);
            _decoderSpacewireBusk.GotSpacewireMsg += new ProtocolSpacewire.SpacewireMsgEventHandler(Spacewire2Notify.OnSpacewire2MsgRawSave);
            _decoderSpacewireBusk.GotSpacewireTimeTick1Msg += new ProtocolSpacewire.SpacewireTimeTickMsgEventHandler((sender, e) => { Spacewire2Notify.BuskTickTime1 = e.TimeTickInfo.Value; });
            _decoderSpacewireBusk.GotSpacewireTimeTick2Msg += new ProtocolSpacewire.SpacewireTimeTickMsgEventHandler((sender, e) => { Spacewire2Notify.BuskTickTime2 = e.TimeTickInfo.Value; });
            _decoderUSB.GotProtocolMsg += new ProtocolUSBBase.ProtocolMsgEventHandler(_decoderSpacewireBusk.OnMessageFunc);

            _decoderSpacewireBuk = new ProtocolSpacewire((uint)Spacewire2.Addr.BukData, (uint)Spacewire2.Addr.BukEnd, (uint)Spacewire2.Addr.BukTime1, (uint)Spacewire2.Addr.BukTime2);
            _decoderSpacewireBuk.GotSpacewireMsg += new ProtocolSpacewire.SpacewireMsgEventHandler(OnSpacewire2Msg);
            _decoderSpacewireBuk.GotSpacewireMsg += new ProtocolSpacewire.SpacewireMsgEventHandler(Spacewire2Notify.OnSpacewire2MsgRawSave);
            _decoderSpacewireBuk.GotSpacewireTimeTick1Msg += new ProtocolSpacewire.SpacewireTimeTickMsgEventHandler((sender, e) => { Spacewire2Notify.BukTickTime1 = e.TimeTickInfo.Value; });
            _decoderSpacewireBuk.GotSpacewireTimeTick2Msg += new ProtocolSpacewire.SpacewireTimeTickMsgEventHandler((sender, e) => { Spacewire2Notify.BukTickTime2 = e.TimeTickInfo.Value; });
            _decoderUSB.GotProtocolMsg += new ProtocolUSBBase.ProtocolMsgEventHandler(_decoderSpacewireBuk.OnMessageFunc);

            _decoderSpacewireSDIn = new ProtocolSpacewire((uint)Spacewire3.Addr.InData, (uint)Spacewire3.Addr.InEnd, (uint)Spacewire3.Addr.InTime1, (uint)Spacewire3.Addr.InTime2, true);
            _decoderSpacewireSDIn.GotSpacewireMsg += new ProtocolSpacewire.SpacewireMsgEventHandler(OnSpacewire3Msg);
            _decoderSpacewireSDIn.GotSpacewireMsg += new ProtocolSpacewire.SpacewireMsgEventHandler(Spacewire3Notify.OnSpacewire3MsgRawSave);
            _decoderSpacewireSDIn.GotSpacewireTimeTick1Msg += new ProtocolSpacewire.SpacewireTimeTickMsgEventHandler((sender, e) => { Spacewire3Notify.BukTickTime1 = e.TimeTickInfo.Value; });
            _decoderSpacewireSDIn.GotSpacewireTimeTick2Msg += new ProtocolSpacewire.SpacewireTimeTickMsgEventHandler((sender, e) => { Spacewire3Notify.BukTickTime2 = e.TimeTickInfo.Value; });
            _decoderUSB.GotProtocolMsg += new ProtocolUSBBase.ProtocolMsgEventHandler(_decoderSpacewireSDIn.OnMessageFunc);

            _decoderSpacewireSDOut = new ProtocolSpacewire((uint)Spacewire3.Addr.OutData, (uint)Spacewire3.Addr.OutEnd, (uint)Spacewire3.Addr.OutTime1, (uint)Spacewire3.Addr.OutTime2, true);
            _decoderSpacewireSDOut.GotSpacewireMsg += new ProtocolSpacewire.SpacewireMsgEventHandler(OnSpacewire3Msg);
            _decoderSpacewireSDOut.GotSpacewireMsg += new ProtocolSpacewire.SpacewireMsgEventHandler(Spacewire3Notify.OnSpacewire3MsgRawSave);
            _decoderSpacewireSDOut.GotSpacewireTimeTick1Msg += new ProtocolSpacewire.SpacewireTimeTickMsgEventHandler((sender, e) => { Spacewire3Notify.SDTickTime1 = e.TimeTickInfo.Value; });
            _decoderSpacewireSDOut.GotSpacewireTimeTick2Msg += new ProtocolSpacewire.SpacewireTimeTickMsgEventHandler((sender, e) => { Spacewire3Notify.SDTickTime2 = e.TimeTickInfo.Value; });
            _decoderUSB.GotProtocolMsg += new ProtocolUSBBase.ProtocolMsgEventHandler(_decoderSpacewireSDOut.OnMessageFunc);

            _decoderHsi = new ProtocolHsi((uint)HSI.Addr);
            _decoderHsi.GotHsiMsg += new ProtocolHsi.HsiMsgEventHandler(OnHsiMsg);
            _decoderHsi.GotHsiMsg += new ProtocolHsi.HsiMsgEventHandler(HSINotify.OnHsiMsgRawSave);
            _decoderUSB.GotProtocolMsg += new ProtocolUSBBase.ProtocolMsgEventHandler(_decoderHsi.OnMessageFunc);

            ControlValuesList.Add(Global.Shutters, new ControlValue());
            ControlValuesList[Global.Shutters].AddProperty(Global.Shutters.Auto, 14, 1, Device.CmdAutoShutters, value => IsIssueManualShutter = !Convert.ToBoolean(value));
            ControlValuesList[Global.Shutters].AddProperty(Global.Shutters.UfesOpen, 10, 1, Device.CmdShutters, value => IssueUfesOpen = (DevEnabled)value);
            ControlValuesList[Global.Shutters].AddProperty(Global.Shutters.UfesClose, 8, 1, Device.CmdShutters, value => IssueUfesClose = (DevEnabled)value);
            ControlValuesList[Global.Shutters].AddProperty(Global.Shutters.VufesOpen, 6, 1, Device.CmdShutters, value => IssueVufesOpen = (DevEnabled)value);
            ControlValuesList[Global.Shutters].AddProperty(Global.Shutters.VufesClose, 4, 1, Device.CmdShutters, value => IssueVufesClose = (DevEnabled)value);
            ControlValuesList[Global.Shutters].AddProperty(Global.Shutters.SdchshOpen, 2, 1, Device.CmdShutters, value => IssueSdchshOpen = (DevEnabled)value);
            ControlValuesList[Global.Shutters].AddProperty(Global.Shutters.SdchshClose, 0, 1, Device.CmdShutters, value => IssueSdchshClose = (DevEnabled)value);
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
        /// Вызывается, когда [получено сообщение по ВСИ].
        /// </summary>
        public event ProtocolHsi.HsiMsgEventHandler GotHsiMsg;

        /// <summary>
        /// Вызывается, когда [получено УКС-сообщение по ВСИ].
        /// </summary>
        public event ProtocolHsi.HsiMsgEventHandler GotHsiCmdMsg;
        
        /// <summary>
        /// Вызывается, когда [получено сообщение по spacewire 3].
        /// </summary>
        public event ProtocolSpacewire.SpacewireMsgEventHandler GotSpacewire3Msg;

        /// <summary>
        /// Состояние прибора.
        /// </summary>
        public enum DevEnabled
        {
            /// <summary>
            /// Прибор включен.
            /// </summary>
            On = 0x01,

            /// <summary>
            /// Прибор выключен.
            /// </summary>
            Off = 0x00
        }

        /// <summary>
        /// Получает нотификатор self-теста.
        /// </summary>
        /// <value>
        /// Экземпляр нотификатора.
        /// </value>
        public UITest UITestNotify { get; private set; }

        /// <summary>
        /// Получает нотификатор телеметрии.
        /// </summary>
        /// <value>
        /// Экземпляр нотификатора.
        /// </value>
        public Telemetry TelemetryNotify { get; private set; }

        /// <summary>
        /// Получает нотификатор ВСИ интерфейса.
        /// </summary>
        /// <value>
        /// Экземпляр нотификатора.
        /// </value>
        public HSI HSINotify { get; private set; }

        /// <summary>
        /// Получает нотификатор spacewire1.
        /// </summary>
        /// <value>
        /// Экземпляр нотификатора.
        /// </value>
        public Spacewire1 Spacewire1Notify { get; private set; }

        /// <summary>
        /// Получает нотификатор spacewire2.
        /// </summary>
        /// <value>
        /// Экземпляр нотификатора.
        /// </value>
        public Spacewire2 Spacewire2Notify { get; private set; }

        /// <summary>
        /// Получает нотификатор spacewire3.
        /// </summary>
        /// <value>
        /// Экземпляр нотификатора.
        /// </value>
        public Spacewire3 Spacewire3Notify { get; private set; }

        /// <summary>
        /// Получает нотификатор spacewire4.
        /// </summary>
        /// <value>
        /// Экземпляр нотификатора.
        /// </value>
        public Spacewire4 Spacewire4Notify { get; private set; }

        /// <summary>
        /// Получает доступ к USB прибора.
        /// </summary>
        public EgseBuk Device { get; private set; }     

        /// <summary>
        /// Получает количество байт доступных для считывания из USB.
        /// </summary>
        /// <value>
        /// Количество байт доступных для считывания из USB.
        /// </value>
        public int BytesAvailable
        {
            get
            {
                return Device.BytesAvailable;
            }
        }

        /// <summary>
        /// Получает значение, показывающее, что [включено ручное управление датчиками затворов].
        /// </summary>
        /// <value>
        ///   <c>true</c> если [включено ручное управление датчиками затворов]; иначе, <c>false</c>.
        /// </value>
        public bool IsIssueManualShutter 
        { 
            get
            {
                return _isIssueManualShutter;
            }

            private set 
            {
                _isIssueManualShutter = value;
                ControlValuesList[Global.Shutters].SetProperty(Global.Shutters.Auto, Convert.ToInt32(!value));
                FirePropertyChangedEvent();
            } 
        }

        /// <summary>
        /// Получает команду на [включение датчика открытия для НП УФЕС].
        /// </summary>
        /// <value>
        /// Команда на [включение датчика открытия для НП УФЕС].
        /// </value>
        public DevEnabled IssueUfesOpen
        {
            get
            {
                return _issueUfesOpen;
            }

            private set 
            {
                _issueUfesOpen = value;
                ControlValuesList[Global.Shutters].SetProperty(Global.Shutters.UfesOpen, (int)value);
                FirePropertyChangedEvent();
            }
        }

        /// <summary>
        /// Получает команду на [включение датчика закрытия для НП УФЕС].
        /// </summary>
        /// <value>
        /// Команда на [включение датчика закрытия для НП УФЕС].
        /// </value>
        public DevEnabled IssueUfesClose
        {
            get
            {
                return _issueUfesClose;
            }

            private set 
            {
                _issueUfesClose = value;
                ControlValuesList[Global.Shutters].SetProperty(Global.Shutters.UfesClose, (int)value);
                FirePropertyChangedEvent();
            }
        }

        /// <summary>
        /// Получает команду на [включение датчика открытия для НП ВУФЕС].
        /// </summary>
        /// <value>
        /// Команда на [включение датчика открытия для НП ВУФЕС].
        /// </value>
        public DevEnabled IssueVufesOpen
        {
            get
            {
                return _issueVufesOpen;
            }

            private set 
            {
                _issueVufesOpen = value;
                ControlValuesList[Global.Shutters].SetProperty(Global.Shutters.VufesOpen, (int)value);
                FirePropertyChangedEvent();
            }
        }

        /// <summary>
        /// Получает команду на [включение датчика закрытия для НП ВУФЕС].
        /// </summary>
        /// <value>
        /// Команда на [включение датчика закрытия для НП ВУФЕС].
        /// </value>
        public DevEnabled IssueVufesClose
        {
            get
            {
                return _issueVufesClose;
            }

            private set 
            {
                _issueVufesClose = value;
                ControlValuesList[Global.Shutters].SetProperty(Global.Shutters.VufesClose, (int)value);
                FirePropertyChangedEvent();
            }
        }

        /// <summary>
        /// Получает команду на [включение датчика открытия для НП СДЩ].
        /// </summary>
        /// <value>
        /// Команда на [включение датчика открытия для НП СДЩ].
        /// </value>
        public DevEnabled IssueSdchshOpen
        {
            get
            {
                return _issueSdchshOpen;
            }

            private set 
            {
                _issueSdchshOpen = value;
                ControlValuesList[Global.Shutters].SetProperty(Global.Shutters.SdchshOpen, (int)value);
                FirePropertyChangedEvent();
            }
        }

        /// <summary>
        /// Получает команду на [включение датчика закрытия для НП СДЩ].
        /// </summary>
        /// <value>
        /// Команда на [включение датчика закрытия для НП СДЩ].
        /// </value>
        public DevEnabled IssueSdchshClose 
        { 
            get 
            {
                return _issueSdchshClose;
            }

            private set 
            {
                _issueSdchshClose = value;
                ControlValuesList[Global.Shutters].SetProperty(Global.Shutters.SdchshClose, (int)value);
                FirePropertyChangedEvent();
            }
        }

        /// <summary>
        /// Получает значение, показывающее, что [подключен прибор].
        /// </summary>
        /// <value>
        ///   <c>true</c> если [подключен прибор]; иначе, <c>false</c>.
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
                FirePropertyChangedEvent();
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
                FirePropertyChangedEvent();
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
                FirePropertyChangedEvent();
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
                FirePropertyChangedEvent();
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
                FirePropertyChangedEvent();
            }
        }

        /// <summary>
        /// Получает значение, показывающее, открыто ли [окно "монитор USB"].
        /// </summary>
        /// <value>
        ///   <c>true</c> если [окно "монитор USB"] открыто; иначе, <c>false</c>.
        /// </value>
        public bool IsShowUsbSendsMonitor
        {
            get
            {
                return _isShowUsbSendsMonitor;
            }

            private set 
            {
                _isShowUsbSendsMonitor = value;
                FirePropertyChangedEvent();
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
                FirePropertyChangedEvent();
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
                FirePropertyChangedEvent();
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
        /// Получает время, пришедшее от прибора.
        /// </summary>       
        public EgseTime DeviceTime
        {
            get
            {                
                return _deviceTime;                
            }

            private set 
            {
                _deviceTime = value;                
            }
        }

        /// <summary>
        /// Получает список управляющих элементов.
        /// </summary>
        public Dictionary<string, ControlValue> ControlValuesList { get; private set; }

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
        /// Открывает файл, для предоставления содержимого как массива байт.
        /// </summary>
        /// <returns>Массив байт открытого файла.</returns>
        public byte[] OpenFromFile()
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.DefaultExt = ".dat";
            dlg.Filter = "Bin data (.dat)|*.dat|All files (*.*)|*.*";

            bool? result = dlg.ShowDialog();

            if (result == true)
            {
                byte[] buf = File.ReadAllBytes(dlg.FileName);
                return buf;
            }

            return new byte[] { };
        }

        /// <summary>
        /// Для каждого элемента управления тикаем временем.
        /// </summary>
        public void TickAllControlsValues()
        {
            UpdateProperties();
            Debug.Assert(ControlValuesList != null, @"ControlValuesList не должны быть равны null!");
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
                Task.Run(() =>
                {                 
                    RefreshAllControlsValues();
                    Task.Delay(1500).Wait();  
                    Device.CmdSetDeviceLogicAddr();
                });
                                         
                LogsClass.LogMain.LogText = Resource.Get(@"stDeviceName") + Resource.Get(@"stConnected");
            }
            else
            {
                LogsClass.LogMain.LogText = Resource.Get(@"stDeviceName") + Resource.Get(@"stDisconnected");
            }
        }

        /// <summary>
        /// Получает название файла, сформированного по правилам.
        /// </summary>
        /// <param name="logName">Название-база лог файла.</param>
        /// <returns>Полное наименование файла.</returns>
        internal string GetNewFileName(string logName)
        {
            string dataLogDir = Directory.GetCurrentDirectory().ToString() + @"\DATA\";
            Directory.CreateDirectory(dataLogDir);
            return dataLogDir + logName + "_" + DateTime.Now.ToString(@"yyMMdd_HHmmss") + @".dat";
        }

        /// <summary>
        /// Called when [spacewire2 MSG].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="SpacewireSptpMsgEventArgs"/> instance containing the event data.</param>
        protected virtual void OnSpacewire2Msg(object sender, BaseMsgEventArgs e)
        {
            if (this.GotSpacewire2Msg != null)
            {
                if ((e is SpacewireSptpMsgEventArgs) && IsRequestSpacewireMsg(e as SpacewireSptpMsgEventArgs))
                {
                    if (Spacewire2Notify.LogicBusk == (e as SpacewireSptpMsgEventArgs).SptpInfo.From)
                    {
                        Spacewire2Notify.RequestQueueFromBusk++;
                    }
                    else if (Spacewire2Notify.LogicBuk == (e as SpacewireSptpMsgEventArgs).SptpInfo.From)
                    {
                        Spacewire2Notify.RequestQueueFromBuk++;
                    }
                    else
                    {
                        this.GotSpacewire2Msg(sender, e);
                    }
                }
                else if ((e is SpacewireSptpMsgEventArgs) && IsReplySpacewireMsg(e as SpacewireSptpMsgEventArgs))
                {
                    if (Spacewire2Notify.LogicBusk == (e as SpacewireSptpMsgEventArgs).SptpInfo.From)
                    {
                        Spacewire2Notify.ReplyQueueFromBusk++;
                    }
                    else if (Spacewire2Notify.LogicBuk == (e as SpacewireSptpMsgEventArgs).SptpInfo.From)
                    {
                        Spacewire2Notify.ReplyQueueFromBuk++;
                    }
                    else
                    {
                        this.GotSpacewire2Msg(sender, e);
                    }
                }
                else if (e is SpacewireObtMsgEventArgs)
                {
                    Spacewire2Notify.CodeOnboardTime = (e as SpacewireObtMsgEventArgs).ObtInfo.Value;
                }
                else
                {
                    this.GotSpacewire2Msg(sender, e);
                }
            }
        }

        /// <summary>
        /// Called when [hsi MSG].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="HsiMsgEventArgs"/> instance containing the event data.</param>
        protected virtual void OnHsiMsg(object sender, HsiMsgEventArgs e)
        {
            if (this.GotHsiMsg != null)
            {
                if (IsHsiRequestStateMsg(e))
                {
                    if (HsiMsgEventArgs.HsiLine.Main == e.Info.Line)
                    {
                        HSINotify.RequestStateMain++;
                    }
                    else
                    {
                        HSINotify.RequestStateResv++;
                    }
                }
                else if (IsHsiRequestDataMsg(e))
                {
                    if (HsiMsgEventArgs.HsiLine.Main == e.Info.Line)
                    {
                        HSINotify.RequestDataMain++;
                    }
                    else
                    {
                        HSINotify.RequestDataResv++;
                    }
                }
                else if (IsHsiCmdMsg(e))
                {
                    if (this.GotHsiCmdMsg != null)
                    {
                        if (IsCmdFromSet1(e))
                        {
                            HSINotify.CmdCounter1++;
                        }
                        else if (IsCmdFromSet2(e))
                        {
                            HSINotify.CmdCounter2++;
                        }

                        this.GotHsiCmdMsg(sender, e);
                    }
                }
                else
                {
                    this.GotHsiMsg(sender, e);
                }
            }
        }

        /// <summary>
        /// Called when [spacewire3 MSG].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="SpacewireSptpMsgEventArgs"/> instance containing the event data.</param>
        protected virtual void OnSpacewire3Msg(object sender, BaseMsgEventArgs e)
        {
            if (this.GotSpacewire3Msg != null)
            {
                {
                    this.GotSpacewire3Msg(sender, e);
                }
            }
        }

        /// <summary>
        /// Вынуждает обновить отображаемые свойства на UI. 
        /// </summary>
        private void UpdateProperties()
        {
            FirePropertyChangedEvent(null, @"DeviceTime");
            FirePropertyChangedEvent(null, @"DeviceSpeed");
            FirePropertyChangedEvent(null, @"DeviceTrafic");
            FirePropertyChangedEvent(null, @"BytesAvailable");
        }

        /// <summary>
        /// Определяет когда [сообщение по spacewire] [является запросом квоты].
        /// </summary>
        /// <param name="msg">The <see cref="SpacewireSptpMsgEventArgs"/> instance containing the event data.</param>
        /// <returns><c>true</c> если сообщение "запрос квоты"</returns>
        private bool IsRequestSpacewireMsg(SpacewireSptpMsgEventArgs msg)
        {
            return SpacewireSptpMsgEventArgs.SptpType.Request == msg.SptpInfo.MsgType;
        }

        /// <summary>
        /// Определяет когда [сообщение по ВСИ] [является запросом статуса].
        /// </summary>
        /// <param name="msg">The <see cref="SpacewireSptpMsgEventArgs"/> instance containing the event data.</param>
        /// <returns><c>true</c> если сообщение "запрос статуса"</returns>
        private bool IsHsiRequestStateMsg(HsiMsgEventArgs msg)
        {
            return HsiMsgEventArgs.Type.RequestState == msg.Info.Flag;
        }

        /// <summary>
        /// Определяет когда [сообщение по ВСИ] [является запросом данных].
        /// </summary>
        /// <param name="msg">The <see cref="SpacewireSptpMsgEventArgs"/> instance containing the event data.</param>
        /// <returns><c>true</c> если сообщение "запрос данных"</returns>
        private bool IsHsiRequestDataMsg(HsiMsgEventArgs msg)
        {
            return HsiMsgEventArgs.Type.RequestData == msg.Info.Flag;
        }

        /// <summary>
        /// Определяет когда [сообщение по ВСИ] [является УКС].
        /// </summary>
        /// <param name="msg">The <see cref="SpacewireSptpMsgEventArgs"/> instance containing the event data.</param>
        /// <returns><c>true</c> если сообщение "УКС", иначе <c>false</c>.</returns>
        private bool IsHsiCmdMsg(HsiMsgEventArgs msg)
        {
            return HsiMsgEventArgs.Type.Cmd == msg.Info.Flag;
        }

        /// <summary>
        /// Определяет когда [сообщение по ВСИ] [пришло от первого полукомплекта].
        /// </summary>
        /// <param name="msg">The <see cref="HsiMsgEventArgs"/> instance containing the event data.</param>
        /// <returns><c>true</c> если сообщение [пришло от первого полукомплекта], иначе <c>false</c>.</returns>
        private bool IsCmdFromSet1(HsiMsgEventArgs msg)
        {            
            if (0 < msg.Data.Length)
            {
                return 0xA1 == msg.Data[0];
            }
            else
            {
                return false;
            }                    
        }

        /// <summary>
        /// Определяет когда [сообщение по ВСИ] [пришло от второго полукомплекта].
        /// </summary>
        /// <param name="msg">The <see cref="HsiMsgEventArgs"/> instance containing the event data.</param>
        /// <returns><c>true</c> если сообщение [пришло от второго полукомплекта], иначе <c>false</c>.</returns>
        private bool IsCmdFromSet2(HsiMsgEventArgs msg)
        {
            if (0 < msg.Data.Length)
            {
                return 0xA2 == msg.Data[0];
            }
            else
            {
                return false;
            }  
        }

        /// <summary>
        /// Определяет когда [сообщение по spacewire] [является КБВ].
        /// </summary>
        /// <param name="msg">The <see cref="SpacewireSptpMsgEventArgs"/> instance containing the event data.</param>
        /// <returns><c>true</c> если сообщение "КБВ"</returns>
        private bool IsKbvSpacewireMsg(SpacewireSptpMsgEventArgs msg)
        {            
            return 6 == msg.Data.Length; // TODO Написать другой детерминатор!
        }

        /// <summary>
        /// Определяет когда [сообщение по spacewire] [является предоставлением квоты].
        /// </summary>
        /// <param name="msg">The <see cref="SpacewireSptpMsgEventArgs"/> instance containing the event data.</param>
        /// <returns><c>true</c> если сообщение "предоставление квоты"</returns>
        private bool IsReplySpacewireMsg(SpacewireSptpMsgEventArgs msg)
        {
            return SpacewireSptpMsgEventArgs.SptpType.Reply == msg.SptpInfo.MsgType;
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
                        ControlValuesList[Global.Spacewire2.Control].UsbValue = msg.Data[7];
                        ControlValuesList[Global.Spacewire2.Record].UsbValue = msg.Data[10]; 
                        ControlValuesList[Global.Spacewire2.BuskLogic].UsbValue = msg.Data[11];
                        ControlValuesList[Global.Spacewire2.BukLogic].UsbValue = msg.Data[12];
                        ControlValuesList[Global.Spacewire2.SPTPControl].UsbValue = msg.Data[14];
                        ControlValuesList[Global.Spacewire3.Control].UsbValue = msg.Data[15];  
                        ControlValuesList[Global.Spacewire1.Control].UsbValue = msg.Data[17];
                        ControlValuesList[Global.Spacewire1.Record].UsbValue = msg.Data[20];
                        ControlValuesList[Global.Spacewire1.SPTPControl].UsbValue = msg.Data[21];
                        ControlValuesList[Global.Spacewire1.BuskLogic].UsbValue = msg.Data[22];
                        ControlValuesList[Global.Spacewire1.SD1Logic].UsbValue = msg.Data[23];
                        ControlValuesList[Global.Spacewire1.SD1SendTime].UsbValue = (msg.Data[26] << 8) | msg.Data[25];
                        ControlValuesList[Global.Spacewire1.SD1DataSize].UsbValue = (msg.Data[28] << 8) | msg.Data[27]; 
                        ControlValuesList[Global.Spacewire4.Control].UsbValue = msg.Data[29];
                        ControlValuesList[Global.Spacewire4.Record].UsbValue = msg.Data[32];
                        ControlValuesList[Global.HSI.Line1].UsbValue = msg.Data[33];
                        ControlValuesList[Global.HSI.Line2].UsbValue = msg.Data[34];
                        ControlValuesList[Global.HSI.Line1StateCounter].UsbValue = (msg.Data[36] << 8) | msg.Data[35];
                        ControlValuesList[Global.HSI.Line1FrameCounter].UsbValue = (msg.Data[38] << 8) | msg.Data[37];
                        ControlValuesList[Global.HSI.Line2StateCounter].UsbValue = (msg.Data[40] << 8) | msg.Data[39];
                        ControlValuesList[Global.HSI.Line2FrameCounter].UsbValue = (msg.Data[42] << 8) | msg.Data[41];
                        ControlValuesList[Global.SimHSI.Control].UsbValue = msg.Data[43];
                        ControlValuesList[Global.SimHSI.Record].UsbValue = msg.Data[44];
                        ControlValuesList[Global.Shutters].UsbValue = (msg.Data[53] << 8) | msg.Data[52];
                        ControlValuesList[Global.HSI.State].UsbValue = msg.Data[54];
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
        /// <param name="sender">The sender.</param>
        /// <param name="propertyName">Имя свойства.</param>
        private void FirePropertyChangedEvent(INotifyPropertyChanged sender = null, [CallerMemberName] string propertyName = null)
        {
            if ((null != PropertyChanged) && (null != propertyName))
            {
                PropertyChanged(null == sender ? this : sender, new PropertyChangedEventArgs(propertyName));
                System.Windows.Input.CommandManager.InvalidateRequerySuggested();
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
            protected void FirePropertyChangedEvent([CallerMemberName] string propertyName = null)
            {
                if ((null != Owner) && (null != PropertyChanged))
                {
                    Owner.FirePropertyChangedEvent(this, propertyName);
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
            /// <summary>
            /// Timeout на запись данных в файл.
            /// </summary>
            public const int WaitForWriteTime = 1000;

            /// <summary>
            /// Адресный байт "Данные интерфейса ВСИ".
            /// </summary>
            public const byte Addr = 0x14;
           
            /// <summary>
            /// КВВ ПК1: Вкл/выкл.
            /// </summary>
            private bool _isIssueEnable1;

            /// <summary>
            /// КВВ ПК2: Вкл/выкл.
            /// </summary>
            private bool _isIssueEnable2;

            /// <summary>
            /// КВВ ПК1: Счетчик выданных статусов.
            /// </summary>
            private int _stateCounter1;

            /// <summary>
            /// КВВ ПК1: Счетчик выданных кадров.
            /// </summary>
            private int _frameCounter1;

            /// <summary>
            /// КВВ ПК2: Счетчик выданных статусов.
            /// </summary>
            private int _stateCounter2;

            /// <summary>
            /// КВВ ПК2: Счетчик выданных кадров.
            /// </summary>
            private int _frameCounter2;

            /// <summary>
            /// Выдача УКС: Выдача УКС.
            /// </summary>
            private bool _isIssueCmd;

            /// <summary>
            /// Управление: Опрос данных.
            /// </summary>
            private bool _isIssueRequest;

            /// <summary>
            /// Управление: Линия передачи.
            /// </summary>
            private SimLine _lineOut;

            /// <summary>
            /// Управление: Линия приема.
            /// </summary>
            private SimLine _lineIn;

            /// <summary>
            /// КВВ ПК1: Линия передачи.
            /// </summary>
            private Line _line1;

            /// <summary>
            /// КВВ ПК2: Линия передачи.
            /// </summary>
            private Line _line2;

            /// <summary>
            /// Сохранять данные в файл.
            /// </summary>
            private bool _isSaveRawData;

            /// <summary>
            /// Имя файла данных.
            /// </summary>
            private string _rawDataFile;

            /// <summary>
            /// Для асинхронной записи в файл.
            /// </summary>
            private FileStream _rawDataStream;

            /// <summary>
            /// Квазиасинхронная запись в файл.
            /// Примечание:
            /// Используется для сигнала, что все данные записались в файл.
            /// </summary>
            private Task _rawDataTask;

            /// <summary>
            /// Количество запросов статуса по основной линии.
            /// </summary>
            private long _requestStateMain;

            /// <summary>
            /// Количество запросов статуса по резервной линии.
            /// </summary>
            private long _requestStateResv;

            /// <summary>
            /// Количество запросов данных по резервной линии.
            /// </summary>
            private long _requestDataResv;

            /// <summary>
            /// Количество запросов данных по основной линии.
            /// </summary>
            private long _requestDataMain;

            /// <summary>
            /// Количество переданных УКС ПК1.
            /// </summary>
            private long _cmdCounter1;

            /// <summary>
            /// Экземпляр команды на [включение КВВ ПК1].
            /// </summary>
            private ICommand _issueEnable1Command;

            /// <summary>
            /// Экземпляр команды на [включение КВВ ПК2].
            /// </summary>
            private ICommand _issueEnable2Command;

            /// <summary>
            /// Экземпляр команды на [включение опроса данных].
            /// </summary>
            private ICommand _issueRequestCommand;

            /// <summary>
            /// Экземпляр команды на [включение записи в файл].
            /// </summary>
            private ICommand _saveRawDataCommand;

            /// <summary>
            /// Экземпляр команды на [открытие данных из файла].
            /// </summary>
            private ICommand _fromFileCommand;

            /// <summary>
            /// Экземпляр команды на [выдачу активного УКС активация ПК1 по интерфейсу ВСИ].
            /// </summary>
            private ICommand _issueCmdEnable1Command;

            /// <summary>
            /// Экземпляр команды на [выдачу активного УКС деактивировать ПК-ы по интерфейсу ВСИ].
            /// </summary>
            private ICommand _issueCmdDisableCommand;

            /// <summary>
            /// Экземпляр команды на [выдачу активного УКС активация ПК2 по интерфейсу ВСИ].
            /// </summary>
            private ICommand _issueCmdEnable2Command;

            /// <summary>
            /// Экземпляр команды на [выдачу УКС по интерфейсу ВСИ].
            /// </summary>
            private ICommand _issueCmdCommand;

            /// <summary>
            /// Количество переданных УКС ПК2.
            /// </summary>
            private long _cmdCounter2;

            /// <summary>
            /// ПК1 статус: готов.
            /// </summary>
            private bool _isIssueReady1;

            /// <summary>
            /// ПК2 статус: готов.
            /// </summary>
            private bool _isIssueReady2;

            /// <summary>
            /// ПК2 статус: busy.
            /// </summary>
            private bool _isIssueBusy2;

            /// <summary>
            /// ПК1 статус: busy.
            /// </summary>
            private bool _isIssueBusy1;

            /// <summary>
            /// ПК2 статус: me.
            /// </summary>
            private bool _isIssueMe2;

            /// <summary>
            /// ПК1 статус: me.
            /// </summary>
            private bool _isIssueMe1;

            /// <summary>
            /// Активность первого полукомплекта.
            /// </summary>
            private bool _isActive1;

            /// <summary>
            /// Активность второго полукомплекта.
            /// </summary>
            private bool _isActive2;
                        
            /// <summary>
            /// Инициализирует новый экземпляр класса <see cref="HSI" />.
            /// </summary>
            /// <param name="owner">The owner.</param>
            public HSI(EgseBukNotify owner)
                : base(owner)
            {
            }

            /// <summary>
            /// Возможные состояния линии передачи.
            /// </summary>
            public enum Line
            {
                /// <summary>
                /// Линия передачи: отсутствует (оснавная и резервная заблокирована).
                /// </summary>
                [Description("Отключена")]
                Off = 0x03,
                
                /// <summary>
                /// Линия передачи: только по основной (резервная заблокирована).
                /// </summary>
                [Description("Основная")]
                Main = 0x02,

                /// <summary>
                /// Линия передачи: только по резервной (основная заблокирована).
                /// </summary>
                [Description("Резервная")]
                Resv = 0x01,

                /// <summary>
                /// Линия передачи: основная + резервная.
                /// </summary>
                [Description("Основная + резервная")]
                MainResv = 0x00
            }

            /// <summary>
            /// Линии имитатора БУК.
            /// </summary>
            public enum SimLine
            {
                /// <summary>
                /// Основная линия.
                /// </summary>
                [Description("Основная")]
                Main = 0x00,

                /// <summary>
                /// Резервная линия.
                /// </summary>
                [Description("Резервная")]
                Resv = 0x01
            }

            /// <summary>
            /// Получает или задает буфер данных для передачи в USB.
            /// </summary>
            /// <value>
            /// Буфер данных.
            /// </value>
            public byte[] Data { get; set; }

            /// <summary>
            /// Получает значение, показывающее, [состояние статуса ПК1: готов].
            /// </summary>
            /// <value>
            /// <c>true</c> если [состояние статуса ПК1: готов]; иначе, <c>false</c>.
            /// </value>
            public bool IsIssueReady1
            {
                get
                {
                    return _isIssueReady1;
                }

                private set 
                {
                    _isIssueReady1 = value;
                    ControlValuesList[Global.HSI.State].SetProperty(Global.HSI.State.IssueReady1, Convert.ToInt32(value));
                    FirePropertyChangedEvent();
                }
            }

            /// <summary>
            /// Получает значение, показывающее, [состояние статуса ПК2: готов].
            /// </summary>
            /// <value>
            /// <c>true</c> если [состояние статуса ПК2: готов]; иначе, <c>false</c>.
            /// </value>
            public bool IsIssueReady2
            {
                get
                {
                    return _isIssueReady2;
                }

                private set 
                {
                    _isIssueReady2 = value;
                    ControlValuesList[Global.HSI.State].SetProperty(Global.HSI.State.IssueReady2, Convert.ToInt32(value));
                    FirePropertyChangedEvent();
                }
            }

            /// <summary>
            /// Получает значение, показывающее, [состояние статуса ПК1: занят].
            /// </summary>
            /// <value>
            /// <c>true</c> если [состояние статуса ПК1: занят]; иначе, <c>false</c>.
            /// </value>
            public bool IsIssueBusy1
            {
                get
                {
                    return _isIssueBusy1;
                }

                private set 
                {
                    _isIssueBusy1 = value;
                    ControlValuesList[Global.HSI.State].SetProperty(Global.HSI.State.IssueBusy1, Convert.ToInt32(value));
                    FirePropertyChangedEvent();
                }
            }

            /// <summary>
            /// Получает значение, показывающее, [состояние статуса ПК2: занят].
            /// </summary>
            /// <value>
            /// <c>true</c> если [состояние статуса ПК2: занят]; иначе, <c>false</c>.
            /// </value>
            public bool IsIssueBusy2
            {
                get
                {
                    return _isIssueBusy2;
                }

                private set 
                {
                    _isIssueBusy2 = value;
                    ControlValuesList[Global.HSI.State].SetProperty(Global.HSI.State.IssueBusy2, Convert.ToInt32(value));
                    FirePropertyChangedEvent();
                }
            }

            /// <summary>
            /// Получает значение, показывающее, [состояние статуса ПК1: me].
            /// </summary>
            /// <value>
            /// <c>true</c> если [состояние статуса ПК1: me]; иначе, <c>false</c>.
            /// </value>
            public bool IsIssueMe1
            {
                get
                {
                    return _isIssueMe1;
                }

                private set 
                {
                    _isIssueMe1 = value;
                    ControlValuesList[Global.HSI.State].SetProperty(Global.HSI.State.IssueMe1, Convert.ToInt32(value));
                    FirePropertyChangedEvent();
                }
            }

            /// <summary>
            /// Получает значение, показывающее, [состояние статуса ПК2: me].
            /// </summary>
            /// <value>
            /// <c>true</c> если [состояние статуса ПК2: me]; иначе, <c>false</c>.
            /// </value>
            public bool IsIssueMe2
            {
                get
                {
                    return _isIssueMe2;
                }

                private set 
                {
                    _isIssueMe2 = value;
                    ControlValuesList[Global.HSI.State].SetProperty(Global.HSI.State.IssueMe2, Convert.ToInt32(value));
                    FirePropertyChangedEvent();
                }
            }

            /// <summary>
            /// Получает значение, показывающее, что [вкл КВВ ПК1].
            /// </summary>
            /// <value>
            ///   <c>true</c> если [вкл КВВ ПК1]; иначе, <c>false</c>.
            /// </value>
            public bool IsIssueEnable1
            {
                get
                {
                    return _isIssueEnable1;
                }

                private set 
                {
                    _isIssueEnable1 = value;
                    ControlValuesList[Global.HSI.Line1].SetProperty(Global.HSI.Line1.IssueEnable, Convert.ToInt32(value));
                    FirePropertyChangedEvent();
                }
            }

            /// <summary>
            /// Получает команду на [включение КВВ ПК1].
            /// </summary>
            /// <value>
            /// Команда на [включение КВВ ПК1].
            /// </value>
            public ICommand IssueEnable1Command
            {
                get
                {
                    if (null == _issueEnable1Command)
                    {
                        _issueEnable1Command = new RelayCommand(obj => { IsIssueEnable1 = !IsIssueEnable1; }, obj => { return true; });
                    }

                    return _issueEnable1Command;
                }
            }

            /// <summary>
            /// Получает значение, показывающее, что [включен КВВ ПК2].
            /// </summary>
            /// <value>
            /// <c>true</c> если [включен КВВ ПК2]; иначе, <c>false</c>.
            /// </value>
            public bool IsIssueEnable2
            {
                get
                {
                    return _isIssueEnable2;
                }

                private set 
                {
                    _isIssueEnable2 = value;
                    ControlValuesList[Global.HSI.Line2].SetProperty(Global.HSI.Line2.IssueEnable, Convert.ToInt32(value));
                    FirePropertyChangedEvent();
                }
            }

            /// <summary>
            /// Получает команду на [включение КВВ ПК2].
            /// </summary>
            /// <value>
            /// Команда на [включение КВВ ПК2].
            /// </value>
            public ICommand IssueEnable2Command
            {
                get
                {
                    if (null == _issueEnable2Command)
                    {
                        _issueEnable2Command = new RelayCommand(obj => { IsIssueEnable2 = !IsIssueEnable2; }, obj => { return true; });
                    }

                    return _issueEnable2Command;
                }
            }

            /// <summary>
            /// Получает количество выданных статусов по первому полукомплекту.
            /// </summary>
            /// <value>
            /// Количество выданных статусов.
            /// </value>
            public int StateCounter1
            {
                get
                {
                    return _stateCounter1;
                }

                private set 
                {
                    _stateCounter1 = value;
                    FirePropertyChangedEvent();
                }
            }

            /// <summary>
            /// Получает наименование файла для записи.
            /// </summary>
            /// <value>
            /// Наименование файла для записи.
            /// </value>
            public string RawDataFile
            {
                get
                {
                    return _rawDataFile;
                }

                private set 
                {
                    _rawDataFile = value;
                    if (string.Empty != value)
                    {
                        _rawDataStream = new FileStream(value, FileMode.Create, FileAccess.Write, FileShare.None, bufferSize: 4096, useAsync: true);
                    }
                    else
                    {
                        if (null != _rawDataStream)
                        {
                            try
                            {
                                if (null != _rawDataTask)
                                {
                                    _rawDataTask.Wait(WaitForWriteTime);
                                }
                            }
                            finally
                            {
                                _rawDataStream.Close();
                            }                            
                        }
                    }

                    FirePropertyChangedEvent();
                }
            }

            /// <summary>
            /// Получает значение, показывающее, что запись в файл включена.
            /// </summary>
            /// <value>
            /// <c>true</c> если требуется запись данных в файл; иначе, <c>false</c>.
            /// </value>
            public bool IsSaveRawData 
            { 
                get
                {
                    return _isSaveRawData;
                }

                private set  
                {
                    _isSaveRawData = value;
                    if (value)
                    {
                        RawDataFile = Owner.GetNewFileName(Resource.Get(@"stHsiLogName"));                    
                    }
                    else
                    {
                       RawDataFile = string.Empty;
                    }

                    FirePropertyChangedEvent();
                }
            }

            /// <summary>
            /// Получает команду на [включение записи данных в файл].
            /// </summary>
            /// <value>
            /// Команда на [включение записи данных в файл].
            /// </value>
            public ICommand SaveRawDataCommand
            {
                get
                {
                    if (null == _saveRawDataCommand)
                    {
                        _saveRawDataCommand = new RelayCommand(obj => { IsSaveRawData = !IsSaveRawData; }, obj => { return true; });
                    }

                    return _saveRawDataCommand;
                }
            }

            /// <summary>
            /// Получает количество выданных кадров по первому полукомплекту.
            /// </summary>
            /// <value>
            /// Количество выданных кадров.
            /// </value>
            public int FrameCounter1
            {
                get
                {
                    return _frameCounter1;
                }

                private set 
                {
                    _frameCounter1 = value;
                    FirePropertyChangedEvent();
                }
            }

            /// <summary>
            /// Получает количество выданных статусов по второму полукомплекту.
            /// </summary>
            /// <value>
            /// Количество выданных статусов.
            /// </value>
            public int StateCounter2
            {
                get
                {
                    return _stateCounter2;
                }

                private set 
                {
                    _stateCounter2 = value;
                    FirePropertyChangedEvent();
                }
            }

            /// <summary>
            /// Получает количество выданных кадров по второму полукомплекту.
            /// </summary>
            /// <value>
            /// Количество выданных кадров.
            /// </value>
            public int FrameCounter2
            {
                get
                {
                    return _frameCounter2;
                }

                private set 
                {
                    _frameCounter2 = value;
                    FirePropertyChangedEvent();
                }
            }

            /// <summary>
            /// Получает линию передачи ПК1.
            /// </summary>
            /// <value>
            /// Линия передачи ПК1.
            /// </value>
            public Line IssueLine1
            {
                get
                {
                    return _line1;
                }

                private set 
                {
                    _line1 = value;
                    ControlValuesList[Global.HSI.Line1].SetProperty(Global.HSI.Line1.Line, Convert.ToInt32(value));
                    FirePropertyChangedEvent();
                }
            }

            /// <summary>
            /// Получает значение, показывающее, что [ПК1 активен].
            /// </summary>
            /// <value>
            /// <c>true</c> если [ПК1 активен]; иначе, <c>false</c>.
            /// </value>
            public bool IsActive1
            {
                get
                {
                    return _isActive1;
                }

                private set 
                {
                    _isActive1 = value;
                    FirePropertyChangedEvent();
                }
            }

            /// <summary>
            /// Получает значение, показывающее, что [ПК2 активен].
            /// </summary>
            /// <value>
            /// <c>true</c> если [ПК2 активен]; иначе, <c>false</c>.
            /// </value>
            public bool IsActive2
            {
                get
                {
                    return _isActive2;
                }

                private set 
                {
                    _isActive2 = value;
                    FirePropertyChangedEvent();
                }
            }

            /// <summary>
            /// Получает линию передачи ПК2.
            /// </summary>
            /// <value>
            /// Линия передачи ПК2.
            /// </value>
            public Line IssueLine2
            {
                get
                {
                    return _line2;
                }

                private set 
                {
                    _line2 = value;
                    ControlValuesList[Global.HSI.Line2].SetProperty(Global.HSI.Line2.Line, Convert.ToInt32(value));
                    FirePropertyChangedEvent();
                }
            }

            /// <summary>
            /// Получает линию приема.
            /// </summary>
            /// <value>
            /// Линия приема.
            /// </value>
            public SimLine IssueLineIn
            {
                get
                {
                    return _lineIn;
                }

                private set 
                {
                    _lineIn = value;
                    ControlValuesList[Global.SimHSI.Control].SetProperty(Global.SimHSI.Control.LineIn, Convert.ToInt32(value));
                    FirePropertyChangedEvent();
                }
            }

            /// <summary>
            /// Получает линию передачи.
            /// </summary>
            /// <value>
            /// Линия передачи.
            /// </value>
            public SimLine IssueLineOut
            {
                get
                {
                    return _lineOut;
                }

                private set 
                {
                    _lineOut = value;
                    ControlValuesList[Global.SimHSI.Control].SetProperty(Global.SimHSI.Control.LineOut, Convert.ToInt32(value));
                    FirePropertyChangedEvent();
                }
            }

            /// <summary>
            /// Получает значение, показывающее, что [включен опрос данных].
            /// </summary>
            /// <value>
            ///   <c>true</c> если [включен опрос данных]; иначе, <c>false</c>.
            /// </value>
            public bool IsIssueRequest
            {
                get
                {
                    return _isIssueRequest;
                }

                private set 
                {
                    _isIssueRequest = value;
                    ControlValuesList[Global.SimHSI.Control].SetProperty(Global.SimHSI.Control.IssueRequest, Convert.ToInt32(value));
                    FirePropertyChangedEvent();
                }
            }

            /// <summary>
            /// Получает команду на [включение опроса данных].
            /// </summary>
            /// <value>
            /// Команда на [включение опроса данных].
            /// </value>
            public ICommand IssueRequestCommand
            {
                get
                {
                    if (_issueRequestCommand == null)
                    {
                        _issueRequestCommand = new RelayCommand(obj => { IsIssueRequest = !IsIssueRequest; }, obj => { return true; });
                    }

                    return _issueRequestCommand;
                }
            }

            /// <summary>
            /// Получает значение, показывающее, что [выдан УКС].
            /// </summary>
            /// <value>
            ///   <c>true</c> если [выдан УКС]; иначе, <c>false</c>.
            /// </value>
            public bool IsIssueCmd
            {
                get
                {
                    return _isIssueCmd;
                }

                private set 
                {
                    _isIssueCmd = value;                   
                    FirePropertyChangedEvent();
                }
            }

            /// <summary>
            /// Получает команду на [получение данных из файла].
            /// </summary>
            /// <value>
            /// Команда на [получение данных из файла].
            /// </value>
            public ICommand FromFileCommand
            {
                get
                {
                    if (_fromFileCommand == null)
                    {
                        _fromFileCommand = new RelayCommand(OpenFromFile, obj => { return true; });
                    }

                    return _fromFileCommand;
                }
            }

            /// <summary>
            /// Получает команду на [выдачу УКС по интерфейсу ВСИ].
            /// </summary>
            /// <value>
            /// Команда на [выдачу УКС по интерфейсу ВСИ].
            /// </value>
            public ICommand IssueCmdCommand
            {
                get
                {
                    if (_issueCmdCommand == null)
                    {
                        _issueCmdCommand = new RelayCommand(obj => { IsIssueCmd = true; ControlValuesList[Global.SimHSI.Record].SetProperty(Global.SimHSI.Record.IssueCmd, 1); }, obj => { return true; });
                    }

                    return _issueCmdCommand;
                }
            }

            /// <summary>
            /// Получает команду на [выдачу активного УКС активация ПК1 по интерфейсу ВСИ].
            /// </summary>
            /// <value>
            /// Команда на [выдачу активного УКС активация ПК1 по интерфейсу ВСИ].
            /// </value>
            public ICommand IssueCmdEnable1Command
            {
                get
                {
                    if (_issueCmdEnable1Command == null)
                    {
                        _issueCmdEnable1Command = new RelayCommand(obj => { Device.CmdSimHSI1(1); }, obj => { return true; });
                    }

                    return _issueCmdEnable1Command;
                }
            }

            /// <summary>
            /// Получает команду на [выдачу активного УКС деактивация ПК по интерфейсу ВСИ].
            /// </summary>
            /// <value>
            /// Команда на [выдачу активного УКС деактивация ПК по интерфейсу ВСИ].
            /// </value>
            public ICommand IssueCmdDisableCommand
            {
                get
                {
                    if (_issueCmdDisableCommand == null)
                    {
                        _issueCmdDisableCommand = new RelayCommand(obj => { Device.CmdSimHSI1(0); }, obj => { return true; });
                    }

                    return _issueCmdDisableCommand;
                }
            }

            /// <summary>
            /// Получает команду на [выдачу активного УКС активация ПК2 по интерфейсу ВСИ].
            /// </summary>
            /// <value>
            /// Команда на [выдачу активного УКС активация ПК2 по интерфейсу ВСИ].
            /// </value>
            public ICommand IssueCmdEnable2Command
            {
                get
                {
                    if (_issueCmdEnable2Command == null)
                    {
                        _issueCmdEnable2Command = new RelayCommand(obj => { Device.CmdSimHSI2(1); }, obj => { return true; });
                    }

                    return _issueCmdEnable2Command;
                }
            }

            /// <summary>
            /// Получает или задает количество запросов статусов по основной линии.
            /// </summary>
            /// <value>
            /// Количество запросов статусов по основной линии.
            /// </value>
            public long RequestStateMain
            {
                get
                {
                    return _requestStateMain;
                }

                set 
                {
                    _requestStateMain = value;
                    FirePropertyChangedEvent();
                }
            }

            /// <summary>
            /// Получает или задает количество запросов статусов по резервной линии.
            /// </summary>
            /// <value>
            /// Количество запросов статусов по резервной линии.
            /// </value>
            public long RequestStateResv
            {
                get
                {
                    return _requestStateResv;
                }

                set 
                {
                    _requestStateResv = value;
                    FirePropertyChangedEvent();
                }
            }

            /// <summary>
            /// Получает или задает количество запросов данных по основной линии.
            /// </summary>
            /// <value>
            /// Количество запросов данных по основной линии.
            /// </value>
            public long RequestDataMain
            {
                get
                {
                    return _requestDataMain;
                }

                set 
                {
                    _requestDataMain = value;
                    FirePropertyChangedEvent();
                }
            }

            /// <summary>
            /// Получает или задает количество запросов данных по резервной линии.
            /// </summary>
            /// <value>
            /// Количество запросов данных по резервной линии.
            /// </value>
            public long RequestDataResv
            {
                get
                {
                    return _requestDataResv;
                }

                set 
                {
                    _requestDataResv = value;
                    FirePropertyChangedEvent();
                }
            }

            /// <summary>
            /// Получает или задает количество полученых УКС ПК1.
            /// </summary>
            /// <value>
            /// Количество полученых УКС ПК1.
            /// </value>
            public long CmdCounter1
            {
                get
                {
                    return _cmdCounter1;
                }

                set 
                {
                    _cmdCounter1 = value;
                    FirePropertyChangedEvent();
                }
            }

            /// <summary>
            /// Получает или задает количество полученых УКС ПК2.
            /// </summary>
            /// <value>
            /// Количество полученых УКС ПК2.
            /// </value>
            public long CmdCounter2
            {
                get
                {
                    return _cmdCounter2;
                }

                set 
                {
                    _cmdCounter2 = value;
                    FirePropertyChangedEvent();
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
            /// Вызов диалога "Открыть файл".
            /// </summary>
            /// <param name="obj">The object.</param>
            public void OpenFromFile(object obj)
            {
                Data = Owner.OpenFromFile();
                FirePropertyChangedEvent("Data");
            }

            /// <summary>
            /// Вызывается когда [требуется записать данные сообщения в файл].
            /// </summary>
            /// <param name="sender">The sender.</param>
            /// <param name="e">The <see cref="HsiMsgEventArgs"/> instance containing the event data.</param>
            public virtual void OnHsiMsgRawSave(object sender, HsiMsgEventArgs e)
            {
                if (null != _rawDataStream)
                {
                    if (null != _rawDataTask)
                    {
                        _rawDataTask.Wait(WaitForWriteTime);
                    }

                    if (_rawDataStream.CanWrite)
                    {
                        _rawDataTask = _rawDataStream.WriteAsync(e.Data, 0, e.Data.Length);
                    }
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
                ControlValuesList.Add(Global.HSI.State, new ControlValue());
            }

            /// <summary>
            /// Initializes the properties.
            /// </summary>
            protected override void InitProperties()
            {
                ControlValuesList[Global.HSI.Line1].AddProperty(Global.HSI.Line1.Line, 1, 2, Device.CmdHSILine1, value => IssueLine1 = (Line)value);
                ControlValuesList[Global.HSI.Line1].AddProperty(Global.HSI.Line1.IssueEnable, 0, 1, Device.CmdHSILine1, value => IsIssueEnable1 = 1 == value);
                ControlValuesList[Global.HSI.Line2].AddProperty(Global.HSI.Line2.Line, 1, 2, Device.CmdHSILine2, value => IssueLine2 = (Line)value);
                ControlValuesList[Global.HSI.Line2].AddProperty(Global.HSI.Line2.IssueEnable, 0, 1, Device.CmdHSILine2, value => IsIssueEnable2 = 1 == value);
                ControlValuesList[Global.HSI.Line1StateCounter].AddProperty(Global.HSI.Line1StateCounter, 0, 32, delegate { }, value => StateCounter1 = value, true);
                ControlValuesList[Global.HSI.Line1FrameCounter].AddProperty(Global.HSI.Line1FrameCounter, 0, 32, delegate { }, value => FrameCounter1 = value, true);
                ControlValuesList[Global.HSI.Line2StateCounter].AddProperty(Global.HSI.Line2StateCounter, 0, 32, delegate { }, value => StateCounter2 = value, true);
                ControlValuesList[Global.HSI.Line2FrameCounter].AddProperty(Global.HSI.Line2FrameCounter, 0, 32, delegate { }, value => FrameCounter2 = value, true);
                ControlValuesList[Global.SimHSI.Control].AddProperty(Global.SimHSI.Control.LineIn, 2, 1, Device.CmdSimHSIControl, value => IssueLineIn = (SimLine)value);
                ControlValuesList[Global.SimHSI.Control].AddProperty(Global.SimHSI.Control.LineOut, 1, 1, Device.CmdSimHSIControl, value => IssueLineOut = (SimLine)value);
                ControlValuesList[Global.SimHSI.Control].AddProperty(Global.SimHSI.Control.IssueRequest, 0, 1, Device.CmdSimHSIControl, value => IsIssueRequest = 1 == value);
                ControlValuesList[Global.SimHSI.Record].AddProperty(Global.SimHSI.Record.IssueCmd, 0, 1, Device.CmdSimHSIRecord, value => IsIssueCmd = 1 == value);

                ControlValuesList[Global.HSI.State].AddProperty(Global.HSI.State.Active1, 6, 1, delegate { }, value => IsActive1 = 1 == value, true);
                ControlValuesList[Global.HSI.State].AddProperty(Global.HSI.State.Active2, 7, 1, delegate { }, value => IsActive2 = 1 == value, true);
                ControlValuesList[Global.HSI.State].AddProperty(Global.HSI.State.IssueReady1, 0, 1, Device.CmdHSIState, value => IsIssueReady1 = 1 == value);
                ControlValuesList[Global.HSI.State].AddProperty(Global.HSI.State.IssueReady2, 3, 1, Device.CmdHSIState, value => IsIssueReady2 = 1 == value);
                ControlValuesList[Global.HSI.State].AddProperty(Global.HSI.State.IssueBusy1, 1, 1, Device.CmdHSIState, value => IsIssueBusy1 = 1 == value);
                ControlValuesList[Global.HSI.State].AddProperty(Global.HSI.State.IssueBusy2, 4, 1, Device.CmdHSIState, value => IsIssueBusy2 = 1 == value);
                ControlValuesList[Global.HSI.State].AddProperty(Global.HSI.State.IssueMe1, 2, 1, Device.CmdHSIState, value => IsIssueMe1 = 1 == value);
                ControlValuesList[Global.HSI.State].AddProperty(Global.HSI.State.IssueMe2, 5, 1, Device.CmdHSIState, value => IsIssueMe2 = 1 == value);
            }            
        }

        /// <summary>
        /// Вспомогательный нотификатор, используется для самопроверки.
        /// </summary>
        public class UITest : SubNotify, IDataErrorInfo
        {
            /// <summary>
            /// Путь к usb-лог файлу.
            /// </summary>
            private string _usbLogFile;

            /// <summary>
            /// Инициализирует новый экземпляр класса <see cref="UITest" />.
            /// </summary>
            /// <param name="owner">The owner.</param>
            public UITest(EgseBukNotify owner)
                : base(owner)
            {
            }

            /// <summary>
            /// Получает или задает путь к файлу USB лога.
            /// </summary>
            /// <value>
            /// Путь к файлу USB лога.
            /// </value>
            public string UsbLogFile
            {
                get
                {
                    return _usbLogFile;
                }

                set 
                {
                    _usbLogFile = value;
                    FirePropertyChangedEvent();
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
            private bool _isPowerBusk1;

            /// <summary>
            /// Телеметрия: Запитан ПК2 от БУСК.
            /// </summary>
            private bool _isPowerBusk2;

            /// <summary>
            /// Телеметрия: Запитан ПК1 от БУНД.
            /// </summary>
            private bool _powerBund1;

            /// <summary>
            /// Телеметрия: Запитан ПК2 от БУНД.
            /// </summary>
            private bool _isPowerBund2;

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
            /// Экземпляр команды на [выдачу питания БУСК ПК1].
            /// </summary>
            private ICommand _issuePowerBusk1Command;

            /// <summary>
            /// Экземпляр команды на [выдачу питания БУСК ПК2].
            /// </summary>
            private ICommand _issuePowerBusk2Command;

            /// <summary>
            /// Экземпляр команды на [выдачу питания БУНД ПК1].
            /// </summary>
            private ICommand _issuePowerBund1Command;

            /// <summary>
            /// Экземпляр команды на [выдачу питания БУНД ПК2].
            /// </summary>
            private ICommand _issuePowerBund2Command;

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
                    FirePropertyChangedEvent();
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
                    FirePropertyChangedEvent();
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
                    FirePropertyChangedEvent();
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
                    FirePropertyChangedEvent();
                }
            }

            /// <summary>
            /// Получает значение, показывающее, есть ли [питание первого полукомплекта БУСК].
            /// </summary>
            /// <value>
            ///   <c>true</c> если [питание первого полукомплекта БУСК]; иначе, <c>false</c>.
            /// </value>
            public bool IsPowerBusk1
            {
                get
                {
                    return _isPowerBusk1;
                }

                private set 
                {
                    _isPowerBusk1 = value;
                    ControlValuesList[Global.Telemetry].SetProperty(Global.Telemetry.PowerBund1, Convert.ToInt32(!value), false);
                    FirePropertyChangedEvent();
                }
            }

            /// <summary>
            /// Получает команду на [выдачу питания БУСК ПК1].
            /// </summary>
            /// <value>
            /// Команда на [выдачу питания БУСК ПК1].
            /// </value>
            public ICommand IssuePowerBusk1Command
            {
                get
                {
                    if (null == _issuePowerBusk1Command)
                    {
                        _issuePowerBusk1Command = new RelayCommand(obj => { ControlValuesList[Global.Telemetry].SetProperty(Global.Telemetry.PowerBusk1, Convert.ToInt32(!IsPowerBusk1)); IsPowerBusk1 = !IsPowerBusk1; }, obj => { return true; });
                    }

                    return _issuePowerBusk1Command;
                }
            }

            /// <summary>
            /// Получает значение, показывающее, есть ли [питание второго полукомплекта БУСК].
            /// </summary>
            /// <value>
            ///   <c>true</c> если [питание второго полукомплекта БУСК]; иначе, <c>false</c>.
            /// </value>
            public bool IsPowerBusk2
            {
                get
                {
                    return _isPowerBusk2;
                }

                private set 
                {
                    _isPowerBusk2 = value;
                    ControlValuesList[Global.Telemetry].SetProperty(Global.Telemetry.PowerBund2, Convert.ToInt32(!value), false);
                    FirePropertyChangedEvent();
                }
            }

            /// <summary>
            /// Получает команду на [выдачу питания БУСК ПК2].
            /// </summary>
            /// <value>
            /// Команда на [выдачу питания БУСК ПК2].
            /// </value>
            public ICommand IssuePowerBusk2Command
            {
                get
                {
                    if (null == _issuePowerBusk2Command)
                    {
                        _issuePowerBusk2Command = new RelayCommand(obj => { ControlValuesList[Global.Telemetry].SetProperty(Global.Telemetry.PowerBusk2, Convert.ToInt32(!IsPowerBusk2)); IsPowerBusk2 = !IsPowerBusk2; }, obj => { return true; });
                    }

                    return _issuePowerBusk2Command;
                }
            }

            /// <summary>
            /// Получает значение, показывающее, есть ли [питание первого полукомплекта БУНД].
            /// </summary>
            /// <value>
            ///   <c>true</c> если [питание первого полукомплекта БУНД]; иначе, <c>false</c>.
            /// </value>
            public bool IsPowerBund1
            {
                get
                {
                    return _powerBund1;
                }

                private set 
                {
                    _powerBund1 = value;
                    ControlValuesList[Global.Telemetry].SetProperty(Global.Telemetry.PowerBusk1, Convert.ToInt32(!value), false);
                    FirePropertyChangedEvent();
                }
            }

            /// <summary>
            /// Получает команду на [выдачу питания БУНД ПК1].
            /// </summary>
            /// <value>
            /// Команда на [выдачу питания БУНД ПК1].
            /// </value>
            public ICommand IssuePowerBund1Command
            {
                get
                {
                    if (null == _issuePowerBund1Command)
                    {
                        _issuePowerBund1Command = new RelayCommand(obj => { ControlValuesList[Global.Telemetry].SetProperty(Global.Telemetry.PowerBund1, Convert.ToInt32(!IsPowerBund1)); IsPowerBund1 = !IsPowerBund1; }, obj => { return true; });
                    }

                    return _issuePowerBund1Command;
                }
            }

            /// <summary>
            /// Получает значение, показывающее, есть ли [питание второго полукомплекта БУНД].
            /// </summary>
            /// <value>
            ///   <c>true</c> если [питание второго полукомплекта БУНД]; иначе, <c>false</c>.
            /// </value>
            public bool IsPowerBund2
            {
                get
                {
                    return _isPowerBund2;
                }

                private set 
                {
                    _isPowerBund2 = value;
                    ControlValuesList[Global.Telemetry].SetProperty(Global.Telemetry.PowerBusk2, Convert.ToInt32(!value), false);
                    FirePropertyChangedEvent();
                }
            }

            /// <summary>
            /// Получает команду на [выдачу питания БУНД ПК2].
            /// </summary>
            /// <value>
            /// Команда на [выдачу питания БУНД ПК2].
            /// </value>
            public ICommand IssuePowerBund2Command
            {
                get
                {
                    if (null == _issuePowerBund2Command)
                    {
                        _issuePowerBund2Command = new RelayCommand(obj => { ControlValuesList[Global.Telemetry].SetProperty(Global.Telemetry.PowerBund2, Convert.ToInt32(!IsPowerBund2)); IsPowerBund2 = !IsPowerBund2; }, obj => { return true; });
                    }

                    return _issuePowerBund2Command;
                }
            }

            /// <summary>
            /// Получает значение, показывающее, включен ли [затвор прибора УФЕС ОСН].
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

                private set 
                {
                    _ufesLock1 = value;
                    ControlValuesList[Global.Telemetry].SetProperty(Global.Telemetry.UfesLock1, Convert.ToInt32(value));
                    FirePropertyChangedEvent();
                }
            }

            /// <summary>
            /// Получает значение, показывающее, включен ли [затвор прибора УФЕС РЕЗ].
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

                private set 
                {
                    _ufesLock2 = value;
                    ControlValuesList[Global.Telemetry].SetProperty(Global.Telemetry.UfesLock2, Convert.ToInt32(value));
                    FirePropertyChangedEvent();
                }
            }

            /// <summary>
            /// Получает значение, показывающее, включен ли [затвор прибора ВУФЕС ОСН].
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

                private set 
                {
                    _vufesLock1 = value;
                    ControlValuesList[Global.Telemetry].SetProperty(Global.Telemetry.VufesLock1, Convert.ToInt32(value));
                    FirePropertyChangedEvent();
                }
            }

            /// <summary>
            /// Получает значение, показывающее, включен ли [затвор прибора ВУФЕС РЕЗ].
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

                private set 
                {
                    _vufesLock2 = value;
                    ControlValuesList[Global.Telemetry].SetProperty(Global.Telemetry.VufesLock2, Convert.ToInt32(value));
                    FirePropertyChangedEvent();
                }
            }

            /// <summary>
            /// Получает значение, показывающее, включен ли [затвор прибора СДЩ ОСН].
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

                private set 
                {
                    _sdchshLock1 = value;
                    ControlValuesList[Global.Telemetry].SetProperty(Global.Telemetry.SdchshLock1, Convert.ToInt32(value));
                    FirePropertyChangedEvent();
                }
            }

            /// <summary>
            /// Получает значение, показывающее, включен ли [затвор прибора СДЩ РЕЗ].
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

                private set 
                {
                    _sdchshLock2 = value;
                    ControlValuesList[Global.Telemetry].SetProperty(Global.Telemetry.SdchshLock2, Convert.ToInt32(value));
                    FirePropertyChangedEvent();
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
                    FirePropertyChangedEvent();
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
                    FirePropertyChangedEvent();
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
                    FirePropertyChangedEvent();
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
                    FirePropertyChangedEvent();
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
                    FirePropertyChangedEvent();
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
                    FirePropertyChangedEvent();
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
                    FirePropertyChangedEvent();
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
                    FirePropertyChangedEvent();
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
                    FirePropertyChangedEvent();
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
                    FirePropertyChangedEvent();
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
                    FirePropertyChangedEvent();
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
                    FirePropertyChangedEvent();
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
                ControlValuesList[Global.Telemetry].AddProperty(Global.Telemetry.PowerBusk1, 15, 1, Device.CmdPowerBusk1, value => IsPowerBusk1 = 1 == value);
                ControlValuesList[Global.Telemetry].AddProperty(Global.Telemetry.PowerBusk2, 14, 1, Device.CmdPowerBusk2, value => IsPowerBusk2 = 1 == value);
                ControlValuesList[Global.Telemetry].AddProperty(Global.Telemetry.PowerBund1, 12, 1, Device.CmdPowerBund1, value => IsPowerBund1 = 1 == value);
                ControlValuesList[Global.Telemetry].AddProperty(Global.Telemetry.PowerBund2, 13, 1, Device.CmdPowerBund2, value => IsPowerBund2 = 1 == value);
                ControlValuesList[Global.Telemetry].AddProperty(Global.Telemetry.UfesLight1, 6, 1, delegate { }, value => UfesLight1 = 1 == value, true);
                ControlValuesList[Global.Telemetry].AddProperty(Global.Telemetry.UfesLight2, 7, 1, delegate { }, value => UfesLight2 = 1 == value, true);
                ControlValuesList[Global.Telemetry].AddProperty(Global.Telemetry.VufesLight1, 8, 1, delegate { }, value => VufesLight1 = 1 == value, true);
                ControlValuesList[Global.Telemetry].AddProperty(Global.Telemetry.VufesLight2, 9, 1, delegate { }, value => VufesLight2 = 1 == value, true);
                ControlValuesList[Global.Telemetry].AddProperty(Global.Telemetry.SdchshLight1, 10, 1, delegate { }, value => SdchshLight1 = 1 == value, true);
                ControlValuesList[Global.Telemetry].AddProperty(Global.Telemetry.SdchshLight2, 11, 1, delegate { }, value => SdchshLight2 = 1 == value, true);
                ControlValuesList[Global.Telemetry].AddProperty(Global.Telemetry.UfesPower1, 20, 1, delegate { }, value => UfesPower1 = 1 == value, true);
                ControlValuesList[Global.Telemetry].AddProperty(Global.Telemetry.UfesPower2, 21, 1, delegate { }, value => UfesPower2 = 1 == value, true);
                ControlValuesList[Global.Telemetry].AddProperty(Global.Telemetry.VufesPower1, 19, 1, delegate { }, value => VufesPower1 = 1 == value, true);
                ControlValuesList[Global.Telemetry].AddProperty(Global.Telemetry.VufesPower2, 18, 1, delegate { }, value => VufesPower2 = 1 == value, true);
                ControlValuesList[Global.Telemetry].AddProperty(Global.Telemetry.SdchshPower1, 17, 1, delegate { }, value => SdchshPower1 = 1 == value, true);
                ControlValuesList[Global.Telemetry].AddProperty(Global.Telemetry.SdchshPower2, 16, 1, delegate { }, value => SdchshPower2 = 1 == value, true);
                ControlValuesList[Global.Telemetry].AddProperty(Global.Telemetry.UfesLock1, 0, 1, delegate { }, value => UfesLock1 = 1 == value, true);
                ControlValuesList[Global.Telemetry].AddProperty(Global.Telemetry.UfesLock2, 1, 1, delegate { }, value => UfesLock2 = 1 == value, true);
                ControlValuesList[Global.Telemetry].AddProperty(Global.Telemetry.VufesLock1, 2, 1, delegate { }, value => VufesLock1 = 1 == value, true);
                ControlValuesList[Global.Telemetry].AddProperty(Global.Telemetry.VufesLock2, 3, 1, delegate { }, value => VufesLock2 = 1 == value, true);
                ControlValuesList[Global.Telemetry].AddProperty(Global.Telemetry.SdchshLock1, 4, 1, delegate { }, value => SdchshLock1 = 1 == value, true);
                ControlValuesList[Global.Telemetry].AddProperty(Global.Telemetry.SdchshLock2, 5, 1, delegate { }, value => SdchshLock2 = 1 == value, true);
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
            private bool _isIssueEnable;

            /// <summary>
            /// Управление: Установлена связь.
            /// </summary>
            private bool _isConnect;

            /// <summary>
            /// SPTP: включение обмена прибора НП1.
            /// </summary>
            private bool _isSD1Trans;

            /// <summary>
            /// SPTP: включение обмена прибора НП2.
            /// </summary>
            private bool _isSD2Trans;

            /// <summary>
            /// SPTP: можно выдавать пакет в НП1.
            /// </summary>
            private bool _isSD1TransData;

            /// <summary>
            /// SPTP: можно выдавать пакет в НП2.
            /// </summary>
            private bool _isSD2TransData;

            /// <summary>
            /// SPTP: Адрес БУСК.
            /// </summary>
            private int _logicBusk = 0;

            /// <summary>
            /// SPTP: Адрес НП1.
            /// </summary>
            private int _logicSD1 = 0;

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
            /// Экземпляр команды на [включение интерфейса spacewire].
            /// </summary>
            private ICommand _issueEnableCommand;

            /// <summary>
            /// Экземпляр команды на [выдачу посылки по интерфейсу spacewire].
            /// </summary>
            private ICommand _issuePackageCommand;
            
            /// <summary>
            /// Экземпляр команды на [включение обмена для НП1].
            /// </summary>
            private ICommand _issueSD1TransCommand;

            /// <summary>
            /// Экземпляр команды на [включение обмена для НП2].
            /// </summary>
            private ICommand _issueSD2TransCommand;

            /// <summary>
            /// Экземпляр команды на [включение выдачи пакетов данных в НП1].
            /// </summary>
            private ICommand _issueSD1TransDataCommand;

            /// <summary>
            /// Экземпляр команды на [включение выдачи пакетов данных в НП2].
            /// </summary>
            private ICommand _issueSD2TransDataCommand;

            /// <summary>
            /// Инициализирует новый экземпляр класса <see cref="Spacewire1" />.
            /// </summary>
            /// <param name="owner">The owner.</param>
            public Spacewire1(EgseBukNotify owner)
                : base(owner)
            {
            }

            /// <summary>
            /// Получает буфер данных для передачи в USB.
            /// </summary>
            /// <value>
            /// Буфер данных.
            /// </value>
            public byte[] Data { get; private set; }

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
                    return string.Format(Resource.Get(@"stShowSimLogicBusk"), LogicBusk.ToString(/*"X2"*/));
                }

                private set 
                {
                    FirePropertyChangedEvent();
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
                    ControlValuesList[Global.Spacewire1.BuskLogic].SetProperty(Global.Spacewire1.BuskLogic, value);                    
                    FirePropertyChangedEvent();
                    ShowLogicBusk = string.Empty;
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
                    return string.Format(Resource.Get(@"stShowSimLogicSD1"), LogicSD1.ToString(/*"X2"*/));
                }

                private set 
                { 
                    FirePropertyChangedEvent(); 
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
                    ControlValuesList[Global.Spacewire1.SD1Logic].SetProperty(Global.Spacewire1.SD1Logic, value);
                    FirePropertyChangedEvent();
                    ShowLogicSD1 = string.Empty;                    
                }
            }

            /// <summary>
            /// Получает значение, показывающее, что [интерфейс SpaceWire1 включен].
            /// </summary>
            /// <value>
            /// <c>true</c> если [интерфейс SpaceWire1 включен]; иначе, <c>false</c>.
            /// </value>
            public bool IsIssueEnable
            {
                get
                {
                    return _isIssueEnable;
                }

                private set 
                {
                    _isIssueEnable = value;
                    ControlValuesList[Global.Spacewire1.Control].SetProperty(Global.Spacewire1.Control.IssueEnable, Convert.ToInt32(value));
                    ControlValuesList[Global.Spacewire2.Control].SetProperty(Global.Spacewire2.Control.Connect, Convert.ToInt32(0));
                    FirePropertyChangedEvent();
                }
            }

            /// <summary>
            /// Получает команду на [включение интерфейса spacewire].
            /// </summary>
            /// <value>
            /// Команда на [включение интерфейса spacewire].
            /// </value>
            public ICommand IssueEnableCommand
            {
                get
                {
                    if (null == _issueEnableCommand)
                    {
                        _issueEnableCommand = new RelayCommand(obj => { IsIssueEnable = !IsIssueEnable; }, obj => { return true; });
                    }

                    return _issueEnableCommand;
                }
            }

            /// <summary>
            /// Получает значение, показывающее, что [установлена связь по интерфейсу Spacewire].
            /// </summary>
            /// <value>
            /// <c>true</c> если [установлена связь по интерфейсу Spacewire]; иначе, <c>false</c>.
            /// </value>
            public bool IsConnect
            {
                get
                {
                    return _isConnect;
                }

                private set 
                {
                    _isConnect = value;
                    ControlValuesList[Global.Spacewire1.Control].SetProperty(Global.Spacewire1.Control.Connect, Convert.ToInt32(value));
                    FirePropertyChangedEvent();
                }
            }

            /// <summary>
            /// Получает значение, показывающее, что [включен обмен для прибора НП1].
            /// </summary>
            /// <value>
            /// <c>true</c> если [включен обмен для прибора НП1]; иначе, <c>false</c>.
            /// </value>
            public bool IsSD1Trans
            {
                get
                {
                    return _isSD1Trans;
                }

                private set 
                {
                    _isSD1Trans = value;
                    ControlValuesList[Global.Spacewire1.SPTPControl].SetProperty(Global.Spacewire1.SPTPControl.SD1Trans, Convert.ToInt32(value));
                    FirePropertyChangedEvent();
                }
            }

            /// <summary>
            /// Получает команду на [включение обмена для НП1].
            /// </summary>
            /// <value>
            /// Команда на [включение обмена для НП1].
            /// </value>
            public ICommand IssueSD1TransCommand
            {
                get
                {
                    if (null == _issueSD1TransCommand)
                    {
                        _issueSD1TransCommand = new RelayCommand(obj => { IsSD1Trans = !IsSD1Trans; }, obj => { return true; });
                    }

                    return _issueSD1TransCommand;
                }
            }

            /// <summary>
            /// Получает значение, показывающее, что [включен обмен для прибора НП2].
            /// </summary>
            /// <value>
            /// <c>true</c> если [включен обмен для прибора НП2]; иначе, <c>false</c>.
            /// </value>
            public bool IsSD2Trans
            {
                get
                {
                    return _isSD2Trans;
                }

                private set 
                {
                    _isSD2Trans = value;
                    ControlValuesList[Global.Spacewire1.SPTPControl].SetProperty(Global.Spacewire1.SPTPControl.SD2Trans, Convert.ToInt32(value));
                    FirePropertyChangedEvent();
                }
            }

            /// <summary>
            /// Получает команду на [включение обмена для НП2].
            /// </summary>
            /// <value>
            /// Команда на [включение обмена для НП2].
            /// </value>
            public ICommand IssueSD2TransCommand
            {
                get
                {
                    if (null == _issueSD2TransCommand)
                    {
                        _issueSD2TransCommand = new RelayCommand(obj => { IsSD2Trans = !IsSD2Trans; }, obj => { return true; });
                    }

                    return _issueSD2TransCommand;
                }
            }

            /// <summary>
            /// Получает значение, показывающее, что [можно выдавать пакеты данных в НП1].
            /// </summary>
            /// <value>
            /// <c>true</c> если [можно выдавать пакеты данных в НП1]; иначе, <c>false</c>.
            /// </value>
            public bool IsSD1TransData
            {
                get
                {
                    return _isSD1TransData;
                }

                private set 
                {
                    _isSD1TransData = value;
                    ControlValuesList[Global.Spacewire1.SPTPControl].SetProperty(Global.Spacewire1.SPTPControl.SD1TransData, Convert.ToInt32(value));
                    FirePropertyChangedEvent();
                }
            }

            /// <summary>
            /// Получает команду на [включение выдачи пакетов данных в НП1].
            /// </summary>
            /// <value>
            /// Команда на [включение выдачи пакетов данных в НП1].
            /// </value>
            public ICommand IssueSD1TransDataCommand
            {
                get
                {
                    if (null == _issueSD1TransDataCommand)
                    {
                        _issueSD1TransDataCommand = new RelayCommand(obj => { IsSD1TransData = !IsSD1TransData; }, obj => { return true; });
                    }

                    return _issueSD1TransDataCommand;
                }
            }

            /// <summary>
            /// Получает значение, показывающее, что [можно выдавать пакеты данных в НП2].
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

                private set 
                {
                    _isSD2TransData = value;
                    ControlValuesList[Global.Spacewire1.SPTPControl].SetProperty(Global.Spacewire1.SPTPControl.SD2TransData, Convert.ToInt32(value));
                    FirePropertyChangedEvent();
                }
            }

            /// <summary>
            /// Получает команду на [включение выдачи пакетов данных в НП2].
            /// </summary>
            /// <value>
            /// Команда на [включение выдачи пакетов данных в НП2].
            /// </value>
            public ICommand IssueSD2TransDataCommand
            {
                get
                {
                    if (null == _issueSD2TransDataCommand)
                    {
                        _issueSD2TransDataCommand = new RelayCommand(obj => { IsSD2TransData = !IsSD2TransData; }, obj => { return true; });
                    }

                    return _issueSD2TransDataCommand;
                }
            }

            /// <summary>
            /// Получает значение [Счетчик миллисекунд для НП1 (через сколько готовы данные)].
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

                private set 
                {
                    _sd1SendTime = value;
                    ControlValuesList[Global.Spacewire1.SD1SendTime].SetProperty(Global.Spacewire1.SD1SendTime, Convert.ToInt32(value));
                    FirePropertyChangedEvent();
                }
            }

            /// <summary>
            /// Получает значение [Счетчик миллисекунд для НП2 (через сколько готовы данные)].
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

                private set 
                {
                    _sd2SendTime = value;
                    ControlValuesList[Global.Spacewire1.SD2SendTime].SetProperty(Global.Spacewire1.SD2SendTime, Convert.ToInt32(value));
                    FirePropertyChangedEvent();
                }
            }

            /// <summary>
            /// Получает значение [кол-во байт в пакете НП1].
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

                private set 
                {
                    _sd1DataSize = value;
                    ControlValuesList[Global.Spacewire1.SD1DataSize].SetProperty(Global.Spacewire1.SD1DataSize, Convert.ToInt32(value));
                    FirePropertyChangedEvent();
                }
            }

            /// <summary>
            /// Получает значение [кол-во байт в пакете НП2].
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

                private set 
                {
                    _sd2DataSize = value;
                    ControlValuesList[Global.Spacewire1.SD2DataSize].SetProperty(Global.Spacewire1.SD2DataSize, Convert.ToInt32(value));
                    FirePropertyChangedEvent();
                }
            }

            /// <summary>
            /// Получает значение, показывающее, что [Бит занятости записи - 1].
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

                private set 
                {
                    _isRecordBusy = value;
                    FirePropertyChangedEvent();
                }
            }

            /// <summary>
            /// Получает значение, показывающее, что [Бит выдачи посылки - 1].
            /// </summary>
            /// <value>
            ///   <c>true</c> если [Бит выдачи посылки - 1]; иначе, <c>false</c>.
            /// </value>
            public bool IsIssuePackage
            {
                get
                {
                    return _isRecordSend;
                }

                private set 
                {
                    _isRecordSend = value;
                    FirePropertyChangedEvent();
                }
            }

            /// <summary>
            /// Получает команду на [выдачу посылки по интерфейсу spacewire].
            /// </summary>
            /// <value>
            /// Команда на [выдачу посылки по интерфейсу spacewire].
            /// </value>
            public ICommand IssuePackageCommand
            {
                get
                {
                    if (_issuePackageCommand == null)
                    {
                        _issuePackageCommand = new RelayCommand(obj => { IsIssuePackage = true; ControlValuesList[Global.Spacewire1.Record].SetProperty(Global.Spacewire1.Record.IssuePackage, 1); }, obj => { return !IsRecordBusy; });
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
                ControlValuesList.Add(Global.Spacewire1.Control, new ControlValue());
                ControlValuesList.Add(Global.Spacewire1.Record, new ControlValue());
                ControlValuesList.Add(Global.Spacewire1.SPTPControl, new ControlValue());
                ControlValuesList.Add(Global.Spacewire1.BuskLogic, new ControlValue());
                ControlValuesList.Add(Global.Spacewire1.SD1Logic, new ControlValue());
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
                ControlValuesList[Global.Spacewire1.Control].AddProperty(Global.Spacewire1.Control.IssueEnable, 0, 1, Device.CmdSpacewire1Control, value => IsIssueEnable = 1 == value);
                ControlValuesList[Global.Spacewire1.Control].AddProperty(Global.Spacewire1.Control.Connect, 3, 1, delegate { }, value => IsConnect = 1 == value, true);
                ControlValuesList[Global.Spacewire1.BuskLogic].AddProperty(Global.Spacewire1.BuskLogic, 0, 8, Device.CmdSpacewire1LogicBusk, value => LogicBusk = value);
                ControlValuesList[Global.Spacewire1.SD1Logic].AddProperty(Global.Spacewire1.SD1Logic, 0, 8, Device.CmdSpacewire1LogicSD1, value => LogicSD1 = value);

                ControlValuesList[Global.Spacewire1.SPTPControl].AddProperty(Global.Spacewire1.SPTPControl.SD1Trans, 0, 1, Device.CmdSpacewire1ControlSPTP, value => IsSD1Trans = 1 == value);
                ControlValuesList[Global.Spacewire1.SPTPControl].AddProperty(Global.Spacewire1.SPTPControl.SD2Trans, 2, 1, Device.CmdSpacewire1ControlSPTP, value => IsSD2Trans = 1 == value);
                ControlValuesList[Global.Spacewire1.SPTPControl].AddProperty(Global.Spacewire1.SPTPControl.SD1TransData, 1, 1, Device.CmdSpacewire1ControlSPTP, value => IsSD1TransData = 1 == value);
                ControlValuesList[Global.Spacewire1.SPTPControl].AddProperty(Global.Spacewire1.SPTPControl.SD2TransData, 3, 1, Device.CmdSpacewire1ControlSPTP, value => IsSD2TransData = 1 == value);

                ControlValuesList[Global.Spacewire1.SD1SendTime].AddProperty(Global.Spacewire1.SD1SendTime, 0, 16, Device.CmdSpacewire1SPTPControlSD1SendTime, value => SD1SendTime = value);
                ControlValuesList[Global.Spacewire1.SD2SendTime].AddProperty(Global.Spacewire1.SD2SendTime, 0, 16, Device.CmdSpacewire1SPTPControlSD2SendTime, value => SD2SendTime = value);
                ControlValuesList[Global.Spacewire1.SD1DataSize].AddProperty(Global.Spacewire1.SD1DataSize, 0, 16, Device.CmdSpacewire1SPTPControlSD1DataSize, value => SD1DataSize = value);
                ControlValuesList[Global.Spacewire1.SD2DataSize].AddProperty(Global.Spacewire1.SD2DataSize, 0, 16, Device.CmdSpacewire1SPTPControlSD2DataSize, value => SD2DataSize = value);
                ControlValuesList[Global.Spacewire1.Record].AddProperty(Global.Spacewire1.Record.Busy, 3, 1, delegate { }, value => IsRecordBusy = 1 == value);
                ControlValuesList[Global.Spacewire1.Record].AddProperty(Global.Spacewire1.Record.IssuePackage, 0, 1, Device.CmdSpacewire1Record, value => IsIssuePackage = 1 == value);
            }
        }

        /// <summary>
        /// Нотификатор spacewire2.
        /// </summary>
        public class Spacewire2 : SubNotify, IDataErrorInfo
        {
            /// <summary>
            /// Timeout на запись данных в файл.
            /// </summary>
            public const int WaitForWriteTime = 1000;

            /// <summary>
            /// SPTP: Адрес ИМИТАТОРА БУСКа.
            /// </summary>
            private int _logicBusk = 0;

            /// <summary>
            /// SPTP: Адрес БС.
            /// </summary>
            private int _logicBuk = 0;

            /// <summary>
            /// Управление: вкл/выкл интерфейса Spacewire.
            /// </summary>
            private bool _isIssueEnable;

            /// <summary>
            /// Управление: Установлена связь.
            /// </summary>
            private bool _isConnect;

            /// <summary>
            /// Запись данных(до 1 Кбайт): Выдача посылки RMAP (самосбр.).
            /// </summary>
            private bool _isIssueRMap;

            /// <summary>
            /// Запись данных(до 1 Кбайт): 1 – выдача посылки в прибор БС (самосбр.).
            /// </summary>
            private bool _isIssuePackage;

            /// <summary>
            /// Управление обменом с приборами по SPTP: включить выдачу секундных меток (1PPS).
            /// </summary>
            private bool _isIssueTimeMark;

            /// <summary>
            /// Управление обменом с приборами по SPTP: включение обмена прибора БС.
            /// </summary>
            private bool _isIssueTrans;

            /// <summary>
            /// Управление обменом с приборами по SPTP: можно выдавать пакет в БС.
            /// </summary>
            private bool _isTransData;

            /// <summary>
            /// Управление обменом с приборами по SPTP: выдача КБВ прибору БС (только при «1 PPS» == 1).
            /// </summary>
            private bool _isIssueKbv;

            /// <summary>
            /// Управление: Выбор канала.
            /// </summary>
            private Channel _spacewireChannel;

            /// <summary>
            /// Количество предоставления квот от БУСК.
            /// </summary>
            private long _replyQueueFromBusk;

            /// <summary>
            /// Количество предоставления квот от БУК.
            /// </summary>
            private long _replyQueueFromBuk;

            /// <summary>
            /// Количество запросов квот от БУК.
            /// </summary>
            private long _requestQueueFromBuk;

            /// <summary>
            /// Количество запросов квот от БУСК.
            /// </summary>
            private long _requestQueueFromBusk;

            /// <summary>
            /// Текущее КБВ.
            /// </summary>
            private long _codeOnboardTime;

            /// <summary>
            /// APID для формирования команды.
            /// </summary>
            private short _setApid;

            /// <summary>
            /// Текущее значение Timetick1 от БУСК.
            /// </summary>
            private byte _buskTickTime1;

            /// <summary>
            /// Текущее значение Timetick2 от БУСК.
            /// </summary>
            private byte _buskTickTime2;

            /// <summary>
            /// Текущее значение Timetick1 от БУК.
            /// </summary>
            private byte _bukTickTime1;

            /// <summary>
            /// Текущее значение Timetick2 от БУК.
            /// </summary>
            private byte _bukTickTime2;

            /// <summary>
            /// Формировать телекоманду.
            /// </summary>
            private bool _isMakeTK;

            /// <summary>
            /// Сохранять данные в файл.
            /// </summary>
            private bool _isSaveRawData;

            /// <summary>
            /// Имя файла данных.
            /// </summary>
            private string _rawDataFile;

            /// <summary>
            /// Для асинхронной записи в файл.
            /// </summary>
            private FileStream _rawDataStream;

            /// <summary>
            /// Квазиасинхронная запись в файл.
            /// Примечание:
            /// Используется для сигнала, что все данные записались в файл.
            /// </summary>
            private Task _rawDataTask;

            /// <summary>
            /// Экземпляр команды на [выдачу посылки по интерфейсу spacewire].
            /// </summary>
            private ICommand _issuePackageCommand;

            /// <summary>
            /// Экземпляр команды на [выдачу посылки RMAP по интерфейсу spacewire].
            /// </summary>
            private ICommand _issueRMapCommand;

            /// <summary>
            /// Экземпляр команды на [формирование посылки телекоманды по интерфейсу spacewire].
            /// </summary>
            private ICommand _issueMakeTKCommand;

            /// <summary>
            /// Экземпляр команды на [включение передачи метки времени по интерфейсу spacewire].
            /// </summary>
            private ICommand _issueTimeMarkCommand;

            /// <summary>
            /// Экземпляр команды на [включение интерфейса spacewire].
            /// </summary>
            private ICommand _issueEnableCommand;

            /// <summary>
            /// Экземпляр команды на [включение обмена для прибора БУК].
            /// </summary>
            private ICommand _issueTransCommand;

            /// <summary>
            /// Экземпляр команды на [включение выдачи КБВ для прибора БУК].
            /// </summary>
            private ICommand _issueKbvCommand;

            /// <summary>
            /// Экземпляр команды на [открыть из файла].
            /// </summary>
            private ICommand _fromFileCommand;

            /// <summary>
            /// Экземпляр команды на [включение записи в файл].
            /// </summary>
            private ICommand _saveRawDataCommand;

            /// <summary>
            /// Инициализирует новый экземпляр класса <see cref="Spacewire2" />.
            /// </summary>
            /// <param name="owner">The owner.</param>
            public Spacewire2(EgseBukNotify owner)
                : base(owner)
            {
                CounterIcd = new Dictionary<short, AutoCounter>();
            }

            /// <summary>
            /// Список возможных каналов имитатора БМ-4.
            /// </summary>
            public enum Channel
            {
                /// <summary>
                /// Канал "БУК ПК1 - БМ-4 ПК1".
                /// </summary>
                [Description("БМ-4 ПК1 <-> БУК ПК1")]
                BUK1BM1 = 0x00,

                /// <summary>
                /// Канал "БУК ПК1 - БМ-4 ПК2".
                /// </summary>
                [Description("БМ-4 ПК2 <-> БУК ПК1")]
                BUK1BM2 = 0x02,

                /// <summary>
                /// Канал "БУК ПК2 - БМ-4 ПК1".
                /// </summary>
                [Description("БМ-4 ПК1 <-> БУК ПК2")]
                BUK2BM1 = 0x01,

                /// <summary>
                /// Канал "БУК ПК2 - БМ-4 ПК2".
                /// </summary>
                [Description("БМ-4 ПК2 <-> БУК ПК2")]
                BUK2BM2 = 0x03
            }

            /// <summary>
            /// Описывает адресные байты Spacewire.
            /// </summary>
            public enum Addr : uint
            {
                /// <summary>
                /// Адресный байт "ВЫХОДНЫЕ ДАННЫЕ (ИМИТАТОР БУСК): Данные Spacewire".
                /// </summary>
                Data = 0x04,

                /// <summary>
                /// Адресный байт "ВЫХОДНЫЕ ДАННЫЕ (ИМИТАТОР БУСК): EOP или EEP".
                /// </summary>
                End = 0x05,

                /// <summary>
                /// Адресный байт "ВЫХОДНЫЕ ДАННЫЕ (ИМИТАТОР БУСК): TIME TICK".
                /// </summary>
                Time1 = 0x06,

                /// <summary>
                /// Адресный байт "ВЫХОДНЫЕ ДАННЫЕ (ИМИТАТОР БУСК): TIME TICK".
                /// </summary>
                Time2 = 0x07,

                /// <summary>
                /// Адресный байт "ВХОДНЫЕ ДАННЫЕ (РЕАЛЬНЫЙ БУК): Данные Spacewire".
                /// </summary>
                BukData = 0x08,

                /// <summary>
                /// Адресный байт "ВХОДНЫЕ ДАННЫЕ (РЕАЛЬНЫЙ БУК): EOP или EEP".
                /// </summary>
                BukEnd = 0x09,

                /// <summary>
                /// Адресный байт "ВХОДНЫЕ ДАННЫЕ (РЕАЛЬНЫЙ БУК): TIME TICK".
                /// </summary>
                BukTime1 = 0x0a,

                /// <summary>
                /// Адресный байт "ВХОДНЫЕ ДАННЫЕ (РЕАЛЬНЫЙ БУК): TIME TICK".
                /// </summary>
                BukTime2 = 0x0b
            }       

            /// <summary>
            /// Получает буфер данных для передачи в USB.
            /// </summary>
            /// <value>
            /// Буфер данных.
            /// </value>
            public byte[] Data { get; private set; }

            /// <summary>
            /// Получает или задает количество запросов кредита от БУСК.
            /// </summary>
            /// <value>
            /// Количество запросов кредита от БУСК.
            /// </value>
            public long RequestQueueFromBusk
            {
                get
                {
                    return _requestQueueFromBusk;
                }

                set 
                {
                    _requestQueueFromBusk = value;
                    FirePropertyChangedEvent();
                }
            }

            /// <summary>
            /// Получает или задает значение [TICK TIME 1 для прибора БУСК].
            /// </summary>
            /// <value>
            /// [TICK TIME 1 для прибора БУСК].
            /// </value>
            public byte BuskTickTime1 
            {
                get
                {
                    return _buskTickTime1;
                }

                set 
                {
                    _buskTickTime1 = value;
                    FirePropertyChangedEvent();
                }
            }

            /// <summary>
            /// Получает наименование файла для записи данных.
            /// </summary>
            /// <value>
            /// Наименование файла для записи данных.
            /// </value>
            public string RawDataFile
            {
                get
                {
                    return _rawDataFile;
                }

                private set 
                {
                    _rawDataFile = value;
                    if (string.Empty != value)
                    {
                        _rawDataStream = new FileStream(value, FileMode.Create, FileAccess.Write, FileShare.None, bufferSize: 4096, useAsync: true);
                    }
                    else
                    {
                        if (null != _rawDataStream)
                        {
                            try
                            {
                                if (null != _rawDataTask)
                                {
                                    _rawDataTask.Wait(WaitForWriteTime);
                                }
                            }
                            finally
                            {
                                _rawDataStream.Close();
                            }
                        }
                    }

                    FirePropertyChangedEvent();
                }
            }

            /// <summary>
            /// Получает значение, показывающее, что включена запись данных в файл.
            /// </summary>
            /// <value>
            /// <c>true</c> исли запись данных включена; иначе, <c>false</c>.
            /// </value>
            public bool IsSaveRawData
            {
                get
                {
                    return _isSaveRawData;
                }

                private set 
                {
                    _isSaveRawData = value;
                    if (value)
                    {
                        RawDataFile = Owner.GetNewFileName(Resource.Get(@"stSpacewireLogName"));
                    }
                    else
                    {
                        RawDataFile = string.Empty;
                    }

                    FirePropertyChangedEvent();
                }
            }

            /// <summary>
            /// Получает команду на [включение записи данных в файл].
            /// </summary>
            /// <value>
            /// Команда на [включение записи данных в файл].
            /// </value>
            public ICommand SaveRawDataCommand
            {
                get
                {
                    if (null == _saveRawDataCommand)
                    {
                        _saveRawDataCommand = new RelayCommand(obj => { IsSaveRawData = !IsSaveRawData; }, obj => { return true; });
                    }

                    return _saveRawDataCommand;
                }
            }

            /// <summary>
            /// Получает или задает значение [TICK TIME 2 для прибора БУСК].
            /// </summary>
            /// <value>
            /// [TICK TIME 2 для прибора БУСК].
            /// </value>
            public byte BuskTickTime2
            {
                get
                {
                    return _buskTickTime2;
                }

                set 
                {
                    _buskTickTime2 = value;
                    FirePropertyChangedEvent();
                }
            }

            /// <summary>
            /// Получает или задает значение [TICK TIME 1 для прибора БУК].
            /// </summary>
            /// <value>
            /// [TICK TIME 1 для прибора БУК].
            /// </value>
            public byte BukTickTime1
            {
                get
                {
                    return _bukTickTime1;
                }

                set 
                {
                    _bukTickTime1 = value;
                    FirePropertyChangedEvent();
                }
            }

            /// <summary>
            /// Получает или задает значение [TICK TIME 2 для прибора БУК].
            /// </summary>
            /// <value>
            /// [TICK TIME 2 для прибора БУК].
            /// </value>
            public byte BukTickTime2
            {
                get
                {
                    return _bukTickTime2;
                }

                set 
                {
                    _bukTickTime2 = value;
                    FirePropertyChangedEvent();
                }
            }

            /// <summary>
            /// Получает значение [Счетчик телекоманд].
            /// </summary>
            /// <value>
            /// [Счетчик телекоманд].
            /// </value>
            public Dictionary<short, AutoCounter> CounterIcd { get; private set; }

            /// <summary>
            /// Получает значение, показывающее, что [выбран первый полукомплект БУК].
            /// </summary>
            /// <value>
            ///   <c>true</c> если [выбран первый полукомплект БУК]; иначе, <c>false</c>.
            /// </value>
            public bool IsChannelBuk1
            {
                get
                {
                    return (Channel.BUK1BM1 == IssueSpacewireChannel) || (Channel.BUK1BM2 == IssueSpacewireChannel);
                }
            }

            /// <summary>
            /// Получает значение, показывающее, что [выбран первый полукомплект БУСК].
            /// </summary>
            /// <value>
            ///   <c>true</c> если [выбран первый полукомплект БУСК]; иначе, <c>false</c>.
            /// </value>
            public bool IsChannelBusk1
            {
                get
                {
                    return (Channel.BUK1BM1 == IssueSpacewireChannel) || (Channel.BUK2BM1 == IssueSpacewireChannel);
                }
            }

            /// <summary>
            /// Получает или задает количество предоставлений кредита от БУСК.
            /// </summary>
            /// <value>
            /// Количество предоставлений кредита от БУСК.
            /// </value>
            public long ReplyQueueFromBusk
            {
                get
                {
                    return _replyQueueFromBusk;
                }

                set 
                {
                    _replyQueueFromBusk = value;
                    FirePropertyChangedEvent();
                }
            }

            /// <summary>
            /// Получает или задает количество запросов кредита от БУК.
            /// </summary>
            /// <value>
            /// Количество запросов кредита от БУК.
            /// </value>
            public long RequestQueueFromBuk
            {
                get
                {
                    return _requestQueueFromBuk;
                }

                set 
                {
                    _requestQueueFromBuk = value;
                    FirePropertyChangedEvent();
                }
            }

            /// <summary>
            /// Получает или задает количество предоставлений кредита от БУК.
            /// </summary>
            /// <value>
            /// Количество предоставлений кредита от БУК.
            /// </value>
            public long ReplyQueueFromBuk
            {
                get
                {
                    return _replyQueueFromBuk;
                }

                set 
                {
                    _replyQueueFromBuk = value;
                    FirePropertyChangedEvent();
                }
            }

            /// <summary>
            /// Получает или задает текущий КБВ по интерфейсу spacewire.
            /// </summary>
            /// <value>
            /// Текущий КБВ по интерфейсу spacewire.
            /// </value>
            public long CodeOnboardTime
            {
                get
                {
                    return _codeOnboardTime;
                }

                set 
                {
                    _codeOnboardTime = value;
                    FirePropertyChangedEvent();
                }
            }        

            /// <summary>
            /// Получает канал имитатора БМ-4.
            /// </summary>
            /// <value>
            /// Канал имитатора БМ-4.
            /// </value>
            public Channel IssueSpacewireChannel
            {
                get
                {
                    return _spacewireChannel;
                }

                private set 
                {
                    if (value == _spacewireChannel)
                    {
                        return;
                    }

                    _spacewireChannel = value;
                    ControlValuesList[Global.Spacewire2.Control].SetProperty(Global.Spacewire2.Control.Channel, (int)value);
                    FirePropertyChangedEvent();
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
                    return string.Format(Resource.Get(@"stShowLogicBusk"), LogicBusk.ToString(/*"X2"*/));
                }

                private set 
                {
                    FirePropertyChangedEvent();
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
                    ControlValuesList[Global.Spacewire2.BuskLogic].SetProperty(Global.Spacewire2.BuskLogic, value);
                    FirePropertyChangedEvent();
                    ShowLogicBusk = string.Empty;
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
                    return string.Format(Resource.Get(@"stShowLogicBuk"), LogicBuk.ToString(/*"X2"*/));
                }

                private set 
                {
                    FirePropertyChangedEvent();
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
                    ControlValuesList[Global.Spacewire2.BukLogic].SetProperty(Global.Spacewire2.BukLogic, value);
                    FirePropertyChangedEvent();
                    ShowLogicBuk = string.Empty;
                }
            }

            /// <summary>
            /// Получает значение, показывающее, что [интерфейс Spacewire включен].
            /// </summary>
            /// <value>
            /// <c>true</c> если [интерфейс Spacewire включен]; иначе, <c>false</c>.
            /// </value>
            public bool IsIssueEnable
            {
                get
                {
                    return _isIssueEnable;
                }

                private set 
                {
                    _isIssueEnable = value;
                    ControlValuesList[Global.Spacewire2.Control].SetProperty(Global.Spacewire2.Control.IssueEnable, Convert.ToInt32(value));
                    FirePropertyChangedEvent();
                }
            }

            /// <summary>
            /// Получает команду на [включение интерфейса spacewire].
            /// </summary>
            /// <value>
            /// Команда на [включение интерфейса spacewire].
            /// </value>
            public ICommand IssueEnableCommand
            {
                get
                {
                    if (null == _issueEnableCommand)
                    {
                        _issueEnableCommand = new RelayCommand(obj => { IsIssueEnable = !IsIssueEnable; }, obj => { return true; });
                    }

                    return _issueEnableCommand;
                }
            }

            /// <summary>
            /// Получает значение, показывающее, что [связь по интерфейсу Spacewire установлена].
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

                private set 
                {
                    _isConnect = value;
                    ControlValuesList[Global.Spacewire2.Control].SetProperty(Global.Spacewire2.Control.Connect, Convert.ToInt32(value));
                    FirePropertyChangedEvent();
                }
            }

            /// <summary>
            /// Получает значение, показывающее, что [Бит отправки RMAP посылки - 1].
            /// </summary>
            /// <value>
            ///   <c>true</c> если [Бит отправки RMAP посылки - 1]; иначе, <c>false</c>.
            /// </value>
            public bool IsIssueRMap
            {
                get
                {
                    return _isIssueRMap;
                }

                private set 
                {
                    _isIssueRMap = value;                    
                    FirePropertyChangedEvent();
                }
            }

            /// <summary>
            /// Получает команду на [выдачу посылки RMAP по интерфейсу spacewire].
            /// </summary>
            /// <value>
            /// Команда на [выдачу посылки RMAP по интерфейсу spacewire].
            /// </value>
            public ICommand IssueRMapCommand
            {
                get
                {
                    if (_issueRMapCommand == null)
                    {
                        _issueRMapCommand = new RelayCommand(obj => { IsIssueRMap = true; ControlValuesList[Global.Spacewire2.Record].SetProperty(Global.Spacewire2.Record.IssueRMap, 1); }, obj => { return !IsRecordBusy; });
                    }

                    return _issueRMapCommand;
                }
            }

            /// <summary>
            /// Получает команду на [получение данных из файла].
            /// </summary>
            /// <value>
            /// Команда на [получение данных из файла].
            /// </value>
            public ICommand FromFileCommand
            {
                get
                {
                    if (_fromFileCommand == null)
                    {
                        _fromFileCommand = new RelayCommand(OpenFromFile, obj => { return true; });
                    }

                    return _fromFileCommand;
                }
            }

            /// <summary>
            /// Получает значение, показывающее, что [запись в прибор занята].
            /// </summary>
            /// <value>
            ///   <c>true</c> если [запись в прибор занята]; иначе, <c>false</c>.
            /// </value>
            public bool IsRecordBusy
            {
                get
                {
                    return IsIssueRMap || IsIssuePackage;
                }
            }

            /// <summary>
            /// Получает значение, показывающее, что [необходимо формировать посылку телекоманды].
            /// </summary>
            /// <value>
            ///   <c>true</c> если [необходимо формировать посылку телекоманды]; иначе, <c>false</c>.
            /// </value>
            public bool IsMakeTK
            {
                get
                {
                    return _isMakeTK;
                }

                private set 
                {
                    _isMakeTK = value;
                    FirePropertyChangedEvent();
                }
            }

            /// <summary>
            /// Получает команду на [формирование посылки телекоманды по интерфейсу spacewire].
            /// </summary>
            /// <value>
            /// Команда на [формирование посылки телекоманды по интерфейсу spacewire].
            /// </value>
            public ICommand IssueMakeTKCommand
            {
                get
                {
                    if (_issueMakeTKCommand == null)
                    {
                        _issueMakeTKCommand = new RelayCommand(obj => { IsMakeTK = !IsMakeTK; }, obj => { return true; });
                    }

                    return _issueMakeTKCommand;
                }
            }
            
            /// <summary>
            /// Получает значение, показывающее, что [Бит отправки посылки в БУК - 1].
            /// </summary>
            /// <value>
            ///   <c>true</c> если [Бит отправки посылки в БУК - 1]; иначе, <c>false</c>.
            /// </value>
            public bool IsIssuePackage
            {
                get
                {
                    return _isIssuePackage;
                }

                private set 
                {
                    _isIssuePackage = value;                    
                    FirePropertyChangedEvent();
                }
            }

            /// <summary>
            /// Получает команду на [выдачу посылки по интерфейсу spacewire].
            /// </summary>
            /// <value>
            /// Команда на [выдачу посылки по интерфейсу spacewire].
            /// </value>
            public ICommand IssuePackageCommand
            {
                get
                {
                    if (_issuePackageCommand == null)
                    {
                        _issuePackageCommand = new RelayCommand(obj => { IsIssuePackage = true; ControlValuesList[Global.Spacewire2.Record].SetProperty(Global.Spacewire2.Record.IssuePackage, 1); }, obj => { return !IsRecordBusy; });
                    }

                    return _issuePackageCommand;
                }
            }

            /// <summary>
            /// Получает значение, показывающее, что [выдаются метки времени приборам].
            /// </summary>
            /// <value>
            /// <c>true</c> если [выдаются метки времени приборам]; иначе, <c>false</c>.
            /// </value>
            public bool IsIssueTimeMark
            {
                get
                {
                    return _isIssueTimeMark;
                }

                private set 
                {
                    _isIssueTimeMark = value;
                    ControlValuesList[Global.Spacewire2.SPTPControl].SetProperty(Global.Spacewire2.SPTPControl.IssueTimeMark, Convert.ToInt32(value));
                    FirePropertyChangedEvent();
                }
            }

            /// <summary>
            /// Получает команду на [включение передачи метки времени по интерфейсу spacewire].
            /// </summary>
            /// <value>
            /// Команда на [включение передачи метки времени по интерфейсу spacewire].
            /// </value>
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
            /// Получает значение, показывающее, что [включен обмен для прибора БУК].
            /// </summary>
            /// <value>
            /// <c>true</c> если [включен обмен для прибора БУК]; иначе, <c>false</c>.
            /// </value>
            public bool IsIssueTrans
            {
                get
                {
                    return _isIssueTrans;
                }

                private set 
                {
                    _isIssueTrans = value;
                    ControlValuesList[Global.Spacewire2.SPTPControl].SetProperty(Global.Spacewire2.SPTPControl.IssueTrans, Convert.ToInt32(value));
                    FirePropertyChangedEvent();
                }
            }

            /// <summary>
            /// Получает команду на [включение обмена для прибора БУК].
            /// </summary>
            /// <value>
            /// Команда на [включение обмена для прибора БУК].
            /// </value>
            public ICommand IssueTransCommand
            {
                get
                {
                    if (_issueTransCommand == null)
                    {
                        _issueTransCommand = new RelayCommand(obj => { IsIssueTrans = !IsIssueTrans; }, obj => { return true; });
                    }

                    return _issueTransCommand;
                }
            }

            /// <summary>
            /// Получает значение, показывающее, что [выдается КБВ для прибора БУК].
            /// </summary>
            /// <value>
            /// <c>true</c> если [выдается КБВ для прибора БУК]; иначе, <c>false</c>.
            /// </value>
            public bool IsIssueKbv
            {
                get
                {
                    return _isIssueKbv;
                }

                private set 
                {
                    _isIssueKbv = value;
                    ControlValuesList[Global.Spacewire2.SPTPControl].SetProperty(Global.Spacewire2.SPTPControl.IssueKbv, Convert.ToInt32(value));
                    FirePropertyChangedEvent();
                }
            }

            /// <summary>
            /// Получает команду на [включение выдачи КБВ для прибора БУК].
            /// </summary>
            /// <value>
            /// Команда на [включение выдачи КБВ для прибора БУК].
            /// </value>
            public ICommand IssueKbvCommand
            {
                get
                {
                    if (_issueKbvCommand == null)
                    {
                        _issueKbvCommand = new RelayCommand(obj => { IsIssueKbv = !IsIssueKbv; }, obj => { return true; });
                    }

                    return _issueKbvCommand;
                }
            }
            
            /// <summary>
            /// Получает значение, показывающее, что [можно выдавать пакеты данных в БУК].
            /// </summary>
            /// <value>
            /// <c>true</c> если [можно выдавать пакеты данных в БУК]; иначе, <c>false</c>.
            /// </value>
            public bool IsTransData
            {
                get
                {
                    return _isTransData;
                }

                private set 
                {
                    _isTransData = value;
                    ControlValuesList[Global.Spacewire2.SPTPControl].SetProperty(Global.Spacewire2.SPTPControl.TransData, Convert.ToInt32(value));
                    FirePropertyChangedEvent();
                }
            }

            /// <summary>
            /// Получает текущий APID для формирования посылки.
            /// </summary>
            /// <value>
            /// Текущий APID для формирования посылки.
            /// </value>
            public short CurApid
            {
                get
                {
                    return _setApid;
                }

                private set 
                {
                    _setApid = value;
                    FirePropertyChangedEvent();
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
            /// Вызывается когда [требуется записать данные сообщения в файл].
            /// </summary>
            /// <param name="sender">The sender.</param>
            /// <param name="e">The <see cref="SpacewireSptpMsgEventArgs"/> instance containing the event data.</param>
            public virtual void OnSpacewire2MsgRawSave(object sender, BaseMsgEventArgs e)
            {
                if (null != _rawDataStream)
                {
                    if (null != _rawDataTask)
                    {
                        _rawDataTask.Wait(WaitForWriteTime);
                    }

                    if (_rawDataStream.CanWrite)
                    {
                        if (e is SpacewireSptpMsgEventArgs)
                        {
                            SpacewireSptpMsgEventArgs sptp = e as SpacewireSptpMsgEventArgs;
                            _rawDataTask = _rawDataStream.WriteAsync(sptp.Data, 0, sptp.Data.Length);
                        }
                    }
                }
            }

            /// <summary>
            /// Вызов диалога "Открыть файл".
            /// </summary>
            /// <param name="obj">The object.</param>
            public void OpenFromFile(object obj)
            {
                Data = Owner.OpenFromFile();
                FirePropertyChangedEvent("Data");
            }

            /// <summary>
            /// Initializes the control value.
            /// </summary>
            protected override void InitControlValue()
            {
                ControlValuesList.Add(Global.Spacewire2.Control, new ControlValue());
                ControlValuesList.Add(Global.Spacewire2.Record, new ControlValue());
                ControlValuesList.Add(Global.Spacewire2.SPTPControl, new ControlValue());
                ControlValuesList.Add(Global.Spacewire2.BuskLogic, new ControlValue());
                ControlValuesList.Add(Global.Spacewire2.BukLogic, new ControlValue());
                ControlValuesList.Add(Global.Spacewire2.SPTPLogicBkp, new ControlValue());
            }

            /// <summary>
            /// Initializes the properties.
            /// </summary>
            protected override void InitProperties()
            {
                ControlValuesList[Global.Spacewire2.Control].AddProperty(Global.Spacewire2.Control.Channel, 1, 2, Device.CmdSpacewire2Control, value => IssueSpacewireChannel = (Channel)value);
                ControlValuesList[Global.Spacewire2.Control].AddProperty(Global.Spacewire2.Control.IssueEnable, 0, 1, Device.CmdSpacewire2Control, value => IsIssueEnable = 1 == value);
                ControlValuesList[Global.Spacewire2.Control].AddProperty(Global.Spacewire2.Control.Connect, 3, 1, delegate { }, value => IsConnect = 1 == value, true);
                ControlValuesList[Global.Spacewire2.Record].AddProperty(Global.Spacewire2.Record.IssueRMap, 0, 1, Device.CmdSpacewire2Record, value => IsIssueRMap = 1 == value);
                ControlValuesList[Global.Spacewire2.Record].AddProperty(Global.Spacewire2.Record.IssuePackage, 1, 1, Device.CmdSpacewire2Record, value => IsIssuePackage = 1 == value);
                ControlValuesList[Global.Spacewire2.BuskLogic].AddProperty(Global.Spacewire2.BuskLogic, 0, 8, Device.CmdSpacewire2LogicBusk, value => LogicBusk = value);
                ControlValuesList[Global.Spacewire2.BukLogic].AddProperty(Global.Spacewire2.BukLogic, 0, 8, Device.CmdSpacewire2LogicBuk, value => LogicBuk = value);
                ControlValuesList[Global.Spacewire2.SPTPControl].AddProperty(Global.Spacewire2.SPTPControl.IssueTimeMark, 0, 1, Device.CmdSpacewire2SPTPControl, value => IsIssueTimeMark = 1 == value);
                ControlValuesList[Global.Spacewire2.SPTPControl].AddProperty(Global.Spacewire2.SPTPControl.IssueTrans, 1, 1, Device.CmdSpacewire2SPTPControl, value => IsIssueTrans = 1 == value);
                ControlValuesList[Global.Spacewire2.SPTPControl].AddProperty(Global.Spacewire2.SPTPControl.IssueKbv, 2, 1, Device.CmdSpacewire2SPTPControl, value => IsIssueKbv = 1 == value);               
                ControlValuesList[Global.Spacewire2.SPTPControl].AddProperty(Global.Spacewire2.SPTPControl.TransData, 3, 1, Device.CmdSpacewire2SPTPControl, value => IsTransData = 1 == value);
            }
        }

        /// <summary>
        /// Нотификатор spacewire3.
        /// </summary>
        public class Spacewire3 : SubNotify, IDataErrorInfo
        {
            /// <summary>
            /// Timeout на запись данных в файл.
            /// </summary>
            public const int WaitForWriteTime = 1000;

            /// <summary>
            /// Рабочий прибор.
            /// </summary>
            private WorkDevice _workDevice;

            /// <summary>
            /// Управление: вкл/выкл интерфейса Spacewire.
            /// </summary>
            private bool _isIssueEnable;

            /// <summary>
            /// Управление: Установлена связь.
            /// </summary>
            private bool _isConnect;

            /// <summary>
            /// Управление: Сигнал передачи кадров.
            /// </summary>
            private bool _isIssueTransmission;

            /// <summary>
            /// Сохранять данные в файл.
            /// </summary>
            private bool _isSaveRawData;

            /// <summary>
            /// Имя файла данных.
            /// </summary>
            private string _rawDataFile;

            /// <summary>
            /// Для асинхронной записи в файл.
            /// </summary>
            private FileStream _rawDataStream;

            /// <summary>
            /// Квазиасинхронная запись в файл.
            /// Примечание:
            /// Используется для сигнала, что все данные записались в файл.
            /// </summary>
            private Task _rawDataTask;

            /// <summary>
            /// Полукомплект рабочего прибора.
            /// </summary>
            private HalfSet _workDeviceHalfSet;

            /// <summary>
            /// Экземпляр команды [включение интерфейса spacewire].
            /// </summary>
            private ICommand _issueEnableCommand;

            /// <summary>
            /// Экземпляр команды на [включение записи в файл].
            /// </summary>
            private ICommand _saveRawDataCommand;

            /// <summary>
            /// Текущее значение Timetick1 от БУК.
            /// </summary>
            private byte _bukTickTime1;

            /// <summary>
            /// Текущее значение Timetick2 от БУК.
            /// </summary>
            private byte _bukTickTime2;

            /// <summary>
            /// Текущее значение Timetick2 от НП.
            /// </summary>
            private byte _scidevTickTime2;

            /// <summary>
            /// Текущее значение Timetick1 от НП.
            /// </summary>
            private byte _scidevTickTime1;

            /// <summary>
            /// Количество запросов квоты от БУК.
            /// </summary>
            private long _requestQueueFromBuk;

            /// <summary>
            /// Количество предоставления квот от БУК.
            /// </summary>
            private long _replyQueueFromBuk;

            /// <summary>
            /// Количество запросов квоты от НП.
            /// </summary>
            private long _replyQueueFromSD;

            /// <summary>
            /// Количество предоставления квот от НП.
            /// </summary>
            private long _requestQueueFromSD;

            /// <summary>
            /// Инициализирует новый экземпляр класса <see cref="Spacewire3" />.
            /// </summary>
            /// <param name="owner">The owner.</param>
            public Spacewire3(EgseBukNotify owner)
                : base(owner)
            {
            }

            /// <summary>
            /// Полукомплекты рабочего прибора.
            /// </summary>
            public enum HalfSet
            {
                /// <summary>
                /// Первый полукомплект.
                /// </summary>
                [Description("Первый полукомплект")]
                First = 0x00,

                /// <summary>
                /// Второй полукомплект.
                /// </summary>
                [Description("Второй полукомплект")]
                Second = 0x01
            }

            /// <summary>
            /// Возможные рабочие приборы.
            /// </summary>
            public enum WorkDevice
            {
                /// <summary>
                /// Рабочий прибор "УФЕС".
                /// </summary>
                [Description("УФЕС")]
                Ufes = 0x00,

                /// <summary>
                /// Рабочий прибор "ВУФЕС".
                /// </summary>
                [Description("ВУФЕС")]
                Vufes = 0x01,

                /// <summary>
                /// Рабочий прибор "СДЩ".
                /// </summary>
                [Description("СДЩ")]
                Sdchsh = 0x02
            }

            /// <summary>
            /// Описывает адресные байты Spacewire.
            /// </summary>
            public enum Addr : uint
            {
                /// <summary>
                /// Адресный байт "ВЫХОДНЫЕ ДАННЫЕ: Данные Spacewire".
                /// </summary>
                OutData = 0x0c,

                /// <summary>
                /// Адресный байт "ВЫХОДНЫЕ ДАННЫЕ: EOP или EEP".
                /// </summary>
                OutEnd = 0x0d,

                /// <summary>
                /// Адресный байт "ВЫХОДНЫЕ ДАННЫЕ: TIME TICK".
                /// </summary>
                OutTime1 = 0x0e,

                /// <summary>
                /// Адресный байт "ВЫХОДНЫЕ ДАННЫЕ: TIME TICK".
                /// </summary>
                OutTime2 = 0x0f,

                /// <summary>
                /// Адресный байт "ВХОДНЫЕ ДАННЫЕ: Данные Spacewire".
                /// </summary>
                InData = 0x10,

                /// <summary>
                /// Адресный байт "ВХОДНЫЕ ДАННЫЕ: EOP или EEP".
                /// </summary>
                InEnd = 0x11,

                /// <summary>
                /// Адресный байт "ВХОДНЫЕ ДАННЫЕ: TIME TICK".
                /// </summary>
                InTime1 = 0x12,

                /// <summary>
                /// Адресный байт "ВХОДНЫЕ ДАННЫЕ: TIME TICK".
                /// </summary>
                InTime2 = 0x13
            }

            /// <summary>
            /// Получает значение, показывающее, что [интерфейс Spacewire включен].
            /// </summary>
            /// <value>
            /// <c>true</c> если [интерфейс SpaceWire включен]; иначе, <c>false</c>.
            /// </value>
            public bool IsIssueEnable
            {
                get
                {
                    return _isIssueEnable;
                }

                private set 
                {
                    _isIssueEnable = value;
                    ControlValuesList[Global.Spacewire3.Control].SetProperty(Global.Spacewire3.Control.IssueEnable, Convert.ToInt32(value));
                    ControlValuesList[Global.Spacewire4.Control].SetProperty(Global.Spacewire4.Control.Connect, Convert.ToInt32(0));
                    FirePropertyChangedEvent();
                }
            }

            /// <summary>
            /// Получает команду на [включение интерфейса spacewire].
            /// </summary>
            /// <value>
            /// Команда на [включение интерфейса spacewire].
            /// </value>
            public ICommand IssueEnableCommand
            {
                get
                {
                    if (null == _issueEnableCommand)
                    {
                        _issueEnableCommand = new RelayCommand(obj => { IsIssueEnable = !IsIssueEnable; }, obj => { return true; });
                    }

                    return _issueEnableCommand;
                }
            }

            /// <summary>
            /// Получает значение, показывающее, что [связь по интерфейсу Spacewire установлена].
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

                private set 
                {
                    _isConnect = value;
                    ControlValuesList[Global.Spacewire3.Control].SetProperty(Global.Spacewire3.Control.Connect, Convert.ToInt32(value));
                    FirePropertyChangedEvent();
                }
            }

            /// <summary>
            /// Получает значение, показывающее, что [передаются кадры данных].
            /// </summary>
            /// <value>
            ///   <c>true</c> если [передаются кадры данных]; иначе, <c>false</c>.
            /// </value>
            public bool IsIssueTransmission
            {
                get
                {
                    return _isIssueTransmission;
                }

                private set 
                {
                    _isIssueTransmission = value;
                    FirePropertyChangedEvent();
                }
            }

            /// <summary>
            /// Получает полукомплект прибора.
            /// </summary>
            /// <value>
            /// Полукомплект прибора.
            /// </value>
            public HalfSet IssueHalfSet
            {
                get
                {
                    return _workDeviceHalfSet;
                }

                private set 
                {
                    _workDeviceHalfSet = value;
                    ControlValuesList[Global.Spacewire3.Control].SetProperty(Global.Spacewire3.Control.HalfSet, (int)value);
                    FirePropertyChangedEvent();
                }
            }

            /// <summary>
            /// Получает рабочий прибор.
            /// </summary>
            /// <value>
            /// Рабочий прибор.
            /// </value>
            public WorkDevice IssueWorkDevice
            {
                get
                {
                    return _workDevice;
                }

                private set 
                {
                    _workDevice = value;
                    ControlValuesList[Global.Spacewire3.Control].SetProperty(Global.Spacewire3.Control.WorkDevice, (int)value);
                    FirePropertyChangedEvent();
                }
            }

            /// <summary>
            /// Получает наименование файла для записи данных.
            /// </summary>
            /// <value>
            /// Наименование файла для записи данных.
            /// </value>
            public string RawDataFile
            {
                get
                {
                    return _rawDataFile;
                }

                private set 
                {
                    _rawDataFile = value;
                    if (string.Empty != value)
                    {
                        _rawDataStream = new FileStream(value, FileMode.Create, FileAccess.Write, FileShare.None, bufferSize: 4096, useAsync: true);
                    }
                    else
                    {
                        if (null != _rawDataStream)
                        {
                            try
                            {
                                if (null != _rawDataTask)
                                {
                                    _rawDataTask.Wait(WaitForWriteTime);
                                }
                            }
                            finally
                            {
                                _rawDataStream.Close();
                            }
                        }
                    }

                    FirePropertyChangedEvent();
                }
            }

            /// <summary>
            /// Получает значение, показывающее, что активна запись данных в файл.
            /// </summary>
            /// <value>
            /// <c>true</c> если активна запись данных в файл; иначе, <c>false</c>.
            /// </value>
            public bool IsSaveRawData
            {
                get
                {
                    return _isSaveRawData;
                }

                private set 
                {
                    _isSaveRawData = value;
                    if (value)
                    {
                        RawDataFile = Owner.GetNewFileName(Resource.Get(@"stSdLogName"));
                    }
                    else
                    {
                        RawDataFile = string.Empty;
                    }

                    FirePropertyChangedEvent();
                }
            }

            /// <summary>
            /// Получает команду на [включение записи данных в файл].
            /// </summary>
            /// <value>
            /// Команда на [включение записи данных в файл].
            /// </value>
            public ICommand SaveRawDataCommand
            {
                get
                {
                    if (null == _saveRawDataCommand)
                    {
                        _saveRawDataCommand = new RelayCommand(obj => { IsSaveRawData = !IsSaveRawData; }, obj => { return true; });
                    }

                    return _saveRawDataCommand;
                }
            }

            /// <summary>
            /// Получает количество запросов квот от БУК.
            /// </summary>
            /// <value>
            /// Количество запросов квот от БУК.
            /// </value>
            public long RequestQueueFromBuk
            {
                get
                {
                    return _requestQueueFromBuk;
                }

                private set 
                {
                    _requestQueueFromBuk = value;
                    FirePropertyChangedEvent();
                }
            }

            /// <summary>
            /// Получает количество предоставления квот от БУК.
            /// </summary>
            /// <value>
            /// Количество предоставления квот от БУК.
            /// </value>
            public long ReplyQueueFromBuk
            {
                get
                {
                    return _replyQueueFromBuk;
                }

                private set 
                {
                    _replyQueueFromBuk = value;
                    FirePropertyChangedEvent();
                }
            }

            /// <summary>
            /// Получает количество запросов квот от НП.
            /// </summary>
            /// <value>
            /// Количество запросов квот от НП.
            /// </value>
            public long RequestQueueFromSD
            {
                get
                {
                    return _requestQueueFromSD;
                }

                private set 
                {
                    _requestQueueFromSD = value;
                    FirePropertyChangedEvent();
                }
            }

            /// <summary>
            /// Получает количество предоставления квот от НП.
            /// </summary>
            /// <value>
            /// Количество предоставления квот от НП.
            /// </value>
            public long ReplyQueueFromSD
            {
                get
                {
                    return _replyQueueFromSD;
                }

                private set 
                {
                    _replyQueueFromSD = value;
                    FirePropertyChangedEvent();
                }
            }

            /// <summary>
            /// Получает или задает текущий TimeTick1 от НП.
            /// </summary>
            /// <value>
            /// Текущий TimeTick1 от НП.
            /// </value>
            public byte SDTickTime1
            {
                get
                {
                    return _scidevTickTime1;
                }

                set 
                {
                    _scidevTickTime1 = value;
                    FirePropertyChangedEvent();
                }
            }

            /// <summary>
            /// Получает или задает текущий TimeTick2 от НП.
            /// </summary>
            /// <value>
            /// Текущий TimeTick2 от НП.
            /// </value>
            public byte SDTickTime2
            {
                get
                {
                    return _scidevTickTime2;
                }

                set 
                {
                    _scidevTickTime2 = value;
                    FirePropertyChangedEvent();
                }
            }

            /// <summary>
            /// Получает или задает текущий TimeTick1 от БУК.
            /// </summary>
            /// <value>
            /// Текущий TimeTick1 от БУК.
            /// </value>
            public byte BukTickTime1
            {
                get
                {
                    return _bukTickTime1;
                }

                set 
                {
                    _bukTickTime1 = value;
                    FirePropertyChangedEvent();
                }
            }

            /// <summary>
            /// Получает или задает текущий TimeTick2 от БУК.
            /// </summary>
            /// <value>
            /// Текущий TimeTick2 от БУК.
            /// </value>
            public byte BukTickTime2
            {
                get
                {
                    return _bukTickTime2;
                }

                set 
                {
                    _bukTickTime2 = value;
                    FirePropertyChangedEvent();
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
            /// Вызывается когда [требуется записать данные сообщения в файл].
            /// </summary>
            /// <param name="sender">The sender.</param>
            /// <param name="e">The <see cref="SpacewireSptpMsgEventArgs"/> instance containing the event data.</param>
            public virtual void OnSpacewire3MsgRawSave(object sender, BaseMsgEventArgs e)
            {
                if (null != _rawDataStream)
                {
                    if (null != _rawDataTask)
                    {
                        _rawDataTask.Wait(WaitForWriteTime);
                    }

                    if (_rawDataStream.CanWrite)
                    {
                        if (e is SpacewireSptpMsgEventArgs)
                        {
                            SpacewireSptpMsgEventArgs sptp = e as SpacewireSptpMsgEventArgs;
                            _rawDataTask = _rawDataStream.WriteAsync(sptp.Data, 0, sptp.Data.Length);
                        }
                    }
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
                ControlValuesList[Global.Spacewire3.Control].AddProperty(Global.Spacewire3.Control.IssueEnable, 0, 1, Device.CmdSpacewire3Control, value => IsIssueEnable = 1 == value);
                ControlValuesList[Global.Spacewire3.Control].AddProperty(Global.Spacewire3.Control.Connect, 3, 1, delegate { }, value => IsConnect = 1 == value, true);
                ControlValuesList[Global.Spacewire3.Control].AddProperty(Global.Spacewire3.Control.Transmission, 5, 1, delegate { }, value => IsIssueTransmission = 1 == value, true);
                ControlValuesList[Global.Spacewire3.Control].AddProperty(Global.Spacewire3.Control.HalfSet, 4, 1, Device.CmdSpacewire3Control, value => IssueHalfSet = (HalfSet)value);
                ControlValuesList[Global.Spacewire3.Control].AddProperty(Global.Spacewire3.Control.WorkDevice, 1, 2, Device.CmdSpacewire3Control, value => IssueWorkDevice = (WorkDevice)value);
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
            private bool _isIssueEnable;

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

            /// <summary>
            /// Экземпляр команды на [включение интерфейса spacewire].
            /// </summary>
            private ICommand _issueEnableCommand;

            /// <summary>
            /// Экземпляр команды на [включение передачи метки времени по интерфейсу spacewire].
            /// </summary>
            private ICommand _issueTimeMarkCommand;

            /// <summary>
            /// Экземпляр команды на [выдачу ошибки EEP по интерфейсу spacewire, при формировании посылки].
            /// </summary>
            private ICommand _issueEEPCommand;

            /// <summary>
            /// Экземпляр команды на [выдачу EOP по интерфейсу spacewire, при формировании посылки].
            /// </summary>
            private ICommand _issueEOPCommand;

            /// <summary>
            /// Экземпляр команды на [выдачу посылки по интерфейсу spacewire].
            /// </summary>
            private ICommand _issuePackageCommand;
            
            /// <summary>
            /// Экземпляр команды на [включение автоматической выдачи посылки по интерфейсу spacewire].
            /// </summary>
            private ICommand _issueAutoCommand;

            /// <summary>
            /// Экземпляр команды на [открыть из файла].
            /// </summary>
            private ICommand _fromFileCommand;

            /// <summary>
            /// Инициализирует новый экземпляр класса <see cref="Spacewire4" />.
            /// </summary>
            /// <param name="owner">The owner.</param>
            public Spacewire4(EgseBukNotify owner)
                : base(owner)
            {
            }

            /// <summary>
            /// Получает буфер данных для передачи в USB.
            /// </summary>
            /// <value>
            /// Буфер данных.
            /// </value>
            public byte[] Data { get; private set; }

            /// <summary>
            /// Получает значение, показывающее, что [интерфейс Spacewire включен].
            /// </summary>
            /// <value>
            /// <c>true</c> если [интерфейс SpaceWire включен]; иначе, <c>false</c>.
            /// </value>
            public bool IsIssueEnable
            {
                get
                {
                    return _isIssueEnable;
                }

                private set 
                {
                    _isIssueEnable = value;
                    ControlValuesList[Global.Spacewire4.Control].SetProperty(Global.Spacewire4.Control.IssueEnable, Convert.ToInt32(value));
                    ControlValuesList[Global.Spacewire3.Control].SetProperty(Global.Spacewire3.Control.Connect, Convert.ToInt32(0));
                    FirePropertyChangedEvent();
                }
            }

            /// <summary>
            /// Получает команду на включение интерфейса spacewire.
            /// </summary>
            /// <value>
            /// Команда на включение интерфейса spacewire.
            /// </value>
            public ICommand IssueEnableCommand
            {
                get
                {
                    if (null == _issueEnableCommand)
                    {
                        _issueEnableCommand = new RelayCommand(obj => { IsIssueEnable = !IsIssueEnable; }, obj => { return true; });
                    }

                    return _issueEnableCommand;
                }
            }

            /// <summary>
            /// Получает значение, показывающее, что [установлена связь по интерфейсу Spacewire].
            /// </summary>
            /// <value>
            /// <c>true</c> если [установлена связь по интерфейсу Spacewire]; иначе, <c>false</c>.
            /// </value>
            public bool IsConnect
            {
                get
                {
                    return _isConnect;
                }

                private set 
                {
                    _isConnect = value;
                    ControlValuesList[Global.Spacewire4.Control].SetProperty(Global.Spacewire4.Control.Connect, Convert.ToInt32(value));
                    FirePropertyChangedEvent();
                }
            }

            /// <summary>
            /// Получает значение, показывающее, что [выдается метка времени].
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

                private set 
                {
                    _isIssueTimeMark = value;
                    ControlValuesList[Global.Spacewire4.Control].SetProperty(Global.Spacewire4.Control.TimeMark, Convert.ToInt32(_isIssueTimeMark));
                    FirePropertyChangedEvent();
                }
            }

            /// <summary>
            /// Получает команду на [включение передачи метки времени по интерфейсу spacewire].
            /// </summary>
            /// <value>
            /// Команда на [включение передачи метки времени по интерфейсу spacewire].
            /// </value>
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
            /// Получает значение, показывающее, что [выдается EEP].
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

                private set 
                {
                    _isIssueEEP = value;
                    ControlValuesList[Global.Spacewire4.Record].SetProperty(Global.Spacewire4.Record.IssueEEP, Convert.ToInt32(value));
                    FirePropertyChangedEvent();
                }
            }

            /// <summary>
            /// Получает команду на [выдачу ошибки EEP по интерфейсу spacewire, при формировании посылки].
            /// </summary>
            /// <value>
            /// Команда на [выдачу ошибки EEP по интерфейсу spacewire, при формировании посылки].
            /// </value>
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
            /// Получает значение, показывающее, что [выдается EOP].
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

                private set 
                {
                    _isIssueEOP = value;
                    ControlValuesList[Global.Spacewire4.Record].SetProperty(Global.Spacewire4.Record.EOPSend, Convert.ToInt32(value));
                    FirePropertyChangedEvent();
                }
            }

            /// <summary>
            /// Получает команду на [выдачу EOP по интерфейсу spacewire, при формировании посылки].
            /// </summary>
            /// <value>
            /// Команда на [выдачу EOP по интерфейсу spacewire, при формировании посылки].
            /// </value>
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
            /// Получает команду на [получение данных из файла].
            /// </summary>
            /// <value>
            /// Команда на [получение данных из файла].
            /// </value>
            public ICommand FromFileCommand
            {
                get
                {
                    if (_fromFileCommand == null)
                    {
                        _fromFileCommand = new RelayCommand(OpenFromFile, obj => { return true; });
                    }

                    return _fromFileCommand;
                }
            }

            /// <summary>
            /// Получает значение, показывающее, что [включена автоматическая выдача посылки].
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

                private set 
                {
                    _isIssueAuto = value;
                    ControlValuesList[Global.Spacewire4.Record].SetProperty(Global.Spacewire4.Record.IssueAuto, Convert.ToInt32(value));
                    FirePropertyChangedEvent();
                }
            }

            /// <summary>
            /// Получает команду на [включение автоматической выдачи посылки по интерфейсу spacewire].
            /// </summary>
            /// <value>
            /// Команда на [включение автоматической выдачи посылки по интерфейсу spacewire].
            /// </value>
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
            /// Получает значение, показывающее, что [Бит занятости записи - 1].
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

                private set 
                {
                    _isRecordBusy = value;
                    FirePropertyChangedEvent();                    
                }
            }

            /// <summary>
            /// Получает значение, показывающее, что [Бит выдачи посылки - 1].
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

                private set 
                {
                    _isIssuePackage = value;
                    ControlValuesList[Global.Spacewire4.Record].SetProperty(Global.Spacewire4.Record.IssuePackage, Convert.ToInt32(value));
                    FirePropertyChangedEvent();                   
                }
            }

            /// <summary>
            /// Получает команду на [выдачу посылки по интерфейсу spacewire].
            /// </summary>
            /// <value>
            /// Команда на [выдачу посылки по интерфейсу spacewire].
            /// </value>
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
            /// Вызов диалога "Открыть файл".
            /// </summary>
            /// <param name="obj">The object.</param>
            public void OpenFromFile(object obj)
            {
                Data = Owner.OpenFromFile();
                FirePropertyChangedEvent("Data");
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
                ControlValuesList[Global.Spacewire4.Control].AddProperty(Global.Spacewire4.Control.IssueEnable, 0, 1, Device.CmdSpacewire4Control, value => IsIssueEnable = 1 == value);
                ControlValuesList[Global.Spacewire4.Control].AddProperty(Global.Spacewire4.Control.Connect, 3, 1, delegate { }, value => IsConnect = 1 == value, true);
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
