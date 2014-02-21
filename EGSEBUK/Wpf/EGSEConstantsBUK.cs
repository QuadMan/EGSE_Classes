﻿//-----------------------------------------------------------------------
// <copyright file="EGSEConstantsBUK.cs" company="IKI RSSI, laboratory №711">
//     Copyright (c) IKI RSSI, laboratory №711. All rights reserved.
// </copyright>
// <author>Коробейщиков Иван</author>
//-----------------------------------------------------------------------

namespace EGSE.Constants
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using System.Threading.Tasks;
    using System.Windows;
    using EGSE.Utilites;

    /// <summary>
    /// Основные значения констант прибора.
    /// </summary>
    public static class Global
    {
        /// <summary>
        /// Индекс свойства [Телеметрия: Запитан ПК1 от БУСК].
        /// </summary>
        public const int PropertyTelePowerBUSK1 = 0;

        /// <summary>
        /// Индекс свойства [Телеметрия: Запитан ПК2 от БУСК].
        /// </summary>
        public const int PropertyTelePowerBUSK2 = 1;

        /// <summary>
        /// Индекс свойства [Телеметрия: Запитан ПК1 от БУНД].
        /// </summary>
        public const int PropertyTelePowerBUND1 = 2;

        /// <summary>
        /// Индекс свойства [Телеметрия: Запитан ПК2 от БУНД].
        /// </summary>
        public const int PropertyBUNDPower2 = 3;

        /// <summary>
        /// Индекс свойства [SPACEWIRE 2: Управление: Выбор канала].
        /// </summary>
        public const int PropertySpaceWire2Channel = 0;

        /// <summary>
        /// Индекс свойства [SPACEWIRE 2: Управление: вкл/выкл интерфейса Spacewire].
        /// </summary>
        public const int PropertySpaceWire2IntfOn = 1;

        /// <summary>
        /// Индекс свойства [SPACEWIRE 2: Управление: Установлена связь].
        /// </summary>
        public const int PropertySpaceWire2Connected = 2;

        public const int PropertySpaceWire2RecordSendRMAP = 0;
        public const int PropertySpaceWire2RecordSendBuk = 1;
        public const int PropertySpaceWire2RecordSendBkp = 2;

        public const int PropertySpaceWire2LogicBuk = 0;
        public const int PropertySpaceWire2LogicBusk = 0;
        public const int PropertySpaceWire2LogicBkp = 0;

        public const int PropertySimSpaceWire1LogicBusk = 0;
        public const int PropertySimSpaceWire1LogicNP1 = 0;
        public const int PropertySimSpaceWire1LogicNP2 = 0;

        /// <summary>
        /// Индекс свойства [SPACEWIRE 2: Управление обменом с приборами по SPTP: включить выдачу секундных меток (1PPS)].
        /// </summary>
        public const int PropertySpaceWire2TimeMark = 0;

        /// <summary>
        /// Индекс свойства [SPACEWIRE 2: Управление обменом с приборами по SPTP: включение обмена прибора БС].
        /// </summary>
        public const int PropertySpaceWire2BukTrans = 1;

        /// <summary>
        /// Индекс свойства [SPACEWIRE 2: Управление обменом с приборами по SPTP: включение обмена прибора БКП].
        /// </summary>
        public const int PropertySpaceWire2BkpTrans = 2;

        /// <summary>
        /// Индекс свойства [SPACEWIRE 2: Управление обменом с приборами по SPTP: можно выдавать пакет в БС].
        /// </summary>
        public const int PropertySpaceWire2BukTransData = 3;

        /// <summary>
        /// Индекс свойства [SPACEWIRE 2: Управление обменом с приборами по SPTP: можно выдавать пакет в БКП].
        /// </summary>
        public const int PropertySpaceWire2BkpTransData = 4;

        /// <summary>
        /// Индекс свойства [SPACEWIRE 2: Управление обменом с приборами по SPTP: выдача КБВ прибору БС (только при «1 PPS» == 1)].
        /// </summary>
        public const int PropertySpaceWire2BukKbv = 5;

        /// <summary>
        /// Индекс свойства [SPACEWIRE 2: Управление обменом с приборами по SPTP: выдача КБВ прибору БКП (только при «1 PPS» == 1)].
        /// </summary>
        public const int PropertySpaceWire2BkpKbv = 6;

        /// <summary>
        /// Индекс свойства [SPACEWIRE 1: Управление: вкл/выкл интерфейса Spacewire].
        /// </summary>
        public const int PropertySpaceWire1IntfOn = 0;

        /// <summary>
        /// Индекс свойства [SPACEWIRE 1: Управление обменом с приборами по SPTP: включение обмена прибора НП1].
        /// </summary>
        public const int PropertySpaceWire1NP1Trans = 0;

        /// <summary>
        /// Индекс свойства [SPACEWIRE 1: Управление обменом с приборами по SPTP: включение обмена прибора НП2].
        /// </summary>
        public const int PropertySpaceWire1NP2Trans = 1;

        /// <summary>
        /// Индекс свойства [SPACEWIRE 1: Управление обменом с приборами по SPTP: можно выдавать пакет в НП1].
        /// </summary>
        public const int PropertySpaceWire1NP1TransData = 2;

        /// <summary>
        /// Индекс свойства [SPACEWIRE 1: Управление обменом с приборами по SPTP: можно выдавать пакет в НП2].
        /// </summary>
        public const int PropertySpaceWire1NP2TransData = 3;

        public const int PropertySpaceWire1Connected = 4;

        public const int PropertySpaceWire1RecordBusy = 0;

        public const int PropertySpaceWire1RecordSend = 1;

        public const int PropertySpaceWire1NP1SendTime = 0;

        public const int PropertySpaceWire1NP2SendTime = 0;

        public const int PropertySpaceWire1NP1DataSize = 0;

        public const int PropertySpaceWire1NP2DataSize = 0;

        /// <summary>
        /// Индекс свойства [SPACEWIRE 4: Управление: вкл/выкл интерфейса Spacewire].
        /// </summary>
        public const int PropertySpaceWire4IntfOn = 0;

        public const int PropertySpaceWire4Connected = 1;

        public const int PropertySpaceWire4TimeMark = 2;
       
        public const int PropertySpaceWire4EEPSend = 0;

        public const int PropertySpaceWire4EOPSend = 1;

        public const int PropertySpaceWire4AutoSend = 2;

        public const int PropertySpaceWire4RecordBusy = 3;

        public const int PropertySpaceWire4RecordSend = 4;
        
        /// <summary>
        /// Индекс объекта "Телеметрия" в массиве ControlValuesList.
        /// </summary>
        public const int PowerControl = 0;

        /// <summary>
        /// Индекс объекта "Управление SpaceWire2" в массиве ControlValuesList.
        /// </summary>
        public const int SpaceWire2Control = 1;

        /// <summary>
        /// Индекс объекта "Управление SPTP SpaceWire2" в массиве ControlValuesList.
        /// </summary>
        public const int SpaceWire2ControlSPTP = 2;

        /// <summary>
        /// Индекс объекта "Управление SpaceWire1" в массиве ControlValuesList.
        /// </summary>
        public const int SpaceWire1Control = 3;

        /// <summary>
        /// Индекс объекта "Управление SpaceWire4" в массиве ControlValuesList.
        /// </summary>
        public const int SpaceWire4Control = 4;

        /// <summary>
        /// Индекс объекта "Управление SPTP SpaceWire1" в массиве ControlValuesList.
        /// </summary>
        public const int SpaceWire1ControlSPTP = 5;

        /// <summary>
        /// Индекс объекта "Запись данных (до 1 Кбайт) SpaceWire4" в массиве ControlValuesList.
        /// </summary>
        public const int SpaceWire4Record = 6;

        /// <summary>
        /// Индекс объекта "Запись данных (до 1 Кбайт) SpaceWire1" в массиве ControlValuesList.
        /// </summary>
        public const int SpaceWire1Record = 7;

        /// <summary>
        /// Индекс объекта "Spacewire1 Счетчик миллисекунд для НП1 (через сколько готовы данные)" в массиве ControlValuesList.
        /// </summary>
        public const int SpaceWire1ControlSPTPNP1SendTime = 8;

        /// <summary>
        /// Индекс объекта "Spacewire1 Счетчик миллисекунд для НП2 (через сколько готовы данные)" в массиве ControlValuesList.
        /// </summary>
        public const int SpaceWire1ControlSPTPNP2SendTime = 9;

        /// <summary>
        /// Индекс объекта "Spacewire1 Кол-во байт в пакете НП1" в массиве ControlValuesList.
        /// </summary>
        public const int SpaceWire1ControlSPTPNP1DataSize = 10;

        /// <summary>
        /// Индекс объекта "Spacewire1 Кол-во байт в пакете НП2" в массиве ControlValuesList.
        /// </summary>
        public const int SpaceWire1ControlSPTPNP2DataSize = 11;

        public const int SpaceWire2SPTPLogicBusk = 12;

        public const int SpaceWire2SPTPLogicBuk = 13;

        public const int SpaceWire2SPTPLogicBkp = 14;

        public const int SpaceWire1SPTPSimLogicBusk = 15;

        public const int SpaceWire1SPTPSimLogicNP1 = 16;

        public const int SpaceWire1SPTPSimLogicNP2 = 17;

        public const int SpaceWire2Record = 18;

        /// <summary>
        /// Заголовок главного окна.
        /// </summary>
        public static readonly string ShowCaption;

        /// <summary>
        /// Название КИА.
        /// </summary>
        public static readonly string DeviceName;

        /// <summary>
        /// Уникальный идентификатор USB.
        /// </summary>
        public static readonly string DeviceSerial;
        
        public const uint LogicAddrBusk1 = 32;
        public const uint LogicAddrBusk2 = 33;

        public const uint LogicAddrBuk1 = 38;
        public const uint LogicAddrBuk2 = 39;

        public const uint LogicAddrBkp1 = 36;
        public const uint LogicAddrBkp2 = 37;

        /// <summary>
        /// Инициализирует статические поля класса <see cref="Global" />.
        /// </summary>
        static Global()
        { 
            ShowCaption = Resource.Get("stShowCaption");
            DeviceName = Resource.Get("stDeviceName");
            DeviceSerial = Resource.Get("stDeviceSerial");
        }
    }
}
