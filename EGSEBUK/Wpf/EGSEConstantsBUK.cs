//-----------------------------------------------------------------------
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
