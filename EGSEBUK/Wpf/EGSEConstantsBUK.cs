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
        /// Логическиq адрес [БУСК ПК1] для подключения к БМ-4.
        /// </summary>
        public const int LogicAddrBusk1 = 32;

        /// <summary>
        /// Логическиq адрес [БУСК ПК2] для подключения к БМ-4.
        /// </summary>
        public const int LogicAddrBusk2 = 33;

        /// <summary>
        /// Логическиq адрес [БС БУК ПК1] для подключения к БМ-4.
        /// </summary>
        public const int LogicAddrBuk1 = 38;

        /// <summary>
        /// Логическиq адрес [БС БУК ПК2] для подключения к БМ-4.
        /// </summary>
        public const int LogicAddrBuk2 = 39;

        /// <summary>
        /// Логическиq адрес [ISSIS ICU N (ПК1)] для подключения к БМ-4.
        /// </summary>
        public const uint LogicAddrBkp1 = 36;

        /// <summary>
        /// Логическиq адрес [ISSIS ICU N (ПК2)] для подключения к БМ-4.
        /// </summary>
        public const uint LogicAddrBkp2 = 37;

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
            Power = new CVPower();
            Telemetry = new CVTelemetry();
            ShowCaption = Resource.Get("stShowCaption");
            DeviceName = Resource.Get("stDeviceName");
            DeviceSerial = Resource.Get("stDeviceSerial");
        }

        /// <summary>
        /// Получает индекс объекта "Питание прибора" в массиве ControlValuesList.
        /// </summary>
        public static CVPower Power { get; private set; }

        /// <summary>
        /// Получает индекс объекта "Телеметрия" в массиве ControlValuesList.
        /// </summary>
        public static CVTelemetry Telemetry { get; private set; }

        /// <summary>
        /// Индексы свойств [SPACEWIRE 1 (ИМИТАТОР НП (БУК))]
        /// </summary>
        public static class Spacewire1
        {
            /// <summary>
            /// Инициализирует статические поля класса <see cref="Spacewire1" />.
            /// </summary>
            static Spacewire1()
            {
                Control = new CVControl();
                Record = new CVRecord();
                SPTPControl = new CVSPTP();
                SPTPLogicBusk = new CVSPTPLogicBusk();
                SPTPLogicSD1 = new CVSPTPLogicNP1();
                SPTPLogicSD2 = new CVSPTPLogicNP2();
                SD1SendTime = new CVNP1SendTime();
                SD2SendTime = new CVNP2SendTime();
                SD1DataSize = new CVNP1DataSize();
                SD2DataSize = new CVNP2DataSize();
            }

            /// <summary>
            /// Получает индекс объекта "Управление" в массиве ControlValuesList.
            /// </summary>
            public static CVControl Control { get; private set; }

            /// <summary>
            /// Получает индекс объекта "Запись данных(до 1 Кбайт)" в массиве ControlValuesList.
            /// </summary>
            public static CVRecord Record { get; private set; }

            /// <summary>
            /// Получает индекс объекта "SPTP" в массиве ControlValuesList.
            /// </summary>
            public static CVSPTP SPTPControl { get; private set; }

            /// <summary>
            /// Получает индекс объекта "SPTP Адрес БУСК" в массиве ControlValuesList.
            /// </summary>
            public static CVSPTPLogicBusk SPTPLogicBusk { get; private set; }

            /// <summary>
            /// Получает индекс объекта "SPTP Адрес НП1" в массиве ControlValuesList.
            /// </summary>
            public static CVSPTPLogicNP1 SPTPLogicSD1 { get; private set; }

            /// <summary>
            /// Получает индекс объекта "SPTP Адрес НП2" в массиве ControlValuesList.
            /// </summary>
            public static CVSPTPLogicNP2 SPTPLogicSD2 { get; private set; }

            /// <summary>
            /// Получает индекс объекта "Счетчик миллисекунд для НП1 (через сколько готовы данные)" в массиве ControlValuesList.
            /// </summary>
            public static CVNP1SendTime SD1SendTime { get; private set; }

            /// <summary>
            /// Получает индекс объекта "Счетчик миллисекунд для НП2 (через сколько готовы данные)" в массиве ControlValuesList.
            /// </summary>
            public static CVNP2SendTime SD2SendTime { get; private set; }

            /// <summary>
            /// Получает индекс объекта "Кол-во байт в пакете НП1" в массиве ControlValuesList.
            /// </summary>
            public static CVNP1DataSize SD1DataSize { get; private set; }

            /// <summary>
            /// Получает индекс объекта "Кол-во байт в пакете НП2" в массиве ControlValuesList.
            /// </summary>
            public static CVNP2DataSize SD2DataSize { get; private set; }

            /// <summary>
            /// Индексы свойств [Управление].
            /// </summary>
            public class CVControl
            {
                /// <summary>
                /// Индекс свойства [вкл/выкл интерфейса].
                /// </summary>
                public readonly string IntfOn = "c1c63372d0464a7581628acbb8e2f5f2";

                /// <summary>
                /// Индекс свойства [Установлена связь].
                /// </summary>
                public readonly string Connected = "feba0ba15e3040c89ff40f76ce5265ed";

                /// <summary>
                /// Неявно преобразует объект к типу "строка". 
                /// </summary>
                /// <param name="obj">Объект для преобразования.</param>
                /// <returns>Возвращает строковое представления объекта.</returns>
                public static implicit operator string(CVControl obj)
                {
                    return "93974c195a404d9abf1069bbb601ddec";
                }
            }

            /// <summary>
            /// Индексы свойств [Запись данных(до 1 Кбайт)].
            /// </summary>
            public class CVRecord
            {
                /// <summary>
                /// Индекс свойства [Бит занятости].
                /// </summary>
                public readonly string Busy = "a4f34a967ad9432092500fb0c0988d8e";

                /// <summary>
                /// Индекс свойства [Бит выдачи посылки].
                /// </summary>
                public readonly string Send = "08283fd972724eaea196ca125fd1f0d4";

                /// <summary>
                /// Неявно преобразует объект к типу "строка". 
                /// </summary>
                /// <param name="obj">Объект для преобразования.</param>
                /// <returns>Возвращает строковое представления объекта.</returns>
                public static implicit operator string(CVRecord obj)
                {
                    return "f49c6fa070af49d2915f622c0e54f910";
                }
            }

            /// <summary>
            /// Индексы свойств [SPTP].
            /// </summary>
            public class CVSPTP
            {
                /// <summary>
                /// Индекс свойства [Включение обмена SPTP (НП1)].
                /// </summary>
                public readonly string NP1Trans = "6612f20286a24221bda280ae3514af14";

                /// <summary>
                /// Индекс свойства [Включение обмена SPTP (НП2)].
                /// </summary>
                public readonly string NP2Trans = "a1fb6e2a1f5f43d88fbc17bd73655003";

                /// <summary>
                /// Индекс свойства [Разрешение выдачи пакетов данных по SPTP(НП1)].
                /// </summary>
                public readonly string NP1TransData = "b7bf0378fbb744e7bf56df93937a11c0";

                /// <summary>
                /// Индекс свойства [Разрешение выдачи пакетов данных по SPTP(НП2)].
                /// </summary>
                public readonly string SD2TransData = "8b075e9b4aa14a81935e8fb2cd2c9e3e";

                /// <summary>
                /// Неявно преобразует объект к типу "строка". 
                /// </summary>
                /// <param name="obj">Объект для преобразования.</param>
                /// <returns>Возвращает строковое представления объекта.</returns>
                public static implicit operator string(CVSPTP obj)
                {
                    return "5f2ab47e7c234ff68b2e305c2e64e943";
                }
            }

            /// <summary>
            /// Индекс свойства [SPTP Адрес БУСК].
            /// </summary>
            public class CVSPTPLogicBusk
            {
                /// <summary>
                /// Неявно преобразует объект к типу "строка". 
                /// </summary>
                /// <param name="obj">Объект для преобразования.</param>
                /// <returns>Возвращает строковое представления объекта.</returns>
                public static implicit operator string(CVSPTPLogicBusk obj)
                {
                    return "2c324fdbd0294049aa57b1b795fd5e02";
                }
            }

            /// <summary>
            /// Индекс свойства [SPTP Адрес НП1].
            /// </summary>
            public class CVSPTPLogicNP1
            {
                /// <summary>
                /// Неявно преобразует объект к типу "строка". 
                /// </summary>
                /// <param name="obj">Объект для преобразования.</param>
                /// <returns>Возвращает строковое представления объекта.</returns>
                public static implicit operator string(CVSPTPLogicNP1 obj)
                {
                    return "d997131f62294a1e9810cf271fc133b0";
                }
            }

            /// <summary>
            /// Индекс свойства [SPTP Адрес НП2].
            /// </summary>
            public class CVSPTPLogicNP2
            {
                /// <summary>
                /// Неявно преобразует объект к типу "строка". 
                /// </summary>
                /// <param name="obj">Объект для преобразования.</param>
                /// <returns>Возвращает строковое представления объекта.</returns>
                public static implicit operator string(CVSPTPLogicNP2 obj)
                {
                    return "6ba0d27f749a43f2bbb0f8899f9b586c";
                }
            }

            /// <summary>
            /// Индекс свойства [Счетчик миллисекунд для НП1 (через сколько готовы данные)].
            /// </summary>
            public class CVNP1SendTime
            {
                /// <summary>
                /// Неявно преобразует объект к типу "строка". 
                /// </summary>
                /// <param name="obj">Объект для преобразования.</param>
                /// <returns>Возвращает строковое представления объекта.</returns>
                public static implicit operator string(CVNP1SendTime obj)
                {
                    return "f841a90ce7b643e9881c01f268577078";
                }
            }

            /// <summary>
            /// Индекс свойства [Счетчик миллисекунд для НП2 (через сколько готовы данные)].
            /// </summary>
            public class CVNP2SendTime
            {
                /// <summary>
                /// Неявно преобразует объект к типу "строка". 
                /// </summary>
                /// <param name="obj">Объект для преобразования.</param>
                /// <returns>Возвращает строковое представления объекта.</returns>
                public static implicit operator string(CVNP2SendTime obj)
                {
                    return "8282d335efdd46dea0ee1ac9753eddf4";
                }
            }

            /// <summary>
            /// Индекс свойства [Кол-во байт в пакете НП1].
            /// </summary>
            public class CVNP1DataSize
            {
                /// <summary>
                /// Неявно преобразует объект к типу "строка". 
                /// </summary>
                /// <param name="obj">Объект для преобразования.</param>
                /// <returns>Возвращает строковое представления объекта.</returns>
                public static implicit operator string(CVNP1DataSize obj)
                {
                    return "87aa706194824b15b403fea1b324ffce";
                }
            }

            /// <summary>
            /// Индекс свойства [Кол-во байт в пакете НП2].
            /// </summary>
            public class CVNP2DataSize
            {
                /// <summary>
                /// Неявно преобразует объект к типу "строка". 
                /// </summary>
                /// <param name="obj">Объект для преобразования.</param>
                /// <returns>Возвращает строковое представления объекта.</returns>
                public static implicit operator string(CVNP2DataSize obj)
                {
                    return "7a518f48c6734bd0889fbb498cb30452";
                }
            }
        }

        /// <summary>
        /// Индексы свойств [SPACEWIRE 2 (ИМИТАТОР БМ-4 (БУСК))].
        /// </summary>
        public static class Spacewire2
        {
            /// <summary>
            /// Инициализирует статические поля класса <see cref="Spacewire2" />.
            /// </summary>
            static Spacewire2()
            {
                Control = new CVControl();
                Record = new CVRecord();
                SPTPLogicBusk = new CVSPTPLogicBusk();
                SPTPLogicBuk = new CVSPTPLogicBuk();
                SPTPLogicBkp = new CVSPTPLogicBkp();
                SPTPControl = new CVSPTPControl();
            }

            /// <summary>
            /// Получает индекс объекта "Управление" в массиве ControlValuesList.
            /// </summary>
            public static CVControl Control { get; private set; }

            /// <summary>
            /// Получает индекс объекта "Запись данных(до 1 Кбайт)" в массиве ControlValuesList.
            /// </summary>
            public static CVRecord Record { get; private set; }

            /// <summary>
            /// Получает индекс объекта "Адрес ИМИТАТОРА БУСКа" в массиве ControlValuesList.
            /// </summary>
            public static CVSPTPLogicBusk SPTPLogicBusk { get; private set; }

            /// <summary>
            /// Получает индекс объекта "Адрес БС" в массиве ControlValuesList.
            /// </summary>
            public static CVSPTPLogicBuk SPTPLogicBuk { get; private set; }

            /// <summary>
            /// Получает индекс объекта "Адрес БКП" в массиве ControlValuesList.
            /// </summary>
            public static CVSPTPLogicBkp SPTPLogicBkp { get; private set; }

            /// <summary>
            /// Получает индекс объекта "Управление обменом с приборами по SPTP" в массиве ControlValuesList.
            /// </summary>
            public static CVSPTPControl SPTPControl { get; private set; }

            /// <summary>
            /// Индексы свойств [Управление].
            /// </summary>
            public class CVControl
            {
                /// <summary>
                /// Индекс свойства [Выбор канала].
                /// </summary>
                public readonly string Channel = "b74d6fe77f4b422893c3d3cad36d6cf5";

                /// <summary>
                /// Индекс свойства [вкл/выкл интерфейса].
                /// </summary>
                public readonly string IntfOn = "64d3be4806ce45b28bcc57cc66a4cea1";

                /// <summary>
                /// Индекс свойства [Установлена связь].
                /// </summary>
                public readonly string Connected = "d0abb25baaf048e18e5d4e0c1c0eb38b";

                /// <summary>
                /// Неявно преобразует объект к типу "строка". 
                /// </summary>
                /// <param name="obj">Объект для преобразования.</param>
                /// <returns>Возвращает строковое представления объекта.</returns>
                public static implicit operator string(CVControl obj)
                {
                    return "6985c157c54e4af59368589146760b00";
                }
            }

            /// <summary>
            /// Индексы свойств [Запись данных(до 1 Кбайт)].
            /// </summary>
            public class CVRecord
            {
                /// <summary>
                /// Индекс свойства [Выдача посылки RMAP (самосбр.)].
                /// </summary>
                public readonly string SendRMAP = "2eb72401c6f5496dbf74ad944446ad13";

                /// <summary>
                /// Индекс свойства [1 – выдача посылки в прибор БС (самосбр.)].
                /// </summary>
                public readonly string SendBuk = "26f797306bda451e82abf3dee854071f";

                /// <summary>
                /// Индекс свойства [1 – выдача посылки в прибор БКП (самосбр.)].
                /// </summary>
                public readonly string SendBkp = "36a141f764b84ab4bd18abdfc028bc41";

                /// <summary>
                /// Неявно преобразует объект к типу "строка". 
                /// </summary>
                /// <param name="obj">Объект для преобразования.</param>
                /// <returns>Возвращает строковое представления объекта.</returns>
                public static implicit operator string(CVRecord obj)
                {
                    return "acd2aef5704546a9b3f3b947ffdab2fc";
                }
            }

            /// <summary>
            /// Индекс свойства [Адрес ИМИТАТОРА БУСКа].
            /// </summary>
            public class CVSPTPLogicBusk
            {
                /// <summary>
                /// Неявно преобразует объект к типу "строка". 
                /// </summary>
                /// <param name="obj">Объект для преобразования.</param>
                /// <returns>Возвращает строковое представления объекта.</returns>
                public static implicit operator string(CVSPTPLogicBusk obj)
                {
                    return "41f85f8a6e3e4c669158fc2f204ee166";
                }
            }

            /// <summary>
            /// Индекс свойства [Адрес БС].
            /// </summary>
            public class CVSPTPLogicBuk
            {
                /// <summary>
                /// Неявно преобразует объект к типу "строка". 
                /// </summary>
                /// <param name="obj">Объект для преобразования.</param>
                /// <returns>Возвращает строковое представления объекта.</returns>
                public static implicit operator string(CVSPTPLogicBuk obj)
                {
                    return "70151354c60943e29ebca8f1801176bb";
                }
            }

            /// <summary>
            /// Индекс свойства [Адрес БКП].
            /// </summary>
            public class CVSPTPLogicBkp
            {
                /// <summary>
                /// Неявно преобразует объект к типу "строка". 
                /// </summary>
                /// <param name="obj">Объект для преобразования.</param>
                /// <returns>Возвращает строковое представления объекта.</returns>
                public static implicit operator string(CVSPTPLogicBkp obj)
                {
                    return "5bedbdbc33e54ed4b3c60b35d2348760";
                }
            }

            /// <summary>
            /// Индекс свойства [Управление обменом с приборами по SPTP].
            /// </summary>
            public class CVSPTPControl
            {
                /// <summary>
                /// Индекс свойства [можно выдавать пакет в БКП].
                /// </summary>
                public readonly string BkpTransData = "3e19221ddd6b4aab838307a8e342f193";

                /// <summary>
                /// Индекс свойства [выдача КБВ прибору БКП (только при «1 PPS» == 1)].
                /// </summary>
                public readonly string BkpKbv = "57cbe2d9fd13479aa4e3eb3c14939471";

                /// <summary>
                /// Индекс свойства [включение обмена прибора БКП].
                /// </summary>
                public readonly string BkpTrans = "2d1516e3b36e4490bb2984e23ec18d34";

                /// <summary>
                /// Индекс свойства [можно выдавать пакет в БС].
                /// </summary>
                public readonly string BukTransData = "738ad555b28c4f8b83754699aaad66de";

                /// <summary>
                /// Индекс свойства [выдача КБВ прибору БС (только при «1 PPS» == 1)].
                /// </summary>
                public readonly string BukKbv = "dedd64001ded40678a1706e91b3ecf61";

                /// <summary>
                /// Индекс свойства [включение обмена прибора БС].
                /// </summary>
                public readonly string BukTrans = "c227d91e647f4b90a1f69cd5e18638e5";

                /// <summary>
                /// Индекс свойства [включить выдачу секундных меток (1PPS)].
                /// </summary>
                public readonly string TimeMark = "01967fc75d7e4c9380a00fc1a08abee9";

                /// <summary>
                /// Неявно преобразует объект к типу "строка". 
                /// </summary>
                /// <param name="obj">Объект для преобразования.</param>
                /// <returns>Возвращает строковое представления объекта.</returns>
                public static implicit operator string(CVSPTPControl obj)
                {
                    return "4fd6ac7f475d427ba59c17e41fa8a111";
                }
            }
        }

        /// <summary>
        /// Индексы свойств [SPACEWIRE 4 (ИМИТАТОР ДЕТЕКТОРОВ (БУК))]
        /// </summary>
        public static class Spacewire4
        {
            /// <summary>
            /// Инициализирует статические поля класса <see cref="Spacewire4" />.
            /// </summary>
            static Spacewire4()
            {
                Control = new CVControl();
                Record = new CVRecord();
            }

            /// <summary>
            /// Получает индекс объекта "Управление" в массиве ControlValuesList.
            /// </summary>
            public static CVControl Control { get; private set; }

            /// <summary>
            /// Получает индекс объекта "Запись данных(до 1 Кбайт)" в массиве ControlValuesList.
            /// </summary>
            public static CVRecord Record { get; private set; }

            /// <summary>
            /// Индексы свойств [Управление].
            /// </summary>
            public class CVControl
            {
                /// <summary>
                /// Индекс свойства [вкл/выкл интерфейса].
                /// </summary>
                public readonly string IntfOn = "fcf9489c447d435189e3d2fb71ada618";

                /// <summary>
                /// Индекс свойства [Установлена связь].
                /// </summary>
                public readonly string Connected = "18842e59a45e4e7b807878cd05726858";

                /// <summary>
                /// Индекс свойства [Включение метки времени (1 Гц)].
                /// </summary>
                public readonly string TimeMark = "97f6534716484e0a87b93e2d272497fa";

                /// <summary>
                /// Неявно преобразует объект к типу "строка". 
                /// </summary>
                /// <param name="obj">Объект для преобразования.</param>
                /// <returns>Возвращает строковое представления объекта.</returns>
                public static implicit operator string(CVControl obj)
                {
                    return "66a08b01518b4b8a92762bb49febe656";
                }
            }

            /// <summary>
            /// Индексы свойств [Запись данных(до 1 Кбайт)].
            /// </summary>
            public class CVRecord
            {
                /// <summary>
                /// Индекс свойства [EEP или EOP].
                /// </summary>
                public readonly string EEPSend = "2dc3af5bdee944879a5ac25dd6a45e92";

                /// <summary>
                /// Индекс свойства [Выдача в конце посылки EOP или EEP].
                /// </summary>
                public readonly string EOPSend = "30f280120fa74d4684407c9a311f99d8";

                /// <summary>
                /// Индекс свойства [Автоматическая выдача].
                /// </summary>
                public readonly string AutoSend = "99538c248ef244aaa03885d1853caafe";

                /// <summary>
                /// Индекс свойства [Бит занятости].
                /// </summary>
                public readonly string RecordBusy = "c1d34d90621240bdbb6296af31b3e0c0";

                /// <summary>
                /// Индекс свойства [Бит выдачи посылки].
                /// </summary>
                public readonly string RecordSend = "66facd91e95048308f194e726113804a";

                /// <summary>
                /// Неявно преобразует объект к типу "строка". 
                /// </summary>
                /// <param name="obj">Объект для преобразования.</param>
                /// <returns>Возвращает строковое представления объекта.</returns>
                public static implicit operator string(CVRecord obj)
                {
                    return "1e560484a6514be0a7bc64e28d1f12a1";
                }
            }
        }

        /// <summary>
        /// Индексы свойств [Питание прибора]
        /// </summary>
        public class CVPower
        {
            /// <summary>
            /// Неявно преобразует объект к типу "строка". 
            /// </summary>
            /// <param name="obj">Объект для преобразования.</param>
            /// <returns>Возвращает строковое представления объекта.</returns>
            public static implicit operator string(CVPower obj)
            {
                return "3a9e7480692c4912861d5fdf7b69c884";
            }
        }

        /// <summary>
        /// Индексы свойств [Телеметрия]
        /// </summary>
        public class CVTelemetry 
        {
            /// <summary>
            /// Индекс свойства [Телеметрия: Запитан ПК1 от БУСК].
            /// </summary>
            public readonly string PowerBusk1 = "60f650d449a24116b5e08253dad27540";

            /// <summary>
            /// Индекс свойства [Телеметрия: Запитан ПК2 от БУСК].
            /// </summary>
            public readonly string PowerBusk2 = "eff5c482574c4554a9afea0a1f9fc89c";

            /// <summary>
            /// Индекс свойства [Телеметрия: Запитан ПК1 от БУНД].
            /// </summary>
            public readonly string PowerBund1 = "604e214e85cf4793bd50b2a02c77aafb";

            /// <summary>
            /// Индекс свойства [Телеметрия: Запитан ПК2 от БУНД].
            /// </summary>
            public readonly string PowerBund2 = "37d7bd49e0c44b9cac09478f117c63c6";

            /// <summary>
            /// Неявно преобразует объект к типу "строка". 
            /// </summary>
            /// <param name="obj">Объект для преобразования.</param>
            /// <returns>Возвращает строковое представления объекта.</returns>
            public static implicit operator string(CVTelemetry obj)
            {
                return "e302dd463fb94adaaf8e9e2cd049cfe6";
            }
        }
    }
}
