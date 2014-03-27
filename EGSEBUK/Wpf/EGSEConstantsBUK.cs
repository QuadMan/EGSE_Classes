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
    using System.Windows.Data;
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
        public static readonly string ShowCaption = InitializeResourceString(@"stShowCaption");

        /// <summary>
        /// Название КИА.
        /// </summary>
        public static readonly string DeviceName = InitializeResourceString(@"stDeviceName");

        /// <summary>
        /// Уникальный идентификатор USB.
        /// </summary>
        public static readonly string DeviceSerial = InitializeResourceString(@"stDeviceSerial");     

        /// <summary>
        /// Инициализирует статические поля класса <see cref="Global" />.
        /// </summary>
        static Global()
        {
            Telemetry = new CVTelemetry();
            Shutters = new CVShutters();
        }

        /// <summary>
        /// Получает индекс объекта "Телеметрия" в массиве ControlValuesList.
        /// </summary>
        internal static CVTelemetry Telemetry { get; private set; }

        /// <summary>
        /// Получает индекс объекта "Датчики затворов" в массиве ControlValuesList.
        /// </summary>
        internal static CVShutters Shutters { get; private set; }

        /// <summary>
        /// Расширяющий метод представления байт в формате: 0,00 байт/КБ/МБ/ГБ.
        /// </summary>
        /// <param name="obj">Расширяемый класс.</param>
        /// <param name="value">Значение для преобразования.</param>
        /// <returns>
        /// Строка в формате: 0,00 байт/КБ/МБ/ГБ.
        /// </returns>
        public static string ReadableByte(this IValueConverter obj, float value)
        {
            string[] sizes = { "байт", "КБ", "МБ", "ГБ" };
            int order = 0;
            while (value >= 1024 && order + 1 < sizes.Length)
            {
                order++;
                value = value / 1024;
            }

            return string.Format("{0:0.##} {1}", value, sizes[order]);
        }

        /// <summary>
        /// Инициализирует статические строки из ресурсов.
        /// </summary>
        /// <param name="resourceName">Наименование ресурса.</param>
        /// <returns>Значение указанного ресурса.</returns>
        internal static string InitializeResourceString(string resourceName)
        {
            return Resource.Get(resourceName);
        }

        /// <summary>
        /// Индексы свойств [ВСИ (ИМИТАТОР КВВ)]
        /// </summary>
        public static class HSI
        {
            /// <summary>
            /// Инициализирует статические поля класса <see cref="HSI" />.
            /// </summary>
            static HSI()
            {
                Line1 = new CVLine1();
                Line1StateCounter = new CVLine1StateCounter();
                Line1FrameCounter = new CVLine1FrameCounter();
                Line2 = new CVLine2();
                Line2StateCounter = new CVLine2StateCounter();
                Line2FrameCounter = new CVLine2FrameCounter();

                State = new CVState();
            }

            /// <summary>
            /// Получает индекс объекта "Дополнительные биты (продолжение имитатора КВВ)" в массиве ControlValuesList.
            /// </summary>
            public static CVState State { get; private set; }

            /// <summary>
            /// Получает индекс объекта "КВВ ПК1" в массиве ControlValuesList.
            /// </summary>
            public static CVLine1 Line1 { get; private set; }

            /// <summary>
            /// Получает индекс объекта "КВВ ПК1 : Счетчик выданных статусов" в массиве ControlValuesList.
            /// </summary>
            public static CVLine1StateCounter Line1StateCounter { get; private set; }

            /// <summary>
            /// Получает индекс объекта "КВВ ПК1 : Счетчик выданных кадров" в массиве ControlValuesList.
            /// </summary>
            public static CVLine1FrameCounter Line1FrameCounter { get; private set; }

            /// <summary>
            /// Получает индекс объекта "КВВ ПК2" в массиве ControlValuesList.
            /// </summary>
            public static CVLine2 Line2 { get; private set; }

            /// <summary>
            /// Получает индекс объекта "КВВ ПК2 : Счетчик выданных статусов" в массиве ControlValuesList.
            /// </summary>
            public static CVLine2StateCounter Line2StateCounter { get; private set; }

            /// <summary>
            /// Получает индекс объекта "КВВ ПК2 : Счетчик выданных кадров" в массиве ControlValuesList.
            /// </summary>
            public static CVLine2FrameCounter Line2FrameCounter { get; private set; }

            /// <summary>
            /// Индексы свойств [КВВ ПК1].
            /// </summary>
            public class CVLine1
            {
                /// <summary>
                /// Индекс свойства [Вкл/выкл].
                /// </summary>
                public readonly string IssueEnable = "29de114f373d4db9a52a90c91c840902";
               
                /// <summary>
                /// Индекс свойства [Линия передачи].
                /// </summary>
                public readonly string Line = "3cbe73537ddb4cb9a7af999eff3e6324";

                /// <summary>
                /// Неявно преобразует объект к типу "строка". 
                /// </summary>
                /// <param name="obj">Объект для преобразования.</param>
                /// <returns>Возвращает строковое представления объекта.</returns>
                public static implicit operator string(CVLine1 obj)
                {
                    return "e8ae7f4e0d5d40a38a5351f963b4c523";
                }
            }

            /// <summary>
            /// Индексы свойств [Дополнительные байты].
            /// </summary>
            public class CVState
            {
                /// <summary>
                /// Индекс свойства [ПК1 статус: готов].
                /// </summary>
                public readonly string IssueReady1 = "1c1652b38a0a49678353b28ebdabdad9";

                /// <summary>
                /// Индекс свойства [ПК1 статус: busy].
                /// </summary>
                public readonly string IssueBusy1 = "29de114f373d4db9a52a90c91c840902";

                /// <summary>
                /// Индекс свойства [ПК1 статус: me].
                /// </summary>
                public readonly string IssueMe1 = "4085abfce08a4ba9b1bf0203b6797a3e";

                /// <summary>
                /// Индекс свойства [ПК1 активен].
                /// </summary>
                public readonly string Active1 = "4085abfce08a4ba9b1bf0203b6797a3e";

                /// <summary>
                /// Индекс свойства [ПК2 статус: готов].
                /// </summary>
                public readonly string IssueReady2 = "06309fea00e74ab8ac6680f37525d71b";

                /// <summary>
                /// Индекс свойства [ПК2 статус: busy].
                /// </summary>
                public readonly string IssueBusy2 = "3f8b05b055cd46a3af3c190c6d188e13";

                /// <summary>
                /// Индекс свойства [ПК2 статус: me].
                /// </summary>
                public readonly string IssueMe2 = "f25b4a223abf4e338aeaa700f0d866a4";

                /// <summary>
                /// Индекс свойства [ПК2 активен].
                /// </summary>
                public readonly string Active2 = "83e3d1f5d0af49d1a37063a618ada8a9";

                /// <summary>
                /// Неявно преобразует объект к типу "строка". 
                /// </summary>
                /// <param name="obj">Объект для преобразования.</param>
                /// <returns>Возвращает строковое представления объекта.</returns>
                public static implicit operator string(CVState obj)
                {
                    return "29dd3908502f41a4905676f89be55b0d";
                }
            }

            /// <summary>
            /// Индексы свойств [КВВ ПК2].
            /// </summary>
            public class CVLine2
            {
                /// <summary>
                /// Индекс свойства [Вкл/выкл].
                /// </summary>
                public readonly string IssueEnable = "c7837417edd743eabe09ff6733c333e3";

                /// <summary>
                /// Индекс свойства [Линия передачи].
                /// </summary>
                public readonly string Line = "cf351176e276489dac1248af4fb043ec";

                /// <summary>
                /// Неявно преобразует объект к типу "строка". 
                /// </summary>
                /// <param name="obj">Объект для преобразования.</param>
                /// <returns>Возвращает строковое представления объекта.</returns>
                public static implicit operator string(CVLine2 obj)
                {
                    return "b458476250d543579d41f34c182370c3";
                }
            }

            /// <summary>
            /// Индексы свойств [КВВ ПК1 : Счетчик выданных статусов].
            /// </summary>
            public class CVLine1StateCounter
            {
                /// <summary>
                /// Неявно преобразует объект к типу "строка". 
                /// </summary>
                /// <param name="obj">Объект для преобразования.</param>
                /// <returns>Возвращает строковое представления объекта.</returns>
                public static implicit operator string(CVLine1StateCounter obj)
                {
                    return "990d7e0b260142478b16edec23c7b831";
                }
            }

            /// <summary>
            /// Индексы свойств [КВВ ПК2 : Счетчик выданных статусов].
            /// </summary>
            public class CVLine2StateCounter
            {
                /// <summary>
                /// Неявно преобразует объект к типу "строка". 
                /// </summary>
                /// <param name="obj">Объект для преобразования.</param>
                /// <returns>Возвращает строковое представления объекта.</returns>
                public static implicit operator string(CVLine2StateCounter obj)
                {
                    return "d1ca6e9f89734002a6c4fb94f6b32083";
                }
            }

            /// <summary>
            /// Индексы свойств [КВВ ПК1 : Счетчик выданных кадров].
            /// </summary>
            public class CVLine1FrameCounter
            {
                /// <summary>
                /// Неявно преобразует объект к типу "строка". 
                /// </summary>
                /// <param name="obj">Объект для преобразования.</param>
                /// <returns>Возвращает строковое представления объекта.</returns>
                public static implicit operator string(CVLine1FrameCounter obj)
                {
                    return "b4061364e1df44ae92f1f8ab54f99e24";
                }
            }

            /// <summary>
            /// Индексы свойств [КВВ ПК2 : Счетчик выданных кадров].
            /// </summary>
            public class CVLine2FrameCounter
            {
                /// <summary>
                /// Неявно преобразует объект к типу "строка". 
                /// </summary>
                /// <param name="obj">Объект для преобразования.</param>
                /// <returns>Возвращает строковое представления объекта.</returns>
                public static implicit operator string(CVLine2FrameCounter obj)
                {
                    return "ac7a515827874052869d942b698bc968";
                }
            }
        }

        /// <summary>
        /// Индексы свойств [ВСИ (ИМИТАТОР БУК)]
        /// </summary>
        public static class SimHSI
        {
            /// <summary>
            /// Инициализирует статические поля класса <see cref="SimHSI" />.
            /// </summary>
            static SimHSI()
            {
                Control = new CVControl();
                Record = new CVRecord();
            }

            /// <summary>
            /// Получает индекс объекта "Управление" в массиве ControlValuesList.
            /// </summary>
            public static CVControl Control { get; private set; }

            /// <summary>
            /// Получает индекс объекта "Выдача УКС" в массиве ControlValuesList.
            /// </summary>
            public static CVRecord Record { get; private set; }

            /// <summary>
            /// Индексы свойств [Управление].
            /// </summary>
            public class CVControl
            {
                /// <summary>
                /// Индекс свойства [Линия приема].
                /// </summary>
                public readonly string LineIn = "47dd05f1b4ef4954885f28a2bda3ed0f";

                /// <summary>
                /// Индекс свойства [Линия передачи].
                /// </summary>
                public readonly string LineOut = "1ff03a21678340b2bf270d98c51dfae4";

                /// <summary>
                /// Индекс свойства [Опрос данных].
                /// </summary>
                public readonly string IssueRequest = "e7864f46ec024878919567582b165b12";

                /// <summary>
                /// Неявно преобразует объект к типу "строка". 
                /// </summary>
                /// <param name="obj">Объект для преобразования.</param>
                /// <returns>Возвращает строковое представления объекта.</returns>
                public static implicit operator string(CVControl obj)
                {
                    return "0e4d597a2a8f4cdf95b45b273581e246";
                }
            }

            /// <summary>
            /// Индексы свойств [Выдача УКС].
            /// </summary>
            public class CVRecord
            {
                /// <summary>
                /// Индекс свойства [Выдача УКС].
                /// </summary>
                public readonly string IssueCmd = "d10b58d4d0f14ae995a8f79631ceb894";

                /// <summary>
                /// Неявно преобразует объект к типу "строка". 
                /// </summary>
                /// <param name="obj">Объект для преобразования.</param>
                /// <returns>Возвращает строковое представления объекта.</returns>
                public static implicit operator string(CVRecord obj)
                {
                    return "98524df893b84c47b6e0d61fa7766c75";
                }
            }
        }

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
                BuskLogic = new CVSPTPLogicBusk();
                SD1Logic = new CVSPTPLogicNP1();
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
            public static CVSPTPLogicBusk BuskLogic { get; private set; }

            /// <summary>
            /// Получает индекс объекта "SPTP Адрес НП1" в массиве ControlValuesList.
            /// </summary>
            public static CVSPTPLogicNP1 SD1Logic { get; private set; }

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
                public readonly string IssueEnable = "c1c63372d0464a7581628acbb8e2f5f2";

                /// <summary>
                /// Индекс свойства [Установлена связь].
                /// </summary>
                public readonly string Connect = "feba0ba15e3040c89ff40f76ce5265ed";

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
                public readonly string IssuePackage = "08283fd972724eaea196ca125fd1f0d4";

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
                public readonly string SD1Trans = "6612f20286a24221bda280ae3514af14";

                /// <summary>
                /// Индекс свойства [Включение обмена SPTP (НП2)].
                /// </summary>
                public readonly string SD2Trans = "a1fb6e2a1f5f43d88fbc17bd73655003";

                /// <summary>
                /// Индекс свойства [Разрешение выдачи пакетов данных по SPTP(НП1)].
                /// </summary>
                public readonly string SD1TransData = "b7bf0378fbb744e7bf56df93937a11c0";

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
                BuskLogic = new CVSPTPLogicBusk();
                BukLogic = new CVSPTPLogicBuk();
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
            public static CVSPTPLogicBusk BuskLogic { get; private set; }

            /// <summary>
            /// Получает индекс объекта "Адрес БС" в массиве ControlValuesList.
            /// </summary>
            public static CVSPTPLogicBuk BukLogic { get; private set; }

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
                public readonly string IssueEnable = "64d3be4806ce45b28bcc57cc66a4cea1";

                /// <summary>
                /// Индекс свойства [Установлена связь].
                /// </summary>
                public readonly string Connect = "d0abb25baaf048e18e5d4e0c1c0eb38b";

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
                public readonly string IssueRMap = "2eb72401c6f5496dbf74ad944446ad13";

                /// <summary>
                /// Индекс свойства [1 – выдача посылки в прибор БС (самосбр.)].
                /// </summary>
                public readonly string IssuePackage = "26f797306bda451e82abf3dee854071f";

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
                public readonly string TransData = "738ad555b28c4f8b83754699aaad66de";

                /// <summary>
                /// Индекс свойства [выдача КБВ прибору БС (только при «1 PPS» == 1)].
                /// </summary>
                public readonly string IssueKbv = "dedd64001ded40678a1706e91b3ecf61";

                /// <summary>
                /// Индекс свойства [включение обмена прибора БС].
                /// </summary>
                public readonly string IssueTrans = "c227d91e647f4b90a1f69cd5e18638e5";

                /// <summary>
                /// Индекс свойства [включить выдачу секундных меток (1PPS)].
                /// </summary>
                public readonly string IssueTimeMark = "01967fc75d7e4c9380a00fc1a08abee9";

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
        /// Индексы свойств [SPACEWIRE 3 (ИМИТАТОР УФЕС, ВУФЕС, СДЩ)]
        /// </summary>
        public static class Spacewire3
        {
            /// <summary>
            /// Инициализирует статические поля класса <see cref="Spacewire3" />.
            /// </summary>
            static Spacewire3()
            {
                Control = new CVControl();
            }

            /// <summary>
            /// Получает индекс объекта "Управление" в массиве ControlValuesList.
            /// </summary>
            public static CVControl Control { get; private set; }

            /// <summary>
            /// Индексы свойств [Управление].
            /// </summary>
            public class CVControl
            {
                /// <summary>
                /// Индекс свойства [вкл/выкл интерфейса].
                /// </summary>
                public readonly string IssueEnable = "a93332e69940441ba93247b7c962eff9";

                /// <summary>
                /// Индекс свойства [Установлена связь].
                /// </summary>
                public readonly string Connect = "f86f7cff64444b89941375109b499a71";

                /// <summary>
                /// Индекс свойства [Выбор рабочего прибора].
                /// </summary>
                public readonly string WorkDevice = "e492ba45d31b49b58178e00f519d7ad1";

                /// <summary>
                /// Индекс свойства [Выбор полукомплекта].
                /// </summary>
                public readonly string HalfSet = "7606efd54556417fb8a0ee3267eee449";

                /// <summary>
                /// Индекс свойства [Сигнал передачи кадров].
                /// </summary>
                public readonly string Transmission = "b265799f39d44309b49263c924b2b26c";

                /// <summary>
                /// Неявно преобразует объект к типу "строка". 
                /// </summary>
                /// <param name="obj">Объект для преобразования.</param>
                /// <returns>Возвращает строковое представления объекта.</returns>
                public static implicit operator string(CVControl obj)
                {
                    return "62d4ddf675c54f628307456ed86fdd0e";
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
                public readonly string IssueEnable = "fcf9489c447d435189e3d2fb71ada618";

                /// <summary>
                /// Индекс свойства [Установлена связь].
                /// </summary>
                public readonly string Connect = "18842e59a45e4e7b807878cd05726858";

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
                public readonly string IssueEEP = "2dc3af5bdee944879a5ac25dd6a45e92";

                /// <summary>
                /// Индекс свойства [Выдача в конце посылки EOP или EEP].
                /// </summary>
                public readonly string EOPSend = "30f280120fa74d4684407c9a311f99d8";

                /// <summary>
                /// Индекс свойства [Автоматическая выдача].
                /// </summary>
                public readonly string IssueAuto = "99538c248ef244aaa03885d1853caafe";

                /// <summary>
                /// Индекс свойства [Бит занятости].
                /// </summary>
                public readonly string RecordBusy = "c1d34d90621240bdbb6296af31b3e0c0";

                /// <summary>
                /// Индекс свойства [Бит выдачи посылки].
                /// </summary>
                public readonly string IssuePackage = "66facd91e95048308f194e726113804a";

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
            /// Индекс свойства [Телеметрия:  Питание УФЕС (осн)].
            /// </summary>
            public readonly string UfesPower1 = "77847f912b2842fd954a53454b1283af";

            /// <summary>
            /// Индекс свойства [Телеметрия:  Питание УФЕС (рез)].
            /// </summary>
            public readonly string UfesPower2 = "eac13e31010a4639a7bb1a8eaa1e7059";

            /// <summary>
            /// Индекс свойства [Телеметрия:  Питание ВУФЕС (осн)].
            /// </summary>
            public readonly string VufesPower1 = "b0efec8196ff41a19193da4432342647";

            /// <summary>
            /// Индекс свойства [Телеметрия:  Питание ВУФЕС (рез)].
            /// </summary>
            public readonly string VufesPower2 = "94e0bcb218424ed9bd4d7c2365996aa9";

            /// <summary>
            /// Индекс свойства [Телеметрия:  Питание СДЩ (осн)].
            /// </summary>
            public readonly string SdchshPower1 = "b59d3528df6b4e9887aa3754e225758a";

            /// <summary>
            /// Индекс свойства [Телеметрия:  Питание СДЩ (рез)].
            /// </summary>
            public readonly string SdchshPower2 = "4c1443fda8bf48bdb019ef8e99325315";

            /// <summary>
            /// Индекс свойства [Телеметрия:  Подсветка УФЕС (осн)].
            /// </summary>
            public readonly string UfesLight1 = "8bd191047cd345759fd844f6c4b643ff";

            /// <summary>
            /// Индекс свойства [Телеметрия:  Подсветка УФЕС (рез)].
            /// </summary>
            public readonly string UfesLight2 = "cb102496daac44ffa4136b4c071df11f";

            /// <summary>
            /// Индекс свойства [Телеметрия:  Подсветка ВУФЕС (осн)].
            /// </summary>
            public readonly string VufesLight1 = "db83722ee76f43f8a8688ccdbac81ef8";

            /// <summary>
            /// Индекс свойства [Телеметрия:  Подсветка ВУФЕС (рез)].
            /// </summary>
            public readonly string VufesLight2 = "0ad388efb4b641688a4134e613a9437e";

            /// <summary>
            /// Индекс свойства [Телеметрия:  Подсветка СДЩ (осн)].
            /// </summary>
            public readonly string SdchshLight1 = "09033a4f5c064af58241ce7bff73198a";

            /// <summary>
            /// Индекс свойства [Телеметрия:  Подсветка СДЩ (рез)].
            /// </summary>
            public readonly string SdchshLight2 = "979aa9840c644636988aa7e96487a973";

            /// <summary>
            /// Индекс свойства [Телеметрия:  Затвор УФЕС (осн)].
            /// </summary>
            public readonly string UfesLock1 = "33d4913434d045d69f8605894ad38b37";
           
            /// <summary>
            /// Индекс свойства [Телеметрия:  Затвор УФЕС (рез)].
            /// </summary>
            public readonly string UfesLock2 = "c2b0b0bba8de4e94abbf7e6a9c8a8ee1";

            /// <summary>
            /// Индекс свойства [Телеметрия: Затвор ВУФЕС (осн)].
            /// </summary>
            public readonly string VufesLock1 = "add2a3069a9a436884e08e707b82a3ae";

            /// <summary>
            /// Индекс свойства [Телеметрия: Затвор ВУФЕС (рез)].
            /// </summary>
            public readonly string VufesLock2 = "a8bf2e88fd684430820e455b6f841df8";

            /// <summary>
            /// Индекс свойства [Телеметрия: Затвор СДЩ (осн)].
            /// </summary>
            public readonly string SdchshLock1 = "e4185fd13e764a90adfa76406eef91f8";

            /// <summary>
            /// Индекс свойства [Телеметрия: Затвор СДЩ (рез)].
            /// </summary>
            public readonly string SdchshLock2 = "1918b080cf9f4755b05e4e97dc8b4cca";
                            
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

        /// <summary>
        /// Индексы свойств [Датчики затворов].
        /// </summary>
        internal class CVShutters
        {
            /// <summary>
            /// Индекс свойства [Датчики затворов: автоматическое управление].
            /// </summary>
            public readonly string Auto = "0dd027489d2841b393f27bd24e1ba827";

            /// <summary>
            /// Индекс свойства [УФЕС: Датчики затворов: открытие].
            /// </summary>
            public readonly string UfesOpen = "1a9eaeba16f746abbd76879400733dd6";

            /// <summary>
            /// Индекс свойства [УФЕС: Датчики затворов: закрытие].
            /// </summary>
            public readonly string UfesClose = "014d72354f774310b3c2d2c05980e72c";

            /// <summary>
            /// Индекс свойства [ВУФЕС: Датчики затворов: открытие].
            /// </summary>
            public readonly string VufesOpen = "b80a1dc893ad4489866979d2a76f0724";

            /// <summary>
            /// Индекс свойства [ВУФЕС: Датчики затворов: закрытие].
            /// </summary>
            public readonly string VufesClose = "ec63f6626d73473ca4dc227a0ba1fb59";

            /// <summary>
            /// Индекс свойства [СДЩ: Датчики затворов: открытие].
            /// </summary>
            public readonly string SdchshOpen = "29b4bbd9acbd4976a67a9198abd093d7";

            /// <summary>
            /// Индекс свойства [СДЩ: Датчики затворов: закрытие].
            /// </summary>
            public readonly string SdchshClose = "f45ba7d713994f2f80e67c92b67cbe87";

            /// <summary>
            /// Неявно преобразует объект к типу "строка". 
            /// </summary>
            /// <param name="obj">Объект для преобразования.</param>
            /// <returns>Возвращает строковое представления объекта.</returns>
            public static implicit operator string(CVShutters obj)
            {
                return "3b48052591d24dbd8e72d922c8721786";
            }
        }
    }
}
