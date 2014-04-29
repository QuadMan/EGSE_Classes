//-----------------------------------------------------------------------
// <copyright file="EGSECyclogramCommandBUK.cs" company="IKI RSSI, laboratory №711">
//     Copyright (c) IKI RSSI, laboratory №711. All rights reserved.
// </copyright>
// <author>Семенов Александр, Коробейщиков Иван</author>
//-----------------------------------------------------------------------

namespace Egse.Cyclogram.Command
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Globalization;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Windows;
    using Egse.CustomAttributes;
    using Egse.Cyclogram;
    using Egse.Defaults;
    using Egse.Devices;
    using Egse.Utilites;

    /// <summary>
    /// Инициализирует набор доступных цикломанд для устройства.
    /// </summary>
    public class CyclogramCommandBuk
    {
        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="CyclogramCommandBuk" />.
        /// </summary>
        public CyclogramCommandBuk()
        {
            CyclogramCommandsAvailable = new CyclogramCommands();
            CyclogramCommandsAvailable.AddCommand("62ac720d56ce449d86ca2cde439ba75a", new CyclogramLine("POWER", PowerTest, PowerExec, string.Empty));
            CyclogramCommandsAvailable.AddCommand("efc854841c554c529966abe5f1e6c2d7", new CyclogramLine("SHUTTER", ShutterTest, ShutterExec, string.Empty));
            CyclogramCommandsAvailable.AddCommand("b1f06daf0810463892707e7edf5d93be", new CyclogramLine("KVV_IMIT", KvvImitTest, KvvImitExec, string.Empty));
            CyclogramCommandsAvailable.AddCommand("954bcc4e3ea240ddacd8766fcc72c9bf", new CyclogramLine("KVV_IMIT_LOG", KvvImitLogTest, KvvImitLogExec, string.Empty));
            CyclogramCommandsAvailable.AddCommand("e333bfb23a204e748699528fb787012c", new CyclogramLine("BM4_IMIT", Bm4ImitTest, Bm4ImitExec, string.Empty));
            CyclogramCommandsAvailable.AddCommand("9e3e94f708eb44819a8ea9d01aa321e9", new CyclogramLine("BM4_IMIT_CMD", Bm4ImitCmdTest, Bm4ImitCmdExec, string.Empty));
            CyclogramCommandsAvailable.AddCommand("1198f53b3a9a4a6b9c69f3ac6ebe466f", new CyclogramLine("BM4_IMIT_LOG", Bm4ImitLogTest, Bm4ImitLogExec, string.Empty));
            CyclogramCommandsAvailable.AddCommand("bdeae631daf8489983760003e615852a", new CyclogramLine("DETECTOR_IMIT", DetectorImitTest, DetectorImitExec, string.Empty));
            CyclogramCommandsAvailable.AddCommand("fac9ef960e6b4a55b48ecd13f51d4acf", new CyclogramLine("DETECTOR_IMIT_LOG", DetectorImitLogTest, DetectorImitLogExec, string.Empty));
            CyclogramCommandsAvailable.AddCommand("2e92651815794233bc062365b3e8278b", new CyclogramLine("BUK_KVV_IMIT", BukKvvImitTest, BukKvvImitExec, string.Empty));
            CyclogramCommandsAvailable.AddCommand("ec4d4c82d1044d3fbcad59d4403c54aa", new CyclogramLine("BUK_KVV_IMIT_CMD", BukKvvImitCmdTest, BukKvvImitCmdExec, string.Empty));
            CyclogramCommandsAvailable.AddCommand("c8567a505600474fb4f8ab24a5a11246", new CyclogramLine("BUK_BM4_IMIT", BukBm4ImitTest, BukBm4ImitExec, string.Empty));
            CyclogramCommandsAvailable.AddCommand("e1ee52714fa547f38701f984328913d9", new CyclogramLine("BUK_DETECTOR_IMIT", BukDetectorImitTest, BukDetectorImitExec, string.Empty));
            CyclogramCommandsAvailable.AddCommand("a4c3ce1691ca4135abdf865959c81b8a", new CyclogramLine("BUK_DETECTOR_IMIT_CMD", BukDetectorImitCmdTest, BukDetectorImitCmdExec, string.Empty));
        }

        /// <summary>
        /// Аргумент "статус для полукомплекта".
        /// </summary>
        private enum StateReady
        {
            /// <summary>
            /// Параметр "готов" аргумента.
            /// </summary>
            [Action(typeof(Execut), "StateReady")]
            READY
        }

        /// <summary>
        /// Аргумент "статус для полукомплекта".
        /// </summary>
        private enum StateBusy
        {
            /// <summary>
            /// Параметр "занят" аргумента.
            /// </summary>
            [Action(typeof(Execut), "StateBusy")]
            BUSY
        }

        /// <summary>
        /// Аргумент "статус для полукомплекта".
        /// </summary>
        private enum StateMe
        {
            /// <summary>
            /// Параметр "me" аргумента.
            /// </summary>
            [Action(typeof(Execut), "StateMe")]
            ME
        }

        /// <summary>
        /// Аргумент "переключатель состояния".
        /// </summary>
        private enum SwitcherDetectorImit
        {
            /// <summary>
            /// Параметр "включения/активации" аргумента.
            /// </summary>
            [Action(typeof(Execut), "SwitcherDetectorImitOn")]
            ON,

            /// <summary>
            /// Параметр "выключения/деактивации" аргумента.
            /// </summary>
            [Action(typeof(Execut), "SwitcherDetectorImitOff")]
            OFF
        }

        /// <summary>
        /// Аргумент "переключатель состояния".
        /// </summary>
        private enum SwitcherBukBm4Imit
        {
            /// <summary>
            /// Параметр "включения/активации" аргумента.
            /// </summary>
            [Action(typeof(Execut), "SwitcherBukBm4ImitOn")]
            ON,

            /// <summary>
            /// Параметр "выключения/деактивации" аргумента.
            /// </summary>
            [Action(typeof(Execut), "SwitcherBukBm4ImitOff")]
            OFF
        }

        /// <summary>
        /// Аргумент "переключатель состояния".
        /// </summary>
        private enum SwitcherBukDetectorImit
        {
            /// <summary>
            /// Параметр "включения/активации" аргумента.
            /// </summary>
            [Action(typeof(Execut), "SwitcherBukDetectorImitOn")]
            ON,

            /// <summary>
            /// Параметр "выключения/деактивации" аргумента.
            /// </summary>
            [Action(typeof(Execut), "SwitcherBukDetectorImitOff")]
            OFF
        }

        /// <summary>
        /// Аргумент "переключатель состояния".
        /// </summary>
        private enum SwitcherPower
        {
            /// <summary>
            /// Параметр "включения/активации" аргумента.
            /// </summary>
            [Action(typeof(Execut), "SwitcherPowerOn")]
            ON,

            /// <summary>
            /// Параметр "выключения/деактивации" аргумента.
            /// </summary>
            [Action(typeof(Execut), "SwitcherPowerOff")]
            OFF
        }

        /// <summary>
        /// Аргумент "переключатель состояния".
        /// </summary>
        private enum SwitcherKvvImit
        {
            /// <summary>
            /// Параметр "включения/активации" аргумента.
            /// </summary>
            [Action(typeof(Execut), "SwitcherKvvImitOn")]
            ON,

            /// <summary>
            /// Параметр "выключения/деактивации" аргумента.
            /// </summary>
            [Action(typeof(Execut), "SwitcherKvvImitOff")]
            OFF
        }

        /// <summary>
        /// Аргумент "переключатель состояния".
        /// </summary>
        private enum SwitcherBm4Imit
        {
            /// <summary>
            /// Параметр "включения/активации" аргумента.
            /// </summary>
            [Action(typeof(Execut), "SwitcherBm4ImitOn")]
            ON,

            /// <summary>
            /// Параметр "выключения/деактивации" аргумента.
            /// </summary>
            [Action(typeof(Execut), "SwitcherBm4ImitOff")]
            OFF
        }

        /// <summary>
        /// Аргумент "выбор устройства".
        /// </summary>
        private enum Device
        {
            /// <summary>
            /// Параметр "устройство БУСК" аргумента.
            /// </summary>
            [Action(typeof(Execut), "DeviceBusk")]
            BUSK,

            /// <summary>
            /// Параметр "устройство БУК" аргумента.
            /// </summary>
            [Action(typeof(Execut), "DeviceBuk")]
            BUK,

            /// <summary>
            /// Параметр "устройство БУНД" аргумента.
            /// </summary>
            [Action(typeof(Execut), "DeviceBund")]
            BUND
        }

        /// <summary>
        /// Аргумент "флаг наличия ошибки в сообщении".
        /// </summary>
        private enum ErrorEndPoint
        {
            /// <summary>
            /// Параметр "выдавать флаг наличия ошибки в сообщении" аргумента.
            /// </summary>
            [Action(typeof(Execut), "IssueEep")]
            EEP
        }

        /// <summary>
        /// Аргумент "вкл/выкл текстовый лог-файл".
        /// </summary>
        private enum TxtLogDetectorImit
        {
            /// <summary>
            /// Параметр "вкл текстовый лог-файл" аргумента.
            /// </summary>
            [Action(typeof(Execut), "TxtLogDetectorImitOn")]
            TXT_ON,

            /// <summary>
            /// Параметр "выкл текстовый лог-файл" аргумента.
            /// </summary>
            [Action(typeof(Execut), "TxtLogDetectorImitOff")]
            TXT_OFF
        }

        /// <summary>
        /// Аргумент "вкл/выкл текстовый лог-файл".
        /// </summary>
        private enum TxtLogKvvImit
        {
            /// <summary>
            /// Параметр "вкл текстовый лог-файл" аргумента.
            /// </summary>
            [Action(typeof(Execut), "TxtLogKvvImitOn")]
            TXT_ON,

            /// <summary>
            /// Параметр "выкл текстовый лог-файл" аргумента.
            /// </summary>
            [Action(typeof(Execut), "TxtLogKvvImitOff")]
            TXT_OFF
        }

        /// <summary>
        /// Аргумент "вкл/выкл текстовый лог-файл".
        /// </summary>
        private enum TxtLogBm4Imit
        {
            /// <summary>
            /// Параметр "вкл текстовый лог-файл" аргумента.
            /// </summary>
            [Action(typeof(Execut), "TxtLogBm4ImitOn")]
            TXT_ON,

            /// <summary>
            /// Параметр "выкл текстовый лог-файл" аргумента.
            /// </summary>
            [Action(typeof(Execut), "TxtLogBm4ImitOff")]
            TXT_OFF
        }

        /// <summary>
        /// Аргумент "вкл/выкл бинарный лог".
        /// </summary>
        private enum BinLogKvvImit
        {
            /// <summary>
            /// Параметр "вкл бинарный лог" аргумента.
            /// </summary>
            [Action(typeof(Execut), "BinLogKvvImitOn")]
            BIN_ON,

            /// <summary>
            /// Параметр "выкл бинарный лог" аргумента.
            /// </summary>
            [Action(typeof(Execut), "BinLogKvvImitOff")]
            BIN_OFF
        }

        /// <summary>
        /// Аргумент "вкл/выкл бинарный лог".
        /// </summary>
        private enum BinLogDetectorImit
        {
            /// <summary>
            /// Параметр "вкл бинарный лог" аргумента.
            /// </summary>
            [Action(typeof(Execut), "BinLogDetectorImitOn")]
            BIN_ON,

            /// <summary>
            /// Параметр "выкл бинарный лог" аргумента.
            /// </summary>
            [Action(typeof(Execut), "BinLogDetectorImitOff")]
            BIN_OFF
        }

        /// <summary>
        /// Аргумент "вкл/выкл бинарный лог".
        /// </summary>
        private enum BinLogBm4Imit
        {
            /// <summary>
            /// Параметр "вкл бинарный лог" аргумента.
            /// </summary>
            [Action(typeof(Execut), "BinLogBm4ImitOn")]
            BIN_ON,

            /// <summary>
            /// Параметр "выкл бинарный лог" аргумента.
            /// </summary>
            [Action(typeof(Execut), "BinLogBm4ImitOff")]
            BIN_OFF
        }

        /// <summary>
        /// Аргумент "вкл/выкл обмен для имитатора БМ-4".
        /// </summary>
        private enum ExchangeBukBm4Imit
        {
            /// <summary>
            /// Параметр "вкл обмен для имитатора БМ-4" аргумента.
            /// </summary>
            [Action(typeof(Execut), "ExchangeBukBm4ImitOn")]
            EXCHANGE_ON,

            /// <summary>
            /// Параметр "выкл обмен для имитатора БМ-4" аргумента.
            /// </summary>
            [Action(typeof(Execut), "ExchangeBukBm4ImitOff")]
            EXCHANGE_OFF
        }

        /// <summary>
        /// Аргумент "вкл/выкл обмен для БМ-4".
        /// </summary>
        private enum ExchangeBm4Imit
        {
            /// <summary>
            /// Параметр "вкл обмен для БМ-4" аргумента.
            /// </summary>
            [Action(typeof(Execut), "ExchangeBm4ImitOn")]
            EXCHANGE_ON,

            /// <summary>
            /// Параметр "выкл обмен для БМ-4" аргумента.
            /// </summary>
            [Action(typeof(Execut), "ExchangeBm4ImitOff")]
            EXCHANGE_OFF
        }

        /// <summary>
        /// Аргумент "выбор канала БС".
        /// </summary>
        private enum ScidevDetectorImit
        {
            /// <summary>
            /// Параметр "канал БС УФЭС" аргумента.
            /// </summary>
            [Action(typeof(Execut), "ScidevDetectorImitUfes")]
            UFES,

            /// <summary>
            /// Параметр "канал БС ВУФЭС" аргумента.
            /// </summary>
            [Action(typeof(Execut), "ScidevDetectorImitVufes")]
            VUFES,

            /// <summary>
            /// Параметр "канал БС СДЩ" аргумента.
            /// </summary>
            [Action(typeof(Execut), "ScidevDetectorImitSdsh")]
            SDSH
        }

        /// <summary>
        /// Аргумент "каналы БС".
        /// </summary>
        private enum ScidevShutter
        {
            /// <summary>
            /// Параметр "канал БС УФЭС" аргумента.
            /// </summary>
            [Action(typeof(Execut), "ScidevShutterUfes")]
            UFES,

            /// <summary>
            /// Параметр "канал БС ВУФЭС" аргумента.
            /// </summary>
            [Action(typeof(Execut), "ScidevShutterVufes")]
            VUFES,

            /// <summary>
            /// Параметр "канал БС СДЩ" аргумента.
            /// </summary>
            [Action(typeof(Execut), "ScidevShutterSdsh")]
            SDSH
        }

        /// <summary>
        /// Аргумент "выбор линии передачи".
        /// </summary>
        private enum HalfsetDetectorImit
        {
            /// <summary>
            /// Параметр "основная линия" аргумента.
            /// </summary>
            [Action(typeof(Execut), "HalfsetDetectorImitMain")]
            MAIN,

            /// <summary>
            /// Параметр "резервная линия" аргумента.
            /// </summary>
            [Action(typeof(Execut), "HalfsetDetectorImitResv")]
            RESV
        }

        /// <summary>
        /// Аргумент "выбор линии передачи".
        /// </summary>
        private enum HalfsetKvvImit
        {
            /// <summary>
            /// Параметр "основная линия" аргумента.
            /// </summary>
            [Action(typeof(Execut), "HalfsetKvvImitMain")]
            MAIN,

            /// <summary>
            /// Параметр "резервная линия" аргумента.
            /// </summary>
            [Action(typeof(Execut), "HalfsetKvvImitResv")]
            RESV,

            /// <summary>
            /// Параметр "основная+резервная линия" аргумента.
            /// </summary>
            [Action(typeof(Execut), "HalfsetKvvImitBoth")]
            BOTH,

            /// <summary>
            /// Параметр "линия передачи отсутствует" аргумента.
            /// </summary>
            [Action(typeof(Execut), "HalfsetKvvImitNone")]
            NONE
        }

        /// <summary>
        /// Аргумент "выбор линии выдачи релейной команды".
        /// </summary>
        private enum Line
        {
            /// <summary>
            /// Параметр "выдача релейной команды по линии A" аргумента.
            /// </summary>
            [Action(typeof(Execut), "LineA")]
            A,

            /// <summary>
            /// Параметр "выдача релейной команды по линии B" аргумента.
            /// </summary>
            [Action(typeof(Execut), "LineB")]
            B,

            /// <summary>
            /// Параметр "выдача релейной команды по линиям A и B" аргумента.
            /// </summary>
            [Action(typeof(Execut), "LineAB")]
            AB
        }

        /// <summary>
        /// Аргумент "включить автоматический режим управления датчиками затворов".
        /// </summary>
        private enum Control
        {
            /// <summary>
            /// Параметр "автоматический режим управления датчиками затворов" аргумента.
            /// </summary>
            [Action(typeof(Execut), "ControlAuto")]
            AUTO
        }

        /// <summary>
        /// Аргумент "выбор канала spacewire".
        /// </summary>
        private enum Channel
        {
            /// <summary>
            /// Параметр "канал spacewire БУК ПК1 - БУСК ПК1" аргумента.
            /// </summary>
            [Action(typeof(Execut), "ChannelCh11")]
            CH1_1,

            /// <summary>
            /// Параметр "канал spacewire БУК ПК1 - БУСК ПК2" аргумента.
            /// </summary>
            [Action(typeof(Execut), "ChannelCh12")]
            CH1_2,

            /// <summary>
            /// Параметр "канал spacewire БУК ПК2 - БУСК ПК1" аргумента.
            /// </summary>
            [Action(typeof(Execut), "ChannelCh21")]
            CH2_1,

            /// <summary>
            /// Параметр "канал spacewire БУК ПК2 - БУСК ПК2" аргумента.
            /// </summary>
            [Action(typeof(Execut), "ChannelCh22")]
            CH2_2
        }

        /// <summary>
        /// Аргумент "выбор формируемую команду".
        /// </summary>
        private enum Command
        {
            /// <summary>
            /// Параметр "сформировать команду TELE" аргумента.
            /// </summary>
            [Action(typeof(Execut), "CommandTele")]
            TELE
        }

        /// <summary>
        /// Аргумент "обмен с прибором".
        /// </summary>
        private enum Poll
        {
            /// <summary>
            /// Параметр "включить обмен с прибором" аргумента.
            /// </summary>
            [Action(typeof(Execut), "PollOn")]
            POLL_ON,

            /// <summary>
            /// Параметр "выключить обмен с прибором" аргумента.
            /// </summary>
            [Action(typeof(Execut), "PollOff")]
            POLL_OFF
        }

        /// <summary>
        /// Аргумент "вкл/выкл выдачу пакетов данных в прибор".
        /// </summary>
        private enum Data
        {
            /// <summary>
            /// Параметр "вкл выдачу пакетов данных в прибор" аргумента.
            /// </summary>
            [Action(typeof(Execut), "DataOn")]
            DATA_ON,

            /// <summary>
            /// Параметр "выкл выдачу пакетов данных в прибор" аргумента.
            /// </summary>
            [Action(typeof(Execut), "DataOff")]
            DATA_OFF
        }

        /// <summary>
        /// Аргумент "частота выдачи пакетов данных (в миллисекундах)".
        /// </summary>
        private enum Time
        {
            /// <summary>
            /// Параметр "частота выдачи пакетов данных (в миллисекундах)" аргумента.
            /// </summary>
            [Action(typeof(Execut), "TimeArg", true)]
            TIME_dec
        }

        /// <summary>
        /// Аргумент "подтверждение получения телекоманды".
        /// </summary>
        private enum Rece
        {
            /// <summary>
            /// Параметр "установить флаг подтверждения получения телекоманды" аргумента.
            /// </summary>
            [Action(typeof(Execut), "MarkReceipt")]
            RECEIPT
        }

        /// <summary>
        /// Аргумент "подтверждение выполнения телекоманды".
        /// </summary>
        private enum Exec
        {
            /// <summary>
            /// Параметр "установить флаг подтверждения выполнения телекоманды" аргумента.
            /// </summary>
            [Action(typeof(Execut), "MarkExecut")]
            EXECUT
        }

        /// <summary>
        /// Аргумент "выдать УКС активации".
        /// </summary>
        private enum Activate
        {
            /// <summary>
            /// Параметр "УКС активации первого полукомплекта" аргумента.
            /// </summary>
            [Action(typeof(Execut), "CmdActivate1")]
            ACTIVATE1,

            /// <summary>
            /// Параметр "УКС активации второго полукомплекта" аргумента.
            /// </summary>
            [Action(typeof(Execut), "CmdActivate2")]
            ACTIVATE2
        }

        /// <summary>
        /// Аргумент "установить APID для телекоманды".
        /// </summary>
        private enum Apid
        {
            /// <summary>
            /// Параметр "APID для телекоманды" аргумента.
            /// </summary>
            [Action(typeof(Execut), "ApidArg", true)]
            APID_hex
        }

        /// <summary>
        /// Аргумент "посылка данных (в байтах)".
        /// </summary>
        [Action(typeof(Execut), "BukDetectorImitHexPackageArg", true)]
        private enum HexPackageBukDetectorImit
        { 
        }

        /// <summary>
        /// Аргумент "посылка данных (в байтах)".
        /// </summary>
        [Action(typeof(Execut), "Bm4ImitHexPackageArg", true)]
        private enum HexPackageBm4Imit
        { 
        }

        /// <summary>
        /// Аргумент "посылка данных (в байтах)".
        /// </summary>
        [Action(typeof(Execut), "BukKvvImitHexPackageArg", true)]
        private enum HexPackageBukKvvImit
        { 
        }

        /// <summary>
        /// Аргумент "выбор линии приема".
        /// </summary>
        private enum Receive
        {
            /// <summary>
            /// Параметр "основная линия приема" аргумента.
            /// </summary>
            [Action(typeof(Execut), "ReceiveMain")]
            RECV_MAIN,

            /// <summary>
            /// Параметр "резервная линия приема" аргумента.
            /// </summary>
            [Action(typeof(Execut), "ReceiveResv")]
            RECV_RESV
        }

        /// <summary>
        /// Аргумент "выбор линии передачи".
        /// </summary>
        private enum Send
        {
            /// <summary>
            /// Параметр "основная линия передачи" аргумента.
            /// </summary>
            [Action(typeof(Execut), "SendMain")]
            SEND_MAIN,

            /// <summary>
            /// Параметр "резервная линия передачи" аргумента.
            /// </summary>
            [Action(typeof(Execut), "SendResv")]
            SEND_RESV
        }

        /// <summary>
        /// Аргумент "секундная метка".
        /// </summary>
        private enum Tick
        {
            /// <summary>
            /// Параметр "включить выдачу секундной метки" аргумента.
            /// </summary>
            [Action(typeof(Execut), "TickOn")]
            TICK_ON,

            /// <summary>
            /// Параметр "выключить выдачу секундной метки" аргумента.
            /// </summary>
            [Action(typeof(Execut), "TickOff")]
            TICK_OFF
        }

        /// <summary>
        /// Аргумент "вкл/выкл датчик открытия затвора".
        /// </summary>
        private enum SensorOpen
        {
            /// <summary>
            /// Параметр "вкл датчик открытия затвора" аргумента.
            /// </summary>
            [Action(typeof(Execut), "SensorOpenOn")]
            SENS_OPEN_ON,

            /// <summary>
            /// Параметр "выкл датчик открытия затвора" аргумента.
            /// </summary>
            [Action(typeof(Execut), "SensorOpenOff")]
            SENS_OPEN_OFF
        }

        /// <summary>
        /// Аргумент "вкл/выкл датчик закрытия затвора".
        /// </summary>
        private enum SensorClose
        {
            /// <summary>
            /// Параметр "вкл датчик закрытия затвора" аргумента.
            /// </summary>
            [Action(typeof(Execut), "SensorCloseOn")]
            SENS_CLOSE_ON,

            /// <summary>
            /// Параметр "выкл датчик закрытия затвора" аргумента.
            /// </summary>
            [Action(typeof(Execut), "SensorCloseOff")]
            SENS_CLOSE_OFF
        }

        /// <summary>
        /// Аргумент "КБВ".
        /// </summary>
        private enum OnBoardTime
        {
            /// <summary>
            /// Параметр "включить выдачу КБВ" аргумента.
            /// </summary>
            [Action(typeof(Execut), "OnBoardTimeOn")]
            OBT_ON,

            /// <summary>
            /// Параметр "выключить выдачу КБВ" аргумента.
            /// </summary>
            [Action(typeof(Execut), "OnBoardTimeOff")]
            OBT_OFF
        }

        /// <summary>
        /// Аргумент "выбор полукомплекта".
        /// </summary>
        private enum SetsPower
        {
            /// <summary>
            /// Параметр "выбрать первый полукомплект" аргумента.
            /// </summary>
            [Action(typeof(Execut), "SetsPower1")]
            SET1,

            /// <summary>
            /// Параметр "выбрать второй полукомплект" аргумента.
            /// </summary>
            [Action(typeof(Execut), "SetsPower2")]
            SET2
        }

        /// <summary>
        /// Аргумент "выбор полукомплекта".
        /// </summary>
        private enum SetsKvvImit
        {
            /// <summary>
            /// Параметр "выбрать первый полукомплект" аргумента.
            /// </summary>
            [Action(typeof(Execut), "SetsKvvImit1")]
            SET1,

            /// <summary>
            /// Параметр "выбрать второй полукомплект" аргумента.
            /// </summary>
            [Action(typeof(Execut), "SetsKvvImit2")]
            SET2
        }

        /// <summary>
        /// Получает набор доступных цикломанд для устройства.
        /// </summary>
        public CyclogramCommands CyclogramCommandsAvailable { get; private set; }

        /// <summary>
        /// Получает или задает ссылку на экземпляр устройства.
        /// </summary>
        public EgseBukNotify BukNotify { get; set; }

        /// <summary>
        /// Buks the detector imit command execute.
        /// </summary>
        /// <param name="cmdParams">The command parameters.</param>
        /// <returns></returns>
        private bool BukDetectorImitCmdExec(string[] cmdParams)
        {
            try
            {
                IncludeExec<ErrorEndPoint, HexPackageBukDetectorImit>(cmdParams.ToList());
                return true;
            }
            catch
            {
                return false;
            } 
        }

        /// <summary>
        /// Buks the detector imit command test.
        /// </summary>
        /// <param name="cmdParams">The command parameters.</param>
        /// <param name="errString">The error string.</param>
        /// <returns></returns>
        private bool BukDetectorImitCmdTest(string[] cmdParams, out string errString)
        {
            string errParam = string.Empty;
            bool isParamCountErr = true;
            List<Type> uniParam = new List<Type>();
            errString = Resource.Get(@"eParamCount");

            if (1 > cmdParams.Length)
            {
                return !isParamCountErr;
            }

            // задаемся минимальным количеством параметров для команды (1).
            isParamCountErr = false;

            if (2 > cmdParams.Length)
            {
                if (!IncludeTest<HexPackageBukDetectorImit>(cmdParams[0], out errParam, ref uniParam))
                {
                    errString = string.Format(Resource.Get(@"eArg1"), errParam);
                    return false;
                }

                return !isParamCountErr;
            }

            if (!IncludeTest<ErrorEndPoint, HexPackageBukDetectorImit>(cmdParams[0], out errParam, ref uniParam))
            {
                errString = string.Format(Resource.Get(@"eArg1"), errParam);
                return false;
            }

            if (!IncludeTest<HexPackageBukDetectorImit>(cmdParams[1], out errParam, ref uniParam))
            {
                errString = string.Format(Resource.Get(@"eArg2"), errParam);
                return false;
            }

            if (3 > cmdParams.Length)
            {
                return !isParamCountErr;
            }

            if (!NeedTest<ErrorEndPoint>(cmdParams[0], out errParam, ref uniParam))
            {
                if (!IncludeTest<HexPackageBukDetectorImit>(cmdParams, out errParam, ref uniParam))
                {
                    errString = string.Format(Resource.Get(@"eArg1"), errParam);
                    return false;
                }
            }
            else
            {
                if (!IncludeTest<HexPackageBukDetectorImit>(cmdParams.Skip(1).ToArray(), out errParam, ref uniParam))
                {
                    errString = string.Format(Resource.Get(@"eArg2"), errParam);
                    return false;
                }
            }
            
            return true;
        }

        /// <summary>
        /// Buks the detector imit execute.
        /// </summary>
        /// <param name="cmdParams">The command parameters.</param>
        /// <returns></returns>
        private bool BukDetectorImitExec(string[] cmdParams)
        {
            try
            {
                IncludeExec<SwitcherBukDetectorImit>(cmdParams.ToList());
                return true;
            }
            catch
            {
                return false;
            } 
        }

        /// <summary>
        /// Buks the detector imit test.
        /// </summary>
        /// <param name="cmdParams">The command parameters.</param>
        /// <param name="errString">The error string.</param>
        /// <returns></returns>
        private bool BukDetectorImitTest(string[] cmdParams, out string errString)
        {
            string errParam = string.Empty;
            bool isParamCountErr = true;
            List<Type> uniParam = new List<Type>();
            errString = Resource.Get(@"eParamCount");

            // исключаем дублирующие параметры.
            if (cmdParams.Distinct().Count() != cmdParams.Length)
            {
                errString = Resource.Get(@"eUnique");
                return false;
            }

            if (1 > cmdParams.Length)
            {
                return !isParamCountErr;
            }

            if (!IncludeTest<SwitcherBukDetectorImit>(cmdParams[0], out errParam, ref uniParam))
            {
                errString = string.Format(Resource.Get(@"eArg1"), errParam);
                return false;
            }

            // задаемся максимальным количеством параметров для команды (1).
            isParamCountErr = true;

            if (1 < cmdParams.Length)
            {
                return !isParamCountErr;
            }

            return true;
        }

        /// <summary>
        /// Buks the BM4 imit execute.
        /// </summary>
        /// <param name="cmdParams">The command parameters.</param>
        /// <returns></returns>
        private bool BukBm4ImitExec(string[] cmdParams)
        {
            try
            {
                IncludeExec<SwitcherBukBm4Imit, ExchangeBukBm4Imit, Data, Time>(cmdParams.ToList());
                return true;
            }
            catch
            {
                return false;
            }  
        }

        /// <summary>
        /// Buks the BM4 imit test.
        /// </summary>
        /// <param name="cmdParams">The command parameters.</param>
        /// <param name="errString">The error string.</param>
        /// <returns></returns>
        private bool BukBm4ImitTest(string[] cmdParams, out string errString)
        {
            string errParam = string.Empty;
            bool isParamCountErr = true;
            List<Type> uniParam = new List<Type>();
            errString = Resource.Get(@"eParamCount");

            // исключаем дублирующие параметры.
            if (cmdParams.Distinct().Count() != cmdParams.Length)
            {
                errString = Resource.Get(@"eUnique");
                return false;
            }

            if (1 > cmdParams.Length)
            {
                return !isParamCountErr;
            }

            if (!IncludeTest<SwitcherBukBm4Imit>(cmdParams[0], out errParam, ref uniParam))
            {
                errString = string.Format(Resource.Get(@"eArg1"), errParam);
                return false;
            }

            // задаемся минимальным количеством параметров для команды (1).
            isParamCountErr = false;

            if (2 > cmdParams.Length)
            {
                return !isParamCountErr;
            }

            if (!IncludeTest<ExchangeBukBm4Imit, Data, Time>(cmdParams[1], out errParam, ref uniParam))
            {
                errString = string.Format(Resource.Get(@"eArg2"), errParam);
                return false;
            }

            if (3 > cmdParams.Length)
            {
                return !isParamCountErr;
            }

            if (!IncludeTest<Time, Data>(cmdParams[2], out errParam, ref uniParam))
            {
                errString = string.Format(Resource.Get(@"eArg3"), errParam);
                return false;
            }

            if (4 > cmdParams.Length)
            {
                return !isParamCountErr;
            }

            if (!IncludeTest<Time>(cmdParams[3], out errParam, ref uniParam))
            {
                errString = string.Format(Resource.Get(@"eArg4"), errParam);
                return false;
            }

            // задаемся максимальным количеством параметров для команды (4).
            isParamCountErr = true;

            if (4 < cmdParams.Length)
            {
                return !isParamCountErr;
            }

            return true;
        }

        /// <summary>
        /// Detectors the imit log test.
        /// </summary>
        /// <param name="cmdParams">The command parameters.</param>
        /// <param name="errString">The error string.</param>
        /// <returns></returns>
        private bool DetectorImitLogTest(string[] cmdParams, out string errString)
        {
            string errParam = string.Empty;
            bool isParamCountErr = true;
            List<Type> uniParam = new List<Type>();
            errString = Resource.Get(@"eParamCount");

            // исключаем дублирующие параметры.
            if (cmdParams.Distinct().Count() != cmdParams.Length)
            {
                errString = Resource.Get(@"eUnique");
                return false;
            }

            if (1 > cmdParams.Length)
            {
                return !isParamCountErr;
            }

            if (!IncludeTest<BinLogDetectorImit, TxtLogDetectorImit>(cmdParams[0], out errParam, ref uniParam))
            {
                errString = string.Format(Resource.Get(@"eArg1"), errParam);
                return false;
            }

            // задаемся минимальным количеством параметров для команды (1).
            isParamCountErr = false;

            if (2 > cmdParams.Length)
            {                
                return !isParamCountErr;
            }

            if (!IncludeTest<BinLogDetectorImit, TxtLogDetectorImit>(cmdParams[1], out errParam, ref uniParam))
            {
                errString = string.Format(Resource.Get(@"eArg2"), errParam);
                return false;
            }

            // задаемся максимальным количеством параметров для команды (2).
            isParamCountErr = true;

            if (2 < cmdParams.Length)
            {
                return !isParamCountErr;
            }

            return true;
        }

        /// <summary>
        /// Detectors the imit log execute.
        /// </summary>
        /// <param name="cmdParams">The command parameters.</param>
        /// <returns></returns>
        private bool DetectorImitLogExec(string[] cmdParams)
        {
            try
            {
                IncludeExec<BinLogDetectorImit, TxtLogDetectorImit>(cmdParams.ToList());
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// BM4s the imit log test.
        /// </summary>
        /// <param name="cmdParams">The command parameters.</param>
        /// <param name="errString">The error string.</param>
        /// <returns></returns>
        private bool Bm4ImitLogTest(string[] cmdParams, out string errString)
        {
            string errParam = string.Empty;
            bool isParamCountErr = true;
            List<Type> uniParam = new List<Type>();
            errString = Resource.Get(@"eParamCount");

            // исключаем дублирующие параметры.
            if (cmdParams.Distinct().Count() != cmdParams.Length)
            {
                errString = Resource.Get(@"eUnique");
                return false;
            }

            if (1 > cmdParams.Length)
            {
                return !isParamCountErr;
            }

            if (!IncludeTest<BinLogBm4Imit, TxtLogBm4Imit>(cmdParams[0], out errParam, ref uniParam))
            {
                errString = string.Format(Resource.Get(@"eArg1"), errParam);
                return false;
            }

            // задаемся минимальным количеством параметров для команды (1).
            isParamCountErr = false;

            if (2 > cmdParams.Length)
            {                
                return !isParamCountErr;
            }

            if (!IncludeTest<BinLogBm4Imit, TxtLogBm4Imit>(cmdParams[1], out errParam, ref uniParam))
            {
                errString = string.Format(Resource.Get(@"eArg2"), errParam);
                return false;
            }

            // задаемся максимальным количеством параметров для команды (2).
            isParamCountErr = true;

            if (2 < cmdParams.Length)
            {
                return !isParamCountErr;
            }

            return true;
        }

        /// <summary>
        /// BM4s the imit log execute.
        /// </summary>
        /// <param name="cmdParams">The command parameters.</param>
        /// <returns></returns>
        private bool Bm4ImitLogExec(string[] cmdParams)
        {            
            try
            {
                IncludeExec<BinLogBm4Imit, TxtLogBm4Imit>(cmdParams.ToList());
                return true;
            }
            catch
            {
                return false;
            }   
        }

        /// <summary>
        /// KVVs the imit log test.
        /// </summary>
        /// <param name="cmdParams">The command parameters.</param>
        /// <param name="errString">The error string.</param>
        /// <returns></returns>
        private bool KvvImitLogTest(string[] cmdParams, out string errString)
        {
            string errParam = string.Empty;
            bool isParamCountErr = true;
            List<Type> uniParam = new List<Type>();
            errString = Resource.Get(@"eParamCount");

            // исключаем дублирующие параметры.
            if (cmdParams.Distinct().Count() != cmdParams.Length)
            {
                errString = Resource.Get(@"eUnique");
                return false;
            }

            if (1 > cmdParams.Length)
            {
                return !isParamCountErr;
            }

            if (!IncludeTest<BinLogKvvImit, TxtLogKvvImit>(cmdParams[0], out errParam, ref uniParam))
            {
                errString = string.Format(Resource.Get(@"eArg1"), errParam);
                return false;
            } 

            // задаемся минимальным количеством параметров для команды (1).
            isParamCountErr = false;

            if (2 > cmdParams.Length)
            {                               
                return !isParamCountErr;
            }

            if (!IncludeTest<BinLogKvvImit, TxtLogKvvImit>(cmdParams[1], out errParam, ref uniParam))
            {
                errString = string.Format(Resource.Get(@"eArg2"), errParam);
                return false;
            }
            
            // задаемся максимальным количеством параметров для команды (2).
            isParamCountErr = true;

            if (2 < cmdParams.Length)
            {
                return !isParamCountErr;
            }

            return true;
        }

        /// <summary>
        /// KVVs the imit log execute.
        /// </summary>
        /// <param name="cmdParams">The command parameters.</param>
        /// <returns></returns>
        private bool KvvImitLogExec(string[] cmdParams)
        {
            try
            {
                IncludeExec<BinLogKvvImit, TxtLogKvvImit>(cmdParams.ToList());
                return true;
            }
            catch
            {
                return false;
            } 
        }

        /// <summary>
        /// Includes the test.
        /// </summary>
        /// <typeparam name="TEnum">The type of the enum.</typeparam>
        /// <param name="cmd">The command.</param>
        /// <param name="errStr">The error string.</param>
        /// <param name="dupl">The dupl.</param>
        /// <param name="exclude">The exclude.</param>
        /// <param name="duplNeedTest">if set to <c>true</c> [dupl need test].</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentException">
        /// @TEnum
        /// or
        /// @TEnum
        /// or
        /// @TEnum
        /// or
        /// @exclude
        /// </exception>
        private bool IncludeTest<TEnum>(string[] cmd, out string errStr, ref List<Type> dupl, TEnum[] exclude = null, bool duplNeedTest = true) 
        {
            new { dupl }.CheckNotNull();

            if ((1 == cmd.Length) && (2 < cmd[0].Length))
            {
                return IncludeTest<TEnum>(cmd[0], out errStr, ref dupl, exclude, duplNeedTest);
            }

            if ((typeof(HexPackageBukDetectorImit) != typeof(TEnum)) && (typeof(HexPackageBm4Imit) != typeof(TEnum)) && (typeof(HexPackageBukKvvImit) != typeof(TEnum)))
            {
                throw new ArgumentException(@"TEnum");
            }

            if (null != exclude)
            {
                throw new ArgumentException(@"exclude");
            }

            byte[] outBuf = new byte[cmd.Length];
            int i = 0;
            foreach (string hex in cmd)
            {
                if (!byte.TryParse(hex, NumberStyles.HexNumber, new CultureInfo("en-US"), out outBuf[i++]))
                {
                    errStr = string.Format(Resource.Get(@"eBadPackage"), hex);
                    return false;
                }
            }

            errStr = string.Empty;
            return true;
        }

        /// <summary>
        /// Incomes the package.
        /// </summary>
        /// <param name="testType">Type of the test.</param>
        /// <param name="cmd">The command.</param>
        /// <param name="errStr">The error string.</param>
        /// <param name="list">The list.</param>
        /// <returns></returns>
        private bool IncomePackage(Type testType, string cmd, out string errStr, ref List<string> list)
        {
            if ((typeof(HexPackageBukDetectorImit) == testType) || (typeof(HexPackageBm4Imit) == testType) || (typeof(HexPackageBukKvvImit) == testType))
            {
                if ((cmd.Length % 2) > 0)
                {
                    errStr = string.Format(Resource.Get(@"eBadPackage") + @", не кратный размер посылки", cmd.Length);
                    return false;
                }

                for (int i = cmd.Length % 2; i < cmd.Length; i += 2)
                {
                    list.Add(cmd.Substring(i, 2));
                }

                errStr = string.Empty;
                return true;
            }
            else
            {
                errStr = string.Empty;
                return false;
            }
        }

        /// <summary>
        /// Проверка существования параметров у команды.
        /// </summary>
        /// <typeparam name="TEnum">Перечисление параметров.</typeparam>
        /// <param name="cmd">Наименование команды.</param>
        /// <param name="errStr">Текст ошибки.</param>
        /// <param name="dupl">Список обнаруженных аргументов.</param>
        /// <param name="exclude">Список исключенных параметров.</param>
        /// <param name="duplNeedTest">if set to <c>true</c> [dupl need test].</param>
        /// <returns>
        ///   <c>true</c> если параметры существуют, иначе <c>false</c>
        /// </returns>
        private bool IncludeTest<TEnum>(string cmd, out string errStr, ref List<Type> dupl, TEnum[] exclude = null, bool duplNeedTest = true)
        {
            new { dupl }.CheckNotNull();

            List<string> list = new List<string>();
            if (IncomePackage(typeof(TEnum), cmd, out errStr, ref list))
            {
                return IncludeTest<TEnum>(list.ToArray(), out errStr, ref dupl, exclude, duplNeedTest); 
            }    
            else
            {
                if (string.Empty != errStr)
                {
                    return false;
                }
            }

            int dec = -1;
            int hex = -1;
            if (!GetAllList<TEnum>(exclude).Exists(x => 
                {
                    return EqualsCmd(x, cmd, ref dec, ref hex);
                }))
            {
                errStr = string.Format(Resource.Get(@"stNeedBy"), string.Join(@" или ", GetAllList<TEnum>(exclude).ToArray()));   
                return false;
            }
            else
            {
                if (duplNeedTest)
                {
                    dupl.Add(typeof(TEnum));
                    if (dupl.Count != dupl.Distinct().Count())
                    {
                        errStr = string.Format(Resource.Get(@"eArgUnique"), string.Join(@" или ", GetAllList<TEnum>(exclude).ToArray()));
                        return false;
                    }
                }
        
                errStr = string.Empty;
                return true;
            }
        }

        /// <summary>
        /// Needs the test.
        /// </summary>
        /// <typeparam name="TEnum">The type of the enum.</typeparam>
        /// <param name="cmd">The command.</param>
        /// <param name="errStr">The error string.</param>
        /// <param name="dupl">The dupl.</param>
        /// <param name="exclude">The exclude.</param>
        /// <returns></returns>
        private bool NeedTest<TEnum>(string cmd, out string errStr, ref List<Type> dupl, TEnum[] exclude = null)
        {
            if (IncludeTest<TEnum>(cmd, out errStr, ref dupl, exclude, false))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Equalses the command.
        /// </summary>
        /// <param name="x">The x.</param>
        /// <param name="cmd">The command.</param>
        /// <param name="dec">The decimal.</param>
        /// <param name="hex">The hexadecimal.</param>
        /// <returns></returns>
        private bool EqualsCmd(string x, string cmd, ref int dec, ref int hex)
        {
            if (x.Contains(@"dec"))
            {
                return cmd.Contains(x.Replace(@"dec", string.Empty)) && int.TryParse(cmd.Replace(x.Replace(@"dec", string.Empty), string.Empty), NumberStyles.Integer, CultureInfo.InvariantCulture, out dec);
            }
            else if (x.Contains(@"hex"))
            {
                return cmd.Contains(x.Replace(@"hex", string.Empty)) && int.TryParse(cmd.Replace(x.Replace(@"hex", string.Empty), string.Empty).Trim('0').Trim('x'), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out hex);
            }
            else
            {
                return x == cmd;
            }
        }

        /// <summary>
        /// Includes the execute hexadecimal package.
        /// </summary>
        /// <typeparam name="TEnum">The type of the enum.</typeparam>
        /// <param name="cmds">The CMDS.</param>
        /// <param name="exclude">The exclude.</param>
        /// <returns></returns>
        private bool IncludeExecHexPackage<TEnum>(string[] cmds, TEnum[] exclude = null)
        {
            ActionAttribute attribute = (ActionAttribute)typeof(TEnum).GetCustomAttributes(typeof(ActionAttribute), false).FirstOrDefault();
            if (null != attribute)
            {
                List<string> list = cmds.ToList();
                if (1 == cmds.Length)
                {
                    list = new List<string>();
                    for (int i = cmds[0].Length % 2; i < cmds[0].Length; i += 2)
                    {
                        list.Add(cmds[0].Substring(i, 2));
                    }
                }

                attribute.ActArg(this.BukNotify, list);               
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Includes the execute.
        /// </summary>
        /// <typeparam name="TEnum">The type of the enum.</typeparam>
        /// <param name="cmd">The command.</param>
        /// <param name="cmds">The CMDS.</param>
        /// <param name="exclude">The exclude.</param>
        /// <returns></returns>
        private bool IncludeExec<TEnum>(string cmd, ref List<string> cmds, TEnum[] exclude = null)
        {
            if ((typeof(HexPackageBukKvvImit) == typeof(TEnum)) || (typeof(HexPackageBukDetectorImit) == typeof(TEnum)) || (typeof(HexPackageBm4Imit) == typeof(TEnum)))
            {
                return IncludeExecHexPackage<TEnum>(cmds.ToArray(), exclude);
            }

            int dec = -1;
            int hex = -1;
            string curCmd = string.Empty;
            if (GetAllList<TEnum>(exclude).Exists(x =>
                {
                    curCmd = x;
                    return EqualsCmd(x, cmd, ref dec, ref hex);
                }))
            {
                System.Reflection.MemberInfo memberInfo = typeof(TEnum).GetMember(curCmd).FirstOrDefault();

                if (null != memberInfo)
                {
                    ActionAttribute attribute = (ActionAttribute)memberInfo.GetCustomAttributes(typeof(ActionAttribute), false).FirstOrDefault();
                    if (null != attribute)
                    {
                        if (-1 != dec)
                        {
                            attribute.ActArg(this.BukNotify, dec);
                        }
                        else if (-1 != hex)
                        {
                            attribute.ActArg(this.BukNotify, hex);
                        }
                        else
                        {
                            attribute.Act(this.BukNotify);
                        }

                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Includes the execute.
        /// </summary>
        /// <typeparam name="TEnum1">The type of the enum1.</typeparam>
        /// <param name="cmds">The CMDS.</param>
        /// <param name="exclude1">The exclude1.</param>
        private void IncludeExec<TEnum1>(List<string> cmds, TEnum1[] exclude1 = null)
        {
            Execut.ClearTransaction();

            foreach (string str in cmds)
            {
                if (IncludeExec<TEnum1>(str, ref cmds, exclude1))
                {
                    cmds.Remove(str);
                    break;
                }
            }
        }

        /// <summary>
        /// Includes the execute.
        /// </summary>
        /// <typeparam name="TEnum1">The type of the enum1.</typeparam>
        /// <typeparam name="TEnum2">The type of the enum2.</typeparam>
        /// <param name="cmds">The CMDS.</param>
        /// <param name="exclude1">The exclude1.</param>
        /// <param name="exclude2">The exclude2.</param>
        private void IncludeExec<TEnum1, TEnum2>(List<string> cmds, TEnum1[] exclude1 = null, TEnum2[] exclude2 = null)
        {
            Execut.ClearTransaction();

            foreach (string str in cmds)
            {
                if (IncludeExec<TEnum1>(str, ref cmds, exclude1))
                {
                    cmds.Remove(str);
                    break;
                }
            }

            foreach (string str in cmds)
            {
                if (IncludeExec<TEnum2>(str, ref cmds, exclude2))
                {
                    cmds.Remove(str);
                    break;
                }
            }
        }

        /// <summary>
        /// Includes the execute.
        /// </summary>
        /// <typeparam name="TEnum1">The type of the enum1.</typeparam>
        /// <typeparam name="TEnum2">The type of the enum2.</typeparam>
        /// <typeparam name="TEnum3">The type of the enum3.</typeparam>
        /// <param name="cmds">The CMDS.</param>
        /// <param name="exclude1">The exclude1.</param>
        /// <param name="exclude2">The exclude2.</param>
        /// <param name="exclude3">The exclude3.</param>
        private void IncludeExec<TEnum1, TEnum2, TEnum3>(List<string> cmds, TEnum1[] exclude1 = null, TEnum2[] exclude2 = null, TEnum3[] exclude3 = null)
        {
            Execut.ClearTransaction();

            foreach (string str in cmds)
            {
                if (IncludeExec<TEnum1>(str, ref cmds, exclude1))
                {
                    cmds.Remove(str);
                    break;
                }
            }

            foreach (string str in cmds)
            {
                if (IncludeExec<TEnum2>(str, ref cmds, exclude2))
                {
                    cmds.Remove(str);
                    break;
                }
            }

            foreach (string str in cmds)
            {
                if (IncludeExec<TEnum3>(str, ref cmds, exclude3))
                {
                    cmds.Remove(str);
                    break;
                }
            }
        }

        /// <summary>
        /// Includes the execute.
        /// </summary>
        /// <typeparam name="TEnum1">The type of the enum1.</typeparam>
        /// <typeparam name="TEnum2">The type of the enum2.</typeparam>
        /// <typeparam name="TEnum3">The type of the enum3.</typeparam>
        /// <typeparam name="TEnum4">The type of the enum4.</typeparam>
        /// <param name="cmds">The CMDS.</param>
        /// <param name="exclude1">The exclude1.</param>
        /// <param name="exclude2">The exclude2.</param>
        /// <param name="exclude3">The exclude3.</param>
        /// <param name="exclude4">The exclude4.</param>
        private void IncludeExec<TEnum1, TEnum2, TEnum3, TEnum4>(List<string> cmds, TEnum1[] exclude1 = null, TEnum2[] exclude2 = null, TEnum3[] exclude3 = null, TEnum4[] exclude4 = null)
        {
            Execut.ClearTransaction();

            foreach (string str in cmds)
            {
                if (IncludeExec<TEnum1>(str, ref cmds, exclude1))
                {
                    cmds.Remove(str);
                    break;
                }
            }

            foreach (string str in cmds)
            {
                if (IncludeExec<TEnum2>(str, ref cmds, exclude2))
                {
                    cmds.Remove(str);
                    break;
                }
            }

            foreach (string str in cmds)
            {
                if (IncludeExec<TEnum3>(str, ref cmds, exclude3))
                {
                    cmds.Remove(str);
                    break;
                }
            }

            foreach (string str in cmds)
            {
                if (IncludeExec<TEnum4>(str, ref cmds, exclude4))
                {
                    cmds.Remove(str);
                    break;
                }
            }
        }

        /// <summary>
        /// Includes the execute.
        /// </summary>
        /// <typeparam name="TEnum1">The type of the enum1.</typeparam>
        /// <typeparam name="TEnum2">The type of the enum2.</typeparam>
        /// <typeparam name="TEnum3">The type of the enum3.</typeparam>
        /// <typeparam name="TEnum4">The type of the enum4.</typeparam>
        /// <typeparam name="TEnum5">The type of the enum5.</typeparam>
        /// <param name="cmds">The CMDS.</param>
        /// <param name="exclude1">The exclude1.</param>
        /// <param name="exclude2">The exclude2.</param>
        /// <param name="exclude3">The exclude3.</param>
        /// <param name="exclude4">The exclude4.</param>
        /// <param name="exclude5">The exclude5.</param>
        private void IncludeExec<TEnum1, TEnum2, TEnum3, TEnum4, TEnum5>(List<string> cmds, TEnum1[] exclude1 = null, TEnum2[] exclude2 = null, TEnum3[] exclude3 = null, TEnum4[] exclude4 = null, TEnum5[] exclude5 = null)
        {
            Execut.ClearTransaction();

            foreach (string str in cmds)
            {
                if (IncludeExec<TEnum1>(str, ref cmds, exclude1))
                {
                    cmds.Remove(str);
                    break;
                }
            }

            foreach (string str in cmds)
            {
                if (IncludeExec<TEnum2>(str, ref cmds, exclude2))
                {
                    cmds.Remove(str);
                    break;
                }
            }

            foreach (string str in cmds)
            {
                if (IncludeExec<TEnum3>(str, ref cmds, exclude3))
                {
                    cmds.Remove(str);
                    break;
                }
            }

            foreach (string str in cmds)
            {
                if (IncludeExec<TEnum4>(str, ref cmds, exclude4))
                {
                    cmds.Remove(str);
                    break;
                }
            }

            foreach (string str in cmds)
            {
                if (IncludeExec<TEnum5>(str, ref cmds, exclude5))
                {
                    cmds.Remove(str);
                    break;
                }
            }
        }

        /// <summary>
        /// Includes the execute.
        /// </summary>
        /// <typeparam name="TEnum1">The type of the enum1.</typeparam>
        /// <typeparam name="TEnum2">The type of the enum2.</typeparam>
        /// <typeparam name="TEnum3">The type of the enum3.</typeparam>
        /// <typeparam name="TEnum4">The type of the enum4.</typeparam>
        /// <typeparam name="TEnum5">The type of the enum5.</typeparam>
        /// <typeparam name="TEnum6">The type of the enum6.</typeparam>
        /// <param name="cmds">The CMDS.</param>
        /// <param name="exclude1">The exclude1.</param>
        /// <param name="exclude2">The exclude2.</param>
        /// <param name="exclude3">The exclude3.</param>
        /// <param name="exclude4">The exclude4.</param>
        /// <param name="exclude5">The exclude5.</param>
        /// <param name="exclude6">The exclude6.</param>
        private void IncludeExec<TEnum1, TEnum2, TEnum3, TEnum4, TEnum5, TEnum6>(List<string> cmds, TEnum1[] exclude1 = null, TEnum2[] exclude2 = null, TEnum3[] exclude3 = null, TEnum4[] exclude4 = null, TEnum5[] exclude5 = null, TEnum6[] exclude6 = null)
        {
            Execut.ClearTransaction();

            foreach (string str in cmds)
            {
                if (IncludeExec<TEnum1>(str, ref cmds, exclude1))
                {
                    cmds.Remove(str);
                    break;
                }
            }

            foreach (string str in cmds)
            {
                if (IncludeExec<TEnum2>(str, ref cmds, exclude2))
                {
                    cmds.Remove(str);
                    break;
                }
            }

            foreach (string str in cmds)
            {
                if (IncludeExec<TEnum3>(str, ref cmds, exclude3))
                {
                    cmds.Remove(str);
                    break;
                }
            }

            foreach (string str in cmds)
            {
                if (IncludeExec<TEnum4>(str, ref cmds, exclude4))
                {
                    cmds.Remove(str);
                    break;
                }
            }

            foreach (string str in cmds)
            {
                if (IncludeExec<TEnum5>(str, ref cmds, exclude5))
                {
                    cmds.Remove(str);
                    break;
                }
            }

            foreach (string str in cmds)
            {
                if (IncludeExec<TEnum6>(str, ref cmds, exclude6))
                {
                    cmds.Remove(str);
                    break;
                }
            }
        }

        /// <summary>
        /// Includes the test.
        /// </summary>
        /// <typeparam name="TEnum1">The type of the enum1.</typeparam>
        /// <typeparam name="TEnum2">The type of the enum2.</typeparam>
        /// <param name="cmd">The command.</param>
        /// <param name="errStr">The error string.</param>
        /// <param name="dupl">The dupl.</param>
        /// <param name="exclude1">The exclude1.</param>
        /// <param name="exclude2">The exclude2.</param>
        /// <returns></returns>
        private bool IncludeTest<TEnum1, TEnum2>(string cmd, out string errStr, ref List<Type> dupl, TEnum1[] exclude1 = null, TEnum2[] exclude2 = null)
        {
            if (IncludeTest<TEnum2>(cmd, out errStr, ref dupl, exclude2))
            {                
                return true;
            }
            
            if (IncludeTest<TEnum1>(cmd, out errStr, ref dupl, exclude1))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Includes the test.
        /// </summary>
        /// <typeparam name="TEnum1">The type of the enum1.</typeparam>
        /// <typeparam name="TEnum2">The type of the enum2.</typeparam>
        /// <typeparam name="TEnum3">The type of the enum3.</typeparam>
        /// <param name="cmd">The command.</param>
        /// <param name="errStr">The error string.</param>
        /// <param name="dupl">The dupl.</param>
        /// <param name="exclude1">The exclude1.</param>
        /// <param name="exclude2">The exclude2.</param>
        /// <param name="exclude3">The exclude3.</param>
        /// <returns></returns>
        private bool IncludeTest<TEnum1, TEnum2, TEnum3>(string cmd, out string errStr, ref List<Type> dupl, TEnum1[] exclude1 = null, TEnum2[] exclude2 = null, TEnum3[] exclude3 = null)
        {
            if (IncludeTest<TEnum3>(cmd, out errStr, ref dupl, exclude3))
            {
                return true;
            }

            if (IncludeTest<TEnum2>(cmd, out errStr, ref dupl, exclude2))
            {
                return true;
            }

            if (IncludeTest<TEnum1>(cmd, out errStr, ref dupl, exclude1))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Includes the test.
        /// </summary>
        /// <typeparam name="TEnum1">The type of the enum1.</typeparam>
        /// <typeparam name="TEnum2">The type of the enum2.</typeparam>
        /// <typeparam name="TEnum3">The type of the enum3.</typeparam>
        /// <typeparam name="TEnum4">The type of the enum4.</typeparam>
        /// <param name="cmd">The command.</param>
        /// <param name="errStr">The error string.</param>
        /// <param name="dupl">The dupl.</param>
        /// <param name="exclude1">The exclude1.</param>
        /// <param name="exclude2">The exclude2.</param>
        /// <param name="exclude3">The exclude3.</param>
        /// <param name="exclude4">The exclude4.</param>
        /// <returns></returns>
        private bool IncludeTest<TEnum1, TEnum2, TEnum3, TEnum4>(string cmd, out string errStr, ref List<Type> dupl, TEnum1[] exclude1 = null, TEnum2[] exclude2 = null, TEnum3[] exclude3 = null, TEnum4[] exclude4 = null)
        {
            if (IncludeTest<TEnum4>(cmd, out errStr, ref dupl, exclude4))
            {
                return true;
            }

            if (IncludeTest<TEnum3>(cmd, out errStr, ref dupl, exclude3))
            {
                return true;
            }

            if (IncludeTest<TEnum2>(cmd, out errStr, ref dupl, exclude2))
            {
                return true;
            }

            if (IncludeTest<TEnum1>(cmd, out errStr, ref dupl, exclude1))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Формируем список всех параметров для данной команды.
        /// </summary>
        /// <typeparam name="TEnum">Перечисление параметров.</typeparam>
        /// <param name="exclude">Список исключенных параметров.</param>
        /// <returns>Список наименований параметров, для данной команды.</returns>
        private List<string> GetAllList<TEnum>(TEnum[] exclude = null)
        {
            if (null != exclude)
            {
                return Enum.GetNames(typeof(TEnum)).ToList().Except(exclude.Where(x => x != null).Select(x => x.ToString()).ToList()).ToList();
            }
            else
            {
                return Enum.GetNames(typeof(TEnum)).ToList();
            }
        }

        /// <summary>
        /// Attributes the specified effect type.
        /// </summary>
        /// <param name="effectType">Type of the effect.</param>
        /// <returns></returns>
        private Action Attribute(Enum effectType)
        {
            System.Reflection.MemberInfo memberInfo = effectType.GetType().GetMember(effectType.ToString()).FirstOrDefault();

            if (memberInfo != null)
            {
                Action attribute = (Action)
                             memberInfo.GetCustomAttributes(typeof(Action), false)
                                       .FirstOrDefault();
                return attribute;
            }

            return null;
        }

        /// <summary>
        /// TODO описание
        /// </summary>
        /// <param name="cmdParams">The command parameters.</param>
        /// <param name="errString">The error string.</param>
        /// <returns>
        /// <c>true</c> если проверка успешно пройдена, иначе <c>false</c>.
        /// </returns>
        private bool PowerTest(string[] cmdParams, out string errString)
        {
            string errParam = string.Empty;
            bool isParamCountErr = true;
            List<Type> uniParam = new List<Type>();
            errString = Resource.Get(@"eParamCount");

            // исключаем дублирующие параметры.
            if (cmdParams.Distinct().Count() != cmdParams.Length)
            {
                errString = Resource.Get(@"eUnique");
                return false;
            }

            if (1 > cmdParams.Length)
            {
                return !isParamCountErr;
            }

            if (!IncludeTest<Device>(cmdParams[0], out errParam, ref uniParam, new Device[1] { Device.BUK }))
            {                
                errString = string.Format(Resource.Get(@"eArg1"), errParam);
                return false;
            }

            if (2 > cmdParams.Length)
            {
                return !isParamCountErr;
            }

            if (!IncludeTest<Line, SetsPower>(cmdParams[1], out errParam, ref uniParam))
            {
                errString = string.Format(Resource.Get(@"eArg2"), errParam);
                return false;
            }

            if (3 > cmdParams.Length)
            {
                return !isParamCountErr;
            }

            if (!IncludeTest<SetsPower, SwitcherPower>(cmdParams[2], out errParam, ref uniParam))
            {
                errString = string.Format(Resource.Get(@"eArg3"), errParam);
                return false;
            }

            // задаемся минимальным количеством параметров для команды (3).
            isParamCountErr = false;

            if (4 > cmdParams.Length)
            {
                // если количество параметров минимально - второй аргумент обязан быть параметром "Sets".
                if (!NeedTest<SetsPower>(cmdParams[1], out errParam, ref uniParam))
                {
                    errString = string.Format(Resource.Get(@"eArg2"), errParam);
                    return false;
                }

                return !isParamCountErr;
            }

            if (!IncludeTest<SwitcherPower>(cmdParams[3], out errParam, ref uniParam))
            {
                errString = string.Format(Resource.Get(@"eArg4"), errParam);
                return false;
            }

            // задаемся максимальным количеством параметров для команды (4).
            isParamCountErr = true;

            if (4 < cmdParams.Length)
            {
                return !isParamCountErr;
            }            

            return true;
        }

        /// <summary>
        /// Powers the execute.
        /// </summary>
        /// <param name="cmdParams">The command parameters.</param>
        /// <returns><c>true</c> если команда выполнена, иначе <c>false</c>.</returns>
        /// <exception cref="System.NotImplementedException">Нет реализации.</exception>
        private bool PowerExec(string[] cmdParams)
        {
            try
            {
                IncludeExec<Device, Line, SetsPower, SwitcherPower>(cmdParams.ToList());
                return true;
            }
            catch
            {
                return false;
            } 
        }

        /// <summary>
        /// Gates the test.
        /// </summary>
        /// <param name="cmdParams">The command parameters.</param>
        /// <param name="errString">The error string.</param>
        /// <returns><c>true</c> если проверка успешно пройдена, иначе <c>false</c>.</returns>
        private bool ShutterTest(string[] cmdParams, out string errString)
        {
            string errParam = string.Empty;
            bool isParamCountErr = true;
            List<Type> uniParam = new List<Type>();
            errString = Resource.Get(@"eParamCount");

            // исключаем дублирующие параметры.
            if (cmdParams.Distinct().Count() != cmdParams.Length)
            {
                errString = Resource.Get(@"eUnique");
                return false;
            }

            if (1 > cmdParams.Length)
            {
                return !isParamCountErr;
            }

            // устанавливаем минимальное количество параметров в команде (1).
            isParamCountErr = false;

            if (2 > cmdParams.Length)
            {
                // если минимальное количество параметров (1) проверить что это параметр "Control".
                if (!IncludeTest<Control>(cmdParams[0], out errParam, ref uniParam))
                {
                    errString = string.Format(Resource.Get(@"eArg1"), errParam);
                    return false;
                }

                return !isParamCountErr;
            }

            // если же команд более 1 - первый аргумент обязан быть параметром "Scidev".
            if (!IncludeTest<ScidevShutter>(cmdParams[0], out errParam, ref uniParam))
            {
                errString = string.Format(Resource.Get(@"eArg1"), errParam);
                return false;
            }

            // следующий параметр может варьироваться.
            if (!IncludeTest<SensorOpen, SensorClose>(cmdParams[1], out errParam, ref uniParam))
            {
                errString = string.Format(Resource.Get(@"eArg2"), errParam);
                return false;
            }

            if (3 > cmdParams.Length)
            {
                return !isParamCountErr;
            }

            if (!IncludeTest<SensorClose>(cmdParams[2], out errParam, ref uniParam))
            {
                errString = string.Format(Resource.Get(@"eArg3"), errParam);
                return false;
            }

            // задаемся максимальным количеством параметров для команды (3).
            isParamCountErr = true;

            if (3 < cmdParams.Length)
            {
                return !isParamCountErr;
            }

            return true;
        }

        /// <summary>
        /// Gates the execute.
        /// </summary>
        /// <param name="cmdParams">The command parameters.</param>
        /// <returns><c>true</c> если команда выполнена, иначе <c>false</c>.</returns>
        /// <exception cref="System.NotImplementedException">Нет реализации.</exception>
        private bool ShutterExec(string[] cmdParams)
        {
            try
            {
                IncludeExec<Control, ScidevShutter, SensorOpen, SensorClose>(cmdParams.ToList());
                return true;
            }
            catch
            {
                return false;
            } 
        }

        /// <summary>
        /// KVVs the imit test.
        /// </summary>
        /// <param name="cmdParams">The command parameters.</param>
        /// <param name="errString">The error string.</param>
        /// <returns><c>true</c> если проверка успешно пройдена, иначе <c>false</c>.</returns>
        private bool KvvImitTest(string[] cmdParams, out string errString)
        {
            string errParam = string.Empty;
            bool isParamCountErr = true;
            List<Type> uniParam = new List<Type>();
            errString = Resource.Get(@"eParamCount");

            // исключаем дублирующие параметры.
            if (cmdParams.Distinct().Count() != cmdParams.Length)
            {
                errString = Resource.Get(@"eUnique");
                return false;
            }

            if (1 > cmdParams.Length)
            {
                return !isParamCountErr;
            }

            if (!IncludeTest<SetsKvvImit>(cmdParams[0], out errParam, ref uniParam))
            {
                errString = string.Format(Resource.Get(@"eArg1"), errParam);
                return false;
            }

            if (2 > cmdParams.Length)
            {
                return !isParamCountErr;
            }

            if (!IncludeTest<SwitcherKvvImit, HalfsetKvvImit>(cmdParams[1], out errParam, ref uniParam))
            {
                errString = string.Format(Resource.Get(@"eArg2"), errParam);
                return false;
            }

            // задаемся минимальным количеством параметров для команды (2).
            isParamCountErr = false;

            if (3 > cmdParams.Length)
            {
                return !isParamCountErr;
            }

            if (!IncludeTest<HalfsetKvvImit, StateReady, StateBusy, StateMe>(cmdParams[2], out errParam, ref uniParam))
            {
                errString = string.Format(Resource.Get(@"eArg3"), errParam);
                return false;
            }
            
            if (4 > cmdParams.Length)
            {
                return !isParamCountErr;
            }

            if (!IncludeTest<StateReady, StateBusy, StateMe>(cmdParams[3], out errParam, ref uniParam))
            {
                errString = string.Format(Resource.Get(@"eArg4"), errParam);
                return false;
            }

            if (5 > cmdParams.Length)
            {
                return !isParamCountErr;
            }

            if (!IncludeTest<StateReady, StateBusy, StateMe>(cmdParams[4], out errParam, ref uniParam))
            {
                errString = string.Format(Resource.Get(@"eArg5"), errParam);
                return false;
            }

            if (6 > cmdParams.Length)
            {
                return !isParamCountErr;
            }

            if (!IncludeTest<StateReady, StateBusy, StateMe>(cmdParams[5], out errParam, ref uniParam))
            {
                errString = string.Format(Resource.Get(@"eArg6"), errParam);
                return false;
            }

            // задаемся максимальным количеством параметров для команды (6).
            isParamCountErr = true;

            if (6 < cmdParams.Length)
            {
                return !isParamCountErr;
            }

            return true;
        }

        /// <summary>
        /// KVVs the imit execute.
        /// </summary>
        /// <param name="cmdParams">The command parameters.</param>
        /// <returns><c>true</c> если команда выполнена, иначе <c>false</c>.</returns>
        /// <exception cref="System.NotImplementedException">Нет реализации.</exception>
        private bool KvvImitExec(string[] cmdParams)
        {
            try
            {
                IncludeExec<SetsKvvImit, SwitcherKvvImit, HalfsetKvvImit, StateReady, StateBusy, StateMe>(cmdParams.ToList());
                return true;
            }
            catch
            {
                return false;
            } 
        }

        /// <summary>
        /// Busks the imit test.
        /// </summary>
        /// <param name="cmdParams">The command parameters.</param>
        /// <param name="errString">The error string.</param>
        /// <returns><c>true</c> если проверка успешно пройдена, иначе <c>false</c>.</returns>
        private bool Bm4ImitTest(string[] cmdParams, out string errString)
        {
            string errParam = string.Empty;
            bool isParamCountErr = true;
            List<Type> uniParam = new List<Type>();
            errString = Resource.Get(@"eParamCount");

            // исключаем дублирующие параметры.
            if (cmdParams.Distinct().Count() != cmdParams.Length)
            {
                errString = Resource.Get(@"eUnique");
                return false;
            }

            if (1 > cmdParams.Length)
            {
                return !isParamCountErr;
            }

            // устанавливаем минимальное количество параметров в команде (1).
            isParamCountErr = false;

            if (2 > cmdParams.Length)
            {
                // если минимальное количество параметров (1) проверить что это параметр "Channel".
                if (!IncludeTest<Channel>(cmdParams[0], out errParam, ref uniParam))
                {
                    errString = string.Format(Resource.Get(@"eArg1"), errParam);
                    return false;
                }

                return !isParamCountErr;
            }

            if (!IncludeTest<Channel, SwitcherBm4Imit>(cmdParams[0], out errParam, ref uniParam))
            {
                errString = string.Format(Resource.Get(@"eArg1"), errParam);
                return false;
            }
            
            if (3 > cmdParams.Length)
            {
                return !isParamCountErr;
            }

            if (!IncludeTest<SwitcherBm4Imit>(cmdParams[1], out errParam, ref uniParam))
            {
                errString = string.Format(Resource.Get(@"eArg2"), errParam);
                return false;
            }

            if (!IncludeTest<ExchangeBm4Imit, Tick, OnBoardTime>(cmdParams[2], out errParam, ref uniParam))
            {
                errString = string.Format(Resource.Get(@"eArg3"), errParam);
                return false;
            }

            if (4 > cmdParams.Length)
            {
                return !isParamCountErr;
            }

            if (!IncludeTest<Tick, OnBoardTime>(cmdParams[3], out errParam, ref uniParam))
            {
                errString = string.Format(Resource.Get(@"eArg4"), errParam);
                return false;
            }

            if (5 > cmdParams.Length)
            {
                return !isParamCountErr;
            }

            if (!IncludeTest<OnBoardTime>(cmdParams[4], out errParam, ref uniParam))
            {
                errString = string.Format(Resource.Get(@"eArg5"), errParam);
                return false;
            }

            // задаемся максимальным количеством параметров для команды (5).
            isParamCountErr = true;

            if (5 < cmdParams.Length)
            {
                return !isParamCountErr;
            }

            return true;
        }

        /// <summary>
        /// Busks the imit execute.
        /// </summary>
        /// <param name="cmdParams">The command parameters.</param>
        /// <returns><c>true</c> если команда выполнена, иначе <c>false</c>.</returns>
        /// <exception cref="System.NotImplementedException">Нет реализации.</exception>
        private bool Bm4ImitExec(string[] cmdParams)
        {
            try
            {
                IncludeExec<Channel, SwitcherBm4Imit, ExchangeBm4Imit, Tick, OnBoardTime>(cmdParams.ToList());
                return true;
            }
            catch
            {
                return false;
            }    
        }

        /// <summary>
        /// Busks the imit command test.
        /// </summary>
        /// <param name="cmdParams">The command parameters.</param>
        /// <param name="errString">The error string.</param>
        /// <returns><c>true</c> если проверка успешно пройдена, иначе <c>false</c>.</returns>
        private bool Bm4ImitCmdTest(string[] cmdParams, out string errString)
        {
            string errParam = string.Empty;
            bool isParamCountErr = true;
            List<Type> uniParam = new List<Type>();
            errString = Resource.Get(@"eParamCount");

            if (1 > cmdParams.Length)
            {
                return !isParamCountErr;
            }

            // задаемся минимальным количеством параметров для команды (1).
            isParamCountErr = false;

            if (2 > cmdParams.Length)
            {
                if (!IncludeTest<HexPackageBm4Imit>(cmdParams[0], out errParam, ref uniParam))
                {
                    errString = string.Format(Resource.Get(@"eArg1"), errParam);
                    return false;
                }

                return !isParamCountErr;
            }

            if (!IncludeTest<Command, HexPackageBm4Imit>(cmdParams[0], out errParam, ref uniParam))
            {
                errString = string.Format(Resource.Get(@"eArg1"), errParam);
                return false;
            }

            if (!IncludeTest<Apid, Rece, Exec, HexPackageBm4Imit>(cmdParams[1], out errParam, ref uniParam))
            {
                errString = string.Format(Resource.Get(@"eArg2"), errParam);
                return false;
            }

            if (3 > cmdParams.Length)
            {
                return !isParamCountErr;
            }

            if (!IncludeTest<Rece, Exec, HexPackageBm4Imit>(cmdParams[2], out errParam, ref uniParam))
            {
                errString = string.Format(Resource.Get(@"eArg3"), errParam);
                return false;
            }

            if (4 > cmdParams.Length)
            {
                return !isParamCountErr;
            }

            if (!IncludeTest<Rece, Exec, HexPackageBm4Imit>(cmdParams[3], out errParam, ref uniParam))
            {
                errString = string.Format(Resource.Get(@"eArg4"), errParam);
                return false;
            }

            if (!NeedTest<Command>(cmdParams[0], out errParam, ref uniParam))
            {
                if (!IncludeTest<HexPackageBm4Imit>(cmdParams, out errParam, ref uniParam))
                {
                    errString = string.Format(Resource.Get(@"eArg1"), errParam);
                    return false;
                }
            }
            else
            {
                int skipCount = 1;
                if (NeedTest<Apid>(cmdParams[1], out errParam, ref uniParam))
                { 
                    skipCount++;
                }

                if (NeedTest<Rece>(cmdParams[1], out errParam, ref uniParam))
                {
                    skipCount++;
                }

                if (NeedTest<Exec>(cmdParams[1], out errParam, ref uniParam))
                {
                    skipCount++;
                }

                if (3 < cmdParams.Length)
                {
                    if (NeedTest<Rece>(cmdParams[2], out errParam, ref uniParam))
                    {
                        skipCount++;
                    }

                    if (NeedTest<Exec>(cmdParams[2], out errParam, ref uniParam))
                    {
                        skipCount++;
                    }
                }

                if (4 < cmdParams.Length)
                {
                    if (NeedTest<Rece>(cmdParams[3], out errParam, ref uniParam))
                    {
                        skipCount++;
                    }

                    if (NeedTest<Exec>(cmdParams[3], out errParam, ref uniParam))
                    {
                        skipCount++;
                    }
                }

                if (!IncludeTest<HexPackageBm4Imit>(cmdParams.Skip(skipCount).ToArray(), out errParam, ref uniParam))
                {
                    errString = string.Format(Resource.Get(@"eArg5"), errParam);
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Busks the imit command execute.
        /// </summary>
        /// <param name="cmdParams">The command parameters.</param>
        /// <returns><c>true</c> если команда выполнена, иначе <c>false</c>.</returns>
        /// <exception cref="System.NotImplementedException">Нет реализации.</exception>
        private bool Bm4ImitCmdExec(string[] cmdParams)
        {
            try
            {
                IncludeExec<Command, Apid, Rece, Exec, HexPackageBm4Imit>(cmdParams.ToList());
                return true;
            }
            catch
            {
                return false;
            }            
        }

        /// <summary>
        /// Busks the imit command file test.
        /// </summary>
        /// <param name="cmdParams">The command parameters.</param>
        /// <param name="errString">The error string.</param>
        /// <returns><c>true</c> если проверка успешно пройдена, иначе <c>false</c>.</returns>
        private bool BukKvvImitTest(string[] cmdParams, out string errString)
        {
            string errParam = string.Empty;
            bool isParamCountErr = true;
            List<Type> uniParam = new List<Type>();
            errString = Resource.Get(@"eParamCount");

            // исключаем дублирующие параметры.
            if (cmdParams.Distinct().Count() != cmdParams.Length)
            {
                errString = Resource.Get(@"eUnique");
                return false;
            }

            if (1 > cmdParams.Length)
            {
                return !isParamCountErr;
            }

            if (!IncludeTest<Receive, Send, Poll>(cmdParams[0], out errParam, ref uniParam))
            {
                errString = string.Format(Resource.Get(@"eArg1"), errParam);
                return false;
            }

            // устанавливаем минимальное количество параметров в команде (1).
            isParamCountErr = false;

            if (2 > cmdParams.Length)
            {
                return !isParamCountErr;
            }

            if (!IncludeTest<Send, Poll>(cmdParams[1], out errParam, ref uniParam))
            {
                errString = string.Format(Resource.Get(@"eArg2"), errParam);
                return false;
            }

            if (3 > cmdParams.Length)
            {
                return !isParamCountErr;
            }

            if (!IncludeTest<Poll>(cmdParams[2], out errParam, ref uniParam))
            {
                errString = string.Format(Resource.Get(@"eArg3"), errParam);
                return false;
            }

            // задаемся максимальным количеством параметров для команды (3).
            isParamCountErr = true;

            if (3 < cmdParams.Length)
            {
                return !isParamCountErr;
            }

            return true;
        }

        /// <summary>
        /// Busks the imit command file execute.
        /// </summary>
        /// <param name="cmdParams">The command parameters.</param>
        /// <returns><c>true</c> если команда выполнена, иначе <c>false</c>.</returns>
        /// <exception cref="System.NotImplementedException">Нет реализации.</exception>
        private bool BukKvvImitExec(string[] cmdParams)
        {
            try
            {
                IncludeExec<Receive, Send, Poll>(cmdParams.ToList());
                return true;
            }
            catch
            {
                return false;
            } 
        }

        /// <summary>
        /// Busks the imit logic test.
        /// </summary>
        /// <param name="cmdParams">The command parameters.</param>
        /// <param name="errString">The error string.</param>
        /// <returns><c>true</c> если проверка успешно пройдена, иначе <c>false</c>.</returns>
        private bool BukKvvImitCmdTest(string[] cmdParams, out string errString)
        {
            string errParam = string.Empty;
            bool isParamCountErr = true;
            List<Type> uniParam = new List<Type>();
            errString = Resource.Get(@"eParamCount");

            if (1 > cmdParams.Length)
            {
                return !isParamCountErr;
            }

            // задаемся минимальным количеством параметров для команды (1).
            isParamCountErr = false;

            if (2 > cmdParams.Length)
            {
                if (!IncludeTest<HexPackageBukKvvImit>(cmdParams[0], out errParam, ref uniParam))
                {
                    errString = string.Format(Resource.Get(@"eArg1"), errParam);
                    return false;
                }

                return !isParamCountErr;
            }

            if (!IncludeTest<Activate, HexPackageBukKvvImit>(cmdParams[0], out errParam, ref uniParam))
            {
                errString = string.Format(Resource.Get(@"eArg1"), errParam);
                return false;
            }

            if (!IncludeTest<HexPackageBukKvvImit>(cmdParams[1], out errParam, ref uniParam))
            {
                errString = string.Format(Resource.Get(@"eArg2"), errParam);
                return false;
            }

            if (3 > cmdParams.Length)
            {
                return !isParamCountErr;
            }

            if (!NeedTest<Activate>(cmdParams[0], out errParam, ref uniParam))
            {
                if (!IncludeTest<HexPackageBukKvvImit>(cmdParams, out errParam, ref uniParam))
                {
                    errString = string.Format(Resource.Get(@"eArg1"), errParam);
                    return false;
                }
            }
            else
            {
                if (!IncludeTest<HexPackageBukKvvImit>(cmdParams.Skip(1).ToArray(), out errParam, ref uniParam))
                {
                    errString = string.Format(Resource.Get(@"eArg2"), errParam);
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Busks the imit logic execute.
        /// </summary>
        /// <param name="cmdParams">The command parameters.</param>
        /// <returns><c>true</c> если команда выполнена, иначе <c>false</c>.</returns>
        /// <exception cref="System.NotImplementedException">Нет реализации.</exception>
        private bool BukKvvImitCmdExec(string[] cmdParams)
        {
            try
            {
                IncludeExec<Activate, HexPackageBukKvvImit>(cmdParams.ToList());
                return true;
            }
            catch
            {
                return false;
            } 
        }

        /// <summary>
        /// Scidevs the imit test.
        /// </summary>
        /// <param name="cmdParams">The command parameters.</param>
        /// <param name="errString">The error string.</param>
        /// <returns><c>true</c> если проверка успешно пройдена, иначе <c>false</c>.</returns>
        private bool DetectorImitTest(string[] cmdParams, out string errString)
        {
            string errParam = string.Empty;
            bool isParamCountErr = true;
            List<Type> uniParam = new List<Type>();
            errString = Resource.Get(@"eParamCount");

            // исключаем дублирующие параметры.
            if (cmdParams.Distinct().Count() != cmdParams.Length)
            {
                errString = Resource.Get(@"eUnique");
                return false;
            }

            if (1 > cmdParams.Length)
            {
                return !isParamCountErr;
            }

            if (!IncludeTest<ScidevDetectorImit>(cmdParams[0], out errParam, ref uniParam))
            {
                errString = string.Format(Resource.Get(@"eArg1"), errParam);
                return false;
            }

            // задаемся минимальным количеством параметров для команды (1).
            isParamCountErr = false;

            if (2 > cmdParams.Length)
            {
                return !isParamCountErr;
            }

            if (!IncludeTest<HalfsetDetectorImit>(cmdParams[1], out errParam, ref uniParam))
            {
                errString = string.Format(Resource.Get(@"eArg2"), errParam);
                return false;
            }

            if (3 > cmdParams.Length)
            {
                return !isParamCountErr;
            }

            if (!IncludeTest<SwitcherDetectorImit>(cmdParams[2], out errParam, ref uniParam))
            {
                errString = string.Format(Resource.Get(@"eArg3"), errParam);
                return false;
            }

            // задаемся максимальным количеством параметров для команды (3).
            isParamCountErr = true;

            if (3 < cmdParams.Length)
            {
                return !isParamCountErr;
            }

            return true;
        }

        /// <summary>
        /// Scidevs the imit execute.
        /// </summary>
        /// <param name="cmdParams">The command parameters.</param>
        /// <returns><c>true</c> если команда выполнена, иначе <c>false</c>.</returns>
        /// <exception cref="System.NotImplementedException">Нет реализации.</exception>
        private bool DetectorImitExec(string[] cmdParams)
        {
            try
            {
                IncludeExec<ScidevDetectorImit, HalfsetDetectorImit, SwitcherDetectorImit>(cmdParams.ToList());
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Набор статических команд, для выполнения циклограммы.
        /// </summary>
        public static class Execut
        {
            /// <summary>
            /// The bin log KVV imit on
            /// </summary>
            private static readonly Action<EgseBukNotify> BinLogKvvImitOn = new Action<EgseBukNotify>(x => x.HsiNotify.IsSaveRawData = true);
            
            /// <summary>
            /// The bin log KVV imit off
            /// </summary>
            private static readonly Action<EgseBukNotify> BinLogKvvImitOff = new Action<EgseBukNotify>(x => x.HsiNotify.IsSaveRawData = false);
            
            /// <summary>
            /// The bin log detector imit on
            /// </summary>
            private static readonly Action<EgseBukNotify> BinLogDetectorImitOn = new Action<EgseBukNotify>(x => x.Spacewire3Notify.IsSaveRawData = true);
           
            /// <summary>
            /// The bin log detector imit off
            /// </summary>           
            private static readonly Action<EgseBukNotify> BinLogDetectorImitOff = new Action<EgseBukNotify>(x => x.Spacewire3Notify.IsSaveRawData = false);
           
            /// <summary>
            /// The bin log BM4 imit on
            /// </summary>
            private static readonly Action<EgseBukNotify> BinLogBm4ImitOn = new Action<EgseBukNotify>(x => x.Spacewire2Notify.IsSaveRawData = true);
            
            /// <summary>
            /// The bin log BM4 imit off
            /// </summary>
            private static readonly Action<EgseBukNotify> BinLogBm4ImitOff = new Action<EgseBukNotify>(x => x.Spacewire2Notify.IsSaveRawData = false);
           
            /// <summary>
            /// The text log detector imit on
            /// </summary>
            private static readonly Action<EgseBukNotify> TxtLogDetectorImitOn = new Action<EgseBukNotify>(x => x.Spacewire3Notify.IsSaveTxtData = true);
           
            /// <summary>
            /// The text log detector imit off
            /// </summary>
            private static readonly Action<EgseBukNotify> TxtLogDetectorImitOff = new Action<EgseBukNotify>(x => { x.Spacewire3Notify.IsSaveTxtData = false; });
            
            /// <summary>
            /// The text log BM4 imit on
            /// </summary>
            private static readonly Action<EgseBukNotify> TxtLogBm4ImitOn = new Action<EgseBukNotify>(x => { x.Spacewire2Notify.IsSaveTxtData = true; });
           
            /// <summary>
            /// The text log BM4 imit off
            /// </summary>
            private static readonly Action<EgseBukNotify> TxtLogBm4ImitOff = new Action<EgseBukNotify>(x => { x.Spacewire2Notify.IsSaveTxtData = false; });
            
            /// <summary>
            /// The text log KVV imit on
            /// </summary>
            private static readonly Action<EgseBukNotify> TxtLogKvvImitOn = new Action<EgseBukNotify>(x => { x.HsiNotify.IsSaveTxtData = true; });
            
            /// <summary>
            /// The text log KVV imit off
            /// </summary>
            private static readonly Action<EgseBukNotify> TxtLogKvvImitOff = new Action<EgseBukNotify>(x => { x.HsiNotify.IsSaveTxtData = false; });
           
            /// <summary>
            /// The exchange BM4 imit on
            /// </summary>
            private static readonly Action<EgseBukNotify> ExchangeBm4ImitOn = new Action<EgseBukNotify>(x => { x.Spacewire2Notify.IsIssueExchange = true; });
           
            /// <summary>
            /// The exchange BM4 imit off
            /// </summary>
            private static readonly Action<EgseBukNotify> ExchangeBm4ImitOff = new Action<EgseBukNotify>(x => { x.Spacewire2Notify.IsIssueExchange = false; });
           
            /// <summary>
            /// The exchange buk BM4 imit on
            /// </summary>
            private static readonly Action<EgseBukNotify> ExchangeBukBm4ImitOn = new Action<EgseBukNotify>(x => { x.Spacewire1Notify.IsIssueExchange = true; });
            
            /// <summary>
            /// The exchange buk BM4 imit off
            /// </summary>
            private static readonly Action<EgseBukNotify> ExchangeBukBm4ImitOff = new Action<EgseBukNotify>(x => { x.Spacewire1Notify.IsIssueExchange = false; });
           
            /// <summary>
            /// The issue eep
            /// </summary>
            private static readonly Action<EgseBukNotify> IssueEep = new Action<EgseBukNotify>(x => { x.Spacewire4Notify.IsIssueEep = true; });
           
            /// <summary>
            /// The device busk
            /// </summary>
            private static readonly Action<EgseBukNotify> DeviceBusk = new Action<EgseBukNotify>(x => { transaction |= (ushort)PowerTransaction.Busk; PowerTransactionExec(x, transaction); });
           
            /// <summary>
            /// The device buk
            /// </summary>
            private static readonly Action<EgseBukNotify> DeviceBuk = new Action<EgseBukNotify>(x => { Task.Run(() => { MessageBox.Show(Resource.Get(@"eNop")); }); });
            
            /// <summary>
            /// The device bund
            /// </summary>
            private static readonly Action<EgseBukNotify> DeviceBund = new Action<EgseBukNotify>(x => { transaction |= (ushort)PowerTransaction.Bund; PowerTransactionExec(x, transaction); });
           
            /// <summary>
            /// The switcher BM4 imit on
            /// </summary>
            private static readonly Action<EgseBukNotify> SwitcherBm4ImitOn = new Action<EgseBukNotify>(x => { x.Spacewire2Notify.IsIssueEnable = true; });
          
            /// <summary>
            /// The switcher BM4 imit off
            /// </summary>
            private static readonly Action<EgseBukNotify> SwitcherBm4ImitOff = new Action<EgseBukNotify>(x => { x.Spacewire2Notify.IsIssueEnable = false; });
           
            /// <summary>
            /// The switcher detector imit on
            /// </summary>
            private static readonly Action<EgseBukNotify> SwitcherDetectorImitOn = new Action<EgseBukNotify>(x => { x.Spacewire3Notify.IsIssueEnable = true; });
           
            /// <summary>
            /// The switcher detector imit off
            /// </summary>
            private static readonly Action<EgseBukNotify> SwitcherDetectorImitOff = new Action<EgseBukNotify>(x => { x.Spacewire3Notify.IsIssueEnable = false; });
            
            /// <summary>
            /// The switcher buk BM4 imit on
            /// </summary>
            private static readonly Action<EgseBukNotify> SwitcherBukBm4ImitOn = new Action<EgseBukNotify>(x => { x.Spacewire1Notify.IsIssueEnable = true; });
           
            /// <summary>
            /// The switcher buk BM4 imit off
            /// </summary>
            private static readonly Action<EgseBukNotify> SwitcherBukBm4ImitOff = new Action<EgseBukNotify>(x => { x.Spacewire1Notify.IsIssueEnable = false; });
           
            /// <summary>
            /// The switcher buk detector imit on
            /// </summary>
            private static readonly Action<EgseBukNotify> SwitcherBukDetectorImitOn = new Action<EgseBukNotify>(x => { x.Spacewire4Notify.IsIssueEnable = true; });
          
            /// <summary>
            /// The switcher buk detector imit off
            /// </summary>
            private static readonly Action<EgseBukNotify> SwitcherBukDetectorImitOff = new Action<EgseBukNotify>(x => { x.Spacewire4Notify.IsIssueEnable = false; });
          
            /// <summary>
            /// The switcher power on
            /// </summary>
            private static readonly Action<EgseBukNotify> SwitcherPowerOn = new Action<EgseBukNotify>(x => { transaction |= (ushort)PowerTransaction.On; PowerTransactionExec(x, transaction); });
         
            /// <summary>
            /// The switcher power off
            /// </summary>
            private static readonly Action<EgseBukNotify> SwitcherPowerOff = new Action<EgseBukNotify>(x => { transaction |= (ushort)PowerTransaction.Off; PowerTransactionExec(x, transaction); });
           
            /// <summary>
            /// The switcher KVV imit on
            /// </summary>
            private static readonly Action<EgseBukNotify> SwitcherKvvImitOn = new Action<EgseBukNotify>(x => { transaction |= (ushort)KvvImitTransaction.On; KvvImitTransactionExec(x, transaction); });
            
            /// <summary>
            /// The switcher KVV imit off
            /// </summary>
            private static readonly Action<EgseBukNotify> SwitcherKvvImitOff = new Action<EgseBukNotify>(x => { transaction |= (ushort)KvvImitTransaction.Off; KvvImitTransactionExec(x, transaction); });
           
            /// <summary>
            /// The state ready
            /// </summary>
            private static readonly Action<EgseBukNotify> StateReady = new Action<EgseBukNotify>(x => { transaction |= (ushort)KvvImitTransaction.Ready; KvvImitTransactionExec(x, transaction); });
           
            /// <summary>
            /// The state busy
            /// </summary>
            private static readonly Action<EgseBukNotify> StateBusy = new Action<EgseBukNotify>(x => { transaction |= (ushort)KvvImitTransaction.Busy; KvvImitTransactionExec(x, transaction); });
            
            /// <summary>
            /// The state me
            /// </summary>
            private static readonly Action<EgseBukNotify> StateMe = new Action<EgseBukNotify>(x => { transaction |= (ushort)KvvImitTransaction.Me; KvvImitTransactionExec(x, transaction); });
           
            /// <summary>
            /// The scidev detector imit ufes
            /// </summary>
            private static readonly Action<EgseBukNotify> ScidevDetectorImitUfes = new Action<EgseBukNotify>(x => { x.Spacewire3Notify.IssueDetectorDevice = EgseBukNotify.Spacewire3.DetectorDevice.Ufes; });
          
            /// <summary>
            /// The scidev detector imit vufes
            /// </summary>
            private static readonly Action<EgseBukNotify> ScidevDetectorImitVufes = new Action<EgseBukNotify>(x => { x.Spacewire3Notify.IssueDetectorDevice = EgseBukNotify.Spacewire3.DetectorDevice.Vufes; });
          
            /// <summary>
            /// The scidev detector imit SDSH
            /// </summary>
            private static readonly Action<EgseBukNotify> ScidevDetectorImitSdsh = new Action<EgseBukNotify>(x => { x.Spacewire3Notify.IssueDetectorDevice = EgseBukNotify.Spacewire3.DetectorDevice.Sdchsh; });
           
            /// <summary>
            /// The scidev shutter ufes
            /// </summary>
            private static readonly Action<EgseBukNotify> ScidevShutterUfes = new Action<EgseBukNotify>(x => { transaction |= (ushort)ShutterTransaction.Ufes; ShutterTransactionExec(x, transaction); });
          
            /// <summary>
            /// The scidev shutter vufes
            /// </summary>
            private static readonly Action<EgseBukNotify> ScidevShutterVufes = new Action<EgseBukNotify>(x => { transaction |= (ushort)ShutterTransaction.Vufes; ShutterTransactionExec(x, transaction); });
          
            /// <summary>
            /// The scidev shutter SDSH
            /// </summary>
            private static readonly Action<EgseBukNotify> ScidevShutterSdsh = new Action<EgseBukNotify>(x => { transaction |= (ushort)ShutterTransaction.Sdsh; ShutterTransactionExec(x, transaction); });
           
            /// <summary>
            /// The halfset detector imit main
            /// </summary>
            private static readonly Action<EgseBukNotify> HalfsetDetectorImitMain = new Action<EgseBukNotify>(x => { x.Spacewire3Notify.IssueHalfSet = EgseBukNotify.Spacewire3.HalfSet.Main; });
          
            /// <summary>
            /// The halfset detector imit resv
            /// </summary>
            private static readonly Action<EgseBukNotify> HalfsetDetectorImitResv = new Action<EgseBukNotify>(x => { x.Spacewire3Notify.IssueHalfSet = EgseBukNotify.Spacewire3.HalfSet.Resv; });
           
            /// <summary>
            /// The halfset KVV imit main
            /// </summary>
            private static readonly Action<EgseBukNotify> HalfsetKvvImitMain = new Action<EgseBukNotify>(x => { transaction |= (ushort)KvvImitTransaction.Main; KvvImitTransactionExec(x, transaction); });
           
            /// <summary>
            /// The halfset KVV imit resv
            /// </summary>
            private static readonly Action<EgseBukNotify> HalfsetKvvImitResv = new Action<EgseBukNotify>(x => { transaction |= (ushort)KvvImitTransaction.Resv; KvvImitTransactionExec(x, transaction); });
          
            /// <summary>
            /// The halfset KVV imit both
            /// </summary>
            private static readonly Action<EgseBukNotify> HalfsetKvvImitBoth = new Action<EgseBukNotify>(x => { transaction |= (ushort)KvvImitTransaction.Both; KvvImitTransactionExec(x, transaction); });
          
            /// <summary>
            /// The halfset KVV imit none
            /// </summary>
            private static readonly Action<EgseBukNotify> HalfsetKvvImitNone = new Action<EgseBukNotify>(x => { transaction |= (ushort)KvvImitTransaction.None; KvvImitTransactionExec(x, transaction); });
           
            /// <summary>
            /// The line a
            /// </summary>
            private static readonly Action<EgseBukNotify> LineA = new Action<EgseBukNotify>(x => { x.TelemetryNotify.IsBuskLineA = true; x.TelemetryNotify.IsBuskLineB = false; x.TelemetryNotify.IsBundLineA = true; x.TelemetryNotify.IsBundLineB = false; });
          
            /// <summary>
            /// The line b
            /// </summary>
            private static readonly Action<EgseBukNotify> LineB = new Action<EgseBukNotify>(x => { x.TelemetryNotify.IsBuskLineA = false; x.TelemetryNotify.IsBuskLineB = true; x.TelemetryNotify.IsBundLineA = false; x.TelemetryNotify.IsBundLineB = true; });
           
            /// <summary>
            /// The line ab
            /// </summary>
            private static readonly Action<EgseBukNotify> LineAB = new Action<EgseBukNotify>(x => { x.TelemetryNotify.IsBuskLineA = true; x.TelemetryNotify.IsBuskLineB = true; x.TelemetryNotify.IsBundLineA = true; x.TelemetryNotify.IsBundLineB = true; });
            
            /// <summary>
            /// The control automatic
            /// </summary>
            private static readonly Action<EgseBukNotify> ControlAuto = new Action<EgseBukNotify>(x => { x.IsIssueManualShutter = false; });

            /// <summary>
            /// Выполняет команду: установить канал БМ-4 в режим БУК ПК1 : БМ-4 ПК1.
            /// </summary>
            private static readonly Action<EgseBukNotify> ChannelCh11 = new Action<EgseBukNotify>(x => x.Spacewire2Notify.IssueSpacewireChannel = EgseBukNotify.Spacewire2.Channel.BUK1BM1);
            
            /// <summary>
            /// Выполняет команду: установить канал БМ-4 в режим БУК ПК1 : БМ-4 ПК2.
            /// </summary>
            private static readonly Action<EgseBukNotify> ChannelCh12 = new Action<EgseBukNotify>(x => x.Spacewire2Notify.IssueSpacewireChannel = EgseBukNotify.Spacewire2.Channel.BUK1BM2);
           
            /// <summary>
            /// Выполняет команду: установить канал БМ-4 в режим БУК ПК2 : БМ-4 ПК1.
            /// </summary>
            private static readonly Action<EgseBukNotify> ChannelCh21 = new Action<EgseBukNotify>(x => x.Spacewire2Notify.IssueSpacewireChannel = EgseBukNotify.Spacewire2.Channel.BUK2BM1);
 
            /// <summary>
            /// Выполняет команду: установить канал БМ-4 в режим БУК ПК2 : БМ-4 ПК2.
            /// </summary>
            private static readonly Action<EgseBukNotify> ChannelCh22 = new Action<EgseBukNotify>(x => x.Spacewire2Notify.IssueSpacewireChannel = EgseBukNotify.Spacewire2.Channel.BUK2BM2);

            /// <summary>
            /// Устанавливает признак команды: формировать телекоманду.
            /// </summary>
            private static readonly Action<EgseBukNotify> CommandTele = new Action<EgseBukNotify>(x => transaction |= (ushort)Bm4ImitCmdTransaction.Tele);

            /// <summary>
            /// Выполняет команду: включить опрос данных для ВСИ интерфейса.
            /// </summary>
            private static readonly Action<EgseBukNotify> PollOn = new Action<EgseBukNotify>(x => x.HsiNotify.IsIssuePoll = true);
 
            /// <summary>
            /// Выполняет команду: выключить опрос данных для ВСИ интерфейса.
            /// </summary>
            private static readonly Action<EgseBukNotify> PollOff = new Action<EgseBukNotify>(x => x.HsiNotify.IsIssuePoll = false);

            /// <summary>
            /// Выполняет команду: включить выдачу пакетов данных от имитатор детекторов.
            /// </summary>
            private static readonly Action<EgseBukNotify> DataOn = new Action<EgseBukNotify>(x => x.Spacewire1Notify.IsSD1TransData = true);

            /// <summary>
            /// Выполняет команду: выключить выдачу пакетов данных от имитатор детекторов.
            /// </summary>
            private static readonly Action<EgseBukNotify> DataOff = new Action<EgseBukNotify>(x => x.Spacewire1Notify.IsSD1TransData = false);

            /// <summary>
            /// Выполняет команду: установить заданое число миллисекунд для выдачи данных от имитатора детекторов.
            /// </summary>
            private static readonly Action<EgseBukNotify, object> TimeArg = new Action<EgseBukNotify, object>((x, obj) => x.Spacewire1Notify.SD1SendTime = (int)obj);
            
            /// <summary>
            /// Устанавливает признак команды: включить подтверждение получения телекоманды.
            /// </summary>
            private static readonly Action<EgseBukNotify> MarkReceipt = new Action<EgseBukNotify>(x => transaction |= (ushort)Bm4ImitCmdTransaction.Receipt);
 
            /// <summary>
            /// Устанавливает признак команды: включить подтверждение исполнения телекоманды.
            /// </summary>
            private static readonly Action<EgseBukNotify> MarkExecut = new Action<EgseBukNotify>(x => transaction |= (ushort)Bm4ImitCmdTransaction.Execut);

            /// <summary>
            /// Выполняет команду: выдать УКС активации первого полукомплекта по интерфейсу ВСИ.
            /// </summary>
            private static readonly Action<EgseBukNotify> CmdActivate1 = new Action<EgseBukNotify>(x => x.HsiNotify.IssueCmdEnable1Command.Execute(null));

            /// <summary>
            /// Выполняет команду: выдать УКС активации второго полукомплекта по интерфейсу ВСИ.
            /// </summary>
            private static readonly Action<EgseBukNotify> CmdActivate2 = new Action<EgseBukNotify>(x => x.HsiNotify.IssueCmdEnable2Command.Execute(null));
            
            /// <summary>
            /// The apid argument
            /// </summary>
            private static readonly Action<EgseBukNotify, object> ApidArg = new Action<EgseBukNotify, object>((x, obj) => { x.Spacewire2Notify.Apid = (short)obj; });
            
            /// <summary>
            /// The receive main
            /// </summary>
            private static readonly Action<EgseBukNotify> ReceiveMain = new Action<EgseBukNotify>(x => { x.HsiNotify.IssueLineRecv = EgseBukNotify.Hsi.SimLine.Main; });
           
            /// <summary>
            /// The receive resv
            /// </summary>
            private static readonly Action<EgseBukNotify> ReceiveResv = new Action<EgseBukNotify>(x => { x.HsiNotify.IssueLineRecv = EgseBukNotify.Hsi.SimLine.Resv; });
          
            /// <summary>
            /// The send main
            /// </summary>
            private static readonly Action<EgseBukNotify> SendMain = new Action<EgseBukNotify>(x => { x.HsiNotify.IssueLineSend = EgseBukNotify.Hsi.SimLine.Main; });
          
            /// <summary>
            /// The send resv
            /// </summary>
            private static readonly Action<EgseBukNotify> SendResv = new Action<EgseBukNotify>(x => { x.HsiNotify.IssueLineSend = EgseBukNotify.Hsi.SimLine.Resv; });
          
            /// <summary>
            /// The tick on
            /// </summary>
            private static readonly Action<EgseBukNotify> TickOn = new Action<EgseBukNotify>(x => { x.Spacewire2Notify.IsIssueTickTime = true; });
           
            /// <summary>
            /// The tick off
            /// </summary>
            private static readonly Action<EgseBukNotify> TickOff = new Action<EgseBukNotify>(x => { x.Spacewire2Notify.IsIssueTickTime = false; });
          
            /// <summary>
            /// The sensor open on
            /// </summary>
            private static readonly Action<EgseBukNotify> SensorOpenOn = new Action<EgseBukNotify>(x => { transaction |= (ushort)ShutterTransaction.SensOpenOn; ShutterTransactionExec(x, transaction); });
          
            /// <summary>
            /// The sensor open off
            /// </summary>
            private static readonly Action<EgseBukNotify> SensorOpenOff = new Action<EgseBukNotify>(x => { transaction |= (ushort)ShutterTransaction.SensOpenOff; ShutterTransactionExec(x, transaction); });
          
            /// <summary>
            /// The sensor close on
            /// </summary>
            private static readonly Action<EgseBukNotify> SensorCloseOn = new Action<EgseBukNotify>(x => { transaction |= (ushort)ShutterTransaction.SensCloseOn; ShutterTransactionExec(x, transaction); });
            
            /// <summary>
            /// The sensor close off
            /// </summary>
            private static readonly Action<EgseBukNotify> SensorCloseOff = new Action<EgseBukNotify>(x => { transaction |= (ushort)ShutterTransaction.SensCloseOff; ShutterTransactionExec(x, transaction); });
          
            /// <summary>
            /// The on board time on
            /// </summary>
            private static readonly Action<EgseBukNotify> OnBoardTimeOn = new Action<EgseBukNotify>(x => { x.Spacewire2Notify.IsIssueObt = true; });
           
            /// <summary>
            /// The on board time off
            /// </summary>
            private static readonly Action<EgseBukNotify> OnBoardTimeOff = new Action<EgseBukNotify>(x => { x.Spacewire2Notify.IsIssueObt = false; });
           
            /// <summary>
            /// The sets power1
            /// </summary>
            private static readonly Action<EgseBukNotify> SetsPower1 = new Action<EgseBukNotify>(x => { transaction |= (ushort)PowerTransaction.Set1; PowerTransactionExec(x, transaction); });
           
            /// <summary>
            /// The sets power2
            /// </summary>
            private static readonly Action<EgseBukNotify> SetsPower2 = new Action<EgseBukNotify>(x => { transaction |= (ushort)PowerTransaction.Set2; PowerTransactionExec(x, transaction); });
          
            /// <summary>
            /// The sets KVV imit1
            /// </summary>
            private static readonly Action<EgseBukNotify> SetsKvvImit1 = new Action<EgseBukNotify>(x => { transaction |= (ushort)KvvImitTransaction.Set1; KvvImitTransactionExec(x, transaction); });
        
            /// <summary>
            /// The sets KVV imit2
            /// </summary>
            private static readonly Action<EgseBukNotify> SetsKvvImit2 = new Action<EgseBukNotify>(x => { transaction |= (ushort)KvvImitTransaction.Set2; KvvImitTransactionExec(x, transaction); });

            /// <summary>
            /// The buk KVV imit hexadecimal package argument
            /// </summary>
            private static readonly Action<EgseBukNotify, object> BukKvvImitHexPackageArg = new Action<EgseBukNotify, object>((x, obj) => { x.HsiNotify.Data = Converter.HexStrToByteArray(string.Join(@" ", obj as List<string>)); x.HsiNotify.IssueCmdCommand.Execute(null); });
         
            /// <summary>
            /// The buk detector imit hexadecimal package argument
            /// </summary>
            private static readonly Action<EgseBukNotify, object> BukDetectorImitHexPackageArg = new Action<EgseBukNotify, object>((x, obj) => { x.Spacewire4Notify.Data = Converter.HexStrToByteArray(string.Join(@" ", obj as List<string>)); x.Spacewire4Notify.IssuePackageCommand.Execute(null); });
         
            /// <summary>
            /// The BM4 imit hexadecimal package argument
            /// </summary>
            private static readonly Action<EgseBukNotify, object> Bm4ImitHexPackageArg = new Action<EgseBukNotify, object>((x, obj) => { x.Spacewire2Notify.IsConfirmExecution = ((Bm4ImitCmdTransaction)transaction).HasFlag(Bm4ImitCmdTransaction.Execut); x.Spacewire2Notify.IsConfirmReceipt = ((Bm4ImitCmdTransaction)transaction).HasFlag(Bm4ImitCmdTransaction.Receipt); x.Spacewire2Notify.IsMakeTeleCmd = ((Bm4ImitCmdTransaction)transaction).HasFlag(Bm4ImitCmdTransaction.Tele); x.Spacewire2Notify.Data = Converter.HexStrToByteArray(string.Join(@" ", obj as List<string>)); x.Spacewire2Notify.IssuePackageCommand.Execute(null); });

            /// <summary>
            /// Используется для формирования команды.
            /// </summary>
            private static ushort transaction;

            /// <summary>
            /// Возможные состояния формирования команды KVV_IMIT.
            /// </summary>
            private enum KvvImitTransaction : ushort
            {
                /// <summary>
                /// The set1
                /// </summary>
                Set1 = 0x01,

                /// <summary>
                /// The set2
                /// </summary>
                Set2 = 0x02,

                /// <summary>
                /// The on
                /// </summary>
                On = 0x04,

                /// <summary>
                /// The off
                /// </summary>
                Off = 0x08,

                /// <summary>
                /// The main
                /// </summary>
                Main = 0x10,

                /// <summary>
                /// The resv
                /// </summary>
                Resv = 0x20,

                /// <summary>
                /// The both
                /// </summary>
                Both = 0x40,

                /// <summary>
                /// The none
                /// </summary>
                None = 0x80,

                /// <summary>
                /// The ready
                /// </summary>
                Ready = 0x100,

                /// <summary>
                /// The busy
                /// </summary>
                Busy = 0x200,

                /// <summary>
                /// The Me
                /// </summary>
                Me = 0x400,
            }

            /// <summary>
            /// Возможные состояния формирования команды SHUTTER.
            /// </summary>
            private enum ShutterTransaction : ushort
            {
                /// <summary>
                /// The ufes
                /// </summary>
                Ufes = 0x01,

                /// <summary>
                /// The vufes
                /// </summary>
                Vufes = 0x02,

                /// <summary>
                /// The SDSH
                /// </summary>
                Sdsh = 0x04,

                /// <summary>
                /// The sens open on
                /// </summary>
                SensOpenOn = 0x08,

                /// <summary>
                /// The sens open off
                /// </summary>
                SensOpenOff = 0x10,

                /// <summary>
                /// The sens close on
                /// </summary>
                SensCloseOn = 0x20,

                /// <summary>
                /// The sens close off
                /// </summary>
                SensCloseOff = 0x40
            }

            /// <summary>
            /// Возможные состояния формирования команды BM4_IMIT_CMD.
            /// </summary>
            private enum Bm4ImitCmdTransaction : ushort
            {
                /// <summary>
                /// The tele
                /// </summary>
                Tele = 0x01,

                /// <summary>
                /// The receipt
                /// </summary>
                Receipt = 0x02,

                /// <summary>
                /// The execut
                /// </summary>
                Execut = 0x04
            }

            /// <summary>
            /// Возможные состояния формирования команды POWER.
            /// </summary>
            private enum PowerTransaction : ushort
            {
                /// <summary>
                /// The busk
                /// </summary>
                Busk = 0x01,

                /// <summary>
                /// The bund
                /// </summary>
                Bund = 0x02,

                /// <summary>
                /// The set1
                /// </summary>
                Set1 = 0x04,

                /// <summary>
                /// The set2
                /// </summary>
                Set2 = 0x08,

                /// <summary>
                /// The on
                /// </summary>
                On = 0x10,

                /// <summary>
                /// The off
                /// </summary>
                Off = 0x20,

                /// <summary>
                /// The busk set1 on
                /// </summary>
                [Description("включить БУСК ПК1")]
                BuskSet1On = Busk | Set1 | On,

                /// <summary>
                /// The busk set1 off
                /// </summary>
                [Description("выключить БУСК ПК1")]
                BuskSet1Off = Busk | Set1 | Off,

                /// <summary>
                /// The bund set1 on
                /// </summary>
                [Description("включить БУНД ПК1")]
                BundSet1On = Bund | Set1 | On,

                /// <summary>
                /// The bund set1 off
                /// </summary>
                [Description("выключить БУНД ПК1")]
                BundSet1Off = Bund | Set1 | Off,

                /// <summary>
                /// The busk set2 on
                /// </summary>
                [Description("включить БУСК ПК2")]
                BuskSet2On = Busk | Set2 | On,

                /// <summary>
                /// The busk set2 off
                /// </summary>
                [Description("выключить БУСК ПК2")]
                BuskSet2Off = Busk | Set2 | Off,

                /// <summary>
                /// The bund set2 on
                /// </summary>
                [Description("включить БУНД ПК2")]
                BundSet2On = Bund | Set2 | On,

                /// <summary>
                /// The bund set2 off
                /// </summary>
                [Description("выключить БУНД ПК2")]
                BundSet2Off = Bund | Set2 | Off
            }

            /// <summary>
            /// Очищает вспомогательное поля для формирования новой команды.
            /// </summary>
            internal static void ClearTransaction()
            {
                transaction = 0;
            }

            /// <summary>
            /// Выпоняет команду SHUTTER, если она сформирована.
            /// </summary>
            /// <param name="x">Нотификатор прибора.</param>
            /// <param name="ta">Состояние формирования команды.</param>
            private static void ShutterTransactionExec(EgseBukNotify x, ushort ta)
            {
                Enum en = (ShutterTransaction)ta;
                if (en.HasFlag(ShutterTransaction.Ufes))
                {
                    if (en.HasFlag(ShutterTransaction.SensOpenOn))
                    {
                        x.IsIssueManualShutter = true;
                        x.IssueUfesOpen = EgseBukNotify.SciDevState.On;
                    }
                    else if (en.HasFlag(ShutterTransaction.SensOpenOff))
                    {
                        x.IsIssueManualShutter = true;
                        x.IssueUfesOpen = EgseBukNotify.SciDevState.Off;
                    }
                    else if (en.HasFlag(ShutterTransaction.SensCloseOn))
                    {
                        x.IsIssueManualShutter = true;
                        x.IssueUfesClose = EgseBukNotify.SciDevState.On;
                    }
                    else if (en.HasFlag(ShutterTransaction.SensCloseOff))
                    {
                        x.IsIssueManualShutter = true;
                        x.IssueUfesClose = EgseBukNotify.SciDevState.Off;
                    }
                }
                else if (en.HasFlag(ShutterTransaction.Vufes))
                {
                    if (en.HasFlag(ShutterTransaction.SensOpenOn))
                    {
                        x.IsIssueManualShutter = true;
                        x.IssueVufesOpen = EgseBukNotify.SciDevState.On;
                    }
                    else if (en.HasFlag(ShutterTransaction.SensOpenOff))
                    {
                        x.IsIssueManualShutter = true;
                        x.IssueVufesOpen = EgseBukNotify.SciDevState.Off;
                    }
                    else if (en.HasFlag(ShutterTransaction.SensCloseOn))
                    {
                        x.IsIssueManualShutter = true;
                        x.IssueVufesClose = EgseBukNotify.SciDevState.On;
                    }
                    else if (en.HasFlag(ShutterTransaction.SensCloseOff))
                    {
                        x.IsIssueManualShutter = true;
                        x.IssueVufesClose = EgseBukNotify.SciDevState.Off;
                    }
                }
                else if (en.HasFlag(ShutterTransaction.Sdsh))
                {
                    if (en.HasFlag(ShutterTransaction.SensOpenOn))
                    {
                        x.IsIssueManualShutter = true;
                        x.IssueSdshOpen = EgseBukNotify.SciDevState.On;
                    }
                    else if (en.HasFlag(ShutterTransaction.SensOpenOff))
                    {
                        x.IsIssueManualShutter = true;
                        x.IssueSdshOpen = EgseBukNotify.SciDevState.Off;
                    }
                    else if (en.HasFlag(ShutterTransaction.SensCloseOn))
                    {
                        x.IsIssueManualShutter = true;
                        x.IssueSdshClose = EgseBukNotify.SciDevState.On;
                    }
                    else if (en.HasFlag(ShutterTransaction.SensCloseOff))
                    {
                        x.IsIssueManualShutter = true;
                        x.IssueSdshClose = EgseBukNotify.SciDevState.Off;
                    }
                }
            }

            /// <summary>
            /// Выпоняет команду KVV_IMIT, если она сформирована.
            /// </summary>
            /// <param name="x">Нотификатор прибора.</param>
            /// <param name="ta">Состояние формирования команды.</param>
            private static void KvvImitTransactionExec(EgseBukNotify x, ushort ta)
            {
                Enum en = (KvvImitTransaction)ta;
                if (en.HasFlag(KvvImitTransaction.Set1))
                {
                    if (en.HasFlag(KvvImitTransaction.On))
                    {
                        x.HsiNotify.IsIssueEnable1 = true;
                    }
                    else if (en.HasFlag(KvvImitTransaction.Off))
                    {
                        x.HsiNotify.IsIssueEnable1 = false;
                    }

                    if (en.HasFlag(KvvImitTransaction.Main))
                    {
                        x.HsiNotify.IssueLine1 = EgseBukNotify.Hsi.Line.Main;
                    }
                    else if (en.HasFlag(KvvImitTransaction.Resv))
                    {
                        x.HsiNotify.IssueLine1 = EgseBukNotify.Hsi.Line.Resv;
                    }
                    else if (en.HasFlag(KvvImitTransaction.Both))
                    {
                        x.HsiNotify.IssueLine1 = EgseBukNotify.Hsi.Line.Both;
                    }
                    else if (en.HasFlag(KvvImitTransaction.None))
                    {
                        x.HsiNotify.IssueLine1 = EgseBukNotify.Hsi.Line.None;
                    }

                    if (en.HasFlag(KvvImitTransaction.Ready))
                    {
                        x.HsiNotify.IsIssueReady1 = true;
                    }
                    else if (en.HasFlag(KvvImitTransaction.Busy))
                    {
                        x.HsiNotify.IsIssueBusy1 = true;
                    }
                    else if (en.HasFlag(KvvImitTransaction.Me))
                    {
                        x.HsiNotify.IsIssueMe1 = true;
                    }
                    else if (!en.HasFlag(KvvImitTransaction.Ready))
                    {
                        if (x.HsiNotify.IsIssueReady1)
                        {
                            x.HsiNotify.IsIssueReady1 = false;
                        }
                    }
                    else if (!en.HasFlag(KvvImitTransaction.Busy))
                    {
                        if (x.HsiNotify.IsIssueBusy1)
                        {
                            x.HsiNotify.IsIssueBusy1 = false;
                        }
                    }
                    else if (!en.HasFlag(KvvImitTransaction.Me))
                    {
                        if (x.HsiNotify.IsIssueMe1)
                        {
                            x.HsiNotify.IsIssueMe1 = false;
                        }
                    }
                }
                else if (en.HasFlag(KvvImitTransaction.Set2))
                {
                    if (en.HasFlag(KvvImitTransaction.On))
                    {
                        x.HsiNotify.IsIssueEnable2 = true;
                    }
                    else if (en.HasFlag(KvvImitTransaction.Off))
                    {
                        x.HsiNotify.IsIssueEnable2 = false;
                    }

                    if (en.HasFlag(KvvImitTransaction.Main))
                    {
                        x.HsiNotify.IssueLine2 = EgseBukNotify.Hsi.Line.Main;
                    }
                    else if (en.HasFlag(KvvImitTransaction.Resv))
                    {
                        x.HsiNotify.IssueLine2 = EgseBukNotify.Hsi.Line.Resv;
                    }
                    else if (en.HasFlag(KvvImitTransaction.Both))
                    {
                        x.HsiNotify.IssueLine2 = EgseBukNotify.Hsi.Line.Both;
                    }
                    else if (en.HasFlag(KvvImitTransaction.None))
                    {
                        x.HsiNotify.IssueLine2 = EgseBukNotify.Hsi.Line.None;
                    }

                    if (en.HasFlag(KvvImitTransaction.Ready))
                    {
                        x.HsiNotify.IsIssueReady2 = true;
                    }
                    else if (en.HasFlag(KvvImitTransaction.Busy))
                    {
                        x.HsiNotify.IsIssueBusy2 = true;
                    }
                    else if (en.HasFlag(KvvImitTransaction.Me))
                    {
                        x.HsiNotify.IsIssueMe2 = true;
                    }
                    else if (!en.HasFlag(KvvImitTransaction.Ready))
                    {
                        if (x.HsiNotify.IsIssueReady2)
                        {
                            x.HsiNotify.IsIssueReady2 = false;
                        }
                    }
                    else if (!en.HasFlag(KvvImitTransaction.Busy))
                    {
                        if (x.HsiNotify.IsIssueBusy2)
                        {
                            x.HsiNotify.IsIssueBusy2 = false;
                        }
                    }
                    else if (!en.HasFlag(KvvImitTransaction.Me))
                    {
                        if (x.HsiNotify.IsIssueMe2)
                        {
                            x.HsiNotify.IsIssueMe2 = false;
                        }
                    }
                }
            }

            /// <summary>
            /// Выпоняет команду POWER, если она сформирована.
            /// </summary>
            /// <param name="x">Нотификатор прибора.</param>
            /// <param name="ta">Состояние формирования команды.</param>
            private static void PowerTransactionExec(EgseBukNotify x, ushort ta)
            {
                string msgOnError = string.Format(Resource.Get(@"eDuplicate"), ((PowerTransaction)ta).Description());
                bool err = false;
                switch ((PowerTransaction)ta)
                {
                    case PowerTransaction.BuskSet1On:
                        {
                            if (!x.TelemetryNotify.IsPowerBusk1)
                            {
                                x.TelemetryNotify.IssuePowerBusk1Command.Execute(null);
                            }
                            else
                            {
                                err = true;
                            }
                        }

                        break;
                    case PowerTransaction.BuskSet1Off:
                        {
                            if (x.TelemetryNotify.IsPowerBusk1)
                            {
                                x.TelemetryNotify.IssuePowerBusk1Command.Execute(null);
                            }
                            else
                            {
                                err = true;
                            }
                        }

                        break;
                    case PowerTransaction.BundSet1On:
                        {
                            if (!x.TelemetryNotify.IsPowerBund1)
                            {
                                x.TelemetryNotify.IssuePowerBund1Command.Execute(null);
                            }
                            else
                            {
                                err = true;
                            }
                        }

                        break;
                    case PowerTransaction.BundSet1Off:
                        {
                            if (x.TelemetryNotify.IsPowerBund1)
                            {
                                x.TelemetryNotify.IssuePowerBund1Command.Execute(null);
                            }
                            else
                            {
                                err = true;
                            }
                        }

                        break;
                    case PowerTransaction.BuskSet2On:
                        {
                            if (!x.TelemetryNotify.IsPowerBusk2)
                            {
                                x.TelemetryNotify.IssuePowerBusk2Command.Execute(null);
                            }
                            else
                            {
                                err = true;
                            }
                        }

                        break;
                    case PowerTransaction.BuskSet2Off:
                        {
                            if (x.TelemetryNotify.IsPowerBusk2)
                            {
                                x.TelemetryNotify.IssuePowerBusk2Command.Execute(null);
                            }
                            else
                            {
                                err = true;
                            }
                        }

                        break;
                    case PowerTransaction.BundSet2On:
                        {
                            if (!x.TelemetryNotify.IsPowerBund2)
                            {
                                x.TelemetryNotify.IssuePowerBund2Command.Execute(null);
                            }
                            else
                            {
                                err = true;
                            }
                        }

                        break;
                    case PowerTransaction.BundSet2Off:
                        {
                            if (x.TelemetryNotify.IsPowerBund2)
                            {
                                x.TelemetryNotify.IssuePowerBund2Command.Execute(null);
                            }
                            else
                            {
                                err = true;
                            }
                        }

                        break;
                    default:
                        break;
                }

                if (err)
                {
                    Task.Run(() => { MessageBox.Show(msgOnError); });
                }
            }
        }
    }
}
