//-----------------------------------------------------------------------
// <copyright file="EGSEDeviceBUK.cs" company="IKI RSSI, laboratory №711">
//     Copyright (c) IKI RSSI, laboratory №711. All rights reserved.
// </copyright>
// <author>Семенов Александр, Коробейщиков Иван</author>
//-----------------------------------------------------------------------

namespace Egse.Devices
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Runtime.Serialization;
    using System.Runtime.Serialization.Formatters.Binary;
    using System.Threading.Tasks;
    using System.Windows.Input;
    using Egse.Constants;
    using Egse.Defaults;
    using Egse.Protocols;
    using Egse.USB;
    using Egse.Utilites;
    using System.Threading;

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
        private const int Hsi1ControlAddr = 0x30;

        /// <summary>
        /// Адресный байт "КВВ ПК2".
        /// </summary>
        private const int Hsi2ControlAddr = 0x31;

        /// <summary>
        /// Адресный байт "Дополнительные байты".
        /// </summary>
        private const int HsiStateAddr = 0x48;

        /// <summary>
        /// Адресный байт "Управление".
        /// </summary>
        private const int SimHsiControlAddr = 0x36;

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
        private const int SimHsiRecordFlushAddr = 0x39;

        /// <summary>
        /// Адресный байт "Данные УКС".
        /// </summary>
        private const int SimHsiRecordDataAddr = 0x38;

        /// <summary>
        /// Адресный байт "Выдача УКС".
        /// </summary>
        private const int SimHsiRecordSendAddr = 0x37;

        /// <summary>
        /// Адресный байт "TX_FLAG".
        /// </summary>
        private const int SimHsiRecordTXFlagAddr = 0x3a;

        /// <summary>
        /// Адресный байт "TX_BYTE_NUMBER".
        /// </summary>
        private const int SimHsiRecordByteNumberAddr = 0x3b;

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
        /// Отправляет команду включить/выключить затвор УФЭС ОСН.
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
        /// Отправляет команду включить/выключить затвор УФЭС РЕЗ.
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
        /// Отправляет команду включить/выключить затвор ВУФЭС ОСН.
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
        /// Отправляет команду включить/выключить затвор ВУФЭС РЕЗ.
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
            //// TODO удалить (только для проверки).
            SendToUSB(Spacewire1SPTPControlSD1SendTimeLoAddr, new byte[1] { (byte)value });  

            SendToUSB(Spacewire1SPTPControlSD1SendTimeHiAddr, new byte[1] { (byte)(value >> 8) });
            SendToUSB(Spacewire1SPTPControlSD1SendTimeLoAddr, new byte[1] { (byte)value });
            //// TODO удалить (только для проверки).
            SendToUSB(Spacewire1SPTPControlSD1SendTimeLoAddr, new byte[1] { (byte)value });  
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
            SendToUSB(Spacewire2RecordDataAddr, _intfBUK.Spacewire2Notify.MakeData());
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
                _intfBUK.Spacewire4Notify.DataToSaveList();

                SendToUSB(Spacewire4RecordDataAddr, _intfBUK.Spacewire4Notify.Data);                
            }

            SendToUSB(Spacewire4RecordSendAddr, new byte[1] { (byte)value });
        }

        /// <summary>
        /// Команда установки внутреннего времени устройства.
        /// </summary>
        internal void CmdSetDeviceTime()
        {
            SendToUSB(TimeResetAddr, new byte[1] { 1 });
            SendToUSB(TimeDataAddr, EgseTime.Now().ToArray());
            SendToUSB(TimeSetAddr, new byte[1] { 1 });
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
        internal void CmdHsiLine1(int value)
        {
            SendToUSB(Hsi1ControlAddr, new byte[1] { (byte)value });
        }

        /// <summary>
        /// Команда [управления ВСИ ПК2].
        /// </summary>
        /// <param name="value">Байт управления.</param>
        internal void CmdHsiLine2(int value)
        {
            SendToUSB(Hsi2ControlAddr, new byte[1] { (byte)value });
        }

        /// <summary>
        /// Команда [управления ВСИ статусом].
        /// </summary>
        /// <param name="value">Байт управления.</param>
        internal void CmdHsiState(int value)
        {
            SendToUSB(HsiStateAddr, new byte[1] { (byte)value });
        }
        
        /// <summary>
        /// Команда [управления имитатором ВСИ].
        /// </summary>
        /// <param name="value">Байт управления.</param>
        internal void CmdSimHsiControl(int value)
        {
            SendToUSB(SimHsiControlAddr, new byte[1] { (byte)value });
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
        internal void CmdSimHsiRecord(int value)
        {
            SendToUSB(SimHsiRecordTXFlagAddr, new byte[1] { 2 });            
            if ((null != _intfBUK.HsiNotify.Data) && (0 < _intfBUK.HsiNotify.Data.Length))
            {
                _intfBUK.HsiNotify.DataToSaveList();

                SendToUSB(SimHsiRecordByteNumberAddr, new byte[1] { Convert.ToByte(_intfBUK.HsiNotify.Data.Length) });
                SendToUSB(SimHsiRecordFlushAddr, new byte[1] { 1 });
                SendToUSB(SimHsiRecordDataAddr, _intfBUK.HsiNotify.Data);
            }

            SendToUSB(SimHsiRecordSendAddr, new byte[1] { (byte)value });            
        }

        /// <summary>
        /// Временно сохраняет массив байт.
        /// </summary>
        internal void TempData()
        {
            if ((null != _intfBUK.HsiNotify.Data) && (0 != _intfBUK.HsiNotify.Data.Length))
            {
                _tempData = new byte[_intfBUK.HsiNotify.Data.Length];
                Array.Copy(_intfBUK.HsiNotify.Data, _tempData, _intfBUK.HsiNotify.Data.Length);
            }
        }

        /// <summary>
        /// Восстанавливает сохраненный массив байт.
        /// </summary>
        internal void RevertData()
        {
            if ((null != _tempData) && (0 != _tempData.Length))
            {
                _intfBUK.HsiNotify.Data = new byte[_tempData.Length];
                Array.Copy(_tempData, _intfBUK.HsiNotify.Data, _tempData.Length);
            }
        }

        /// <summary>
        /// Команда [выдачи активного УКС активировать/деактивировать ПК1 ВСИ].
        /// Примечание:
        /// Временно подменяет данные УКС в ВСИ нотификаторе для формирования УКС активации.
        /// </summary>
        /// <param name="value">1 - для активации, 0 - деактивация.</param>
        internal void CmdSimHsi1(int value)
        { 
            TempData();
            if (1 == value)
            {                                
                _intfBUK.HsiNotify.Data = new byte[1] { 0xA1 };
            }
            else
            {
                _intfBUK.HsiNotify.Data = new byte[1] { 0xFF };
            }

            CmdSimHsiRecord(1);
            RevertData();
        }

        /// <summary>
        /// Команда [выдачи активного УКС активировать/деактивировать ПК2 ВСИ].
        /// Примечание:
        /// Временно подменяет данные УКС в ВСИ нотификаторе для формирования УКС активации.
        /// </summary>
        /// <param name="value">1 - для активации, 0 - деактивация.</param>
        internal void CmdSimHsi2(int value)
        {
            TempData();
            if (1 == value)
            {
                _intfBUK.HsiNotify.Data = new byte[1] { 0xA2 };
            }
            else
            {
                _intfBUK.HsiNotify.Data = new byte[1] { 0xFF };
            }

            CmdSimHsiRecord(1);
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
        private const int HsiDataAddr = 0x14;

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
        private bool isConnected;

        /// <summary>
        /// Отображать ли [окно "имитатор ВСИ"].
        /// </summary>
        private bool isShowHsi;

        /// <summary>
        /// Отображать ли [окно "имитатор БУСК"].
        /// </summary>
        private bool isShowSpacewire;

        /// <summary>
        /// Отображать ли [окно "имитатор НП"].
        /// </summary>
        private bool isShowSD;

        /// <summary>
        /// Отображать ли [окно "имитатор БУК (для ВСИ)"].
        /// </summary>
        private bool isShowSimHsi;

        /// <summary>
        /// Отображать ли [окно "имитатор БУК (для БУСК)"].
        /// </summary>
        private bool isShowSimSpacewire;

        /// <summary>
        /// Отображать ли [окно "имитатор БУК (для НП)"].
        /// </summary>
        private bool isShowSimSD;

        /// <summary>
        /// The is show control buk
        /// </summary>
        private bool isShowControlBuk;

        /// <summary>
        /// The is show tele buk
        /// </summary>
        private bool isShowTeleBuk;

        /// <summary>
        /// The is show tele KVV
        /// </summary>
        private bool isShowTeleKvv;

        /// <summary>
        /// Отображать ли [окно "монитор USB"].
        /// </summary>
        private bool isShowUsbSendsMonitor;

        /// <summary>
        /// Время, пришедшее от прибора.
        /// </summary>
        private EgseTime _deviceTime;

        /// <summary>
        /// Датчики затворов: бит выбора режима.
        /// </summary>
        private bool isIssueManualShutter = true;

        /// <summary>
        /// СДЩ: Датчики затворов: закрытия.
        /// </summary>
        private SciDevState issueSdshClose = SciDevState.Off;

        /// <summary>
        /// СДЩ: Датчики затворов: открытия.
        /// </summary>
        private SciDevState issueSdshOpen = SciDevState.Off;

        /// <summary>
        /// ВУФЭС: Датчики затворов: закрытия.
        /// </summary>
        private SciDevState issueVufesClose = SciDevState.Off;

        /// <summary>
        /// ВУФЭС: Датчики затворов: открытия.
        /// </summary>
        private SciDevState issueVufesOpen = SciDevState.Off;

        /// <summary>
        /// УФЭС: Датчики затворов: закрытия.
        /// </summary>
        private SciDevState issueUfesClose = SciDevState.Off;

        /// <summary>
        /// УФЭС: Датчики затворов: открытия.
        /// </summary>
        private SciDevState issueUfesOpen = SciDevState.Off;

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="EgseBukNotify" />.
        /// </summary>
        public EgseBukNotify()
        {
            IsConnected = false;

            _decoderUSB = new ProtocolUSB7C6E(null, LogsClass.LogEncoder, false, false);
            _decoderUSB.GotProtocolMsg += new ProtocolUSBBase.ProtocolMsgEventHandler(OnMessageFunc);
            _decoderUSB.GotProtocolError += new ProtocolUSBBase.ProtocolErrorEventHandler(OnErrorFunc);
            DeviceTime = new EgseTime();

            Device = new EgseBuk(Global.DeviceSerial, _decoderUSB, this);
            Device.ChangeStateEvent = OnChangeConnection;

            ControlValuesList = new Dictionary<string, ControlValue>();

            HsiNotify = new Hsi(this);
            TelemetryNotify = new Telemetry(this);
            Spacewire1Notify = new Spacewire1(this);
            Spacewire2Notify = new Spacewire2(this);
            Spacewire3Notify = new Spacewire3(this);
            Spacewire4Notify = new Spacewire4(this);
            ControlBukNotify = new ControlBuk(this);
            TeleBukNotify = new TeleBuk(this);
            TeleKvvNotify = new TeleKvv(this);

            UITestNotify = new UITest(this);
            UITestNotify.UsbLogFile = LogsClass.LogUSB.FileName;

            _decoderSpacewireBusk = new ProtocolSpacewire((uint)Spacewire2.Addr.Data, (uint)Spacewire2.Addr.End, (uint)Spacewire2.Addr.Time1, (uint)Spacewire2.Addr.Time2);
            _decoderSpacewireBusk.GotSpacewireMsg += new ProtocolSpacewire.SpacewireMsgEventHandler(OnSpacewire2Msg);
            _decoderSpacewireBusk.GotSpacewireMsg += new ProtocolSpacewire.SpacewireMsgEventHandler(Spacewire2Notify.OnSpacewire2MsgRawSave);
            _decoderSpacewireBusk.GotSpacewireTimeTick1Msg += new ProtocolSpacewire.SpacewireTimeTickMsgEventHandler((sender, e) => { Spacewire2Notify.BuskTickTime1 = (byte)(e.TimeTickInfo.Value | Spacewire2Notify.BuskTickTime2); });
            _decoderSpacewireBusk.GotSpacewireTimeTick2Msg += new ProtocolSpacewire.SpacewireTimeTickMsgEventHandler((sender, e) => { Spacewire2Notify.BuskTickTime2 = e.TimeTickInfo.Value; });
            _decoderUSB.GotProtocolMsg += new ProtocolUSBBase.ProtocolMsgEventHandler(_decoderSpacewireBusk.OnMessageFunc);

            _decoderSpacewireBuk = new ProtocolSpacewire((uint)Spacewire2.Addr.BukData, (uint)Spacewire2.Addr.BukEnd, (uint)Spacewire2.Addr.BukTime1, (uint)Spacewire2.Addr.BukTime2);
            _decoderSpacewireBuk.GotSpacewireMsg += new ProtocolSpacewire.SpacewireMsgEventHandler(OnSpacewire2Msg);
            _decoderSpacewireBuk.GotSpacewireMsg += new ProtocolSpacewire.SpacewireMsgEventHandler(Spacewire2Notify.OnSpacewire2MsgRawSave);
            _decoderSpacewireBuk.GotSpacewireTimeTick1Msg += new ProtocolSpacewire.SpacewireTimeTickMsgEventHandler((sender, e) => { Spacewire2Notify.BukTickTime1 = (byte)(e.TimeTickInfo.Value | Spacewire2Notify.BukTickTime2); });
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

            _decoderHsi = new ProtocolHsi((uint)Hsi.Addr);
            _decoderHsi.GotHsiMsg += new ProtocolHsi.HsiMsgEventHandler(OnHsiMsg);
            _decoderHsi.GotHsiMsg += new ProtocolHsi.HsiMsgEventHandler(HsiNotify.OnHsiMsgRawSave);
            _decoderUSB.GotProtocolMsg += new ProtocolUSBBase.ProtocolMsgEventHandler(_decoderHsi.OnMessageFunc);

            ControlValuesList.Add(Global.Shutters, new ControlValue());
            ControlValuesList[Global.Shutters].AddProperty(Global.Shutters.Auto, 14, 1, Device.CmdAutoShutters, value => IsIssueManualShutter = !Convert.ToBoolean(value));
            ControlValuesList[Global.Shutters].AddProperty(Global.Shutters.UfesOpen, 10, 1, Device.CmdShutters, value => IssueUfesOpen = (SciDevState)value);
            ControlValuesList[Global.Shutters].AddProperty(Global.Shutters.UfesClose, 8, 1, Device.CmdShutters, value => IssueUfesClose = (SciDevState)value);
            ControlValuesList[Global.Shutters].AddProperty(Global.Shutters.VufesOpen, 6, 1, Device.CmdShutters, value => IssueVufesOpen = (SciDevState)value);
            ControlValuesList[Global.Shutters].AddProperty(Global.Shutters.VufesClose, 4, 1, Device.CmdShutters, value => IssueVufesClose = (SciDevState)value);
            ControlValuesList[Global.Shutters].AddProperty(Global.Shutters.SdshOpen, 2, 1, Device.CmdShutters, value => IssueSdshOpen = (SciDevState)value);
            ControlValuesList[Global.Shutters].AddProperty(Global.Shutters.SdshClose, 0, 1, Device.CmdShutters, value => IssueSdshClose = (SciDevState)value);
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
        public enum SciDevState
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
        public Hsi HsiNotify { get; private set; }

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
        /// Gets the tele buk notify.
        /// </summary>
        /// <value>
        /// The tele buk notify.
        /// </value>
        public TeleBuk TeleBukNotify { get; private set; }

        /// <summary>
        /// Gets the tele KVV notify.
        /// </summary>
        /// <value>
        /// The tele KVV notify.
        /// </value>
        public TeleKvv TeleKvvNotify { get; private set; }

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
        /// Gets the control buk notify.
        /// </summary>
        /// <value>
        /// The control buk notify.
        /// </value>
        public ControlBuk ControlBukNotify { get; private set; }

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
        /// Получает или задает значение, показывающее, что [ручное управление датчиками затворов включено].
        /// </summary>
        /// <value>
        /// <c>true</c> если [ручное управление датчиками затворов включено]; иначе, <c>false</c>.
        /// </value>
        public bool IsIssueManualShutter 
        { 
            get
            {
                return this.isIssueManualShutter;
            }

            set 
            {
                if (value == this.isIssueManualShutter)
                { 
                    return;
                }

                this.isIssueManualShutter = value;
                ControlValuesList[Global.Shutters].SetProperty(Global.Shutters.Auto, Convert.ToInt32(!value));
                this.OnPropertyChanged();
            } 
        }

        /// <summary>
        /// Получает или задает значение [состояния датчика открытия затвора для канала УФЭС].
        /// </summary>
        /// <value>
        /// Значение [состояния датчика открытия затвора для канала УФЭС].
        /// </value>
        public SciDevState IssueUfesOpen
        {
            get
            {
                return this.issueUfesOpen;
            }

            set 
            {
                if (value == this.issueUfesOpen)
                {
                    return;
                }

                this.issueUfesOpen = value;
                ControlValuesList[Global.Shutters].SetProperty(Global.Shutters.UfesOpen, (int)value);
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// Получает или задает значение [состояния датчика закрытия затвора для канала УФЭС].
        /// </summary>
        /// <value>
        /// Значение [состояния датчика закрытия затвора для канала УФЭС].
        /// </value>
        public SciDevState IssueUfesClose
        {
            get
            {
                return this.issueUfesClose;
            }

            set 
            {
                if (value == this.issueUfesClose)
                {
                    return;
                }

                this.issueUfesClose = value;
                ControlValuesList[Global.Shutters].SetProperty(Global.Shutters.UfesClose, (int)value);
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// Получает или задает значение [состояния датчика открытия затвора для канала ВУФЭС].
        /// </summary>
        /// <value>
        /// Значение [состояния датчика открытия затвора для канала ВУФЭС].
        /// </value>
        public SciDevState IssueVufesOpen
        {
            get
            {
                return this.issueVufesOpen;
            }

            set 
            {
                if (value == this.issueVufesOpen)
                {
                    return;
                }

                this.issueVufesOpen = value;
                ControlValuesList[Global.Shutters].SetProperty(Global.Shutters.VufesOpen, (int)value);
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Получает или задает значение [состояния датчика закрытия затвора для канала ВУФЭС].
        /// </summary>
        /// <value>
        /// Значение [состояния датчика закрытия затвора для канала ВУФЭС].
        /// </value>
        public SciDevState IssueVufesClose
        {
            get
            {
                return this.issueVufesClose;
            }

            set 
            {
                this.issueVufesClose = value;
                ControlValuesList[Global.Shutters].SetProperty(Global.Shutters.VufesClose, (int)value);
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Получает или задает значение [состояния датчика открытия затвора для канала СДЩ].
        /// </summary>
        /// <value>
        /// Значение [состояния датчика открытия затвора для канала СДЩ].
        /// </value>
        public SciDevState IssueSdshOpen
        {
            get
            {
                return this.issueSdshOpen;
            }

            set 
            {
                this.issueSdshOpen = value;
                ControlValuesList[Global.Shutters].SetProperty(Global.Shutters.SdshOpen, (int)value);
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Получает или задает значение [состояния датчика закрытия затвора для канала СДЩ].
        /// </summary>
        /// <value>
        /// Значение [состояния датчика закрытия затвора для канала СДЩ].
        /// </value>
        public SciDevState IssueSdshClose 
        { 
            get 
            {
                return this.issueSdshClose;
            }

            set 
            {
                this.issueSdshClose = value;
                ControlValuesList[Global.Shutters].SetProperty(Global.Shutters.SdshClose, (int)value);
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Получает значение, показывающее, что [прибор подключен].
        /// </summary>
        /// <value>
        /// <c>true</c> если [прибор подключен]; иначе, <c>false</c>.
        /// </value>
        public bool IsConnected
        {
            get
            {
                return this.isConnected;
            }

            private set 
            {
                this.isConnected = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Получает значение, показывающее, открыто ли [окно "имитатор ВСИ"].
        /// </summary>
        /// <value>
        ///   <c>true</c> если [окно "имитатор ВСИ"] открыто; иначе, <c>false</c>.
        /// </value>
        public bool IsShowHsi
        {
            get
            {
                return this.isShowHsi;
            }

            private set 
            {
                this.isShowHsi = value;
                OnPropertyChanged();
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
                return this.isShowSpacewire;
            }

            private set 
            {
                this.isShowSpacewire = value;
                OnPropertyChanged();
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
                return this.isShowSD;
            }

            private set 
            {
                this.isShowSD = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Получает значение, показывающее, открыто ли [окно "имитатор БУК (для ВСИ)"].
        /// </summary>
        /// <value>
        ///   <c>true</c> если [окно "имитатор БУК (для ВСИ)"] открыто; иначе, <c>false</c>.
        /// </value>
        public bool IsShowSimHsi
        {
            get
            {
                return this.isShowSimHsi;
            }

            private set 
            {
                this.isShowSimHsi = value;
                OnPropertyChanged();
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
                return this.isShowUsbSendsMonitor;
            }

            private set 
            {
                this.isShowUsbSendsMonitor = value;
                OnPropertyChanged();
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
                return this.isShowSimSpacewire;
            }

            private set 
            {
                this.isShowSimSpacewire = value;
                OnPropertyChanged();
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
                return this.isShowSimSD;
            }

            private set 
            {
                this.isShowSimSD = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is show control buk.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is show control buk; otherwise, <c>false</c>.
        /// </value>
        public bool IsShowControlBuk
        {
            get
            {
                return this.isShowControlBuk;
            }

            private set
            {
                this.isShowControlBuk = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is show tele buk.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is show tele buk; otherwise, <c>false</c>.
        /// </value>
        public bool IsShowTeleBuk
        {
            get
            {
                return this.isShowTeleBuk;
            }
            
            private set
            {
                this.isShowTeleBuk = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is show tele KVV.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is show tele KVV; otherwise, <c>false</c>.
        /// </value>
        public bool IsShowTeleKvv
        {
            get
            {
                return this.isShowTeleKvv;
            }
            
            private set
            {
                this.isShowTeleKvv = value;
                OnPropertyChanged();
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
        /// Событие: требуется загрузить настройки.
        /// </summary>
        public void LoadAppEvent()
        {            
            Spacewire2Notify.LoadDataList(Wpf.Properties.Settings.Default.Spw2Cmds);                     
            Spacewire4Notify.LoadDataList(Wpf.Properties.Settings.Default.Spw4Cmds);            
            HsiNotify.LoadDataList(Wpf.Properties.Settings.Default.HsiCmds);
            TelemetryNotify.Serialize();
            HsiNotify.Serialize();
            Spacewire1Notify.Serialize();
            Spacewire2Notify.Serialize();
            Spacewire3Notify.Serialize();
            Spacewire4Notify.Serialize();            
        }

        /// <summary>
        /// Событие: требуется сохранить настройки.
        /// </summary>
        public void SaveAppEvent()
        {
            Spacewire2Notify.SaveDataList(Wpf.Properties.Settings.Default.Spw2Cmds);
            Spacewire4Notify.SaveDataList(Wpf.Properties.Settings.Default.Spw4Cmds);
            HsiNotify.SaveDataList(Wpf.Properties.Settings.Default.HsiCmds);
            Wpf.Properties.Settings.Default.Save();
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
                RefreshAllControlsValues();
                Task.Run(() =>
                {
                    // задержка для получения текущих значений от прибора
                    Thread.Sleep(3000);                    
                    Device.CmdSetDeviceLogicAddr();
                    Device.CmdSetDeviceTime();
                    Spacewire1Notify.SD1SendTime = 1000;                                
                });                
                LogsClass.LogMain.LogText = Resource.Get(@"stDeviceName") + Resource.Get(@"stConnected");
            }
            else
            {
                LogsClass.LogMain.LogText = Resource.Get(@"stDeviceName") + Resource.Get(@"stDisconnected");
                TelemetryNotify.Deserialize();
                HsiNotify.Deserialize();
                Spacewire1Notify.Deserialize();
                Spacewire2Notify.Deserialize();
                Spacewire3Notify.Deserialize();
                Spacewire4Notify.Deserialize();
            }
        }

        /// <summary>
        /// Вызывается при подключении прибора, чтобы все элементы управления обновили свои значения.
        /// </summary>
        public void RefreshAllControlsValues()
        {
            Debug.Assert(ControlValuesList != null, Resource.Get(@"eNotAssigned"));
            foreach (var cv in ControlValuesList)
            {
                (cv.Value as ControlValue).RefreshGetValue();
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
                else if (e is SpacewireTm604MsgEventArgs)
                {
                    TeleBukNotify.TmBukData = (e as SpacewireTm604MsgEventArgs).Tm604Info.TmBukInfo;
                    TeleKvvNotify.TmKvvData = (e as SpacewireTm604MsgEventArgs).Tm604Info.TmKvvInfo;
                    this.GotSpacewire2Msg(sender, e);
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
                        HsiNotify.RequestStateMain++;
                    }
                    else
                    {
                        HsiNotify.RequestStateResv++;
                    }
                }
                else if (IsHsiRequestDataMsg(e))
                {
                    if (HsiMsgEventArgs.HsiLine.Main == e.Info.Line)
                    {
                        HsiNotify.RequestDataMain++;
                    }
                    else
                    {
                        HsiNotify.RequestDataResv++;
                    }
                }
                else if (IsHsiCmdMsg(e))
                {
                    if (this.GotHsiCmdMsg != null)
                    {
                        if (IsCmdFromSet1(e))
                        {
                            HsiNotify.CmdCounter1++;
                        }
                        else if (IsCmdFromSet2(e))
                        {
                            HsiNotify.CmdCounter2++;
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
        /// Called when [property changed].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="propertyName">Name of the property.</param>
        protected void OnPropertyChanged(INotifyPropertyChanged sender, [CallerMemberName] string propertyName = null)
        {
            OnPropertyChangedExplicit(sender, propertyName);
        }

        /// <summary>
        /// Принудительно обновляет свойства. 
        /// </summary>
        private void UpdateProperties()
        {
            OnPropertyChanged(() => this.DeviceTime);
            OnPropertyChanged(() => this.DeviceSpeed);
            OnPropertyChanged(() => this.DeviceTrafic);
            OnPropertyChanged(() => this.BytesAvailable);
            Spacewire1Notify.UpdateProperties();
            Spacewire2Notify.UpdateProperties();
            Spacewire3Notify.UpdateProperties();
            Spacewire4Notify.UpdateProperties();
            HsiNotify.UpdateProperties();
            ControlBukNotify.UpdateProperties();
            TeleBukNotify.UpdateProperties();
            TeleKvvNotify.UpdateProperties();
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
                        byte[] buf = new byte[6];
                        Array.Copy(msg.Data, 1, buf, 0, 6);
                        DeviceTime = buf.AsEgseTime();
                        ControlValuesList[Global.Spacewire2.Control].UsbValue = msg.Data[7];
                        ControlValuesList[Global.Spacewire2.Record].UsbValue = msg.Data[10]; 
                        ControlValuesList[Global.Spacewire2.BuskLogic].UsbValue = msg.Data[11];
                        ControlValuesList[Global.Spacewire2.BukLogic].UsbValue = msg.Data[12];
                        ControlValuesList[Global.Spacewire2.SptpControl].UsbValue = msg.Data[14];
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
                        ControlValuesList[Global.Hsi.Line1].UsbValue = msg.Data[33];
                        ControlValuesList[Global.Hsi.Line2].UsbValue = msg.Data[34];
                        ControlValuesList[Global.Hsi.Line1StateCounter].UsbValue = (msg.Data[36] << 8) | msg.Data[35];
                        ControlValuesList[Global.Hsi.Line1FrameCounter].UsbValue = (msg.Data[38] << 8) | msg.Data[37];
                        ControlValuesList[Global.Hsi.Line2StateCounter].UsbValue = (msg.Data[40] << 8) | msg.Data[39];
                        ControlValuesList[Global.Hsi.Line2FrameCounter].UsbValue = (msg.Data[42] << 8) | msg.Data[41];
                        ControlValuesList[Global.SimHsi.Control].UsbValue = msg.Data[43];
                        ControlValuesList[Global.SimHsi.Record].UsbValue = msg.Data[44];
                        ControlValuesList[Global.Shutters].UsbValue = (msg.Data[53] << 8) | msg.Data[52];
                        ControlValuesList[Global.Hsi.State].UsbValue = msg.Data[54];
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
        /// Called when [property changed].
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            OnPropertyChangedExplicit(this, propertyName);
        }

        /// <summary>
        /// Called when [property changed].
        /// </summary>
        /// <typeparam name="TProperty">The type of the property.</typeparam>
        /// <param name="propertyExpressions">The property expressions.</param>
        private void OnPropertyChanged<TProperty>(params System.Linq.Expressions.Expression<Func<TProperty>>[] propertyExpressions)
        {
            foreach (var projection in propertyExpressions)
            {
                System.Linq.Expressions.MemberExpression memberExpression = (System.Linq.Expressions.MemberExpression)projection.Body;
                OnPropertyChangedExplicit(this, memberExpression.Member.Name);
            }
        }

        /// <summary>
        /// Called when [property changed explicit].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="propertyName">Name of the property.</param>
        private void OnPropertyChangedExplicit(INotifyPropertyChanged sender = null, string propertyName = null)
        {
            if ((null != PropertyChanged) && (null != propertyName))
            {
                PropertyChanged(null == sender ? this : sender, new PropertyChangedEventArgs(propertyName));
                System.Windows.Input.CommandManager.InvalidateRequerySuggested();
            }
        }

        /// <summary>
        /// Реализует сохранение в user.config файл списков отправленных команд(данных).
        /// Примечание(особенность работы):
        /// Жестко привязана к текущей версии программы. Для каждой версии существует отдельный user.config
        /// </summary>
        [Serializable]
        public class PropSerializer
        {
            /// <summary>
            /// Количество сохраняемых команд(данных) в списке.
            /// </summary>
            protected const int MaxDataListCount = 15;
            
            /// <summary>
            /// Список отправленных команд(данных).
            /// </summary>
            [field: NonSerialized]
            private ObservableCollection<string> dataList = new ObservableCollection<string>();

            /// <summary>
            /// Получает список отправленных команд(данных).
            /// </summary>
            /// <value>
            /// Список отправленных команд(данных).
            /// </value>
            public ObservableCollection<string> DataList
            {
                get
                {
                    return this.dataList;
                }
            }

            /// <summary>
            /// Загружает из коллекции в список.
            /// </summary>
            /// <param name="collection">Коллекция строк.</param>
            /// <param name="useIniFile">Если установлен <c>true</c> [будет использован ini файл].</param>
            public void LoadDataList(StringCollection collection, bool useIniFile = true)
            {
                if (null != collection)
                {
                    List<string> list = collection.Cast<string>().ToList();
                    foreach (string cmd in list)
                    {
                        DataList.Add(cmd);
                    }

                    if (useIniFile)
                    {
                        List<string> listIni = new List<string>();
                        AppSettings.LoadList(listIni, this.ToString());
                        if (0 < listIni.Count)
                        {
                            DataList.Clear();
                        }

                        foreach (string cmd in listIni)
                        {
                            DataList.Add(cmd);
                        }
                    }
                }                
            }

            /// <summary>
            /// Сохраняет список команд(данных) в коллекцию.
            /// </summary>
            /// <param name="collection">Коллекция строк.</param>
            /// <param name="useIniFile">Если установлено <c>true</c> [будет использован ini файл].</param>
            public void SaveDataList(StringCollection collection, bool useIniFile = true)
            {                
                if (0 < DataList.Count)
                {
                    if (useIniFile)
                    {
                        List<string> list = collection.Cast<string>().ToList();
                        AppSettings.SaveList(list, this.ToString());
                    }

                    collection.Clear();
                    collection.AddRange(DataList.ToArray());
                }
            }
        }

        /// <summary>
        /// Прототип подкласса нотификатора.
        /// </summary>
        [Serializable]
        public class SubNotify : PropSerializer, INotifyPropertyChanged
        {
            /// <summary>
            /// Экземпляр потока памяти, для сохранения дефолтных настроек.
            /// </summary>
            [field: NonSerialized]
            private MemoryStream serializeStream = new MemoryStream();

            /// <summary>
            /// Экземпляр сериализатора.
            /// </summary>
            [field: NonSerialized]
            private BinaryFormatter serializer;

            /// <summary>
            /// Ссылка на экземпляр словаря ControlValue.
            /// </summary>
            [field: NonSerialized]
            private Dictionary<string, ControlValue> controlValuesList;

            /// <summary>
            /// Ссылка на экземпляр главного нотификатора.
            /// </summary>
            [field: NonSerialized]
            private EgseBukNotify owner;

            /// <summary>
            /// Ссылка на экземпляр устройства.
            /// </summary>
            [field: NonSerialized]
            private EgseBuk device;

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
            [field: NonSerialized]          
            public event PropertyChangedEventHandler PropertyChanged;
                                                         
            /// <summary>
            /// Получает доступ к интерфейсу устройства. 
            /// </summary>
            protected EgseBukNotify Owner 
            { 
                get
                {
                    return this.owner;
                }

                private set
                {
                    this.owner = value;
                }
            }

            /// <summary>
            /// Получает доступ к USB прибора.
            /// </summary>
            protected EgseBuk Device 
            { 
                get
                {
                    return this.device;
                }

                private set
                {
                    this.device = value;
                }
            }
                       
            /// <summary>
            /// Получает список управляющих элементов.
            /// </summary>
            protected Dictionary<string, ControlValue> ControlValuesList 
            { 
                get
                {
                    return this.controlValuesList;
                }

                private set
                {
                    this.controlValuesList = value;
                }
            }

            /// <summary>
            /// Serializes the specified object.
            /// </summary>
            /// <param name="obj">The object.</param>
            public virtual void Serialize(object obj)
            {
                if (null == serializer)
                {
                    serializer = new BinaryFormatter();
                    serializer.FilterLevel = System.Runtime.Serialization.Formatters.TypeFilterLevel.Low;
                }

                serializer.Serialize(serializeStream, obj);
                serializeStream.Flush();
            }

            /// <summary>
            /// Deserializes this instance.
            /// </summary>
            /// <exception cref="System.NullReferenceException">Не удалось восстановить экземпляр.</exception>
            public virtual void Deserialize()
            {
                if (null != serializer)
                {
                    serializeStream.Position = 0;
                    var defaultObj = serializer.Deserialize(serializeStream);
                    MemberInfo[] members = FormatterServices.GetSerializableMembers(defaultObj.GetType());
                    foreach (MemberInfo mi in members)
                    {
                        if (mi.MemberType == MemberTypes.Field)
                        {
                            FieldInfo fi = mi as FieldInfo;
                            FieldInfo ownerField = this.GetType().GetField(fi.Name, BindingFlags.NonPublic | BindingFlags.Instance);
                            if (null != ownerField)
                            {
                                var ownerValue = ownerField.GetValue(this);
                                var defaultValue = fi.GetValue(defaultObj);
                                if ((null != ownerValue) && (null != defaultValue) && (!ownerValue.Equals(defaultValue)))
                                {
                                    ownerField.SetValue(this, defaultValue);
                                }
                            }
                            else
                            {
                                throw new NullReferenceException("Не удалось восстановить экземпляр.");
                            }
                        }
                    }

                    this.OnPropertyChanged(string.Empty);
                }
            }

            public virtual void UpdateProperties()
            {
            }

            /// <summary>
            /// Called when [property changed].
            /// </summary>
            /// <param name="propertyName">Name of the property.</param>
            protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
            {
                OnPropertyChangedExplicit(propertyName);
            }

            /// <summary>
            /// Called when [property changed].
            /// </summary>
            /// <typeparam name="TProperty">The type of the property.</typeparam>
            /// <param name="propertyExpressions">The property expressions.</param>
            protected void OnPropertyChanged<TProperty>(params System.Linq.Expressions.Expression<Func<TProperty>>[] propertyExpressions)
            {
                foreach (var projection in propertyExpressions)
                {
                    System.Linq.Expressions.MemberExpression memberExpression = (System.Linq.Expressions.MemberExpression)projection.Body;
                    OnPropertyChangedExplicit(memberExpression.Member.Name);
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

            /// <summary>
            /// Called when [property changed explicit].
            /// </summary>
            /// <param name="propertyName">Name of the property.</param>
            private void OnPropertyChangedExplicit(string propertyName)
            {
                if ((null != Owner) && (null != PropertyChanged))
                {
                    Owner.OnPropertyChanged(this, propertyName);
                }
            }
        }

        /// <summary>
        /// Нотификатор ВСИ интерфейса.
        /// </summary>
        [Serializable]     
        public class Hsi : SubNotify, IDataErrorInfo
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
            private bool isIssueEnable1;

            /// <summary>
            /// КВВ ПК2: Вкл/выкл.
            /// </summary>
            private bool isIssueEnable2;

            /// <summary>
            /// КВВ ПК1: Счетчик выданных статусов.
            /// </summary>
            private int stateCounter1;

            /// <summary>
            /// КВВ ПК1: Счетчик выданных кадров.
            /// </summary>
            private int frameCounter1;

            /// <summary>
            /// КВВ ПК2: Счетчик выданных статусов.
            /// </summary>
            private int stateCounter2;

            /// <summary>
            /// КВВ ПК2: Счетчик выданных кадров.
            /// </summary>
            private int frameCounter2;

            /// <summary>
            /// Выдача УКС: Выдача УКС.
            /// </summary>
            private bool isIssueCmd;

            /// <summary>
            /// Управление: Опрос данных.
            /// </summary>
            private bool isIssuePoll;

            /// <summary>
            /// Управление: Линия передачи.
            /// </summary>
            private SimLine lineSend;

            /// <summary>
            /// Управление: Линия приема.
            /// </summary>
            private SimLine lineRecv;

            /// <summary>
            /// КВВ ПК1: Линия передачи.
            /// </summary>
            private Line line1;

            /// <summary>
            /// КВВ ПК2: Линия передачи.
            /// </summary>
            private Line line2;

            /// <summary>
            /// Сохранять данные в файл.
            /// </summary>
            private bool isSaveRawData;

            /// <summary>
            /// Имя файла данных.
            /// </summary>
            private string rawDataFile;
            
            /// <summary>
            /// Для асинхронной записи в файл.
            /// </summary>
            [field: NonSerialized]
            private FileStream rawDataStream;
            
            /// <summary>
            /// Квазиасинхронная запись в файл.
            /// Примечание:
            /// Используется для сигнала, что все данные записались в файл.
            /// </summary>
            [field: NonSerialized]
            private Task rawDataTask;

            /// <summary>
            /// Количество запросов статуса по основной линии.
            /// </summary>
            private long requestStateMain;

            /// <summary>
            /// Количество запросов статуса по резервной линии.
            /// </summary>
            private long requestStateResv;

            /// <summary>
            /// Количество запросов данных по резервной линии.
            /// </summary>
            private long requestDataResv;

            /// <summary>
            /// Количество запросов данных по основной линии.
            /// </summary>
            private long requestDataMain;

            /// <summary>
            /// Количество переданных УКС ПК1.
            /// </summary>
            private long cmdCounter1;
            
            /// <summary>
            /// Экземпляр команды на [включение КВВ ПК1].
            /// </summary>
            [field: NonSerialized]
            private ICommand _issueEnable1Command;
            
            /// <summary>
            /// Экземпляр команды на [включение КВВ ПК2].
            /// </summary>
            [field: NonSerialized]
            private ICommand _issueEnable2Command;           

            /// <summary>
            /// Экземпляр команды на [включение записи в файл].
            /// </summary>
            [field: NonSerialized]
            private ICommand _saveRawDataCommand;

            /// <summary>
            /// Экземпляр команды на [открытие данных из файла].
            /// </summary>
            [field: NonSerialized]
            private ICommand _fromFileCommand;
            
            /// <summary>
            /// Экземпляр команды на [выдачу активного УКС активация ПК1 по интерфейсу ВСИ].
            /// </summary>
            [field: NonSerialized]
            private ICommand issueCmdEnable1Command;
            
            /// <summary>
            /// Экземпляр команды на [выдачу активного УКС деактивировать ПК-ы по интерфейсу ВСИ].
            /// </summary>
            [field: NonSerialized]
            private ICommand _issueCmdDisableCommand;
            
            /// <summary>
            /// Экземпляр команды на [выдачу активного УКС активация ПК2 по интерфейсу ВСИ].
            /// </summary>
            [field: NonSerialized]
            private ICommand issueCmdEnable2Command;
            
            /// <summary>
            /// Экземпляр команды на [выдачу УКС по интерфейсу ВСИ].
            /// </summary>
            [field: NonSerialized]
            private ICommand issueCmdCommand;

            /// <summary>
            /// Количество переданных УКС ПК2.
            /// </summary>
            private long cmdCounter2;

            /// <summary>
            /// ПК1 статус: готов.
            /// </summary>
            private bool isIssueReady1;

            /// <summary>
            /// ПК2 статус: готов.
            /// </summary>
            private bool isIssueReady2;

            /// <summary>
            /// ПК2 статус: busy.
            /// </summary>
            private bool isIssueBusy2;

            /// <summary>
            /// ПК1 статус: busy.
            /// </summary>
            private bool isIssueBusy1;

            /// <summary>
            /// ПК2 статус: me.
            /// </summary>
            private bool isIssueMe2;

            /// <summary>
            /// ПК1 статус: me.
            /// </summary>
            private bool isIssueMe1;

            /// <summary>
            /// Активность первого полукомплекта.
            /// </summary>
            private bool isActive1;

            /// <summary>
            /// Активность второго полукомплекта.
            /// </summary>
            private bool isActive2;

            /// <summary>
            /// Сохранять в текстовый лог-файл.
            /// </summary>
            private bool isSaveTxtData = true;

            /// <summary>
            /// Инициализирует новый экземпляр класса <see cref="Hsi" />.
            /// </summary>
            /// <param name="owner">The owner.</param>
            public Hsi(EgseBukNotify owner)
                : base(owner)
            {
                LogsClass.LogHsi.GotLogChange += (sender, e) => OnPropertyChanged(() => this.TxtDataFileSizeFormated);
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
                None = 0x03,
                
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
                Both = 0x00
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
            /// Получает или задает значение, показывающее, что [установлено состояние статуса ПК1: готов].
            /// </summary>
            /// <value>
            /// <c>true</c> если [установлено состояние статуса ПК1: готов]; иначе, <c>false</c>.
            /// </value>
            public bool IsIssueReady1
            {
                get
                {
                    return this.isIssueReady1;
                }

                set 
                {
                    if (value == this.isIssueReady1)
                    {
                        return;
                    }

                    this.isIssueReady1 = value;
                    ControlValuesList[Global.Hsi.State].SetProperty(Global.Hsi.State.IssueReady1, Convert.ToInt32(value));
                    OnPropertyChanged();
                }
            }

            /// <summary>
            /// Получает или задает значение, показывающее, что [установлено состояние статуса ПК2: готов].
            /// </summary>
            /// <value>
            /// <c>true</c> если [установлено состояние статуса ПК2: готов]; иначе, <c>false</c>.
            /// </value>
            public bool IsIssueReady2
            {
                get
                {
                    return this.isIssueReady2;
                }

                set 
                {
                    if (value == this.isIssueReady2)
                    {
                        return;
                    }

                    this.isIssueReady2 = value;
                    ControlValuesList[Global.Hsi.State].SetProperty(Global.Hsi.State.IssueReady2, Convert.ToInt32(value));
                    OnPropertyChanged();
                }
            }

            /// <summary>
            /// Получает или задает значение, показывающее, что [установлено состояние статуса ПК1: занят].
            /// </summary>
            /// <value>
            /// <c>true</c> если [установлено состояние статуса ПК1: занят]; иначе, <c>false</c>.
            /// </value>
            public bool IsIssueBusy1
            {
                get
                {
                    return this.isIssueBusy1;
                }

                set 
                {
                    if (value == this.isIssueBusy1)
                    {
                        return;
                    }

                    this.isIssueBusy1 = value;
                    ControlValuesList[Global.Hsi.State].SetProperty(Global.Hsi.State.IssueBusy1, Convert.ToInt32(value));
                    OnPropertyChanged();
                }
            }

            /// <summary>
            /// Получает или задает значение, показывающее, что [установлено состояние статуса ПК2: занят].
            /// </summary>
            /// <value>
            /// <c>true</c> если [установлено состояние статуса ПК2: занят]; иначе, <c>false</c>.
            /// </value>
            public bool IsIssueBusy2
            {
                get
                {
                    return this.isIssueBusy2;
                }

                set 
                {
                    if (value == this.isIssueBusy2)
                    {
                        return;
                    }

                    this.isIssueBusy2 = value;
                    ControlValuesList[Global.Hsi.State].SetProperty(Global.Hsi.State.IssueBusy2, Convert.ToInt32(value));
                    OnPropertyChanged();
                }
            }

            /// <summary>
            /// Получает или задает значение, показывающее, что [запись текстового лог-файла включена].
            /// </summary>
            /// <value>
            /// <c>true</c> если [запись текстового лог-файла включена]; иначе, <c>false</c>.
            /// </value>
            public bool IsSaveTxtData
            {
                get
                {
                    return this.isSaveTxtData;
                }

                set
                {
                    if (value == this.isSaveTxtData)
                    {
                        return;
                    }

                    this.isSaveTxtData = value;
                    OnPropertyChanged();
                    if (value)
                    {
                        LogsClass.LogHsi.NewLog();
                    }

                    OnPropertyChanged(() => this.TxtDataFile);
                }
            }

            /// <summary>
            /// Получает или задает значение, показывающее, что [установлено состояние статуса ПК1: me].
            /// </summary>
            /// <value>
            /// <c>true</c> если [установлено состояние статуса ПК1: me]; иначе, <c>false</c>.
            /// </value>
            public bool IsIssueMe1
            {
                get
                {
                    return this.isIssueMe1;
                }

                set 
                {
                    if (value == this.isIssueMe1)
                    {
                        return;
                    }

                    this.isIssueMe1 = value;
                    ControlValuesList[Global.Hsi.State].SetProperty(Global.Hsi.State.IssueMe1, Convert.ToInt32(value));
                    OnPropertyChanged();
                }
            }

            /// <summary>
            /// Получает или задает значение, показывающее, что [установлено состояние статуса ПК2: me].
            /// </summary>
            /// <value>
            /// <c>true</c> если [установлено состояние статуса ПК2: me]; иначе, <c>false</c>.
            /// </value>
            public bool IsIssueMe2
            {
                get
                {
                    return this.isIssueMe2;
                }

                set 
                {
                    if (value == this.isIssueMe2)
                    {
                        return;
                    }

                    this.isIssueMe2 = value;
                    ControlValuesList[Global.Hsi.State].SetProperty(Global.Hsi.State.IssueMe2, Convert.ToInt32(value));
                    OnPropertyChanged();
                }
            }

            /// <summary>
            /// Получает наименование текстовго лог-файла.
            /// </summary>
            /// <value>
            /// Наименование текстовго лог-файла.
            /// </value>
            public string TxtDataFile
            {
                get
                {
                    if (IsSaveTxtData)
                    {
                        return LogsClass.LogHsi.FileName;
                    }
                    else
                    {
                        return string.Empty;
                    }
                }
            }

            public string TxtDataFileSizeFormated
            {
                get
                {
                    if (IsSaveTxtData)
                    {
                        return LogsClass.LogHsi.FileSize.AsFileSizeString();
                    }
                    else
                    {
                        return ((long)0).AsFileSizeString();
                    }
                }
            }

            /// <summary>
            /// Получает или задает значение, показывающее, что [включен КВВ ПК1].
            /// </summary>
            /// <value>
            ///   <c>true</c> если [включен КВВ ПК1]; иначе, <c>false</c>.
            /// </value>
            public bool IsIssueEnable1
            {
                get
                {
                    return this.isIssueEnable1;
                }

                set 
                {
                    if (value == this.isIssueEnable1)
                    {
                        return;
                    }

                    this.isIssueEnable1 = value;
                    ControlValuesList[Global.Hsi.Line1].SetProperty(Global.Hsi.Line1.IssueEnable, Convert.ToInt32(value));
                    OnPropertyChanged();
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
            /// Получает или задает значение, показывающее, что [включен КВВ ПК2].
            /// </summary>
            /// <value>
            /// <c>true</c> если [включен КВВ ПК2]; иначе, <c>false</c>.
            /// </value>
            public bool IsIssueEnable2
            {
                get
                {
                    return this.isIssueEnable2;
                }

                set 
                {
                    if (value == this.isIssueEnable2)
                    {
                        return;
                    }

                    this.isIssueEnable2 = value;
                    ControlValuesList[Global.Hsi.Line2].SetProperty(Global.Hsi.Line2.IssueEnable, Convert.ToInt32(value));
                    OnPropertyChanged();
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
            /// Получает значение [количества выданных статусов по 1 ПК].
            /// </summary>
            /// <value>
            /// Значение [количества выданных статусов по 1 ПК].
            /// </value>
            public int StateCounter1
            {
                get
                {
                    return this.stateCounter1;
                }

                private set 
                {
                    if (value == this.stateCounter1)
                    {
                        return;
                    }

                    this.stateCounter1 = value;
                    OnPropertyChanged();
                }
            }

            /// <summary>
            /// Получает наименование [файла для записи данных].
            /// </summary>
            /// <value>
            /// Наименование [файла для записи данных].
            /// </value>
            public string RawDataFile
            {
                get
                {
                    return this.rawDataFile;
                }

                private set 
                {
                    if (value == this.rawDataFile)
                    {
                        return;
                    }

                    this.rawDataFile = value;
                    if (string.Empty != value)
                    {
                        this.rawDataStream = new FileStream(value, FileMode.Create, FileAccess.Write, FileShare.None, bufferSize: 4096, useAsync: true);
                    }
                    else
                    {
                        if (null != this.rawDataStream)
                        {
                            try
                            {
                                if (null != this.rawDataTask)
                                {
                                    this.rawDataTask.Wait(WaitForWriteTime);
                                }
                            }
                            finally
                            {
                                this.rawDataStream.Close();
                            }                            
                        }
                    }

                    OnPropertyChanged();
                }
            }

            /// <summary>
            /// Получает или задает значение, показывающее, что [запись данных в файл включена].
            /// </summary>
            /// <value>
            /// <c>true</c> если [запись данных в файл включена]; иначе, <c>false</c>.
            /// </value>
            public bool IsSaveRawData 
            { 
                get
                {
                    return this.isSaveRawData;
                }

                set  
                {
                    if (value == this.isSaveRawData)
                    {
                        return;
                    }

                    this.isSaveRawData = value;
                    if (value)
                    {
                        RawDataFile = Owner.GetNewFileName(Resource.Get(@"stHsiLogName"));                    
                    }
                    else
                    {
                       RawDataFile = string.Empty;
                    }

                    OnPropertyChanged();
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

            public long RawDataFileSize
            {
                get
                {
                    if ((null != RawDataFile) && IsSaveRawData)
                    {
                        return new FileInfo(RawDataFile).Length;
                    }
                    else
                    {
                        return 0;
                    }
                }
            }

            public string RawDataFileSizeFormated
            {
                get
                {
                    return RawDataFileSize.AsFileSizeString();
                }
            }

            /// <summary>
            /// Получает количество выданных кадров по ПК1.
            /// </summary>
            /// <value>
            /// Количество выданных кадров по ПК1.
            /// </value>
            public int FrameCounter1
            {
                get
                {
                    return this.frameCounter1;
                }

                private set 
                {
                    if (value == this.frameCounter1)
                    {
                        return;
                    }

                    this.frameCounter1 = value;
                    OnPropertyChanged();
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
                    return this.stateCounter2;
                }

                private set 
                {
                    if (value == this.stateCounter2)
                    {
                        return;
                    }

                    this.stateCounter2 = value;
                    OnPropertyChanged();
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
                    return this.frameCounter2;
                }

                private set 
                {
                    if (value == this.frameCounter2)
                    { 
                        return;
                    }

                    this.frameCounter2 = value;
                    OnPropertyChanged();
                }
            }

            /// <summary>
            /// Получает или задает линию передачи первого полукомплекта.
            /// </summary>
            /// <value>
            /// Линия передачи первого полукомплекта.
            /// </value>
            public Line IssueLine1
            {
                get
                {
                    return this.line1;
                }

                set 
                {
                    if (value == this.line1)
                    {
                        return;
                    }

                    this.line1 = value;
                    ControlValuesList[Global.Hsi.Line1].SetProperty(Global.Hsi.Line1.Line, Convert.ToInt32(value));
                    OnPropertyChanged();
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
                    return this.isActive1;
                }

                private set 
                {
                    if (value == this.isActive1)
                    {
                        return;
                    }

                    this.isActive1 = value;
                    ControlValuesList[Global.Hsi.State].SetProperty(Global.Hsi.State.Active1, Convert.ToInt32(value), false);                    
                    OnPropertyChanged();
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
                    return this.isActive2;
                }

                private set 
                {
                    if (value == this.isActive2)
                    {
                        return;
                    }

                    this.isActive2 = value;
                    ControlValuesList[Global.Hsi.State].SetProperty(Global.Hsi.State.Active2, Convert.ToInt32(value), false);  
                    OnPropertyChanged();
                }
            }

            /// <summary>
            /// Получает или задает линию передачи второго полукомплекта.
            /// </summary>
            /// <value>
            /// Линия передачи второго полукомплекта.
            /// </value>
            public Line IssueLine2
            {
                get
                {
                    return this.line2;
                }

                set 
                {
                    if (value == this.line2)
                    {
                        return;
                    }

                    this.line2 = value;
                    ControlValuesList[Global.Hsi.Line2].SetProperty(Global.Hsi.Line2.Line, Convert.ToInt32(value));
                    OnPropertyChanged();
                }
            }

            /// <summary>
            /// Получает или задает линию приема.
            /// </summary>
            /// <value>
            /// Линия приема.
            /// </value>
            public SimLine IssueLineRecv
            {
                get
                {
                    return this.lineRecv;
                }

                set 
                {
                    if (value == this.lineRecv)
                    {
                        return;
                    }

                    this.lineRecv = value;
                    ControlValuesList[Global.SimHsi.Control].SetProperty(Global.SimHsi.Control.LineIn, Convert.ToInt32(value));
                    OnPropertyChanged();
                }
            }

            /// <summary>
            /// Получает или задает линию передачи.
            /// </summary>
            /// <value>
            /// Линия передачи.
            /// </value>
            public SimLine IssueLineSend
            {
                get
                {
                    return this.lineSend;
                }

                set 
                {
                    if (value == this.lineSend)
                    {
                        return;
                    }

                    this.lineSend = value;
                    ControlValuesList[Global.SimHsi.Control].SetProperty(Global.SimHsi.Control.LineOut, Convert.ToInt32(value));
                    OnPropertyChanged();
                }
            }

            /// <summary>
            /// Получает или задает значение, показывающее, что [включен опрос данных].
            /// </summary>
            /// <value>
            ///   <c>true</c> если [включен опрос данных]; иначе, <c>false</c>.
            /// </value>
            public bool IsIssuePoll
            {
                get
                {
                    return this.isIssuePoll;
                }

                set 
                {
                    if (value == this.isIssuePoll)
                    {
                        return;
                    }

                    this.isIssuePoll = value;
                    ControlValuesList[Global.SimHsi.Control].SetProperty(Global.SimHsi.Control.IssuePoll, Convert.ToInt32(value));
                    OnPropertyChanged();
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
                    return this.isIssueCmd;
                }

                private set 
                {
                    if (value == this.isIssueCmd)
                    {
                        return;
                    }

                    this.isIssueCmd = value;                   
                    OnPropertyChanged();
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
                    if (this.issueCmdCommand == null)
                    {
                        this.issueCmdCommand = new RelayCommand(obj => { IsIssueCmd = true; ControlValuesList[Global.SimHsi.Record].SetProperty(Global.SimHsi.Record.IssueCmd, 1); }, obj => { return true; });
                    }

                    return this.issueCmdCommand;
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
                    if (this.issueCmdEnable1Command == null)
                    {
                        this.issueCmdEnable1Command = new RelayCommand(obj => { Device.CmdSimHsi1(1); }, obj => { return true; });
                    }

                    return this.issueCmdEnable1Command;
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
                        _issueCmdDisableCommand = new RelayCommand(obj => { Device.CmdSimHsi1(0); }, obj => { return true; });
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
                    if (this.issueCmdEnable2Command == null)
                    {
                        this.issueCmdEnable2Command = new RelayCommand(obj => { Device.CmdSimHsi2(1); }, obj => { return true; });
                    }

                    return this.issueCmdEnable2Command;
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
                    return this.requestStateMain;
                }

                set 
                {
                    if (value == this.requestStateMain)
                    {
                        return;
                    }

                    this.requestStateMain = value;
                    OnPropertyChanged();
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
                    return this.requestStateResv;
                }

                set 
                {
                    if (value == this.requestStateResv)
                    {
                        return;
                    }

                    this.requestStateResv = value;
                    OnPropertyChanged();
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
                    return this.requestDataMain;
                }

                set 
                {
                    if (value == this.requestDataMain)
                    {
                        return;
                    }

                    this.requestDataMain = value;
                    OnPropertyChanged();
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
                    return this.requestDataResv;
                }

                set 
                {
                    if (value == this.requestDataResv)
                    {
                        return;
                    }

                    this.requestDataResv = value;
                    OnPropertyChanged();
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
                    return this.cmdCounter1;
                }

                set 
                {
                    if (value == this.cmdCounter1)
                    { 
                        return; 
                    }

                    this.cmdCounter1 = value;
                    OnPropertyChanged();
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
                    return this.cmdCounter2;
                }

                set 
                {
                    if (value == this.cmdCounter2)
                    {
                        return;
                    }

                    this.cmdCounter2 = value;
                    OnPropertyChanged();
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

            public override void UpdateProperties()
            {
                OnPropertyChanged(() => this.TxtDataFileSizeFormated);
            }

            /// <summary>
            /// Serializes the specified object.
            /// </summary>
            /// <param name="obj">The object.</param>
            public override void Serialize(object obj = null)
            {
                base.Serialize(this);
            }

            /// <summary>
            /// Deserializes this instance.
            /// </summary>
            public override void Deserialize()
            {
                base.Deserialize();
            }

            /// <summary>
            /// Вызов диалога "Открыть файл".
            /// </summary>
            /// <param name="obj">The object.</param>
            public void OpenFromFile(object obj)
            {
                Data = Owner.OpenFromFile();
                OnPropertyChanged(() => this.Data);
            }

            /// <summary>
            /// Вызывается когда [требуется записать данные сообщения в файл].
            /// </summary>
            /// <param name="sender">The sender.</param>
            /// <param name="e">The <see cref="HsiMsgEventArgs"/> instance containing the event data.</param>
            public virtual void OnHsiMsgRawSave(object sender, HsiMsgEventArgs e)
            {
                if (null != this.rawDataStream)
                {
                    if (null != this.rawDataTask)
                    {
                        this.rawDataTask.Wait(WaitForWriteTime);
                    }

                    if (this.rawDataStream.CanWrite)
                    {
                        this.rawDataTask = this.rawDataStream.WriteAsync(e.Data, 0, e.Data.Length);
                        OnPropertyChanged(() => this.RawDataFileSizeFormated);
                    }
                }
            }

            /// <summary>
            /// Добавляет отправленную команду(данные) в список.
            /// </summary>
            internal void DataToSaveList()
            {
                string str = Converter.ByteArrayToHexStr(Data);
                if (!DataList.Contains(str))
                {
                    if (DataList.Count >= MaxDataListCount)
                    {
                        DataList.RemoveAt(DataList.Count - 1);
                    }

                    DataList.Insert(0, str);
                }

                OnPropertyChanged(() => this.DataList);
            }

            /// <summary>
            /// Initializes the control value.
            /// </summary>
            protected override void InitControlValue()
            {
                ControlValuesList.Add(Global.Hsi.Line1, new ControlValue());
                ControlValuesList.Add(Global.Hsi.Line1StateCounter, new ControlValue());
                ControlValuesList.Add(Global.Hsi.Line1FrameCounter, new ControlValue());
                ControlValuesList.Add(Global.Hsi.Line2, new ControlValue());
                ControlValuesList.Add(Global.Hsi.Line2StateCounter, new ControlValue());
                ControlValuesList.Add(Global.Hsi.Line2FrameCounter, new ControlValue());
                ControlValuesList.Add(Global.SimHsi.Control, new ControlValue());
                ControlValuesList.Add(Global.SimHsi.Record, new ControlValue());
                ControlValuesList.Add(Global.Hsi.State, new ControlValue());
            }

            /// <summary>
            /// Initializes the properties.
            /// </summary>
            protected override void InitProperties()
            {
                ControlValuesList[Global.Hsi.Line1].AddProperty(Global.Hsi.Line1.Line, 1, 2, Device.CmdHsiLine1, value => IssueLine1 = (Line)value);
                ControlValuesList[Global.Hsi.Line1].AddProperty(Global.Hsi.Line1.IssueEnable, 0, 1, Device.CmdHsiLine1, value => IsIssueEnable1 = 1 == value);
                ControlValuesList[Global.Hsi.Line2].AddProperty(Global.Hsi.Line2.Line, 1, 2, Device.CmdHsiLine2, value => IssueLine2 = (Line)value);
                ControlValuesList[Global.Hsi.Line2].AddProperty(Global.Hsi.Line2.IssueEnable, 0, 1, Device.CmdHsiLine2, value => IsIssueEnable2 = 1 == value);
                ControlValuesList[Global.Hsi.Line1StateCounter].AddProperty(Global.Hsi.Line1StateCounter, 0, 32, delegate { }, value => StateCounter1 = value, true);
                ControlValuesList[Global.Hsi.Line1FrameCounter].AddProperty(Global.Hsi.Line1FrameCounter, 0, 32, delegate { }, value => FrameCounter1 = value, true);
                ControlValuesList[Global.Hsi.Line2StateCounter].AddProperty(Global.Hsi.Line2StateCounter, 0, 32, delegate { }, value => StateCounter2 = value, true);
                ControlValuesList[Global.Hsi.Line2FrameCounter].AddProperty(Global.Hsi.Line2FrameCounter, 0, 32, delegate { }, value => FrameCounter2 = value, true);
                ControlValuesList[Global.SimHsi.Control].AddProperty(Global.SimHsi.Control.LineIn, 2, 1, Device.CmdSimHsiControl, value => IssueLineRecv = (SimLine)value);
                ControlValuesList[Global.SimHsi.Control].AddProperty(Global.SimHsi.Control.LineOut, 1, 1, Device.CmdSimHsiControl, value => IssueLineSend = (SimLine)value);
                ControlValuesList[Global.SimHsi.Control].AddProperty(Global.SimHsi.Control.IssuePoll, 0, 1, Device.CmdSimHsiControl, value => IsIssuePoll = 1 == value);
                ControlValuesList[Global.SimHsi.Record].AddProperty(Global.SimHsi.Record.IssueCmd, 0, 1, Device.CmdSimHsiRecord, value => IsIssueCmd = 1 == value);

                ControlValuesList[Global.Hsi.State].AddProperty(Global.Hsi.State.Active1, 6, 1, delegate { }, value => IsActive1 = 1 == value, true);
                ControlValuesList[Global.Hsi.State].AddProperty(Global.Hsi.State.Active2, 7, 1, delegate { }, value => IsActive2 = 1 == value, true);
                ControlValuesList[Global.Hsi.State].AddProperty(Global.Hsi.State.IssueReady1, 0, 1, Device.CmdHsiState, value => IsIssueReady1 = 1 == value);
                ControlValuesList[Global.Hsi.State].AddProperty(Global.Hsi.State.IssueReady2, 3, 1, Device.CmdHsiState, value => IsIssueReady2 = 1 == value);
                ControlValuesList[Global.Hsi.State].AddProperty(Global.Hsi.State.IssueBusy1, 1, 1, Device.CmdHsiState, value => IsIssueBusy1 = 1 == value);
                ControlValuesList[Global.Hsi.State].AddProperty(Global.Hsi.State.IssueBusy2, 4, 1, Device.CmdHsiState, value => IsIssueBusy2 = 1 == value);
                ControlValuesList[Global.Hsi.State].AddProperty(Global.Hsi.State.IssueMe1, 2, 1, Device.CmdHsiState, value => IsIssueMe1 = 1 == value);
                ControlValuesList[Global.Hsi.State].AddProperty(Global.Hsi.State.IssueMe2, 5, 1, Device.CmdHsiState, value => IsIssueMe2 = 1 == value);
            }
        }

        /// <summary>
        /// Вспомогательный нотификатор, используется для самопроверки.
        /// </summary>
        public class UITest : SubNotify
        {
            /// <summary>
            /// Путь к usb-лог файлу.
            /// </summary>
            private string usbLogFile;

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
                    return this.usbLogFile;
                }

                set 
                {
                    if (value == this.usbLogFile)
                    {
                        return;
                    }

                    this.usbLogFile = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Нотификатор телеметрии.
        /// </summary>
        [Serializable]
        public class Telemetry : SubNotify, IDataErrorInfo
        {
            /// <summary>
            /// Передавать релейные команды БУСК по линии A.
            /// </summary>
            private bool isBuskLineA = true;

            /// <summary>
            /// Передавать релейные команды БУСК по линии B.
            /// </summary>
            private bool isBuskLineB = true;

            /// <summary>
            /// Передавать релейные команды БУНД по линии A.
            /// </summary>
            private bool isBundLineA = true;

            /// <summary>
            /// Передавать релейные команды БУНД по линии B.
            /// </summary>
            private bool isBundLineB = true;

            /// <summary>
            /// Телеметрия: Запитан ПК1 от БУСК.
            /// </summary>
            private bool isPowerBusk1;

            /// <summary>
            /// Телеметрия: Запитан ПК2 от БУСК.
            /// </summary>
            private bool isPowerBusk2;

            /// <summary>
            /// Телеметрия: Запитан ПК1 от БУНД.
            /// </summary>
            private bool powerBund1;

            /// <summary>
            /// Телеметрия: Запитан ПК2 от БУНД.
            /// </summary>
            private bool isPowerBund2;

            /// <summary>
            /// Телеметрия: Питание УФЭС ОСН.
            /// </summary>
            private bool ufesPower1;

            /// <summary>
            /// Телеметрия: Питание УФЭС РЕЗ.
            /// </summary>
            private bool ufesPower2;

            /// <summary>
            /// Телеметрия: Питание ВУФЭС ОСН.
            /// </summary>
            private bool vufesPower1;

            /// <summary>
            /// Телеметрия: Питание ВУФЭС РЕЗ.
            /// </summary>
            private bool vufesPower2;

            /// <summary>
            /// Телеметрия: Питание СДЩ ОСН.
            /// </summary>
            private bool sdshPower1;

            /// <summary>
            /// Телеметрия: Питание СДЩ РЕЗ.
            /// </summary>
            private bool sdshPower2;

            /// <summary>
            /// Телеметрия: Подсветка УФЭС ОСН.
            /// </summary>
            private bool ufesLight1;

            /// <summary>
            /// Телеметрия: Подсветка УФЭС РЕЗ.
            /// </summary>
            private bool ufesLight2;

            /// <summary>
            /// Телеметрия: Подсветка ВУФЭС ОСН.
            /// </summary>
            private bool vufesLight1;

            /// <summary>
            /// Телеметрия: Подсветка ВУФЭС РЕЗ.
            /// </summary>
            private bool vufesLight2;

            /// <summary>
            /// Телеметрия: Подсветка СДЩ ОСН.
            /// </summary>
            private bool sdshLight1;

            /// <summary>
            /// Телеметрия: Подсветка СДЩ РЕЗ.
            /// </summary>
            private bool sdshLight2;

            /// <summary>
            /// Телеметрия: Затвор УФЭС ОСН.
            /// </summary>
            private bool ufesLock1;

            /// <summary>
            /// Телеметрия: Затвор УФЭС РЕЗ.
            /// </summary>
            private bool ufesLock2;

            /// <summary>
            /// Телеметрия: Затвор ВУФЭС ОСН.
            /// </summary>
            private bool vufesLock1;

            /// <summary>
            /// Телеметрия: Затвор ВУФЭС РЕЗ.
            /// </summary>
            private bool vufesLock2;

            /// <summary>
            /// Телеметрия: Затвор СДЩ ОСН.
            /// </summary>
            private bool sdshLock1;

            /// <summary>
            /// Телеметрия: Затвор СДЩ РЕЗ.
            /// </summary>
            private bool sdshLock2;

            /// <summary>
            /// Экземпляр команды на [выдачу питания БУСК ПК1].
            /// </summary>
            [field: NonSerialized]
            private ICommand issuePowerBusk1Command;

            /// <summary>
            /// Экземпляр команды на [выдачу питания БУСК ПК2].
            /// </summary>
            [field: NonSerialized]
            private ICommand issuePowerBusk2Command;

            /// <summary>
            /// Экземпляр команды на [выдачу питания БУНД ПК1].
            /// </summary>
            [field: NonSerialized]
            private ICommand issuePowerBund1Command;

            /// <summary>
            /// Экземпляр команды на [выдачу питания БУНД ПК2].
            /// </summary>
            [field: NonSerialized]
            private ICommand issuePowerBund2Command;

            /// <summary>
            /// Инициализирует новый экземпляр класса <see cref="Telemetry" />.
            /// </summary>
            /// <param name="owner">The owner.</param>
            public Telemetry(EgseBukNotify owner)
                : base(owner)
            {
            }

            /// <summary>
            /// Получает или задает значение, показывающее, что [передавать релейную команду БУСК необходимо по линии А].
            /// </summary>
            /// <value>
            /// <c>true</c> если [передавать релейную команду БУСК необходимо по линии А]; иначе, <c>false</c>.
            /// </value>
            public bool IsBuskLineA
            {
                get
                {
                    return this.isBuskLineA;
                }

                set 
                {
                    if (value == this.isBuskLineA)
                    {
                        return;
                    }

                    if (!value && !this.isBuskLineB)
                    {
                        return;
                    }

                    this.isBuskLineA = value;
                    OnPropertyChanged();
                }
            }

            /// <summary>
            /// Получает или задает значение, показывающее, что [передавать релейную команду БУСК необходимо по линии Б].
            /// </summary>
            /// <value>
            /// <c>true</c> если [передавать релейную команду БУСК необходимо по линии Б]; иначе, <c>false</c>.
            /// </value>
            public bool IsBuskLineB
            {
                get
                {
                    return this.isBuskLineB;
                }

                set 
                {
                    if (value == this.isBuskLineB)
                    {
                        return;
                    }

                    if (!value && !this.isBuskLineA)
                    {
                        return;
                    }

                    this.isBuskLineB = value;
                    OnPropertyChanged();
                }
            }

            /// <summary>
            /// Получает или задает значение, показывающее, что [передавать релейную команду БУНД необходимо по линии А].
            /// </summary>
            /// <value>
            /// <c>true</c> если [передавать релейную команду БУНД необходимо по линии А]; иначе, <c>false</c>.
            /// </value>
            public bool IsBundLineA
            {
                get
                {
                    return this.isBundLineA;
                }

                set 
                {
                    if (value == this.isBundLineA)
                    {
                        return;
                    }

                    if (!value && !this.isBundLineB)
                    {
                        return;
                    }

                    this.isBundLineA = value;
                    OnPropertyChanged();
                }
            }

            /// <summary>
            /// Получает или задает значение, показывающее, что [передавать релейную команду БУНД необходимо по линии Б].
            /// </summary>
            /// <value>
            /// <c>true</c> если [передавать релейную команду БУНД необходимо по линии Б]; иначе, <c>false</c>.
            /// </value>
            public bool IsBundLineB
            {
                get
                {
                    return this.isBundLineB;
                }

                set 
                {
                    if (value == this.isBundLineB)
                    {
                        return;
                    }

                    if (!value && !this.isBundLineA)
                    {
                        return;
                    }

                    this.isBundLineB = value;
                    OnPropertyChanged();
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
                    return this.isPowerBusk1;
                }

                private set 
                {
                    if (value == this.isPowerBusk1)
                    {
                        return;
                    }

                    this.isPowerBusk1 = value;
                    ControlValuesList[Global.Telemetry].SetProperty(Global.Telemetry.PowerBund1, Convert.ToInt32(!value), false);
                    OnPropertyChanged();
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
                    if (null == this.issuePowerBusk1Command)
                    {
                        this.issuePowerBusk1Command = new RelayCommand(obj => { ControlValuesList[Global.Telemetry].SetProperty(Global.Telemetry.PowerBusk1, Convert.ToInt32(!IsPowerBusk1)); IsPowerBusk1 = !IsPowerBusk1; }, obj => { return true; });
                    }

                    return this.issuePowerBusk1Command;
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
                    return this.isPowerBusk2;
                }

                private set 
                {
                    if (value == this.isPowerBusk2)
                    {
                        return;
                    }

                    this.isPowerBusk2 = value;
                    ControlValuesList[Global.Telemetry].SetProperty(Global.Telemetry.PowerBund2, Convert.ToInt32(!value), false);
                    OnPropertyChanged();
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
                    if (null == this.issuePowerBusk2Command)
                    {
                        this.issuePowerBusk2Command = new RelayCommand(obj => { ControlValuesList[Global.Telemetry].SetProperty(Global.Telemetry.PowerBusk2, Convert.ToInt32(!IsPowerBusk2)); IsPowerBusk2 = !IsPowerBusk2; }, obj => { return true; });
                    }

                    return this.issuePowerBusk2Command;
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
                    return this.powerBund1;
                }

                private set 
                {
                    if (value == this.powerBund1)
                    {
                        return;
                    }

                    this.powerBund1 = value;
                    ControlValuesList[Global.Telemetry].SetProperty(Global.Telemetry.PowerBusk1, Convert.ToInt32(!value), false);
                    OnPropertyChanged();
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
                    if (null == this.issuePowerBund1Command)
                    {
                        this.issuePowerBund1Command = new RelayCommand(obj => { ControlValuesList[Global.Telemetry].SetProperty(Global.Telemetry.PowerBund1, Convert.ToInt32(!IsPowerBund1)); IsPowerBund1 = !IsPowerBund1; }, obj => { return true; });
                    }

                    return this.issuePowerBund1Command;
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
                    return this.isPowerBund2;
                }

                private set 
                {
                    if (value == this.isPowerBund2)
                    {
                        return;
                    }

                    this.isPowerBund2 = value;
                    ControlValuesList[Global.Telemetry].SetProperty(Global.Telemetry.PowerBusk2, Convert.ToInt32(!value), false);
                    OnPropertyChanged();
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
                    if (null == this.issuePowerBund2Command)
                    {
                        this.issuePowerBund2Command = new RelayCommand(obj => { ControlValuesList[Global.Telemetry].SetProperty(Global.Telemetry.PowerBund2, Convert.ToInt32(!IsPowerBund2)); IsPowerBund2 = !IsPowerBund2; }, obj => { return true; });
                    }

                    return this.issuePowerBund2Command;
                }
            }

            /// <summary>
            /// Получает значение, показывающее, включен ли [затвор прибора УФЭС ОСН].
            /// </summary>
            /// <value>
            ///   <c>true</c> если [затвор прибора УФЭС ОСН] включен; иначе, <c>false</c>.
            /// </value>
            public bool UfesLock1 
            { 
                get
                {
                    return this.ufesLock1;
                }

                private set 
                {
                    if (value == this.ufesLock1)
                    {
                        return;
                    }

                    this.ufesLock1 = value;
                    ControlValuesList[Global.Telemetry].SetProperty(Global.Telemetry.UfesLock1, Convert.ToInt32(value));
                    OnPropertyChanged();
                }
            }

            /// <summary>
            /// Получает значение, показывающее, включен ли [затвор прибора УФЭС РЕЗ].
            /// </summary>
            /// <value>
            ///   <c>true</c> если [затвор прибора УФЭС РЕЗ] включен; иначе, <c>false</c>.
            /// </value>
            public bool UfesLock2
            {
                get
                {
                    return this.ufesLock2;
                }

                private set 
                {
                    if (value == this.ufesLock2)
                    {
                        return;
                    }

                    this.ufesLock2 = value;
                    ControlValuesList[Global.Telemetry].SetProperty(Global.Telemetry.UfesLock2, Convert.ToInt32(value));
                    OnPropertyChanged();
                }
            }

            /// <summary>
            /// Получает значение, показывающее, включен ли [затвор прибора ВУФЭС ОСН].
            /// </summary>
            /// <value>
            ///   <c>true</c> если [затвор прибора ВУФЭС ОСН] включен; иначе, <c>false</c>.
            /// </value>
            public bool VufesLock1
            {
                get
                {
                    return this.vufesLock1;
                }

                private set 
                {
                    if (value == this.vufesLock1)
                    {
                        return;
                    }

                    this.vufesLock1 = value;
                    ControlValuesList[Global.Telemetry].SetProperty(Global.Telemetry.VufesLock1, Convert.ToInt32(value));
                    OnPropertyChanged();
                }
            }

            /// <summary>
            /// Получает значение, показывающее, включен ли [затвор прибора ВУФЭС РЕЗ].
            /// </summary>
            /// <value>
            ///   <c>true</c> если [затвор прибора ВУФЭС РЕЗ] включен; иначе, <c>false</c>.
            /// </value>
            public bool VufesLock2
            {
                get
                {
                    return this.vufesLock2;
                }

                private set 
                {
                    if (value == this.vufesLock2)
                    {
                        return;
                    }

                    this.vufesLock2 = value;
                    ControlValuesList[Global.Telemetry].SetProperty(Global.Telemetry.VufesLock2, Convert.ToInt32(value));
                    OnPropertyChanged();
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
                    return this.sdshLock1;
                }

                private set 
                {
                    if (value == this.sdshLock1)
                    {
                        return;
                    }

                    this.sdshLock1 = value;
                    ControlValuesList[Global.Telemetry].SetProperty(Global.Telemetry.SdchshLock1, Convert.ToInt32(value));
                    OnPropertyChanged();
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
                    return this.sdshLock2;
                }

                private set 
                {
                    if (value == this.sdshLock2)
                    {
                        return;
                    }

                    this.sdshLock2 = value;
                    ControlValuesList[Global.Telemetry].SetProperty(Global.Telemetry.SdchshLock2, Convert.ToInt32(value));
                    OnPropertyChanged();
                }
            }

            /// <summary>
            /// Получает значение, показывающее, есть ли [питание УФЭС ОСН].
            /// </summary>
            /// <value>
            ///   <c>true</c> если [питание УФЭС ОСН]; иначе, <c>false</c>.
            /// </value>
            public bool UfesPower1
            {
                get
                {
                    return this.ufesPower1;
                }

                private set 
                {
                    if (value == this.ufesPower1)
                    {
                        return;
                    }

                    this.ufesPower1 = value;
                    ControlValuesList[Global.Telemetry].SetProperty(Global.Telemetry.UfesPower1, Convert.ToInt32(value), false);
                    OnPropertyChanged();
                }
            }

            /// <summary>
            /// Получает значение, показывающее, есть ли [питание УФЭС РЕЗ].
            /// </summary>
            /// <value>
            ///   <c>true</c> если [питание УФЭС РЕЗ]; иначе, <c>false</c>.
            /// </value>
            public bool UfesPower2
            {
                get
                {
                    return this.ufesPower2;
                }

                private set 
                {
                    if (value == this.ufesPower2)
                    {
                        return;
                    }

                    this.ufesPower2 = value;
                    ControlValuesList[Global.Telemetry].SetProperty(Global.Telemetry.UfesPower2, Convert.ToInt32(value), false);
                    OnPropertyChanged();
                }
            }

            /// <summary>
            /// Получает значение, показывающее, есть ли [питание ВУФЭС ОСН].
            /// </summary>
            /// <value>
            ///   <c>true</c> если [питание ВУФЭС ОСН]; иначе, <c>false</c>.
            /// </value>
            public bool VufesPower1
            {
                get
                {
                    return this.vufesPower1;
                }

                private set 
                {
                    if (value == this.vufesPower1)
                    {
                        return;
                    }

                    this.vufesPower1 = value;
                    ControlValuesList[Global.Telemetry].SetProperty(Global.Telemetry.VufesPower1, Convert.ToInt32(value), false);
                    OnPropertyChanged();
                }
            }

            /// <summary>
            /// Получает значение, показывающее, есть ли [питание ВУФЭС РЕЗ].
            /// </summary>
            /// <value>
            ///   <c>true</c> если [питание ВУФЭС РЕЗ]; иначе, <c>false</c>.
            /// </value>
            public bool VufesPower2
            {
                get
                {
                    return this.vufesPower2;
                }

                private set 
                {
                    if (value == this.vufesPower2)
                    {
                        return;
                    }

                    this.vufesPower2 = value;
                    ControlValuesList[Global.Telemetry].SetProperty(Global.Telemetry.VufesPower2, Convert.ToInt32(value), false);
                    OnPropertyChanged();
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
                    return this.sdshPower1;
                }

                private set 
                {
                    if (value == this.sdshPower1)
                    {
                        return;
                    }

                    this.sdshPower1 = value;
                    ControlValuesList[Global.Telemetry].SetProperty(Global.Telemetry.SdchshPower1, Convert.ToInt32(value), false);
                    OnPropertyChanged();
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
                    return this.sdshPower2;
                }

                private set 
                {
                    if (value == this.sdshPower2)
                    {
                        return;
                    }

                    this.sdshPower2 = value;
                    ControlValuesList[Global.Telemetry].SetProperty(Global.Telemetry.SdchshPower2, Convert.ToInt32(value), false);
                    OnPropertyChanged();
                }
            }

            /// <summary>
            /// Получает значение, показывающее, что [подсветка УФЭС включена (сигнал по основному каналу)].
            /// </summary>
            /// <value>
            /// <c>true</c> если [подсветка УФЭС включена (сигнал по основному каналу)]; иначе, <c>false</c>.
            /// </value>
            public bool UfesLight1
            {
                get
                {
                    return this.ufesLight1;
                }

                private set 
                {
                    if (value == this.ufesLight1)
                    {
                        return;
                    }

                    this.ufesLight1 = value;
                    ControlValuesList[Global.Telemetry].SetProperty(Global.Telemetry.UfesLight1, Convert.ToInt32(value), false);
                    OnPropertyChanged();
                }
            }

            /// <summary>
            /// Получает значение, показывающее, что [подсветка УФЭС включена (сигнал по резервному каналу)].
            /// </summary>
            /// <value>
            /// <c>true</c> если [подсветка УФЭС включена (сигнал по резервному каналу)]; иначе, <c>false</c>.
            /// </value>
            public bool UfesLight2
            {
                get
                {
                    return this.ufesLight2;
                }

                private set 
                {
                    if (value == this.ufesLight2)
                    {
                        return;
                    }

                    this.ufesLight2 = value;
                    ControlValuesList[Global.Telemetry].SetProperty(Global.Telemetry.UfesLight2, Convert.ToInt32(value), false);
                    OnPropertyChanged();
                }
            }

            /// <summary>
            /// Получает значение, показывающее, что [подсветка ВУФЭС включена (сигнал по основному каналу)].
            /// </summary>
            /// <value>
            /// <c>true</c> если [подсветка ВУФЭС включена (сигнал по основному каналу)]; иначе, <c>false</c>.
            /// </value>
            public bool VufesLight1
            {
                get
                {
                    return this.vufesLight1;
                }

                private set 
                {
                    if (value == this.vufesLight1)
                    {
                        return;
                    }

                    this.vufesLight1 = value;
                    ControlValuesList[Global.Telemetry].SetProperty(Global.Telemetry.VufesLight1, Convert.ToInt32(value), false);
                    OnPropertyChanged();
                }
            }
            
            /// <summary>
            /// Получает значение, показывающее, что [подсветка ВУФЭС включена (сигнал по резервному каналу)].
            /// </summary>
            /// <value>
            /// <c>true</c> если [подсветка ВУФЭС включена (сигнал по резервному каналу)]; иначе, <c>false</c>.
            /// </value>
            public bool VufesLight2
            {
                get
                {
                    return this.vufesLight2;
                }

                private set 
                {
                    if (value == this.vufesLight2)
                    {
                        return;
                    }

                    this.vufesLight2 = value;
                    ControlValuesList[Global.Telemetry].SetProperty(Global.Telemetry.VufesLight2, Convert.ToInt32(value), false);
                    OnPropertyChanged();
                }
            }

            /// <summary>
            /// Получает значение, показывающее, что [подсветка СДЩ включена (сигнал по основному каналу)].
            /// </summary>
            /// <value>
            /// <c>true</c> если [подсветка СДЩ включена (сигнал по основному каналу)]; иначе, <c>false</c>.
            /// </value>
            public bool SdchshLight1
            {
                get
                {
                    return this.sdshLight1;
                }

                private set 
                {
                    if (value == this.sdshLight1)
                    {
                        return;
                    }

                    this.sdshLight1 = value;
                    ControlValuesList[Global.Telemetry].SetProperty(Global.Telemetry.SdshLight1, Convert.ToInt32(value), false);
                    OnPropertyChanged();
                }
            }

            /// <summary>
            /// Получает значение, показывающее, что [подсветка СДЩ включена (сигнал по резервному каналу)].
            /// </summary>
            /// <value>
            /// <c>true</c> если [подсветка СДЩ включена (сигнал по резервному каналу)]; иначе, <c>false</c>.
            /// </value>
            public bool SdchshLight2
            {
                get
                {
                    return this.sdshLight2;
                }

                private set 
                {
                    if (value == this.sdshLight2)
                    {
                        return;
                    }

                    this.sdshLight2 = value;
                    ControlValuesList[Global.Telemetry].SetProperty(Global.Telemetry.SdshLight2, Convert.ToInt32(value), false);
                    OnPropertyChanged();
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
            /// Serializes the specified object.
            /// </summary>
            /// <param name="obj">The object.</param>
            public override void Serialize(object obj = null)
            {
                base.Serialize(this);
            }

            /// <summary>
            /// Deserializes this instance.
            /// </summary>
            public override void Deserialize()
            {
                base.Deserialize();
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
                ControlValuesList[Global.Telemetry].AddProperty(Global.Telemetry.UfesLight1, 10, 1, delegate { }, value => UfesLight1 = 1 == value, true);
                ControlValuesList[Global.Telemetry].AddProperty(Global.Telemetry.UfesLight2, 9, 1, delegate { }, value => UfesLight2 = 1 == value, true);
                ControlValuesList[Global.Telemetry].AddProperty(Global.Telemetry.VufesLight1, 8, 1, delegate { }, value => VufesLight1 = 1 == value, true);
                ControlValuesList[Global.Telemetry].AddProperty(Global.Telemetry.VufesLight2, 7, 1, delegate { }, value => VufesLight2 = 1 == value, true);
                ControlValuesList[Global.Telemetry].AddProperty(Global.Telemetry.SdshLight1, 6, 1, delegate { }, value => SdchshLight1 = 1 == value, true);
                ControlValuesList[Global.Telemetry].AddProperty(Global.Telemetry.SdshLight2, 11, 1, delegate { }, value => SdchshLight2 = 1 == value, true);
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
        [Serializable]
        public class Spacewire1 : SubNotify, IDataErrorInfo
        {
            /// <summary>
            /// Управление: вкл/выкл интерфейса Spacewire.
            /// </summary>
            private bool isIssueEnable;

            /// <summary>
            /// Управление: Установлена связь.
            /// </summary>
            private bool isConnect;

            /// <summary>
            /// SPTP: включение обмена прибора НП1.
            /// </summary>
            private bool isIssueExchange;

            /// <summary>
            /// SPTP: можно выдавать пакет в НП1.
            /// </summary>
            private bool isSD1TransData;

            /// <summary>
            /// SPTP: Адрес БУСК.
            /// </summary>
            private int logicBusk = 0;

            /// <summary>
            /// SPTP: Адрес НП1.
            /// </summary>
            private int logicSD1 = 0;

            /// <summary>
            /// SPTP: Счетчик миллисекунд для НП1 (через сколько готовы данные).
            /// </summary>
            private int sd1SendTime = 0;

            /// <summary>
            /// SPTP: Кол-во байт в пакете НП1.
            /// </summary>
            private int sd1DataSize = 0;

            /// <summary>
            /// Запись данных(до 1 Кбайт): Бит занятости.
            /// </summary>
            private bool isRecordBusy;

            /// <summary>
            /// Запись данных(до 1 Кбайт): Бит выдачи посылки.
            /// </summary>
            private bool isRecordSend;

            /// <summary>
            /// Экземпляр команды на [включение интерфейса spacewire].
            /// </summary>
            [field: NonSerialized]
            private ICommand _issueEnableCommand;

            /// <summary>
            /// Экземпляр команды на [выдачу посылки по интерфейсу spacewire].
            /// </summary>
            [field: NonSerialized]
            private ICommand _issuePackageCommand;

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
            /// Получает или задает [логический адрес БУСК].
            /// </summary>
            /// <value>
            /// Значение [логический адрес БУСК].
            /// </value>
            public int LogicBusk
            {
                get
                {
                    return this.logicBusk;
                }

                set 
                {
                    if (value == this.logicBusk)
                    {
                        return;
                    }

                    this.logicBusk = value;
                    ControlValuesList[Global.Spacewire1.BuskLogic].SetProperty(Global.Spacewire1.BuskLogic, value);
                    OnPropertyChanged();
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
                    return this.logicSD1;
                }

                set 
                {
                    if (value == this.logicSD1)
                    {
                        return;
                    }

                    this.logicSD1 = value;
                    ControlValuesList[Global.Spacewire1.SD1Logic].SetProperty(Global.Spacewire1.SD1Logic, value);
                    OnPropertyChanged();                  
                }
            }

            /// <summary>
            /// Получает или задает значение, показывающее, что [интерфейс включен].
            /// </summary>
            /// <value>
            /// <c>true</c> если [интерфейс включен]; иначе, <c>false</c>.
            /// </value>
            public bool IsIssueEnable
            {
                get
                {
                    return this.isIssueEnable;
                }

                set 
                {
                    if (value == this.isIssueEnable)
                    {
                        return;
                    }

                    this.isIssueEnable = value;
                    ControlValuesList[Global.Spacewire1.Control].SetProperty(Global.Spacewire1.Control.IssueEnable, Convert.ToInt32(value));
                    ControlValuesList[Global.Spacewire2.Control].SetProperty(Global.Spacewire2.Control.Connect, Convert.ToInt32(0));
                    OnPropertyChanged();
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
            /// Получает значение, показывающее, что [связь по интерфейсу установлена].
            /// </summary>
            /// <value>
            /// <c>true</c> если [связь по интерфейсу установлена]; иначе, <c>false</c>.
            /// </value>
            public bool IsConnect
            {
                get
                {
                    return this.isConnect;
                }

                private set 
                {
                    if (value == this.isConnect)
                    {
                        return;
                    }

                    this.isConnect = value;
                    ControlValuesList[Global.Spacewire1.Control].SetProperty(Global.Spacewire1.Control.Connect, Convert.ToInt32(value));
                    OnPropertyChanged();
                }
            }

            /// <summary>
            /// Получает или задает значение, показывающее, что [включен обмен для имитатора детектора].
            /// </summary>
            /// <value>
            /// <c>true</c> если [включен обмен для имитатора детектора]; иначе, <c>false</c>.
            /// </value>
            public bool IsIssueExchange
            {
                get
                {
                    return this.isIssueExchange;
                }

                set 
                {
                    if (value == this.isIssueExchange)
                    {
                        return;
                    }

                    this.isIssueExchange = value;
                    ControlValuesList[Global.Spacewire1.SPTPControl].SetProperty(Global.Spacewire1.SPTPControl.SD1Trans, Convert.ToInt32(value));
                    OnPropertyChanged();
                }
            }

            /// <summary>
            /// Получает или задает значение, показывающее, что [необходимо выдавать пакеты данных из имитатора детекторов].
            /// </summary>
            /// <value>
            /// <c>true</c> если [необходимо выдавать пакеты данных из имитатора детекторов]; иначе, <c>false</c>.
            /// </value>
            public bool IsSD1TransData
            {
                get
                {
                    return this.isSD1TransData;
                }

                set 
                {
                    if (value == this.isSD1TransData)
                    {
                        return;
                    }

                    this.isSD1TransData = value;
                    ControlValuesList[Global.Spacewire1.SPTPControl].SetProperty(Global.Spacewire1.SPTPControl.SD1TransData, Convert.ToInt32(value));
                    OnPropertyChanged();
                }
            }

            /// <summary>
            /// Получает или задает значение [счетчика миллисекунд для имитатора детекторов (через сколько готовы данные)].
            /// </summary>
            /// <value>
            /// Значение [счетчика миллисекунд для имитатора детекторов (через сколько готовы данные)].
            /// </value>
            public int SD1SendTime
            {
                get
                {
                    return this.sd1SendTime;
                }

                set 
                {
                    if (value == this.sd1SendTime)
                    {
                        return;
                    }

                    this.sd1SendTime = value;
                    ControlValuesList[Global.Spacewire1.SD1SendTime].SetProperty(Global.Spacewire1.SD1SendTime, Convert.ToInt32(value));
                    OnPropertyChanged();
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
                    return this.sd1DataSize;
                }

                private set 
                {
                    if (value == this.sd1DataSize)
                    {
                        return;
                    }

                    this.sd1DataSize = value;
                    ControlValuesList[Global.Spacewire1.SD1DataSize].SetProperty(Global.Spacewire1.SD1DataSize, Convert.ToInt32(value));
                    OnPropertyChanged();
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
                    return this.isRecordBusy || IsIssuePackage;
                }

                private set 
                {
                    if (value == this.isRecordBusy)
                    { 
                        return;
                    }

                    this.isRecordBusy = value;
                    OnPropertyChanged();
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
                    return this.isRecordSend;
                }

                private set 
                {
                    if (value == this.isRecordSend)
                    {
                        return;
                    }

                    this.isRecordSend = value;
                    OnPropertyChanged();
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
            /// Serializes the specified object.
            /// </summary>
            /// <param name="obj">The object.</param>
            public override void Serialize(object obj = null)
            {
                base.Serialize(this);
            }

            /// <summary>
            /// Deserializes this instance.
            /// </summary>
            public override void Deserialize()
            {
                base.Deserialize();
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

                ControlValuesList[Global.Spacewire1.SPTPControl].AddProperty(Global.Spacewire1.SPTPControl.SD1Trans, 0, 1, Device.CmdSpacewire1ControlSPTP, value => IsIssueExchange = 1 == value);
                ControlValuesList[Global.Spacewire1.SPTPControl].AddProperty(Global.Spacewire1.SPTPControl.SD1TransData, 1, 1, Device.CmdSpacewire1ControlSPTP, value => IsSD1TransData = 1 == value);

                ControlValuesList[Global.Spacewire1.SD1SendTime].AddProperty(Global.Spacewire1.SD1SendTime, 0, 16, Device.CmdSpacewire1SPTPControlSD1SendTime, value => SD1SendTime = value);
                ControlValuesList[Global.Spacewire1.SD1DataSize].AddProperty(Global.Spacewire1.SD1DataSize, 0, 16, Device.CmdSpacewire1SPTPControlSD1DataSize, value => SD1DataSize = value);
                ControlValuesList[Global.Spacewire1.Record].AddProperty(Global.Spacewire1.Record.Busy, 3, 1, delegate { }, value => IsRecordBusy = 1 == value);
                ControlValuesList[Global.Spacewire1.Record].AddProperty(Global.Spacewire1.Record.IssuePackage, 0, 1, Device.CmdSpacewire1Record, value => IsIssuePackage = 1 == value);
            }
        }

        /// <summary>
        /// Содержит нотификатор команд для управления БУК.
        /// </summary>
        public class ControlBuk : SubNotify
        {
            /// <summary>
            /// The issue command get frame
            /// </summary>
            private ICommand issueCmdGetFrame;

            /// <summary>
            /// The SPW2 is need save data
            /// </summary>
            private bool spw2IsNeedSaveData;

            /// <summary>
            /// The SPW2 data
            /// </summary>
            private byte[] spw2Data;

            /// <summary>
            /// The SPW2 is confirm execution
            /// </summary>
            private bool spw2IsConfirmExecution;

            /// <summary>
            /// The SPW2 is confirm receipt
            /// </summary>
            private bool spw2IsConfirmReceipt;

            /// <summary>
            /// The SPW2 is make tele command
            /// </summary>
            private bool spw2IsMakeTeleCmd;

            /// <summary>
            /// The is issue test
            /// </summary>
            private bool isIssueTest;

            /// <summary>
            /// The SPW2 apid
            /// </summary>
            private short spw2Apid;

            /// <summary>
            /// The threshold
            /// </summary>
            private short threshold;

            /// <summary>
            /// The issue command threshold
            /// </summary>
            private ICommand issueCmdThreshold;

            /// <summary>
            /// The algo type
            /// </summary>
            private byte algoType;

            /// <summary>
            /// The param1
            /// </summary>
            private byte param1;

            /// <summary>
            /// The param3
            /// </summary>
            //private byte param3;

            /// <summary>
            /// The param2
            /// </summary>
            //private byte param2;

            /// <summary>
            /// The issue command get tele
            /// </summary>
            private ICommand issueCmdGetTele;

            /// <summary>
            /// The sel detector
            /// </summary>
            private byte selDetector;

            /// <summary>
            /// The PWR detector
            /// </summary>
            private bool pwrDetector;

            /// <summary>
            /// The issue command sel detector
            /// </summary>
            private ICommand issueCmdSelDetector;

            /// <summary>
            /// The sel conf
            /// </summary>
            private byte selConf;

            /// <summary>
            /// The issue command configuration
            /// </summary>
            private ICommand issueCmdConfig;

            /// <summary>
            /// The mode conf
            /// </summary>
            private byte modeConf;

            /// <summary>
            /// The compress conf
            /// </summary>
            private byte compressConf;

            /// <summary>
            /// The line recv conf
            /// </summary>
            private byte lineRecvConf;

            /// <summary>
            /// The line send conf
            /// </summary>
            private byte lineSendConf;

            /// <summary>
            /// The activate conf
            /// </summary>
            private byte activateConf;

            /// <summary>
            /// The exchange conf
            /// </summary>
            private bool exchangeConf;

            /// <summary>
            /// The number shutter
            /// </summary>
            //private byte numberShutter;

            /// <summary>
            /// The shutter time
            /// </summary>
            private int shutterTime;

            /// <summary>
            /// The issue command shutter
            /// </summary>
            private ICommand issueCmdShutter;

            /// <summary>
            /// The number light
            /// </summary>
            //private byte numberLight;

            /// <summary>
            /// The light time
            /// </summary>
            private int lightTime;

            /// <summary>
            /// The issue command light
            /// </summary>
            private ICommand issueCmdLight;

            /// <summary>
            /// The command bytes
            /// </summary>
            private byte[] cmdBytes;

            /// <summary>
            /// The issue command send
            /// </summary>
            private ICommand issueCmdSend;

            /// <summary>
            /// The issue data send
            /// </summary>
            private ICommand issueDataSend;

            /// <summary>
            /// The data bytes
            /// </summary>
            private byte[] dataBytes;

            /// <summary>
            /// The line shutter
            /// </summary>
            private byte lineShutter;
            private ICommand issueCmdSelDetectorSdshOff;
            private ICommand issueCmdSelDetectorSdshOn;
            private ICommand issueCmdSelDetectorVufesOff;
            private ICommand issueCmdSelDetectorVufesOn;
            private ICommand issueCmdSelDetectorUfesOff;
            private ICommand issueCmdSelDetectorUfesOn;
            private ICommand issueCmdIndicateDetectorSdshOff;
            private ICommand issueCmdIndicateDetectorSdshOn;
            private ICommand issueCmdIndicateDetectorVufesOff;
            private ICommand issueCmdIndicateDetectorVufesOn;
            private ICommand issueCmdIndicateDetectorUfesOff;
            private ICommand issueCmdIndicateDetectorUfesOn;

            /// <summary>
            /// Инициализирует новый экземпляр класса <see cref="ControlBuk" />.
            /// </summary>
            /// <param name="owner">The owner.</param>
            public ControlBuk(EgseBukNotify owner)
                : base(owner)
            {                
            }

            /// <summary>
            /// Gets the issue command get frame.
            /// </summary>
            /// <value>
            /// The issue command get frame.
            /// </value>
            public ICommand IssueCmdGetFrame
            {
                get
                {
                    if (this.issueCmdGetFrame == null)
                    {
                        this.issueCmdGetFrame = new RelayCommand(CmdGetFrame, obj => { return true; });
                    }

                    return this.issueCmdGetFrame;
                }
            }

            /// <summary>
            /// Получает значение, показывющее, что [интерфейс spacewire готов к приему посылок].
            /// </summary>
            /// <value>
            /// <c>true</c> если [интерфейс spacewire готов к приему посылок]; иначе, <c>false</c>.
            /// </value>
            public bool IntfReady
            {
                get
                {
                    return Owner.Spacewire2Notify.IssuePackageCommand.CanExecute(null);
                }
            }

            /// <summary>
            /// Gets the issue command threshold.
            /// </summary>
            /// <value>
            /// The issue command threshold.
            /// </value>
            public ICommand IssueCmdThreshold
            {
                get
                {
                    if (this.issueCmdThreshold == null)
                    {
                        this.issueCmdThreshold = new RelayCommand(CmdThreshold, obj => { return true; });
                    }

                    return this.issueCmdThreshold;
                }
            }

            /// <summary>
            /// Gets the issue command get tele.
            /// </summary>
            /// <value>
            /// The issue command get tele.
            /// </value>
            public ICommand IssueCmdGetTele
            {
                get
                {
                    if (this.issueCmdGetTele == null)
                    {
                        this.issueCmdGetTele = new RelayCommand(CmdGetTele, obj => { return true; });
                    }

                    return this.issueCmdGetTele;
                }
            }

            /// <summary>
            /// Gets the issue command sel detector.
            /// </summary>
            /// <value>
            /// The issue command sel detector.
            /// </value>
            public ICommand IssueCmdSelDetector
            {
                get
                {
                    if (this.issueCmdSelDetector == null)
                    {
                        this.issueCmdSelDetector = new RelayCommand(CmdSelDetector, obj => { return true; });
                    }

                    return this.issueCmdSelDetector;
                }
            }

            public ICommand IssueCmdSelDetectorUfesOn
            {
                get
                {
                    if (this.issueCmdSelDetectorUfesOn == null)
                    {
                        this.issueCmdSelDetectorUfesOn = new RelayCommand((obj) => { SelDetector = 0; PwrDetector = true; CmdSelDetector(obj); }, obj => { return true; });
                    }

                    return this.issueCmdSelDetectorUfesOn;
                }
            }

            public ICommand IssueCmdSelDetectorUfesOff
            {
                get
                {
                    if (this.issueCmdSelDetectorUfesOff == null)
                    {
                        this.issueCmdSelDetectorUfesOff = new RelayCommand((obj) => { SelDetector = 0; PwrDetector = false; CmdSelDetector(obj); }, obj => { return true; });
                    }

                    return this.issueCmdSelDetectorUfesOff;
                }
            }

            public ICommand IssueCmdSelDetectorVufesOn
            {
                get
                {
                    if (this.issueCmdSelDetectorVufesOn == null)
                    {
                        this.issueCmdSelDetectorVufesOn = new RelayCommand((obj) => { SelDetector = 1; PwrDetector = true; CmdSelDetector(obj); }, obj => { return true; });
                    }

                    return this.issueCmdSelDetectorVufesOn;
                }
            }

            public ICommand IssueCmdSelDetectorVufesOff
            {
                get
                {
                    if (this.issueCmdSelDetectorVufesOff == null)
                    {
                        this.issueCmdSelDetectorVufesOff = new RelayCommand((obj) => { SelDetector = 1; PwrDetector = false; CmdSelDetector(obj); }, obj => { return true; });
                    }

                    return this.issueCmdSelDetectorVufesOff;
                }
            }

            public ICommand IssueCmdSelDetectorSdshOn
            {
                get
                {
                    if (this.issueCmdSelDetectorSdshOn == null)
                    {
                        this.issueCmdSelDetectorSdshOn = new RelayCommand((obj) => { SelDetector = 2; PwrDetector = true; CmdSelDetector(obj); }, obj => { return true; });
                    }

                    return this.issueCmdSelDetectorSdshOn;
                }
            }

            public ICommand IssueCmdSelDetectorSdshOff
            {
                get
                {
                    if (this.issueCmdSelDetectorSdshOff == null)
                    {
                        this.issueCmdSelDetectorSdshOff = new RelayCommand((obj) => { SelDetector = 2; PwrDetector = false; CmdSelDetector(obj); }, obj => { return true; });
                    }

                    return this.issueCmdSelDetectorSdshOff;
                }
            }

            public ICommand IssueCmdIndicateDetectorUfesOn
            {
                get
                {
                    if (this.issueCmdIndicateDetectorUfesOn == null)
                    {
                        this.issueCmdIndicateDetectorUfesOn = new RelayCommand((obj) => { SelDetector = 0; PwrDetector = true; CmdIndicateDetector(obj); }, obj => { return true; });
                    }

                    return this.issueCmdIndicateDetectorUfesOn;
                }
            }

            public ICommand IssueCmdIndicateDetectorUfesOff
            {
                get
                {
                    if (this.issueCmdIndicateDetectorUfesOff == null)
                    {
                        this.issueCmdIndicateDetectorUfesOff = new RelayCommand((obj) => { SelDetector = 0; PwrDetector = false; CmdIndicateDetector(obj); }, obj => { return true; });
                    }

                    return this.issueCmdIndicateDetectorUfesOff;
                }
            }

            public ICommand IssueCmdIndicateDetectorVufesOn
            {
                get
                {
                    if (this.issueCmdIndicateDetectorVufesOn == null)
                    {
                        this.issueCmdIndicateDetectorVufesOn = new RelayCommand((obj) => { SelDetector = 1; PwrDetector = true; CmdIndicateDetector(obj); }, obj => { return true; });
                    }

                    return this.issueCmdIndicateDetectorVufesOn;
                }
            }

            public ICommand IssueCmdIndicateDetectorVufesOff
            {
                get
                {
                    if (this.issueCmdIndicateDetectorVufesOff == null)
                    {
                        this.issueCmdIndicateDetectorVufesOff = new RelayCommand((obj) => { SelDetector = 1; PwrDetector = false; CmdIndicateDetector(obj); }, obj => { return true; });
                    }

                    return this.issueCmdIndicateDetectorVufesOff;
                }
            }

            public ICommand IssueCmdIndicateDetectorSdshOn
            {
                get
                {
                    if (this.issueCmdIndicateDetectorSdshOn == null)
                    {
                        this.issueCmdIndicateDetectorSdshOn = new RelayCommand((obj) => { SelDetector = 2; PwrDetector = true; CmdIndicateDetector(obj); }, obj => { return true; });
                    }

                    return this.issueCmdIndicateDetectorSdshOn;
                }
            }

            public ICommand IssueCmdIndicateDetectorSdshOff
            {
                get
                {
                    if (this.issueCmdIndicateDetectorSdshOff == null)
                    {
                        this.issueCmdIndicateDetectorSdshOff = new RelayCommand((obj) => { SelDetector = 2; PwrDetector = false; CmdIndicateDetector(obj); }, obj => { return true; });
                    }

                    return this.issueCmdIndicateDetectorSdshOff;
                }
            }

            /// <summary>
            /// Gets the issue command configuration.
            /// </summary>
            /// <value>
            /// The issue command configuration.
            /// </value>
            public ICommand IssueCmdConfigBuk
            {
                get
                {
                    if (this.issueCmdConfig == null)
                    {
                        this.issueCmdConfig = new RelayCommand((obj) => { SelConf = 1; CmdConfig(obj); }, obj => { return true; });
                    }

                    return this.issueCmdConfig;
                }
            }
            public ICommand IssueCmdConfigKvv
            {
                get
                {
                    if (this.issueCmdConfig == null)
                    {
                        this.issueCmdConfig = new RelayCommand((obj) => { SelConf = 2; CmdConfig(obj); }, obj => { return true; });
                    }

                    return this.issueCmdConfig;
                }
            }
            /// <summary>
            /// Gets the issue command shutter.
            /// </summary>
            /// <value>
            /// The issue command shutter.
            /// </value>
            public ICommand IssueCmdShutter
            {
                get
                {
                    if (this.issueCmdShutter == null)
                    {
                        this.issueCmdShutter = new RelayCommand(CmdShutter, obj => { return true; });
                    }

                    return this.issueCmdShutter;
                }
            }

            /// <summary>
            /// Gets the issue command light.
            /// </summary>
            /// <value>
            /// The issue command light.
            /// </value>
            public ICommand IssueCmdLight
            {
                get
                {
                    if (this.issueCmdLight == null)
                    {
                        this.issueCmdLight = new RelayCommand(CmdLight, obj => true);
                    }

                    return this.issueCmdLight;
                }
            }

            /// <summary>
            /// Gets the issue command send.
            /// </summary>
            /// <value>
            /// The issue command send.
            /// </value>
            public ICommand IssueCmdSend
            {
                get
                {
                    if (this.issueCmdSend == null)
                    {
                        this.issueCmdSend = new RelayCommand(CmdSend, obj => { return true; });
                    }

                    return this.issueCmdSend;
                }
            }

            /// <summary>
            /// Gets the issue data send.
            /// </summary>
            /// <value>
            /// The issue data send.
            /// </value>
            public ICommand IssueDataSend
            {
                get
                {
                    if (this.issueDataSend == null)
                    {
                        this.issueDataSend = new RelayCommand(DataSend, obj => { return true; });
                    }

                    return this.issueDataSend;
                }
            }

            /// <summary>
            /// Gets the type of the algo.
            /// </summary>
            /// <value>
            /// The type of the algo.
            /// </value>
            public byte AlgoType
            {
                get
                {
                    return this.algoType;
                }

                private set
                {
                    if (value == this.algoType)
                    {
                        return;
                    }

                    this.algoType = value;
                    OnPropertyChanged();
                }
            }

            /// <summary>
            /// Gets the sel conf.
            /// </summary>
            /// <value>
            /// The sel conf.
            /// </value>
            public byte SelConf
            {
                get
                {
                    return this.selConf;
                }

                private set
                {
                    if (value == this.selConf)
                    {
                        return;
                    }

                    this.selConf = value;
                    OnPropertyChanged();
                }
            }

            /// <summary>
            /// Gets the data bytes.
            /// </summary>
            /// <value>
            /// The data bytes.
            /// </value>
            public byte[] DataBytes
            {
                get
                {
                    return this.dataBytes;
                }

                private set
                {
                    if ((null != value) && (value == this.dataBytes))
                    {
                        return;
                    }

                    this.dataBytes = value;
                    OnPropertyChanged();
                }
            }

            /// <summary>
            /// Gets the command bytes.
            /// </summary>
            /// <value>
            /// The command bytes.
            /// </value>
            public byte[] CmdBytes
            {
                get
                {
                    return this.cmdBytes;
                }

                private set
                {
                    if (value.Equals(this.cmdBytes))
                    {
                        return;
                    }

                    this.cmdBytes = value;
                    OnPropertyChanged();
                }
            }

            /// <summary>
            /// Gets the sel detector.
            /// </summary>
            /// <value>
            /// The sel detector.
            /// </value>
            public byte SelDetector
            {
                get
                {
                    return this.selDetector;
                }

                private set
                {
                    if (value == this.selDetector)
                    {
                        return;
                    }

                    this.selDetector = value;
                    OnPropertyChanged();
                }
            }

            /// <summary>
            /// Gets the compress conf.
            /// </summary>
            /// <value>
            /// The compress conf.
            /// </value>
            public byte CompressConf
            {
                get
                {
                    return this.compressConf;
                }

                private set
                {
                    if (value == this.compressConf)
                    {
                        return;
                    }

                    this.compressConf = value;
                    OnPropertyChanged();
                }
            }

            /// <summary>
            /// Gets the line send conf.
            /// </summary>
            /// <value>
            /// The line send conf.
            /// </value>
            public byte LineSendConf
            {
                get
                {
                    return this.lineSendConf;
                }

                private set
                {
                    if (value == this.lineSendConf)
                    {
                        return;
                    }

                    this.lineSendConf = value;
                    OnPropertyChanged();
                }
            }

            /// <summary>
            /// Gets the activate conf.
            /// </summary>
            /// <value>
            /// The activate conf.
            /// </value>
            public byte ActivateConf
            {
                get
                {
                    return this.activateConf;
                }

                private set
                {
                    if (value == this.activateConf)
                    {
                        return;
                    }

                    this.activateConf = value;
                    OnPropertyChanged();
                }
            }

            /// <summary>
            /// Gets a value indicating whether [exchange conf].
            /// </summary>
            /// <value>
            ///   <c>true</c> if [exchange conf]; otherwise, <c>false</c>.
            /// </value>
            public bool ExchangeConf
            {
                get
                {
                    return this.exchangeConf;
                }

                private set
                {
                    if (value == this.exchangeConf)
                    { 
                        return;
                    }

                    this.exchangeConf = value;
                    OnPropertyChanged();
                }
            }

            /// <summary>
            /// Gets the number light.
            /// </summary>
            /// <value>
            /// The number light.
            /// </value>
            //public byte NumberLight
            //{
            //    get
            //    {
            //        return this.numberLight;
            //    }

            //    private set
            //    {
            //        if (value == this.numberLight)
            //        { 
            //            return;
            //        }

            //        this.numberLight = value;
            //        OnPropertyChanged();
            //    }
            //}

            /// <summary>
            /// Gets the light time.
            /// </summary>
            /// <value>
            /// The light time.
            /// </value>
            public int LightTime
            {
                get
                {
                    return this.lightTime;
                }

                private set
                {
                    if (value == this.lightTime)
                    {
                        return;
                    }

                    this.lightTime = value;
                    OnPropertyChanged();
                }
            }

            /// <summary>
            /// Gets the line shutter.
            /// </summary>
            /// <value>
            /// The line shutter.
            /// </value>
            public byte LineShutter
            {
                get
                {
                    return this.lineShutter;
                }

                private set
                {
                    if (value == this.lineShutter)
                    {
                        return;
                    }

                    this.lineShutter = value;
                    OnPropertyChanged();
                }
            }

            /// <summary>
            /// Gets the number shutter.
            /// </summary>
            /// <value>
            /// The number shutter.
            /// </value>
            //public byte NumberShutter
            //{
            //    get
            //    {
            //        return this.numberShutter;
            //    }

            //    private set
            //    {
            //        if (value == this.numberShutter)
            //        {
            //            return;
            //        }

            //        this.numberShutter = value;
            //        OnPropertyChanged();
            //    }
            //}

            /// <summary>
            /// Gets the shutter time.
            /// </summary>
            /// <value>
            /// The shutter time.
            /// </value>
            public int ShutterTime
            {
                get
                {
                    return this.shutterTime;
                }

                private set
                {
                    if (value == this.shutterTime)
                    { 
                        return;
                    }

                    this.shutterTime = value;
                    OnPropertyChanged();
                }
            }

            /// <summary>
            /// Gets the line recv conf.
            /// </summary>
            /// <value>
            /// The line recv conf.
            /// </value>
            public byte LineRecvConf
            {
                get
                {
                    return this.lineRecvConf;
                }

                private set
                {
                    if (value == this.lineRecvConf)
                    {
                        return;
                    }

                    this.lineRecvConf = value;
                    OnPropertyChanged();
                }
            }

            /// <summary>
            /// Gets the mode conf.
            /// </summary>
            /// <value>
            /// The mode conf.
            /// </value>
            public byte ModeConf
            {
                get
                {
                    return this.modeConf;
                }

                private set
                {
                    if (value == this.modeConf)
                    {
                        return;
                    }

                    this.modeConf = value;
                    OnPropertyChanged();
                }
            }

            /// <summary>
            /// Gets a value indicating whether [PWR detector].
            /// </summary>
            /// <value>
            ///   <c>true</c> if [PWR detector]; otherwise, <c>false</c>.
            /// </value>
            public bool PwrDetector
            {
                get
                {
                    return this.pwrDetector;
                }

                private set
                {
                    if (value == this.pwrDetector)
                    {
                        return;
                    }

                    this.pwrDetector = value;
                    OnPropertyChanged();
                }
            }

            /// <summary>
            /// Gets the param1.
            /// </summary>
            /// <value>
            /// The param1.
            /// </value>
            public byte Param1
            {
                get
                {
                    return this.param1;
                }

                private set
                {
                    if (value == this.param1)
                    {
                        return;
                    }

                    this.param1 = value;
                    OnPropertyChanged();
                }
            }

            /// <summary>
            /// Gets the param2.
            /// </summary>
            /// <value>
            /// The param2.
            /// </value>
            //public byte Param2
            //{
            //    get
            //    {
            //        return this.param2;
            //    }

            //    private set
            //    {
            //        if (value == this.param2)
            //        {
            //            return;
            //        }

            //        this.param2 = value;
            //        OnPropertyChanged();
            //    }
            //}

            /// <summary>
            /// Gets the param3.
            /// </summary>
            /// <value>
            /// The param3.
            /// </value>
            //public byte Param3
            //{
            //    get
            //    {
            //        return this.param3;
            //    }

            //    private set
            //    {
            //        if (value == this.param3)
            //        {
            //            return;
            //        }

            //        this.param3 = value;
            //        OnPropertyChanged();
            //    }
            //}

            /// <summary>
            /// Gets the threshold.
            /// </summary>
            /// <value>
            /// The threshold.
            /// </value>
            public short Threshold
            {
                get
                {
                    return this.threshold;
                }

                private set
                {
                    if (value == this.threshold)
                    { 
                        return;
                    }

                    this.threshold = value;
                    OnPropertyChanged();
                }
            }

            /// <summary>
            /// Gets a value indicating whether this instance is issue test.
            /// </summary>
            /// <value>
            /// <c>true</c> if this instance is issue test; otherwise, <c>false</c>.
            /// </value>
            public bool IsIssueTest
            {
                get
                {
                    return this.isIssueTest;
                }

                private set
                {
                    if (value == this.isIssueTest)
                    {
                        return;
                    }

                    this.isIssueTest = value;
                    OnPropertyChanged();
                }
            }

            public override void UpdateProperties()
            {
                OnPropertyChanged(() => this.IntfReady);
            }

            private void SetSpw2Prop(bool needSaveData, short apid, bool needExec, bool needRece, bool makeTele, byte[] bufData)
            {
                Owner.Spacewire2Notify.IsNeedSaveData = needSaveData;
                Owner.Spacewire2Notify.Apid = apid;
                Owner.Spacewire2Notify.IsConfirmExecution = needExec;
                Owner.Spacewire2Notify.IsConfirmReceipt = needRece;
                Owner.Spacewire2Notify.IsMakeTeleCmd = makeTele;
                Owner.Spacewire2Notify.Data = bufData;
                if (Owner.Spacewire2Notify.IssuePackageCommand.CanExecute(null))
                {
                    Owner.Spacewire2Notify.IssuePackageCommand.Execute(null);
                }
                else
                {
                    System.Windows.MessageBox.Show(Resource.Get(@"eNotSendCmd"));
                }
            }

            /// <summary>
            /// Datas the send.
            /// </summary>
            /// <param name="obj">The object.</param>
            private void DataSend(object obj)
            {
                PushSpw2Prop();                
                try
                {
                    if ((null == DataBytes) || (250 < DataBytes.Length))
                    {
                        System.Windows.MessageBox.Show(Resource.Get(@"eBadDataSendMsg"));
                        return;
                    }

                    byte[] buf = new byte[DataBytes.Length + 3];
                    Array.Copy(DataBytes, 0, buf, 2, DataBytes.Length);
                    buf[0] = 0x0E;
                    buf[1] = 0;
                    buf[2] = (byte)DataBytes.Length;
                    SetSpw2Prop(false, 0x610, true, true, true, buf);
                }
                finally
                {
                    PopSpw2Prop();
                }
            }

            /// <summary>
            /// Commands the send.
            /// </summary>
            /// <param name="obj">The object.</param>
            private void CmdSend(object obj)
            {
                PushSpw2Prop();
                try
                {                   
                    if ((null == CmdBytes) || (250 < CmdBytes.Length))
                    {
                        System.Windows.MessageBox.Show(Resource.Get(@"eBadDataSendMsg"));
                        return;
                    }

                    byte[] buf = new byte[4] { 0, 0, 0, 0 };
                    int i = 0;
                    if (null != CmdBytes)
                    {
                        foreach (byte t in CmdBytes)
                        {
                            buf[i++] = t;
                        }
                    }

                    SetSpw2Prop(false, 0x610, true, true, true, new byte[7] { 13, 0, 0, buf[0], buf[1], buf[2], buf[3] });
                }
                finally
                {
                    PopSpw2Prop();
                }
            }

            /// <summary>
            /// Управление БУК: команда "управление засветкой".
            /// </summary>
            /// <param name="obj">The object.</param>
            private void CmdLight(object obj)
            {
                PushSpw2Prop();
                try
                {
                    SetSpw2Prop(false, 0x610, true, true, true, new byte[7] { 12, 0, 0/*NumberLight*/, 0, (byte)(LightTime >> 16), (byte)(LightTime >> 8), (byte)LightTime });
                }
                finally
                {
                    PopSpw2Prop();
                }
            }

            /// <summary>
            /// Управление БУК: команда "открытие затворов".
            /// </summary>
            /// <param name="obj">The object.</param>
            private void CmdShutter(object obj)
            {
                PushSpw2Prop();
                try
                {
                    SetSpw2Prop(false, 0x610, true, true, true, new byte[7] {11, 0, (byte)((LineShutter << 4) | 0/*NumberShutter*/), 0, (byte)(ShutterTime >> 16), (byte)(ShutterTime >> 8), (byte)ShutterTime });
                }
                finally
                {
                    PopSpw2Prop();
                }
            }

            /// <summary>
            /// Commands the configuration.
            /// </summary>
            /// <param name="obj">The object.</param>
            private void CmdConfig(object obj)
            {
                PushSpw2Prop();
                try
                {
                    SetSpw2Prop(false, 0x610, false, true, true, 1 == SelConf ? new byte[7] { 10, 0, SelConf, 0, (byte)((ModeConf << 4) | CompressConf), 0, 0 } : new byte[7] { 10, 0, SelConf, 0, (byte)((Convert.ToByte(ExchangeConf) << 3) | (ActivateConf << 2) | (LineSendConf << 1) | LineRecvConf), 0, 0 });
                }
                finally
                {
                    PopSpw2Prop();
                }
            }

            /// <summary>
            /// Commands the sel detector.
            /// </summary>
            /// <param name="obj">The object.</param>
            private void CmdSelDetector(object obj)
            {
                PushSpw2Prop();
                try
                {
                    SetSpw2Prop(false, 0x610, true, true, true, new byte[3] { 8, 0, (byte)((Convert.ToByte(PwrDetector) << 4) | SelDetector) });
                }
                finally
                {
                    PopSpw2Prop();
                }
            }

            private void CmdIndicateDetector(object obj)
            {
                PushSpw2Prop();
                try
                {
                    SetSpw2Prop(false, 0x610, true, true, true, new byte[3] { 0x0F, 0, (byte)((Convert.ToByte(PwrDetector) << 4) | SelDetector) });
                }
                finally
                {
                    PopSpw2Prop();
                }
            }
            
            /// <summary>
            /// Commands the get tele.
            /// </summary>
            /// <param name="obj">The object.</param>
            private void CmdGetTele(object obj)
            {
                PushSpw2Prop();
                try
                {
                    SetSpw2Prop(false, 0x610, false, false, true, new byte[3] { 5, 0, 0 });
                }
                finally
                {
                    PopSpw2Prop();
                }
            }

            /// <summary>
            /// Commands the get frame.
            /// </summary>
            /// <param name="obj">The object.</param>
            private void CmdGetFrame(object obj)
            {
                PushSpw2Prop();
                try
                {
                    SetSpw2Prop(false, 0x610, true, true, true, new byte[3] { 1, 0, Convert.ToByte(IsIssueTest) });
                }
                finally
                {
                    PopSpw2Prop();
                }
            }

            /// <summary>
            /// Commands the threshold.
            /// </summary>
            /// <param name="obj">The object.</param>
            private void CmdThreshold(object obj)
            {
                PushSpw2Prop();
                try
                { 
                    SetSpw2Prop(false, 0x610, false, true, true, new byte[7] { 4, 0, AlgoType, (byte)(Threshold >> 8), (byte)Threshold, Param1, 0 /*Param2*/});
                }
                finally
                {
                    PopSpw2Prop();
                }
            }

            /// <summary>
            /// Pushes the SPW2 property.
            /// </summary>
            private void PushSpw2Prop()
            {
                this.spw2Apid = Owner.Spacewire2Notify.Apid;
                this.spw2IsNeedSaveData = Owner.Spacewire2Notify.IsNeedSaveData;
                this.spw2Data = Owner.Spacewire2Notify.Data;
                this.spw2IsConfirmExecution = Owner.Spacewire2Notify.IsConfirmExecution;
                this.spw2IsConfirmReceipt = Owner.Spacewire2Notify.IsConfirmReceipt;
                this.spw2IsMakeTeleCmd = Owner.Spacewire2Notify.IsMakeTeleCmd;
            }

            /// <summary>
            /// Pops the SPW2 property.
            /// </summary>
            private void PopSpw2Prop()
            {
                Owner.Spacewire2Notify.Apid = this.spw2Apid;
                Owner.Spacewire2Notify.IsNeedSaveData = this.spw2IsNeedSaveData;
                Owner.Spacewire2Notify.Data = null == this.spw2Data ? new byte[] { } : this.spw2Data;
                Owner.Spacewire2Notify.IsConfirmExecution = this.spw2IsConfirmExecution;
                Owner.Spacewire2Notify.IsConfirmReceipt = this.spw2IsConfirmReceipt;
                Owner.Spacewire2Notify.IsMakeTeleCmd = this.spw2IsMakeTeleCmd;
            }
        }

        /// <summary>
        /// Предоставляет информацию по телеметрии КВВ.
        /// </summary>
        public class TeleKvv : SubNotify
        {
            /// <summary>
            /// The TMKVV data
            /// </summary>
            private Egse.Protocols.SpacewireTm604MsgEventArgs.TmKvv tmkvvData = new SpacewireTm604MsgEventArgs.TmKvv();

            private EgseTime updateTime;
 
            /// <summary>
            /// Инициализирует новый экземпляр класса <see cref="TeleKvv" />.
            /// </summary>
            /// <param name="owner">The owner.</param>
            public TeleKvv(EgseBukNotify owner)
                : base(owner)
            {
            }

            /// <summary>
            /// Gets the KVV data.
            /// </summary>
            /// <value>
            /// The KVV data.
            /// </value>
            public byte[] KvvData
            {
                get
                {
                    return this.tmkvvData.Buffer;
                }
            }

            public string UpdateTime
            {
                get
                {
                    return this.updateTime.ToString();
                }
            }

            /// <summary>
            /// Получает или задает данные телеметрии КВВ.
            /// </summary>
            /// <value>
            /// Данные телеметрии КВВ.
            /// </value>
            internal Egse.Protocols.SpacewireTm604MsgEventArgs.TmKvv TmKvvData
            {
                get
                {
                    return this.tmkvvData;
                }

                set
                {
                    this.updateTime = Owner.DeviceTime;
                    this.tmkvvData = value;
                    OnPropertyChanged(string.Empty);
                }
            }
        }

        /// <summary>
        /// Предоставляет информацию по телеметрии БУК.
        /// </summary>
        public class TeleBuk : SubNotify
        {
            /// <summary>
            /// The tmbuk data
            /// </summary>
            private Egse.Protocols.SpacewireTm604MsgEventArgs.TmBuk tmbukData = new SpacewireTm604MsgEventArgs.TmBuk();
 
            private EgseTime updateTime;
 
            /// <summary>
            /// Инициализирует новый экземпляр класса <see cref="TeleBuk" />.
            /// </summary>
            /// <param name="owner">The owner.</param>
            public TeleBuk(EgseBukNotify owner)
                : base(owner)
            {
            }

            /// <summary>
            /// Gets the buk data.
            /// </summary>
            /// <value>
            /// The buk data.
            /// </value>
            public byte[] BukData
            {
                get
                {
                    return this.tmbukData.Buffer;
                }
            }

            public string UpdateTime
            {
                get
                {
                    return this.updateTime.ToString();
                }
            }

            /// <summary>
            /// Получает или задает данные телеметрии БУК.
            /// </summary>
            /// <value>
            /// Данные телеметрии БУК.
            /// </value>
            internal Egse.Protocols.SpacewireTm604MsgEventArgs.TmBuk TmBukData           
            {
                get
                {
                    return this.tmbukData;
                }

                set
                {
                    this.updateTime = Owner.DeviceTime;
                    this.tmbukData = value;
                    OnPropertyChanged(string.Empty);
                }
            }
        }

        /// <summary>
        /// Нотификатор spacewire2.
        /// </summary>
        [Serializable]
        public class Spacewire2 : SubNotify, IDataErrorInfo
        {         
            /// <summary>
            /// Timeout на запись данных в файл.
            /// </summary>
            public const int WaitForWriteTime = 1000;

            /// <summary>
            /// SPTP: Адрес ИМИТАТОРА БУСКа.
            /// </summary>
            private int logicBusk = 0;

            /// <summary>
            /// SPTP: Адрес БС.
            /// </summary>
            private int logicBuk = 0;

            /// <summary>
            /// Управление: вкл/выкл интерфейса Spacewire.
            /// </summary>
            private bool isIssueEnable;

            /// <summary>
            /// Управление: Установлена связь.
            /// </summary>
            private bool isConnect;

            /// <summary>
            /// Запись данных(до 1 Кбайт): Выдача посылки RMAP (самосбр.).
            /// </summary>
            private bool isIssueRMap;

            /// <summary>
            /// Запись данных(до 1 Кбайт): 1 – выдача посылки в прибор БС (самосбр.).
            /// </summary>
            private bool isIssuePackage;

            /// <summary>
            /// Управление обменом с приборами по SPTP: включить выдачу секундных меток (1PPS).
            /// </summary>
            private bool isIssueTickTime;

            /// <summary>
            /// Управление обменом с приборами по SPTP: включение обмена прибора БС.
            /// </summary>
            private bool isIssueExchange;

            /// <summary>
            /// Управление обменом с приборами по SPTP: выдача КБВ прибору БС (только при «1 PPS» == 1).
            /// </summary>
            private bool isIssueObt;

            /// <summary>
            /// Управление: Выбор канала.
            /// </summary>
            private Channel spacewireChannel;

            /// <summary>
            /// Количество предоставления квот от БУСК.
            /// </summary>
            private long replyQueueFromBusk;

            /// <summary>
            /// Количество предоставления квот от БУК.
            /// </summary>
            private long replyQueueFromBuk;

            /// <summary>
            /// Количество запросов квот от БУК.
            /// </summary>
            private long requestQueueFromBuk;

            /// <summary>
            /// Количество запросов квот от БУСК.
            /// </summary>
            private long requestQueueFromBusk;

            /// <summary>
            /// Текущее КБВ.
            /// </summary>
            private long codeOnboardTime;

            /// <summary>
            /// APID для формирования команды.
            /// </summary>
            private short setApid = 0x610;

            /// <summary>
            /// Текущее значение Timetick1 от БУСК.
            /// </summary>
            private byte buskTickTime1;

            /// <summary>
            /// Текущее значение Timetick2 от БУСК.
            /// </summary>
            private byte buskTickTime2;

            /// <summary>
            /// Текущее значение Timetick1 от БУК.
            /// </summary>
            private byte bukTickTime1;

            /// <summary>
            /// Текущее значение Timetick2 от БУК.
            /// </summary>
            private byte bukTickTime2;

            /// <summary>
            /// Формировать телекоманду.
            /// </summary>
            private bool isMakeTeleCmd = true;

            /// <summary>
            /// Сохранять данные в файл.
            /// </summary>
            private bool isSaveRawData;

            /// <summary>
            /// Имя файла данных.
            /// </summary>
            private string rawDataFile;

            /// <summary>
            /// Для асинхронной записи в файл.
            /// </summary>
            [field: NonSerialized]
            private FileStream binDataStream;

            /// <summary>
            /// Квазиасинхронная запись в файл.
            /// Примечание:
            /// Используется для сигнала, что все данные записались в файл.
            /// </summary>
            [field: NonSerialized]
            private Task binDataTask;

            /// <summary>
            /// Экземпляр команды на [выдачу посылки по интерфейсу spacewire].
            /// </summary>
            [field: NonSerialized]
            private ICommand issuePackageCommand;

            /// <summary>
            /// Экземпляр команды на [выдачу посылки RMAP по интерфейсу spacewire].
            /// </summary>
            [field: NonSerialized]
            private ICommand _issueRMapCommand;

            /// <summary>
            /// Экземпляр команды на [открыть из файла].
            /// </summary>
            [field: NonSerialized]
            private ICommand fromFileCommand;

            /// <summary>
            /// Экземпляр команды на [включение записи в файл].
            /// </summary>
            [field: NonSerialized]
            private ICommand _saveRawDataCommand;

            /// <summary>
            /// Данные/команды для отправки.
            /// </summary>
            [field: NonSerialized]
            private byte[] data;

            /// <summary>
            /// Включено ли подтверждение получения.
            /// </summary>
            private bool isConfirmReceipt;

            /// <summary>
            /// Включено ли подтверждение исполнения.
            /// </summary>
            private bool isConfirmExecution = true;

            /// <summary>
            /// The is need save data
            /// </summary>
            private bool isNeedSaveData = true;

            /// <summary>
            /// Список возможных apid-ов.
            /// </summary>
            [field: NonSerialized]
            private ObservableCollection<string> apidList = new ObservableCollection<string>() { "0x610", "0x612", "0x614", "0x616" };

            /// <summary>
            /// Сохранять ли текстовый лог-файл.
            /// </summary>
            private bool isSaveTxtData = true;

            /// <summary>
            /// Словарь для корректного подсчета счетчика телекоманд.
            /// </summary>
            [field: NonSerialized]
            private Dictionary<short, AutoCounter> counterIcd;

            /// <summary>
            /// The issue enable command
            /// </summary>
            private ICommand issueEnableCommand;

            /// <summary>
            /// Инициализирует новый экземпляр класса <see cref="Spacewire2" />.
            /// </summary>
            /// <param name="owner">Ссылка на экземпляр главного нотификатора.</param>
            public Spacewire2(EgseBukNotify owner)
                : base(owner)
            {
                counterIcd = new Dictionary<short, AutoCounter>();
                LogsClass.LogSpacewire2.GotLogChange += (sender, e) => OnPropertyChanged(() => this.TxtDataFileSizeFormated);
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
            /// Получает или задает буфер данных для передачи в USB.
            /// </summary>
            /// <value>
            /// Буфер данных для передачи в USB.
            /// </value>            
            public byte[] Data
            { 
                get 
                {                   
                    return this.data;
                }

                set
                {
                    if ((null != value) && (value.Equals(this.data)))
                    {
                        return;                    
                    }

                    this.data = value;
                }
            }

            /// <summary>
            /// Получает ссылку на экземпляр списка доступных apid-ов.
            /// </summary>
            /// <value>
            /// Ссылка на экземпляр списка доступных apid-ов.
            /// </value>
            public ObservableCollection<string> ApidList
            {
                get
                {
                    return this.apidList;
                }
            }

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
                    return this.requestQueueFromBusk;
                }

                set 
                {
                    if (value == this.requestQueueFromBusk)
                    {
                        return;
                    }

                    this.requestQueueFromBusk = value;
                    OnPropertyChanged();
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
                    return this.buskTickTime1;
                }

                set 
                {
                    if (value == this.buskTickTime1)
                    {
                        return;
                    }

                    this.buskTickTime1 = value;
                    OnPropertyChanged();
                }
            }

            /// <summary>
            /// Получает наименование текстового лог-файла.
            /// </summary>
            /// <value>
            /// Наименование текстового лог-файла.
            /// </value>
            public string TxtDataFile
            {
                get
                {
                    if (IsSaveTxtData)
                    {
                        return LogsClass.LogSpacewire2.FileName;
                    }
                    else
                    {
                        return string.Empty;
                    }
                }
            }

            public string TxtDataFileSizeFormated
            {
                get
                {
                    if (IsSaveTxtData)
                    {
                        return LogsClass.LogSpacewire2.FileSize.AsFileSizeString();
                    }
                    else
                    {
                        return ((long)0).AsFileSizeString();
                    }
                }
            }

            /// <summary>
            /// Получает наименование [файла для записи данных].
            /// </summary>
            /// <value>
            /// Наименование [файла для записи данных].
            /// </value>
            public string RawDataFile
            {
                get
                {
                    return this.rawDataFile;
                }

                private set 
                {
                    if (value == this.rawDataFile)
                    {
                        return;
                    }

                    this.rawDataFile = value;
                    if (string.Empty != value)
                    {
                        this.binDataStream = new FileStream(value, FileMode.Create, FileAccess.Write, FileShare.None, bufferSize: 4096, useAsync: true);
                    }
                    else
                    {
                        if (null != this.binDataStream)
                        {
                            try
                            {
                                if (null != this.binDataTask)
                                {
                                    this.binDataTask.Wait(WaitForWriteTime);
                                }
                            }
                            finally
                            {
                                this.binDataStream.Close();
                            }
                        }
                    }

                    OnPropertyChanged();
                }
            }

            public long RawDataFileSize
            {
                get
                {
                    if ((null != RawDataFile) && IsSaveRawData)
                    {
                        return new FileInfo(RawDataFile).Length;
                    }
                    else
                    {
                        return 0;
                    }
                }
            }

            public string RawDataFileSizeFormated
            {
                get
                {
                    return RawDataFileSize.AsFileSizeString();
                }
            }
            
            /// <summary>
            /// Получает или задает значение, показывающее, что [запись текстового лог-файла включена].
            /// </summary>
            /// <value>
            /// <c>true</c> если [запись текстового лог-файла включена]; иначе, <c>false</c>.
            /// </value>
            public bool IsSaveTxtData
            {
                get
                {
                    return this.isSaveTxtData;
                }

                set
                {
                    if (value == this.isSaveTxtData)
                    {
                        return;
                    }

                    this.isSaveTxtData = value;
                    OnPropertyChanged();

                    if (value)
                    {
                        LogsClass.LogSpacewire2.NewLog();                        
                    }

                    OnPropertyChanged(() => this.TxtDataFile);
                }
            }

            /// <summary>
            /// Получает или задает значение, показывающее, что [запись данных в файл включена].
            /// </summary>
            /// <value>
            /// <c>true</c> если [запись данных в файл включена]; иначе, <c>false</c>.
            /// </value>
            public bool IsSaveRawData
            {
                get
                {
                    return this.isSaveRawData;
                }

                set 
                {
                    if (value == this.isSaveRawData)
                    {
                        return;
                    }

                    this.isSaveRawData = value;
                    if (value)
                    {
                        RawDataFile = Owner.GetNewFileName(Resource.Get(@"stSpacewireLogName"));
                    }
                    else
                    {
                        RawDataFile = string.Empty;
                    }

                    OnPropertyChanged();
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
                    return this.buskTickTime2;
                }

                set 
                {
                    if (value == this.buskTickTime2)
                    {
                        return;
                    }

                    this.buskTickTime2 = value;
                    OnPropertyChanged();
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
                    return this.bukTickTime1;
                }

                set 
                {
                    if (value == this.bukTickTime1)
                    {
                        return;
                    }

                    this.bukTickTime1 = value;
                    OnPropertyChanged();
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
                    return this.bukTickTime2;
                }

                set 
                {
                    if (value == this.bukTickTime2)
                    {
                        return;
                    }

                    this.bukTickTime2 = value;
                    OnPropertyChanged();
                }
            }

            /// <summary>
            /// Получает значение [Счетчик телекоманд].
            /// </summary>
            /// <value>
            /// [Счетчик телекоманд].
            /// </value>
            public Dictionary<short, AutoCounter> CounterIcd 
            {
                get
                {
                    return counterIcd;
                }

                private set
                {
                    counterIcd = value;
                }
            }

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
                    return this.replyQueueFromBusk;
                }

                set 
                {
                    if (value == this.replyQueueFromBusk)
                    {
                        return;
                    }

                    this.replyQueueFromBusk = value;
                    OnPropertyChanged();
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
                    return this.requestQueueFromBuk;
                }

                set 
                {
                    if (value == this.requestQueueFromBuk)
                    {
                        return;
                    }

                    this.requestQueueFromBuk = value;
                    OnPropertyChanged();
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
                    return this.replyQueueFromBuk;
                }

                set 
                {
                    if (value == this.replyQueueFromBuk)
                    {
                        return;
                    }

                    this.replyQueueFromBuk = value;
                    OnPropertyChanged();
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
                    return this.codeOnboardTime;
                }

                set 
                {
                    if (value == this.codeOnboardTime)
                    { 
                        return; 
                    }

                    this.codeOnboardTime = value;
                    OnPropertyChanged();
                }
            }        

            /// <summary>
            /// Получает или задает канал имитатора БМ-4.
            /// </summary>
            /// <value>
            /// Канал имитатора БМ-4.
            /// </value>
            public Channel IssueSpacewireChannel
            {
                get
                {
                    return this.spacewireChannel;
                }

                set 
                {
                    if (value == this.spacewireChannel)
                    {
                        return;
                    }

                    this.spacewireChannel = value;
                    ControlValuesList[Global.Spacewire2.Control].SetProperty(Global.Spacewire2.Control.Channel, (int)value);
                    OnPropertyChanged();
                    Device.CmdSetDeviceLogicAddr();
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
                    if (null == this.issueEnableCommand)
                    {
                        this.issueEnableCommand = new RelayCommand(obj => { IsIssueEnable = !IsIssueEnable; }, obj => { return true; });
                    }

                    return this.issueEnableCommand;
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
                    return this.logicBusk;
                }

                set 
                {
                    if (value == this.logicBusk)
                    {
                        return;
                    }

                    this.logicBusk = value;
                    ControlValuesList[Global.Spacewire2.BuskLogic].SetProperty(Global.Spacewire2.BuskLogic, value);
                    OnPropertyChanged();
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
                    return this.logicBuk;
                }

                set 
                {
                    if (value == this.logicBuk)
                    {
                        return;
                    }

                    this.logicBuk = value;
                    ControlValuesList[Global.Spacewire2.BukLogic].SetProperty(Global.Spacewire2.BukLogic, value);
                    OnPropertyChanged();
                }
            }

            /// <summary>
            /// Получает или задает значение, показывающее, что [интерфейс включен].
            /// </summary>
            /// <value>
            /// <c>true</c> если [интерфейс включен]; иначе, <c>false</c>.
            /// </value>
            public bool IsIssueEnable
            {
                get
                {
                    return this.isIssueEnable;
                }

                set 
                {
                    if (value == this.isIssueEnable)
                    {
                        return;
                    }

                    this.isIssueEnable = value;
                    ControlValuesList[Global.Spacewire2.Control].SetProperty(Global.Spacewire2.Control.IssueEnable, Convert.ToInt32(value));
                    OnPropertyChanged();
                }
            }
 
            /// <summary>
            /// Получает значение, показывающее, что [связь по интерфейсу установлена].
            /// </summary>
            /// <value>
            /// <c>true</c> если [связь по интерфейсу установлена]; иначе, <c>false</c>.
            /// </value>
            public bool IsConnect
            {
                get
                {
                    return this.isConnect;
                }

                private set 
                {
                    if (value == this.isConnect)
                    {
                        return;
                    }

                    this.isConnect = value;
                    ControlValuesList[Global.Spacewire2.Control].SetProperty(Global.Spacewire2.Control.Connect, Convert.ToInt32(value));
                    OnPropertyChanged();
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
                    return this.isIssueRMap;
                }

                private set 
                {
                    if (value == this.isIssueRMap)
                    {
                        return;
                    }

                    this.isIssueRMap = value;
                    OnPropertyChanged();
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
                    if (this.fromFileCommand == null)
                    {
                        this.fromFileCommand = new RelayCommand(OpenFromFile, obj => { return true; });
                    }

                    return this.fromFileCommand;
                }
            }

            /// <summary>
            /// Получает значение, показывающее, что [запись в прибор занята].
            /// </summary>
            /// <value>
            /// <c>true</c> если [запись в прибор занята]; иначе, <c>false</c>.
            /// </value>
            public bool IsRecordBusy
            {
                get
                {
                    return IsIssueRMap || IsIssuePackage || !IsConnect || !IsIssueExchange;
                }
            }

            /// <summary>
            /// Получает или задает значение, показывающее, что [необходимо формировать посылку телекоманды].
            /// </summary>
            /// <value>
            /// <c>true</c> если [необходимо формировать посылку телекоманды]; иначе, <c>false</c>.
            /// </value>
            public bool IsMakeTeleCmd
            {
                get
                {
                    return this.isMakeTeleCmd;
                }

                set 
                {
                    if (value == this.isMakeTeleCmd)
                    {
                        return;
                    }

                    this.isMakeTeleCmd = value;
                    OnPropertyChanged();
                }
            }

            /// <summary>
            /// Получает или задает значение, показывающее, что [необходимо включить подтверждение получения телекоманды].
            /// </summary>
            /// <value>
            /// <c>true</c> если [необходимо включить подтверждение получения телекоманды]; иначе, <c>false</c>.
            /// </value>
            public bool IsConfirmReceipt
            {
                get
                {
                    return this.isConfirmReceipt;
                }

                set
                {
                    if (value == this.isConfirmReceipt)
                    {
                        return;
                    }

                    this.isConfirmReceipt = value;
                    OnPropertyChanged();
                }
            }

            /// <summary>
            /// Получает или задает значение, показывающее, что [необходимо включить подтверждение выполнения телекоманды].
            /// </summary>
            /// <value>
            /// <c>true</c> если [необходимо включить подтверждение выполнения телекоманды]; иначе, <c>false</c>.
            /// </value>
            public bool IsConfirmExecution 
            { 
                get
                {
                    return this.isConfirmExecution;
                }

                set
                {
                    if (value == this.isConfirmExecution)
                    {
                        return;
                    }

                    this.isConfirmExecution = value;
                    OnPropertyChanged();
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
                    return this.isIssuePackage;
                }

                private set 
                {
                    if (value == this.isIssuePackage)
                    {
                        return;
                    }

                    this.isIssuePackage = value;
                    ControlValuesList[Global.Spacewire2.Record].SetProperty(Global.Spacewire2.Record.IssuePackage, Convert.ToInt32(value), false);
                    OnPropertyChanged();
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
                    if (this.issuePackageCommand == null)
                    {
                        this.issuePackageCommand = new RelayCommand(obj => { IsIssuePackage = true; ControlValuesList[Global.Spacewire2.Record].SetProperty(Global.Spacewire2.Record.IssuePackage, 1); }, obj => { return !IsRecordBusy; });
                    }

                    return this.issuePackageCommand;
                }
            }

            /// <summary>
            /// Получает или задает значение, показывающее, что [выдаются метки времени приборам].
            /// </summary>
            /// <value>
            /// <c>true</c> если [выдаются метки времени приборам]; иначе, <c>false</c>.
            /// </value>
            public bool IsIssueTickTime
            {
                get
                {
                    return this.isIssueTickTime;
                }

                set 
                {
                    if (value == this.isIssueTickTime)
                    {
                        return;
                    }

                    this.isIssueTickTime = value;
                    ControlValuesList[Global.Spacewire2.SptpControl].SetProperty(Global.Spacewire2.SptpControl.IssueTimeMark, Convert.ToInt32(value));
                    OnPropertyChanged();
                }
            }

            /// <summary>
            /// Получает или задает значение, показывающее, что [включен обмен для прибора БУК].
            /// </summary>
            /// <value>
            /// <c>true</c> если [включен обмен для прибора БУК]; иначе, <c>false</c>.
            /// </value>
            public bool IsIssueExchange
            {
                get
                {
                    return this.isIssueExchange;
                }

                set 
                {
                    if (value == this.isIssueExchange)
                    {
                        return;
                    }

                    this.isIssueExchange = value;
                    ControlValuesList[Global.Spacewire2.SptpControl].SetProperty(Global.Spacewire2.SptpControl.IssueExchange, Convert.ToInt32(value));
                    OnPropertyChanged();
                }
            }

            /// <summary>
            /// Получает или задает значение, показывающее, что [необходимо выдавать КБВ для прибора БУК].
            /// </summary>
            /// <value>
            /// <c>true</c> если [необходимо выдавать КБВ для прибора БУК]; иначе, <c>false</c>.
            /// </value>
            public bool IsIssueObt
            {
                get
                {
                    return this.isIssueObt;
                }

                set 
                {
                    if (value == this.isIssueObt)
                    {
                        return;
                    }

                    this.isIssueObt = value;
                    ControlValuesList[Global.Spacewire2.SptpControl].SetProperty(Global.Spacewire2.SptpControl.IssueKbv, Convert.ToInt32(value));
                    OnPropertyChanged();
                }
            }

            /// <summary>
            /// Получает номер Apid из списка выбранного по-умолчанию.
            /// </summary>
            /// <value>
            /// Номер Apid из списка выбранного по-умолчанию.
            /// </value>
            public int ApidSelected
            {
                get
                {
                    return 0;
                }
            }

            /// <summary>
            /// Получает или задает текущий APID для формирования посылки.
            /// </summary>
            /// <value>
            /// Текущий APID для формирования посылки.
            /// </value>
            public short Apid
            {
                get
                {
                    return this.setApid;
                }

                set 
                {
                    if (value == this.setApid)
                    {
                        return;
                    }

                    this.setApid = value;
                    OnPropertyChanged();
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
            /// Получает или задает значение, показывающее, что [необходимо сохранять данные в список].
            /// </summary>
            /// <value>
            /// <c>true</c> если [необходимо сохранять данные в список]; иначе, <c>false</c>.
            /// </value>
            internal bool IsNeedSaveData
            {
                get
                {
                    return this.isNeedSaveData;
                }

                set
                {
                    if (value == this.isNeedSaveData)
                    {
                        return;
                    }

                    this.isNeedSaveData = value;
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
            /// Makes the data.
            /// </summary>
            /// <returns></returns>
            public byte[] MakeData()
            {
                if ((null != this.Data) && (0 < this.Data.Length))
                {
                    if (IsNeedSaveData)
                    {
                        DataToSaveList();
                    }

                    if (IsMakeTeleCmd)
                    {
                        return this.Data.ToTk((byte)LogicBuk, (byte)LogicBusk, Apid, IsConfirmReceipt, IsConfirmExecution).ToArray();
                    }
                    else
                    {
                        return this.Data.ToSptp((byte)LogicBuk, (byte)LogicBusk).ToArray();
                    }
                }
                else
                {
                    return new byte[] { };
                }
            }

            public override void UpdateProperties()
            {
                OnPropertyChanged(() => this.TxtDataFileSizeFormated);
            }

            /// <summary>
            /// Serializes the specified object.
            /// </summary>
            /// <param name="obj">The object.</param>
            public override void Serialize(object obj = null)
            {
                base.Serialize(this);
            }

            /// <summary>
            /// Deserializes this instance.
            /// </summary>
            public override void Deserialize()
            {
                base.Deserialize();
            }

            /// <summary>
            /// Вызывается когда [требуется записать данные сообщения в файл].
            /// </summary>
            /// <param name="sender">The sender.</param>
            /// <param name="e">The <see cref="SpacewireSptpMsgEventArgs"/> instance containing the event data.</param>
            public virtual void OnSpacewire2MsgRawSave(object sender, BaseMsgEventArgs e)
            {
                if (null != this.binDataStream)
                {
                    if (null != this.binDataTask)
                    {
                        this.binDataTask.Wait(WaitForWriteTime);
                    }

                    if (this.binDataStream.CanWrite)
                    {
                        SpacewireTmMsgEventArgs spw = e as SpacewireTmMsgEventArgs;
                        if (null == spw)
                        {
                            return;
                        }

                        // фильтры на кадры с apid 600 и 610 (правка от 19 апреля)
                        if ((0x600 == spw.IcdInfo.Apid) || (0x610 == spw.IcdInfo.Apid))
                        {
                            return;
                        }

                        this.binDataTask = this.binDataStream.WriteAsync(spw.Data, 0, spw.Data.Length);
                        OnPropertyChanged(() => this.RawDataFileSizeFormated);
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
                OnPropertyChanged(() => this.Data);
            }

            /// <summary>
            /// Добавляет отправленную команду(данные) в список.
            /// </summary>
            internal void DataToSaveList()
            {
                string str = Converter.ByteArrayToHexStr(Data);
                if (!DataList.Contains(str))
                {
                    if (DataList.Count >= MaxDataListCount)
                    {
                        DataList.RemoveAt(DataList.Count - 1);
                    }

                    DataList.Insert(0, str);
                }

                OnPropertyChanged(() => this.DataList);
            }

            /// <summary>
            /// Initializes the control value.
            /// </summary>
            protected override void InitControlValue()
            {
                ControlValuesList.Add(Global.Spacewire2.Control, new ControlValue());
                ControlValuesList.Add(Global.Spacewire2.Record, new ControlValue());
                ControlValuesList.Add(Global.Spacewire2.SptpControl, new ControlValue());
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
                ControlValuesList[Global.Spacewire2.Record].AddProperty(Global.Spacewire2.Record.IssuePackage, 1, 1, Device.CmdSpacewire2Record, value => IsIssuePackage = 1 == value, true);
                ControlValuesList[Global.Spacewire2.BuskLogic].AddProperty(Global.Spacewire2.BuskLogic, 0, 8, Device.CmdSpacewire2LogicBusk, value => LogicBusk = value);
                ControlValuesList[Global.Spacewire2.BukLogic].AddProperty(Global.Spacewire2.BukLogic, 0, 8, Device.CmdSpacewire2LogicBuk, value => LogicBuk = value);
                ControlValuesList[Global.Spacewire2.SptpControl].AddProperty(Global.Spacewire2.SptpControl.IssueTimeMark, 0, 1, Device.CmdSpacewire2SPTPControl, value => IsIssueTickTime = 1 == value);
                ControlValuesList[Global.Spacewire2.SptpControl].AddProperty(Global.Spacewire2.SptpControl.IssueExchange, 1, 1, Device.CmdSpacewire2SPTPControl, value => IsIssueExchange = 1 == value);
                ControlValuesList[Global.Spacewire2.SptpControl].AddProperty(Global.Spacewire2.SptpControl.IssueKbv, 2, 1, Device.CmdSpacewire2SPTPControl, value => IsIssueObt = 1 == value);
            }
        }

        /// <summary>
        /// Нотификатор spacewire3.
        /// </summary>
        [Serializable]
        public class Spacewire3 : SubNotify, IDataErrorInfo
        {
            /// <summary>
            /// Timeout на запись данных в файл.
            /// </summary>
            public const int WaitForWriteTime = 1000;

            /// <summary>
            /// Рабочий прибор.
            /// </summary>
            private DetectorDevice detectorDevice;

            /// <summary>
            /// Управление: вкл/выкл интерфейса Spacewire.
            /// </summary>
            private bool isIssueEnable;

            /// <summary>
            /// Управление: Установлена связь.
            /// </summary>
            private bool isConnect;

            /// <summary>
            /// Управление: Сигнал передачи кадров.
            /// </summary>
            private bool isIssueTransmission;

            /// <summary>
            /// Сохранять данные в файл.
            /// </summary>
            private bool isSaveRawData;

            /// <summary>
            /// Имя файла данных.
            /// </summary>
            private string rawDataFile;

            /// <summary>
            /// Для асинхронной записи в файл.
            /// </summary>
            [field: NonSerialized]
            private FileStream rawDataStream;

            /// <summary>
            /// Квазиасинхронная запись в файл.
            /// Примечание:
            /// Используется для сигнала, что все данные записались в файл.
            /// </summary>
            [field: NonSerialized]
            private Task rawDataTask;

            /// <summary>
            /// Полукомплект рабочего прибора.
            /// </summary>
            private HalfSet halfSet;

            /// <summary>
            /// Экземпляр команды [включение интерфейса spacewire].
            /// </summary>
            [field: NonSerialized]
            private ICommand _issueEnableCommand;

            /// <summary>
            /// Экземпляр команды на [включение записи в файл].
            /// </summary>
            [field: NonSerialized]
            private ICommand _saveRawDataCommand;

            /// <summary>
            /// Текущее значение Timetick1 от БУК.
            /// </summary>
            private byte bukTickTime1;

            /// <summary>
            /// Текущее значение Timetick2 от БУК.
            /// </summary>
            private byte bukTickTime2;

            /// <summary>
            /// Текущее значение Timetick2 от НП.
            /// </summary>
            private byte scidevTickTime2;

            /// <summary>
            /// Текущее значение Timetick1 от НП.
            /// </summary>
            private byte scidevTickTime1;

            /// <summary>
            /// Количество запросов квоты от БУК.
            /// </summary>
            private long requestQueueFromBuk;

            /// <summary>
            /// Количество предоставления квот от БУК.
            /// </summary>
            private long replyQueueFromBuk;

            /// <summary>
            /// Количество запросов квоты от НП.
            /// </summary>
            private long replyQueueFromSD;

            /// <summary>
            /// Количество предоставления квот от НП.
            /// </summary>
            private long requestQueueFromSD;

            /// <summary>
            /// Сохранять ли текстовый лог-файл.
            /// </summary>
            private bool isSaveTxtData = true;

            /// <summary>
            /// Инициализирует новый экземпляр класса <see cref="Spacewire3" />.
            /// </summary>
            /// <param name="owner">The owner.</param>
            public Spacewire3(EgseBukNotify owner)
                : base(owner)
            {
                LogsClass.LogSpacewire3.GotLogChange += (sender, e) => OnPropertyChanged(() => this.TxtDataFileSizeFormated);
            }

            /// <summary>
            /// Полукомплекты рабочего прибора.
            /// </summary>
            public enum HalfSet
            {
                /// <summary>
                /// Основная линия spacewire.
                /// </summary>
                [Description("Основная")]
                Main = 0x00,

                /// <summary>
                /// Резервная линия spacewire.
                /// </summary>
                [Description("Резервная")]
                Resv = 0x01
            }

            /// <summary>
            /// Возможные рабочие приборы.
            /// </summary>
            public enum DetectorDevice
            {
                /// <summary>
                /// Рабочий прибор "УФЭС".
                /// </summary>
                [Description("УФЭС")]
                Ufes = 0x00,

                /// <summary>
                /// Рабочий прибор "ВУФЭС".
                /// </summary>
                [Description("ВУФЭС")]
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
            /// Получает или задает значение, показывающее, что [интерфейс включен].
            /// </summary>
            /// <value>
            /// <c>true</c> если [интерфейс включен]; иначе, <c>false</c>.
            /// </value>
            public bool IsIssueEnable
            {
                get
                {
                    return this.isIssueEnable;
                }

                set 
                {
                    if (value == this.isIssueEnable)
                    {
                        return;
                    }

                    this.isIssueEnable = value;
                    ControlValuesList[Global.Spacewire3.Control].SetProperty(Global.Spacewire3.Control.IssueEnable, Convert.ToInt32(value));
                    ControlValuesList[Global.Spacewire4.Control].SetProperty(Global.Spacewire4.Control.Connect, Convert.ToInt32(0));
                    OnPropertyChanged();
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
            /// Получает значение, показывающее, что [связь по интерфейсу установлена].
            /// </summary>
            /// <value>
            /// <c>true</c> если [связь по интерфейсу установлена]; иначе, <c>false</c>.
            /// </value>
            public bool IsConnect
            {
                get
                {
                    return this.isConnect;
                }

                private set 
                {
                    if (value == this.isConnect)
                    {
                        return;
                    }

                    this.isConnect = value;
                    ControlValuesList[Global.Spacewire3.Control].SetProperty(Global.Spacewire3.Control.Connect, Convert.ToInt32(value));
                    OnPropertyChanged();
                }
            }

            /// <summary>
            /// Получает наименование текстового лог-файла.
            /// </summary>
            /// <value>
            /// Наименование текстового лог-файла.
            /// </value>
            public string TxtDataFile
            {
                get
                {
                    if (IsSaveTxtData)
                    {
                        return LogsClass.LogSpacewire3.FileName;
                    }
                    else
                    {
                        return string.Empty;
                    }
                }
            }

            public string TxtDataFileSizeFormated
            {
                get
                {
                    if (IsSaveTxtData)
                    {
                        return LogsClass.LogSpacewire3.FileSize.AsFileSizeString();
                    }
                    else
                    {
                        return ((long)0).AsFileSizeString();
                    }
                }
            }

            /// <summary>
            /// Получает или задает значение, показывающее, что [запись текстового лог-файла включена].
            /// </summary>
            /// <value>
            /// <c>true</c> если [запись текстового лог-файла включена]; иначе, <c>false</c>.
            /// </value>
            public bool IsSaveTxtData
            {
                get
                {
                    return this.isSaveTxtData;
                }

                set
                {
                    if (value == this.isSaveTxtData)
                    {
                        return;
                    }

                    this.isSaveTxtData = value;                    
                    if (value)
                    {
                        LogsClass.LogSpacewire3.NewLog();
                    }

                    OnPropertyChanged();
                    OnPropertyChanged(() => this.TxtDataFile);
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
                    return this.isIssueTransmission;
                }

                private set 
                {
                    if (value == this.isIssueTransmission)
                    {
                        return;
                    }

                    this.isIssueTransmission = value;
                    ControlValuesList[Global.Spacewire3.Control].SetProperty(Global.Spacewire3.Control.Transmission, Convert.ToInt32(value), false);
                    OnPropertyChanged();
                }
            }

            /// <summary>
            /// Получает или задает полукомплект прибора.
            /// </summary>
            /// <value>
            /// Полукомплект прибора.
            /// </value>
            public HalfSet IssueHalfSet
            {
                get
                {
                    return this.halfSet;
                }

                set 
                {
                    if (value == this.halfSet)
                    {
                        return;
                    }

                    this.halfSet = value;
                    ControlValuesList[Global.Spacewire3.Control].SetProperty(Global.Spacewire3.Control.HalfSet, (int)value);
                    OnPropertyChanged();
                }
            }

            /// <summary>
            /// Получает или задает рабочий детектор.
            /// </summary>
            /// <value>
            /// Рабочий детектор.
            /// </value>
            public DetectorDevice IssueDetectorDevice
            {
                get
                {
                    return this.detectorDevice;
                }

                set 
                {
                    if (value == this.detectorDevice)
                    {
                        return;
                    }

                    this.detectorDevice = value;
                    ControlValuesList[Global.Spacewire3.Control].SetProperty(Global.Spacewire3.Control.WorkDevice, (int)value);
                    OnPropertyChanged();
                }
            }

            /// <summary>
            /// Получает наименование [файла для записи данных].
            /// </summary>
            /// <value>
            /// Наименование [файла для записи данных].
            /// </value>
            public string RawDataFile
            {
                get
                {
                    return this.rawDataFile;
                }

                private set 
                {
                    if (value == this.rawDataFile)
                    {
                        return;
                    }

                    this.rawDataFile = value;
                    if (string.Empty != value)
                    {
                        this.rawDataStream = new FileStream(value, FileMode.Create, FileAccess.Write, FileShare.None, bufferSize: 4096, useAsync: true);
                    }
                    else
                    {
                        if (null != this.rawDataStream)
                        {
                            try
                            {
                                if (null != this.rawDataTask)
                                {
                                    this.rawDataTask.Wait(WaitForWriteTime);
                                }
                            }
                            finally
                            {
                                this.rawDataStream.Close();
                            }
                        }
                    }

                    OnPropertyChanged();
                }
            }

            /// <summary>
            /// Получает или задает значение, показывающее, что [запись данных в файл включена].
            /// </summary>
            /// <value>
            /// <c>true</c> если [запись данных в файл включена]; иначе, <c>false</c>.
            /// </value>
            public bool IsSaveRawData
            {
                get
                {
                    return this.isSaveRawData;
                }

                set 
                {
                    if (value == this.isSaveRawData)
                    {
                        return;
                    }

                    this.isSaveRawData = value;
                    if (value)
                    {
                        RawDataFile = Owner.GetNewFileName(Resource.Get(@"stSdLogName"));
                    }
                    else
                    {
                        RawDataFile = string.Empty;
                    }

                    OnPropertyChanged();
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

            public long RawDataFileSize
            {
                get
                {
                    if ((null != RawDataFile) && IsSaveRawData)
                    {
                        return new FileInfo(RawDataFile).Length;
                    }
                    else
                    {
                        return 0;
                    }
                }
            }

            public string RawDataFileSizeFormated
            {
                get
                {
                    return RawDataFileSize.AsFileSizeString();
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
                    return this.requestQueueFromBuk;
                }

                private set 
                {
                    if (value == this.requestQueueFromBuk)
                    {
                        return;
                    }

                    this.requestQueueFromBuk = value;
                    OnPropertyChanged();
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
                    return this.replyQueueFromBuk;
                }

                private set 
                {
                    if (value == this.replyQueueFromBuk)
                    {
                        return;
                    }

                    this.replyQueueFromBuk = value;
                    OnPropertyChanged();
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
                    return this.requestQueueFromSD;
                }

                private set 
                {
                    if (value == this.requestQueueFromSD)
                    {
                        return;
                    }

                    this.requestQueueFromSD = value;
                    OnPropertyChanged();
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
                    return this.replyQueueFromSD;
                }

                private set 
                {
                    if (value == this.replyQueueFromSD)
                    {
                        return;
                    }

                    this.replyQueueFromSD = value;
                    OnPropertyChanged();
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
                    return this.scidevTickTime1;
                }

                set 
                {
                    if (value == this.scidevTickTime1)
                    {
                        return;
                    }

                    this.scidevTickTime1 = value;
                    OnPropertyChanged();
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
                    return this.scidevTickTime2;
                }

                set 
                {
                    if (value == this.scidevTickTime2)
                    {
                        return;
                    }

                    this.scidevTickTime2 = value;
                    OnPropertyChanged();
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
                    return this.bukTickTime1;
                }

                set 
                {
                    if (value == this.bukTickTime1)
                    {
                        return;
                    }

                    this.bukTickTime1 = value;
                    OnPropertyChanged();
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
                    return this.bukTickTime2;
                }

                set 
                {
                    if (value == this.bukTickTime2)
                    {
                        return;
                    }

                    this.bukTickTime2 = value;
                    OnPropertyChanged();
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

            public override void UpdateProperties()
            {
                OnPropertyChanged(() => this.TxtDataFileSizeFormated);
            }

            /// <summary>
            /// Serializes the specified object.
            /// </summary>
            /// <param name="obj">The object.</param>
            public override void Serialize(object obj = null)
            {
                base.Serialize(this);
            }

            /// <summary>
            /// Deserializes this instance.
            /// </summary>
            public override void Deserialize()
            {
                base.Deserialize();
            }

            /// <summary>
            /// Вызывается когда [требуется записать данные сообщения в файл].
            /// </summary>
            /// <param name="sender">The sender.</param>
            /// <param name="e">The <see cref="SpacewireSptpMsgEventArgs"/> instance containing the event data.</param>
            public virtual void OnSpacewire3MsgRawSave(object sender, BaseMsgEventArgs e)
            {
                if (null != this.rawDataStream)
                {
                    if (null != this.rawDataTask)
                    {
                        this.rawDataTask.Wait(WaitForWriteTime);
                    }

                    if (this.rawDataStream.CanWrite)
                    {
                        if (e is SpacewireSptpMsgEventArgs)
                        {
                            SpacewireSptpMsgEventArgs sptp = e as SpacewireSptpMsgEventArgs;
                            this.rawDataTask = this.rawDataStream.WriteAsync(sptp.Data, 0, sptp.Data.Length);
                            OnPropertyChanged(() => this.RawDataFileSizeFormated);
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
                ControlValuesList[Global.Spacewire3.Control].AddProperty(Global.Spacewire3.Control.HalfSet, 4, 1, Device.CmdSpacewire3Control, value => IssueHalfSet = (HalfSet)value);
                ControlValuesList[Global.Spacewire3.Control].AddProperty(Global.Spacewire3.Control.WorkDevice, 1, 2, Device.CmdSpacewire3Control, value => IssueDetectorDevice = (DetectorDevice)value);
                ControlValuesList[Global.Spacewire3.Control].AddProperty(Global.Spacewire3.Control.Connect, 3, 1, delegate { }, value => IsConnect = 1 == value, true);
                ControlValuesList[Global.Spacewire3.Control].AddProperty(Global.Spacewire3.Control.Transmission, 5, 1, delegate { }, value => IsIssueTransmission = 1 == value, true);
            }
        }

        /// <summary>
        /// Управление spacewire4.
        /// </summary>
        [Serializable]
        public class Spacewire4 : SubNotify, IDataErrorInfo
        {
            /// <summary>
            /// Управление: вкл/выкл интерфейса Spacewire.
            /// </summary>
            private bool isIssueEnable;

            /// <summary>
            /// Управление: Установлена связь.
            /// </summary>
            private bool isConnect;

            /// <summary>
            /// Управление: Включение метки времени (1 Гц).
            /// </summary>
            private bool isIssueTimeMark;

            /// <summary>
            /// Запись данных(до 1 Кбайт): EEP или EOP.
            /// </summary>
            private bool isIssueEEP;

            /// <summary>
            /// Запись данных(до 1 Кбайт): Выдача в конце посылки EOP или EEP.
            /// </summary>
            private bool isIssueEOP = true;

            /// <summary>
            /// Запись данных(до 1 Кбайт): Автоматическая выдача.
            /// </summary>
            private bool isIssueAuto;

            /// <summary>
            /// Запись данных(до 1 Кбайт): Бит занятости.
            /// </summary>
            private bool isRecordBusy;

            /// <summary>
            /// Запись данных(до 1 Кбайт): Бит занятости.
            /// </summary>
            private bool isIssuePackage;

            /// <summary>
            /// Экземпляр команды на [включение интерфейса spacewire].
            /// </summary>
            [field: NonSerialized]
            private ICommand _issueEnableCommand;

            /// <summary>
            /// Экземпляр команды на [включение передачи метки времени по интерфейсу spacewire].
            /// </summary>
            [field: NonSerialized]
            private ICommand _issueTimeMarkCommand;

            /// <summary>
            /// Экземпляр команды на [открыть из файла].
            /// </summary>
            [field: NonSerialized]
            private ICommand _fromFileCommand;

            /// <summary>
            /// Экземпляр команды на [отправить пакет].
            /// </summary>
            [field: NonSerialized]
            private ICommand issuePackageCommand;

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
            /// Буфер данных для передачи в USB.
            /// </value>
            public byte[] Data { get; set; }

            /// <summary>
            /// Получает или задает значение, показывающее, что [интерфейс включен].
            /// </summary>
            /// <value>
            /// <c>true</c> если [интерфейс включен]; иначе, <c>false</c>.
            /// </value>
            public bool IsIssueEnable
            {
                get
                {
                    return this.isIssueEnable;
                }

                set 
                {
                    if (value == this.isIssueEnable)
                    {
                        return;
                    }

                    this.isIssueEnable = value;
                    ControlValuesList[Global.Spacewire4.Control].SetProperty(Global.Spacewire4.Control.IssueEnable, Convert.ToInt32(value));
                    ControlValuesList[Global.Spacewire3.Control].SetProperty(Global.Spacewire3.Control.Connect, Convert.ToInt32(0));
                    OnPropertyChanged();
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
            /// Получает значение, показывающее, что [связь по интерфейсу установлена].
            /// </summary>
            /// <value>
            /// <c>true</c> если [связь по интерфейсу установлена]; иначе, <c>false</c>.
            /// </value>
            public bool IsConnect
            {
                get
                {
                    return this.isConnect;
                }

                private set 
                {
                    if (value == this.isConnect)
                    {
                        return;
                    }

                    this.isConnect = value;
                    ControlValuesList[Global.Spacewire4.Control].SetProperty(Global.Spacewire4.Control.Connect, Convert.ToInt32(value));
                    OnPropertyChanged();
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
                    return this.isIssueTimeMark;
                }

                private set 
                {
                    if (value == this.isIssueTimeMark)
                    {
                        return;
                    }

                    this.isIssueTimeMark = value;
                    ControlValuesList[Global.Spacewire4.Control].SetProperty(Global.Spacewire4.Control.TimeMark, Convert.ToInt32(value));
                    OnPropertyChanged();
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
            /// Получает или задает значение, показывающее, что [выдается EEP].
            /// </summary>
            /// <value>
            /// <c>true</c> если [выдается EEP]; иначе, <c>false</c>.
            /// </value>
            public bool IsIssueEep
            {
                get
                {
                    return this.isIssueEEP;
                }

                set 
                {
                    if (value == this.isIssueEEP)
                    {
                        return;
                    }

                    this.isIssueEEP = value;
                    ControlValuesList[Global.Spacewire4.Record].SetProperty(Global.Spacewire4.Record.Eep, Convert.ToInt32(value));
                    OnPropertyChanged();
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
                    return this.isIssueEOP;
                }

                private set 
                {
                    if (value == this.isIssueEOP)
                    {
                        return;
                    }

                    this.isIssueEOP = value;
                    ControlValuesList[Global.Spacewire4.Record].SetProperty(Global.Spacewire4.Record.EOPSend, Convert.ToInt32(value));
                    OnPropertyChanged();
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
                    return this.isIssueAuto;
                }

                private set 
                {
                    if (value == this.isIssueAuto)
                    {
                        return;
                    }

                    this.isIssueAuto = value;
                    ControlValuesList[Global.Spacewire4.Record].SetProperty(Global.Spacewire4.Record.IssueAuto, Convert.ToInt32(value));
                    OnPropertyChanged();
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
                    return this.isRecordBusy || IsIssuePackage;
                }

                private set 
                {
                    if (value == this.isRecordBusy)
                    {
                        return;
                    }

                    this.isRecordBusy = value;
                    OnPropertyChanged();                    
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
                    return this.isIssuePackage;
                }

                private set 
                {
                    if (value == this.isIssuePackage)
                    {
                        return;
                    }

                    this.isIssuePackage = value;
                    ControlValuesList[Global.Spacewire4.Record].SetProperty(Global.Spacewire4.Record.IssuePackage, Convert.ToInt32(value));
                    OnPropertyChanged();                   
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
                    if (this.issuePackageCommand == null)
                    {
                        this.issuePackageCommand = new RelayCommand(
                            obj => 
                            { 
                                IsIssuePackage = true; 
                                if (IsIssueEep) 
                                { 
                                    IsIssueEep = false; 
                                } 
                            }, 
                            obj => { return !IsRecordBusy; });
                    }

                    return this.issuePackageCommand;
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
            /// Serializes the specified object.
            /// </summary>
            /// <param name="obj">The object.</param>
            public override void Serialize(object obj = null)
            {
                base.Serialize(this);
            }

            /// <summary>
            /// Deserializes this instance.
            /// </summary>
            public override void Deserialize()
            {
                base.Deserialize();
            }

            /// <summary>
            /// Вызов диалога "Открыть файл".
            /// </summary>
            /// <param name="obj">The object.</param>
            public void OpenFromFile(object obj)
            {
                Data = Owner.OpenFromFile();                
                OnPropertyChanged(() => this.Data);
            }

            /// <summary>
            /// Добавляет отправленную команду(данные) в список.
            /// </summary>
            internal void DataToSaveList()
            {
                string str = Converter.ByteArrayToHexStr(Data);
                if (!DataList.Contains(str))
                {
                    if (DataList.Count >= MaxDataListCount)
                    {
                        DataList.RemoveAt(DataList.Count - 1);
                    }

                    DataList.Insert(0, str);
                }

                OnPropertyChanged(() => this.DataList);
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
                ControlValuesList[Global.Spacewire4.Record].AddProperty(Global.Spacewire4.Record.Eep, 2, 1, Device.CmdSpacewire4Record, value => IsIssueEep = 1 == value);
                ControlValuesList[Global.Spacewire4.Record].AddProperty(Global.Spacewire4.Record.RecordBusy, 3, 1, delegate { }, value => IsRecordBusy = 1 == value);
                ControlValuesList[Global.Spacewire4.Record].AddProperty(Global.Spacewire4.Record.IssuePackage, 0, 1, Device.CmdSpacewire4Record, value => IsIssuePackage = 1 == value);
            }
        }
    }
}
