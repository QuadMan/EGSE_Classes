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
    using System.Linq;
    using Egse.Cyclogram;
    using Egse.Devices;
    using Egse.Utilites;
    using Egse.CustomAttributes;
    using System.Linq.Expressions;
    using System.Windows;
    using System.Globalization;

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

        private bool BukDetectorImitCmdExec(string[] cmdParams)
        {
            try
            {
                IncludeExec<ErrorEndPoint, HexPackage>(cmdParams.ToList());
                return true;
            }
            catch
            {
                return false;
            } 
        }

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
                if (!IncludeTest<HexPackage>(cmdParams[0], out errParam, ref uniParam))
                {
                    errString = string.Format(Resource.Get(@"eArg1"), errParam);
                    return false;
                }

                return !isParamCountErr;
            }

            if (!IncludeTest<ErrorEndPoint, HexPackage>(cmdParams[0], out errParam, ref uniParam))
            {
                errString = string.Format(Resource.Get(@"eArg1"), errParam);
                return false;
            }

            if (!IncludeTest<HexPackage>(cmdParams[1], out errParam, ref uniParam))
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
                if (!IncludeTest<HexPackage>(cmdParams, out errParam, ref uniParam))
                {
                    errString = string.Format(Resource.Get(@"eArg1"), errParam);
                    return false;
                }
            }
            else
            {
                if (!IncludeTest<HexPackage>(cmdParams.Skip(1).ToArray(), out errParam, ref uniParam))
                {
                    errString = string.Format(Resource.Get(@"eArg2"), errParam);
                    return false;
                }
            }
            
            return true;
        }

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

        private bool BukBm4ImitExec(string[] cmdParams)
        {
            try
            {
                IncludeExec<SwitcherBukBm4Imit, Exchange, Data, Time>(cmdParams.ToList());
                return true;
            }
            catch
            {
                return false;
            }  
        }

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

            if (!IncludeTest<Exchange, Data, Time>(cmdParams[1], out errParam, ref uniParam))
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
      
        private enum StateReady
        {
            [Action(typeof(Execut), "StateReady")]
            READY
        }

        private enum StateBusy
        {
            [Action(typeof(Execut), "StateBusy")]
            BUSY
        }

        private enum StateMe
        {
            [Action(typeof(Execut), "StateMe")]
            ME
        }

        private enum SwitcherDetectorImit
        {
            [Action(typeof(Execut), "SwitcherDetectorImitOn")]
            ON,

            [Action(typeof(Execut), "SwitcherDetectorImitOff")]
            OFF
        }

        private enum SwitcherBukBm4Imit
        {
            [Action(typeof(Execut), "SwitcherBukBm4ImitOn")]
            ON,

            [Action(typeof(Execut), "SwitcherBukBm4ImitOff")]
            OFF
        }

        private enum SwitcherBukDetectorImit
        {
            [Action(typeof(Execut), "SwitcherBukDetectorImitOn")]
            ON,

            [Action(typeof(Execut), "SwitcherBukDetectorImitOff")]
            OFF
        }

        private enum SwitcherPower
        {
            [Action(typeof(Execut), "SwitcherPowerOn")]
            ON,

            [Action(typeof(Execut), "SwitcherPowerOff")]
            OFF
        }

        private enum SwitcherKvvImit
        {
            [Action(typeof(Execut), "SwitcherKvvImitOn")]
            ON,

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

        private enum ErrorEndPoint
        {
            [Action(typeof(Execut), "IssueEep")]
            EEP
        }
        
        private enum TxtLogDetectorImit
        {
            [Action(typeof(Execut), "TxtLogDetectorImitOn")]
            TXT_ON,

            [Action(typeof(Execut), "TxtLogDetectorImitOff")]
            TXT_OFF
        }

        private enum TxtLogKvvImit
        {
            [Action(typeof(Execut), "TxtLogKvvImitOn")]
            TXT_ON,

            [Action(typeof(Execut), "TxtLogKvvImitOff")]
            TXT_OFF
        }

        private enum TxtLogBm4Imit
        {
            [Action(typeof(Execut), "TxtLogBm4ImitOn")]
            TXT_ON,

            [Action(typeof(Execut), "TxtLogBm4ImitOff")]
            TXT_OFF
        }

        public static class Execut
        {
            public static readonly Action<EgseBukNotify> BinLogKvvImitOn = new Action<EgseBukNotify>(x => { MessageBox.Show("executed BinLogKvvImitOn"); });
            public static readonly Action<EgseBukNotify> BinLogKvvImitOff = new Action<EgseBukNotify>(x => { MessageBox.Show("executed BinLogKvvImitOff"); });
            public static readonly Action<EgseBukNotify> BinLogDetectorImitOn = new Action<EgseBukNotify>(x => { MessageBox.Show("executed BinLogDetectorImitOn"); });
            public static readonly Action<EgseBukNotify> BinLogDetectorImitOff = new Action<EgseBukNotify>(x => { MessageBox.Show("executed BinLogDetectorImitOff"); });
            public static readonly Action<EgseBukNotify> BinLogBm4ImitOn = new Action<EgseBukNotify>(x => { MessageBox.Show("executed BinLogBm4ImitOn"); });
            public static readonly Action<EgseBukNotify> BinLogBm4ImitOff = new Action<EgseBukNotify>(x => { MessageBox.Show("executed BinLogBm4ImitOff"); });
            public static readonly Action<EgseBukNotify> TxtLogDetectorImitOn = new Action<EgseBukNotify>(x => { MessageBox.Show("executed TxtLogDetectorImitOn"); });
            public static readonly Action<EgseBukNotify> TxtLogDetectorImitOff = new Action<EgseBukNotify>(x => { MessageBox.Show("executed TxtLogDetectorImitOff"); });
            public static readonly Action<EgseBukNotify> TxtLogBm4ImitOn = new Action<EgseBukNotify>(x => { MessageBox.Show("executed TxtLogBm4ImitOn"); });
            public static readonly Action<EgseBukNotify> TxtLogBm4ImitOff = new Action<EgseBukNotify>(x => { MessageBox.Show("executed TxtLogBm4ImitOff"); });
            public static readonly Action<EgseBukNotify> TxtLogKvvImitOn = new Action<EgseBukNotify>(x => { MessageBox.Show("executed TxtLogKvvImitOn"); });
            public static readonly Action<EgseBukNotify> TxtLogKvvImitOff = new Action<EgseBukNotify>(x => { MessageBox.Show("executed TxtLogKvvImitOff"); });            
            public static readonly Action<EgseBukNotify> ExchangeOn = new Action<EgseBukNotify>(x => { MessageBox.Show("executed ExchangeOn"); });
            public static readonly Action<EgseBukNotify> ExchangeOff = new Action<EgseBukNotify>(x => { MessageBox.Show("executed ExchangeOff"); });
            public static readonly Action<EgseBukNotify> IssueEep = new Action<EgseBukNotify>(x => { MessageBox.Show("executed IssueEep"); });
            public static readonly Action<EgseBukNotify> DeviceBusk = new Action<EgseBukNotify>(x => { MessageBox.Show("executed DeviceBusk"); });
            public static readonly Action<EgseBukNotify> DeviceBuk = new Action<EgseBukNotify>(x => { MessageBox.Show("executed DeviceBuk"); });
            public static readonly Action<EgseBukNotify> DeviceBund = new Action<EgseBukNotify>(x => { MessageBox.Show("executed DeviceBund"); });
            public static readonly Action<EgseBukNotify> SwitcherBm4ImitOn = new Action<EgseBukNotify>(x => { MessageBox.Show("executed SwitcherBm4ImitOn"); });
            public static readonly Action<EgseBukNotify> SwitcherBm4ImitOff = new Action<EgseBukNotify>(x => { MessageBox.Show("executed SwitcherBm4ImitOff"); });
            public static readonly Action<EgseBukNotify> SwitcherDetectorImitOn = new Action<EgseBukNotify>(x => { MessageBox.Show("executed SwitcherDetectorImitOn"); });
            public static readonly Action<EgseBukNotify> SwitcherDetectorImitOff = new Action<EgseBukNotify>(x => { MessageBox.Show("executed SwitcherDetectorImitOff"); });
            public static readonly Action<EgseBukNotify> SwitcherBukBm4ImitOn = new Action<EgseBukNotify>(x => { MessageBox.Show("executed SwitcherBukBm4ImitOn"); });
            public static readonly Action<EgseBukNotify> SwitcherBukBm4ImitOff = new Action<EgseBukNotify>(x => { MessageBox.Show("executed SwitcherBukBm4ImitOff"); });
            public static readonly Action<EgseBukNotify> SwitcherBukDetectorImitOn = new Action<EgseBukNotify>(x => { MessageBox.Show("executed SwitcherBukDetectorImitOn"); });
            public static readonly Action<EgseBukNotify> SwitcherBukDetectorImitOff = new Action<EgseBukNotify>(x => { MessageBox.Show("executed SwitcherBukDetectorImitOff"); });
            public static readonly Action<EgseBukNotify> SwitcherPowerOn = new Action<EgseBukNotify>(x => { MessageBox.Show("executed SwitcherPowerOn"); });
            public static readonly Action<EgseBukNotify> SwitcherPowerOff = new Action<EgseBukNotify>(x => { MessageBox.Show("executed SwitcherPowerOff"); });
            public static readonly Action<EgseBukNotify> SwitcherKvvImitOn = new Action<EgseBukNotify>(x => { MessageBox.Show("executed SwitcherKvvImitOn"); });
            public static readonly Action<EgseBukNotify> SwitcherKvvImitOff = new Action<EgseBukNotify>(x => { MessageBox.Show("executed SwitcherKvvImitOff"); });                                                            
            public static readonly Action<EgseBukNotify> StateReady = new Action<EgseBukNotify>(x => { MessageBox.Show("executed StateReady"); });
            public static readonly Action<EgseBukNotify> StateBusy = new Action<EgseBukNotify>(x => { MessageBox.Show("executed StateBusy"); });
            public static readonly Action<EgseBukNotify> StateMe = new Action<EgseBukNotify>(x => { MessageBox.Show("executed StateMe"); });
            public static readonly Action<EgseBukNotify> ScidevDetectorImitUfes = new Action<EgseBukNotify>(x => { MessageBox.Show("executed ScidevDetectorImitUfes"); });
            public static readonly Action<EgseBukNotify> ScidevDetectorImitVufes = new Action<EgseBukNotify>(x => { MessageBox.Show("executed ScidevDetectorImitVufes"); });
            public static readonly Action<EgseBukNotify> ScidevDetectorImitSdsh = new Action<EgseBukNotify>(x => { MessageBox.Show("executed ScidevDetectorImitSdsh"); });
            public static readonly Action<EgseBukNotify> ScidevShutterUfes = new Action<EgseBukNotify>(x => { MessageBox.Show("executed ScidevShutterUfes"); });
            public static readonly Action<EgseBukNotify> ScidevShutterVufes = new Action<EgseBukNotify>(x => { MessageBox.Show("executed ScidevShutterVufes"); });
            public static readonly Action<EgseBukNotify> ScidevShutterSdsh = new Action<EgseBukNotify>(x => { MessageBox.Show("executed ScidevShutterSdsh"); });            
            public static readonly Action<EgseBukNotify> HalfsetDetectorImitMain = new Action<EgseBukNotify>(x => { MessageBox.Show("executed HalfsetDetectorImitMain"); });
            public static readonly Action<EgseBukNotify> HalfsetDetectorImitResv = new Action<EgseBukNotify>(x => { MessageBox.Show("executed HalfsetDetectorImitResv"); });
            public static readonly Action<EgseBukNotify> HalfsetKvvImitMain = new Action<EgseBukNotify>(x => { MessageBox.Show("executed HalfsetKvvImitMain"); });
            public static readonly Action<EgseBukNotify> HalfsetKvvImitResv = new Action<EgseBukNotify>(x => { MessageBox.Show("executed HalfsetKvvImitResv"); });
            public static readonly Action<EgseBukNotify> HalfsetKvvImitBoth = new Action<EgseBukNotify>(x => { MessageBox.Show("executed HalfsetKvvImitBoth"); });
            public static readonly Action<EgseBukNotify> HalfsetKvvImitNone = new Action<EgseBukNotify>(x => { MessageBox.Show("executed HalfsetKvvImitNone"); });
            public static readonly Action<EgseBukNotify> LineA = new Action<EgseBukNotify>(x => { MessageBox.Show("executed LineA"); });
            public static readonly Action<EgseBukNotify> LineB = new Action<EgseBukNotify>(x => { MessageBox.Show("executed LineB"); });
            public static readonly Action<EgseBukNotify> LineAB = new Action<EgseBukNotify>(x => { MessageBox.Show("executed LineAB"); });
            public static readonly Action<EgseBukNotify> ControlAuto = new Action<EgseBukNotify>(x => { MessageBox.Show("executed ControlAuto"); });
            public static readonly Action<EgseBukNotify> ChannelCh1_1 = new Action<EgseBukNotify>(x => { MessageBox.Show("executed ChannelCh1_1"); });
            public static readonly Action<EgseBukNotify> ChannelCh1_2 = new Action<EgseBukNotify>(x => { MessageBox.Show("executed ChannelCh1_2"); });
            public static readonly Action<EgseBukNotify> ChannelCh2_1 = new Action<EgseBukNotify>(x => { MessageBox.Show("executed ChannelCh2_1"); });
            public static readonly Action<EgseBukNotify> ChannelCh2_2 = new Action<EgseBukNotify>(x => { MessageBox.Show("executed ChannelCh2_2"); });
            public static readonly Action<EgseBukNotify> CommandTele = new Action<EgseBukNotify>(x => { MessageBox.Show("executed CommandTele"); });
            public static readonly Action<EgseBukNotify> PollOn = new Action<EgseBukNotify>(x => { MessageBox.Show("executed PollOn"); });
            public static readonly Action<EgseBukNotify> PollOff = new Action<EgseBukNotify>(x => { MessageBox.Show("executed PollOff"); });
            public static readonly Action<EgseBukNotify> DataOn = new Action<EgseBukNotify>(x => { MessageBox.Show("executed DataOn"); });
            public static readonly Action<EgseBukNotify> DataOff = new Action<EgseBukNotify>(x => { MessageBox.Show("executed DataOff"); });
            public static readonly Action<EgseBukNotify, object> TimeArg = new Action<EgseBukNotify, object>((x, obj) => { MessageBox.Show("executed TimeArg:" + (int)obj); });
            public static readonly Action<EgseBukNotify> MarkReceipt = new Action<EgseBukNotify>(x => { MessageBox.Show("executed MarkReceipt"); });
            public static readonly Action<EgseBukNotify> MarkExecut = new Action<EgseBukNotify>(x => { MessageBox.Show("executed MarkExecut"); });
            public static readonly Action<EgseBukNotify> CmdActivate1 = new Action<EgseBukNotify>(x => { MessageBox.Show("executed CmdActivate1"); });
            public static readonly Action<EgseBukNotify> CmdActivate2 = new Action<EgseBukNotify>(x => { MessageBox.Show("executed CmdActivate2"); });
            public static readonly Action<EgseBukNotify, object> ApidArg = new Action<EgseBukNotify, object>((x, obj) => { MessageBox.Show("executed ApidArg:" + (int)obj); });
            public static readonly Action<EgseBukNotify> ReceiveMain = new Action<EgseBukNotify>(x => { MessageBox.Show("executed ReceiveMain"); });
            public static readonly Action<EgseBukNotify> ReceiveResv = new Action<EgseBukNotify>(x => { MessageBox.Show("executed ReceiveResv"); });
            public static readonly Action<EgseBukNotify> SendMain = new Action<EgseBukNotify>(x => { MessageBox.Show("executed SendMain"); });
            public static readonly Action<EgseBukNotify> SendResv = new Action<EgseBukNotify>(x => { MessageBox.Show("executed SendResv"); });
            public static readonly Action<EgseBukNotify> TickOn = new Action<EgseBukNotify>(x => { MessageBox.Show("executed TickOn"); });
            public static readonly Action<EgseBukNotify> TickOff = new Action<EgseBukNotify>(x => { MessageBox.Show("executed TickOff"); });
            public static readonly Action<EgseBukNotify> SensorOpenOn = new Action<EgseBukNotify>(x => { MessageBox.Show("executed SensorOpenOn"); });
            public static readonly Action<EgseBukNotify> SensorOpenOff = new Action<EgseBukNotify>(x => { MessageBox.Show("executed SensorOpenOff"); });
            public static readonly Action<EgseBukNotify> SensorCloseOn = new Action<EgseBukNotify>(x => { MessageBox.Show("executed SensorCloseOn"); });
            public static readonly Action<EgseBukNotify> SensorCloseOff = new Action<EgseBukNotify>(x => { MessageBox.Show("executed SensorCloseOff"); });
            public static readonly Action<EgseBukNotify> OnBoardTimeOn = new Action<EgseBukNotify>(x => { MessageBox.Show("executed OnBoardTimeOn"); });
            public static readonly Action<EgseBukNotify> OnBoardTimeOff = new Action<EgseBukNotify>(x => { MessageBox.Show("executed OnBoardTimeOff"); });
            public static readonly Action<EgseBukNotify> SetsPower1 = new Action<EgseBukNotify>(x => { MessageBox.Show("executed SetsPower1"); });
            public static readonly Action<EgseBukNotify> SetsPower2 = new Action<EgseBukNotify>(x => { MessageBox.Show("executed SetsPower2"); });
            public static readonly Action<EgseBukNotify> SetsKvvImit1 = new Action<EgseBukNotify>(x => { MessageBox.Show("executed SetsKvvImit1"); });
            public static readonly Action<EgseBukNotify> SetsKvvImit2 = new Action<EgseBukNotify>(x => { MessageBox.Show("executed SetsKvvImit2"); });
            public static readonly Action<EgseBukNotify, object> HexPackageArg = new Action<EgseBukNotify, object>((x, obj) => { MessageBox.Show("executed HexPackageArg: " + string.Join(" ", (obj as List<string>))); });
        }

        private enum BinLogKvvImit
        {
            [Action(typeof(Execut), "BinLogKvvImitOn")]
            BIN_ON,

            [Action(typeof(Execut), "BinLogKvvImitOff")]
            BIN_OFF
        }

        private enum BinLogDetectorImit
        {
            [Action(typeof(Execut), "BinLogDetectorImitOn")]
            BIN_ON,

            [Action(typeof(Execut), "BinLogDetectorImitOff")]
            BIN_OFF
        }

        private enum BinLogBm4Imit
        {
            [Action(typeof(Execut), "BinLogBm4ImitOn")]
            BIN_ON,

            [Action(typeof(Execut), "BinLogBm4ImitOff")]
            BIN_OFF
        }

        private enum Exchange
        {
            [Action(typeof(Execut), "ExchangeOn")]
            EXCHANGE_ON,

            [Action(typeof(Execut), "ExchangeOff")]
            EXCHANGE_OFF
        }


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

        private enum HalfsetDetectorImit
        {
            [Action(typeof(Execut), "HalfsetDetectorImitMain")]
            MAIN,

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
            [Action(typeof(Execut), "ChannelCh1_1")]
            CH1_1,

            /// <summary>
            /// Параметр "канал spacewire БУК ПК1 - БУСК ПК2" аргумента.
            /// </summary>
            [Action(typeof(Execut), "ChannelCh1_2")]
            CH1_2,

            /// <summary>
            /// Параметр "канал spacewire БУК ПК2 - БУСК ПК1" аргумента.
            /// </summary>
            [Action(typeof(Execut), "ChannelCh2_1")]
            CH2_1,

            /// <summary>
            /// Параметр "канал spacewire БУК ПК2 - БУСК ПК2" аргумента.
            /// </summary>
            [Action(typeof(Execut), "ChannelCh2_2")]
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

        private enum Data
        {
            [Action(typeof(Execut), "DataOn")]
            DATA_ON,

            [Action(typeof(Execut), "DataOff")]
            DATA_OFF
        }

        private enum Time
        {
            [Action(typeof(Execut), "TimeArg", true)]
            TIME_dec
        }

        private enum Rece
        {
            [Action(typeof(Execut), "MarkReceipt")]
            RECEIPT           
        }

        private enum Exec
        {
            [Action(typeof(Execut), "MarkExecut")]
            EXECUT
        }

        private enum Activate
        {
            [Action(typeof(Execut), "CmdActivate1")]
            ACTIVATE1,

            [Action(typeof(Execut), "CmdActivate2")]
            ACTIVATE2
        }

        private enum Apid
        {
            [Action(typeof(Execut), "ApidArg", true)]
            APID_hex
        }

        [Action(typeof(Execut), "HexPackageArg", true)]
        private enum HexPackage
        { }

        private enum Receive
        {
            [Action(typeof(Execut), "ReceiveMain")]
            RECV_MAIN,

            [Action(typeof(Execut), "ReceiveResv")]
            RECV_RESV
        }

        private enum Send
        {
            [Action(typeof(Execut), "SendMain")]
            SEND_MAIN,

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

        private enum SensorOpen
        {
            [Action(typeof(Execut), "SensorOpenOn")]
            SENS_OPEN_ON,

            [Action(typeof(Execut), "SensorOpenOff")]
            SENS_OPEN_OFF
        }

        private enum SensorClose
        {
            [Action(typeof(Execut), "SensorCloseOn")]
            SENS_CLOSE_ON,

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

        private bool IncludeTest<TEnum>(string[] cmd, out string errStr, ref List<Type> dupl, TEnum[] exclude = null, bool duplNeedTest = true) 
        {
            new { dupl }.CheckNotNull();

            if ((1 == cmd.Length) && (2 < cmd[0].Length))
            {
                return IncludeTest<TEnum>(cmd[0], out errStr, ref dupl, exclude, duplNeedTest);
            }

            if (typeof(HexPackage) != typeof(TEnum))
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

        private bool IncomePackage(Type testType, string cmd, out string errStr, ref List<string> list)
        {
            if (typeof(HexPackage) == testType)
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
        /// <param name="exclude">Список исключенных параметров.</param>
        /// <returns><c>true</c> если параметры существуют, иначе <c>false</c></returns>
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

        private bool NeedTest<TEnum>(string cmd, out string errStr, ref List<Type> dupl, TEnum[] exclude = null)
        {
            if (IncludeTest<TEnum>(cmd, out errStr, ref dupl, exclude, false))
            {
                return true;
            }

            return false;
        }

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

        private bool IncludeExec<TEnum>(string cmd, ref List<string> cmds, TEnum[] exclude = null)
        {
            if (typeof(HexPackage) == typeof(TEnum))
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

        private void IncludeExec<TEnum1>(List<string> cmds, TEnum1[] exclude1 = null)
        {
            foreach (string str in cmds)
            {
                if (IncludeExec<TEnum1>(str, ref cmds, exclude1))
                {
                    cmds.Remove(str);
                    break;
                }
            }
        }

        private void IncludeExec<TEnum1, TEnum2>(List<string> cmds, TEnum1[] exclude1 = null, TEnum2[] exclude2 = null)
        {
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

        private void IncludeExec<TEnum1, TEnum2, TEnum3>(List<string> cmds, TEnum1[] exclude1 = null, TEnum2[] exclude2 = null, TEnum3[] exclude3 = null)
        {
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

        private void IncludeExec<TEnum1, TEnum2, TEnum3, TEnum4>(List<string> cmds, TEnum1[] exclude1 = null, TEnum2[] exclude2 = null, TEnum3[] exclude3 = null, TEnum4[] exclude4 = null)
        {
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

        private void IncludeExec<TEnum1, TEnum2, TEnum3, TEnum4, TEnum5>(List<string> cmds, TEnum1[] exclude1 = null, TEnum2[] exclude2 = null, TEnum3[] exclude3 = null, TEnum4[] exclude4 = null, TEnum5[] exclude5 = null)
        {
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

        private void IncludeExec<TEnum1, TEnum2, TEnum3, TEnum4, TEnum5, TEnum6>(List<string> cmds, TEnum1[] exclude1 = null, TEnum2[] exclude2 = null, TEnum3[] exclude3 = null, TEnum4[] exclude4 = null, TEnum5[] exclude5 = null, TEnum6[] exclude6 = null)
        {
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

            if (!IncludeTest<Exchange, Tick, OnBoardTime>(cmdParams[2], out errParam, ref uniParam))
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
                IncludeExec<Channel, SwitcherBm4Imit, Exchange, Tick, OnBoardTime>(cmdParams.ToList());
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
                if (!IncludeTest<HexPackage>(cmdParams[0], out errParam, ref uniParam))
                {
                    errString = string.Format(Resource.Get(@"eArg1"), errParam);
                    return false;
                }

                return !isParamCountErr;
            }

            if (!IncludeTest<Command, HexPackage>(cmdParams[0], out errParam, ref uniParam))
            {
                errString = string.Format(Resource.Get(@"eArg1"), errParam);
                return false;
            }

            if (!IncludeTest<Apid, Rece, Exec, HexPackage>(cmdParams[1], out errParam, ref uniParam))
            {
                errString = string.Format(Resource.Get(@"eArg2"), errParam);
                return false;
            }

            if (3 > cmdParams.Length)
            {
                return !isParamCountErr;
            }

            if (!IncludeTest<Rece, Exec, HexPackage>(cmdParams[2], out errParam, ref uniParam))
            {
                errString = string.Format(Resource.Get(@"eArg3"), errParam);
                return false;
            }

            if (4 > cmdParams.Length)
            {
                return !isParamCountErr;
            }

            if (!IncludeTest<Rece, Exec, HexPackage>(cmdParams[3], out errParam, ref uniParam))
            {
                errString = string.Format(Resource.Get(@"eArg4"), errParam);
                return false;
            }

            if (!NeedTest<Command>(cmdParams[0], out errParam, ref uniParam))
            {
                if (!IncludeTest<HexPackage>(cmdParams, out errParam, ref uniParam))
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

                if (!IncludeTest<HexPackage>(cmdParams.Skip(skipCount).ToArray(), out errParam, ref uniParam))
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
                IncludeExec<Command, Apid, Rece, Exec, HexPackage>(cmdParams.ToList());
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
                if (!IncludeTest<HexPackage>(cmdParams[0], out errParam, ref uniParam))
                {
                    errString = string.Format(Resource.Get(@"eArg1"), errParam);
                    return false;
                }

                return !isParamCountErr;
            }

            if (!IncludeTest<Activate, HexPackage>(cmdParams[0], out errParam, ref uniParam))
            {
                errString = string.Format(Resource.Get(@"eArg1"), errParam);
                return false;
            }

            if (!IncludeTest<HexPackage>(cmdParams[1], out errParam, ref uniParam))
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
                if (!IncludeTest<HexPackage>(cmdParams, out errParam, ref uniParam))
                {
                    errString = string.Format(Resource.Get(@"eArg1"), errParam);
                    return false;
                }
            }
            else
            {
                if (!IncludeTest<HexPackage>(cmdParams.Skip(1).ToArray(), out errParam, ref uniParam))
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
                IncludeExec<Activate, HexPackage>(cmdParams.ToList());
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
    }
}
